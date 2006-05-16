//
// flowanalyis.cs: The control flow analysis code
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc.
//

using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Mono.CSharp
{
	public enum TriState : byte {
		// Never < Sometimes < Always
		Never,
		Sometimes,
		Always
	}

	// <summary>
	//   A new instance of this class is created every time a new block is resolved
	//   and if there's branching in the block's control flow.
	// </summary>
	public abstract class FlowBranching
	{
		// <summary>
		//   The type of a FlowBranching.
		// </summary>
		public enum BranchingType : byte {
			// Normal (conditional or toplevel) block.
			Block,

			// Conditional.
			Conditional,

			// A loop block.
			Loop,

			// The statement embedded inside a loop
			Embedded,

			// part of a block headed by a jump target
			Labeled,

			// Try/Catch block.
			Exception,

			// Switch block.
			Switch,

			// Switch section.
			SwitchSection,

			// The toplevel block of a function
			Toplevel
		}

		// <summary>
		//   The type of one sibling of a branching.
		// </summary>
		public enum SiblingType : byte {
			Block,
			Conditional,
			SwitchSection,
			Try,
			Catch,
			Finally
		}

		public sealed class Reachability
		{
			TriState returns, throws, barrier;

			public TriState Returns {
				get { return returns; }
			}
			public TriState Throws {
				get { return throws; }
			}
			public TriState Barrier {
				get { return barrier; }
			}

			Reachability (TriState returns, TriState throws, TriState barrier)
			{
				this.returns = returns;
				this.throws = throws;
				this.barrier = barrier;
			}

			public Reachability Clone ()
			{
				return new Reachability (returns, throws, barrier);
			}

			public static TriState TriState_Meet (TriState a, TriState b)
			{
				// (1) if both are Never, return Never
				// (2) if both are Always, return Always
				// (3) otherwise, return Sometimes
				// note that (3) => (3') if both are Sometimes, return Sometimes
				return a == b ? a : TriState.Sometimes;
			}

			public static TriState TriState_Max (TriState a, TriState b)
			{
				return ((byte) a > (byte) b) ? a : b;
			}

			public void Meet (Reachability b)
			{
				if ((AlwaysReturns && b.AlwaysHasBarrier) || (AlwaysHasBarrier && b.AlwaysReturns))
					returns = TriState.Always;
				else
					returns = TriState_Meet (returns, b.returns);

				throws = TriState_Meet (throws, b.throws);
				barrier = TriState_Meet (barrier, b.barrier);
			}

			public void Or (Reachability b)
			{
				returns = TriState_Max (returns, b.returns);
				throws = TriState_Max (throws, b.throws);
				barrier = TriState_Max (barrier, b.barrier);
			}

			public static Reachability Always ()
			{
				return new Reachability (TriState.Never, TriState.Never, TriState.Never);
			}

			TriState Unreachable {
				get { return TriState_Max (returns, TriState_Max (throws, barrier)); }
			}

			TriState Reachable {
				get {
					TriState unreachable = Unreachable;
					if (unreachable == TriState.Sometimes)
						return TriState.Sometimes;
					return unreachable == TriState.Always ? TriState.Never : TriState.Always;
				}
			}

			public bool AlwaysReturns {
				get { return returns == TriState.Always; }
			}

			public bool AlwaysThrows {
				get { return throws == TriState.Always; }
			}

			public bool AlwaysHasBarrier {
				get { return barrier == TriState.Always; }
			}

			public bool IsUnreachable {
				get { return Unreachable == TriState.Always; }
			}

			public void SetReturns ()
			{
				returns = TriState.Always;
			}

			public void SetThrows ()
			{
				throws = TriState.Always;
			}

			public void SetBarrier ()
			{
				barrier = TriState.Always;
			}

			static string ShortName (TriState returns)
			{
				switch (returns) {
				case TriState.Never:
					return "N";
				case TriState.Sometimes:
					return "S";
				default:
					return "A";
				}
			}

			public override string ToString ()
			{
				return String.Format ("[{0}:{1}:{2}:{3}]",
						      ShortName (returns), ShortName (throws), ShortName (barrier),
						      ShortName (Reachable));
			}
		}

		public static FlowBranching CreateBranching (FlowBranching parent, BranchingType type, Block block, Location loc)
		{
			switch (type) {
			case BranchingType.Exception:
			case BranchingType.Labeled:
			case BranchingType.Toplevel:
				throw new InvalidOperationException ();

			case BranchingType.Switch:
				return new FlowBranchingBreakable (parent, type, SiblingType.SwitchSection, block, loc);

			case BranchingType.SwitchSection:
				return new FlowBranchingBlock (parent, type, SiblingType.Block, block, loc);

			case BranchingType.Block:
				return new FlowBranchingBlock (parent, type, SiblingType.Block, block, loc);

			case BranchingType.Loop:
				return new FlowBranchingBreakable (parent, type, SiblingType.Conditional, block, loc);

			case BranchingType.Embedded:
				return new FlowBranchingContinuable (parent, type, SiblingType.Conditional, block, loc);

			default:
				return new FlowBranchingBlock (parent, type, SiblingType.Conditional, block, loc);
			}
		}

		// <summary>
		//   The type of this flow branching.
		// </summary>
		public readonly BranchingType Type;

		// <summary>
		//   The block this branching is contained in.  This may be null if it's not
		//   a top-level block and it doesn't declare any local variables.
		// </summary>
		public readonly Block Block;

		// <summary>
		//   The parent of this branching or null if this is the top-block.
		// </summary>
		public readonly FlowBranching Parent;

		// <summary>
		//   Start-Location of this flow branching.
		// </summary>
		public readonly Location Location;

		protected VariableMap param_map, local_map;

		static int next_id = 0;
		int id;

		// <summary>
		//   The vector contains a BitArray with information about which local variables
		//   and parameters are already initialized at the current code position.
		// </summary>
		public class UsageVector {
			// <summary>
			//   The type of this branching.
			// </summary>
			public readonly SiblingType Type;

			// <summary>
			//   Start location of this branching.
			// </summary>
			public Location Location;

			// <summary>
			//   This is only valid for SwitchSection, Try, Catch and Finally.
			// </summary>
			public readonly Block Block;

			// <summary>
			//   The number of parameters in this block.
			// </summary>
			public readonly int CountParameters;

			// <summary>
			//   The number of locals in this block.
			// </summary>
			public readonly int CountLocals;

			// <summary>
			//   If not null, then we inherit our state from this vector and do a
			//   copy-on-write.  If null, then we're the first sibling in a top-level
			//   block and inherit from the empty vector.
			// </summary>
			public readonly UsageVector InheritsFrom;

			// <summary>
			//   This is used to construct a list of UsageVector's.
			// </summary>
			public UsageVector Next;

			//
			// Private.
			//
			MyBitVector locals, parameters;
			Reachability reachability;

			static int next_id = 0;
			int id;

			//
			// Normally, you should not use any of these constructors.
			//
			public UsageVector (SiblingType type, UsageVector parent, Block block, Location loc, int num_params, int num_locals)
			{
				this.Type = type;
				this.Block = block;
				this.Location = loc;
				this.InheritsFrom = parent;
				this.CountParameters = num_params;
				this.CountLocals = num_locals;

				locals = num_locals == 0 
					? MyBitVector.Empty
					: new MyBitVector (parent == null ? MyBitVector.Empty : parent.locals, num_locals);

				parameters = num_params == 0
					? MyBitVector.Empty
					: new MyBitVector (parent == null ? MyBitVector.Empty : parent.parameters, num_params);

				reachability = parent == null ? Reachability.Always () : parent.Reachability.Clone ();

				id = ++next_id;
			}

			public UsageVector (SiblingType type, UsageVector parent, Block block, Location loc)
				: this (type, parent, block, loc, parent.CountParameters, parent.CountLocals)
			{ }

			public UsageVector (MyBitVector parameters, MyBitVector locals, Reachability reachability, Block block, Location loc)
			{
				this.Type = SiblingType.Block;
				this.Location = loc;
				this.Block = block;

				this.reachability = reachability;
				this.parameters = parameters;
				this.locals = locals;

				id = ++next_id;
			}

			// <summary>
			//   This does a deep copy of the usage vector.
			// </summary>
			public UsageVector Clone ()
			{
				UsageVector retval = new UsageVector (Type, null, Block, Location, CountParameters, CountLocals);

				retval.locals = locals.Clone ();
				retval.parameters = parameters.Clone ();
				retval.reachability = reachability.Clone ();

				return retval;
			}

			public bool IsAssigned (VariableInfo var, bool ignoreReachability)
			{
				if (!ignoreReachability && !var.IsParameter && Reachability.IsUnreachable)
					return true;

				return var.IsAssigned (var.IsParameter ? parameters : locals);
			}

			public void SetAssigned (VariableInfo var)
			{
				if (!var.IsParameter && Reachability.IsUnreachable)
					return;

				var.SetAssigned (var.IsParameter ? parameters : locals);
			}

			public bool IsFieldAssigned (VariableInfo var, string name)
			{
				if (!var.IsParameter && Reachability.IsUnreachable)
					return true;

				return var.IsFieldAssigned (var.IsParameter ? parameters : locals, name);
			}

			public void SetFieldAssigned (VariableInfo var, string name)
			{
				if (!var.IsParameter && Reachability.IsUnreachable)
					return;

				var.SetFieldAssigned (var.IsParameter ? parameters : locals, name);
			}

			public Reachability Reachability {
				get { return reachability; }
			}

			public void Return ()
			{
				if (!reachability.IsUnreachable)
					reachability.SetReturns ();
			}

			public void Throw ()
			{
				if (!reachability.IsUnreachable) {
					reachability.SetThrows ();
					reachability.SetBarrier ();
				}
			}

			public void Goto ()
			{
				if (!reachability.IsUnreachable)
					reachability.SetBarrier ();
			}

			public static UsageVector MergeSiblings (UsageVector sibling_list, Location loc)
			{
				if (sibling_list.Next == null)
					return sibling_list;

				MyBitVector locals = null;
				MyBitVector parameters = null;
				Reachability reachability = null;

				for (UsageVector child = sibling_list; child != null; child = child.Next) {
					Report.Debug (2, "    MERGING SIBLING   ", reachability, child);

					if (reachability == null)
						reachability = child.Reachability.Clone ();
					else
						reachability.Meet (child.Reachability);

					// A local variable is initialized after a flow branching if it
					// has been initialized in all its branches which do neither
					// always return or always throw an exception.
					//
					// If a branch may return, but does not always return, then we
					// can treat it like a never-returning branch here: control will
					// only reach the code position after the branching if we did not
					// return here.
					//
					// It's important to distinguish between always and sometimes
					// returning branches here:
					//
					//    1   int a;
					//    2   if (something) {
					//    3      return;
					//    4      a = 5;
					//    5   }
					//    6   Console.WriteLine (a);
					//
					// The if block in lines 3-4 always returns, so we must not look
					// at the initialization of `a' in line 4 - thus it'll still be
					// uninitialized in line 6.
					//
					// On the other hand, the following is allowed:
					//
					//    1   int a;
					//    2   if (something)
					//    3      a = 5;
					//    4   else
					//    5      return;
					//    6   Console.WriteLine (a);
					//
					// Here, `a' is initialized in line 3 and we must not look at
					// line 5 since it always returns.
					// 
					bool unreachable = child.Reachability.IsUnreachable;

					Report.Debug (2, "    MERGING SIBLING #1", reachability,
						      child.Type, child.Reachability.IsUnreachable, unreachable);

					if (!unreachable)
						MyBitVector.And (ref locals, child.locals);

					// An `out' parameter must be assigned in all branches which do
					// not always throw an exception.
					if (!child.Reachability.AlwaysThrows)
						MyBitVector.And (ref parameters, child.parameters);

					Report.Debug (2, "    MERGING SIBLING #2", parameters, locals);
				}
				
				if (reachability == null)
					throw new InternalErrorException ("Cannot happen: the loop above runs at least twice");

				return new UsageVector (parameters, locals, reachability, null, loc);
			}

			// <summary>
			//   Merges a child branching.
			// </summary>
			public UsageVector MergeChild (UsageVector child, bool implicit_block)
			{
				Report.Debug (2, "    MERGING CHILD EFFECTS", this, child, reachability, Type);

				Reachability new_r = child.Reachability;

				//
				// We've now either reached the point after the branching or we will
				// never get there since we always return or always throw an exception.
				//
				// If we can reach the point after the branching, mark all locals and
				// parameters as initialized which have been initialized in all branches
				// we need to look at (see above).
				//

				if ((Type == SiblingType.SwitchSection) && !new_r.IsUnreachable) {
					Report.Error (163, Location,
						      "Control cannot fall through from one " +
						      "case label to another");
					return child;
				}

				MyBitVector.Or (ref locals, child.locals);
				MyBitVector.Or (ref parameters, child.parameters);

				if (implicit_block)
					reachability = new_r.Clone ();
				else
					reachability.Or (new_r);

				return child;
			}

			public void MergeOrigins (UsageVector o_vectors)
			{
				Report.Debug (1, "  MERGING BREAK ORIGINS", this);

				if (o_vectors == null)
					return;

				if (reachability.IsUnreachable) {
					if (locals != null)
						locals.SetAll (true);
					if (parameters != null)
						parameters.SetAll (true);
				}

				for (UsageVector vector = o_vectors; vector != null; vector = vector.Next) {
					Report.Debug (1, "    MERGING BREAK ORIGIN", vector);
					MyBitVector.And (ref locals, vector.locals);
					MyBitVector.And (ref parameters, vector.parameters);
					reachability.Meet (vector.Reachability);
				}

				Report.Debug (1, "  MERGING BREAK ORIGINS DONE", this);
			}

			//
			// Debugging stuff.
			//

			public override string ToString ()
			{
				return String.Format ("Vector ({0},{1},{2}-{3}-{4})", Type, id, reachability, parameters, locals);
			}
		}

		// <summary>
		//   Creates a new flow branching which is contained in `parent'.
		//   You should only pass non-null for the `block' argument if this block
		//   introduces any new variables - in this case, we need to create a new
		//   usage vector with a different size than our parent's one.
		// </summary>
		protected FlowBranching (FlowBranching parent, BranchingType type, SiblingType stype,
					 Block block, Location loc)
		{
			Parent = parent;
			Block = block;
			Location = loc;
			Type = type;
			id = ++next_id;

			UsageVector vector;
			if (Block != null) {
				param_map = Block.ParameterMap;
				local_map = Block.LocalMap;

				UsageVector parent_vector = parent != null ? parent.CurrentUsageVector : null;
				vector = new UsageVector (
					stype, parent_vector, Block, loc,
					param_map.Length, local_map.Length);
			} else {
				param_map = Parent.param_map;
				local_map = Parent.local_map;
				vector = new UsageVector (
					stype, Parent.CurrentUsageVector, null, loc);
			}

			AddSibling (vector);
		}

		public abstract UsageVector CurrentUsageVector {
			get;
		}				

		// <summary>
		//   Creates a sibling of the current usage vector.
		// </summary>
		public virtual void CreateSibling (Block block, SiblingType type)
		{
			UsageVector vector = new UsageVector (
				type, Parent.CurrentUsageVector, block, Location);
			AddSibling (vector);

			Report.Debug (1, "  CREATED SIBLING", CurrentUsageVector);
		}

		public void CreateSibling ()
		{
			CreateSibling (null, SiblingType.Conditional);
		}

		protected abstract void AddSibling (UsageVector uv);

		public virtual LabeledStatement LookupLabel (string name, Location loc)
		{
			return Parent.LookupLabel (name, loc);
		}

		public abstract void Label (UsageVector origin_vectors);

		protected abstract UsageVector Merge ();

		// <summary>
		//   Merge a child branching.
		// </summary>
		public UsageVector MergeChild (FlowBranching child)
		{
			bool implicit_block = child.Type == BranchingType.Block && child.Block.Implicit;
			Report.Debug (2, "  MERGING CHILD", this, child);
			UsageVector result = CurrentUsageVector.MergeChild (child.Merge (), implicit_block);
			Report.Debug (2, "  MERGING CHILD DONE", this, result);
			return result;
 		}

		public virtual bool InTryWithCatch ()
		{
			return Parent.InTryWithCatch ();
		}

		// returns true if we crossed an unwind-protected region (try/catch/finally, lock, using, ...)
		public virtual bool AddBreakOrigin (UsageVector vector, Location loc)
		{
			return Parent.AddBreakOrigin (vector, loc);
		}

		// returns true if we crossed an unwind-protected region (try/catch/finally, lock, using, ...)
		public virtual bool AddContinueOrigin (UsageVector vector, Location loc)
		{
			return Parent.AddContinueOrigin (vector, loc);
		}

		// returns true if we crossed an unwind-protected region (try/catch/finally, lock, using, ...)
		public virtual bool AddReturnOrigin (UsageVector vector, Location loc)
		{
			return Parent.AddReturnOrigin (vector, loc);
		}

		public virtual void StealFinallyClauses (ref ArrayList list)
		{
			Parent.StealFinallyClauses (ref list);
		}

		public bool IsAssigned (VariableInfo vi)
		{
			return CurrentUsageVector.IsAssigned (vi, false);
		}

		public bool IsFieldAssigned (VariableInfo vi, string field_name)
		{
			return CurrentUsageVector.IsAssigned (vi, false) || CurrentUsageVector.IsFieldAssigned (vi, field_name);
		}

		public void SetAssigned (VariableInfo vi)
		{
			CurrentUsageVector.SetAssigned (vi);
		}

		public void SetFieldAssigned (VariableInfo vi, string name)
		{
			CurrentUsageVector.SetFieldAssigned (vi, name);
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (GetType ());
			sb.Append (" (");

			sb.Append (id);
			sb.Append (",");
			sb.Append (Type);
			if (Block != null) {
				sb.Append (" - ");
				sb.Append (Block.ID);
				sb.Append (" - ");
				sb.Append (Block.StartLocation);
			}
			sb.Append (" - ");
			// sb.Append (Siblings.Length);
			// sb.Append (" - ");
			sb.Append (CurrentUsageVector);
			sb.Append (")");
			return sb.ToString ();
		}

		public string Name {
			get { return String.Format ("{0} ({1}:{2}:{3})", GetType (), id, Type, Location); }
		}
	}

	public class FlowBranchingBlock : FlowBranching
	{
		UsageVector sibling_list = null;

		public FlowBranchingBlock (FlowBranching parent, BranchingType type,
					   SiblingType stype, Block block, Location loc)
			: base (parent, type, stype, block, loc)
		{ }

		public override UsageVector CurrentUsageVector {
			get { return sibling_list; }
		}

		protected override void AddSibling (UsageVector sibling)
		{
			sibling.Next = sibling_list;
			sibling_list = sibling;
		}

		public override LabeledStatement LookupLabel (string name, Location loc)
		{
			LabeledStatement stmt = Block == null ? null : Block.LookupLabel (name);
			if (stmt == null)
				return Parent.LookupLabel (name, loc);

			stmt.AddReference ();
			return stmt;
		}

		public override void Label (UsageVector origin_vectors)
		{
			if (!CurrentUsageVector.Reachability.IsUnreachable) {
				UsageVector vector = CurrentUsageVector.Clone ();
				vector.Next = origin_vectors;
				origin_vectors = vector;
			}

			CurrentUsageVector.MergeOrigins (origin_vectors);
		}

		protected override UsageVector Merge ()
		{
			Report.Debug (2, "  MERGING SIBLINGS", Name);
			UsageVector vector = UsageVector.MergeSiblings (sibling_list, Location);
			Report.Debug (2, "  MERGING SIBLINGS DONE", Name, vector);
			return vector;
		}
	}

	public class FlowBranchingBreakable : FlowBranchingBlock
	{
		UsageVector break_origins;

		public FlowBranchingBreakable (FlowBranching parent, BranchingType type, SiblingType stype, Block block, Location loc)
			: base (parent, type, stype, block, loc)
		{ }

		public override bool AddBreakOrigin (UsageVector vector, Location loc)
		{
			vector = vector.Clone ();
			vector.Next = break_origins;
			break_origins = vector;
			return false;
		}

		protected override UsageVector Merge ()
		{
			UsageVector vector = base.Merge ();
			vector.MergeOrigins (break_origins);
			return vector;
		}
	}

	public class FlowBranchingContinuable : FlowBranchingBlock
	{
		UsageVector continue_origins;

		public FlowBranchingContinuable (FlowBranching parent, BranchingType type, SiblingType stype, Block block, Location loc)
			: base (parent, type, stype, block, loc)
		{ }

		public override bool AddContinueOrigin (UsageVector vector, Location loc)
		{
			vector = vector.Clone ();
			vector.Next = continue_origins;
			continue_origins = vector;
			return false;
		}

		protected override UsageVector Merge ()
		{
			UsageVector vector = base.Merge ();
			vector.MergeOrigins (continue_origins);
			return vector;
		}
	}

	public class FlowBranchingLabeled : FlowBranchingBlock
	{
		LabeledStatement stmt;
		public FlowBranchingLabeled (FlowBranching parent, LabeledStatement stmt)
			: base (parent, BranchingType.Labeled, SiblingType.Conditional, null, stmt.loc)
		{
			this.stmt = stmt;
		}

		public override LabeledStatement LookupLabel (string name, Location loc)
		{
			if (name != stmt.Name)
				return Parent.LookupLabel (name, loc);

			stmt.AddReference ();
			return stmt;
		}
	}

	public class FlowBranchingToplevel : FlowBranchingBlock
	{
		public FlowBranchingToplevel (FlowBranching parent, ToplevelBlock stmt)
			: base (parent, BranchingType.Toplevel, SiblingType.Conditional, stmt, stmt.loc)
		{
		}

		// <summary>
		//   Check whether all `out' parameters have been assigned.
		// </summary>
		void CheckOutParameters (UsageVector vector, Location loc)
		{
			for (int i = 0; i < param_map.Count; i++) {
				VariableInfo var = param_map [i];

				if (var == null)
					continue;

				if (vector.IsAssigned (var, false))
					continue;

				Report.Error (177, loc, "The out parameter `{0}' must be assigned to before control leaves the current method",
					var.Name);
			}
		}

		public override bool InTryWithCatch ()
		{
			return false;
		}

		public override bool AddBreakOrigin (UsageVector vector, Location loc)
		{
			Report.Error (139, loc, "No enclosing loop out of which to break or continue");
			return false;
		}

		public override bool AddContinueOrigin (UsageVector vector, Location loc)
		{
			Report.Error (139, loc, "No enclosing loop out of which to break or continue");
			return false;
		}

		public override bool AddReturnOrigin (UsageVector vector, Location loc)
		{
			CheckOutParameters (vector, loc);
			return false;
		}

		public override void StealFinallyClauses (ref ArrayList list)
		{
			// nothing to do
		}

		public override LabeledStatement LookupLabel (string name, Location loc)
		{
			LabeledStatement s = Block.LookupLabel (name);
			if (s != null)
				throw new InternalErrorException ("Shouldn't get here");

			if (Parent != null) {
				s = Parent.LookupLabel (name, loc);
				if (s != null) {
					Report.Error (1632, loc, "Control cannot leave the body of an anonymous method");
					return null;
				}
			}

			Report.Error (159, loc, "No such label `{0}' in this scope", name);
			return null;
		}

		public Reachability End ()
		{
			UsageVector result = Merge ();

			Report.Debug (4, "MERGE TOP BLOCK", Location, result);

			if (!result.Reachability.AlwaysThrows && !result.Reachability.AlwaysHasBarrier)
				CheckOutParameters (result, Location);

			return result.Reachability;
		}
	}

	public class FlowBranchingException : FlowBranching
	{
		ExceptionStatement stmt;
		UsageVector current_vector;
		UsageVector catch_vectors;
		UsageVector finally_vector;

		UsageVector break_origins;
		UsageVector continue_origins;
		UsageVector return_origins;

		bool emit_finally;

		public FlowBranchingException (FlowBranching parent,
					       ExceptionStatement stmt)
			: base (parent, BranchingType.Exception, SiblingType.Try,
				null, stmt.loc)
		{
			this.stmt = stmt;
			this.emit_finally = true;
		}

		protected override void AddSibling (UsageVector sibling)
		{
			switch (sibling.Type) {
			case SiblingType.Try:
			case SiblingType.Catch:
				sibling.Next = catch_vectors;
				catch_vectors = sibling;
				break;
			case SiblingType.Finally:
				finally_vector = sibling;
				break;
			default:
				throw new InvalidOperationException ();
			}
			current_vector = sibling;
		}

		public override UsageVector CurrentUsageVector {
			get { return current_vector; }
		}

		public override bool InTryWithCatch ()
		{
			if (finally_vector == null) {
				Try t = stmt as Try;
				if (t != null && t.HasCatch)
					return true;
			}

			return base.InTryWithCatch ();
		}

		public override bool AddBreakOrigin (UsageVector vector, Location loc)
		{
			if (finally_vector != null) {
				Report.Error (157, loc, "Control cannot leave the body of a finally clause");
			} else {
				vector = vector.Clone ();
				vector.Location = loc;
				vector.Next = break_origins;
				break_origins = vector;
			}
			return true;
		}

		public override bool AddContinueOrigin (UsageVector vector, Location loc)
		{
			if (finally_vector != null) {
				Report.Error (157, loc, "Control cannot leave the body of a finally clause");
			} else {
				vector = vector.Clone ();
				vector.Location = loc;
				vector.Next = continue_origins;
				continue_origins = vector;
			}
			return true;
		}

		public override bool AddReturnOrigin (UsageVector vector, Location loc)
		{
			if (finally_vector != null) {
				Report.Error (157, loc, "Control cannot leave the body of a finally clause");
			} else {
				vector = vector.Clone ();
				vector.Location = loc;
				vector.Next = return_origins;
				return_origins = vector;
			}
			return true;
		}

		public override void StealFinallyClauses (ref ArrayList list)
		{
			if (list == null)
				list = new ArrayList ();
			list.Add (stmt);
			emit_finally = false;
			base.StealFinallyClauses (ref list);
		}

		public bool EmitFinally {
			get { return emit_finally; }
		}

		public override LabeledStatement LookupLabel (string name, Location loc)
		{
			if (current_vector.Block == null)
				return base.LookupLabel (name, loc);

			LabeledStatement s = current_vector.Block.LookupLabel (name);
			if (s != null)
				return s;

			if (finally_vector != null) {
				Report.Error (157, loc, "Control cannot leave the body of a finally clause");
				return null;
			}

			return base.LookupLabel (name, loc);
		}

		public override void Label (UsageVector origin_vectors)
		{
			CurrentUsageVector.MergeOrigins (origin_vectors);
		}

		protected override UsageVector Merge ()
		{
			Report.Debug (2, "  MERGING TRY/CATCH", Name);
			UsageVector vector = UsageVector.MergeSiblings (catch_vectors, Location);
			Report.Debug (2, "  MERGING TRY/CATCH DONE", vector);

			if (finally_vector != null)
				vector.MergeChild (finally_vector, false);

			for (UsageVector origin = break_origins; origin != null; origin = origin.Next) {
				if (finally_vector != null)
					origin.MergeChild (finally_vector, false);
				if (!origin.Reachability.IsUnreachable)
					Parent.AddBreakOrigin (origin, origin.Location);
			}

			for (UsageVector origin = continue_origins; origin != null; origin = origin.Next) {
				if (finally_vector != null)
					origin.MergeChild (finally_vector, false);
				if (!origin.Reachability.IsUnreachable)
					Parent.AddContinueOrigin (origin, origin.Location);
			}

			for (UsageVector origin = return_origins; origin != null; origin = origin.Next) {
				if (finally_vector != null)
					origin.MergeChild (finally_vector, false);
				if (!origin.Reachability.IsUnreachable)
					Parent.AddReturnOrigin (origin, origin.Location);
			}

			return vector;
		}
	}

	// <summary>
	//   This is used by the flow analysis code to keep track of the type of local variables
	//   and variables.
	//
	//   The flow code uses a BitVector to keep track of whether a variable has been assigned
	//   or not.  This is easy for fundamental types (int, char etc.) or reference types since
	//   you can only assign the whole variable as such.
	//
	//   For structs, we also need to keep track of all its fields.  To do this, we allocate one
	//   bit for the struct itself (it's used if you assign/access the whole struct) followed by
	//   one bit for each of its fields.
	//
	//   This class computes this `layout' for each type.
	// </summary>
	public class TypeInfo
	{
		public readonly Type Type;

		// <summary>
		//   Total number of bits a variable of this type consumes in the flow vector.
		// </summary>
		public readonly int TotalLength;

		// <summary>
		//   Number of bits the simple fields of a variable of this type consume
		//   in the flow vector.
		// </summary>
		public readonly int Length;

		// <summary>
		//   This is only used by sub-structs.
		// </summary>
		public readonly int Offset;

		// <summary>
		//   If this is a struct.
		// </summary>
		public readonly bool IsStruct;	     

		// <summary>
		//   If this is a struct, all fields which are structs theirselves.
		// </summary>
		public TypeInfo[] SubStructInfo;

		protected readonly StructInfo struct_info;
		private static Hashtable type_hash = new Hashtable ();

		public static TypeInfo GetTypeInfo (Type type)
		{
			TypeInfo info = (TypeInfo) type_hash [type];
			if (info != null)
				return info;

			info = new TypeInfo (type);
			type_hash.Add (type, info);
			return info;
		}

		public static TypeInfo GetTypeInfo (TypeContainer tc)
		{
			TypeInfo info = (TypeInfo) type_hash [tc.TypeBuilder];
			if (info != null)
				return info;

			info = new TypeInfo (tc);
			type_hash.Add (tc.TypeBuilder, info);
			return info;
		}

		private TypeInfo (Type type)
		{
			this.Type = type;

			struct_info = StructInfo.GetStructInfo (type);
			if (struct_info != null) {
				Length = struct_info.Length;
				TotalLength = struct_info.TotalLength;
				SubStructInfo = struct_info.StructFields;
				IsStruct = true;
			} else {
				Length = 0;
				TotalLength = 1;
				IsStruct = false;
			}
		}

		private TypeInfo (TypeContainer tc)
		{
			this.Type = tc.TypeBuilder;

			struct_info = StructInfo.GetStructInfo (tc);
			if (struct_info != null) {
				Length = struct_info.Length;
				TotalLength = struct_info.TotalLength;
				SubStructInfo = struct_info.StructFields;
				IsStruct = true;
			} else {
				Length = 0;
				TotalLength = 1;
				IsStruct = false;
			}
		}

		protected TypeInfo (StructInfo struct_info, int offset)
		{
			this.struct_info = struct_info;
			this.Offset = offset;
			this.Length = struct_info.Length;
			this.TotalLength = struct_info.TotalLength;
			this.SubStructInfo = struct_info.StructFields;
			this.Type = struct_info.Type;
			this.IsStruct = true;
		}

		public int GetFieldIndex (string name)
		{
			if (struct_info == null)
				return 0;

			return struct_info [name];
		}

		public TypeInfo GetSubStruct (string name)
		{
			if (struct_info == null)
				return null;

			return struct_info.GetStructField (name);
		}

		// <summary>
		//   A struct's constructor must always assign all fields.
		//   This method checks whether it actually does so.
		// </summary>
		public bool IsFullyInitialized (FlowBranching branching, VariableInfo vi, Location loc)
		{
			if (struct_info == null)
				return true;

			bool ok = true;
			for (int i = 0; i < struct_info.Count; i++) {
				FieldInfo field = struct_info.Fields [i];

				if (!branching.IsFieldAssigned (vi, field.Name)) {
					Report.Error (171, loc,
						"Field `{0}' must be fully assigned before control leaves the constructor",
						TypeManager.GetFullNameSignature (field));
					ok = false;
				}
			}

			return ok;
		}

		public override string ToString ()
		{
			return String.Format ("TypeInfo ({0}:{1}:{2}:{3})",
					      Type, Offset, Length, TotalLength);
		}

		protected class StructInfo {
			public readonly Type Type;
			public readonly FieldInfo[] Fields;
			public readonly TypeInfo[] StructFields;
			public readonly int Count;
			public readonly int CountPublic;
			public readonly int CountNonPublic;
			public readonly int Length;
			public readonly int TotalLength;
			public readonly bool HasStructFields;

			private static Hashtable field_type_hash = new Hashtable ();
			private Hashtable struct_field_hash;
			private Hashtable field_hash;

			protected bool InTransit = false;

			// Private constructor.  To save memory usage, we only need to create one instance
			// of this class per struct type.
			private StructInfo (Type type)
			{
				this.Type = type;

				field_type_hash.Add (type, this);

				if (type is TypeBuilder) {
					TypeContainer tc = TypeManager.LookupTypeContainer (type);

					ArrayList fields = null;
					if (tc != null)
						fields = tc.Fields;

					ArrayList public_fields = new ArrayList ();
					ArrayList non_public_fields = new ArrayList ();

					if (fields != null) {
						foreach (FieldMember field in fields) {
							if ((field.ModFlags & Modifiers.STATIC) != 0)
								continue;
							if ((field.ModFlags & Modifiers.PUBLIC) != 0)
								public_fields.Add (field.FieldBuilder);
							else
								non_public_fields.Add (field.FieldBuilder);
						}
					}

					CountPublic = public_fields.Count;
					CountNonPublic = non_public_fields.Count;
					Count = CountPublic + CountNonPublic;

					Fields = new FieldInfo [Count];
					public_fields.CopyTo (Fields, 0);
					non_public_fields.CopyTo (Fields, CountPublic);
				} else if (type is GenericTypeParameterBuilder) {
					CountPublic = CountNonPublic = Count = 0;

					Fields = new FieldInfo [0];
				} else {
					FieldInfo[] public_fields = type.GetFields (
						BindingFlags.Instance|BindingFlags.Public);
					FieldInfo[] non_public_fields = type.GetFields (
						BindingFlags.Instance|BindingFlags.NonPublic);

					CountPublic = public_fields.Length;
					CountNonPublic = non_public_fields.Length;
					Count = CountPublic + CountNonPublic;

					Fields = new FieldInfo [Count];
					public_fields.CopyTo (Fields, 0);
					non_public_fields.CopyTo (Fields, CountPublic);
				}

				struct_field_hash = new Hashtable ();
				field_hash = new Hashtable ();

				Length = 0;
				StructFields = new TypeInfo [Count];
				StructInfo[] sinfo = new StructInfo [Count];

				InTransit = true;

				for (int i = 0; i < Count; i++) {
					FieldInfo field = (FieldInfo) Fields [i];

					sinfo [i] = GetStructInfo (field.FieldType);
					if (sinfo [i] == null)
						field_hash.Add (field.Name, ++Length);
					else if (sinfo [i].InTransit) {
						Report.Error (523, String.Format (
								      "Struct member `{0}.{1}' of type `{2}' causes " +
								      "a cycle in the structure layout",
								      type, field.Name, sinfo [i].Type));
						sinfo [i] = null;
						return;
					}
				}

				InTransit = false;

				TotalLength = Length + 1;
				for (int i = 0; i < Count; i++) {
					FieldInfo field = (FieldInfo) Fields [i];

					if (sinfo [i] == null)
						continue;

					field_hash.Add (field.Name, TotalLength);

					HasStructFields = true;
					StructFields [i] = new TypeInfo (sinfo [i], TotalLength);
					struct_field_hash.Add (field.Name, StructFields [i]);
					TotalLength += sinfo [i].TotalLength;
				}
			}

			public int this [string name] {
				get {
					if (field_hash.Contains (name))
						return (int) field_hash [name];
					else
						return 0;
				}
			}

			public TypeInfo GetStructField (string name)
			{
				return (TypeInfo) struct_field_hash [name];
			}

			public static StructInfo GetStructInfo (Type type)
			{
				if (!TypeManager.IsValueType (type) || TypeManager.IsEnumType (type) ||
				    TypeManager.IsBuiltinType (type))
					return null;

				StructInfo info = (StructInfo) field_type_hash [type];
				if (info != null)
					return info;

				return new StructInfo (type);
			}

			public static StructInfo GetStructInfo (TypeContainer tc)
			{
				StructInfo info = (StructInfo) field_type_hash [tc.TypeBuilder];
				if (info != null)
					return info;

				return new StructInfo (tc.TypeBuilder);
			}
		}
	}

	// <summary>
	//   This is used by the flow analysis code to store information about a single local variable
	//   or parameter.  Depending on the variable's type, we need to allocate one or more elements
	//   in the BitVector - if it's a fundamental or reference type, we just need to know whether
	//   it has been assigned or not, but for structs, we need this information for each of its fields.
	// </summary>
	public class VariableInfo {
		public readonly string Name;
		public readonly TypeInfo TypeInfo;

		// <summary>
		//   The bit offset of this variable in the flow vector.
		// </summary>
		public readonly int Offset;

		// <summary>
		//   The number of bits this variable needs in the flow vector.
		//   The first bit always specifies whether the variable as such has been assigned while
		//   the remaining bits contain this information for each of a struct's fields.
		// </summary>
		public readonly int Length;

		// <summary>
		//   If this is a parameter of local variable.
		// </summary>
		public readonly bool IsParameter;

		public readonly LocalInfo LocalInfo;
		public readonly int ParameterIndex;

		readonly VariableInfo Parent;
		VariableInfo[] sub_info;

		protected VariableInfo (string name, Type type, int offset)
		{
			this.Name = name;
			this.Offset = offset;
			this.TypeInfo = TypeInfo.GetTypeInfo (type);

			Length = TypeInfo.TotalLength;

			Initialize ();
		}

		protected VariableInfo (VariableInfo parent, TypeInfo type)
		{
			this.Name = parent.Name;
			this.TypeInfo = type;
			this.Offset = parent.Offset + type.Offset;
			this.Parent = parent;
			this.Length = type.TotalLength;

			this.IsParameter = parent.IsParameter;
			this.LocalInfo = parent.LocalInfo;
			this.ParameterIndex = parent.ParameterIndex;

			Initialize ();
		}

		protected void Initialize ()
		{
			TypeInfo[] sub_fields = TypeInfo.SubStructInfo;
			if (sub_fields != null) {
				sub_info = new VariableInfo [sub_fields.Length];
				for (int i = 0; i < sub_fields.Length; i++) {
					if (sub_fields [i] != null)
						sub_info [i] = new VariableInfo (this, sub_fields [i]);
				}
			} else
				sub_info = new VariableInfo [0];
		}

		public VariableInfo (LocalInfo local_info, int offset)
			: this (local_info.Name, local_info.VariableType, offset)
		{
			this.LocalInfo = local_info;
			this.IsParameter = false;
		}

		public VariableInfo (string name, Type type, int param_idx, int offset)
			: this (name, type, offset)
		{
			this.ParameterIndex = param_idx;
			this.IsParameter = true;
		}

		public bool IsAssigned (EmitContext ec)
		{
			return !ec.DoFlowAnalysis ||
				ec.OmitStructFlowAnalysis && TypeInfo.IsStruct ||
				ec.CurrentBranching.IsAssigned (this);
		}

		public bool IsAssigned (EmitContext ec, Location loc)
		{
			if (IsAssigned (ec))
				return true;

			Report.Error (165, loc,
				      "Use of unassigned local variable `" + Name + "'");
			ec.CurrentBranching.SetAssigned (this);
			return false;
		}

		public bool IsAssigned (MyBitVector vector)
		{
			if (vector == null)
				return true;

			if (vector [Offset])
				return true;

			for (VariableInfo parent = Parent; parent != null; parent = parent.Parent)
				if (vector [parent.Offset])
					return true;

			// Return unless this is a struct.
			if (!TypeInfo.IsStruct)
				return false;

			// Ok, so each field must be assigned.
			for (int i = 0; i < TypeInfo.Length; i++) {
				if (!vector [Offset + i + 1])
					return false;
			}

			// Ok, now check all fields which are structs.
			for (int i = 0; i < sub_info.Length; i++) {
				VariableInfo sinfo = sub_info [i];
				if (sinfo == null)
					continue;

				if (!sinfo.IsAssigned (vector))
					return false;
			}

			vector [Offset] = true;
			return true;
		}

		public void SetAssigned (EmitContext ec)
		{
			if (ec.DoFlowAnalysis)
				ec.CurrentBranching.SetAssigned (this);
		}

		public void SetAssigned (MyBitVector vector)
		{
			vector [Offset] = true;
		}

		public bool IsFieldAssigned (EmitContext ec, string name, Location loc)
		{
			if (!ec.DoFlowAnalysis ||
				ec.OmitStructFlowAnalysis && TypeInfo.IsStruct ||
				ec.CurrentBranching.IsFieldAssigned (this, name))
				return true;

			Report.Error (170, loc,
				      "Use of possibly unassigned field `" + name + "'");
			ec.CurrentBranching.SetFieldAssigned (this, name);
			return false;
		}

		public bool IsFieldAssigned (MyBitVector vector, string field_name)
		{
			int field_idx = TypeInfo.GetFieldIndex (field_name);

			if (field_idx == 0)
				return true;

			return vector [Offset + field_idx];
		}

		public void SetFieldAssigned (EmitContext ec, string name)
		{
			if (ec.DoFlowAnalysis)
				ec.CurrentBranching.SetFieldAssigned (this, name);
		}

		public void SetFieldAssigned (MyBitVector vector, string field_name)
		{
			int field_idx = TypeInfo.GetFieldIndex (field_name);

			if (field_idx == 0)
				return;

			vector [Offset + field_idx] = true;
		}

		public VariableInfo GetSubStruct (string name)
		{
			TypeInfo type = TypeInfo.GetSubStruct (name);

			if (type == null)
				return null;

			return new VariableInfo (this, type);
		}

		public override string ToString ()
		{
			return String.Format ("VariableInfo ({0}:{1}:{2}:{3}:{4})",
					      Name, TypeInfo, Offset, Length, IsParameter);
		}
	}

	// <summary>
	//   This is used by the flow code to hold the `layout' of the flow vector for
	//   all locals and all parameters (ie. we create one instance of this class for the
	//   locals and another one for the params).
	// </summary>
	public class VariableMap {
		// <summary>
		//   The number of variables in the map.
		// </summary>
		public readonly int Count;

		// <summary>
		//   Total length of the flow vector for this map.
		// <summary>
		public readonly int Length;

		VariableInfo[] map;

		public VariableMap (Parameters ip)
		{
			Count = ip != null ? ip.Count : 0;
			
			// Dont bother allocating anything!
			if (Count == 0)
				return;
			
			Length = 0;

			for (int i = 0; i < Count; i++) {
				Parameter.Modifier mod = ip.ParameterModifier (i);

				if ((mod & Parameter.Modifier.OUT) != Parameter.Modifier.OUT)
					continue;

				// Dont allocate till we find an out var.
				if (map == null)
					map = new VariableInfo [Count];

				map [i] = new VariableInfo (ip.ParameterName (i),
					TypeManager.GetElementType (ip.ParameterType (i)), i, Length);

				Length += map [i].Length;
			}
		}

		public VariableMap (LocalInfo[] locals)
			: this (null, locals)
		{ }

		public VariableMap (VariableMap parent, LocalInfo[] locals)
		{
			int offset = 0, start = 0;
			if (parent != null && parent.map != null) {
				offset = parent.Length;
				start = parent.Count;
			}

			Count = locals.Length + start;
			
			if (Count == 0)
				return;
			
			map = new VariableInfo [Count];
			Length = offset;

			if (parent != null && parent.map != null) {
				parent.map.CopyTo (map, 0);
			}

			for (int i = start; i < Count; i++) {
				LocalInfo li = locals [i-start];

				if (li.VariableType == null)
					continue;

				map [i] = li.VariableInfo = new VariableInfo (li, Length);
				Length += map [i].Length;
			}
		}

		// <summary>
		//   Returns the VariableInfo for variable @index or null if we don't need to
		//   compute assignment info for this variable.
		// </summary>
		public VariableInfo this [int index] {
			get {
				if (map == null)
					return null;
				
				return map [index];
			}
		}

		public override string ToString ()
		{
			return String.Format ("VariableMap ({0}:{1})", Count, Length);
		}
	}

	// <summary>
	//   This is a special bit vector which can inherit from another bit vector doing a
	//   copy-on-write strategy.  The inherited vector may have a smaller size than the
	//   current one.
	// </summary>
	public class MyBitVector {
		public readonly int Count;
		public MyBitVector InheritsFrom;
		public static readonly MyBitVector Empty = new MyBitVector ();

		BitArray vector;

		MyBitVector ()
		{
			InheritsFrom = null;
			Count = 0;
		}

		public MyBitVector (MyBitVector InheritsFrom, int Count)
		{
			if (InheritsFrom != null) {
				while (InheritsFrom.InheritsFrom != null)
					InheritsFrom = InheritsFrom.InheritsFrom;				
				if (InheritsFrom.Count >= Count && InheritsFrom.vector == null)
					InheritsFrom = null;
			}

			this.InheritsFrom = InheritsFrom;
			this.Count = Count;
		}

		// <summary>
		//   Get/set bit `index' in the bit vector.
		// </summary>
		public bool this [int index]
		{
			get {
				if (index >= Count)
					throw new ArgumentOutOfRangeException ();

				// We're doing a "copy-on-write" strategy here; as long
				// as nobody writes to the array, we can use our parent's
				// copy instead of duplicating the vector.

				if (vector != null)
					return vector [index];
				if (InheritsFrom == null)
					return true;
				if (index < InheritsFrom.Count)
					return InheritsFrom [index];
				return false;
			}

			set {
				// Only copy the vector if we're actually modifying it.
				if (this [index] != value) {
					if (vector == null)
						initialize_vector ();
					vector [index] = value;
				}
			}
		}

		// <summary>
		//   Performs an `or' operation on the bit vector.  The `new_vector' may have a
		//   different size than the current one.
		// </summary>
		private void Or (MyBitVector new_vector)
		{
			int min = new_vector.Count;
			if (Count < min)
				min = Count;

			for (int i = 0; i < min; i++)
				this [i] |= new_vector [i];
		}

		// <summary>
		//   Perfonrms an `and' operation on the bit vector.  The `new_vector' may have
		//   a different size than the current one.
		// </summary>
		private void And (MyBitVector new_vector)
		{
			int min = new_vector.Count;
			if (Count < min)
				min = Count;

			for (int i = 0; i < min; i++)
				this [i] &= new_vector [i];

			for (int i = min; i < Count; i++)
				this [i] = false;
		}

		public static void And (ref MyBitVector target, MyBitVector vector)
		{
			if (vector == null)
				return;
			if (target == null)
				target = vector.Clone ();
			else
				target.And (vector);
		}

		public static void Or (ref MyBitVector target, MyBitVector vector)
		{
			if (target == null)
				return;
			if (vector == null)
				target.SetAll (true);
			else
				target.Or (vector);
		}

		// <summary>
		//   This does a deep copy of the bit vector.
		// </summary>
		public MyBitVector Clone ()
		{
			if (Count == 0)
				return Empty;
			MyBitVector retval = new MyBitVector (this, Count);
			retval.initialize_vector ();
			return retval;
		}

		public void SetAll (bool value)
		{
			InheritsFrom = value ? null : Empty;
			vector = null;
		}

		void initialize_vector ()
		{
			if (InheritsFrom == null) {
				vector = new BitArray (Count, true);
				return;
			}

			vector = new BitArray (Count, false);

			int min = InheritsFrom.Count;
			if (min > Count)
				min = Count;

			for (int i = 0; i < min; i++)
				vector [i] = InheritsFrom [i];

			InheritsFrom = null;
		}

		StringBuilder Dump (StringBuilder sb)
		{
			if (vector == null)
				return InheritsFrom == null ? sb.Append ("/") : InheritsFrom.Dump (sb.Append ("="));
			for (int i = 0; i < Count; i++)
				sb.Append (this [i] ? "1" : "0");
			return sb;
		}

		public override string ToString ()
		{
			return Dump (new StringBuilder ("{")).Append ("}").ToString ();
		}
	}
}

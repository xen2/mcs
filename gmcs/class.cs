//
// class.cs: Class and Struct handlers
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@ximian.com)
//          Marek Safar (marek.safar@seznam.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell, Inc
//
//
//  2002-10-11  Miguel de Icaza  <miguel@ximian.com>
//
//	* class.cs: Following the comment from 2002-09-26 to AddMethod, I
//	have fixed a remaining problem: not every AddXXXX was adding a
//	fully qualified name.  
//
//	Now everyone registers a fully qualified name in the DeclSpace as
//	being defined instead of the partial name.  
//
//	Downsides: we are slower than we need to be due to the excess
//	copies and the names being registered this way.  
//
//	The reason for this is that we currently depend (on the corlib
//	bootstrap for instance) that types are fully qualified, because
//	we dump all the types in the namespace, and we should really have
//	types inserted into the proper namespace, so we can only store the
//	basenames in the defined_names array.
//
//
#define CACHE
using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Xml;

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {

	public enum Kind {
		Root,
		Struct,
		Class,
		Interface
	}

	/// <summary>
	///   This is the base class for structs and classes.  
	/// </summary>
	public abstract class TypeContainer : DeclSpace, IMemberContainer {

 		public class MemberCoreArrayList: ArrayList
 		{
			/// <summary>
			///   Defines the MemberCore objects that are in this array
			/// </summary>
			public virtual void DefineContainerMembers ()
			{
				foreach (MemberCore mc in this) {
					mc.Define ();
				}
			}

			public virtual void Emit ()
			{
				foreach (MemberCore mc in this)
					mc.Emit ();
			}
 		}

 		public class MethodArrayList: MemberCoreArrayList
 		{
 			[Flags]
 			enum CachedMethods {
 				Equals			= 1,
 				GetHashCode		= 1 << 1
 			}
 
 			CachedMethods cached_method;
			TypeContainer container;

			public MethodArrayList (TypeContainer container)
			{
				this.container = container;
			}
 
 			/// <summary>
 			/// Method container contains Equals method
 			/// </summary>
 			public bool HasEquals {
 				set {
 					cached_method |= CachedMethods.Equals;
 				}
 
 				get {
 					return (cached_method & CachedMethods.Equals) != 0;
 				}
 			}
 
 			/// <summary>
 			/// Method container contains GetHashCode method
 			/// </summary>
 			public bool HasGetHashCode {
 				set {
 					cached_method |= CachedMethods.GetHashCode;
 				}
 
 				get {
 					return (cached_method & CachedMethods.GetHashCode) != 0;
 				}
 			}
 
 			public override void DefineContainerMembers ()
 			{
 				base.DefineContainerMembers ();
 
 				if ((RootContext.WarningLevel >= 3) && HasEquals && !HasGetHashCode) {
 					Report.Warning (659, container.Location, "'{0}' overrides Object.Equals(object) but does not override Object.GetHashCode()", container.GetSignatureForError ());
 				}
 			}
 
 		}

		public sealed class IndexerArrayList: MemberCoreArrayList
		{
			/// <summary>
			/// The indexer name for this container
			/// </summary>
 			public string IndexerName = DefaultIndexerName;

			bool seen_normal_indexers = false;

			TypeContainer container;

			public IndexerArrayList (TypeContainer container)
			{
				this.container = container;
			}

			/// <summary>
			/// Defines the indexers, and also verifies that the IndexerNameAttribute in the
			/// class is consistent.  Either it is `Item' or it is the name defined by all the
			/// indexers with the `IndexerName' attribute.
			///
			/// Turns out that the IndexerNameAttribute is applied to each indexer,
			/// but it is never emitted, instead a DefaultMember attribute is attached
			/// to the class.
			/// </summary>
			public override void DefineContainerMembers()
			{
				base.DefineContainerMembers ();

				string class_indexer_name = null;

				//
				// If there's both an explicit and an implicit interface implementation, the
				// explicit one actually implements the interface while the other one is just
				// a normal indexer.  See bug #37714.
				//

				// Invariant maintained by AddIndexer(): All explicit interface indexers precede normal indexers
				foreach (Indexer i in this) {
					if (i.InterfaceType != null) {
						if (seen_normal_indexers)
							throw new Exception ("Internal Error: 'Indexers' array not sorted properly.");
						continue;
					}

					seen_normal_indexers = true;

					if (class_indexer_name == null) {
						class_indexer_name = i.ShortName;
						continue;
					}

					if (i.ShortName != class_indexer_name)
						Report.Error (668, i.Location, "Two indexers have different names; the IndexerName attribute must be used with the same name on every indexer within a type");
				}

				if (class_indexer_name != null)
					IndexerName = class_indexer_name;
			}

			public override void Emit ()
			{
				base.Emit ();

				if (!seen_normal_indexers)
					return;

				CustomAttributeBuilder cb = new CustomAttributeBuilder (TypeManager.default_member_ctor, new string [] { IndexerName });
				container.TypeBuilder.SetCustomAttribute (cb);
			}
		}

 		public class OperatorArrayList: MemberCoreArrayList
		{
			TypeContainer container;

			public OperatorArrayList (TypeContainer container)
			{
				this.container = container;
			}

			//
			// Operator pair checking
			//
			class OperatorEntry
			{
				public int flags;
				public Type ret_type;
				public Type type1, type2;
				public Operator op;
				public Operator.OpType ot;
				
				public OperatorEntry (int f, Operator o)
				{
					flags = f;

					ret_type = o.OperatorMethod.ReturnType;
					Type [] pt = o.OperatorMethod.ParameterTypes;
					type1 = pt [0];
					type2 = pt [1];
					op = o;
					ot = o.OperatorType;
				}

				public override int GetHashCode ()
				{	
					return ret_type.GetHashCode ();
				}

				public override bool Equals (object o)
				{
					OperatorEntry other = (OperatorEntry) o;

					if (other.ret_type != ret_type)
						return false;
					if (other.type1 != type1)
						return false;
					if (other.type2 != type2)
						return false;
					return true;
				}
			}
				
			//
			// Checks that some operators come in pairs:
			//  == and !=
			// > and <
			// >= and <=
			// true and false
			//
			// They are matched based on the return type and the argument types
			//
			void CheckPairedOperators ()
			{
				Hashtable pairs = new Hashtable (null, null);
				Operator true_op = null;
				Operator false_op = null;
				bool has_equality_or_inequality = false;
				
				// Register all the operators we care about.
				foreach (Operator op in this){
					int reg = 0;

					// Skip erroneous code.
					if (op.OperatorMethod == null)
						continue;

					switch (op.OperatorType){
					case Operator.OpType.Equality:
						reg = 1;
						has_equality_or_inequality = true;
						break;
					case Operator.OpType.Inequality:
						reg = 2;
						has_equality_or_inequality = true;
						break;

					case Operator.OpType.True:
						true_op = op;
						break;
					case Operator.OpType.False:
						false_op = op;
						break;
						
					case Operator.OpType.GreaterThan:
						reg = 1; break;
					case Operator.OpType.LessThan:
						reg = 2; break;
						
					case Operator.OpType.GreaterThanOrEqual:
						reg = 1; break;
					case Operator.OpType.LessThanOrEqual:
						reg = 2; break;
					}
					if (reg == 0)
						continue;

					OperatorEntry oe = new OperatorEntry (reg, op);

					object o = pairs [oe];
					if (o == null)
						pairs [oe] = oe;
					else {
						oe = (OperatorEntry) o;
						oe.flags |= reg;
					}
				}

				if (true_op != null){
					if (false_op == null)
						Report.Error (216, true_op.Location, "operator true requires a matching operator false");
				} else if (false_op != null)
					Report.Error (216, false_op.Location, "operator false requires a matching operator true");
				
				//
				// Look for the mistakes.
				//
				foreach (DictionaryEntry de in pairs){
					OperatorEntry oe = (OperatorEntry) de.Key;

					if (oe.flags == 3)
						continue;

					string s = "";
					switch (oe.ot){
					case Operator.OpType.Equality:
						s = "!=";
						break;
					case Operator.OpType.Inequality: 
						s = "==";
						break;
					case Operator.OpType.GreaterThan: 
						s = "<";
						break;
					case Operator.OpType.LessThan:
						s = ">";
						break;
					case Operator.OpType.GreaterThanOrEqual:
						s = "<=";
						break;
					case Operator.OpType.LessThanOrEqual:
						s = ">=";
						break;
					}
					Report.Error (216, oe.op.Location,
							"The operator `" + oe.op + "' requires a matching operator `" + s + "' to also be defined");
				}

 				if (has_equality_or_inequality && (RootContext.WarningLevel > 2)) {
 					if (container.Methods == null || !container.Methods.HasEquals)
 						Report.Warning (660, container.Location, "'{0}' defines operator == or operator != but does not override Object.Equals(object o)", container.GetSignatureForError ());
 
 					if (container.Methods == null || !container.Methods.HasGetHashCode)
 						Report.Warning (661, container.Location, "'{0}' defines operator == or operator != but does not override Object.GetHashCode()", container.GetSignatureForError ());
 				}
			}

	 		public override void DefineContainerMembers ()
	 		{
	 			base.DefineContainerMembers ();
	 			CheckPairedOperators ();
			}
		}


		// Whether this is a struct, class or interface
		public readonly Kind Kind;

		// Holds a list of classes and structures
		ArrayList types;

		// Holds the list of properties
		MemberCoreArrayList properties;

		// Holds the list of enumerations
		MemberCoreArrayList enums;

		// Holds the list of delegates
		MemberCoreArrayList delegates;
		
		// Holds the list of constructors
		protected MemberCoreArrayList instance_constructors;

		// Holds the list of fields
		MemberCoreArrayList fields;

		// Holds a list of fields that have initializers
		protected ArrayList initialized_fields;

		// Holds a list of static fields that have initializers
		protected ArrayList initialized_static_fields;

		// Holds the list of constants
		MemberCoreArrayList constants;

		// Holds the list of
		MemberCoreArrayList interfaces;

		// Holds the methods.
		MethodArrayList methods;

		// Holds the events
		protected MemberCoreArrayList events;

		// Holds the indexers
		IndexerArrayList indexers;

		// Holds the operators
		MemberCoreArrayList operators;

		// Holds the iterators
		ArrayList iterators;

		// Holds the parts of a partial class;
		ArrayList parts;

		//
		// Pointers to the default constructor and the default static constructor
		//
		protected Constructor default_constructor;
		protected Constructor default_static_constructor;

		//
		// Whether we have at least one non-static field
		//
		bool have_nonstatic_fields = false;
		
		//
		// This one is computed after we can distinguish interfaces
		// from classes from the arraylist `type_bases' 
		//
		string base_class_name;
		TypeExpr base_type;
		TypeExpr[] iface_exprs;

		ArrayList type_bases;

		bool members_defined;
		bool members_defined_ok;

		// The interfaces we implement.
		protected Type[] ifaces;
		protected Type ptype;

		// The base member cache and our member cache
		MemberCache base_cache;
		MemberCache member_cache;

		public const string DefaultIndexerName = "Item";

		Type GenericType;
		GenericTypeParameterBuilder[] gen_params;

		public TypeContainer (NamespaceEntry ns, TypeContainer parent, MemberName name,
				      Attributes attrs, Kind kind, Location l)
			: base (ns, parent, name, attrs, l)
		{
			if (parent != null && parent != RootContext.Tree.Types && parent.NamespaceEntry != ns)
				throw new InternalErrorException ("A nested type should be in the same NamespaceEntry as its enclosing class");

			this.Kind = kind;

			types = new ArrayList ();

			base_class_name = null;
		}

		public bool AddToMemberContainer (MemberCore symbol)
		{
			return AddToContainer (symbol, symbol.Name);
		}

		bool AddToTypeContainer (DeclSpace ds)
		{
			// Parent == null ==> this == RootContext.Tree.Types
			return AddToContainer (ds, (Parent == null) ? ds.Name : ds.Basename);
		}

		public void AddConstant (Const constant)
		{
			if (!AddToMemberContainer (constant))
				return;

			if (constants == null)
				constants = new MemberCoreArrayList ();

			constants.Add (constant);
		}

		public void AddEnum (Mono.CSharp.Enum e)
		{
			if (!AddToTypeContainer (e))
				return;

			if (enums == null)
				enums = new MemberCoreArrayList ();

			enums.Add (e);
		}
		
		public void AddClassOrStruct (TypeContainer c)
		{
			if (!AddToTypeContainer (c))
				return;

			types.Add (c);
		}

		public void AddDelegate (Delegate d)
		{
			if (!AddToTypeContainer (d))
				return;

			if (delegates == null)
				delegates = new MemberCoreArrayList ();
			
			delegates.Add (d);
		}

		public void AddMethod (Method method)
		{
			if (!AddToMemberContainer (method))
				return;

			if (methods == null)
				methods = new MethodArrayList (this);

			if (method.MemberName.Left != null)
				methods.Insert (0, method);
			else 
				methods.Add (method);
		}

		public void AddConstructor (Constructor c)
		{
			if (c.Name != MemberName.Name) {
				Report.Error (1520, c.Location, "Class, struct, or interface method must have a return type");
			}

			bool is_static = (c.ModFlags & Modifiers.STATIC) != 0;
			
			if (is_static){
				if (default_static_constructor != null) {
					Report.SymbolRelatedToPreviousError (default_static_constructor);
					Report.Error (111, c.Location, "Type '{0}' already defines a member " +
						      "called '{1}' with the same parameter types", Name, c.Name);
					return;
				}

				default_static_constructor = c;
			} else {
				if (c.IsDefault ()){
					if (default_constructor != null) {
						Report.SymbolRelatedToPreviousError (default_constructor);
						Report.Error (111, c.Location, "Type '{0}' already defines a member " +
						      "called '{1}' with the same parameter types", Name, c.Name);
						return;
					}
					default_constructor = c;
				}
				
				if (instance_constructors == null)
					instance_constructors = new MemberCoreArrayList ();
				
				instance_constructors.Add (c);
			}
		}
		
		public void AddInterface (TypeContainer iface)
		{
			if (!AddToTypeContainer (iface))
				return;

			if (interfaces == null) {
				interfaces = new MemberCoreArrayList ();
			}

			interfaces.Add (iface);
		}

		public void AddField (FieldMember field)
		{
			if (!AddToMemberContainer (field))
				return;

			if (fields == null)
				fields = new MemberCoreArrayList ();
			
			fields.Add (field);
			
			if (field.HasInitializer)
				RegisterFieldForInitialization (field);

			if ((field.ModFlags & Modifiers.STATIC) == 0)
				have_nonstatic_fields = true;
		}

		public void AddProperty (Property prop)
		{
			if (!AddToMemberContainer (prop) || 
				!AddToMemberContainer (prop.Get) || !AddToMemberContainer (prop.Set))
				return;

			if (properties == null)
				properties = new MemberCoreArrayList ();

			if (prop.MemberName.Left != null)
				properties.Insert (0, prop);
			else
				properties.Add (prop);
		}

		public void AddEvent (Event e)
		{
			if (!AddToMemberContainer (e))
				return;

			if (e is EventProperty) {
				if (!AddToMemberContainer (e.Add))
					return;

				if (!AddToMemberContainer (e.Remove))
					return;
			}

			if (events == null)
				events = new MemberCoreArrayList ();

			events.Add (e);
		}

		/// <summary>
		/// Indexer has special handling in constrast to other AddXXX because the name can be driven by IndexerNameAttribute
		/// </summary>
		public void AddIndexer (Indexer i)
		{
			if (indexers == null)
				indexers = new IndexerArrayList (this);

			if (i.IsExplicitImpl)
				indexers.Insert (0, i);
			else
				indexers.Add (i);
		}

		public void AddOperator (Operator op)
		{
			if (!AddToMemberContainer (op))
				return;

			if (operators == null)
				operators = new OperatorArrayList (this);

			operators.Add (op);
		}

		public void AddIterator (Iterator i)
		{
			if (iterators == null)
				iterators = new ArrayList ();

			iterators.Add (i);
		}

		public void AddType (TypeContainer tc)
		{
			types.Add (tc);
		}

		public void AddPart (ClassPart part)
		{
			if (parts == null)
				parts = new ArrayList ();

			parts.Add (part);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.default_member_type) {
				if (Indexers != null) {
					Report.Error (646, a.Location,
						      "Cannot specify the DefaultMember attribute on" +
						      " a type containing an indexer");
					return;
				}
			}
			
			base.ApplyAttributeBuilder (a, cb);
               } 

		public override AttributeTargets AttributeTargets {
			get {
				switch (Kind) {
				case Kind.Class:
					return AttributeTargets.Class;
				case Kind.Struct:
					return AttributeTargets.Struct;
				case Kind.Interface:
					return AttributeTargets.Interface;
				default:
					throw new NotSupportedException ();
				}
			}
		}

		public ArrayList Types {
			get {
				return types;
			}
		}

		public MethodArrayList Methods {
			get {
				return methods;
			}
		}

		public ArrayList Constants {
			get {
				return constants;
			}
		}

		public ArrayList Interfaces {
			get {
				return interfaces;
			}
		}

		public ArrayList Iterators {
			get {
				return iterators;
			}
		}
		
		public string Base {
			get {
				return base_class_name;
			}
		}
		
		public ArrayList Bases {
			get {
				return type_bases;
			}

			set {
				type_bases = value;
			}
		}

		public ArrayList Fields {
			get {
				return fields;
			}
		}

		public ArrayList InstanceConstructors {
			get {
				return instance_constructors;
			}
		}

		public ArrayList Properties {
			get {
				return properties;
			}
		}

		public ArrayList Events {
			get {
				return events;
			}
		}
		
		public ArrayList Enums {
			get {
				return enums;
			}
		}

		public ArrayList Indexers {
			get {
				return indexers;
			}
		}

		public ArrayList Operators {
			get {
				return operators;
			}
		}

		public ArrayList Delegates {
			get {
				return delegates;
			}
		}
		
		public ArrayList Parts {
			get {
				return parts;
			}
		}

		protected override TypeAttributes TypeAttr {
			get {
				return Modifiers.TypeAttr (ModFlags, this) | base.TypeAttr;
			}
		}

		public string IndexerName {
			get {
				return indexers == null ? DefaultIndexerName : indexers.IndexerName;
			}
		}

		public virtual void RegisterFieldForInitialization (FieldMember field)
		{
			if ((field.ModFlags & Modifiers.STATIC) != 0){
				if (initialized_static_fields == null)
					initialized_static_fields = new ArrayList ();

				initialized_static_fields.Add (field);
			} else {
				if (initialized_fields == null)
					initialized_fields = new ArrayList ();

				initialized_fields.Add (field);
			}
		}

		//
		// Emits the instance field initializers
		//
		public virtual bool EmitFieldInitializers (EmitContext ec)
		{
			ArrayList fields;
			Expression instance_expr;
			
			if (ec.IsStatic){
				fields = initialized_static_fields;
				instance_expr = null;
			} else {
				fields = initialized_fields;
				instance_expr = new This (Location.Null).Resolve (ec);
			}

			if (fields == null)
				return true;

			foreach (FieldMember f in fields){
				Expression e = f.GetInitializerExpression (ec);
				if (e == null)
					return false;

				Location l = f.Location;
				FieldExpr fe = new FieldExpr (f.FieldBuilder, l, true);
				fe.InstanceExpression = instance_expr;

				ExpressionStatement a = new Assign (fe, e, l);

				a = a.ResolveStatement (ec);
				if (a == null)
					return false;

				Constant c = e as Constant;
				if (c != null) {
					if (c.IsDefaultValue)
						continue;
				}

				a.EmitStatement (ec);
			}

			return true;
		}
		
		//
		// Defines the default constructors
		//
		protected void DefineDefaultConstructor (bool is_static)
		{
			Constructor c;

			// The default constructor is public
			// If the class is abstract, the default constructor is protected
			// The default static constructor is private

			int mods = Modifiers.PUBLIC;
			if (is_static)
				mods = Modifiers.STATIC | Modifiers.PRIVATE;
			else if ((ModFlags & Modifiers.ABSTRACT) != 0)
				mods = Modifiers.PROTECTED;

			TypeContainer constructor_parent = this;
			if (Parts != null)
				constructor_parent = (TypeContainer) Parts [0];

			c = new Constructor (constructor_parent, MemberName.Name, mods,
					     Parameters.EmptyReadOnlyParameters,
					     new ConstructorBaseInitializer (null, Location),
					     Location);
			
			AddConstructor (c);
			
			c.Block = new ToplevelBlock (null, Location);
			
		}

		/// <remarks>
		///  The pending methods that need to be implemented
		//   (interfaces or abstract methods)
		/// </remarks>
		public PendingImplementation Pending;

		public abstract PendingImplementation GetPendingImplementations ();

		TypeExpr[] GetPartialBases (out TypeExpr base_class)
		{
			ArrayList ifaces = new ArrayList ();

			base_class = null;
			Location base_loc = Location.Null;

			foreach (ClassPart part in parts) {
				TypeExpr new_base_class;
				TypeExpr[] new_ifaces;

				new_ifaces = part.GetClassBases (out new_base_class);
				if (new_ifaces == null && new_base_class != null)
					return null;

				if ((base_class != null) && (new_base_class != null) &&
				    !base_class.Equals (new_base_class)) {
					Report.Error (263, part.Location,
						      "Partial declarations of `{0}' must " +
						      "not specify different base classes",
						      Name);

					if (!Location.IsNull (base_loc))
						Report.LocationOfPreviousError (base_loc);

					return null;
				}

				if ((base_class == null) && (new_base_class != null)) {
					base_class = new_base_class;
					base_loc = part.Location;
				}

				if (new_ifaces == null)
					continue;

				foreach (TypeExpr iface in new_ifaces) {
					bool found = false;
					foreach (TypeExpr old_iface in ifaces) {
						if (old_iface.Equals (iface)) {
							found = true;
							break;
						}
					}

					if (!found)
						ifaces.Add (iface);
				}
			}

			TypeExpr[] retval = new TypeExpr [ifaces.Count];
			ifaces.CopyTo (retval, 0);
			return retval;
		}

		TypeExpr[] GetNormalBases (out TypeExpr base_class)
		{
			base_class = null;

			int count = Bases.Count;
			int start = 0, i, j;

			if (Kind == Kind.Class){
				TypeExpr name = ResolveBaseTypeExpr (
					(Expression) Bases [0], false, Location);

				if (name == null){
					return null;
				}

				if (!name.IsInterface) {
					// base_class could be a class, struct, enum, delegate.
					// This is validated in GetClassBases.
					base_class = name;
					start = 1;
				}
			}

			TypeExpr [] ifaces = new TypeExpr [count-start];
			
			for (i = start, j = 0; i < count; i++, j++){
				TypeExpr resolved = ResolveBaseTypeExpr ((Expression) Bases [i], false, Location);
				if (resolved == null) {
					return null;
				}
				
				ifaces [j] = resolved;
			}

			return ifaces;
		}

		/// <summary>
		///   This function computes the Base class and also the
		///   list of interfaces that the class or struct @c implements.
		///   
		///   The return value is an array (might be null) of
		///   interfaces implemented (as Types).
		///   
		///   The @base_class argument is set to the base object or null
		///   if this is `System.Object'. 
		/// </summary>
		TypeExpr [] GetClassBases (out TypeExpr base_class)
		{
			int i;

			TypeExpr[] ifaces;

			if (parts != null)
				ifaces = GetPartialBases (out base_class);
			else if (Bases == null){
				base_class = null;
				return null;
			} else
				ifaces = GetNormalBases (out base_class);

			if (ifaces == null)
				return null;

			if ((base_class != null) && (Kind == Kind.Class)){
				if (base_class is TypeParameterExpr){
					Report.Error (
						689, base_class.Location,
						"Type parameter `{0}' can not be used as a " +
						"base class or interface", base_class.Name);
					error = true;
					return null;
				}

				if (base_class.Type.IsArray || base_class.Type.IsPointer) {
					Report.Error (1521, base_class.Location, "Invalid base type");
					return null;
				}

				if (base_class.IsSealed){
					Report.SymbolRelatedToPreviousError (base_class.Type);
					if (base_class.Type.IsAbstract) {
						Report.Error (709, Location, "'{0}': Cannot derive from static class", GetSignatureForError ());
					} else {
						Report.Error (509, Location, "'{0}': Cannot derive from sealed class", GetSignatureForError ());
					}
					return null;
				}

				if (!base_class.CanInheritFrom ()){
					Report.Error (644, Location,
						      "`{0}' cannot inherit from special class `{1}'",
						      Name, base_class.Name);
					return null;
				}

				if (!base_class.AsAccessible (this, ModFlags)) {
					Report.SymbolRelatedToPreviousError (base_class.Type);
					Report.Error (60, Location, "Inconsistent accessibility: base class '{0}' is less accessible than class '{1}'", 
						TypeManager.CSharpName (base_class.Type), GetSignatureForError ());
				}
			}

			if (base_class != null)
				base_class_name = base_class.Name;

			if (ifaces == null)
				return null;

			int count = ifaces != null ? ifaces.Length : 0;

			for (i = 0; i < count; i++) {
				TypeExpr iface = (TypeExpr) ifaces [i];

				if (!iface.IsInterface) {
					if (Kind != Kind.Class) {
						// TODO: location of symbol related ....
						Error_TypeInListIsNotInterface (Location, iface.FullName);
					}
					else if (base_class != null)
						Report.Error (1721, Location,
							      "In Class `{0}', `{1}' is not an interface, and a base class has already been defined",
							      Name, iface.Name);
					else {
						Report.Error (1722, Location,
							      "In Class `{0}', `{1}' is not " +
							      "an interface, a base class must be listed first", Name, iface.Name);
					}
					return null;
				}
  
				for (int x = 0; x < i; x++) {
					if (iface.Equals (ifaces [x])) {
						Report.Error (528, Location,
							      "`{0}' is already listed in " +
							      "interface list", iface.Name);
						return null;
					}
				}

				if ((Kind == Kind.Interface) &&
				    !iface.AsAccessible (Parent, ModFlags)) {
					Report.Error (61, Location,
						      "Inconsistent accessibility: base " +
						      "interface `{0}' is less accessible " +
						      "than interface `{1}'", iface.Name,
						      Name);
					return null;
				}
			}
			return ifaces;
		}

		bool CheckGenericInterfaces (Type[] ifaces)
		{
			ArrayList already_checked = new ArrayList ();

			for (int i = 0; i < ifaces.Length; i++) {
				Type iface = ifaces [i];
				foreach (Type t in already_checked) {
					if (iface == t)
						continue;

					Type[] infered = new Type [CountTypeParameters];
					if (!TypeManager.MayBecomeEqualGenericInstances (iface, t, infered, null))
						continue;

					Report.Error (
						695, Location,
						"`{0}' cannot implement both `{1}' and `{2}' " +
						"because they may unify for some type " +
						"parameter substitutions",
						TypeManager.GetFullName (TypeBuilder),
						iface, t);
					return false;
				}

				already_checked.Add (iface);
			}

			return true;
		}

		bool error = false;
		
		protected void Error_TypeInListIsNotInterface (Location loc, string type)
		{
			Report.Error (527, loc, "'{0}': type in interface list is not an interface", type);
		}

		//
		// Defines the type in the appropriate ModuleBuilder or TypeBuilder.
		//
		public override TypeBuilder DefineType ()
		{
			if (error)
				return null;

			if (TypeBuilder != null)
				return TypeBuilder;

			TypeAttributes type_attributes = TypeAttr;

			try {
				if (IsTopLevel){
					if (TypeManager.NamespaceClash (Name, Location)) {
						error = true;
						return null;
					}

					ModuleBuilder builder = CodeGen.Module.Builder;
					TypeBuilder = builder.DefineType (
						Name, type_attributes, null, null);
				} else {
					TypeBuilder builder = Parent.TypeBuilder;
					if (builder == null) {
						error = true;
						return null;
					}

					TypeBuilder = builder.DefineNestedType (
						Basename, type_attributes, ptype, null);
				}
			} catch (ArgumentException) {
				Report.RuntimeMissingSupport (Location, "static classes");
				error = true;
				return null;
			}

			TypeManager.AddUserType (Name, TypeBuilder, this);

			if (Parts != null) {
				ec = null;
				foreach (ClassPart part in Parts) {
					part.TypeBuilder = TypeBuilder;
					part.ptype = ptype;
					part.ec = new EmitContext (part, Mono.CSharp.Location.Null, null, null, ModFlags);
					part.ec.ContainerType = TypeBuilder;
				}
			} else {
				//
				// Normally, we create the EmitContext here.
				// The only exception is if we're an Iterator - in this case,
				// we already have the `ec', so we don't want to create a new one.
				//
				if (ec == null)
					ec = new EmitContext (this, Mono.CSharp.Location.Null, null, null, ModFlags);
				ec.ContainerType = TypeBuilder;
			}

			if (IsGeneric) {
				string[] param_names = new string [TypeParameters.Length];
				for (int i = 0; i < TypeParameters.Length; i++)
					param_names [i] = TypeParameters [i].Name;

				gen_params = TypeBuilder.DefineGenericParameters (param_names);

				int offset = CountTypeParameters - CurrentTypeParameters.Length;
				for (int i = offset; i < gen_params.Length; i++)
					CurrentTypeParameters [i - offset].Define (gen_params [i]);
			}

			iface_exprs = GetClassBases (out base_type);
			if (iface_exprs == null && base_type != null) {
				error = true;
				return null;
			}

			if (base_type == null) {
				if (Kind == Kind.Class){
					if (RootContext.StdLib)
						base_type = TypeManager.system_object_expr;
					else if (Name != "System.Object")
						base_type = TypeManager.system_object_expr;
				} else if (Kind == Kind.Struct){
					//
					// If we are compiling our runtime,
					// and we are defining ValueType, then our
					// base is `System.Object'.
					//
					if (!RootContext.StdLib && Name == "System.ValueType")
						base_type = TypeManager.system_object_expr;
					else if (Kind == Kind.Struct)
						base_type = TypeManager.system_valuetype_expr;
				}
			}

			if (base_type != null) {
				// FIXME: I think this should be ...ResolveType (Parent.EmitContext).
				//        However, if Parent == RootContext.Tree.Types, its NamespaceEntry will be null.
				ptype = base_type.ResolveType (TypeResolveEmitContext);
				if (ptype == null) {
					error = true;
					return null;
				}

				ptype = base_type.Type;

				if (IsGeneric && TypeManager.IsAttributeType (ptype)) {
					Report.Error (698, base_type.Location,
						      "A generic type cannot derive from `{0}' " +
						      "because it is an attribute class",
						      base_type.Name);
					error = true;
					return null;
				}
			}

			if (!CheckRecursiveDefinition (this)) {
				error = true;
				return null;
			}

			if (ptype != null)
				TypeBuilder.SetParent (ptype);

			// add interfaces that were not added at type creation
			if (iface_exprs != null) {
				// FIXME: I think this should be ...ExpandInterfaces (Parent.EmitContext, ...).
				//        However, if Parent == RootContext.Tree.Types, its NamespaceEntry will be null.
				TypeResolveEmitContext.ContainerType = TypeBuilder;
				ifaces = TypeManager.ExpandInterfaces (TypeResolveEmitContext, iface_exprs);
				if (ifaces == null) {
					error = true;
					return null;
				}

				foreach (Type itype in ifaces)
 					TypeBuilder.AddInterfaceImplementation (itype);

				if (!CheckGenericInterfaces (ifaces)) {
					error = true;
					return null;
				}

				TypeManager.RegisterBuilder (TypeBuilder, ifaces);
			}

			if (!(this is Iterator))
				RootContext.RegisterOrder (this); 
			else if (!ResolveType ()) {
				error = true;
				return null;
			}

			if (!DefineNestedTypes ()) {
				error = true;
				return null;
			}

			return TypeBuilder;
		}

		public bool ResolveType ()
		{
			if (!IsGeneric)
				return true;

			TypeExpr current_type = null;
			if (Parts != null) {
				foreach (ClassPart part in Parts) {
					if (!part.DefineTypeParameters ()) {
						error = true;
						return false;
					}
				}
			} else {
				foreach (TypeParameter type_param in CurrentTypeParameters) {
					if (!type_param.Resolve (this)) {
						error = true;
						return false;
					}
				}

				int offset = CountTypeParameters - CurrentTypeParameters.Length;
				for (int i = offset; i < gen_params.Length; i++)
					CurrentTypeParameters [i - offset].DefineConstraints ();

				foreach (TypeParameter type_param in TypeParameters) {
					if (!type_param.DefineType (ec)) {
						error = true;
						return false;
					}
				}

				current_type = new ConstructedType (
					TypeBuilder, TypeParameters, Location);
			}

			foreach (TypeParameter type_param in TypeParameters)
				if (!type_param.CheckDependencies (ec)) {
					error = true;
					return false;
				}

			if (current_type != null) {
				current_type = current_type.ResolveAsTypeTerminal (ec);
				if (current_type == null) {
					error = true;
					return false;
				}

				CurrentType = current_type.Type;
			}

			return true;
		}

		protected virtual bool DefineNestedTypes ()
		{
			if (Interfaces != null) {
				foreach (TypeContainer iface in Interfaces)
					if (iface.DefineType () == null)
						return false;
			}
			
			if (Types != null) {
				foreach (TypeContainer tc in Types)
					if (tc.DefineType () == null)
						return false;
			}

			if (Delegates != null) {
				foreach (Delegate d in Delegates)
					if (d.DefineType () == null)
						return false;
			}

			if (Enums != null) {
				foreach (Enum en in Enums)
					if (en.DefineType () == null)
						return false;
			}

			return true;
		}

		TypeContainer InTransit;

		protected bool CheckRecursiveDefinition (TypeContainer tc)
		{
			if (InTransit != null) {
				Report.SymbolRelatedToPreviousError (this);
				if (this is Interface)
					Report.Error (
						529, tc.Location, "Inherited interface '{0}' causes a " +
						"cycle in the interface hierarchy of '{1}'",
						GetSignatureForError (), tc.GetSignatureForError ());
				else
					Report.Error (
						146, tc.Location, "Circular base class dependency " +
						"involving '{0}' and '{1}'",
						tc.GetSignatureForError (), GetSignatureForError ());
				return false;
			}

			InTransit = tc;

			Type parent = ptype;
			if (parent != null) {
				if (parent.IsGenericInstance)
					parent = parent.GetGenericTypeDefinition ();

				TypeContainer ptc = TypeManager.LookupTypeContainer (parent);
				if ((ptc != null) && !ptc.CheckRecursiveDefinition (this))
					return false;
			}

			if (iface_exprs != null) {
				foreach (TypeExpr iface in iface_exprs) {
					Type itype = iface.Type;

					if (itype.IsGenericInstance)
						itype = itype.GetGenericTypeDefinition ();

					TypeContainer ptc = TypeManager.LookupTypeContainer (itype);
					if ((ptc != null) && !ptc.CheckRecursiveDefinition (this))
						return false;
				}
			}

			InTransit = null;
			return true;
		}

		static void Error_KeywordNotAllowed (Location loc)
		{
			Report.Error (1530, loc, "Keyword new not allowed for namespace elements");
		}

		/// <summary>
		///   Populates our TypeBuilder with fields and methods
		/// </summary>
		public override bool DefineMembers (TypeContainer container)
		{
			if (members_defined)
				return members_defined_ok;

			members_defined_ok = DoDefineMembers ();
			members_defined = true;

			return members_defined_ok;
		}

		protected virtual bool DoDefineMembers ()
		{
			if (iface_exprs != null) {
				foreach (TypeExpr iface in iface_exprs) {
					ConstructedType ct = iface as ConstructedType;
					if ((ct != null) && !ct.CheckConstraints (ec))
						return false;
				}
			}

			if (base_type != null) {
				ConstructedType ct = base_type as ConstructedType;
				if ((ct != null) && !ct.CheckConstraints (ec))
					return false;
			}

 			if (IsTopLevel) {
 				if ((ModFlags & Modifiers.NEW) != 0)
 					Error_KeywordNotAllowed (Location);
 			} else {
 				MemberInfo conflict_symbol = Parent.MemberCache.FindMemberWithSameName (Basename, false, TypeBuilder);
 				if (conflict_symbol == null) {
 					if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0))
 						Report.Warning (109, Location, "The member '{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
 				} else {
 					if ((ModFlags & Modifiers.NEW) == 0) {
 						Report.SymbolRelatedToPreviousError (conflict_symbol);
 						Report.Warning (108, Location, "The keyword new is required on '{0}' because it hides inherited member", GetSignatureForError ());
 					}
 				}
  			}

			DefineContainerMembers (constants);
			DefineContainerMembers (fields);

			if ((Kind == Kind.Class) && !(this is ClassPart)){
				if ((instance_constructors == null) &&
				    !(this is StaticClass)) {
					if (default_constructor == null)
						DefineDefaultConstructor (false);
				}

				if (initialized_static_fields != null &&
				    default_static_constructor == null)
					DefineDefaultConstructor (true);
			}

			if (Kind == Kind.Struct){
				//
				// Structs can not have initialized instance
				// fields
				//
				if (initialized_static_fields != null &&
				    default_static_constructor == null)
					DefineDefaultConstructor (true);

				if (initialized_fields != null)
					ReportStructInitializedInstanceError ();
			}

			Pending = GetPendingImplementations ();

			if (parts != null) {
				foreach (ClassPart part in parts) {
					if (!part.DefineMembers (this))
						return false;
				}
			}
			
			//
			// Constructors are not in the defined_names array
			//
			DefineContainerMembers (instance_constructors);

			if (default_static_constructor != null)
				default_static_constructor.Define ();

			DefineContainerMembers (properties);
			DefineContainerMembers (events);
			DefineContainerMembers (indexers);
			DefineContainerMembers (methods);
			DefineContainerMembers (operators);
			DefineContainerMembers (enums);
			DefineContainerMembers (delegates);

			if (CurrentType != null) {
				GenericType = CurrentType;

				ec.ContainerType = GenericType;
			}


#if CACHE
			if (!(this is ClassPart))
				member_cache = new MemberCache (this);
#endif

			if (parts != null) {
				foreach (ClassPart part in parts)
					part.member_cache = member_cache;
			}

			if (iterators != null) {
				foreach (Iterator iterator in iterators) {
					if (iterator.DefineType () == null)
						return false;
				}

				foreach (Iterator iterator in iterators) {
					if (!iterator.DefineMembers (this))
						return false;
				}
			}

			return true;
		}

		void ReportStructInitializedInstanceError ()
		{
			string n = TypeBuilder.FullName;
			
			foreach (Field f in initialized_fields){
				Report.Error (
					573, Location,
					"`" + n + "." + f.Name + "': can not have " +
					"instance field initializers in structs");
			}
		}

		protected virtual void DefineContainerMembers (MemberCoreArrayList mcal)
		{
			if (mcal != null)
				mcal.DefineContainerMembers ();
		}

		public override bool Define ()
		{
			if (parts != null) {
				foreach (ClassPart part in parts) {
					if (!part.Define ())
						return false;
				}
			}

			if (iterators != null) {
				foreach (Iterator iterator in iterators) {
					if (!iterator.Define ())
						return false;
				}
			}

			return true;
		}

		public MemberInfo FindBaseMemberWithSameName (string name, bool ignore_methods)
		{
			return BaseCache.FindMemberWithSameName (name, ignore_methods, null);
		}

		/// <summary>
		///   This function is based by a delegate to the FindMembers routine
		/// </summary>
		static bool AlwaysAccept (MemberInfo m, object filterCriteria)
		{
			return true;
		}

		/// <summary>
		///   This filter is used by FindMembers, and we just keep
		///   a global for the filter to `AlwaysAccept'
		/// </summary>
		static MemberFilter accepting_filter;

		
		static TypeContainer ()
		{
			accepting_filter = new MemberFilter (AlwaysAccept);
		}
		
		public MethodInfo[] GetMethods ()
		{
			ArrayList members = new ArrayList ();

			DefineMembers (null);

			if (methods != null) {
				int len = methods.Count;
				for (int i = 0; i < len; i++) {
					Method m = (Method) methods [i];

					members.Add (m.MethodBuilder);
				}
			}

			if (operators != null) {
				int len = operators.Count;
				for (int i = 0; i < len; i++) {
					Operator o = (Operator) operators [i];

					members.Add (o.OperatorMethodBuilder);
				}
			}

			if (properties != null) {
				int len = properties.Count;
				for (int i = 0; i < len; i++) {
					Property p = (Property) properties [i];

					if (p.GetBuilder != null)
						members.Add (p.GetBuilder);
					if (p.SetBuilder != null)
						members.Add (p.SetBuilder);
				}
			}
				
			if (indexers != null) {
				int len = indexers.Count;
				for (int i = 0; i < len; i++) {
					Indexer ix = (Indexer) indexers [i];

					if (ix.GetBuilder != null)
						members.Add (ix.GetBuilder);
					if (ix.SetBuilder != null)
						members.Add (ix.SetBuilder);
				}
			}

			if (events != null) {
				int len = events.Count;
				for (int i = 0; i < len; i++) {
					Event e = (Event) events [i];

					if (e.AddBuilder != null)
						members.Add (e.AddBuilder);
					if (e.RemoveBuilder != null)
						members.Add (e.RemoveBuilder);
				}
			}

			MethodInfo[] retMethods = new MethodInfo [members.Count];
			members.CopyTo (retMethods, 0);
			return retMethods;
		}
		
		// Indicated whether container has StructLayout attribute set Explicit
		public virtual bool HasExplicitLayout {
			get {
				return false;
			}
		}

		public override Type FindNestedType (string name)
		{
			ArrayList [] lists = { types, enums, delegates, interfaces };

			for (int j = 0; j < lists.Length; ++j) {
				ArrayList list = lists [j];
				if (list == null)
					continue;
				
				int len = list.Count;
				for (int i = 0; i < len; ++i) {
					DeclSpace ds = (DeclSpace) list [i];
					if (ds.Basename == name) {
						ds.DefineType ();
						return ds.TypeBuilder;
					}
				}
			}

			return null;
		}

		private void FindMembers_NestedTypes (int modflags,
						      BindingFlags bf, MemberFilter filter, object criteria,
						      ref ArrayList members)
		{
			ArrayList [] lists = { types, enums, delegates, interfaces };

			for (int j = 0; j < lists.Length; ++j) {
				ArrayList list = lists [j];
				if (list == null)
					continue;
			
				int len = list.Count;
				for (int i = 0; i < len; i++) {
					DeclSpace ds = (DeclSpace) list [i];
					
					if ((ds.ModFlags & modflags) == 0)
						continue;
					
					TypeBuilder tb = ds.TypeBuilder;
					if (tb == null) {
						if (!(criteria is string) || ds.Basename.Equals (criteria))
							tb = ds.DefineType ();
					}
					
					if (tb != null && (filter (tb, criteria) == true)) {
						if (members == null)
							members = new ArrayList ();
						
						members.Add (tb);
					}
				}
			}
		}
		
		/// <summary>
		///   This method returns the members of this type just like Type.FindMembers would
		///   Only, we need to use this for types which are _being_ defined because MS' 
		///   implementation can't take care of that.
		/// </summary>
		//
		// FIXME: return an empty static array instead of null, that cleans up
		// some code and is consistent with some coding conventions I just found
		// out existed ;-)
		//
		//
		// Notice that in various cases we check if our field is non-null,
		// something that would normally mean that there was a bug elsewhere.
		//
		// The problem happens while we are defining p-invoke methods, as those
		// will trigger a FindMembers, but this happens before things are defined
		//
		// Since the whole process is a no-op, it is fine to check for null here.
		//
		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			ArrayList members = null;

			int modflags = 0;
			if ((bf & BindingFlags.Public) != 0)
				modflags |= Modifiers.PUBLIC | Modifiers.PROTECTED |
					Modifiers.INTERNAL;
			if ((bf & BindingFlags.NonPublic) != 0)
				modflags |= Modifiers.PRIVATE;

			int static_mask = 0, static_flags = 0;
			switch (bf & (BindingFlags.Static | BindingFlags.Instance)) {
			case BindingFlags.Static:
				static_mask = static_flags = Modifiers.STATIC;
				break;

			case BindingFlags.Instance:
				static_mask = Modifiers.STATIC;
				static_flags = 0;
				break;

			default:
				static_mask = static_flags = 0;
				break;
			}

			Timer.StartTimer (TimerType.TcFindMembers);

			if (filter == null)
				filter = accepting_filter; 

			if ((mt & MemberTypes.Field) != 0) {
				if (fields != null) {
					int len = fields.Count;
					for (int i = 0; i < len; i++) {
						FieldMember f = (FieldMember) fields [i];
						
						if ((f.ModFlags & modflags) == 0)
							continue;
						if ((f.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = f.FieldBuilder;
						if (fb != null && filter (fb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (fb);
					}
				}
				}

				if (constants != null) {
					int len = constants.Count;
					for (int i = 0; i < len; i++) {
						Const con = (Const) constants [i];
						
						if ((con.ModFlags & modflags) == 0)
							continue;
						if ((con.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = con.FieldBuilder;
						if (fb == null) {
							if (con.Define ())
								fb = con.FieldBuilder;
						}
						if (fb != null && filter (fb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (fb);
					}
				}
			}
			}

			if ((mt & MemberTypes.Method) != 0) {
				if (methods != null) {
					int len = methods.Count;
					for (int i = 0; i < len; i++) {
						Method m = (Method) methods [i];
						
						if ((m.ModFlags & modflags) == 0)
							continue;
						if ((m.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder mb = m.MethodBuilder;

						if (mb != null && filter (mb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (mb);
					}
				}
				}

				if (operators != null) {
					int len = operators.Count;
					for (int i = 0; i < len; i++) {
						Operator o = (Operator) operators [i];

						if ((o.ModFlags & modflags) == 0)
							continue;
						if ((o.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder ob = o.OperatorMethodBuilder;
						if (ob != null && filter (ob, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (ob);
					}
				}
				}

				if (properties != null) {
					int len = properties.Count;
					for (int i = 0; i < len; i++) {
						Property p = (Property) properties [i];

						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = p.GetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}

						b = p.SetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
					}
				}
				}
				
				if (indexers != null) {
					int len = indexers.Count;
					for (int i = 0; i < len; i++) {
						Indexer ix = (Indexer) indexers [i];
				
						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = ix.GetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}

						b = ix.SetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
					}
				}
			}
			}

			if ((mt & MemberTypes.Event) != 0) {
				if (events != null) {
					int len = events.Count;
					for (int i = 0; i < len; i++) {
						Event e = (Event) events [i];
						
						if ((e.ModFlags & modflags) == 0)
							continue;
						if ((e.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo eb = e.EventBuilder;
						if (eb != null && filter (eb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
						        members.Add (e.EventBuilder);
					}
			}
				}
			}
			
			if ((mt & MemberTypes.Property) != 0){
				if (properties != null) {
					int len = properties.Count;
					for (int i = 0; i < len; i++) {
						Property p = (Property) properties [i];
						
						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo pb = p.PropertyBuilder;
						if (pb != null && filter (pb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (p.PropertyBuilder);
						}
					}
				}

				if (indexers != null) {
					int len = indexers.Count;
					for (int i = 0; i < len; i++) {
						Indexer ix = (Indexer) indexers [i];

						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo ib = ix.PropertyBuilder;
						if (ib != null && filter (ib, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (ix.PropertyBuilder);
						}
					}
			}
			}
			
			if ((mt & MemberTypes.NestedType) != 0)
				FindMembers_NestedTypes (modflags, bf, filter, criteria, ref members);

			if ((mt & MemberTypes.Constructor) != 0){
				if (((bf & BindingFlags.Instance) != 0) && (instance_constructors != null)){
					int len = instance_constructors.Count;
					for (int i = 0; i < len; i++) {
						Constructor c = (Constructor) instance_constructors [i];
						
						ConstructorBuilder cb = c.ConstructorBuilder;
						if (cb != null && filter (cb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
								members.Add (cb);
					}
				}
				}

				if (((bf & BindingFlags.Static) != 0) && (default_static_constructor != null)){
					ConstructorBuilder cb =
						default_static_constructor.ConstructorBuilder;
					
					if (cb != null && filter (cb, criteria) == true) {
						if (members == null)
							members = new ArrayList ();
						
						members.Add (cb);
				}
			}
			}

			//
			// Lookup members in base if requested.
			//
			if ((bf & BindingFlags.DeclaredOnly) == 0) {
				if (TypeBuilder.BaseType != null) {
				MemberList list = FindMembers (TypeBuilder.BaseType, mt, bf, filter, criteria);
				if (list.Count > 0) {
					if (members == null)
						members = new ArrayList ();
					
				members.AddRange (list);
			}
			}
				
 			}

			Timer.StopTimer (TimerType.TcFindMembers);

			if (members == null)
				return MemberList.Empty;
			else
				return new MemberList (members);
		}

		public override MemberCache MemberCache {
			get {
				return member_cache;
			}
		}

		public static MemberList FindMembers (Type t, MemberTypes mt, BindingFlags bf,
						      MemberFilter filter, object criteria)
		{
			DeclSpace ds = TypeManager.LookupDeclSpace (t);

			if (ds != null)
				return ds.FindMembers (mt, bf, filter, criteria);
			else
				return new MemberList (t.FindMembers (mt, bf, filter, criteria));
		}

		//
		// FindMethods will look for methods not only in the type `t', but in
		// any interfaces implemented by the type.
		//
		public static MethodInfo [] FindMethods (Type t, BindingFlags bf,
							 MemberFilter filter, object criteria)
		{
			return null;
		}

		/// <summary>
		///   Emits the values for the constants
		/// </summary>
		public void EmitConstants ()
		{
			if (constants != null)
				foreach (Const con in constants)
					con.Emit ();
			return;
		}

		void CheckMemberUsage (MemberCoreArrayList al, string member_type)
		{
			if (al == null)
				return;

			foreach (MemberCore mc in al) {
				if ((mc.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE)
					continue;

				if (!mc.IsUsed) {
					Report.Warning (169, mc.Location, "The private {0} '{1}' is never used", member_type, mc.GetSignatureForError ());
				}
			}
		}

		public virtual void VerifyMembers ()
		{
			//
			// Check for internal or private fields that were never assigned
			//
			if (RootContext.WarningLevel >= 3) {
				CheckMemberUsage (properties, "property");
				CheckMemberUsage (methods, "method");
				CheckMemberUsage (constants, "constant");

				if (fields != null){
					foreach (FieldMember f in fields) {
						if ((f.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE)
							continue;
						
						if (!f.IsUsed){
							Report.Warning (169, f.Location, "The private field '{0}' is never used", f.GetSignatureForError ());
							continue;
						}
						
						//
						// Only report 649 on level 4
						//
						if (RootContext.WarningLevel < 4)
							continue;
						
						if ((f.status & Field.Status.ASSIGNED) != 0)
							continue;
						
						Report.Warning (649, f.Location, "Field '{0}' is never assigned to, and will always have its default value '{1}'", f.GetSignatureForError (), "");
					}
				}
			}
		}

		/// <summary>
		///   Emits the code, this step is performed after all
		///   the types, enumerations, constructors
		/// </summary>
		public void EmitType ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit (ec, this);

			//
			// Structs with no fields need to have at least one byte.
			// The right thing would be to set the PackingSize in a DefineType
			// but there are no functions that allow interfaces *and* the size to
			// be specified.
			//

			if ((Kind == Kind.Struct) && !have_nonstatic_fields){
				FieldBuilder fb = TypeBuilder.DefineField ("$PRIVATE$", TypeManager.byte_type,
									   FieldAttributes.Private);

				if (HasExplicitLayout){
					object [] ctor_args = new object [1];
					ctor_args [0] = 0;
				
					CustomAttributeBuilder cba = new CustomAttributeBuilder (
						TypeManager.field_offset_attribute_ctor, ctor_args);
					fb.SetCustomAttribute (cba);
				}
			}

			Emit ();

			if (instance_constructors != null) {
				if (TypeBuilder.IsSubclassOf (TypeManager.attribute_type) && RootContext.VerifyClsCompliance && IsClsComplianceRequired (this)) {
					bool has_compliant_args = false;

					foreach (Constructor c in instance_constructors) {
						c.Emit ();

						if (has_compliant_args)
							continue;

						has_compliant_args = c.HasCompliantArgs;
					}
					if (!has_compliant_args)
						Report.Error (3015, Location, "'{0}' has no accessible constructors which use only CLS-compliant types", GetSignatureForError ());
				} else {
				foreach (Constructor c in instance_constructors)
						c.Emit ();
				}
			}

			// Can not continue if constants are broken
			EmitConstants ();
			if (Report.Errors > 0)
				return;

			if (default_static_constructor != null)
				default_static_constructor.Emit ();
			
			if (methods != null)
				foreach (Method m in methods)
					m.Emit ();

			if (operators != null)
				foreach (Operator o in operators)
					o.Emit ();

			if (properties != null)
				foreach (Property p in properties)
					p.Emit ();

			if (indexers != null){
				indexers.Emit ();
			}
			
			if (fields != null)
				foreach (FieldMember f in fields)
					f.Emit ();

			if (events != null){
				foreach (Event e in Events)
					e.Emit ();
			}

			if (delegates != null) {
				foreach (Delegate d in Delegates) {
					d.Emit ();
				}
			}

			if (enums != null) {
				foreach (Enum e in enums) {
					e.Emit ();
				}
			}

			if (parts != null) {
				foreach (ClassPart part in parts)
					part.EmitType ();
			}

			if ((Pending != null) && !(this is ClassPart))
				if (Pending.VerifyPendingMethods ())
					return;

			if (iterators != null)
				foreach (Iterator iterator in iterators)
					iterator.EmitType ();
			
//			if (types != null)
//				foreach (TypeContainer tc in types)
//					tc.Emit ();
		}
		
		public override void CloseType ()
		{
			if ((caching_flags & Flags.CloseTypeCreated) != 0)
				return;

			try {
				caching_flags |= Flags.CloseTypeCreated;
					TypeBuilder.CreateType ();
			} catch (TypeLoadException){
				//
				// This is fine, the code still created the type
				//
//				Report.Warning (-20, "Exception while creating class: " + TypeBuilder.Name);
//				Console.WriteLine (e.Message);
			} catch {
				Console.WriteLine ("In type: " + Name);
				throw;
			}
			
			if (Enums != null)
				foreach (Enum en in Enums)
					en.CloseType ();

			if (Types != null){
				foreach (TypeContainer tc in Types)
					if (tc.Kind == Kind.Struct)
						tc.CloseType ();

				foreach (TypeContainer tc in Types)
					if (tc.Kind != Kind.Struct)
						tc.CloseType ();
			}

			if (Delegates != null)
				foreach (Delegate d in Delegates)
					d.CloseType ();

			if (Iterators != null)
				foreach (Iterator i in Iterators)
					i.CloseType ();
			
			types = null;
			properties = null;
			enums = null;
			delegates = null;
			fields = null;
			initialized_fields = null;
			initialized_static_fields = null;
			constants = null;
			interfaces = null;
			methods = null;
			events = null;
			indexers = null;
			operators = null;
			iterators = null;
			ec = null;
			default_constructor = null;
			default_static_constructor = null;
			type_bases = null;
			OptAttributes = null;
			ifaces = null;
			base_cache = null;
			member_cache = null;
		}

		// TODO: make it obsolete and use GetSignatureForError
		public string MakeName (string n)
		{
			return "`" + Name + "." + n + "'";
		}

		//
		// Performs the validation on a Method's modifiers (properties have
		// the same properties).
		//
		public bool MethodModifiersValid (MemberCore mc)
		{
			const int vao = (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE);
			const int va = (Modifiers.VIRTUAL | Modifiers.ABSTRACT);
			const int nv = (Modifiers.NEW | Modifiers.VIRTUAL);
			bool ok = true;
			int flags = mc.ModFlags;
			
			//
			// At most one of static, virtual or override
			//
			if ((flags & Modifiers.STATIC) != 0){
				if ((flags & vao) != 0){
					Report.Error (112, mc.Location, "static method '{0}' can not be marked as virtual, abstract or override",
						GetSignatureForError ());
					ok = false;
				}
			}

			if (Kind == Kind.Struct){
				if ((flags & va) != 0){
					Modifiers.Error_InvalidModifier (mc.Location, "virtual or abstract");
					ok = false;
				}
			}

			if ((flags & Modifiers.OVERRIDE) != 0 && (flags & nv) != 0){
				Report.Error (113, mc.Location, "'{0}' marked as override cannot be marked as new or virtual", mc.GetSignatureForError ());
				ok = false;
			}

			//
			// If the declaration includes the abstract modifier, then the
			// declaration does not include static, virtual or extern
			//
			if ((flags & Modifiers.ABSTRACT) != 0){
				if ((flags & Modifiers.EXTERN) != 0){
					Report.Error (
						180, mc.Location, "'{0}' can not be both abstract and extern", mc.GetSignatureForError ());
					ok = false;
				}

				if ((flags & Modifiers.SEALED) != 0) {
					Report.Error (502, mc.Location, "'{0}' cannot be both abstract and sealed", mc.GetSignatureForError ());
					ok = false;
				}

				if ((flags & Modifiers.VIRTUAL) != 0){
					Report.Error (503, mc.Location, "'{0}' can not be both abstract and virtual", mc.GetSignatureForError ());
					ok = false;
				}

				if ((ModFlags & Modifiers.ABSTRACT) == 0){
					Report.Error (513, mc.Location, "'{0}' is abstract but its container class is not", mc.GetSignatureForError ());
					ok = false;
				}
			}

			if ((flags & Modifiers.PRIVATE) != 0){
				if ((flags & vao) != 0){
					Report.Error (621, mc.Location, "'{0}' virtual or abstract members can not be private", mc.GetSignatureForError ());
					ok = false;
				}
			}

			if ((flags & Modifiers.SEALED) != 0){
				if ((flags & Modifiers.OVERRIDE) == 0){
					Report.Error (238, mc.Location, "'{0}' cannot be sealed because it is not an override", mc.GetSignatureForError ());
					ok = false;
				}
			}

			return ok;
		}

		public bool UserDefinedStaticConstructor {
			get {
				return default_static_constructor != null;
			}
		}

		public Constructor DefaultStaticConstructor {
			get { return default_static_constructor; }
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			VerifyClsName ();

			Type base_type = TypeBuilder.BaseType;
			if (base_type != null && !AttributeTester.IsClsCompliant (base_type)) {
				Report.Error (3009, Location, "'{0}': base type '{1}' is not CLS-compliant", GetSignatureForError (), TypeManager.CSharpName (base_type));
			}

			if (!Parent.IsClsComplianceRequired (ds)) {
				Report.Error (3018, Location, "'{0}' cannot be marked as CLS-Compliant because it is a member of non CLS-Compliant type '{1}'", 
					GetSignatureForError (), Parent.GetSignatureForError ());
			}
			return true;
		}


		/// <summary>
		/// Checks whether container name is CLS Compliant
		/// </summary>
		void VerifyClsName ()
		{
			Hashtable base_members = base_cache == null ? 
				new Hashtable () :
				base_cache.GetPublicMembers ();
			Hashtable this_members = new Hashtable ();

			foreach (DictionaryEntry entry in defined_names) {
				MemberCore mc = (MemberCore)entry.Value;
				if (!mc.IsClsComplianceRequired (this))
					continue;

				string name = (string)entry.Key;
				string basename = name.Substring (name.LastIndexOf ('.') + 1);

				string lcase = basename.ToLower (System.Globalization.CultureInfo.InvariantCulture);
				object found = base_members [lcase];
				if (found == null) {
					found = this_members [lcase];
					if (found == null) {
						this_members.Add (lcase, mc);
						continue;
					}
				}

				if ((mc.ModFlags & Modifiers.OVERRIDE) != 0)
					continue;					

				if (found is MemberInfo) {
					if (basename == ((MemberInfo)found).Name)
						continue;
					Report.SymbolRelatedToPreviousError ((MemberInfo)found);
				} else {
					Report.SymbolRelatedToPreviousError ((MemberCore) found);
				}
				Report.Error (3005, mc.Location, "Identifier '{0}' differing only in case is not CLS-compliant", mc.GetSignatureForError ());
			}
		}


		/// <summary>
		///   Performs checks for an explicit interface implementation.  First it
		///   checks whether the `interface_type' is a base inteface implementation.
		///   Then it checks whether `name' exists in the interface type.
		/// </summary>
		public virtual bool VerifyImplements (Type interface_type, string full,
						      string name, Location loc)
		{
			bool found = false;

			if (ifaces != null){
				foreach (Type t in ifaces){
					if (t == interface_type){
						found = true;
						break;
					}
				}
			}
			
			if (!found){
				Report.Error (540, loc, "`{0}': containing class does not implement interface `{1}'",
					      full, interface_type.FullName);
				return false;
			}

			return true;
		}

		protected override void VerifyObsoleteAttribute()
		{
			CheckUsageOfObsoleteAttribute (TypeBuilder.BaseType);

			if (ifaces == null)
				return;

			foreach (Type iface in ifaces) {
				CheckUsageOfObsoleteAttribute (iface);
			}
		}

		//
		// IMemberContainer
		//

		string IMemberContainer.Name {
			get {
				return Name;
			}
		}

		Type IMemberContainer.Type {
			get {
				return TypeBuilder;
			}
		}

		MemberCache IMemberContainer.MemberCache {
			get {
				return member_cache;
			}
		}

		bool IMemberContainer.IsInterface {
			get {
				return Kind == Kind.Interface;
			}
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			BindingFlags new_bf = bf | BindingFlags.DeclaredOnly;

			if (GenericType != null)
				return TypeManager.FindMembers (GenericType, mt, new_bf,
								null, null);
			else
				return FindMembers (mt, new_bf, null, null);
		}

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal override void GenerateDocComment (DeclSpace ds)
		{
			DocUtil.GenerateTypeDocComment (this, ds);
		}

		public override string DocCommentHeader {
			get { return "T:"; }
		}

		public virtual MemberCache BaseCache {
			get {
				if (base_cache != null)
					return base_cache;
				if (TypeBuilder.BaseType != null)
					base_cache = TypeManager.LookupMemberCache (TypeBuilder.BaseType);
				if (TypeBuilder.IsInterface)
					base_cache = TypeManager.LookupBaseInterfacesCache (TypeBuilder);
				return base_cache;
			}
		}
		
	}

	public class PartialContainer : TypeContainer {

		public readonly Namespace Namespace;
		public readonly int OriginalModFlags;
		public readonly int AllowedModifiers;
		public readonly TypeAttributes DefaultTypeAttributes;

		static PartialContainer Create (NamespaceEntry ns, TypeContainer parent,
						MemberName member_name, int mod_flags, Kind kind,
						Location loc)
		{
			PartialContainer pc;
			DeclSpace ds = RootContext.Tree.GetDecl (member_name);
			if (ds != null) {
				pc = ds as PartialContainer;

				if (pc == null) {
					Report.Error (
						260, ds.Location, "Missing partial modifier " +
						"on declaration of type `{0}'; another " +
						"partial implementation of this type exists",
						member_name.GetTypeName());

					Report.LocationOfPreviousError (loc);
					return null;
				}

				if (pc.Kind != kind) {
					Report.Error (
						261, loc, "Partial declarations of `{0}' " +
						"must be all classes, all structs or " +
						"all interfaces", member_name.GetTypeName ());
					return null;
				}

				if (pc.OriginalModFlags != mod_flags) {
					Report.Error (
						262, loc, "Partial declarations of `{0}' " +
						"have conflicting accessibility modifiers",
						member_name.GetTypeName ());
					return null;
				}

				if (pc.IsGeneric) {
					if (pc.CountTypeParameters != member_name.CountTypeArguments) {
						Report.Error (
							264, loc, "Partial declarations of `{0}' " +
							"must have the same type parameter names in " +
							"the same order", member_name.GetTypeName ());
						return null;
					}

					string[] pc_names = pc.MemberName.TypeArguments.GetDeclarations ();
					string[] names = member_name.TypeArguments.GetDeclarations ();

					for (int i = 0; i < pc.CountTypeParameters; i++) {
						if (pc_names [i] == names [i])
							continue;

						Report.Error (
							264, loc, "Partial declarations of `{0}' " +
							"must have the same type parameter names in " +
							"the same order", member_name.GetTypeName ());
						return null;
					}
				}

				return pc;
			}

			if (parent is ClassPart)
				parent = ((ClassPart) parent).PartialContainer;

			pc = new PartialContainer (ns.NS, parent, member_name, mod_flags, kind, loc);
			RootContext.Tree.RecordDecl (ns.NS, member_name, pc);

			if (kind == Kind.Interface)
				parent.AddInterface (pc);
			else if (kind == Kind.Class || kind == Kind.Struct)
				parent.AddClassOrStruct (pc);
			else
				throw new InvalidOperationException ();

			// This is needed to define our type parameters; we define the constraints later.
			pc.SetParameterInfo (null);
			return pc;
		}

		public static ClassPart CreatePart (NamespaceEntry ns, TypeContainer parent,
						    MemberName name, int mod, Attributes attrs,
						    Kind kind, Location loc)
		{
			PartialContainer pc = Create (ns, parent, name, mod, kind, loc);
			if (pc == null) {
				// An error occured; create a dummy container, but don't
				// register it.
				pc = new PartialContainer (ns.NS, parent, name, mod, kind, loc);
			}

			ClassPart part = new ClassPart (ns, pc, parent, mod, attrs, kind, loc);
			pc.AddPart (part);
			return part;
		}

		protected PartialContainer (Namespace ns, TypeContainer parent,
					    MemberName name, int mod, Kind kind, Location l)
			: base (null, parent, name, null, kind, l)
		{
			this.Namespace = ns;

			switch (kind) {
			case Kind.Class:
				AllowedModifiers = Class.AllowedModifiers;
				DefaultTypeAttributes = Class.DefaultTypeAttributes;
				break;

			case Kind.Struct:
				AllowedModifiers = Struct.AllowedModifiers;
				DefaultTypeAttributes = Struct.DefaultTypeAttributes;
				break;

			case Kind.Interface:
				AllowedModifiers = Interface.AllowedModifiers;
				DefaultTypeAttributes = Interface.DefaultTypeAttributes;
				break;

			default:
				throw new InvalidOperationException ();
			}

			int accmods;
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);
			this.OriginalModFlags = mod;
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return PendingImplementation.GetPendingImplementations (this);
		}

		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	public class ClassPart : TypeContainer, IMemberContainer {
		public readonly PartialContainer PartialContainer;
		public readonly bool IsPartial;

		Constraints[] constraints;

		public ClassPart (NamespaceEntry ns, PartialContainer pc, TypeContainer parent,
				  int mod, Attributes attrs, Kind kind, Location l)
			: base (ns, parent, pc.MemberName, attrs, kind, l)
		{
			this.PartialContainer = pc;
			this.IsPartial = true;

			int accmods;
			if (parent == null || parent == RootContext.Tree.Types)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (pc.AllowedModifiers, mod, accmods, l);

			if (pc.IsGeneric)
				constraints = new Constraints [pc.CountCurrentTypeParameters];
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return PartialContainer.Pending;
		}

		public override bool VerifyImplements (Type interface_type, string full,
						       string name, Location loc)
		{
			return PartialContainer.VerifyImplements (
				interface_type, full, name, loc);
		}

		public override void SetParameterInfo (ArrayList constraints_list)
		{
			if (constraints_list == null)
				return;

			TypeParameter[] current_params = PartialContainer.CurrentTypeParameters;
			for (int i = 0; i < constraints.Length; i++) {
				foreach (Constraints constraint in constraints_list) {
					if (constraint.TypeParameter == current_params [i].Name) {
						constraints [i] = constraint;
						break;
					}
				}
			}
		}

		public bool DefineTypeParameters ()
		{
			TypeParameter[] current_params = PartialContainer.CurrentTypeParameters;

			for (int i = 0; i < current_params.Length; i++) {
				Constraints new_constraints = constraints [i];
				if (new_constraints == null)
					continue;

				if (!current_params [i].UpdateConstraints (ec, new_constraints)) {
					Report.Error (265, Location, "Partial declarations of `{0}' have " +
						      "inconsistent constraints for type parameter `{1}'.",
						      MemberName.GetTypeName (), current_params [i].Name);
					return false;
				}
			}

			for (int i = 0; i < current_params.Length; i++) {
				if (!current_params [i].Resolve (this))
					return false;
			}

			for (int i = 0; i < current_params.Length; i++)
				current_params [i].DefineConstraints ();

			foreach (TypeParameter type_param in PartialContainer.TypeParameters) {
				if (!type_param.DefineType (ec))
					return false;
			}

			return true;
		}

		public override void RegisterFieldForInitialization (FieldMember field)
		{
			PartialContainer.RegisterFieldForInitialization (field);
		}

		public override bool EmitFieldInitializers (EmitContext ec)
		{
			return PartialContainer.EmitFieldInitializers (ec);
		}

		public override Type FindNestedType (string name)
		{
			return PartialContainer.FindNestedType (name);
		}

		public override MemberCache BaseCache {
			get {
				return PartialContainer.BaseCache;
			}
		}

		public override TypeBuilder DefineType ()
		{
			throw new InternalErrorException ("Should not get here");
		}

	}

	public abstract class ClassOrStruct : TypeContainer {
		bool has_explicit_layout = false;
		ListDictionary declarative_security;

		public ClassOrStruct (NamespaceEntry ns, TypeContainer parent,
				      MemberName name, Attributes attrs, Kind kind,
				      Location l)
			: base (ns, parent, name, attrs, kind, l)
		{
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return PendingImplementation.GetPendingImplementations (this);
		}

		public override bool HasExplicitLayout {
			get {
				return has_explicit_layout;
				}
			}

		public override void VerifyMembers ()
		{
			base.VerifyMembers ();

			if ((events != null) && (RootContext.WarningLevel >= 3)) {
				foreach (Event e in events){
					if (e.status == 0)
						Report.Warning (67, e.Location, "The event '{0}' is never used", e.GetSignatureForError ());
				}
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();

				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == TypeManager.struct_layout_attribute_type && a.GetLayoutKindValue () == LayoutKind.Explicit)
				has_explicit_layout = true;

			base.ApplyAttributeBuilder (a, cb);
		}

		public override void Emit()
		{
			base.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					TypeBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}
		}
	}

	/// <summary>
	/// Class handles static classes declaration
	/// </summary>
	public sealed class StaticClass: Class {
		public StaticClass (NamespaceEntry ns, TypeContainer parent, MemberName name, int mod,
				    Attributes attrs, Location l)
			: base (ns, parent, name, mod, attrs, l)
		{
			if (RootContext.Version == LanguageVersion.ISO_1) {
				Report.FeatureIsNotStandardized (l, "static classes");
			}
		}

		protected override int AllowedModifiersProp {
			get {
				return Modifiers.NEW | Modifiers.PUBLIC | Modifiers.PROTECTED | Modifiers.INTERNAL | Modifiers.PRIVATE |
					Modifiers.STATIC | Modifiers.UNSAFE;
			}
		}

		protected override void DefineContainerMembers (MemberCoreArrayList list)
		{
			if (list == null)
				return;

			foreach (MemberCore m in list) {
				if (m is Operator) {
					Report.Error (715, m.Location, "'{0}': static classes cannot contain user-defined operators", m.GetSignatureForError (this));
					continue;
				}

				if ((m.ModFlags & Modifiers.PROTECTED) != 0)
					Report.Warning (628, 4, m.Location, "'{0}': new protected member declared in static class", m.GetSignatureForError (this));

				if (m is Indexer) {
					Report.Error (720, m.Location, "'{0}': cannot declare indexers in a static class", m.GetSignatureForError (this));
					continue;
				}

				if ((m.ModFlags & Modifiers.STATIC) != 0 || m is Enum || m is Delegate)
					continue;

				if (m is Constructor) {
					Report.Error (710, m.Location, "'{0}': Static classes cannot have instance constructors", GetSignatureForError ());
					continue;
				}

				if (m is Destructor) {
					Report.Error (711, m.Location, "'{0}': Static class cannot contain destructor", GetSignatureForError ());
					continue;
				}
				Report.Error (708, m.Location, "'{0}': cannot declare instance members in a static class", m.GetSignatureForError (this));
			}

			base.DefineContainerMembers (list);
		}

		public override TypeBuilder DefineType()
		{
			if ((ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) == (Modifiers.SEALED | Modifiers.STATIC)) {
				Report.Error (441, Location, "'{0}': a class cannot be both static and sealed", GetSignatureForError ());
				return null;
			}

			TypeBuilder tb = base.DefineType ();
			if (tb == null)
				return null;

			if ((ptype != null) && (ptype != TypeManager.object_type)) {
				Report.Error (
					713, Location,
					"Static class '{0}' cannot derive from type '{1}'. " +
					"Static classes must derive from object",
					GetSignatureForError (), ptype);
				return null;
			}

			if (ifaces != null) {
				foreach (Type t in ifaces)
					Report.SymbolRelatedToPreviousError (t);
				Report.Error (
					714, Location,
					"'{0}': static classes cannot implement interfaces",
					GetSignatureForError ());
			}
			return tb;
		}

		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | TypeAttributes.Abstract | TypeAttributes.Sealed;
			}
		}
	}

	public class Class : ClassOrStruct {
		// TODO: remove this and use only AllowedModifiersProp to fix partial classes bugs
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.ABSTRACT |
			Modifiers.SEALED |
			Modifiers.UNSAFE;

		public Class (NamespaceEntry ns, TypeContainer parent, MemberName name, int mod,
			      Attributes attrs, Location l)
			: base (ns, parent, name, attrs, Kind.Class, l)
		{
			this.ModFlags = mod;
		}

		virtual protected int AllowedModifiersProp {
			get {
				return AllowedModifiers;
			}
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.attribute_usage_type) {
				if (ptype != TypeManager.attribute_type &&
				    !ptype.IsSubclassOf (TypeManager.attribute_type) &&
				    TypeBuilder.FullName != "System.Attribute") {
					Report.Error (641, a.Location, "Attribute '{0}' is only valid on classes derived from System.Attribute", a.Name);
				}
			}

			if (a.Type == TypeManager.conditional_attribute_type &&
				!(ptype == TypeManager.attribute_type || ptype.IsSubclassOf (TypeManager.attribute_type))) {
				Report.Error (1689, a.Location, "Attribute 'System.Diagnostics.ConditionalAttribute' is only valid on methods or attribute classes");
				return;
			}

			if (AttributeTester.IsAttributeExcluded (a.Type))
				return;

			base.ApplyAttributeBuilder (a, cb);
		}

		public const TypeAttributes DefaultTypeAttributes =
			TypeAttributes.AutoLayout | TypeAttributes.Class;

		public override TypeBuilder DefineType()
		{
			if ((ModFlags & Modifiers.ABSTRACT) == Modifiers.ABSTRACT && (ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) != 0) {
				Report.Error (418, Location, "'{0}': an abstract class cannot be sealed or static", GetSignatureForError ());
				return null;
			}

			int accmods = Parent.Parent == null ? Modifiers.INTERNAL : Modifiers.PRIVATE;
			ModFlags = Modifiers.Check (AllowedModifiersProp, ModFlags, accmods, Location);

			return base.DefineType ();
		}

		/// Search for at least one defined condition in ConditionalAttribute of attribute class
		/// Valid only for attribute classes.
		public bool IsExcluded ()
		{
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (OptAttributes == null)
				return false;

			Attribute[] attrs = OptAttributes.SearchMulti (TypeManager.conditional_attribute_type, ec);

			if (attrs == null)
				return false;

			foreach (Attribute a in attrs) {
				string condition = a.GetConditionalAttributeValue (Parent.EmitContext);
				if (RootContext.AllDefines.Contains (condition))
					return false;
			}

			caching_flags |= Flags.Excluded;
			return true;
		}

		//
		// FIXME: How do we deal with the user specifying a different
		// layout?
		//
		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	public class Struct : ClassOrStruct {
		// <summary>
		//   Modifiers allowed in a struct declaration
		// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
			Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Struct (NamespaceEntry ns, TypeContainer parent, MemberName name,
			       int mod, Attributes attrs, Location l)
			: base (ns, parent, name, attrs, Kind.Struct, l)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);

			this.ModFlags |= Modifiers.SEALED;
		}

		public const TypeAttributes DefaultTypeAttributes =
			TypeAttributes.SequentialLayout |
			TypeAttributes.Sealed |
			TypeAttributes.BeforeFieldInit;

		//
		// FIXME: Allow the user to specify a different set of attributes
		// in some cases (Sealed for example is mandatory for a class,
		// but what SequentialLayout can be changed
		//
		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	/// <summary>
	///   Interfaces
	/// </summary>
	public class Interface : TypeContainer, IMemberContainer {

		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
		 	Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Interface (NamespaceEntry ns, TypeContainer parent, MemberName name,
				  int mod, Attributes attrs, Location l)
			: base (ns, parent, name, attrs, Kind.Interface, l)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, l);
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return null;
		}

		public const TypeAttributes DefaultTypeAttributes =
					TypeAttributes.AutoLayout |
					TypeAttributes.Abstract |
					TypeAttributes.Interface;

		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			if (ifaces != null) {
				foreach (Type t in ifaces) {
					if (AttributeTester.IsClsCompliant (t))
						continue;

					Report.SymbolRelatedToPreviousError (t);
					Report.Warning (3027, 1, Location, "'{0}' is not CLS-compliant because base interface '{1}' is not CLS-compliant",
						GetSignatureForError (), TypeManager.CSharpName (t));
				}
			}

			return true;
		}
	}

	public abstract class MethodCore : MemberBase {
		public readonly Parameters Parameters;
		public readonly GenericMethod GenericMethod;
		protected ToplevelBlock block;
		
		//
		// Parameters, cached for semantic analysis.
		//
		protected InternalParameters parameter_info;
		protected Type [] parameter_types;

		// Whether this is an operator method.
		public bool IsOperator;

		//
		// The method we're overriding if this is an override method.
		//
		protected MethodInfo base_method = null;

		static string[] attribute_targets = new string [] { "method", "return" };

		public MethodCore (TypeContainer parent, GenericMethod generic,
				   Expression type, int mod, int allowed_mod, bool is_iface,
				   MemberName name, Attributes attrs, Parameters parameters,
				   Location loc)
			: base (parent, generic != null ? generic : (DeclSpace) parent,
				type, mod, allowed_mod, Modifiers.PRIVATE, name, attrs, loc)
		{
			Parameters = parameters;
			IsInterface = is_iface;
			this.GenericMethod = generic;
		}
		
		//
		//  Returns the System.Type array for the parameters of this method
		//
		public Type [] ParameterTypes {
			get {
				return parameter_types;
			}
		}

		public InternalParameters ParameterInfo
		{
			get {
				return parameter_info;
			}
		}
		
		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		protected override bool CheckBase ()
		{
			if (!base.CheckBase ())
				return false;
			
			// Check whether arguments were correct.
			if (!DoDefineParameters ())
				return false;

			if ((caching_flags & Flags.TestMethodDuplication) != 0 && !CheckForDuplications ())
				return false;

			if (IsExplicitImpl)
				return true;

			// Is null for System.Object while compiling corlib and base interfaces
			if (Parent.BaseCache == null) {
				if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0)) {
					Report.Warning (109, Location, "The member '{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError (Parent));
				}
				return true;
			}

			Type base_ret_type = null;
			base_method = FindOutBaseMethod (Parent, ref base_ret_type);

			// method is override
			if (base_method != null) {

				if (!CheckMethodAgainstBase ())
					return false;

				if ((ModFlags & Modifiers.NEW) == 0) {
					if (!TypeManager.IsEqual (MemberType, TypeManager.TypeToCoreType (base_ret_type))) {
						Report.SymbolRelatedToPreviousError (base_method);
						if (this is PropertyBase) {
							Report.Error (1715, Location, "'{0}': type must be '{1}' to match overridden member '{2}'", 
								GetSignatureForError (), TypeManager.CSharpName (base_ret_type), TypeManager.CSharpSignature (base_method));
						}
						else {
							Report.Error (508, Location, GetSignatureForError (Parent) + ": cannot " +
								"change return type when overriding inherited member");
						}
						return false;
					}
				} else {
					if (base_method.IsAbstract && !IsInterface) {
						Report.SymbolRelatedToPreviousError (base_method);
						Report.Error (533, Location, "'{0}' hides inherited abstract member", GetSignatureForError (Parent));
						return false;
					}
				}

				if (base_method.IsSpecialName && !(this is PropertyBase)) {
					Report.Error (561, Location, "'{0}': cannot override '{1}' because it is a special compiler-generated method", GetSignatureForError (Parent), TypeManager.GetFullNameSignature (base_method));
					return false;
				}

				if (RootContext.WarningLevel > 2) {
					if (Name == "Equals" && parameter_types.Length == 1 && parameter_types [0] == TypeManager.object_type)
						Parent.Methods.HasEquals = true;
					else if (Name == "GetHashCode" && parameter_types.Length == 0)
						Parent.Methods.HasGetHashCode = true;
				}

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					ObsoleteAttribute oa = AttributeTester.GetMethodObsoleteAttribute (base_method);
					if (oa != null) {
						EmitContext ec = new EmitContext (this.Parent, this.Parent, Location, null, null, ModFlags, false);
						if (OptAttributes == null || !OptAttributes.Contains (TypeManager.obsolete_attribute_type, ec)) {
							Report.SymbolRelatedToPreviousError (base_method);
							Report.Warning (672, 1, Location, "Member '{0}' overrides obsolete member. Add the Obsolete attribute to '{0}'", GetSignatureForError (Parent));
						}
					}
				}
				return true;
			}

			MemberInfo conflict_symbol = Parent.FindBaseMemberWithSameName (Name, !(this is Property));
			if ((ModFlags & Modifiers.OVERRIDE) != 0) {
				if (conflict_symbol != null) {
					Report.SymbolRelatedToPreviousError (conflict_symbol);
					if (this is PropertyBase)
						Report.Error (544, Location, "'{0}': cannot override because '{1}' is not a property", GetSignatureForError (Parent), TypeManager.GetFullNameSignature (conflict_symbol));
					else
						Report.Error (505, Location, "'{0}': cannot override because '{1}' is not a method", GetSignatureForError (Parent), TypeManager.GetFullNameSignature (conflict_symbol));
				} else
				Report.Error (115, Location, "'{0}': no suitable methods found to override", GetSignatureForError (Parent));
				return false;
			}

			if (conflict_symbol == null) {
				if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0)) {
					Report.Warning (109, Location, "The member '{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError (Parent));
				}
				return true;
			}

			if ((ModFlags & Modifiers.NEW) == 0) {
				if (this is Method && conflict_symbol is MethodBase)
					return true;

				Report.SymbolRelatedToPreviousError (conflict_symbol);
				Report.Warning (108, Location, "The keyword new is required on '{0}' because it hides inherited member", GetSignatureForError (Parent));
			}

			return true;
		}


		//
		// Performs various checks on the MethodInfo `mb' regarding the modifier flags
		// that have been defined.
		//
		// `name' is the user visible name for reporting errors (this is used to
		// provide the right name regarding method names and properties)
		//
		bool CheckMethodAgainstBase ()
		{
			bool ok = true;

			// TODO: replace with GetSignatureForError 
			string name = base_method.DeclaringType.Name + "." + base_method.Name;

			if ((ModFlags & Modifiers.OVERRIDE) != 0){
				if (!(base_method.IsAbstract || base_method.IsVirtual)){
					Report.Error (
						506, Location, Parent.MakeName (Name) +
						": cannot override inherited member `" +
						name + "' because it is not " +
						"virtual, abstract or override");
					ok = false;
				}
				
				// Now we check that the overriden method is not final
				
				if (base_method.IsFinal) {
					Report.SymbolRelatedToPreviousError (base_method);
					Report.Error (239, Location, "'{0}': cannot override inherited member '{1}' because it is sealed",
							      GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					ok = false;
				}
				//
				// Check that the permissions are not being changed
				//
				MethodAttributes thisp = flags & MethodAttributes.MemberAccessMask;
				MethodAttributes base_classp = base_method.Attributes & MethodAttributes.MemberAccessMask;

				if (!CheckAccessModifiers (thisp, base_classp, base_method)) {
					Error_CannotChangeAccessModifiers (Parent, base_method, name);
					ok = false;
				}
			}

			if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0 && Name != "Finalize") {
				ModFlags |= Modifiers.NEW;
				Report.SymbolRelatedToPreviousError (base_method);
				if (!IsInterface && (base_method.IsVirtual || base_method.IsAbstract)) {
					if (RootContext.WarningLevel >= 2)
						Report.Warning (114, Location, "'{0}' hides inherited member '{1}'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword", GetSignatureForError (Parent), TypeManager.CSharpSignature (base_method));
				} else
					Report.Warning (108, Location, "The keyword new is required on '{0}' because it hides inherited member", GetSignatureForError (Parent));
			}

			return ok;
		}
		
		protected bool CheckAccessModifiers (MethodAttributes thisp, MethodAttributes base_classp, MethodInfo base_method)
		{
			if ((base_classp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
				//
				// when overriding protected internal, the method can be declared
				// protected internal only within the same assembly
				//

				if ((thisp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
					if (Parent.TypeBuilder.Assembly != base_method.DeclaringType.Assembly){
						//
						// assemblies differ - report an error
						//
						
						return false;
					} else if (thisp != base_classp) {
						//
						// same assembly, but other attributes differ - report an error
						//
						
						return false;
					};
				} else if ((thisp & MethodAttributes.Family) != MethodAttributes.Family) {
					//
					// if it's not "protected internal", it must be "protected"
					//

					return false;
				} else if (Parent.TypeBuilder.Assembly == base_method.DeclaringType.Assembly) {
					//
					// protected within the same assembly - an error
					//
					return false;
				} else if ((thisp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem)) != 
					   (base_classp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem))) {
					//
					// protected ok, but other attributes differ - report an error
					//
					return false;
				}
				return true;
			} else {
				return (thisp == base_classp);
			}
		}
		
		void Error_CannotChangeAccessModifiers (TypeContainer parent, MethodInfo base_method, string name)
		{
			//
			// FIXME: report the old/new permissions?
			//
			Report.Error (
				507, Location, parent.MakeName (Name) +
				": can't change the access modifiers when overriding inherited " +
				"member `" + name + "'");
		}

		protected static string Error722 {
			get {
				return "'{0}': static types cannot be used as return types";
			}
		}

		/// <summary>
		/// For custom member duplication search in a container
		/// </summary>
		protected abstract bool CheckForDuplications ();

		/// <summary>
		/// Gets base method and its return type
		/// </summary>
		protected abstract MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type);

		protected virtual bool DoDefineParameters ()
		{
			EmitContext ec = ds.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("DoDefineParameters invoked too early");

			bool old_unsafe = ec.InUnsafe;
			ec.InUnsafe = InUnsafe;
			// Check if arguments were correct
			parameter_types = Parameters.GetParameterInfo (ec);
			ec.InUnsafe = old_unsafe;

			if ((parameter_types == null) ||
			    !CheckParameters (ds, parameter_types))
				return false;

			TypeParameter[] tparam = ds.IsGeneric ? ds.TypeParameters : null;
			parameter_info = new InternalParameters (parameter_types, Parameters, tparam);

			Parameter array_param = Parameters.ArrayParameter;
			if ((array_param != null) &&
			    (!array_param.ParameterType.IsArray ||
			     (array_param.ParameterType.GetArrayRank () != 1))) {
				Report.Error (225, Location, "params parameter has to be a single dimensional array");
				return false;
			}

			return true;
		}

		void error_425 (Type old, Type t, string name)
		{
			Report.Error (425, Location,
				      "The constraints of type parameter `{0}' " +
				      "of method `{1}' must match the constraints for " +
				      "type parameter `{2}' of method `{3}'",
				      TypeManager.CSharpName (old), Name,
				      TypeManager.CSharpName (t), name);
		}

		protected override bool CheckGenericOverride (MethodInfo method, string name)
		{
			ParameterData pd = TypeManager.GetParameterData (method);

			for (int i = 0; i < ParameterTypes.Length; i++) {
				GenericConstraints ogc = pd.GenericConstraints (i);
				GenericConstraints gc = ParameterInfo.GenericConstraints (i);

				if ((gc == null) && (ogc == null))
					continue;

				Type ot = pd.ParameterType (i);
				Type t = ParameterTypes [i];

				if (!((gc != null) && (ogc != null))) {
					error_425 (ot, t, name);
					return false;
				}

				if ((gc.Attributes != ogc.Attributes) ||
				    (gc.HasClassConstraint != ogc.HasClassConstraint)) {
					error_425 (ot, t, name);
					return false;
				}

				if (ogc.HasClassConstraint &&
				    !ogc.ClassConstraint.Equals (gc.ClassConstraint)) {
					error_425 (ot, t, name);
					return false;
				}

				Type[] oct = ogc.InterfaceConstraints;
				Type[] ct = gc.InterfaceConstraints;

				if (oct.Length != ct.Length) {
					error_425 (ot, t, name);
					return false;
				}

				for (int j = 0; j < oct.Length; j++)
					if (!oct [j].Equals (ct [j])) {
						error_425 (ot, t, name);
						return false;
					}
			}

			return true;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds)) {
				if ((ModFlags & Modifiers.ABSTRACT) != 0 && IsExposedFromAssembly (ds) && ds.IsClsComplianceRequired (ds)) {
					Report.Error (3011, Location, "'{0}': only CLS-compliant members can be abstract", GetSignatureForError ());
				}
				return false;
			}

			if (Parameters.HasArglist) {
				Report.Error (3000, Location, "Methods with variable arguments are not CLS-compliant");
			}

			if (!AttributeTester.IsClsCompliant (MemberType)) {
				if (this is PropertyBase)
					Report.Error (3003, Location, "Type of `{0}' is not CLS-compliant",
						      GetSignatureForError ());
				else
					Report.Error (3002, Location, "Return type of '{0}' is not CLS-compliant",
						      GetSignatureForError ());
			}

			AttributeTester.AreParametersCompliant (Parameters.FixedParameters, Location);

			return true;
		}

		bool MayUnify (MethodCore first, MethodCore second)
		{
			int a_type_params = 0;
			if (first.GenericMethod != null)
				a_type_params = first.GenericMethod.CountTypeParameters;

			int b_type_params = 0;
			if (second.GenericMethod != null)
				b_type_params = second.GenericMethod.CountTypeParameters;

			if (a_type_params != b_type_params)
				return false;

			Type[] class_infered, method_infered;
			if (Parent.CountTypeParameters > 0)
				class_infered = new Type [Parent.CountTypeParameters];
			else
				class_infered = null;

			if (a_type_params > 0)
				method_infered = new Type [a_type_params];
			else
				method_infered = null;

			return TypeManager.MayBecomeEqualGenericInstances (
				first.ParameterTypes, second.ParameterTypes, class_infered, method_infered);
		}

		protected bool IsDuplicateImplementation (MethodCore method)
		{
			if ((method == this) ||
			    (method.MemberName.GetTypeName () != MemberName.GetTypeName ()))
				return false;

			Type[] param_types = method.ParameterTypes;
			if (param_types == null)
				return false;

			if (param_types.Length != ParameterTypes.Length)
				return false;

			bool equal = true;
			bool may_unify = MayUnify (this, method);

			for (int i = 0; i < param_types.Length; i++) {
				if (param_types [i] != ParameterTypes [i])
					equal = false;
			}

			if (IsExplicitImpl && (method.InterfaceType != InterfaceType))
				equal = may_unify = false;

			// TODO: make operator compatible with MethodCore to avoid this
			if (this is Operator && method is Operator) {
				if (MemberType != method.MemberType)
					equal = may_unify = false;
			}

			if (equal) {
				//
				// Try to report 663: method only differs on out/ref
				//
				ParameterData info = ParameterInfo;
				ParameterData other_info = method.ParameterInfo;
				for (int i = 0; i < info.Count; i++){
					if (info.ParameterModifier (i) != other_info.ParameterModifier (i)){
						Report.Error (663, Location,
							      "Overload method only differs " +
							      "in parameter modifier");
						return false;
					}
				}

				Report.SymbolRelatedToPreviousError (method);
				if (this is Operator && method is Operator)
					Report.Error (557, Location,
						      "Duplicate user-defined conversion in type '{0}'",
						      Parent.Name);
				else
					Report.Error (111, Location,
						      "Type '{0}' already defines a member called '{1}' " +
						      "with the same parameter types", Parent.Name, Name);
				return true;
			} else if (may_unify) {
				Report.Error (408, Location,
					      "`{0}' cannot define overload members that " +
					      "may unify for some type parameter substitutions",
					      Parent.MemberName);
				return true;
			}
			return false;
		}

		public override bool IsUsed
		{
			get {
				if (IsExplicitImpl)
					return true;

				return base.IsUsed;
			}
		}

		//
		// Returns a string that represents the signature for this 
		// member which should be used in XML documentation.
		//
		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, ds);
		}

		//
		// Raised (and passed an XmlElement that contains the comment)
		// when GenerateDocComment is writing documentation expectedly.
		//
		// FIXME: with a few effort, it could be done with XmlReader,
		// that means removal of DOM use.
		//
		internal override void OnGenerateDocComment (DeclSpace ds, XmlElement el)
		{
			DocUtil.OnMethodGenerateDocComment (this, ds, el);
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "M:"; }
		}

		protected override void VerifyObsoleteAttribute()
		{
			base.VerifyObsoleteAttribute ();

			if (parameter_types == null)
				return;

			foreach (Type type in parameter_types) {
				CheckUsageOfObsoleteAttribute (type);
			}
		}
	}

	public class SourceMethod : ISourceMethod
	{
		TypeContainer container;
		MethodBase builder;

		protected SourceMethod (TypeContainer container, MethodBase builder,
					ISourceFile file, Location start, Location end)
		{
			this.container = container;
			this.builder = builder;
			
			CodeGen.SymbolWriter.OpenMethod (
				file, this, start.Row, 0, end.Row, 0);
		}

		public string Name {
			get { return builder.Name; }
		}

		public int NamespaceID {
			get { return container.NamespaceEntry.SymbolFileID; }
		}

		public int Token {
			get {
				if (builder is MethodBuilder)
					return ((MethodBuilder) builder).GetToken ().Token;
				else if (builder is ConstructorBuilder)
					return ((ConstructorBuilder) builder).GetToken ().Token;
				else
					throw new NotSupportedException ();
			}
		}

		public void CloseMethod ()
		{
			if (CodeGen.SymbolWriter != null)
				CodeGen.SymbolWriter.CloseMethod ();
		}

		public static SourceMethod Create (TypeContainer parent,
						   MethodBase builder, Block block)
		{
			if (CodeGen.SymbolWriter == null)
				return null;
			if (block == null)
				return null;

			Location start_loc = block.StartLocation;
			if (Location.IsNull (start_loc))
				return null;

			Location end_loc = block.EndLocation;
			if (Location.IsNull (end_loc))
				return null;

			ISourceFile file = start_loc.SourceFile;
			if (file == null)
				return null;

			return new SourceMethod (
				parent, builder, file, start_loc, end_loc);
		}
	}

	public class Method : MethodCore, IIteratorContainer, IMethodData {
		public MethodBuilder MethodBuilder;
		public MethodData MethodData;
		ReturnParameter return_attributes;
		ListDictionary declarative_security;

		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
		        Modifiers.UNSAFE |
			Modifiers.METHOD_YIELDS | 
			Modifiers.EXTERN;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW | Modifiers.UNSAFE;

		//
		// return_type can be "null" for VOID values.
		//
		public Method (TypeContainer parent, GenericMethod generic,
			       Expression return_type, int mod, bool is_iface,
			       MemberName name, Parameters parameters, Attributes attrs,
			       Location l)
			: base (parent, generic, return_type, mod,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, attrs, parameters, l)
		{
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method;
			}
		}
		
		public override string GetSignatureForError()
		{
			if (MethodBuilder == null) {
				return GetSignatureForError (Parent);
			}
			return TypeManager.CSharpSignature (MethodBuilder);
		}

		/// <summary>
		/// Use this method when MethodBuilder is null
		/// </summary>
		public override string GetSignatureForError (TypeContainer tc)
		{
			// TODO: get params from somewhere
			if (parameter_info == null)
				return base.GetSignatureForError (tc);

			// TODO: move to parameters
			System.Text.StringBuilder args = new System.Text.StringBuilder ();
			if (parameter_info.Parameters.FixedParameters != null) {
				for (int i = 0; i < parameter_info.Parameters.FixedParameters.Length; ++i) {
					Parameter p = parameter_info.Parameters.FixedParameters [i];
					args.Append (p.GetSignatureForError ());

					if (i < parameter_info.Parameters.FixedParameters.Length - 1)
						args.Append (',');
				}
			}

			return String.Concat (base.GetSignatureForError (tc), "(", args.ToString (), ")");
		}

                void DuplicateEntryPoint (MethodInfo b, Location location)
                {
                        Report.Error (
                                17, location,
                                "Program `" + CodeGen.FileName +
                                "'  has more than one entry point defined: `" +
                                TypeManager.CSharpSignature(b) + "'");
                }

                public bool IsEntryPoint (MethodBuilder b, InternalParameters pinfo)
                {
                        if (b.ReturnType != TypeManager.void_type &&
                            b.ReturnType != TypeManager.int32_type)
                                return false;

                        if (pinfo.Count == 0)
                                return true;

                        if (pinfo.Count > 1)
                                return false;

                        Type t = pinfo.ParameterType(0);
                        if (t.IsArray &&
                            (t.GetArrayRank() == 1) &&
                            (TypeManager.GetElementType(t) == TypeManager.string_type) &&
                            (pinfo.ParameterModifier(0) == Parameter.Modifier.NONE))
                                return true;
                        else
                                return false;
                }

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb);
				return;
			}

			if (a.Type == TypeManager.methodimpl_attr_type &&
				(a.GetMethodImplOptions () & MethodImplOptions.InternalCall) != 0) {
				MethodBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime);
			}

			if (a.Type == TypeManager.dllimport_type) {
				const int extern_static = Modifiers.EXTERN | Modifiers.STATIC;
				if ((ModFlags & extern_static) != extern_static) {
					Report.Error (601, a.Location, "The DllImport attribute must be specified on a method marked `static' and `extern'");
				}

				return;
			}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == TypeManager.conditional_attribute_type) {
				if (IsOperator || IsExplicitImpl) {
					Report.Error (577, Location, "Conditional not valid on '{0}' because it is a destructor, operator, or explicit interface implementation", GetSignatureForError ());
					return;
				}

				if (ReturnType != TypeManager.void_type) {
					Report.Error (578, Location, "Conditional not valid on '{0}' because its return type is not void", GetSignatureForError ());
					return;
				}

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					Report.Error (243, Location, "Conditional not valid on '{0}' because it is an override method", GetSignatureForError ());
					return;
				}

				if (IsInterface) {
					Report.Error (582, Location, "Conditional not valid on interface members");
					return;
				}

				if (MethodData.IsImplementing) {
					Report.Error (629, Location, "Conditional member '{0}' cannot implement interface member", GetSignatureForError ());
					return;
				}

				for (int i = 0; i < parameter_info.Count; ++i) {
					if ((parameter_info.ParameterModifier (i) & Parameter.Modifier.OUT) != 0) {
						Report.Error (685, Location, "Conditional method '{0}' cannot have an out parameter", GetSignatureForError ());
						return;
					}
				}
			}

			MethodBuilder.SetCustomAttribute (cb);
		}

  		protected override bool CheckForDuplications ()
		{
			ArrayList ar = Parent.Methods;
  			if (ar != null) {
  				int arLen = ar.Count;
   					
  				for (int i = 0; i < arLen; i++) {
  					Method m = (Method) ar [i];
  					if (IsDuplicateImplementation (m))
  						return false;
   				}
  			}

			ar = Parent.Properties;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			ar = Parent.Indexers;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			ar = Parent.Events;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					Event ev = (Event) ar [i];
					if (ev.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			return true;
		}

		//
		// Creates the type
		//
		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			MethodBuilder mb = null;
			if (GenericMethod != null) {
				string method_name = MemberName.Name;

				if (IsExplicitImpl)
					method_name = TypeManager.GetFullName (InterfaceType) + "." + method_name;

				mb = Parent.TypeBuilder.DefineGenericMethod (method_name, flags);
				if (!GenericMethod.Define (mb))
					return false;
			}

			if (!DoDefine ())
				return false;

			if (RootContext.StdLib && (ReturnType == TypeManager.arg_iterator_type || ReturnType == TypeManager.typed_reference_type)) {
				Error1599 (Location, ReturnType);
				return false;
			}

			if (!CheckBase ())
				return false;

			if (IsOperator)
				flags |= MethodAttributes.SpecialName | MethodAttributes.HideBySig;

			MethodData = new MethodData (this, ParameterInfo, ModFlags, flags,
						     this, mb, GenericMethod, base_method);

			if (!MethodData.Define (Parent))
				return false;

			if (ReturnType == TypeManager.void_type && ParameterTypes.Length == 0 && 
				Name == "Finalize" && !(this is Destructor)) {
				Report.Warning (465, 1, Location, "Introducing a 'Finalize' method can interfere with destructor invocation. Did you intend to declare a destructor?");
			}

			//
			// Setup iterator if we are one
			//
			if ((ModFlags & Modifiers.METHOD_YIELDS) != 0){
				Iterator iterator = new Iterator (
					Parent, Name, MemberType,
					ParameterInfo, ModFlags, block, Location);

				if (!iterator.DefineIterator ())
					return false;

				block = iterator.Block;
			}

			MethodBuilder = MethodData.MethodBuilder;

			//
			// This is used to track the Entry Point,
			//
			if (Name == "Main" &&
			    ((ModFlags & Modifiers.STATIC) != 0) && RootContext.NeedsEntryPoint && 
			    (RootContext.MainClass == null ||
			     RootContext.MainClass == Parent.TypeBuilder.FullName)){
                                if (IsEntryPoint (MethodBuilder, ParameterInfo)) {
                                        IMethodData md = TypeManager.GetMethod (MethodBuilder);
                                        md.SetMemberIsUsed ();

                                        if (RootContext.EntryPoint == null) {
						if (Parent.IsGeneric){
							Report.Error (-201, Location,
								      "Entry point can not be defined in a generic class");
						}
						
                                                RootContext.EntryPoint = MethodBuilder;
                                                RootContext.EntryPointLocation = Location;
                                        } else {
                                                DuplicateEntryPoint (RootContext.EntryPoint, RootContext.EntryPointLocation);
                                                DuplicateEntryPoint (MethodBuilder, Location);
                                        }
                                } else {
                                 	if (RootContext.WarningLevel >= 4)
						Report.Warning (28, Location, "'{0}' has the wrong signature to be an entry point", TypeManager.CSharpSignature(MethodBuilder) );
				}
			}

			if (MemberType.IsAbstract && MemberType.IsSealed) {
				Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
				return false;
			}

			return true;
		}

		//
		// Emits the code
		// 
		public override void Emit ()
		{
			MethodData.Emit (Parent, this);
			base.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					MethodBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			Block = null;
			MethodData = null;
		}

		public static void Error1599 (Location loc, Type t)
		{
			Report.Error (1599, loc, "Method or delegate cannot return type '{0}'", TypeManager.CSharpName (t));
		}

		protected override MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type)
		{
			MethodInfo mi = (MethodInfo) container.BaseCache.FindMemberToOverride (
				container.TypeBuilder, Name, ParameterTypes, false);

			if (mi == null)
				return null;

			base_ret_type = mi.ReturnType;
			return mi;
		}
	
		public override bool MarkForDuplicationCheck ()
		{
			caching_flags |= Flags.TestMethodDuplication;
			return true;
		}

		protected override bool VerifyClsCompliance(DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			if (parameter_types.Length > 0) {
				ArrayList al = (ArrayList)ds.MemberCache.Members [Name];
				if (al.Count > 1)
					ds.MemberCache.VerifyClsParameterConflict (al, this, MethodBuilder);
			}

			return true;
		}


		void IIteratorContainer.SetYields ()
		{
			ModFlags |= Modifiers.METHOD_YIELDS;
		}

		#region IMethodData Members

		public CallingConventions CallingConventions {
			get {
				CallingConventions cc = Parameters.GetCallingConvention ();
				if (Parameters.HasArglist)
					block.HasVarargs = true;

				if (!IsInterface)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
			}
		}

		public Type ReturnType {
			get {
				return MemberType;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public new Location Location {
			get {
				return base.Location;
			}
		}

		protected override bool CheckBase() {
			if (!base.CheckBase ())
				return false;

			// TODO: Destructor should derive from MethodCore
			if (base_method != null && (ModFlags & Modifiers.OVERRIDE) != 0 && Name == "Finalize" &&
				base_method.DeclaringType == TypeManager.object_type && !(this is Destructor)) {
				Report.Error (249, Location, "Do not override object.Finalize. Instead, provide a destructor");
				return false;
			}

			return true;
		}

		public EmitContext CreateEmitContext (TypeContainer tc, ILGenerator ig)
		{
			EmitContext ec = new EmitContext (
				tc, ds, Location, ig, ReturnType, ModFlags, false);

			Iterator iterator = tc as Iterator;
			if (iterator != null)
				ec.CurrentAnonymousMethod = iterator.Host;

			return ec;
		}

		public ObsoleteAttribute GetObsoleteAttribute ()
		{
			return GetObsoleteAttribute (ds);
		}

		/// <summary>
		/// Returns true if method has conditional attribute and the conditions is not defined (method is excluded).
		/// </summary>
		public bool IsExcluded (EmitContext ec)
		{
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (base_method == null) {
				if (OptAttributes == null)
					return false;

				Attribute[] attrs = OptAttributes.SearchMulti (TypeManager.conditional_attribute_type, ec);

				if (attrs == null)
					return false;

				foreach (Attribute a in attrs) {
					string condition = a.GetConditionalAttributeValue (Parent.EmitContext);
					if (RootContext.AllDefines.Contains (condition))
						return false;
				}

				caching_flags |= Flags.Excluded;
				return true;
			}

			IMethodData md = TypeManager.GetMethod (base_method);
			if (md == null) {
				if (AttributeTester.IsConditionalMethodExcluded (base_method)) {
					caching_flags |= Flags.Excluded;
					return true;
				}
				return false;
			}

			if (md.IsExcluded (ec)) {
				caching_flags |= Flags.Excluded;
				return true;
			}
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return GenericMethod;
			}
		}

		#endregion
	}

	public abstract class ConstructorInitializer {
		ArrayList argument_list;
		protected ConstructorInfo base_constructor;
		Location loc;
		
		public ConstructorInitializer (ArrayList argument_list, Location loc)
		{
			this.argument_list = argument_list;
			this.loc = loc;
		}

		public ArrayList Arguments {
			get {
				return argument_list;
			}
		}

		public bool Resolve (ConstructorBuilder caller_builder, Block block, EmitContext ec)
		{
			Expression base_constructor_group;
			Type t;
			bool error = false;

			ec.CurrentBlock = block;

			if (argument_list != null){
				foreach (Argument a in argument_list){
					if (!a.Resolve (ec, loc))
						return false;
				}
			}
			ec.CurrentBlock = null;

			if (this is ConstructorBaseInitializer) {
				if (ec.ContainerType.BaseType == null)
					return true;

				t = ec.ContainerType.BaseType;
				if (ec.ContainerType.IsValueType) {
					Report.Error (522, loc,
						"structs cannot call base class constructors");
					return false;
				}
			} else
				t = ec.ContainerType;

			base_constructor_group = Expression.MemberLookup (
				ec, t, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				loc);
			
			if (base_constructor_group == null){
				error = true;
				base_constructor_group = Expression.MemberLookup (
					ec, t, null, t, ".ctor", MemberTypes.Constructor,
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
					loc);
			}

			int errors = Report.Errors;
			if (base_constructor_group != null)
				base_constructor = (ConstructorInfo) Invocation.OverloadResolve (
					ec, (MethodGroupExpr) base_constructor_group, argument_list,
					false, loc);
			
			if (base_constructor == null) {
				if (errors == Report.Errors)
					Report.Error (1501, loc, "Can not find a constructor for this argument list");
				return false;
			}

			if (error) {
				Report.Error (122, loc, "`{0}' is inaccessible due to its protection level",
					      TypeManager.CSharpSignature (base_constructor));
				base_constructor = null;
				return false;
			}
			
			if (base_constructor == caller_builder){
				Report.Error (516, loc, "Constructor `{0}' can not call itself", TypeManager.CSharpSignature (caller_builder));
				return false;
			}
			
			return true;
		}

		public void Emit (EmitContext ec)
		{
			if (base_constructor != null){
				ec.Mark (loc, false);
				if (ec.IsStatic)
					Invocation.EmitCall (ec, true, true, null, base_constructor, argument_list, loc);
				else
					Invocation.EmitCall (ec, true, false, ec.GetThis (loc), base_constructor, argument_list, loc);
			}
		}
	}

	public class ConstructorBaseInitializer : ConstructorInitializer {
		public ConstructorBaseInitializer (ArrayList argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}

	public class ConstructorThisInitializer : ConstructorInitializer {
		public ConstructorThisInitializer (ArrayList argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}
	
	public class Constructor : MethodCore, IMethodData {
		public ConstructorBuilder ConstructorBuilder;
		public ConstructorInitializer Initializer;
		ListDictionary declarative_security;

		// <summary>
		//   Modifiers allowed for a constructor.
		// </summary>
		public const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.STATIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |		
			Modifiers.PRIVATE;

		bool has_compliant_args = false;
		//
		// The spec claims that static is not permitted, but
		// my very own code has static constructors.
		//
		public Constructor (TypeContainer ds, string name, int mod, Parameters args,
				    ConstructorInitializer init, Location l)
			: base (ds, null, null, mod, AllowedModifiers, false,
				new MemberName (name), null, args, l)
		{
			Initializer = init;
		}

		public override string GetSignatureForError()
		{
			if (ConstructorBuilder == null)
				return GetSignatureForError (Parent);

			return TypeManager.CSharpSignature (ConstructorBuilder);
		}

		public bool HasCompliantArgs {
			get {
				return has_compliant_args;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Constructor;
			}
		}


		//
		// Returns true if this is a default constructor
		//
		public bool IsDefault ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0)
				return  (Parameters.FixedParameters == null ? true : Parameters.Empty) &&
					(Parameters.ArrayParameter == null ? true : Parameters.Empty);
			
			else
				return  (Parameters.FixedParameters == null ? true : Parameters.Empty) &&
					(Parameters.ArrayParameter == null ? true : Parameters.Empty) &&
					(Initializer is ConstructorBaseInitializer) &&
					(Initializer.Arguments == null);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null) {
					declarative_security = new ListDictionary ();
				}
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			ConstructorBuilder.SetCustomAttribute (cb);
		}

 		protected override bool CheckForDuplications ()
		{
			ArrayList ar = Parent.InstanceConstructors;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Constructor m = (Constructor) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}
			return true;
		}
			
		protected override bool CheckBase ()
		{
			// Check whether arguments were correct.
			if (!DoDefineParameters ())
				return false;
			
			// TODO: skip the rest for generated ctor
			if ((ModFlags & Modifiers.STATIC) != 0)
				return true;
			
			if (!CheckForDuplications ())
				return false;

			if (Parent.Kind == Kind.Struct) {
				if (ParameterTypes.Length == 0) {
				Report.Error (568, Location, 
					"Structs can not contain explicit parameterless " +
					"constructors");
				return false;
			}
				
				if ((ModFlags & Modifiers.PROTECTED) != 0) {
					Report.Error (666, Location, "Protected member in struct declaration");
						return false;
				}
			}
			
			if ((RootContext.WarningLevel >= 4) && ((Parent.ModFlags & Modifiers.SEALED) != 0 && (ModFlags & Modifiers.PROTECTED) != 0)) {
				Report.Warning (628, Location, "'{0}': new protected member declared in sealed class", GetSignatureForError (Parent));
			}
			
			return true;
		}
		
		//
		// Creates the ConstructorBuilder
		//
		public override bool Define ()
		{
			if (ConstructorBuilder != null)
				return true;

			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);

			if ((ModFlags & Modifiers.STATIC) != 0){
				ca |= MethodAttributes.Static | MethodAttributes.Private;
			} else {
				ca |= MethodAttributes.HideBySig;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					ca |= MethodAttributes.Public;
				else if ((ModFlags & Modifiers.PROTECTED) != 0){
					if ((ModFlags & Modifiers.INTERNAL) != 0)
						ca |= MethodAttributes.FamORAssem;
					else 
						ca |= MethodAttributes.Family;
				} else if ((ModFlags & Modifiers.INTERNAL) != 0)
					ca |= MethodAttributes.Assembly;
				else if (IsDefault ())
					ca |= MethodAttributes.Public;
				else
					ca |= MethodAttributes.Private;
			}

			// Check if arguments were correct.
			if (!CheckBase ())
				return false;

			ConstructorBuilder = Parent.TypeBuilder.DefineConstructor (
				ca, CallingConventions,
				ParameterTypes);

			if ((ModFlags & Modifiers.UNSAFE) != 0)
				ConstructorBuilder.InitLocals = false;
			
			TypeManager.AddMethod (ConstructorBuilder, this);

			//
			// HACK because System.Reflection.Emit is lame
			//
			TypeManager.RegisterMethod (ConstructorBuilder, ParameterInfo, ParameterTypes);

			return true;
		}

		//
		// Emits the code
		//
		public override void Emit ()
		{
			EmitContext ec = CreateEmitContext (null, null);

			//
			// extern methods have no bodies
			//
			if ((ModFlags & Modifiers.EXTERN) != 0) {
				if ((block != null) && ((ModFlags & Modifiers.EXTERN) != 0)) {
					Report.Error (
						179, Location, "External constructor `" +
						TypeManager.CSharpSignature (ConstructorBuilder) +
						"' can not have a body");
					return;
				}
			} else if (block == null) {
				Report.Error (
					501, Location, "Constructor `" +
					TypeManager.CSharpSignature (ConstructorBuilder) +
					"' must declare a body since it is not marked extern");
				return;
			}

			// If this is a non-static `struct' constructor and doesn't have any
			// initializer, it must initialize all of the struct's fields.
			if ((Parent.Kind == Kind.Struct) &&
			    ((ModFlags & Modifiers.STATIC) == 0) && (Initializer == null))
				Block.AddThisVariable (Parent, Location);

			if (block != null)
				block.ResolveMeta (ec, ParameterInfo);

			if ((ModFlags & Modifiers.STATIC) == 0){
				if (Parent.Kind == Kind.Class && Initializer == null)
					Initializer = new ConstructorBaseInitializer (
						null, Location);


				//
				// Spec mandates that Initializers will not have
				// `this' access
				//
				ec.IsStatic = true;
				if ((Initializer != null) &&
				    !Initializer.Resolve (ConstructorBuilder, block, ec))
					return;
				ec.IsStatic = false;
			}

			Parameters.LabelParameters (ec, ConstructorBuilder);
			
			SourceMethod source = SourceMethod.Create (
				Parent, ConstructorBuilder, block);

			//
			// Classes can have base initializers and instance field initializers.
			//
			if (Parent.Kind == Kind.Class){
				if ((ModFlags & Modifiers.STATIC) == 0){

					//
					// If we use a "this (...)" constructor initializer, then
					// do not emit field initializers, they are initialized in the other constructor
					//
					if (!(Initializer != null && Initializer is ConstructorThisInitializer))
						Parent.EmitFieldInitializers (ec);
				}
			}
			if (Initializer != null) {
				if (GetObsoleteAttribute () != null || Parent.GetObsoleteAttribute (Parent) != null)
					ec.TestObsoleteMethodUsage = false;

				Initializer.Emit (ec);
			}
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				Parent.EmitFieldInitializers (ec);

			if (OptAttributes != null) 
				OptAttributes.Emit (ec, this);

			ec.EmitTopBlock (block, ParameterInfo, Location);

			if (source != null)
				source.CloseMethod ();

			base.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					ConstructorBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			block = null;
		}

		// Is never override
		protected override MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type)
		{
			return null;
		}
						
		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds) || !IsExposedFromAssembly (ds)) {
				return false;
			}

 			if (parameter_types.Length > 0) {
 				ArrayList al = (ArrayList)ds.MemberCache.Members [".ctor"];
 				if (al.Count > 3)
 					ds.MemberCache.VerifyClsParameterConflict (al, this, ConstructorBuilder);
				
				if (ds.TypeBuilder.IsSubclassOf (TypeManager.attribute_type)) {
					foreach (Type param in parameter_types) {
						if (param.IsArray) {
							return true;
				}
			}
				}
			}
			has_compliant_args = true;
				return true;
		}

		#region IMethodData Members

		public System.Reflection.CallingConventions CallingConventions {
			get {
				CallingConventions cc = Parameters.GetCallingConvention ();

				if (Parent.Kind == Kind.Class)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
				}
			}
			
		public new Location Location {
			get {
				return base.Location;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}
			
		public Type ReturnType {
			get {
				return MemberType;
			}
		}

		public EmitContext CreateEmitContext (TypeContainer tc, ILGenerator ig)
		{
			ILGenerator ig_ = ConstructorBuilder.GetILGenerator ();
			return new EmitContext (Parent, Location, ig_, null, ModFlags, true);
		}

		public ObsoleteAttribute GetObsoleteAttribute ()
		{
			return GetObsoleteAttribute (Parent);
		}

		public bool IsExcluded(EmitContext ec)
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		#endregion
	}

	/// <summary>
	/// Interface for MethodData class. Holds links to parent members to avoid member duplication.
	/// </summary>
	public interface IMethodData
	{
		CallingConventions CallingConventions { get; }
		Location Location { get; }
		MemberName MethodName { get; }
		Type[] ParameterTypes { get; }
		Type ReturnType { get; }
		GenericMethod GenericMethod { get; }

		Attributes OptAttributes { get; }
		ToplevelBlock Block { get; }

		EmitContext CreateEmitContext (TypeContainer tc, ILGenerator ig);
		ObsoleteAttribute GetObsoleteAttribute ();
		string GetSignatureForError (TypeContainer tc);
		bool IsExcluded (EmitContext ec);
		bool IsClsComplianceRequired (DeclSpace ds);
		void SetMemberIsUsed ();
	}

	//
	// Encapsulates most of the Method's state
	//
	public class MethodData {

		readonly IMethodData method;

		//
		// The return type of this method
		//
		public readonly GenericMethod GenericMethod;
		public readonly InternalParameters ParameterInfo;

		//
		// Are we implementing an interface ?
		//
		public bool IsImplementing = false;

		//
		// Protected data.
		//
		protected MemberBase member;
		protected int modifiers;
		protected MethodAttributes flags;
		protected Type declaring_type;
		protected MethodInfo parent_method;

		MethodBuilder builder = null;
		public MethodBuilder MethodBuilder {
			get {
				return builder;
			}
		}

		public Type DeclaringType {
			get {
				return declaring_type;
			}
		}

		public MethodData (MemberBase member, InternalParameters parameters,
				   int modifiers, MethodAttributes flags, IMethodData method)
		{
			this.member = member;
			this.ParameterInfo = parameters;
			this.modifiers = modifiers;
			this.flags = flags;

			this.method = method;
		}

		public MethodData (MemberBase member, InternalParameters parameters,
				   int modifiers, MethodAttributes flags, 
				   IMethodData method, MethodBuilder builder,
				   GenericMethod generic, MethodInfo parent_method)
			: this (member, parameters, modifiers, flags, method)
		{
			this.builder = builder;
			this.GenericMethod = generic;
			this.parent_method = parent_method;
		}

		public bool Define (TypeContainer container)
		{
			MethodInfo implementing = null;

			string name = method.MethodName.Basename;
			string method_name = method.MethodName.FullName;

			Type[] ParameterTypes = method.ParameterTypes;

			if (container.Pending != null){
				if (member is Indexer) // TODO: test it, but it should work without this IF
					implementing = container.Pending.IsInterfaceIndexer (
						member.InterfaceType, method.ReturnType, ParameterInfo);
				else
					implementing = container.Pending.IsInterfaceMethod (
						member.InterfaceType, name, method.ReturnType, ParameterInfo);

				if (member.InterfaceType != null){
					if (implementing == null){
						if (member is PropertyBase) {
							Report.Error (550, method.Location, "'{0}' is an accessor not found in interface member '{1}'",
								      method.GetSignatureForError (container), member.Name);

						} else {
							Report.Error (539, method.Location, "'{0}' in explicit interface " +
								      "declaration is not a member of interface",
								      member.Name);
						}
						return false;
					}
					if (implementing.IsSpecialName && !((member is PropertyBase || member is EventProperty))) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (683, method.Location, "'{0}' explicit method implementation cannot implement '{1}' because it is an accessor",
							member.GetSignatureForError (), TypeManager.CSharpSignature (implementing));
						return false;
					}

					method_name = TypeManager.GetFullName (member.InterfaceType) + "." + method_name;
				} else {
					if (implementing != null && method is AbstractPropertyEventMethod && !implementing.IsSpecialName) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (686, method.Location, "Accessor '{0}' cannot implement interface member '{1}' for type '{2}'. Use an explicit interface implementation",
							method.GetSignatureForError (container), TypeManager.CSharpSignature (implementing), container.GetSignatureForError ());
						return false;
					}
				}
			}

			//
			// For implicit implementations, make sure we are public, for
			// explicit implementations, make sure we are private.
			//
			if (implementing != null){
				//
				// Setting null inside this block will trigger a more
				// verbose error reporting for missing interface implementations
				//
				// The "candidate" function has been flagged already
				// but it wont get cleared
				//
				if (member.IsExplicitImpl){
					if ((modifiers & (Modifiers.PUBLIC | Modifiers.ABSTRACT | Modifiers.VIRTUAL)) != 0){
						Modifiers.Error_InvalidModifier (method.Location, "public, virtual or abstract");
						implementing = null;
					}
				} else if ((flags & MethodAttributes.MemberAccessMask) != MethodAttributes.Public){
					if (TypeManager.IsInterfaceType (implementing.DeclaringType)){
						//
						// If this is an interface method implementation,
						// check for public accessibility
						//
						implementing = null;
					} else if ((flags & MethodAttributes.MemberAccessMask) == MethodAttributes.Private){
						// We may never be private.
						implementing = null;
					} else if ((modifiers & Modifiers.OVERRIDE) == 0){
						//
						// We may be protected if we're overriding something.
						//
						implementing = null;
					}
				} 
					
				//
				// Static is not allowed
				//
				if ((modifiers & Modifiers.STATIC) != 0){
					implementing = null;
					Modifiers.Error_InvalidModifier (method.Location, "static");
				}
			}
			
			//
			// If implementing is still valid, set flags
			//
			if (implementing != null){
				//
				// When implementing interface methods, set NewSlot
				// unless, we are overwriting a method.
				//
				if (implementing.DeclaringType.IsInterface){
					if ((modifiers & Modifiers.OVERRIDE) == 0)
						flags |= MethodAttributes.NewSlot;
				}
				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.HideBySig;

				// Set Final unless we're virtual, abstract or already overriding a method.
				if ((modifiers & (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE)) == 0)
					flags |= MethodAttributes.Final;

				IsImplementing = true;
			}

			EmitContext ec = method.CreateEmitContext (container, null);

			DefineMethodBuilder (ec, container, method_name, ParameterTypes);

			if (builder == null)
				return false;

			if (container.CurrentType != null)
				declaring_type = container.CurrentType;
			else
				declaring_type = container.TypeBuilder;

			if ((modifiers & Modifiers.UNSAFE) != 0)
				builder.InitLocals = false;

			if (IsImplementing){
				//
				// clear the pending implemntation flag
				//
				if (member is Indexer) {
					container.Pending.ImplementIndexer (
						member.InterfaceType, builder, method.ReturnType,
						ParameterInfo, member.IsExplicitImpl);
				} else
					container.Pending.ImplementMethod (
						member.InterfaceType, name, method.ReturnType,
						ParameterInfo, member.IsExplicitImpl);

				if (member.IsExplicitImpl)
					container.TypeBuilder.DefineMethodOverride (
						builder, implementing);
			}

			TypeManager.RegisterMethod (builder, ParameterInfo, ParameterTypes);
			TypeManager.AddMethod (builder, method);

			if (GenericMethod != null) {
				bool is_override = member.IsExplicitImpl |
					((modifiers & Modifiers.OVERRIDE) != 0);

				if (implementing != null)
					parent_method = implementing;

				if (!GenericMethod.DefineType (ec, builder, parent_method, is_override))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Create the MethodBuilder for the method 
		/// </summary>
		void DefineMethodBuilder (EmitContext ec, TypeContainer container, string method_name, Type[] ParameterTypes)
		{
			const int extern_static = Modifiers.EXTERN | Modifiers.STATIC;

			if ((modifiers & extern_static) == extern_static) {

				if (method.OptAttributes != null) {
					Attribute dllimport_attribute = method.OptAttributes.Search (TypeManager.dllimport_type, ec);
					if (dllimport_attribute != null) {
						flags |= MethodAttributes.PinvokeImpl;
						builder = dllimport_attribute.DefinePInvokeMethod (
							ec, container.TypeBuilder, method_name, flags,
							method.ReturnType, ParameterTypes);

						return;
					}
				}

				// for extern static method must be specified either DllImport attribute or MethodImplAttribute.
				// We are more strict than Microsoft and report CS0626 like error
				if (method.OptAttributes == null ||
					!method.OptAttributes.Contains (TypeManager.methodimpl_attr_type, ec)) {
					Report.Error (626, method.Location, "Method, operator, or accessor '{0}' is marked external and has no attributes on it. Consider adding a DllImport attribute to specify the external implementation", method.GetSignatureForError (container));
					return;
				}
			}

			if (builder == null)
				builder = container.TypeBuilder.DefineMethod (
					method_name, flags, method.CallingConventions,
					method.ReturnType, ParameterTypes);
			else
				builder.SetGenericMethodSignature (
					flags, method.CallingConventions,
					method.ReturnType, ParameterTypes);
		}

		//
		// Emits the code
		// 
		public void Emit (TypeContainer container, Attributable kind)
		{
			EmitContext ec;
			if ((flags & MethodAttributes.PinvokeImpl) == 0)
				ec = method.CreateEmitContext (container, builder.GetILGenerator ());
			else
				ec = method.CreateEmitContext (container, null);

			if (method.GetObsoleteAttribute () != null || container.GetObsoleteAttribute (container) != null)
				ec.TestObsoleteMethodUsage = false;

			Location loc = method.Location;
			Attributes OptAttributes = method.OptAttributes;

			if (OptAttributes != null)
				OptAttributes.Emit (ec, kind);

			if (member is MethodCore)
				((MethodCore) member).Parameters.LabelParameters (ec, MethodBuilder);
                        
			ToplevelBlock block = method.Block;
			
			//
			// abstract or extern methods have no bodies
			//
			if ((modifiers & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0){
				if (block == null)
					return;

				//
				// abstract or extern methods have no bodies.
				//
				if ((modifiers & Modifiers.ABSTRACT) != 0)
					Report.Error (
						500, method.Location, "Abstract method `" +
						TypeManager.CSharpSignature (builder) +
						"' can not have a body");

				if ((modifiers & Modifiers.EXTERN) != 0)
					Report.Error (
						179, method.Location, "External method `" +
						TypeManager.CSharpSignature (builder) +
						"' can not have a body");

				return;
			}

			//
			// Methods must have a body unless they're extern or abstract
			//
			if (block == null) {
				Report.Error (
					501, method.Location, "Method `" +
					TypeManager.CSharpSignature (builder) +
					"' must declare a body since it is not marked " +
					"abstract or extern");
				return;
			}

			SourceMethod source = SourceMethod.Create (
				container, MethodBuilder, method.Block);

			//
			// Handle destructors specially
			//
			// FIXME: This code generates buggy code
			//
			if (member is Destructor)
				EmitDestructor (ec, block);
			else
				ec.EmitTopBlock (block, ParameterInfo, loc);

			if (source != null)
				source.CloseMethod ();
		}

		void EmitDestructor (EmitContext ec, ToplevelBlock block)
		{
			ILGenerator ig = ec.ig;
			
			Label finish = ig.DefineLabel ();

			block.SetDestructor ();
			
			ig.BeginExceptionBlock ();
			ec.ReturnLabel = finish;
			ec.HasReturnLabel = true;
			ec.EmitTopBlock (block, null, method.Location);
			
			// ig.MarkLabel (finish);
			ig.BeginFinallyBlock ();
			
			if (ec.ContainerType.BaseType != null) {
				Expression member_lookup = Expression.MemberLookup (
					ec, ec.ContainerType.BaseType, null, ec.ContainerType.BaseType,
					"Finalize", MemberTypes.Method, Expression.AllBindingFlags, method.Location);

				if (member_lookup != null){
					MethodGroupExpr base_destructor = ((MethodGroupExpr) member_lookup);
				
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Call, (MethodInfo) base_destructor.Methods [0]);
				}
			}
			
			ig.EndExceptionBlock ();
			//ig.MarkLabel (ec.ReturnLabel);
			ig.Emit (OpCodes.Ret);
		}
	}

	// Should derive from MethodCore
	public class Destructor : Method {

		public Destructor (TypeContainer ds, Expression return_type, int mod, string name,
				   Parameters parameters, Attributes attrs, Location l)
			: base (ds, null, return_type, mod, false, new MemberName (name),
				parameters, attrs, l)
		{ }

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.conditional_attribute_type) {
				Report.Error (577, Location, "Conditional not valid on '{0}' because it is a destructor, operator, or explicit interface implementation", GetSignatureForError ());
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}
	}
	
	abstract public class MemberBase : MemberCore {
		public Expression Type;

		public MethodAttributes flags;
		public readonly DeclSpace ds;

		protected readonly int explicit_mod_flags;

		//
		// The "short" name of this property / indexer / event.  This is the
		// name without the explicit interface.
		//
		public string ShortName {
			get { return MemberName.Name; }
			set {
				SetMemberName (new MemberName (MemberName.Left, value));
			}
		}

		//
		// The type of this property / indexer / event
		//
		protected Type member_type;
		public Type MemberType {
			get {
				if (member_type == null && Type != null) {
					EmitContext ec = ds.EmitContext;
					bool old_unsafe = ec.InUnsafe;
					ec.InUnsafe = InUnsafe;
					Type = Type.ResolveAsTypeTerminal (ec);
					ec.InUnsafe = old_unsafe;

					member_type = Type == null ? null : Type.Type;
				}
				return member_type;
			}
		}

		//
		// Whether this is an interface member.
		//
		public bool IsInterface;

		//
		// If true, this is an explicit interface implementation
		//
		public bool IsExplicitImpl;

		//
		// The interface type we are explicitly implementing
		//
		public Type InterfaceType = null;

		//
		// The constructor is only exposed to our children
		//
		protected MemberBase (TypeContainer parent, DeclSpace ds, Expression type, int mod,
				      int allowed_mod, int def_mod, MemberName name,
				      Attributes attrs, Location loc)
			: base (parent, name, attrs, loc)
		{
			this.ds = ds;
			explicit_mod_flags = mod;
			Type = type;
			ModFlags = Modifiers.Check (allowed_mod, mod, def_mod, loc);
			IsExplicitImpl = (MemberName.Left != null);
		}

		protected virtual bool CheckBase ()
		{
  			if ((ModFlags & Modifiers.PROTECTED) != 0 && Parent.Kind == Kind.Struct) {
				Report.Error (666, Location, "Protected member in struct declaration");
				return false;
			}

  			if ((RootContext.WarningLevel >= 4) &&
			    ((Parent.ModFlags & Modifiers.SEALED) != 0) &&
			    ((ModFlags & Modifiers.PROTECTED) != 0) &&
			    ((ModFlags & Modifiers.OVERRIDE) == 0) && (Name != "Finalize")) {
				Report.Warning (628, Location, "'{0}': new protected member declared in sealed class", GetSignatureForError (Parent));
			}

			return true;
		}

		protected abstract bool CheckGenericOverride (MethodInfo method, string name);

		protected virtual bool CheckParameters (DeclSpace ds, Type [] parameters)
		{
			bool error = false;

			foreach (Type partype in parameters){
				if (partype == TypeManager.void_type) {
					Report.Error (
						1547, Location, "Keyword 'void' cannot " +
						"be used in this context");
					return false;
				}

				if (partype.IsPointer){
					if (!UnsafeOK (ds))
						error = true;
					if (!TypeManager.VerifyUnManaged (TypeManager.GetElementType (partype), Location))
						error = true;
				}

				if (ds.AsAccessible (partype, ModFlags))
					continue;

				if (this is Indexer)
					Report.Error (55, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than indexer `" + Name + "'");
				else if ((this is Method) && ((Method) this).IsOperator)
					Report.Error (57, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than operator `" + Name + "'");
				else
					Report.Error (51, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than method `" + Name + "'");
				error = true;
			}

			return !error;
		}

		protected virtual bool DoDefineBase ()
		{
			EmitContext ec = Parent.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("MemberBase.DoDefine called too early");

			if (Name == null)
				throw new InternalErrorException ();

			if (IsInterface) {
				ModFlags = Modifiers.PUBLIC |
					Modifiers.ABSTRACT |
					Modifiers.VIRTUAL | (ModFlags & Modifiers.UNSAFE) | (ModFlags & Modifiers.NEW);

				flags = MethodAttributes.Public |
					MethodAttributes.Abstract |
					MethodAttributes.HideBySig |
					MethodAttributes.NewSlot |
					MethodAttributes.Virtual;
			} else {
				if (!Parent.MethodModifiersValid (this))
					return false;

				flags = Modifiers.MethodAttr (ModFlags);
			}

			if (IsExplicitImpl) {
				Expression expr = MemberName.Left.GetTypeExpression (Location);
				TypeExpr iface_texpr = expr.ResolveAsTypeTerminal (ec);
				if (iface_texpr == null)
					return false;

				InterfaceType = iface_texpr.Type;

				if (!InterfaceType.IsInterface) {
					Report.Error (538, Location, "'{0}' in explicit interface declaration is not an interface", TypeManager.CSharpName (InterfaceType));
					return false;
				}

				if (!Parent.VerifyImplements (InterfaceType, ShortName, Name, Location))
					return false;
				
				Modifiers.Check (Modifiers.AllowedExplicitImplFlags, explicit_mod_flags, 0, Location);
			}

			return true;
		}

		protected virtual bool DoDefine ()
		{
			EmitContext ec = ds.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("MemberBase.DoDefine called too early");

			ec.InUnsafe = InUnsafe;

			if (MemberType == null)
				return false;

			if ((Parent.ModFlags & Modifiers.SEALED) != 0){
				if ((ModFlags & (Modifiers.VIRTUAL|Modifiers.ABSTRACT)) != 0){
					Report.Error (549, Location, "Virtual method can not be contained in sealed class");
					return false;
				}
			}
			
			// verify accessibility
			if (!Parent.AsAccessible (MemberType, ModFlags)) {
				Report.SymbolRelatedToPreviousError (MemberType);
				if (this is Property)
					Report.Error (53, Location,
						      "Inconsistent accessibility: property type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than property `" + GetSignatureForError () + "'");
				else if (this is Indexer)
					Report.Error (54, Location,
						      "Inconsistent accessibility: indexer return type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than indexer `" + Name + "'");
				else if (this is MethodCore) {
					if (this is Operator)
						Report.Error (56, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than operator `" + Name + "'");
					else
						Report.Error (50, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than method `" + Name + "'");
				} else {
					Report.Error (52, Location,
						      "Inconsistent accessibility: field type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than field `" + Name + "'");
				}
				return false;
			}

			if (MemberType.IsPointer && !UnsafeOK (Parent))
				return false;

			return true;
		}

		public override string GetSignatureForError (TypeContainer tc)
		{
			return String.Concat (tc.Name, '.', base.GetSignatureForError (tc));
		}

		protected bool IsTypePermitted ()
		{
			if (MemberType == TypeManager.arg_iterator_type || MemberType == TypeManager.typed_reference_type) {
				Report.Error (610, Location, "Field or property cannot be of type '{0}'", TypeManager.CSharpName (MemberType));
				return false;
			}
			return true;
		}

		protected override bool VerifyClsCompliance(DeclSpace ds)
		{
			if (base.VerifyClsCompliance (ds)) {
				return true;
			}

			if (IsInterface && HasClsCompliantAttribute && ds.IsClsComplianceRequired (ds)) {
				Report.Error (3010, Location, "'{0}': CLS-compliant interfaces must have only CLS-compliant members", GetSignatureForError ());
			}
			return false;
		}

		protected override void VerifyObsoleteAttribute()
		{
			CheckUsageOfObsoleteAttribute (MemberType);
		}
	}

	//
	// Fields and Events both generate FieldBuilders, we use this to share 
	// their common bits.  This is also used to flag usage of the field
	//
	abstract public class FieldBase : MemberBase {
		public FieldBuilder  FieldBuilder;
		public Status status;

		[Flags]
		public enum Status : byte {
			ASSIGNED = 1,
			HAS_OFFSET = 4		// Used by FieldMember.
		}

		static string[] attribute_targets = new string [] { "field" };

		/// <summary>
		///  Symbol with same name in base class/struct
		/// </summary>
		public MemberInfo conflict_symbol;

		//
		// The constructor is only exposed to our children
		//
		protected FieldBase (TypeContainer parent, Expression type, int mod,
				     int allowed_mod, MemberName name, object init,
				     Attributes attrs, Location loc)
			: base (parent, parent, type, mod, allowed_mod, Modifiers.PRIVATE,
				name, attrs, loc)
		{
			this.init = init;
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Field;
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.marshal_as_attr_type) {
				UnmanagedMarshal marshal = a.GetMarshal (this);
				if (marshal != null) {
					FieldBuilder.SetMarshal (marshal);
				}
					return;
				}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			FieldBuilder.SetCustomAttribute (cb);
		}

		//
		// Whether this field has an initializer.
		//
		public bool HasInitializer {
			get {
				return init != null;
			}
		}

		protected readonly Object init;
		// Private.
		Expression init_expr;
		bool init_expr_initialized = false;

		protected override bool CheckGenericOverride (MethodInfo method, string name)
		{
			return true;
		}

		//
		// Resolves and returns the field initializer.
		//
		public Expression GetInitializerExpression (EmitContext ec)
		{
			if (init_expr_initialized)
				return init_expr;

			Expression e;
			if (init is Expression)
				e = (Expression) init;
			else
				e = new ArrayCreation (Type, "", (ArrayList)init, Location);

			// TODO: Any reason why we are using parent EC ?
			EmitContext parent_ec = Parent.EmitContext;

			bool old_is_static = parent_ec.IsStatic;
			bool old_is_ctor = parent_ec.IsConstructor;
			parent_ec.IsStatic = ec.IsStatic;
			parent_ec.IsConstructor = ec.IsConstructor;
			parent_ec.IsFieldInitializer = true;
			e = e.DoResolve (parent_ec);
			parent_ec.IsFieldInitializer = false;
			parent_ec.IsStatic = old_is_static;
			parent_ec.IsConstructor = old_is_ctor;

			init_expr = e;
			init_expr_initialized = true;

			return init_expr;
		}

 		protected override bool CheckBase ()
		{
 			if (!base.CheckBase ())
 				return false;

 			// TODO: Implement
 			if (IsInterface)
 				return true;

 			conflict_symbol = Parent.FindBaseMemberWithSameName (Name, false);
 			if (conflict_symbol == null) {
 				if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0)) {
 					Report.Warning (109, Location, "The member '{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError (Parent));
 				}
 				return true;
 			}

 			if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0) {
				Report.SymbolRelatedToPreviousError (conflict_symbol);
				Report.Warning (108, Location, "The keyword new is required on '{0}' because it hides inherited member", GetSignatureForError (Parent));
			}

			return true;
 		}

		protected override bool DoDefine ()
		{
			if (!base.DoDefine ())
				return false;

			if (MemberType == TypeManager.void_type) {
				Report.Error (1547, Location,
					      "Keyword 'void' cannot be used in this context");
				return false;
			}
			return true;
		}

		public override string GetSignatureForError ()
		{
			if (FieldBuilder == null) {
				return base.GetSignatureForError (Parent);
			}
			return TypeManager.GetFullNameSignature (FieldBuilder);
		}

		protected virtual bool IsFieldClsCompliant {
			get {
				if (FieldBuilder == null)
					return true;

				return AttributeTester.IsClsCompliant (FieldBuilder.FieldType);
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			if (!IsFieldClsCompliant) {
				Report.Error (3003, Location, "Type of '{0}' is not CLS-compliant", GetSignatureForError ());
			}
			return true;
		}


		public void SetAssigned ()
		{
			status |= Status.ASSIGNED;
		}
	}

	public abstract class FieldMember: FieldBase
	{
		protected FieldMember (TypeContainer parent, Expression type, int mod,
			int allowed_mod, MemberName name, object init, Attributes attrs, Location loc)
			: base (parent, type, mod, allowed_mod | Modifiers.ABSTRACT, name, init, attrs, loc)
		{
			if ((mod & Modifiers.ABSTRACT) != 0)
				Report.Error (681, loc, "The modifier 'abstract' is not valid on fields. Try using a property instead");
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.field_offset_attribute_type)
			{
				status |= Status.HAS_OFFSET;

				if (!Parent.HasExplicitLayout) {
					Report.Error (636, Location, "The FieldOffset attribute can only be placed on members of types marked with the StructLayout(LayoutKind.Explicit)");
					return;
				}

				if ((ModFlags & Modifiers.STATIC) != 0 || this is Const) {
					Report.Error (637, Location, "The FieldOffset attribute is not allowed on static or const fields");
					return;
				}
			}

			if (a.Type == TypeManager.fixed_buffer_attr_type) {
				Report.Error (1716, Location, "Do not use 'System.Runtime.CompilerServices.FixedBuffer' attribute. Use the 'fixed' field modifier instead");
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}


		public override bool Define()
		{
			EmitContext ec = Parent.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("FieldMember.Define called too early");

			if (MemberType == null)
				return false;
			
			if (MemberType == TypeManager.void_type) {
				Report.Error (1547, Location, "Keyword 'void' cannot be used in this context");
				return false;
			}

			if (!CheckBase ())
				return false;
			
			if (!Parent.AsAccessible (MemberType, ModFlags)) {
				Report.Error (52, Location,
					"Inconsistent accessibility: field type `" +
					TypeManager.CSharpName (MemberType) + "' is less " +
					"accessible than field `" + Name + "'");
				return false;
			}

			if (!IsTypePermitted ())
				return false;

			if (MemberType.IsPointer && !UnsafeOK (Parent))
				return false;

			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				EmitContext ec = new EmitContext (Parent, Location, null, FieldBuilder.FieldType, ModFlags);
				OptAttributes.Emit (ec, this);
			}

			if (Parent.HasExplicitLayout && ((status & Status.HAS_OFFSET) == 0) && (ModFlags & Modifiers.STATIC) == 0) {
				Report.Error (625, Location, "'{0}': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute.", GetSignatureForError ());
			}

			base.Emit ();
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "F:"; }
		}
	}

	interface IFixedBuffer
	{
		FieldInfo Element { get; }
		Type ElementType { get; }
	}

	public class FixedFieldExternal: IFixedBuffer
	{
		FieldInfo element_field;

		public FixedFieldExternal (FieldInfo fi)
		{
			element_field = fi.FieldType.GetField (FixedField.FixedElementName);
		}

		#region IFixedField Members

		public FieldInfo Element {
			get {
				return element_field;
			}
		}

		public Type ElementType {
			get {
				return element_field.FieldType;
			}
		}

		#endregion
	}

	/// <summary>
	/// Fixed buffer implementation
	/// </summary>
	public class FixedField: FieldMember, IFixedBuffer
	{
		public const string FixedElementName = "FixedElementField";
		static int GlobalCounter = 0;
		static object[] ctor_args = new object[] { (short)LayoutKind.Sequential };
		static FieldInfo[] fi;

		TypeBuilder fixed_buffer_type;
		FieldBuilder element;
		Expression size_expr;
		int buffer_size;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public FixedField (TypeContainer parent, Expression type, int mod, string name,
			Expression size_expr, Attributes attrs, Location loc):
			base (parent, type, mod, AllowedModifiers, new MemberName (name), null, attrs, loc)
		{
			if (RootContext.Version == LanguageVersion.ISO_1)
				Report.FeatureIsNotStandardized (loc, "fixed sized buffers");

			this.size_expr = size_expr;
		}

		public override bool Define()
		{
			if (Parent.Kind != Kind.Struct) {
				Report.Error (1642, Location, "Fixed buffer fields may only be members of structs");
				return false;
			}

			if (!base.Define ())
				return false;

			if (!TypeManager.IsPrimitiveType (MemberType)) {
				Report.Error (1663, Location, "Fixed sized buffer type must be one of the following: bool, byte, short, int, long, char, sbyte, ushort, uint, ulong, float or double");
				return false;
			}

			Expression e = size_expr.Resolve (Parent.EmitContext);
			if (e == null)
				return false;

			Constant c = e as Constant;
			if (c == null) {
				Report.Error (133, Location, "The expression being assigned to '{0}' must be constant", GetSignatureForError ());
				return false;
			}

			IntConstant buffer_size_const = c.ToInt (Location);
			if (buffer_size_const == null)
				return false;

			buffer_size = buffer_size_const.Value;

			if (buffer_size <= 0) {
				Report.Error (1665, Location, "Fixed sized buffer '{0}' must have a length greater than zero", GetSignatureForError ());
				return false;
			}

			int type_size = Expression.GetTypeSize (MemberType);

			if (buffer_size > int.MaxValue / type_size) {
				Report.Error (1664, Location, "Fixed sized buffer of length '{0}' and type '{1}' exceeded 2^31 limit",
					buffer_size.ToString (), TypeManager.CSharpName (MemberType));
				return false;
			}

			buffer_size *= type_size;

			// Define nested
			string name = String.Format ("<{0}>__FixedBuffer{1}", Name, GlobalCounter++);

			fixed_buffer_type = Parent.TypeBuilder.DefineNestedType (name,
				TypeAttributes.NestedPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, TypeManager.value_type);
			element = fixed_buffer_type.DefineField (FixedElementName, MemberType, FieldAttributes.Public);
			RootContext.RegisterCompilerGeneratedType (fixed_buffer_type);

			FieldBuilder = Parent.TypeBuilder.DefineField (Name, fixed_buffer_type, Modifiers.FieldAttr (ModFlags));
			TypeManager.RegisterFieldBase (FieldBuilder, this);

			return true;
		}

		public override void Emit()
		{
			if (fi == null)
				fi = new FieldInfo [] { TypeManager.struct_layout_attribute_type.GetField ("Size") };

			object[] fi_val = new object[1];
			fi_val [0] = buffer_size;

			CustomAttributeBuilder cab = new CustomAttributeBuilder (TypeManager.struct_layout_attribute_ctor, 
				ctor_args, fi, fi_val);
			fixed_buffer_type.SetCustomAttribute (cab);

			cab = new CustomAttributeBuilder (TypeManager.fixed_buffer_attr_ctor, new object[] { MemberType, buffer_size } );
			FieldBuilder.SetCustomAttribute (cab);
			base.Emit ();
		}

		protected override bool IsFieldClsCompliant {
			get {
				return false;
			}
		}

		#region IFixedField Members

		public FieldInfo Element {
			get {
				return element;
			}
		}

		public Type ElementType {
			get {
				return MemberType;
			}
		}

		#endregion
	}

	//
	// The Field class is used to represents class/struct fields during parsing.
	//
	public class Field : FieldMember {
		// <summary>
		//   Modifiers allowed in a class declaration
		// </summary>
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
		        Modifiers.VOLATILE |
		        Modifiers.UNSAFE |
			Modifiers.READONLY;

		public Field (TypeContainer parent, Expression type, int mod, string name,
			      Object expr_or_array_init, Attributes attrs, Location loc)
			: base (parent, type, mod, AllowedModifiers, new MemberName (name),
				expr_or_array_init, attrs, loc)
		{
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (RootContext.WarningLevel > 1){
				Type ptype = Parent.TypeBuilder.BaseType;

				// ptype is only null for System.Object while compiling corlib.
				if (ptype != null){
					TypeContainer.FindMembers (
						ptype, MemberTypes.Method,
						BindingFlags.Public |
						BindingFlags.Static | BindingFlags.Instance,
						System.Type.FilterName, Name);
				}
			}

			if ((ModFlags & Modifiers.VOLATILE) != 0){
				if (!MemberType.IsClass){
					Type vt = MemberType;
					
					if (TypeManager.IsEnumType (vt))
						vt = TypeManager.EnumToUnderlying (MemberType);

					if (!((vt == TypeManager.bool_type) ||
					      (vt == TypeManager.sbyte_type) ||
					      (vt == TypeManager.byte_type) ||
					      (vt == TypeManager.short_type) ||
					      (vt == TypeManager.ushort_type) ||
					      (vt == TypeManager.int32_type) ||
					      (vt == TypeManager.uint32_type) ||    
					      (vt == TypeManager.char_type) ||
					      (vt == TypeManager.float_type) ||
					      (!vt.IsValueType))){
						Report.Error (
							677, Location, Parent.MakeName (Name) +
							" A volatile field can not be of type `" +
							TypeManager.CSharpName (vt) + "'");
						return false;
					}
				}

				if ((ModFlags & Modifiers.READONLY) != 0){
					Report.Error (
						      678, Location,
						      "A field can not be both volatile and readonly");
					return false;
				}
			}

			FieldAttributes fa = Modifiers.FieldAttr (ModFlags);

			if (Parent.Kind == Kind.Struct && 
			    ((fa & FieldAttributes.Static) == 0) &&
			    MemberType == Parent.TypeBuilder &&
			    !TypeManager.IsBuiltinType (MemberType)){
				Report.Error (523, Location, "Struct member `" + Parent.Name + "." + Name + 
					      "' causes a cycle in the structure layout");
				return false;
			}

			try {
				FieldBuilder = Parent.TypeBuilder.DefineField (
					Name, MemberType, Modifiers.FieldAttr (ModFlags));

			TypeManager.RegisterFieldBase (FieldBuilder, this);
			}
			catch (ArgumentException) {
				Report.Warning (-24, Location, "The Microsoft runtime is unable to use [void|void*] as a field type, try using the Mono runtime.");
				return false;
			}

			return true;
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			if ((ModFlags & Modifiers.VOLATILE) != 0) {
				Report.Warning (3026, 1, Location, "CLS-compliant field '{0}' cannot be volatile", GetSignatureForError ());
			}

			return true;
		}
	}

	//
	// `set' and `get' accessors are represented with an Accessor.
	// 
	public class Accessor {
		//
		// Null if the accessor is empty, or a Block if not
		//
		public const int AllowedModifiers = 
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;
		
		public ToplevelBlock Block;
		public Attributes Attributes;
		public Location Location;
		public int ModFlags;
		
		public Accessor (ToplevelBlock b, int mod, Attributes attrs, Location loc)
		{
			Block = b;
			Attributes = attrs;
			Location = loc;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, 0, loc);
		}
	}


	// Ooouh Martin, templates are missing here.
	// When it will be possible move here a lot of child code and template method type.
	public abstract class AbstractPropertyEventMethod: MemberCore, IMethodData {
		protected MethodData method_data;
		protected ToplevelBlock block;
		protected ListDictionary declarative_security;

		// The accessor are created event if they are not wanted.
		// But we need them because their names are reserved.
		// Field says whether accessor will be emited or not
		public readonly bool IsDummy;

		protected readonly string prefix;

		ReturnParameter return_attributes;

		public AbstractPropertyEventMethod (MemberBase member, string prefix)
			: base (null, SetupName (prefix, member), null, member.Location)
		{
			this.prefix = prefix;
			IsDummy = true;
		}

		public AbstractPropertyEventMethod (MemberBase member, Accessor accessor,
						    string prefix)
			: base (null, SetupName (prefix, member),
				accessor.Attributes, accessor.Location)
		{
			this.prefix = prefix;
			this.block = accessor.Block;
		}

		static MemberName SetupName (string prefix, MemberBase member)
		{
			return new MemberName (member.MemberName.Left, prefix + member.ShortName);
		}

		public void UpdateName (MemberBase member)
		{
			SetMemberName (SetupName (prefix, member));
		}

		#region IMethodData Members

		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		public CallingConventions CallingConventions {
			get {
				return CallingConventions.Standard;
			}
		}

		public bool IsExcluded (EmitContext ec)
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public abstract ObsoleteAttribute GetObsoleteAttribute ();
		public abstract Type[] ParameterTypes { get; }
		public abstract Type ReturnType { get; }
		public abstract EmitContext CreateEmitContext(TypeContainer tc, ILGenerator ig);

		#endregion

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.cls_compliant_attribute_type || a.Type == TypeManager.obsolete_attribute_type ||
					a.Type == TypeManager.conditional_attribute_type) {
				Report.Error (1667, a.Location, "'{0}' is not valid on property or event accessors. It is valid on {1} declarations only", TypeManager.CSharpName (a.Type), a.GetValidTargets ());
				return;
			}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				method_data.MethodBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (method_data.MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb);
				return;
			}

			ApplyToExtraTarget (a, cb);
		}

		virtual protected void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb)
		{
			System.Diagnostics.Debug.Fail ("You forgot to define special attribute target handling");
		}

		public override bool Define()
		{
			throw new NotSupportedException ();
		}

		public virtual void Emit (TypeContainer container)
		{
			EmitMethod (container);

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					method_data.MethodBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			block = null;
		}

		protected virtual void EmitMethod (TypeContainer container)
		{
			method_data.Emit (container, this);
		}

		public override bool IsClsComplianceRequired(DeclSpace ds)
		{
			return false;
		}

		public bool IsDuplicateImplementation (MethodCore method)
		{
			if (Name != method.Name)
				return false;

			Type[] param_types = method.ParameterTypes;

			if (param_types.Length != ParameterTypes.Length)
				return false;

			for (int i = 0; i < param_types.Length; i++)
				if (param_types [i] != ParameterTypes [i])
					return false;

			Report.SymbolRelatedToPreviousError (method);
			Report.Error (111, Location, "Type '{0}' already defines a member called '{1}' with " +
				      "the same parameter types", Parent.Name, Name);
			return true;
		}

		public override bool IsUsed
		{
			get {
				if (IsDummy)
					return false;

				return base.IsUsed;
			}
		}

		public new Location Location { 
			get {
				return base.Location;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { throw new InvalidOperationException ("Unexpected attempt to get doc comment from " + this.GetType () + "."); }
		}

		protected override void VerifyObsoleteAttribute()
		{
		}

	}

	//
	// Properties and Indexers both generate PropertyBuilders, we use this to share 
	// their common bits.
	//
	abstract public class PropertyBase : MethodCore {

		public class GetMethod: PropertyMethod
		{
			static string[] attribute_targets = new string [] { "method", "return" };

			public GetMethod (MethodCore method):
				base (method, "get_")
			{
			}

			public GetMethod (MethodCore method, Accessor accessor):
				base (method, accessor, "get_")
			{
			}

			public override MethodBuilder Define(TypeContainer container)
			{
				base.Define (container);
				
				method_data = new MethodData (method, method.ParameterInfo, ModFlags, flags, this);

				if (!method_data.Define (container))
					return null;

				return method_data.MethodBuilder;
			}

			public override string GetSignatureForError (TypeContainer tc)
			{
				return String.Concat (base.GetSignatureForError (tc), ".get");
			}

			public override Type ReturnType {
				get {
					return method.MemberType;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		public class SetMethod: PropertyMethod {

			static string[] attribute_targets = new string [] { "method", "param", "return" };
                       ImplicitParameter param_attr;

			public SetMethod (MethodCore method):
				base (method, "set_")
			{
			}

			public SetMethod (MethodCore method, Accessor accessor):
				base (method, accessor, "set_")
			{
			}

			protected override void ApplyToExtraTarget(Attribute a, CustomAttributeBuilder cb)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder, method.Location);

					param_attr.ApplyAttributeBuilder (a, cb);
					return;
				}

				base.ApplyAttributeBuilder (a, cb);
			}

			protected virtual InternalParameters GetParameterInfo (EmitContext ec)
			{
				Parameter [] parms = new Parameter [1];
				parms [0] = new Parameter (method.Type, "value", Parameter.Modifier.NONE, null, method.Location);
				Parameters parameters = new Parameters (parms, null);

				bool old_unsafe = ec.InUnsafe;
				ec.InUnsafe = InUnsafe;
				Type [] types = parameters.GetParameterInfo (ec);
				ec.InUnsafe = old_unsafe;

				return new InternalParameters (types, parameters);
			}

			public override MethodBuilder Define (TypeContainer container)
			{
				if (container.EmitContext == null)
					throw new InternalErrorException ("SetMethod.Define called too early");
					
				base.Define (container);
				
				method_data = new MethodData (method, GetParameterInfo (container.EmitContext), ModFlags, flags, this);

				if (!method_data.Define (container))
					return null;

				return method_data.MethodBuilder;
			}

			public override string GetSignatureForError (TypeContainer tc)
			{
				return String.Concat (base.GetSignatureForError (tc), ".set");
			}

			public override Type[] ParameterTypes {
				get {
					return new Type[] { method.MemberType };
				}
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		static string[] attribute_targets = new string [] { "property" };

		public abstract class PropertyMethod: AbstractPropertyEventMethod {
			protected readonly MethodCore method;
			protected MethodAttributes flags;

			public PropertyMethod (MethodCore method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
				Parent = method.Parent;
			}

			public PropertyMethod (MethodCore method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
				Parent = method.Parent;
				this.ModFlags = accessor.ModFlags;

				if (accessor.ModFlags != 0 && RootContext.Version == LanguageVersion.ISO_1) {
					Report.FeatureIsNotStandardized (Location, "accessor modifiers");
				}
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired(DeclSpace ds)
			{
				return method.IsClsComplianceRequired (ds);
			}

			public InternalParameters ParameterInfo 
			{
				get {
					return method_data.ParameterInfo;
				}
			}

			public virtual MethodBuilder Define (TypeContainer container)
			{
				//
				// Check for custom access modifier
				//
				if (ModFlags == 0) {
					ModFlags = method.ModFlags;
					flags = method.flags;
				} else {
					if ((method.ModFlags & Modifiers.ABSTRACT) != 0 && (ModFlags & Modifiers.PRIVATE) != 0) {
						Report.Error (442, Location, "{0}': abstract properties cannot have private accessors", GetSignatureForError (container));
					}

					CheckModifiers (container, ModFlags);
					ModFlags |= (method.ModFlags & (~Modifiers.Accessibility));
					ModFlags |= Modifiers.PROPERTY_CUSTOM;
					flags = Modifiers.MethodAttr (ModFlags);
					flags |= (method.flags & (~MethodAttributes.MemberAccessMask));
				}

				return null;

			}

			public bool HasCustomAccessModifier
			{
				get {
					return (ModFlags & Modifiers.PROPERTY_CUSTOM) != 0;
				}
			}

			public override Type[] ParameterTypes {
				get {
					return TypeManager.NoTypes;
				}
			}

			public override EmitContext CreateEmitContext (TypeContainer tc,
								       ILGenerator ig)
			{
				return new EmitContext (
					tc, method.ds, method.Location, ig, ReturnType,
					method.ModFlags, false);
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute (method.ds);
			}

			public override string GetSignatureForError (TypeContainer tc)
  			{
				return String.Concat (tc.Name, '.', method.Name);
  			}

			void CheckModifiers (TypeContainer container, int modflags)
			{
				int flags = 0;
				int mflags = method.ModFlags & Modifiers.Accessibility;

				if ((mflags & Modifiers.PUBLIC) != 0) {
					flags |= Modifiers.PROTECTED | Modifiers.INTERNAL | Modifiers.PRIVATE;
				}
				else if ((mflags & Modifiers.PROTECTED) != 0) {
					if ((mflags & Modifiers.INTERNAL) != 0)
						flags |= Modifiers.PROTECTED | Modifiers.INTERNAL;

					flags |= Modifiers.PRIVATE;
				}
				else if ((mflags & Modifiers.INTERNAL) != 0)
					flags |= Modifiers.PRIVATE;

				if ((mflags == modflags) || (modflags & (~flags)) != 0)
					Report.Error (273, Location, "{0}: accessibility modifier must be more restrictive than the property or indexer",
						GetSignatureForError (container));
			}

			public override bool MarkForDuplicationCheck ()
			{
				caching_flags |= Flags.TestMethodDuplication;
				return true;
			}
		}

		public PropertyMethod Get, Set;
		public PropertyBuilder PropertyBuilder;
		public MethodBuilder GetBuilder, SetBuilder;

		protected EmitContext ec;

		public PropertyBase (TypeContainer parent, Expression type, int mod_flags,
				     int allowed_mod, bool is_iface, MemberName name,
				     Parameters parameters, Attributes attrs,
				     Location loc)
			: base (parent, null, type, mod_flags, allowed_mod, is_iface, name,
				attrs, parameters, loc)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			PropertyBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Property;
			}
		}

		public override bool Define ()
		{
			if (!DoDefine ())
				return false;

			if (!IsTypePermitted ())
				return false;

			return true;
		}

		protected override bool DoDefine ()
		{
			if (!base.DoDefine ())
				return false;

			//
			// Accessors modifiers check
			//
			if (Get.ModFlags != 0 && Set.ModFlags != 0) {
				Report.Error (274, Location, "'{0}': cannot specify accessibility modifiers for both accessors of the property or indexer.",
						GetSignatureForError ());
				return false;
			}

			if ((Get.IsDummy || Set.IsDummy)
					&& (Get.ModFlags != 0 || Set.ModFlags != 0) && (ModFlags & Modifiers.OVERRIDE) == 0) {
				Report.Error (276, Location, 
					"'{0}': accessibility modifiers on accessors may only be used if the property or indexer has both a get and a set accessor.",
					GetSignatureForError ());
				return false;
			}

			if (MemberType.IsAbstract && MemberType.IsSealed) {
				Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
				return false;
			}

			ec = new EmitContext (Parent, Location, null, MemberType, ModFlags);
			return true;
		}

		public override string GetSignatureForError()
		{
			if (PropertyBuilder == null)
				return GetSignatureForError (Parent);

			return TypeManager.CSharpSignature (PropertyBuilder, false);
		}

		protected override bool CheckForDuplications ()
		{
			ArrayList ar = Parent.Indexers;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Indexer m = (Indexer) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}

			ar = Parent.Properties;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Property m = (Property) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}

			return true;
		}

		// TODO: rename to Resolve......
 		protected override MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type)
 		{
 			PropertyInfo base_property = container.BaseCache.FindMemberToOverride (
 				container.TypeBuilder, Name, ParameterTypes, true) as PropertyInfo;

 			if (base_property == null)
 				return null;

 			base_ret_type = base_property.PropertyType;
			MethodInfo get_accessor = base_property.GetGetMethod (true);
			MethodInfo set_accessor = base_property.GetSetMethod (true);
			MethodAttributes get_accessor_access, set_accessor_access;

			if ((ModFlags & Modifiers.OVERRIDE) != 0) {
				if (Get != null && !Get.IsDummy && get_accessor == null) {
					Report.SymbolRelatedToPreviousError (base_property);
					Report.Error (545, Location, "'{0}': cannot override because '{1}' does not have an overridable get accessor", GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
				}

				if (Set != null && !Set.IsDummy && set_accessor == null) {
					Report.SymbolRelatedToPreviousError (base_property);
					Report.Error (546, Location, "'{0}': cannot override because '{1}' does not have an overridable set accessor", GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
				}
			}
			
			//
			// Check base class accessors access
			//
			get_accessor_access = set_accessor_access = 0;
			if ((ModFlags & Modifiers.NEW) == 0) {
				if (get_accessor != null) {
					MethodAttributes get_flags = Modifiers.MethodAttr (Get.ModFlags != 0 ? Get.ModFlags : ModFlags);
					get_accessor_access = (get_accessor.Attributes & MethodAttributes.MemberAccessMask);

					if (!Get.IsDummy && !CheckAccessModifiers (get_flags & MethodAttributes.MemberAccessMask, get_accessor_access, get_accessor))
						Report.Error (507, Location, "'{0}' can't change the access modifiers when overriding inherited member '{1}'",
								GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
				}

				if (set_accessor != null)  {
					MethodAttributes set_flags = Modifiers.MethodAttr (Set.ModFlags != 0 ? Set.ModFlags : ModFlags);
					set_accessor_access = (set_accessor.Attributes & MethodAttributes.MemberAccessMask);

					if (!Set.IsDummy && !CheckAccessModifiers (set_flags & MethodAttributes.MemberAccessMask, set_accessor_access, set_accessor))
						Report.Error (507, Location, "'{0}' can't change the access modifiers when overriding inherited member '{1}'",
								GetSignatureForError (container), TypeManager.GetFullNameSignature (base_property));
				}
			}

			//
			// Get the less restrictive access
			//
			return get_accessor_access > set_accessor_access ? get_accessor : set_accessor;
		}

		public override void Emit ()
		{
			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null && OptAttributes != null)
				OptAttributes.Emit (ec, this);

			if (!Get.IsDummy)
				Get.Emit (Parent);

			if (!Set.IsDummy)
				Set.Emit (Parent);

			base.Emit ();
		}

		/// <summary>
		/// Tests whether accessors are not in collision with some method (CS0111)
		/// </summary>
		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Get.IsDuplicateImplementation (mc) || Set.IsDuplicateImplementation (mc);
		}

		public override bool IsUsed
		{
			get {
				if (IsExplicitImpl)
					return true;

				return Get.IsUsed | Set.IsUsed;
			}
		}

		protected override void SetMemberName (MemberName new_name)
		{
			base.SetMemberName (new_name);

			Get.UpdateName (this);
			Set.UpdateName (this);
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "P:"; }
		}
	}
			
	public class Property : PropertyBase, IIteratorContainer {
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
		        Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.METHOD_YIELDS |
			Modifiers.VIRTUAL;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public Property (TypeContainer parent, Expression type, int mod_flags,
				 bool is_iface, MemberName name, Attributes attrs,
				 Accessor get_block, Accessor set_block, Location loc)
			: base (parent, type, mod_flags,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, Parameters.EmptyReadOnlyParameters, attrs,
				loc)
		{
			if (get_block == null)
				Get = new GetMethod (this);
			else
				Get = new GetMethod (this, get_block);

			if (set_block == null)
				Set = new SetMethod (this);
			else
				Set = new SetMethod (this, set_block);
		}

		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			if (!base.Define ())
				return false;

			if (!CheckBase ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;

			if (!Get.IsDummy) {

				GetBuilder = Get.Define (Parent);
				if (GetBuilder == null)
					return false;

				//
				// Setup iterator if we are one
				//
				if ((ModFlags & Modifiers.METHOD_YIELDS) != 0){
					Iterator iterator = new Iterator (
						Parent, "get", MemberType,
						Get.ParameterInfo,
						ModFlags, Get.Block, Location);
					
					if (!iterator.DefineIterator ())
						return false;
					Get.Block = iterator.Block;
				}
			}

			if (!Set.IsDummy) {
				SetBuilder = Set.Define (Parent);
				if (SetBuilder == null)
					return false;

				SetBuilder.DefineParameter (1, ParameterAttributes.None, "value"); 
			}

			// FIXME - PropertyAttributes.HasDefault ?
			
			PropertyAttributes prop_attr = PropertyAttributes.None;
			if (!IsInterface)
				prop_attr |= PropertyAttributes.RTSpecialName |
			PropertyAttributes.SpecialName;

				PropertyBuilder = Parent.TypeBuilder.DefineProperty (
					Name, prop_attr, MemberType, null);
				
				if (!Get.IsDummy)
					PropertyBuilder.SetGetMethod (GetBuilder);
				
				if (!Set.IsDummy)
					PropertyBuilder.SetSetMethod (SetBuilder);

				TypeManager.RegisterProperty (PropertyBuilder, GetBuilder, SetBuilder);
			return true;
		}

		public void SetYields ()
		{
			ModFlags |= Modifiers.METHOD_YIELDS;
		}
	}

	/// </summary>
	///  Gigantic workaround  for lameness in SRE follows :
	///  This class derives from EventInfo and attempts to basically
	///  wrap around the EventBuilder so that FindMembers can quickly
	///  return this in it search for members
	/// </summary>
	public class MyEventBuilder : EventInfo {
		
		//
		// We use this to "point" to our Builder which is
		// not really a MemberInfo
		//
		EventBuilder MyBuilder;
		
		//
		// We "catch" and wrap these methods
		//
		MethodInfo raise, remove, add;

		EventAttributes attributes;
		Type declaring_type, reflected_type, event_type;
		string name;

		Event my_event;

		public MyEventBuilder (Event ev, TypeBuilder type_builder, string name, EventAttributes event_attr, Type event_type)
		{
			MyBuilder = type_builder.DefineEvent (name, event_attr, event_type);

			// And now store the values in our own fields.
			
			declaring_type = type_builder;

			reflected_type = type_builder;
			
			attributes = event_attr;
			this.name = name;
			my_event = ev;
			this.event_type = event_type;
		}
		
		//
		// Methods that you have to override.  Note that you only need 
		// to "implement" the variants that take the argument (those are
		// the "abstract" methods, the others (GetAddMethod()) are 
		// regular.
		//
		public override MethodInfo GetAddMethod (bool nonPublic)
		{
			return add;
		}
		
		public override MethodInfo GetRemoveMethod (bool nonPublic)
		{
			return remove;
		}
		
		public override MethodInfo GetRaiseMethod (bool nonPublic)
		{
			return raise;
		}
		
		//
		// These methods make "MyEventInfo" look like a Builder
		//
		public void SetRaiseMethod (MethodBuilder raiseMethod)
		{
			raise = raiseMethod;
			MyBuilder.SetRaiseMethod (raiseMethod);
		}

		public void SetRemoveOnMethod (MethodBuilder removeMethod)
		{
			remove = removeMethod;
			MyBuilder.SetRemoveOnMethod (removeMethod);
		}

		public void SetAddOnMethod (MethodBuilder addMethod)
		{
			add = addMethod;
			MyBuilder.SetAddOnMethod (addMethod);
		}

		public void SetCustomAttribute (CustomAttributeBuilder cb)
		{
			MyBuilder.SetCustomAttribute (cb);
		}
		
		public override object [] GetCustomAttributes (bool inherit)
		{
			// FIXME : There's nothing which can be seemingly done here because
			// we have no way of getting at the custom attribute objects of the
			// EventBuilder !
			return null;
		}

		public override object [] GetCustomAttributes (Type t, bool inherit)
		{
			// FIXME : Same here !
			return null;
		}

		public override bool IsDefined (Type t, bool b)
		{
			return true;
		}

		public override EventAttributes Attributes {
			get {
				return attributes;
			}
		}

		public override string Name {
			get {
				return name;
			}
		}

		public override Type DeclaringType {
			get {
				return declaring_type;
			}
		}

		public override Type ReflectedType {
			get {
				return reflected_type;
			}
		}

		public Type EventType {
			get {
				return event_type;
			}
		}
		
		public void SetUsed ()
		{
			if (my_event != null) {
				my_event.status = FieldBase.Status.ASSIGNED;
				my_event.SetMemberIsUsed ();
			}
		}
	}
	
	/// <summary>
	/// For case when event is declared like property (with add and remove accessors).
	/// </summary>
	public class EventProperty: Event {

		static string[] attribute_targets = new string [] { "event", "property" };

		public EventProperty (TypeContainer parent, Expression type, int mod_flags,
				      bool is_iface, MemberName name, Object init,
				      Attributes attrs, Accessor add, Accessor remove,
				      Location loc)
			: base (parent, type, mod_flags, is_iface, name, init, attrs, loc)
		{
			Add = new AddDelegateMethod (this, add);
			Remove = new RemoveDelegateMethod (this, remove);

			// For this event syntax we don't report error CS0067
			// because it is hard to do it.
			SetAssigned ();
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	/// <summary>
	/// Event is declared like field.
	/// </summary>
	public class EventField: Event {

		static string[] attribute_targets = new string [] { "event", "field", "method" };
		static string[] attribute_targets_interface = new string[] { "event", "method" };

		public EventField (TypeContainer parent, Expression type, int mod_flags,
				   bool is_iface, MemberName name, Object init,
				   Attributes attrs, Location loc)
			: base (parent, type, mod_flags, is_iface, name, init, attrs, loc)
		{
			Add = new AddDelegateMethod (this);
			Remove = new RemoveDelegateMethod (this);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Target == AttributeTargets.Field) {
				FieldBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				Add.ApplyAttributeBuilder (a, cb);
				Remove.ApplyAttributeBuilder (a, cb);
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		public override string[] ValidAttributeTargets {
			get {
				return IsInterface ? attribute_targets_interface : attribute_targets;
			}
		}
	}

	public abstract class Event : FieldBase {

		protected sealed class AddDelegateMethod: DelegateMethod
		{

			public AddDelegateMethod (Event method):
				base (method, "add_")
			{
			}

			public AddDelegateMethod (Event method, Accessor accessor):
				base (method, accessor, "add_")
			{
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_combine_delegate_delegate;
				}
			}

		}

		protected sealed class RemoveDelegateMethod: DelegateMethod
		{
			public RemoveDelegateMethod (Event method):
				base (method, "remove_")
			{
			}

			public RemoveDelegateMethod (Event method, Accessor accessor):
				base (method, accessor, "remove_")
			{
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_remove_delegate_delegate;
				}
			}

		}

		public abstract class DelegateMethod: AbstractPropertyEventMethod
		{
			protected readonly Event method;
                       ImplicitParameter param_attr;

			static string[] attribute_targets = new string [] { "method", "param", "return" };

			public DelegateMethod (Event method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
			}

			public DelegateMethod (Event method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
			}

			protected override void ApplyToExtraTarget(Attribute a, CustomAttributeBuilder cb)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder, method.Location);

					param_attr.ApplyAttributeBuilder (a, cb);
					return;
				}

				base.ApplyAttributeBuilder (a, cb);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired(DeclSpace ds)
			{
				return method.IsClsComplianceRequired (ds);
			}

			public MethodBuilder Define (TypeContainer container, InternalParameters ip)
			{
				method_data = new MethodData (method, ip, method.ModFlags,
					method.flags | MethodAttributes.HideBySig | MethodAttributes.SpecialName, this);

				if (!method_data.Define (container))
					return null;

				MethodBuilder mb = method_data.MethodBuilder;
				mb.DefineParameter (1, ParameterAttributes.None, "value");
				return mb;
			}


			protected override void EmitMethod (TypeContainer tc)
			{
				if (block != null) {
					base.EmitMethod (tc);
					return;
				}

				if ((method.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
					return;

				ILGenerator ig = method_data.MethodBuilder.GetILGenerator ();
				FieldInfo field_info = (FieldInfo)method.FieldBuilder;

				method_data.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Synchronized);
				if ((method.ModFlags & Modifiers.STATIC) != 0) {
					ig.Emit (OpCodes.Ldsfld, field_info);
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Call, DelegateMethodInfo);
					ig.Emit (OpCodes.Castclass, method.MemberType);
					ig.Emit (OpCodes.Stsfld, field_info);
				} else {
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldfld, field_info);
					ig.Emit (OpCodes.Ldarg_1);
					ig.Emit (OpCodes.Call, DelegateMethodInfo);
					ig.Emit (OpCodes.Castclass, method.MemberType);
					ig.Emit (OpCodes.Stfld, field_info);
				}
				ig.Emit (OpCodes.Ret);
			}

			protected abstract MethodInfo DelegateMethodInfo { get; }

			public override Type[] ParameterTypes {
				get {
					return new Type[] { method.MemberType };
				}
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override EmitContext CreateEmitContext (TypeContainer tc,
								       ILGenerator ig)
			{
				return new EmitContext (
					tc, method.Parent, Location, ig, ReturnType,
					method.ModFlags, false);
			}

			public override string GetSignatureForError (TypeContainer tc)
			{
				return String.Concat (tc.Name, '.', method.Name);
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute (method.Parent);
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}


		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.ABSTRACT;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public DelegateMethod Add, Remove;
		public MyEventBuilder     EventBuilder;
		public MethodBuilder AddBuilder, RemoveBuilder;

		public Event (TypeContainer parent, Expression type, int mod_flags,
			      bool is_iface, MemberName name, Object init, Attributes attrs,
			      Location loc)
			: base (parent, type, mod_flags,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, init, attrs, loc)
		{
			IsInterface = is_iface;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}
			
			EventBuilder.SetCustomAttribute (cb);
		}

		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Add.IsDuplicateImplementation (mc) || Remove.IsDuplicateImplementation (mc);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Event;
			}
		}
  
		public override bool Define ()
		{
			EventAttributes e_attr;
			e_attr = EventAttributes.None;

			if (!DoDefineBase ())
				return false;

			if (!DoDefine ())
				return false;

			if (init != null && ((ModFlags & Modifiers.ABSTRACT) != 0)){
				Report.Error (74, Location, "'" + Parent.Name + "." + Name +
					      "': abstract event can not have an initializer");
				return false;
			}

			if (!TypeManager.IsDelegateType (MemberType)) {
				Report.Error (66, Location, "'" + Parent.Name + "." + Name +
					      "' : event must be of a delegate type");
				return false;
			}

			EmitContext ec = Parent.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("Event.Define called too early?");
			bool old_unsafe = ec.InUnsafe;
			ec.InUnsafe = InUnsafe;

			Parameter [] parms = new Parameter [1];
			parms [0] = new Parameter (Type, "value", Parameter.Modifier.NONE, null, Location);
			Parameters parameters = new Parameters (parms, null);
			Type [] types = parameters.GetParameterInfo (ec);
			InternalParameters ip = new InternalParameters (types, parameters);

			ec.InUnsafe = old_unsafe;

			if (!CheckBase ())
				return false;

			//
			// Now define the accessors
			//

			AddBuilder = Add.Define (Parent, ip);
			if (AddBuilder == null)
				return false;

			RemoveBuilder = Remove.Define (Parent, ip);
			if (RemoveBuilder == null)
				return false;

			EventBuilder = new MyEventBuilder (this, Parent.TypeBuilder, Name, e_attr, MemberType);
					
			if (Add.Block == null && Remove.Block == null && !IsInterface) {
					FieldBuilder = Parent.TypeBuilder.DefineField (
						Name, MemberType,
						FieldAttributes.Private | ((ModFlags & Modifiers.STATIC) != 0 ? FieldAttributes.Static : 0));
					TypeManager.RegisterPrivateFieldOfEvent (
						(EventInfo) EventBuilder, FieldBuilder);
					TypeManager.RegisterFieldBase (FieldBuilder, this);
				}
			
				EventBuilder.SetAddOnMethod (AddBuilder);
				EventBuilder.SetRemoveOnMethod (RemoveBuilder);

				TypeManager.RegisterEvent (EventBuilder, AddBuilder, RemoveBuilder);
			return true;
		}

 		protected override bool CheckBase ()
 		{
 			if (!base.CheckBase ())
 				return false;
 
 			if (conflict_symbol != null && (ModFlags & Modifiers.NEW) == 0) {
 				if (!(conflict_symbol is EventInfo)) {
 					Report.SymbolRelatedToPreviousError (conflict_symbol);
 					Report.Error (72, Location, "Event '{0}' can override only event", GetSignatureForError (Parent));
 					return false;
 				}
 			}
 
 			return true;
 		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				EmitContext ec = new EmitContext (
					Parent, Location, null, MemberType, ModFlags);
				OptAttributes.Emit (ec, this);
			}

			Add.Emit (Parent);
			Remove.Emit (Parent);

			base.Emit ();
		}

		public override string GetSignatureForError ()
		{
			if (EventBuilder == null)
				return base.GetSignatureForError (Parent);

			return TypeManager.GetFullNameSignature (EventBuilder);
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "E:"; }
		}
	}

 
	public class Indexer : PropertyBase, IIteratorContainer {

		class GetIndexerMethod: GetMethod
		{
			public GetIndexerMethod (MethodCore method):
				base (method)
			{
			}

			public GetIndexerMethod (MethodCore method, Accessor accessor):
				base (method, accessor)
			{
			}

			// TODO: one GetSignatureForError is enough (reuse Parent member)
			public override string GetSignatureForError (TypeContainer tc)
			{
				string core = base.GetSignatureForError (tc);
				return core.Replace (TypeContainer.DefaultIndexerName, 
					String.Format ("this[{0}]", TypeManager.CSharpName (ParameterTypes)));
			}

			public override Type[] ParameterTypes {
				get {
					return method.ParameterTypes;
				}
			}
		}

		class SetIndexerMethod: SetMethod
		{
			readonly Parameters parameters;

			public SetIndexerMethod (MethodCore method):
				base (method)
			{
			}

			public SetIndexerMethod (MethodCore method, Parameters parameters, Accessor accessor):
				base (method, accessor)
			{
				this.parameters = parameters;
			}

			public override Type[] ParameterTypes {
				get {
					int top = method.ParameterTypes.Length;
					Type [] set_pars = new Type [top + 1];
					method.ParameterTypes.CopyTo (set_pars, 0);
					set_pars [top] = method.MemberType;
					return set_pars;
				}
			}

			protected override InternalParameters GetParameterInfo (EmitContext ec)
			{
				Parameter [] fixed_parms = parameters.FixedParameters;

				if (fixed_parms == null){
					throw new Exception ("We currently do not support only array arguments in an indexer at: " + method.Location);
					// BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG
					// BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG
					//
					// Here is the problem: the `value' parameter has
					// to come *after* the array parameter in the declaration
					// like this:
					// X (object [] x, Type value)
					// .param [0]
					//
					// BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG
					// BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG
					
				}
				
				Parameter [] tmp = new Parameter [fixed_parms.Length + 1];

				fixed_parms.CopyTo (tmp, 0);
				tmp [fixed_parms.Length] = new Parameter (
					method.Type, "value", Parameter.Modifier.NONE, null, method.Location);

				Parameters set_formal_params = new Parameters (tmp, null);
				Type [] types = set_formal_params.GetParameterInfo (ec);
				
				return new InternalParameters (types, set_formal_params);
			}
		}

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.ABSTRACT;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		//
		// Are we implementing an interface ?
		//
		public Indexer (TypeContainer parent, Expression type, MemberName name, int mod,
				bool is_iface, Parameters parameters, Attributes attrs,
				Accessor get_block, Accessor set_block, Location loc)
			: base (parent, type, mod,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, parameters, attrs, loc)
		{
			if (get_block == null)
				Get = new GetIndexerMethod (this);
			else
				Get = new GetIndexerMethod (this, get_block);

			if (set_block == null)
				Set = new SetIndexerMethod (this);
			else
				Set = new SetIndexerMethod (this, parameters, set_block);
		}

		public override bool Define ()
		{
			PropertyAttributes prop_attr =
				PropertyAttributes.RTSpecialName |
				PropertyAttributes.SpecialName;
			
			if (!DoDefineBase ())
				return false;

			if (!base.Define ())
				return false;

			if (MemberType == TypeManager.void_type) {
				Report.Error (620, Location, "Indexers cannot have void type");
				return false;
			}

			if (OptAttributes != null) {
				Attribute indexer_attr = OptAttributes.Search (TypeManager.indexer_name_type, ec);
				if (indexer_attr != null) {
					// Remove the attribute from the list because it is not emitted
					OptAttributes.Attrs.Remove (indexer_attr);

					ShortName = indexer_attr.GetIndexerAttributeValue (ec);

					if (IsExplicitImpl) {
						Report.Error (415, indexer_attr.Location,
							      "The 'IndexerName' attribute is valid only on an " +
							      "indexer that is not an explicit interface member declaration");
						return false;
					}
				
					if ((ModFlags & Modifiers.OVERRIDE) != 0) {
						Report.Error (609, indexer_attr.Location,
							      "Cannot set the 'IndexerName' attribute on an indexer marked override");
						return false;
					}

					if (!Tokenizer.IsValidIdentifier (ShortName)) {
						Report.Error (633, indexer_attr.Location,
							      "The argument to the 'IndexerName' attribute must be a valid identifier");
						return false;
					}
				}
			}

			if (InterfaceType != null) {
				string base_IndexerName = TypeManager.IndexerPropertyName (InterfaceType);
				if (base_IndexerName != Name)
					ShortName = base_IndexerName;
			}

			if (!Parent.AddToMemberContainer (this) ||
				!Parent.AddToMemberContainer (Get) || !Parent.AddToMemberContainer (Set))
				return false;

			if (!CheckBase ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			if (!Get.IsDummy){
				GetBuilder = Get.Define (Parent);
				if (GetBuilder == null)
					return false;

				//
				// Setup iterator if we are one
				//
				if ((ModFlags & Modifiers.METHOD_YIELDS) != 0){
					Iterator iterator = new Iterator (
						Parent, "get", MemberType,
						Get.ParameterInfo,
						ModFlags, Get.Block, Location);
					
					if (!iterator.DefineIterator ())
						return false;
					Get.Block = iterator.Block;
				}
			}
			
			if (!Set.IsDummy){
				SetBuilder = Set.Define (Parent);
				if (SetBuilder == null)
					return false;
			}

			//
			// Now name the parameters
			//
			Parameter [] p = Parameters.FixedParameters;
			if (p != null) {
				if ((p [0].ModFlags & Parameter.Modifier.ISBYREF) != 0) {
					Report.Error (631, Location, "ref and out are not valid in this context");
					return false;
				}

				int i;
				
				for (i = 0; i < p.Length; ++i) {
					if (!Get.IsDummy)
						GetBuilder.DefineParameter (
							i + 1, p [i].Attributes, p [i].Name);

					if (!Set.IsDummy)
						SetBuilder.DefineParameter (
							i + 1, p [i].Attributes, p [i].Name);
				}

				if (!Set.IsDummy)
					SetBuilder.DefineParameter (
						i + 1, ParameterAttributes.None, "value");
					
				if (i != ParameterTypes.Length) {
					Parameter array_param = Parameters.ArrayParameter;

					SetBuilder.DefineParameter (
						i + 1, array_param.Attributes, array_param.Name);
				}
			}

				PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				Name, prop_attr, MemberType, ParameterTypes);

				if (!Get.IsDummy)
					PropertyBuilder.SetGetMethod (GetBuilder);

				if (!Set.IsDummy)
					PropertyBuilder.SetSetMethod (SetBuilder);
				
			TypeManager.RegisterIndexer (PropertyBuilder, GetBuilder, SetBuilder, ParameterTypes);

			return true;
		}

		public override string GetSignatureForError ()
		{
			if (PropertyBuilder == null)
				return GetSignatureForError (Parent);

			return TypeManager.CSharpSignature (PropertyBuilder, true);
		}

		public override string GetSignatureForError(TypeContainer tc)
		{
			return String.Concat (tc.Name, ".this[", Parameters.FixedParameters [0].TypeName.ToString (), ']');
		}

		public override bool MarkForDuplicationCheck ()
		{
			caching_flags |= Flags.TestMethodDuplication;
			return true;
		}

		public void SetYields ()
		{
			ModFlags |= Modifiers.METHOD_YIELDS;
		}
	}

	public class Operator : MethodCore, IIteratorContainer {

		const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.STATIC;

		public enum OpType : byte {

			// Unary operators
			LogicalNot,
			OnesComplement,
			Increment,
			Decrement,
			True,
			False,

			// Unary and Binary operators
			Addition,
			Subtraction,

			UnaryPlus,
			UnaryNegation,
			
			// Binary operators
			Multiply,
			Division,
			Modulus,
			BitwiseAnd,
			BitwiseOr,
			ExclusiveOr,
			LeftShift,
			RightShift,
			Equality,
			Inequality,
			GreaterThan,
			LessThan,
			GreaterThanOrEqual,
			LessThanOrEqual,

			// Implicit and Explicit
			Implicit,
			Explicit
		};

		public readonly OpType OperatorType;
		public MethodBuilder   OperatorMethodBuilder;
		
		public Method OperatorMethod;

		static string[] attribute_targets = new string [] { "method", "return" };

		public Operator (TypeContainer parent, OpType type, Expression ret_type,
				 int mod_flags, Parameters parameters,
				 ToplevelBlock block, Attributes attrs, Location loc)
			: base (parent, null, ret_type, mod_flags, AllowedModifiers, false,
				new MemberName ("op_" + type), attrs, parameters, loc)
		{
			OperatorType = type;
			Block = block;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb) 
		{
			OperatorMethod.ApplyAttributeBuilder (a, cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method; 
			}
		}
		
		protected override bool CheckGenericOverride (MethodInfo method,  string name)
		{
			return true;
		}

		protected override bool CheckForDuplications()
		{
			ArrayList ar = Parent.Operators;
			if (ar != null) {
				int arLen = ar.Count;

				for (int i = 0; i < arLen; i++) {
					Operator o = (Operator) ar [i];
					if (IsDuplicateImplementation (o))
						return false;
				}
			}

			ar = Parent.Methods;
			if (ar != null) {
				int arLen = ar.Count;

				for (int i = 0; i < arLen; i++) {
					Method m = (Method) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}

			return true;
		}

		public override bool Define ()
		{
			const int RequiredModifiers = Modifiers.PUBLIC | Modifiers.STATIC;
			if ((ModFlags & RequiredModifiers) != RequiredModifiers){
				Report.Error (558, Location, "User defined operators '{0}' must be declared static and public", GetSignatureForError (Parent));
				return false;
			}

			if (!DoDefine ())
				return false;

			if (MemberType == TypeManager.void_type) {
				Report.Error (590, Location, "User-defined operators cannot return void");
				return false;
			}

			OperatorMethod = new Method (
				Parent, null, Type, ModFlags, false, MemberName,
				Parameters, OptAttributes, Location);

			OperatorMethod.Block = Block;
			OperatorMethod.IsOperator = true;			
			OperatorMethod.flags |= MethodAttributes.SpecialName | MethodAttributes.HideBySig;
			OperatorMethod.Define ();

			if (OperatorMethod.MethodBuilder == null)
				return false;
			
			OperatorMethodBuilder = OperatorMethod.MethodBuilder;

			parameter_types = OperatorMethod.ParameterTypes;
			Type declaring_type = OperatorMethod.MethodData.DeclaringType;
			Type return_type = OperatorMethod.ReturnType;
			Type first_arg_type = parameter_types [0];

			if (!CheckBase ())
				return false;

			// Rules for conversion operators
			
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				if (first_arg_type == return_type && first_arg_type == declaring_type){
					Report.Error (
						555, Location,
						"User-defined conversion cannot take an object of the " +
						"enclosing type and convert to an object of the enclosing" +
						" type");
					return false;
				}
				
				if (first_arg_type != declaring_type && return_type != declaring_type){
					Report.Error (
						556, Location, 
						"User-defined conversion must convert to or from the " +
						"enclosing type");
					return false;
				}
				
				if (first_arg_type == TypeManager.object_type ||
				    return_type == TypeManager.object_type){
					Report.Error (
						-8, Location,
						"User-defined conversion cannot convert to or from " +
						"object type");
					return false;
				}

				if (first_arg_type.IsInterface || return_type.IsInterface){
					Report.Error (
						552, Location,
						"User-defined conversion cannot convert to or from an " +
						"interface type");
					return false;
				}
				
				if (first_arg_type.IsSubclassOf (return_type)
					|| return_type.IsSubclassOf (first_arg_type)){
					if (declaring_type.IsSubclassOf (return_type)) {
						Report.Error (553, Location, "'{0}' : user defined conversion to/from base class", GetSignatureForError ());
						return false;
					}
					Report.Error (554, Location, "'{0}' : user defined conversion to/from derived class", GetSignatureForError ());
					return false;
				}
			} else if (OperatorType == OpType.LeftShift || OperatorType == OpType.RightShift) {
				if (first_arg_type != declaring_type || parameter_types [1] != TypeManager.int32_type) {
					Report.Error (564, Location, "Overloaded shift operator must have the type of the first operand be the containing type, and the type of the second operand must be int");
					return false;
				}
			} else if (Parameters.FixedParameters.Length == 1) {
				// Checks for Unary operators
				
				if (OperatorType == OpType.Increment || OperatorType == OpType.Decrement) {
					if (return_type != declaring_type && !return_type.IsSubclassOf (declaring_type)) {
						Report.Error (448, Location,
							"The return type for ++ or -- operator must be the containing type or derived from the containing type");
						return false;
					}
				if (first_arg_type != declaring_type){
					Report.Error (
							559, Location, "The parameter type for ++ or -- operator must be the containing type");
					return false;
				}
				}
				
				if (first_arg_type != declaring_type){
						Report.Error (
						562, Location,
						"The parameter of a unary operator must be the " +
						"containing type");
						return false;
				}
				
				if (OperatorType == OpType.True || OperatorType == OpType.False) {
					if (return_type != TypeManager.bool_type){
						Report.Error (
							215, Location,
							"The return type of operator True or False " +
							"must be bool");
						return false;
					}
				}
				
			} else {
				if (OperatorType == OpType.BitwiseAnd && 
					(first_arg_type != return_type || first_arg_type != parameter_types [1])) {
					Report.Error (217, Location, "In order to be applicable as a short circuit operator a user-defined logical operator ('{0}') " +
						"must have the same return type as the type of its 2 parameters", GetSignatureForError ());
					return false;
				}

				// Checks for Binary operators
				
				if (first_arg_type != declaring_type &&
				    parameter_types [1] != declaring_type){
					Report.Error (
						563, Location,
						"One of the parameters of a binary operator must " +
						"be the containing type");
					return false;
				}
			}

			return true;
		}
		
		public override void Emit ()
		{
			//
			// abstract or extern methods have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;
			
			OperatorMethod.Emit ();
			Block = null;
		}

		// Operator cannot be override
		protected override MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type)
		{
			return null;
		}

		public static string GetName (OpType ot)
		{
			switch (ot){
			case OpType.LogicalNot:
				return "!";
			case OpType.OnesComplement:
				return "~";
			case OpType.Increment:
				return "++";
			case OpType.Decrement:
				return "--";
			case OpType.True:
				return "true";
			case OpType.False:
				return "false";
			case OpType.Addition:
				return "+";
			case OpType.Subtraction:
				return "-";
			case OpType.UnaryPlus:
				return "+";
			case OpType.UnaryNegation:
				return "-";
			case OpType.Multiply:
				return "*";
			case OpType.Division:
				return "/";
			case OpType.Modulus:
				return "%";
			case OpType.BitwiseAnd:
				return "&";
			case OpType.BitwiseOr:
				return "|";
			case OpType.ExclusiveOr:
				return "^";
			case OpType.LeftShift:
				return "<<";
			case OpType.RightShift:
				return ">>";
			case OpType.Equality:
				return "==";
			case OpType.Inequality:
				return "!=";
			case OpType.GreaterThan:
				return ">";
			case OpType.LessThan:
				return "<";
			case OpType.GreaterThanOrEqual:
				return ">=";
			case OpType.LessThanOrEqual:
				return "<=";
			case OpType.Implicit:
				return "implicit";
			case OpType.Explicit:
				return "explicit";
			default: return "";
			}
		}

		public override string GetSignatureForError (TypeContainer tc)
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("{0}.operator {1} {2}({3}", tc.Name, GetName (OperatorType), Type.Type == null ? Type.ToString () : TypeManager.CSharpName (Type.Type),
				Parameters.FixedParameters [0].GetSignatureForError ());
			
			if (Parameters.FixedParameters.Length > 1) {
				sb.Append (",");
				sb.Append (Parameters.FixedParameters [1].GetSignatureForError ());
			}
			sb.Append (")");
			return sb.ToString ();
		}

		public override string GetSignatureForError ()
		{
			return ToString ();
		}
		
		public override bool MarkForDuplicationCheck ()
		{
			caching_flags |= Flags.TestMethodDuplication;
			return true;
		}

		public override string ToString ()
		{
			if (OperatorMethod == null)
				return Name;

			Type return_type = OperatorMethod.ReturnType;
			Type [] param_types = OperatorMethod.ParameterTypes;
			
			if (Parameters.FixedParameters.Length == 1)
				return String.Format (
					"{0} operator {1}({2})",
					TypeManager.CSharpName (return_type),
					GetName (OperatorType),
					param_types [0]);
			else
				return String.Format (
					"{0} operator {1}({2}, {3})",
					TypeManager.CSharpName (return_type),
					GetName (OperatorType),
					param_types [0], param_types [1]);
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		public void SetYields ()
		{
			ModFlags |= Modifiers.METHOD_YIELDS;
		}
	}

	//
	// This is used to compare method signatures
	//
	struct MethodSignature {
		public string Name;
		public Type RetType;
		public Type [] Parameters;
		
		/// <summary>
		///    This delegate is used to extract methods which have the
		///    same signature as the argument
		/// </summary>
		public static MemberFilter method_signature_filter = new MemberFilter (MemberSignatureCompare);
		
		public MethodSignature (string name, Type ret_type, Type [] parameters)
		{
			Name = name;
			RetType = ret_type;

			if (parameters == null)
				Parameters = TypeManager.NoTypes;
			else
				Parameters = parameters;
		}

		public override string ToString ()
		{
			string pars = "";
			if (Parameters.Length != 0){
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				for (int i = 0; i < Parameters.Length; i++){
					sb.Append (Parameters [i]);
					if (i+1 < Parameters.Length)
						sb.Append (", ");
				}
				pars = sb.ToString ();
			}

			return String.Format ("{0} {1} ({2})", RetType, Name, pars);
		}
		
		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override bool Equals (Object o)
		{
			MethodSignature other = (MethodSignature) o;

			if (other.Name != Name)
				return false;

			if (other.RetType != RetType)
				return false;
			
			if (Parameters == null){
				if (other.Parameters == null)
					return true;
				return false;
			}

			if (other.Parameters == null)
				return false;
			
			int c = Parameters.Length;
			if (other.Parameters.Length != c)
				return false;

			for (int i = 0; i < c; i++)
				if (other.Parameters [i] != Parameters [i])
					return false;

			return true;
		}

		static bool MemberSignatureCompare (MemberInfo m, object filter_criteria)
		{
			MethodSignature sig = (MethodSignature) filter_criteria;

			if (m.Name != sig.Name)
				return false;

			Type ReturnType;
			MethodInfo mi = m as MethodInfo;
			PropertyInfo pi = m as PropertyInfo;

			if (mi != null)
				ReturnType = mi.ReturnType;
			else if (pi != null)
				ReturnType = pi.PropertyType;
			else
				return false;
			
			//
			// we use sig.RetType == null to mean `do not check the
			// method return value.  
			//
			if (sig.RetType != null)
				if (ReturnType != sig.RetType)
					return false;

			Type [] args;
			if (mi != null)
				args = TypeManager.GetArgumentTypes (mi);
			else
				args = TypeManager.GetArgumentTypes (pi);
			Type [] sigp = sig.Parameters;

			if (args.Length != sigp.Length)
				return false;

			for (int i = args.Length; i > 0; ){
				i--;
				if (args [i] != sigp [i])
					return false;
			}
			return true;
		}
	}
}

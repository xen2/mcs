//
// typemanager.cs: C# type manager
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//
using System;
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Mono.CSharp {

/// <summary>
///   This is a readonly list of MemberInfo's.      
/// </summary>
public class MemberList : IList {
	public readonly IList List;
	int count;

	/// <summary>
	///   Create a new MemberList from the given IList.
	/// </summary>
	public MemberList (IList list)
	{
		if (list != null)
			this.List = list;
		else
			this.List = new ArrayList ();
		count = List.Count;
	}

	/// <summary>
	///   Concatenate the ILists `first' and `second' to a new MemberList.
	/// </summary>
	public MemberList (IList first, IList second)
	{
		ArrayList list = new ArrayList ();
		list.AddRange (first);
		list.AddRange (second);
		count = list.Count;
		List = list;
	}

	public static readonly MemberList Empty = new MemberList (new ArrayList ());

	public static explicit operator MemberInfo [] (MemberList list)
	{
		Timer.StartTimer (TimerType.MiscTimer);
		MemberInfo [] result = new MemberInfo [list.Count];
		list.CopyTo (result, 0);
		Timer.StopTimer (TimerType.MiscTimer);
		return result;
	}

	// ICollection

	public int Count {
		get {
			return count;
		}
	}

	public bool IsSynchronized {
		get {
			return List.IsSynchronized;
		}
	}

	public object SyncRoot {
		get {
			return List.SyncRoot;
		}
	}

	public void CopyTo (Array array, int index)
	{
		List.CopyTo (array, index);
	}

	// IEnumerable

	public IEnumerator GetEnumerator ()
	{
		return List.GetEnumerator ();
	}

	// IList

	public bool IsFixedSize {
		get {
			return true;
		}
	}

	public bool IsReadOnly {
		get {
			return true;
		}
	}

	object IList.this [int index] {
		get {
			return List [index];
		}

		set {
			throw new NotSupportedException ();
		}
	}

	// FIXME: try to find out whether we can avoid the cast in this indexer.
	public MemberInfo this [int index] {
		get {
			return (MemberInfo) List [index];
		}
	}

	public int Add (object value)
	{
		throw new NotSupportedException ();
	}

	public void Clear ()
	{
		throw new NotSupportedException ();
	}

	public bool Contains (object value)
	{
		return List.Contains (value);
	}

	public int IndexOf (object value)
	{
		return List.IndexOf (value);
	}

	public void Insert (int index, object value)
	{
		throw new NotSupportedException ();
	}

	public void Remove (object value)
	{
		throw new NotSupportedException ();
	}

	public void RemoveAt (int index)
	{
		throw new NotSupportedException ();
	}
}

public interface IMemberFinder {
	MemberList FindMembers (MemberTypes mt, BindingFlags bf,
				MemberFilter filter, object criteria);

	MemberCache MemberCache {
		get;
	}
}

public interface IMemberContainer : IMemberFinder {
	string Name {
		get;
	}

	Type Type {
		get;
	}

	IMemberContainer Parent {
		get;
	}

	bool IsInterface {
		get;
	}

	MemberList GetMembers (MemberTypes mt, BindingFlags bf);
}

public class TypeManager {
	//
	// A list of core types that the compiler requires or uses
	//
	static public Type object_type;
	static public Type value_type;
	static public Type string_type;
	static public Type int32_type;
	static public Type uint32_type;
	static public Type int64_type;
	static public Type uint64_type;
	static public Type float_type;
	static public Type double_type;
	static public Type char_type;
	static public Type char_ptr_type;
	static public Type short_type;
	static public Type decimal_type;
	static public Type bool_type;
	static public Type sbyte_type;
	static public Type byte_type;
	static public Type ushort_type;
	static public Type enum_type;
	static public Type delegate_type;
	static public Type multicast_delegate_type;
	static public Type void_type;
	static public Type enumeration_type;
	static public Type array_type;
	static public Type runtime_handle_type;
	static public Type icloneable_type;
	static public Type type_type;
	static public Type ienumerator_type;
	static public Type idisposable_type;
	static public Type default_member_type;
	static public Type iasyncresult_type;
	static public Type asynccallback_type;
	static public Type intptr_type;
	static public Type monitor_type;
	static public Type runtime_field_handle_type;
	static public Type attribute_type;
	static public Type attribute_usage_type;
	static public Type dllimport_type;
	static public Type unverifiable_code_type;
	static public Type methodimpl_attr_type;
	static public Type marshal_as_attr_type;
	static public Type param_array_type;
	static public Type void_ptr_type;
	static public Type indexer_name_type;
	static public Type exception_type;
	static public object obsolete_attribute_type;
	static public object conditional_attribute_type;

	//
	// An empty array of types
	//
	static public Type [] NoTypes;


	// 
	// Expressions representing the internal types.  Used during declaration
	// definition.
	//
	static public Expression system_object_expr, system_string_expr; 
	static public Expression system_boolean_expr, system_decimal_expr;
	static public Expression system_single_expr, system_double_expr;
	static public Expression system_sbyte_expr, system_byte_expr;
	static public Expression system_int16_expr, system_uint16_expr;
	static public Expression system_int32_expr, system_uint32_expr;
	static public Expression system_int64_expr, system_uint64_expr;
	static public Expression system_char_expr, system_void_expr;
	static public Expression system_asynccallback_expr;
	static public Expression system_iasyncresult_expr;

	//
	// This is only used when compiling corlib
	//
	static public Type system_int32_type;
	static public Type system_array_type;
	static public Type system_type_type;
	static public Type system_assemblybuilder_type;
	static public MethodInfo system_int_array_get_length;
	static public MethodInfo system_int_array_get_rank;
	static public MethodInfo system_object_array_clone;
	static public MethodInfo system_int_array_get_length_int;
	static public MethodInfo system_int_array_get_lower_bound_int;
	static public MethodInfo system_int_array_get_upper_bound_int;
	static public MethodInfo system_void_array_copyto_array_int;
	static public MethodInfo system_void_set_corlib_type_builders;

	
	//
	// Internal, not really used outside
	//
	static Type runtime_helpers_type;
	
	//
	// These methods are called by code generated by the compiler
	//
	static public MethodInfo string_concat_string_string;
	static public MethodInfo string_concat_object_object;
	static public MethodInfo string_isinterneted_string;
	static public MethodInfo system_type_get_type_from_handle;
	static public MethodInfo object_getcurrent_void;
	static public MethodInfo bool_movenext_void;
	static public MethodInfo void_dispose_void;
	static public MethodInfo void_monitor_enter_object;
	static public MethodInfo void_monitor_exit_object;
	static public MethodInfo void_initializearray_array_fieldhandle;
	static public MethodInfo int_getlength_int;
	static public MethodInfo delegate_combine_delegate_delegate;
	static public MethodInfo delegate_remove_delegate_delegate;
	static public MethodInfo int_get_offset_to_string_data;
	static public MethodInfo int_array_get_length;
	static public MethodInfo int_array_get_rank;
	static public MethodInfo object_array_clone;
	static public MethodInfo int_array_get_length_int;
	static public MethodInfo int_array_get_lower_bound_int;
	static public MethodInfo int_array_get_upper_bound_int;
	static public MethodInfo void_array_copyto_array_int;
	
	//
	// The attribute constructors.
	//
	static public ConstructorInfo cons_param_array_attribute;
	static public ConstructorInfo void_decimal_ctor_five_args;
	static public ConstructorInfo unverifiable_code_ctor;
	
	// <remarks>
	//   Holds the Array of Assemblies that have been loaded
	//   (either because it is the default or the user used the
	//   -r command line option)
	// </remarks>
	static Assembly [] assemblies;

	// <remarks>
	//  Keeps a list of module builders. We used this to do lookups
	//  on the modulebuilder using GetType -- needed for arrays
	// </remarks>
	static ModuleBuilder [] modules;

	// <remarks>
	//   This is the type_cache from the assemblies to avoid
	//   hitting System.Reflection on every lookup.
	// </summary>
	static Hashtable types;

	// <remarks>
	//  This is used to hotld the corresponding TypeContainer objects
	//  since we need this in FindMembers
	// </remarks>
	static Hashtable typecontainers;

	// <remarks>
	//   Keeps track of those types that are defined by the
	//   user's program
	// </remarks>
	static ArrayList user_types;

	static PtrHashtable builder_to_member_finder;

	// <remarks>
	//   Tracks the interfaces implemented by typebuilders.  We only
	//   enter those who do implement or or more interfaces
	// </remarks>
	static PtrHashtable builder_to_ifaces;

	// <remarks>
	//   Maps MethodBase.RuntimeTypeHandle to a Type array that contains
	//   the arguments to the method
	// </remarks>
	static Hashtable method_arguments;

	// <remarks>
	//   Maps PropertyBuilder to a Type array that contains
	//   the arguments to the indexer
	// </remarks>
	static Hashtable indexer_arguments;

	// <remarks>
	//   Maybe `method_arguments' should be replaced and only
	//   method_internal_params should be kept?
	// <remarks>
	static Hashtable method_internal_params;

	// <remarks>
	//  Keeps track of attribute types
	// </remarks>

	static Hashtable builder_to_attr;

	// <remarks>
	//  Keeps track of methods
	// </remarks>

	static Hashtable builder_to_method;

	struct Signature {
		public string name;
		public Type [] args;
	}

	/// <summary>
	///   A filter for Findmembers that uses the Signature object to
	///   extract objects
	/// </summary>
	static bool SignatureFilter (MemberInfo mi, object criteria)
	{
		Signature sig = (Signature) criteria;

		if (!(mi is MethodBase))
			return false;
		
		if (mi.Name != sig.name)
			return false;

		int count = sig.args.Length;
		
		if (mi is MethodBuilder || mi is ConstructorBuilder){
			Type [] candidate_args = GetArgumentTypes ((MethodBase) mi);

			if (candidate_args.Length != count)
				return false;
			
			for (int i = 0; i < count; i++)
				if (candidate_args [i] != sig.args [i])
					return false;
			
			return true;
		} else {
			ParameterInfo [] pars = ((MethodBase) mi).GetParameters ();

			if (pars.Length != count)
				return false;

			for (int i = 0; i < count; i++)
				if (pars [i].ParameterType != sig.args [i])
					return false;
			return true;
		}
	}

	// A delegate that points to the filter above.
	static MemberFilter signature_filter;

	//
	// These are expressions that represent some of the internal data types, used
	// elsewhere
	//
	static void InitExpressionTypes ()
	{
		system_object_expr  = new TypeLookupExpression ("System.Object");
		system_string_expr  = new TypeLookupExpression ("System.String");
		system_boolean_expr = new TypeLookupExpression ("System.Boolean");
		system_decimal_expr = new TypeLookupExpression ("System.Decimal");
		system_single_expr  = new TypeLookupExpression ("System.Single");
		system_double_expr  = new TypeLookupExpression ("System.Double");
		system_sbyte_expr   = new TypeLookupExpression ("System.SByte");
		system_byte_expr    = new TypeLookupExpression ("System.Byte");
		system_int16_expr   = new TypeLookupExpression ("System.Int16");
		system_uint16_expr  = new TypeLookupExpression ("System.UInt16");
		system_int32_expr   = new TypeLookupExpression ("System.Int32");
		system_uint32_expr  = new TypeLookupExpression ("System.UInt32");
		system_int64_expr   = new TypeLookupExpression ("System.Int64");
		system_uint64_expr  = new TypeLookupExpression ("System.UInt64");
		system_char_expr    = new TypeLookupExpression ("System.Char");
		system_void_expr    = new TypeLookupExpression ("System.Void");
		system_asynccallback_expr = new TypeLookupExpression ("System.AsyncCallback");
		system_iasyncresult_expr = new TypeLookupExpression ("System.IAsyncResult");
	}
	
	static TypeManager ()
	{
		assemblies = new Assembly [0];
		modules = null;
		user_types = new ArrayList ();
		
		types = new Hashtable ();
		typecontainers = new Hashtable ();
		
		builder_to_member_finder = new PtrHashtable ();
		builder_to_attr = new PtrHashtable ();
		builder_to_method = new PtrHashtable ();
		method_arguments = new PtrHashtable ();
		method_internal_params = new PtrHashtable ();
		indexer_arguments = new PtrHashtable ();
		builder_to_ifaces = new PtrHashtable ();
		
		NoTypes = new Type [0];

		signature_filter = new MemberFilter (SignatureFilter);
		InitExpressionTypes ();
	}

	public static void AddUserType (string name, TypeBuilder t, Type [] ifaces)
	{
		try {
			types.Add (name, t);
		} catch {
			Type prev = (Type) types [name];
			TypeContainer tc = builder_to_member_finder [prev] as TypeContainer;

			if (tc != null){
				//
				// This probably never happens, as we catch this before
				//
				Report.Error (-17, "The type `" + name + "' has already been defined.");
				return;
			}

			tc = builder_to_member_finder [t] as TypeContainer;
			
			Report.Warning (
				1595, "The type `" + name + "' is defined in an existing assembly;"+
				" Using the new definition from: " + tc.Location);
			Report.Warning (1595, "Previously defined in: " + prev.Assembly.FullName);
			
			types.Remove (name);
			types.Add (name, t);
		}
		user_types.Add (t);
			
		if (ifaces != null)
			builder_to_ifaces [t] = ifaces;
	}

	//
	// This entry point is used by types that we define under the covers
	// 
	public static void RegisterBuilder (TypeBuilder tb, Type [] ifaces)
	{
		if (ifaces != null)
			builder_to_ifaces [tb] = ifaces;
	}
	
	public static void AddUserType (string name, TypeBuilder t, TypeContainer tc, Type [] ifaces)
	{
		builder_to_member_finder.Add (t, tc);
		typecontainers.Add (name, tc);
		AddUserType (name, t, ifaces);
	}

	public static void AddDelegateType (string name, TypeBuilder t, Delegate del)
	{
		types.Add (name, t);
		builder_to_member_finder.Add (t, del);
	}
	
	public static void AddEnumType (string name, TypeBuilder t, Enum en)
	{
		types.Add (name, t);
		builder_to_member_finder.Add (t, en);
	}

	public static void AddUserInterface (string name, TypeBuilder t, Interface i, Type [] ifaces)
	{
		AddUserType (name, t, ifaces);
		builder_to_member_finder.Add (t, i);
	}

	public static void AddMethod (MethodBuilder builder, MethodData method)
	{
		builder_to_method.Add (builder, method);
	}

	public static void RegisterAttrType (Type t, TypeContainer tc)
	{
		builder_to_attr.Add (t, tc);
	}

	/// <summary>
	///   Returns the TypeContainer whose Type is `t' or null if there is no
	///   TypeContainer for `t' (ie, the Type comes from a library)
	/// </summary>
	public static TypeContainer LookupTypeContainer (Type t)
	{
		return builder_to_member_finder [t] as TypeContainer;
	}

	public static IMemberContainer LookupMemberContainer (Type t)
	{
		IMemberContainer container = builder_to_member_finder [t] as IMemberContainer;
		if (container != null)
			return container;

		return TypeHandle.GetTypeHandle (t);
	}

	public static Interface LookupInterface (Type t)
	{
		return builder_to_member_finder [t] as Interface;
	}

	public static Delegate LookupDelegate (Type t)
	{
		return builder_to_member_finder [t] as Delegate;
	}

	public static Enum LookupEnum (Type t)
	{
		return builder_to_member_finder [t] as Enum;
	}
	
	public static TypeContainer LookupAttr (Type t)
	{
		return (TypeContainer) builder_to_attr [t];
	}
	
	/// <summary>
	///   Registers an assembly to load types from.
	/// </summary>
	public static void AddAssembly (Assembly a)
	{
		int top = assemblies.Length;
		Assembly [] n = new Assembly [top + 1];

		assemblies.CopyTo (n, 0);
		
		n [top] = a;
		assemblies = n;
	}

	/// <summary>
	///  Registers a module builder to lookup types from
	/// </summary>
	public static void AddModule (ModuleBuilder mb)
	{
		int top = modules != null ? modules.Length : 0;
		ModuleBuilder [] n = new ModuleBuilder [top + 1];

		if (modules != null)
			modules.CopyTo (n, 0);
		n [top] = mb;
		modules = n;
	}

	/// <summary>
	///   Returns the Type associated with @name
	/// </summary>
	public static Type LookupType (string name)
	{
		Type t;

		//
		// First lookup in user defined and cached values
		//

		t = (Type) types [name];
		if (t != null)
			return t;

		foreach (Assembly a in assemblies){
			t = a.GetType (name);
			if (t != null){
				types [name] = t;

				return t;
			}
		}

		foreach (ModuleBuilder mb in modules) {
			t = mb.GetType (name);
			if (t != null) {
				types [name] = t;
				return t;
			}
		}
		
		return null;
	}

	/// <summary>
	///   Returns the C# name of a type if possible, or the full type name otherwise
	/// </summary>
	static public string CSharpName (Type t)
	{
		return Regex.Replace (t.FullName, 
			@"^System\." +
			@"(Int32|UInt32|Int16|Uint16|Int64|UInt64|" +
			@"Single|Double|Char|Decimal|Byte|SByte|Object|" +
			@"Boolean|String|Void)" +
			@"(\W+|\b)", 
			new MatchEvaluator (CSharpNameMatch));
	}	
	
	static String CSharpNameMatch (Match match) 
	{
		string s = match.Groups [1].Captures [0].Value;
		return s.ToLower ().
		Replace ("int32", "int").
		Replace ("uint32", "uint").
		Replace ("int16", "short").
		Replace ("uint16", "ushort").
		Replace ("int64", "long").
		Replace ("uint64", "ulong").
		Replace ("single", "float").
		Replace ("boolean", "bool")
		+ match.Groups [2].Captures [0].Value;
	}

        /// <summary>
        ///   Returns the signature of the method
        /// </summary>
        static public string CSharpSignature (MethodBase mb)
        {
                string sig = "(";

		//
		// FIXME: We should really have a single function to do
		// everything instead of the following 5 line pattern
		//
                ParameterData iparams = LookupParametersByBuilder (mb);

		if (iparams == null){
			ParameterInfo [] pi = mb.GetParameters ();
			iparams = new ReflectionParameters (pi);
		}
		
                for (int i = 0; i < iparams.Count; i++) {
                        if (i > 0) {
                                sig += ", ";
                        }
                        sig += iparams.ParameterDesc(i);
                }
                sig += ")";

                return mb.DeclaringType.Name + "." + mb.Name + sig;
        }

	/// <summary>
	///   Looks up a type, and aborts if it is not found.  This is used
	///   by types required by the compiler
	/// </summary>
	static Type CoreLookupType (string name)
	{
		Type t = LookupType (name);

		if (t == null){
			Report.Error (518, "The predefined type `" + name + "' is not defined or imported");
			Environment.Exit (0);
		}

		return t;
	}

	/// <summary>
	///   Returns the MethodInfo for a method named `name' defined
	///   in type `t' which takes arguments of types `args'
	/// </summary>
	static MethodInfo GetMethod (Type t, string name, Type [] args)
	{
		MemberList list;
		Signature sig;

		sig.name = name;
		sig.args = args;
		
		list = FindMembers (t, MemberTypes.Method, instance_and_static | BindingFlags.Public,
				    signature_filter, sig);
		if (list.Count == 0) {
			Report.Error (-19, "Can not find the core function `" + name + "'");
			return null;
		}

		MethodInfo mi = list [0] as MethodInfo;
		if (mi == null) {
			Report.Error (-19, "Can not find the core function `" + name + "'");
			return null;
		}

		return mi;
	}

	/// <summary>
	///    Returns the ConstructorInfo for "args"
	/// </summary>
	static ConstructorInfo GetConstructor (Type t, Type [] args)
	{
		MemberList list;
		Signature sig;

		sig.name = ".ctor";
		sig.args = args;
		
		list = FindMembers (t, MemberTypes.Constructor,
				    instance_and_static | BindingFlags.Public | BindingFlags.DeclaredOnly,
				    signature_filter, sig);
		if (list.Count == 0){
			Report.Error (-19, "Can not find the core constructor for type `" + t.Name + "'");
			return null;
		}

		ConstructorInfo ci = list [0] as ConstructorInfo;
		if (ci == null){
			Report.Error (-19, "Can not find the core constructor for type `" + t.Name + "'");
			return null;
		}

		return ci;
	}

	public static void InitEnumUnderlyingTypes ()
	{

		int32_type    = CoreLookupType ("System.Int32");
		int64_type    = CoreLookupType ("System.Int64");
		uint32_type   = CoreLookupType ("System.UInt32"); 
		uint64_type   = CoreLookupType ("System.UInt64"); 
		byte_type     = CoreLookupType ("System.Byte");
		sbyte_type    = CoreLookupType ("System.SByte");
		short_type    = CoreLookupType ("System.Int16");
		ushort_type   = CoreLookupType ("System.UInt16");
	}
	
	/// <remarks>
	///   The types have to be initialized after the initial
	///   population of the type has happened (for example, to
	///   bootstrap the corlib.dll
	/// </remarks>
	public static void InitCoreTypes ()
	{
		object_type   = CoreLookupType ("System.Object");
		value_type    = CoreLookupType ("System.ValueType");

		InitEnumUnderlyingTypes ();

		char_type     = CoreLookupType ("System.Char");
		string_type   = CoreLookupType ("System.String");
		float_type    = CoreLookupType ("System.Single");
		double_type   = CoreLookupType ("System.Double");
		char_ptr_type = CoreLookupType ("System.Char*");
		decimal_type  = CoreLookupType ("System.Decimal");
		bool_type     = CoreLookupType ("System.Boolean");
		enum_type     = CoreLookupType ("System.Enum");

		multicast_delegate_type = CoreLookupType ("System.MulticastDelegate");
		delegate_type           = CoreLookupType ("System.Delegate");

		array_type    = CoreLookupType ("System.Array");
		void_type     = CoreLookupType ("System.Void");
		type_type     = CoreLookupType ("System.Type");

		runtime_field_handle_type = CoreLookupType ("System.RuntimeFieldHandle");
		runtime_helpers_type = CoreLookupType ("System.Runtime.CompilerServices.RuntimeHelpers");
		default_member_type  = CoreLookupType ("System.Reflection.DefaultMemberAttribute");
		runtime_handle_type  = CoreLookupType ("System.RuntimeTypeHandle");
		asynccallback_type   = CoreLookupType ("System.AsyncCallback");
		iasyncresult_type    = CoreLookupType ("System.IAsyncResult");
		ienumerator_type     = CoreLookupType ("System.Collections.IEnumerator");
		idisposable_type     = CoreLookupType ("System.IDisposable");
		icloneable_type      = CoreLookupType ("System.ICloneable");
		monitor_type         = CoreLookupType ("System.Threading.Monitor");
		intptr_type          = CoreLookupType ("System.IntPtr");

		attribute_type       = CoreLookupType ("System.Attribute");
		attribute_usage_type = CoreLookupType ("System.AttributeUsageAttribute");
		dllimport_type       = CoreLookupType ("System.Runtime.InteropServices.DllImportAttribute");
		methodimpl_attr_type = CoreLookupType ("System.Runtime.CompilerServices.MethodImplAttribute");
		marshal_as_attr_type  = CoreLookupType ("System.Runtime.InteropServices.MarshalAsAttribute");
		param_array_type      = CoreLookupType ("System.ParamArrayAttribute");

		unverifiable_code_type= CoreLookupType ("System.Security.UnverifiableCodeAttribute");

		void_ptr_type         = CoreLookupType ("System.Void*");

		indexer_name_type     = CoreLookupType ("System.Runtime.CompilerServices.IndexerNameAttribute");

		exception_type        = CoreLookupType ("System.Exception");

		//
		// Attribute types
		//
		obsolete_attribute_type = CoreLookupType ("System.ObsoleteAttribute");
		conditional_attribute_type = CoreLookupType ("System.Diagnostics.ConditionalAttribute");

		//
		// When compiling corlib, store the "real" types here.
		//
		if (!RootContext.StdLib) {
			system_int32_type = typeof (System.Int32);
			system_array_type = typeof (System.Array);
			system_type_type = typeof (System.Type);
			system_assemblybuilder_type = typeof (System.Reflection.Emit.AssemblyBuilder);

			Type [] void_arg = {  };
			system_int_array_get_length = GetMethod (
				system_array_type, "get_Length", void_arg);
			system_int_array_get_rank = GetMethod (
				system_array_type, "get_Rank", void_arg);
			system_object_array_clone = GetMethod (
				system_array_type, "Clone", void_arg);

			Type [] system_int_arg = { system_int32_type };
			system_int_array_get_length_int = GetMethod (
				system_array_type, "GetLength", system_int_arg);
			system_int_array_get_upper_bound_int = GetMethod (
				system_array_type, "GetUpperBound", system_int_arg);
			system_int_array_get_lower_bound_int = GetMethod (
				system_array_type, "GetLowerBound", system_int_arg);

			Type [] system_array_int_arg = { system_array_type, system_int32_type };
			system_void_array_copyto_array_int = GetMethod (
				system_array_type, "CopyTo", system_array_int_arg);

			Type [] system_type_type_arg = { system_type_type, system_type_type, system_type_type };

			try {
			system_void_set_corlib_type_builders = GetMethod (
				system_assemblybuilder_type, "SetCorlibTypeBuilders",
				system_type_type_arg);

			object[] args = new object [3];
			args [0] = object_type;
			args [1] = value_type;
			args [2] = enum_type;

			system_void_set_corlib_type_builders.Invoke (CodeGen.AssemblyBuilder, args);
			} catch {
				Console.WriteLine ("Corlib compilation is not supported in Microsoft.NET due to bugs in it");
			}
		}
	}

	//
	// The helper methods that are used by the compiler
	//
	public static void InitCodeHelpers ()
	{
		//
		// Now load the default methods that we use.
		//
		Type [] string_string = { string_type, string_type };
		string_concat_string_string = GetMethod (
			string_type, "Concat", string_string);

		Type [] object_object = { object_type, object_type };
		string_concat_object_object = GetMethod (
			string_type, "Concat", object_object);

		Type [] string_ = { string_type };
		string_isinterneted_string = GetMethod (
			string_type, "IsInterned", string_);
		
		Type [] runtime_type_handle = { runtime_handle_type };
		system_type_get_type_from_handle = GetMethod (
			type_type, "GetTypeFromHandle", runtime_type_handle);

		Type [] delegate_delegate = { delegate_type, delegate_type };
		delegate_combine_delegate_delegate = GetMethod (
				delegate_type, "Combine", delegate_delegate);

		delegate_remove_delegate_delegate = GetMethod (
				delegate_type, "Remove", delegate_delegate);

		//
		// Void arguments
		//
		Type [] void_arg = {  };
		object_getcurrent_void = GetMethod (
			ienumerator_type, "get_Current", void_arg);
		bool_movenext_void = GetMethod (
			ienumerator_type, "MoveNext", void_arg);
		void_dispose_void = GetMethod (
			idisposable_type, "Dispose", void_arg);
		int_get_offset_to_string_data = GetMethod (
			runtime_helpers_type, "get_OffsetToStringData", void_arg);
		int_array_get_length = GetMethod (
			array_type, "get_Length", void_arg);
		int_array_get_rank = GetMethod (
			array_type, "get_Rank", void_arg);

		//
		// Int32 arguments
		//
		Type [] int_arg = { int32_type };
		int_array_get_length_int = GetMethod (
			array_type, "GetLength", int_arg);
		int_array_get_upper_bound_int = GetMethod (
			array_type, "GetUpperBound", int_arg);
		int_array_get_lower_bound_int = GetMethod (
			array_type, "GetLowerBound", int_arg);

		//
		// System.Array methods
		//
		object_array_clone = GetMethod (
			array_type, "Clone", void_arg);
		Type [] array_int_arg = { array_type, int32_type };
		void_array_copyto_array_int = GetMethod (
			array_type, "CopyTo", array_int_arg);
		
		//
		// object arguments
		//
		Type [] object_arg = { object_type };
		void_monitor_enter_object = GetMethod (
			monitor_type, "Enter", object_arg);
		void_monitor_exit_object = GetMethod (
			monitor_type, "Exit", object_arg);

		Type [] array_field_handle_arg = { array_type, runtime_field_handle_type };
		
		void_initializearray_array_fieldhandle = GetMethod (
			runtime_helpers_type, "InitializeArray", array_field_handle_arg);

		//
		// Array functions
		//
		int_getlength_int = GetMethod (
			array_type, "GetLength", int_arg);

		//
		// Decimal constructors
		//
		Type [] dec_arg = { int32_type, int32_type, int32_type, bool_type, byte_type };
		void_decimal_ctor_five_args = GetConstructor (
			decimal_type, dec_arg);
		
		//
		// Attributes
		//
		cons_param_array_attribute = GetConstructor (
			param_array_type, void_arg);

		unverifiable_code_ctor = GetConstructor (
			unverifiable_code_type, void_arg);
		
	}

	const BindingFlags instance_and_static = BindingFlags.Static | BindingFlags.Instance;

	static Hashtable type_hash = new Hashtable ();

	public static MemberList FindMembers (Type t, MemberTypes mt, BindingFlags bf,
					      MemberFilter filter, object criteria)
	{
		IMemberFinder finder = (IMemberFinder) builder_to_member_finder [t];

		if (finder != null) {
			MemberList list;
			Timer.StartTimer (TimerType.FindMembers);
			list = finder.FindMembers (mt, bf, filter, criteria);
			Timer.StopTimer (TimerType.FindMembers);
			return list;
		}

		//
		// We have to take care of arrays specially, because GetType on
		// a TypeBuilder array will return a Type, not a TypeBuilder,
		// and we can not call FindMembers on this type.
		//

		if (t.IsSubclassOf (TypeManager.array_type))
			return new MemberList (TypeManager.array_type.FindMembers (mt, bf, filter, criteria));

		//
		// Since FindMembers will not lookup both static and instance
		// members, we emulate this behaviour here.
		//
		if ((bf & instance_and_static) == instance_and_static){
			MemberInfo [] i_members = t.FindMembers (
				mt, bf & ~BindingFlags.Static, filter, criteria);

			int i_len = i_members.Length;
			if (i_len == 1){
				MemberInfo one = i_members [0];

				//
				// If any of these are present, we are done!
				//
				if ((one is Type) || (one is EventInfo) || (one is FieldInfo))
					return new MemberList (i_members);
			}
				
			MemberInfo [] s_members = t.FindMembers (
				mt, bf & ~BindingFlags.Instance, filter, criteria);

			int s_len = s_members.Length;
			if (i_len > 0 || s_len > 0)
				return new MemberList (i_members, s_members);
			else {
				if (i_len > 0)
					return new MemberList (i_members);
				else
					return new MemberList (s_members);
			}
		}

		return new MemberList (t.FindMembers (mt, bf, filter, criteria));
	}


	/// FIXME FIXME FIXME
	///   This method is a big hack until the new MemberCache is finished, it will be gone in
	///   a few days.
	/// FIXME FIXME FIXME

	private static MemberList MemberLookup_FindMembers (Type t, MemberTypes mt, BindingFlags bf,
							    string name, ref bool searching)
	{
		//
		// We have to take care of arrays specially, because GetType on
		// a TypeBuilder array will return a Type, not a TypeBuilder,
		// and we can not call FindMembers on this type.
		//

		if (t.IsSubclassOf (TypeManager.array_type)) {
			searching = false;
			return TypeHandle.ArrayType.MemberCache.FindMembers (
				mt, bf, name, FilterWithClosure_delegate, null);
		}

		IMemberFinder finder = (IMemberFinder) builder_to_member_finder [t];

		if (finder != null) {
			MemberCache cache = finder.MemberCache;

			if (cache != null) {
				searching = false;
				return cache.FindMembers (
					mt, bf, name, FilterWithClosure_delegate, null);
			}

			MemberList list;
			Timer.StartTimer (TimerType.FindMembers);
			list = finder.FindMembers (mt, bf | BindingFlags.DeclaredOnly,
						   FilterWithClosure_delegate, name);
			Timer.StopTimer (TimerType.FindMembers);
			return list;
		}

		finder = TypeHandle.GetTypeHandle (t);
		builder_to_member_finder.Add (t, finder);

		searching = false;
		return finder.MemberCache.FindMembers (mt, bf, name, FilterWithClosure_delegate, null);
	}

	public static bool IsBuiltinType (Type t)
	{
		if (t == object_type || t == string_type || t == int32_type || t == uint32_type ||
		    t == int64_type || t == uint64_type || t == float_type || t == double_type ||
		    t == char_type || t == short_type || t == decimal_type || t == bool_type ||
		    t == sbyte_type || t == byte_type || t == ushort_type || t == void_type)
			return true;
		else
			return false;
	}

	public static bool IsDelegateType (Type t)
	{
		if (t.IsSubclassOf (TypeManager.delegate_type))
			return true;
		else
			return false;
	}
	
	public static bool IsEnumType (Type t)
	{
		if (t.IsSubclassOf (TypeManager.enum_type))
			return true;
		else
			return false;
	}
	
	public static bool IsValueType (Type t)
	{
		if (t.IsSubclassOf (TypeManager.value_type))
			return true;
		else
			return false;
	}
	
	public static bool IsInterfaceType (Type t)
	{
		Interface iface = builder_to_member_finder [t] as Interface;

		if (iface != null)
			return true;
		else
			return false;
	}

	//
	// Checks whether `type' is a subclass or nested child of `parent'.
	//
	public static bool IsSubclassOrNestedChildOf (Type type, Type parent)
	{
		do {
			if ((type == parent) || type.IsSubclassOf (parent))
				return true;

			// Handle nested types.
			type = type.DeclaringType;
		} while (type != null);

		return false;
	}

	/// <summary>
	///   Returns the User Defined Types
	/// </summary>
	public static ArrayList UserTypes {
		get {
			return user_types;
		}
	}

	public static Hashtable TypeContainers {
		get {
			return typecontainers;
		}
	}

	static Hashtable builder_to_constant;

	public static void RegisterConstant (FieldBuilder fb, Const c)
	{
		if (builder_to_constant == null)
			builder_to_constant = new PtrHashtable ();

		if (builder_to_constant.Contains (fb))
			return;

		builder_to_constant.Add (fb, c);
	}

	public static Const LookupConstant (FieldBuilder fb)
	{
		if (builder_to_constant == null)
			return null;
		
		return (Const) builder_to_constant [fb];
	}
	
	/// <summary>
	///   Gigantic work around for missing features in System.Reflection.Emit follows.
	/// </summary>
	///
	/// <remarks>
	///   Since System.Reflection.Emit can not return MethodBase.GetParameters
	///   for anything which is dynamic, and we need this in a number of places,
	///   we register this information here, and use it afterwards.
	/// </remarks>
	static public bool RegisterMethod (MethodBase mb, InternalParameters ip, Type [] args)
	{
		if (args == null)
			args = NoTypes;
				
		method_arguments.Add (mb, args);
		method_internal_params.Add (mb, ip);
		
		return true;
	}
	
	static public InternalParameters LookupParametersByBuilder (MethodBase mb)
	{
		if (! (mb is ConstructorBuilder || mb is MethodBuilder))
			return null;
		
		if (method_internal_params.Contains (mb))
			return (InternalParameters) method_internal_params [mb];
		else
			throw new Exception ("Argument for Method not registered" + mb);
	}

	/// <summary>
	///    Returns the argument types for a method based on its methodbase
	///
	///    For dynamic methods, we use the compiler provided types, for
	///    methods from existing assemblies we load them from GetParameters,
	///    and insert them into the cache
	/// </summary>
	static public Type [] GetArgumentTypes (MethodBase mb)
	{
		if (method_arguments.Contains (mb))
			return (Type []) method_arguments [mb];
		else {
			ParameterInfo [] pi = mb.GetParameters ();
			int c = pi.Length;
			Type [] types = new Type [c];
			
			for (int i = 0; i < c; i++)
				types [i] = pi [i].ParameterType;

			method_arguments.Add (mb, types);
			return types;
		}
	}

	/// <summary>
	///    Returns the argument types for an indexer based on its PropertyInfo
	///
	///    For dynamic indexers, we use the compiler provided types, for
	///    indexers from existing assemblies we load them from GetParameters,
	///    and insert them into the cache
	/// </summary>
	static public Type [] GetArgumentTypes (PropertyInfo indexer)
	{
		if (indexer_arguments.Contains (indexer))
			return (Type []) indexer_arguments [indexer];
		else if (indexer is PropertyBuilder)
			// If we're a PropertyBuilder and not in the
			// `indexer_arguments' hash, then we're a property and
			// not an indexer.
			return NoTypes;
		else {
			ParameterInfo [] pi = indexer.GetIndexParameters ();
			// Property, not an indexer.
			if (pi == null)
				return NoTypes;
			int c = pi.Length;
			Type [] types = new Type [c];
			
			for (int i = 0; i < c; i++)
				types [i] = pi [i].ParameterType;

			indexer_arguments.Add (indexer, types);
			return types;
		}
	}
	
	// <remarks>
	//  This is a workaround the fact that GetValue is not
	//  supported for dynamic types
	// </remarks>
	static Hashtable fields = new Hashtable ();
	static public bool RegisterFieldValue (FieldBuilder fb, object value)
	{
		if (fields.Contains (fb))
			return false;

		fields.Add (fb, value);

		return true;
	}

	static public object GetValue (FieldBuilder fb)
	{
		return fields [fb];
	}

	static Hashtable fieldbuilders_to_fields = new Hashtable ();
	static public bool RegisterFieldBase (FieldBuilder fb, FieldBase f)
	{
		if (fieldbuilders_to_fields.Contains (fb))
			return false;

		fieldbuilders_to_fields.Add (fb, f);
		return true;
	}

	static public FieldBase GetField (FieldInfo fb)
	{
		return (FieldBase) fieldbuilders_to_fields [fb];
	}
	
	static Hashtable events;

	static public bool RegisterEvent (MyEventBuilder eb, MethodBase add, MethodBase remove)
	{
		if (events == null)
			events = new Hashtable ();

		if (events.Contains (eb))
			return false;

		events.Add (eb, new Pair (add, remove));

		return true;
	}

	static public MethodInfo GetAddMethod (EventInfo ei)
	{
		if (ei is MyEventBuilder) {
			Pair pair = (Pair) events [ei];

			return (MethodInfo) pair.First;
		} else
			return ei.GetAddMethod ();
	}

	static public MethodInfo GetRemoveMethod (EventInfo ei)
	{
		if (ei is MyEventBuilder) {
			Pair pair = (Pair) events [ei];

			return (MethodInfo) pair.Second;
		} else
			return ei.GetAddMethod ();
	}

	static Hashtable priv_fields_events;

	static public bool RegisterPrivateFieldOfEvent (EventInfo einfo, FieldBuilder builder)
	{
		if (priv_fields_events == null)
			priv_fields_events = new Hashtable ();

		if (priv_fields_events.Contains (einfo))
			return false;

		priv_fields_events.Add (einfo, builder);

		return true;
	}

	static public MemberInfo GetPrivateFieldOfEvent (EventInfo ei)
	{
		return (MemberInfo) priv_fields_events [ei];
	}
		
	static Hashtable properties;
	
	static public bool RegisterProperty (PropertyBuilder pb, MethodBase get, MethodBase set)
	{
		if (properties == null)
			properties = new Hashtable ();

		if (properties.Contains (pb))
			return false;

		properties.Add (pb, new Pair (get, set));

		return true;
	}

	static public bool RegisterIndexer (PropertyBuilder pb, MethodBase get, MethodBase set, Type[] args)
	{
		if (!RegisterProperty (pb, get,set))
			return false;

		indexer_arguments.Add (pb, args);

		return true;
	}

	//
	// FIXME: we need to return the accessors depending on whether
	// they are visible or not.
	//
	static public MethodInfo [] GetAccessors (PropertyInfo pi)
	{
		MethodInfo [] ret;

		if (pi is PropertyBuilder){
			Pair pair = (Pair) properties [pi];

			ret = new MethodInfo [2];
			ret [0] = (MethodInfo) pair.First;
			ret [1] = (MethodInfo) pair.Second;

			return ret;
		} else {
			MethodInfo [] mi = new MethodInfo [2];

			//
			// Why this and not pi.GetAccessors?
			// Because sometimes index 0 is the getter
			// sometimes it is 1
			//
			mi [0] = pi.GetGetMethod (true);
			mi [1] = pi.GetSetMethod (true);

			return mi;
		}
	}

	static public MethodInfo GetPropertyGetter (PropertyInfo pi)
	{
		if (pi is PropertyBuilder){
			Pair de = (Pair) properties [pi];

			return (MethodInfo) de.Second;
		} else
			return pi.GetSetMethod ();
	}

	static public MethodInfo GetPropertySetter (PropertyInfo pi)
	{
		if (pi is PropertyBuilder){
			Pair de = (Pair) properties [pi];

			return (MethodInfo) de.First;
		} else
			return pi.GetGetMethod ();
	}

	/// <summary>
	///   Given an array of interface types, expand and eliminate repeated ocurrences
	///   of an interface.  
	/// </summary>
	///
	/// <remarks>
	///   This expands in context like: IA; IB : IA; IC : IA, IB; the interface "IC" to
	///   be IA, IB, IC.
	/// </remarks>
	public static Type [] ExpandInterfaces (Type [] base_interfaces)
	{
		ArrayList new_ifaces = new ArrayList ();
		
		foreach (Type iface in base_interfaces){
			if (!new_ifaces.Contains (iface))
				new_ifaces.Add (iface);
			
			Type [] implementing = TypeManager.GetInterfaces (iface);
			
			foreach (Type imp in implementing){
				if (!new_ifaces.Contains (imp))
					new_ifaces.Add (imp);
			}
		}
		Type [] ret = new Type [new_ifaces.Count];
		new_ifaces.CopyTo (ret, 0);
		return ret;
	}
		
	/// <summary>
	///   This function returns the interfaces in the type `t'.  Works with
	///   both types and TypeBuilders.
	/// </summary>
	public static Type [] GetInterfaces (Type t)
	{
		//
		// The reason for catching the Array case is that Reflection.Emit
		// will not return a TypeBuilder for Array types of TypeBuilder types,
		// but will still throw an exception if we try to call GetInterfaces
		// on the type.
		//
		// Since the array interfaces are always constant, we return those for
		// the System.Array
		//
		
		if (t.IsArray)
			t = TypeManager.array_type;
		
		if (t is TypeBuilder){
			Type [] parent_ifaces;
			
			if (t.BaseType == null)
				parent_ifaces = NoTypes;
			else
				parent_ifaces = GetInterfaces (t.BaseType);
			Type [] type_ifaces = (Type []) builder_to_ifaces [t];
			if (type_ifaces == null)
				type_ifaces = NoTypes;

			int parent_count = parent_ifaces.Length;
			Type [] result = new Type [parent_count + type_ifaces.Length];
			parent_ifaces.CopyTo (result, 0);
			type_ifaces.CopyTo (result, parent_count);

			return result;
		} else
			return t.GetInterfaces ();
	}
	
	/// <remarks>
	///  The following is used to check if a given type implements an interface.
	///  The cache helps us reduce the expense of hitting Type.GetInterfaces everytime.
	/// </remarks>
	public static bool ImplementsInterface (Type t, Type iface)
	{
		Type [] interfaces;

		//
		// FIXME OPTIMIZATION:
		// as soon as we hit a non-TypeBuiler in the interface
		// chain, we could return, as the `Type.GetInterfaces'
		// will return all the interfaces implement by the type
		// or its parents.
		//
		do {
			interfaces = GetInterfaces (t);

			if (interfaces != null){
				foreach (Type i in interfaces){
					if (i == iface)
						return true;
				}
			}
			
			t = t.BaseType;
		} while (t != null);
		
		return false;
	}

	// This is a custom version of Convert.ChangeType() which works
	// with the TypeBuilder defined types when compiling corlib.
	public static object ChangeType (object value, Type conversionType)
	{
		if (!(value is IConvertible))
			throw new ArgumentException ();

		IConvertible convertValue = (IConvertible) value;
		CultureInfo ci = CultureInfo.CurrentCulture;
		NumberFormatInfo provider = ci.NumberFormat;

		//
		// We must use Type.Equals() here since `conversionType' is
		// the TypeBuilder created version of a system type and not
		// the system type itself.  You cannot use Type.GetTypeCode()
		// on such a type - it'd always return TypeCode.Object.
		//
		if (conversionType.Equals (typeof (Boolean)))
			return (object)(convertValue.ToBoolean (provider));
		else if (conversionType.Equals (typeof (Byte)))
			return (object)(convertValue.ToByte (provider));
		else if (conversionType.Equals (typeof (Char)))
			return (object)(convertValue.ToChar (provider));
		else if (conversionType.Equals (typeof (DateTime)))
			return (object)(convertValue.ToDateTime (provider));
		else if (conversionType.Equals (typeof (Decimal)))
			return (object)(convertValue.ToDecimal (provider));
		else if (conversionType.Equals (typeof (Double)))
			return (object)(convertValue.ToDouble (provider));
		else if (conversionType.Equals (typeof (Int16)))
			return (object)(convertValue.ToInt16 (provider));
		else if (conversionType.Equals (typeof (Int32)))
			return (object)(convertValue.ToInt32 (provider));
		else if (conversionType.Equals (typeof (Int64)))
			return (object)(convertValue.ToInt64 (provider));
		else if (conversionType.Equals (typeof (SByte)))
			return (object)(convertValue.ToSByte (provider));
		else if (conversionType.Equals (typeof (Single)))
			return (object)(convertValue.ToSingle (provider));
		else if (conversionType.Equals (typeof (String)))
			return (object)(convertValue.ToString (provider));
		else if (conversionType.Equals (typeof (UInt16)))
			return (object)(convertValue.ToUInt16 (provider));
		else if (conversionType.Equals (typeof (UInt32)))
			return (object)(convertValue.ToUInt32 (provider));
		else if (conversionType.Equals (typeof (UInt64)))
			return (object)(convertValue.ToUInt64 (provider));
		else if (conversionType.Equals (typeof (Object)))
			return (object)(value);
		else 
			throw new InvalidCastException ();
	}

	//
	// This is needed, because enumerations from assemblies
	// do not report their underlyingtype, but they report
	// themselves
	//
	public static Type EnumToUnderlying (Type t)
	{
		if (t == TypeManager.enum_type)
			return t;

		t = t.UnderlyingSystemType;
		if (!TypeManager.IsEnumType (t))
			return t;
	
		if (t is TypeBuilder) {
			// slow path needed to compile corlib
			if (t == TypeManager.bool_type ||
					t == TypeManager.byte_type ||
					t == TypeManager.sbyte_type ||
					t == TypeManager.char_type ||
					t == TypeManager.short_type ||
					t == TypeManager.ushort_type ||
					t == TypeManager.int32_type ||
					t == TypeManager.uint32_type ||
					t == TypeManager.int64_type ||
					t == TypeManager.uint64_type)
				return t;
			throw new Exception ("Unhandled typecode in enum " + " from " + t.AssemblyQualifiedName);
		}
		TypeCode tc = Type.GetTypeCode (t);

		switch (tc){
		case TypeCode.Boolean:
			return TypeManager.bool_type;
		case TypeCode.Byte:
			return TypeManager.byte_type;
		case TypeCode.SByte:
			return TypeManager.sbyte_type;
		case TypeCode.Char:
			return TypeManager.char_type;
		case TypeCode.Int16:
			return TypeManager.short_type;
		case TypeCode.UInt16:
			return TypeManager.ushort_type;
		case TypeCode.Int32:
			return TypeManager.int32_type;
		case TypeCode.UInt32:
			return TypeManager.uint32_type;
		case TypeCode.Int64:
			return TypeManager.int64_type;
		case TypeCode.UInt64:
			return TypeManager.uint64_type;
		}
		throw new Exception ("Unhandled typecode in enum " + tc + " from " + t.AssemblyQualifiedName);
	}

	//
	// When compiling corlib and called with one of the core types, return
	// the corresponding typebuilder for that type.
	//
	public static Type TypeToCoreType (Type t)
	{
		if (RootContext.StdLib || (t is TypeBuilder))
			return t;

		TypeCode tc = Type.GetTypeCode (t);

		switch (tc){
		case TypeCode.Boolean:
			return TypeManager.bool_type;
		case TypeCode.Byte:
			return TypeManager.byte_type;
		case TypeCode.SByte:
			return TypeManager.sbyte_type;
		case TypeCode.Char:
			return TypeManager.char_type;
		case TypeCode.Int16:
			return TypeManager.short_type;
		case TypeCode.UInt16:
			return TypeManager.ushort_type;
		case TypeCode.Int32:
			return TypeManager.int32_type;
		case TypeCode.UInt32:
			return TypeManager.uint32_type;
		case TypeCode.Int64:
			return TypeManager.int64_type;
		case TypeCode.UInt64:
			return TypeManager.uint64_type;
		case TypeCode.String:
			return TypeManager.string_type;
		default:
			if (t == typeof (void))
				return TypeManager.void_type;
			if (t == typeof (object))
				return TypeManager.object_type;
			if (t == typeof (System.Type))
				return TypeManager.type_type;
			return t;
		}
	}

	/// <summary>
	///   Utility function that can be used to probe whether a type
	///   is managed or not.  
	/// </summary>
	public static bool VerifyUnManaged (Type t, Location loc)
	{
		if (t.IsValueType || t.IsPointer){
			//
			// FIXME: this is more complex, we actually need to
			// make sure that the type does not contain any
			// classes itself
			//
			return true;
		}

		if (!RootContext.StdLib && (t == TypeManager.decimal_type))
			// We need this explicit check here to make it work when
			// compiling corlib.
			return true;

		Report.Error (
			208, loc,
			"Cannot take the address or size of a variable of a managed type ('" +
			CSharpName (t) + "')");
		return false;	
	}
	
	/// <summary>
	///   Returns the name of the indexer in a given type.
	/// </summary>
	/// <remarks>
	///   The default is not always `Item'.  The user can change this behaviour by
	///   using the DefaultMemberAttribute in the class.
	///
	///   For example, the String class indexer is named `Chars' not `Item' 
	/// </remarks>
	public static string IndexerPropertyName (Type t)
	{
		if (t is TypeBuilder) {
			if (t.IsInterface) {
				Interface i = LookupInterface (t);

				if ((i == null) || (i.IndexerName == null))
					return "Item";

				return i.IndexerName;
			} else {
				TypeContainer tc = LookupTypeContainer (t);

				if ((tc == null) || (tc.IndexerName == null))
					return "Item";

				return tc.IndexerName;
			}
		}
		
		System.Attribute attr = System.Attribute.GetCustomAttribute (
			t, TypeManager.default_member_type);
		if (attr != null){
			DefaultMemberAttribute dma = (DefaultMemberAttribute) attr;
			return dma.MemberName;
		}

		return "Item";
	}

	public static void MakePinned (LocalBuilder builder)
	{
		//
		// FIXME: Flag the "LocalBuilder" type as being
		// pinned.  Figure out API.
		//
	}


	//
	// Returns whether the array of memberinfos contains the given method
	//
	static bool ArrayContainsMethod (MemberInfo [] array, MethodBase new_method)
	{
		Type [] new_args = TypeManager.GetArgumentTypes (new_method);
		
		foreach (MethodBase method in array){
			if (method.Name != new_method.Name)
				continue;
			
			Type [] old_args = TypeManager.GetArgumentTypes (method);
			int old_count = old_args.Length;
			int i;
			
			if (new_args.Length != old_count)
				continue;
			
			for (i = 0; i < old_count; i++){
				if (old_args [i] != new_args [i])
					break;
			}
			if (i != old_count)
				continue;

			return true;
		}
		return false;
	}
	
	//
	// We copy methods from `new_members' into `target_list' if the signature
	// for the method from in the new list does not exist in the target_list
	//
	// The name is assumed to be the same.
	//
	public static ArrayList CopyNewMethods (ArrayList target_list, MemberList new_members)
	{
		if (target_list == null){
			target_list = new ArrayList ();

			foreach (MemberInfo mi in new_members){
				if (mi is MethodBase)
					target_list.Add (mi);
			}
			return target_list;
		}
		
		MemberInfo [] target_array = new MemberInfo [target_list.Count];
		target_list.CopyTo (target_array, 0);
		
		foreach (MemberInfo mi in new_members){
			MethodBase new_method = (MethodBase) mi;
			
			if (!ArrayContainsMethod (target_array, new_method))
				target_list.Add (new_method);
		}
		return target_list;
	}

	[Flags]
	public enum MethodFlags {
		IsObsolete = 1,
		IsObsoleteError = 2,
		ShouldIgnore = 3
	}
	
	//
	// Returns the TypeManager.MethodFlags for this method.
	// This emits an error 619 / warning 618 if the method is obsolete.
	// In the former case, TypeManager.MethodFlags.IsObsoleteError is returned.
	//
	static public MethodFlags GetMethodFlags (MethodBase mb, Location loc)
	{
		MethodFlags flags = 0;
		
		if (mb.DeclaringType is TypeBuilder){
			MethodData method = (MethodData) builder_to_method [mb];
			if (method == null) {
				// FIXME: implement Obsolete attribute on Property,
				//        Indexer and Event.
				return 0;
			}

			return method.GetMethodFlags (loc);
		}

		object [] attrs = mb.GetCustomAttributes (true);
		foreach (object ta in attrs){
			if (!(ta is System.Attribute)){
				Console.WriteLine ("Unknown type in GetMethodFlags: " + ta);
				continue;
			}
			System.Attribute a = (System.Attribute) ta;
			if (a.TypeId == TypeManager.obsolete_attribute_type){
				ObsoleteAttribute oa = (ObsoleteAttribute) a;

				string method_desc = TypeManager.CSharpSignature (mb);

				if (oa.IsError) {
					Report.Error (619, loc, "Method `" + method_desc +
						      "' is obsolete: `" + oa.Message + "'");
					return MethodFlags.IsObsoleteError;
				} else
					Report.Warning (618, loc, "Method `" + method_desc +
							"' is obsolete: `" + oa.Message + "'");

				flags |= MethodFlags.IsObsolete;

				continue;
			}
			
			//
			// Skip over conditional code.
			//
			if (a.TypeId == TypeManager.conditional_attribute_type){
				ConditionalAttribute ca = (ConditionalAttribute) a;

				if (RootContext.AllDefines [ca.ConditionString] == null)
					flags |= MethodFlags.ShouldIgnore;
			}
		}

		return flags;
	}
	
#region MemberLookup implementation
	
	//
	// Name of the member
	//
	static string   closure_name;

	//
	// Whether we allow private members in the result (since FindMembers
	// uses NonPublic for both protected and private), we need to distinguish.
	//
	static bool     closure_private_ok;

	//
	// Who is invoking us and which type is being queried currently.
	//
	static Type     closure_invocation_type;
	static Type     closure_queried_type;
	static Type     closure_start_type;

	//
	// The assembly that defines the type is that is calling us
	//
	static Assembly closure_invocation_assembly;

	//
	// This filter filters by name + whether it is ok to include private
	// members in the search
	//
	static internal bool FilterWithClosure (MemberInfo m, object filter_criteria)
	{
		//
		// Hack: we know that the filter criteria will always be in the `closure'
		// fields. 
		//

		if ((filter_criteria != null) && (m.Name != (string) filter_criteria))
				return false;

		if (closure_start_type == closure_invocation_type)
			return true;

		//
		// Ugly: we need to find out the type of `m', and depending
		// on this, tell whether we accept or not
		//
		if (m is MethodBase){
			MethodBase mb = (MethodBase) m;
			MethodAttributes ma = mb.Attributes & MethodAttributes.MemberAccessMask;

			if (ma == MethodAttributes.Private)
				return closure_private_ok || (closure_invocation_type == m.DeclaringType);

			//
			// FamAndAssem requires that we not only derivate, but we are on the
			// same assembly.  
			//
			if (ma == MethodAttributes.FamANDAssem){
				if (closure_invocation_assembly != mb.DeclaringType.Assembly)
					return false;
			}

			// Assembly and FamORAssem succeed if we're in the same assembly.
			if ((ma == MethodAttributes.Assembly) || (ma == MethodAttributes.FamORAssem)){
				if (closure_invocation_assembly == mb.DeclaringType.Assembly)
					return true;
			}

			// We already know that we aren't in the same assembly.
			if (ma == MethodAttributes.Assembly)
				return false;

			// Family and FamANDAssem require that we derive.
			if ((ma == MethodAttributes.Family) || (ma == MethodAttributes.FamANDAssem)){
				if (closure_invocation_type == null)
					return false;

				if (!IsSubclassOrNestedChildOf (closure_invocation_type, mb.DeclaringType))
					return false;

				// Although a derived class can access protected members of its base class
				// it cannot do so through an instance of the base class (CS1540).
				if ((closure_invocation_type != closure_start_type) &&
				    closure_invocation_type.IsSubclassOf (closure_start_type))
					return false;

				return true;
			}

			// Public.
			return true;
		}

		if (m is FieldInfo){
			FieldInfo fi = (FieldInfo) m;
			FieldAttributes fa = fi.Attributes & FieldAttributes.FieldAccessMask;

			if (fa == FieldAttributes.Private)
				return closure_private_ok || (closure_invocation_type == m.DeclaringType);

			//
			// FamAndAssem requires that we not only derivate, but we are on the
			// same assembly.  
			//
			if (fa == FieldAttributes.FamANDAssem){
				if (closure_invocation_assembly != fi.DeclaringType.Assembly)
					return false;
			}

			// Assembly and FamORAssem succeed if we're in the same assembly.
			if ((fa == FieldAttributes.Assembly) || (fa == FieldAttributes.FamORAssem)){
				if (closure_invocation_assembly == fi.DeclaringType.Assembly)
					return true;
			}

			// We already know that we aren't in the same assembly.
			if (fa == FieldAttributes.Assembly)
				return false;

			// Family and FamANDAssem require that we derive.
			if ((fa == FieldAttributes.Family) || (fa == FieldAttributes.FamANDAssem)){
				if (closure_invocation_type == null)
					return false;

				if (!IsSubclassOrNestedChildOf (closure_invocation_type, fi.DeclaringType))
					return false;

				// Although a derived class can access protected members of its base class
				// it cannot do so through an instance of the base class (CS1540).
				if ((closure_invocation_type != closure_start_type) &&
				    closure_invocation_type.IsSubclassOf (closure_start_type))
					return false;

				return true;
			}

			// Public.
			return true;
		}

		//
		// EventInfos and PropertyInfos, return true
		//
		return true;
	}

	static MemberFilter FilterWithClosure_delegate = new MemberFilter (FilterWithClosure);

	//
	// Looks up a member called `name' in the `queried_type'.  This lookup
	// is done by code that is contained in the definition for `invocation_type'.
	//
	// The binding flags are `bf' and the kind of members being looked up are `mt'
	//
	// Returns an array of a single element for everything but Methods/Constructors
	// that might return multiple matches.
	//
	public static MemberInfo [] MemberLookup (Type invocation_type, Type queried_type, 
						  MemberTypes mt, BindingFlags original_bf, string name)
	{
		Timer.StartTimer (TimerType.MemberLookup);

		MemberInfo[] retval = RealMemberLookup (invocation_type, queried_type,
							mt, original_bf, name);

		Timer.StopTimer (TimerType.MemberLookup);

		return retval;
	}

	static MemberInfo [] RealMemberLookup (Type invocation_type, Type queried_type, 
					       MemberTypes mt, BindingFlags original_bf, string name)
	{
		BindingFlags bf = original_bf;
		
		ArrayList method_list = null;
		Type current_type = queried_type;
		bool searching = (original_bf & BindingFlags.DeclaredOnly) == 0;
		bool private_ok;
		bool always_ok_flag = false;

		closure_name = name;
		closure_invocation_type = invocation_type;
		closure_invocation_assembly = invocation_type != null ? invocation_type.Assembly : null;
		closure_start_type = queried_type;

		//
		// If we are a nested class, we always have access to our container
		// type names
		//
		if (invocation_type != null){
			string invocation_name = invocation_type.FullName;
			if (invocation_name.IndexOf ('+') != -1){
				string container = queried_type.FullName + "+";
				int container_length = container.Length;
				
				if (invocation_name.Length > container_length){
					string shared = invocation_name.Substring (0, container_length);
				
					if (shared == container)
						always_ok_flag = true;
				}
			}
		}
		
		do {
			MemberList list;

			//
			// `NonPublic' is lame, because it includes both protected and
			// private methods, so we need to control this behavior by
			// explicitly tracking if a private method is ok or not.
			//
			// The possible cases are:
			//    public, private and protected (internal does not come into the
			//    equation)
			//
			if (invocation_type != null){
				if (invocation_type == current_type){
					private_ok = true;
				} else
					private_ok = always_ok_flag;
				
				if (private_ok || invocation_type.IsSubclassOf (current_type))
					bf = original_bf | BindingFlags.NonPublic;
			} else {
				private_ok = false;
				bf = original_bf & ~BindingFlags.NonPublic;
			}

			closure_private_ok = private_ok;
			closure_queried_type = current_type;

			Timer.StopTimer (TimerType.MemberLookup);

			list = MemberLookup_FindMembers (current_type, mt, bf, name, ref searching);

			Timer.StartTimer (TimerType.MemberLookup);

			if (current_type == TypeManager.object_type)
				searching = false;
			else {
				current_type = current_type.BaseType;
				
				//
				// This happens with interfaces, they have a null
				// basetype.  Look members up in the Object class.
				//
				if (current_type == null)
					current_type = TypeManager.object_type;
			}
			
			if (list.Count == 0)
				continue;
			
			//
			// Events and types are returned by both `static' and `instance'
			// searches, which means that our above FindMembers will
			// return two copies of the same.
			//
			if (list.Count == 1 && !(list [0] is MethodBase)){
				return (MemberInfo []) list;
			}

			//
			// Multiple properties: we query those just to find out the indexer
			// name
			//
			if (list [0] is PropertyInfo)
				return (MemberInfo []) list;

			//
			// We found methods, turn the search into "method scan"
			// mode.
			//
			
			method_list = CopyNewMethods (method_list, list);
			mt &= (MemberTypes.Method | MemberTypes.Constructor);
		} while (searching);

		if (method_list != null && method_list.Count > 0)
			return (MemberInfo []) method_list.ToArray (typeof (MemberInfo));
	
		//
		// Interfaces do not list members they inherit, so we have to
		// scan those.
		// 
		if (!queried_type.IsInterface)
			return null;

		if (queried_type.IsArray)
			queried_type = TypeManager.array_type;
		
		Type [] ifaces = GetInterfaces (queried_type);
		if (ifaces == null)
			return null;
		
		foreach (Type itype in ifaces){
			MemberInfo [] x;

			x = MemberLookup (null, itype, mt, bf, name);
			if (x != null)
				return x;
		}
					
		return null;
	}
#endregion
	
}

public class MemberCache {
	public readonly IMemberContainer Container;
	protected Hashtable member_hash;

	/// <summary>
	///   Create a new MemberCache for the given IMemberContainer `container'.
	/// </summary>
	public MemberCache (IMemberContainer container)
	{
		this.Container = container;

		Timer.IncrementCounter (CounterType.MemberCache);
		Timer.StartTimer (TimerType.CacheInit);

		if (Container.Parent != null)
			member_hash = SetupCache (Container.Parent.MemberCache);
		else if (Container.IsInterface)
			member_hash = SetupCache (TypeHandle.ObjectType.MemberCache);
		else
			member_hash = new Hashtable ();

		AddMembers (Container);

		Timer.StopTimer (TimerType.CacheInit);
	}

	/// <summary>
	///   Bootstrap this member cache by doing a deep-copy of our parent.
	/// </summary>
	Hashtable SetupCache (MemberCache parent)
	{
		Hashtable hash = new Hashtable ();

		IDictionaryEnumerator it = parent.member_hash.GetEnumerator ();
		while (it.MoveNext ()) {
			hash [it.Key] = ((ArrayList) it.Value).Clone ();
		}

		return hash;
	}

	void AddMembers (IMemberContainer container)
	{
		AddMembers (MemberTypes.Constructor | MemberTypes.Field | MemberTypes.Method |
			    MemberTypes.Property, container);
		AddMembers (MemberTypes.NestedType | MemberTypes.Event,
			    BindingFlags.Public, container);
		AddMembers (MemberTypes.NestedType | MemberTypes.Event,
			    BindingFlags.NonPublic, container);
	}

	void AddMembers (MemberTypes mt, IMemberContainer container)
	{
		AddMembers (mt, BindingFlags.Static | BindingFlags.Public, container);
		AddMembers (mt, BindingFlags.Static | BindingFlags.NonPublic, container);
		AddMembers (mt, BindingFlags.Instance | BindingFlags.Public, container);
		AddMembers (mt, BindingFlags.Instance | BindingFlags.NonPublic, container);
	}

	void AddMembers (MemberTypes mt, BindingFlags bf, IMemberContainer container)
	{
		MemberList members = container.GetMembers (mt, bf);
		BindingFlags new_bf = (container == Container) ? bf | BindingFlags.DeclaredOnly : bf;

		foreach (MemberInfo member in members) {
			string name = member.Name;

			ArrayList list = (ArrayList) member_hash [name];
			if (list == null) {
				list = new ArrayList ();
				member_hash.Add (name, list);
			}

			list.Add (new CacheEntry (container, member, mt, new_bf));
		}
	}

	protected static EntryType GetEntryType (MemberTypes mt, BindingFlags bf)
	{
		EntryType type = EntryType.None;

		if ((mt & MemberTypes.Constructor) != 0)
			type |= EntryType.Constructor;
		if ((mt & MemberTypes.Event) != 0)
			type |= EntryType.Event;
		if ((mt & MemberTypes.Field) != 0)
			type |= EntryType.Field;
		if ((mt & MemberTypes.Method) != 0)
			type |= EntryType.Method;
		if ((mt & MemberTypes.Property) != 0)
			type |= EntryType.Property;
		if ((mt & MemberTypes.NestedType) != 0)
			type |= EntryType.NestedType;

		if ((bf & BindingFlags.Instance) != 0)
			type |= EntryType.Instance;
		if ((bf & BindingFlags.Static) != 0)
			type |= EntryType.Static;
		if ((bf & (BindingFlags.Instance | BindingFlags.Static)) == 0)
			type |= EntryType.Instance | EntryType.Static;
		if ((bf & BindingFlags.Public) != 0)
			type |= EntryType.Public;
		if ((bf & BindingFlags.NonPublic) != 0)
			type |= EntryType.NonPublic;
		if ((bf & BindingFlags.DeclaredOnly) != 0)
			type |= EntryType.Declared;

		return type;
	}

	public static bool IsSingleMemberType (MemberTypes mt)
	{
		switch (mt) {
		case MemberTypes.Constructor:
		case MemberTypes.Event:
		case MemberTypes.Field:
		case MemberTypes.Method:
		case MemberTypes.Property:
		case MemberTypes.NestedType:
			return true;

		default:
			return false;
		}
	}

	[Flags]
	protected enum EntryType {
		None		= 0x000,

		Instance	= 0x001,
		Static		= 0x002,
		MaskStatic	= Instance|Static,

		Public		= 0x004,
		NonPublic	= 0x008,
		MaskProtection	= Public|NonPublic,

		Declared	= 0x010,

		Constructor	= 0x020,
		Event		= 0x040,
		Field		= 0x080,
		Method		= 0x100,
		Property	= 0x200,
		NestedType	= 0x400,

		MaskType	= Constructor|Event|Field|Method|Property|NestedType
	}

	protected struct CacheEntry {
		public readonly IMemberContainer Container;
		public readonly EntryType EntryType;
		public readonly MemberInfo Member;

		public CacheEntry (IMemberContainer container, MemberInfo member,
				   MemberTypes mt, BindingFlags bf)
		{
			this.Container = container;
			this.Member = member;
			this.EntryType = GetEntryType (mt, bf);
		}
	}

	protected bool DoneSearching (ArrayList list)
	{
		//
		// We've found exactly one member in the current class and it's not
		// a method or constructor.
		//
		if (list.Count == 1 && !(list [0] is MethodBase))
			return true;

		//
		// Multiple properties: we query those just to find out the indexer
		// name
		//
		if ((list.Count > 0) && (list [0] is PropertyInfo))
			return true;

		return false;
	}

	public MemberList FindMembers (MemberTypes mt, BindingFlags bf, string name,
				       MemberFilter filter, object criteria)
	{
		bool declared_only = (bf & BindingFlags.DeclaredOnly) != 0;

		ArrayList applicable = (ArrayList) member_hash [name];
		if (applicable == null)
			return MemberList.Empty;

		ArrayList list = new ArrayList ();

		Timer.StartTimer (TimerType.CachedLookup);

		IMemberContainer current = Container;

		// `applicable' is a list of all members with the given member name `name'
		// in the current class and all its parent classes.  The list is sorted in
		// reverse order due to the way how the cache is initialy created (to speed
		// things up, we're doing a deep-copy of our parent).

		for (int i = applicable.Count-1; i >= 0; i--) {
			CacheEntry entry = (CacheEntry) applicable [i];

			// This happens each time we're walking one level up in the class
			// hierarchy.  If we're doing a DeclaredOnly search, we must abort
			// the first time this happens (this may already happen in the first
			// iteration of this loop if there are no members with the name we're
			// looking for in the current class).
			if (entry.Container != current) {
				if (declared_only || DoneSearching (list))
					break;

				current = entry.Container;
			}

			EntryType type = GetEntryType (mt, bf);

			// Is the member of the correct type ?
			if ((entry.EntryType & type & EntryType.MaskType) == 0)
				continue;

			// Is the member static/non-static ?
			if ((entry.EntryType & type & EntryType.MaskStatic) == 0)
				continue;

			// Apply the filter to it.
			if (filter (entry.Member, criteria))
				list.Add (entry.Member);
		}

		Timer.StopTimer (TimerType.CachedLookup);

		return new MemberList (list);
	}
}

public sealed class TypeHandle : IMemberContainer {
	public readonly TypeHandle BaseType;

	readonly int id = ++next_id;
	static int next_id = 0;

	/// <summary>
	///   Lookup a TypeHandle instance for the given type.  If the type doesn't have
	///   a TypeHandle yet, a new instance of it is created.  This static method
	///   ensures that we'll only have one TypeHandle instance per type.
	/// </summary>
	public static TypeHandle GetTypeHandle (Type t)
	{
		TypeHandle handle = (TypeHandle) type_hash [t];
		if (handle != null)
			return handle;

		handle = new TypeHandle (t);
		type_hash.Add (t, handle);
		return handle;
	}

	/// <summary>
	///   Returns the TypeHandle for TypeManager.object_type.
	/// </summary>
	public static IMemberContainer ObjectType {
		get {
			if (object_type != null)
				return object_type;

			object_type = GetTypeHandle (TypeManager.object_type);

			return object_type;
		}
	}

	/// <summary>
	///   Returns the TypeHandle for TypeManager.array_type.
	/// </summary>
	public static IMemberContainer ArrayType {
		get {
			if (array_type != null)
				return array_type;

			array_type = GetTypeHandle (TypeManager.array_type);

			return array_type;
		}
	}

	private static PtrHashtable type_hash = new PtrHashtable ();

	private static TypeHandle object_type = null;
	private static TypeHandle array_type = null;

	private Type type;
	private bool is_interface;
	private MemberCache member_cache;

	private TypeHandle (Type type)
	{
		this.type = type;
		if (type.BaseType != null)
			BaseType = GetTypeHandle (type.BaseType);
		else if ((type != TypeManager.object_type) && (type != typeof (object)))
			is_interface = true;
		this.member_cache = new MemberCache (this);
	}

	// IMemberContainer methods

	public string Name {
		get {
			return Type.FullName;
		}
	}

	public Type Type {
		get {
			return type;
		}
	}

	public IMemberContainer Parent {
		get {
			return BaseType;
		}
	}

	public bool IsInterface {
		get {
			return is_interface;
		}
	}

	public MemberList GetMembers (MemberTypes mt, BindingFlags bf)
	{
		return new MemberList (Type.FindMembers (mt, bf | BindingFlags.DeclaredOnly, null, null));
	}

	// IMemberFinder methods

	public MemberList FindMembers (MemberTypes mt, BindingFlags bf,
				       MemberFilter filter, object criteria)
	{
		throw new NotSupportedException ();
	}

	public MemberCache MemberCache {
		get {
			return member_cache;
		}
	}

	public override string ToString ()
	{
		if (BaseType != null)
			return "TypeHandle (" + id + "," + Name + " : " + BaseType + ")";
		else
			return "TypeHandle (" + id + "," + Name + ")";
	}
}

}

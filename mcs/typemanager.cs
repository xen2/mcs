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
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace Mono.CSharp {

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
	static public Type structlayout_type;

	static public Type [] NoTypes;
	
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
	
	//
	// The attribute constructors.
	//
	static public ConstructorInfo cons_param_array_attribute;
	static public ConstructorInfo void_decimal_ctor_five_args;
	
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

	// <remarks>
	//   Keeps a mapping between TypeBuilders and their TypeContainers
	// </remarks>
	static PtrHashtable builder_to_container;

	// <remarks>
	//   Maps MethodBase.RuntimeTypeHandle to a Type array that contains
	//   the arguments to the method
	// </remarks>
	static Hashtable method_arguments;

	// <remarks>
	//   Maybe `method_arguments' should be replaced and only
	//   method_internal_params should be kept?
	// <remarks>
	static Hashtable method_internal_params;

	static PtrHashtable builder_to_interface;

	// <remarks>
	//  Keeps track of delegate types
	// </remarks>

	static Hashtable builder_to_delegate;

	// <remarks>
	//  Keeps track of enum types
	// </remarks>

	static Hashtable builder_to_enum;

	// <remarks>
	//  Keeps track of attribute types
	// </remarks>

	static Hashtable builder_to_attr;

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

	static TypeManager ()
	{
		assemblies = new Assembly [0];
		modules = null;
		user_types = new ArrayList ();
		
		types = new Hashtable ();
		typecontainers = new Hashtable ();
		
		builder_to_interface = new PtrHashtable ();
		builder_to_delegate = new PtrHashtable ();
		builder_to_enum  = new PtrHashtable ();
		builder_to_attr = new PtrHashtable ();
		method_arguments = new PtrHashtable ();
		method_internal_params = new PtrHashtable ();
		builder_to_container = new PtrHashtable ();
		
		NoTypes = new Type [0];

		signature_filter = new MemberFilter (SignatureFilter);
	}

	public static void AddUserType (string name, TypeBuilder t)
	{
		try {
			types.Add (name, t);
			user_types.Add (t);
		} catch {
			Report.Error (-17, "The type `" + name + "' has already been defined");
		}
	}
	
	public static void AddUserType (string name, TypeBuilder t, TypeContainer tc)
	{
		AddUserType (name, t);
		builder_to_container.Add (t, tc);
		typecontainers.Add (name, tc);
	}

	public static void AddDelegateType (string name, TypeBuilder t, Delegate del)
	{
		types.Add (name, t);
		builder_to_delegate.Add (t, del);
	}
	
	public static void AddEnumType (string name, TypeBuilder t, Enum en)
	{
		types.Add (name, t);
		builder_to_enum.Add (t, en);
	}

	public static void AddUserInterface (string name, TypeBuilder t, Interface i)
	{
		AddUserType (name, t);
		builder_to_interface.Add (t, i);
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
		return (TypeContainer) builder_to_container [t];
	}

	public static Interface LookupInterface (Type t)
	{
		return (Interface) builder_to_interface [t];
	}

	public static Delegate LookupDelegate (Type t)
	{
		return (Delegate) builder_to_delegate [t];
	}

	public static Enum LookupEnum (Type t)
	{
		return (Enum) builder_to_enum [t];
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
                InternalParameters iparams = LookupParametersByBuilder(mb);

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
		MemberInfo [] mi;
		Signature sig;

		sig.name = name;
		sig.args = args;
		
		mi = FindMembers (
			t, MemberTypes.Method,
			instance_and_static | BindingFlags.Public, signature_filter, sig);
		if (mi == null || mi.Length == 0 || !(mi [0] is MethodInfo)){
			Report.Error (-19, "Can not find the core function `" + name + "'");
			return null;
		}

		return (MethodInfo) mi [0];
	}

	/// <summary>
	///    Returns the ConstructorInfo for "args"
	/// </summary>
	static ConstructorInfo GetConstructor (Type t, Type [] args)
	{
		MemberInfo [] mi;
		Signature sig;

		sig.name = ".ctor";
		sig.args = args;
		
		mi = FindMembers (t, MemberTypes.Constructor,
				  instance_and_static | BindingFlags.Public, signature_filter, sig);
		if (mi == null || mi.Length == 0 || !(mi [0] is ConstructorInfo)){
			Report.Error (-19, "Can not find the core constructor for type `" + t.Name + "'");
			return null;
		}

		return (ConstructorInfo) mi [0];
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
		string_type   = CoreLookupType ("System.String");
		int32_type    = CoreLookupType ("System.Int32");
		int64_type    = CoreLookupType ("System.Int64");
		uint32_type   = CoreLookupType ("System.UInt32"); 
		uint64_type   = CoreLookupType ("System.UInt64"); 
		float_type    = CoreLookupType ("System.Single");
		double_type   = CoreLookupType ("System.Double");
		byte_type     = CoreLookupType ("System.Byte");
		sbyte_type    = CoreLookupType ("System.SByte");
		char_type     = CoreLookupType ("System.Char");
		char_ptr_type = CoreLookupType ("System.Char*");
		short_type    = CoreLookupType ("System.Int16");
		ushort_type   = CoreLookupType ("System.UInt16");
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
		structlayout_type    = CoreLookupType ("System.Runtime.InteropServices.StructLayoutAttribute");
		attribute_usage_type = CoreLookupType ("System.AttributeUsageAttribute");
		dllimport_type       = CoreLookupType ("System.Runtime.InteropServices.DllImportAttribute");
		methodimpl_attr_type = CoreLookupType ("System.Runtime.CompilerServices.MethodImplAttribute");
		marshal_as_attr_type  = CoreLookupType ("System.Runtime.InteropServices.MarshalAsAttribute");
		param_array_type      = CoreLookupType ("System.ParamArrayAttribute");

		unverifiable_code_type= CoreLookupType ("System.Security.UnverifiableCodeAttribute");

		void_ptr_type         = CoreLookupType ("System.Void*");

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
		Type [] int_arg = { int32_type };
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
		
	}

	const BindingFlags instance_and_static = BindingFlags.Static | BindingFlags.Instance;

	//
	// FIXME: This can be optimized easily.  speedup by having a single builder mapping
	//
	public static MemberInfo [] RealFindMembers (Type t, MemberTypes mt, BindingFlags bf,
						     MemberFilter filter, object criteria)
	{
		//
		// We have to take care of arrays specially, because GetType on
		// a TypeBuilder array will return a Type, not a TypeBuilder,
		// and we can not call FindMembers on this type.
		//
		if (t.IsSubclassOf (TypeManager.array_type))
			return TypeManager.array_type.FindMembers (mt, bf, filter, criteria);
		
		if (!(t is TypeBuilder)){
			//
			// Since FindMembers will not lookup both static and instance
			// members, we emulate this behaviour here.
			//
			if ((bf & instance_and_static) == instance_and_static){
				MemberInfo [] i_members = t.FindMembers (
					mt, bf & ~BindingFlags.Static, filter, criteria);
				MemberInfo [] s_members = t.FindMembers (
					mt, bf & ~BindingFlags.Instance, filter, criteria);

				int i_len = i_members.Length;
				int s_len = s_members.Length;
				if (i_len > 0 || s_len > 0){
					MemberInfo [] both = new MemberInfo [i_len + s_len];

					i_members.CopyTo (both, 0);
					s_members.CopyTo (both, i_len);

					return both;
				} else
					return i_members;
			}
		        return t.FindMembers (mt, bf, filter, criteria);
		}

		//
		// FIXME: We should not have builder_to_blah everywhere,
		// we should just have a builder_to_findmemberizable
		// and have them implement a new ICanFindMembers interface
		//
		Enum e = (Enum) builder_to_enum [t];

		if (e != null)
		        return e.FindMembers (mt, bf, filter, criteria);
		
		Delegate del = (Delegate) builder_to_delegate [t];

		if (del != null)
		        return del.FindMembers (mt, bf, filter, criteria);

		Interface iface = (Interface) builder_to_interface [t];

		if (iface != null) 
		        return iface.FindMembers (mt, bf, filter, criteria);
		
		TypeContainer tc = (TypeContainer) builder_to_container [t];

		if (tc != null)
			return tc.FindMembers (mt, bf, filter, criteria);

		return null;
	}

	struct CriteriaKey {
		public MemberTypes mt;
		public BindingFlags bf;
		public MemberFilter filter;
	}
	
	static Hashtable criteria_cache = new PtrHashtable ();

	//
	// This is a wrapper for RealFindMembers, this provides a front-end cache
	//
	public static MemberInfo [] FindMembers (Type t, MemberTypes mt, BindingFlags bf,
						 MemberFilter filter, object criteria)
	{
		Hashtable criteria_hash = (Hashtable) criteria_cache [criteria];
		MemberInfo [] val;
		Hashtable type_hash;
		CriteriaKey ck;
		
		ck.mt = mt;
		ck.bf = bf;
		ck.filter = filter;
		
		if (criteria_hash != null){
			type_hash = (Hashtable) criteria_hash [t];

			if (type_hash != null){
				if (type_hash.Contains (ck))
					return (MemberInfo []) type_hash [ck];
			} else {
				type_hash = new Hashtable ();
				criteria_hash [t] = type_hash;
			}
		} else {
			criteria_hash = new Hashtable ();
			type_hash = new Hashtable ();
			criteria_cache [criteria] = criteria_hash;
			criteria_hash [t] = type_hash;
		}
		
		val = RealFindMembers (t, mt, bf, filter, criteria);
		type_hash [ck] = val;
		return val;
	}
	

	public static bool IsBuiltinType (Type t)
	{
		if (t == object_type || t == string_type || t == int32_type || t == uint32_type ||
		    t == int64_type || t == uint64_type || t == float_type || t == double_type ||
		    t == char_type || t == short_type || t == decimal_type || t == bool_type ||
		    t == sbyte_type || t == byte_type || t == ushort_type)
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
	
	public static bool IsInterfaceType (Type t)
	{
		Interface iface = (Interface) builder_to_interface [t];

		if (iface != null)
			return true;
		else
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
			interfaces = t.GetInterfaces ();

			for (int i = interfaces.Length; i > 0; ){
				i--;
				if (interfaces [i] == iface)
					return true;
			}
			t = t.BaseType;
		} while (t != null);
		
		return false;
	}

	//
	// This is needed, because enumerations from assemblies
	// do not report their underlyingtype, but they report
	// themselves
	//
	public static Type EnumToUnderlying (Type t)
	{
		t = t.UnderlyingSystemType;
		if (!TypeManager.IsEnumType (t))
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
		}
		throw new Exception ("Unhandled typecode in enum" + tc);
	}

	/// <summary>
	///   Utility function that can be used to probe whether a type
	///   is managed or not.  
	/// </summary>
	public static bool VerifyUnManaged (Type t, Location loc)
	{
		if (t.IsValueType){
			//
			// FIXME: this is more complex, we actually need to
			// make sure that the type does not contain any
			// classes itself
			//
			return true;
		}

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
			TypeContainer tc = (TypeContainer) builder_to_container [t];

			Attributes attrs = tc.OptAttributes;
			
			if (attrs == null || attrs.AttributeSections == null)
				return "Item";

			foreach (AttributeSection asec in attrs.AttributeSections) {

				if (asec.Attributes == null)
					continue;

				foreach (Attribute a in asec.Attributes) {
					if (a.Name.IndexOf ("DefaultMember") != -1) {
						ArrayList pos_args = (ArrayList) a.Arguments [0];
						Expression e = ((Argument) pos_args [0]).expr;

						if (e is StringConstant)
							return ((StringConstant) e).Value;
					}
				}
			}

			return "Item";
		}
		
		System.Attribute attr = System.Attribute.GetCustomAttribute (t, TypeManager.default_member_type);
		
		if (attr != null)
		{
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
			
			if (!(method is MethodInfo && new_method is MethodInfo))
				return true;
			
			if (((MethodInfo) method).ReturnType == ((MethodInfo) new_method).ReturnType)
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
	public static ArrayList CopyNewMethods (ArrayList target_list, MemberInfo [] new_members)
	{
		if (target_list == null){
			target_list = new ArrayList ();
			
			target_list.AddRange (new_members);
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

		if (m.Name != closure_name)
			return false;

		//
		// Ugly: we need to find out the type of `m', and depending
		// on this, tell whether we accept or not
		//
		if (m is MethodBase){
			MethodBase mb = (MethodBase) m;
			MethodAttributes ma = mb.Attributes & MethodAttributes.MemberAccessMask;

			if (ma == MethodAttributes.Private)
				return closure_private_ok;

			//
			// FamAndAssem requires that we not only derivate, but we are on the
			// same assembly.  
			//
			if (ma == MethodAttributes.FamANDAssem){
				if (closure_invocation_assembly != mb.DeclaringType.Assembly)
					return false;
			}

			// FamORAssem, Family and Public:
			return true;
		}

		if (m is FieldInfo){
			FieldInfo fi = (FieldInfo) m;
			FieldAttributes fa = fi.Attributes & FieldAttributes.FieldAccessMask;

			if (fa == FieldAttributes.Private)
				return closure_private_ok;

			//
			// FamAndAssem requires that we not only derivate, but we are on the
			// same assembly.  
			//
			if (fa == FieldAttributes.FamANDAssem){
				if (closure_invocation_assembly != fi.DeclaringType.Assembly)
					return false;
			}
			// FamORAssem, Family and Public:
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
		BindingFlags bf = original_bf;
		
		ArrayList method_list = null;
		Type current_type = queried_type;
		bool searching = (original_bf & BindingFlags.DeclaredOnly) == 0;
		bool private_ok;
		
		closure_name = name;
		closure_invocation_type = invocation_type;
		closure_invocation_assembly = invocation_type != null ? invocation_type.Assembly : null;
		
		do {
			MemberInfo [] mi;

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
				if (invocation_type == current_type)
					private_ok = true;
				else
					private_ok = false;
				
				if (private_ok || invocation_type.IsSubclassOf (current_type))
					bf = original_bf | BindingFlags.NonPublic;
			} else {
				private_ok = false;
				bf = original_bf & ~BindingFlags.NonPublic;
			}

			closure_private_ok = private_ok;
			closure_queried_type = current_type;
			
			mi = TypeManager.FindMembers (
				current_type, mt, bf | BindingFlags.DeclaredOnly,
				FilterWithClosure_delegate, name);
			
			if (current_type == TypeManager.object_type)
				searching = false;
			else {
				current_type = current_type.BaseType;
				
				//
				// This happens with interfaces, they have a null
				// basetype
				//
				if (current_type == null)
					searching = false;
			}
			
			if (mi == null)
				continue;
			
			int count = mi.Length;
			
			if (count == 0)
				continue;
			
			//
			// Events are returned by both `static' and `instance'
			// searches, which means that our above FindMembers will
			// return two copies of the same.
			//
			if (count == 1 && !(mi [0] is MethodBase)){
				return mi;
			}
			
			if (count == 2 && (mi [0] is EventInfo))
				return mi;
			
			//
			// We found methods, turn the search into "method scan"
			// mode.
			//
			
			method_list = CopyNewMethods (method_list, mi);
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

		Type [] ifaces = queried_type.GetInterfaces ();

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

}

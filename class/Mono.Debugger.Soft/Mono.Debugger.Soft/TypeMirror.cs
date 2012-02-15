using System;
using System.Collections.Generic;
using System.Reflection;
using C = Mono.Cecil;
using Mono.Cecil.Metadata;

namespace Mono.Debugger.Soft
{
	/*
	 * Represents a type in the remote virtual machine.
	 * It might be better to make this a subclass of Type, but that could be
	 * difficult as some of our methods like GetMethods () return Mirror objects.
	 */
	public class TypeMirror : Mirror
	{
		MethodMirror[] methods;
		AssemblyMirror ass;
		ModuleMirror module;
		C.TypeDefinition meta;
		FieldInfoMirror[] fields;
		PropertyInfoMirror[] properties;
		TypeInfo info;
		TypeMirror base_type, element_type, gtd;
		TypeMirror[] nested;
		CustomAttributeDataMirror[] cattrs;
		TypeMirror[] ifaces;
		Dictionary<TypeMirror, InterfaceMappingMirror> iface_map;
		TypeMirror[] type_args;

		internal const BindingFlags DefaultBindingFlags =
		BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

		internal TypeMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		public string Name {
			get {
				return GetInfo ().name;
			}
		}

		public string Namespace {
			get {
				return GetInfo ().ns;
			}
		}

		public AssemblyMirror Assembly {
			get {
				if (ass == null) {
					ass = vm.GetAssembly (GetInfo ().assembly);
				}
				return ass;
			}
		}

		public ModuleMirror Module {
			get {
				if (module == null) {
					module = vm.GetModule (GetInfo ().module);
				}										   
				return module;
			}
		}

		public int MetadataToken {
			get {
				return GetInfo ().token;
			}
		}

		public TypeAttributes Attributes {
			get {
				return (TypeAttributes)GetInfo ().attributes;
			}
		}

		public TypeMirror BaseType {
			get {
				// FIXME: base_type could be null for object/interfaces
				if (base_type == null) {
					base_type = vm.GetType (GetInfo ().base_type);
				}
				return base_type;
			}
		}

		public int GetArrayRank () {
			GetInfo ();
			if (info.rank == 0)
				throw new ArgumentException ("Type must be an array type.");
			return info.rank;
		}


		public bool IsAbstract {
			get {
				return (Attributes & TypeAttributes.Abstract) != 0;
			}
		}

		public bool IsAnsiClass {
			get {
				return (Attributes & TypeAttributes.StringFormatMask)
				== TypeAttributes.AnsiClass;
			}
		}

		public bool IsArray {
			get {
				return IsArrayImpl ();
			}
		}

		public bool IsAutoClass {
			get {
				return (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass;
			}
		}

		public bool IsAutoLayout {
			get {
				return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout;
			}
		}

		public bool IsByRef {
			get {
				return IsByRefImpl ();
			}
		}

		public bool IsClass {
			get {
				if (IsInterface)
					return false;

				return !IsValueType;
			}
		}

		public bool IsCOMObject {
			get {
				return IsCOMObjectImpl ();
			}
		}

		public bool IsContextful {
			get {
				return IsContextfulImpl ();
			}
		}

		public bool IsEnum {
			get {
				return GetInfo ().is_enum;
			}
		}

		public bool IsExplicitLayout {
			get {
				return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout;
			}
		}

		public bool IsImport {
			get {
				return (Attributes & TypeAttributes.Import) != 0;
			}
		}

		public bool IsInterface {
			get {
				return (Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
			}
		}

		public bool IsLayoutSequential {
			get {
				return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout;
			}
		}

		public bool IsMarshalByRef {
			get {
				return IsMarshalByRefImpl ();
			}
		}

		public bool IsNestedAssembly {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedAssembly;
			}
		}

		public bool IsNestedFamANDAssem {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamANDAssem;
			}
		}

		public bool IsNestedFamily {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily;
			}
		}

		public bool IsNestedFamORAssem {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamORAssem;
			}
		}

		public bool IsNestedPrivate {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate;
			}
		}

		public bool IsNestedPublic {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic;
			}
		}

		public bool IsNotPublic {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic;
			}
		}

		public bool IsPointer {
			get {
				return IsPointerImpl ();
			}
		}

		public bool IsPrimitive {
			get {
				return IsPrimitiveImpl ();
			}
		}

		public bool IsPublic {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public;
			}
		}

		public bool IsSealed {
			get {
				return (Attributes & TypeAttributes.Sealed) != 0;
			}
		}

		public bool IsSerializable {
			get {
				if ((Attributes & TypeAttributes.Serializable) != 0)
					return true;

				// FIXME:
				return false;
			}
		}

		public bool IsSpecialName {
			get {
				return (Attributes & TypeAttributes.SpecialName) != 0;
			}
		}

		public bool IsUnicodeClass {
			get {
				return (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass;
			}
		}

		public bool IsValueType {
			get {
				return IsValueTypeImpl ();
			}
		}

		public bool HasElementType {
			get {
				return HasElementTypeImpl ();
			}
		}

		// Since protocol version 2.12
		public bool IsGenericTypeDefinition {
			get {
				vm.CheckProtocolVersion (2, 12);
				GetInfo ();
				return info.is_gtd;
			}
		}

		public bool IsGenericType {
			get {
				if (vm.Version.AtLeast (2, 12)) {
					return GetInfo ().is_generic_type;
				} else {
					return Name.IndexOf ('`') != -1;
				}
			}
		}

		public TypeMirror GetElementType () {
			GetInfo ();
			if (element_type == null && info.element_type != 0)
				element_type = vm.GetType (info.element_type);
			return element_type;
		}

		public TypeMirror GetGenericTypeDefinition () {
			vm.CheckProtocolVersion (2, 12);
			GetInfo ();
			if (gtd == null) {
				if (info.gtd == 0)
					throw new InvalidOperationException ();
				gtd = vm.GetType (info.gtd);
			}
			return gtd;
		}

		// Since protocol version 2.15
		public TypeMirror[] GetGenericArguments () {
			vm.CheckProtocolVersion (2, 15);
			if (type_args == null)
				type_args = vm.GetTypes (GetInfo ().type_args);
			return type_args;
		}

		public string FullName {
			get {
				return GetInfo ().full_name;
			}
		}

		public string CSharpName {
			get {
				if (IsArray) {
					if (GetArrayRank () == 1)
						return GetElementType ().CSharpName + "[]";
					else {
						string ranks = "";
						for (int i = 0; i < GetArrayRank (); ++i)
							ranks += ',';
						return GetElementType ().CSharpName + "[" + ranks + "]";
					}
				}
				if (IsPrimitive) {
					switch (Name) {
					case "Byte":
						return "byte";
					case "Int32":
						return "int";
					case "Boolean":
						return "bool";
					default:
						return FullName;
					}
				}
				// FIXME: Only do this for real corlib types
				if (Namespace == "System") {
					string s = Name;
					switch (s) {
					case "String":
						return "string";
					default:
						return FullName;
					}
				} else {
					return FullName;
				}
			}
		}

		public MethodMirror[] GetMethods () {
			if (methods == null) {
				long[] ids = vm.conn.Type_GetMethods (id);
				MethodMirror[] m = new MethodMirror [ids.Length];
				for (int i = 0; i < ids.Length; ++i) {
					m [i] = vm.GetMethod (ids [i]);
				}
				methods = m;
			}
			return methods;
		}

		// FIXME: Sync this with Type
		public MethodMirror GetMethod (string name) {
			foreach (var m in GetMethods ())
				if (m.Name == name)
					return m;
			return null;
		}

		public FieldInfoMirror[] GetFields () {
			if (fields != null)
				return fields;

			string[] names;
			long[] types;
			int[] attrs;
			long[] ids = vm.conn.Type_GetFields (id, out names, out types, out attrs);

			FieldInfoMirror[] res = new FieldInfoMirror [ids.Length];
			for (int i = 0; i < res.Length; ++i)
				res [i] = new FieldInfoMirror (this, ids [i], names [i], vm.GetType (types [i]), (FieldAttributes)attrs [i]);

			fields = res;
			return fields;
		}

		public FieldInfoMirror GetField (string name) {
			if (name == null)
				throw new ArgumentNullException ("name");
			foreach (var f in GetFields ())
				if (f.Name == name)
					return f;
			return null;
		}

		public TypeMirror[] GetNestedTypes ()
		{
			return GetNestedTypes (DefaultBindingFlags);
		}

		public TypeMirror[] GetNestedTypes (BindingFlags bindingAttr) {
			if (nested != null)
				return nested;

			// FIXME: bindingAttr
			GetInfo ();
			var arr = new TypeMirror [info.nested.Length];
			for (int i = 0; i < arr.Length; ++i)
				arr [i] = vm.GetType (info.nested [i]);
			nested = arr;

			return nested;
		}

		public PropertyInfoMirror[] GetProperties () {
			return GetProperties (DefaultBindingFlags);
		}

		public PropertyInfoMirror[] GetProperties (BindingFlags bindingAttr) {
			if (properties != null)
				return properties;

			PropInfo[] info = vm.conn.Type_GetProperties (id);

			PropertyInfoMirror[] res = new PropertyInfoMirror [info.Length];
			for (int i = 0; i < res.Length; ++i)
				res [i] = new PropertyInfoMirror (this, info [i].id, info [i].name, vm.GetMethod (info [i].get_method), vm.GetMethod (info [i].set_method), (PropertyAttributes)info [i].attrs);

			properties = res;
			return properties;
		}

		public PropertyInfoMirror GetProperty (string name) {
			if (name == null)
				throw new ArgumentNullException ("name");
			foreach (var p in GetProperties ())
				if (p.Name == name)
					return p;
			return null;
		}

		public virtual bool IsAssignableFrom (TypeMirror c) {
			if (c == null)
				throw new ArgumentNullException ("c");

			CheckMirror (c);

			// This is complex so do it in the debuggee
			return vm.conn.Type_IsAssignableFrom (id, c.Id);
		}

		public Value GetValue (FieldInfoMirror field) {
			return GetValues (new FieldInfoMirror [] { field }) [0];
		}

		public Value[] GetValues (IList<FieldInfoMirror> fields, ThreadMirror thread) {
			if (fields == null)
				throw new ArgumentNullException ("fields");
			foreach (FieldInfoMirror f in fields) {
				if (f == null)
					throw new ArgumentNullException ("field");
				CheckMirror (f);
			}
			long[] ids = new long [fields.Count];
			for (int i = 0; i < fields.Count; ++i)
				ids [i] = fields [i].Id;
			try {
				return vm.DecodeValues (vm.conn.Type_GetValues (id, ids, thread !=  null ? thread.Id : 0));
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_FIELDID)
					throw new ArgumentException ("One of the fields is not valid for this type.", "fields");
				else
					throw;
			}
		}

		public Value[] GetValues (IList<FieldInfoMirror> fields) {
			return GetValues (fields, null);
		}

		/*
		 * Return the value of the [ThreadStatic] field FIELD on the thread THREAD.
		 */
		public Value GetValue (FieldInfoMirror field, ThreadMirror thread) {
			if (thread == null)
				throw new ArgumentNullException ("thread");
			CheckMirror (thread);
			return GetValues (new FieldInfoMirror [] { field }, thread) [0];
		}

		public void SetValues (IList<FieldInfoMirror> fields, Value[] values) {
			if (fields == null)
				throw new ArgumentNullException ("fields");
			if (values == null)
				throw new ArgumentNullException ("values");
			foreach (FieldInfoMirror f in fields) {
				if (f == null)
					throw new ArgumentNullException ("field");
				CheckMirror (f);
			}
			foreach (Value v in values) {
				if (v == null)
					throw new ArgumentNullException ("values");
				CheckMirror (v);
			}
			long[] ids = new long [fields.Count];
			for (int i = 0; i < fields.Count; ++i)
				ids [i] = fields [i].Id;
			try {
				vm.conn.Type_SetValues (id, ids, vm.EncodeValues (values));
			} catch (CommandException ex) {
				if (ex.ErrorCode == ErrorCode.INVALID_FIELDID)
					throw new ArgumentException ("One of the fields is not valid for this type.", "fields");
				else
					throw;
			}
		}

		public void SetValue (FieldInfoMirror field, Value value) {
			SetValues (new FieldInfoMirror [] { field }, new Value [] { value });
		}

		public ObjectMirror GetTypeObject () {
			return vm.GetObject (vm.conn.Type_GetObject (id));
		}

		/*
		 * Return a list of source files without path info, where methods of 
		 * this type are defined. Return an empty list if the information is not 
		 * available. 
		 * This can be used by a debugger to find out which types occur in a 
		 * given source file, to filter the list of methods whose locations
		 * have to be checked when placing breakpoints.
		 */
		public string[] GetSourceFiles () {
			return GetSourceFiles (false);
		}

		string[] source_files;
		string[] source_files_full_path;
		public string[] GetSourceFiles (bool return_full_paths) {
			string[] res = return_full_paths ? source_files_full_path : source_files;
			if (res == null) {
				res = vm.conn.Type_GetSourceFiles (id, return_full_paths);
				if (return_full_paths)
					source_files_full_path = res;
				else
					source_files = res;
			}
			return res;
		}

		public C.TypeDefinition Metadata {
			get {
				if (meta == null) {
					if (Assembly.Metadata == null || MetadataToken == 0)
						return null;
					meta = (C.TypeDefinition)Assembly.Metadata.MainModule.LookupToken (MetadataToken);
				}
				return meta;
			}
		}

		TypeInfo GetInfo () {
			if (info == null)
				info = vm.conn.Type_GetInfo (id);
			return info;
		}

		protected virtual TypeAttributes GetAttributeFlagsImpl () {
			return (TypeAttributes)GetInfo ().attributes;
		}

		protected virtual bool HasElementTypeImpl () {
			return IsArray || IsByRef || IsPointer;
		}

		protected virtual bool IsArrayImpl () {
			return GetInfo ().rank > 0;
		}

		protected virtual bool IsByRefImpl () {
			return GetInfo ().is_byref;
		}

		protected virtual bool IsCOMObjectImpl () {
			return false;
		}

		protected virtual bool IsPointerImpl () {
			return GetInfo ().is_pointer;
		}

		protected virtual bool IsPrimitiveImpl () {
			return GetInfo ().is_primitive;
		}

		protected virtual bool IsValueTypeImpl ()
		{
			return GetInfo ().is_valuetype;
		}
		
		protected virtual bool IsContextfulImpl ()
		{
			// FIXME:
			return false;
		}

		protected virtual bool IsMarshalByRefImpl ()
		{
			// FIXME:
			return false;
		}

		// Same as Enum.GetUnderlyingType ()
		public TypeMirror EnumUnderlyingType {
			get {
				if (!IsEnum)
					throw new ArgumentException ("Type is not an enum type.");
				foreach (FieldInfoMirror f in GetFields ()) {
					if (!f.IsStatic)
						return f.FieldType;
				}
				throw new NotImplementedException ();
			}
		}

		/*
		 * Creating the custom attributes themselves could modify the behavior of the
		 * debuggee, so we return objects similar to the CustomAttributeData objects
		 * used by the reflection-only functionality on .net.
		 */
		public CustomAttributeDataMirror[] GetCustomAttributes (bool inherit) {
			return GetCAttrs (null, inherit);
		}

		public CustomAttributeDataMirror[] GetCustomAttributes (TypeMirror attributeType, bool inherit) {
			if (attributeType == null)
				throw new ArgumentNullException ("attributeType");
			return GetCAttrs (attributeType, inherit);
		}

		CustomAttributeDataMirror[] GetCAttrs (TypeMirror type, bool inherit) {
			if (cattrs == null && Metadata != null && !Metadata.HasCustomAttributes)
				cattrs = new CustomAttributeDataMirror [0];

			// FIXME: Handle inherit
			if (cattrs == null) {
				CattrInfo[] info = vm.conn.Type_GetCustomAttributes (id, 0, false);
				cattrs = CustomAttributeDataMirror.Create (vm, info);
			}
			var res = new List<CustomAttributeDataMirror> ();
			foreach (var attr in cattrs)
				if (type == null || attr.Constructor.DeclaringType == type)
					res.Add (attr);
			return res.ToArray ();
		}

		public MethodMirror[] GetMethodsByNameFlags (string name, BindingFlags flags, bool ignoreCase) {
			if (vm.conn.Version.AtLeast (2, 6)) {
				long[] ids = vm.conn.Type_GetMethodsByNameFlags (id, name, (int)flags, ignoreCase);
				MethodMirror[] m = new MethodMirror [ids.Length];
				for (int i = 0; i < ids.Length; ++i)
					m [i] = vm.GetMethod (ids [i]);
				return m;
			} else {
				if (flags != (BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.NonPublic))
					throw new NotImplementedException ();
				var res = new List<MethodMirror> ();
				foreach (MethodMirror m in GetMethods ()) {
					if ((!ignoreCase && m.Name == name) || (ignoreCase && m.Name.Equals (name, StringComparison.CurrentCultureIgnoreCase)))
						res.Add (m);
				}
				return res.ToArray ();
			}
		}

		public Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments) {
			return ObjectMirror.InvokeMethod (vm, thread, method, null, arguments, InvokeOptions.None);
		}

		public Value InvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options) {
			return ObjectMirror.InvokeMethod (vm, thread, method, null, arguments, options);
		}

		[Obsolete ("Use the overload without the 'vm' argument")]
		public IAsyncResult BeginInvokeMethod (VirtualMachine vm, ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return ObjectMirror.BeginInvokeMethod (vm, thread, method, null, arguments, options, callback, state);
		}

		public IAsyncResult BeginInvokeMethod (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options, AsyncCallback callback, object state) {
			return ObjectMirror.BeginInvokeMethod (vm, thread, method, null, arguments, options, callback, state);
		}

		public Value EndInvokeMethod (IAsyncResult asyncResult) {
			return ObjectMirror.EndInvokeMethodInternal (asyncResult);
		}

		public Value NewInstance (ThreadMirror thread, MethodMirror method, IList<Value> arguments) {
			return ObjectMirror.InvokeMethod (vm, thread, method, null, arguments, InvokeOptions.None);
		}			

		public Value NewInstance (ThreadMirror thread, MethodMirror method, IList<Value> arguments, InvokeOptions options) {
			return ObjectMirror.InvokeMethod (vm, thread, method, null, arguments, options);
		}

		// Since protocol version 2.11
		public TypeMirror[] GetInterfaces () {
			if (ifaces == null)
				ifaces = vm.GetTypes (vm.conn.Type_GetInterfaces (id));
			return ifaces;
		}

		// Since protocol version 2.11
		public InterfaceMappingMirror GetInterfaceMap (TypeMirror interfaceType) {
			if (interfaceType == null)
				throw new ArgumentNullException ("interfaceType");
			if (!interfaceType.IsInterface)
				throw new ArgumentException ("Argument must be an interface.", "interfaceType");
			if (IsInterface)
				throw new ArgumentException ("'this' type cannot be an interface itself");

			if (iface_map == null) {
				// Query the info in bulk
				GetInterfaces ();
				var ids = new long [ifaces.Length];
				for (int i = 0; i < ifaces.Length; ++i)
					ids [i] = ifaces [i].Id;

				var ifacemap = vm.conn.Type_GetInterfaceMap (id, ids);

				var imap = new Dictionary<TypeMirror, InterfaceMappingMirror> ();
				for (int i = 0; i < ifacemap.Length; ++i) {
					IfaceMapInfo info = ifacemap [i];

					MethodMirror[] imethods = new MethodMirror [info.iface_methods.Length];
					for (int j = 0; j < info.iface_methods.Length; ++j)
						imethods [j] = vm.GetMethod (info.iface_methods [j]);

					MethodMirror[] tmethods = new MethodMirror [info.iface_methods.Length];
					for (int j = 0; j < info.target_methods.Length; ++j)
						tmethods [j] = vm.GetMethod (info.target_methods [j]);

					InterfaceMappingMirror map = new InterfaceMappingMirror (vm, this, vm.GetType (info.iface_id), imethods, tmethods);

					imap [map.InterfaceType] = map;
				}

				iface_map = imap;
			}

			InterfaceMappingMirror res;
			if (!iface_map.TryGetValue (interfaceType, out res))
				throw new ArgumentException ("Interface not found", "interfaceType");
			return res;
		}

    }
}

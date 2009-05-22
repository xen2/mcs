//
// System.Reflection.Emit/AssemblyBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Security.Policy;
using System.Runtime.Serialization;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;

using Mono.Security;
using Mono.Security.Cryptography;

namespace System.Reflection.Emit
{
	internal enum NativeResourceType
	{
		None,
		Unmanaged,
		Assembly,
		Explicit
	}

	internal struct RefEmitPermissionSet {
		public SecurityAction action;
		public string pset;

		public RefEmitPermissionSet (SecurityAction action, string pset) {
			this.action = action;
			this.pset = pset;
		}
	}

	internal struct MonoResource {
		public byte[] data;
		public string name;
		public string filename;
		public ResourceAttributes attrs;
		public int offset;
		public Stream stream;
	}

	internal struct MonoWin32Resource {
		public int res_type;
		public int res_id;
		public int lang_id;
		public byte[] data;

		public MonoWin32Resource (int res_type, int res_id, int lang_id, byte[] data) {
			this.res_type = res_type;
			this.res_id = res_id;
			this.lang_id = lang_id;
			this.data = data;
		}
	}

#if NET_2_0
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_AssemblyBuilder))]
#endif
	[ClassInterface (ClassInterfaceType.None)]
	public sealed class AssemblyBuilder : Assembly, _AssemblyBuilder {
#pragma warning disable 169, 414
		#region Sync with object-internals.h
		private UIntPtr dynamic_assembly; /* GC-tracked */
		private MethodInfo entry_point;
		private ModuleBuilder[] modules;
		private string name;
		private string dir;
		private CustomAttributeBuilder[] cattrs;
		private MonoResource[] resources;
		byte[] public_key;
		string version;
		string culture;
		uint algid;
		uint flags;
		PEFileKinds pekind = PEFileKinds.Dll;
		bool delay_sign;
		uint access;
		Module[] loaded_modules;
		MonoWin32Resource[] win32_resources;
		private RefEmitPermissionSet[] permissions_minimum;
		private RefEmitPermissionSet[] permissions_optional;
		private RefEmitPermissionSet[] permissions_refused;
		PortableExecutableKinds peKind;
		ImageFileMachine machine;
		bool corlib_internal;
		Type[] type_forwarders;
		#endregion
#pragma warning restore 169, 414
		
		internal Type corlib_object_type = typeof (System.Object);
		internal Type corlib_value_type = typeof (System.ValueType);
		internal Type corlib_enum_type = typeof (System.Enum);
		internal Type corlib_void_type = typeof (void);
		ArrayList resource_writers = null;
		Win32VersionResource version_res;
		bool created;
		bool is_module_only;
		private Mono.Security.StrongName sn;
		NativeResourceType native_resource;
		readonly bool is_compiler_context;
		string versioninfo_culture;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void basic_init (AssemblyBuilder ab);

		/* Keep this in sync with codegen.cs in mcs */
		private const AssemblyBuilderAccess COMPILER_ACCESS = (AssemblyBuilderAccess) 0x800;

		internal AssemblyBuilder (AssemblyName n, string directory, AssemblyBuilderAccess access, bool corlib_internal)
		{
#if BOOTSTRAP_WITH_OLDLIB
			is_compiler_context = true;
#else
			is_compiler_context = (access & COMPILER_ACCESS) != 0;
#endif

			// remove Mono specific flag to allow enum check to pass
			access &= ~COMPILER_ACCESS;

#if NET_2_0
			if (!Enum.IsDefined (typeof (AssemblyBuilderAccess), access))
				throw new ArgumentException (string.Format (CultureInfo.InvariantCulture,
					"Argument value {0} is not valid.", (int) access),
					"access");
#endif

#if NET_4_0
			if ((access & AssemblyBuilderAccess.RunAndCollect) == AssemblyBuilderAccess.RunAndCollect)
				throw new NotSupportedException ("RunAndCollect not yet supported.");
#endif

			name = n.Name;
			this.access = (uint)access;
			flags = (uint) n.Flags;

			// don't call GetCurrentDirectory for Run-only builders (CAS may not like that)
			if (IsSave && (directory == null || directory.Length == 0)) {
				dir = Directory.GetCurrentDirectory ();
			} else {
				dir = directory;
			}

			/* Set defaults from n */
			if (n.CultureInfo != null) {
				culture = n.CultureInfo.Name;
				versioninfo_culture = n.CultureInfo.Name;
			}
			Version v = n.Version;
			if (v != null) {
				version = v.ToString ();
			}

			if (n.KeyPair != null) {
				// full keypair is available (for signing)
				sn = n.KeyPair.StrongName ();
			} else {
				// public key is available (for delay-signing)
				byte[] pk = n.GetPublicKey ();
				if ((pk != null) && (pk.Length > 0)) {
					sn = new Mono.Security.StrongName (pk);
				}
			}

			if (sn != null)
				flags |= (uint) AssemblyNameFlags.PublicKey;

			this.corlib_internal = corlib_internal;

			basic_init (this);
		}

		public override string CodeBase {
			get {
				throw not_supported ();
			}
		}
		
		public override MethodInfo EntryPoint {
			get {
				return entry_point;
			}
		}

		public override string Location {
			get {
				throw not_supported ();
			}
		}

#if NET_1_1
		/* This is to keep signature compatibility with MS.NET */
		public override string ImageRuntimeVersion {
			get {
				return base.ImageRuntimeVersion;
			}
		}
#endif

#if NET_2_0
		[MonoTODO]
		public override bool ReflectionOnly {
			get { return base.ReflectionOnly; }
		}
#endif

		public void AddResourceFile (string name, string fileName)
		{
			AddResourceFile (name, fileName, ResourceAttributes.Public);
		}

		public void AddResourceFile (string name, string fileName, ResourceAttributes attribute)
		{
			AddResourceFile (name, fileName, attribute, true);
		}

		private void AddResourceFile (string name, string fileName, ResourceAttributes attribute, bool fileNeedsToExists)
		{
			check_name_and_filename (name, fileName, fileNeedsToExists);

			// Resource files are created/searched under the assembly storage
			// directory
			if (dir != null)
				fileName = Path.Combine (dir, fileName);

			if (resources != null) {
				MonoResource[] new_r = new MonoResource [resources.Length + 1];
				System.Array.Copy(resources, new_r, resources.Length);
				resources = new_r;
			} else {
				resources = new MonoResource [1];
			}
			int p = resources.Length - 1;
			resources [p].name = name;
			resources [p].filename = fileName;
			resources [p].attrs = attribute;
		}

		/// <summary>
		/// Don't change the method name and parameters order. It is used by mcs 
		/// </summary>
		internal void AddPermissionRequests (PermissionSet required, PermissionSet optional, PermissionSet refused)
		{
			if (created)
				throw new InvalidOperationException ("Assembly was already saved.");

			// required for base Assembly class (so the permissions
			// can be used even if the assembly isn't saved to disk)
			_minimum = required;
			_optional = optional;
			_refuse = refused;

			// required to reuse AddDeclarativeSecurity support 
			// already present in the runtime
			if (required != null) {
				permissions_minimum = new RefEmitPermissionSet [1];
				permissions_minimum [0] = new RefEmitPermissionSet (
					SecurityAction.RequestMinimum, required.ToXml ().ToString ());
			}
			if (optional != null) {
				permissions_optional = new RefEmitPermissionSet [1];
				permissions_optional [0] = new RefEmitPermissionSet (
					SecurityAction.RequestOptional, optional.ToXml ().ToString ());
			}
			if (refused != null) {
				permissions_refused = new RefEmitPermissionSet [1];
				permissions_refused [0] = new RefEmitPermissionSet (
					SecurityAction.RequestRefuse, refused.ToXml ().ToString ());
			}
		}

		internal void EmbedResourceFile (string name, string fileName)
		{
			EmbedResourceFile (name, fileName, ResourceAttributes.Public);
		}

		internal void EmbedResourceFile (string name, string fileName, ResourceAttributes attribute)
		{
			if (resources != null) {
				MonoResource[] new_r = new MonoResource [resources.Length + 1];
				System.Array.Copy(resources, new_r, resources.Length);
				resources = new_r;
			} else {
				resources = new MonoResource [1];
			}
			int p = resources.Length - 1;
			resources [p].name = name;
			resources [p].attrs = attribute;
			try {
				FileStream s = new FileStream (fileName, FileMode.Open, FileAccess.Read);
				long len = s.Length;
				resources [p].data = new byte [len];
				s.Read (resources [p].data, 0, (int)len);
				s.Close ();
			} catch {
				/* do something */
			}
		}

		internal void EmbedResource (string name, byte[] blob, ResourceAttributes attribute)
		{
			if (resources != null) {
				MonoResource[] new_r = new MonoResource [resources.Length + 1];
				System.Array.Copy(resources, new_r, resources.Length);
				resources = new_r;
			} else {
				resources = new MonoResource [1];
			}
			int p = resources.Length - 1;
			resources [p].name = name;
			resources [p].attrs = attribute;
			resources [p].data = blob;
		}

#if NET_2_0
		internal void AddTypeForwarder (Type t) {
			if (t == null)
				throw new ArgumentNullException ("t");

			if (type_forwarders == null) {
				type_forwarders = new Type [1] { t };
			} else {
				Type[] arr = new Type [type_forwarders.Length + 1];
				Array.Copy (type_forwarders, arr, type_forwarders.Length);
				arr [type_forwarders.Length] = t;
				type_forwarders = arr;
			}
		}
#endif

		public ModuleBuilder DefineDynamicModule (string name)
		{
			return DefineDynamicModule (name, name, false, true);
		}

		public ModuleBuilder DefineDynamicModule (string name, bool emitSymbolInfo)
		{
			return DefineDynamicModule (name, name, emitSymbolInfo, true);
		}

		public ModuleBuilder DefineDynamicModule(string name, string fileName)
		{
			return DefineDynamicModule (name, fileName, false, false);
		}

		public ModuleBuilder DefineDynamicModule (string name, string fileName,
							  bool emitSymbolInfo)
		{
			return DefineDynamicModule (name, fileName, emitSymbolInfo, false);
		}

		private ModuleBuilder DefineDynamicModule (string name, string fileName, bool emitSymbolInfo, bool transient)
		{
			check_name_and_filename (name, fileName, false);

			if (!transient) {
				if (Path.GetExtension (fileName) == String.Empty)
					throw new ArgumentException ("Module file name '" + fileName + "' must have file extension.");
				if (!IsSave)
					throw new NotSupportedException ("Persistable modules are not supported in a dynamic assembly created with AssemblyBuilderAccess.Run");
				if (created)
					throw new InvalidOperationException ("Assembly was already saved.");
			}

			ModuleBuilder r = new ModuleBuilder (this, name, fileName, emitSymbolInfo, transient);

			if ((modules != null) && is_module_only)
				throw new InvalidOperationException ("A module-only assembly can only contain one module.");

			if (modules != null) {
				ModuleBuilder[] new_modules = new ModuleBuilder [modules.Length + 1];
				System.Array.Copy(modules, new_modules, modules.Length);
				modules = new_modules;
			} else {
				modules = new ModuleBuilder [1];
			}
			modules [modules.Length - 1] = r;
			return r;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern Module InternalAddModule (string fileName);

		/*
		 * Mono extension to support /addmodule in mcs.
		 */
		internal Module AddModule (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException (fileName);

			Module m = InternalAddModule (fileName);

			if (loaded_modules != null) {
				Module[] new_modules = new Module [loaded_modules.Length + 1];
				System.Array.Copy (loaded_modules, new_modules, loaded_modules.Length);
				loaded_modules = new_modules;
			} else {
				loaded_modules = new Module [1];
			}
			loaded_modules [loaded_modules.Length - 1] = m;

			return m;
		}

		public IResourceWriter DefineResource (string name, string description, string fileName)
		{
			return DefineResource (name, description, fileName, ResourceAttributes.Public);
		}

		public IResourceWriter DefineResource (string name, string description,
						       string fileName, ResourceAttributes attribute)
		{
			IResourceWriter writer;

			// description seems to be ignored
			AddResourceFile (name, fileName, attribute, false);
			writer = new ResourceWriter (fileName);
			if (resource_writers == null)
				resource_writers = new ArrayList ();
			resource_writers.Add (writer);
			return writer;
		}

		private void AddUnmanagedResource (Win32Resource res) {
			MemoryStream ms = new MemoryStream ();
			res.WriteTo (ms);

			if (win32_resources != null) {
				MonoWin32Resource[] new_res = new MonoWin32Resource [win32_resources.Length + 1];
				System.Array.Copy (win32_resources, new_res, win32_resources.Length);
				win32_resources = new_res;
			}
			else
				win32_resources = new MonoWin32Resource [1];

			win32_resources [win32_resources.Length - 1] = new MonoWin32Resource (res.Type.Id, res.Name.Id, res.Language, ms.ToArray ());
		}

		[MonoTODO ("Not currently implemenented")]
		public void DefineUnmanagedResource (byte[] resource)
		{
			if (resource == null)
				throw new ArgumentNullException ("resource");
			if (native_resource != NativeResourceType.None)
				throw new ArgumentException ("Native resource has already been defined.");

			// avoid definition of more than one unmanaged resource
			native_resource = NativeResourceType.Unmanaged;

			/*
			 * The format of the argument byte array is not documented
			 * so this method is impossible to implement.
			 */

			throw new NotImplementedException ();
		}

		public void DefineUnmanagedResource (string resourceFileName)
		{
			if (resourceFileName == null)
				throw new ArgumentNullException ("resourceFileName");
			if (resourceFileName.Length == 0)
				throw new ArgumentException ("resourceFileName");
			if (!File.Exists (resourceFileName) || Directory.Exists (resourceFileName))
				throw new FileNotFoundException ("File '" + resourceFileName + "' does not exists or is a directory.");
			if (native_resource != NativeResourceType.None)
				throw new ArgumentException ("Native resource has already been defined.");

			// avoid definition of more than one unmanaged resource
			native_resource = NativeResourceType.Unmanaged;

			using (FileStream fs = new FileStream (resourceFileName, FileMode.Open, FileAccess.Read)) {
				Win32ResFileReader reader = new Win32ResFileReader (fs);

				foreach (Win32EncodedResource res in reader.ReadResources ()) {
					if (res.Name.IsName || res.Type.IsName)
						throw new InvalidOperationException ("resource files with named resources or non-default resource types are not supported.");

					AddUnmanagedResource (res);
				}
			}
		}

		public void DefineVersionInfoResource ()
		{
			if (native_resource != NativeResourceType.None)
				throw new ArgumentException ("Native resource has already been defined.");

			// avoid definition of more than one unmanaged resource
			native_resource = NativeResourceType.Assembly;

			version_res = new Win32VersionResource (1, 0, IsCompilerContext);
		}

		public void DefineVersionInfoResource (string product, string productVersion,
						       string company, string copyright, string trademark)
		{
			if (native_resource != NativeResourceType.None)
				throw new ArgumentException ("Native resource has already been defined.");

			// avoid definition of more than one unmanaged resource
			native_resource = NativeResourceType.Explicit;

			/*
			 * We can only create the resource later, when the file name and
			 * the binary version is known.
			 */

			version_res = new Win32VersionResource (1, 0, false);
			version_res.ProductName = product != null ? product : " ";
			version_res.ProductVersion = productVersion != null ? productVersion : " ";
			version_res.CompanyName = company != null ? company : " ";
			version_res.LegalCopyright = copyright != null ? copyright : " ";
			version_res.LegalTrademarks = trademark != null ? trademark : " ";
		}

		/* 
		 * Mono extension to support /win32icon in mcs
		 */
		internal void DefineIconResource (string iconFileName)
		{
			if (iconFileName == null)
				throw new ArgumentNullException ("iconFileName");
			if (iconFileName.Length == 0)
				throw new ArgumentException ("iconFileName");
			if (!File.Exists (iconFileName) || Directory.Exists (iconFileName))
				throw new FileNotFoundException ("File '" + iconFileName + "' does not exists or is a directory.");

			using (FileStream fs = new FileStream (iconFileName, FileMode.Open, FileAccess.Read)) {
				Win32IconFileReader reader = new Win32IconFileReader (fs);
				
				ICONDIRENTRY[] entries = reader.ReadIcons ();

				Win32IconResource[] icons = new Win32IconResource [entries.Length];
				for (int i = 0; i < entries.Length; ++i) {
					icons [i] = new Win32IconResource (i + 1, 0, entries [i]);
					AddUnmanagedResource (icons [i]);
				}

				Win32GroupIconResource group = new Win32GroupIconResource (1, 0, icons);
				AddUnmanagedResource (group);
			}
		}

		private void DefineVersionInfoResourceImpl (string fileName)
		{
			if (versioninfo_culture != null)
				version_res.FileLanguage = new CultureInfo (versioninfo_culture).LCID;
			version_res.Version = version == null ? "0.0.0.0" : version;

			if (cattrs != null) {
				switch (native_resource) {
				case NativeResourceType.Assembly:
					foreach (CustomAttributeBuilder cb in cattrs) {
						string attrname = cb.Ctor.ReflectedType.FullName;

						if (attrname == "System.Reflection.AssemblyProductAttribute")
							version_res.ProductName = cb.string_arg ();
						else if (attrname == "System.Reflection.AssemblyCompanyAttribute")
							version_res.CompanyName = cb.string_arg ();
						else if (attrname == "System.Reflection.AssemblyCopyrightAttribute")
							version_res.LegalCopyright = cb.string_arg ();
						else if (attrname == "System.Reflection.AssemblyTrademarkAttribute")
							version_res.LegalTrademarks = cb.string_arg ();
						else if (attrname == "System.Reflection.AssemblyCultureAttribute") {
							if (!IsCompilerContext)
								version_res.FileLanguage = new CultureInfo (cb.string_arg ()).LCID;
						} else if (attrname == "System.Reflection.AssemblyFileVersionAttribute") {
							string fileversion = cb.string_arg ();
							if (!IsCompilerContext || fileversion != null && fileversion.Length != 0)
								version_res.FileVersion = fileversion;
						} else if (attrname == "System.Reflection.AssemblyInformationalVersionAttribute")
							version_res.ProductVersion = cb.string_arg ();
						else if (attrname == "System.Reflection.AssemblyTitleAttribute")
							version_res.FileDescription = cb.string_arg ();
						else if (attrname == "System.Reflection.AssemblyDescriptionAttribute")
							version_res.Comments = cb.string_arg ();
					}
					break;
				case NativeResourceType.Explicit:
					foreach (CustomAttributeBuilder cb in cattrs) {
						string attrname = cb.Ctor.ReflectedType.FullName;

						if (attrname == "System.Reflection.AssemblyCultureAttribute") {
							if (!IsCompilerContext)
								version_res.FileLanguage = new CultureInfo (cb.string_arg ()).LCID;
						} else if (attrname == "System.Reflection.AssemblyDescriptionAttribute")
							version_res.Comments = cb.string_arg ();
					}
					break;
				}
			}

			version_res.OriginalFilename = fileName;

			if (IsCompilerContext) {
				version_res.InternalName = fileName;
				if (version_res.ProductVersion.Trim ().Length == 0)
					version_res.ProductVersion = version_res.FileVersion;
			} else {
				version_res.InternalName = Path.GetFileNameWithoutExtension (fileName);
			}

			AddUnmanagedResource (version_res);
		}

		public ModuleBuilder GetDynamicModule (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("Empty name is not legal.", "name");

			if (modules != null)
				for (int i = 0; i < modules.Length; ++i)
					if (modules [i].name == name)
						return modules [i];
			return null;
		}

		public override Type[] GetExportedTypes ()
		{
			throw not_supported ();
		}

		public override FileStream GetFile (string name)
		{
			throw not_supported ();
		}

		public override FileStream[] GetFiles(bool getResourceModules) {
			throw not_supported ();
		}

		internal override Module[] GetModulesInternal () {
			if (modules == null)
				return new Module [0];
			else
				return (Module[])modules.Clone ();
		}

		internal override Type[] GetTypes (bool exportedOnly) {
			Type[] res = null;
			if (modules != null) {
				for (int i = 0; i < modules.Length; ++i) {
					Type[] types = modules [i].GetTypes ();
					if (res == null)
						res = types;
					else {
						Type[] tmp = new Type [res.Length + types.Length];
						Array.Copy (res, 0, tmp, 0, res.Length);
						Array.Copy (types, 0, tmp, res.Length, types.Length);
					}
				}
			}
			if (loaded_modules != null) {
				for (int i = 0; i < loaded_modules.Length; ++i) {
					Type[] types = loaded_modules [i].GetTypes ();
					if (res == null)
						res = types;
					else {
						Type[] tmp = new Type [res.Length + types.Length];
						Array.Copy (res, 0, tmp, 0, res.Length);
						Array.Copy (types, 0, tmp, res.Length, types.Length);
					}
				}
			}

			return res == null ? Type.EmptyTypes : res;
		}

		public override ManifestResourceInfo GetManifestResourceInfo(string resourceName) {
			throw not_supported ();
		}

		public override string[] GetManifestResourceNames() {
			throw not_supported ();
		}

		public override Stream GetManifestResourceStream(string name) {
			throw not_supported ();
		}
		public override Stream GetManifestResourceStream(Type type, string name) {
			throw not_supported ();
		}

		/*
		 * This is set when the the AssemblyBuilder is created by (g)mcs
		 * or vbnc.
		 */
		internal bool IsCompilerContext
		{
			get { return is_compiler_context; }
		}

		internal bool IsSave {
			get {
				return access != (uint)AssemblyBuilderAccess.Run;
			}
		}

		internal string AssemblyDir {
			get {
				return dir;
			}
		}

		/*
		 * Mono extension. If this is set, the assembly can only contain one
		 * module, access should be Save, and the saved image will not contain an
		 * assembly manifest.
		 */
		internal bool IsModuleOnly {
			get {
				return is_module_only;
			}
			set {
				is_module_only = value;
			}
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		ModuleBuilder manifest_module;

		//
		// MS.NET seems to return a ModuleBuilder when GetManifestModule () is called
		// on an assemblybuilder.
		//
		internal override Module GetManifestModule () {
			if (manifest_module == null)
				manifest_module = DefineDynamicModule ("Default Dynamic Module");
			return manifest_module;
		}
#endif

#if NET_2_0
		public 
#else
		internal
#endif
		void Save (string assemblyFileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			this.peKind = portableExecutableKind;
			this.machine = imageFileMachine;

			if (resource_writers != null) {
				foreach (IResourceWriter writer in resource_writers) {
					writer.Generate ();
					writer.Close ();
				}
			}

			// Create a main module if not already created
			ModuleBuilder mainModule = null;
			if (modules != null) {
				foreach (ModuleBuilder module in modules)
					if (module.FullyQualifiedName == assemblyFileName)
						mainModule = module;
			}
			if (mainModule == null)
				mainModule = DefineDynamicModule ("RefEmit_OnDiskManifestModule", assemblyFileName);

			if (!is_module_only)
				mainModule.IsMain = true;

			/* 
			 * Create a new entry point if the one specified
			 * by the user is in another module.
			 */
			if ((entry_point != null) && entry_point.DeclaringType.Module != mainModule) {
				Type[] paramTypes;
				if (entry_point.GetParameters ().Length == 1)
					paramTypes = new Type [] { typeof (string) };
				else
					paramTypes = Type.EmptyTypes;

				MethodBuilder mb = mainModule.DefineGlobalMethod ("__EntryPoint$", MethodAttributes.Static|MethodAttributes.PrivateScope, entry_point.ReturnType, paramTypes);
				ILGenerator ilgen = mb.GetILGenerator ();
				if (paramTypes.Length == 1)
					ilgen.Emit (OpCodes.Ldarg_0);
				ilgen.Emit (OpCodes.Tailcall);
				ilgen.Emit (OpCodes.Call, entry_point);
				ilgen.Emit (OpCodes.Ret);

				entry_point = mb;
			}

			if (version_res != null)
				DefineVersionInfoResourceImpl (assemblyFileName);

			if (sn != null) {
				// runtime needs to value to embed it into the assembly
				public_key = sn.PublicKey;
			}

			foreach (ModuleBuilder module in modules)
				if (module != mainModule)
					module.Save ();

			// Write out the main module at the end, because it needs to
			// contain the hash of the other modules
			mainModule.Save ();

			if ((sn != null) && (sn.CanSign)) {
				sn.Sign (System.IO.Path.Combine (this.AssemblyDir, assemblyFileName));
			}

			created = true;
		}

		public void Save (string assemblyFileName)
		{
			Save (assemblyFileName, PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
		}

		public void SetEntryPoint (MethodInfo entryMethod)
		{
			SetEntryPoint (entryMethod, PEFileKinds.ConsoleApplication);
		}

		public void SetEntryPoint (MethodInfo entryMethod, PEFileKinds fileKind)
		{
			if (entryMethod == null)
				throw new ArgumentNullException ("entryMethod");
			if (entryMethod.DeclaringType.Assembly != this)
				throw new InvalidOperationException ("Entry method is not defined in the same assembly.");

			entry_point = entryMethod;
			pekind = fileKind;
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) 
		{
			if (customBuilder == null)
				throw new ArgumentNullException ("customBuilder");

			if (IsCompilerContext) {
				string attrname = customBuilder.Ctor.ReflectedType.FullName;
				byte [] data;
				int pos;

				if (attrname == "System.Reflection.AssemblyVersionAttribute") {
					version = create_assembly_version (customBuilder.string_arg ());
					return;
				} else if (attrname == "System.Reflection.AssemblyCultureAttribute") {
					culture = GetCultureString (customBuilder.string_arg ());
				} else if (attrname == "System.Reflection.AssemblyAlgorithmIdAttribute") {
					data = customBuilder.Data;
					pos = 2;
					algid = (uint) data [pos];
					algid |= ((uint) data [pos + 1]) << 8;
					algid |= ((uint) data [pos + 2]) << 16;
					algid |= ((uint) data [pos + 3]) << 24;
				} else if (attrname == "System.Reflection.AssemblyFlagsAttribute") {
					data = customBuilder.Data;
					pos = 2;
					flags |= (uint) data [pos];
					flags |= ((uint) data [pos + 1]) << 8;
					flags |= ((uint) data [pos + 2]) << 16;
					flags |= ((uint) data [pos + 3]) << 24;

#if NET_2_0
					// ignore PublicKey flag if assembly is not strongnamed
					if (sn == null)
						flags &= ~(uint) AssemblyNameFlags.PublicKey;
#endif
				}
			}

			if (cattrs != null) {
				CustomAttributeBuilder[] new_array = new CustomAttributeBuilder [cattrs.Length + 1];
				cattrs.CopyTo (new_array, 0);
				new_array [cattrs.Length] = customBuilder;
				cattrs = new_array;
			} else {
				cattrs = new CustomAttributeBuilder [1];
				cattrs [0] = customBuilder;
			}
		}

#if NET_2_0
		[ComVisible (true)]
#endif
		public void SetCustomAttribute ( ConstructorInfo con, byte[] binaryAttribute) {
			if (con == null)
				throw new ArgumentNullException ("con");
			if (binaryAttribute == null)
				throw new ArgumentNullException ("binaryAttribute");

			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		internal void SetCorlibTypeBuilders (Type corlib_object_type, Type corlib_value_type, Type corlib_enum_type) {
			this.corlib_object_type = corlib_object_type;
			this.corlib_value_type = corlib_value_type;
			this.corlib_enum_type = corlib_enum_type;
		}

		internal void SetCorlibTypeBuilders (Type corlib_object_type, Type corlib_value_type, Type corlib_enum_type, Type corlib_void_type)
		{
			SetCorlibTypeBuilders (corlib_object_type, corlib_value_type, corlib_enum_type);
			this.corlib_void_type = corlib_void_type;
		}

		private Exception not_supported () {
			// Strange message but this is what MS.NET prints...
			return new NotSupportedException ("The invoked member is not supported in a dynamic module.");
		}

		private void check_name_and_filename (string name, string fileName,
											  bool fileNeedsToExists) {
			if (name == null)
				throw new ArgumentNullException ("name");
			if (fileName == null)
				throw new ArgumentNullException ("fileName");
			if (name.Length == 0)
				throw new ArgumentException ("Empty name is not legal.", "name");
			if (fileName.Length == 0)
				throw new ArgumentException ("Empty file name is not legal.", "fileName");
			if (Path.GetFileName (fileName) != fileName)
				throw new ArgumentException ("fileName '" + fileName + "' must not include a path.", "fileName");

			// Resource files are created/searched under the assembly storage
			// directory
			string fullFileName = fileName;
			if (dir != null)
				fullFileName = Path.Combine (dir, fileName);

			if (fileNeedsToExists && !File.Exists (fullFileName))
				throw new FileNotFoundException ("Could not find file '" + fileName + "'");

			if (resources != null) {
				for (int i = 0; i < resources.Length; ++i) {
					if (resources [i].filename == fullFileName)
						throw new ArgumentException ("Duplicate file name '" + fileName + "'");
					if (resources [i].name == name)
						throw new ArgumentException ("Duplicate name '" + name + "'");
				}
			}

			if (modules != null) {
				for (int i = 0; i < modules.Length; ++i) {
					// Use fileName instead of fullFileName here
					if (!modules [i].IsTransient () && (modules [i].FileName == fileName))
						throw new ArgumentException ("Duplicate file name '" + fileName + "'");
					if (modules [i].Name == name)
						throw new ArgumentException ("Duplicate name '" + name + "'");
				}
			}
		}

		private String create_assembly_version (String version) {
			String[] parts = version.Split ('.');
			int[] ver = new int [4] { 0, 0, 0, 0 };

			if ((parts.Length < 0) || (parts.Length > 4))
				throw new ArgumentException ("The version specified '" + version + "' is invalid");

			for (int i = 0; i < parts.Length; ++i) {
				if (parts [i] == "*") {
					DateTime now = DateTime.Now;

					if (i == 2) {
						ver [2] = (now - new DateTime (2000, 1, 1)).Days;
						if (parts.Length == 3)
							ver [3] = (now.Second + (now.Minute * 60) + (now.Hour * 3600)) / 2;
					}
					else
						if (i == 3)
							ver [3] = (now.Second + (now.Minute * 60) + (now.Hour * 3600)) / 2;
					else
						throw new ArgumentException ("The version specified '" + version + "' is invalid");
				}
				else {
					try {
						ver [i] = Int32.Parse (parts [i]);
					}
					catch (FormatException) {
						throw new ArgumentException ("The version specified '" + version + "' is invalid");
					}
				}
			}

			return ver [0] + "." + ver [1] + "." + ver [2] + "." + ver [3];
		}

		private string GetCultureString (string str)
		{
			return (str == "neutral" ? String.Empty : str);
		}

		internal override AssemblyName UnprotectedGetName ()
		{
			AssemblyName an = base.UnprotectedGetName ();
			if (sn != null) {
				an.SetPublicKey (sn.PublicKey);
				an.SetPublicKeyToken (sn.PublicKeyToken);
			}
			return an;
		}

		void _AssemblyBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _AssemblyBuilder.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _AssemblyBuilder.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _AssemblyBuilder.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}

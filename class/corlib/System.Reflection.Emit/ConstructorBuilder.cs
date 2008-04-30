//
// System.Reflection.Emit/ConstructorBuilder.cs
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
using System.Reflection.Emit;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Diagnostics.SymbolStore;

namespace System.Reflection.Emit {

#if NET_2_0
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_ConstructorBuilder))]
#endif
	[ClassInterface (ClassInterfaceType.None)]
	public sealed class ConstructorBuilder : ConstructorInfo, _ConstructorBuilder {
		private RuntimeMethodHandle mhandle;
		private ILGenerator ilgen;
		private Type[] parameters;
		private MethodAttributes attrs;
		private MethodImplAttributes iattrs;
		private int table_idx;
		private CallingConventions call_conv;
		private TypeBuilder type;
		private ParameterBuilder[] pinfo;
		private CustomAttributeBuilder[] cattrs;
		private bool init_locals = true;
		private Type[][] paramModReq;
		private Type[][] paramModOpt;
		private RefEmitPermissionSet[] permissions;

		internal ConstructorBuilder (TypeBuilder tb, MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] paramModReq, Type[][] paramModOpt)
		{
			attrs = attributes | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
			call_conv = callingConvention;
			if (parameterTypes != null) {
				for (int i = 0; i < parameterTypes.Length; ++i)
					if (parameterTypes [i] == null)
						throw new ArgumentException ("Elements of the parameterTypes array cannot be null", "parameterTypes");

				this.parameters = new Type [parameterTypes.Length];
				System.Array.Copy (parameterTypes, this.parameters, parameterTypes.Length);
			}
			type = tb;
			this.paramModReq = paramModReq;
			this.paramModOpt = paramModOpt;
			table_idx = get_next_table_index (this, 0x06, true);

			((ModuleBuilder) tb.Module).RegisterToken (this, GetToken ().Token);
		}

#if NET_2_0
		[MonoTODO]
		public override CallingConventions CallingConvention {
			get {
				return call_conv;
			}
		}
#endif
		public bool InitLocals {
			get {
				return init_locals;
			}
			set {
				init_locals = value;
			}
		}

		internal TypeBuilder TypeBuilder {
			get {
				return type;
			}
		}
		
		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			return iattrs;
		}

		public override ParameterInfo[] GetParameters ()
		{
			if (!type.is_created && !IsCompilerContext)
				throw not_created ();

			if (parameters == null)
				return new ParameterInfo [0];

			ParameterInfo[] retval = new ParameterInfo [parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
				retval [i] = new ParameterInfo (pinfo == null ? null
					: pinfo [i+1], parameters [i], this, i + 1);

			return retval;
		}
		
		internal override int GetParameterCount ()
		{
			if (parameters == null)
				return 0;
			
			return parameters.Length;
		}
		
		public override Object Invoke (Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
		{
			throw not_supported ();
		}

		public override object Invoke (BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			throw not_supported ();
		}

		public override RuntimeMethodHandle MethodHandle {
			get {
				throw not_supported ();
			}
		}

		public override MethodAttributes Attributes {
			get {
				return attrs;
			}
		}

		public override Type ReflectedType {
			get {
				return type;
			}
		}

		public override Type DeclaringType {
			get {
				return type;
			}
		}

		public Type ReturnType {
			get {
				return null;
			}
		}

		public override string Name {
			get {
				return (attrs & MethodAttributes.Static) != 0 ? ConstructorInfo.TypeConstructorName : ConstructorInfo.ConstructorName;
			}
		}

		public string Signature {
			get {
				return "constructor signature";
			}
		}

		public void AddDeclarativeSecurity (SecurityAction action, PermissionSet pset)
		{
			if (pset == null)
				throw new ArgumentNullException ("pset");
			if ((action == SecurityAction.RequestMinimum) ||
				(action == SecurityAction.RequestOptional) ||
				(action == SecurityAction.RequestRefuse))
				throw new ArgumentOutOfRangeException ("Request* values are not permitted", "action");

			RejectIfCreated ();

			if (permissions != null) {
				/* Check duplicate actions */
				foreach (RefEmitPermissionSet set in permissions)
					if (set.action == action)
						throw new InvalidOperationException ("Multiple permission sets specified with the same SecurityAction.");

				RefEmitPermissionSet[] new_array = new RefEmitPermissionSet [permissions.Length + 1];
				permissions.CopyTo (new_array, 0);
				permissions = new_array;
			}
			else
				permissions = new RefEmitPermissionSet [1];

			permissions [permissions.Length - 1] = new RefEmitPermissionSet (action, pset.ToXml ().ToString ());
			attrs |= MethodAttributes.HasSecurity;
		}

		public ParameterBuilder DefineParameter (int iSequence, ParameterAttributes attributes, string strParamName)
		{
			if (iSequence < 1 || iSequence > GetParameterCount ())
				throw new ArgumentOutOfRangeException ("iSequence");
			if (type.is_created)
				throw not_after_created ();

			ParameterBuilder pb = new ParameterBuilder (this, iSequence, attributes, strParamName);
			if (pinfo == null)
				pinfo = new ParameterBuilder [parameters.Length + 1];
			pinfo [iSequence] = pb;
			return pb;
		}

		public override bool IsDefined (Type attribute_type, bool inherit)
		{
			throw not_supported ();
		}

		public override object [] GetCustomAttributes (bool inherit)
		{
			/*
			 * On MS.NET, this always returns not_supported, but we can't do this
			 * since there would be no way to obtain custom attributes of 
			 * dynamically created ctors.
			 */
			if (type.is_created && IsCompilerContext)
				return MonoCustomAttrs.GetCustomAttributes (this, inherit);
			else
				throw not_supported ();
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			if (type.is_created && IsCompilerContext)
				return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
			else
				throw not_supported ();
		}

		public ILGenerator GetILGenerator ()
		{
			return GetILGenerator (64);
		}

#if NET_2_0
		public
#else
		internal
#endif
		ILGenerator GetILGenerator (int size)
		{
			if (ilgen != null)
				return ilgen;
			ilgen = new ILGenerator (type.Module, ((ModuleBuilder)type.Module).GetTokenGenerator (), size);
			return ilgen;
		}

		public void SetCustomAttribute (CustomAttributeBuilder customBuilder)
		{
			if (customBuilder == null)
				throw new ArgumentNullException ("customBuilder");

			string attrname = customBuilder.Ctor.ReflectedType.FullName;
			if (attrname == "System.Runtime.CompilerServices.MethodImplAttribute") {
				byte[] data = customBuilder.Data;
				int impla; // the (stupid) ctor takes a short or an int ... 
				impla = (int)data [2];
				impla |= ((int)data [3]) << 8;
				SetImplementationFlags ((MethodImplAttributes)impla);
				return;
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
		public void SetCustomAttribute (ConstructorInfo con, byte[] binaryAttribute)
		{
			if (con == null)
				throw new ArgumentNullException ("con");
			if (binaryAttribute == null)
				throw new ArgumentNullException ("binaryAttribute");

			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		public void SetImplementationFlags (MethodImplAttributes attributes)
		{
			if (type.is_created)
				throw not_after_created ();

			iattrs = attributes;
		}

		public Module GetModule ()
		{
			return type.Module;
		}

		public MethodToken GetToken ()
		{
			return new MethodToken (0x06000000 | table_idx);
		}

		[MonoTODO]
		public void SetSymCustomAttribute (string name, byte[] data)
		{
			if (type.is_created)
				throw not_after_created ();
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public override Module Module {
			get {
				return base.Module;
			}
		}
#endif

		public override string ToString ()
		{
			return "ConstructorBuilder ['" + type.Name + "']";
		}

		internal void fixup ()
		{
			if (((attrs & (MethodAttributes.Abstract | MethodAttributes.PinvokeImpl)) == 0) && ((iattrs & (MethodImplAttributes.Runtime | MethodImplAttributes.InternalCall)) == 0)) {
			if ((ilgen == null) || (ILGenerator.Mono_GetCurrentOffset (ilgen) == 0))
				throw new InvalidOperationException ("Method '" + Name + "' does not have a method body.");
			}
			if (ilgen != null)
				ilgen.label_fixup ();
		}
		
		internal void GenerateDebugInfo (ISymbolWriter symbolWriter)
		{
			if (ilgen != null && ilgen.HasDebugInfo) {
				SymbolToken token = new SymbolToken (GetToken().Token);
				symbolWriter.OpenMethod (token);
				symbolWriter.SetSymAttribute (token, "__name", System.Text.Encoding.UTF8.GetBytes (Name));
				ilgen.GenerateDebugInfo (symbolWriter);
				symbolWriter.CloseMethod ();
			}
		}

		internal override int get_next_table_index (object obj, int table, bool inc)
		{
			return type.get_next_table_index (obj, table, inc);
		}

		private bool IsCompilerContext {
			get {
				ModuleBuilder mb = (ModuleBuilder) TypeBuilder.Module;
				AssemblyBuilder ab = (AssemblyBuilder) mb.Assembly;
				return ab.IsCompilerContext;
			}
		}

		private void RejectIfCreated ()
		{
			if (type.is_created)
				throw new InvalidOperationException ("Type definition of the method is complete.");
		}

		private Exception not_supported ()
		{
			return new NotSupportedException ("The invoked member is not supported in a dynamic module.");
		}

		private Exception not_after_created ()
		{
			return new InvalidOperationException ("Unable to change after type has been created.");
		}

		private Exception not_created ()
		{
#if NET_2_0
			return new NotSupportedException ("The type is not yet created.");
#else
			return new InvalidOperationException ("The type is not yet created.");
#endif
		}

		void _ConstructorBuilder.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _ConstructorBuilder.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _ConstructorBuilder.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _ConstructorBuilder.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}

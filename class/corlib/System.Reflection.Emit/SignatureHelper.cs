
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

//
// System.Reflection.Emit/SignatureHelper.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {
#if NET_2_0
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_SignatureHelper))]
#endif
	[ClassInterface (ClassInterfaceType.None)]
	public sealed class SignatureHelper : _SignatureHelper {
		internal enum SignatureHelperType {
			HELPER_FIELD,
			HELPER_LOCAL,
			HELPER_METHOD,
			HELPER_PROPERTY
		}

		private ModuleBuilder module; // can be null in 2.0
		private Type[] arguments;
		private SignatureHelperType type;
		private Type returnType;
		private CallingConventions callConv;
		private CallingConvention unmanagedCallConv;
		private Type[][] modreqs;
		private Type[][] modopts;

		internal SignatureHelper (ModuleBuilder module, SignatureHelperType type)
		{
			this.type = type;
			this.module = module;
		}

		public static SignatureHelper GetFieldSigHelper (Module mod)
		{
			if (mod != null && !(mod is ModuleBuilder))
				throw new ArgumentException ("ModuleBuilder is expected");

			return new SignatureHelper ((ModuleBuilder) mod, SignatureHelperType.HELPER_FIELD);
		}

		public static SignatureHelper GetLocalVarSigHelper (Module mod)
		{
			if (mod != null && !(mod is ModuleBuilder))
				throw new ArgumentException ("ModuleBuilder is expected");

			return new SignatureHelper ((ModuleBuilder) mod, SignatureHelperType.HELPER_LOCAL);
		}

#if NET_2_0
		public static SignatureHelper GetLocalVarSigHelper ()
		{
			return new SignatureHelper (null, SignatureHelperType.HELPER_LOCAL);
		}

		public static SignatureHelper GetMethodSigHelper(CallingConventions callingConvention, Type returnType)
		{
			return GetMethodSigHelper (null, callingConvention, (CallingConvention)0, returnType, null);
		}

		public static SignatureHelper GetMethodSigHelper (CallingConvention unmanagedCallingConvention,
								  Type returnType)
		{
			return GetMethodSigHelper (null, CallingConventions.Standard, unmanagedCallingConvention, returnType, null);
		}
#endif

		public static SignatureHelper GetMethodSigHelper( Module mod, CallingConventions callingConvention, Type returnType)
		{
			return GetMethodSigHelper (mod, callingConvention, (CallingConvention)0, returnType, null);
		}

		public static SignatureHelper GetMethodSigHelper( Module mod, CallingConvention unmanagedCallingConvention, Type returnType)
		{
			return GetMethodSigHelper (mod, CallingConventions.Standard, unmanagedCallingConvention, returnType, null);
		}

		public static SignatureHelper GetMethodSigHelper( Module mod, Type returnType, Type[] parameterTypes)
		{
			return GetMethodSigHelper (mod, CallingConventions.Standard, 
									   (CallingConvention)0, returnType, 
									   parameterTypes);
		}
		[MonoTODO("Not implemented")]
		public static SignatureHelper GetPropertySigHelper( Module mod, Type returnType, Type[] parameterTypes)
		{
			throw new NotImplementedException ();
		}

		//
		// Grows the given array, and returns the index where the element
		// was added
		//
		static int AppendArray (ref Type [] array, Type t)
		{
			if (array != null) {
				Type[] new_a = new Type [array.Length + 1];
				System.Array.Copy (array, new_a, array.Length);
				new_a [array.Length] = t;
				array = new_a;
				return array.Length;
			} else {
				array = new Type [1];
				array [0] = t;
				return 0;
			}
		}

#if NET_2_0
		//
		// Appends the given type array @t into the @array passed at
		// position @pos.   If there is no array, it gets created
		//
		// This allows adding data to a null array at position 5 for
		// example, creating 4 empty slots before the slot where @t
		// is stored.
		//
		//
		static void AppendArrayAt (ref Type [][] array, Type [] t, int pos)
		{
			int top = Math.Max (pos, array == null ? 0 : array.Length);
			Type[][] new_a = new Type [top+1][];
			if (array != null)
				System.Array.Copy (array, new_a, top);
			new_a [pos] = t;
			array = new_a;
		}
		
		static void ValidateParameterModifiers(string name, Type [] parameter_modifiers)
		{
			foreach (Type modifier in parameter_modifiers){
				if (modifier == null)
					throw new ArgumentNullException (name);
				if (modifier.IsArray)
					throw new ArgumentException (Locale.GetText ("Array type not permitted"), name);
				if (modifier.ContainsGenericParameters)
					throw new ArgumentException (Locale.GetText ("Open Generic Type not permitted"), name);
			}
		}

		static void ValidateCustomModifier (int n, Type [][] custom_modifiers, string name)
		{
			if (custom_modifiers == null)
				return;

			if (custom_modifiers.Length != n)
				throw new ArgumentException (
				     Locale.GetText (
				     	String.Format ("Custom modifiers length `{0}' does not match the size of the arguments")));
			
			for (int i = 0; i < n; i++)
			foreach (Type [] parameter_modifiers in custom_modifiers){
				if (parameter_modifiers == null)
					continue;

				ValidateParameterModifiers (name, parameter_modifiers);
			}
		}

		static Exception MissingFeature ()
		{
			throw new NotImplementedException ("Mono does not currently support setting modOpt/modReq through SignatureHelper");
		}

		[MonoTODO("Currently we ignore requiredCustomModifiers and optionalCustomModifiers")]
		public void AddArguments (Type[] arguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			if (arguments == null)
				throw new ArgumentNullException ("arguments");

			// For now
			if (requiredCustomModifiers != null || optionalCustomModifiers != null){
				throw MissingFeature();
			}
			
			ValidateCustomModifier (arguments.Length, requiredCustomModifiers, "requiredCustomModifiers");
			ValidateCustomModifier (arguments.Length, optionalCustomModifiers, "optionalCustomModifiers");

			for (int i = 0; i < arguments.Length; i++){
				AddArgument (arguments [i],
					     requiredCustomModifiers != null ? requiredCustomModifiers [i] : null,
					     optionalCustomModifiers != null ? optionalCustomModifiers [i] : null);
			}
		}

		[MonoTODO ("pinned is ignored")]
		public void AddArgument (Type argument, bool pinned)
		{
			AddArgument (argument);
		}

		[MonoTODO ("not implemented")]
		public void AddArgument (Type argument, Type [] requiredCustomModifiers, Type [] optionalCustomModifiers)
		{
			if (argument == null)
				throw new ArgumentNullException ("argument");

			ValidateParameterModifiers ("requiredCustomModifiers", requiredCustomModifiers);
			ValidateParameterModifiers ("optionalCustomModifiers", optionalCustomModifiers);

			int p = AppendArray (ref arguments, argument);
			if (requiredCustomModifiers != null)
				AppendArrayAt (ref modreqs, requiredCustomModifiers, p);
			if (optionalCustomModifiers != null)
				AppendArrayAt (ref modopts, optionalCustomModifiers, p);
		}

		[MonoTODO("Not implemented")]
		public static SignatureHelper GetPropertySigHelper (Module mod, Type returnType,
								    Type [] requiredReturnTypeCustomModifiers,
								    Type [] optionalReturnTypeCustomModifiers,
								    Type [] parameterTypes,
								    Type [] [] requiredParameterTypeCustomModifiers,
								    Type [] [] optionalParameterTypeCustomModifiers)
		{
			throw new NotImplementedException ();
		}
#endif

		public void AddArgument (Type clsArgument)
		{
			if (clsArgument == null)
				throw new ArgumentNullException ("argument");

			AppendArray (ref arguments, clsArgument);
		}

		[MonoTODO("Not implemented")]
		public void AddSentinel ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern byte[] get_signature_local ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern byte[] get_signature_field ();

		public byte[] GetSignature ()
		{
			switch (type) {
			case SignatureHelperType.HELPER_LOCAL:
				return get_signature_local ();
			case SignatureHelperType.HELPER_FIELD:
				return get_signature_field ();
			default:
				throw new NotImplementedException ();
			}
		}

		public override string ToString() {
			return "SignatureHelper";
		}

		internal static SignatureHelper GetMethodSigHelper( Module mod, CallingConventions callConv, CallingConvention unmanagedCallConv, Type returnType,
														   Type [] parameters)
		{
			if (mod != null && !(mod is ModuleBuilder))
				throw new ArgumentException ("ModuleBuilder is expected");

			SignatureHelper helper = 
				new SignatureHelper ((ModuleBuilder)mod, SignatureHelperType.HELPER_METHOD);
			helper.returnType = returnType;
			helper.callConv = callConv;
			helper.unmanagedCallConv = unmanagedCallConv;

			if (parameters != null) {
				helper.arguments = new Type [parameters.Length];
				for (int i = 0; i < parameters.Length; ++i)
					helper.arguments [i] = parameters [i];
			}

			return helper;
		}

                void _SignatureHelper.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
                {
                        throw new NotImplementedException ();
                }

                void _SignatureHelper.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
                {
                        throw new NotImplementedException ();
                }

                void _SignatureHelper.GetTypeInfoCount (out uint pcTInfo)
                {
                        throw new NotImplementedException ();
                }

                void _SignatureHelper.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
                {
                        throw new NotImplementedException ();
                }
	}
}


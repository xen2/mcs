//
// System.Reflection.Emit/MethodOnTypeBuilderInst.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Reflection;

#if NET_2_0

namespace System.Reflection.Emit
{
	/*
	 * This class represents a method of an instantiation of a generic type builder.
	 */
	internal class MethodOnTypeBuilderInst : MethodInfo
	{
		#region Keep in sync with object-internals.h
		MonoGenericClass instantiation;
		MethodBuilder mb;
		#endregion

		public MethodOnTypeBuilderInst (MonoGenericClass instantiation, MethodBuilder mb) {
			this.instantiation = instantiation;
			this.mb = mb;
		}

		//
		// MemberInfo members
		//
		
		public override Type DeclaringType {
			get {
				return instantiation;
			}
		}

		public override string Name {
			get {
				return mb.Name;
			}
		}

		public override Type ReflectedType {
			get {
				return instantiation;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit) {
			throw new NotSupportedException ();
		}

		public override object [] GetCustomAttributes (bool inherit) {
			throw new NotSupportedException ();
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit) {
			throw new NotSupportedException ();
		}

		//
		// MethodBase members
		//

		public override MethodImplAttributes GetMethodImplementationFlags() {
			return mb.GetMethodImplementationFlags ();
		}

		public override ParameterInfo[] GetParameters() {
			throw new NotImplementedException ();
		}

		internal override int GetParameterCount () {
			return mb.GetParameterCount ();
		}

		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			throw new NotImplementedException ();
		}

		public override RuntimeMethodHandle MethodHandle {
			get {
				throw new NotImplementedException ();
			}
		}

		public override MethodAttributes Attributes {
			get {
				return mb.Attributes;
			}
		}

		public override CallingConventions CallingConvention {
			get {
				return mb.CallingConvention;
			}
		}

		public override Type [] GetGenericArguments () {
			throw new NotImplementedException ();
		}

		public override bool ContainsGenericParameters {
			get {
				// FIXME:
				throw new NotImplementedException ();
			}
		}

		public override bool IsGenericMethodDefinition {
			get {
				return false;
			}
		}

		public override bool IsGenericMethod {
			get {
				return true;
			}
		}

		//
		// MethodInfo members
		//

		public override MethodInfo GetBaseDefinition() {
			throw new NotImplementedException ();
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

#endif

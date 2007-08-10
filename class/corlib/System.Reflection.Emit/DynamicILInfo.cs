//
// System.Reflection.Emit/DynamicILInfo.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[ComVisible (true)]
	public class DynamicILInfo {

		[MonoTODO]
		public DynamicMethod DynamicMethod { 
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int GetTokenFor (byte[] signature) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetTokenFor (DynamicMethod method) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetTokenFor (RuntimeFieldHandle field) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetTokenFor (RuntimeMethodHandle method) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetTokenFor (RuntimeTypeHandle type) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetTokenFor (string literal) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetTokenFor (RuntimeMethodHandle method, RuntimeTypeHandle contextType) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetCode (byte[] code, int maxStackSize) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[CLSCompliantAttribute(false)] 
		public unsafe void SetCode (byte* code, int codeSize, int maxStackSize) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetExceptions (byte[] exceptions) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[CLSCompliantAttribute(false)] 
		public unsafe void SetExceptions (byte* exceptions, int exceptionsSize) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetLocalSignature (byte[] localSignature) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[CLSCompliantAttribute(false)] 
		public unsafe void SetLocalSignature (byte* localSignature, int signatureSize) {
			throw new NotImplementedException ();
		}
	}
}

#endif

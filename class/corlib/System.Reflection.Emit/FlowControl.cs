// FlowControl.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com

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

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	/// <summary>
	///  Describes how an instruction alters the flow of control.
	/// </summary>
#if NET_2_0
	[ComVisible (true)]
	[Serializable]
#endif
	public enum FlowControl {

		/// <summary>
		/// Branch instruction (ex: br, leave).
		/// </summary>
		Branch = 0,

		/// <summary>
		///  Break instruction (ex: break).
		/// </summary>
		Break = 1,

		/// <summary>
		///  Call instruction (ex: jmp, call, callvirt).
		/// </summary>
		Call = 2,

		/// <summary>
		///  Conditional branch instruction (ex: brtrue, brfalse).
		/// </summary>
		Cond_Branch = 3,

		/// <summary>
		///  Changes the behaviour of or provides additional
		///  about a subsequent instruction. 
		///  (ex: prefixes such as volatile, unaligned).
		/// </summary>
		Meta = 4,

		/// <summary>
		///  Transition to the next instruction.
		/// </summary>
		Next = 5,

		/// <summary>
		///  Annotation for ann.phi instruction.
		/// </summary>
#if NET_2_0
		[Obsolete ("This API has been deprecated.")]
#endif
		Phi = 6,

		/// <summary>
		///  Return instruction.
		/// </summary>
		Return = 7,

		/// <summary>
		///  Throw instruction.
		/// </summary>
		Throw = 8
	}

}


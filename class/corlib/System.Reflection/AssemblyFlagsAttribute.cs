//
// System.Reflection.AssemblyFlagsAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
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

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyFlagsAttribute : Attribute
	{
		// Field
		private uint flags;
		
		// Constructor
		[CLSCompliant (false)]
		public AssemblyFlagsAttribute (uint flags)
		{
			this.flags = flags;
		}

#if NET_1_1
		public AssemblyFlagsAttribute (int flags)
		{
			this.flags = (uint)flags;
		}
#endif

		// Property
 		[CLSCompliant (false)]
		public uint Flags
		{
			get { return flags; }
		}

#if NET_1_1
		public int AssemblyFlags
		{
			get { return (int)flags; }
		}
#endif
	}
}

//
// ArrayConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// (C) 2005, Novell Inc. (http://novell.com)
//

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

namespace Microsoft.JScript {

	public class ArrayConstructor : ScriptFunction {

		internal static ArrayConstructor Ctr = new ArrayConstructor ();

		internal ArrayConstructor ()
		{
			_prototype = ArrayPrototype.Proto;
		}

		public ArrayObject ConstructArray (Object [] args)
		{
			throw new NotImplementedException ();
		}
		

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new ArrayObject CreateInstance (params Object [] args)
		{
			if (args == null || args.Length == 0)
				return new ArrayObject ();
			else if (args.Length == 1)
				return new ArrayObject (args [0]);
			else
				return new ArrayObject (args);
		}
		
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public ArrayObject Invoke (params Object [] args)
		{
			return CreateInstance (args);
		}
	}
}

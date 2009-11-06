//
// System.Runtime.Remoting.ActivatedServiceTypeEntry.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting {

	[System.Runtime.InteropServices.ComVisible (true)]
	public class ActivatedServiceTypeEntry : TypeEntry
	{
		Type obj_type;
		
		public ActivatedServiceTypeEntry (Type type)			
		{
			AssemblyName = type.Assembly.FullName;
			TypeName = type.FullName;
			obj_type = type;
		}

		public ActivatedServiceTypeEntry (string typeName, string assemblyName)
		{
			AssemblyName = assemblyName;
			TypeName = typeName;
			Assembly a = Assembly.Load (assemblyName);
			obj_type = a.GetType (typeName);
			if (obj_type == null) 
				throw new RemotingException ("Type not found: " + typeName + ", " + assemblyName);
		}
		
		public IContextAttribute [] ContextAttributes {
			get { return null; }
			set { } // This is not implemented in the MS runtime yet.
		}

		public Type ObjectType {
			get { return obj_type; }
		}

		public override string ToString ()
		{
			return AssemblyName + TypeName;
		}
	}
}

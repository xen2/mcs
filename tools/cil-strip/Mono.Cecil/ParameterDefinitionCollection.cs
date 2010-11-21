//
// ParameterDefinitionCollection.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Generated by /CodeGen/cecil-gen.rb do not edit
// Wed Sep 27 12:46:52 CEST 2006
//
// (C) 2005 Jb Evain
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

namespace Mono.Cecil {

	using System;
	using System.Collections;

	using Mono.Cecil.Cil;

	internal sealed class ParameterDefinitionCollection : CollectionBase, IReflectionVisitable {

		IMemberReference m_container;

		public ParameterDefinition this [int index] {
			get { return List [index] as ParameterDefinition; }
			set { List [index] = value; }
		}

		public IMemberReference Container {
			get { return m_container; }
		}

		public ParameterDefinitionCollection (IMemberReference container)
		{
			m_container = container;
		}

		public void Add (ParameterDefinition value)
		{
			List.Add (value);
		}

		public bool Contains (ParameterDefinition value)
		{
			return List.Contains (value);
		}

		public int IndexOf (ParameterDefinition value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, ParameterDefinition value)
		{
			List.Insert (index, value);
		}

		public void Remove (ParameterDefinition value)
		{
			List.Remove (value);
		}

		protected override void OnValidate (object o)
		{
			if (! (o is ParameterDefinition))
				throw new ArgumentException ("Must be of type " + typeof (ParameterDefinition).FullName);
		}

		public void Accept (IReflectionVisitor visitor)
		{
			visitor.VisitParameterDefinitionCollection (this);
		}
	}
}

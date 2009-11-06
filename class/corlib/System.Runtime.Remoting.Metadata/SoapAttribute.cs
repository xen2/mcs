//
// System.Runtime.Remoting.Metadata.SoapAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
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

namespace System.Runtime.Remoting.Metadata {
	[System.Runtime.InteropServices.ComVisible (true)]
	public class SoapAttribute : Attribute
	{
		bool _nested;
		bool _useAttribute;
		
		protected string ProtXmlNamespace;
		protected object ReflectInfo;
		
		public SoapAttribute ()
		{
		}

		public virtual bool Embedded {
			get {
				return _nested;
			}

			set {
				_nested = value;
			}
		}

		public virtual bool UseAttribute {
			get {
				return _useAttribute;
			}

			set {
				_useAttribute = value;
			}
		}

		public virtual string XmlNamespace {
			get {
				return ProtXmlNamespace;
			}

			set {
				ProtXmlNamespace = value;
			}
		}
		
		internal virtual void SetReflectionObject (object reflectionObject)
		{
			ReflectInfo = reflectionObject;
		}
	}
}

#if !NET_4_0
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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;

namespace System.Windows.Markup {

	[AttributeUsage (AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public sealed class ContentWrapperAttribute : Attribute
	{
		public ContentWrapperAttribute (Type contentWrapper)
		{
			this.contentWrapper = contentWrapper;
		}

		public Type ContentWrapper {
			get { return contentWrapper; }
		}

		public override object TypeId {
			get { return this; }
		}

		public override bool Equals (object obj)
		{
			if (obj is ContentWrapperAttribute) {
				if (((ContentWrapperAttribute)obj).ContentWrapper == contentWrapper)
					return true;
			}

			return false;
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		Type contentWrapper;
	}

}
#endif

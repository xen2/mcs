//
// System.Web.Configuration.HttpHandlerActionCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Configuration;

namespace System.Web.Configuration
{
	[ConfigurationCollection (typeof (HttpHandlerAction), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMapAlternate)]
	public sealed class HttpHandlerActionCollection : ConfigurationElementCollection
	{
		[MonoTODO]
		public HttpHandlerActionCollection ()
		{
		}
			
		[MonoTODO]
		public void Add (HttpHandlerAction httpHandlerAction)
		{
		}

		[MonoTODO]
		public void Clear ()
		{
		}

		[MonoTODO]
		protected override ConfigurationElement CreateNewElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override object GetElementKey (ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf (HttpHandlerAction action)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (string verb, string path)
		{
		}

		[MonoTODO]
		public void Remove (HttpHandlerAction action)
		{
		}

		[MonoTODO]
		public void RemoveAt (int index)
		{
		}

		[MonoTODO]
		protected override ConfigurationElementCollectionType CollectionType {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected override ConfigurationPropertyCollection Properties {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public HttpHandlerAction this[int index] {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		[MonoTODO]
		protected override bool ThrowOnDuplicate {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

#endif

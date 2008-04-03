//
// System.Net.WebRequest (for 2.1 profile)
//
// Authors:
//	Jb Evain  <jbevain@novell.com>
//
// (c) 2008 Novell, Inc. (http://www.novell.com)
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

#if NET_2_1

using System;
using System.IO;

namespace System.Net {

	public abstract class WebRequest {

		public abstract string ContentType { get; set; }
		public abstract WebHeaderCollection Headers { get; set; }
		public abstract string Method { get; set; }
		public abstract Uri RequestUri { get; }

		protected WebRequest ()
		{
		}

		public abstract void Abort();
		public abstract IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state);
		public abstract IAsyncResult BeginGetResponse (AsyncCallback callback, object state);
		public abstract Stream EndGetRequestStream (IAsyncResult asyncResult);
		public abstract WebResponse EndGetResponse (IAsyncResult asyncResult);

		[MonoTODO]
		public static WebRequest Create (Uri uri)
		{
			throw new NotImplementedException ("WebRequest::Create factory method");
		}

		[MonoTODO]
		public static bool RegisterPrefix (string prefix, IWebRequestCreate creator)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif

//
// System.Web.Configuration.TraceConfig
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Web;

namespace System.Web.Configuration {

	internal class TraceConfig {

		private bool enabled;
		private bool local_only;
		private bool page_output;
		private int request_limit;
		private TraceMode trace_mode;

		public TraceConfig ()
		{
			request_limit = 10;
		}

		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}

		public bool LocalOnly {
			get { return local_only; }
			set { local_only = value; }
		}

		public bool PageOutput {
			get { return page_output; }
			set { page_output = value; }
		}

		public int RequestLimit {
			get { return request_limit; }
			set { request_limit = value; }
		}
		
		public TraceMode TraceMode {
			get { return trace_mode; }
			set { trace_mode = value; }
		}		
	}
}


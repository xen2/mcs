//
// Mono.Data.PostgreSqlClient.PgSqlInfoMessageEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
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
using System.Data;

namespace Mono.Data.PostgreSqlClient
{
	public sealed class PgSqlInfoMessageEventArgs : EventArgs
	{
		[MonoTODO]
		public PgSqlErrorCollection Errors {
			get { 
				throw new NotImplementedException (); 
			}
		}	

		[MonoTODO]
		public string Message 
		{
			get { 
				throw new NotImplementedException (); 
			}
		}	

		[MonoTODO]
		public string Source {
			get { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public override string ToString() {
			// representation of InfoMessage event
			return "'ToString() for SqlInfoMessageEventArgs Not Implemented'";
		}

		//[MonoTODO]
		//~PgSqlInfoMessageEventArgs() {
			// FIXME: destructor needs to release resources
		//}
	}
}

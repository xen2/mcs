//
// System.Threading.AutoResetEvent.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//   Veronica De Santis (veron78@interfree.it)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004, 2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.CompilerServices;

namespace System.Threading 
{

 	public sealed class AutoResetEvent :
#if NET_2_0
	EventWaitHandle
#else
	WaitHandle 
#endif
	{
		// Constructor
#if NET_2_0
		public AutoResetEvent (bool initialState)
			: base(initialState, EventResetMode.AutoReset)
		{
		}
#else
		public AutoResetEvent(bool initialState) {
			bool created;
			
			Handle = NativeEventCalls.CreateEvent_internal(false,initialState,null, out created);
		}
#endif

		// Methods

/* Need BOOTSTRAP_NET_2_0 because System.Threading.Timer wants to use
 * the Set and Reset methods that have moved to EventWaitHandle in the
 * 2.0 profile
 */
#if ONLY_1_1 || BOOTSTRAP_NET_2_0
		public bool Set() {
			CheckDisposed ();
			
			return(NativeEventCalls.SetEvent_internal(Handle));
		}

		public bool Reset() {
			CheckDisposed ();
			
			return(NativeEventCalls.ResetEvent_internal(Handle));
		}
#endif
	}
}

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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//
// $Log: Message.cs,v $
// Revision 1.2  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// COMPLETE

using System; 
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace System.Windows.Forms {
	public struct Message {
		private int	msg;
		private IntPtr	hwnd;
		private IntPtr	lParam;
		private IntPtr	wParam;
		private IntPtr	result;

		#region Public Instance Properties
		public IntPtr HWnd {
			get { return hwnd; }
			set { hwnd=value; }
		}

		public IntPtr LParam {
			get { return lParam; }
			set { lParam=value; }
		}

		public int Msg {
			get { return msg; }
			set { msg=value; }
		}

		public IntPtr Result {
			get { return result; }
			set { result=value; }
		}

		public IntPtr WParam {
			get { return wParam; }
			set { wParam=value; }
		}
		#endregion	// Public Instance Properties

		#region Public Static Methods
		public static Message Create(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam) {
			Message new_message = new Message();

			new_message.msg=msg;
			new_message.hwnd=hWnd;
			new_message.wParam=wparam;
			new_message.lParam=lparam;
			return new_message;
		}
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public override bool Equals(object o) {
			if (!(o is Message)) {
				return false;
			}

			return ((this.msg == ((Message)o).msg) && 
				(this.hwnd == ((Message)o).hwnd) && 
				(this.lParam == ((Message)o).lParam) && 
				(this.wParam == ((Message)o).wParam) && 
				(this.result == ((Message)o).result));
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public object GetLParam(Type cls) {
			object o = Marshal.PtrToStructure(this.lParam, cls);
			
			return(o);
		}

		public override string ToString() {
			return String.Format("[{0} {1} {2} {3}]", msg.ToString(), lParam.ToString(), wParam.ToString(), hwnd.ToString());
		}
		#endregion	// Public Instance Methods
	}
}

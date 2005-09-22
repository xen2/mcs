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
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Drawing;


namespace System.Windows.Forms {

	public class StatusBarDrawItemEventArgs : DrawItemEventArgs {

		private StatusBarPanel panel;

		public StatusBarDrawItemEventArgs (Graphics g, Font font, Rectangle rect,
				int item, DrawItemState state, StatusBarPanel panel) :
		this (g, font, rect, item, state, panel, Control.DefaultForeColor,
				Control.DefaultBackColor)
		
		{
		}

		public StatusBarDrawItemEventArgs (Graphics g, Font font, Rectangle rect,
				int item, DrawItemState state, StatusBarPanel panel,
				Color foreColor, Color backColor) : base (g, font, rect,
						item, state)
		{
			this.panel = panel;
		}

		public StatusBarPanel Panel {
			get { return panel; }
		}
	}
}


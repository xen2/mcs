//
// System.Windows.Forms.StatusBarDrawItemEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
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
using System.Drawing;
namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the DrawItem event.
	/// </summary>
	public class StatusBarDrawItemEventArgs : DrawItemEventArgs {

		private StatusBarPanel panel;

		/// --- Constructor ---
		public StatusBarDrawItemEventArgs(Graphics g, Font font, 
			Rectangle r, int itemId, DrawItemState itemState, 
			StatusBarPanel panel, Color foreColor, Color backColor)
			: base(g, font, r, itemId, itemState, foreColor, backColor) {
			this.panel = panel;
		}

		public StatusBarDrawItemEventArgs(Graphics g, Font font, 
			Rectangle r, int itemId, DrawItemState itemState, StatusBarPanel panel)
			: base(g, font, r, itemId, itemState) {
			this.panel = panel;
		}
		
		#region Public Properties
		public StatusBarPanel Panel 
		{
			get {
				return panel;
			}
		}
		#endregion
	}
}

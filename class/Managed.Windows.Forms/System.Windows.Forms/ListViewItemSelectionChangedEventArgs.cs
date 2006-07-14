//
// ListViewItemSelectionChangedEventArgs.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
namespace System.Windows.Forms
{
	public class ListViewItemSelectionChangedEventArgs : EventArgs
	{
		private bool is_selected;
		private ListViewItem item;
		private int item_index;

		#region Public Constructors
		public ListViewItemSelectionChangedEventArgs (ListViewItem item, int itemIndex, bool isSelected) : base ()
		{
			this.item = item;
			this.item_index = itemIndex;
			this.is_selected = isSelected;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public ListViewItem Item {
			get { return this.item; }
		}
		
		public bool IsSelected {
			get { return this.is_selected; }
		}
		
		public int ItemIndex {
			get { return this.item_index; }
		}
		#endregion	// Public Instance Properties
	}
}
#endif
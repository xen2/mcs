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
//	Ravindra (rkumar@novell.com)
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: ColumnClickEventArgs.cs,v $
// Revision 1.1  2004/09/16 03:18:11  ravindra
// Event for ListView Control.
//
//
// COMPLETE
//

using System;

namespace System.Windows.Forms
{
	public class ColumnClickEventArgs : EventArgs
	{
		private int column;

		#region Public Constructors
		public ColumnClickEventArgs (int column)
		{
			this.column = column;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public int Column {
			get {
				return column;
			}
		}
		#endregion	// Public Instance Properties
	}
}

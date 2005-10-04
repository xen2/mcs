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


// COMPLETE

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {
	public class CreateParams {
		#region Local Variables
		private string	caption;
		private string	class_name;
		private int	class_style;
		private int	ex_style;
		private int	x;
		private int	y;
		private int	height;
		private int	width;
		private int	style;
		private object	param;
		private IntPtr	parent;
		#endregion 	// Local variables

		#region Public Constructors
		public CreateParams() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public string Caption {
			get { return caption; }
			set { caption = value; }
		}

		public string ClassName {
			get { return class_name; }
			set { class_name = value; }
		}

		public int ClassStyle {
			get { return class_style; }
			set { class_style = value; }
		}

		public int ExStyle {
			get { return ex_style; }
			set { ex_style = value; }
		}

		public int X {
			get { return x; }
			set { x = value; }
		}

		public int Y {
			get { return y; }
			set { y = value; }
		}

		public int Width {
			get { return width; }
			set { width = value; }
		}

		public int Height {
			get { return height; }
			set { height = value; }
		}

		public int Style {
			get { return style; }
			set { style = value; }
		}

		public object Param {
			get { return param; }
			set { param = value; }
		}

		public IntPtr Parent {
			get { return parent; }
			set { parent = value; }
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public override string ToString() {
			return "CreateParams {'" + class_name + 
				"', '" + caption + 
				"', " + String.Format("0x{0:X}", class_style) +
				", " + String.Format("0x{0:X}", ex_style) +
				", {" + String.Format("{0}", x) +
				", " + String.Format("{0}", y) +
				", " + String.Format("{0}", width) +
				", " + String.Format("{0}", height) +
				"}}";
 		}
		#endregion	// Public Instance Methods

	}
}

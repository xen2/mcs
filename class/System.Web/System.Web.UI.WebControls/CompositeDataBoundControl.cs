//
// System.Web.UI.WebControls.CompositeDataBoundControl.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Collections;

namespace System.Web.UI.WebControls
{
	public abstract class CompositeDataBoundControl : DataBoundControl, INamingContainer
	{
		protected CompositeDataBoundControl ()
		{
		}

		public override ControlCollection Controls
		{
			get {
				EnsureChildControls();
				return base.Controls;
			}
		}

		protected internal override void CreateChildControls ()
		{
			Controls.Clear ();

			object itemCount = ViewState ["_ItemCount"];
			if (itemCount != null) {
				RequiresDataBinding = false;
				object [] data = new object [(int) itemCount];
				ViewState ["_ItemCount"] = CreateChildControls (data, false);
			}
			else if (RequiresDataBinding)
				EnsureDataBound ();
		}
		
		protected internal override void PerformDataBinding (IEnumerable data)
		{
			Controls.Clear ();
			ViewState ["_ItemCount"] = CreateChildControls (data, true);
		}
		
		protected abstract int CreateChildControls (IEnumerable dataSource, bool dataBinding);
	}
}

#endif

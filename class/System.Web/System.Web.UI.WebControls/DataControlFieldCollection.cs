//
// System.Web.UI.WebControls.DataControlFieldCollection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Collections;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class DataControlFieldCollection: StateManagedCollection
	{
		public void Add (DataControlField field)
		{
			((IList)this).Add (field);
			OnInsertField (field);
		}
		
		public bool Contains (DataControlField field)
		{
			return ((IList)this).Contains (field);
		}
		
		public void CopyTo (DataControlField[] array, int index)
		{
			((IList)this).CopyTo (array, index);
		}
		
		public int IndexOf (DataControlField field)
		{
			return ((IList)this).IndexOf (field);
		}
		
		public void Insert (int index, DataControlField field)
		{
			((IList)this).Insert (index, field);
			OnInsertField (field);
		}
		
		public void Remove (DataControlField field)
		{
			((IList)this).Remove (field);
			OnRemoveField (field);
		}
		
		public void RemoveAt (int index)
		{
			DataControlField field = this [index];
			((IList)this).RemoveAt (index);
			OnRemoveField (field);
		}
		
		public DataControlField this [int i] {
			get { return (DataControlField) ((IList)this) [i]; }
		}
		
		void OnInsertField (DataControlField field)
		{
			field.FieldChanged += new EventHandler (OnFieldChanged);
			OnFieldsChanged ();
		}
		
		void OnRemoveField (DataControlField field)
		{
			field.FieldChanged -= new EventHandler (OnFieldChanged);
			OnFieldsChanged ();
		}
		
		void OnFieldChanged (object sender, EventArgs args)
		{
			OnFieldsChanged ();
		}
		
		void OnFieldsChanged ()
		{
			if (FieldsChanged != null) FieldsChanged (this, EventArgs.Empty);
		}
		
		protected override void SetDirtyObject (object o)
		{
			((DataControlField)o).SetDirty ();
		}

		protected override object CreateKnownType (int index)
		{
			return null;
		}
		
		public event EventHandler FieldsChanged;
	}
}

#endif

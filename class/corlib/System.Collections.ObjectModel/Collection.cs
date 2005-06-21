// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.ObjectModel.Collection
//
// Author:
//    Zoltan Varga (vargaz@gmail.com)
//    David Waite (mass@akuma.org)
//
// (C) 2005 Novell, Inc.
// (C) 2005 David Waite
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 David Waite
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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Collections.ObjectModel
{
	[ComVisible (false)]
	[Serializable]
	public class Collection <T> : IList <T>, ICollection <T>, IEnumerable <T>, IList, ICollection, IEnumerable
	{
		IList <T> list;
		object syncRoot;
		
		public Collection ()
		{
			List <T> l = new List <T> ();
			IList l2 = l as IList;
			syncRoot = l2.SyncRoot;
			list = l;
		}

		public Collection (IList <T> list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");
			this.list = list;
			ICollection l = list as ICollection;
			syncRoot = (l != null) ? l.SyncRoot : new object ();
		}

		public void Add (T item)
		{
			list.Add (item);			
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (T item)
		{
			return list.Contains (item);
		}

		public void CopyTo (T [] array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator <T> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (T item)
		{
			return list.IndexOf (item);
		}

		public void Insert (int index, T item)
		{
			list.Insert (index, item);
		}

		public bool Remove (T item)
		{
			return list.Remove (item);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		public virtual int Count {
			get { return list.Count; }
		}

		public virtual T this [int index] {
			get { return list [index]; }
			set { list [index] = value; }
		}

		public bool IsReadOnly {
			get { return list.IsReadOnly; }
		}
		
#region Helper methods for non-generic interfaces
		
		internal static bool IsValidItem (object item)
		{
			return (item is T || (item == null && ! typeof (T).IsValueType));
		}
		
		internal static T ConvertItem (object item)
		{
			if (IsValidItem (item))
				return (T)item;
			throw new ArgumentException ("item");
		}
		
		internal static void CheckWritable (IList <T> list)
		{
			if (list.IsReadOnly)
				throw new NotSupportedException ();
		}
		
		internal static bool IsSynchronized (IList <T> list)
		{
			ICollection c = list as ICollection;
			return (c != null) ? c.IsSynchronized : false;
		}
		
		internal static bool IsFixedSize (IList <T> list)
		{
			IList l = list as IList;
			return (l != null) ? l.IsFixedSize : false;
		}
#endregion

#region Not generic interface implementations
		void ICollection.CopyTo (Array array, int arrayIndex)
		{
			T [] target = array as T [];
			if (target == null)
				throw new ArgumentException ("array");
			list.CopyTo (target, arrayIndex);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator) list.GetEnumerator ();
		}
				
		int IList.Add (object item)
		{
			list.Add (ConvertItem (item));
			return list.Count - 1;
		}
		
		bool IList.Contains (object item)
		{
			if (IsValidItem (item))
				return list.Contains ((T) item);
			return false;
		}
		
		int IList.IndexOf (object item)
		{
			if (IsValidItem (item))
				return list.IndexOf ((T) item);
			return -1;
		}
		
		void IList.Insert (int index, object item)
		{
			list.Insert (index, ConvertItem (item));
		}
		
		void IList.Remove (object item)
		{
			CheckWritable (list);
			list.Remove (ConvertItem (item));
		}
		
		bool ICollection.IsSynchronized {
			get { return IsSynchronized (list); }
		}
		
		object ICollection.SyncRoot {
			get { return syncRoot; }
		}
		bool IList.IsFixedSize {
			get { return IsFixedSize (list); }
		}
		
		bool IList.IsReadOnly {
			get { return list.IsReadOnly; }
		}
		
		object IList.this [int index] {
			get { return list [index]; }
			set { list [index] = ConvertItem (value); }
		}
#endregion
	}
}
#endif

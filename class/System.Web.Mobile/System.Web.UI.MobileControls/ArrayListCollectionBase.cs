
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
/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ArrayListCollectionBase
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Collections;

namespace System.Web.UI.MobileControls
{
	public class ArrayListCollectionBase : ICollection, IEnumerable
	{
		private ArrayList items;
		internal ArrayListCollectionBase()
		{
		}

		internal ArrayListCollectionBase(ArrayList items)
		{
			this.items = items;
		}

		public int Count
		{
			get
			{
				return (items == null ? 0 : items.Count);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return (items == null ? false : items.IsReadOnly);
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		protected ArrayList Items
		{
			get
			{
				if(items == null)
					items = new ArrayList();
				return items;
			}
			set
			{
				items = value;
			}
		}

		public void CopyTo(Array array, int index)
		{
			Items.CopyTo(array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return Items.GetEnumerator();
		}
	}
}

//
// System.Text.RegularExpressions.GroupCollection
//
// Authors:
//	Dan Lewis (dlewis@gmx.co.uk)
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Dan Lewis
// (C) 2004 Novell, Inc.
//

using System;
using System.Collections;

namespace System.Text.RegularExpressions 
{
	[Serializable]
	public class GroupCollection: ICollection, IEnumerable
	{
		private ArrayList list;

		/* No public constructor */
		internal GroupCollection () {
			list = new ArrayList ();
		}

		public virtual int Count {
			get {
				return(list.Count);
			}
		}

		public bool IsReadOnly {
			get {
				return(true);
			}
		}

		public virtual bool IsSynchronized {
			get {
				return(false);
			}
		}

		public Group this[int i] {
			get {
				if (i < list.Count &&
				    i >= 0) {
					return((Group)list[i]);
				} else {
					return(new Group ());
				}
			}
		}

		public Group this[string groupName] {
			get {
				foreach (object o in list) {
					if (!(o is Match)) {
						continue;
					}

					int index = ((Match)o).Regex.GroupNumberFromName (groupName);

					if (index != -1) {
						return(this[index]);
					}
				}

				return(new Group ());
			}
		}

		public virtual object SyncRoot {
			get {
				return(list);
			}
		}

		public virtual void CopyTo (Array array, int index) {
			foreach (object o in list) {
				if (index > array.Length) {
					break;
				}

				array.SetValue (o, index++);
			}
		}

		public virtual IEnumerator GetEnumerator () {
			return(new Enumerator (list));
		}

		internal void Add (object o) {
			list.Add (o);
		}

		internal void Reverse () {
			list.Reverse ();
		}

		private class Enumerator: IEnumerator {
			private IList list;
			private int ptr;

			public Enumerator (IList list) {
				this.list = list;
				Reset ();
			}

			public object Current {
				get {
					if (ptr >= list.Count) {
						throw new InvalidOperationException ();
					}

					return(list[ptr]);
				}
			}

			public bool MoveNext () {
				if (ptr > list.Count) {
					throw new InvalidOperationException ();
				}

				return(++ptr < list.Count);
			}

			public void Reset () {
				ptr = -1;
			}
		}
	}
}

		
		

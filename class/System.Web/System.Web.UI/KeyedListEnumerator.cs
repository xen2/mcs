//
// System.Web.UI/KeyedListEnumerator.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

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

using System.Collections;

namespace System.Web.UI
{
	public class KeyedListEnumerator : IDictionaryEnumerator
	{
		private int index = -1;
		private ArrayList objs;

		internal KeyedListEnumerator (ArrayList list)
		{
			objs = list;
		}

		public bool MoveNext ()
		{
			index++;
			if (index >= objs.Count)
				return false;

			return true;
		}

		public void Reset ()
		{
			index = -1;
		}

		public object Current {
			get {
				if (index < 0 || index >= objs.Count)
					throw new InvalidOperationException ();

				return objs[index];
			}
		}

		public DictionaryEntry Entry {
			get {
				return (DictionaryEntry) Current;
			}
		}

		public object Key {
			get {
				return Entry.Key;
			}
		}

		public object Value {
			get {
				return Entry.Value;
			}
		}
	}
}

#endif

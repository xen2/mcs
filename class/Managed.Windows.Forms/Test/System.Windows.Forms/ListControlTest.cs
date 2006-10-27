//
// ListControlTest.cs: Tests for ListControl abstract class.
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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//

using System;
using System.Collections;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListControlTest
	{
		[Test]
		public void DisplayMemberNullTest ()
		{
			ListControlChild lc = new ListControlChild ();
			lc.DisplayMember = null;
			Assert.AreEqual (String.Empty, lc.DisplayMember, "#1");
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void DataSourceWrongArgumentType ()
		{
			ListControlChild lc = new ListControlChild ();
			lc.DataSource = new object ();
		}
	}

	public class ListControlChild : ListControl
	{
		public override int SelectedIndex {
			get {
				return -1;
			}
			set {
			}
		}

		protected override void RefreshItem (int index)
		{
		}

		protected override void SetItemsCore (IList items)
		{
		}
	}
}


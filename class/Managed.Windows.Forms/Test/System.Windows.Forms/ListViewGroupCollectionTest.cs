//
//  ListViewGroupCollectionTest.cs
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
// Copyright (c) 2006 Daniel Nauck
//
// Author:
//      Daniel Nauck    (dna(at)mono-project(dot)de)
//      Carlos Alberto Cortez <calberto.cortez@gmail.com>

#if NET_2_0

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListViewGroupCollectionTest
	{
		ListViewGroupCollection grpCol = null;
		ListView lv = null;

		[SetUp]
		public void SetUp()
		{
			lv = new ListView ();
			grpCol = lv.Groups;
		}

		[Test]
		public void DefaultProperties ()
		{
			Assert.AreEqual (false, ((IList)grpCol).IsReadOnly, "#A1");
            		Assert.AreEqual (false, ((IList)grpCol).IsFixedSize, "#A2");
			Assert.AreEqual (true, ((ICollection)grpCol).IsSynchronized, "#A3");
			Assert.AreEqual (grpCol, ((ICollection)grpCol).SyncRoot, "#A4");
			Assert.AreEqual (0, grpCol.Count, "#A5");
		}

		[Test]
		public void AddTest ()
		{
			ListViewGroup group1 = new ListViewGroup ("Item1");
			ListViewGroup group2 = new ListViewGroup ("Item2");
			grpCol.Add (group1);
			grpCol.Add (group2);

			Assert.AreEqual (2, grpCol.Count, "#B1");
			Assert.AreEqual (lv, group1.ListView, "#B2");
			Assert.AreEqual (lv, group2.ListView, "#B2");
		}

		[Test]
		public void ClearTest ()
		{
			ListViewGroup group1 = new ListViewGroup ("Item1");
			ListViewGroup group2 = new ListViewGroup ("Item2");
			grpCol.Add (group1);
			grpCol.Add (group2);
			grpCol.Clear ();

			Assert.AreEqual (0, grpCol.Count, "#C1");
			Assert.AreEqual (null, group1.ListView, "#C2");
			Assert.AreEqual (null, group2.ListView, "#C3");
		}

		[Test]
		public void ContainsTest ()
		{
			ListViewGroup obj = new ListViewGroup ("Item1");
			ListViewGroup obj2 = new ListViewGroup ("Item2");
			grpCol.Add (obj);
			Assert.AreEqual (true, grpCol.Contains (obj), "#D1");
			Assert.AreEqual (false, grpCol.Contains (obj2), "#D2");
		}

		[Test]
		public void IndexOfTest ()
		{
			ListViewGroup obj = new ListViewGroup ("Item1");
			ListViewGroup obj2 = new ListViewGroup ("Item2");
			grpCol.Add (obj);
			grpCol.Add (obj2);
			Assert.AreEqual (1, grpCol.IndexOf (obj2), "#E1");
		}

		[Test]
		public void RemoveTest ()
		{
			ListViewGroup obj = new ListViewGroup ("Item1");
			ListViewGroup obj2 = new ListViewGroup ("Item2");
			grpCol.Add (obj);
			grpCol.Add (obj2);
			grpCol.Remove (obj);
			Assert.AreEqual (1, grpCol.Count, "#F1");
			Assert.AreEqual (null, obj.ListView, "#F2");
			Assert.AreEqual (lv, obj2.ListView, "#F3");
		}

		[Test]
		public void RemoveAtTest ()
		{
			ListViewGroup obj = new ListViewGroup ("Item1");
			ListViewGroup obj2 = new ListViewGroup ("Item2");
			grpCol.Add (obj);
			grpCol.Add (obj2);
			grpCol.RemoveAt (0);
			Assert.AreEqual (1, grpCol.Count, "#G1");
			Assert.AreEqual (true, grpCol.Contains (obj2), "#G2");
			Assert.AreEqual (null, obj.ListView, "#G3");
			Assert.AreEqual (lv, obj2.ListView, "#G4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IndexerOutOfRangeTest ()
		{
			grpCol.Add (new ListViewGroup ("Item1"));
			grpCol[10] = null;
		}

		[Test]
        	public void IndexerOutOfRangeTest2()
		{   //.NET 2.0 don't throw a exception here
			grpCol.Add (new ListViewGroup ("Item1"));
			grpCol["TestItemThatDoesNotExist"] = null;
		}
	}
}
#endif

//
// System.Drawing.RegionData unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class RegionDataTest {

		private Bitmap bitmap;
		private Graphics graphic;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			bitmap = new Bitmap (10, 10);
			graphic = Graphics.FromImage (bitmap);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void RegionData_Null ()
		{
			RegionData data = new Region ().GetRegionData ();
			data.Data = null;
			Assert.IsNull (data.Data, "Data");
			Region region = new Region (data);
		}

		[Test]
		public void RegionData_EmptyData ()
		{
			RegionData data = new Region ().GetRegionData ();
			data.Data = new byte[0];
			Assert.AreEqual (0, data.Data.Length, "Data");
			try {
				new Region (data);
			}
			catch (ExternalException) {
				// MS
			}
			catch (ArgumentException) {
				// Mono
			}
		}

		[Test]
		public void EmptyRegion ()
		{
			// note: an empty region is (for libgdiplus) a rectangular based region
			Region empty = new Region ();
			RegionData data = empty.GetRegionData ();
			Assert.IsNotNull (data.Data, "Data");
			Region region = new Region (data);
		}

		[Test]
		public void PathRegion ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddCurve (new Point[2] { new Point (1, 1), new Point (2, 2) });
			Region r = new Region (path);
			RegionData data = r.GetRegionData ();
			Assert.IsNotNull (data.Data, "Data");
			Region region = new Region (data);
			Assert.IsTrue (r.GetBounds (graphic).Equals (region.GetBounds (graphic)), "Bounds");
		}
	}
}

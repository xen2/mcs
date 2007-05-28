//
// Graphics class testing unit
//
// Authors:
//   Jordi Mas, jordi@ximian.com
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	public class GraphicsTest {

		private RectangleF[] rects;
		private Font font;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			try {
				font = new Font ("Arial", 12);
			}
			catch {
			}
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			if (font != null)
				font.Dispose ();
		}
		

		private bool IsEmptyBitmap (Bitmap bitmap, out int x, out int y)
		{
			bool result = true;
			int empty = Color.Empty.ToArgb ();
#if false
			for (y = 0; y < bitmap.Height; y++) {
				for (x = 0; x < bitmap.Width; x++) {
					if (bitmap.GetPixel (x, y).ToArgb () != empty) {
						Console.Write ("X");
						result = false;
					} else
						Console.Write (" ");
				}
				Console.WriteLine ();
			}
#else
			for (y = 0; y < bitmap.Height; y++) {
				for (x = 0; x < bitmap.Width; x++) {
					if (bitmap.GetPixel (x, y).ToArgb () != empty)
						return false;
				}
			}
#endif
			x = -1;
			y = -1;
			return result;
		}

		private void CheckForEmptyBitmap (Bitmap bitmap)
		{
			int x, y;
			if (!IsEmptyBitmap (bitmap, out x, out y))
				Assert.Fail (String.Format ("Position {0},{1}", x, y));
		}

		private void CheckForNonEmptyBitmap (Bitmap bitmap)
		{
			int x, y;
			if (IsEmptyBitmap (bitmap, out x, out y))
				Assert.Fail ("Bitmap was empty");
		}

		private void AssertEquals (string msg, object expected, object actual)
		{
			Assert.AreEqual (expected, actual, msg);
		}

		private void AssertEquals (string msg, double expected, double actual, double delta)
		{
			Assert.AreEqual (expected, actual, delta, msg);
		}

		[Test]
		public void DefaultProperties ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			Region r = new Region ();

			AssertEquals ("DefaultProperties1", r.GetBounds (g) , g.ClipBounds);
			AssertEquals ("DefaultProperties2", CompositingMode.SourceOver, g.CompositingMode);
			AssertEquals ("DefaultProperties3", CompositingQuality.Default, g.CompositingQuality);
			AssertEquals ("DefaultProperties4", InterpolationMode.Bilinear, g.InterpolationMode);
			AssertEquals ("DefaultProperties5", 1, g.PageScale);
			AssertEquals ("DefaultProperties6", GraphicsUnit.Display, g.PageUnit);
			AssertEquals ("DefaultProperties7", PixelOffsetMode.Default, g.PixelOffsetMode);
			AssertEquals ("DefaultProperties8", new Point (0, 0) , g.RenderingOrigin);
			AssertEquals ("DefaultProperties9", SmoothingMode.None, g.SmoothingMode);
			AssertEquals ("DefaultProperties10", TextRenderingHint.SystemDefault, g.TextRenderingHint);

			r.Dispose ();
		}
		
		[Test]
		public void SetGetProperties ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);						
			
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.GammaCorrected;
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			g.PageScale = 2;
			g.PageUnit = GraphicsUnit.Inch;			
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (10, 20);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.SystemDefault;

			//Clipping set/get tested in clipping functions			
			AssertEquals ("SetGetProperties2", CompositingMode.SourceCopy, g.CompositingMode);
			AssertEquals ("SetGetProperties3", CompositingQuality.GammaCorrected, g.CompositingQuality);
			AssertEquals ("SetGetProperties4", InterpolationMode.HighQualityBilinear, g.InterpolationMode);
			AssertEquals ("SetGetProperties5", 2, g.PageScale);
			AssertEquals ("SetGetProperties6", GraphicsUnit.Inch, g.PageUnit);
			AssertEquals ("SetGetProperties7", PixelOffsetMode.Half, g.PixelOffsetMode);
			AssertEquals ("SetGetProperties8", new Point (10, 20), g.RenderingOrigin);
			AssertEquals ("SetGetProperties9", SmoothingMode.AntiAlias, g.SmoothingMode);
			AssertEquals ("SetGetProperties10", TextRenderingHint.SystemDefault, g.TextRenderingHint);			
		}

		// Properties
		[Test]
		public void Clip ()
		{
			RectangleF[] rects ;
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			g.Clip = new Region (new Rectangle (50, 40, 210, 220));
			rects = g.Clip.GetRegionScans (new Matrix ());

			AssertEquals ("Clip1", 1, rects.Length);
			AssertEquals ("Clip2", 50, rects[0].X);
			AssertEquals ("Clip3", 40, rects[0].Y);
			AssertEquals ("Clip4", 210, rects[0].Width);
			AssertEquals ("Clip5", 220, rects[0].Height);
		}

		[Test]
		public void Clip_NotAReference ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			Assert.IsTrue (g.Clip.IsInfinite (g), "IsInfinite");
			g.Clip.IsEmpty (g);
			Assert.IsFalse (g.Clip.IsEmpty (g), "!IsEmpty");
			Assert.IsTrue (g.Clip.IsInfinite (g), "IsInfinite-2");
		}

		[Test]
		public void ExcludeClip ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			g.Clip = new Region (new RectangleF (10, 10, 100, 100));
			g.ExcludeClip (new Rectangle (40, 60, 100, 20));
			rects = g.Clip.GetRegionScans (new Matrix ());

			AssertEquals ("ExcludeClip1", 3, rects.Length);

			AssertEquals ("ExcludeClip2", 10, rects[0].X);
			AssertEquals ("ExcludeClip3", 10, rects[0].Y);
			AssertEquals ("ExcludeClip4", 100, rects[0].Width);
			AssertEquals ("ExcludeClip5", 50, rects[0].Height);

			AssertEquals ("ExcludeClip6", 10, rects[1].X);
			AssertEquals ("ExcludeClip7", 60, rects[1].Y);
			AssertEquals ("ExcludeClip8", 30, rects[1].Width);
			AssertEquals ("ExcludeClip9", 20, rects[1].Height);

			AssertEquals ("ExcludeClip10", 10, rects[2].X);
			AssertEquals ("ExcludeClip11", 80, rects[2].Y);
			AssertEquals ("ExcludeClip12", 100, rects[2].Width);
			AssertEquals ("ExcludeClip13", 30, rects[2].Height);
		}

		[Test]
		public void IntersectClip ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			g.Clip = new Region (new RectangleF (260, 30, 60, 80));
			g.IntersectClip (new Rectangle (290, 40, 60, 80));
			rects = g.Clip.GetRegionScans (new Matrix ());

			AssertEquals ("IntersectClip", 1, rects.Length);

			AssertEquals ("IntersectClip", 290, rects[0].X);
			AssertEquals ("IntersectClip", 40, rects[0].Y);
			AssertEquals ("IntersectClip", 30, rects[0].Width);
			AssertEquals ("IntersectClip", 70, rects[0].Height);
		}

		[Test]
		public void ResetClip ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			g.Clip = new Region (new RectangleF (260, 30, 60, 80));
			g.IntersectClip (new Rectangle (290, 40, 60, 80));
			g.ResetClip ();
			rects = g.Clip.GetRegionScans (new Matrix ());

			AssertEquals ("ResetClip", 1, rects.Length);

			AssertEquals ("ResetClip", -4194304, rects[0].X);
			AssertEquals ("ResetClip", -4194304, rects[0].Y);
			AssertEquals ("ResetClip", 8388608, rects[0].Width);
			AssertEquals ("ResetClip", 8388608, rects[0].Height);
		}

		[Test]
		public void SetClip ()
		{
			RectangleF[] rects ;
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			// Region
			g.SetClip (new Region (new Rectangle (50, 40, 210, 220)), CombineMode.Replace);
			rects = g.Clip.GetRegionScans (new Matrix ());
			AssertEquals ("SetClip1", 1, rects.Length);
			AssertEquals ("SetClip2", 50, rects[0].X);
			AssertEquals ("SetClip3", 40, rects[0].Y);
			AssertEquals ("SetClip4", 210, rects[0].Width);
			AssertEquals ("SetClip5", 220, rects[0].Height);

			// RectangleF
			g = Graphics.FromImage (bmp);
			g.SetClip (new RectangleF (50, 40, 210, 220));
			rects = g.Clip.GetRegionScans (new Matrix ());
			AssertEquals ("SetClip6", 1, rects.Length);
			AssertEquals ("SetClip7", 50, rects[0].X);
			AssertEquals ("SetClip8", 40, rects[0].Y);
			AssertEquals ("SetClip9", 210, rects[0].Width);
			AssertEquals ("SetClip10", 220, rects[0].Height);

			// Rectangle
			g = Graphics.FromImage (bmp);
			g.SetClip (new Rectangle (50, 40, 210, 220));
			rects = g.Clip.GetRegionScans (new Matrix ());
			AssertEquals ("SetClip10", 1, rects.Length);
			AssertEquals ("SetClip11", 50, rects[0].X);
			AssertEquals ("SetClip12", 40, rects[0].Y);
			AssertEquals ("SetClip13", 210, rects[0].Width);
			AssertEquals ("SetClip14", 220, rects[0].Height);
		}
		
		[Test]
		public void SetSaveReset ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			GraphicsState state_default, state_modified;
			
			state_default = g.Save (); // Default
			
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.GammaCorrected;
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			g.PageScale = 2;
			g.PageUnit = GraphicsUnit.Inch;			
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.Clip = new Region (new Rectangle (0, 0, 100, 100));
			g.RenderingOrigin = new Point (10, 20);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			
			
			state_modified = g.Save (); // Modified
			
			g.CompositingMode = CompositingMode.SourceOver;
			g.CompositingQuality = CompositingQuality.Default;
			g.InterpolationMode = InterpolationMode.Bilinear;
			g.PageScale = 5;
			g.PageUnit = GraphicsUnit.Display;			
			g.PixelOffsetMode = PixelOffsetMode.Default;
			g.Clip = new Region (new Rectangle (1, 2, 20, 25));
			g.RenderingOrigin = new Point (5, 6);
			g.SmoothingMode = SmoothingMode.None;
			g.TextRenderingHint = TextRenderingHint.SystemDefault;			
						
			g.Restore (state_modified);
			
			AssertEquals ("SetSaveReset1", CompositingMode.SourceCopy, g.CompositingMode);
			AssertEquals ("SetSaveReset2", CompositingQuality.GammaCorrected, g.CompositingQuality);
			AssertEquals ("SetSaveReset3", InterpolationMode.HighQualityBilinear, g.InterpolationMode);
			AssertEquals ("SetSaveReset4", 2, g.PageScale);
			AssertEquals ("SetSaveReset5", GraphicsUnit.Inch, g.PageUnit);
			AssertEquals ("SetSaveReset6", PixelOffsetMode.Half, g.PixelOffsetMode);
			AssertEquals ("SetSaveReset7", new Point (10, 20), g.RenderingOrigin);
			AssertEquals ("SetSaveReset8", SmoothingMode.AntiAlias, g.SmoothingMode);
			AssertEquals ("SetSaveReset9", TextRenderingHint.ClearTypeGridFit, g.TextRenderingHint);			
			AssertEquals ("SetSaveReset10", 0, (int) g.ClipBounds.X);
			AssertEquals ("SetSaveReset10", 0, (int) g.ClipBounds.Y);
			
			g.Restore (state_default);			
			
			AssertEquals ("SetSaveReset11", CompositingMode.SourceOver, g.CompositingMode);
			AssertEquals ("SetSaveReset12", CompositingQuality.Default, g.CompositingQuality);
			AssertEquals ("SetSaveReset13", InterpolationMode.Bilinear, g.InterpolationMode);
			AssertEquals ("SetSaveReset14", 1, g.PageScale);
			AssertEquals ("SetSaveReset15", GraphicsUnit.Display, g.PageUnit);
			AssertEquals ("SetSaveReset16", PixelOffsetMode.Default, g.PixelOffsetMode);
			AssertEquals ("SetSaveReset17", new Point (0, 0) , g.RenderingOrigin);
			AssertEquals ("SetSaveReset18", SmoothingMode.None, g.SmoothingMode);
			AssertEquals ("SetSaveReset19", TextRenderingHint.SystemDefault, g.TextRenderingHint);		

			Region r = new Region ();
			AssertEquals ("SetSaveReset20", r.GetBounds (g) , g.ClipBounds);
			
			g.Dispose ();			
		}

		[Test]
		[Category ("NotWorking")] // looks like MS PNG codec promote indexed format to 32bpp ARGB
		public void LoadIndexed_PngStream ()
		{
			// Tests that we can load an indexed file
			using (Stream s = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("indexed.png")) {
				using (Image img = Image.FromStream (s)) {
					// however it's no more indexed once loaded
					Assert.AreEqual (PixelFormat.Format32bppArgb, img.PixelFormat, "PixelFormat");
					using (Graphics g = Graphics.FromImage (img)) {
						Assert.AreEqual (img.Height, g.VisibleClipBounds.Height, "Height");
						Assert.AreEqual (img.Width, g.VisibleClipBounds.Width, "Width");
					}
				}
			}
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void LoadIndexed_BmpFile ()
		{
			// Tests that we can load an indexed file, but...
			string sInFile = TestBitmap.getInFile ("bitmaps/almogaver1bit.bmp");
			// note: file is misnamed (it's a 4bpp bitmap)
			using (Image img = Image.FromFile (sInFile)) {
				Assert.AreEqual (PixelFormat.Format4bppIndexed, img.PixelFormat, "PixelFormat");
				Graphics.FromImage (img);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromImage ()
		{
			Graphics g = Graphics.FromImage (null);
		}

		private Graphics Get (int w, int h)
		{
			Bitmap bitmap = new Bitmap (w, h);
			Graphics g = Graphics.FromImage (bitmap);
			g.Clip = new Region (new Rectangle (0, 0, w, h));
			return g;
		}

		private void Compare (string msg, RectangleF b1, RectangleF b2)
		{
			AssertEquals (msg + ".compare.X", b1.X, b2.X);
			AssertEquals (msg + ".compare.Y", b1.Y, b2.Y);
			AssertEquals (msg + ".compare.Width", b1.Width, b2.Width);
			AssertEquals (msg + ".compare.Height", b1.Height, b2.Height);
		}

		[Test]
		public void Clip_GetBounds ()
		{
			Graphics g = Get (16, 16);
			RectangleF bounds = g.Clip.GetBounds (g);
			AssertEquals ("X", 0, bounds.X);
			AssertEquals ("Y", 0, bounds.Y);
			AssertEquals ("Width", 16, bounds.Width);
			AssertEquals ("Height", 16, bounds.Height);
			Assert.IsTrue (g.Transform.IsIdentity, "Identity");
			g.Dispose ();
		}

		[Test]
		public void Clip_TranslateTransform ()
		{
			Graphics g = Get (16, 16);
			g.TranslateTransform (12.22f, 10.10f);
			RectangleF bounds = g.Clip.GetBounds (g);
			Compare ("translate", bounds, g.ClipBounds);
			AssertEquals ("translate.X", -12.22, bounds.X);
			AssertEquals ("translate.Y", -10.10, bounds.Y);
			AssertEquals ("translate.Width", 16, bounds.Width);
			AssertEquals ("translate.Height", 16, bounds.Height);
			float[] elements = g.Transform.Elements;
			AssertEquals ("translate.0", 1, elements[0]);
			AssertEquals ("translate.1", 0, elements[1]);
			AssertEquals ("translate.2", 0, elements[2]);
			AssertEquals ("translate.3", 1, elements[3]);
			AssertEquals ("translate.4", 12.22, elements[4]);
			AssertEquals ("translate.5", 10.10, elements[5]);

			g.ResetTransform ();
			bounds = g.Clip.GetBounds (g);
			Compare ("reset", bounds, g.ClipBounds);
			AssertEquals ("reset.X", 0, bounds.X);
			AssertEquals ("reset.Y", 0, bounds.Y);
			AssertEquals ("reset.Width", 16, bounds.Width);
			AssertEquals ("reset.Height", 16, bounds.Height);
			Assert.IsTrue (g.Transform.IsIdentity, "Identity");
			g.Dispose ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Transform_NonInvertibleMatrix ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			Assert.IsFalse (matrix.IsInvertible, "IsInvertible");
			Graphics g = Get (16, 16);
			g.Transform = matrix;
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Multiply_NonInvertibleMatrix ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			Assert.IsFalse (matrix.IsInvertible, "IsInvertible");
			Graphics g = Get (16, 16);
			g.MultiplyTransform (matrix);
		}

		private void CheckBounds (string msg, RectangleF bounds, float x, float y, float w, float h)
		{
			AssertEquals (msg + ".X", x, bounds.X, 0.1);
			AssertEquals (msg + ".Y", y, bounds.Y, 0.1);
			AssertEquals (msg + ".Width", w, bounds.Width, 0.1);
			AssertEquals (msg + ".Height", h, bounds.Height, 0.1);
		}

		[Test]
		public void ClipBounds ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);

			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			CheckBounds ("clip.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("clip.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Rotate ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.RotateTransform (90);
			CheckBounds ("rotate.ClipBounds", g.ClipBounds, 0, -8, 8, 8);
			CheckBounds ("rotate.Clip.GetBounds", g.Clip.GetBounds (g), 0, -8, 8, 8);

			g.Transform = new Matrix ();
			CheckBounds ("identity.ClipBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
			CheckBounds ("identity.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Scale ()
		{
			RectangleF clip = new Rectangle (0, 0, 8, 8);
			Graphics g = Get (16, 16);
			g.Clip = new Region (clip);
			g.ScaleTransform (0.25f, 0.5f);
			CheckBounds ("scale.ClipBounds", g.ClipBounds, 0, 0, 32, 16);
			CheckBounds ("scale.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 32, 16);

			g.SetClip (clip);
			CheckBounds ("setclip.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("setclip.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Translate ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			Region clone = g.Clip.Clone ();
			g.TranslateTransform (8, 8);
			CheckBounds ("translate.ClipBounds", g.ClipBounds, -8, -8, 8, 8);
			CheckBounds ("translate.Clip.GetBounds", g.Clip.GetBounds (g), -8, -8, 8, 8);

			g.SetClip (clone, CombineMode.Replace);
			CheckBounds ("setclip.ClipBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
			CheckBounds ("setclip.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Transform_Translation ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.Transform = new Matrix (1, 0, 0, 1, 8, 8);
			CheckBounds ("transform.ClipBounds", g.ClipBounds, -8, -8, 8, 8);
			CheckBounds ("transform.Clip.GetBounds", g.Clip.GetBounds (g), -8, -8, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reset.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("reset.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Transform_Scale ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.Transform = new Matrix (0.5f, 0, 0, 0.25f, 0, 0);
			CheckBounds ("scale.ClipBounds", g.ClipBounds, 0, 0, 16, 32);
			CheckBounds ("scale.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 32);

			g.ResetClip ();
			// see next test for ClipBounds
			CheckBounds ("resetclip.Clip.GetBounds", g.Clip.GetBounds (g), -4194304, -4194304, 8388608, 8388608);
			Assert.IsTrue (g.Clip.IsInfinite (g), "IsInfinite");
		}

		[Test]
		[Category ("NotWorking")]
		public void ClipBounds_Transform_Scale_Strange ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.Transform = new Matrix (0.5f, 0, 0, 0.25f, 0, 0);
			CheckBounds ("scale.ClipBounds", g.ClipBounds, 0, 0, 16, 32);
			CheckBounds ("scale.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 32);

			g.ResetClip ();
			// note: strange case where g.ClipBounds and g.Clip.GetBounds are different
			CheckBounds ("resetclip.ClipBounds", g.ClipBounds, -8388608, -16777216, 16777216, 33554432);
		}

		[Test]
		public void ClipBounds_Multiply ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.Transform = new Matrix (1, 0, 0, 1, 8, 8);
			g.MultiplyTransform (g.Transform);
			CheckBounds ("multiply.ClipBounds", g.ClipBounds, -16, -16, 8, 8);
			CheckBounds ("multiply.Clip.GetBounds", g.Clip.GetBounds (g), -16, -16, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reset.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("reset.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Cumulative_Effects ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);

			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			CheckBounds ("clip.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("clip.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.RotateTransform (90);
			CheckBounds ("rotate.ClipBounds", g.ClipBounds, 0, -8, 8, 8);
			CheckBounds ("rotate.Clip.GetBounds", g.Clip.GetBounds (g), 0, -8, 8, 8);

			g.ScaleTransform (0.25f, 0.5f);
			CheckBounds ("scale.ClipBounds", g.ClipBounds, 0, -16, 32, 16);
			CheckBounds ("scale.Clip.GetBounds", g.Clip.GetBounds (g), 0, -16, 32, 16);

			g.TranslateTransform (8, 8);
			CheckBounds ("translate.ClipBounds", g.ClipBounds, -8, -24, 32, 16);
			CheckBounds ("translate.Clip.GetBounds", g.Clip.GetBounds (g), -8, -24, 32, 16);
			
			g.MultiplyTransform (g.Transform);
			CheckBounds ("multiply.ClipBounds", g.ClipBounds, -104, -56, 64, 64);
			CheckBounds ("multiply.Clip.GetBounds", g.Clip.GetBounds (g), -104, -56, 64, 64);

			g.ResetTransform ();
			CheckBounds ("reset.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("reset.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void Clip_TranslateTransform_BoundsChange ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			g.TranslateTransform (-16, -16);
			CheckBounds ("translated.ClipBounds", g.ClipBounds, 16, 16, 16, 16);
			CheckBounds ("translated.Clip.GetBounds", g.Clip.GetBounds (g), 16, 16, 16, 16);

			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds isn't affected by a previous translation
			CheckBounds ("rectangle.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			// Clip.GetBounds isn't affected by a previous translation
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, -16, -16, 8, 8);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), -16, -16, 8, 8);
		}

		[Test]
		public void Clip_RotateTransform_BoundsChange ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			// we select a "simple" angle because the region will be converted into
			// a bitmap (well for libgdiplus) and we would lose precision after that
			g.RotateTransform (90);
			CheckBounds ("rotated.ClipBounds", g.ClipBounds, 0, -16, 16, 16);
			CheckBounds ("rotated.Clip.GetBounds", g.Clip.GetBounds (g), 0, -16, 16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds isn't affected by a previous rotation (90)
			CheckBounds ("rectangle.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			// Clip.GetBounds isn't affected by a previous rotation
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, -8, 0, 8, 8);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), -8, 0, 8, 8);
		}

		private void CheckBoundsInt (string msg, RectangleF bounds, int x, int y, int w, int h)
		{
			// currently bounds are rounded at 8 pixels (FIXME - we can go down to 1 pixel)
			AssertEquals (msg + ".X", x, bounds.X, 4f);
			AssertEquals (msg + ".Y", y, bounds.Y, 4f);
			AssertEquals (msg + ".Width", w, bounds.Width, 4f);
			AssertEquals (msg + ".Height", h, bounds.Height, 4f);
		}

		[Test]
		[Category ("NotWorking")]
		public void Clip_RotateTransform_BoundsChange_45 ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			g.RotateTransform (45);
			// we can't use the "normal" CheckBound here because of libgdiplus crude rounding
			CheckBoundsInt ("rotated.ClipBounds", g.ClipBounds, 0, -11, 24, 24);
			CheckBoundsInt ("rotated.Clip.GetBounds", g.Clip.GetBounds (g), 0, -11, 24, 24);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds IS affected by a previous rotation (45)
			CheckBoundsInt ("rectangle.ClipBounds", g.ClipBounds, -3, -4, 16, 16);
			// Clip.GetBounds isn't affected by a previous rotation
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, -5, 1, 11, 11);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), -5.6f, 0, 11.3f, 11.3f);
		}

		[Test]
		public void Clip_ScaleTransform_NoBoundsChange ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			g.ScaleTransform (2, 0.5f);
			CheckBounds ("scaled.ClipBounds", g.ClipBounds, 0, 0, 8, 32);
			CheckBounds ("scaled.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 32);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds isn't affected by a previous scaling
			CheckBounds ("rectangle.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			// Clip.GetBounds isn't affected by a previous scaling
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, 0, 0, 16, 4);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 4);
		}

		[Test]
		[Category ("NotWorking")]
		public void Clip_MultiplyTransform_NoBoundsChange ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			g.MultiplyTransform (new Matrix (2.5f, 0.5f, -2.5f, 0.5f, 4, -4));
			CheckBounds ("multiplied.ClipBounds", g.ClipBounds, 3.2f, 1.6f, 19.2f, 19.2f);
			CheckBounds ("multiplied.Clip.GetBounds", g.Clip.GetBounds (g), 3.2f, 1.6f, 19.2f, 19.2f);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds IS affected by the previous multiplication
			CheckBounds ("rectangle.ClipBounds", g.ClipBounds, -3, -3, 15, 15);
			// Clip.GetBounds isn't affected by the previous multiplication
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, -16, -3, 40, 7);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), -16, -4, 40, 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ScaleTransform_X0 ()
		{
			Graphics g = Get (16, 16);
			g.ScaleTransform (0, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ScaleTransform_Y0 ()
		{
			Graphics g = Get (16, 16);
			g.ScaleTransform (1, 0);
		}

		[Test]
		public void TranslateTransform_Order ()
		{
			Graphics g = Get (16, 16);
			g.Transform = new Matrix (1, 2, 3, 4, 5, 6);
			g.TranslateTransform (3, -3);
			float[] elements = g.Transform.Elements;
			AssertEquals ("default.0", 1, elements[0]);
			AssertEquals ("default.1", 2, elements[1]);
			AssertEquals ("default.2", 3, elements[2]);
			AssertEquals ("default.3", 4, elements[3]);
			AssertEquals ("default.4", -1, elements[4]);
			AssertEquals ("default.5", 0, elements[5]);

			g.Transform = new Matrix (1, 2, 3, 4, 5, 6);
			g.TranslateTransform (3, -3, MatrixOrder.Prepend);
			elements = g.Transform.Elements;
			AssertEquals ("prepend.0", 1, elements[0]);
			AssertEquals ("prepend.1", 2, elements[1]);
			AssertEquals ("prepend.2", 3, elements[2]);
			AssertEquals ("prepend.3", 4, elements[3]);
			AssertEquals ("prepend.4", -1, elements[4]);
			AssertEquals ("prepend.5", 0, elements[5]);

			g.Transform = new Matrix (1, 2, 3, 4, 5, 6);
			g.TranslateTransform (3, -3, MatrixOrder.Append);
			elements = g.Transform.Elements;
			AssertEquals ("append.0", 1, elements[0]);
			AssertEquals ("append.1", 2, elements[1]);
			AssertEquals ("append.2", 3, elements[2]);
			AssertEquals ("append.3", 4, elements[3]);
			AssertEquals ("append.4", 8, elements[4]);
			AssertEquals ("append.5", 3, elements[5]);
		}

		static Point[] SmallCurve = new Point[3] { new Point (0, 0), new Point (15, 5), new Point (5, 15) };
		static PointF[] SmallCurveF = new PointF[3] { new PointF (0, 0), new PointF (15, 5), new PointF (5, 15) };

		static Point[] TooSmallCurve = new Point[2] { new Point (0, 0), new Point (15, 5) };
		static PointF[] LargeCurveF = new PointF[4] { new PointF (0, 0), new PointF (15, 5), new PointF (5, 15), new PointF (0, 20) };

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawCurve_PenNull ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (null, SmallCurveF);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawCurve_PointFNull ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, (PointF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawCurve_PointNull ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, (Point[]) null);
		}

		[Test]
		public void DrawCurve_NotEnoughPoints ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			CheckForEmptyBitmap (bitmap);
			g.DrawCurve (Pens.Black, TooSmallCurve, 0.5f);
			CheckForNonEmptyBitmap (bitmap);
			// so a "curve" can be drawn with less than 3 points!
			// actually I used to call that a line... (and it's not related to tension)
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve_SinglePoint ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, new Point[1] { new Point (10, 10) }, 0.5f);
			// a single point isn't enough
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve3_NotEnoughPoints ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, TooSmallCurve, 0, 2, 0.5f);
			// aha, this is API dependent
		}

		[Test]
		public void DrawCurve_NegativeTension ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			// documented as bigger (or equals) to 0
			g.DrawCurve (Pens.Black, SmallCurveF, -0.9f);
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawCurve_PositiveTension ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, SmallCurveF, 0.9f);
			// this is not the same as -1
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus is drawing something
		public void DrawCurve_LargeTension ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, SmallCurve, Single.MaxValue);
			CheckForEmptyBitmap (bitmap);
			// too much tension ;)
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve_ZeroSegments ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, SmallCurveF, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve_NegativeSegments ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, SmallCurveF, 0, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve_OffsetTooLarge ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			// starting offset 1 doesn't give 3 points to make a curve
			g.DrawCurve (Pens.Black, SmallCurveF, 1, 2);
			// and in this case 2 points aren't enough to draw something
		}

		[Test]
		public void DrawCurve_Offset_0 ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, LargeCurveF, 0, 2, 0.5f);
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawCurve_Offset_1 ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, LargeCurveF, 1, 2, 0.5f);
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawCurve_Offset_2 ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			// it works even with two points because we know the previous ones
			g.DrawCurve (Pens.Black, LargeCurveF, 2, 1, 0.5f);
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawRectangle_Negative ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Pen pen = new Pen (Color.Red);
			g.DrawRectangle (pen, 5, 5, -10, -10);
			g.DrawRectangle (pen, 0.0f, 0.0f, 5.0f, -10.0f);
			g.DrawRectangle (pen, new Rectangle (15, 0, -10, 5));
			CheckForEmptyBitmap (bitmap);
			pen.Dispose ();
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawRectangles_Negative ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Pen pen = new Pen (Color.Red);
			Rectangle[] rects = new Rectangle[2] {
				new Rectangle (5, 5, -10, -10), new Rectangle (0, 0, 5, -10)
			};
			RectangleF[] rectf = new RectangleF[2] {
				new RectangleF (0.0f, 5.0f, -10.0f, -10.0f), new RectangleF (15.0f, 0.0f, -10.0f, 5.0f)
			};
			g.DrawRectangles (pen, rects);
			g.DrawRectangles (pen, rectf);
			CheckForEmptyBitmap (bitmap);
			pen.Dispose ();
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void FillRectangle_Negative ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			SolidBrush brush = new SolidBrush (Color.Red);
			g.FillRectangle (brush, 5, 5, -10, -10);
			g.FillRectangle (brush, 0.0f, 0.0f, 5.0f, -10.0f);
			g.FillRectangle (brush, new Rectangle (15, 0, -10, 5));
			CheckForEmptyBitmap (bitmap);
			brush.Dispose ();
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void FillRectangles_Negative ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			SolidBrush brush = new SolidBrush (Color.Red);
			Rectangle[] rects = new Rectangle[2] {
				new Rectangle (5, 5, -10, -10), new Rectangle (0, 0, 5, -10)
			};
			RectangleF[] rectf = new RectangleF[2] {
				new RectangleF (0.0f, 5.0f, -10.0f, -10.0f), new RectangleF (15.0f, 0.0f, -10.0f, 5.0f)
			};
			g.FillRectangles (brush, rects);
			g.FillRectangles (brush, rectf);
			CheckForEmptyBitmap (bitmap);
			brush.Dispose ();
			g.Dispose ();
			bitmap.Dispose ();
		}

		private void CheckDefaultProperties (string message, Graphics g)
		{
			Assert.IsTrue (g.Clip.IsInfinite (g), message + ".Clip.IsInfinite");
			AssertEquals (message + ".CompositingMode", CompositingMode.SourceOver, g.CompositingMode);
			AssertEquals (message + ".CompositingQuality", CompositingQuality.Default, g.CompositingQuality);
			AssertEquals (message + ".InterpolationMode", InterpolationMode.Bilinear, g.InterpolationMode);
			AssertEquals (message + ".PageScale", 1.0f, g.PageScale);
			AssertEquals (message + ".PageUnit", GraphicsUnit.Display, g.PageUnit);
			AssertEquals (message + ".PixelOffsetMode", PixelOffsetMode.Default, g.PixelOffsetMode);
			AssertEquals (message + ".SmoothingMode", SmoothingMode.None, g.SmoothingMode);
			AssertEquals (message + ".TextContrast", 4, g.TextContrast);
			AssertEquals (message + ".TextRenderingHint", TextRenderingHint.SystemDefault, g.TextRenderingHint);
			Assert.IsTrue (g.Transform.IsIdentity, message + ".Transform.IsIdentity");
		}

		private void CheckCustomProperties (string message, Graphics g)
		{
			Assert.IsFalse (g.Clip.IsInfinite (g), message + ".Clip.IsInfinite");
			AssertEquals (message + ".CompositingMode", CompositingMode.SourceCopy, g.CompositingMode);
			AssertEquals (message + ".CompositingQuality", CompositingQuality.HighQuality, g.CompositingQuality);
			AssertEquals (message + ".InterpolationMode", InterpolationMode.HighQualityBicubic, g.InterpolationMode);
			AssertEquals (message + ".PageScale", 0.5f, g.PageScale);
			AssertEquals (message + ".PageUnit", GraphicsUnit.Inch, g.PageUnit);
			AssertEquals (message + ".PixelOffsetMode", PixelOffsetMode.Half, g.PixelOffsetMode);
			AssertEquals (message + ".RenderingOrigin", new Point (-1, -1), g.RenderingOrigin);
			AssertEquals (message + ".SmoothingMode", SmoothingMode.AntiAlias, g.SmoothingMode);
			AssertEquals (message + ".TextContrast", 0, g.TextContrast);
			AssertEquals (message + ".TextRenderingHint", TextRenderingHint.AntiAlias, g.TextRenderingHint);
			Assert.IsFalse (g.Transform.IsIdentity, message + ".Transform.IsIdentity");
		}

		private void CheckMatrix (string message, Matrix m, float xx, float yx, float xy, float yy, float x0, float y0)
		{
			float[] elements = m.Elements;
			AssertEquals (message + ".Matrix.xx", xx, elements[0], 0.01);
			AssertEquals (message + ".Matrix.yx", yx, elements[1], 0.01);
			AssertEquals (message + ".Matrix.xy", xy, elements[2], 0.01);
			AssertEquals (message + ".Matrix.yy", yy, elements[3], 0.01);
			AssertEquals (message + ".Matrix.x0", x0, elements[4], 0.01);
			AssertEquals (message + ".Matrix.y0", y0, elements[5], 0.01);
		}

		[Test]
		public void BeginContainer ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			AssertEquals ("default.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

			g.Clip = new Region (new Rectangle (10, 10, 10, 10));
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PageScale = 0.5f;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (-1, -1);
			g.RotateTransform (45);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextContrast = 0;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			CheckCustomProperties ("modified", g);
			CheckMatrix ("modified.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			GraphicsContainer gc = g.BeginContainer ();
			// things gets reseted after calling BeginContainer
			CheckDefaultProperties ("BeginContainer", g);
			// but not everything 
			AssertEquals ("BeginContainer.RenderingOrigin", new Point (-1, -1), g.RenderingOrigin);

			g.EndContainer (gc);
			CheckCustomProperties ("EndContainer", g);
		}

		[Test]
		public void BeginContainer_Rect ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			AssertEquals ("default.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

			g.Clip = new Region (new Rectangle (10, 10, 10, 10));
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PageScale = 0.5f;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (-1, -1);
			g.RotateTransform (45);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextContrast = 0;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			CheckCustomProperties ("modified", g);
			CheckMatrix ("modified.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			GraphicsContainer gc = g.BeginContainer (new Rectangle (10, 20, 30, 40), new Rectangle (10, 20, 300, 400), GraphicsUnit.Millimeter);
			// things gets reseted after calling BeginContainer
			CheckDefaultProperties ("BeginContainer", g);
			// but not everything 
			AssertEquals ("BeginContainer.RenderingOrigin", new Point (-1, -1), g.RenderingOrigin);

			g.EndContainer (gc);
			CheckCustomProperties ("EndContainer", g);
			CheckMatrix ("EndContainer.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);
		}

		[Test]
		public void BeginContainer_RectF ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			AssertEquals ("default.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

			g.Clip = new Region (new Rectangle (10, 10, 10, 10));
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PageScale = 0.5f;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (-1, -1);
			g.RotateTransform (45);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextContrast = 0;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			CheckCustomProperties ("modified", g);
			CheckMatrix ("modified.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			GraphicsContainer gc = g.BeginContainer (new RectangleF (40, 30, 20, 10), new RectangleF (10, 20, 30, 40), GraphicsUnit.Inch);
			// things gets reseted after calling BeginContainer
			CheckDefaultProperties ("BeginContainer", g);
			// but not everything 
			AssertEquals ("BeginContainer.RenderingOrigin", new Point (-1, -1), g.RenderingOrigin);

			g.EndContainer (gc);
			CheckCustomProperties ("EndContainer", g);
		}

		private void BeginContainer_GraphicsUnit (GraphicsUnit unit)
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.BeginContainer (new RectangleF (40, 30, 20, 10), new RectangleF (10, 20, 30, 40), unit);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BeginContainer_GraphicsUnit_Display ()
		{
			BeginContainer_GraphicsUnit (GraphicsUnit.Display);
		}

		[Test]
		public void BeginContainer_GraphicsUnit_Valid ()
		{
			BeginContainer_GraphicsUnit (GraphicsUnit.Document);
			BeginContainer_GraphicsUnit (GraphicsUnit.Inch);
			BeginContainer_GraphicsUnit (GraphicsUnit.Millimeter);
			BeginContainer_GraphicsUnit (GraphicsUnit.Pixel);
			BeginContainer_GraphicsUnit (GraphicsUnit.Point);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BeginContainer_GraphicsUnit_World ()
		{
			BeginContainer_GraphicsUnit (GraphicsUnit.World);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BeginContainer_GraphicsUnit_Bad ()
		{
			BeginContainer_GraphicsUnit ((GraphicsUnit)Int32.MinValue);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void EndContainer_Null ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.EndContainer (null);
		}

		[Test]
		public void Save ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			AssertEquals ("default.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

			GraphicsState gs1 = g.Save ();
			// nothing is changed after a save
			CheckDefaultProperties ("save1", g);
			AssertEquals ("save1.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

			g.Clip = new Region (new Rectangle (10, 10, 10, 10));
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PageScale = 0.5f;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (-1, -1);
			g.RotateTransform (45);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextContrast = 0;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			CheckCustomProperties ("modified", g);
			CheckMatrix ("modified.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			GraphicsState gs2 = g.Save ();
			CheckCustomProperties ("save2", g);

			g.Restore (gs2);
			CheckCustomProperties ("restored1", g);
			CheckMatrix ("restored1.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			g.Restore (gs1);
			CheckDefaultProperties ("restored2", g);
			AssertEquals ("restored2.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Restore_Null ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.Restore (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FillRectangles_BrushNull_Rectangle ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.FillRectangles (null, new Rectangle[1]);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FillRectangles_Rectangle_Null ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.FillRectangles (Brushes.Red, (Rectangle[]) null);
				}
			}
		}

		[Test] // see bug #78408
		[ExpectedException (typeof (ArgumentException))]
		public void FillRectanglesZeroRectangle ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.FillRectangles (Brushes.Red, new Rectangle[0]);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FillRectangles_BrushNull_RectangleF ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.FillRectangles (null, new RectangleF[1]);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FillRectangles_RectangleF_Null ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.FillRectangles (Brushes.Red, (RectangleF[])null);
				}
			}
		}

		[Test] // see bug #78408
		[ExpectedException (typeof (ArgumentException))]
		public void FillRectanglesZeroRectangleF ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.FillRectangles (Brushes.Red, new RectangleF[0]);
				}
			}
		}

		[Test]
		public void FillRectangles_NormalBehavior ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.Clear (Color.Fuchsia);
					Rectangle rect = new Rectangle (5, 5, 10, 10);
					g.Clip = new Region (rect);
					g.FillRectangle (Brushes.Red, rect);
				}
				Assert.AreEqual (Color.Red.ToArgb (), bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (Color.Red.ToArgb (), bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				Assert.AreEqual (Color.Red.ToArgb (), bitmap.GetPixel (5, 14).ToArgb (), "5,14");
				Assert.AreEqual (Color.Red.ToArgb (), bitmap.GetPixel (14, 14).ToArgb (), "14,14");

				Assert.AreEqual (Color.Fuchsia.ToArgb (), bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (Color.Fuchsia.ToArgb (), bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (Color.Fuchsia.ToArgb (), bitmap.GetPixel (15, 15).ToArgb (), "15,15");
			}
		}

		// see bug #81737 for details
		private Bitmap FillDrawRectangle (float width)
		{
			Bitmap bitmap = new Bitmap (20, 20);
			using (Graphics g = Graphics.FromImage (bitmap)) {
				g.Clear (Color.Red);
				Rectangle rect = new Rectangle (5, 5, 10, 10);
				g.FillRectangle (Brushes.Green, rect);
				if (width >= 0) {
					using (Pen pen = new Pen (Color.Blue, width)) {
						g.DrawRectangle (pen, rect);
					}
				} else {
					g.DrawRectangle (Pens.Blue, rect);
				}
			}
			return bitmap;
		}

		[Test]
		public void FillDrawRectangle_Width_Default ()
		{
			// default pen size
			using (Bitmap bitmap = FillDrawRectangle (Single.MinValue)) {
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 6).ToArgb (), "6,6");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 6).ToArgb (), "9,6");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 6).ToArgb (), "14,6");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 14).ToArgb (), "6,14");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 9).ToArgb (), "6,9");
			}
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus is one pixel off (+1,+1)
		public void FillDrawRectangle_Width_2 ()
		{
			// even pen size
			using (Bitmap bitmap = FillDrawRectangle (2.0f)) {
				bitmap.Save (@"FillDrawRectangle_Width_2.bmp");
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 3).ToArgb (), "3,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 6).ToArgb (), "6,6");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 3).ToArgb (), "9,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 6).ToArgb (), "9,6");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 3).ToArgb (), "16,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 4).ToArgb (), "15,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 6).ToArgb (), "13,6");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 9).ToArgb (), "13,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 13).ToArgb (), "13,13");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 13).ToArgb (), "9,13");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 16).ToArgb (), "3,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 15).ToArgb (), "4,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 14).ToArgb (), "5,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 13).ToArgb (), "6,13");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 9).ToArgb (), "3,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 9).ToArgb (), "6,9");
			}
		}

		[Test]
		public void FillDrawRectangle_Width_3 ()
		{
			// odd pen size
			using (Bitmap bitmap = FillDrawRectangle (3.0f)) {
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 3).ToArgb (), "3,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (6, 6).ToArgb (), "6,6");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (7, 7).ToArgb (), "7,7");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 3).ToArgb (), "9,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 6).ToArgb (), "9,6");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 7).ToArgb (), "9,7");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 3).ToArgb (), "17,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 6).ToArgb (), "14,6");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 7).ToArgb (), "13,7");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 9).ToArgb (), "17,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 9).ToArgb (), "13,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 17).ToArgb (), "17,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 13).ToArgb (), "13,13");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 17).ToArgb (), "9,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 13).ToArgb (), "9,13");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 17).ToArgb (), "3,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (6, 14).ToArgb (), "6,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (7, 13).ToArgb (), "7,13");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 9).ToArgb (), "3,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (6, 9).ToArgb (), "6,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (7, 9).ToArgb (), "7,9");
			}
		}

		// reverse, draw the fill over
		private Bitmap DrawFillRectangle (float width)
		{
			Bitmap bitmap = new Bitmap (20, 20);
			using (Graphics g = Graphics.FromImage (bitmap)) {
				g.Clear (Color.Red);
				Rectangle rect = new Rectangle (5, 5, 10, 10);
				if (width >= 0) {
					using (Pen pen = new Pen (Color.Blue, width)) {
						g.DrawRectangle (pen, rect);
					}
				} else {
					g.DrawRectangle (Pens.Blue, rect);
				}
				g.FillRectangle (Brushes.Green, rect);
			}
			return bitmap;
		}

		[Test]
		public void DrawFillRectangle_Width_Default ()
		{
			// default pen size
			using (Bitmap bitmap = DrawFillRectangle (Single.MinValue)) {
				// NW - no blue border
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 6).ToArgb (), "6,6");
				// N - no blue border
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 6).ToArgb (), "9,6");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 6).ToArgb (), "14,6");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 14).ToArgb (), "6,14");
				// W - no blue border
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 9).ToArgb (), "6,9");
			}
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus is one pixel off (+1,+1)
		public void DrawFillRectangle_Width_2 ()
		{
			// even pen size
			using (Bitmap bitmap = DrawFillRectangle (2.0f)) {
				// looks like a one pixel border - but enlarged
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 3).ToArgb (), "3,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 3).ToArgb (), "9,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 3).ToArgb (), "16,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 4).ToArgb (), "15,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 14).ToArgb (), "6,14");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 9).ToArgb (), "3,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
			}
		}

		[Test]
		public void DrawFillRectangle_Width_3 ()
		{
			// odd pen size
			using (Bitmap bitmap = DrawFillRectangle (3.0f)) {
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 3).ToArgb (), "3,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 3).ToArgb (), "9,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 3).ToArgb (), "17,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 4).ToArgb (), "15,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 9).ToArgb (), "17,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 17).ToArgb (), "17,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 17).ToArgb (), "9,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 17).ToArgb (), "3,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 14).ToArgb (), "6,14");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 9).ToArgb (), "3,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
			}
		}

		[Test]
		public void MeasureString_StringFont ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString (null, font);
					Assert.IsTrue (size.IsEmpty, "MeasureString(null,font)");
					size = g.MeasureString (String.Empty, font);
					Assert.IsTrue (size.IsEmpty, "MeasureString(empty,font)");
					// null font
					size = g.MeasureString (null, null);
					Assert.IsTrue (size.IsEmpty, "MeasureString(null,null)");
					size = g.MeasureString (String.Empty, null);
					Assert.IsTrue (size.IsEmpty, "MeasureString(empty,null)");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MeasureString_StringFont_Null ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.MeasureString ("a", null);
				}
			}
		}

		[Test]
		public void MeasureString_StringFontSizeF ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString ("a", font, SizeF.Empty);
					Assert.IsFalse (size.IsEmpty, "MeasureString(a,font,empty)");

					size = g.MeasureString (String.Empty, font, SizeF.Empty);
					Assert.IsTrue (size.IsEmpty, "MeasureString(empty,font,empty)");
				}
			}
		}

		private void MeasureString_StringFontInt (string s)
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size0 = g.MeasureString (s, font, 0);
					SizeF sizeN = g.MeasureString (s, font, Int32.MinValue);
					SizeF sizeP = g.MeasureString (s, font, Int32.MaxValue);
					Assert.AreEqual (size0, sizeN, "0-Min");
					Assert.AreEqual (size0, sizeP, "0-Max");
				}
			}
		}

		[Test]
		public void MeasureString_StringFontInt_ShortString ()
		{
			MeasureString_StringFontInt ("a");
		}

		[Test]
		public void MeasureString_StringFontInt_LongString ()
		{
			HostIgnoreList.CheckTest ("MonoTests.System.Drawing.GraphicsTest.MeasureString_StringFontInt_LongString");
			MeasureString_StringFontInt ("A very long string..."); // see bug #79643
		}

		[Test]
		public void MeasureString_StringFormat_Alignment ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			StringFormat string_format = new StringFormat ();

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.Alignment = StringAlignment.Near;
					SizeF near = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.Alignment = StringAlignment.Center;
					SizeF center = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.Alignment = StringAlignment.Far;
					SizeF far = g.MeasureString (text, font, Int32.MaxValue, string_format);

					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		public void MeasureString_StringFormat_Alignment_DirectionVertical ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.DirectionVertical;

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.Alignment = StringAlignment.Near;
					SizeF near = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.Alignment = StringAlignment.Center;
					SizeF center = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.Alignment = StringAlignment.Far;
					SizeF far = g.MeasureString (text, font, Int32.MaxValue, string_format);

					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		public void MeasureString_StringFormat_LineAlignment ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			StringFormat string_format = new StringFormat ();

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.LineAlignment = StringAlignment.Near;
					SizeF near = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.LineAlignment = StringAlignment.Center;
					SizeF center = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.LineAlignment = StringAlignment.Far;
					SizeF far = g.MeasureString (text, font, Int32.MaxValue, string_format);

					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		public void MeasureString_StringFormat_LineAlignment_DirectionVertical ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.DirectionVertical;

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.LineAlignment = StringAlignment.Near;
					SizeF near = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.LineAlignment = StringAlignment.Center;
					SizeF center = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.LineAlignment = StringAlignment.Far;
					SizeF far = g.MeasureString (text, font, Int32.MaxValue, string_format);

					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void MeasureString_MultlineString_Width ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					StringFormat string_format = new StringFormat();
					
					string text1 = "Test\nTest123\nTest 456\nTest 1,2,3,4,5...";
					string text2 = "Test 1,2,3,4,5...";
				
					SizeF size1 = g.MeasureString (text1, font, SizeF.Empty, string_format);
					SizeF size2 = g.MeasureString (text2, font, SizeF.Empty, string_format);
					
					Assert.AreEqual ((int) size1.Width, (int) size2.Width, "Multiline Text Width");
				}
			}
		}

		[Test]
		public void MeasureString_Bug76664 ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string s = "aaa aa aaaa a aaa";
					SizeF size = g.MeasureString (s, font);

					int chars, lines;
					SizeF size2 = g.MeasureString (s, font, new SizeF (80, size.Height), null, out chars, out lines);

					// in pixels
					Assert.IsTrue (size2.Width < size.Width, "Width/pixel");
					Assert.AreEqual (size2.Height, size.Height, "Height/pixel");

					Assert.AreEqual (1, lines, "lines fitted");
					// LAMESPEC: documentation seems to suggest chars is total length
					Assert.IsTrue (chars < s.Length, "characters fitted");
				}
			}
		}

		[Test]
		public void MeasureString_Bug80680 ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string s = String.Empty;
					SizeF size = g.MeasureString (s, font);
					Assert.AreEqual (0, size.Height, "Empty.Height");
					Assert.AreEqual (0, size.Width, "Empty.Width");

					s += " ";
					SizeF expected = g.MeasureString (s, font);
					for (int i = 1; i < 10; i++) {
						s += " ";
						size = g.MeasureString (s, font);
						Assert.AreEqual (expected.Height, size.Height, 0.1, ">" + s + "< Height");
						Assert.AreEqual (expected.Width, size.Width, 0.1, ">" + s + "< Width");
					}

					s = "a";
					expected = g.MeasureString (s, font);
					s = " " + s;
					size = g.MeasureString (s, font);
					float space_width = size.Width - expected.Width;
					for (int i = 1; i < 10; i++) {
						size = g.MeasureString (s, font);
						Assert.AreEqual (expected.Height, size.Height, 0.1, ">" + s + "< Height");
						Assert.AreEqual (expected.Width + i * space_width, size.Width, 0.1, ">" + s + "< Width");
						s = " " + s;
					}

					s = "a";
					expected = g.MeasureString (s, font);
					for (int i = 1; i < 10; i++) {
						s = s + " ";
						size = g.MeasureString (s, font);
						Assert.AreEqual (expected.Height, size.Height, 0.1, ">" + s + "< Height");
						Assert.AreEqual (expected.Width, size.Width, 0.1, ">" + s + "< Width");
					}
				}
			}
		}

		[Test]
		public void MeasureCharacterRanges_NullOrEmptyText ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Region[] regions = g.MeasureCharacterRanges (null, font, new RectangleF (), null);
					AssertEquals ("text null", 0, regions.Length);
					regions = g.MeasureCharacterRanges (String.Empty, font, new RectangleF (), null);
					AssertEquals ("text empty", 0, regions.Length);
					// null font is ok with null or empty string
					regions = g.MeasureCharacterRanges (null, null, new RectangleF (), null);
					AssertEquals ("text null/null font", 0, regions.Length);
					regions = g.MeasureCharacterRanges (String.Empty, null, new RectangleF (), null);
					AssertEquals ("text empty/null font", 0, regions.Length);
				}
			}
		}

		[Test]
		public void MeasureCharacterRanges_EmptyStringFormat ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					// string format without character ranges
					Region[] regions = g.MeasureCharacterRanges ("Mono", font, new RectangleF (), new StringFormat ());
					AssertEquals ("empty stringformat", 0, regions.Length);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MeasureCharacterRanges_FontNull ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.MeasureCharacterRanges ("a", null, new RectangleF (), null);
				}
			}
		}

		[Test] // adapted from bug #78777
		public void MeasureCharacterRanges_TwoLines ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "this\nis a test";
			CharacterRange[] ranges = new CharacterRange [2];
			ranges[0] = new CharacterRange (0,5);
			ranges[1] = new CharacterRange (5,9);

			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.NoClip;
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString (text, font, new Point (0,0), string_format);
					RectangleF layout_rect = new RectangleF (0.0f, 0.0f, size.Width, size.Height);			
					Region[] regions = g.MeasureCharacterRanges (text, font, layout_rect, string_format);

					AssertEquals ("Length", 2, regions.Length);
					AssertEquals ("Height", regions[0].GetBounds (g).Height, regions[1].GetBounds (g).Height);
				}
			}
		}

		private void MeasureCharacterRanges (string text, int first, int length)
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (first, length);

			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.NoClip;
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString (text, font, new Point (0, 0), string_format);
					RectangleF layout_rect = new RectangleF (0.0f, 0.0f, size.Width, size.Height);
					g.MeasureCharacterRanges (text, font, layout_rect, string_format);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MeasureCharacterRanges_FirstTooFar ()
		{
			string text = "this\nis a test";
			MeasureCharacterRanges (text, text.Length, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MeasureCharacterRanges_LengthTooLong ()
		{
			string text = "this\nis a test";
			MeasureCharacterRanges (text, 0, text.Length + 1);
		}

		[Test]
		public void MeasureCharacterRanges_Prefix ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello &Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);

			StringFormat string_format = new StringFormat ();
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString (text, font, new Point (0, 0), string_format);
					RectangleF layout_rect = new RectangleF (0.0f, 0.0f, size.Width, size.Height);

					// here & is part of the measure and visible
					string_format.HotkeyPrefix = HotkeyPrefix.None;
					Region[] regions = g.MeasureCharacterRanges (text, font, layout_rect, string_format);
					RectangleF bounds_none = regions[0].GetBounds (g);

					// here & is part of the measure (range) but visible as an underline
					string_format.HotkeyPrefix = HotkeyPrefix.Show;
					regions = g.MeasureCharacterRanges (text, font, layout_rect, string_format);
					RectangleF bounds_show = regions[0].GetBounds (g);
					Assert.IsTrue (bounds_show.Width < bounds_none.Width, "Show<None");

					// here & is part of the measure (range) but invisible
					string_format.HotkeyPrefix = HotkeyPrefix.Hide;
					regions = g.MeasureCharacterRanges (text, font, layout_rect, string_format);
					RectangleF bounds_hide = regions[0].GetBounds (g);
					AssertEquals ("Hide==None", bounds_hide.Width, bounds_show.Width);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MeasureCharacterRanges_NullStringFormat ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.MeasureCharacterRanges ("Mono", font, new RectangleF (), null);
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void MeasureCharacterRanges_StringFormat_Alignment ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);
			StringFormat string_format = new StringFormat ();
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.Alignment = StringAlignment.Near;
					Region[] regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Near.Region");
					RectangleF near = regions[0].GetBounds (g);

					string_format.Alignment = StringAlignment.Center;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Center.Region");
					RectangleF center = regions[0].GetBounds (g);

					string_format.Alignment = StringAlignment.Far;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Far.Region");
					RectangleF far = regions[0].GetBounds (g);

					Assert.IsTrue (near.X < center.X, "near-center/X");
					Assert.AreEqual (near.Y, center.Y, 0.1, "near-center/Y");
					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.IsTrue (center.X < far.X, "center-far/X");
					Assert.AreEqual (center.Y, far.Y, "center-far/Y");
					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void MeasureCharacterRanges_StringFormat_LineAlignment ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);
			StringFormat string_format = new StringFormat ();
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.LineAlignment = StringAlignment.Near;
					Region[] regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Near.Region");
					RectangleF near = regions[0].GetBounds (g);

					string_format.LineAlignment = StringAlignment.Center;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Center.Region");
					RectangleF center = regions[0].GetBounds (g);

					string_format.LineAlignment = StringAlignment.Far;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Far.Region");
					RectangleF far = regions[0].GetBounds (g);

					Assert.AreEqual (near.X, center.X, 0.1, "near-center/X");
					Assert.IsTrue (near.Y < center.Y, "near-center/Y");
					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.X, far.X, 0.1, "center-far/X");
					Assert.IsTrue (center.Y < far.Y, "center-far/Y");
					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void MeasureCharacterRanges_StringFormat_Alignment_DirectionVertical ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.DirectionVertical;
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.Alignment = StringAlignment.Near;
					Region[] regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Near.Region");
					RectangleF near = regions[0].GetBounds (g);

					string_format.Alignment = StringAlignment.Center;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Center.Region");
					RectangleF center = regions[0].GetBounds (g);

					string_format.Alignment = StringAlignment.Far;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Far.Region");
					RectangleF far = regions[0].GetBounds (g);

					Assert.IsTrue (near.X < center.X, "near-center/X"); // ???
					Assert.IsTrue (near.Y < center.Y, "near-center/Y");
					Assert.IsTrue (near.Width > center.Width, "near-center/Width"); // ???
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.X, far.X, 0.1, "center-far/X");
					Assert.IsTrue (center.Y < far.Y, "center-far/Y");
					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void MeasureCharacterRanges_StringFormat_LineAlignment_DirectionVertical ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.DirectionVertical;
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.LineAlignment = StringAlignment.Near;
					Region[] regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Near.Region");
					RectangleF near = regions[0].GetBounds (g);

					string_format.LineAlignment = StringAlignment.Center;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Center.Region");
					RectangleF center = regions[0].GetBounds (g);

					string_format.LineAlignment = StringAlignment.Far;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Far.Region");
					RectangleF far = regions[0].GetBounds (g);

					Assert.IsTrue (near.X < center.X, "near-center/X");
					Assert.AreEqual (near.Y, center.Y, 0.1, "near-center/Y");
					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.IsTrue (center.X < far.X, "center-far/X");
					Assert.AreEqual (center.Y, far.Y, 0.1, "center-far/Y");
					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		public void DrawString_EndlessLoop_Bug77699 ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Rectangle rect = Rectangle.Empty;
					rect.Location = new Point (10, 10);
					rect.Size = new Size (1, 20);
					StringFormat fmt = new StringFormat ();
					fmt.Alignment = StringAlignment.Center;
					fmt.LineAlignment = StringAlignment.Center;
					fmt.FormatFlags = StringFormatFlags.NoWrap;
					fmt.Trimming = StringTrimming.EllipsisWord;
					g.DrawString ("Test String", font, Brushes.Black, rect, fmt);
				}
			}
		}

		[Test]
		public void DrawString_EndlessLoop_Wrapping ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Rectangle rect = Rectangle.Empty;
					rect.Location = new Point (10, 10);
					rect.Size = new Size (1, 20);
					StringFormat fmt = new StringFormat ();
					fmt.Alignment = StringAlignment.Center;
					fmt.LineAlignment = StringAlignment.Center;
					fmt.Trimming = StringTrimming.EllipsisWord;
					g.DrawString ("Test String", font, Brushes.Black, rect, fmt);
				}
			}
		}

		[Test]
		public void MeasureString_Wrapping_Dots ()
		{
			HostIgnoreList.CheckTest ("MonoTests.System.Drawing.GraphicsTest.MeasureString_Wrapping_Dots");

			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "this is really long text........................................... with a lot o periods.";
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					using (StringFormat format = new StringFormat ()) {
						format.Alignment = StringAlignment.Center;
						SizeF sz = g.MeasureString (text, font, 80, format);
						Assert.IsTrue (sz.Width < 80, "Width");
						Assert.IsTrue (sz.Height > font.Height * 2, "Height");
					}
				}
			}
		}
#if NET_2_0
		[Test]
		public void TestReleaseHdc ()
		{
			Bitmap b = new Bitmap (100, 100);
			Graphics g = Graphics.FromImage (b);

			g.GetHdc ();
			g.ReleaseHdc ();
			g.GetHdc ();
			g.ReleaseHdc ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestReleaseHdcException ()
		{
			Bitmap b = new Bitmap (100, 100);
			Graphics g = Graphics.FromImage (b);

			g.ReleaseHdc ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestReleaseHdcException2 ()
		{
			Bitmap b = new Bitmap (100, 100);
			Graphics g = Graphics.FromImage (b);

			g.GetHdc ();
			g.ReleaseHdc ();
			g.ReleaseHdc ();
		}
#endif
		[Test]
		public void VisibleClipBound ()
		{
			// see #78958
			using (Bitmap bmp = new Bitmap (100, 100)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF noclip = g.VisibleClipBounds;
					Assert.AreEqual (0, noclip.X, "noclip.X");
					Assert.AreEqual (0, noclip.Y, "noclip.Y");
					Assert.AreEqual (100, noclip.Width, "noclip.Width");
					Assert.AreEqual (100, noclip.Height, "noclip.Height");

					// note: libgdiplus regions are precise to multiple of multiple of 8
					g.Clip = new Region (new RectangleF (0, 0, 32, 32));
					RectangleF clip = g.VisibleClipBounds;
					Assert.AreEqual (0, clip.X, "clip.X");
					Assert.AreEqual (0, clip.Y, "clip.Y");
					Assert.AreEqual (32, clip.Width, "clip.Width");
					Assert.AreEqual (32, clip.Height, "clip.Height");

					g.RotateTransform (90);
					RectangleF rotclip = g.VisibleClipBounds;
					Assert.AreEqual (0, rotclip.X, "rotclip.X");
					Assert.AreEqual (-32, rotclip.Y, "rotclip.Y");
					Assert.AreEqual (32, rotclip.Width, "rotclip.Width");
					Assert.AreEqual (32, rotclip.Height, "rotclip.Height");
				}
			}
		}

		[Test]
		public void VisibleClipBound_BigClip ()
		{
			using (Bitmap bmp = new Bitmap (100, 100)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF noclip = g.VisibleClipBounds;
					Assert.AreEqual (0, noclip.X, "noclip.X");
					Assert.AreEqual (0, noclip.Y, "noclip.Y");
					Assert.AreEqual (100, noclip.Width, "noclip.Width");
					Assert.AreEqual (100, noclip.Height, "noclip.Height");

					// clip is larger than bitmap
					g.Clip = new Region (new RectangleF (0, 0, 200, 200));
					RectangleF clipbound = g.ClipBounds;
					Assert.AreEqual (0, clipbound.X, "clipbound.X");
					Assert.AreEqual (0, clipbound.Y, "clipbound.Y");
					Assert.AreEqual (200, clipbound.Width, "clipbound.Width");
					Assert.AreEqual (200, clipbound.Height, "clipbound.Height");

					RectangleF clip = g.VisibleClipBounds;
					Assert.AreEqual (0, clip.X, "clip.X");
					Assert.AreEqual (0, clip.Y, "clip.Y");
					Assert.AreEqual (100, clip.Width, "clip.Width");
					Assert.AreEqual (100, clip.Height, "clip.Height");

					g.RotateTransform (90);
					RectangleF rotclipbound = g.ClipBounds;
					Assert.AreEqual (0, rotclipbound.X, "rotclipbound.X");
					Assert.AreEqual (-200, rotclipbound.Y, "rotclipbound.Y");
					Assert.AreEqual (200, rotclipbound.Width, "rotclipbound.Width");
					Assert.AreEqual (200, rotclipbound.Height, "rotclipbound.Height");

					RectangleF rotclip = g.VisibleClipBounds;
					Assert.AreEqual (0, rotclip.X, "rotclip.X");
					Assert.AreEqual (-100, rotclip.Y, "rotclip.Y");
					Assert.AreEqual (100, rotclip.Width, "rotclip.Width");
					Assert.AreEqual (100, rotclip.Height, "rotclip.Height");
				}
			}
		}

		[Test]
		public void Rotate ()
		{
			using (Bitmap bmp = new Bitmap (100, 50)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF vcb = g.VisibleClipBounds;
					Assert.AreEqual (0, vcb.X, "vcb.X");
					Assert.AreEqual (0, vcb.Y, "vcb.Y");
					Assert.AreEqual (100, vcb.Width, "vcb.Width");
					Assert.AreEqual (50, vcb.Height, "vcb.Height");

					g.RotateTransform (90);
					RectangleF rvcb = g.VisibleClipBounds;
					Assert.AreEqual (0, rvcb.X, "rvcb.X");
					Assert.AreEqual (-100, rvcb.Y, "rvcb.Y");
					Assert.AreEqual (50, rvcb.Width, "rvcb.Width");
					Assert.AreEqual (100, rvcb.Height, "rvcb.Height");
				}
			}
		}

		[Test]
		public void Scale ()
		{
			using (Bitmap bmp = new Bitmap (100, 50)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF vcb = g.VisibleClipBounds;
					Assert.AreEqual (0, vcb.X, "vcb.X");
					Assert.AreEqual (0, vcb.Y, "vcb.Y");
					Assert.AreEqual (100, vcb.Width, "vcb.Width");
					Assert.AreEqual (50, vcb.Height, "vcb.Height");

					g.ScaleTransform (2, 0.5f);
					RectangleF svcb = g.VisibleClipBounds;
					Assert.AreEqual (0, svcb.X, "svcb.X");
					Assert.AreEqual (0, svcb.Y, "svcb.Y");
					Assert.AreEqual (50, svcb.Width, "svcb.Width");
					Assert.AreEqual (100, svcb.Height, "svcb.Height");
				}
			}
		}

		[Test]
		public void Translate ()
		{
			using (Bitmap bmp = new Bitmap (100, 50)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF vcb = g.VisibleClipBounds;
					Assert.AreEqual (0, vcb.X, "vcb.X");
					Assert.AreEqual (0, vcb.Y, "vcb.Y");
					Assert.AreEqual (100, vcb.Width, "vcb.Width");
					Assert.AreEqual (50, vcb.Height, "vcb.Height");

					g.TranslateTransform (-25, 25);
					RectangleF tvcb = g.VisibleClipBounds;
					Assert.AreEqual (25, tvcb.X, "tvcb.X");
					Assert.AreEqual (-25, tvcb.Y, "tvcb.Y");
					Assert.AreEqual (100, tvcb.Width, "tvcb.Width");
					Assert.AreEqual (50, tvcb.Height, "tvcb.Height");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawIcon_NullRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawIcon (null, new Rectangle (0, 0, 32, 32));
				}
			}
		}

		[Test]
		public void DrawIcon_IconRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawIcon (SystemIcons.Application, new Rectangle (0, 0, 40, 20));
					// Rectangle is empty when X, Y, Width and Height == 0 
					// (yep X and Y too, RectangleF only checks for Width and Height)
					g.DrawIcon (SystemIcons.Asterisk, new Rectangle (0, 0, 0, 0));
					// so this one is half-empty ;-)
					g.DrawIcon (SystemIcons.Error, new Rectangle (20, 40, 0, 0));
					// negative width or height isn't empty (for Rectangle)
					g.DrawIconUnstretched (SystemIcons.WinLogo, new Rectangle (10, 20, -1, 0));
					g.DrawIconUnstretched (SystemIcons.WinLogo, new Rectangle (20, 10, 0, -1));
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawIcon_NullIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawIcon (null, 4, 2);
				}
			}
		}

		[Test]
		public void DrawIcon_IconIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawIcon (SystemIcons.Exclamation, 4, 2);
					g.DrawIcon (SystemIcons.Hand, 0, 0);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawIconUnstretched_NullRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawIconUnstretched (null, new Rectangle (0, 0, 40, 20));
				}
			}
		}

		[Test]
		public void DrawIconUnstretched_IconRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawIconUnstretched (SystemIcons.Information, new Rectangle (0, 0, 40, 20));
					// Rectangle is empty when X, Y, Width and Height == 0 
					// (yep X and Y too, RectangleF only checks for Width and Height)
					g.DrawIconUnstretched (SystemIcons.Question, new Rectangle (0, 0, 0, 0));
					// so this one is half-empty ;-)
					g.DrawIconUnstretched (SystemIcons.Warning, new Rectangle (20, 40, 0, 0));
					// negative width or height isn't empty (for Rectangle)
					g.DrawIconUnstretched (SystemIcons.WinLogo, new Rectangle (10, 20, -1, 0));
					g.DrawIconUnstretched (SystemIcons.WinLogo, new Rectangle (20, 10, 0, -1));
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullRectangleF ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, new RectangleF (0, 0, 0, 0));
				}
			}
		}

		[Test]
		public void DrawImage_ImageRectangleF ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new RectangleF (0, 0, 0, 0));
					g.DrawImage (bmp, new RectangleF (20, 40, 0, 0));
					g.DrawImage (bmp, new RectangleF (10, 20, -1, 0));
					g.DrawImage (bmp, new RectangleF (20, 10, 0, -1));
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullPointF ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, new PointF (0, 0));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointF ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new PointF (0, 0));
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullPointFArray ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, new PointF[0]);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_ImagePointFArrayNull ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, (PointF[])null);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImagePointFArrayEmpty ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new PointF[0]);
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointFArray ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new PointF[] { 
						new PointF (0, 0), new PointF (1, 1), new PointF (2, 2) });
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, new Rectangle (0, 0, 0, 0));
				}
			}
		}

		[Test]
		public void DrawImage_ImageRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					// Rectangle is empty when X, Y, Width and Height == 0 
					// (yep X and Y too, RectangleF only checks for Width and Height)
					g.DrawImage (bmp, new Rectangle (0, 0, 0, 0));
					// so this one is half-empty ;-)
					g.DrawImage (bmp, new Rectangle (20, 40, 0, 0));
					// negative width or height isn't empty (for Rectangle)
					g.DrawImage (bmp, new Rectangle (10, 20, -1, 0));
					g.DrawImage (bmp, new Rectangle (20, 10, 0, -1));
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullPoint ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, new Point (0, 0));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePoint ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new Point (0, 0));
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullPointArray ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, new Point[0]);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_ImagePointArrayNull ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, (Point[]) null);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImagePointArrayEmpty ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new Point[0]);
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointArray ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new Point[] { 
						new Point (0, 0), new Point (1, 1), new Point (2, 2) });
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, Int32.MaxValue, Int32.MinValue);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void DrawImage_ImageIntInt_Overflow ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, Int32.MaxValue, Int32.MinValue);
				}
			}
		}

		[Test]
		public void DrawImage_ImageIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, -40, -40);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullFloat ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, Single.MaxValue, Single.MinValue);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void DrawImage_ImageFloatFloat_Overflow ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, Single.MaxValue, Single.MinValue);
				}
			}
		}

		[Test]
		public void DrawImage_ImageFloatFloat ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, -40.0f, -40.0f);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullRectangleRectangleGraphicsUnit ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, new Rectangle (), new Rectangle (), GraphicsUnit.Display);
				}
			}
		}

		private void DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit unit)
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Rectangle r = new Rectangle (0, 0, 40, 40);
					g.DrawImage (bmp, r, r, unit);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Display ()
		{
			DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Display);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Document ()
		{
			DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Document);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Inch ()
		{
			DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Inch);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Millimeter ()
		{
			DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Millimeter);
		}

		[Test]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Pixel ()
		{
			// this unit works
			DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Pixel);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Point ()
		{
			DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Point);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_World ()
		{
			DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.World);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullPointRectangleGraphicsUnit ()
		{
			Rectangle r = new Rectangle (1, 2, 3, 4);
			Point[] pts = new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) };
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, pts, r, GraphicsUnit.Pixel);
				}
			}
		}

		private void DrawImage_ImagePointRectangleGraphicsUnit (Point[] pts)
		{
			Rectangle r = new Rectangle (1, 2, 3, 4);
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, pts, r, GraphicsUnit.Pixel);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_ImageNullRectangleGraphicsUnit ()
		{
			DrawImage_ImagePointRectangleGraphicsUnit (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImagePoint0RectangleGraphicsUnit ()
		{
			DrawImage_ImagePointRectangleGraphicsUnit (new Point[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImagePoint1RectangleGraphicsUnit ()
		{
			Point p = new Point (1, 1);
			DrawImage_ImagePointRectangleGraphicsUnit (new Point[1] { p });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImagePoint2RectangleGraphicsUnit ()
		{
			Point p = new Point (1, 1);
			DrawImage_ImagePointRectangleGraphicsUnit (new Point[2] { p, p });
		}

		[Test]
		public void DrawImage_ImagePoint3RectangleGraphicsUnit ()
		{
			Point p = new Point (1, 1);
			DrawImage_ImagePointRectangleGraphicsUnit (new Point[3] { p, p, p });
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void DrawImage_ImagePoint4RectangleGraphicsUnit ()
		{
			Point p = new Point (1, 1);
			DrawImage_ImagePointRectangleGraphicsUnit (new Point[4] { p, p, p, p });
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_NullPointFRectangleGraphicsUnit ()
		{
			Rectangle r = new Rectangle (1, 2, 3, 4);
			PointF[] pts = new PointF[3] { new PointF (1, 1), new PointF (2, 2), new PointF (3, 3) };
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (null, pts, r, GraphicsUnit.Pixel);
				}
			}
		}

		private void DrawImage_ImagePointFRectangleGraphicsUnit (PointF[] pts)
		{
			Rectangle r = new Rectangle (1, 2, 3, 4);
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, pts, r, GraphicsUnit.Pixel);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImage_ImageNullFRectangleGraphicsUnit ()
		{
			DrawImage_ImagePointFRectangleGraphicsUnit (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImagePointF0RectangleGraphicsUnit ()
		{
			DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImagePointF1RectangleGraphicsUnit ()
		{
			PointF p = new PointF (1, 1);
			DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[1] { p });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawImage_ImagePointF2RectangleGraphicsUnit ()
		{
			PointF p = new PointF (1, 1);
			DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[2] { p, p });
		}

		[Test]
		public void DrawImage_ImagePointF3RectangleGraphicsUnit ()
		{
			PointF p = new PointF (1, 1);
			DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[3] { p, p, p });
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void DrawImage_ImagePointF4RectangleGraphicsUnit ()
		{
			PointF p = new PointF (1, 1);
			DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[4] { p, p, p, p });
		}

		[Test]
		public void DrawImage_ImagePointRectangleGraphicsUnitNull ()
		{
			Point p = new Point (1, 1);
			Point[] pts = new Point[3] { p, p, p };
			Rectangle r = new Rectangle (1, 2, 3, 4);
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, pts, r, GraphicsUnit.Pixel, null);
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointRectangleGraphicsUnitAttributes ()
		{
			Point p = new Point (1, 1);
			Point[] pts = new Point[3] { p, p, p };
			Rectangle r = new Rectangle (1, 2, 3, 4);
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					ImageAttributes ia = new ImageAttributes ();
					g.DrawImage (bmp, pts, r, GraphicsUnit.Pixel, ia);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImageUnscaled_NullPoint ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (null, new Point (0, 0));
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_ImagePoint ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (bmp, new Point (0, 0));
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImageUnscaled_NullRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (null, new Rectangle (0, 0, -1, -1));
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_ImageRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (bmp, new Rectangle (0, 0, -1, -1));
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImageUnscaled_NullIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (null, 0, 0);
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_ImageIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (bmp, 0, 0);
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImageUnscaled_NullIntIntIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (null, 0, 0, -1, -1);
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_ImageIntIntIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (bmp, 0, 0, -1, -1);
				}
			}
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawImageUnscaledAndClipped_Null ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaledAndClipped (null, new Rectangle (0, 0, 0, 0));
				}
			}
		}

		[Test]
		public void DrawImageUnscaledAndClipped ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					// Rectangle is empty when X, Y, Width and Height == 0 
					// (yep X and Y too, RectangleF only checks for Width and Height)
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (0, 0, 0, 0));
					// so this one is half-empty ;-)
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (20, 40, 0, 0));
					// negative width or height isn't empty (for Rectangle)
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (10, 20, -1, 0));
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (20, 10, 0, -1));
					// smaller
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (0, 0, 10, 20));
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (0, 0, 40, 10));
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (0, 0, 80, 20));
				}
			}
		}
#endif
	}
}

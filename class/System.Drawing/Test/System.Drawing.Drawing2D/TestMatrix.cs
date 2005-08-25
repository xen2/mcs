//
// Tests for System.Drawing.Drawing2D.Matrix.cs
//
// Author:
//  Jordi Mas i Hernandez <jordi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace MonoTests.System.Drawing.Drawing2D
{
	[TestFixture]
	public class MatrixTest : Assertion
	{
		[TearDown]
		public void TearDown () { }

		[SetUp]
		public void SetUp () { }

		[Test]
		public void Constructors ()
		{
			{
				Matrix matrix = new Matrix ();
				AssertEquals ("C#1", 6, matrix.Elements.Length);
			}

			{

				Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
				AssertEquals ("C#2", 6, matrix.Elements.Length);
				AssertEquals ("C#3", 10, matrix.Elements[0]);
				AssertEquals ("C#4", 20, matrix.Elements[1]);
				AssertEquals ("C#5", 30, matrix.Elements[2]);
				AssertEquals ("C#6", 40, matrix.Elements[3]);
				AssertEquals ("C#7", 50, matrix.Elements[4]);
				AssertEquals ("C#8", 60, matrix.Elements[5]);
			}
		}

		// Properties

		[Test]
		public void Invertible ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			AssertEquals ("I#1", false, matrix.IsInvertible);

			matrix = new Matrix (156, 46, 0, 0, 106, 19);
			AssertEquals ("I#2", false, matrix.IsInvertible);

			matrix = new Matrix (146, 66, 158, 104, 42, 150);
			AssertEquals ("I#3", true, matrix.IsInvertible);

			matrix = new Matrix (119, 140, 145, 74, 102, 58);
			AssertEquals ("I#4", true, matrix.IsInvertible);
		}
		
		[Test]
		public void IsIdentity ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			AssertEquals ("N#1", false, matrix.IsIdentity);
			
			matrix = new Matrix (1, 0, 0, 1, 0, 0);
			AssertEquals ("N#2", true, matrix.IsIdentity);			
		}
		
		[Test]
		public void IsOffsetX ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			AssertEquals ("X#1", 47, matrix.OffsetX);			
		}
		
		[Test]
		public void IsOffsetY ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			AssertEquals ("Y#1", 30, matrix.OffsetY);			
		}
		
		// Elements Property is checked implicity in other test

		//
		// Methods
		//
		

		[Test]
		public void Clone ()
		{
			Matrix matsrc = new Matrix (10, 20, 30, 40, 50, 60);
			Matrix matrix  = matsrc.Clone ();

			AssertEquals ("D#1", 6, matrix.Elements.Length);
			AssertEquals ("D#2", 10, matrix.Elements[0]);
			AssertEquals ("D#3", 20, matrix.Elements[1]);
			AssertEquals ("D#4", 30, matrix.Elements[2]);
			AssertEquals ("D#5", 40, matrix.Elements[3]);
			AssertEquals ("D#6", 50, matrix.Elements[4]);
			AssertEquals ("D#7", 60, matrix.Elements[5]);
		}

		[Test]
		public void Reset ()
		{
			Matrix matrix = new Matrix (51, 52, 53, 54, 55, 56);
			matrix.Reset ();

			AssertEquals ("F#1", 6, matrix.Elements.Length);
			AssertEquals ("F#2", 1, matrix.Elements[0]);
			AssertEquals ("F#3", 0, matrix.Elements[1]);
			AssertEquals ("F#4", 0, matrix.Elements[2]);
			AssertEquals ("F#5", 1, matrix.Elements[3]);
			AssertEquals ("F#6", 0, matrix.Elements[4]);
			AssertEquals ("F#7", 0, matrix.Elements[5]);
		}

		[Test]
		public void Rotate ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Rotate (180);

			AssertEquals ("H#1", -10, matrix.Elements[0]);
			AssertEquals ("H#2", -20, matrix.Elements[1]);
			AssertEquals ("H#3", -30, matrix.Elements[2]);
			AssertEquals ("H#4", -40, matrix.Elements[3]);
			AssertEquals ("H#5", 50, matrix.Elements[4]);
			AssertEquals ("H#6", 60, matrix.Elements[5]);
		}

		[Test]
		public void RotateAt ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.RotateAt (180, new PointF (10, 10));

			AssertEquals ("I#1", -10, matrix.Elements[0]);
			AssertEquals ("I#2", -20, matrix.Elements[1]);
			AssertEquals ("I#3", -30, matrix.Elements[2]);
			AssertEquals ("I#4", -40, matrix.Elements[3]);
			AssertEquals ("I#5", 850, matrix.Elements[4]);
			AssertEquals ("I#6", 1260, matrix.Elements[5]);
		}

		[Test]
		public void Multiply ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Multiply (new Matrix (10, 20, 30, 40, 50, 60));

			AssertEquals ("J#1", 700, matrix.Elements[0]);
			AssertEquals ("J#2", 1000, matrix.Elements[1]);
			AssertEquals ("J#3", 1500, matrix.Elements[2]);
			AssertEquals ("J#4", 2200, matrix.Elements[3]);
			AssertEquals ("J#5", 2350, matrix.Elements[4]);
			AssertEquals ("J#6", 3460, matrix.Elements[5]);
		}

		[Test]
		public void Equals ()
		{
			Matrix mat1 = new Matrix (10, 20, 30, 40, 50, 60);
			Matrix mat2 = new Matrix (10, 20, 30, 40, 50, 60);
			Matrix mat3 = new Matrix (10, 20, 30, 40, 50, 10);

			AssertEquals ("E#1", true, mat1.Equals (mat2));
			AssertEquals ("E#2", false, mat2.Equals (mat3));
			AssertEquals ("E#3", false, mat1.Equals (mat3));
		}
		
		[Test]
		public void Invert ()
		{
			Matrix matrix = new Matrix (1, 2, 3, 4, 5, 6);
			matrix.Invert ();
			
			AssertEquals ("V#1", -2, matrix.Elements[0]);
			AssertEquals ("V#2", 1, matrix.Elements[1]);
			AssertEquals ("V#3", 1.5, matrix.Elements[2]);
			AssertEquals ("V#4", -0.5, matrix.Elements[3]);
			AssertEquals ("V#5", 1, matrix.Elements[4]);
			AssertEquals ("V#6", -2, matrix.Elements[5]);			
		}
		
		[Test]
		public void Scale ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Scale (2, 4);

			AssertEquals ("S#1", 20, matrix.Elements[0]);
			AssertEquals ("S#2", 40, matrix.Elements[1]);
			AssertEquals ("S#3", 120, matrix.Elements[2]);
			AssertEquals ("S#4", 160, matrix.Elements[3]);
			AssertEquals ("S#5", 50, matrix.Elements[4]);
			AssertEquals ("S#6", 60, matrix.Elements[5]);			
		}
		
		[Test]
		public void Shear ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Shear (2, 4);

			AssertEquals ("H#1", 130, matrix.Elements[0]);
			AssertEquals ("H#2", 180, matrix.Elements[1]);
			AssertEquals ("H#3", 50, matrix.Elements[2]);
			AssertEquals ("H#4", 80, matrix.Elements[3]);
			AssertEquals ("H#5", 50, matrix.Elements[4]);
			AssertEquals ("H#6", 60, matrix.Elements[5]);
			
			matrix = new Matrix (5, 3, 9, 2, 2, 1);
			matrix.Shear  (10, 20);			
			
			AssertEquals ("H#7", 185, matrix.Elements[0]);
			AssertEquals ("H#8", 43, matrix.Elements[1]);
			AssertEquals ("H#9", 59, matrix.Elements[2]);
			AssertEquals ("H#10", 32, matrix.Elements[3]);
			AssertEquals ("H#11", 2, matrix.Elements[4]);
			AssertEquals ("H#12", 1, matrix.Elements[5]);			    
		}
		
		[Test]
		public void TransformPoints ()
		{
			Matrix matrix = new Matrix (2, 4, 6, 8, 10, 12);
			PointF [] pointsF = new PointF [] {new PointF (2, 4), new PointF (4, 8)};
			matrix.TransformPoints (pointsF);
						
			AssertEquals ("K#1", 38, pointsF[0].X);
			AssertEquals ("K#2", 52, pointsF[0].Y);
			AssertEquals ("K#3", 66, pointsF[1].X);
			AssertEquals ("K#4", 92, pointsF[1].Y);
			
			Point [] points = new Point [] {new Point (2, 4), new Point (4, 8)};
			matrix.TransformPoints (points);
			AssertEquals ("K#5", 38, pointsF[0].X);
			AssertEquals ("K#6", 52, pointsF[0].Y);
			AssertEquals ("K#7", 66, pointsF[1].X);
			AssertEquals ("K#8", 92, pointsF[1].Y);						    
		}
		
		[Test]
		public void TransformVectors  ()
		{
			Matrix matrix = new Matrix (2, 4, 6, 8, 10, 12);
			PointF [] pointsF = new PointF [] {new PointF (2, 4), new PointF (4, 8)};
			matrix.TransformVectors (pointsF);
						
			AssertEquals ("N#1", 28, pointsF[0].X);
			AssertEquals ("N#2", 40, pointsF[0].Y);
			AssertEquals ("N#3", 56, pointsF[1].X);
			AssertEquals ("N#4", 80, pointsF[1].Y);
			
			Point [] points = new Point [] {new Point (2, 4), new Point (4, 8)};
			matrix.TransformVectors (points);
			AssertEquals ("N#5", 28, pointsF[0].X);
			AssertEquals ("N#6", 40, pointsF[0].Y);
			AssertEquals ("N#7", 56, pointsF[1].X);
			AssertEquals ("N#8", 80, pointsF[1].Y);						    
		}		
		
		[Test]
		public void Translate  ()
		{
			Matrix matrix = new Matrix (2, 4, 6, 8, 10, 12);			
			matrix.Translate (5, 10);
						
			AssertEquals ("Y#1", 2, matrix.Elements[0]);
			AssertEquals ("Y#2", 4, matrix.Elements[1]);
			AssertEquals ("Y#3", 6, matrix.Elements[2]);
			AssertEquals ("Y#4", 8, matrix.Elements[3]);
			AssertEquals ("Y#5", 80, matrix.Elements[4]);
			AssertEquals ("Y#6", 112, matrix.Elements[5]);			
									    
		}			
	}
}

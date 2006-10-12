//
// UXTheme.cs
// - Internal class for P/Invokes to uxtheme.dll
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text;

namespace System.Windows.Forms.VisualStyles
{
	internal class UXTheme
	{
		#region DllImports
		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 CloseThemeData (IntPtr hTheme);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 DrawThemeBackground (IntPtr hTheme, IntPtr hdc, int iPartId,
		   int iStateId, ref RECT pRect, ref RECT pClipRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 DrawThemeBackground (IntPtr hTheme, IntPtr hdc, int iPartId,
		   int iStateId, ref RECT pRect, IntPtr pClipRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 DrawThemeEdge (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pDestRect, uint egde, uint flags, out RECT pRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 DrawThemeEdge (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pDestRect, uint edge, uint flags, int pRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 DrawThemeIcon (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, IntPtr himl, int iImageIndex);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 DrawThemeParentBackground (IntPtr hWnd, IntPtr hdc, ref RECT pRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 DrawThemeParentBackground (IntPtr hWnd, IntPtr hdc, int pRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 DrawThemeText (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, String text, int textLength, UInt32 textFlags, UInt32 textFlags2, ref RECT pRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 EnableTheming (int fEnable);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr OpenThemeData (IntPtr hWnd, String classList);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeBackgroundContentRect (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pBoundingRect, out RECT pContentRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeBackgroundExtent (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, ref RECT pClipRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeBackgroundRegion (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, out IntPtr pRegion);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeBool (IntPtr hTheme, int iPartId, int iStateId, int iPropId, out int pfVal);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeColor (IntPtr hTheme, int iPartId, int iStateId, int iPropId, out Int32 pColor);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeEnumValue (IntPtr hTheme, int iPartId, int iStateId, int iPropId, out int piVal);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeFilename (IntPtr hTheme, int iPartId, int iStateId, int iPropId, [MarshalAs (UnmanagedType.LPWStr)] Text.StringBuilder themeFileName, int themeFileNameLength);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeFont (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, [MarshalAs (UnmanagedType.LPStruct)] out LOGFONT lf);

		[DllImport ("gdi32", CharSet = CharSet.Auto)]
		public static extern IntPtr CreateFontIndirect ([In, MarshalAs (UnmanagedType.LPStruct)] LOGFONT lplf);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeInt (IntPtr hTheme, int iPartId, int iStateId, int iPropId, out int piVal);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeMargins (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int iPropId, out RECT prc, out MARGINS pMargins);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemePartSize (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref RECT pRect, int eSize, out SIZE size);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemePartSize (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, int pRect, int eSize, out SIZE size);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemePosition (IntPtr hTheme, int iPartId, int iStateId, int iPropId, out POINT pPoint);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeString (IntPtr hTheme, int iPartId, int iStateId, int iPropId, [MarshalAs (UnmanagedType.LPWStr)] Text.StringBuilder themeString, int themeStringLength);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeTextExtent (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, String text, int textLength, int textFlags, ref RECT boundingRect, out RECT extentRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeTextExtent (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, String text, int textLength, int textFlags, int boundingRect, out RECT extentRect);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeTextMetrics (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, out TEXTMETRIC textMetric);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 HitTestThemeBackground (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, UInt32 dwOptions, ref RECT pRect, IntPtr hrgn, POINT ptTest, out HitTestCode code);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static int IsThemeBackgroundPartiallyTransparent (IntPtr hTheme, int iPartId, int iStateId);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static bool IsThemePartDefined (IntPtr hTheme, int iPartId, int iStateId);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static bool IsThemeActive ();

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static bool IsAppThemed ();

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 HitTestThemeBackground (IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, UInt32 dwOptions, ref RECT pRect, IntPtr hrgn, POINT ptTest, out int code);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeDocumentationProperty (String stringThemeName, String stringPropertyName, StringBuilder stringValue, int lengthValue);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetCurrentThemeName (StringBuilder stringThemeName, int lengthThemeName, StringBuilder stringColorName, int lengthColorName, StringBuilder stringSizeName, int lengthSizeName);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static UInt32 GetThemeSysColor (IntPtr hTheme, int iColorId);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static Int32 GetThemeSysInt (IntPtr hTheme, int iIntId, out int piVal);

		[DllImport ("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public extern static int GetThemeSysBool (IntPtr hTheme, int iBoolId);
		#endregion
		
		#region RECT Type
		[Serializable, StructLayout (LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public RECT (int left, int top, int right, int bottom)
			{
				this.Left = left;
				this.Top = top;
				this.Right = right;
				this.Bottom = bottom;
			}

			#region Instance Properties
			public int Height { get { return Bottom - Top + 1; } }
			public int Width { get { return Right - Left + 1; } }
			public Size Size { get { return new Size (Width, Height); } }
			public Point Location { get { return new Point (Left, Top); } }
			#endregion

			#region Instance Methods
			public Rectangle ToRectangle ()
			{
				return Rectangle.FromLTRB (Left, Top, Right, Bottom);
			}

			public static RECT FromRectangle (Rectangle rectangle)
			{
				return new RECT (rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
			}

			public override int GetHashCode ()
			{
				return Left ^ ((Top << 13) | (Top >> 0x13))
				  ^ ((Width << 0x1a) | (Width >> 6))
				  ^ ((Height << 7) | (Height >> 0x19));
			}
			#endregion

			#region Operator overloads
			public static implicit operator Rectangle (RECT rect)
			{
				return Rectangle.FromLTRB (rect.Left, rect.Top, rect.Right, rect.Bottom);
			}

			public static implicit operator RECT (Rectangle rect)
			{
				return new RECT (rect.Left, rect.Top, rect.Right, rect.Bottom);
			}
			#endregion
		}
		#endregion

		#region LOGFONT Type
		[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class LOGFONT
		{
			public int lfHeight = 0;
			public int lfWidth = 0;
			public int lfEscapement = 0;
			public int lfOrientation = 0;
			public int lfWeight = 0;
			public byte lfItalic = 0;
			public byte lfUnderline = 0;
			public byte lfStrikeOut = 0;
			public byte lfCharSet = 0;
			public byte lfOutPrecision = 0;
			public byte lfClipPrecision = 0;
			public byte lfQuality = 0;
			public byte lfPitchAndFamily = 0;
			[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 32)]
			public string lfFaceName = string.Empty;
		}
		#endregion

		#region MARGINS Type
		[StructLayout (LayoutKind.Sequential)]
		public struct MARGINS
		{
			public int leftWidth;
			public int rightWidth;
			public int topHeight;
			public int bottomHeight;

			public Padding ToPadding ()
			{
				return new Padding (leftWidth, topHeight, rightWidth, bottomHeight);
			}
		}
		#endregion
		
		#region SIZE Type
		[StructLayout (LayoutKind.Sequential)]
		public struct SIZE
		{
			public int cx;
			public int cy;

			public Size ToSize ()
			{
				return new Size (cx, cy);
			}
		}
		#endregion
		
		#region POINT Type
		[StructLayout (LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT (int x, int y)
			{
				this.X = x;
				this.Y = y;
			}

			public Point ToPoint ()
			{
				return new Point (X, Y);
			}

			public static implicit operator System.Drawing.Point (POINT p)
			{
				return new System.Drawing.Point (p.X, p.Y);
			}

			public static implicit operator POINT (System.Drawing.Point p)
			{
				return new POINT (p.X, p.Y);
			}
		}
		#endregion
		
		#region TEXTMETRIC Type
		[Serializable, StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct TEXTMETRIC
		{
			public int tmHeight;
			public int tmAscent;
			public int tmDescent;
			public int tmInternalLeading;
			public int tmExternalLeading;
			public int tmAveCharWidth;
			public int tmMaxCharWidth;
			public int tmWeight;
			public int tmOverhang;
			public int tmDigitizedAspectX;
			public int tmDigitizedAspectY;
			public char tmFirstChar;
			public char tmLastChar;
			public char tmDefaultChar;
			public char tmBreakChar;
			public byte tmItalic;
			public byte tmUnderlined;
			public byte tmStruckOut;
			public byte tmPitchAndFamily;
			public byte tmCharSet;
		}
		#endregion
	}
}
#endif
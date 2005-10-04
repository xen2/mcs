//
// System.Drawing.gdipEnums.cs
//
// Author: 
// Alexandre Pigolkine (pigolkine@gmx.de)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Drawing {
	/// <summary>
	/// GDI+ API enumerations
	/// </summary>
	
	#region Status
	internal enum Status {
		Ok = 0,
		GenericError = 1,
		InvalidParameter = 2,
		OutOfMemory = 3,
		ObjectBusy = 4,
		InsufficientBuffer = 5,
		NotImplemented = 6,
		Win32Error = 7,
		WrongState = 8,
		Aborted = 9,
		FileNotFound = 10,
		ValueOverflow = 11,
		AccessDenied = 12,
		UnknownImageFormat = 13,
		FontFamilyNotFound = 14,
		FontStyleNotFound = 15,
		NotTrueTypeFont = 16,
		UnsupportedGdiplusVersion = 17,
		GdiplusNotInitialized = 18,
		PropertyNotFound = 19,
		PropertyNotSupported = 20,
		ProfileNotFound = 21
	}
	#endregion
	
	#region Unit
	internal enum Unit
	{
		UnitWorld		= 0,
		UnitDisplay 	= 1,
		UnitPixel 		= 2,
		UnitPoint		= 3,
		UnitInch		= 4,
		UnitDocument	= 5,
		UnitMillimeter	= 6
	};
	#endregion

        #region BrushType
        internal enum BrushType {
                BrushTypeSolidColor = 0,
                BrushTypeHatchFill = 1,
                BrushTypeTextureFill = 2,
                BrushTypePathGradient = 3,
                BrushTypeLinearGradient = 4
        }
        #endregion
	
}

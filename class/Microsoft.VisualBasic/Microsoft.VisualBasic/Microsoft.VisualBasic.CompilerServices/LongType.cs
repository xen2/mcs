//
// LongType.cs
//
// Author:
//  Chris J Breisch (cjbreisch@altavista.net) 
//	Dennis Hayes	(dennish@raytek.com)
//
//  (C) 2002 Chris J Breisch
//
 /*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
  * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */

using System;
using System.ComponentModel;

namespace Microsoft.VisualBasic.CompilerServices
{
	[StandardModule, EditorBrowsable(EditorBrowsableState.Never)] 
	sealed public class LongType {
		private LongType () {}

		// Methods
		/**
		 * The method converts given object to long by the following logic:
		 * 1. If input object is null - return 0
		 * 2. If input object is String - run FromString method
		 * 3. Otherwise run .NET default conversion - Convert.ToInt64
		 * @param value - The object that going to be converted
		 * @return long The long value that converted from the source object
		 * @see system.Convert#ToInt64
		 */
		public static long FromObject(object Value) {
			if (Value == null)return 0;

			if (Value is string)return FromString((string) Value);

			return Convert.ToInt64(Value);
		}
		/**
		 * The method try to convert given string to long in a following way:
		 * 1. If input string is null return 0.
		 * 2. If input string represents number: return value of this number, 
		 * @exception InvalidCastException - in case if number translation failed 
		 * @param str - The string that converted to long
		 * @return long The value that extracted from the input string.
		 * @see Microsoft.VisualBasic.VBUtils#isNumber
		 */
		public static long FromString(string Value) {
			if (Value == null)return 0;

			return long.Parse(Value); 
		}

	}
}

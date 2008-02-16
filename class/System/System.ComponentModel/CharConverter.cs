//
// System.ComponentModel.CharConverter
//
// Author:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//  Ivan N. Zlatev (contact@i-nz.net)
//
// (C) 2003 Andreas Nahr
// (C) 2008 Novell, Inc. (http://www.novell.com)
//

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

using System.Globalization;

namespace System.ComponentModel
{
	public class CharConverter : TypeConverter  
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string)) 
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value.GetType() == typeof (string)) {
				if (value == null)
					return '\0';
				string str = (String) value;
				if (str.Length > 1)
					str = str.Trim ();
				if (str.Length > 1)
					throw new FormatException ("String has to be less than or equal to one char long");
				if (str.Length == 0)
					return '\0';
				return str[0];
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string)) {
				if (value != null && value is char) {
					if ((char)value == '\0')
						return String.Empty;
					else
						return new string ((char) value, 1);
				}
			}

			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}


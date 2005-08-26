//
// System.Web.UI.WebControls.FontUnit.cs
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//   Ben Maurer (bmaurer@ximian.com).
//
// (C) 2005 Novell, Inc.
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

using System;
using System.Threading;
using System.Globalization;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	[TypeConverter  (typeof (FontUnitConverter))]
#if NET_2_0
	[Serializable]
#endif
	public struct FontUnit {
		FontSize type;
		Unit unit;
		
		public static readonly FontUnit Empty;
		public static readonly FontUnit Smaller = new FontUnit (FontSize.Smaller);
		public static readonly FontUnit Larger = new FontUnit (FontSize.Larger);
		public static readonly FontUnit XXSmall = new FontUnit (FontSize.XXSmall);
		public static readonly FontUnit XSmall = new FontUnit (FontSize.XSmall);
		public static readonly FontUnit Small = new FontUnit (FontSize.Small);
		public static readonly FontUnit Medium = new FontUnit (FontSize.Medium);
		public static readonly FontUnit Large = new FontUnit (FontSize.Large);
		public static readonly FontUnit XLarge = new FontUnit (FontSize.XLarge);
		public static readonly FontUnit XXLarge = new FontUnit (FontSize.XXLarge);
		
		public FontUnit (FontSize type)
		{
			int t = (int) type;
			
			if (t < 0 || t > (int)FontSize.XXLarge)
				throw new ArgumentOutOfRangeException ("type");
			
			this.type = type;

			if (type == FontSize.AsUnit)
				unit = new Unit (10, UnitType.Point);
			else
				unit = Unit.Empty;
		}

		public FontUnit (int value) : this (new Unit (value, UnitType.Point))
		{
		}

#if NET_2_0
		public FontUnit (double value) : this (new Unit (value, UnitType.Point))
		{
		}

		public FontUnit (double value, UnitType type) : this (new Unit (value, type))
		{
		}
#endif

		public FontUnit (Unit value)
		{
			type = FontSize.AsUnit;
			unit = value;
		}
		
		public FontUnit (string value) : this (value, Thread.CurrentThread.CurrentCulture) {}

		public FontUnit (string value, CultureInfo culture)
		{
			if (value == null || value == String.Empty){
				type = FontSize.NotSet;
				unit = Unit.Empty;
				return;
			}

			switch (value.ToLower (CultureInfo.InvariantCulture)){
			case "smaller": type = FontSize.Smaller; break;
			case "larger": type = FontSize.Larger; break;
			case "xxsmall": type = FontSize.XXSmall; break;
			case "xsmall": type = FontSize.XSmall; break;
			case "small": type = FontSize.Small; break;
			case "medium": type = FontSize.Medium; break;
			case "large": type = FontSize.Large; break;
			case "xlarge": type = FontSize.XLarge; break;
			case "xxlarge": type = FontSize.XXLarge; break;
			default:
				type = FontSize.AsUnit;
				unit = new Unit (value, culture);
				return;
			}
			unit = Unit.Empty;
		}
		
		public bool IsEmpty {
			get {
				return type == FontSize.NotSet;
			}
		}

		public FontSize Type {
			get {
				return type;
			}
		}

		public Unit Unit {
			get {
				return unit;
			}
		}
		
		public static FontUnit Parse (string s)
		{
			return new FontUnit (s);
		}

		public static FontUnit Parse (string s, CultureInfo culture)
		{
			return new FontUnit (s, culture);
		}

		public static FontUnit Point (int n)
		{
			return new FontUnit (n);
		}
		public override bool Equals (object obj)
		{
			if (obj is FontUnit){
				FontUnit other = (FontUnit) obj;
				return (other.type == type && other.unit == unit);
			}
			return false;
		}
		
		public override int GetHashCode ()
		{
			return type.GetHashCode () ^ unit.GetHashCode ();
		}
		
		public static bool operator == (FontUnit left, FontUnit right)
		{
			return left.type == right.type && left.unit == right.unit;
		}

		public static bool operator != (FontUnit left, FontUnit right)
		{
			return left.type != right.type || left.unit != right.unit;
		}
		
		public static implicit operator FontUnit (int n)
		{
			return new FontUnit (n);
		}

#if NET_2_0
		public string ToString (IFormatProvider fmt)
		{
			if (type == FontSize.NotSet)
				return "";
			else if (type == FontSize.AsUnit)
				return unit.ToString (fmt);
			else
				return type.ToString();
		}
#endif

		public string ToString (CultureInfo culture)
		{
			if (type == FontSize.NotSet)
				return "";

			if (type == FontSize.AsUnit)
				return unit.ToString (culture);

			return type.ToString ();
		}
			
		public override string ToString ()
		{
			return ToString (CultureInfo.InvariantCulture);
		}
		
	}

}

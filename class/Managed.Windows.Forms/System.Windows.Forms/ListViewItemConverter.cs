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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Ravindra (rkumar@novell.com)
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: ListViewItemConverter.cs,v $
// Revision 1.1  2004/09/30 13:26:35  ravindra
// Converter for ListViewItem.
//
//
// NOTE COMPLETE
//

using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Forms
{
	public class ListViewItemConverter : ExpandableObjectConverter
	{
		#region Public Constructors
		public ListViewItemConverter () { }
		#endregion	// Public Constructors

		#region Public Instance Methods
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture, object value,
						  Type destinationType)
		{
			if (destinationType == typeof (string))
				return value.ToString ();
			else
				return base.ConvertTo (context, culture, value, destinationType);
		}
		#endregion	// Public Instance Methods
	}
}

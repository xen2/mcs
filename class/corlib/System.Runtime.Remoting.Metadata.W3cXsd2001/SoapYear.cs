//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapYear
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
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
using System.Globalization;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
	[Serializable]
	public sealed class SoapYear : ISoapXsd
	{
		static readonly string[] _datetimeFormats = new string[]
		{
			"yyyy",
			"'+'yyyy",
			"'-'yyyy",
			"yyyyzzz",
			"'+'yyyyzzz",
			"'-'yyyyzzz"
		};
		
		int _sign;
		DateTime _value;
		
		public SoapYear()
		{
		}

		public SoapYear (DateTime date)
		{
			_value = date;
		}

		public SoapYear (DateTime date, int sign)
		{
			_value = date;
			_sign = sign;
		}

		public int Sign {
			get { return _sign; } 
			set { _sign = value; }
		}
		
		public DateTime Value {
			get { return _value; } 
			set { _value = value; }
		}

		public static string XsdType {
			get { return "gYear"; }
		}

		public string GetXsdType()
		{
			return XsdType;
		}
		
		public static SoapYear Parse (string value)
		{
			DateTime d = DateTime.ParseExact (value, _datetimeFormats, null, DateTimeStyles.None);
			
			SoapYear res = new SoapYear (d);
			if (value.StartsWith ("-")) res.Sign = -1;
			else res.Sign = 0;
			return res;
		}

		public override string ToString()
		{
			if (_sign >= 0)
				return _value.ToString("yyyy", CultureInfo.InvariantCulture);
			else
				return _value.ToString("'-'yyyy", CultureInfo.InvariantCulture);
		}
	}
}

//
// System.Web.UI.DataBinding.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
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

namespace System.Web.UI {

	public sealed class DataBinding
	{
		string propertyName;
		Type propertyType;
		string expression;

		public DataBinding (string propertyName, Type propertyType,
				    string expression)
		{
			this.propertyName = propertyName;
			this.propertyType = propertyType;
			this.expression = expression;
		}

		public string Expression {
			get { return expression; }
			set { expression = value; }
		}

		public string PropertyName {
			get { return propertyName; }
		}

		public Type PropertyType {
			get { return propertyType; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is DataBinding))
				return false;
			
			DataBinding o = (DataBinding) obj;
			return (o.Expression == expression &&
				o.PropertyName == propertyName &&
				o.PropertyType == propertyType);
		}

		public override int GetHashCode ()
		{
			return propertyName.GetHashCode () +
			       (propertyType.GetHashCode () << 1) +
			       (expression.GetHashCode () << 2) ;
		}
	}
}

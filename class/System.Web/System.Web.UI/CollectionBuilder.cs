//
// System.Web.UI.CollectionBuilder.cs
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
using System.Collections;
using System.Reflection;

namespace System.Web.UI
{
	sealed class CollectionBuilder : ControlBuilder
	{
		Type elementType;

		internal CollectionBuilder ()
		{
		}

		public override void AppendLiteralString (string s)
		{
			if (s != null && s.Trim () != "")
				throw new HttpException ("Literal content not allowed for " + ControlType);
		}

		public override Type GetChildControlType (string tagName, IDictionary attribs)
		{
			Type t = Root.GetChildControlType (tagName, attribs);
			if (elementType != null && !elementType.IsAssignableFrom (t)) 
				throw new HttpException ("Cannot add a " + t + " to " + elementType);

			return t;
		}

		public override void Init (TemplateParser parser,
					   ControlBuilder parentBuilder,
					   Type type,
					   string tagName,
					   string id,
					   IDictionary attribs)
		{
			base.Init (parser, parentBuilder, type, tagName, id, attribs);

			PropertyInfo prop = parentBuilder.ControlType.GetProperty (tagName, flagsNoCase);
			SetControlType (prop.PropertyType);

			MemberInfo[] mems = ControlType.GetMember ("Item", MemberTypes.Property, flagsNoCase & ~BindingFlags.IgnoreCase);
			if (mems.Length > 0) prop = (PropertyInfo) mems [0];
			else throw new HttpException ("Collection of type '" + ControlType + "' does not have an indexer.");
			
			elementType = prop.PropertyType;
		}
	}
}


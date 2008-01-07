//
// System.Web.UI.ClientScriptManager.cs
//
// Authors:
//   Igor Zelmanovich (igorz@mainsoft.com)
//
// (c) 2008 Mainsoft, Inc. (http://www.mainsoft.com)
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

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Web.UI
{
#if (TARGET_JVM || TARGET_DOTNET)
	public
#endif
	interface IScriptManager
	{
		void RegisterOnSubmitStatement (Control control, Type type, string key, string script);
		void RegisterExpandoAttribute (Control control, string controlId, string attributeName, string attributeValue, bool encode);
		void RegisterHiddenField (Control control, string hiddenFieldName, string hiddenFieldInitialValue);
		void RegisterStartupScript (Control control, Type type, string key, string script, bool addScriptTags);
		void RegisterArrayDeclaration (Control control, string arrayName, string arrayValue);
		void RegisterClientScriptBlock (Control control, Type type, string key, string script, bool addScriptTags);
		void RegisterClientScriptInclude (Control control, Type type, string key, string url);
		void RegisterClientScriptResource (Control control, Type type, string resourceName);
	}
}
#endif
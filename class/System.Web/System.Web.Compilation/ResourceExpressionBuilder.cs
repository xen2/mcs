//
// System.Web.Compilation.ResourceExpressionBuilder
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.CodeDom;
using System.Web;
using System.Web.UI;

namespace System.Web.Compilation {

	[ExpressionEditor("System.Web.UI.Design.ResourceExpressionEditor, " + Consts.AssemblySystem_Design)]
	[ExpressionPrefix("Resources")]
	public class ResourceExpressionBuilder : ExpressionBuilder {

		public ResourceExpressionBuilder ()
		{
		}

		public override object EvaluateExpression (object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			ResourceExpressionFields fields = parsedData as ResourceExpressionFields;
			return HttpContext.GetGlobalResourceObject ("Resources." + fields.ClassKey, fields.ResourceKey);
		}

		public override CodeExpression GetCodeExpression (BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			ResourceExpressionFields fields = parsedData as ResourceExpressionFields;
			return new CodeMethodInvokeExpression (
				new CodeThisReferenceExpression (),
				"GetGlobalResourceObject",
				new CodeExpression [] { new CodePrimitiveExpression ("Resources." + fields.ClassKey),
							new CodePrimitiveExpression (fields.ResourceKey) });
		}

		public static ResourceExpressionFields ParseExpression (string expression)
		{
			int comma = expression.IndexOf (',');
			if (comma == -1)
				return new ResourceExpressionFields (expression.Trim ());
			else
				return new ResourceExpressionFields (expression.Substring (0, comma).Trim (),
								     expression.Substring (comma + 1).Trim ());
		}

		public override object ParseExpression (string expression, Type propertyType, ExpressionBuilderContext context)
		{
			//FIXME: not sure what the propertyType should be used for
			return ParseExpression (expression);
		}

		public override bool SupportsEvaluate {
			get { return true; }
		}
	}

}

#endif



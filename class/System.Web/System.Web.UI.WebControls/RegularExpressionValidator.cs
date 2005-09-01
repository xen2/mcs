//
// System.Web.UI.WebControls.RegularExpressionValidator
//
// Authors:
//	Chris Toshok (toshok@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Web;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

namespace System.Web.UI.WebControls {
#if NET_2_0
	[ToolboxData ("<{0}:RegularExpressionValidator runat=\"server\" ErrorMessage=\"RegularExpressionValidator\"></{0}:RegularExpressionValidator>")]
#else
	[ToolboxData ("<{0}:RegularExpressionValidator runat=server ErrorMessage=\"RegularExpressionValidator\"></{0}:RegularExpressionValidator>")]
#endif
	public class RegularExpressionValidator : BaseValidator
	{
		public RegularExpressionValidator ()
		{
		}

		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			if (RenderUplevel) {
				w.AddAttribute ("evaluationfunction", "RegularExpressionValidatorEvaluateIsValid");
				if (ValidationExpression != "")
					w.AddAttribute ("validationexpression", ValidationExpression);
			}

			base.AddAttributesToRender (w);
		}

		protected override bool EvaluateIsValid ()
		{
			if (GetControlValidationValue (ControlToValidate).Trim() == "")
				return true;

			return Regex.IsMatch (GetControlValidationValue(ControlToValidate), ValidationExpression);
		}

		[Bindable(true)]
		[DefaultValue ("")]
		[Editor ("System.Web.UI.Design.WebControls.RegexTypeEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public string ValidationExpression {
			get {
				return ViewState.GetString ("ValidationExpression", "");
			}
			set {
				ViewState ["ValidationExpression"] = value;
			}
		}
	}
}

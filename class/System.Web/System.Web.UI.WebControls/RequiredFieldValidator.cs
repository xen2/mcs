//
// System.Web.UI.WebControls.RequiredFieldValidator
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
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
using System.Security.Permissions;

namespace System.Web.UI.WebControls {
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
#if NET_2_0
	[ToolboxData ("<{0}:RequiredFieldValidator runat=\"server\" ErrorMessage=\"RequiredFieldValidator\"></{0}:RequiredFieldValidator>")]
#else
	[ToolboxData ("<{0}:RequiredFieldValidator runat=server ErrorMessage=\"RequiredFieldValidator\"></{0}:RequiredFieldValidator>")]
#endif
	public class RequiredFieldValidator : BaseValidator {
		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			if (RenderUplevel) {
#if NET_2_0
				Page.ClientScript.RegisterExpandoAttribute (ClientID, "evaluationfunction", "RequiredFieldValidatorEvaluateIsValid");
				Page.ClientScript.RegisterExpandoAttribute (ClientID, "initialvalue", InitialValue);
#else
				w.AddAttribute ("evaluationfunction", "RequiredFieldValidatorEvaluateIsValid", false);
				w.AddAttribute ("initialvalue", InitialValue);
#endif
			}

			base.AddAttributesToRender (w);
		}
		
		protected override bool EvaluateIsValid ()
		{
			return GetControlValidationValue (ControlToValidate) != InitialValue;
		}
		

#if NET_2_0
		[Themeable(false)]
#else
		[Bindable(true)]
#endif
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public string InitialValue {
			get {
				return ViewState.GetString ("InitialValue", "");
			}
			set {
				ViewState ["InitialValue"] = value;
			}
		}
	}
}

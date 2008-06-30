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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent("ServerValidate")]
#if NET_2_0
	[ToolboxData("<{0}:CustomValidator runat=\"server\" ErrorMessage=\"CustomValidator\"></{0}:CustomValidator>")]
#else
	[ToolboxData("<{0}:CustomValidator runat=server ErrorMessage=\"CustomValidator\"></{0}:CustomValidator>")]
#endif
	public class CustomValidator : BaseValidator {
		static readonly object serverValidateEvent = new object ();

		EventHandlerList events = new EventHandlerList ();
		
		#region Events
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
			public event ServerValidateEventHandler ServerValidate {
			add { events.AddHandler (serverValidateEvent, value); }
			remove { events.RemoveHandler (serverValidateEvent, value); }
		}
		
		#endregion	// Events

		#region Public Constructors
		public CustomValidator() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
#if NET_2_0
		[Themeable (false)]
#endif
		public string ClientValidationFunction {
			get { return ViewState.GetString("ClientValidationFunction", ""); }
			set { ViewState["ClientValidationFunction"] = value; }
		}

#if NET_2_0
		[Themeable (false)]
		[DefaultValue (false)]
		public bool ValidateEmptyText {
			get { return ViewState.GetBool ("ValidateEmptyText", false); }
			set { ViewState ["ValidateEmptyText"] = value; }
		}
#endif
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		protected override void AddAttributesToRender(HtmlTextWriter writer) {
			base.AddAttributesToRender (writer);

			if (base.RenderUplevel) {
				string s;
				
#if NET_2_0
				RegisterExpandoAttribute (ClientID, "evaluationfunction", "CustomValidatorEvaluateIsValid");
				if (ValidateEmptyText)
					RegisterExpandoAttribute (ClientID, "validateemptytext", "true");
				s = ClientValidationFunction;
				if (!String.IsNullOrEmpty (s))
					RegisterExpandoAttribute (ClientID, "clientvalidationfunction", s, true);
#else
				writer.AddAttribute ("evaluationfunction", "CustomValidatorEvaluateIsValid", false);

				s = ClientValidationFunction;
				if (s != String.Empty)
					writer.AddAttribute("clientvalidationfunction", s);
#endif
			}
		}

		protected override bool ControlPropertiesValid ()
		{
			if (ControlToValidate == "")
				return true;
			return base.ControlPropertiesValid ();
		}

		protected override bool EvaluateIsValid() {
			if (ControlToValidate.Length > 0) {
				string value = GetControlValidationValue (ControlToValidate);
#if NET_2_0
				if (String.IsNullOrEmpty (value) && !ValidateEmptyText)
					return true;
#endif
				return OnServerValidate (value);
			}
			return OnServerValidate(string.Empty);
		}

		protected virtual bool OnServerValidate(string value) {
			ServerValidateEventHandler eh = events [serverValidateEvent] as ServerValidateEventHandler;
			if (eh != null) {
				ServerValidateEventArgs	e;

				e = new ServerValidateEventArgs(value, true);
				eh (this, e);
				return e.IsValid;
			}
			return true;
		}
		#endregion	// Public Instance Methods
	}
}

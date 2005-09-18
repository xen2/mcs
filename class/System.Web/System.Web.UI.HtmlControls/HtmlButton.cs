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
//
// System.Web.UI.HtmlControls.HtmlButton
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc.


using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent("ServerClick")]
	public class HtmlButton : HtmlContainerControl, IPostBackEventHandler {

		private static readonly object ServerClickEvent = new object();

		public HtmlButton () : base ("button")
		{
		}

		[DefaultValue(true)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
#if NET_2_0
		public virtual
#else		
		public
#endif		
		bool CausesValidation {
			get {
				return ViewState.GetBool ("CausesValidation", true);
			}
			set {
				ViewState ["CausesValidation"] = value;
			}
		}

#if NET_2_0
		[DefaultValue ("")]
		public string ValidationGroup 
		{
			get {
				return ViewState.GetString ("ValidationGroup", "");
			}
			set {
				ViewState ["ValidationGroup"] = value;
			}
		}
#endif		

#if NET_2_0
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}

		protected virtual void RaisePostBackEvent (string eventArgument)
#else
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
#endif
		{
			if (CausesValidation)
#if NET_2_0
				Page.Validate (ValidationGroup);
#else
				Page.Validate ();
#endif
			OnServerClick (EventArgs.Empty);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected virtual void OnServerClick (EventArgs e)
		{
			EventHandler server_click = (EventHandler) Events [ServerClickEvent];
			if (server_click != null)
				server_click (this, e);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			ClientScriptManager csm = new ClientScriptManager (Page);
			bool postback = false;

			if (Page != null && Events [ServerClickEvent] != null)
				postback = true;

			if (CausesValidation && Page != null && Page.AreValidatorsUplevel ()) {
				if (postback)
					writer.WriteAttribute ("onclick",
							       String.Format ("javascript:{{if (typeof(Page_ClientValidate) != 'function' ||  Page_ClientValidate()) {0}}}",
									      csm.GetPostBackEventReference (this, String.Empty)));
				else
					writer.WriteAttribute ("onclick",
							       "if (typeof(Page_ClientValidate) == 'function') Page_ClientValidate();");

				writer.WriteAttribute ("language", "javascript");
			}
			else if (postback) {
				writer.WriteAttribute ("onclick",
						       Page.ClientScript.GetPostBackClientHyperlink (this, ""));

				writer.WriteAttribute ("language", "javascript");
			}

			base.RenderAttributes (writer);
		}

		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerClick {
			add { Events.AddHandler (ServerClickEvent, value); }
			remove { Events.RemoveHandler (ServerClickEvent, value); }
		}
	}

}


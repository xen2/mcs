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
// System.Web.UI.HtmlControls.HtmlInputImage.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc.


//
// TODO: getting the .x and .y in LoadData doesn't work with mozilla
//

using System.Globalization;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent("ServerClick")]
	public class HtmlInputImage : HtmlInputControl, IPostBackDataHandler,
		      IPostBackEventHandler {

		private static readonly object ServerClickEvent = new object ();

		private int clicked_x;
		private int clicked_y;

		public HtmlInputImage () : base ("image")
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

		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public string Align {
			get { return GetAtt ("align"); }
			set { SetAtt ("align", value); }
		}

		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		[Localizable (true)]
#endif		
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public string Alt {
			get { return GetAtt ("alt"); }
			set { SetAtt ("alt", value); }
		}

		[DefaultValue("")]
		[WebSysDescription("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebCategory("Appearance")]
		public string Src {
			get { return GetAtt ("src"); }
			set { SetAtt ("src", value); }
		}

#if NET_2_0
		[DefaultValue("-1")]
#else		
		[DefaultValue("")]
#endif		
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public int Border {
			get {
				string border = Attributes ["border"];
				if (border == null)
					return -1;
				return Int32.Parse (border, CultureInfo.InvariantCulture);
			}
			set {
				if (value == -1) {
					Attributes.Remove ("border");
					return;
				}
				Attributes ["border"] = value.ToString (CultureInfo.InvariantCulture);
			}
		}

		bool LoadPostDataInternal (string postDataKey, NameValueCollection postCollection)
		{
			string x = postCollection [UniqueID + ".x"];
			string y = postCollection [UniqueID + ".y"];

			if (x != null && x.Length != 0 &&
					y != null && y.Length != 0) {
				clicked_x = Int32.Parse (x, CultureInfo.InvariantCulture);
				clicked_y = Int32.Parse (y, CultureInfo.InvariantCulture);
				Page.RegisterRequiresRaiseEvent (this);
				return true;
			}

			return false;
		}

		
		void RaisePostBackEventInternal (string eventArgument)
		{
			if (CausesValidation)
#if NET_2_0
				Page.Validate (ValidationGroup);
#else
				Page.Validate ();
#endif
			OnServerClick (new ImageClickEventArgs (clicked_x, clicked_y));
		}

		void RaisePostDataChangedEventInternal ()
		{
			/* no events to raise */
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

		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostDataInternal (postDataKey, postCollection);
		}

		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEventInternal (eventArgument);
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEventInternal ();
		}
#endif		

		bool IPostBackDataHandler.LoadPostData (string postDataKey,
				NameValueCollection postCollection)
		{
			return LoadPostDataInternal (postDataKey, postCollection);
		}

		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEventInternal ();
		}
				
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEventInternal (eventArgument);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Page != null)
				Page.RegisterRequiresPostBack (this);
		}

		protected virtual void OnServerClick (ImageClickEventArgs e)
		{
			EventHandler handler = Events [ServerClickEvent] as EventHandler;
			if (handler != null)
				handler (this, e);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			if (CausesValidation && Page != null && Page.AreValidatorsUplevel ()) {
				ClientScriptManager csm = new ClientScriptManager (Page);
				writer.WriteAttribute ("onclick", csm.GetClientValidationEvent ());
			}

			base.RenderAttributes (writer);
		}

		private void SetAtt (string name, string value)
		{
			if (value == null)
				Attributes.Remove (name);
			else
				Attributes [name] = value;
		}

		private string GetAtt (string name)
		{
			string res = Attributes [name];
			if (res == null)
				return String.Empty;
			return res;
		}

		[WebSysDescription("")]
		[WebCategory("Action")]
		public event ImageClickEventHandler ServerClick {
			add { Events.AddHandler (ServerClickEvent, value); }
			remove { Events.AddHandler (ServerClickEvent, value); }
		}
	}
}


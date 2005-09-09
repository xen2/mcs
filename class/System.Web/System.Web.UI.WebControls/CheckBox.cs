//
// System.Web.UI.WebControls.CheckBox.cs
//
// Author:
//      Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Web.UI;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	[Designer ("System.Web.UI.Design.WebControls.CheckBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DataBindingHandler ("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultEvent ("CheckedChanged")]
	[DefaultProperty ("Text")]
#if NET_2_0
	[ControlValueProperty ("Checked", null)]
#endif		
	public class CheckBox : WebControl, IPostBackDataHandler
#if NET_2_0
	, ICheckBoxControl
#endif
	{
		string render_type;

#if NET_2_0
		AttributeCollection inputAttributes;
		StateBag inputAttributesState;
		AttributeCollection labelAttributes;
		StateBag labelAttributesState;
#endif
		
		public CheckBox () : base (HtmlTextWriterTag.Input)
		{
			render_type = "checkbox";
		}

		internal CheckBox (string render_type) : base (HtmlTextWriterTag.Input)
		{
			this.render_type = render_type;
		}

		[DefaultValue (false)]
#if NET_2_0
		[Themeable (false)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AutoPostBack 
		{
			get {
				return (ViewState.GetBool ("AutoPostBack",
							   false));
			}
			set {
				ViewState["AutoPostBack"] = value;
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		[Themeable (false)]
		[MonoTODO]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool CausesValidation 
		{
			get { return ViewState.GetBool ("CausesValidation", false); }
			set { ViewState ["CausesValidation"] = value; }
		}
#endif		
		

		[DefaultValue (false)]
#if NET_2_0
		[Bindable (true, BindingDirection.TwoWay)]
		[Themeable (false)]
#else		
		[Bindable (true)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool Checked 
		{
			get {
				return (ViewState.GetBool ("Checked", false));
			}
			set {
				ViewState["Checked"] = value;
			}
		}

#if NET_2_0
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AttributeCollection InputAttributes 
		{
			get {
				if (inputAttributes == null) {
					if (inputAttributesState == null) {
						inputAttributesState = new StateBag (true);
						if (IsTrackingViewState)
							inputAttributesState.TrackViewState();
					}
					inputAttributes = new AttributeCollection (inputAttributesState);
				}
				return inputAttributes;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AttributeCollection LabelAttributes
		{
			get {
				if (labelAttributes == null) {
					if (labelAttributesState == null) {
						labelAttributesState = new StateBag (true);
						if (IsTrackingViewState)
							labelAttributesState.TrackViewState();
					}
					labelAttributes = new AttributeCollection (labelAttributesState);
				}
				return labelAttributes;
			}
		}
#endif		

		[DefaultValue ("")]
		[Bindable (true)]
#if NET_2_0
		[Localizable (true)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string Text 
		{
			get {
				return (ViewState.GetString ("Text",
							     String.Empty));
			}
			set {
				ViewState["Text"] = value;
			}
		}

		[DefaultValue (TextAlign.Right)]
#if ONLY_1_1
		[Bindable (true)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual TextAlign TextAlign
		{
			get {
				object o = ViewState["TextAlign"];

				if (o == null) {
					return (TextAlign.Right);
				} else {
					return ((TextAlign)o);
				}
			}
			set {
				if (value != TextAlign.Left &&
				    value != TextAlign.Right) {
					throw new ArgumentOutOfRangeException ("value");
				}
				
				ViewState["TextAlign"] = value;
			}
		}

#if NET_2_0
		[Themeable (false)]
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public string ValidationGroup
		{
			get { return ViewState.GetString ("ValidationGroup", String.Empty); }
			set { ViewState["ValidationGroup"] = value; }
		}
#endif		

		private static readonly object EventCheckedChanged = new object ();
		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event EventHandler CheckedChanged 
		{
			add {
				Events.AddHandler (EventCheckedChanged, value);
			}
			remove {
				Events.RemoveHandler (EventCheckedChanged, value);
			}
		}

		protected virtual void OnCheckedChanged (EventArgs e)
		{
			EventHandler handler = (EventHandler)Events[EventCheckedChanged];
			
			if (handler != null) {
				handler (this, e);
			}
		}

		internal virtual string NameAttribute 
		{
			get {
				return (this.UniqueID);
			}
		}
		
		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);

			/* This is a nasty kludge to avoid rendering
			 * "style" and the ControlStyle in checkboxes
			 * (we use a surrounding <span> instead), but
			 * still pass the unit test that shows the
			 * style being rendered in multiple calls to
			 * Render () (which means we can't just delete
			 * Attributes["style"] or ControlStyle.Reset()
			 * in Render ())
			 */
			string css_style = Attributes ["style"];
			if (css_style != null) {
				Attributes.Remove ("style");
			}

			Style style = new Style ();
			if (ControlStyleCreated) {
				style.CopyFrom (ControlStyle);
				ControlStyle.Reset ();
			}
			
			base.AddAttributesToRender (w);

			if (css_style != null) {
				Attributes ["style"] = css_style;
			}
			if (!style.IsEmpty) {
				ApplyStyle (style);
			}
			
			InternalAddAttributesToRender (w);
			
			w.AddAttribute (HtmlTextWriterAttribute.Type,
					render_type);
			w.AddAttribute (HtmlTextWriterAttribute.Name,
					NameAttribute);
			
			if (AutoPostBack) {
				w.AddAttribute (HtmlTextWriterAttribute.Onclick, Page.ClientScript.GetPostBackClientHyperlink (this, ""));
			}

			if (Checked) {
				w.AddAttribute (HtmlTextWriterAttribute.Checked, "checked");
			}
		}

#if NET_2_0
		protected override void LoadViewState (object savedState)
		{
			if (savedState == null) {
				base.LoadViewState (null);
				return;
			}

			Triplet saved = (Triplet) savedState;
			base.LoadViewState (saved.First);

			if (saved.Second != null) {
				if (inputAttributesState == null) {
					inputAttributesState = new StateBag(true);
					inputAttributesState.TrackViewState ();
				}
				inputAttributesState.LoadViewState (saved.Second);
			}

			if (saved.Third != null) {
				if (labelAttributesState == null) {
					labelAttributesState = new StateBag(true);
					labelAttributesState.TrackViewState ();
				}
				labelAttributesState.LoadViewState (saved.Third);
			}
		}

		protected override object SaveViewState ()
		{
			object baseView = base.SaveViewState ();
			object inputAttrView = null;
			object labelAttrView = null;

			if (inputAttributesState != null)
				inputAttrView = inputAttributesState.SaveViewState ();

			if (labelAttributesState != null)
				labelAttrView = labelAttributesState.SaveViewState ();

			if (baseView == null && inputAttrView == null && labelAttrView == null)
				return null;

			return new Triplet (baseView, inputAttrView, labelAttrView);		
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState();
			if (inputAttributesState != null)
				inputAttributesState.TrackViewState ();
			if (labelAttributesState != null)
				labelAttributesState.TrackViewState ();
		}
#endif		

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			if (Page != null) {
				Page.RegisterRequiresPostBack (this);
			}
		}

		void RenderLabel (HtmlTextWriter w)
		{
			if (Text.Length > 0) {
#if NET_2_0
				if (labelAttributes != null)
					labelAttributes.AddAttributes (w);
#endif
				w.AddAttribute (HtmlTextWriterAttribute.For, ClientID);
				w.RenderBeginTag (HtmlTextWriterTag.Label);
				w.Write (this.Text);
				w.RenderEndTag ();
			}
		}
		
#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter w)
		{
			bool control_style = false;
			
			/* Need to apply the styles around the text
			 * label too
			 */
			if (ControlStyleCreated) {
				ControlStyle.AddAttributesToRender (w, this);
				w.RenderBeginTag (HtmlTextWriterTag.Span);

				control_style = true;
			} else if (Attributes ["style"] != null) {
				/* TODO: check if this or the style
				 * has precendence, or if they should
				 * be merged (if I can figure out how
				 * to turn a CssStyleCollection into a
				 * Style)
				 */
				CssStyleCollection style = Attributes.CssStyle;
				
				w.AddAttribute (HtmlTextWriterAttribute.Style,
						style.BagToString ());
				w.RenderBeginTag (HtmlTextWriterTag.Span);

				control_style = true;
			}
			
			if (TextAlign == TextAlign.Left) {
				RenderLabel (w);
				base.Render (w);
			} else {
				base.Render (w);
				RenderLabel (w);
			}

			if (control_style) {
				w.RenderEndTag ();
			}
		}

#if NET_2_0
		protected virtual
#endif
		bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			string postedValue = postCollection[postDataKey];
			bool postedBool = ((postedValue != null) &&
					   (postedValue.Length > 0));
			
			if (Checked != postedBool) {
				Checked = postedBool;
				return (true);
			}

			return (false);
		}

#if NET_2_0
		protected virtual
#endif
		void RaisePostDataChangedEvent ()
		{
#if NET_2_0
			if (CausesValidation)
				Page.Validate (ValidationGroup);
#endif
			OnCheckedChanged (EventArgs.Empty);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}

		internal virtual void InternalAddAttributesToRender (HtmlTextWriter w)
		{
		}
	}
}

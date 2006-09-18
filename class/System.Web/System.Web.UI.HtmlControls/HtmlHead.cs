//
// System.Web.UI.HtmlControls.HtmlHead
//
// Authors:
// 	Lluis Sanchez Gual (lluis@novell.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Collections;
using System.Security.Permissions;
using System.Web.UI.WebControls;

namespace System.Web.UI.HtmlControls
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ControlBuilder (typeof(HtmlHeadBuilder))]
	public sealed class HtmlHead: HtmlGenericControl, IParserAccessor {

		string titleText;
		HtmlTitle title;
		Hashtable metadata;
		ArrayList styleSheets;
		StyleSheetBag styleSheet;
		
		public HtmlHead(): base("head") {}

		public HtmlHead (string tag) : base (tag)
		{
		}
		
		protected internal override void OnInit (EventArgs e)
		{
			//You can only have one <head runat="server"> control on a page.
			if(Page.Header!=null)
				throw new HttpException ("You can only have one <head runat=\"server\"> control on a page.");
			Page.SetHeader (this);
		}
		
		protected internal override void RenderChildren (HtmlTextWriter writer)
		{
			EnsureTitleControl ();

			base.RenderChildren (writer);
			if (metadata != null) {
				foreach (DictionaryEntry entry in metadata) {
					writer.AddAttribute ("name", entry.Key.ToString ());
					writer.AddAttribute ("content", entry.Value.ToString ());
					writer.RenderBeginTag (HtmlTextWriterTag.Meta);
					writer.RenderEndTag ();
				}
			}
			
			if (styleSheet != null)
				styleSheet.Render (writer);
		}
		
		protected internal override void AddedControl (Control control, int index)
		{
			//You can only have one <title> element within the <head> element.
			HtmlTitle t = control as HtmlTitle;
			if (t != null) {
				if (title != null)
					throw new HttpException ("You can only have one <title> element within the <head> element.");
				title = t;
			}

			base.AddedControl (control, index);
		}

		protected internal override void RemovedControl (Control control)
		{
			if (title == control)
				title = null;

			base.RemovedControl (control);
		}
		
		void EnsureTitleControl () {
			if (title != null)
				return;

			HtmlTitle t = new HtmlTitle ();
			t.Text = titleText;
			Controls.Add (t);
		}

		IList LinkedStyleSheets {
			get {
				if (styleSheets == null) styleSheets = new ArrayList ();
				return styleSheets;
			}
		} 
		
		IDictionary Metadata {
			get {
				if (metadata == null) metadata = new Hashtable ();
				return metadata;
			}
		}
		
		public IStyleSheet StyleSheet {
			get {
				if (styleSheet == null) styleSheet = new StyleSheetBag ();
				return styleSheet;
			}
		}
		
		public string Title {
			get {
				if (title != null)
					return title.Text;
				else
					return titleText;
			}
			set {
				if (title != null)
					title.Text = value;
				else
					titleText = value;
			}
		}
	}
	
	internal class StyleSheetBag: IStyleSheet
	{
		ArrayList entries = new ArrayList ();
		
		internal class StyleEntry
		{
			public Style Style;
			public string Selection;
			public IUrlResolutionService UrlResolver;
		}
		
		public StyleSheetBag ()
		{
		}
		
		public void CreateStyleRule (Style style, IUrlResolutionService urlResolver, string selection)
		{
			StyleEntry entry = new StyleEntry ();
			entry.Style = style;
			entry.UrlResolver = urlResolver;
			entry.Selection = selection;
			entries.Add (entry);
		}
		
		public void RegisterStyle (Style style, IUrlResolutionService urlResolver)
		{
			for (int n=0; n<entries.Count; n++) {
				if (((StyleEntry)entries[n]).Style == style)
					return;
			}
			
			string name = "aspnet_" + entries.Count;
			style.SetRegisteredCssClass (name);
			CreateStyleRule (style, urlResolver, "." + name);
		}
		
		public void Render (HtmlTextWriter writer)
		{
			writer.AddAttribute ("type", "text/css");
			writer.RenderBeginTag (HtmlTextWriterTag.Style);

			foreach (StyleEntry entry in entries) {
				CssStyleCollection sts = entry.Style.GetStyleAttributes (entry.UrlResolver);
				writer.Write ("\n" + entry.Selection + " {" + sts.Value + "}");
			}

			writer.RenderEndTag ();
		}
	}
}

#endif

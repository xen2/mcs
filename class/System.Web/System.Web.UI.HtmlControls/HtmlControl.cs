//
// System.Web.UI.HtmlControls.HtmlControl.cs
//
// Author
//   Bob Smith <bob@thestuff.net>
//
//
// (C) Bob Smith
//

using System;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	public abstract class HtmlControl : Control, IAttributeAccessor
	{
		private string _tagName = "span";
		private AttributeCollection _attributes;
		private bool _disabled = false;
		
		public HtmlControl() : this ("span") {}
		
		public HtmlControl(string tag)
		{
			_tagName = tag;
		}
		
		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		internal static string AttributeToString(int n){
			if (n != -1)return n.ToString(NumberFormatInfo.InvariantInfo);
			return null;
		}
		
		internal static string AttributeToString(string s){
			if (s != null && s.Length != 0) return s;
			return null;
		}
		
		internal void PreProcessRelativeReference(HtmlTextWriter writer, string attribName){
			string attr = Attributes[attribName];
			if (attr != null){
				if (attr.Length != 0){
					try{
						attr = ResolveUrl(attr);
					}
					catch (Exception e) {
						throw new HttpException(attribName + " property had malformed url");
					}
					writer.WriteAttribute(attribName, attr);
					Attributes.Remove(attribName);
				}
			}
		}
		
		string System.Web.UI.IAttributeAccessor.GetAttribute(string name){
			return Attributes[name];
		}
		
		void System.Web.UI.IAttributeAccessor.SetAttribute(string name, string value){
			Attributes[name] = value;
		}
		
		protected virtual void RenderBeginTag (HtmlTextWriter writer)
		{
			writer.WriteBeginTag (TagName);
			RenderAttributes (writer);
			writer.Write ('>');
		}

		protected override void Render (HtmlTextWriter writer)
		{
			RenderBeginTag (writer);
		}
		
		protected virtual void RenderAttributes(HtmlTextWriter writer){
			if (ID != null){
				writer.WriteAttribute("id",ClientID);
			}
			Attributes.Render(writer);
		}
		
		public AttributeCollection Attributes
		{
			get { 
				if (_attributes == null)
					_attributes = new AttributeCollection (ViewState);
				return _attributes;
			}
		}

		public bool Disabled
		{
			get { return _disabled; }
			set { _disabled = value; }
		}

		public CssStyleCollection Style
		{
			get { return Attributes.CssStyle; }
		}

		public virtual string TagName
		{
			get { return _tagName; }
		}

		protected override bool ViewStateIgnoresCase 
		{
			get {
				return true;
			}
		}
	}
}

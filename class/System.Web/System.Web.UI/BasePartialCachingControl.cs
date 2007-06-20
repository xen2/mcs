//
// System.Web.UI.BasePartialCachingControl.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Andreas Nahr
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

using System.IO;
using System.Text;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.Caching;

namespace System.Web.UI
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ToolboxItem (false)]
	public abstract class BasePartialCachingControl : Control
	{
		private CacheDependency dependency;
		private string ctrl_id;
		private string guid;
		private int duration;
		private string varyby_params;
		private string varyby_controls;
		private string varyby_custom;

		private Control control;
		
		protected BasePartialCachingControl()
		{
		}

		internal string CtrlID {
			get { return ctrl_id; }
			set { ctrl_id = value; }
		}

		internal string Guid {
			get { return guid; }
			set { guid = value; }
		}

		internal int Duration {
			get { return duration; }
			set { duration = value; }
		}

		internal string VaryByParams {
			get { return varyby_params; }
			set { varyby_params = value; }
		}

		internal string VaryByControls {
			get { return varyby_controls; }
			set { varyby_controls = value; }
		}

		internal string VaryByCustom {
			get { return varyby_custom; }
			set { varyby_custom = value; }
		}

		internal abstract Control CreateControl ();

		public override void Dispose ()
		{
			if (dependency != null) {
				dependency.Dispose ();
				dependency = null;
			}
		}

#if NET_2_0
		protected internal
#else
		protected
#endif
		override void OnInit (EventArgs e)
		{
			control = CreateControl ();
			Controls.Add (control);
		}

#if NET_2_0
		protected internal
#else
		protected
#endif
		override void Render (HtmlTextWriter output)
		{
			Cache cache = HttpRuntime.InternalCache;
			string key = CreateKey ();
			string data = cache [key] as string;

			if (data != null) {
				output.Write (data);
				return;
			}

			HttpContext context = HttpContext.Current;
			StringWriter writer = new StringWriter ();
			TextWriter prev = context.Response.SetTextWriter (writer);
			HtmlTextWriter txt_writer = new HtmlTextWriter (writer);
			string text;
			try {
				control.RenderControl (txt_writer);
			} finally {
				text = writer.ToString ();
				context.Response.SetTextWriter (prev);
				output.Write (text);
			}

			context.InternalCache.Insert (key, text, dependency,
						      DateTime.Now.AddSeconds (duration),
						      Cache.NoSlidingExpiration,
						      CacheItemPriority.Normal, null);
		}

#if NET_2_0
		public ControlCachePolicy CachePolicy 
		{
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		public CacheDependency Dependency {
			get {return dependency;}
			set {dependency = value;}
		}

		private string CreateKey ()
		{
			StringBuilder builder = new StringBuilder ();
			HttpContext context = HttpContext.Current;

			builder.Append ("PartialCachingControl\n");
			builder.Append ("GUID: " + guid + "\n");

			if (varyby_params != null && varyby_params.Length > 0) {
				string[] prms = varyby_params.Split (';');
				for (int i=0; i<prms.Length; i++) {
					string val = context.Request.Params [prms [i]];
					builder.Append ("VP:");
					builder.Append (prms [i]);
					builder.Append ('=');
					builder.Append (val != null ? val : "__null__");
					builder.Append ('\n');
				}
			}

			if (varyby_controls != null && varyby_params.Length > 0) {
				string[] prms = varyby_controls.Split (';');
				for (int i=0; i<prms.Length; i++) {
					string val = context.Request.Params [prms [i]];
					builder.Append ("VCN:");
					builder.Append (prms [i]);
					builder.Append ('=');
					builder.Append (val != null ? val : "__null__");
					builder.Append ('\n');
				}
			}

			if (varyby_custom != null) {
				string val = context.ApplicationInstance.GetVaryByCustomString (context,
						varyby_custom);
				builder.Append ("VC:");
				builder.Append (varyby_custom);
				builder.Append ('=');
				builder.Append (val != null ? val : "__null__");
				builder.Append ('\n');
			}

			return builder.ToString ();
		}
	}
}


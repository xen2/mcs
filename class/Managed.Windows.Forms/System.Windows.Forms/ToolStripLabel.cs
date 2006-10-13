//
// ToolStripLabel.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class ToolStripLabel : ToolStripItem
	{
		private Color active_link_color;
		private bool is_link;
		private LinkBehavior link_behavior;
		private Color link_color;
		private bool link_visited;
		private Color visited_link_color;

		#region Public Constructors
		public ToolStripLabel ()
			: this (String.Empty, null, false, null, String.Empty)
		{
		}

		public ToolStripLabel (Image image)
			: this (String.Empty, image, false, null, String.Empty)
		{
		}

		public ToolStripLabel (string text)
			: this (text, null, false, null, String.Empty)
		{
		}

		public ToolStripLabel (string text, Image image)
			: this (text, image, false, null, String.Empty)
		{
		}

		public ToolStripLabel (string text, Image image, bool isLink)
			: this (text, image, isLink, null, String.Empty)
		{
		}

		public ToolStripLabel (string text, Image image, bool isLink, EventHandler onClick)
			: this (text, image, isLink, onClick, String.Empty)
		{
		}

		public ToolStripLabel (string text, Image image, bool isLink, EventHandler onClick, string name)
			: base (text, image, onClick, name)
		{
			this.active_link_color = Color.Red;
			this.is_link = isLink;
			this.link_behavior = LinkBehavior.SystemDefault;
			this.link_color = Color.Blue;
			this.link_visited = false;
			this.visited_link_color = Color.Purple;
		}
		#endregion

		#region Public Properties
		public Color ActiveLinkColor {
			get { return this.active_link_color; }
			set {
				this.active_link_color = value; 
				this.Invalidate ();
			}
		}

		public override bool CanSelect { get { return false; } }
		
		[DefaultValue (false)]
		public bool IsLink {
			get { return this.is_link; }
			set {
				this.is_link = value; 
				this.Invalidate ();
			}
		}

		[DefaultValue (LinkBehavior.SystemDefault)]
		public LinkBehavior LinkBehavior {
			get { return this.link_behavior; }
			set {
				this.link_behavior = value; 
				this.Invalidate ();
			}
		}

		public Color LinkColor {
			get { return this.link_color; }
			set {
				this.link_color = value; 
				this.Invalidate ();
			}
		}

		[DefaultValue (false)]
		public bool LinkVisited {
			get { return this.link_visited; }
			set {
				this.link_visited = value; 
				this.Invalidate ();
			}
		}

		public Color VisitedLinkColor
		{
			get { return this.visited_link_color; }
			set {
				this.visited_link_color = value; 
				this.Invalidate ();
			}
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			ToolStripItemAccessibleObject ao = new ToolStripItemAccessibleObject (this);

			ao.role = AccessibleRole.StaticText;
			ao.state = AccessibleStates.ReadOnly;	

			return ao;
		}
		
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnMouseEnter (EventArgs e)
		{
			base.OnMouseEnter (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}
		
		protected override void OnPaint (System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint (e);

			if (this.Owner != null) {
				Color font_color = this.Enabled ? this.ForeColor : SystemColors.GrayText;
				Image draw_image = this.Enabled ? this.Image : ToolStripRenderer.CreateDisabledImage (this.Image);

				this.Owner.Renderer.DrawLabelBackground (new System.Windows.Forms.ToolStripItemRenderEventArgs (e.Graphics, this));

				Rectangle text_layout_rect;
				Rectangle image_layout_rect;

				this.CalculateTextAndImageRectangles (out text_layout_rect, out image_layout_rect);

				if (image_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemImage (new System.Windows.Forms.ToolStripItemImageRenderEventArgs (e.Graphics, this, draw_image, image_layout_rect));
				if (text_layout_rect != Rectangle.Empty)
					if (this.is_link) {
						if (this.Pressed)		// Mouse Down
						{
							switch (this.link_behavior) {
								case LinkBehavior.SystemDefault:
								case LinkBehavior.AlwaysUnderline:
								case LinkBehavior.HoverUnderline:
									this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, this.active_link_color, new Font (this.Font, FontStyle.Underline), this.TextAlign));
									break;
								case LinkBehavior.NeverUnderline:
									this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, this.active_link_color, this.Font, this.TextAlign));
									break;
							}
						}
						else if (this.Selected)		// Hover
						{
							switch (this.link_behavior) {
								case LinkBehavior.SystemDefault:
								case LinkBehavior.AlwaysUnderline:
								case LinkBehavior.HoverUnderline:
									this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, this.link_color, new Font (this.Font, FontStyle.Underline), this.TextAlign));
									break;
								case LinkBehavior.NeverUnderline:
									this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, this.link_color, this.Font, this.TextAlign));
									break;
							}
						}
						else {

							if (this.link_visited)		// Normal, Visited
							{
								switch (this.link_behavior) {
									case LinkBehavior.SystemDefault:
									case LinkBehavior.AlwaysUnderline:
										this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, this.visited_link_color, new Font (this.Font, FontStyle.Underline), this.TextAlign));
										break;
									case LinkBehavior.NeverUnderline:
									case LinkBehavior.HoverUnderline:
										this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, this.visited_link_color, this.Font, this.TextAlign));
										break;
								}
							}
							else				// Normal
							{
								switch (this.link_behavior) {
									case LinkBehavior.SystemDefault:
									case LinkBehavior.AlwaysUnderline:
										this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, this.link_color, new Font (this.Font, FontStyle.Underline), this.TextAlign));
										break;
									case LinkBehavior.NeverUnderline:
									case LinkBehavior.HoverUnderline:
										this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, this.link_color, this.Font, this.TextAlign));
										break;
								}

							}
						}
					} else
						this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, font_color, this.Font, this.TextAlign));
			}
		}
		#endregion
	}
}
#endif

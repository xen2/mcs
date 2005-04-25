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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//

// COMPLETE

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;

namespace System.Windows.Forms {
	public abstract class ButtonBase : Control {
		#region Local Variables
		internal FlatStyle		flat_style;
		internal int			image_index;
		internal Image			image;
		internal ImageList		image_list;
		internal ContentAlignment	image_alignment;
		internal ContentAlignment	text_alignment;
		private bool			is_default;
		internal bool			is_pressed;
		internal bool			enter_state;
		internal StringFormat		text_format;
		#endregion	// Local Variables

		#region Private Properties and Methods
		internal ButtonState ButtonState {
			get {
				ButtonState	ret = ButtonState.Normal;

				if (Enabled) {
					// Popup style is only followed as long as the mouse isn't "in" the control
					if (is_entered) {
						if (flat_style == FlatStyle.Flat) {
							ret |= ButtonState.Flat;
						}
					} else {
						if (flat_style == FlatStyle.Flat || flat_style == FlatStyle.Popup) {
							ret |= ButtonState.Flat;
						}
					}

					if (is_entered && is_pressed) {
						ret |= ButtonState.Pushed;
					}
				} else {
					ret |= ButtonState.Inactive;
					if ((flat_style == FlatStyle.Flat) || (flat_style == FlatStyle.Popup)) {
						ret |= ButtonState.Flat;
					}
				}
				return ret;
			}
		}

		[MonoTODO("Make the FillRectangle use a global brush instead of creating one every time")]
		internal void Redraw() {
			Refresh ();
		}

		// Derived classes should override Draw method and we dont want
		// to break the control signature, hence this approach.
		internal virtual void Draw (PaintEventArgs pevent) {
			ThemeEngine.Current.DrawButtonBase (pevent.Graphics, pevent.ClipRectangle, this);
		}

		internal virtual void HaveDoubleClick() {
			// override me
		}

		private void RedrawEvent(object sender, System.EventArgs e) {
			Redraw();
		}

		#endregion	// Private Properties and Methods

		#region Public Constructors
		protected ButtonBase() : base() {
			flat_style	= FlatStyle.Standard;
			image_index	= -1;
			image		= null;
			image_list	= null;
			image_alignment	= ContentAlignment.MiddleCenter;
			text_alignment	= ContentAlignment.MiddleCenter;
			ime_mode	= ImeMode.Inherit;
			is_default	= false;
			is_entered	= false;
			is_pressed	= false;
			has_focus	= false;
			text_format	= new StringFormat();
			text_format.Alignment = StringAlignment.Center;
			text_format.LineAlignment = StringAlignment.Center;
			text_format.HotkeyPrefix = HotkeyPrefix.Show;

			TextChanged+=new System.EventHandler(RedrawEvent);
			ForeColorChanged+=new EventHandler(RedrawEvent);
			BackColorChanged+=new System.EventHandler(RedrawEvent);
			FontChanged+=new EventHandler(RedrawEvent);
			SizeChanged+=new EventHandler(RedrawEvent);

			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[Localizable(true)]
		[DefaultValue(FlatStyle.Standard)]
		public FlatStyle FlatStyle {
			get {
				return flat_style;
			}

			set { 
				flat_style = value; 
				Redraw();
			}
		}
		
		[Localizable(true)]
		public Image Image {
			get {
				return image;
			}

			set { 
				image = value;
				Redraw();
			}
		}

		[Localizable(true)]
		[DefaultValue(ContentAlignment.MiddleCenter)]
		public ContentAlignment ImageAlign {
			get {
				return image_alignment;
			}

			set {
				image_alignment=value;
				Redraw();
			}
		}

		[Localizable(true)]
		[DefaultValue(-1)]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[TypeConverter(typeof(ImageIndexConverter))]
		public int ImageIndex {
			get {
				if (image_list==null) {
					return -1;
				}
				return image_index;
			}

			set {
				image_index=value;
				Redraw();
			}
		}

		[DefaultValue(null)]
		public ImageList ImageList {
			get {
				return image_list;
			}

			set {
				if (image_list != null) {
					image_list.Dispose();
				}

				image_list = value;
				if (value != null) {
					if (image != null) {
						image=null;
					}
					if (image_list.Images.Count >= image_index) {
						image_index=image_list.Images.Count-1;
					}
				}
				Redraw();
			}
		}

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get {
				return ime_mode;
			}

			set {
				if (ime_mode != value) {
					ime_mode = value;

					if (ImeModeChanged != null) {
						ImeModeChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(ContentAlignment.MiddleCenter)]
		public virtual ContentAlignment TextAlign {
			get {
				return text_alignment;
			}

			set {
				if (text_alignment != value) {
					text_alignment = value;
					switch(text_alignment) {
						case ContentAlignment.TopLeft: {
							text_format.Alignment=StringAlignment.Near;
							text_format.LineAlignment=StringAlignment.Near;
							break;
						}

						case ContentAlignment.TopCenter: {
							text_format.Alignment=StringAlignment.Center;
							text_format.LineAlignment=StringAlignment.Near;
							break;
						}

						case ContentAlignment.TopRight: {
							text_format.Alignment=StringAlignment.Far;
							text_format.LineAlignment=StringAlignment.Near;
							break;
						}

						case ContentAlignment.MiddleLeft: {
							text_format.Alignment=StringAlignment.Near;
							text_format.LineAlignment=StringAlignment.Center;
							break;
						}

						case ContentAlignment.MiddleCenter: {
							text_format.Alignment=StringAlignment.Center;
							text_format.LineAlignment=StringAlignment.Center;
							break;
						}

						case ContentAlignment.MiddleRight: {
							text_format.Alignment=StringAlignment.Far;
							text_format.LineAlignment=StringAlignment.Center;
							break;
						}

						case ContentAlignment.BottomLeft: {
							text_format.Alignment=StringAlignment.Near;
							text_format.LineAlignment=StringAlignment.Far;
							break;
						}

						case ContentAlignment.BottomCenter: {
							text_format.Alignment=StringAlignment.Center;
							text_format.LineAlignment=StringAlignment.Far;
							break;
						}

						case ContentAlignment.BottomRight: {
							text_format.Alignment=StringAlignment.Far;
							text_format.LineAlignment=StringAlignment.Far;
							break;
						}
					}	
					Redraw();
				}
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get { 
				CreateParams	cp;

				cp=base.CreateParams;

				cp.Style=(int)(WindowStyles.WS_VISIBLE | WindowStyles.WS_CHILD | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN);

				return cp;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {
				return ImeMode.Inherit;
			}
		}

		protected override Size DefaultSize {
			get {
				return ThemeEngine.Current.ButtonBaseDefaultSize;
			}
		}

		protected bool IsDefault {
			get {
				return is_default;
			}

			set {
				if (is_default != value) {
					is_default = true;
					Redraw();
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		[MonoTODO("Finish setting properties of the AccessibleObject")]
		protected override AccessibleObject CreateAccessibilityInstance() {
			AccessibleObject ao;
			ao=base.CreateAccessibilityInstance();
			ao.description="Button";

			return ao;
		}

		protected override void Dispose(bool Disposing) {
			base.Dispose(Disposing);
		}

		protected override void OnEnabledChanged(EventArgs e) {
			Redraw();
			base.OnEnabledChanged(e);
		}

		protected override void OnGotFocus(EventArgs e) {
			has_focus=true;
			Redraw();
			base.OnGotFocus(e);
		}

		protected override void OnKeyDown(KeyEventArgs kevent) {
			if (is_enabled && (kevent.KeyData == Keys.Space)) {
				enter_state = is_entered;
				is_entered = true;
				OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 2, 2, 0));
				kevent.Handled=true;
			}
			base.OnKeyDown(kevent);
		}

		protected override void OnKeyUp(KeyEventArgs kevent) {
			if (is_enabled && (kevent.KeyData == Keys.Space)) {
				OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, 2, 2, 0));
				is_entered = enter_state;
				kevent.Handled=true;
			}
			base.OnKeyUp(kevent);
		}

		protected override void OnLostFocus(EventArgs e) {
			has_focus=false;
			Redraw();
			base.OnLostFocus(e);
		}

		protected override void OnMouseDown(MouseEventArgs mevent) {
			if (is_enabled && (mevent.Button == MouseButtons.Left)) {
				is_pressed = true;
				this.Capture = true;
				Redraw();
			}

			base.OnMouseDown(mevent);
		}

		protected override void OnMouseEnter(EventArgs e) {
			is_entered=true;
			if ((this.flat_style == FlatStyle.Flat) || (this.flat_style == FlatStyle.Popup)) {
				Redraw();
			}
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e) {
			is_entered=false;
			if ((this.flat_style == FlatStyle.Flat) || (this.flat_style == FlatStyle.Popup)) {
				Redraw();
			}
			base.OnMouseLeave(e);
		}

		protected override void OnMouseMove(MouseEventArgs mevent) {
			bool	inside = false;
			bool	redraw = false;

			if (mevent.X>=0 && mevent.Y>=0 && mevent.X<this.client_size.Width && mevent.Y<=this.client_size.Height) {
				inside = true;
			}

			// If the button was pressed and we leave, release the button press and vice versa
			if (this.Capture && (inside != is_pressed)) {
				is_pressed = inside;
				redraw = true;
			}

			if (is_entered != inside) {
				is_entered = inside;
				redraw = true;
			}

			if (redraw) {
				Redraw();
			}

			base.OnMouseMove(mevent);
		}

		protected override void OnMouseUp(MouseEventArgs mevent) {
			if (this.Capture && mevent.Button == MouseButtons.Left) {
				this.Capture = false;
				if (is_pressed) {
					is_pressed = false;
					Redraw();
				} else if ((this.flat_style == FlatStyle.Flat) || (this.flat_style == FlatStyle.Popup)) {
					Redraw();
				}

				if (mevent.X>=0 && mevent.Y>=0 && mevent.X<this.client_size.Width && mevent.Y<=this.client_size.Height) {
					OnClick(EventArgs.Empty);
				}
			}
			base.OnMouseUp(mevent);
		}

		protected override void OnPaint(PaintEventArgs pevent) {
			base.OnPaint (pevent);
			Draw (pevent);
		}

		protected override void OnParentChanged(EventArgs e) {
			base.OnParentChanged(e);
		}

		protected override void OnTextChanged(EventArgs e) {
			Redraw();
			base.OnTextChanged(e);
		}

		protected override void OnVisibleChanged(EventArgs e) {
			if (!Visible) {
				is_pressed = false;
				has_focus = false;
				is_entered = false;
			}
			base.OnVisibleChanged(e);
		}

		protected void ResetFlagsandPaint() {
			// Nothing to do; MS internal
			// Should we do Redraw (); ?
		}

		protected override void WndProc(ref Message m) {
			switch((Msg)m.Msg) {
				case Msg.WM_LBUTTONDBLCLK: {
					HaveDoubleClick();
					break;
				}

				case Msg.WM_MBUTTONDBLCLK: {
					HaveDoubleClick();
					break;
				}

				case Msg.WM_RBUTTONDBLCLK: {
					HaveDoubleClick();
					break;
				}
			}
			base.WndProc (ref m);
		}
		#endregion	// Public Instance Properties

		#region	Events
		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged;
		#endregion	// Events
	}
}

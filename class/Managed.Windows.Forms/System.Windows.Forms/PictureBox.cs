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
//	Jackson Harper (jackson@ximian.com)
//


using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace System.Windows.Forms {

	public class PictureBox : Control {

		private Image image;
		private PictureBoxSizeMode size_mode;
		private bool redraw;
		private bool recalc;

		private EventHandler frame_handler;

		public PictureBox ()
		{
			redraw = true;
			recalc = true;
		}

		public PictureBoxSizeMode SizeMode {
			get { return size_mode; }
			set {
				if (size_mode == value)
					return;
				size_mode = value;
				UpdateSize ();
				Redraw (true);
				Invalidate ();

				OnSizeModeChanged (EventArgs.Empty);
			}
		}

		public Image Image {
			get { return image; }
			set {
				StopAnimation ();

				image = value;
				UpdateSize ();
				if (image != null && ImageAnimator.CanAnimate (image)) {
					frame_handler = new EventHandler (OnAnimateImage);
					ImageAnimator.Animate (image, frame_handler);
				}
				Redraw (true);
				Invalidate ();
			}
		}

		public BorderStyle BorderStyle {
			get { return border_style; }
			set {
				border_style = value;
				Redraw (true);
			}
		}

		public new bool CausesValidation {
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}

		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set { base.RightToLeft = value;	}
		}

		public new int TabIndex	{
			get { return base.TabIndex; }
			set { base.TabIndex = value; }
		}

		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get { return base.DefaultImeMode; }
		}

		public override Font Font {
			get { return base.Font;	}
			set { base.Font = value; }
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.PictureBoxDefaultSize; }
		}

		protected override void Dispose (bool disposing)
		{
			if (image != null) {
				StopAnimation ();
				image.Dispose ();
				image = null;
			}
			base.Dispose (disposing);
		}

		protected override void OnPaint (PaintEventArgs pe)
		{
			if (this.Width <= 0 || this.Height <=  0 || this.Visible == false)
				return;

			Draw ();
			pe.Graphics.DrawImage (this.ImageBuffer, pe.ClipRectangle, pe.ClipRectangle, GraphicsUnit.Pixel);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
			Redraw (true);
		}

		protected virtual void OnSizeModeChanged (EventArgs e)
		{
			if (SizeModeChanged != null)
				SizeModeChanged (this, e);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			redraw = true;

			if (size_mode == PictureBoxSizeMode.CenterImage || size_mode == PictureBoxSizeMode.StretchImage)
				Refresh ();
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (size_mode == PictureBoxSizeMode.AutoSize && image != null) {
				width = image.Width;
				height = image.Height;
			}
			base.SetBoundsCore (x, y, width, height, specified);
		}

		private void StopAnimation ()
		{
			if (frame_handler == null)
				return;
			ImageAnimator.StopAnimate (image, frame_handler);
			frame_handler = null;
		}

		private void UpdateSize ()
		{
			if (image == null)
				return;
			if (size_mode == PictureBoxSizeMode.AutoSize)
				ClientSize = image.Size; 
		}

		private void Redraw (bool recalc)
		{
			redraw = true;
			this.recalc = recalc;
		}

		private void OnAnimateImage (object sender, EventArgs e)
		{
			// This is called from a worker thread,BeginInvoke is used
			// so the control is updated from the correct thread
			BeginInvoke (new EventHandler (UpdateAnimatedImage), new object [] { this, e });
		}

		private void UpdateAnimatedImage (object sender, EventArgs e)
		{
			ImageAnimator.UpdateFrames (image);
			Redraw (false);
			Refresh ();
		}

		[MonoTODO ("Borders and stuff, and move into the Theme")]
		private void Draw ()
		{
			if (redraw) {
				ThemeEngine.Current.DrawPictureBox (DeviceContext, this);
			}
			redraw = false;
		}

		public new event EventHandler CausesValidationChanged {
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
		}

		public new event EventHandler Enter {
			add { base.Enter += value; }
			remove { base.Enter -= value; }
		}

		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}

		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		public new event EventHandler Leave {
			add { base.Leave += value; }
			remove { base.Leave -= value; }
		}

		public new event EventHandler RightToLeftChanged {
			add { base.RightToLeftChanged += value; }
			remove { base.RightToLeftChanged -= value; }
		}

		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		public event EventHandler SizeModeChanged;

	}
}


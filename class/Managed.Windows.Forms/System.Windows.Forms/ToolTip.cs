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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;

namespace System.Windows.Forms {
#if NET_2_0
	[DefaultEvent ("Popup")]
#endif
	[ProvideProperty ("ToolTip", typeof(System.Windows.Forms.Control))]
	[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Allow)]
	public
#if !NET_2_0
	sealed
#endif
	class ToolTip : System.ComponentModel.Component, System.ComponentModel.IExtenderProvider {
		#region Local variables
		internal bool		is_active;
		internal int		automatic_delay;
		internal int		autopop_delay;
		internal int		initial_delay;
		internal int		re_show_delay;
		internal bool		show_always;

		internal ToolTipWindow	tooltip_window;			// The actual tooltip window
		internal Hashtable	tooltip_strings;		// List of strings for each control, indexed by control
		internal ArrayList	controls;
		internal Control	active_control;			// Control for which the tooltip is currently displayed
		internal Control	last_control;			// last control the mouse was in
		internal Timer		timer;				// Used for the various intervals

#if NET_2_0
		private bool isBalloon;
		private bool stripAmpersands;
		private bool useAnimation;
		private bool useFading;
#endif

		#endregion	// Local variables

		#region ToolTipWindow Class
		internal class ToolTipWindow : Control {
			#region ToolTipWindow Class Local Variables
			internal StringFormat string_format;
			#endregion	// ToolTipWindow Class Local Variables

			#region ToolTipWindow Class Constructor
			internal ToolTipWindow() {

				string_format = new StringFormat();
				string_format.LineAlignment = StringAlignment.Center;
				string_format.FormatFlags = StringFormatFlags.NoWrap;
				string_format.HotkeyPrefix = HotkeyPrefix.Hide;

				Visible = false;
				Size = new Size(100, 20);
				ForeColor = ThemeEngine.Current.ColorInfoText;
				BackColor = ThemeEngine.Current.ColorInfo;

				VisibleChanged += new EventHandler(ToolTipWindow_VisibleChanged);

				SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
			}

			#endregion	// ToolTipWindow Class Constructor

			#region ToolTipWindow Class Protected Instance Methods
			protected override void OnCreateControl() {
				base.OnCreateControl ();
				XplatUI.SetTopmost(this.window.Handle, true);
			}

			protected override CreateParams CreateParams {
				get {
					CreateParams cp;

					cp = base.CreateParams;

					cp.Style = (int)WindowStyles.WS_POPUP;
					cp.Style |= (int)WindowStyles.WS_CLIPSIBLINGS;

					cp.ExStyle = (int)(WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TOPMOST);

					return cp;
				}
			}

			protected override void OnPaint(PaintEventArgs pevent) {
				// We don't do double-buffering on purpose:
				// 1) we'd have to meddle with is_visible, it destroys the buffers if !visible
				// 2) We don't draw much, no need to double buffer
				ThemeEngine.Current.DrawToolTip(pevent.Graphics, ClientRectangle, this);

				base.OnPaint(pevent);
			}

			protected override void OnTextChanged (EventArgs args)
			{
				Invalidate ();
				base.OnTextChanged (args); 
			}

			protected override void Dispose(bool disposing) {
				if (disposing) {
					this.string_format.Dispose();
				}
				base.Dispose (disposing);
			}

			protected override void WndProc(ref Message m) {
				if (m.Msg == (int)Msg.WM_SETFOCUS) {
					if (m.WParam != IntPtr.Zero) {
						XplatUI.SetFocus(m.WParam);
					}
				}
				base.WndProc (ref m);
			}


			#endregion	// ToolTipWindow Class Protected Instance Methods

			#region ToolTipWindow Class Private Methods
			private void ToolTipWindow_VisibleChanged(object sender, EventArgs e) {
				Control control = (Control)sender;

				if (control.is_visible) {
					XplatUI.SetTopmost(control.window.Handle, true);
				} else {
					XplatUI.SetTopmost(control.window.Handle, false);
				}
			}
			#endregion	// ToolTipWindow Class Protected Instance Methods

			#region Internal Properties
			internal override bool ActivateOnShow { get { return false; } }
			#endregion
			
			public void Present (Control control, string text)
			{
				if (IsDisposed)
					return;

				Size display_size;
				XplatUI.GetDisplaySize (out display_size);

				Size size = ThemeEngine.Current.ToolTipSize (this, text);
				Width = size.Width;
				Height = size.Height;
				Text = text;

				int cursor_w, cursor_h, hot_x, hot_y;
				XplatUI.GetCursorInfo (control.Cursor.Handle, out cursor_w, out cursor_h, out hot_x, out hot_y);
				Point loc = Control.MousePosition;
				loc.Y += (cursor_h - hot_y);

				if ((loc.X + Width) > display_size.Width)
					loc.X = display_size.Width - Width;

				if ((loc.Y + Height) > display_size.Height)
					loc.Y = Control.MousePosition.Y - Height - hot_y;
				
				Location = loc;
				Visible = true;
			}
		}
		#endregion	// ToolTipWindow Class

		#region Public Constructors & Destructors
		public ToolTip() {

			// Defaults from MS
			is_active = true;
			automatic_delay = 500;
			autopop_delay = 5000;
			initial_delay = 500;
			re_show_delay = 100;
			show_always = false;
#if NET_2_0
			isBalloon = false;
			stripAmpersands = false;
			useAnimation = true;
			useFading = true;
#endif
			tooltip_strings = new Hashtable(5);
			controls = new ArrayList(5);

			tooltip_window = new ToolTipWindow();
			tooltip_window.MouseLeave += new EventHandler(control_MouseLeave);

			timer = new Timer();
			timer.Enabled = false;
			timer.Tick +=new EventHandler(timer_Tick);
		}

		public ToolTip(System.ComponentModel.IContainer cont) : this() {
			cont.Add (this);
		}

		~ToolTip() {
		}
		#endregion	// Public Constructors & Destructors

		#region Public Instance Properties
		[DefaultValue (true)]
		public bool Active {
			get {
				return is_active;
			}

			set {
				if (is_active != value) {
					is_active = value;

					if (tooltip_window.Visible) {
						tooltip_window.Visible = false;
						active_control = null;
					}
				}
			}
		}

		[DefaultValue (500)]
		[RefreshProperties (RefreshProperties.All)]
		public int AutomaticDelay {
			get {
				return automatic_delay;
			}

			set {
				if (automatic_delay != value) {
					automatic_delay = value;
					autopop_delay = automatic_delay * 10;
					initial_delay = automatic_delay;
					re_show_delay = automatic_delay / 5;
				}
			}
		}

		[RefreshProperties (RefreshProperties.All)]
		public int AutoPopDelay {
			get {
				return autopop_delay;
			}

			set {
				if (autopop_delay != value) {
					autopop_delay = value;
				}
			}
		}

		[RefreshProperties (RefreshProperties.All)]
		public int InitialDelay {
			get {
				return initial_delay;
			}

			set {
				if (initial_delay != value) {
					initial_delay = value;
				}
			}
		}

		[RefreshProperties (RefreshProperties.All)]
		public int ReshowDelay {
			get {
				return re_show_delay;
			}

			set {
				if (re_show_delay != value) {
					re_show_delay = value;
				}
			}
		}

		[DefaultValue (false)]
		public bool ShowAlways {
			get {
				return show_always;
			}

			set {
				if (show_always != value) {
					show_always = value;
				}
			}
		}


#if NET_2_0
		[DefaultValue (false)]
		public bool IsBalloon {
			get { return isBalloon; }
			set { isBalloon = value; }
		}

		[Browsable (true)]
		[DefaultValue (false)]
		public bool StripAmpersands {
			get { return stripAmpersands; }
			set { stripAmpersands = value; }
		}

		[Browsable (true)]
		[DefaultValue (true)]
		public bool UseAnimation {
			get { return useAnimation; }
			set { useAnimation = value; }
		}

		[Browsable (true)]
		[DefaultValue (true)]
		public bool UseFading {
			get { return useFading; }
			set { useFading = value; }
		}
#endif

		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public bool CanExtend(object target) {
			return false;
		}

#if NET_2_0
		[Editor ("System.ComponentModel.Design.MultilineStringEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
#endif
		[Localizable (true)]
		[DefaultValue ("")]
		public string GetToolTip (Control control)
		{
			string tooltip = (string)tooltip_strings[control];
			if (tooltip == null)
				return "";
			return tooltip;
		}

		public void RemoveAll() {
			tooltip_strings.Clear();
			controls.Clear();
		}

		public void SetToolTip(Control control, string caption) {
			tooltip_strings[control] = caption;

			// no need for duplicates
			if (!controls.Contains(control)) {
				control.MouseEnter += new EventHandler(control_MouseEnter);
				control.MouseMove += new MouseEventHandler(control_MouseMove);
				control.MouseLeave += new EventHandler(control_MouseLeave);
				controls.Add(control);
			}
			
			// if SetToolTip is called from a control and the mouse is currently over that control,
			// make sure that tooltip_window.Text gets updated if it's being shown,
			// or show the tooltip for it if is not
			if (active_control == control && caption != null && state == TipState.Show) {
				Size size = ThemeEngine.Current.ToolTipSize(tooltip_window, caption);
				tooltip_window.Width = size.Width;
				tooltip_window.Height = size.Height;
				tooltip_window.Text = caption;
				timer.Stop ();
				timer.Start ();
			} else if (MouseInControl (control))
				ShowTooltip (control);
		}

		public override string ToString() {
			return base.ToString() + " InitialDelay: " + initial_delay + ", ShowAlways: " + show_always;
		}

#if NET_2_0
		public void Show (string text, IWin32Window win)
		{
			SetToolTip (win as Control, text);
			ShowTooltip (win as Control);
		}

		[MonoTODO ("Finish implementing tooltip location")]
		public void Show (string text, IWin32Window win, Point p)
		{
			SetToolTip (win as Control, text);
			ShowTooltip (win as Control);
		}

		public void Hide (IWin32Window win) 
		{
			Hide (win as Control);
		}
#endif

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void Dispose(bool disposing) {
			if (disposing) {
				// Mop up the mess; or should we wait for the GC to kick in?
				timer.Stop();
				timer.Dispose();

				// Not sure if we should clean up tooltip_window
				tooltip_window.Dispose();

				tooltip_strings.Clear();
				
				controls.Clear();
			}
		}
		#endregion	// Protected Instance Methods

		enum TipState {
			Initial,
			Show,
			Down
		}

		TipState state = TipState.Initial;

		#region Private Methods
		private void control_MouseEnter(object sender, EventArgs e) 
		{
			ShowTooltip (sender as Control);
		}

		private void ShowTooltip (Control control) 
		{
			last_control = control;

			// Whatever we're displaying right now, we don't want it anymore
			tooltip_window.Visible = false;
			timer.Stop();
			state = TipState.Initial;

			if (!is_active)
				return;

			if (!show_always) {
				IContainerControl cc = last_control.GetContainerControl ();
				if ((cc == null) || (cc.ActiveControl == null)) {
					return;
				}
			}

			string text = (string)tooltip_strings[control];
			if (text != null && text.Length > 0) {

				if (active_control == null) {
					timer.Interval = initial_delay;
				} else {
					timer.Interval = re_show_delay;
				}

				active_control = control;
				timer.Start ();
			}
		}

		private void timer_Tick(object sender, EventArgs e) {
			timer.Stop();

			switch (state) {
			case TipState.Initial:
				if (active_control == null)
					return;
				tooltip_window.Present (active_control, (string)tooltip_strings[active_control]);
				state = TipState.Show;
				timer.Interval = autopop_delay;
				timer.Start();
				break;

			case TipState.Show:
				tooltip_window.Visible = false;
				state = TipState.Down;
				break;

			default:
				throw new Exception ("Timer shouldn't be running in state: " + state);
			}
		}


		private bool MouseInControl(Control control) {
			Point	m;
			Point	c;
			Size	cw;

			if (control == null) {
				return false;
			}

			m = Control.MousePosition;
			c = new Point(control.Bounds.X, control.Bounds.Y);
			if (control.Parent != null) {
				c = control.Parent.PointToScreen(c);
			}
			cw = control.ClientSize;

			if (c.X<=m.X && m.X<(c.X+cw.Width) &&
				c.Y<=m.Y && m.Y<(c.Y+cw.Height)) {
				return true;
			}
			return false;
		}

		private void control_MouseLeave(object sender, EventArgs e) 
		{
			Hide (sender as Control);
		}

		private void Hide (Control sender)
		{
			timer.Stop();

			if (!MouseInControl(tooltip_window) && !MouseInControl(active_control)) {
				active_control = null;
				tooltip_window.Visible = false;
			}
			
			if (last_control == sender)
				last_control = null;
		}

		private void control_MouseMove(object sender, MouseEventArgs e) {
			if (state != TipState.Down) {
				timer.Stop();
				timer.Start();
			}
		}

#if NET_2_0
		internal virtual void OnPopup (PopupEventArgs e)
		{
			PopupEventHandler eh = (PopupEventHandler) (Events [PopupEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif
		#endregion	// Private Methods

		#region Events
#if NET_2_0
		static object PopupEvent = new object ();
		
		public event PopupEventHandler Popup {
			add { Events.AddHandler (PopupEvent, value); }
			remove { Events.RemoveHandler (PopupEvent, value); }
		}
#endif
		#endregion
	}
}

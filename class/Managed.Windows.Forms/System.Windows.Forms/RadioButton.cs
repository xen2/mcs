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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultProperty("Checked")]
	[DefaultEvent("CheckedChanged")]
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[DefaultBindingProperty ("Checked")]
	[ToolboxItem ("System.Windows.Forms.Design.AutoSizeToolboxItem," + Consts.AssemblySystem_Design)]
	[Designer ("System.Windows.Forms.Design.RadioButtonDesigner, " + Consts.AssemblySystem_Design)]
#endif
	public class RadioButton : ButtonBase {
		#region Local Variables
		internal Appearance		appearance;
		internal bool			auto_check;
		internal ContentAlignment	radiobutton_alignment;
		internal CheckState		check_state;
		#endregion	// Local Variables

		#region RadioButtonAccessibleObject Subclass
		[ComVisible(true)]
#if NET_2_0
		public class RadioButtonAccessibleObject : ButtonBaseAccessibleObject {
#else
		public class RadioButtonAccessibleObject : ControlAccessibleObject {
#endif
			#region RadioButtonAccessibleObject Local Variables
			private new RadioButton	owner;
			#endregion	// RadioButtonAccessibleObject Local Variables

			#region RadioButtonAccessibleObject Constructors
			public RadioButtonAccessibleObject(RadioButton owner) : base(owner) {
				this.owner = owner;
			}
			#endregion	// RadioButtonAccessibleObject Constructors

			#region RadioButtonAccessibleObject Properties
			public override string DefaultAction {
				get {
					return "Select";
				}
			}

			public override AccessibleRole Role {
				get {
					return AccessibleRole.RadioButton;
				}
			}

			public override AccessibleStates State {
				get {
					AccessibleStates	retval;

					retval = AccessibleStates.Default;

					if (owner.check_state == CheckState.Checked) {
						retval |= AccessibleStates.Checked;
					}

					if (owner.Focused) {
						retval |= AccessibleStates.Focused;
					}

					if (owner.CanFocus) {
						retval |= AccessibleStates.Focusable;
					}

					return retval;
				}
			}
			#endregion	// RadioButtonAccessibleObject Properties

			#region RadioButtonAccessibleObject Methods
			public override void DoDefaultAction() {
				owner.PerformClick();
			}
			#endregion	// RadioButtonAccessibleObject Methods
		}
		#endregion	// RadioButtonAccessibleObject Sub-class

		#region Public Constructors
		public RadioButton() {
			appearance = Appearance.Normal;
			auto_check = true;
			radiobutton_alignment = ContentAlignment.MiddleLeft;
			TextAlign = ContentAlignment.MiddleLeft;
			TabStop = false;
		}
		#endregion	// Public Constructors

		#region Private Methods
		private void UpdateSiblings() {
			Control	c;

			if (auto_check == false) {
				return;
			}

			// Remove tabstop property from and uncheck our radio-button siblings
			c = this.Parent;
			if (c != null) {
				for (int i = 0; i < c.Controls.Count; i++) {
					if ((this != c.Controls[i]) && (c.Controls[i] is RadioButton)) {
						if (((RadioButton)(c.Controls[i])).auto_check) {
							c.Controls[i].TabStop = false;
							((RadioButton)(c.Controls[i])).Checked = false;
						}
					}
				}
			}

			this.TabStop = true;
		}

		internal override void Draw (PaintEventArgs pe) {
#if NET_2_0
			// FIXME: This should be called every time something that can affect it
			// is changed, not every paint.  Can only change so many things at a time.

			// Figure out where our text and image should go
			Rectangle glyph_rectangle;
			Rectangle text_rectangle;
			Rectangle image_rectangle;

			ThemeEngine.Current.CalculateRadioButtonTextAndImageLayout (this, Point.Empty, out glyph_rectangle, out text_rectangle, out image_rectangle);

			// Draw our button
			if (FlatStyle != FlatStyle.System)
				ThemeEngine.Current.DrawRadioButton (pe.Graphics, this, glyph_rectangle, text_rectangle, image_rectangle, pe.ClipRectangle);
			else
				ThemeEngine.Current.DrawRadioButton (pe.Graphics, this.ClientRectangle, this);
#else
			ThemeEngine.Current.DrawRadioButton (pe.Graphics, this.ClientRectangle, this);
#endif
		}
		#endregion	// Private Methods

		#region Public Instance Properties
		[DefaultValue(Appearance.Normal)]
		[Localizable(true)]
		public Appearance Appearance {
			get {
				return appearance;
			}

			set {
				if (value != appearance) {
					appearance = value;
					EventHandler eh = (EventHandler)(Events [AppearanceChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
					Invalidate();
				}
			}
		}

		[DefaultValue(true)]
		public bool AutoCheck {
			get {
				return auto_check;
			}

			set {
				auto_check = value;
			}
		}

#if !NET_2_0
		[Bindable(true)]
#endif
		[Localizable(true)]
		[DefaultValue(ContentAlignment.MiddleLeft)]
		public ContentAlignment CheckAlign {
			get {
				return radiobutton_alignment;
			}

			set {
				if (value != radiobutton_alignment) {
					radiobutton_alignment = value;

					Invalidate();
				}
			}
		}

		[DefaultValue(false)]
#if NET_2_0
		[SettingsBindable (true)]
		[Bindable (true, BindingDirection.OneWay)]
#endif
		public bool Checked {
			get {
				if (check_state != CheckState.Unchecked) {
					return true;
				}
				return false;
			}

			set {
				if (value && (check_state != CheckState.Checked)) {
					UpdateSiblings();
					check_state = CheckState.Checked;
					Invalidate();
					OnCheckedChanged(EventArgs.Empty);
				} else if (!value && (check_state != CheckState.Unchecked)) {
					check_state = CheckState.Unchecked;
					Invalidate();
					OnCheckedChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(false)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[DefaultValue(ContentAlignment.MiddleLeft)]
		[Localizable(true)]
		public override ContentAlignment TextAlign {
			get { return base.TextAlign; }
			set { base.TextAlign = value; }
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				SetStyle(ControlStyles.UserPaint, true);

				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get {
				return ThemeEngine.Current.RadioButtonDefaultSize;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void PerformClick() {
			OnClick(EventArgs.Empty);
		}

		public override string ToString() {
			return base.ToString() + ", Checked: " + this.Checked;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override AccessibleObject CreateAccessibilityInstance() {
			AccessibleObject	ao;

			ao = base.CreateAccessibilityInstance ();
			ao.role = AccessibleRole.RadioButton;

			return ao;
		}

		protected virtual void OnCheckedChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [CheckedChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnClick(EventArgs e) {
			if (auto_check) {
				if (!Checked) {
					Checked = true;
				}
			} else {
				Checked = !Checked;
			}
			
			base.OnClick (e);
		}

		protected override void OnEnter(EventArgs e) {
			base.OnEnter(e);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
		}

		protected override void OnMouseUp(MouseEventArgs mevent) {
			base.OnMouseUp(mevent);
		}

		protected override bool ProcessMnemonic(char charCode) {
			if (IsMnemonic(charCode, Text) == true) {
				Select();
				PerformClick();
				return true;
			}
			
			return base.ProcessMnemonic(charCode);
		}
		#endregion	// Protected Instance Methods

		#region Events
		static object AppearanceChangedEvent = new object ();
		static object CheckedChangedEvent = new object ();

		public event EventHandler AppearanceChanged {
			add { Events.AddHandler (AppearanceChangedEvent, value); }
			remove { Events.RemoveHandler (AppearanceChangedEvent, value); }
		}

		public event EventHandler CheckedChanged {
			add { Events.AddHandler (CheckedChangedEvent, value); }
			remove { Events.RemoveHandler (CheckedChangedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}
		
#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDoubleClick { 
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}
#endif
		#endregion	// Events
	}
}

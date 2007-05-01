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
// Copyright (c) 2004-2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Ravindra Kumar (rkumar@novell.com)
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Mike Kestner (mkestner@novell.com)
//	Daniel Nauck (dna(at)mono-project(dot)de)
//
// TODO:
//   - Feedback for item activation, change in cursor types as mouse moves.
//   - Drag and drop


// NOT COMPLETE


using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Globalization;
#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.Windows.Forms
{
	[DefaultEvent ("SelectedIndexChanged")]
	[DefaultProperty ("Items")]
	[Designer ("System.Windows.Forms.Design.ListViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[Docking (DockingBehavior.Ask)]
#endif
	public class ListView : Control
	{
		private ItemActivation activation = ItemActivation.Standard;
		private ListViewAlignment alignment = ListViewAlignment.Top;
		private bool allow_column_reorder;
		private bool auto_arrange = true;
		private bool check_boxes;
		private readonly CheckedIndexCollection checked_indices;
		private readonly CheckedListViewItemCollection checked_items;
		private readonly ColumnHeaderCollection columns;
		internal ListViewItem focused_item;
		private bool full_row_select;
		private bool grid_lines;
		private ColumnHeaderStyle header_style = ColumnHeaderStyle.Clickable;
		private bool hide_selection = true;
		private bool hover_selection;
		private IComparer item_sorter;
		private readonly ListViewItemCollection items;
#if NET_2_0
		private readonly ListViewGroupCollection groups;
		private bool owner_draw;
		private bool show_groups = true;
#endif
		private bool label_edit;
		private bool label_wrap = true;
		private bool multiselect = true;
		private bool scrollable = true;
		private bool hover_pending;
		private readonly SelectedIndexCollection selected_indices;
		private readonly SelectedListViewItemCollection selected_items;
		private SortOrder sort_order = SortOrder.None;
		private ImageList state_image_list;
		private bool updating;
		private View view = View.LargeIcon;
		private int layout_wd;    // We might draw more than our client area
		private int layout_ht;    // therefore we need to have these two.
		HeaderControl header_control;
		internal ItemControl item_control;
		internal ScrollBar h_scroll; // used for scrolling horizontally
		internal ScrollBar v_scroll; // used for scrolling vertically
		internal int h_marker;		// Position markers for scrolling
		internal int v_marker;
		private int keysearch_tickcnt;
		private string keysearch_text;
		static private readonly int keysearch_keydelay = 1000;
		private int[] reordered_column_indices;
		private Point [] items_location;
		private ItemMatrixLocation [] items_matrix_location;
		private Size item_size; // used for caching item size
#if NET_2_0
		private Size tile_size;
		private bool virtual_mode;
		private int virtual_list_size;
#endif

		// internal variables
		internal ImageList large_image_list;
		internal ImageList small_image_list;
		internal Size text_size = Size.Empty;

		#region Events
		static object AfterLabelEditEvent = new object ();
		static object BeforeLabelEditEvent = new object ();
		static object ColumnClickEvent = new object ();
		static object ItemActivateEvent = new object ();
		static object ItemCheckEvent = new object ();
		static object ItemDragEvent = new object ();
		static object SelectedIndexChangedEvent = new object ();
#if NET_2_0
		static object DrawColumnHeaderEvent = new object();
		static object DrawItemEvent = new object();
		static object DrawSubItemEvent = new object();
		static object ItemCheckedEvent = new object ();
		static object ItemMouseHoverEvent = new object ();
		static object CacheVirtualItemsEvent = new object ();
		static object RetrieveVirtualItemEvent = new object ();
#endif

		public event LabelEditEventHandler AfterLabelEdit {
			add { Events.AddHandler (AfterLabelEditEvent, value); }
			remove { Events.RemoveHandler (AfterLabelEditEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		public event LabelEditEventHandler BeforeLabelEdit {
			add { Events.AddHandler (BeforeLabelEditEvent, value); }
			remove { Events.RemoveHandler (BeforeLabelEditEvent, value); }
		}

		public event ColumnClickEventHandler ColumnClick {
			add { Events.AddHandler (ColumnClickEvent, value); }
			remove { Events.RemoveHandler (ColumnClickEvent, value); }
		}

#if NET_2_0
		public event DrawListViewColumnHeaderEventHandler DrawColumnHeader {
			add { Events.AddHandler(DrawColumnHeaderEvent, value); }
			remove { Events.RemoveHandler(DrawColumnHeaderEvent, value); }
		}

		public event DrawListViewItemEventHandler DrawItem {
			add { Events.AddHandler(DrawItemEvent, value); }
			remove { Events.RemoveHandler(DrawItemEvent, value); }
		}

		public event DrawListViewSubItemEventHandler DrawSubItem {
			add { Events.AddHandler(DrawSubItemEvent, value); }
			remove { Events.RemoveHandler(DrawSubItemEvent, value); }
		}
#endif

		public event EventHandler ItemActivate {
			add { Events.AddHandler (ItemActivateEvent, value); }
			remove { Events.RemoveHandler (ItemActivateEvent, value); }
		}

		public event ItemCheckEventHandler ItemCheck {
			add { Events.AddHandler (ItemCheckEvent, value); }
			remove { Events.RemoveHandler (ItemCheckEvent, value); }
		}

#if NET_2_0
		public event ItemCheckedEventHandler ItemChecked {
			add { Events.AddHandler (ItemCheckedEvent, value); }
			remove { Events.RemoveHandler (ItemCheckedEvent, value); }
		}
#endif

		public event ItemDragEventHandler ItemDrag {
			add { Events.AddHandler (ItemDragEvent, value); }
			remove { Events.RemoveHandler (ItemDragEvent, value); }
		}

#if NET_2_0
		public event ListViewItemMouseHoverEventHandler ItemMouseHover {
			add { Events.AddHandler (ItemMouseHoverEvent, value); }
			remove { Events.RemoveHandler (ItemMouseHoverEvent, value); }
		}
#endif

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

#if NET_2_0
		public event CacheVirtualItemsEventHandler CacheVirtualItems {
			add { Events.AddHandler (CacheVirtualItemsEvent, value); }
			remove { Events.RemoveHandler (CacheVirtualItemsEvent, value); }
		}

		public event RetrieveVirtualItemEventHandler RetrieveVirtualItem {
			add { Events.AddHandler (RetrieveVirtualItemEvent, value); }
			remove { Events.RemoveHandler (RetrieveVirtualItemEvent, value); }
		}
#endif

		#endregion // Events

		#region Public Constructors
		public ListView ()
		{
			background_color = ThemeEngine.Current.ColorWindow;
			items = new ListViewItemCollection (this);
#if NET_2_0
			groups = new ListViewGroupCollection (this);
#endif
			checked_indices = new CheckedIndexCollection (this);
			checked_items = new CheckedListViewItemCollection (this);
			columns = new ColumnHeaderCollection (this);
			foreground_color = SystemColors.WindowText;
			selected_indices = new SelectedIndexCollection (this);
			selected_items = new SelectedListViewItemCollection (this);
			items_location = new Point [16];
			items_matrix_location = new ItemMatrixLocation [16];

			InternalBorderStyle = BorderStyle.Fixed3D;

			header_control = new HeaderControl (this);
			header_control.Visible = false;
			Controls.AddImplicit (header_control);

			item_control = new ItemControl (this);
			Controls.AddImplicit (item_control);

			h_scroll = new ImplicitHScrollBar ();
			Controls.AddImplicit (this.h_scroll);

			v_scroll = new ImplicitVScrollBar ();
			Controls.AddImplicit (this.v_scroll);

			h_marker = v_marker = 0;
			keysearch_tickcnt = 0;

			// scroll bars are disabled initially
			h_scroll.Visible = false;
			h_scroll.ValueChanged += new EventHandler(HorizontalScroller);
			v_scroll.Visible = false;
			v_scroll.ValueChanged += new EventHandler(VerticalScroller);

			// event handlers
			base.KeyDown += new KeyEventHandler(ListView_KeyDown);
			SizeChanged += new EventHandler (ListView_SizeChanged);
			GotFocus += new EventHandler (FocusChanged);
			LostFocus += new EventHandler (FocusChanged);
			MouseWheel += new MouseEventHandler(ListView_MouseWheel);
			MouseEnter += new EventHandler (ListView_MouseEnter);

			this.SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick
#if NET_2_0
				| ControlStyles.UseTextForAccessibility
#endif
				, false);
		}
		#endregion	// Public Constructors

		#region Private Internal Properties
		internal Size CheckBoxSize {
			get {
				if (this.check_boxes) {
					if (this.state_image_list != null)
						return this.state_image_list.ImageSize;
					else
						return ThemeEngine.Current.ListViewCheckBoxSize;
				}
				return Size.Empty;
			}
		}

		internal Size ItemSize {
			get {
				if (view != View.Details)
					return item_size;

				Size size = new Size ();
				size.Height = item_size.Height;
				for (int i = 0; i < columns.Count; i++)
					size.Width += columns [i].Wd;

				return size;
			}
			set {
				item_size = value;
			}
		}

		#endregion	// Private Internal Properties

		#region	 Protected Properties
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.ListViewDefaultSize; }
		}
		#endregion	// Protected Properties

		#region Public Instance Properties
		[DefaultValue (ItemActivation.Standard)]
		public ItemActivation Activation {
			get { return activation; }
			set { 
				if (value != ItemActivation.Standard && value != ItemActivation.OneClick && 
					value != ItemActivation.TwoClick) {
					throw new InvalidEnumArgumentException (string.Format
						("Enum argument value '{0}' is not valid for Activation", value));
				}
				
				activation = value;
			}
		}

		[DefaultValue (ListViewAlignment.Top)]
		[Localizable (true)]
		public ListViewAlignment Alignment {
			get { return alignment; }
			set {
				if (value != ListViewAlignment.Default && value != ListViewAlignment.Left && 
					value != ListViewAlignment.SnapToGrid && value != ListViewAlignment.Top) {
					throw new InvalidEnumArgumentException (string.Format 
						("Enum argument value '{0}' is not valid for Alignment", value));
				}
				
				if (this.alignment != value) {
					alignment = value;
					// alignment does not matter in Details/List views
					if (this.view == View.LargeIcon || this.View == View.SmallIcon)
						this.Redraw (true);
				}
			}
		}

		[DefaultValue (false)]
		public bool AllowColumnReorder {
			get { return allow_column_reorder; }
			set { allow_column_reorder = value; }
		}

		[DefaultValue (true)]
		public bool AutoArrange {
			get { return auto_arrange; }
			set {
				if (auto_arrange != value) {
					auto_arrange = value;
					// autoarrange does not matter in Details/List views
					if (this.view == View.LargeIcon || this.View == View.SmallIcon)
						this.Redraw (true);
				}
			}
		}

		public override Color BackColor {
			get {
				if (background_color.IsEmpty)
					return ThemeEngine.Current.ColorWindow;
				else
					return background_color;
			}
			set { background_color = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[DefaultValue (BorderStyle.Fixed3D)]
		[DispId (-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}

		[DefaultValue (false)]
		public bool CheckBoxes {
			get { return check_boxes; }
			set {
				if (check_boxes != value) {
#if NET_2_0
					if (value && View == View.Tile)
						throw new NotSupportedException ("CheckBoxes are not"
							+ " supported in Tile view. Choose a different"
							+ " view or set CheckBoxes to false.");
#endif

					check_boxes = value;
					this.Redraw (true);
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedIndexCollection CheckedIndices {
			get { return checked_indices; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedListViewItemCollection CheckedItems {
			get { return checked_items; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]
		public ColumnHeaderCollection Columns {
			get { return columns; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ListViewItem FocusedItem {
			get {
				return focused_item;
			}
#if NET_2_0
			set {
				throw new NotImplementedException ();
			}
#endif
		}

		public override Color ForeColor {
			get {
				if (foreground_color.IsEmpty)
					return ThemeEngine.Current.ColorWindowText;
				else
					return foreground_color;
			}
			set { foreground_color = value; }
		}

		[DefaultValue (false)]
		public bool FullRowSelect {
			get { return full_row_select; }
			set { 
				if (full_row_select != value) {
					full_row_select = value;
					InvalidateSelection ();
				}
			}
		}

		[DefaultValue (false)]
		public bool GridLines {
			get { return grid_lines; }
			set {
				if (grid_lines != value) {
					grid_lines = value;
					this.Redraw (false);
				}
			}
		}

		[DefaultValue (ColumnHeaderStyle.Clickable)]
		public ColumnHeaderStyle HeaderStyle {
			get { return header_style; }
			set {
				if (header_style == value)
					return;

				switch (value) {
				case ColumnHeaderStyle.Clickable:
				case ColumnHeaderStyle.Nonclickable:
				case ColumnHeaderStyle.None:
					break;
				default:
					throw new InvalidEnumArgumentException (string.Format 
						("Enum argument value '{0}' is not valid for ColumnHeaderStyle", value));
				}
				
				header_style = value;
				if (view == View.Details)
					Redraw (true);
			}
		}

		[DefaultValue (true)]
		public bool HideSelection {
			get { return hide_selection; }
			set {
				if (hide_selection != value) {
					hide_selection = value;
					InvalidateSelection ();
				}
			}
		}

		[DefaultValue (false)]
		public bool HoverSelection {
			get { return hover_selection; }
			set { hover_selection = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]
		public ListViewItemCollection Items {
			get { return items; }
		}

		[DefaultValue (false)]
		public bool LabelEdit {
			get { return label_edit; }
			set { label_edit = value; }
		}

		[DefaultValue (true)]
		[Localizable (true)]
		public bool LabelWrap {
			get { return label_wrap; }
			set {
				if (label_wrap != value) {
					label_wrap = value;
					this.Redraw (true);
				}
			}
		}

		[DefaultValue (null)]
		public ImageList LargeImageList {
			get { return large_image_list; }
			set {
				large_image_list = value;
				this.Redraw (true);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IComparer ListViewItemSorter {
			get {
				if (View != View.SmallIcon && View != View.LargeIcon && item_sorter is ItemComparer)
					return null;
				return item_sorter;
			}
			set {
				if (item_sorter != value) {
					item_sorter = value;
					Sort ();
				}
			}
		}

		[DefaultValue (true)]
		public bool MultiSelect {
			get { return multiselect; }
			set { multiselect = value; }
		}


#if NET_2_0
		[DefaultValue(false)]
		public bool OwnerDraw {
			get { return owner_draw; }
		}
#endif

		[DefaultValue (true)]
		public bool Scrollable {
			get { return scrollable; }
			set {
				if (scrollable != value) {
					scrollable = value;
					this.Redraw (true);
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedIndexCollection SelectedIndices {
			get { return selected_indices; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedListViewItemCollection SelectedItems {
			get { return selected_items; }
		}

#if NET_2_0
		[DefaultValue(true)]
		public bool ShowGroups {
			get { return show_groups; }
			set {
				if (show_groups != value) {
					show_groups = value;
					Redraw(true);
				}
			}
		}

		[LocalizableAttribute (true)]
		[MergableProperty (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public ListViewGroupCollection Groups {
			get { return groups; }
		}
#endif

		[DefaultValue (null)]
		public ImageList SmallImageList {
			get { return small_image_list; }
			set {
				small_image_list = value;
				this.Redraw (true);
			}
		}

		[DefaultValue (SortOrder.None)]
		public SortOrder Sorting {
			get { return sort_order; }
			set { 
				if (!Enum.IsDefined (typeof (SortOrder), value)) {
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (SortOrder));
				}
				
				if (sort_order == value)
					return;

				sort_order = value;

#if NET_2_0
				if (virtual_mode) // Sorting is not allowed in virtual mode
					return;
#endif

				if (value == SortOrder.None) {
					if (item_sorter != null) {
						// ListViewItemSorter should never be reset for SmallIcon
						// and LargeIcon view
						if (View != View.SmallIcon && View != View.LargeIcon)
#if NET_2_0
							item_sorter = null;
#else
							// in .NET 1.1, only internal IComparer would be
							// set to null
							if (item_sorter is ItemComparer)
								item_sorter = null;
#endif
					}
					this.Redraw (false);
				} else {
					if (item_sorter == null)
						item_sorter = new ItemComparer (value);
					if (item_sorter is ItemComparer) {
#if NET_2_0
						item_sorter = new ItemComparer (value);
#else
						// in .NET 1.1, the sort order is not updated for
						// SmallIcon and LargeIcon views if no custom IComparer
						// is set
						if (View != View.SmallIcon && View != View.LargeIcon)
							item_sorter = new ItemComparer (value);
#endif
					}
					Sort ();
				}
			}
		}

		private void OnImageListChanged (object sender, EventArgs args)
		{
			item_control.Invalidate ();
		}

		[DefaultValue (null)]
		public ImageList StateImageList {
			get { return state_image_list; }
			set {
				if (state_image_list == value)
					return;

				if (state_image_list != null)
					state_image_list.Images.Changed -= new EventHandler (OnImageListChanged);

				state_image_list = value;

				if (state_image_list != null)
					state_image_list.Images.Changed += new EventHandler (OnImageListChanged);

				this.Redraw (true);
			}
		}

		[Bindable (false)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; } 
			set {
				if (value == base.Text)
					return;

				base.Text = value;
				this.Redraw (true);
			}
		}

#if NET_2_0
		[Browsable (true)]
		public Size TileSize {
			get {
				return tile_size;
			}
			set {
				if (value.Width <= 0 || value.Height <= 0)
					throw new ArgumentOutOfRangeException ("value");

				tile_size = value;
				Redraw (true);
			}
		}
#endif

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ListViewItem TopItem {
			get {
				// there is no item
				if (this.items.Count == 0)
					return null;
				// if contents are not scrolled
				// it is the first item
				else if (h_marker == 0 && v_marker == 0)
					return this.items [0];
				// do a hit test for the scrolled position
				else {
					for (int i = 0; i < items.Count; i++) {
						Point item_loc = GetItemLocation (i);
						if (item_loc.X >= 0 && item_loc.Y >= 0)
							return items [i];
					}
					return null;
				}
			}
#if NET_2_0
			set {
				throw new NotImplementedException ();
			}
#endif
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DefaultValue (true)]
		[Browsable (false)]
		[MonoInternalNote ("Stub, not implemented")]
		public bool UseCompatibleStateImageBehavior {
			get {
				return false;
			}
			set {
			}
		}
#endif

		[DefaultValue (View.LargeIcon)]
		public View View {
			get { return view; }
			set { 
				if (!Enum.IsDefined (typeof (View), value))
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (View));

				if (view != value) {
#if NET_2_0
					if (CheckBoxes && value == View.Tile)
						throw new NotSupportedException ("CheckBoxes are not"
							+ " supported in Tile view. Choose a different"
							+ " view or set CheckBoxes to false.");
#endif

					h_scroll.Value = v_scroll.Value = 0;
					view = value; 
					Redraw (true);
				}
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool VirtualMode {
			get {
				return virtual_mode;
			}
			set {
				if (virtual_mode == value)
					return;

				if (!virtual_mode && items.Count > 0)
					throw new InvalidOperationException ();

				virtual_mode = value;
				Redraw (true);
			}
		}

		[DefaultValue (0)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int VirtualListSize {
			get {
				return virtual_list_size;
			}
			set {
				if (value < 0)
					throw new ArgumentException ("value");

				if (virtual_list_size == value)
					return;

				virtual_list_size = value;
				if (virtual_mode)
					Redraw (true);
			}
		}
#endif
		#endregion	// Public Instance Properties

		#region Internal Methods Properties
		
		internal int FirstVisibleIndex {
			get {
				// there is no item
				if (this.items.Count == 0)
					return 0;
				
				if (h_marker == 0 && v_marker == 0)
					return 0;
				
				Size item_size = ItemSize;
				for (int i = 0; i < items.Count; i++) {
					Rectangle item_rect = new Rectangle (GetItemLocation (i), item_size);
					if (item_rect.Right >= 0 && item_rect.Bottom >= 0)
						return i;
				}

				return 0;
			}
		}

		
		internal int LastVisibleIndex {
			get {
				for (int i = FirstVisibleIndex; i < Items.Count; i++) {
					if (View == View.List || Alignment == ListViewAlignment.Left) {
						if (GetItemLocation (i).X > item_control.ClientRectangle.Right)
							return i - 1;
					} else {
						if (GetItemLocation (i).Y > item_control.ClientRectangle.Bottom)
							return i - 1;
					}
				}
				
				return Items.Count - 1;
			}
		}

		internal void OnSelectedIndexChanged ()
		{
			if (IsHandleCreated)
				OnSelectedIndexChanged (EventArgs.Empty);
		}

		internal int TotalWidth {
			get { return Math.Max (this.Width, this.layout_wd); }
		}

		internal int TotalHeight {
			get { return Math.Max (this.Height, this.layout_ht); }
		}

		internal void Redraw (bool recalculate)
		{
			// Avoid calculations when control is being updated
			if (updating)
				return;
#if NET_2_0
			// VirtualMode doesn't do any calculations until handle is created
			if (virtual_mode && !IsHandleCreated)
				return;
#endif


			if (recalculate)
				CalculateListView (this.alignment);

			Refresh ();
		}

		void InvalidateSelection ()
		{
			foreach (int selected_index in SelectedIndices)
				items [selected_index].Invalidate ();
		}

		const int text_padding = 15;

		internal Size GetChildColumnSize (int index)
		{
			Size ret_size = Size.Empty;
			ColumnHeader col = this.columns [index];

			if (col.Width == -2) { // autosize = max(items, columnheader)
				Size size = Size.Ceiling (this.DeviceContext.MeasureString
					(col.Text, this.Font));
				size.Width += text_padding;
				ret_size = BiggestItem (index);
				if (size.Width > ret_size.Width)
					ret_size = size;
			}
			else { // -1 and all the values < -2 are put under one category
				ret_size = BiggestItem (index);
				// fall back to empty columns' width if no subitem is available for a column
				if (ret_size.IsEmpty) {
					ret_size.Width = ThemeEngine.Current.ListViewEmptyColumnWidth;
					if (col.Text.Length > 0)
						ret_size.Height = Size.Ceiling (this.DeviceContext.MeasureString
										(col.Text, this.Font)).Height;
					else
						ret_size.Height = this.Font.Height;
				}
			}

			ret_size.Height += text_padding;

			// adjust the size for icon and checkbox for 0th column
			if (index == 0) {
				ret_size.Width += (this.CheckBoxSize.Width + 4);
				if (this.small_image_list != null)
					ret_size.Width += this.small_image_list.ImageSize.Width;
			}
			return ret_size;
		}

		// Returns the size of biggest item text in a column.
		private Size BiggestItem (int col)
		{
			Size temp = Size.Empty;
			Size ret_size = Size.Empty;

#if NET_2_0
			// VirtualMode uses the first item text size
			if (virtual_mode && items.Count > 0) {
				ListViewItem item = items [0];
				ret_size = Size.Ceiling (DeviceContext.MeasureString (item.SubItems [col].Text,
							Font));
			} else {
#endif
				// 0th column holds the item text, we check the size of
				// the various subitems falling in that column and get
				// the biggest one's size.
				foreach (ListViewItem item in items) {
					if (col >= item.SubItems.Count)
						continue;

					temp = Size.Ceiling (DeviceContext.MeasureString
								(item.SubItems [col].Text, Font));
					if (temp.Width > ret_size.Width)
						ret_size = temp;
				}
#if NET_2_0
			}
#endif

			// adjustment for space
			if (!ret_size.IsEmpty)
				ret_size.Width += 4;

			return ret_size;
		}

		const int max_wrap_padding = 38;

		// Sets the size of the biggest item text as per the view
		private void CalcTextSize ()
		{
			// clear the old value
			text_size = Size.Empty;

			if (items.Count == 0)
				return;

			text_size = BiggestItem (0);

			if (view == View.LargeIcon && this.label_wrap) {
				Size temp = Size.Empty;
				if (this.check_boxes)
					temp.Width += 2 * this.CheckBoxSize.Width;
				int icon_w = LargeImageList == null ? 12 : LargeImageList.ImageSize.Width;
				temp.Width += icon_w + max_wrap_padding;
				// wrapping is done for two lines only
				if (text_size.Width > temp.Width) {
					text_size.Width = temp.Width;
					text_size.Height *= 2;
				}
			}
			else if (view == View.List) {
				// in list view max text shown in determined by the
				// control width, even if scolling is enabled.
				int max_wd = this.Width - (this.CheckBoxSize.Width - 2);
				if (this.small_image_list != null)
					max_wd -= this.small_image_list.ImageSize.Width;

				if (text_size.Width > max_wd)
					text_size.Width = max_wd;
			}

			// we do the default settings, if we have got 0's
			if (text_size.Height <= 0)
				text_size.Height = this.Font.Height;
			if (text_size.Width <= 0)
				text_size.Width = this.Width;

			// little adjustment
			text_size.Width += 4;
			text_size.Height += 2;
		}

		private void Scroll (ScrollBar scrollbar, int delta)
		{
			if (delta == 0 || !scrollbar.Visible)
				return;

			int max;
			if (scrollbar == h_scroll)
				max = h_scroll.Maximum - item_control.Width;
			else
				max = v_scroll.Maximum - item_control.Height;

			int val = scrollbar.Value + delta;
			if (val > max)
				val = max;
			else if (val < scrollbar.Minimum)
				val = scrollbar.Minimum;
			scrollbar.Value = val;
		}

		private void CalculateScrollBars ()
		{
			Rectangle client_area = ClientRectangle;
			
			if (!scrollable) {
				h_scroll.Visible = false;
				v_scroll.Visible = false;
				item_control.Location = new Point (0, header_control.Height);
				item_control.Height = ClientRectangle.Width - header_control.Height;
				item_control.Width = ClientRectangle.Width;
				header_control.Width = ClientRectangle.Width;
				return;
			}

			// Don't calculate if the view is not displayable
			if (client_area.Height < 0 || client_area.Width < 0)
				return;

			// making a scroll bar visible might make
			// other scroll bar visible
			if (layout_wd > client_area.Right) {
				h_scroll.Visible = true;
				if ((layout_ht + h_scroll.Height) > client_area.Bottom)
					v_scroll.Visible = true;
				else
					v_scroll.Visible = false;
			} else if (layout_ht > client_area.Bottom) {
				v_scroll.Visible = true;
				if ((layout_wd + v_scroll.Width) > client_area.Right)
					h_scroll.Visible = true;
				else
					h_scroll.Visible = false;
			} else {
				h_scroll.Visible = false;
				v_scroll.Visible = false;
			}

			item_control.Height = ClientRectangle.Height - header_control.Height;

			if (h_scroll.is_visible) {
				h_scroll.Location = new Point (client_area.X, client_area.Bottom - h_scroll.Height);
				h_scroll.Minimum = 0;

				// if v_scroll is visible, adjust the maximum of the
				// h_scroll to account for the width of v_scroll
				if (v_scroll.Visible) {
					h_scroll.Maximum = layout_wd + v_scroll.Width;
					h_scroll.Width = client_area.Width - v_scroll.Width;
				}
				else {
					h_scroll.Maximum = layout_wd;
					h_scroll.Width = client_area.Width;
				}

				h_scroll.LargeChange = client_area.Width;
				h_scroll.SmallChange = Font.Height;
				item_control.Height -= h_scroll.Height;
			}

			if (header_control.is_visible)
				header_control.Width = ClientRectangle.Width;
			item_control.Width = ClientRectangle.Width;

			if (v_scroll.is_visible) {
				v_scroll.Location = new Point (client_area.Right - v_scroll.Width, client_area.Y);
				v_scroll.Minimum = 0;

				// if h_scroll is visible, adjust the maximum of the
				// v_scroll to account for the height of h_scroll
				if (h_scroll.Visible) {
					v_scroll.Maximum = layout_ht + h_scroll.Height;
					v_scroll.Height = client_area.Height; // - h_scroll.Height already done 
				} else {
					v_scroll.Maximum = layout_ht;
					v_scroll.Height = client_area.Height;
				}

				v_scroll.LargeChange = client_area.Height;
				v_scroll.SmallChange = Font.Height;
				if (header_control.Visible)
					header_control.Width -= v_scroll.Width;
				item_control.Width -= v_scroll.Width;
			}
		}

#if NET_2_0
		internal int GetReorderedColumnIndex (ColumnHeader column)
		{
			if (reordered_column_indices == null)
				return column.Index;

			for (int i = 0; i < Columns.Count; i++)
				if (reordered_column_indices [i] == column.Index)
					return i;

			return -1;
		}
#endif

		internal ColumnHeader GetReorderedColumn (int index)
		{
			if (reordered_column_indices == null)
				return Columns [index];
			else
				return Columns [reordered_column_indices [index]];
		}

		internal void ReorderColumn (ColumnHeader col, int index, bool fireEvent)
		{
#if NET_2_0
			if (fireEvent) {
				ColumnReorderedEventHandler eh = (ColumnReorderedEventHandler) (Events [ColumnReorderedEvent]);
				if (eh != null){
					ColumnReorderedEventArgs args = new ColumnReorderedEventArgs (col.Index, index, col);

					eh (this, args);
					if (args.Cancel) {
						header_control.Invalidate ();
						item_control.Invalidate ();
						return;
					}
				}
			}
#endif
			int column_count = Columns.Count;

			if (reordered_column_indices == null) {
				reordered_column_indices = new int [column_count];
				for (int i = 0; i < column_count; i++)
					reordered_column_indices [i] = i;
			}

			if (reordered_column_indices [index] == col.Index)
				return;

			int[] curr = reordered_column_indices;
			int [] result = new int [column_count];
			int curr_idx = 0;
			for (int i = 0; i < column_count; i++) {
				if (curr_idx < column_count && curr [curr_idx] == col.Index)
					curr_idx++;

				if (i == index)
					result [i] = col.Index;
				else
					result [i] = curr [curr_idx++];
			}

			ReorderColumns (result, true);
		}

		internal void ReorderColumns (int [] display_indices, bool redraw)
		{
			reordered_column_indices = display_indices;
			for (int i = 0; i < Columns.Count; i++) {
				ColumnHeader col = Columns [i];
				col.InternalDisplayIndex = reordered_column_indices [i];
			}
			if (redraw && view == View.Details && IsHandleCreated) {
				LayoutDetails ();
				header_control.Invalidate ();
				item_control.Invalidate ();
			}
		}

		internal void AddColumn (ColumnHeader newCol, int index, bool redraw)
		{
			int column_count = Columns.Count;
			newCol.SetListView (this);

			int [] display_indices = new int [column_count];
			for (int i = 0; i < column_count; i++) {
				ColumnHeader col = Columns [i];
				if (i == index) {
					display_indices [i] = index;
				} else {
					int display_index = col.InternalDisplayIndex;
					if (display_index < index) {
						display_indices [i] = display_index;
					} else {
						display_indices [i] = (display_index + 1);
					}
				}
			}

			ReorderColumns (display_indices, redraw);
			Invalidate ();
		}

		Size LargeIconItemSize
		{
			get {
				int image_w = LargeImageList == null ? 12 : LargeImageList.ImageSize.Width;
				int image_h = LargeImageList == null ? 2 : LargeImageList.ImageSize.Height;
				int w = CheckBoxSize.Width + 2 + Math.Max (text_size.Width, image_w);
				int h = text_size.Height + 2 + Math.Max (CheckBoxSize.Height, image_h);
				return new Size (w, h);
			}
		}

		Size SmallIconItemSize {
			get {
				int image_w = SmallImageList == null ? 0 : SmallImageList.ImageSize.Width;
				int image_h = SmallImageList == null ? 0 : SmallImageList.ImageSize.Height;
				int w = text_size.Width + 2 + CheckBoxSize.Width + image_w;
				int h = Math.Max (text_size.Height, Math.Max (CheckBoxSize.Height, image_h));
				return new Size (w, h);
			}
		}

#if NET_2_0
		Size TileItemSize {
			get {
				// Calculate tile size if needed
				// It appears that using Font.Size instead of a SizeF value can give us
				// a slightly better approach to the proportions defined in .Net
				if (tile_size == Size.Empty) {
					int image_w = LargeImageList == null ? 0 : LargeImageList.ImageSize.Width;
					int image_h = LargeImageList == null ? 0 : LargeImageList.ImageSize.Height;
					int w = (int)Font.Size * ThemeEngine.Current.ListViewTileWidthFactor + image_w + 4;
					int h = Math.Max ((int)Font.Size * ThemeEngine.Current.ListViewTileHeightFactor, image_h);
				
					tile_size = new Size (w, h);
				}
			
				return tile_size;
			}
		}
#endif

		int GetDetailsItemHeight ()
		{
			int item_height;
			int checkbox_height = CheckBoxes ? CheckBoxSize.Height : 0;
			int small_image_height = SmallImageList == null ? 0 : SmallImageList.ImageSize.Height;
			item_height = Math.Max (checkbox_height, text_size.Height);
			item_height = Math.Max (item_height, small_image_height);
			return item_height;
		}


		void SetItemLocation (int index, int x, int y, int row, int col)
		{
			Point old_location = items_location [index];
			if (old_location.X == x && old_location.Y == y)
				return;

			Size item_size = ItemSize;
			Rectangle old_rect = new Rectangle (GetItemLocation (index), item_size);

			items_location [index] = new Point (x, y);
			items_matrix_location [index] = new ItemMatrixLocation (row, col);

			// Invalidate both previous and new bounds
			item_control.Invalidate (old_rect);
			item_control.Invalidate (new Rectangle (GetItemLocation (index), item_size));
		}

		int rows;
		int cols;
		int[,] item_index_matrix;

		void LayoutIcons (Size item_size, bool left_aligned, int x_spacing, int y_spacing)
		{
			header_control.Visible = false;
			header_control.Size = Size.Empty;
			item_control.Visible = true;
			item_control.Location = Point.Empty;
			ItemSize = item_size; // Cache item size

			if (items.Count == 0)
				return;

			Size sz = item_size;
			Rectangle area = ClientRectangle;

			if (left_aligned) {
				rows = (int) Math.Floor ((double)(area.Height - h_scroll.Height + y_spacing) / (double)(sz.Height + y_spacing));
				if (rows <= 0)
					rows = 1;
				cols = (int) Math.Ceiling ((double)items.Count / (double)rows);
			} else {
				cols = (int) Math.Floor ((double)(area.Width - v_scroll.Width + x_spacing) / (double)(sz.Width + x_spacing));
				if (cols <= 0)
					cols = 1;
				rows = (int) Math.Ceiling ((double)items.Count / (double)cols);
			}

			layout_ht = rows * (sz.Height + y_spacing) - y_spacing;
			layout_wd = cols * (sz.Width + x_spacing) - x_spacing;
			item_index_matrix = new int [rows, cols];
			int row = 0;
			int col = 0;
			for (int i = 0; i < items.Count; i++) {
				int x = col * (sz.Width + x_spacing);
				int y = row * (sz.Height + y_spacing);
				SetItemLocation (i, x, y, row, col);
				item_index_matrix [row, col] = i;
#if NET_2_0
				if (!virtual_mode) // Virtual mode sets Layout until draw time
#endif
					items [i].Layout ();

				if (left_aligned) {
					if (++row == rows) {
						row = 0;
						col++;
					}
				} else {
					if (++col == cols) {
						col = 0;
						row++;
					}
				}
			}

			item_control.Size = new Size (layout_wd, layout_ht);
		}

		void LayoutHeader ()
		{
			int x = 0;
			for (int i = 0; i < Columns.Count; i++) {
				ColumnHeader col = GetReorderedColumn (i);
				col.X = x;
				col.Y = 0;
				col.CalcColumnHeader ();
				x += col.Wd;
			}

			if (x < ClientRectangle.Width)
				x = ClientRectangle.Width;

			if (header_style == ColumnHeaderStyle.None) {
				header_control.Visible = false;
				header_control.Size = Size.Empty;
			} else {
				header_control.Width = x;
				header_control.Height = columns.Count > 0 ? columns [0].Ht : Font.Height + 5;
				header_control.Visible = true;
			}
		}

		void LayoutDetails ()
		{
			LayoutHeader ();

			if (columns.Count == 0) {
				item_control.Visible = false;
				layout_wd = ClientRectangle.Width;
				layout_ht = ClientRectangle.Height;
				return;
			}

			item_control.Visible = true;
			item_control.Location = new Point (0, header_control.Height);
			item_control.Width = ClientRectangle.Width;

			int item_height = GetDetailsItemHeight ();
			ItemSize = new Size (0, item_height); // We only cache Height for details view

			int y = 0; 
			if (items.Count > 0) {
				for (int i = 0; i < items.Count; i++) {
					SetItemLocation (i, 0, y, 0, 0);
#if NET_2_0
					if (!virtual_mode) // Virtual mode sets Layout until draw time
#endif
						items [i].Layout ();

					y += item_height + 2;
				}

				// some space for bottom gridline
				if (grid_lines)
					y += 2;
			}

			layout_wd = Math.Max (header_control.Width, item_control.Width);
			layout_ht = y + header_control.Height;
		}

		private void AdjustItemsPositionArray (int count)
		{
			if (items_location.Length >= count)
				return;

			// items_location and items_matrix_location must keep the same length
			count = Math.Max (count, items_location.Length * 2);
			items_location = new Point [count];
			items_matrix_location = new ItemMatrixLocation [count];
		}

		private void CalculateListView (ListViewAlignment align)
		{
			CalcTextSize ();

			AdjustItemsPositionArray (items.Count);

			switch (view) {
			case View.Details:
				LayoutDetails ();
				break;

			case View.SmallIcon:
				LayoutIcons (SmallIconItemSize, alignment == ListViewAlignment.Left, 4, 2);
				break;

			case View.LargeIcon:
				LayoutIcons (LargeIconItemSize, alignment == ListViewAlignment.Left,
					ThemeEngine.Current.ListViewHorizontalSpacing,
					ThemeEngine.Current.ListViewVerticalSpacing);
				break;

			case View.List:
				LayoutIcons (SmallIconItemSize, true, 4, 2);
				break;
#if NET_2_0
			case View.Tile:
				LayoutIcons (TileItemSize, alignment == ListViewAlignment.Left, 
						ThemeEngine.Current.ListViewHorizontalSpacing,
						ThemeEngine.Current.ListViewVerticalSpacing);
				break;
#endif
			}

			CalculateScrollBars ();
		}

		internal Point GetItemLocation (int index)
		{
			Point loc = items_location [index];
			loc.X -= h_marker; // Adjust to scroll
			loc.Y -= v_marker;

			return loc;
		}

		private bool KeySearchString (KeyEventArgs ke)
		{
			int current_tickcnt = Environment.TickCount;
			if (keysearch_tickcnt > 0 && current_tickcnt - keysearch_tickcnt > keysearch_keydelay) {
				keysearch_text = string.Empty;
			}
			
			keysearch_text += (char) ke.KeyData;
			keysearch_tickcnt = current_tickcnt;

			int start = FocusedItem == null ? 0 : FocusedItem.Index;
			int i = start;
			while (true) {
				if (CultureInfo.CurrentCulture.CompareInfo.IsPrefix (Items[i].Text, keysearch_text,
					CompareOptions.IgnoreCase)) {
					SetFocusedItem (Items [i]);
					items [i].Selected = true;
					EnsureVisible (i);
					break;
				}
				i = (i + 1 < Items.Count) ? i+1 : 0;

				if (i == start)
					break;
			}
			return true;
		}

		int GetAdjustedIndex (Keys key)
		{
			int result = -1;

			if (View == View.Details) {
				switch (key) {
				case Keys.Up:
					result = FocusedItem.Index - 1;
					break;
				case Keys.Down:
					result = FocusedItem.Index + 1;
					if (result == items.Count)
						result = -1;
					break;
				case Keys.PageDown:
					int last_index = LastVisibleIndex;
					Rectangle item_rect = new Rectangle (GetItemLocation (last_index), ItemSize);
					if (item_rect.Bottom > item_control.ClientRectangle.Bottom)
						last_index--;
					if (FocusedItem.Index == last_index) {
						if (FocusedItem.Index < Items.Count - 1) {
							int page_size = item_control.Height / ItemSize.Height - 1;
							result = FocusedItem.Index + page_size - 1;
							if (result >= Items.Count)
								result = Items.Count - 1;
						}
					} else
						result = last_index;
					break;
				case Keys.PageUp:
					int first_index = FirstVisibleIndex;
					if (GetItemLocation (first_index).Y < 0)
						first_index++;
					if (FocusedItem.Index == first_index) {
						if (first_index > 0) {
							int page_size = item_control.Height / ItemSize.Height - 1;
							result = first_index - page_size + 1;
							if (result < 0)
								result = 0;
						}
					} else
						result = first_index;
					break;
				}
				return result;
			}

			ItemMatrixLocation item_matrix_location = items_matrix_location [FocusedItem.Index];
			int row = item_matrix_location.Row;
			int col = item_matrix_location.Col;

			switch (key) {
			case Keys.Left:
				if (col == 0)
					return -1;
				return item_index_matrix [row, col - 1];

			case Keys.Right:
				if (col == (cols - 1))
					return -1;
				while (item_index_matrix [row, col + 1] == 0) {
					row--;
					if (row < 0)
						return -1;
				}
				return item_index_matrix [row, col + 1];

			case Keys.Up:
				if (row == 0)
					return -1;
				return item_index_matrix [row - 1, col];

			case Keys.Down:
				if (row == (rows - 1) || row == Items.Count - 1)
					return -1;
				while (item_index_matrix [row + 1, col] == 0) {
					col--;
					if (col < 0)
						return -1;
				}
				return item_index_matrix [row + 1, col];

			default:
				return -1;
			}
		}

		ListViewItem selection_start;

		private bool SelectItems (ArrayList sel_items)
		{
			bool changed = false;
			foreach (ListViewItem item in SelectedItems)
				if (!sel_items.Contains (item)) {
					item.Selected = false;
					changed = true;
				}
			foreach (ListViewItem item in sel_items)
				if (!item.Selected) {
					item.Selected = true;
					changed = true;
				}
			return changed;
		}

		private void UpdateMultiSelection (int index, bool reselect)
		{
			bool shift_pressed = (XplatUI.State.ModifierKeys & Keys.Shift) != 0;
			bool ctrl_pressed = (XplatUI.State.ModifierKeys & Keys.Control) != 0;
			ListViewItem item = items [index];

			if (shift_pressed && selection_start != null) {
				ArrayList list = new ArrayList ();
				int start_index = selection_start.Index;
				int start = Math.Min (start_index, index);
				int end = Math.Max (start_index, index);
				if (View == View.Details) {
					for (int i = start; i <= end; i++)
						list.Add (items [i]);
				} else {
					ItemMatrixLocation start_item_matrix_location = items_matrix_location [start];
					ItemMatrixLocation end_item_matrix_location = items_matrix_location [end];
					int left = Math.Min (start_item_matrix_location.Col, end_item_matrix_location.Col);
					int right = Math.Max (start_item_matrix_location.Col, end_item_matrix_location.Col);
					int top = Math.Min (start_item_matrix_location.Row, end_item_matrix_location.Row);
					int bottom = Math.Max (start_item_matrix_location.Row, end_item_matrix_location.Row);

					for (int i = 0; i < items.Count; i++) {
						ItemMatrixLocation item_matrix_loc = items_matrix_location [i];

						if (item_matrix_loc.Row >= top && item_matrix_loc.Row <= bottom &&
								item_matrix_loc.Col >= left && item_matrix_loc.Col <= right)
							list.Add (items [i]);
					}
				}
				SelectItems (list);
			} else if (ctrl_pressed) {
				item.Selected = !item.Selected;
				selection_start = item;
			} else {
				if (!reselect) {
					// do not unselect, and reselect the item
					foreach (int itemIndex in SelectedIndices) {
						if (index == itemIndex)
							continue;
						items [itemIndex].Selected = false;
					}
				} else {
					SelectedItems.Clear ();
					item.Selected = true;
				}
				selection_start = item;
			}
		}

		internal override bool InternalPreProcessMessage (ref Message msg)
		{
			if (msg.Msg == (int)Msg.WM_KEYDOWN) {
				Keys key_data = (Keys)msg.WParam.ToInt32();
				if (HandleNavKeys (key_data))
					return true;
			} 
			return base.InternalPreProcessMessage (ref msg);
		}

		bool HandleNavKeys (Keys key_data)
		{
			if (Items.Count == 0 || !item_control.Visible)
				return false;

			if (FocusedItem == null)
				SetFocusedItem (Items [0]);

			switch (key_data) {
			case Keys.End:
				SelectIndex (Items.Count - 1);
				break;

			case Keys.Home:
				SelectIndex (0);
				break;

			case Keys.Left:
			case Keys.Right:
			case Keys.Up:
			case Keys.Down:
			case Keys.PageUp:
			case Keys.PageDown:
				SelectIndex (GetAdjustedIndex (key_data));
				break;

			case Keys.Space:
				ToggleItemsCheckState ();
				break;

			default:
				return false;
			}

			return true;
		}

		void ToggleItemsCheckState ()
		{
			if (!CheckBoxes)
				return;

			// Don't modify check state if StateImageList has less than 2 elements
			if (StateImageList != null && StateImageList.Images.Count < 2)
				return;

			if (SelectedIndices.Count > 0) {
				BeginUpdate ();
				for (int i = 0; i < SelectedIndices.Count; i++) {
					ListViewItem item = Items [SelectedIndices [i]];
					item.Checked = !item.Checked;
				}
				EndUpdate ();
				return;
			} 
			
			if (FocusedItem != null) {
				FocusedItem.Checked = !FocusedItem.Checked;
				SelectIndex (FocusedItem.Index);
			}
		}

		void SelectIndex (int index)
		{
			if (index == -1)
				return;

			if (MultiSelect)
				UpdateMultiSelection (index, true);
			else if (!items [index].Selected)
				items [index].Selected = true;

			SetFocusedItem (items [index]);
			EnsureVisible (index);
		}

		private void ListView_KeyDown (object sender, KeyEventArgs ke)
		{
			if (ke.Handled || Items.Count == 0 || !item_control.Visible)
				return;

			ke.Handled = KeySearchString (ke);
		}

		private MouseEventArgs TranslateMouseEventArgs (MouseEventArgs args)
		{
			Point loc = PointToClient (Control.MousePosition);
			return new MouseEventArgs (args.Button, args.Clicks, loc.X, loc.Y, args.Delta);
		}

		internal class ItemControl : Control {

			ListView owner;
			ListViewItem clicked_item;
			ListViewItem last_clicked_item;
			bool hover_processed = false;
			bool checking = false;
			ListViewItem prev_hovered_item;
			int clicks;
			
			ListViewLabelEditTextBox edit_text_box;
			internal ListViewItem edit_item;
			LabelEditEventArgs edit_args;

			public ItemControl (ListView owner)
			{
				this.owner = owner;
				DoubleClick += new EventHandler(ItemsDoubleClick);
				MouseDown += new MouseEventHandler(ItemsMouseDown);
				MouseMove += new MouseEventHandler(ItemsMouseMove);
				MouseHover += new EventHandler(ItemsMouseHover);
				MouseUp += new MouseEventHandler(ItemsMouseUp);
			}

			void ItemsDoubleClick (object sender, EventArgs e)
			{
				if (owner.activation == ItemActivation.Standard)
					owner.OnItemActivate (EventArgs.Empty);
			}

			enum BoxSelect {
				None,
				Normal,
				Shift,
				Control
			}

			BoxSelect box_select_mode = BoxSelect.None;
			ArrayList prev_selection;
			Point box_select_start;

			Rectangle box_select_rect;
			internal Rectangle BoxSelectRectangle {
				get { return box_select_rect; }
				set {
					if (box_select_rect == value)
						return;

					InvalidateBoxSelectRect ();
					box_select_rect = value;
					InvalidateBoxSelectRect ();
				}
			}

			void InvalidateBoxSelectRect ()
			{
				if (BoxSelectRectangle.Size.IsEmpty)
					return;

				Rectangle edge = BoxSelectRectangle;
				edge.X -= 1;
				edge.Y -= 1;
				edge.Width += 2;
				edge.Height = 2;
				Invalidate (edge);
				edge.Y = BoxSelectRectangle.Bottom - 1;
				Invalidate (edge);
				edge.Y = BoxSelectRectangle.Y - 1;
				edge.Width = 2;
				edge.Height = BoxSelectRectangle.Height + 2;
				Invalidate (edge);
				edge.X = BoxSelectRectangle.Right - 1;
				Invalidate (edge);
			}

			private Rectangle CalculateBoxSelectRectangle (Point pt)
			{
				int left = Math.Min (box_select_start.X, pt.X);
				int right = Math.Max (box_select_start.X, pt.X);
				int top = Math.Min (box_select_start.Y, pt.Y);
				int bottom = Math.Max (box_select_start.Y, pt.Y);
				return Rectangle.FromLTRB (left, top, right, bottom);
			}

			bool BoxIntersectsItem (int index)
			{
				Rectangle r = new Rectangle (owner.GetItemLocation (index), owner.ItemSize);
				if (owner.View != View.Details) {
					r.X += r.Width / 4;
					r.Y += r.Height / 4;
					r.Width /= 2;
					r.Height /= 2;
				}
				return BoxSelectRectangle.IntersectsWith (r);
			}

			bool BoxIntersectsText (int index)
			{
				Rectangle r = owner.Items [index].TextBounds;
				return BoxSelectRectangle.IntersectsWith (r);
			}

			ArrayList BoxSelectedItems {
				get {
					ArrayList result = new ArrayList ();
					for (int i = 0; i < owner.Items.Count; i++) {
						bool intersects;
						if (owner.View == View.Details && !owner.FullRowSelect)
							intersects = BoxIntersectsText (i);
						else
							intersects = BoxIntersectsItem (i);

						if (intersects)
							result.Add (owner.Items [i]);
					}
					return result;
				}
			}

			private bool PerformBoxSelection (Point pt)
			{
				if (box_select_mode == BoxSelect.None)
					return false;

				BoxSelectRectangle = CalculateBoxSelectRectangle (pt);
				
				ArrayList box_items = BoxSelectedItems;

				ArrayList items;

				switch (box_select_mode) {

				case BoxSelect.Normal:
					items = box_items;
					break;

				case BoxSelect.Control:
					items = new ArrayList ();
					foreach (int index in prev_selection)
						if (!box_items.Contains (owner.Items [index]))
							items.Add (owner.Items [index]);
					foreach (ListViewItem item in box_items)
						if (!prev_selection.Contains (item.Index))
							items.Add (item);
					break;

				case BoxSelect.Shift:
					items = box_items;
					foreach (ListViewItem item in box_items)
						prev_selection.Remove (item.Index);
					foreach (int index in prev_selection)
						items.Add (owner.Items [index]);
					break;

				default:
					throw new Exception ("Unexpected Selection mode: " + box_select_mode);
				}

				SuspendLayout ();
				owner.SelectItems (items);
				ResumeLayout ();

				return true;
			}

			private void ItemsMouseDown (object sender, MouseEventArgs me)
			{
				owner.OnMouseDown (owner.TranslateMouseEventArgs (me));
				if (owner.items.Count == 0)
					return;

				bool box_selecting = false;
				Size item_size = owner.ItemSize;
				Point pt = new Point (me.X, me.Y);
				for (int i = 0; i < owner.items.Count; i++) {
					Rectangle item_rect = new Rectangle (owner.GetItemLocation (i), item_size);
					if (!item_rect.Contains (pt))
						continue;

					if (owner.items [i].CheckRectReal.Contains (pt)) {
						// Don't check if StateImageList has less than two items
						if (owner.StateImageList != null && owner.StateImageList.Images.Count < 2)
							return;

						checking = true;
						ListViewItem item = owner.items [i];
						item.Checked = !item.Checked;
						return;
					}
					
					if (owner.View == View.Details) {
						bool over_text = owner.items [i].TextBounds.Contains (pt);
						if (owner.FullRowSelect) {
							clicked_item = owner.items [i];
							bool over_item_column = (me.X > owner.Columns[0].X && me.X < owner.Columns[0].X + owner.Columns[0].Width);
							if (!over_text && over_item_column && owner.MultiSelect)
								box_selecting = true;
						} else if (over_text)
							clicked_item = owner.items [i];
						else
							owner.SetFocusedItem (owner.Items [i]);
					} else
						clicked_item = owner.items [i];

					break;
				}


				if (clicked_item != null) {
					bool changed = !clicked_item.Selected;
					if (me.Button == MouseButtons.Left || (XplatUI.State.ModifierKeys == Keys.None && changed))
						owner.SetFocusedItem (clicked_item);

					if (owner.MultiSelect) {
						bool reselect = (!owner.LabelEdit || changed);
						if (me.Button == MouseButtons.Left || (XplatUI.State.ModifierKeys == Keys.None && changed))
							owner.UpdateMultiSelection (clicked_item.Index, reselect);
					} else {
						clicked_item.Selected = true;
					}

					// Report clicks only if the item was clicked. On MS the
					// clicks are only raised if you click an item
					clicks = me.Clicks;
					if (me.Clicks > 1) {
						if (owner.CheckBoxes)
							clicked_item.Checked = !clicked_item.Checked;
					} else if (me.Clicks == 1) {
						if (owner.LabelEdit && !changed)
							BeginEdit (clicked_item); // this is probably not the correct place to execute BeginEdit
					}
				} else {
					if (owner.MultiSelect)
						box_selecting = true;
					else if (owner.SelectedItems.Count > 0)
						owner.SelectedItems.Clear ();
				}

				if (box_selecting) {
					Keys mods = XplatUI.State.ModifierKeys;
					if ((mods & Keys.Shift) != 0)
						box_select_mode = BoxSelect.Shift;
					else if ((mods & Keys.Control) != 0)
						box_select_mode = BoxSelect.Control;
					else
						box_select_mode = BoxSelect.Normal;
					box_select_start = pt; 
					prev_selection = owner.SelectedIndices.List;
				}
			}

			private void ItemsMouseMove (object sender, MouseEventArgs me)
			{
				bool done = PerformBoxSelection (new Point (me.X, me.Y));

				if (!done && hover_processed) {

					Point pt = PointToClient (Control.MousePosition);
					ListViewItem item = owner.GetItemAt (pt.X, pt.Y);
					if (item != null && item != prev_hovered_item) {
						hover_processed = false;
						XplatUI.ResetMouseHover (Handle);
					}
				}

				owner.OnMouseMove (owner.TranslateMouseEventArgs (me));
			}


			private void ItemsMouseHover (object sender, EventArgs e)
			{
				if (owner.hover_pending) {
					owner.OnMouseHover (e);
					owner.hover_pending = false;
				}

				if (Capture)
					return;

				hover_processed = true;
				Point pt = PointToClient (Control.MousePosition);
				ListViewItem item = owner.GetItemAt (pt.X, pt.Y);
				if (item == null)
					return;

				prev_hovered_item = item;

				if (owner.HoverSelection) {
					if (owner.MultiSelect)
						owner.UpdateMultiSelection (item.Index, true);
					else
						item.Selected = true;
					
					owner.SetFocusedItem (item);
					Select (); // Make sure we have the focus, since MouseHover doesn't give it to us
				}

#if NET_2_0
				owner.OnItemMouseHover (new ListViewItemMouseHoverEventArgs (item));
#endif
			}

			void HandleClicks (MouseEventArgs me)
			{
				// if the click is not on an item,
				// clicks remains as 0
				if (clicks > 1) {
#if !NET_2_0
					owner.OnDoubleClick (EventArgs.Empty);
				} else if (clicks == 1) {
					owner.OnClick (EventArgs.Empty);
#else
					owner.OnDoubleClick (EventArgs.Empty);
					owner.OnMouseDoubleClick (me);
				} else if (clicks == 1) {
					owner.OnClick (EventArgs.Empty);
					owner.OnMouseClick (me);
#endif
				}

				clicks = 0;
			}

			private void ItemsMouseUp (object sender, MouseEventArgs me)
			{
				MouseEventArgs owner_me = owner.TranslateMouseEventArgs (me);
				HandleClicks (owner_me);

				Capture = false;
				if (owner.Items.Count == 0) {
					owner.OnMouseUp (owner_me);
					return;
				}

				Point pt = new Point (me.X, me.Y);

				Rectangle rect = Rectangle.Empty;
				if (clicked_item != null) {
					if (owner.view == View.Details && !owner.full_row_select)
						rect = clicked_item.GetBounds (ItemBoundsPortion.Label);
					else
						rect = clicked_item.Bounds;

					if (rect.Contains (pt)) {
						switch (owner.activation) {
						case ItemActivation.OneClick:
							owner.OnItemActivate (EventArgs.Empty);
							break;

						case ItemActivation.TwoClick:
							if (last_clicked_item == clicked_item) {
								owner.OnItemActivate (EventArgs.Empty);
								last_clicked_item = null;
							} else
								last_clicked_item = clicked_item;
							break;
						default:
							// DoubleClick activation is handled in another handler
							break;
						}
					}
				} else if (!checking && owner.SelectedItems.Count > 0 && BoxSelectRectangle.Size.IsEmpty) {
					// Need this to clean up background clicks
					owner.SelectedItems.Clear ();
				}

				clicked_item = null;
				box_select_start = Point.Empty;
				BoxSelectRectangle = Rectangle.Empty;
				prev_selection = null;
				box_select_mode = BoxSelect.None;
				checking = false;
				owner.OnMouseUp (owner_me);
			}
			
			private void LabelEditFinished (object sender, EventArgs e)
			{
				EndEdit (edit_item);
			}

			private void LabelEditCancelled (object sender, EventArgs e)
			{
				edit_args.SetLabel (null);
				EndEdit (edit_item);
			}

			private void LabelTextChanged (object sender, EventArgs e)
			{
				if (edit_args != null)
					edit_args.SetLabel (edit_text_box.Text);
			}

			internal void BeginEdit (ListViewItem item)
			{
				if (edit_item != null)
					EndEdit (edit_item);
				
				if (edit_text_box == null) {
					edit_text_box = new ListViewLabelEditTextBox ();
					edit_text_box.BorderStyle = BorderStyle.FixedSingle;
					edit_text_box.EditingCancelled += new EventHandler (LabelEditCancelled);
					edit_text_box.EditingFinished += new EventHandler (LabelEditFinished);
					edit_text_box.TextChanged += new EventHandler (LabelTextChanged);
					edit_text_box.Visible = false;
					Controls.Add (edit_text_box);
				}
				
				item.EnsureVisible();
				
				edit_text_box.Reset ();
				
				switch (owner.view) {
					case View.List:
					case View.SmallIcon:
					case View.Details:
						edit_text_box.TextAlign = HorizontalAlignment.Left;
						edit_text_box.Bounds = item.GetBounds (ItemBoundsPortion.Label);
						SizeF sizef = DeviceContext.MeasureString (item.Text, item.Font);
						edit_text_box.Width = (int)sizef.Width + 4;
						edit_text_box.MaxWidth = owner.ClientRectangle.Width - edit_text_box.Bounds.X;
						edit_text_box.WordWrap = false;
						edit_text_box.Multiline = false;
						break;
					case View.LargeIcon:
						edit_text_box.TextAlign = HorizontalAlignment.Center;
						edit_text_box.Bounds = item.GetBounds (ItemBoundsPortion.Label);
						sizef = DeviceContext.MeasureString (item.Text, item.Font);
						edit_text_box.Width = (int)sizef.Width + 4;
						edit_text_box.MaxWidth = item.GetBounds(ItemBoundsPortion.Entire).Width;
						edit_text_box.MaxHeight = owner.ClientRectangle.Height - edit_text_box.Bounds.Y;
						edit_text_box.WordWrap = true;
						edit_text_box.Multiline = true;
						break;
				}

				edit_item = item;

				edit_text_box.Text = item.Text;
				edit_text_box.Font = item.Font;
				edit_text_box.Visible = true;
				edit_text_box.Focus ();
				edit_text_box.SelectAll ();

				edit_args = new LabelEditEventArgs (owner.Items.IndexOf (edit_item));
				owner.OnBeforeLabelEdit (edit_args);

				if (edit_args.CancelEdit)
					EndEdit (item);
			}

			internal void CancelEdit (ListViewItem item)
			{
				// do nothing if there's no item being edited, or if the
				// item being edited is not the one passed in
				if (edit_item == null || edit_item != item)
					return;

				edit_args.SetLabel (null);
				EndEdit (item);
			}

			internal void EndEdit (ListViewItem item)
			{
				// do nothing if there's no item being edited, or if the
				// item being edited is not the one passed in
				if (edit_item == null || edit_item != item)
					return;

				owner.OnAfterLabelEdit (edit_args);
				if (!edit_args.CancelEdit && edit_args.Label != null)
					edit_item.Text = edit_text_box.Text;

				if (edit_text_box != null) {
					if (edit_text_box.Visible)
						edit_text_box.Visible = false;
					// ensure listview gets focus
					owner.Focus ();
				}

				edit_item = null;
			}

			internal override void OnPaintInternal (PaintEventArgs pe)
			{
				ThemeEngine.Current.DrawListViewItems (pe.Graphics, pe.ClipRectangle, owner);
			}

			protected override void WndProc (ref Message m)
			{
				switch ((Msg)m.Msg) {
				case Msg.WM_KILLFOCUS:
					owner.Select (false, true);
					break;
				case Msg.WM_SETFOCUS:
					owner.Select (false, true);
					break;
				case Msg.WM_RBUTTONDOWN:
					owner.Select (false, true);
					break;
				default:
					break;
				}
				base.WndProc (ref m);
			}
		}
		
		internal class ListViewLabelEditTextBox : TextBox
		{
			int max_width = -1;
			int min_width = -1;
			
			int max_height = -1;
			int min_height = -1;
			
			int old_number_lines = 1;
			
			SizeF text_size_one_char;
			
			public ListViewLabelEditTextBox ()
			{
				min_height = DefaultSize.Height;
				text_size_one_char = DeviceContext.MeasureString ("B", Font);
			}
			
			public int MaxWidth {
				set {
					if (value < min_width)
						max_width = min_width;
					else
						max_width = value;
				}
			}
			
			public int MaxHeight {
				set {
					if (value < min_height)
						max_height = min_height;
					else
						max_height = value;
				}
			}
			
			public new int Width {
				get {
					return base.Width;
				}
				set {
					min_width = value;
					base.Width = value;
				}
			}
			
			public override Font Font {
				get {
					return base.Font;
				}
				set {
					base.Font = value;
					text_size_one_char = DeviceContext.MeasureString ("B", Font);
				}
			}
			
			protected override void OnTextChanged (EventArgs e)
			{
				SizeF text_size = DeviceContext.MeasureString (Text, Font);
				
				int new_width = (int)text_size.Width + 8;
				
				if (!Multiline)
					ResizeTextBoxWidth (new_width);
				else {
					if (Width != max_width)
						ResizeTextBoxWidth (new_width);
					
					int number_lines = Lines.Length;
					
					if (number_lines != old_number_lines) {
						int new_height = number_lines * (int)text_size_one_char.Height + 4;
						old_number_lines = number_lines;
						
						ResizeTextBoxHeight (new_height);
					}
				}
				
				base.OnTextChanged (e);
			}
			
			protected override bool IsInputKey (Keys key_data)
			{
				if ((key_data & Keys.Alt) == 0) {
					switch (key_data & Keys.KeyCode) {
						case Keys.Enter:
							return true;
						case Keys.Escape:
							return true;
					}
				}
				return base.IsInputKey (key_data);
			}
			
			protected override void OnKeyDown (KeyEventArgs e)
			{
				if (!Visible)
					return;

				switch (e.KeyCode) {
				case Keys.Return:
					Visible = false;
					e.Handled = true;
					OnEditingFinished (e);
					break;
				case Keys.Escape:
					Visible = false;
					e.Handled = true;
					OnEditingCancelled (e);
					break;
				}
			}
			
			protected override void OnLostFocus (EventArgs e)
			{
				if (Visible) {
					OnEditingFinished (e);
				}
			}

			protected void OnEditingCancelled (EventArgs e)
			{
				EventHandler eh = (EventHandler)(Events [EditingCancelledEvent]);
				if (eh != null)
					eh (this, e);
			}
			
			protected void OnEditingFinished (EventArgs e)
			{
				EventHandler eh = (EventHandler)(Events [EditingFinishedEvent]);
				if (eh != null)
					eh (this, e);
			}
			
			private void ResizeTextBoxWidth (int new_width)
			{
				if (new_width > max_width)
					base.Width = max_width;
				else 
				if (new_width >= min_width)
					base.Width = new_width;
				else
					base.Width = min_width;
			}
			
			private void ResizeTextBoxHeight (int new_height)
			{
				if (new_height > max_height)
					base.Height = max_height;
				else 
				if (new_height >= min_height)
					base.Height = new_height;
				else
					base.Height = min_height;
			}
			
			public void Reset ()
			{
				max_width = -1;
				min_width = -1;
				
				max_height = -1;
				
				old_number_lines = 1;
				
				Text = String.Empty;
				
				Size = DefaultSize;
			}

			static object EditingCancelledEvent = new object ();
			public event EventHandler EditingCancelled {
				add { Events.AddHandler (EditingCancelledEvent, value); }
				remove { Events.RemoveHandler (EditingCancelledEvent, value); }
			}

			static object EditingFinishedEvent = new object ();
			public event EventHandler EditingFinished {
				add { Events.AddHandler (EditingFinishedEvent, value); }
				remove { Events.RemoveHandler (EditingFinishedEvent, value); }
			}
		}

		internal override void OnPaintInternal (PaintEventArgs pe)
		{
			if (updating)
				return;
				
			CalculateScrollBars ();
		}

		void FocusChanged (object o, EventArgs args)
		{
			if (Items.Count == 0)
				return;

			if (FocusedItem == null)
				SetFocusedItem (Items [0]);

			item_control.Invalidate (FocusedItem.Bounds);
		}

		private void ListView_MouseEnter (object sender, EventArgs args)
		{
			hover_pending = true; // Need a hover event for every Enter/Leave cycle
		}

		private void ListView_MouseWheel (object sender, MouseEventArgs me)
		{
			if (Items.Count == 0)
				return;

			int lines = me.Delta / 120;

			if (lines == 0)
				return;

			switch (View) {
			case View.Details:
			case View.SmallIcon:
				Scroll (v_scroll, -ItemSize.Height * SystemInformation.MouseWheelScrollLines * lines);
				break;
			case View.LargeIcon:
				Scroll (v_scroll, -(ItemSize.Height + ThemeEngine.Current.ListViewVerticalSpacing)  * lines);
				break;
			case View.List:
				Scroll (h_scroll, -ItemSize.Width * lines);
				break;
#if NET_2_0
			case View.Tile:
				Scroll (v_scroll, -(ItemSize.Height + ThemeEngine.Current.ListViewVerticalSpacing) * 2 * lines);
				break;
#endif
			}
		}

		private void ListView_SizeChanged (object sender, EventArgs e)
		{
			CalculateListView (alignment);
		}
		
		private void SetFocusedItem (ListViewItem item)
		{
			if (focused_item != null)
				focused_item.Focused = false;
			
			if (item != null)
				item.Focused = true;
				
			focused_item = item;
		}

		private void HorizontalScroller (object sender, EventArgs e)
		{
			item_control.EndEdit (item_control.edit_item);
			
			// Avoid unnecessary flickering, when button is
			// kept pressed at the end
			if (h_marker != h_scroll.Value) {
				
				int pixels = h_marker - h_scroll.Value;
				
				h_marker = h_scroll.Value;
				if (header_control.Visible)
					XplatUI.ScrollWindow (header_control.Handle, pixels, 0, false);

				XplatUI.ScrollWindow (item_control.Handle, pixels, 0, false);
			}
		}

		private void VerticalScroller (object sender, EventArgs e)
		{
			item_control.EndEdit (item_control.edit_item);
			
			// Avoid unnecessary flickering, when button is
			// kept pressed at the end
			if (v_marker != v_scroll.Value) {
				int pixels = v_marker - v_scroll.Value;
				Rectangle area = item_control.ClientRectangle;
				v_marker = v_scroll.Value;
				XplatUI.ScrollWindow (item_control.Handle, area, 0, pixels, false);
			}
		}
		#endregion	// Internal Methods Properties

		#region Protected Methods
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
			for (int i = 0; i < SelectedItems.Count; i++)
				OnSelectedIndexChanged (EventArgs.Empty);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				h_scroll.Dispose ();
				v_scroll.Dispose ();
				
				large_image_list = null;
				small_image_list = null;
				state_image_list = null;

				foreach (ColumnHeader col in columns)
					col.SetListView (null);

#if NET_2_0
				if (!virtual_mode) // In virtual mode we don't save the items
#endif
					foreach (ListViewItem item in items)
						item.Owner = null;
			}
			
			base.Dispose (disposing);
		}

		protected override bool IsInputKey (Keys keyData)
		{
			switch (keyData) {
			case Keys.Up:
			case Keys.Down:
			case Keys.PageUp:
			case Keys.PageDown:
			case Keys.Right:
			case Keys.Left:
			case Keys.End:
			case Keys.Home:
				return true;

			default:
				break;
			}
			
			return base.IsInputKey (keyData);
		}

		protected virtual void OnAfterLabelEdit (LabelEditEventArgs e)
		{
			LabelEditEventHandler eh = (LabelEditEventHandler)(Events [AfterLabelEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBeforeLabelEdit (LabelEditEventArgs e)
		{
			LabelEditEventHandler eh = (LabelEditEventHandler)(Events [BeforeLabelEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnClick (ColumnClickEventArgs e)
		{
			ColumnClickEventHandler eh = (ColumnClickEventHandler)(Events [ColumnClickEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		protected internal virtual void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
		{
			DrawListViewColumnHeaderEventHandler eh = (DrawListViewColumnHeaderEventHandler)(Events[DrawColumnHeaderEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected internal virtual void OnDrawItem(DrawListViewItemEventArgs e)
		{
			DrawListViewItemEventHandler eh = (DrawListViewItemEventHandler)(Events[DrawItemEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected internal virtual void OnDrawSubItem(DrawListViewSubItemEventArgs e)
		{
			DrawListViewSubItemEventHandler eh = (DrawListViewSubItemEventHandler)(Events[DrawSubItemEvent]);
			if (eh != null)
				eh(this, e);
		}
#endif

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			Redraw (true);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			CalculateListView (alignment);
#if NET_2_0
			if (!virtual_mode) // Sorting is not allowed in virtual mode
#endif
				Sort ();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnItemActivate (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ItemActivateEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnItemCheck (ItemCheckEventArgs ice)
		{
			ItemCheckEventHandler eh = (ItemCheckEventHandler)(Events [ItemCheckEvent]);
			if (eh != null)
				eh (this, ice);
		}

#if NET_2_0
		protected internal virtual void OnItemChecked (ItemCheckedEventArgs icea)
		{
			ItemCheckedEventHandler eh = (ItemCheckedEventHandler)(Events [ItemCheckedEvent]);
			if (eh != null)
				eh (this, icea);
		}
#endif

		protected virtual void OnItemDrag (ItemDragEventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ItemDragEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		protected virtual void OnItemMouseHover (ListViewItemMouseHoverEventArgs args)
		{
			ListViewItemMouseHoverEventHandler eh = (ListViewItemMouseHoverEventHandler)(Events [ItemMouseHoverEvent]);
			if (eh != null)
				eh (this, args);
		}
#endif

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SelectedIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnSystemColorsChanged (EventArgs e)
		{
			base.OnSystemColorsChanged (e);
		}

#if NET_2_0
		protected virtual void OnCacheVirtualItems (CacheVirtualItemsEventArgs args)
		{
			EventHandler eh = (EventHandler)Events [CacheVirtualItemsEvent];
			if (eh != null)
				eh (this, args);
		}

		protected virtual void OnRetrieveVirtualItem (RetrieveVirtualItemEventArgs args)
		{
			RetrieveVirtualItemEventHandler eh = (RetrieveVirtualItemEventHandler)Events [RetrieveVirtualItemEvent];
			if (eh != null)
				eh (this, args);
		}
#endif

		protected void RealizeProperties ()
		{
			// FIXME: TODO
		}

		protected void UpdateExtendedStyles ()
		{
			// FIXME: TODO
		}

		bool refocusing = false;

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
			case Msg.WM_KILLFOCUS:
				Control receiver = Control.FromHandle (m.WParam);
				if (receiver == item_control) {
					has_focus = false;
					refocusing = true;
					return;
				}
				break;
			case Msg.WM_SETFOCUS:
				if (refocusing) {
					has_focus = true;
					refocusing = false;
					return;
				}
				break;
			default:
				break;
			}
			base.WndProc (ref m);
		}
		#endregion // Protected Methods

		#region Public Instance Methods
		public void ArrangeIcons ()
		{
			ArrangeIcons (this.alignment);
		}

		public void ArrangeIcons (ListViewAlignment alignment)
		{
			// Icons are arranged only if view is set to LargeIcon or SmallIcon
			if (view == View.LargeIcon || view == View.SmallIcon) {
				this.CalculateListView (alignment);
				// we have done the calculations already
				this.Redraw (false);
			}
		}

#if NET_2_0
		public void AutoResizeColumn (int columnIndex, ColumnHeaderAutoResizeStyle headerAutoResize)
		{
			if (columnIndex < 0 || columnIndex >= columns.Count)
				throw new ArgumentOutOfRangeException ("columnIndex");

			columns [columnIndex].AutoResize (headerAutoResize);
		}

		public void AutoResizeColumns (ColumnHeaderAutoResizeStyle headerAutoResize)
		{
			BeginUpdate ();
			foreach (ColumnHeader col in columns) 
				col.AutoResize (headerAutoResize);
			EndUpdate ();
		}
#endif

		public void BeginUpdate ()
		{
			// flag to avoid painting
			updating = true;
		}

		public void Clear ()
		{
			columns.Clear ();
			items.Clear ();	// Redraw (true) called here
		}

		public void EndUpdate ()
		{
			// flag to avoid painting
			updating = false;

			// probably, now we need a redraw with recalculations
			this.Redraw (true);
		}

		public void EnsureVisible (int index)
		{
			if (index < 0 || index >= items.Count || scrollable == false)
				return;

			Rectangle view_rect = item_control.ClientRectangle;
			Rectangle bounds = new Rectangle (GetItemLocation (index), ItemSize);

			if (view_rect.Contains (bounds))
				return;

			if (View != View.Details) {
				if (bounds.Left < 0)
					h_scroll.Value += bounds.Left;
				else if (bounds.Right > view_rect.Right)
					h_scroll.Value += (bounds.Right - view_rect.Right);
			}

			if (bounds.Top < 0)
				v_scroll.Value += bounds.Top;
			else if (bounds.Bottom > view_rect.Bottom)
				v_scroll.Value += (bounds.Bottom - view_rect.Bottom);
		}

#if NET_2_0
		public ListViewItem FindItemWithText (string text)
		{
			if (items.Count == 0)
				return null;

			return FindItemWithText (text, true, 0, true);
		}

		public ListViewItem FindItemWithText (string text, bool includeSubItems, int startIndex)
		{
			return FindItemWithText (text, includeSubItems, startIndex, true);
		}

		public ListViewItem FindItemWithText (string text, bool includeSubItems, int startIndex, bool prefixSearch)
		{
			if (startIndex < 0 || startIndex >= items.Count)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (text == null)
				throw new ArgumentNullException ("text");

			for (int i = startIndex; i < items.Count; i++) {
				ListViewItem lvi = items [i];

				if ((prefixSearch && lvi.Text.StartsWith (text, true, CultureInfo.CurrentCulture)) // prefix search
						|| String.Compare (lvi.Text, text, true) == 0) // match
					return lvi;
			}

			if (includeSubItems) {
				for (int i = startIndex; i < items.Count; i++) {
					ListViewItem lvi = items [i];
					foreach (ListViewItem.ListViewSubItem sub_item in lvi.SubItems)
						if ((prefixSearch && sub_item.Text.StartsWith (text, true, CultureInfo.CurrentCulture))
								|| String.Compare (sub_item.Text, text, true) == 0)
							return lvi;
				}
			}

			return null;
		}
#endif
		
		public ListViewItem GetItemAt (int x, int y)
		{
			Size item_size = ItemSize;
			for (int i = 0; i < items.Count; i++) {
				Point item_location = GetItemLocation (i);
				Rectangle item_rect = new Rectangle (item_location, item_size);
				if (item_rect.Contains (x, y))
					return items [i];
			}

			return null;
		}

		public Rectangle GetItemRect (int index)
		{
			return GetItemRect (index, ItemBoundsPortion.Entire);
		}

		public Rectangle GetItemRect (int index, ItemBoundsPortion portion)
		{
			if (index < 0 || index >= items.Count)
				throw new IndexOutOfRangeException ("index");

			return items [index].GetBounds (portion);
		}

		public void Sort ()
		{
#if NET_2_0
			if (virtual_mode)
				throw new InvalidOperationException ();
#endif

			Sort (true);
		}

		// we need this overload to reuse the logic for sorting, while allowing
		// redrawing to be done by caller or have it done by this method when
		// sorting is really performed
		//
		// ListViewItemCollection's Add and AddRange methods call this overload
		// with redraw set to false, as they take care of redrawing themselves
		// (they even want to redraw the listview if no sort is performed, as 
		// an item was added), while ListView.Sort () only wants to redraw if 
		// sorting was actually performed
		private void Sort (bool redraw)
		{
			if (!IsHandleCreated || item_sorter == null) {
				return;
			}
			
			items.Sort (item_sorter);
			if (redraw)
				this.Redraw (true);
		}

		public override string ToString ()
		{
			int count = this.Items.Count;

			if (count == 0)
				return string.Format ("System.Windows.Forms.ListView, Items.Count: 0");
			else
				return string.Format ("System.Windows.Forms.ListView, Items.Count: {0}, Items[0]: {1}", count, this.Items [0].ToString ());
		}
		#endregion	// Public Instance Methods


		#region Subclasses

		class HeaderControl : Control {

			ListView owner;
			bool column_resize_active = false;
			ColumnHeader resize_column;
			ColumnHeader clicked_column;
			ColumnHeader drag_column;
			int drag_x;
			int drag_to_index = -1;

			public HeaderControl (ListView owner)
			{
				this.owner = owner;
				MouseDown += new MouseEventHandler (HeaderMouseDown);
				MouseMove += new MouseEventHandler (HeaderMouseMove);
				MouseUp += new MouseEventHandler (HeaderMouseUp);
			}

			private ColumnHeader ColumnAtX (int x)
			{
				Point pt = new Point (x, 0);
				ColumnHeader result = null;
				foreach (ColumnHeader col in owner.Columns) {
					if (col.Rect.Contains (pt)) {
						result = col;
						break;
					}
				}
				return result;
			}

			private int GetReorderedIndex (ColumnHeader col)
			{
				if (owner.reordered_column_indices == null)
					return col.Index;
				else
					for (int i = 0; i < owner.Columns.Count; i++)
						if (owner.reordered_column_indices [i] == col.Index)
							return i;
				throw new Exception ("Column index missing from reordered array");
			}

			private void HeaderMouseDown (object sender, MouseEventArgs me)
			{
				if (resize_column != null) {
					column_resize_active = true;
					Capture = true;
					return;
				}

				clicked_column = ColumnAtX (me.X + owner.h_marker);

				if (clicked_column != null) {
					Capture = true;
					if (owner.AllowColumnReorder) {
						drag_x = me.X;
						drag_column = (ColumnHeader) (clicked_column as ICloneable).Clone ();
						drag_column.Rect = clicked_column.Rect;
						drag_to_index = GetReorderedIndex (clicked_column);
					}
					clicked_column.Pressed = true;
					Rectangle bounds = clicked_column.Rect;
					bounds.X -= owner.h_marker;
					Invalidate (bounds);
					return;
				}
			}

			void StopResize ()
			{
				column_resize_active = false;
				resize_column = null;
				Capture = false;
				Cursor = Cursors.Default;
			}
			
			private void HeaderMouseMove (object sender, MouseEventArgs me)
			{
				Point pt = new Point (me.X + owner.h_marker, me.Y);

				if (column_resize_active) {
					int width = pt.X - resize_column.X;
					if (width < 0)
						width = 0;

					if (!owner.CanProceedWithResize (resize_column, width)){
						StopResize ();
						return;
					}
					resize_column.Width = width;
					return;
				}

				resize_column = null;

				if (clicked_column != null) {
					if (owner.AllowColumnReorder) {
						Rectangle r;

						r = drag_column.Rect;
						r.X = clicked_column.Rect.X + me.X - drag_x;
						drag_column.Rect = r;

						int x = me.X + owner.h_marker;
						ColumnHeader over = ColumnAtX (x);
						if (over == null)
							drag_to_index = owner.Columns.Count;
						else if (x < over.X + over.Width / 2)
							drag_to_index = GetReorderedIndex (over);
						else
							drag_to_index = GetReorderedIndex (over) + 1;
						Invalidate ();
					} else {
						ColumnHeader over = ColumnAtX (me.X + owner.h_marker);
						bool pressed = clicked_column.Pressed;
						clicked_column.Pressed = over == clicked_column;
						if (clicked_column.Pressed ^ pressed) {
							Rectangle bounds = clicked_column.Rect;
							bounds.X -= owner.h_marker;
							Invalidate (bounds);
						}
					}
					return;
				}

				for (int i = 0; i < owner.Columns.Count; i++) {
					Rectangle zone = owner.Columns [i].Rect;
					zone.X = zone.Right - 5;
					zone.Width = 10;
					if (zone.Contains (pt)) {
						if (i < owner.Columns.Count - 1 && owner.Columns [i + 1].Width == 0)
							i++;
						resize_column = owner.Columns [i];
						break;
					}
				}

				if (resize_column == null)
					Cursor = Cursors.Default;
				else
					Cursor = Cursors.VSplit;
			}

			void HeaderMouseUp (object sender, MouseEventArgs me)
			{
				Capture = false;

				if (column_resize_active) {
					int column_idx = resize_column.Index;
					StopResize ();
					owner.RaiseColumnWidthChanged (column_idx);
					return;
				}

				if (clicked_column != null && clicked_column.Pressed) {
					clicked_column.Pressed = false;
					Rectangle bounds = clicked_column.Rect;
					bounds.X -= owner.h_marker;
					Invalidate (bounds);
					owner.OnColumnClick (new ColumnClickEventArgs (clicked_column.Index));
				}

				if (drag_column != null && owner.AllowColumnReorder) {
					drag_column = null;
					if (drag_to_index > GetReorderedIndex (clicked_column))
						drag_to_index--;
					if (owner.GetReorderedColumn (drag_to_index) != clicked_column)
						owner.ReorderColumn (clicked_column, drag_to_index, true);
					drag_to_index = -1;
					Invalidate ();
				}

				clicked_column = null;
			}

			internal override void OnPaintInternal (PaintEventArgs pe)
			{
				if (owner.updating)
					return;
				
				Theme theme = ThemeEngine.Current;
				theme.DrawListViewHeader (pe.Graphics, pe.ClipRectangle, this.owner);

				if (drag_column == null)
					return;

				int target_x;
				if (drag_to_index == owner.Columns.Count)
					target_x = owner.GetReorderedColumn (drag_to_index - 1).Rect.Right - owner.h_marker;
				else
					target_x = owner.GetReorderedColumn (drag_to_index).Rect.X - owner.h_marker;
				theme.DrawListViewHeaderDragDetails (pe.Graphics, owner, drag_column, target_x);
			}

			protected override void WndProc (ref Message m)
			{
				switch ((Msg)m.Msg) {
				case Msg.WM_SETFOCUS:
					owner.Focus ();
					break;
				default:
					base.WndProc (ref m);
					break;
				}
			}
		}

		private class ItemComparer : IComparer {
			readonly SortOrder sort_order;

			public ItemComparer (SortOrder sortOrder)
			{
				sort_order = sortOrder;
			}

			public int Compare (object x, object y)
			{
				ListViewItem item_x = x as ListViewItem;
				ListViewItem item_y = y as ListViewItem;
				if (sort_order == SortOrder.Ascending)
					return String.Compare (item_x.Text, item_y.Text);
				else
					return String.Compare (item_y.Text, item_x.Text);
			}
		}

		public class CheckedIndexCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;

			#region Public Constructor
			public CheckedIndexCollection (ListView owner)
			{
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return owner.CheckedItems.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					int [] indices = GetIndices ();
					if (index < 0 || index >= indices.Length)
						throw new ArgumentOutOfRangeException ("index");
					return indices [index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public bool Contains (int checkedIndex)
			{
				int [] indices = GetIndices ();
				for (int i = 0; i < indices.Length; i++) {
					if (indices [i] == checkedIndex)
						return true;
				}
				return false;
			}

			public IEnumerator GetEnumerator ()
			{
				int [] indices = GetIndices ();
				return indices.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				int [] indices = GetIndices ();
				Array.Copy (indices, 0, dest, index, indices.Length);
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ("Clear operation is not supported.");
			}

			bool IList.Contains (object checkedIndex)
			{
				if (!(checkedIndex is int))
					return false;
				return Contains ((int) checkedIndex);
			}

			int IList.IndexOf (object checkedIndex)
			{
				if (!(checkedIndex is int))
					return -1;
				return IndexOf ((int) checkedIndex);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (int checkedIndex)
			{
				int [] indices = GetIndices ();
				for (int i = 0; i < indices.Length; i++) {
					if (indices [i] == checkedIndex)
						return i;
				}
				return -1;
			}
			#endregion	// Public Methods

			private int [] GetIndices ()
			{
				ArrayList checked_items = owner.CheckedItems.List;
				int [] indices = new int [checked_items.Count];
				for (int i = 0; i < checked_items.Count; i++) {
					ListViewItem item = (ListViewItem) checked_items [i];
					indices [i] = item.Index;
				}
				return indices;
			}
		}	// CheckedIndexCollection

		public class CheckedListViewItemCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;
			private ArrayList list;

			#region Public Constructor
			public CheckedListViewItemCollection (ListView owner)
			{
				this.owner = owner;
				this.owner.Items.Changed += new CollectionChangedHandler (
					ItemsCollection_Changed);
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					if (!owner.CheckBoxes)
						return 0;
					return List.Count;
				}
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					ArrayList checked_items = List;
					if (index < 0 || index >= checked_items.Count)
						throw new ArgumentOutOfRangeException ("index");
					return (ListViewItem) checked_items [index];
				}
			}

#if NET_2_0
			public virtual ListViewItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					return idx == -1 ? null : (ListViewItem) List [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public bool Contains (ListViewItem item)
			{
				if (!owner.CheckBoxes)
					return false;
				return List.Contains (item);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public void CopyTo (Array dest, int index)
			{
				if (!owner.CheckBoxes)
					return;
				List.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
			{
				if (!owner.CheckBoxes)
					return (new ListViewItem [0]).GetEnumerator ();
				return List.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ("Clear operation is not supported.");
			}

			bool IList.Contains (object item)
			{
				if (!(item is ListViewItem))
					return false;
				return Contains ((ListViewItem) item);
			}

			int IList.IndexOf (object item)
			{
				if (!(item is ListViewItem))
					return -1;
				return IndexOf ((ListViewItem) item);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (ListViewItem item)
			{
				if (!owner.CheckBoxes)
					return -1;
				return List.IndexOf (item);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key.Length == 0)
					return -1;

				ArrayList checked_items = List;
				for (int i = 0; i < checked_items.Count; i++) {
					ListViewItem item = (ListViewItem) checked_items [i];
					if (String.Compare (key, item.Name, true) == 0)
						return i;
				}

				return -1;
			}
#endif
			#endregion	// Public Methods

			internal ArrayList List {
				get {
					if (list == null) {
						list = new ArrayList ();
						foreach (ListViewItem item in owner.Items) {
							if (item.Checked)
								list.Add (item);
						}
					}
					return list;
				}
			}

			internal void Reset ()
			{
				// force re-population of list
				list = null;
			}

			private void ItemsCollection_Changed ()
			{
				Reset ();
			}
		}	// CheckedListViewItemCollection

		public class ColumnHeaderCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public ColumnHeaderCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return list.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			public virtual ColumnHeader this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("index");
					return (ColumnHeader) list [index];
				}
			}

#if NET_2_0
			public virtual ColumnHeader this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return (ColumnHeader) list [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual int Add (ColumnHeader value)
			{
				int idx = list.Add (value);
				owner.AddColumn (value, idx, true);
				return idx;
			}

			public virtual ColumnHeader Add (string str, int width, HorizontalAlignment textAlign)
			{
				ColumnHeader colHeader = new ColumnHeader (this.owner, str, textAlign, width);
				this.Add (colHeader);
				return colHeader;
			}

#if NET_2_0
			public virtual ColumnHeader Add (string text)
			{
				return Add (String.Empty, text);
			}

			public virtual ColumnHeader Add (string text, int iwidth)
			{
				return Add (String.Empty, text, iwidth);
			}

			public virtual ColumnHeader Add (string key, string text)
			{
				ColumnHeader colHeader = new ColumnHeader ();
				colHeader.Name = key;
				colHeader.Text = text;
				Add (colHeader);
				return colHeader;
			}

			public virtual ColumnHeader Add (string key, string text, int iwidth)
			{
				return Add (key, text, iwidth, HorizontalAlignment.Left, -1);
			}

			public virtual ColumnHeader Add (string key, string text, int iwidth, HorizontalAlignment textAlign, int imageIndex)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, iwidth, textAlign);
				colHeader.ImageIndex = imageIndex;
				Add (colHeader);
				return colHeader;
			}

			public virtual ColumnHeader Add (string key, string text, int iwidth, HorizontalAlignment textAlign, string imageKey)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, iwidth, textAlign);
				colHeader.ImageKey = imageKey;
				Add (colHeader);
				return colHeader;
			}
#endif

			public virtual void AddRange (ColumnHeader [] values)
			{
				foreach (ColumnHeader colHeader in values) {
					int idx = list.Add (colHeader);
					owner.AddColumn (colHeader, idx, false);
				}
				
				owner.Redraw (true);
			}

			public virtual void Clear ()
			{
				foreach (ColumnHeader col in list)
					col.SetListView (null);
				list.Clear ();
				owner.ReorderColumns (new int [0], true);
			}

			public bool Contains (ColumnHeader value)
			{
				return list.Contains (value);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			int IList.Add (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				return this.Add ((ColumnHeader) value);
			}

			bool IList.Contains (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				return this.Contains ((ColumnHeader) value);
			}

			int IList.IndexOf (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				return this.IndexOf ((ColumnHeader) value);
			}

			void IList.Insert (int index, object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				this.Insert (index, (ColumnHeader) value);
			}

			void IList.Remove (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				this.Remove ((ColumnHeader) value);
			}

			public int IndexOf (ColumnHeader value)
			{
				return list.IndexOf (value);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key.Length == 0)
					return -1;

				for (int i = 0; i < list.Count; i++) {
					ColumnHeader col = (ColumnHeader) list [i];
					if (String.Compare (key, col.Name, true) == 0)
						return i;
				}

				return -1;
			}
#endif

			public void Insert (int index, ColumnHeader value)
			{
				// LAMESPEC: MSDOCS say greater than or equal to the value of the Count property
				// but it's really only greater.
				if (index < 0 || index > list.Count)
					throw new ArgumentOutOfRangeException ("index");

				list.Insert (index, value);
				owner.AddColumn (value, index, true);
			}

#if NET_2_0
			public void Insert (int index, string text)
			{
				Insert (index, String.Empty, text);
			}

			public void Insert (int index, string text, int width)
			{
				Insert (index, String.Empty, text, width);
			}

			public void Insert (int index, string key, string text)
			{
				ColumnHeader colHeader = new ColumnHeader ();
				colHeader.Name = key;
				colHeader.Text = text;
				Insert (index, colHeader);
			}

			public void Insert (int index, string key, string text, int width)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, HorizontalAlignment.Left);
				Insert (index, colHeader);
			}

			public void Insert (int index, string key, string text, int width, HorizontalAlignment textAlign, int imageIndex)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, textAlign);
				colHeader.ImageIndex = imageIndex;
				Insert (index, colHeader);
			}

			public void Insert (int index, string key, string text, int width, HorizontalAlignment textAlign, string imageKey)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, textAlign);
				colHeader.ImageKey = imageKey;
				Insert (index, colHeader);
			}
#endif

			public void Insert (int index, string str, int width, HorizontalAlignment textAlign)
			{
				ColumnHeader colHeader = new ColumnHeader (this.owner, str, textAlign, width);
				this.Insert (index, colHeader);
			}

			public virtual void Remove (ColumnHeader column)
			{
				if (!Contains (column))
					return;

				list.Remove (column);
				column.SetListView (null);

				int rem_display_index = column.InternalDisplayIndex;
				int [] display_indices = new int [list.Count];
				for (int i = 0; i < display_indices.Length; i++) {
					ColumnHeader col = (ColumnHeader) list [i];
					int display_index = col.InternalDisplayIndex;
					if (display_index < rem_display_index) {
						display_indices [i] = display_index;
					} else {
						display_indices [i] = (display_index - 1);
					}
				}

				column.InternalDisplayIndex = -1;
				owner.ReorderColumns (display_indices, true);
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int idx = IndexOfKey (key);
				if (idx != -1)
					RemoveAt (idx);
			}
#endif

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("index");

				ColumnHeader col = (ColumnHeader) list [index];
				Remove (col);
			}
			#endregion	// Public Methods
			

		}	// ColumnHeaderCollection

		public class ListViewItemCollection : IList, ICollection, IEnumerable
		{
			private readonly ArrayList list;
			private readonly ListView owner;

			#region Public Constructor
			public ListViewItemCollection (ListView owner)
			{
				list = new ArrayList (0);
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
#if NET_2_0
					if (owner != null && owner.VirtualMode)
						return owner.VirtualListSize;
#endif

					return list.Count; 
				}
			}

			public bool IsReadOnly {
				get { return false; }
			}

			public virtual ListViewItem this [int displayIndex] {
				get {
					if (displayIndex < 0 || displayIndex >= Count)
						throw new ArgumentOutOfRangeException ("displayIndex");

#if NET_2_0
					if (owner != null && owner.VirtualMode)
						return RetrieveVirtualItemFromOwner (displayIndex);
#endif
					return (ListViewItem) list [displayIndex];
				}

				set {
					if (displayIndex < 0 || displayIndex >= Count)
						throw new ArgumentOutOfRangeException ("displayIndex");

#if NET_2_0
					if (owner != null && owner.VirtualMode)
						throw new InvalidOperationException ();
#endif

					if (list.Contains (value))
						throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

					if (value.ListView != null && value.ListView != owner)
						throw new ArgumentException ("Cannot add or insert the item '" + value.Text + "' in more than one place. You must first remove it from its current location or clone it.", "value");

					value.Owner = owner;
					list [displayIndex] = value;
					OnChange ();

					owner.Redraw (true);
				}
			}

#if NET_2_0
			public virtual ListViewItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return this [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set {
					if (value is ListViewItem)
						this [index] = (ListViewItem) value;
					else
						this [index] = new ListViewItem (value.ToString ());
					OnChange ();
				}
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual ListViewItem Add (ListViewItem value)
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				AddItem (value);
				CollectionChanged (true);

				return value;
			}

			public virtual ListViewItem Add (string text)
			{
				ListViewItem item = new ListViewItem (text);
				return this.Add (item);
			}

			public virtual ListViewItem Add (string text, int imageIndex)
			{
				ListViewItem item = new ListViewItem (text, imageIndex);
				return this.Add (item);
			}

#if NET_2_0
			public virtual ListViewItem Add (string text, string imageKey)
			{
				ListViewItem item = new ListViewItem (text, imageKey);
				return this.Add (item);
			}

			public virtual ListViewItem Add (string key, string text, int imageIndex)
			{
				ListViewItem item = new ListViewItem (text, imageIndex);
				item.Name = key;
				return this.Add (item);
			}

			public virtual ListViewItem Add (string key, string text, string imageKey)
			{
				ListViewItem item = new ListViewItem (text, imageKey);
				item.Name = key;
				return this.Add (item);
			}
#endif

			public void AddRange (ListViewItem [] values)
			{
				if (values == null)
					throw new ArgumentNullException ("Argument cannot be null!", "values");
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				foreach (ListViewItem item in values)
					AddItem (item);

				CollectionChanged (true);
			}

#if NET_2_0
			public void AddRange (ListViewItemCollection items)
			{
				if (items == null)
					throw new ArgumentNullException ("Argument cannot be null!", "items");

				ListViewItem[] itemArray = new ListViewItem[items.Count];
				items.CopyTo (itemArray,0);
				this.AddRange (itemArray);
			}
#endif

			public virtual void Clear ()
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif
				owner.SetFocusedItem (null);
				owner.h_scroll.Value = owner.v_scroll.Value = 0;
				foreach (ListViewItem item in list) {
					owner.item_control.CancelEdit (item);
					item.Owner = null;
				}
				list.Clear ();
				CollectionChanged (false);
			}

			public bool Contains (ListViewItem item)
			{
				return IndexOf (item) != -1;
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

#if NET_2_0
			public ListViewItem [] Find (string key, bool searchAllSubitems)
			{
				if (key == null)
					return new ListViewItem [0];

				List<ListViewItem> temp_list = new List<ListViewItem> ();
				
				for (int i = 0; i < list.Count; i++) {
					ListViewItem lvi = (ListViewItem) list [i];
					if (String.Compare (key, lvi.Name, true) == 0)
						temp_list.Add (lvi);
				}

				ListViewItem [] retval = new ListViewItem [temp_list.Count];
				temp_list.CopyTo (retval);

				return retval;
			}
#endif

			public IEnumerator GetEnumerator ()
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif
				
				return list.GetEnumerator ();
			}

			int IList.Add (object item)
			{
				int result;
				ListViewItem li;

#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				if (item is ListViewItem) {
					li = (ListViewItem) item;
					if (list.Contains (li))
						throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");

					if (li.ListView != null && li.ListView != owner)
						throw new ArgumentException ("Cannot add or insert the item '" + li.Text + "' in more than one place. You must first remove it from its current location or clone it.", "item");
				}
				else
					li = new ListViewItem (item.ToString ());

				li.Owner = owner;
				result = list.Add (li);
				CollectionChanged (true);

				return result;
			}

			bool IList.Contains (object item)
			{
				return Contains ((ListViewItem) item);
			}

			int IList.IndexOf (object item)
			{
				return IndexOf ((ListViewItem) item);
			}

			void IList.Insert (int index, object item)
			{
				if (item is ListViewItem)
					this.Insert (index, (ListViewItem) item);
				else
					this.Insert (index, item.ToString ());
			}

			void IList.Remove (object item)
			{
				Remove ((ListViewItem) item);
			}

			public int IndexOf (ListViewItem item)
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode) {
					for (int i = 0; i < Count; i++)
						if (RetrieveVirtualItemFromOwner (i) == item)
							return i;

					return -1;
				}
#endif
				
				return list.IndexOf (item);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key.Length == 0)
					return -1;

				for (int i = 0; i < Count; i++) {
					ListViewItem lvi = this [i];
					if (String.Compare (key, lvi.Name, true) == 0)
						return i;
				}

				return -1;
			}
#endif

			public ListViewItem Insert (int index, ListViewItem item)
			{
				if (index < 0 || index > list.Count)
					throw new ArgumentOutOfRangeException ("index");

#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				if (list.Contains (item))
					throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");

				if (item.ListView != null && item.ListView != owner)
					throw new ArgumentException ("Cannot add or insert the item '" + item.Text + "' in more than one place. You must first remove it from its current location or clone it.", "item");

				item.Owner = owner;
				list.Insert (index, item);
				CollectionChanged (true);
				return item;
			}

			public ListViewItem Insert (int index, string text)
			{
				return this.Insert (index, new ListViewItem (text));
			}

			public ListViewItem Insert (int index, string text, int imageIndex)
			{
				return this.Insert (index, new ListViewItem (text, imageIndex));
			}

#if NET_2_0
			public ListViewItem Insert (int index, string key, string text, int imageIndex)
			{
				ListViewItem lvi = new ListViewItem (text, imageIndex);
				lvi.Name = key;
				return Insert (index, lvi);
			}
#endif

			public virtual void Remove (ListViewItem item)
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif
				if (!list.Contains (item))
					return;
	 				
				bool selection_changed = owner.SelectedItems.Contains (item);
				owner.item_control.CancelEdit (item);
				list.Remove (item);
				item.Owner = null;
				CollectionChanged (false);
				if (selection_changed)
					owner.OnSelectedIndexChanged (EventArgs.Empty);
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");

#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				ListViewItem item = (ListViewItem) list [index];
				Remove (item);
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int idx = IndexOfKey (key);
				if (idx != -1)
					RemoveAt (idx);
			}
#endif

			#endregion	// Public Methods

			void AddItem (ListViewItem value)
			{
				if (list.Contains (value))
					throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

				if (value.ListView != null && value.ListView != owner)
					throw new ArgumentException ("Cannot add or insert the item '" + value.Text + "' in more than one place. You must first remove it from its current location or clone it.", "value");
				value.Owner = owner;
				list.Add (value);
			}

			void CollectionChanged (bool sort)
			{
				if (owner != null) {
					if (sort)
						owner.Sort (false);

					OnChange ();
					owner.Redraw (true);
				}
			}

#if NET_2_0
			ListViewItem RetrieveVirtualItemFromOwner (int displayIndex)
			{
				RetrieveVirtualItemEventArgs args = new RetrieveVirtualItemEventArgs (displayIndex);

				owner.OnRetrieveVirtualItem (args);
				ListViewItem retval = args.Item;
				retval.Owner = owner;
				retval.SetIndex (displayIndex);

				return retval;
			}
#endif

			internal event CollectionChangedHandler Changed;

			internal void Sort (IComparer comparer)
			{
				list.Sort (comparer);
				OnChange ();
			}

			internal void OnChange ()
			{
				if (Changed != null)
					Changed ();
			}
		}	// ListViewItemCollection

		public class SelectedIndexCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;
			private ArrayList list;

			#region Public Constructor
			public SelectedIndexCollection (ListView owner)
			{
				this.owner = owner;
				owner.Items.Changed += new CollectionChangedHandler (ItemsCollection_Changed);
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					if (!owner.IsHandleCreated)
						return 0;

					return List.Count;
				}
			}

			public bool IsReadOnly {
				get { 
#if NET_2_0
					return false;
#else
					return true; 
#endif
				}
			}

			public int this [int index] {
				get {
					if (!owner.IsHandleCreated || index < 0 || index >= List.Count)
						throw new ArgumentOutOfRangeException ("index");

					return (int) List [index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { 
#if NET_2_0
					return false;
#else
					return true;
#endif
				}
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
#if NET_2_0
			public int Add (int itemIndex)
			{
				if (itemIndex < 0 || itemIndex >= owner.Items.Count)
					throw new ArgumentOutOfRangeException ("index");

				owner.Items [itemIndex].Selected = true;
				if (!owner.IsHandleCreated)
					return 0;

				return List.Count;
			}
#endif

#if NET_2_0
			public 
#else
			internal
#endif	
			void Clear ()
			{
				if (!owner.IsHandleCreated)
					return;

				foreach (int index in List)
					owner.Items [index].Selected = false;
			}

			public bool Contains (int selectedIndex)
			{
				return IndexOf (selectedIndex) != -1;
			}

			public void CopyTo (Array dest, int index)
			{
				List.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
			{
				return List.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				Clear ();
			}

			bool IList.Contains (object selectedIndex)
			{
				if (!(selectedIndex is int))
					return false;
				return Contains ((int) selectedIndex);
			}

			int IList.IndexOf (object selectedIndex)
			{
				if (!(selectedIndex is int))
					return -1;
				return IndexOf ((int) selectedIndex);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (int selectedIndex)
			{
				return List.IndexOf (selectedIndex);
			}

#if NET_2_0
			public void Remove (int itemIndex)
			{
				if (itemIndex < 0 || itemIndex >= owner.Items.Count)
					throw new ArgumentOutOfRangeException ("itemIndex");

				owner.Items [itemIndex].Selected = false;
			}
#endif
			#endregion	// Public Methods

			internal ArrayList List {
				get {
					if (list == null) {
						list = new ArrayList ();
						for (int i = 0; i < owner.Items.Count; i++) {
							if (owner.Items [i].Selected)
								list.Add (i);
						}
					}

					return list;
				}
			}

			internal void Reset ()
			{
				// force re-population of list
				list = null;
			}

			private void ItemsCollection_Changed ()
			{
				Reset ();
			}

		}	// SelectedIndexCollection

		public class SelectedListViewItemCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;

			#region Public Constructor
			public SelectedListViewItemCollection (ListView owner)
			{
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					return owner.SelectedIndices.Count;
				}
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					if (!owner.IsHandleCreated || index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("index");

					int item_index = owner.SelectedIndices [index];
					return owner.Items [item_index];
				}
			}

#if NET_2_0
			public virtual ListViewItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return this [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public void Clear ()
			{
				owner.SelectedIndices.Clear ();
			}

			public bool Contains (ListViewItem item)
			{
				return IndexOf (item) != -1;
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public void CopyTo (Array dest, int index)
			{
				if (!owner.IsHandleCreated)
					return;
				if (index > Count) // Throws ArgumentException instead of IOOR exception
					throw new ArgumentException ("index");

				for (int i = 0; i < Count; i++)
					dest.SetValue (this [i], index++);
			}

			public IEnumerator GetEnumerator ()
			{
				if (!owner.IsHandleCreated)
					return (new ListViewItem [0]).GetEnumerator ();

				ListViewItem [] items = new ListViewItem [Count];
				for (int i = 0; i < Count; i++)
					items [i] = this [i];

				return items.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			bool IList.Contains (object item)
			{
				if (!(item is ListViewItem))
					return false;
				return Contains ((ListViewItem) item);
			}

			int IList.IndexOf (object item)
			{
				if (!(item is ListViewItem))
					return -1;
				return IndexOf ((ListViewItem) item);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (ListViewItem item)
			{
				if (!owner.IsHandleCreated)
					return -1;

				for (int i = 0; i < Count; i++)
					if (this [i] == item)
						return i;

				return -1;
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (!owner.IsHandleCreated || key == null || key.Length == 0)
					return -1;

				for (int i = 0; i < Count; i++) {
					ListViewItem item = this [i];
					if (String.Compare (item.Name, key, true) == 0)
						return i;
				}

				return -1;
			}
#endif
			#endregion	// Public Methods

		}	// SelectedListViewItemCollection

		internal delegate void CollectionChangedHandler ();

		struct ItemMatrixLocation
		{
			int row;
			int col;

			public ItemMatrixLocation (int row, int col)
			{
				this.row = row;
				this.col = col;
		
			}
		
			public int Col {
				get {
					return col;
				}
				set {
					col = value;
				}
			}

			public int Row {
				get {
					return row;
				}
				set {
					row = value;
				}
			}
	
		}

		#endregion // Subclasses
#if NET_2_0
		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		//
		// ColumnReorder event
		//
		static object ColumnReorderedEvent = new object ();
		public event ColumnReorderedEventHandler ColumnReordered {
			add { Events.AddHandler (ColumnReorderedEvent, value); }
			remove { Events.RemoveHandler (ColumnReorderedEvent, value); }
		}

		protected virtual void OnColumnReordered (ColumnReorderedEventArgs e)
		{
			ColumnReorderedEventHandler creh = (ColumnReorderedEventHandler) (Events [ColumnReorderedEvent]);

			if (creh != null)
				creh (this, e);
		}

		//
		// ColumnWidthChanged
		//
		static object ColumnWidthChangedEvent = new object ();
		public event ColumnWidthChangedEventHandler ColumnWidthChanged {
			add { Events.AddHandler (ColumnWidthChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnWidthChangedEvent, value); }
		}

		protected virtual void OnColumnWidthChanged (ColumnWidthChangedEventArgs e)
		{
			ColumnWidthChangedEventHandler eh = (ColumnWidthChangedEventHandler) (Events[ColumnWidthChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		void RaiseColumnWidthChanged (int resize_column)
		{
			ColumnWidthChangedEventArgs n = new ColumnWidthChangedEventArgs (resize_column);

			OnColumnWidthChanged (n);
		}
		
		//
		// ColumnWidthChanging
		//
		static object ColumnWidthChangingEvent = new object ();
		public event ColumnWidthChangingEventHandler ColumnWidthChanging {
			add { Events.AddHandler (ColumnWidthChangingEvent, value); }
			remove { Events.RemoveHandler (ColumnWidthChangingEvent, value); }
		}

		protected virtual void OnColumnWidthChanging (ColumnWidthChangingEventArgs e)
		{
			ColumnWidthChangingEventHandler cwceh = (ColumnWidthChangingEventHandler) (Events[ColumnWidthChangingEvent]);
			if (cwceh != null)
				cwceh (this, e);
		}
		
		//
		// 2.0 profile based implementation
		//
		bool CanProceedWithResize (ColumnHeader col, int width)
		{
			ColumnWidthChangingEventHandler cwceh = (ColumnWidthChangingEventHandler) (Events[ColumnWidthChangingEvent]);
			if (cwceh == null)
				return true;
			
			ColumnWidthChangingEventArgs changing = new ColumnWidthChangingEventArgs (col.Index, width);
			cwceh (this, changing);
			return !changing.Cancel;
		}
#else
		//
		// 1.0 profile based implementation
		//
		bool CanProceedWithResize (ColumnHeader col, int width)
		{
			return true;
		}

		void RaiseColumnWidthChanged (int resize_column)
		{
		}
#endif
	}
}

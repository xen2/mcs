//
// System.Web.UI.WebControls.FormView.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Security.Permissions;
using System.Text;
using System.IO;
using System.Reflection;

namespace System.Web.UI.WebControls
{
	[SupportsEventValidation]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.FormViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ControlValuePropertyAttribute ("SelectedValue")]
	[DefaultEventAttribute ("PageIndexChanging")]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class FormView: CompositeDataBoundControl, IDataItemContainer, INamingContainer, IPostBackEventHandler, IPostBackContainer
	{
		object dataItem;
		
		Table table;
		FormViewRow headerRow;
		FormViewRow footerRow;
		FormViewRow bottomPagerRow;
		FormViewRow topPagerRow;
		FormViewRow itemRow;
		
		IOrderedDictionary currentEditRowKeys;
		IOrderedDictionary currentEditNewValues;
		IOrderedDictionary currentEditOldValues;
		
		ITemplate pagerTemplate;
		ITemplate emptyDataTemplate;
		ITemplate headerTemplate;
		ITemplate footerTemplate;
		ITemplate editItemTemplate;
		ITemplate insertItemTemplate;
		ITemplate itemTemplate;
		
		PropertyDescriptor[] cachedKeyProperties;
		readonly string[] emptyKeys = new string[0];
		
		// View state
		PagerSettings pagerSettings;
		
		TableItemStyle editRowStyle;
		TableItemStyle insertRowStyle;
		TableItemStyle emptyDataRowStyle;
		TableItemStyle footerStyle;
		TableItemStyle headerStyle;
		TableItemStyle pagerStyle;
		TableItemStyle rowStyle;
		
		DataKey key;
		DataKey oldEditValues;
		
		private static readonly object PageIndexChangedEvent = new object();
		private static readonly object PageIndexChangingEvent = new object();
		private static readonly object ItemCommandEvent = new object();
		private static readonly object ItemCreatedEvent = new object();
		private static readonly object ItemDeletedEvent = new object();
		private static readonly object ItemDeletingEvent = new object();
		private static readonly object ItemInsertedEvent = new object();
		private static readonly object ItemInsertingEvent = new object();
		private static readonly object ModeChangingEvent = new object();
		private static readonly object ModeChangedEvent = new object();
		private static readonly object ItemUpdatedEvent = new object();
		private static readonly object ItemUpdatingEvent = new object();
		
		// Control state
		int pageIndex;
		FormViewMode currentMode = FormViewMode.ReadOnly; 
		bool hasCurrentMode;
		int pageCount = 0;
		
		public FormView ()
		{
			key = new DataKey (new OrderedDictionary ());
		}
		
		public event EventHandler PageIndexChanged {
			add { Events.AddHandler (PageIndexChangedEvent, value); }
			remove { Events.RemoveHandler (PageIndexChangedEvent, value); }
		}
		
		public event FormViewPageEventHandler PageIndexChanging {
			add { Events.AddHandler (PageIndexChangingEvent, value); }
			remove { Events.RemoveHandler (PageIndexChangingEvent, value); }
		}
		
		public event FormViewCommandEventHandler ItemCommand {
			add { Events.AddHandler (ItemCommandEvent, value); }
			remove { Events.RemoveHandler (ItemCommandEvent, value); }
		}
		
		public event EventHandler ItemCreated {
			add { Events.AddHandler (ItemCreatedEvent, value); }
			remove { Events.RemoveHandler (ItemCreatedEvent, value); }
		}
		
		public event FormViewDeletedEventHandler ItemDeleted {
			add { Events.AddHandler (ItemDeletedEvent, value); }
			remove { Events.RemoveHandler (ItemDeletedEvent, value); }
		}
		
		public event FormViewDeleteEventHandler ItemDeleting {
			add { Events.AddHandler (ItemDeletingEvent, value); }
			remove { Events.RemoveHandler (ItemDeletingEvent, value); }
		}
		
		public event FormViewInsertedEventHandler ItemInserted {
			add { Events.AddHandler (ItemInsertedEvent, value); }
			remove { Events.RemoveHandler (ItemInsertedEvent, value); }
		}
		
		public event FormViewInsertEventHandler ItemInserting {
			add { Events.AddHandler (ItemInsertingEvent, value); }
			remove { Events.RemoveHandler (ItemInsertingEvent, value); }
		}
		
		public event FormViewModeEventHandler ModeChanging {
			add { Events.AddHandler (ModeChangingEvent, value); }
			remove { Events.RemoveHandler (ModeChangingEvent, value); }
		}
		
		public event EventHandler ModeChanged {
			add { Events.AddHandler (ModeChangedEvent, value); }
			remove { Events.RemoveHandler (ModeChangedEvent, value); }
		}
		
		public event FormViewUpdatedEventHandler ItemUpdated {
			add { Events.AddHandler (ItemUpdatedEvent, value); }
			remove { Events.RemoveHandler (ItemUpdatedEvent, value); }
		}
		
		public event FormViewUpdateEventHandler ItemUpdating {
			add { Events.AddHandler (ItemUpdatingEvent, value); }
			remove { Events.RemoveHandler (ItemUpdatingEvent, value); }
		}
		
		protected virtual void OnPageIndexChanged (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [PageIndexChangedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnPageIndexChanging (FormViewPageEventArgs e)
		{
			if (Events != null) {
				FormViewPageEventHandler eh = (FormViewPageEventHandler) Events [PageIndexChangingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnItemCommand (FormViewCommandEventArgs e)
		{
			if (Events != null) {
				FormViewCommandEventHandler eh = (FormViewCommandEventHandler) Events [ItemCommandEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnItemCreated (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [ItemCreatedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnItemDeleted (FormViewDeletedEventArgs e)
		{
			if (Events != null) {
				FormViewDeletedEventHandler eh = (FormViewDeletedEventHandler) Events [ItemDeletedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnItemInserted (FormViewInsertedEventArgs e)
		{
			if (Events != null) {
				FormViewInsertedEventHandler eh = (FormViewInsertedEventHandler) Events [ItemInsertedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnItemInserting (FormViewInsertEventArgs e)
		{
			if (Events != null) {
				FormViewInsertEventHandler eh = (FormViewInsertEventHandler) Events [ItemInsertingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnItemDeleting (FormViewDeleteEventArgs e)
		{
			if (Events != null) {
				FormViewDeleteEventHandler eh = (FormViewDeleteEventHandler) Events [ItemDeletingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnModeChanged (EventArgs e)
		{
			if (Events != null) {
				EventHandler eh = (EventHandler) Events [ModeChangedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnModeChanging (FormViewModeEventArgs e)
		{
			if (Events != null) {
				FormViewModeEventHandler eh = (FormViewModeEventHandler) Events [ModeChangingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnItemUpdated (FormViewUpdatedEventArgs e)
		{
			if (Events != null) {
				FormViewUpdatedEventHandler eh = (FormViewUpdatedEventHandler) Events [ItemUpdatedEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnItemUpdating (FormViewUpdateEventArgs e)
		{
			if (Events != null) {
				FormViewUpdateEventHandler eh = (FormViewUpdateEventHandler) Events [ItemUpdatingEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		
		[WebCategoryAttribute ("Paging")]
		[DefaultValueAttribute (false)]
		public virtual bool AllowPaging {
			get {
				object ob = ViewState ["AllowPaging"];
				if (ob != null) return (bool) ob;
				return false;
			}
			set {
				ViewState ["AllowPaging"] = value;
				RequireBinding ();
			}
		}
		
		[UrlPropertyAttribute]
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string BackImageUrl {
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).BackImageUrl;
				return String.Empty;
			}
			set {
				((TableStyle) ControlStyle).BackImageUrl = value;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual FormViewRow BottomPagerRow {
			get {
				EnsureDataBound ();
				return bottomPagerRow;
			}
		}
	
		[WebCategoryAttribute ("Accessibility")]
		[DefaultValueAttribute ("")]
		[LocalizableAttribute (true)]
		public virtual string Caption {
			get {
				object ob = ViewState ["Caption"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["Caption"] = value;
				RequireBinding ();
			}
		}
		
		[WebCategoryAttribute ("Accessibility")]
		[DefaultValueAttribute (TableCaptionAlign.NotSet)]
		public virtual TableCaptionAlign CaptionAlign
		{
			get {
				object o = ViewState ["CaptionAlign"];
				if(o != null) return (TableCaptionAlign) o;
				return TableCaptionAlign.NotSet;
			}
			set {
				ViewState ["CaptionAlign"] = value;
				RequireBinding ();
			}
		}

		[WebCategoryAttribute ("Layout")]
		[DefaultValueAttribute (-1)]
		public virtual int CellPadding
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).CellPadding;
				return -1;
			}
			set {
				((TableStyle) ControlStyle).CellPadding = value;
			}
		}

		[WebCategoryAttribute ("Layout")]
		[DefaultValueAttribute (0)]
		public virtual int CellSpacing
		{
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).CellSpacing;
				return 0;
			}
			set {
				((TableStyle) ControlStyle).CellSpacing = value;
			}
		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public FormViewMode CurrentMode {
			get {
				return hasCurrentMode ? currentMode : DefaultMode;
			}
			private set {
				hasCurrentMode = true;
				currentMode = value;
			}
		}

		FormViewMode defaultMode;

		[DefaultValueAttribute (FormViewMode.ReadOnly)]
		[WebCategoryAttribute ("Behavior")]
		public virtual FormViewMode DefaultMode {
			get {
				return defaultMode;
			}
			set {
				defaultMode = value;
				RequireBinding ();
			}
		}

		string[] dataKeyNames;
		[DefaultValueAttribute (null)]
		[WebCategoryAttribute ("Data")]
		[TypeConverter (typeof(StringArrayConverter))]
		[EditorAttribute ("System.Web.UI.Design.WebControls.DataFieldEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string[] DataKeyNames
		{
			get {
				if (dataKeyNames == null)
					return emptyKeys;
				return dataKeyNames;
			}
			set {
				dataKeyNames = value;
				RequireBinding ();
			}
		}
		
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual DataKey DataKey {
			get {
				EnsureDataBound ();
				return key;
			}
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(FormView), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate EditItemTemplate {
			get { return editItemTemplate; }
			set { editItemTemplate = value; RequireBinding (); }
		}

		[WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[DefaultValueAttribute (null)]
		public TableItemStyle EditRowStyle {
			get {
				if (editRowStyle == null) {
					editRowStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						editRowStyle.TrackViewState();
				}
				return editRowStyle;
			}
		}
		
		[WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[DefaultValueAttribute (null)]
		public TableItemStyle EmptyDataRowStyle {
			get {
				if (emptyDataRowStyle == null) {
					emptyDataRowStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						emptyDataRowStyle.TrackViewState();
				}
				return emptyDataRowStyle;
			}
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(FormView), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate EmptyDataTemplate {
			get { return emptyDataTemplate; }
			set { emptyDataTemplate = value; RequireBinding (); }
		}
		
		[LocalizableAttribute (true)]
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute ("")]
		public virtual string EmptyDataText {
			get {
				object ob = ViewState ["EmptyDataText"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["EmptyDataText"] = value;
				RequireBinding ();
			}
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual FormViewRow FooterRow {
			get {
				EnsureChildControls ();
				return footerRow;
			}
		}
	
		[DefaultValue (null)]
		[TemplateContainer (typeof(FormView), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate FooterTemplate {
			get { return footerTemplate; }
			set { footerTemplate = value; RequireBinding (); }
		}

		[LocalizableAttribute (true)]
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute ("")]
		public virtual string FooterText {
			get {
				object ob = ViewState ["FooterText"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["FooterText"] = value;
				RequireBinding ();
			}
		}
		
		[WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TableItemStyle FooterStyle {
			get {
				if (footerStyle == null) {
					footerStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						footerStyle.TrackViewState();
				}
				return footerStyle;
			}
		}
		
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute (GridLines.None)]
		public virtual GridLines GridLines {
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).GridLines;
				return GridLines.None;
			}
			set {
				((TableStyle) ControlStyle).GridLines = value;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual FormViewRow HeaderRow {
			get {
				EnsureChildControls ();
				return headerRow;
			}
		}
	
		[WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TableItemStyle HeaderStyle {
			get {
				if (headerStyle == null) {
					headerStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						headerStyle.TrackViewState();
				}
				return headerStyle;
			}
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(FormView), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate HeaderTemplate {
			get { return headerTemplate; }
			set { headerTemplate = value; RequireBinding (); }
		}

		[LocalizableAttribute (true)]
		[WebCategoryAttribute ("Appearance")]
		[DefaultValueAttribute ("")]
		public virtual string HeaderText {
			get {
				object ob = ViewState ["HeaderText"];
				if (ob != null) return (string) ob;
				return string.Empty;
			}
			set {
				ViewState ["HeaderText"] = value;
				RequireBinding ();
			}
		}
		
		[Category ("Layout")]
		[DefaultValueAttribute (HorizontalAlign.NotSet)]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				if (ControlStyleCreated)
					return ((TableStyle) ControlStyle).HorizontalAlign;
				return HorizontalAlign.NotSet;
			}
			set {
				((TableStyle) ControlStyle).HorizontalAlign = value;
			}
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(FormView), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate InsertItemTemplate {
			get { return insertItemTemplate; }
			set { insertItemTemplate = value; RequireBinding (); }
		}

		[WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[DefaultValueAttribute (null)]
		public TableItemStyle InsertRowStyle {
			get {
				if (insertRowStyle == null) {
					insertRowStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						insertRowStyle.TrackViewState();
				}
				return insertRowStyle;
			}
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(FormView), BindingDirection.TwoWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate ItemTemplate {
			get { return itemTemplate; }
			set { itemTemplate = value; RequireBinding (); }
		}
		
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual int PageCount {
			get {
				if (pageCount != 0) return pageCount;
				EnsureDataBound ();
				return pageCount;
			}
		}

		[WebCategoryAttribute ("Paging")]
		[BindableAttribute (true, BindingDirection.OneWay)]
		[DefaultValueAttribute (0)]
		public virtual int PageIndex {
			get {
				return pageIndex;
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("PageIndex must be non-negative");
				if (pageIndex == value)
					return;
				pageIndex = value;
				RequireBinding ();
			}
		}
	
		[WebCategoryAttribute ("Paging")]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
		[NotifyParentPropertyAttribute (true)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public virtual PagerSettings PagerSettings {
			get {
				if (pagerSettings == null) {
					pagerSettings = new PagerSettings (this);
					if (IsTrackingViewState)
						((IStateManager)pagerSettings).TrackViewState ();
				}
				return pagerSettings;
			}
		}
	
		[WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TableItemStyle PagerStyle {
			get {
				if (pagerStyle == null) {
					pagerStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						pagerStyle.TrackViewState();
				}
				return pagerStyle;
			}
		}
		
		
		[DefaultValue (null)]
		/* DataControlPagerCell isnt specified in the docs */
		//[TemplateContainer (typeof(DataControlPagerCell), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public virtual ITemplate PagerTemplate {
			get { return pagerTemplate; }
			set { pagerTemplate = value; RequireBinding (); }
		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual FormViewRow Row {
			get {
				EnsureDataBound ();
				return itemRow;
			}
		}
		
		[WebCategoryAttribute ("Styles")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[DefaultValue (null)]
		public TableItemStyle RowStyle {
			get {
				if (rowStyle == null) {
					rowStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						rowStyle.TrackViewState();
				}
				return rowStyle;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public object SelectedValue {
			get { return DataKey.Value; }
		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual FormViewRow TopPagerRow {
			get {
				EnsureDataBound ();
				return topPagerRow;
			}
		}
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual object DataItem {
			get {
				EnsureDataBound ();
				return dataItem;
			}
		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public int DataItemCount {
			get { return PageCount; }
		}		
	
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public virtual int DataItemIndex {
			get { return PageIndex; }
		}

		int IDataItemContainer.DataItemIndex {
			get { return DataItemIndex; }
		}
	
		int IDataItemContainer.DisplayIndex {
			get { return PageIndex; }
		}		
	
		public virtual bool IsBindableType (Type type)
		{
			return type.IsPrimitive || type == typeof (string) || type == typeof (DateTime) || type == typeof (Guid) || type == typeof (Decimal);
		}
		
		protected override DataSourceSelectArguments CreateDataSourceSelectArguments ()
		{
			DataSourceSelectArguments arg = new DataSourceSelectArguments ();
			DataSourceView view = GetData ();
			if (AllowPaging && view.CanPage) {
				arg.StartRowIndex = PageIndex;
				if (view.CanRetrieveTotalRowCount) {
					arg.RetrieveTotalRowCount = true;
					arg.MaximumRows = 1;
				}
				else {
					arg.MaximumRows = -1;
				}
			}
			return arg;
		}
		
		protected virtual FormViewRow CreateRow (int rowIndex, DataControlRowType rowType, DataControlRowState rowState)
		{
			FormViewRow row = new FormViewRow (rowIndex, rowType, rowState);
			OnItemCreated (EventArgs.Empty);
			return row;
		}
		
		void RequireBinding ()
		{
			if (Initialized) {
				pageCount = -1;
				RequiresDataBinding = true;
			}
		}
		
		protected virtual Table CreateTable ()
		{
			return new ContainedTable (this);
		}

		[MonoTODO]
		protected override void EnsureDataBound ()
		{
			base.EnsureDataBound ();
		}
	
		protected override Style CreateControlStyle ()
		{
			TableStyle style = new TableStyle (ViewState);
			style.CellSpacing = 0;
			return style;
		}
		
		protected override int CreateChildControls (IEnumerable data, bool dataBinding)
		{
			PagedDataSource dataSource = new PagedDataSource ();
			dataSource.DataSource = data;
			dataSource.AllowPaging = AllowPaging;
			dataSource.PageSize = 1;
			dataSource.CurrentPageIndex = PageIndex;

			if (dataBinding) {
				DataSourceView view = GetData ();
				if (view != null && view.CanPage) {
					dataSource.AllowServerPaging = true;
					if (view.CanRetrieveTotalRowCount)
						dataSource.VirtualCount = SelectArguments.TotalRowCount;
				}
			}

			pageCount = dataSource.DataSourceCount;
			bool showPager = AllowPaging && (pageCount > 1);
			
			Controls.Clear ();
			table = CreateTable ();
			Controls.Add (table);

			// Gets the current data item
			
			IEnumerator e = dataSource.GetEnumerator (); 
			dataItem = null;

			if (AllowPaging) {
				if (e.MoveNext ())
					dataItem = e.Current;
			}
			else
			for (int page = 0; e.MoveNext (); page++ )
				if (page == PageIndex)
					dataItem = e.Current;
			
			// Main table creation
			
			if (HeaderText.Length != 0 || headerTemplate != null) {
				headerRow = CreateRow (-1, DataControlRowType.Header, DataControlRowState.Normal);
				InitializeRow (headerRow);
				table.Rows.Add (headerRow);
			}
			
			if (showPager && PagerSettings.Position == PagerPosition.Top || PagerSettings.Position == PagerPosition.TopAndBottom) {
				topPagerRow = CreateRow (-1, DataControlRowType.Pager, DataControlRowState.Normal);
				InitializePager (topPagerRow, dataSource);
				table.Rows.Add (topPagerRow);
			}

			if (pageCount > 0) {
				DataControlRowState rstate = GetRowState ();
				itemRow = CreateRow (0, DataControlRowType.DataRow, rstate);
				InitializeRow (itemRow);
				table.Rows.Add (itemRow);
				
				if (!dataBinding) {
					if (CurrentMode == FormViewMode.Edit)
						oldEditValues = new DataKey (new OrderedDictionary ());
					key = new DataKey (new OrderedDictionary (), DataKeyNames);
				}
			} else {
				switch (CurrentMode) {
				case FormViewMode.Edit:
					itemRow = CreateRow (-1, DataControlRowType.EmptyDataRow, DataControlRowState.Edit);
					break;
				case FormViewMode.Insert:
					itemRow = CreateRow (-1, DataControlRowType.DataRow, DataControlRowState.Insert);
					break;
				default:
					itemRow = CreateRow (-1, DataControlRowType.EmptyDataRow, DataControlRowState.Normal);
					break;
				}
				table.Rows.Add (itemRow);
				InitializeRow (itemRow);
			}

			if (FooterText.Length != 0 || footerTemplate != null) {
				footerRow = CreateRow (-1, DataControlRowType.Footer, DataControlRowState.Normal);
				InitializeRow (footerRow);
				table.Rows.Add (footerRow);
			}
			
			if (showPager && PagerSettings.Position == PagerPosition.Bottom || PagerSettings.Position == PagerPosition.TopAndBottom) {
				bottomPagerRow = CreateRow (0, DataControlRowType.Pager, DataControlRowState.Normal);
				InitializePager (bottomPagerRow, dataSource);
				table.Rows.Add (bottomPagerRow);
			}

			if (dataBinding)
				DataBind (false);
			
			return dataSource.DataSourceCount;
		}
		
		DataControlRowState GetRowState ()
		{
			DataControlRowState rstate = DataControlRowState.Normal;
			if (CurrentMode == FormViewMode.Edit) rstate |= DataControlRowState.Edit;
			else if (CurrentMode == FormViewMode.Insert) rstate |= DataControlRowState.Insert;
			return rstate;
		}
		
		protected virtual void InitializePager (FormViewRow row, PagedDataSource dataSource)
		{
			TableCell cell = new TableCell ();
			cell.ColumnSpan = 2;

			if (pagerTemplate != null)
				pagerTemplate.InstantiateIn (cell);
			else
				cell.Controls.Add (PagerSettings.CreatePagerControl (dataSource.CurrentPageIndex, dataSource.PageCount));
			
			row.Cells.Add (cell);
		}
		
		protected virtual void InitializeRow (FormViewRow row)
		{
			TableCell cell = new TableCell ();
			
			if (row.RowType == DataControlRowType.DataRow)
			{
				if ((row.RowState & DataControlRowState.Edit) != 0) {
					if (editItemTemplate != null)
						editItemTemplate.InstantiateIn (cell);
				} else if ((row.RowState & DataControlRowState.Insert) != 0) {
					if (insertItemTemplate != null)
						insertItemTemplate.InstantiateIn (cell);
				} else if (itemTemplate != null)
					itemTemplate.InstantiateIn (cell);
			}
			else if (row.RowType == DataControlRowType.EmptyDataRow)
			{
				if (emptyDataTemplate != null)
					emptyDataTemplate.InstantiateIn (cell);
				else
					cell.Text = EmptyDataText;
			}
			else if (row.RowType == DataControlRowType.Footer)
			{
				if (footerTemplate != null)
					footerTemplate.InstantiateIn (cell);
				else
					cell.Text = FooterText;
			}
			else if (row.RowType == DataControlRowType.Header)
			{
				if (headerTemplate != null)
					headerTemplate.InstantiateIn (cell);
				else
					cell.Text = HeaderText;
			}
			cell.ColumnSpan = 2;
			row.Cells.Add (cell);
		}
		
		IOrderedDictionary CreateRowDataKey (object dataItem)
		{
			if (cachedKeyProperties == null) {
				PropertyDescriptorCollection props = TypeDescriptor.GetProperties (dataItem);
				cachedKeyProperties = new PropertyDescriptor [DataKeyNames.Length];
				for (int n=0; n<DataKeyNames.Length; n++) { 
					PropertyDescriptor p = props [DataKeyNames[n]];
					if (p == null)
						new InvalidOperationException ("Property '" + DataKeyNames[n] + "' not found in object of type " + dataItem.GetType());
					cachedKeyProperties [n] = p;
				}
			}
			
			OrderedDictionary dic = new OrderedDictionary ();
			foreach (PropertyDescriptor p in cachedKeyProperties)
				dic [p.Name] = p.GetValue (dataItem);
			return dic;
		}
		
		IOrderedDictionary GetRowValues (bool includePrimaryKey)
		{
			OrderedDictionary dic = new OrderedDictionary ();
			ExtractRowValues (dic, includePrimaryKey);
			return dic;
		}
		
		protected virtual void ExtractRowValues (IOrderedDictionary fieldValues, bool includeKeys)
		{
			if (Row == null)
				return;

			DataControlRowState rowState = Row.RowState;
			IBindableTemplate bt;
			
			if ((rowState & DataControlRowState.Insert) != 0)
				bt = insertItemTemplate as IBindableTemplate; 
			else if ((rowState & DataControlRowState.Edit) != 0)
				bt = editItemTemplate as IBindableTemplate;
			else
				return;
			
			if (bt != null) {
				IOrderedDictionary values = bt.ExtractValues (Row.Cells [0]);
				foreach (DictionaryEntry e in values) {
					if (includeKeys || Array.IndexOf (DataKeyNames, e.Key) == -1)
						fieldValues [e.Key] = e.Value;
				}
			}
		}
		
		protected override HtmlTextWriterTag TagKey {
			get {
				return HtmlTextWriterTag.Table;
			}
		}
		
		public sealed override void DataBind ()
		{
			if (CurrentMode == FormViewMode.Insert) {
				RequiresDataBinding = false;
				PerformDataBinding (new object [] { null });
				return;
			}

			cachedKeyProperties = null;
			base.DataBind ();
			
			if (pageCount > 0) {
				if (CurrentMode == FormViewMode.Edit)
					oldEditValues = new DataKey (GetRowValues (true));
				else
					oldEditValues = new DataKey (new OrderedDictionary ());
				key = new DataKey (CreateRowDataKey (dataItem), DataKeyNames);
			}
		}
		
		protected internal override void PerformDataBinding (IEnumerable data)
		{
			base.PerformDataBinding (data);
		}

		protected internal virtual void PrepareControlHierarchy ()
		{
			if (table == null)
				return;

			table.Caption = Caption;
			table.CaptionAlign = CaptionAlign;

			foreach (FormViewRow row in table.Rows) {
				switch (row.RowType) {
				case DataControlRowType.Header:
					if (headerStyle != null && !headerStyle.IsEmpty)
						row.ControlStyle.CopyFrom (headerStyle);
					break;
				case DataControlRowType.Footer:
					if (footerStyle != null && !footerStyle.IsEmpty)
						row.ControlStyle.CopyFrom (footerStyle);
					break;
				case DataControlRowType.Pager:
					if (pagerStyle != null && !pagerStyle.IsEmpty)
						row.ControlStyle.CopyFrom (pagerStyle);
					break;
				case DataControlRowType.EmptyDataRow:
					if (emptyDataRowStyle != null && !emptyDataRowStyle.IsEmpty)
						row.ControlStyle.CopyFrom (emptyDataRowStyle);
					break;
				case DataControlRowType.DataRow:
					if (rowStyle != null && !rowStyle.IsEmpty)
						row.ControlStyle.CopyFrom (rowStyle);
					if ((row.RowState & (DataControlRowState.Edit | DataControlRowState.Insert)) != 0 && editRowStyle != null && !editRowStyle.IsEmpty)
						row.ControlStyle.CopyFrom (editRowStyle);
					if ((row.RowState & DataControlRowState.Insert) != 0 && insertRowStyle != null && !insertRowStyle.IsEmpty)
						row.ControlStyle.CopyFrom (insertRowStyle);
					break;
				default:
					break;
				}
			}
		}
		
		protected internal override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}
		
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			FormViewCommandEventArgs args = e as FormViewCommandEventArgs;
			if (args != null) {
				OnItemCommand (args);
				ProcessEvent (args.CommandName, args.CommandArgument as string);
				return true;
			}
			return base.OnBubbleEvent (source, e);
		}
		
                void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}

		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			int i = eventArgument.IndexOf ('$');
			if (i != -1)
				ProcessEvent (eventArgument.Substring (0, i), eventArgument.Substring (i + 1));
			else
				ProcessEvent (eventArgument, null);
		}
		
		void ProcessEvent (string eventName, string param)
		{
			switch (eventName)
			{
			case DataControlCommands.PageCommandName:
				int newIndex = -1;
				switch (param) {
				case DataControlCommands.FirstPageCommandArgument:
					newIndex = 0;
					break;
				case DataControlCommands.LastPageCommandArgument:
					newIndex = PageCount - 1;
					break;
				case DataControlCommands.NextPageCommandArgument:
					newIndex = PageIndex + 1;
					break;
				case DataControlCommands.PreviousPageCommandArgument:
					newIndex = PageIndex - 1;
					break;
				default:
					int paramIndex = 0;
					int.TryParse (param, out paramIndex);
					newIndex = paramIndex - 1;
					break;
				}
				ShowPage (newIndex);
				break;
					
			case DataControlCommands.FirstPageCommandArgument:
				ShowPage (0);
				break;

			case DataControlCommands.LastPageCommandArgument:
				ShowPage (PageCount - 1);
				break;
					
			case DataControlCommands.NextPageCommandArgument:
				if (PageIndex < PageCount - 1)
					ShowPage (PageIndex + 1);
				break;

			case DataControlCommands.PreviousPageCommandArgument:
				if (PageIndex > 0)
					ShowPage (PageIndex - 1);
				break;
					
			case DataControlCommands.EditCommandName:
				ChangeMode (FormViewMode.Edit);
				break;
					
			case DataControlCommands.NewCommandName:
				ChangeMode (FormViewMode.Insert);
				break;
					
			case DataControlCommands.UpdateCommandName:
				UpdateItem (param, true);
				break;
					
			case DataControlCommands.CancelCommandName:
				CancelEdit ();
				break;
					
			case DataControlCommands.DeleteCommandName:
				DeleteItem ();
				break;
					
			case DataControlCommands.InsertCommandName:
				InsertItem (true);
				break;
			}
		}
		
		void ShowPage (int newIndex)
		{
			FormViewPageEventArgs args = new FormViewPageEventArgs (newIndex);
			OnPageIndexChanging (args);
			if (!args.Cancel) {
				newIndex = args.NewPageIndex;
				if (newIndex < 0 || newIndex >= PageCount)
					return;
				EndRowEdit (false);
				PageIndex = newIndex;
				OnPageIndexChanged (EventArgs.Empty);
			}
		}
		
		public void ChangeMode (FormViewMode newMode)
		{
			FormViewModeEventArgs args = new FormViewModeEventArgs (newMode, false);
			OnModeChanging (args);
			if (!args.Cancel) {
				CurrentMode = args.NewMode;
				OnModeChanged (EventArgs.Empty);
				RequireBinding ();
			}
		}
		
		void CancelEdit ()
		{
			FormViewModeEventArgs args = new FormViewModeEventArgs (FormViewMode.ReadOnly, true);
			OnModeChanging (args);
			if (!args.Cancel) {
				EndRowEdit ();
			}
		}

		public virtual void UpdateItem (bool causesValidation)
		{
			UpdateItem (null, causesValidation);
		}
		
		void UpdateItem (string param, bool causesValidation)
		{
			if (causesValidation)
				Page.Validate ();
			
			if (currentMode != FormViewMode.Edit) throw new HttpException ("Must be in Edit mode");
			
			currentEditOldValues = oldEditValues.Values;
			currentEditRowKeys = DataKey.Values;
			currentEditNewValues = GetRowValues (false);
			
			FormViewUpdateEventArgs args = new FormViewUpdateEventArgs (param, currentEditRowKeys, currentEditOldValues, currentEditNewValues);
			OnItemUpdating (args);
			if (!args.Cancel) {
				DataSourceView view = GetData ();
				if (view == null)
					throw new HttpException ("The DataSourceView associated to data bound control was null");
				if (view.CanUpdate)
					view.Update (currentEditRowKeys, currentEditNewValues, currentEditOldValues, new DataSourceViewOperationCallback (UpdateCallback));
			}
			else
				EndRowEdit ();
		}

		bool UpdateCallback (int recordsAffected, Exception exception)
		{
			FormViewUpdatedEventArgs dargs = new FormViewUpdatedEventArgs (recordsAffected, exception, currentEditRowKeys, currentEditOldValues, currentEditNewValues);
			OnItemUpdated (dargs);

			if (!dargs.KeepInEditMode)				
				EndRowEdit ();

			return dargs.ExceptionHandled;
		}

		public virtual void InsertItem (bool causesValidation)
		{
			InsertItem (null, causesValidation);
		}
		
		void InsertItem (string param, bool causesValidation)
		{
			if (causesValidation)
				Page.Validate ();
			
			if (currentMode != FormViewMode.Insert) throw new HttpException ("Must be in Insert mode");
			
			currentEditNewValues = GetRowValues (true);
			FormViewInsertEventArgs args = new FormViewInsertEventArgs (param, currentEditNewValues);
			OnItemInserting (args);
			if (!args.Cancel) {
				DataSourceView view = GetData ();
				if (view == null)
					throw new HttpException ("The DataSourceView associated to data bound control was null");
				if (view.CanInsert)
					view.Insert (currentEditNewValues, new DataSourceViewOperationCallback (InsertCallback));
			}
			else
				EndRowEdit ();
		}
		
		bool InsertCallback (int recordsAffected, Exception exception)
		{
			FormViewInsertedEventArgs dargs = new FormViewInsertedEventArgs (recordsAffected, exception, currentEditNewValues);
			OnItemInserted (dargs);

			if (!dargs.KeepInInsertMode)				
				EndRowEdit ();

			return dargs.ExceptionHandled;
		}

		public virtual void DeleteItem ()
		{
			currentEditRowKeys = DataKey.Values;
			currentEditNewValues = GetRowValues (true);
			
			FormViewDeleteEventArgs args = new FormViewDeleteEventArgs (PageIndex, currentEditRowKeys, currentEditNewValues);
			OnItemDeleting (args);

			if (!args.Cancel) {
				if (PageIndex == PageCount - 1)
					PageIndex --;
					
				RequireBinding ();
					
				DataSourceView view = GetData ();
				if (view != null && view.CanDelete)
					view.Delete (currentEditRowKeys, currentEditNewValues, new DataSourceViewOperationCallback (DeleteCallback));
				else {
					FormViewDeletedEventArgs dargs = new FormViewDeletedEventArgs (0, null, currentEditRowKeys, currentEditNewValues);
					OnItemDeleted (dargs);
				}
			}
		}

		bool DeleteCallback (int recordsAffected, Exception exception)
		{
			FormViewDeletedEventArgs dargs = new FormViewDeletedEventArgs (recordsAffected, exception, currentEditRowKeys, currentEditNewValues);
			OnItemDeleted (dargs);
			return dargs.ExceptionHandled;
		}
		
		void EndRowEdit ()
		{
			EndRowEdit (true);
		}

		void EndRowEdit (bool switchToDefaultMode) 
		{
			if (switchToDefaultMode)
				ChangeMode (DefaultMode);
			oldEditValues = new DataKey (new OrderedDictionary ());
			currentEditRowKeys = null;
			currentEditOldValues = null;
			currentEditNewValues = null;
			RequireBinding ();
		}

		protected internal override void LoadControlState (object ob)
		{
			if (ob == null) return;
			object[] state = (object[]) ob;
			base.LoadControlState (state[0]);
			pageIndex = (int) state[1];
			pageCount = (int) state[2];
			CurrentMode = (FormViewMode) state[3];
			defaultMode = (FormViewMode) state[4];
			dataKeyNames = (string[]) state[5];
		}
		
		protected internal override object SaveControlState ()
		{
			object bstate = base.SaveControlState ();
			return new object[] {
				bstate, pageIndex, pageCount, CurrentMode, defaultMode, dataKeyNames
			};
		}
		
		protected override void TrackViewState()
		{
			base.TrackViewState();
			if (pagerSettings != null) ((IStateManager)pagerSettings).TrackViewState();
			if (footerStyle != null) ((IStateManager)footerStyle).TrackViewState();
			if (headerStyle != null) ((IStateManager)headerStyle).TrackViewState();
			if (pagerStyle != null) ((IStateManager)pagerStyle).TrackViewState();
			if (rowStyle != null) ((IStateManager)rowStyle).TrackViewState();
			if (editRowStyle != null) ((IStateManager)editRowStyle).TrackViewState();
			if (insertRowStyle != null) ((IStateManager)insertRowStyle).TrackViewState();
			if (emptyDataRowStyle != null) ((IStateManager)emptyDataRowStyle).TrackViewState();
			if (key != null) ((IStateManager)key).TrackViewState();
		}

		protected override object SaveViewState()
		{
			object[] states = new object [14];
			states[0] = base.SaveViewState();
			states[2] = (pagerSettings == null ? null : ((IStateManager)pagerSettings).SaveViewState());
			states[4] = (footerStyle == null ? null : ((IStateManager)footerStyle).SaveViewState());
			states[5] = (headerStyle == null ? null : ((IStateManager)headerStyle).SaveViewState());
			states[6] = (pagerStyle == null ? null : ((IStateManager)pagerStyle).SaveViewState());
			states[7] = (rowStyle == null ? null : ((IStateManager)rowStyle).SaveViewState());
			states[8] = (insertRowStyle == null ? null : ((IStateManager)insertRowStyle).SaveViewState());
			states[9] = (editRowStyle == null ? null : ((IStateManager)editRowStyle).SaveViewState());
			states[10] = (emptyDataRowStyle == null ? null : ((IStateManager)emptyDataRowStyle).SaveViewState());
			states[11] = (key == null ? null : ((IStateManager)key).SaveViewState());
			states[12] = (oldEditValues == null ? null : ((IStateManager)oldEditValues).SaveViewState());
			
			for (int i = states.Length - 1; i >= 0; i--) {
				if (states [i] != null)
					return states;
			}

			return null;
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null) {
				base.LoadViewState (null);
				return;
			}

			object [] states = (object []) savedState;
			
			base.LoadViewState (states[0]);
			EnsureChildControls ();
			
			if (states[2] != null) ((IStateManager)PagerSettings).LoadViewState (states[2]);
			if (states[4] != null) ((IStateManager)FooterStyle).LoadViewState (states[4]);
			if (states[5] != null) ((IStateManager)HeaderStyle).LoadViewState (states[5]);
			if (states[6] != null) ((IStateManager)PagerStyle).LoadViewState (states[6]);
			if (states[7] != null) ((IStateManager)RowStyle).LoadViewState (states[7]);
			if (states[8] != null) ((IStateManager)InsertRowStyle).LoadViewState (states[8]);
			if (states[9] != null) ((IStateManager)EditRowStyle).LoadViewState (states[9]);
			if (states[10] != null) ((IStateManager)EmptyDataRowStyle).LoadViewState (states[10]);
			if (states[11] != null && DataKey != null) ((IStateManager)DataKey).LoadViewState (states[11]);
			if (states[12] != null && oldEditValues != null) ((IStateManager)oldEditValues).LoadViewState (states[12]);
		}
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			PrepareControlHierarchy ();
			
			if (table == null)
				return;

			table.Render (writer);
		}

		PostBackOptions IPostBackContainer.GetPostBackOptions (IButtonControl control)
		{
			if (control == null)
				throw new ArgumentNullException ("control");

			if (control.CausesValidation)
				throw new InvalidOperationException ("A button that causes validation in FormView '" + ID + "' is attempting to use the container GridView as the post back target.  The button should either turn off validation or use itself as the post back container.");

			PostBackOptions options = new PostBackOptions (this);
			options.Argument = control.CommandName + "$" + control.CommandArgument;
			options.RequiresJavaScriptProtocol = true;

			return options;
		}

	}
}

#endif

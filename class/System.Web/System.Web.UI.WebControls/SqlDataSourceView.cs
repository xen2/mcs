//
// System.Web.UI.WebControls.SqlDataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	public class SqlDataSourceView : DataSourceView, IStateManager {

		public SqlDataSourceView (SqlDataSource owner, string name)
		{
			this.owner = owner;
			this.name = name;
		}
	
		public int Delete ()
		{
			return Delete (null);
		}
		
		[MonoTODO]
		public override int Delete (IDictionary parameters)
		{
			throw new NotImplementedException ();
		}
		
		public int Insert ()
		{
			return Insert (null);
		}
		
		[MonoTODO]
		public override int Insert (IDictionary values)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IEnumerable Select ()
		{
			throw new NotImplementedException ();
		}
		
		public int Update ()
		{
			return Update (null, null);
		}
		
		[MonoTODO]
		public override int Update (IDictionary parameters, IDictionary values)
		{
			throw new NotImplementedException ();
		}

		void IStateManager.LoadViewState (object savedState)
		{
			LoadViewState (savedState);
		}
		
		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}
		
		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}
		
		protected virtual void LoadViewState (object savedState)
		{
			object [] vs = savedState as object [];
			if (vs == null)
				return;
			
			if (vs [0] != null) ((IStateManager) deleteParameters).LoadViewState (vs [0]);
			if (vs [1] != null) ((IStateManager) filterParameters).LoadViewState (vs [1]);
			if (vs [2] != null) ((IStateManager) insertParameters).LoadViewState (vs [2]);
			if (vs [3] != null) ((IStateManager) selectParameters).LoadViewState (vs [3]);
			if (vs [4] != null) ((IStateManager) updateParameters).LoadViewState (vs [4]);
			if (vs [5] != null) ((IStateManager) viewState).LoadViewState (vs [5]);
		}

		protected virtual object SaveViewState ()
		{
			object [] vs = new object [6];
			
			if (deleteParameters != null) vs [0] = ((IStateManager) deleteParameters).SaveViewState ();
			if (filterParameters != null) vs [1] = ((IStateManager) filterParameters).SaveViewState ();
			if (insertParameters != null) vs [2] = ((IStateManager) insertParameters).SaveViewState ();
			if (selectParameters != null) vs [3] = ((IStateManager) selectParameters).SaveViewState ();
			if (updateParameters != null) vs [4] = ((IStateManager) updateParameters).SaveViewState ();
			if (viewState != null) vs [5] = ((IStateManager) viewState).SaveViewState ();
				
			foreach (object o in vs)
				if (o != null) return vs;
			return null;
		}
		
		protected virtual void TrackViewState ()
		{
			tracking = true;
			
			if (deleteParameters != null) ((IStateManager) deleteParameters).TrackViewState ();
			if (filterParameters != null) ((IStateManager) filterParameters).TrackViewState ();
			if (insertParameters != null) ((IStateManager) insertParameters).TrackViewState ();
			if (selectParameters != null) ((IStateManager) selectParameters).TrackViewState ();
			if (updateParameters != null) ((IStateManager) updateParameters).TrackViewState ();
			if (viewState != null) ((IStateManager) viewState).TrackViewState ();
		}
		
		protected bool IsTrackingViewState {
			get { return tracking; }
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return IsTrackingViewState; }
		}
		
		public string DeleteCommand {
			get {
				string val = ViewState ["DeleteCommand"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["DeleteCommand"] = value; }
		}
		
		public string FilterExpression {
			get {
				string val = ViewState ["FilterExpression"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["FilterExpression"] = value; }
		}
		
		public string InsertCommand {
			get {
				string val = ViewState ["InsertCommand"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["InsertCommand"] = value; }
		}
		
		public string SelectCommand {
			get {
				string val = ViewState ["SelectCommand"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["SelectCommand"] = value; }
		}
		
		public string UpdateCommand {
			get {
				string val = ViewState ["UpdateCommand"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["UpdateCommand"] = value; }
		}
		
		public override string SortExpression {
			get {
				string val = ViewState ["SortExpression"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["SortExpression"] = value; }
		}
		
		public override bool CanDelete {
			get { return DeleteCommand != ""; }
		}
		
		public override bool CanInsert {
			get { return UpdateCommand != ""; }
		}
		
		public override bool CanSort {
			get { return owner.DataSourceMode == SqlDataSourceMode.DataSet; }
		}
		
		public override bool CanUpdate {
			get { return UpdateCommand != ""; }
		}
	
		EventHandlerList events;
		protected EventHandlerList Events {
			get {
				if (events == null)
					events = new EventHandlerList ();
				
				return events;
			}
		}

		void ParametersChanged (object source, EventArgs args)
		{
			OnDataSourceViewChanged (EventArgs.Empty);
		}
		
		ParameterCollection GetParameterCollection (ref ParameterCollection output)
		{
			if (output != null)
				return output;
			
			output = new ParameterCollection ();
			output.ParametersChanged += new EventHandler (ParametersChanged);
			
			if (IsTrackingViewState)
				((IStateManager) output).TrackViewState ();
			
			return output;
		}
		
		public ParameterCollection DeleteParameters {
			get { return GetParameterCollection (ref deleteParameters); }
		}
		
		public ParameterCollection FilterParameters {
			get { return GetParameterCollection (ref filterParameters); }
		}
		
		public ParameterCollection InsertParameters {
			get { return GetParameterCollection (ref insertParameters); }
		}
		
		public ParameterCollection SelectParameters {
			get { return GetParameterCollection (ref selectParameters); }
		}
		
		public ParameterCollection UpdateParameters {
			get { return GetParameterCollection (ref updateParameters); }
		}
		
		
		public override string Name {
			get { return name; }
		}
		
		protected virtual string ParameterPrefix {
			get { return "@"; }
		}

		StateBag viewState;
		protected StateBag ViewState {
			get {
				if (viewState != null)
					return viewState;
				
				viewState = new StateBag ();
				if (IsTrackingViewState)
					viewState.TrackViewState ();
				
				return viewState;
			}
		}
			
		ParameterCollection deleteParameters;
		ParameterCollection filterParameters;
		ParameterCollection insertParameters;
		ParameterCollection selectParameters;
		ParameterCollection updateParameters;

		bool tracking;
	
		string name;
		SqlDataSource owner;
		
		static readonly object EventDataSourceViewChanged = new object ();
				
		protected virtual void OnDataSourceViewChanged (EventArgs e)
		{
			if (events == null) return;
			EventHandler h = events [EventDataSourceViewChanged] as EventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event EventHandler DataSourceViewChanged {
			add { Events.AddHandler (EventDataSourceViewChanged, value); }
			remove { Events.RemoveHandler (EventDataSourceViewChanged, value); }
		}

		#region OnDelete
		static readonly object EventDeleted = new object ();
		protected virtual void OnDeleted (SqlDataSourceStatusEventArgs e)
		{
			if (events == null) return;
			SqlDataSourceStatusEventHandler h = events [EventDeleted] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Deleted {
			add { Events.AddHandler (EventDeleted, value); }
			remove { Events.RemoveHandler (EventDeleted, value); }
		}
		
		static readonly object EventDeleting = new object ();
		protected virtual void OnDeleting (SqlDataSourceCommandEventArgs e)
		{
			if (events == null) return;
			SqlDataSourceCommandEventHandler h = events [EventDeleting] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Deleting {
			add { Events.AddHandler (EventDeleting, value); }
			remove { Events.RemoveHandler (EventDeleting, value); }
		}
		#endregion
		
		#region OnInsert
		static readonly object EventInserted = new object ();
		protected virtual void OnInserted (SqlDataSourceStatusEventArgs e)
		{
			if (events == null) return;
			SqlDataSourceStatusEventHandler h = events [EventInserted] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Inserted {
			add { Events.AddHandler (EventInserted, value); }
			remove { Events.RemoveHandler (EventInserted, value); }
		}
		
		static readonly object EventInserting = new object ();
		protected virtual void OnInserting (SqlDataSourceCommandEventArgs e)
		{
			if (events == null) return;
			SqlDataSourceCommandEventHandler h = events [EventInserting] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Inserting {
			add { Events.AddHandler (EventInserting, value); }
			remove { Events.RemoveHandler (EventInserting, value); }
		}
		#endregion
		
		#region OnSelect
		static readonly object EventSelected = new object ();
		protected virtual void OnSelected (SqlDataSourceStatusEventArgs e)
		{
			if (events == null) return;
			SqlDataSourceStatusEventHandler h = events [EventSelected] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Selected {
			add { Events.AddHandler (EventSelected, value); }
			remove { Events.RemoveHandler (EventSelected, value); }
		}
		
		static readonly object EventSelecting = new object ();
		protected virtual void OnSelecting (SqlDataSourceCommandEventArgs e)
		{
			if (events == null) return;
			SqlDataSourceCommandEventHandler h = events [EventSelecting] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Selecting {
			add { Events.AddHandler (EventSelecting, value); }
			remove { Events.RemoveHandler (EventSelecting, value); }
		}
		#endregion
		
		#region OnUpdate
		static readonly object EventUpdated = new object ();
		protected virtual void OnUpdated (SqlDataSourceStatusEventArgs e)
		{
			if (events == null) return;
			SqlDataSourceStatusEventHandler h = events [EventUpdated] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Updated {
			add { Events.AddHandler (EventUpdated, value); }
			remove { Events.RemoveHandler (EventUpdated, value); }
		}
		
		static readonly object EventUpdating = new object ();
		protected virtual void OnUpdating (SqlDataSourceCommandEventArgs e)
		{
			if (events == null) return;
			SqlDataSourceCommandEventHandler h = events [EventUpdating] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Updating {
			add { Events.AddHandler (EventUpdating, value); }
			remove { Events.RemoveHandler (EventUpdating, value); }
		}
		#endregion
	}
	
}
#endif


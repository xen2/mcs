//
// System.Data.SqlClient.SqlCommand.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002 http://www.ximian.com/
// (C) Daniel Morgan, 2002
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace System.Data.SqlClient {
	public sealed class SqlCommand : Component, IDbCommand, ICloneable
	{
		#region Fields

		int commandTimeout;
		bool designTimeVisible;
		string commandText;

		CommandType commandType;
		SqlConnection connection;
		SqlTransaction transaction;
		SqlParameterCollection parameters = new SqlParameterCollection ();

		CommandBehavior behavior = CommandBehavior.Default;
		NameValueCollection preparedStatements = new NameValueCollection ();

		#endregion // Fields

		#region Constructors

		public SqlCommand() 
			: this (String.Empty, null, null)
		{
		}

		public SqlCommand (string commandText) 
			: this (commandText, null, null)
		{
			commandText = commandText;
		}

		public SqlCommand (string commandText, SqlConnection connection) 
			: this (commandText, connection, null)
		{
			Connection = connection;
		}

		public SqlCommand (string commandText, SqlConnection connection, SqlTransaction transaction) 
		{
			this.commandText = commandText;
			this.connection = connection;
			this.transaction = transaction;
			this.commandType = CommandType.Text;

			this.designTimeVisible = false;
			this.commandTimeout = 30;
		}

		#endregion // Constructors

		#region Properties

		internal CommandBehavior CommandBehavior {
			get { return behavior; }
		}

		public string CommandText {
			get { return CommandText; }
			set { commandText = value; }
		}

		public int CommandTimeout {
			get { return commandTimeout;  }
			set { 
				if (commandTimeout < 0)
					throw new ArgumentException ("The property value assigned is less than 0.");
				commandTimeout = value; 
			}
		}

		public CommandType CommandType	{
			get { return commandType; }
			set { commandType = value; }
		}

		public SqlConnection Connection {
			get { return connection; }
			set { 
				if (transaction != null && connection.Transaction != null && connection.Transaction.IsOpen)
					throw new InvalidOperationException ("The Connection property was changed while a transaction was in progress.");
				transaction = null;
				connection = value; 
			}
		}

		public bool DesignTimeVisible {
			get { return designTimeVisible; } 
			set { designTimeVisible = value; }
		}

		public SqlParameterCollection Parameters {
			get { return parameters; }
		}

		internal ITds Tds {
			get { return connection.Tds; }
		}

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { 
				if (!(value is SqlConnection))
					throw new InvalidCastException ("The value was not a valid SqlConnection.");
				Connection = (SqlConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters	{
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set { 
				if (!(value is SqlTransaction))
					throw new ArgumentException ();
				Transaction = (SqlTransaction) value; 
			}
		}

		public SqlTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}	

		[MonoTODO]
		public UpdateRowSource UpdatedRowSource	{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Fields

		#region Methods

		private string BuildCommand ()
		{
			string statementHandle = preparedStatements [commandText];
			if (statementHandle != null) {
				string proc = String.Format ("sp_execute {0}", statementHandle);	
				if (parameters.Count > 0)
					proc += ",";
				return BuildProcedureCall (proc, parameters);
			}

			if (commandType == CommandType.StoredProcedure)
				return BuildProcedureCall (commandText, parameters);

			if ((behavior & CommandBehavior.KeyInfo) > 0)
				commandText += " FOR BROWSE";

			if (commandType == CommandType.Text)
				return BuildExec ();

			if (commandType == CommandType.TableDirect)
				return BuildExec (String.Format ("select * from {0}", commandText));

			throw new InvalidOperationException ("The CommandType was invalid.");
		}

		private string BuildExec ()
		{
			return BuildExec (commandText);	
		}
	
		private string BuildExec (string sql)
		{
			StringBuilder parms = new StringBuilder ();
			foreach (SqlParameter parameter in parameters) {
				if (parms.Length > 0)
					parms.Append (", ");
				parms.Append (parameter.Prepare (parameter.ParameterName));
				if (parameter.Direction == ParameterDirection.Output)
					parms.Append (" output");
			}

			SqlParameterCollection localParameters = new SqlParameterCollection ();
			SqlParameter parm;
		
			parm = new SqlParameter ("@P1", SqlDbType.NVarChar);
			parm.Value = sql;
			parm.Size = ((string) parm.Value).Length;
			localParameters.Add (parm);

			if (parameters.Count > 0) {
				parm = new SqlParameter ("@P2", SqlDbType.NVarChar);
				parm.Value = parms.ToString ();
				parm.Size = ((string) parm.Value).Length;
				localParameters.Add (parm);
			}

			foreach (SqlParameter p in parameters)
				localParameters.Add (p);

			return BuildProcedureCall ("sp_executesql", localParameters);
		}

		private string BuildPrepare ()
		{
			StringBuilder parms = new StringBuilder ();
			foreach (SqlParameter parameter in parameters) {
				if (parms.Length > 0)
					parms.Append (", ");
				parms.Append (parameter.Prepare (parameter.ParameterName));
				if (parameter.Direction == ParameterDirection.Output)
					parms.Append (" output");
			}

			SqlParameterCollection localParameters = new SqlParameterCollection ();
			SqlParameter parm;
		
			parm = new SqlParameter ("@P1", SqlDbType.Int);
			parm.Direction = ParameterDirection.Output;
			localParameters.Add (parm);

			parm = new SqlParameter ("@P2", SqlDbType.NVarChar);
			parm.Value = parms.ToString ();
			parm.Size = ((string) parm.Value).Length;
			localParameters.Add (parm);

			parm = new SqlParameter ("@P3", SqlDbType.NVarChar);
			parm.Value = commandText;
			parm.Size = ((string) parm.Value).Length;
			localParameters.Add (parm);

			return BuildProcedureCall ("sp_prepare", localParameters);
		}

		private static string BuildProcedureCall (string procedure, SqlParameterCollection parameters)
		{
			StringBuilder parms = new StringBuilder ();
			StringBuilder declarations = new StringBuilder ();
			StringBuilder outParms = new StringBuilder ();
			StringBuilder set = new StringBuilder ();

			int index = 1;
			foreach (SqlParameter parameter in parameters) {
				string parmName = String.Format ("@P{0}", index);

				switch (parameter.Direction) {
				case ParameterDirection.Input :
					if (parms.Length > 0)
						parms.Append (", ");
					parms.Append (FormatParameter (parameter));
					break;
				case ParameterDirection.Output :
					if (parms.Length > 0)
						parms.Append (", ");
					parms.Append (parmName);
					parms.Append (" output");

					if (outParms.Length > 0) {
						outParms.Append (", ");
						declarations.Append (", ");
					}
					else {
						outParms.Append ("select ");
						declarations.Append ("declare ");
					}

					declarations.Append (parameter.Prepare (parmName));
					set.Append (String.Format ("set {0}=NULL\n", parmName));
					outParms.Append (parmName);
					break;
				default :
					throw new NotImplementedException ("Only support input and output parameters.");
				}
				index += 1;
			}

			return String.Format ("{0}\n{1}exec {2} {3}\n{4}", declarations.ToString (), set.ToString (), procedure, parms.ToString (), outParms.ToString ());
		}

		public void Cancel () 
		{
			if (connection == null || connection.Tds == null)
				return;
			connection.Tds.Cancel ();
			connection.CheckForErrors ();
		}

		internal void CloseDataReader (bool moreResults)
		{
			GetOutputParameters ();
			connection.DataReader = null;

			if ((behavior & CommandBehavior.CloseConnection) != 0)
				connection.Close ();
		}

		public SqlParameter CreateParameter () 
		{
			return new SqlParameter ();
		}

		public int ExecuteNonQuery ()
		{
			ValidateCommand ("ExecuteNonQuery");
			int result = connection.Tds.ExecuteNonQuery (BuildCommand ());
			connection.CheckForErrors ();
			GetOutputParameters ();
			return result;
		}

		public SqlDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public SqlDataReader ExecuteReader (CommandBehavior behavior)
		{
			ValidateCommand ("ExecuteReader");
			this.behavior = behavior;
			connection.Tds.ExecuteQuery (BuildCommand ());
			connection.CheckForErrors ();
			connection.DataReader = new SqlDataReader (this);

			return connection.DataReader;
		}

		public object ExecuteScalar ()
		{
			ValidateCommand ("ExecuteScalar");
			connection.Tds.ExecuteQuery (BuildCommand ());
			connection.CheckForErrors ();

			bool moreResults = connection.Tds.NextResult ();
			connection.CheckForErrors ();

			if (!moreResults)
				return null;

			moreResults = connection.Tds.NextRow ();
			connection.CheckForErrors ();

			if (!moreResults)
				return null;

			object result = connection.Tds.ColumnValues[0];
			CloseDataReader (true);
			return result;
		}

		public XmlReader ExecuteXmlReader ()
		{
			ValidateCommand ("ExecuteXmlReader");
			connection.Tds.ExecuteQuery (BuildCommand ());
			connection.CheckForErrors ();

			SqlDataReader dataReader = new SqlDataReader (this);
			SqlXmlTextReader textReader = new SqlXmlTextReader (dataReader);
			XmlReader xmlReader = new XmlTextReader (textReader);
			return xmlReader;
		}

		static string FormatParameter (SqlParameter parameter)
		{
			if (parameter.Value == null)
				return "NULL";

			switch (parameter.SqlDbType) {
				case SqlDbType.BigInt :
				case SqlDbType.Bit :
				case SqlDbType.Decimal :
				case SqlDbType.Float :
				case SqlDbType.Int :
				case SqlDbType.Money :
				case SqlDbType.Real :
				case SqlDbType.SmallInt :
				case SqlDbType.SmallMoney :
				case SqlDbType.TinyInt :
					return parameter.Value.ToString ();
				case SqlDbType.NVarChar :
				case SqlDbType.NChar :
					return String.Format ("N'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
				default:
					return String.Format ("'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
			}
		}

		private void GetOutputParameters ()
		{
			connection.Tds.SkipToEnd ();

			IList list = connection.Tds.ColumnValues;

			if (list != null && list.Count > 0) {
				int index = 0;
				foreach (SqlParameter parameter in parameters) {
					if (parameter.Direction != ParameterDirection.Input) {
						parameter.Value = list [index];
						index += 1;
					}
					if (index >= list.Count)
						break;
				}
			}
		}

		object ICloneable.Clone ()
		{
			return new SqlCommand (commandText, connection);
		}

		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
		}

		public void Prepare ()
		{
			ValidateCommand ("Prepare");
			connection.Tds.ExecuteNonQuery (BuildPrepare ());
			connection.CheckForErrors ();

			if (connection.Tds.OutputParameters.Count == 0 || connection.Tds.OutputParameters[0] == null)
				throw new Exception ("Could not prepare the statement.");

			preparedStatements [commandText] = ((int) connection.Tds.OutputParameters [0]).ToString ();
		}

		public void ResetCommandTimeout ()
		{
			commandTimeout = 30;
		}

		private void ValidateCommand (string method)
		{
			if (connection == null)
				throw new InvalidOperationException (String.Format ("{0} requires a Connection object to continue.", method));
			if (connection.Transaction != null && transaction != connection.Transaction)
				throw new InvalidOperationException ("The Connection object does not have the same transaction as the command object.");
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException (String.Format ("ExecuteNonQuery requires an open Connection object to continue. This connection is closed.", method));
			if (commandText == String.Empty || commandText == null)
				throw new InvalidOperationException ("The command text for this Command has not been set.");
			if (connection.DataReader != null)
				throw new InvalidOperationException ("There is already an open DataReader associated with this Connection which must be closed first.");
			if (connection.XmlReader != null)
				throw new InvalidOperationException ("There is already an open XmlReader associated with this Connection which must be closed first.");
		}

		#endregion // Methods
	}
}

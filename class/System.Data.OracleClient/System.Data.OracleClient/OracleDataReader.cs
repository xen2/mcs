//
// OracleDataReader.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: Tim Coleman <tim@timcoleman.com>
//          Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient.Oci;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient {
	public sealed class OracleDataReader : MarshalByRefObject, IDataReader, IDisposable, IDataRecord, IEnumerable
	{
		OracleCommand command;
		ArrayList dataTypeNames;
		bool disposed = false;
		int fieldCount;
		bool isClosed;
		bool isSelect;
		bool hasRows;
		bool moreResults;
		DataTable schemaTable;

		internal OracleDataReader (OracleCommand command)
		{
			this.command = command;
			this.hasRows = false;
			this.isClosed = false;
			this.isSelect = (command.CommandText.Trim ().ToUpper ().StartsWith ("SELECT"));
			this.schemaTable = ConstructSchemaTable ();
			this.fieldCount = command.StatementHandle.ColumnCount;
			Read ();
		}

		public int Depth {
			get { return 0; }
		}

		public int FieldCount {
			get { return fieldCount; }
		}

		public bool HasRows {
			get { return hasRows; }
		}

		public bool IsClosed {
			get { return isClosed; }
		}

		public object this [string name] {
			get { return GetValue (GetOrdinal (name)); }
		}

		public object this [int i] {
			get { return GetValue (i); }
		}

		public int RecordsAffected {
			get { 
				// FIXME: get RecordsAffected for DML, otherwise, -1
				return -1;
				//if (isSelect) 
				//	return -1;
				//else
				//	throw new NotImplementedException ();
			}
		}

		public void Close ()
		{
			isClosed = true;
			command.CloseDataReader ();
		}

		private static DataTable ConstructSchemaTable ()
		{
			Type booleanType = Type.GetType ("System.Boolean");
			Type stringType = Type.GetType ("System.String");
			Type intType = Type.GetType ("System.Int32");
			Type typeType = Type.GetType ("System.Type");
			Type shortType = Type.GetType ("System.Int16");

			DataTable schemaTable = new DataTable ("SchemaTable");
			schemaTable.Columns.Add ("ColumnName", stringType);
			schemaTable.Columns.Add ("ColumnOrdinal", intType);
			schemaTable.Columns.Add ("ColumnSize", intType);
			schemaTable.Columns.Add ("NumericPrecision", shortType);
			schemaTable.Columns.Add ("NumericScale", shortType);
			schemaTable.Columns.Add ("DataType", typeType);
			schemaTable.Columns.Add ("IsLong", booleanType);
			schemaTable.Columns.Add ("AllowDBNull", booleanType);
			schemaTable.Columns.Add ("IsUnique", booleanType);
			schemaTable.Columns.Add ("IsKey", booleanType);
			schemaTable.Columns.Add ("BaseSchemaTable", stringType);
			schemaTable.Columns.Add ("BaseCatalogName", stringType);
			schemaTable.Columns.Add ("BaseTableName", stringType);
			schemaTable.Columns.Add ("BaseColumnName", stringType);
			schemaTable.Columns.Add ("BaseSchemaName", stringType);

			return schemaTable;
		}

		private void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					schemaTable.Dispose ();
					Close ();
				}
				disposed = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public bool GetBoolean (int i)
		{
			object value = GetValue (i);
			if (!(value is bool))
				throw new InvalidCastException ();
			return (bool) value;
		}

		public byte GetByte (int i)
		{
			object value = GetValue (i);
			if (!(value is byte))
				throw new InvalidCastException ();
			return (byte) value;
		}

		public long GetBytes (int i, long fieldOffset, byte[] buffer2, int bufferoffset, int length)
		{
			object value = GetValue (i);
			if (!(value is byte[]))
				throw new InvalidCastException ();
			Array.Copy ((byte[]) value, (int) fieldOffset, buffer2, bufferoffset, length);
			return ((byte[]) value).Length - fieldOffset;
		}

		public char GetChar (int i)
		{
			object value = GetValue (i);
			if (!(value is char))
				throw new InvalidCastException ();
			return (char) value;
		}

		public long GetChars (int i, long fieldOffset, char[] buffer2, int bufferoffset, int length)
		{
			object value = GetValue (i);
			if (!(value is char[]))
				throw new InvalidCastException ();
			Array.Copy ((char[]) value, (int) fieldOffset, buffer2, bufferoffset, length);
			return ((char[]) value).Length - fieldOffset;
		}

		[MonoTODO]
		public IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		public string GetDataTypeName (int i)
		{
			return (string) dataTypeNames [i];
		}

		public DateTime GetDateTime (int i)
		{
			object value = GetValue (i);
			if (!(value is DateTime))
				throw new InvalidCastException ();
			return (DateTime) value;
		}

		public decimal GetDecimal (int i)
		{
			object value = GetValue (i);
			if (!(value is decimal))
				throw new InvalidCastException ();
			return (decimal) value;
		}

		public double GetDouble (int i)
		{
			object value = GetValue (i);
			if (!(value is double))
				throw new InvalidCastException ();
			return (double) value;
		}

		public Type GetFieldType (int i)
		{
			// FIXME: "DataType" need to implement
			//OciColumnInfo columnInfo = command.StatementHandle.DescribeColumn (i);
			//Type fieldType = OciGlue.OciDataTypeToDbType (columnInfo.DataType);
			//return fieldType;
			return typeof(string);
		}

		public float GetFloat (int i)
		{
			object value = GetValue (i);
			if (!(value is float))
				throw new InvalidCastException ();
			return (float) value;
		}

		public Guid GetGuid (int i)
		{
			object value = GetValue (i);
			if (!(value is Guid))
				throw new InvalidCastException ();
			return (Guid) value;
		}

		public short GetInt16 (int i)
		{
			object value = GetValue (i);
			if (!(value is short))
				throw new InvalidCastException ();
			return (short) value;
		}

		public int GetInt32 (int i)
		{
			object value = GetValue (i);
			if (!(value is int))
				throw new InvalidCastException ();
			return (int) value;
		}

		public long GetInt64 (int i)
		{
			object value = GetValue (i);
			if (!(value is long))
				throw new InvalidCastException ();
			return (long) value;
		}

		public string GetName (int i)
		{
			OciColumnInfo columnInfo = command.StatementHandle.DescribeColumn (i);
			return columnInfo.ColumnName;
		}

		public int GetOrdinal (string name)
		{
			foreach (DataRow schemaRow in schemaTable.Rows)
				if (((string) schemaRow ["ColumnName"]).Equals (name))
					return (int) schemaRow ["ColumnOrdinal"];
			foreach (DataRow schemaRow in schemaTable.Rows)
				if (String.Compare (((string) schemaRow ["ColumnName"]), name, true) == 0)
					return (int) schemaRow ["ColumnOrdinal"];
			throw new IndexOutOfRangeException ();
		}

		public DataTable GetSchemaTable ()
		{
			if (schemaTable.Rows != null && schemaTable.Rows.Count > 0)
				return schemaTable;
			
			dataTypeNames = new ArrayList ();

			for (int i = 0; i < command.StatementHandle.ColumnCount; i += 1) {
				DataRow row = schemaTable.NewRow ();
				OciColumnInfo columnInfo = command.StatementHandle.DescribeColumn (i);

				row ["ColumnName"] = columnInfo.ColumnName;
				row ["ColumnOrdinal"] = i + 1;
				row ["ColumnSize"] = (int) columnInfo.ColumnSize;
				row ["NumericPrecision"] = (short) columnInfo.Precision;
				row ["NumericScale"] = (short) columnInfo.Scale;
				// FIXME: "DataType" need to implement
				//row ["DataType"] = OciGlue.OciDataTypeToDbType (columnInfo.DataType);
				row ["DataType"] = typeof(string);
				row ["AllowDBNull"] = columnInfo.AllowDBNull;
				row ["BaseColumnName"] = columnInfo.BaseColumnName;

				schemaTable.Rows.Add (row);
			}

			return schemaTable;
		}

		public string GetString (int i)
		{
			object value = GetValue (i);
			if (!(value is string))
				throw new InvalidCastException ();
			return (string) value;
		}

		public TimeSpan GetTimeSpan (int i)
		{
			object value = GetValue (i);
			if (!(value is TimeSpan))
				throw new InvalidCastException ();
			return (TimeSpan) value;
		}

		[MonoTODO]
		public object GetValue (int i)
		{
			OciDefineHandle defineHandle = (OciDefineHandle) command.StatementHandle.Values [i];
			object tmp;

			if (defineHandle.IsNull)
				return DBNull.Value;

			switch (defineHandle.DataType) {
			case OciDataType.VarChar2:
			case OciDataType.String:
			case OciDataType.VarChar:
			case OciDataType.Char:
			case OciDataType.CharZ:
			case OciDataType.OciString:
				tmp = Marshal.PtrToStringAnsi (defineHandle.Value, defineHandle.Size);
				if (tmp != null)
					return String.Copy ((string) tmp);
				break;
			case OciDataType.Integer:
				tmp = Marshal.PtrToStringAnsi (defineHandle.Value, defineHandle.Size);
				if (tmp != null) 
					return Int32.Parse (String.Copy ((string) tmp));
				break;
			case OciDataType.Number:
				tmp = Marshal.PtrToStringAnsi (defineHandle.Value, defineHandle.Size);
				if (tmp != null) {
					if (defineHandle.Scale == 0) 
						return Int32.Parse (String.Copy ((string) tmp));
					else
						return Decimal.Parse (String.Copy ((string) tmp));
				}
				break;
			case OciDataType.Float:
				tmp = Marshal.PtrToStringAnsi (defineHandle.Value, defineHandle.Size);
				if (tmp != null) 
					return Double.Parse (String.Copy ((string) tmp));
				break;
			case OciDataType.Date:
				tmp = Marshal.PtrToStringAnsi (defineHandle.Value, defineHandle.Size);
				Console.WriteLine ((string) tmp);
				if (tmp != null)
					return DateTime.Parse ((string) tmp);
				break;
			}

			return DBNull.Value;
		}

		[MonoTODO]
		public int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new DbEnumerator (this);
		}

		public bool IsDBNull (int i)
		{
			return GetValue (i) == null;
		}

		[MonoTODO]
		public bool NextResult ()
		{
			// FIXME: get next result
			//throw new NotImplementedException ();
			return false; 
		}

		public bool Read ()
		{
			return command.StatementHandle.Fetch ();
		}
	}
}

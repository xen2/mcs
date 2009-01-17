// SqlCommandBuilderTest.cs - NUnit Test Cases for testing the
// SqlCommandBuilder class
//
// Authors:
//      Sureshkumar T (tsureshkumar@novell.com)
// 
// Copyright (c) 2004 Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
#if NET_2_0
using System.Data.SqlTypes;
#endif
using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("sqlserver")]
	public class SqlCommandBuilderTest
	{
		[Test]
		public void GetInsertCommand1 ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, fname, lname " +
					"from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				SqlCommandBuilder cb;
				
				cb = new SqlCommandBuilder (da);
				cmd = cb.GetInsertCommand ();
#if NET_2_0
				Assert.AreEqual ("INSERT INTO [employee] ([id], " +
					"[fname], [lname]) VALUES (@p1, @p2, @p3)",
					cmd.CommandText, "#A1");
#else
				Assert.AreEqual ("INSERT INTO employee( id , " +
					"fname , lname ) VALUES ( @p1 , @p2 , @p3 )",
					cmd.CommandText, "#A1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#A2");
				AssertInsertParameters (cmd, false, "#A3:");
				Assert.AreSame (cmd, cb.GetInsertCommand (), "#A4");

				cb.RefreshSchema ();
				cb.QuotePrefix = "\"";
				cmd = cb.GetInsertCommand ();
#if NET_2_0
				Assert.AreEqual ("INSERT INTO \"employee] (\"id], " +
					"\"fname], \"lname]) VALUES (@p1, @p2, @p3)",
					cmd.CommandText, "#B1");
#else
				Assert.AreEqual ("INSERT INTO \"employee( \"id , " +
					"\"fname , \"lname ) VALUES ( @p1 , @p2 , @p3 )",
					cmd.CommandText, "#B1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#B2");
				AssertInsertParameters (cmd, false, "#B3:");
				Assert.AreSame (cmd, cb.GetInsertCommand (), "#B4");

				cb.RefreshSchema ();
#if NET_2_0
				cb.QuoteSuffix = "\"";
#else
				cb.QuoteSuffix = "�";
#endif
				cmd = cb.GetInsertCommand ();
#if NET_2_0
				Assert.AreEqual ("INSERT INTO \"employee\" (\"id\", "
					+ "\"fname\", \"lname\") VALUES (@p1, @p2, @p3)",
					cmd.CommandText, "#C1");
#else
				Assert.AreEqual ("INSERT INTO \"employee�( \"id� , " +
					"\"fname� , \"lname� ) VALUES ( @p1 , @p2 , @p3 )",
					cmd.CommandText, "#C1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#C2");
				AssertInsertParameters (cmd, false, "#C3");
				Assert.AreSame (cmd, cb.GetInsertCommand (), "#C4");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetInsertCommand1_Expression ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, fname, lname, " +
					"id+1 as next_id from employee where " +
					"id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetInsertCommand ();
#if NET_2_0
				Assert.AreEqual ("INSERT INTO [employee] " +
					"([id], [fname], [lname]) VALUES " +
					"(@p1, @p2, @p3)", cmd.CommandText, "#1");
#else
				Assert.AreEqual ("INSERT INTO employee( id , " +
					"fname , lname ) VALUES ( @p1 , @p2 , " +
					"@p3 )", cmd.CommandText, "#1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#2");
				AssertInsertParameters (cmd, false, "#3:");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

#if NET_2_0
		[Test] // GetInsertCommand (Boolean)
		public void GetInsertCommand2 ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, fname, lname " +
					"from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				SqlCommandBuilder cb;

				cb = new SqlCommandBuilder (da);
				cmd = cb.GetInsertCommand (true);
				Assert.AreEqual ("INSERT INTO [employee] ([id], " +
					"[fname], [lname]) VALUES (@id, @fname, " +
					"@lname)", cmd.CommandText, "#A1");
				Assert.AreSame (conn, cmd.Connection, "#A2");
				AssertInsertParameters (cmd, true, "#A3:");

				cmd = cb.GetInsertCommand (false);
				Assert.AreEqual ("INSERT INTO [employee] ([id], " +
					"[fname], [lname]) VALUES (@id, @fname, " +
					"@lname)", cmd.CommandText, "#B1");
				Assert.AreSame (conn, cmd.Connection, "#B2");
				AssertInsertParameters (cmd, true, "#B3:");

				cb = new SqlCommandBuilder (da);
				cmd = cb.GetInsertCommand (false);
				Assert.AreEqual ("INSERT INTO [employee] ([id], " +
					"[fname], [lname]) VALUES (@p1, @p2, @p3)",
					cmd.CommandText, "#C1");
				Assert.AreSame (conn, cmd.Connection, "#C2");
				AssertInsertParameters (cmd, false, "#C3:");

				cmd = cb.GetInsertCommand (true);
				Assert.AreEqual ("INSERT INTO [employee] ([id], " +
					"[fname], [lname]) VALUES (@id, @fname, " +
					"@lname)", cmd.CommandText, "#D1");
				Assert.AreSame (conn, cmd.Connection, "#D2");
				AssertInsertParameters (cmd, true, "#D3:");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
#endif

		[Test] // GetUpdateCommand ()
		public void GetUpdateCommand1 ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, fname, lname, " +
					"id+1 as next_id from employee where " +
					"id = 3 and lname = 'A' and fname = 'B'";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetUpdateCommand ();
#if NET_2_0
				Assert.AreEqual ("UPDATE [employee] SET [id] = @p1, " +
					"[fname] = @p2, [lname] = @p3 WHERE (([id] = @p4) " +
					"AND ([fname] = @p5) AND ((@p6 = 1 " +
					"AND [lname] IS NULL) OR ([lname] = @p7)))",
					cmd.CommandText, "#A1");
#else
				Assert.AreEqual ("UPDATE employee SET id = @p1 , " +
					"fname = @p2 , lname = @p3 WHERE ( (id = @p4) " +
					"AND ((@p5 = 1 AND fname IS NULL) OR " +
					"(fname = @p6)) AND ((@p7 = 1 AND " +
					"lname IS NULL) OR (lname = @p8)) )",
					cmd.CommandText, "#A1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#A2");
				AssertUpdateParameters (cmd, false, "#A3:");
				Assert.AreSame (cmd, cb.GetUpdateCommand (), "#A4");

				cb.RefreshSchema ();
				cb.QuotePrefix = "\"";
				cmd = cb.GetUpdateCommand ();
#if NET_2_0
				Assert.AreEqual ("UPDATE \"employee] SET \"id] = @p1, " +
					"\"fname] = @p2, \"lname] = @p3 WHERE ((\"id] = @p4) " +
					"AND (\"fname] = @p5) AND ((@p6 = 1 " +
					"AND \"lname] IS NULL) OR (\"lname] = @p7)))",
					cmd.CommandText, "#B1");
#else
				Assert.AreEqual ("UPDATE \"employee SET \"id = @p1 , " +
					"\"fname = @p2 , \"lname = @p3 WHERE ( (\"id = @p4) " +
					"AND ((@p5 = 1 AND \"fname IS NULL) OR " +
					"(\"fname = @p6)) AND ((@p7 = 1 AND " +
					"\"lname IS NULL) OR (\"lname = @p8)) )",
					cmd.CommandText, "#B1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#B2");
				AssertUpdateParameters (cmd, false, "#B3:");
				Assert.AreSame (cmd, cb.GetUpdateCommand (), "#B4");

				cb.RefreshSchema ();
#if NET_2_0
				cb.QuoteSuffix = "\"";
#else
				cb.QuoteSuffix = "�";
#endif
				cmd = cb.GetUpdateCommand ();
#if NET_2_0
				Assert.AreEqual ("UPDATE \"employee\" SET \"id\" = @p1, " +
					"\"fname\" = @p2, \"lname\" = @p3 WHERE ((\"id\" = @p4) " +
					"AND (\"fname\" = @p5) AND ((@p6 = 1 " +
					"AND \"lname\" IS NULL) OR (\"lname\" = @p7)))",
					cmd.CommandText, "#C1");
#else
				Assert.AreEqual ("UPDATE \"employee� SET \"id� = @p1 , " +
					"\"fname� = @p2 , \"lname� = @p3 WHERE ( (\"id� = @p4) " +
					"AND ((@p5 = 1 AND \"fname� IS NULL) OR " +
					"(\"fname� = @p6)) AND ((@p7 = 1 AND " +
					"\"lname� IS NULL) OR (\"lname� = @p8)) )",
					cmd.CommandText, "#C1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#C2");
				AssertUpdateParameters (cmd, false, "#C3:");
				Assert.AreSame (cmd, cb.GetUpdateCommand (), "#C4");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test] // GetUpdateCommand ()
		public void GetUpdateCommand1_AutoIncrement ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = "create table #tmp_table (id int primary key , counter int identity(1,1), value varchar(10))";
				cmd.ExecuteNonQuery ();

				string selectQuery = "select id, counter, value, id+1 as next_id from #tmp_table";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds);
				Assert.AreEqual (1, ds.Tables.Count);
				Assert.AreEqual (4, ds.Tables [0].Columns.Count);

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetUpdateCommand ();
#if NET_2_0
				Assert.AreEqual ("UPDATE [#tmp_table] SET [id] = @p1, " +
					"[value] = @p2 WHERE (([id] = @p3) AND (" +
					"[counter] = @p4) AND ((@p5 = 1 AND [value] IS NULL) " +
					"OR ([value] = @p6)))", cmd.CommandText, "#1");
				Assert.AreEqual (6, cmd.Parameters.Count, "#2");
#else
				Assert.AreEqual ("UPDATE #tmp_table SET id = @p1 , " +
					"value = @p2 WHERE ( (id = @p3) AND (" +
					"(@p4 = 1 AND counter IS NULL) OR (counter = @p5)) " +
					"AND ((@p6 = 1 AND value IS NULL) OR (value = @p7)) )",
					cmd.CommandText, "#1");
				Assert.AreEqual (7, cmd.Parameters.Count, "#2");
#endif
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test] // GetUpdateCommand ()
		public void GetUpdateCommand1_CheckParameters ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				SqlDataAdapter adapter = new SqlDataAdapter (
					"select id, type_varchar from string_family",
					conn);
				SqlCommandBuilder cb = new SqlCommandBuilder (adapter);

				cmd = cb.GetUpdateCommand ();
				Assert.AreEqual (5, cmd.Parameters.Count, "#1");
				Assert.AreEqual (SqlDbType.Int, cmd.Parameters ["@p4"].SqlDbType, "#2");
				Assert.AreEqual (1, cmd.Parameters ["@p4"].Value, "#3");

				cmd = cb.GetDeleteCommand ();
				Assert.AreEqual (3, cmd.Parameters.Count, "#4");
				Assert.AreEqual (SqlDbType.Int, cmd.Parameters ["@p2"].SqlDbType, "#5");
				Assert.AreEqual (1, cmd.Parameters ["@p2"].Value, "#6");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

#if NET_2_0
		[Test] // GetUpdateCommand (Boolean)
		public void GetUpdateCommand2 ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, fname, lname, id+1 as next_id from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				SqlCommandBuilder cb;
		
				cb = new SqlCommandBuilder (da);
				cmd = cb.GetUpdateCommand (true);
				Assert.AreEqual ("UPDATE [employee] SET [id] = @id, " +
					"[fname] = @fname, [lname] = @lname WHERE " +
					"(([id] = @Original_id) AND ([fname] = " +
					"@Original_fname) AND ((@IsNull_lname = 1 " +
					"AND [lname] IS NULL) OR ([lname] = " +
					"@Original_lname)))", cmd.CommandText, "#A1");
				Assert.AreSame (conn, cmd.Connection, "#A2");
				AssertUpdateParameters (cmd, true, "#A3:");

				cmd = cb.GetUpdateCommand (false);
				Assert.AreEqual ("UPDATE [employee] SET [id] = @id, " +
					"[fname] = @fname, [lname] = @lname WHERE " +
					"(([id] = @Original_id) AND ([fname] = " +
					"@Original_fname) AND ((@IsNull_lname = 1 " +
					"AND [lname] IS NULL) OR ([lname] = " +
					"@Original_lname)))", cmd.CommandText, "#B1");
				Assert.AreSame (conn, cmd.Connection, "#B2");
				AssertUpdateParameters (cmd, true, "#B3:");

				cb = new SqlCommandBuilder (da);
				cmd = cb.GetUpdateCommand (false);
				Assert.AreEqual ("UPDATE [employee] SET [id] = @p1, " +
					"[fname] = @p2, [lname] = @p3 WHERE " +
					"(([id] = @p4) AND ([fname] = @p5) AND " +
					"((@p6 = 1 AND [lname] IS NULL) OR " +
					"([lname] = @p7)))", cmd.CommandText, "#C1");
				Assert.AreSame (conn, cmd.Connection, "#C2");
				AssertUpdateParameters (cmd, false, "#C3:");

				cmd = cb.GetUpdateCommand (true);
				Assert.AreEqual ("UPDATE [employee] SET [id] = @id, " +
					"[fname] = @fname, [lname] = @lname WHERE " +
					"(([id] = @Original_id) AND ([fname] = " +
					"@Original_fname) AND ((@IsNull_lname = 1 " +
					"AND [lname] IS NULL) OR ([lname] = " +
					"@Original_lname)))", cmd.CommandText, "#D1");
				Assert.AreSame (conn, cmd.Connection, "#D2");
				AssertUpdateParameters (cmd, true, "#D3:");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test] // GetUpdateCommand (Boolean)
		public void GetUpdateCommand2_AutoIncrement ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = "create table #tmp_table (id int primary key , counter int identity(1,1), value varchar(10))";
				cmd.ExecuteNonQuery ();

				string selectQuery = "select id, counter, value, id+1 as next_id from #tmp_table";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds);
				Assert.AreEqual (1, ds.Tables.Count);
				Assert.AreEqual (4, ds.Tables [0].Columns.Count);

				SqlCommandBuilder cb;
		
				cb = new SqlCommandBuilder (da);
				cmd = cb.GetUpdateCommand (true);
				Assert.AreEqual ("UPDATE [#tmp_table] SET [id] = @id, " +
					"[value] = @value WHERE (([id] = @Original_id) " +
					"AND ([counter] = @Original_counter) AND " +
					"((@IsNull_value = 1 AND [value] IS NULL) " +
					"OR ([value] = @Original_value)))",
					cmd.CommandText, "#A1");
				Assert.AreEqual (6, cmd.Parameters.Count, "#A2");

				cmd = cb.GetUpdateCommand (false);
				Assert.AreEqual ("UPDATE [#tmp_table] SET [id] = @id, " +
					"[value] = @value WHERE (([id] = @Original_id) " +
					"AND ([counter] = @Original_counter) AND " +
					"((@IsNull_value = 1 AND [value] IS NULL) " +
					"OR ([value] = @Original_value)))",
					cmd.CommandText, "#B1");
				Assert.AreEqual (6, cmd.Parameters.Count, "#B2");

				cb = new SqlCommandBuilder (da);
				cmd = cb.GetUpdateCommand (false);
				Assert.AreEqual ("UPDATE [#tmp_table] SET [id] = @p1, " +
					"[value] = @p2 WHERE (([id] = @p3) " +
					"AND ([counter] = @p4) AND ((@p5 = 1 " +
					"AND [value] IS NULL) OR ([value] = @p6)))",
					cmd.CommandText, "#C1");
				Assert.AreEqual (6, cmd.Parameters.Count, "#C2");

				cmd = cb.GetUpdateCommand (true);
				Assert.AreEqual ("UPDATE [#tmp_table] SET [id] = @id, " +
					"[value] = @value WHERE (([id] = @Original_id) " +
					"AND ([counter] = @Original_counter) AND " +
					"((@IsNull_value = 1 AND [value] IS NULL) " +
					"OR ([value] = @Original_value)))",
					cmd.CommandText, "#D1");
				Assert.AreEqual (6, cmd.Parameters.Count, "#D2");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test] // GetUpdateCommand (Boolean)
		public void GetUpdateDeleteCommand2_CheckParameters ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				SqlDataAdapter adapter = new SqlDataAdapter (
					"select id, type_varchar from string_family",
					conn);
				SqlCommandBuilder cb = new SqlCommandBuilder (adapter);

				SqlCommand updateCommand = cb.GetUpdateCommand (true);
				Assert.AreEqual (5, updateCommand.Parameters.Count, "#A1");
				Assert.AreEqual (SqlDbType.VarChar, updateCommand.Parameters ["@type_varchar"].SqlDbType, "#A2");
				// FIXME: NotWorking
				//Assert.AreEqual (1, updateCommand.Parameters ["@type_char"].Value, "#A3");

				SqlCommand delCommand = cb.GetDeleteCommand (true);
				Assert.AreEqual (3, delCommand.Parameters.Count, "#B");
				Assert.AreEqual (DbType.Int32, delCommand.Parameters [0].DbType, "#B: DbType (0)");
				Assert.AreEqual ("@Original_id", delCommand.Parameters [0].ParameterName, "#B: ParameterName (0)");
				Assert.AreEqual ("id", delCommand.Parameters [0].SourceColumn, "#B: SourceColumn (0)");
				Assert.AreEqual (SqlDbType.Int, delCommand.Parameters [0].SqlDbType, "#B: SqlDbType (0)");
				Assert.IsNull (delCommand.Parameters [0].Value, "#B: Value (0)");

				Assert.AreEqual (DbType.Int32, delCommand.Parameters [1].DbType, "#B: DbType (1)");
				Assert.AreEqual ("@IsNull_type_varchar", delCommand.Parameters [1].ParameterName, "#B: ParameterName (1)");
				Assert.AreEqual ("type_varchar", delCommand.Parameters [1].SourceColumn, "#B: SourceColumn (1)");
				Assert.AreEqual (SqlDbType.Int, delCommand.Parameters [1].SqlDbType, "#B: SqlDbType (1)");
				Assert.AreEqual (1, delCommand.Parameters [1].Value, "#B: Value (1)");

				Assert.AreEqual (DbType.AnsiString, delCommand.Parameters [2].DbType, "#B: DbType (2)");
				Assert.AreEqual ("@Original_type_varchar", delCommand.Parameters [2].ParameterName, "#B: ParameterName (2)");
				Assert.AreEqual ("type_varchar", delCommand.Parameters [2].SourceColumn, "#B: SourceColumn (2)");
				Assert.AreEqual (SqlDbType.VarChar, delCommand.Parameters [2].SqlDbType, "#B: SqlDbType (2)");
				Assert.IsNull (delCommand.Parameters [2].Value, "#B: Value (2)");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
#endif

		[Test]
		public void GetUpdateCommandDBConcurrencyExceptionTest ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			try {
				string selectQuery = "select id, fname from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				Assert.IsNotNull (cb);

				DataRow [] rows = ds.Tables [0].Select ("id=1");
				rows [0] [0] = 6660; // non existent 
				ds.Tables [0].AcceptChanges (); // moves 6660 to original value
				rows [0] [0] = 1; // moves 6660 as search key into db table
				try {
					da.Update (rows);
					Assert.Fail ("#1");
				} catch (DBConcurrencyException ex) {
					// Concurrency violation: the UpdateCommand
					// affected 0 of the expected 1 records
					Assert.AreEqual (typeof (DBConcurrencyException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
					Assert.AreSame (rows [0], ex.Row, "#6");
#if NET_2_0
					Assert.AreEqual (1, ex.RowCount, "#7");
#endif
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetDeleteCommandDBConcurrencyExceptionTest ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			try {
				string selectQuery = "select id, fname from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				Assert.IsNotNull (cb);

				DataRow [] rows = ds.Tables [0].Select ("id=1");
				rows [0] [0] = 6660; // non existent 
				ds.Tables [0].AcceptChanges (); // moves 6660 to original value
				rows [0].Delete ();  // moves 6660 as search key into db table
				try {
					da.Update (rows);
					Assert.Fail ("#1");
				} catch (DBConcurrencyException ex) {
					// Concurrency violation: the DeleteCommand
					// affected 0 of the expected 1 records
					Assert.AreEqual (typeof (DBConcurrencyException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreSame (rows [0], ex.Row, "#5");
#if NET_2_0
					Assert.AreEqual (1, ex.RowCount, "#6");
#endif
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test] // GetDeleteCommand ()
		public void GetDeleteCommand1 ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, fname, lname, " +
					"id+2 as next_id from employee where " +
					"id = 3 and lname = 'A' and fname = 'B'";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetDeleteCommand ();
#if NET_2_0
				Assert.AreEqual ("DELETE FROM [employee] WHERE " +
					"(([id] = @p1) AND ([fname] = @p2) AND " +
					"((@p3 = 1 AND [lname] IS NULL) OR " +
					"([lname] = @p4)))", cmd.CommandText, "#A1");
#else
				Assert.AreEqual ("DELETE FROM  employee WHERE ( " +
					"(id = @p1) AND ((@p2 = 1 AND fname IS NULL) " +
					"OR (fname = @p3)) AND ((@p4 = 1 AND " +
					"lname IS NULL) OR (lname = @p5)) )",
					cmd.CommandText, "#A1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#A2");
				AssertDeleteParameters (cmd, false, "#A3:");
				Assert.AreSame (cmd, cb.GetDeleteCommand (), "#A4");

				cb.RefreshSchema ();
				cb.QuotePrefix = "\"";
				cmd = cb.GetDeleteCommand ();

#if NET_2_0
				Assert.AreEqual ("DELETE FROM \"employee] WHERE " +
					"((\"id] = @p1) AND (\"fname] = @p2) AND " +
					"((@p3 = 1 AND \"lname] IS NULL) OR " +
					"(\"lname] = @p4)))", cmd.CommandText, "#B1");
#else
				Assert.AreEqual ("DELETE FROM  \"employee WHERE ( " +
					"(\"id = @p1) AND ((@p2 = 1 AND \"fname IS NULL) " +
					"OR (\"fname = @p3)) AND ((@p4 = 1 AND " +
					"\"lname IS NULL) OR (\"lname = @p5)) )",
					cmd.CommandText, "#B1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#B2");
				AssertDeleteParameters (cmd, false, "#B3:");
				Assert.AreSame (cmd, cb.GetDeleteCommand (), "#B4");

				cb.RefreshSchema ();
#if NET_2_0
				cb.QuoteSuffix = "\"";
#else
				cb.QuoteSuffix = "�";
#endif
				cmd = cb.GetDeleteCommand ();

#if NET_2_0
				Assert.AreEqual ("DELETE FROM \"employee\" WHERE " +
					"((\"id\" = @p1) AND (\"fname\" = @p2) AND " +
					"((@p3 = 1 AND \"lname\" IS NULL) OR " +
					"(\"lname\" = @p4)))", cmd.CommandText, "#C1");
#else
				Assert.AreEqual ("DELETE FROM  \"employee� WHERE ( " +
					"(\"id� = @p1) AND ((@p2 = 1 AND \"fname� IS NULL) " +
					"OR (\"fname� = @p3)) AND ((@p4 = 1 AND " +
					"\"lname� IS NULL) OR (\"lname� = @p5)) )",
					cmd.CommandText, "#C1");
#endif
				Assert.AreSame (conn, cmd.Connection, "#C2");
				AssertDeleteParameters (cmd, false, "#C3:");
				Assert.AreSame (cmd, cb.GetDeleteCommand (), "#C4");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test] // GetDeleteCommand ()
		public void GetDeleteCommand1_AutoIncrement ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = "create table #tmp_table (id int primary key , counter int identity(1,1), value varchar(10))";
				cmd.ExecuteNonQuery ();

				string selectQuery = "select id, counter, value, id+1 as next_id from #tmp_table";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds);
				Assert.AreEqual (1, ds.Tables.Count);
				Assert.AreEqual (4, ds.Tables [0].Columns.Count);

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetDeleteCommand ();
#if NET_2_0
				Assert.AreEqual ("DELETE FROM [#tmp_table] WHERE " +
					"(([id] = @p1) AND ([counter] = @p2) AND " +
					"((@p3 = 1 AND [value] IS NULL) OR ([value] = @p4)))",
					cmd.CommandText, "#1");
				Assert.AreEqual (4, cmd.Parameters.Count, "#2");
#else
				Assert.AreEqual ("DELETE FROM  #tmp_table WHERE ( " +
					"(id = @p1) AND ((@p2 = 1 AND counter IS NULL) " +
					"OR (counter = @p3)) AND ((@p4 = 1 AND value " +
					"IS NULL) OR (value = @p5)) )",
					cmd.CommandText, "#1");
				Assert.AreEqual (5, cmd.Parameters.Count, "#2");
#endif
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test] // GetDeleteCommand ()
		public void GetDeleteCommand1_CheckParameters ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				SqlDataAdapter adapter = new SqlDataAdapter (
					"select id, type_varchar from string_family",
					conn);
				SqlCommandBuilder cb = new SqlCommandBuilder (adapter);

				cmd = cb.GetDeleteCommand ();
				Assert.AreEqual (3, cmd.Parameters.Count, "#1");
				Assert.AreEqual (SqlDbType.Int, cmd.Parameters ["@p2"].SqlDbType, "#2");
				Assert.AreEqual (1, cmd.Parameters ["@p2"].Value, "#3");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

#if NET_2_0
		[Test] // GetDeleteCommand ()
		public void GetDeleteCommand2 ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, fname, lname, id+2 as next_id from employee where id = 3";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				SqlCommandBuilder cb;
		
				cb = new SqlCommandBuilder (da);
				cmd = cb.GetDeleteCommand (true);
				Assert.AreEqual ("DELETE FROM [employee] WHERE " +
					"(([id] = @Original_id) AND ([fname] = " +
					"@Original_fname) AND ((@IsNull_lname = 1 " +
					"AND [lname] IS NULL) OR ([lname] = " +
					"@Original_lname)))", cmd.CommandText, "#A1");
				Assert.AreSame (conn, cmd.Connection, "#A2");
				AssertDeleteParameters (cmd, true, "#A3:");
				Assert.AreSame (cmd, cb.GetDeleteCommand (true), "#A4");

				cmd = cb.GetDeleteCommand (false);
				Assert.AreEqual ("DELETE FROM [employee] WHERE " +
					"(([id] = @Original_id) AND ([fname] = " +
					"@Original_fname) AND ((@IsNull_lname = 1 " +
					"AND [lname] IS NULL) OR ([lname] = " +
					"@Original_lname)))", cmd.CommandText, "#B1");
				Assert.AreSame (conn, cmd.Connection, "#B2");
				AssertDeleteParameters (cmd, true, "#B3:");
				Assert.AreSame (cmd, cb.GetDeleteCommand (false), "#B4");

				cb = new SqlCommandBuilder (da);
				cmd = cb.GetDeleteCommand (false);
				Assert.AreEqual ("DELETE FROM [employee] WHERE " +
					"(([id] = @p1) AND ([fname] = @p2) AND " +
					"((@p3 = 1 AND [lname] IS NULL) OR " +
					"([lname] = @p4)))", cmd.CommandText, "#C1");
				Assert.AreSame (conn, cmd.Connection, "#C2");
				AssertDeleteParameters (cmd, false, "#C3:");
				Assert.AreSame (cmd, cb.GetDeleteCommand (false), "#C4");

				cmd = cb.GetDeleteCommand (true);
				Assert.AreEqual ("DELETE FROM [employee] WHERE " +
					"(([id] = @Original_id) AND ([fname] = " +
					"@Original_fname) AND ((@IsNull_lname = 1 " +
					"AND [lname] IS NULL) OR ([lname] = " +
					"@Original_lname)))", cmd.CommandText, "#D1");
				Assert.AreSame (conn, cmd.Connection, "#D2");
				AssertDeleteParameters (cmd, true, "#D3:");
				Assert.AreSame (cmd, cb.GetDeleteCommand (false), "#D4");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test] // GetDeleteCommand (Boolean)
		public void GetDeleteCommand2_AutoIncrement ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			SqlCommand cmd = null;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = "create table #tmp_table (id int primary key , counter int identity(1,1), value varchar(10))";
				cmd.ExecuteNonQuery ();

				string selectQuery = "select id, counter, value, id+1 as next_id from #tmp_table";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds);
				Assert.AreEqual (1, ds.Tables.Count);
				Assert.AreEqual (4, ds.Tables [0].Columns.Count);

				SqlCommandBuilder cb;
		
				cb = new SqlCommandBuilder (da);
				cmd = cb.GetDeleteCommand (true);
				Assert.AreEqual ("DELETE FROM [#tmp_table] WHERE " +
					"(([id] = @Original_id) AND ([counter] = @Original_counter) " +
					"AND ((@IsNull_value = 1 AND [value] IS NULL) " +
					"OR ([value] = @Original_value)))",
					cmd.CommandText, "#A1");
				Assert.AreEqual (4, cmd.Parameters.Count, "#A2");

				cmd = cb.GetDeleteCommand (false);
				Assert.AreEqual ("DELETE FROM [#tmp_table] WHERE " +
					"(([id] = @Original_id) AND ([counter] = @Original_counter) " +
					"AND ((@IsNull_value = 1 AND [value] IS NULL) " +
					"OR ([value] = @Original_value)))",
					cmd.CommandText, "#B1");
				Assert.AreEqual (4, cmd.Parameters.Count, "#B2");

				cb = new SqlCommandBuilder (da);
				cmd = cb.GetDeleteCommand (false);
				Assert.AreEqual ("DELETE FROM [#tmp_table] WHERE " +
					"(([id] = @p1) AND ([counter] = @p2) " +
					"AND ((@p3 = 1 AND [value] IS NULL) " +
					"OR ([value] = @p4)))",
					cmd.CommandText, "#C1");
				Assert.AreEqual (4, cmd.Parameters.Count, "#C2");

				cmd = cb.GetDeleteCommand (true);
				Assert.AreEqual ("DELETE FROM [#tmp_table] WHERE " +
					"(([id] = @Original_id) AND ([counter] = @Original_counter) " +
					"AND ((@IsNull_value = 1 AND [value] IS NULL) " +
					"OR ([value] = @Original_value)))",
					cmd.CommandText, "#D1");
				Assert.AreEqual (4, cmd.Parameters.Count, "#D2");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
#endif

		[Test]
		public void DefaultProperties ()
		{
			try {
				ConnectionManager.Singleton.OpenConnection ();
				//IDbConnection conn = ConnectionManager.Singleton.Connection;
				SqlCommandBuilder cb = new SqlCommandBuilder ();
#if NET_2_0
				Assert.AreEqual ("[", cb.QuotePrefix, "#5");
				Assert.AreEqual ("]", cb.QuoteSuffix, "#6");
				Assert.AreEqual (".", cb.CatalogSeparator, "#2");
				//Assert.AreEqual ("", cb.DecimalSeparator, "#3");
				Assert.AreEqual (".", cb.SchemaSeparator, "#4");
				Assert.AreEqual (CatalogLocation.Start, cb.CatalogLocation, "#1");
				Assert.AreEqual ("[monotest]", cb.QuoteIdentifier ("monotest"), "#7");
				Assert.AreEqual ("\"monotest\"", cb.UnquoteIdentifier ("\"monotest\""), "#8");
				//Assert.AreEqual (cb.ConflictOption.CompareAllSearchableValues, cb.ConflictDetection);
#else
				Assert.AreEqual ("", cb.QuotePrefix, "#5");
				Assert.AreEqual ("", cb.QuoteSuffix, "#6");
				//Assert.AreEqual ("\"monotest\"", cb.QuoteIdentifier ("monotest"), "#7");
				//Assert.AreEqual ("monotest", cb.UnquoteIdentifier ("\"monotest\""), "#8");
#endif
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
			// FIXME: test SetAllValues
		}

		// FIXME:  Add tests for examining RowError
		// FIXME: Add test for ContinueUpdateOnError property
		
		[Test]
		public void CheckParameters_BuiltCommand ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			try {
				SqlDataAdapter adapter = new SqlDataAdapter ("select id,type_varchar from string_family", conn);
				SqlCommandBuilder cb = new SqlCommandBuilder(adapter);
				DataSet ds = new DataSet ();
				adapter.Fill(ds);

				Assert.AreEqual (2, cb.GetInsertCommand().Parameters.Count, "#1");

				DataRow row_rsInput = ds.Tables[0].NewRow();
				row_rsInput["id"] = 100;
				row_rsInput["type_varchar"] = "ttt";
				ds.Tables[0].Rows.Add(row_rsInput);

				Assert.AreEqual (2, cb.GetInsertCommand().Parameters.Count, "#2");

				row_rsInput = ds.Tables[0].NewRow();
				row_rsInput["id"] = 101;
				row_rsInput["type_varchar"] = "ttt";
				ds.Tables[0].Rows.Add(row_rsInput);

				Assert.AreEqual (2, cb.GetInsertCommand().Parameters.Count, "#3");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void DeriveParameters ()
		{
			SqlConnection conn;
			SqlCommand cmd = null;

			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = "sp_326182a";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout = 90;
				cmd.Parameters.Add ("dummy", SqlDbType.Image, 5);

				SqlCommandBuilder.DeriveParameters (cmd);
				Assert.AreEqual (5, cmd.Parameters.Count, "#A1");

				cmd = conn.CreateCommand ();
				cmd.CommandText = "sp_326182b";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout = 90;
				cmd.Parameters.Add ("dummy", SqlDbType.Image, 5);

				SqlCommandBuilder.DeriveParameters (cmd);
				Assert.AreEqual (3, cmd.Parameters.Count, "#A");

				Assert.AreEqual (ParameterDirection.ReturnValue, cmd.Parameters [0].Direction, "#B:Direction");
				Assert.IsFalse (cmd.Parameters [0].IsNullable, "#B:IsNullable");
				Assert.AreEqual ("@RETURN_VALUE", cmd.Parameters [0].ParameterName, "#B:ParameterName");
				//Assert.AreEqual (0, cmd.Parameters [0].Precision, "#B:Precision");
				Assert.AreEqual (0, cmd.Parameters [0].Scale, "#B:Scale");
				Assert.AreEqual (SqlDbType.Int, cmd.Parameters [0].SqlDbType, "#B:SqlDbType");
				Assert.IsNull (cmd.Parameters [0].Value, "#B:Value");

				Assert.AreEqual (ParameterDirection.Input, cmd.Parameters [1].Direction, "#C:Direction");
				//Assert.IsFalse (cmd.Parameters [1].IsNullable, "#C:IsNullable");
				Assert.AreEqual ("@param0", cmd.Parameters [1].ParameterName, "#C:ParameterName");
				//Assert.AreEqual (0, cmd.Parameters [1].Precision, "#C:Precision");
				Assert.AreEqual (0, cmd.Parameters [1].Scale, "#C:Scale");
				Assert.AreEqual (SqlDbType.Int, cmd.Parameters [1].SqlDbType, "#C:SqlDbType");
				Assert.IsNull (cmd.Parameters [1].Value, "#C:Value");

				//Assert.AreEqual (ParameterDirection.InputOutput, cmd.Parameters [2].Direction, "#D:Direction");
				//Assert.IsFalse (cmd.Parameters [2].IsNullable, "#D:IsNullable");
				Assert.AreEqual ("@param1", cmd.Parameters [2].ParameterName, "#D:ParameterName");
				Assert.AreEqual (5, cmd.Parameters [2].Precision, "#D:Precision");
				Assert.AreEqual (2, cmd.Parameters [2].Scale, "#D:Scale");
				Assert.AreEqual (SqlDbType.Decimal, cmd.Parameters [2].SqlDbType, "#D:SqlDbType");
				Assert.IsNull (cmd.Parameters [2].Value, "#D:Value");

				cmd.Parameters ["@param0"].Value = 5;
				cmd.Parameters ["@param1"].Value = 4;
				cmd.ExecuteNonQuery ();
				Assert.AreEqual (666, cmd.Parameters ["@RETURN_VALUE"].Value, "#E1");
				Assert.AreEqual (5, cmd.Parameters ["@param0"].Value, "#E2");
				Assert.AreEqual (7m, cmd.Parameters ["@param1"].Value, "#E3");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void QuotePrefix_DeleteCommand_Generated ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetDeleteCommand ();
#if NET_2_0
				Assert.AreEqual ("[", cb.QuotePrefix, "#1");
#else
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#1");
#endif
				try {
					cb.QuotePrefix = "\"";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
#if NET_2_0
				Assert.AreEqual ("[", cb.QuotePrefix, "#6");
#else
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#6");
#endif
				cb.RefreshSchema ();
				cb.QuotePrefix = "\"";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void QuotePrefix_InsertCommand_Generated ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetInsertCommand ();
#if NET_2_0
				Assert.AreEqual ("[", cb.QuotePrefix, "#1");
#else
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#1");
#endif
				try {
					cb.QuotePrefix = "\"";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
#if NET_2_0
				Assert.AreEqual ("[", cb.QuotePrefix, "#6");
#else
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#6");
#endif
				cb.RefreshSchema ();
				cb.QuotePrefix = "\"";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void QuotePrefix_UpdateCommand_Generated ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetUpdateCommand ();
#if NET_2_0
				Assert.AreEqual ("[", cb.QuotePrefix, "#1");
#else
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#1");
#endif
				try {
					cb.QuotePrefix = "\"";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
#if NET_2_0
				Assert.AreEqual ("[", cb.QuotePrefix, "#6");
#else
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#6");
#endif
				cb.RefreshSchema ();
				cb.QuotePrefix = "\"";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void QuoteSuffix_DeleteCommand_Generated ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetDeleteCommand ();
#if NET_2_0
				Assert.AreEqual ("]", cb.QuoteSuffix, "#1");
#else
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#1");
#endif
				try {
					cb.QuoteSuffix = "\"";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
#if NET_2_0
				Assert.AreEqual ("]", cb.QuoteSuffix, "#6");
#else
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#6");
#endif
				cb.RefreshSchema ();
				cb.QuoteSuffix = "\"";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void QuoteSuffix_InsertCommand_Generated ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetInsertCommand ();
#if NET_2_0
				Assert.AreEqual ("]", cb.QuoteSuffix, "#1");
#else
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#1");
#endif
				try {
					cb.QuoteSuffix = "\"";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
#if NET_2_0
				Assert.AreEqual ("]", cb.QuoteSuffix, "#6");
#else
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#6");
#endif
				cb.RefreshSchema ();
				cb.QuoteSuffix = "\"";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void QuoteSuffix_UpdateCommand_Generated ()
		{
			SqlConnection conn = (SqlConnection) ConnectionManager.Singleton.Connection;

			SqlCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				cmd = cb.GetUpdateCommand ();
#if NET_2_0
				Assert.AreEqual ("]", cb.QuoteSuffix, "#1");
#else
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#1");
#endif
				try {
					cb.QuoteSuffix = "\"";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
#if NET_2_0
				Assert.AreEqual ("]", cb.QuoteSuffix, "#6");
#else
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#6");
#endif
				cb.RefreshSchema ();
				cb.QuoteSuffix = "\"";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		static void AssertDeleteParameters (SqlCommand cmd, bool useColumnsForParameterNames, string prefix)
		{
			SqlParameter param;

#if NET_2_0
			Assert.AreEqual (4, cmd.Parameters.Count, prefix + "Count");
#else
			Assert.AreEqual (5, cmd.Parameters.Count, prefix + "Count");
#endif

			param = cmd.Parameters [0];
			Assert.AreEqual (DbType.Int32, param.DbType, prefix + "DbType (0)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (0)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (0)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (0)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@Original_id", param.ParameterName, prefix + "ParameterName (0)");
			else
				Assert.AreEqual ("@p1", param.ParameterName, prefix + "ParameterName (0)");
			Assert.AreEqual (10, param.Precision, prefix + "Precision (0)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (0)");
			//Assert.AreEqual (0, param.Size, prefix + "Size (0)");
			Assert.AreEqual ("id", param.SourceColumn, prefix + "SourceColumn (0)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (0)");
#endif
			Assert.AreEqual (DataRowVersion.Original, param.SourceVersion, prefix + "SourceVersion (0)");
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, prefix + "SqlDbType (0)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (0)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (0)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (0)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (0)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (0)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (0)");
#endif

#if ONLY_1_1
			param = cmd.Parameters [1];
			Assert.AreEqual (DbType.Int32, param.DbType, prefix + "DbType (1)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (1)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (1)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (1)");
			Assert.AreEqual ("@p2", param.ParameterName, prefix + "ParameterName (1)");
			Assert.AreEqual (0, param.Precision, prefix + "Precision (1)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (1)");
			//Assert.AreEqual (0, param.Size, prefix + "Size (1)");
			Assert.AreEqual (string.Empty, param.SourceColumn, prefix + "SourceColumn (1)");
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (1)");
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, prefix + "SqlDbType (1)");
			Assert.AreEqual (1, param.Value, prefix + "Value (1)");
#endif

#if NET_2_0
			param = cmd.Parameters [1];
#else
			param = cmd.Parameters [2];
#endif
			Assert.AreEqual (DbType.AnsiString, param.DbType, prefix + "DbType (2)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (2)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (2)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (2)");
#if NET_2_0
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@Original_fname", param.ParameterName, prefix + "ParameterName (2)");
			else
				Assert.AreEqual ("@p2", param.ParameterName, prefix + "ParameterName (2)");
#else
			Assert.AreEqual ("@p3", param.ParameterName, prefix + "ParameterName (2)");
#endif
			Assert.AreEqual (0, param.Precision, prefix + "Precision (2)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (2)");
			Assert.AreEqual (0, param.Size, prefix + "Size (2)");
			Assert.AreEqual ("fname", param.SourceColumn, prefix + "SourceColumn (2)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (2)");
#endif
			Assert.AreEqual (DataRowVersion.Original, param.SourceVersion, prefix + "SourceVersion (2)");
			Assert.AreEqual (SqlDbType.VarChar, param.SqlDbType, prefix + "SqlDbType (2)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (2)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (2)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (2)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (2)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (2)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (2)");
#endif

#if NET_2_0
			param = cmd.Parameters [2];
#else
			param = cmd.Parameters [3];
#endif
			Assert.AreEqual (DbType.Int32, param.DbType, prefix + "DbType (3)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (3)");
#if NET_2_0
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (3)");
#else
			Assert.IsTrue (param.IsNullable, prefix + "IsNullable (3)");
#endif
			Assert.AreEqual (0, param.Offset, prefix + "Offset (3)");
#if NET_2_0
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@IsNull_lname", param.ParameterName, prefix + "ParameterName (3)");
			else
				Assert.AreEqual ("@p3", param.ParameterName, prefix + "ParameterName (3)");
#else
			Assert.AreEqual ("@p4", param.ParameterName, prefix + "ParameterName (3)");
#endif
			Assert.AreEqual (0, param.Precision, prefix + "Precision (3)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (3)");
			//Assert.AreEqual (0, param.Size, prefix + "Size (3)");
#if NET_2_0
			Assert.AreEqual ("lname", param.SourceColumn, prefix + "SourceColumn (3)");
			Assert.IsTrue (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (3)");
			Assert.AreEqual (DataRowVersion.Original, param.SourceVersion, prefix + "SourceVersion (3)");
#else
			Assert.AreEqual (string.Empty, param.SourceColumn, prefix + "SourceColumn (3)");
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (3)");
#endif
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, prefix + "SqlDbType (3)");
#if NET_2_0
			Assert.IsNotNull (param.SqlValue, prefix + "SqlValue (3)");
			Assert.AreEqual (typeof (SqlInt32), param.SqlValue.GetType (), prefix + "SqlValue (3)");
			Assert.AreEqual (1, ((SqlInt32) param.SqlValue).Value, prefix + "SqlValue (3)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (3)");
#endif
			Assert.AreEqual (1, param.Value, prefix + "Value (3)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (3)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (3)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (3)");
#endif

#if NET_2_0
			param = cmd.Parameters [3];
#else
			param = cmd.Parameters [4];
#endif
			Assert.AreEqual (DbType.AnsiString, param.DbType, prefix + "DbType (4)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (4)");
#if NET_2_0
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (4)");
#else
			Assert.IsTrue (param.IsNullable, prefix + "IsNullable (4)");
#endif
			Assert.AreEqual (0, param.Offset, prefix + "Offset (4)");
#if NET_2_0
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@Original_lname", param.ParameterName, prefix + "ParameterName (4)");
			else
				Assert.AreEqual ("@p4", param.ParameterName, prefix + "ParameterName (4)");
#else
			Assert.AreEqual ("@p5", param.ParameterName, prefix + "ParameterName (4)");
#endif
			Assert.AreEqual (0, param.Precision, prefix + "Precision (4)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (4)");
			Assert.AreEqual (0, param.Size, prefix + "Size (4)");
			Assert.AreEqual ("lname", param.SourceColumn, prefix + "SourceColumn (4)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (4)");
#endif
			Assert.AreEqual (DataRowVersion.Original, param.SourceVersion, prefix + "SourceVersion (4)");
			Assert.AreEqual (SqlDbType.VarChar, param.SqlDbType, prefix + "SqlDbType (4)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (4)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (4)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (4)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (4)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (4)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (4)");
#endif
		}

		static void AssertInsertParameters (SqlCommand cmd, bool useColumnsForParameterNames, string prefix)
		{
			SqlParameter param;

			Assert.AreEqual (3, cmd.Parameters.Count, prefix + "Count");

			param = cmd.Parameters [0];
			Assert.AreEqual (DbType.Int32, param.DbType, prefix + "DbType (0)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (0)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (0)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (0)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@id", param.ParameterName, prefix + "ParameterName (0)");
			else
				Assert.AreEqual ("@p1", param.ParameterName, prefix + "ParameterName (0)");
			Assert.AreEqual (10, param.Precision, prefix + "Precision (0)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (0)");
			//Assert.AreEqual (0, param.Size, prefix + "Size (0)");
			Assert.AreEqual ("id", param.SourceColumn, prefix + "SourceColumn (0)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (0)");
#endif
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (0)");
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, prefix + "SqlDbType (0)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (0)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (0)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (0)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (0)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (0)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (0)");
#endif

			param = cmd.Parameters [1];
			Assert.AreEqual (DbType.AnsiString, param.DbType, prefix + "DbType (1)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (1)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (1)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (1)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@fname", param.ParameterName, prefix + "ParameterName (1)");
			else
				Assert.AreEqual ("@p2", param.ParameterName, prefix + "ParameterName (1)");
			Assert.AreEqual (0, param.Precision, prefix + "Precision (1)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (1)");
			Assert.AreEqual (0, param.Size, prefix + "Size (1)");
			Assert.AreEqual ("fname", param.SourceColumn, prefix + "SourceColumn (1)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (1)");
#endif
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (1)");
			Assert.AreEqual (SqlDbType.VarChar, param.SqlDbType, prefix + "SqlDbType (1)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (1)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (1)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (1)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (1)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (1)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (1)");
#endif

			param = cmd.Parameters [2];
			Assert.AreEqual (DbType.AnsiString, param.DbType, prefix + "DbType (2)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (2)");
#if NET_2_0
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (2)");
#else
			Assert.IsTrue (param.IsNullable, prefix + "IsNullable (2)");
#endif
			Assert.AreEqual (0, param.Offset, prefix + "Offset (2)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@lname", param.ParameterName, prefix + "ParameterName (2)");
			else
				Assert.AreEqual ("@p3", param.ParameterName, prefix + "ParameterName (2)");
			Assert.AreEqual (0, param.Precision, prefix + "Precision (2)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (2)");
			Assert.AreEqual (0, param.Size, prefix + "Size (2)");
			Assert.AreEqual ("lname", param.SourceColumn, prefix + "SourceColumn (2)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (2)");
#endif
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (2)");
			Assert.AreEqual (SqlDbType.VarChar, param.SqlDbType, prefix + "SqlDbType (2)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (2)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (2)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (2)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (2)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (2)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (2)");
#endif
		}

		static void AssertUpdateParameters (SqlCommand cmd, bool useColumnsForParameterNames, string prefix)
		{
			SqlParameter param;

#if NET_2_0
			Assert.AreEqual (7, cmd.Parameters.Count, prefix + "Count");
#else
			Assert.AreEqual (8, cmd.Parameters.Count, prefix + "Count");
#endif

			param = cmd.Parameters [0];
			Assert.AreEqual (DbType.Int32, param.DbType, prefix + "DbType (0)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (0)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (0)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (0)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@id", param.ParameterName, prefix + "ParameterName (0)");
			else
				Assert.AreEqual ("@p1", param.ParameterName, prefix + "ParameterName (0)");
			Assert.AreEqual (10, param.Precision, prefix + "Precision (0)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (0)");
			//Assert.AreEqual (0, param.Size, prefix + "Size (0)");
			Assert.AreEqual ("id", param.SourceColumn, prefix + "SourceColumn (0)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (0)");
#endif
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (0)");
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, prefix + "SqlDbType (0)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (0)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (0)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (0)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (0)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (0)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (0)");
#endif

			param = cmd.Parameters [1];
			Assert.AreEqual (DbType.AnsiString, param.DbType, prefix + "DbType (1)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (1)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (1)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (1)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@fname", param.ParameterName, prefix + "ParameterName (1)");
			else
				Assert.AreEqual ("@p2", param.ParameterName, prefix + "ParameterName (1)");
			Assert.AreEqual (0, param.Precision, prefix + "Precision (1)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (1)");
			//Assert.AreEqual (0, param.Size, prefix + "Size (1)");
			Assert.AreEqual ("fname", param.SourceColumn, prefix + "SourceColumn (1)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (1)");
#endif
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (1)");
			Assert.AreEqual (SqlDbType.VarChar, param.SqlDbType, prefix + "SqlDbType (1)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (1)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (1)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (1)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (1)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (1)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (1)");
#endif

			param = cmd.Parameters [2];
			Assert.AreEqual (DbType.AnsiString, param.DbType, prefix + "DbType (2)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (2)");
#if NET_2_0
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (2)");
#else
			Assert.IsTrue (param.IsNullable, prefix + "IsNullable (2)");
#endif
			Assert.AreEqual (0, param.Offset, prefix + "Offset (2)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@lname", param.ParameterName, prefix + "ParameterName (2)");
			else
				Assert.AreEqual ("@p3", param.ParameterName, prefix + "ParameterName (2)");
			Assert.AreEqual (0, param.Precision, prefix + "Precision (2)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (2)");
			Assert.AreEqual (0, param.Size, prefix + "Size (2)");
			Assert.AreEqual ("lname", param.SourceColumn, prefix + "SourceColumn (2)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (2)");
#endif
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (2)");
			Assert.AreEqual (SqlDbType.VarChar, param.SqlDbType, prefix + "SqlDbType (2)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (2)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (2)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (2)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (2)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (2)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (2)");
#endif

			param = cmd.Parameters [3];
			Assert.AreEqual (DbType.Int32, param.DbType, prefix + "DbType (3)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (3)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (3)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (3)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@Original_id", param.ParameterName, prefix + "ParameterName (3)");
			else
				Assert.AreEqual ("@p4", param.ParameterName, prefix + "ParameterName (3)");
			Assert.AreEqual (10, param.Precision, prefix + "Precision (3)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (3)");
			//Assert.AreEqual (0, param.Size, prefix + "Size (3)");
			Assert.AreEqual ("id", param.SourceColumn, prefix + "SourceColumn (3)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (3)");
#endif
			Assert.AreEqual (DataRowVersion.Original, param.SourceVersion, prefix + "SourceVersion (3)");
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, prefix + "SqlDbType (3)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (3)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (3)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (3)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (3)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (3)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (3)");
#endif

#if ONLY_1_1
			param = cmd.Parameters [4];
			Assert.AreEqual (DbType.Int32, param.DbType, prefix + "DbType (4)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (4)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (4)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (4)");
			Assert.AreEqual ("@p5", param.ParameterName, prefix + "ParameterName (4)");
			Assert.AreEqual (0, param.Precision, prefix + "Precision (4)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (4)");
			//Assert.AreEqual (0, param.Size, prefix + "Size (4)");
			Assert.AreEqual (string.Empty, param.SourceColumn, prefix + "SourceColumn (4)");
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (4)");
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, prefix + "SqlDbType (4)");
			Assert.AreEqual (1, param.Value, prefix + "Value (4)");
#endif

#if NET_2_0
			param = cmd.Parameters [4];
#else
			param = cmd.Parameters [5];
#endif
			Assert.AreEqual (DbType.AnsiString, param.DbType, prefix + "DbType (5)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (5)");
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (5)");
			Assert.AreEqual (0, param.Offset, prefix + "Offset (5)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@Original_fname", param.ParameterName, prefix + "ParameterName (5)");
			else
#if NET_2_0
				Assert.AreEqual ("@p5", param.ParameterName, prefix + "ParameterName (5)");
#else
				Assert.AreEqual ("@p6", param.ParameterName, prefix + "ParameterName (5)");
#endif
			Assert.AreEqual (0, param.Precision, prefix + "Precision (5)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (5)");
			Assert.AreEqual (0, param.Size, prefix + "Size (5)");
			Assert.AreEqual ("fname", param.SourceColumn, prefix + "SourceColumn (5)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (5)");
#endif
			Assert.AreEqual (DataRowVersion.Original, param.SourceVersion, prefix + "SourceVersion (5)");
			Assert.AreEqual (SqlDbType.VarChar, param.SqlDbType, prefix + "SqlDbType (5)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (5)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (5)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (5)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (5)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (5)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (5)");
#endif

#if NET_2_0
			param = cmd.Parameters [5];
#else
			param = cmd.Parameters [6];
#endif
			Assert.AreEqual (DbType.Int32, param.DbType, prefix + "DbType (6)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (6)");
#if NET_2_0
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (6)");
#else
			Assert.IsTrue (param.IsNullable, prefix + "IsNullable (6)");
#endif
			Assert.AreEqual (0, param.Offset, prefix + "Offset (6)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@IsNull_lname", param.ParameterName, prefix + "ParameterName (6)");
			else
#if NET_2_0
				Assert.AreEqual ("@p6", param.ParameterName, prefix + "ParameterName (6)");
#else
				Assert.AreEqual ("@p7", param.ParameterName, prefix + "ParameterName (6)");
#endif
			Assert.AreEqual (0, param.Precision, prefix + "Precision (6)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (6)");
			//Assert.AreEqual (0, param.Size, prefix + "Size (6)");
#if NET_2_0
			Assert.AreEqual ("lname", param.SourceColumn, prefix + "SourceColumn (6)");
			Assert.IsTrue (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (6)");
			Assert.AreEqual (DataRowVersion.Original, param.SourceVersion, prefix + "SourceVersion (6)");
#else
			Assert.AreEqual (string.Empty, param.SourceColumn, prefix + "SourceColumn (6)");
			Assert.AreEqual (DataRowVersion.Current, param.SourceVersion, prefix + "SourceVersion (6)");
#endif
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, prefix + "SqlDbType (6)");
#if NET_2_0
			Assert.IsNotNull (param.SqlValue, prefix + "SqlValue (6)");
			Assert.AreEqual (typeof (SqlInt32), param.SqlValue.GetType (), prefix + "SqlValue (6)");
			Assert.AreEqual (1, ((SqlInt32) param.SqlValue).Value, prefix + "SqlValue (6)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (6)");
#endif
			Assert.AreEqual (1, param.Value, prefix + "Value (6)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (6)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (6)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (6)");
#endif

#if NET_2_0
			param = cmd.Parameters [6];
#else
			param = cmd.Parameters [7];
#endif
			Assert.AreEqual (DbType.AnsiString, param.DbType, prefix + "DbType (7)");
			Assert.AreEqual (ParameterDirection.Input, param.Direction, prefix + "Direction (7)");
#if NET_2_0
			Assert.IsFalse (param.IsNullable, prefix + "IsNullable (7)");
#else
			Assert.IsTrue (param.IsNullable, prefix + "IsNullable (7)");
#endif
			Assert.AreEqual (0, param.Offset, prefix + "Offset (7)");
			if (useColumnsForParameterNames)
				Assert.AreEqual ("@Original_lname", param.ParameterName, prefix + "ParameterName (7)");
			else
#if NET_2_0
				Assert.AreEqual ("@p7", param.ParameterName, prefix + "ParameterName (7)");
#else
				Assert.AreEqual ("@p8", param.ParameterName, prefix + "ParameterName (7)");
#endif
			Assert.AreEqual (0, param.Precision, prefix + "Precision (7)");
			Assert.AreEqual (0, param.Scale, prefix + "Scale (7)");
			Assert.AreEqual (0, param.Size, prefix + "Size (7)");
			Assert.AreEqual ("lname", param.SourceColumn, prefix + "SourceColumn (7)");
#if NET_2_0
			Assert.IsFalse (param.SourceColumnNullMapping, prefix + "SourceColumnNullMapping (7)");
#endif
			Assert.AreEqual (DataRowVersion.Original, param.SourceVersion, prefix + "SourceVersion (7)");
			Assert.AreEqual (SqlDbType.VarChar, param.SqlDbType, prefix + "SqlDbType (7)");
#if NET_2_0
			Assert.IsNull (param.SqlValue, prefix + "SqlValue (7)");
			//Assert.AreEqual (string.Empty, param.UdtTypeName, prefix + "UdtTypeName (7)");
#endif
			Assert.IsNull (param.Value, prefix + "Value (7)");
#if NET_2_0
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionDatabase, prefix + "XmlSchemaCollectionDatabase (7)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionName, prefix + "XmlSchemaCollectionName (7)");
			Assert.AreEqual (string.Empty, param.XmlSchemaCollectionOwningSchema, prefix + "XmlSchemaCollectionOwningSchema (7)");
#endif
		}
	}
}

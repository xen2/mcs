//Tests to check the behavior of ReadXml and WriteXml

#if NET_2_0
using System;
using System.Data;
using System.Collections;
using System.Data.Odbc;
using System.IO;	
using System.Xml;
using NUnit.Framework;

namespace Monotests_System.Data
{
	[TestFixture]	
	public class DataTableTest4
	{
		string fileName1 = "Test/System.Data/TestFile5.xml";

		DataSet dataSet = null;
		DataTable dummyTable = null;	
		DataTable parentTable1 = null;
		DataTable childTable = null;
		DataTable secondChildTable = null;
	
		private void MakeParentTable1 ()
		{
			// Create a new Table
			parentTable1 = new DataTable ("ParentTable");
			dataSet = new DataSet ("XmlDataSet");
			DataColumn column;
			DataRow row;
 
			// Create new DataColumn, set DataType, 
			// ColumnName and add to Table.    
			column = new DataColumn ();
			column.DataType = System.Type.GetType ("System.Int32");
			column.ColumnName = "id";
			column.Unique = true;
			// Add the Column to the DataColumnCollection.
			parentTable1.Columns.Add (column);

			// Create second column
			column = new DataColumn ();
			column.DataType = System.Type.GetType ("System.String");
			column.ColumnName = "ParentItem";
			column.AutoIncrement = false;
			column.Caption = "ParentItem";
			column.Unique = false;
			// Add the column to the table
			parentTable1.Columns.Add (column);

			// Create third column.
			column = new DataColumn ();
			column.DataType = System.Type.GetType ("System.Int32");
			column.ColumnName = "DepartmentID";
			column.Caption = "DepartmentID";
			// Add the column to the table.
			parentTable1.Columns.Add (column);
 

			// Make the ID column the primary key column.
			DataColumn [] PrimaryKeyColumns = new DataColumn [2];
			PrimaryKeyColumns [0] = parentTable1.Columns ["id"];
			PrimaryKeyColumns [1] = parentTable1.Columns ["DepartmentID"];
			parentTable1.PrimaryKey = PrimaryKeyColumns;

			dataSet.Tables.Add (parentTable1);
 
			// Create three new DataRow objects and add 
			// them to the DataTable
			for (int i = 0; i <= 2; i++)
			{
				row = parentTable1.NewRow ();
				row ["id"] = i + 1 ;
				row ["ParentItem"] = "ParentItem " + (i + 1);
				row ["DepartmentID"] = i + 1;
				parentTable1.Rows.Add (row);
			}
		}

		private void MakeDummyTable ()
		{
			// Create a new Table
			dataSet = new DataSet ();
			dummyTable = new DataTable ("DummyTable");
			DataColumn column;
			DataRow row;
 
			// Create new DataColumn, set DataType, 
			// ColumnName and add to Table.    
			column = new DataColumn ();
			column.DataType = System.Type.GetType ("System.Int32");
			column.ColumnName = "id";
			column.Unique = true;
			// Add the Column to the DataColumnCollection.
			dummyTable.Columns.Add (column);

			// Create second column
			column = new DataColumn ();
			column.DataType = System.Type.GetType ("System.String");
			column.ColumnName = "DummyItem";
			column.AutoIncrement = false;
			column.Caption = "DummyItem";
			column.Unique = false;
			// Add the column to the table
			dummyTable.Columns.Add (column);

			dataSet.Tables.Add (dummyTable);

			// Create three new DataRow objects and add 
			// them to the DataTable
			for (int i=0; i<=2; i++)
			{
				row = dummyTable.NewRow ();
				row ["id"] = i + 1 ;
				row ["DummyItem"] = "DummyItem " + (i + 1);
				dummyTable.Rows.Add (row);
			}

			DataRow row1 = dummyTable.Rows [1];
			dummyTable.AcceptChanges ();
			row1.BeginEdit ();
			row1 [1] = "Changed_DummyItem " + 2;
			row1.EndEdit ();			
		}
		
		private void MakeChildTable ()
		{
			// Create a new Table
			childTable = new DataTable ("ChildTable");
			DataColumn column;
			DataRow row;
 
			// Create first column and add to the DataTable.
			column = new DataColumn ();
			column.DataType= System.Type.GetType ("System.Int32");
			column.ColumnName = "ChildID";
			column.AutoIncrement = true;
			column.Caption = "ID";
			column.Unique = true;
			
			// Add the column to the DataColumnCollection
			childTable.Columns.Add (column);
			 
			// Create second column
			column = new DataColumn ();
			column.DataType= System.Type.GetType ("System.String");
			column.ColumnName = "ChildItem";
			column.AutoIncrement = false;
			column.Caption = "ChildItem";
			column.Unique = false;
			childTable.Columns.Add (column);
			 
			//Create third column
			column = new DataColumn ();
			column.DataType= System.Type.GetType ("System.Int32");
			column.ColumnName = "ParentID";
			column.AutoIncrement = false;
			column.Caption = "ParentID";
			column.Unique = false;
			childTable.Columns.Add (column);

			dataSet.Tables.Add (childTable);
 
			// Create three sets of DataRow objects, 
			// five rows each, and add to DataTable.
			for(int i = 0; i <= 1; i ++)
			{
				row = childTable.NewRow ();
				row ["childID"] = i + 1;
				row ["ChildItem"] = "ChildItem " + (i + 1);
				row ["ParentID"] = 1 ;
				childTable.Rows.Add (row);
			}		
			for(int i=0; i<=1; i ++)
			{
				row = childTable.NewRow ();	
				row ["childID"] = i + 5;
				row ["ChildItem"] = "ChildItem " + (i + 1);
				row ["ParentID"] = 2 ;
				childTable.Rows.Add (row);
			}
			for(int i = 0; i <= 1; i ++)
			{
				row = childTable.NewRow ();
				row ["childID"] = i + 10;
				row ["ChildItem"] = "ChildItem " + (i + 1);
				row ["ParentID"] = 3 ;
				childTable.Rows.Add (row);
			}
		}

		private void MakeSecondChildTable ()
		{
			// Create a new Table
			secondChildTable = new DataTable ("SecondChildTable");
			DataColumn column;
			DataRow row;
 
			// Create first column and add to the DataTable.
			column = new DataColumn ();
			column.DataType= System.Type.GetType ("System.Int32");
			column.ColumnName = "ChildID";
			column.AutoIncrement = true;
			column.Caption = "ID";
			column.ReadOnly = true;
			column.Unique = true;
			
			// Add the column to the DataColumnCollection.
			secondChildTable.Columns.Add (column);
			 
			// Create second column.
			column = new DataColumn ();
			column.DataType= System.Type.GetType ("System.String");
			column.ColumnName = "ChildItem";
			column.AutoIncrement = false;
			column.Caption = "ChildItem";
			column.ReadOnly = false;
			column.Unique = false;
			secondChildTable.Columns.Add (column);
			 
			//Create third column.
			column = new DataColumn ();
			column.DataType= System.Type.GetType ("System.Int32");
			column.ColumnName = "ParentID";
			column.AutoIncrement = false;
			column.Caption = "ParentID";
			column.ReadOnly = false;
			column.Unique = false;
			secondChildTable.Columns.Add (column);

			//Create fourth column.
			column = new DataColumn ();
			column.DataType= System.Type.GetType ("System.Int32");
			column.ColumnName = "DepartmentID";
			column.Caption = "DepartmentID";
			column.Unique = false;
			secondChildTable.Columns.Add (column);

			dataSet.Tables.Add (secondChildTable);
			// Create three sets of DataRow objects, 
			// five rows each, and add to DataTable.
			for(int i = 0; i <= 1; i ++)
			{
				row = secondChildTable.NewRow ();
				row ["childID"] = i + 1;
				row ["ChildItem"] = "SecondChildItem " + (i + 1);
				row ["ParentID"] = 1 ;
				row ["DepartmentID"] = 1; 
				secondChildTable.Rows.Add (row);
			}		
			for(int i = 0; i <= 1; i++)
			{
				row = secondChildTable.NewRow ();
				row ["childID"] = i + 5;
				row ["ChildItem"] = "SecondChildItem " + (i + 1);
				row ["ParentID"] = 2;
				row ["DepartmentID"] = 2;
				secondChildTable.Rows.Add (row);
			}
			for(int i = 0; i <= 1; i++)
			{
				row = secondChildTable.NewRow ();
				row ["childID"] = i + 10;
				row ["ChildItem"] = "SecondChildItem " + (i + 1);
				row ["ParentID"] = 3 ;
				row ["DepartmentID"] = 3;
				secondChildTable.Rows.Add (row);
			}
		}

		private void MakeDataRelation ()
		{
			DataColumn parentColumn = dataSet.Tables ["ParentTable"].Columns ["id"];
			DataColumn childColumn = dataSet.Tables ["ChildTable"].Columns ["ParentID"];
			DataRelation relation = new DataRelation ("ParentChild_Relation1", parentColumn, childColumn);
			dataSet.Tables ["ChildTable"].ParentRelations.Add (relation);

			DataColumn [] parentColumn1 = new DataColumn [2];
			DataColumn [] childColumn1 = new DataColumn [2];

			parentColumn1 [0] = dataSet.Tables ["ParentTable"].Columns ["id"];
			parentColumn1 [1] = dataSet.Tables ["ParentTable"].Columns ["DepartmentID"];

			childColumn1 [0] = dataSet.Tables ["SecondChildTable"].Columns ["ParentID"];
			childColumn1 [1] = dataSet.Tables ["SecondChildTable"].Columns ["DepartmentID"];

			DataRelation secondRelation = new DataRelation("ParentChild_Relation2", parentColumn1, childColumn1);
			dataSet.Tables ["SecondChildTable"].ParentRelations.Add (secondRelation);
		}

		//Test properties of a table which does not belongs to a DataSet
		private void TestTableSchema (DataTable table, string tableName, DataSet ds)
		{
			//Test Schema 
			//Check Properties of Table
			Assert.AreEqual ("", table.Namespace, "#1");
                        Assert.AreEqual (ds, table.DataSet, "#2");
                        Assert.AreEqual (3, table.Columns.Count, "#3");
                        Assert.AreEqual (false, table.CaseSensitive, "#5");
			Assert.AreEqual (tableName, table.TableName, "#6");
			Assert.AreEqual (2, table.Constraints.Count, "#7");
                        Assert.AreEqual ("", table.Prefix, "#8");
			Assert.AreEqual ("Constraint1", table.Constraints [0].ToString (), "#9");
			Assert.AreEqual ("Constraint2", table.Constraints [1].ToString (), "#10");
			Assert.AreEqual ("System.Data.UniqueConstraint", table.Constraints [0].GetType ().ToString (), "#11");
			Assert.AreEqual ("System.Data.UniqueConstraint", table.Constraints [1].GetType ().ToString (), "#12");
			Assert.AreEqual (2, table.PrimaryKey.Length, "#13");
			Assert.AreEqual ("id", table.PrimaryKey [0].ToString (), "#14");
			Assert.AreEqual ("DepartmentID", table.PrimaryKey [1].ToString (), "#15");
			Assert.AreEqual (0, table.ParentRelations.Count, "#16");
			Assert.AreEqual (0, table.ChildRelations.Count, "#17");

			//Check properties of each column
			//First Column
			DataColumn col = table.Columns [0];
                        Assert.AreEqual (false, col.AllowDBNull, "#18"); 
                        Assert.AreEqual (false, col.AutoIncrement, "#19");
                        Assert.AreEqual (0, col.AutoIncrementSeed, "#20");
                        Assert.AreEqual (1, col.AutoIncrementStep, "#21");
                        Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#22");
                        Assert.AreEqual ("id", col.Caption, "#23");
                        Assert.AreEqual ("id", col.ColumnName, "#24");
                        Assert.AreEqual ("System.Int32", col.DataType.ToString (), "#25");
                        Assert.AreEqual ("", col.DefaultValue.ToString (), "#26");
                        Assert.AreEqual (false, col.DesignMode, "#27");
                        Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#28");
			Assert.AreEqual (-1, col.MaxLength, "#29");
                        Assert.AreEqual (0, col.Ordinal, "#30");
                        Assert.AreEqual ("", col.Prefix, "#31");
                        Assert.AreEqual ("ParentTable", col.Table.ToString (), "#32");
                        Assert.AreEqual (true, col.Unique, "#33");

			//Second Column
			col = table.Columns [1];
			Assert.AreEqual (true, col.AllowDBNull, "#34");
			Assert.AreEqual (false, col.AutoIncrement, "#35");
			Assert.AreEqual (0, col.AutoIncrementSeed, "#36");
			Assert.AreEqual (1, col.AutoIncrementStep, "#37");
			Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#38");
			Assert.AreEqual ("ParentItem", col.Caption, "#39");
			Assert.AreEqual ("ParentItem", col.ColumnName, "#40");
			Assert.AreEqual ("System.String", col.DataType.ToString (), "#41");
			Assert.AreEqual ("", col.DefaultValue.ToString (), "#42");
			Assert.AreEqual (false, col.DesignMode, "#43");
			Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#44");
			Assert.AreEqual (-1, col.MaxLength, "#45");
			Assert.AreEqual (1, col.Ordinal, "#46");
			Assert.AreEqual ("", col.Prefix, "#47");
			Assert.AreEqual ("ParentTable", col.Table.ToString (), "#48");
			Assert.AreEqual (false, col.Unique, "#49");

			//Third Column
			col = table.Columns [2];
			Assert.AreEqual (false, col.AllowDBNull, "#50");
			Assert.AreEqual (false, col.AutoIncrement, "#51");
			Assert.AreEqual (0, col.AutoIncrementSeed, "#52");
			Assert.AreEqual (1, col.AutoIncrementStep, "#53");
			Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#54");
			Assert.AreEqual ("DepartmentID", col.Caption, "#55");
			Assert.AreEqual ("DepartmentID", col.ColumnName, "#56");
			Assert.AreEqual ("System.Int32", col.DataType.ToString (), "#57");
			Assert.AreEqual ("", col.DefaultValue.ToString (), "#58");
			Assert.AreEqual (false, col.DesignMode, "#59");
			Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#60");
			Assert.AreEqual (-1, col.MaxLength, "#61");
			Assert.AreEqual (2, col.Ordinal, "#62");
			Assert.AreEqual ("", col.Prefix, "#63");
			Assert.AreEqual ("ParentTable", col.Table.ToString (), "#64");
			Assert.AreEqual (false, col.Unique, "#65");

			//Test the Xml
			Assert.AreEqual (3, table.Rows.Count, "#66");
			//Test values of each row
			DataRow row = table.Rows [0];
			Assert.AreEqual (1, row ["id"], "#67");
			Assert.AreEqual ("ParentItem 1", row ["ParentItem"], "#68");
			Assert.AreEqual (1, row ["DepartmentID"], "#69");

			row = table.Rows [1];
			Assert.AreEqual (2, row ["id"], "#70");
			Assert.AreEqual ("ParentItem 2", row ["ParentItem"], "#71");
			Assert.AreEqual (2, row ["DepartmentID"], "#72");

			row = table.Rows [2];
			Assert.AreEqual (3, row ["id"], "#73");
			Assert.AreEqual ("ParentItem 3", row ["ParentItem"], "#74");
			Assert.AreEqual (3, row ["DepartmentID"], "#75");
		}

		private void TestTable_WithChildren (DataTable table, string tableName, DataSet ds)
		{
			//Test Schema 
			//Check Properties of Table
			Assert.AreEqual ("", table.Namespace, "#1");
                        Assert.AreEqual (ds.DataSetName, table.DataSet.DataSetName, "#2");  
                        Assert.AreEqual (3, table.Columns.Count, "#3");
                        Assert.AreEqual (false, table.CaseSensitive, "#5");
			Assert.AreEqual (tableName, table.TableName, "#6");
			Assert.AreEqual (2, table.Constraints.Count, "#7"); 
                        Assert.AreEqual ("", table.Prefix, "#8"); 
			Assert.AreEqual ("Constraint1", table.Constraints [0].ToString (), "#9");
			Assert.AreEqual ("Constraint2", table.Constraints [1].ToString (), "#10");
			Assert.AreEqual ("System.Data.UniqueConstraint", table.Constraints [0].GetType ().ToString (), "#11");
			Assert.AreEqual ("System.Data.UniqueConstraint", table.Constraints [1].GetType ().ToString (), "#12");
			Assert.AreEqual (2, table.PrimaryKey.Length, "#13");
			Assert.AreEqual ("id", table.PrimaryKey [0].ToString (), "#14");
			Assert.AreEqual ("DepartmentID", table.PrimaryKey [1].ToString (), "#15");
			Assert.AreEqual (0, table.ParentRelations.Count, "#16");
			Assert.AreEqual (2, table.ChildRelations.Count, "#17");

			//Check properties of each column
			//First Column
			DataColumn col = table.Columns [0];
                        Assert.AreEqual (false, col.AllowDBNull, "#18"); 
                        Assert.AreEqual (false, col.AutoIncrement, "#19"); 
                        Assert.AreEqual (0, col.AutoIncrementSeed, "#20"); 
                        Assert.AreEqual (1, col.AutoIncrementStep, "#21"); 
                        Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#22"); 
                        Assert.AreEqual ("id", col.Caption, "#23"); 
                        Assert.AreEqual ("id", col.ColumnName, "#24"); 
                        Assert.AreEqual ("System.Int32", col.DataType.ToString (), "#25"); 
                        Assert.AreEqual ("", col.DefaultValue.ToString (), "#26"); 
                        Assert.AreEqual (false, col.DesignMode, "#27"); 
                        Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#28");
			Assert.AreEqual (-1, col.MaxLength, "#29"); 
                        Assert.AreEqual (0, col.Ordinal, "#30"); 
                        Assert.AreEqual ("", col.Prefix, "#31"); 
                        Assert.AreEqual ("ParentTable", col.Table.ToString (), "#32"); 
                        Assert.AreEqual (true, col.Unique, "#33");

			//Second Column
			col = table.Columns [1];
			Assert.AreEqual (true, col.AllowDBNull, "#34");
			Assert.AreEqual (false, col.AutoIncrement, "#35");
			Assert.AreEqual (0, col.AutoIncrementSeed, "#36");
			Assert.AreEqual (1, col.AutoIncrementStep, "#37");
			Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#38");
			Assert.AreEqual ("ParentItem", col.Caption, "#39");
			Assert.AreEqual ("ParentItem", col.ColumnName, "#40");
			Assert.AreEqual ("System.String", col.DataType.ToString (), "#41");
			Assert.AreEqual ("", col.DefaultValue.ToString (), "#42");
			Assert.AreEqual (false, col.DesignMode, "#43");
			Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#44");
			Assert.AreEqual (-1, col.MaxLength, "#45");
			Assert.AreEqual (1, col.Ordinal, "#46");
			Assert.AreEqual ("", col.Prefix, "#47");
			Assert.AreEqual ("ParentTable", col.Table.ToString (), "#48");
			Assert.AreEqual (false, col.Unique, "#49");

			//Third Column
			col = table.Columns [2];
			Assert.AreEqual (false, col.AllowDBNull, "#50");
			Assert.AreEqual (false, col.AutoIncrement, "#51");
			Assert.AreEqual (0, col.AutoIncrementSeed, "#52");
			Assert.AreEqual (1, col.AutoIncrementStep, "#53");
			Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#54");
			Assert.AreEqual ("DepartmentID", col.Caption, "#55");
			Assert.AreEqual ("DepartmentID", col.ColumnName, "#56");
			Assert.AreEqual ("System.Int32", col.DataType.ToString (), "#57");
			Assert.AreEqual ("", col.DefaultValue.ToString (), "#58");
			Assert.AreEqual (false, col.DesignMode, "#59");
			Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#60");
			Assert.AreEqual (-1, col.MaxLength, "#61");
			Assert.AreEqual (2, col.Ordinal, "#62");
			Assert.AreEqual ("", col.Prefix, "#63");
			Assert.AreEqual ("ParentTable", col.Table.ToString (), "#64");
			Assert.AreEqual (false, col.Unique, "#65");

			//Test the Xml
			Assert.AreEqual (3, table.Rows.Count, "#66");
			//Test values of each row
			DataRow row = table.Rows [0];
			Assert.AreEqual (1, row ["id"], "#67");
			Assert.AreEqual ("ParentItem 1", row ["ParentItem"], "#68");
			Assert.AreEqual (1, row ["DepartmentID"], "#69");

			row = table.Rows [1];
			Assert.AreEqual (2, row ["id"], "#70");
			Assert.AreEqual ("ParentItem 2", row ["ParentItem"], "#71");
			Assert.AreEqual (2, row ["DepartmentID"], "#72");

			row = table.Rows [2];
			Assert.AreEqual (3, row ["id"], "#73");
			Assert.AreEqual ("ParentItem 3", row ["ParentItem"], "#74");
			Assert.AreEqual (3, row ["DepartmentID"], "#75");

		}

		private void TestDiffGramElement1 (XmlReader reader)	
		{
			//This method checks the properties of the <id> element
			Assert.AreEqual (true, reader.IsStartElement (), "#1");
			Assert.AreEqual (3, reader.Depth, "#2");
			Assert.AreEqual (false, reader.HasAttributes, "#3");
			Assert.AreEqual (false, reader.HasValue, "#4");
			Assert.AreEqual (false, reader.IsDefault, "#5");
			Assert.AreEqual (false, reader.IsEmptyElement, "#6");
			Assert.AreEqual ("id", reader.Name, "#7");
			Assert.AreEqual ("id", reader.LocalName, "#8");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#9");
		}

		private void TestDiffGramElement3 (XmlReader reader)
		{
			//This method checks the property of </id> end elem
			Assert.AreEqual (false, reader.IsStartElement (), "#1");
			Assert.AreEqual ("id", reader.Name, "#2");
			Assert.AreEqual ("id", reader.LocalName, "#3");			
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#4");
		}

		private void TestDiffGramElement2 (XmlReader reader)
		{
			//This method tests the properties of the <DummyItem> elemnent
			Assert.AreEqual (true, reader.IsStartElement (), "#1");
			Assert.AreEqual (3, reader.Depth, "#2");
			Assert.AreEqual (false, reader.HasAttributes, "#3");
			Assert.AreEqual (false, reader.HasValue, "#4");
			Assert.AreEqual (false, reader.IsDefault, "#5");
			Assert.AreEqual (false, reader.IsEmptyElement, "#6");
			Assert.AreEqual ("DummyItem", reader.Name, "#7");
			Assert.AreEqual ("DummyItem", reader.LocalName, "#8");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#9");
		}

		private void TestDiffGramElement4 (XmlReader reader)
		{
			//This method checks the properties of </DummyItem> end element
			Assert.AreEqual (false, reader.IsStartElement (), "#1");
			Assert.AreEqual ("DummyItem", reader.Name, "#2");
			Assert.AreEqual ("DummyItem", reader.LocalName, "#3");			
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#4");
		}
		
		private void TestDiffGramElement5 (XmlReader reader)
		{
			//This method check the properties of </DummyTable> end element
			Assert.AreEqual (false, reader.IsStartElement (), "#1");
			Assert.AreEqual ("DummyTable", reader.Name, "#2");
			Assert.AreEqual ("DummyTable", reader.LocalName, "#3");			
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#4");
		}

		[Test]
		public void XmlTest1 ()
		{
			MakeParentTable1 ();	
			dataSet.Tables.Remove (parentTable1);

			FileStream stream = new FileStream (fileName1, FileMode.Create);
			//Write the XML without any Schema information
			parentTable1.WriteXml (stream);
			stream.Close ();

			DataTable table = new DataTable ();
			try {
				stream = new FileStream (fileName1, FileMode.Open);
				table.ReadXml (stream);
				//Should throw an exception if the Xml File has no schema and target table too 	
				//does not define any schema
				Assert.Fail ("#1 Exception was Expected");
			} catch (Exception e) {
			 	Assert.AreEqual ("System.InvalidOperationException", e.GetType ().ToString (), "#1 Exception Type Not Correct");											
			} finally {
				stream.Close ();
			}
		}

		[Test]
		public void XmlTest2 ()
		{
			//Make a table without any relations
			MakeParentTable1 ();	
			dataSet.Tables.Remove (parentTable1);

			FileStream stream = new FileStream (fileName1, FileMode.Create);
			//Write Xml along with the Schema
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema);
			stream.Close ();

			DataTable table = new DataTable ();
			//Read the Xml and the Schema into a table which does not belongs to any DataSet
			table.ReadXml (fileName1);
			TestTableSchema (table, parentTable1.TableName, parentTable1.DataSet);
		}

		[Test]
		public void XmlTest3 ()
		{
			//Make a table without any Relations
			MakeParentTable1 ();	
			dataSet.Tables.Remove (parentTable1);
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			//Write the Xml and the Schema
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema);
			stream.Close ();

			DataTable table = new DataTable ();
			dataSet.Tables.Add (table);
			
			//Read the Xml and the Schema into a table which already belongs to a DataSet
			//and the table name does not match with the table ion the source XML 
			try {
				table.ReadXml (fileName1);
				Assert.Fail ("#1 Exception was expected as table names dont match");
			} catch (Exception e) {
				Assert.AreEqual ("System.ArgumentException", e.GetType ().ToString(), "#2");
			}
		}

		[Test]
		public void XmlTest4 ()
		{
			MakeParentTable1 ();	
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			//Here the table belong to a dataset
			//Write the Xml and the Schema
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema);
			stream.Close ();

			DataTable table = new DataTable ("ParentTable");
			//Read the Xml and the Schema into a table which already belongs to a DataSet
			//and the table name matches with the table in the source XML 
			table.ReadXml (fileName1);
			TestTableSchema (table, parentTable1.TableName, null);
		}

		[Test]
		public void XmlTest5 ()
		{
			//Create a parent table and create child tables
			MakeParentTable1 ();	
			MakeChildTable ();
			MakeSecondChildTable ();
			//Relate the parent and the children
			MakeDataRelation ();

			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema, false);
			stream.Close ();

			DataTable table = new DataTable ();
			table.ReadXml (fileName1);
			TestTableSchema (table, parentTable1.TableName, null);
		}

		[Test]
		public void XmlTest6 ()
		{
			//Create a parent table and create child tables
			MakeParentTable1 ();	
			MakeChildTable ();
			MakeSecondChildTable ();
			//Relate the parent and the children
			MakeDataRelation ();

			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema, true);
			stream.Close ();
			
			DataTable table = new DataTable ();
			table.ReadXml (fileName1);
					
			TestTable_WithChildren (table, parentTable1.TableName, parentTable1.DataSet);
		
			
			//Check Properties of First Child Table
			DataTable firstChildTable = table.ChildRelations [0].ChildTable; 
			Assert.AreEqual ("", firstChildTable.Namespace, "#1");
                        Assert.AreEqual ("XmlDataSet", firstChildTable.DataSet.DataSetName, "#2");  
                        Assert.AreEqual (3, firstChildTable.Columns.Count, "#3");
			Assert.AreEqual ("System.Int32", firstChildTable.Columns [0].DataType.ToString (), "#4");
			Assert.AreEqual ("System.String", firstChildTable.Columns [1].DataType.ToString (), "#5");	
			Assert.AreEqual ("System.Int32", firstChildTable.Columns [2].DataType.ToString (), "#6");
                        Assert.AreEqual (6, firstChildTable.Rows.Count, "#7");  
                        Assert.AreEqual (false, firstChildTable.CaseSensitive, "#8");
			Assert.AreEqual ("ChildTable", firstChildTable.TableName, "#9");
			Assert.AreEqual ("", firstChildTable.Prefix, "#10"); 
                        Assert.AreEqual (2, firstChildTable.Constraints.Count, "#11");
			Assert.AreEqual ("Constraint1", firstChildTable.Constraints [0].ToString (), "#12");
			Assert.AreEqual ("ParentChild_Relation1", firstChildTable.Constraints [1].ToString (), "#13");
			Assert.AreEqual (1, firstChildTable.ParentRelations.Count, "#14");
			Assert.AreEqual ("ParentTable", firstChildTable.ParentRelations [0].ParentTable.TableName, "#15");
			Assert.AreEqual (0, firstChildTable.ChildRelations.Count, "#16");
			Assert.AreEqual (0, firstChildTable.PrimaryKey.Length, "#17");
			
			
			//Check Properties of Second Child Table
			DataTable secondChildTable = table.ChildRelations [1].ChildTable;
			Assert.AreEqual ("", secondChildTable.Namespace, "#18");
                        Assert.AreEqual ("XmlDataSet", secondChildTable.DataSet.DataSetName, "#19");  
                        Assert.AreEqual (4, secondChildTable.Columns.Count, "#20");
			Assert.AreEqual ("System.Int32", secondChildTable.Columns [0].DataType.ToString (), "#21");
			Assert.AreEqual ("System.String", secondChildTable.Columns [1].DataType.ToString (), "#22");
			Assert.AreEqual ("System.Int32", secondChildTable.Columns [2].DataType.ToString (), "#23");
			Assert.AreEqual ("System.Int32", secondChildTable.Columns [3].DataType.ToString (), "#24");
                        Assert.AreEqual (6, secondChildTable.Rows.Count, "#25");  
                        Assert.AreEqual (false, secondChildTable.CaseSensitive, "#26");
			Assert.AreEqual ("SecondChildTable", secondChildTable.TableName, "#27");
			Assert.AreEqual ("", secondChildTable.Prefix, "#28"); 
                        Assert.AreEqual (2, secondChildTable.Constraints.Count, "#29");
			Assert.AreEqual ("Constraint1", secondChildTable.Constraints [0].ToString (), "#30");
			Assert.AreEqual ("ParentChild_Relation2", secondChildTable.Constraints [1].ToString (), "#31");
			Assert.AreEqual (1, secondChildTable.ParentRelations.Count, "#32");;
			Assert.AreEqual ("ParentTable", secondChildTable.ParentRelations [0].ParentTable.TableName, "#33");
			Assert.AreEqual (0, secondChildTable.ChildRelations.Count, "#34");
			Assert.AreEqual (0, secondChildTable.PrimaryKey.Length, "#35");
			
		}

		[Test]
		public void XmlTest7 ()
		{
			//Create a parent table and create child tables
			MakeParentTable1 ();	
			MakeChildTable ();
			MakeSecondChildTable ();
			//Relate the parent and the children
			MakeDataRelation ();

			FileStream stream = new FileStream (fileName1, FileMode.Create);
			//WriteXml on any of the children
			childTable.WriteXml (stream, XmlWriteMode.WriteSchema, false);
			stream.Close ();
		
			
			DataTable table = new DataTable ();
			table.ReadXml (fileName1);
			
			//Test Schema 
			//Check Properties of Table
			Assert.AreEqual ("", table.Namespace, "#1");
                        Assert.AreEqual (null, table.DataSet, "#2");  
                        Assert.AreEqual (3, table.Columns.Count, "#3");
                        Assert.AreEqual (false, table.CaseSensitive, "#5");
			Assert.AreEqual ("ChildTable", table.TableName, "#6");
			Assert.AreEqual ("", table.Prefix, "#7"); 
			Assert.AreEqual (1, table.Constraints.Count, "#8");
			Assert.AreEqual ("Constraint1", table.Constraints [0].ToString (), "#9");
			Assert.AreEqual ("System.Data.UniqueConstraint", table.Constraints [0].GetType ().ToString (), "#10");
			Assert.AreEqual (0, table.PrimaryKey.Length, "#11");
			Assert.AreEqual (0, table.ParentRelations.Count, "#12");
			Assert.AreEqual (0, table.ChildRelations.Count, "#13");

			
			//Check properties of each column
			//First Column
			DataColumn col = table.Columns [0];
                        Assert.AreEqual (true, col.AllowDBNull, "#14"); 
                       	Assert.AreEqual (0, col.AutoIncrementSeed, "#15"); 
                        Assert.AreEqual (1, col.AutoIncrementStep, "#16"); 
                        Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#17"); 
                        Assert.AreEqual ("ChildID", col.Caption, "#18"); 
                        Assert.AreEqual ("ChildID", col.ColumnName, "#19"); 
                        Assert.AreEqual ("System.Int32", col.DataType.ToString (), "#20"); 
                        Assert.AreEqual ("", col.DefaultValue.ToString (), "#21"); 
                        Assert.AreEqual (false, col.DesignMode, "#22"); 
                        Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#23");
			Assert.AreEqual (-1, col.MaxLength, "#24"); 
                        Assert.AreEqual (0, col.Ordinal, "#25"); 
                        Assert.AreEqual ("", col.Prefix, "#26"); 
                        Assert.AreEqual ("ChildTable", col.Table.ToString (), "#27"); 
                        Assert.AreEqual (true, col.Unique, "#28");

			
			//Second Column
			col = table.Columns [1];
			Assert.AreEqual (true, col.AllowDBNull, "#29");
			Assert.AreEqual (0, col.AutoIncrementSeed, "#30");
			Assert.AreEqual (1, col.AutoIncrementStep, "#31");
			Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#32");
			Assert.AreEqual ("ChildItem", col.Caption, "#33");
			Assert.AreEqual ("ChildItem", col.ColumnName, "#34");
			Assert.AreEqual ("System.String", col.DataType.ToString (), "#35");
			Assert.AreEqual ("", col.DefaultValue.ToString (), "#36");
			Assert.AreEqual (false, col.DesignMode, "#37");
			Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#38");
			Assert.AreEqual (-1, col.MaxLength, "#39");
			Assert.AreEqual (1, col.Ordinal, "#40");
			Assert.AreEqual ("", col.Prefix, "#41");
			Assert.AreEqual ("ChildTable", col.Table.ToString (), "#42");
			Assert.AreEqual (false, col.Unique, "#43");

			//Third Column
			col = table.Columns [2];
			Assert.AreEqual (true, col.AllowDBNull, "#44");
			Assert.AreEqual (false, col.AutoIncrement, "#45");
			Assert.AreEqual (0, col.AutoIncrementSeed, "#46");
			Assert.AreEqual (1, col.AutoIncrementStep, "#47");
			Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#48");
			Assert.AreEqual ("ParentID", col.Caption, "#49");
			Assert.AreEqual ("ParentID", col.ColumnName, "#50");
			Assert.AreEqual ("System.Int32", col.DataType.ToString (), "#51");
			Assert.AreEqual ("", col.DefaultValue.ToString (), "#52");
			Assert.AreEqual (false, col.DesignMode, "#53");
			Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#54");
			Assert.AreEqual (-1, col.MaxLength, "#55");
			Assert.AreEqual (2, col.Ordinal, "#56");
			Assert.AreEqual ("", col.Prefix, "#57");
			Assert.AreEqual ("ChildTable", col.Table.ToString (), "#58");
			Assert.AreEqual (false, col.Unique, "#59");

			
			//Test the Xml
			Assert.AreEqual (6, table.Rows.Count, "#60");

			//Test values of each row
			DataRow row = table.Rows [0];
			Assert.AreEqual (1, row ["ChildID"], "#61");
			Assert.AreEqual ("ChildItem 1", row ["ChildItem"], "#62");
			Assert.AreEqual (1, row ["ParentID"], "#63");

			row = table.Rows [1];
			Assert.AreEqual (2, row ["ChildID"], "#64");
			Assert.AreEqual ("ChildItem 2", row ["ChildItem"], "#65");
			Assert.AreEqual (1, row ["ParentID"], "#66");

			row = table.Rows [2];
			Assert.AreEqual (5, row ["ChildID"], "#67");
			Assert.AreEqual ("ChildItem 1", row ["ChildItem"], "#68");
			Assert.AreEqual (2, row ["ParentID"], "#69");

			row = table.Rows [3];
			Assert.AreEqual (6, row ["ChildID"], "#70");
			Assert.AreEqual ("ChildItem 2", row ["ChildItem"], "#71");
			Assert.AreEqual (2, row ["ParentID"], "#72");
	
			row = table.Rows [4];
			Assert.AreEqual (10, row ["ChildID"], "#73");
			Assert.AreEqual ("ChildItem 1", row ["ChildItem"], "#74");
			Assert.AreEqual (3, row ["ParentID"], "#75");

			row = table.Rows [5];
			Assert.AreEqual (11, row ["ChildID"], "#75");
			Assert.AreEqual ("ChildItem 2", row ["ChildItem"], "#76");
			Assert.AreEqual (3, row ["ParentID"], "#77");
			
		}

		[Test]
		public void XmlTest8 ()
		{
			MakeParentTable1 ();	
			MakeChildTable ();
			MakeSecondChildTable ();
			//Relate the parent and the children
			MakeDataRelation ();
			
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			//Write only the Xml
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema, false);
			stream.Close ();

			DataSet ds = new DataSet ();
			ds.ReadXml (fileName1);

			Assert.AreEqual (1, ds.Tables.Count, "#1");
			Assert.AreEqual ("ParentTable", ds.Tables [0].TableName, "#2");
			DataTable table = ds.Tables [0];
			
			
			Assert.AreEqual (3, table.Rows.Count, "#3");
			//Test values of each row
			DataRow row = table.Rows [0];
			Assert.AreEqual ("1", row ["id"], "#4");
			Assert.AreEqual ("ParentItem 1", row ["ParentItem"], "#5");
			Assert.AreEqual ("1", row ["DepartmentID"], "#6");

			row = table.Rows [1];
			Assert.AreEqual ("2", row ["id"], "#7");
			Assert.AreEqual ("ParentItem 2", row ["ParentItem"], "#8");
			Assert.AreEqual ("2", row ["DepartmentID"], "#9");

			row = table.Rows [2];
			Assert.AreEqual ("3", row ["id"], "#10");
			Assert.AreEqual ("ParentItem 3", row ["ParentItem"], "#11");
			Assert.AreEqual ("3", row ["DepartmentID"], "#12");

		}

		[Test]
		public void XmlTest9 ()
		{
			MakeParentTable1 ();	
			MakeChildTable ();
			MakeSecondChildTable ();
			//Relate the parent and the children
			MakeDataRelation ();
			
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			//Write only the Xml
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema, true);
			stream.Close ();

			DataSet ds = new DataSet ();
			ds.ReadXml (fileName1);

			Assert.AreEqual (3, ds.Tables.Count, "#1");
			Assert.AreEqual ("ParentTable", ds.Tables [0].TableName, "#2");
			Assert.AreEqual ("ChildTable", ds.Tables [1].TableName, "#3");
			Assert.AreEqual ("SecondChildTable", ds.Tables [2].TableName, "#4");

			//get the first table
			DataTable table = ds.Tables [0];
			Assert.AreEqual (3, table.Rows.Count, "#5");
			
			DataRow row = table.Rows [0];
			Assert.AreEqual ("1", row ["id"], "#6");
			Assert.AreEqual ("ParentItem 1", row ["ParentItem"], "#7");
			Assert.AreEqual ("1", row ["DepartmentID"], "#8");

			row = table.Rows [1];
			Assert.AreEqual ("2", row ["id"], "#9");
			Assert.AreEqual ("ParentItem 2", row ["ParentItem"], "#10");
			Assert.AreEqual ("2", row ["DepartmentID"], "#11");

			row = table.Rows [2];
			Assert.AreEqual ("3", row ["id"], "#12");
			Assert.AreEqual ("ParentItem 3", row ["ParentItem"], "#13");
			Assert.AreEqual ("3", row ["DepartmentID"], "#14");
		
			
			//get the second table
			table = ds.Tables [1];	
			Assert.AreEqual (6, table.Rows.Count);

			row = table.Rows [0];
			Assert.AreEqual ("1", row ["ChildID"], "#15");
			Assert.AreEqual ("ChildItem 1", row ["ChildItem"], "#16");
			Assert.AreEqual ("1", row ["ParentID"], "#17");

			row = table.Rows [1];
			Assert.AreEqual ("2", row ["ChildID"], "#18");
			Assert.AreEqual ("ChildItem 2", row ["ChildItem"], "#19");
			Assert.AreEqual ("1", row ["ParentID"], "#20");

			row = table.Rows [2];
			Assert.AreEqual ("5", row ["ChildID"], "#21");
			Assert.AreEqual ("ChildItem 1", row ["ChildItem"], "#22");
			Assert.AreEqual ("2", row ["ParentID"], "#23");

			row = table.Rows [3];
			Assert.AreEqual ("6", row ["ChildID"], "#24");
			Assert.AreEqual ("ChildItem 2", row ["ChildItem"], "#25");
			Assert.AreEqual ("2", row ["ParentID"], "#26");
	
			row = table.Rows [4];
			Assert.AreEqual ("10", row ["ChildID"], "#27");
			Assert.AreEqual ("ChildItem 1", row ["ChildItem"], "#28");
			Assert.AreEqual ("3", row ["ParentID"], "#29");

			row = table.Rows [5];
			Assert.AreEqual ("11", row ["ChildID"], "#30");
			Assert.AreEqual ("ChildItem 2", row ["ChildItem"], "#31");
			Assert.AreEqual ("3", row ["ParentID"], "#32");
			
			//get the third table
			table = ds.Tables [2];	
			Assert.AreEqual (6, table.Rows.Count);

			row = table.Rows [0];
			Assert.AreEqual ("1", row ["ChildID"], "#33");
			Assert.AreEqual ("SecondChildItem 1", row ["ChildItem"], "#34");
			Assert.AreEqual ("1", row ["ParentID"], "#35");
			Assert.AreEqual ("1", row ["DepartmentID"], "#36");

			row = table.Rows [1];
			Assert.AreEqual ("2", row ["ChildID"], "#37");
			Assert.AreEqual ("SecondChildItem 2", row ["ChildItem"], "#38");
			Assert.AreEqual ("1", row ["ParentID"], "#39");
			Assert.AreEqual ("1", row ["DepartmentID"], "#40");

			row = table.Rows [2];
			Assert.AreEqual ("5", row ["ChildID"], "#41");
			Assert.AreEqual ("SecondChildItem 1", row ["ChildItem"], "#42");
			Assert.AreEqual ("2", row ["ParentID"], "#43");
			Assert.AreEqual ("2", row ["DepartmentID"], "#44");	

			row = table.Rows [3];
			Assert.AreEqual ("6", row ["ChildID"], "#45");
			Assert.AreEqual ("SecondChildItem 2", row ["ChildItem"], "#46");
			Assert.AreEqual ("2", row ["ParentID"], "#47");
			Assert.AreEqual ("2", row ["DepartmentID"], "#48");
	
			row = table.Rows [4];
			Assert.AreEqual ("10", row ["ChildID"], "#49");
			Assert.AreEqual ("SecondChildItem 1", row ["ChildItem"], "#50");
			Assert.AreEqual ("3", row ["ParentID"], "#51");
			Assert.AreEqual ("3", row ["DepartmentID"], "#52");

			row = table.Rows [5];
			Assert.AreEqual ("11", row ["ChildID"], "#53");
			Assert.AreEqual ("SecondChildItem 2", row ["ChildItem"], "#54");
			Assert.AreEqual ("3", row ["ParentID"], "#55");
			Assert.AreEqual ("3", row ["DepartmentID"], "#56");

		}

		[Test]	
		public void XmlTest10 ()
		{
			MakeDummyTable ();

			FileStream stream = new FileStream (fileName1, FileMode.Create);
			dummyTable.WriteXml (stream, XmlWriteMode.DiffGram);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.IgnoreWhitespace = true;
			XmlReader reader = XmlReader.Create (stream, settings);
			
			reader.Read ();
			Assert.AreEqual (true, reader.IsStartElement (), "#1");
			Assert.AreEqual (0, reader.Depth, "#2");
			Assert.AreEqual (true, reader.HasAttributes, "#3");
			Assert.AreEqual (false, reader.HasValue, "#4");
			Assert.AreEqual (false, reader.IsDefault, "#5");
			Assert.AreEqual (false, reader.IsEmptyElement, "#6");
			Assert.AreEqual ("diffgr:diffgram", reader.Name, "#7");
			Assert.AreEqual ("diffgram", reader.LocalName, "#8");
			Assert.AreEqual (2, reader.AttributeCount, "#9");
			Assert.AreEqual ("urn:schemas-microsoft-com:xml-msdata", reader [0], "#10");
			Assert.AreEqual ("urn:schemas-microsoft-com:xml-diffgram-v1", reader [1], "#11");
			Assert.AreEqual ("urn:schemas-microsoft-com:xml-diffgram-v1", reader.NamespaceURI, "#12");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#13");

			reader.Read ();	
			Assert.AreEqual (true, reader.IsStartElement (), "#14");
			Assert.AreEqual (1, reader.Depth, "#15");
			Assert.AreEqual (false, reader.HasAttributes, "#16");
			Assert.AreEqual (false, reader.HasValue, "#17");
			Assert.AreEqual (false, reader.IsDefault, "#18");
			Assert.AreEqual (false, reader.IsEmptyElement, "#19");
			Assert.AreEqual ("NewDataSet", reader.Name, "#20");
			Assert.AreEqual ("NewDataSet", reader.LocalName, "#21");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#22");

			reader.Read ();
			Assert.AreEqual (true, reader.IsStartElement (), "#23");
			Assert.AreEqual (2, reader.Depth, "#24");
			Assert.AreEqual (true, reader.HasAttributes, "#25");
			Assert.AreEqual (false, reader.HasValue, "#26");
			Assert.AreEqual (false, reader.IsDefault, "#27");
			Assert.AreEqual (false, reader.IsEmptyElement, "#28");
			Assert.AreEqual ("DummyTable", reader.Name, "#29");
			Assert.AreEqual ("DummyTable", reader.LocalName, "#30");
			Assert.AreEqual (2, reader.AttributeCount, "#31");
			Assert.AreEqual ("DummyTable1", reader [0], "#32");
			Assert.AreEqual ("0", reader [1], "#33");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#34");

			reader.Read ();
			TestDiffGramElement1 (reader);

			reader.Read ();
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#35");
			Assert.AreEqual (true, reader.HasValue, "#36");
			Assert.AreEqual ("1", reader.Value, "#37");

			reader.Read ();
			TestDiffGramElement3 (reader);
	
			reader.Read ();		
			TestDiffGramElement2 (reader);

			reader.Read ();
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#38");
			Assert.AreEqual (true, reader.HasValue, "#39");
			Assert.AreEqual ("DummyItem 1", reader.Value, "#40");

			reader.Read ();
			Assert.AreEqual (false, reader.IsStartElement (), "#41");
			Assert.AreEqual ("DummyItem", reader.Name, "#42");
			Assert.AreEqual ("DummyItem", reader.LocalName, "#43");			
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#44");
	
			reader.Read ();
			Assert.AreEqual (false, reader.IsStartElement (), "#45");
			Assert.AreEqual ("DummyTable", reader.Name, "#46");
			Assert.AreEqual ("DummyTable", reader.LocalName, "#47");			
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#48");
		
			reader.Read ();
			Assert.AreEqual (true, reader.IsStartElement (), "#49");
			Assert.AreEqual (2, reader.Depth, "#50");
			Assert.AreEqual (true, reader.HasAttributes, "#51");
			Assert.AreEqual (false, reader.HasValue, "#52");
			Assert.AreEqual (false, reader.IsDefault, "#53");
			Assert.AreEqual (false, reader.IsEmptyElement, "#54");
			Assert.AreEqual ("DummyTable", reader.Name, "#55");
			Assert.AreEqual ("DummyTable", reader.LocalName, "#56");
			Assert.AreEqual (3, reader.AttributeCount, "#57");
			Assert.AreEqual ("DummyTable2", reader [0], "#58");
			Assert.AreEqual ("1", reader [1], "#59");
			Assert.AreEqual ("modified", reader [2], "#60");	
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#61");

			reader.Read ();
			TestDiffGramElement1 (reader);
			 
			reader.Read ();
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#62");
			Assert.AreEqual (true, reader.HasValue, "#63");
			Assert.AreEqual ("2", reader.Value, "#64");

			reader.Read ();
			TestDiffGramElement3 (reader);
		
			reader.Read ();	
			TestDiffGramElement2 (reader);
	
			reader.Read ();
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#65");
			Assert.AreEqual (true, reader.HasValue, "#66");
			Assert.AreEqual ("Changed_DummyItem 2", reader.Value, "#67");

			reader.Read ();
			TestDiffGramElement4 (reader);

			reader.Read ();
			TestDiffGramElement5 (reader);	

			reader.Read ();
			Assert.AreEqual (true, reader.IsStartElement (), "#68");
			Assert.AreEqual (2, reader.Depth, "#69");
			Assert.AreEqual (true, reader.HasAttributes, "#70");
			Assert.AreEqual (false, reader.HasValue, "#71");
			Assert.AreEqual (false, reader.IsDefault, "#72");
			Assert.AreEqual (false, reader.IsEmptyElement, "#73");
			Assert.AreEqual ("DummyTable", reader.Name, "#74");
			Assert.AreEqual ("DummyTable", reader.LocalName, "#75");
			Assert.AreEqual (2, reader.AttributeCount, "#76");
			Assert.AreEqual ("DummyTable3", reader [0], "#77");
			Assert.AreEqual ("2", reader [1], "#78");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#79");

			reader.Read ();
			TestDiffGramElement1 (reader);

			reader.Read ();
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#80");
			Assert.AreEqual (true, reader.HasValue, "#81");
			Assert.AreEqual ("3", reader.Value, "#82");

			reader.Read ();
			TestDiffGramElement3 (reader);

			reader.Read ();
			TestDiffGramElement2 (reader);

	
			reader.Read();
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#83");
			Assert.AreEqual (true, reader.HasValue, "#84");
			Assert.AreEqual ("DummyItem 3", reader.Value, "#85");

			reader.Read ();
			TestDiffGramElement4 (reader);

			reader.Read ();
			TestDiffGramElement5 (reader);

			reader.Read ();
			Assert.AreEqual (false, reader.IsStartElement (), "#86");
			Assert.AreEqual ("NewDataSet", reader.Name, "#87");
			Assert.AreEqual ("NewDataSet", reader.LocalName, "#88");			
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#89");
	
			reader.Read ();
			Assert.AreEqual (true, reader.IsStartElement (), "#90");
			Assert.AreEqual (1, reader.Depth, "#91");
			Assert.AreEqual (false, reader.HasAttributes, "#92");
			Assert.AreEqual (false, reader.HasValue, "#93");
			Assert.AreEqual (false, reader.IsDefault, "#94");
			Assert.AreEqual (false, reader.IsEmptyElement, "#95");
			Assert.AreEqual ("diffgr:before", reader.Name, "#96");
			Assert.AreEqual ("before", reader.LocalName, "#97");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#98");

			reader.Read ();
			Assert.AreEqual (true, reader.IsStartElement (), "#99");
			Assert.AreEqual (2, reader.Depth, "#100");
			Assert.AreEqual (true, reader.HasAttributes, "#101");
			Assert.AreEqual (false, reader.HasValue, "#102");
			Assert.AreEqual (false, reader.IsDefault, "#103");
			Assert.AreEqual (false, reader.IsEmptyElement, "#104");
			Assert.AreEqual ("DummyTable", reader.Name, "#105");
			Assert.AreEqual ("DummyTable", reader.LocalName, "#106");
			Assert.AreEqual (2, reader.AttributeCount, "#107");
			Assert.AreEqual ("DummyTable2", reader [0], "#108");
			Assert.AreEqual ("1", reader [1], "#109");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#110");

			reader.Read ();
			TestDiffGramElement1 (reader);

			reader.Read ();
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#111");
			Assert.AreEqual (true, reader.HasValue, "#112");
			Assert.AreEqual ("2", reader.Value, "#113");

			reader.Read ();
			TestDiffGramElement3 (reader);

			reader.Read ();
			TestDiffGramElement2 (reader);

			reader.Read ();
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#114");
			Assert.AreEqual (true, reader.HasValue, "#115");
			Assert.AreEqual ("DummyItem 2", reader.Value, "#116");
			
			reader.Read ();
			TestDiffGramElement4 (reader);		
			
			reader.Read ();
			TestDiffGramElement5 (reader);

			reader.Read ();
			Assert.AreEqual (false, reader.IsStartElement (), "#117");
			Assert.AreEqual ("diffgr:before", reader.Name, "#118");
			Assert.AreEqual ("before", reader.LocalName, "#119");			
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#120");

			reader.Read ();
			Assert.AreEqual (false, reader.IsStartElement (), "#121");
			Assert.AreEqual ("diffgr:diffgram", reader.Name, "#122");
			Assert.AreEqual ("diffgram", reader.LocalName, "#123");			
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#124");

			stream.Close ();
		} 

		[Test]
		public void XmlTest11 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			XmlReadMode mode = XmlReadMode.Auto ;
			DataTable table = new DataTable ();
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			//This should not read anything as table name is not set	
			mode = table.ReadXml (stream);
			stream.Close ();		

			Assert.AreEqual (XmlReadMode.IgnoreSchema, mode, "#1");
			Assert.AreEqual (String.Empty, table.TableName, "#2");
			Assert.AreEqual (0, table.Rows.Count, "#3");
			Assert.AreEqual (1, table.Columns.Count, "#4");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#5");
			Assert.AreEqual (null, table.DataSet, "#6");	
				
		}

		[Test]
		public void XmlTest12 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			DataTable table = new DataTable ("Table1");
			XmlReadMode mode = XmlReadMode.Auto;
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			//Same as last test. ReadXml does not read anything as table names dont match
			mode = table.ReadXml (stream);
			stream.Close ();		

			Assert.AreEqual (XmlReadMode.IgnoreSchema, mode, "#1");
			Assert.AreEqual ("Table1", table.TableName, "#2");
			Assert.AreEqual (0, table.Rows.Count, "#3");
			Assert.AreEqual (1, table.Columns.Count, "#4");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#5");
			Assert.AreEqual (null, table.DataSet, "#6");	
				
		}

		[Test]
		public void XmlTest13 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			DataTable table = new DataTable ("ParentTable");
			XmlReadMode mode = XmlReadMode.Auto;
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.String")));
			mode = table.ReadXml (stream);
			stream.Close ();		

			Assert.AreEqual (XmlReadMode.IgnoreSchema, mode, "#1");
			Assert.AreEqual ("ParentTable", table.TableName, "#2");
			Assert.AreEqual (3, table.Rows.Count, "#3");
			Assert.AreEqual (1, table.Columns.Count, "#4");
			Assert.AreEqual ("System.String", table.Columns [0].DataType.ToString (), "#5");
			Assert.AreEqual (null, table.DataSet, "#6");	

			//Check Rows
			DataRow row = table.Rows [0];
			Assert.AreEqual ("1", row [0], "#7");
			
			row = table.Rows [1];
			Assert.AreEqual ("2", row [0], "#8");

			row = table.Rows [2];
			Assert.AreEqual ("3", row [0], "#9");
		}

		[Test]
		public void XmlTest14 ()
		{
			
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			//Create a target table which has nomatching column(s) names
			DataTable table = new DataTable ("ParentTable");
			XmlReadMode mode = XmlReadMode.Auto;
			table.Columns.Add (new DataColumn ("sid", System.Type.GetType ("System.String")));
			//ReadXml does not read anything as the column names are not matching
			mode = table.ReadXml (stream);
			stream.Close ();		

			Assert.AreEqual (XmlReadMode.IgnoreSchema, mode, "#1");
			Assert.AreEqual ("ParentTable", table.TableName, "#2");
			Assert.AreEqual (3, table.Rows.Count, "#3");
			Assert.AreEqual (1, table.Columns.Count, "#4");
			Assert.AreEqual ("sid", table.Columns [0].ColumnName, "#5");
			Assert.AreEqual ("System.String", table.Columns [0].DataType.ToString (), "#6");
			Assert.AreEqual (null, table.DataSet, "#7");

			//Check the rows
			foreach (DataRow row in table.Rows)
				Assert.AreEqual (null, row [0], "#8"); 
		}

		[Test]
		public void XmlTest15 ()
		{
			
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			//Create a target table which has matching column(s) name and an extra column
			DataTable table = new DataTable ("ParentTable");
			XmlReadMode mode = XmlReadMode.Auto;
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			table.Columns.Add (new DataColumn ("ParentItem", System.Type.GetType ("System.String")));
			table.Columns.Add (new DataColumn ("DepartmentID", System.Type.GetType ("System.Int32")));
			table.Columns.Add (new DataColumn ("DummyColumn", System.Type.GetType ("System.String")));

			mode = table.ReadXml (stream);
			stream.Close ();		

			Assert.AreEqual (XmlReadMode.IgnoreSchema, mode, "#1");
			Assert.AreEqual ("ParentTable", table.TableName, "#2");
			Assert.AreEqual (3, table.Rows.Count, "#3");
			Assert.AreEqual (4, table.Columns.Count, "#4");
			Assert.AreEqual (null, table.DataSet, "#5");
		
			//Check the Columns
			Assert.AreEqual ("id", table.Columns [0].ColumnName, "#6");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#7");

			Assert.AreEqual ("ParentItem", table.Columns [1].ColumnName, "#8");
			Assert.AreEqual ("System.String", table.Columns [1].DataType.ToString (), "#9");

			Assert.AreEqual ("DepartmentID", table.Columns [2].ColumnName, "#10");
			Assert.AreEqual ("System.Int32", table.Columns [2].DataType.ToString (), "#11");

			Assert.AreEqual ("DummyColumn", table.Columns [3].ColumnName, "#12");
			Assert.AreEqual ("System.String", table.Columns [3].DataType.ToString (), "#13");

			//Check the rows
			DataRow row = table.Rows [0];
			Assert.AreEqual (1, row ["id"], "#14");
			Assert.AreEqual ("ParentItem 1", row ["ParentItem"], "#15");
			Assert.AreEqual (1, row ["DepartmentID"], "#16");
			Assert.AreEqual (null, row ["DummyColumn"], "#17");

			row = table.Rows [1];
			Assert.AreEqual (2, row ["id"], "#18");
			Assert.AreEqual ("ParentItem 2", row ["ParentItem"], "#19");
			Assert.AreEqual (2, row ["DepartmentID"], "#20");
			Assert.AreEqual (null, row ["DummyColumn"], "#21");


			row = table.Rows [2];
			Assert.AreEqual (3, row ["id"], "#22");
			Assert.AreEqual ("ParentItem 3", row ["ParentItem"], "#23");
			Assert.AreEqual (3, row ["DepartmentID"], "#24");
			Assert.AreEqual (null, row ["DummyColumn"], "#25");
		}

		[Test]
		public void XmlTest16 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			//Write the Xml with schema information
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			XmlReadMode mode = XmlReadMode.Auto ;
			DataTable table = new DataTable ();
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));

			mode = table.ReadXml (stream);

			stream.Close ();		
			Assert.AreEqual (XmlReadMode.ReadSchema, mode, "#1");
			Assert.AreEqual (String.Empty, table.TableName, "#2");
			Assert.AreEqual (0, table.Rows.Count, "#3");
			Assert.AreEqual (1, table.Columns.Count, "#4");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#5");
			Assert.AreEqual (null, table.DataSet, "#6");	
		}

		[Test]
		public void XmlTest17 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			DataTable table = new DataTable ("Table1");
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			try {
				table.ReadXml (stream);
				Assert.Fail ("#1 Exception was expected as table names dont match");
			} catch (Exception e) {
				Assert.AreEqual ("System.ArgumentException", e.GetType ().ToString (), "#1");		
			} finally {
				stream.Close ();		
			}
		}

		
		[Test]
		public void XmlTest18 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			DataTable table = new DataTable ("ParentTable");
			XmlReadMode mode = XmlReadMode.Auto;
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			table.Columns.Add (new DataColumn ("DepartmentID", System.Type.GetType ("System.Int32")));

			mode = table.ReadXml (stream);
			stream.Close ();		

			Assert.AreEqual (XmlReadMode.ReadSchema, mode, "#1");
			Assert.AreEqual ("ParentTable", table.TableName, "#2");
			Assert.AreEqual (3, table.Rows.Count, "#3");
			Assert.AreEqual (2, table.Columns.Count, "#4");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#5");
			Assert.AreEqual ("System.Int32", table.Columns [1].DataType.ToString (), "#6");
			Assert.AreEqual (null, table.DataSet, "#6");	

			//Check rows
			DataRow row = table.Rows [0];
			Assert.AreEqual (1, row [0], "#7");
			Assert.AreEqual (1, row [1], "#8");
			
			row = table.Rows [1];
			Assert.AreEqual (2, row [0], "#9");
			Assert.AreEqual (2, row [1], "#10");

			row = table.Rows [2];
			Assert.AreEqual (3, row [0], "#11");
			Assert.AreEqual (3, row [1], "#12");
		}
		
		[Test]
		public void XmlTest19 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			DataSet ds = new DataSet ();
			DataTable table = new DataTable ();
			XmlReadMode mode = XmlReadMode.Auto;
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			ds.Tables.Add (table);
			//ReadXml wont read anything as TableNames dont match
			mode = table.ReadXml (stream);
			stream.Close ();		

			Assert.AreEqual (XmlReadMode.IgnoreSchema, mode, "#1");
			Assert.AreEqual ("Table1", table.TableName, "#2");
			Assert.AreEqual (0, table.Rows.Count, "#3");
			Assert.AreEqual (1, table.Columns.Count, "#4");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#5");
			Assert.AreEqual ("System.Data.DataSet", table.DataSet.ToString (), "#6");	
			Assert.AreEqual ("NewDataSet", table.DataSet.DataSetName, "#7");
		}

		[Test]
		public void XmlTest20 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			DataSet ds = new DataSet ();
			DataTable table = new DataTable ("HelloWorldTable");
			XmlReadMode mode = XmlReadMode.Auto;
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			ds.Tables.Add (table);
			//ReadXml wont read anything as TableNames dont match
			mode = table.ReadXml (stream);
			stream.Close ();		

			Assert.AreEqual (XmlReadMode.IgnoreSchema, mode, "#1");
			Assert.AreEqual ("HelloWorldTable", table.TableName, "#2");
			Assert.AreEqual (0, table.Rows.Count, "#3");
			Assert.AreEqual (1, table.Columns.Count, "#4");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#5");
			Assert.AreEqual ("System.Data.DataSet", table.DataSet.ToString (), "#6");	
			Assert.AreEqual ("NewDataSet", table.DataSet.DataSetName, "#7");
		}

		[Test]
		public void XmlTest21 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.IgnoreSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			DataSet ds = new DataSet ();
			DataTable table = new DataTable ("ParentTable");
			XmlReadMode mode = XmlReadMode.Auto;
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			ds.Tables.Add (table);
			mode = table.ReadXml (stream);
			stream.Close ();		

			Assert.AreEqual (XmlReadMode.IgnoreSchema, mode, "#1");
			Assert.AreEqual ("ParentTable", table.TableName, "#2");
			Assert.AreEqual (3, table.Rows.Count, "#3");
			Assert.AreEqual (1, table.Columns.Count, "#4");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#5");
			Assert.AreEqual ("System.Data.DataSet", table.DataSet.ToString (), "#6");
			Assert.AreEqual ("NewDataSet", table.DataSet.DataSetName, "#7");	

			//Check the rows
			DataRow row = table.Rows [0];
			Assert.AreEqual (1, row [0], "#8");
			
			row = table.Rows [1];
			Assert.AreEqual (2, row [0], "#9");

			row = table.Rows [2];
			Assert.AreEqual (3, row [0], "#10");
		}

		[Test]
		public void XmlTest22 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			DataSet ds = new DataSet ();
			DataTable table = new DataTable ("Table1");
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			ds.Tables.Add (table);
 
			try {
				table.ReadXml (stream);
				Assert.Fail ("#1 Exception was expected");
			} catch (Exception e) {
				Assert.AreEqual ("System.ArgumentException", e.GetType ().ToString (), "#1 Incorrect Exception Type");	
			} finally {
				stream.Close ();
			}
		}	

		[Test]
		public void XmlTest23 ()
		{
			MakeParentTable1 ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.WriteSchema);
			stream.Close ();

			stream = new FileStream (fileName1, FileMode.Open);
			DataSet ds = new DataSet ();
			DataTable table = new DataTable ("ParentTable");
			XmlReadMode mode = XmlReadMode.Auto;
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			table.Columns.Add (new DataColumn ("DepartmentID", System.Type.GetType ("System.String")));

			ds.Tables.Add (table);
			mode = table.ReadXml (stream);
			stream.Close ();

			Assert.AreEqual (XmlReadMode.ReadSchema, mode, "#1");
			Assert.AreEqual ("ParentTable", table.TableName, "#2");
			Assert.AreEqual ("NewDataSet", table.DataSet.DataSetName, "#3");
			Assert.AreEqual (3, table.Rows.Count, "#4");
			Assert.AreEqual (2, table.Columns.Count, "#5");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#6");
			Assert.AreEqual ("System.String", table.Columns [1].DataType.ToString (), "#7");

			//Check rows
			DataRow row = table.Rows [0];
			Assert.AreEqual (1, row [0], "#8");
			Assert.AreEqual ("1", row [1], "#9");
			
			row = table.Rows [1];
			Assert.AreEqual (2, row [0], "#10");
			Assert.AreEqual ("2", row [1], "#11");

			row = table.Rows [2];
			Assert.AreEqual (3, row [0], "#12");
			Assert.AreEqual ("3", row [1], "#13");
		}	

		[Test]
		public void XmlTest24 ()
		{
			MakeDummyTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			dummyTable.WriteXml (stream, XmlWriteMode.DiffGram);
			stream.Close ();

			//This test is the same for case when the table name is set but no schema is defined
			DataTable table = new DataTable ();
			stream = new FileStream (fileName1, FileMode.Open);

			try {
				table.ReadXml (stream);
				Assert.Fail ("#1 Exception expected as schema is not defined in the target table");
			} catch (Exception e) {
				Assert.AreEqual ("System.InvalidOperationException", e.GetType ().ToString (), "#1");	
			} finally {
				stream.Close ();
			}
		
		}

		[Test]
		public void XmlTest25 ()
		{
			
			MakeDummyTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			dummyTable.WriteXml (stream, XmlWriteMode.DiffGram);
			stream.Close ();

			XmlReadMode mode = XmlReadMode.Auto;
			//Create a table but dont set the table name
			DataTable table = new DataTable ();
			//define the table schame partially
			table.Columns.Add ("id", typeof (int));
			stream = new FileStream (fileName1, FileMode.Open);
			mode = table.ReadXml (stream);
			stream.Close ();
		
			Assert.AreEqual (String.Empty, table.TableName, "#1");
			Assert.AreEqual (1, table.Columns.Count, "#2");
			Assert.AreEqual (0, table.Rows.Count, "#3");
			Assert.AreEqual (XmlReadMode.DiffGram, mode, "#4");	
		}

		[Test]
		public void XmlTest26 ()
		{
			MakeDummyTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			dummyTable.WriteXml (stream, XmlWriteMode.DiffGram);
			stream.Close ();

			//Create a table and set the table name
			DataTable table = new DataTable ("DummyTable");
			//define the table schame partially
			table.Columns.Add (new DataColumn ("DummyItem", System.Type.GetType ("System.String")));
			stream = new FileStream (fileName1, FileMode.Open);
			XmlReadMode mode = table.ReadXml (stream);
			stream.Close ();
		
			Assert.AreEqual (XmlReadMode.DiffGram, mode, "#1");	
			Assert.AreEqual (null, table.DataSet, "#2");
			Assert.AreEqual (1, table.Columns.Count, "#3");
			Assert.AreEqual ("System.String", table.Columns [0].DataType.ToString (), "#4");
			Assert.AreEqual (3, table.Rows.Count, "#5");

			//Check Rows
			DataRow row = table.Rows [0];
			Assert.AreEqual ("DummyItem 1", row [0], "#1");
			Assert.AreEqual (DataRowState.Unchanged, row.RowState, "#2");

			row = table.Rows [1];
			Assert.AreEqual ("Changed_DummyItem 2", row [0], "#3");
			Assert.AreEqual (DataRowState.Modified, row.RowState, "#4");

			row = table.Rows [2];
			Assert.AreEqual ("DummyItem 3", row [0], "#5");
			Assert.AreEqual (DataRowState.Unchanged, row.RowState, "#6");

		}

		[Test]
		public void XmlTest27 ()
		{
			MakeDummyTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			dummyTable.WriteXml (stream, XmlWriteMode.DiffGram);
			stream.Close ();

			//Create a table and set the table name
			DataTable table = new DataTable ("DummyTable");
			//define the table and add an extra column in the table
			table.Columns.Add (new DataColumn ("id", System.Type.GetType ("System.Int32")));
			table.Columns.Add (new DataColumn ("DummyItem", System.Type.GetType ("System.String")));
			//Add an extra column which does not match any column in the source diffram
			table.Columns.Add (new DataColumn ("ExtraColumn", System.Type.GetType ("System.Double")));

			stream = new FileStream (fileName1, FileMode.Open);
			XmlReadMode mode = table.ReadXml (stream);
			stream.Close ();
		
			Assert.AreEqual (XmlReadMode.DiffGram, mode, "#1");	
			Assert.AreEqual (null, table.DataSet, "#2");
			Assert.AreEqual (3, table.Columns.Count, "#3");
			Assert.AreEqual ("System.Int32", table.Columns [0].DataType.ToString (), "#4");
			Assert.AreEqual ("System.String", table.Columns [1].DataType.ToString (), "#5");
			Assert.AreEqual ("System.Double", table.Columns [2].DataType.ToString (), "#6");
			Assert.AreEqual (3, table.Rows.Count, "#7");

			//Check Rows
			DataRow row = table.Rows [0];
			Assert.AreEqual (1, row [0], "#8");
			Assert.AreEqual ("DummyItem 1", row [1], "#9");
			Assert.AreEqual ("System.DBNull", row [2].GetType ().ToString (), "#10");	
			Assert.AreEqual (DataRowState.Unchanged, row.RowState, "#11");

			row = table.Rows [1];
			Assert.AreEqual (2, row [0], "#12");
			Assert.AreEqual ("Changed_DummyItem 2", row [1], "#13");
			Assert.AreEqual ("System.DBNull", row [2].GetType ().ToString (), "#14");	
			Assert.AreEqual (DataRowState.Modified, row.RowState, "#15");

			row = table.Rows [2];
			Assert.AreEqual (3, row [0], "#16");
			Assert.AreEqual ("DummyItem 3", row [1], "#17");
			Assert.AreEqual ("System.DBNull", row [2].GetType ().ToString (), "#18");	
			Assert.AreEqual (DataRowState.Unchanged, row.RowState, "#19");
		}

		[Test]
		public void XmlTest28 ()
		{
			MakeDummyTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			dummyTable.WriteXml (stream, XmlWriteMode.DiffGram);
			stream.Close ();

			//Create a table and set the table name
			DataTable table = new DataTable ("DummyTable");
			//define the table schame partially with a column name which does not match with any
			//table columns in the diffgram
			table.Columns.Add (new DataColumn ("WrongColumnName", System.Type.GetType ("System.String")));
			stream = new FileStream (fileName1, FileMode.Open);
			XmlReadMode mode = table.ReadXml (stream);
			stream.Close ();

			Assert.AreEqual (XmlReadMode.DiffGram, mode, "#1");
			Assert.AreEqual (null, table.DataSet, "#2");
			Assert.AreEqual ("DummyTable", table.TableName, "#3");
			Assert.AreEqual (1, table.Columns.Count, "#4");
			Assert.AreEqual ("System.String", table.Columns [0].DataType.ToString (), "#5");
		
			Assert.AreEqual (3, table.Rows.Count, "#6");
			foreach (DataRow row in table.Rows)
				Assert.AreEqual ("System.DBNull", row [0].GetType ().ToString (), "#7");
		}

		[Test]
		public void XmlTest29 ()
		{
			MakeParentTable1 ();
			MakeChildTable ();
			MakeSecondChildTable ();
			MakeDataRelation ();
			
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable1.WriteXml (stream, XmlWriteMode.DiffGram, true);
			stream.Close ();
			
			DataTable table1 = new DataTable ("ParentTable");
			table1.Columns.Add (new DataColumn (parentTable1.Columns [0].ColumnName, typeof (int)));
			table1.Columns.Add (new DataColumn (parentTable1.Columns [1].ColumnName, typeof (string)));
			table1.Columns.Add (new DataColumn (parentTable1.Columns [2].ColumnName, typeof (int)));

			//ReadXml on a DiffGram will never create any child relation
			XmlReadMode mode = table1.ReadXml (fileName1);

			Assert.AreEqual (XmlReadMode.DiffGram, mode, "#1");
			Assert.AreEqual (null, table1.DataSet, "#2");
			Assert.AreEqual ("ParentTable", table1.TableName, "#3");
			Assert.AreEqual (3, table1.Columns.Count, "#4");
			Assert.AreEqual ("System.Int32", table1.Columns [0].DataType.ToString (), "#5");
			Assert.AreEqual ("System.String", table1.Columns [1].DataType.ToString (), "#6");
			Assert.AreEqual ("System.Int32", table1.Columns [2].DataType.ToString (), "#7");
			Assert.AreEqual (0, table1.ChildRelations.Count, "#8");

			Assert.AreEqual (3, table1.Rows.Count, "#9");
			//Check the row
			DataRow row = table1.Rows [0];
			Assert.AreEqual (1, row [0], "#10");
			Assert.AreEqual ("ParentItem 1", row [1], "#11");
			Assert.AreEqual (1, row [2], "#12");

			row = table1.Rows [1];
			Assert.AreEqual (2, row [0], "#13");
			Assert.AreEqual ("ParentItem 2", row [1], "#14");
			Assert.AreEqual (2, row [2], "#15");

			row = table1.Rows [2];
			Assert.AreEqual (3, row [0], "#16");
			Assert.AreEqual ("ParentItem 3", row [1], "#17");
			Assert.AreEqual (3, row [2], "#18");
		}

		[Test]
		public void XmlTest30 ()
		{
			MakeDummyTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			dummyTable.WriteXml (stream, XmlWriteMode.DiffGram);
			stream.Close ();
			
			Assert.AreEqual (3, dummyTable.Rows.Count, "#4b");

			DataSet dataSet = new DataSet ("HelloWorldDataSet");
			DataTable table = new DataTable ("DummyTable"); 
			table.Columns.Add (new DataColumn ("DummyItem", typeof (string)));		
			dataSet.Tables.Add (table);

			//Call ReadXml on a table which belong to a DataSet
			table.ReadXml (fileName1);

			Assert.AreEqual ("HelloWorldDataSet", table.DataSet.DataSetName, "#1");
			Assert.AreEqual (1, table.Columns.Count, "#2");
			Assert.AreEqual ("System.String", table.Columns [0].DataType.ToString (), "#3");
			Assert.AreEqual (3, table.Rows.Count, "#4");

			//Check Rows
			DataRow row = table.Rows [0];
			Assert.AreEqual ("DummyItem 1", row [0], "#5");
			Assert.AreEqual (DataRowState.Unchanged, row.RowState, "#6");

			row = table.Rows [1];
			Assert.AreEqual ("Changed_DummyItem 2", row [0], "#7");
			Assert.AreEqual (DataRowState.Modified, row.RowState, "#8");

			row = table.Rows [2];
			Assert.AreEqual ("DummyItem 3", row [0], "#9");
			Assert.AreEqual (DataRowState.Unchanged, row.RowState, "#10");
		}

		[Test]
		public void XmlTest31 ()
		{
			FileStream stream = null;
			
			DataSet ds = new DataSet ();
			DataTable parent = new DataTable ("Parent");
			parent.Columns.Add (new DataColumn ("col1", typeof (int)));
			parent.Columns.Add (new DataColumn ("col2", typeof (string)));
			parent.Columns [0].Unique = true;

			DataTable child1 = new DataTable ("Child1");
			child1.Columns.Add (new DataColumn ("col3", typeof (int)));
			child1.Columns.Add (new DataColumn ("col4", typeof (string)));
			child1.Columns.Add (new DataColumn ("col5", typeof (int)));
			child1.Columns [2].Unique = true;

			DataTable child2 = new DataTable ("Child2");
			child2.Columns.Add (new DataColumn ("col6", typeof (int)));
			child2.Columns.Add (new DataColumn ("col7"));	

			parent.Rows.Add (new object [] {1, "P_"});
			parent.Rows.Add (new object [] {2, "P_"});

			child1.Rows.Add (new object [] {1, "C1_", 3});
			child1.Rows.Add (new object [] {1, "C1_", 4});
			child1.Rows.Add (new object [] {2, "C1_", 5});
			child1.Rows.Add (new object [] {2, "C1_", 6});
	
			child2.Rows.Add (new object [] {3, "C2_"});
			child2.Rows.Add (new object [] {3, "C2_"});
			child2.Rows.Add (new object [] {4, "C2_"});
			child2.Rows.Add (new object [] {4, "C2_"});
			child2.Rows.Add (new object [] {5, "C2_"});
			child2.Rows.Add (new object [] {5, "C2_"});
			child2.Rows.Add (new object [] {6, "C2_"});
			child2.Rows.Add (new object [] {6, "C2_"});


			ds.Tables.Add (parent);
			ds.Tables.Add (child1);
			ds.Tables.Add (child2);

			DataRelation relation = new DataRelation ("Relation1", parent.Columns [0], child1.Columns [0]);
			parent.ChildRelations.Add (relation);

			relation = new DataRelation ("Relation2", child1.Columns [2], child2.Columns [0]);
			child1.ChildRelations.Add (relation);

			stream = new FileStream (fileName1, FileMode.Create);
			parent.WriteXml (stream, XmlWriteMode.WriteSchema, true);

			stream.Close ();

				
			DataTable table = new DataTable ();
			table.ReadXml (fileName1);

			Assert.AreEqual ("Parent", table.TableName, "#1");
			Assert.AreEqual ("NewDataSet", table.DataSet.DataSetName, "#2");
			Assert.AreEqual (2, table.Columns.Count, "#3");
			Assert.AreEqual (2, table.Rows.Count, "#4");
			Assert.AreEqual (typeof (System.Int32), table.Columns [0].DataType, "#5");
			Assert.AreEqual (typeof (System.String), table.Columns [1].DataType, "#6");
			Assert.AreEqual (1, table.Constraints.Count, "#7");
			Assert.AreEqual (typeof (UniqueConstraint), table.Constraints [0].GetType (), "#8");	
			Assert.AreEqual (1, table.ChildRelations.Count, "#9");
			Assert.AreEqual ("Relation1", table.ChildRelations [0].RelationName, "#10");
			Assert.AreEqual ("Parent", table.ChildRelations [0].ParentTable.TableName, "#11");
			Assert.AreEqual ("Child1", table.ChildRelations [0].ChildTable.TableName, "#12"); 
		
			DataTable table1 = table.ChildRelations [0].ChildTable;
			Assert.AreEqual ("Child1", table1.TableName, "#13");
			Assert.AreEqual ("NewDataSet", table1.DataSet.DataSetName, "#14");
			Assert.AreEqual (3, table1.Columns.Count, "#15");
			Assert.AreEqual (4, table1.Rows.Count, "#16");
			Assert.AreEqual (typeof (System.Int32), table1.Columns [0].DataType, "#17");
			Assert.AreEqual (typeof (System.String), table1.Columns [1].DataType, "#18");
			Assert.AreEqual (typeof (System.Int32), table1.Columns [2].DataType, "#19");
			Assert.AreEqual (2, table1.Constraints.Count, "#20");
			Assert.AreEqual (typeof (UniqueConstraint), table1.Constraints [0].GetType (), "#21");
			Assert.AreEqual (typeof (ForeignKeyConstraint), table1.Constraints [1].GetType (), "#22");	
			Assert.AreEqual (1, table1.ParentRelations.Count, "#23");
			Assert.AreEqual (1, table1.ChildRelations.Count, "#24");
			Assert.AreEqual ("Relation1", table1.ParentRelations [0].RelationName, "#25");
			Assert.AreEqual ("Relation2", table1.ChildRelations [0].RelationName, "#26");
			Assert.AreEqual ("Parent", table1.ParentRelations [0].ParentTable.TableName, "#27");
			Assert.AreEqual ("Child2", table1.ChildRelations [0].ChildTable.TableName, "#28"); 
	
			table1 = table1.ChildRelations [0].ChildTable;
			Assert.AreEqual ("Child2", table1.TableName, "#28");
			Assert.AreEqual ("NewDataSet", table1.DataSet.DataSetName, "#29");
			Assert.AreEqual (2, table1.Columns.Count, "#30");
			Assert.AreEqual (8, table1.Rows.Count, "#31");
			Assert.AreEqual (typeof (System.Int32), table1.Columns [0].DataType, "#32");
			Assert.AreEqual (typeof (System.String), table1.Columns [1].DataType, "#33");
			Assert.AreEqual (1, table1.Constraints.Count, "#34");
			Assert.AreEqual (typeof (ForeignKeyConstraint), table1.Constraints [0].GetType (), "#35");	
			Assert.AreEqual (1, table1.ParentRelations.Count, "#36");
			Assert.AreEqual (0, table1.ChildRelations.Count, "#37");
			Assert.AreEqual ("Relation2", table1.ParentRelations [0].RelationName, "#38");
			Assert.AreEqual ("Child1", table1.ParentRelations [0].ParentTable.TableName, "#39");
			
		}
	}
}
#endif

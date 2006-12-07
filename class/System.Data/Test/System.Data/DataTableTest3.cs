//Tests to check the behavior of ReadXmlSchema and WriteXmlSchema
#if NET_2_0
using System;
using System.Data;
using System.Data.Odbc;
using System.IO;	
using System.Xml;
using NUnit.Framework;

namespace Monotests_System.Data
{
	[TestFixture]	
	public class DataTableTest3
	{
		string fileName1 = "Test/System.Data/TestFile3.xml";
		DataSet dataSet = null;
		DataTable parentTable = null;
		DataTable childTable = null;
		DataTable secondChildTable = null;

		private void MakeParentTable ()
		{
			// Create a new Table
			parentTable = new DataTable ("ParentTable");
			dataSet = new DataSet ("XmlSchemaDataSet");
			DataColumn column;
			DataRow row;
 
			// Create new DataColumn, set DataType, 
			// ColumnName and add to Table.    
			column = new DataColumn ();
			column.DataType = System.Type.GetType ("System.Int32");
			column.ColumnName = "id";
			column.Unique = true;
			// Add the Column to the DataColumnCollection.
			parentTable.Columns.Add (column);

			// Create second column
			column = new DataColumn ();
			column.DataType = System.Type.GetType ("System.String");
			column.ColumnName = "ParentItem";
			column.AutoIncrement = false;
			column.Caption = "ParentItem";
			column.Unique = false;
			// Add the column to the table
			parentTable.Columns.Add (column);

			// Create third column.
			column = new DataColumn ();
			column.DataType = System.Type.GetType ("System.Int32");
			column.ColumnName = "DepartmentID";
			column.Caption = "DepartmentID";
			// Add the column to the table.
			parentTable.Columns.Add (column);
 

			// Make the ID column the primary key column.
			DataColumn [] PrimaryKeyColumns = new DataColumn [2];
			PrimaryKeyColumns [0] = parentTable.Columns ["id"];
			PrimaryKeyColumns [1] = parentTable.Columns ["DepartmentID"];
			parentTable.PrimaryKey = PrimaryKeyColumns;

			dataSet.Tables.Add (parentTable);
 
			// Create three new DataRow objects and add 
			// them to the DataTable
			for (int i = 0; i <= 2; i++)
			{
				row = parentTable.NewRow ();
				row ["id"] = i + 1 ;
				row ["ParentItem"] = "ParentItem " + (i + 1);
				row ["DepartmentID"] = i + 1;
				parentTable.Rows.Add (row);
			}
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
			for(int i = 0; i <= 1; i ++)
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
			for(int i = 0; i <= 1; i++)
			{
				row = secondChildTable.NewRow ();
				row ["childID"] = i + 1;
				row ["ChildItem"] = "SecondChildItem " + (i + 1);
				row ["ParentID"] = 1 ;
				row ["DepartmentID"] = 1; 
				secondChildTable.Rows.Add (row);
			}		
			for(int i = 0;i <= 1;i++)
			{
				row = secondChildTable.NewRow ();	
				row ["childID"] = i + 5;
				row ["ChildItem"] = "SecondChildItem " + (i + 1);
				row ["ParentID"] = 2;
				row ["DepartmentID"] = 2;
				secondChildTable.Rows.Add (row);
			}
			for(int i = 0;i <= 1;i++)
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
			//Check Properties of Table
			Assert.AreEqual ("", table.Namespace, "#1");
                        Assert.AreEqual (ds, table.DataSet, "#2");  
                        Assert.AreEqual (3, table.Columns.Count, "#3");
                        Assert.AreEqual (0, table.Rows.Count, "#4");
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
                        Assert.AreEqual (tableName, col.Table.ToString (), "#32"); 
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
			Assert.AreEqual (tableName, col.Table.ToString (), "#48");
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
			Assert.AreEqual (tableName, col.Table.ToString (), "#64");
			Assert.AreEqual (false, col.Unique, "#65");
		}

		private void TestParentTableSchema (DataTable table, string tableName, DataSet ds)
		{
			//Check Properties of Table
			Assert.AreEqual ("", table.Namespace, "#1");
                        Assert.AreEqual (ds.DataSetName, table.DataSet.DataSetName, "#2");  
                        Assert.AreEqual (3, table.Columns.Count, "#3");
                        Assert.AreEqual (0, table.Rows.Count, "#4");
                        Assert.AreEqual (false, table.CaseSensitive, "#5");
			Assert.AreEqual ("ParentTable", table.TableName, "#6");
			Assert.AreEqual (2, table.Constraints.Count, "#7"); 
                        Assert.AreEqual ("", table.Prefix, "#8"); 
                        
                        //Check Constraints
			Assert.AreEqual ("Constraint1", table.Constraints [0].ToString (), "#9");
			Assert.AreEqual ("Constraint2", table.Constraints [1].ToString (), "#10");
			Assert.AreEqual ("System.Data.UniqueConstraint", table.Constraints [0].GetType ().ToString (), "#11");
			Assert.AreEqual ("System.Data.UniqueConstraint", table.Constraints [1].GetType ().ToString (), "#12");
			Assert.AreEqual (2, table.PrimaryKey.Length, "#13");
			Assert.AreEqual ("id", table.PrimaryKey [0].ToString (), "#14");
			Assert.AreEqual ("DepartmentID", table.PrimaryKey [1].ToString (), "#15");
			
			//Check Relations of the ParentTable	
			Assert.AreEqual (0, table.ParentRelations.Count, "#16");
			Assert.AreEqual (2, table.ChildRelations.Count, "#17");
			Assert.AreEqual ("ParentChild_Relation1", table.ChildRelations [0].ToString (), "#18");
			Assert.AreEqual ("ParentChild_Relation2", table.ChildRelations [1].ToString (), "#19");
			Assert.AreEqual ("ChildTable", table.ChildRelations [0].ChildTable.TableName, "#20");	
			Assert.AreEqual ("SecondChildTable", table.ChildRelations [1].ChildTable.TableName, "#21");	
				
			Assert.AreEqual (1, table.ChildRelations [0].ParentColumns.Length, "#22");	
			Assert.AreEqual ("id", table.ChildRelations [0].ParentColumns [0].ColumnName, "#23");
			Assert.AreEqual (1, table.ChildRelations [0].ChildColumns.Length, "#24");
			Assert.AreEqual ("ParentID", table.ChildRelations [0].ChildColumns [0].ColumnName, "#25");
				
			Assert.AreEqual (2, table.ChildRelations [1].ParentColumns.Length, "#26");	
			Assert.AreEqual ("id", table.ChildRelations [1].ParentColumns [0].ColumnName, "#27");
			Assert.AreEqual ("DepartmentID", table.ChildRelations [1].ParentColumns [1].ColumnName, "#28");	
			Assert.AreEqual (2, table.ChildRelations [1].ChildColumns.Length, "#29");
			
			Assert.AreEqual ("ParentID", table.ChildRelations [1].ChildColumns [0].ColumnName, "#30");
			Assert.AreEqual ("DepartmentID", table.ChildRelations [1].ChildColumns [1].ColumnName, "#31");

			//Check properties of each column
			//First Column
			DataColumn col = table.Columns [0];
                        Assert.AreEqual (false, col.AllowDBNull, "#32"); 
                        Assert.AreEqual (false, col.AutoIncrement, "#33"); 
                        Assert.AreEqual (0, col.AutoIncrementSeed, "#34"); 
                        Assert.AreEqual (1, col.AutoIncrementStep, "#35"); 
                        Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#36"); 
                        Assert.AreEqual ("id", col.Caption, "#37"); 
                        Assert.AreEqual ("id", col.ColumnName, "#38"); 
                        Assert.AreEqual ("System.Int32", col.DataType.ToString (), "#39"); 
                        Assert.AreEqual ("", col.DefaultValue.ToString (), "#40"); 
                        Assert.AreEqual (false, col.DesignMode, "#41"); 
                        Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#42");
			Assert.AreEqual (-1, col.MaxLength, "#43"); 
                        Assert.AreEqual (0, col.Ordinal, "#44"); 
                        Assert.AreEqual ("", col.Prefix, "#45"); 
                        Assert.AreEqual ("ParentTable", col.Table.ToString (), "#46"); 
                        Assert.AreEqual (true, col.Unique, "#47");

			//Second Column
			col = table.Columns [1];
			Assert.AreEqual (true, col.AllowDBNull, "#48");
			Assert.AreEqual (false, col.AutoIncrement, "#49");
			Assert.AreEqual (0, col.AutoIncrementSeed, "#50");
			Assert.AreEqual (1, col.AutoIncrementStep, "#51");
			Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#52");
			Assert.AreEqual ("ParentItem", col.Caption, "#53");
			Assert.AreEqual ("ParentItem", col.ColumnName, "#54");
			Assert.AreEqual ("System.String", col.DataType.ToString (), "#55");
			Assert.AreEqual ("", col.DefaultValue.ToString (), "#56");
			Assert.AreEqual (false, col.DesignMode, "#57");
			Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#58");
			Assert.AreEqual (-1, col.MaxLength, "#59");
			Assert.AreEqual (1, col.Ordinal, "#60");
			Assert.AreEqual ("", col.Prefix, "#61");
			Assert.AreEqual ("ParentTable", col.Table.ToString (), "#62");
			Assert.AreEqual (false, col.Unique, "#63");

			//Third Column
			col = table.Columns [2];
			Assert.AreEqual (false, col.AllowDBNull, "#64");
			Assert.AreEqual (false, col.AutoIncrement, "#65");
			Assert.AreEqual (0, col.AutoIncrementSeed, "#66");
			Assert.AreEqual (1, col.AutoIncrementStep, "#67");
			Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#68");
			Assert.AreEqual ("DepartmentID", col.Caption, "#69");
			Assert.AreEqual ("DepartmentID", col.ColumnName, "#70");
			Assert.AreEqual ("System.Int32", col.DataType.ToString (), "#71");
			Assert.AreEqual ("", col.DefaultValue.ToString (), "#72");
			Assert.AreEqual (false, col.DesignMode, "#73");
			Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#74");
			Assert.AreEqual (-1, col.MaxLength, "#75");
			Assert.AreEqual (2, col.Ordinal, "#76");
			Assert.AreEqual ("", col.Prefix, "#77");
			Assert.AreEqual ("ParentTable", col.Table.ToString (), "#78");
			Assert.AreEqual (false, col.Unique, "#79");
			
		}

		[Test]
		public void XmlSchemaTest1 ()
		{
			
			MakeParentTable ();
			//Detach the table from the DataSet
			dataSet.Tables.Remove (parentTable);
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			//Write
			parentTable.WriteXmlSchema (stream);
			stream.Dispose ();

			//Read
			stream = new FileStream (fileName1, FileMode.Open);
			DataTable table = new DataTable ();
			table.ReadXmlSchema (stream);

			TestTableSchema (table, parentTable.TableName, parentTable.DataSet);
			stream.Dispose ();
		}	
		

		[Test]
		public void XmlSchemaTest2 ()
		{
			MakeParentTable ();
			
			dataSet.Tables.Remove (parentTable);
			parentTable.TableName = String.Empty;
								
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			try {
				parentTable.WriteXmlSchema (stream);
				Assert.Fail ("#1 Exception was expected as table name is not set");	
			} catch (Exception e) {
				Assert.AreEqual ("System.InvalidOperationException", e.GetType ().ToString (), "#2");	
			} finally {
				stream.Dispose ();
			}	
		}
		
		[Test]
		public void XmlSchemaTest3 ()
		{
			//Write
			MakeParentTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable.WriteXmlSchema (stream);
			stream.Dispose ();

			//Read
			stream = new FileStream (fileName1, FileMode.Open);
			DataTable table = new DataTable ();
			table.ReadXmlSchema (stream);

			TestTableSchema (table, parentTable.TableName, null);
			stream.Dispose ();
		}
		
		[Test]
		[Category ("NotWorking")]
		public void XmlSchemaTest4 ()
		{
			MakeParentTable ();
			MakeChildTable ();
			MakeSecondChildTable ();
			MakeDataRelation ();

			//Write
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable.WriteXmlSchema (stream, true);
			stream.Dispose ();

			
			//Read
			stream = new FileStream (fileName1, FileMode.Open);
			DataTable table = new DataTable ();
			table.ReadXmlSchema (stream);
			
			//Test Property of Parent	
			TestParentTableSchema (table, parentTable.TableName, parentTable.DataSet);
			
			
			//Check Properties of First Child Table
			DataTable firstChildTable = parentTable.ChildRelations [0].ChildTable; 
			Assert.AreEqual ("", firstChildTable.Namespace, "#1");
                        Assert.AreEqual ("XmlSchemaDataSet", firstChildTable.DataSet.DataSetName, "#2");  
                        Assert.AreEqual (3, firstChildTable.Columns.Count, "#3");
                        Assert.AreEqual (6, firstChildTable.Rows.Count, "#4");  
                        Assert.AreEqual (false, firstChildTable.CaseSensitive, "#5");
			Assert.AreEqual ("ChildTable", firstChildTable.TableName, "#6");
			Assert.AreEqual ("", firstChildTable.Prefix, "#7"); 
                        Assert.AreEqual (2, firstChildTable.Constraints.Count, "#8");
			Assert.AreEqual ("Constraint1", firstChildTable.Constraints [0].ToString (), "#9");
			Assert.AreEqual ("ParentChild_Relation1", firstChildTable.Constraints [1].ToString (), "#10");
			Assert.AreEqual (1, firstChildTable.ParentRelations.Count, "#11");
			Assert.AreEqual (0, firstChildTable.ChildRelations.Count, "#12");
			Assert.AreEqual (0, firstChildTable.PrimaryKey.Length, "#13");
			
			//Check Properties of Second Child Table
			DataTable secondChildTable = parentTable.ChildRelations [1].ChildTable;
			Assert.AreEqual ("", secondChildTable.Namespace, "#14");
                        Assert.AreEqual ("XmlSchemaDataSet", secondChildTable.DataSet.DataSetName, "#15");  
                        Assert.AreEqual (4, secondChildTable.Columns.Count, "#16");
                        Assert.AreEqual (6, secondChildTable.Rows.Count, "#17");  
                        Assert.AreEqual (false, secondChildTable.CaseSensitive, "#18");
			Assert.AreEqual ("SecondChildTable", secondChildTable.TableName, "#19");
			Assert.AreEqual ("", secondChildTable.Prefix, "#20"); 
                        Assert.AreEqual (2, secondChildTable.Constraints.Count, "#21");
			Assert.AreEqual ("Constraint1", secondChildTable.Constraints [0].ToString (), "#22");
			Assert.AreEqual ("ParentChild_Relation2", secondChildTable.Constraints [1].ToString (), "#23");
			Assert.AreEqual (1, secondChildTable.ParentRelations.Count, "#24");;
			Assert.AreEqual (0, secondChildTable.ChildRelations.Count, "#25");
			Assert.AreEqual (0, secondChildTable.PrimaryKey.Length, "#26");			
			stream.Dispose ();		
			
		}
		
                [Test]
                public void XmlSchemaTest5 ()
                {
                	MakeParentTable ();
			MakeChildTable ();
			MakeSecondChildTable ();
			MakeDataRelation ();
			
			//Write
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			childTable.WriteXmlSchema (stream);
			stream.Dispose ();
	
			
			//Read
			stream = new FileStream (fileName1, FileMode.Open);
			DataTable table = new DataTable (childTable.TableName);
			table.ReadXmlSchema (stream);
			
			//Check Properties of the table
			Assert.AreEqual ("", table.Namespace, "#1");
                        Assert.AreEqual (null, table.DataSet, "#2");  
                        Assert.AreEqual (3, table.Columns.Count, "#3");
                        Assert.AreEqual (0, table.Rows.Count, "#4");  
                        Assert.AreEqual (false, table.CaseSensitive, "#5");
			Assert.AreEqual ("ChildTable", table.TableName, "#6");
			Assert.AreEqual ("", table.Prefix, "#7"); 
			
                        Assert.AreEqual (1, table.Constraints.Count, "#8");
			Assert.AreEqual ("Constraint1", table.Constraints [0].ToString (), "#9");
			Assert.AreEqual ("System.Data.UniqueConstraint", table.Constraints [0].GetType().ToString (), "#10");
			Assert.AreEqual (0, table.ParentRelations.Count, "#11");
			Assert.AreEqual (0, table.ChildRelations.Count, "#12");
			Assert.AreEqual (0, table.PrimaryKey.Length, "#13");			
			
			//First Column
			DataColumn col = table.Columns [0];
			Assert.AreEqual (true, col.AllowDBNull, "#14"); 
                        Assert.AreEqual (true, col.AutoIncrement, "#15"); 
                        Assert.AreEqual (0, col.AutoIncrementSeed, "#16"); 
                        Assert.AreEqual (1, col.AutoIncrementStep, "#17"); 
                        Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#18"); 
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
			Assert.AreEqual (false, col.AutoIncrement, "#30");
			Assert.AreEqual (0, col.AutoIncrementSeed, "#31");
			Assert.AreEqual (1, col.AutoIncrementStep, "#32");
			Assert.AreEqual ("Element", col.ColumnMapping.ToString (), "#33");
			Assert.AreEqual ("ChildItem", col.Caption, "#34");
			Assert.AreEqual ("ChildItem", col.ColumnName, "#35");
			Assert.AreEqual ("System.String", col.DataType.ToString (), "#36");
			Assert.AreEqual ("", col.DefaultValue.ToString (), "#37");
			Assert.AreEqual (false, col.DesignMode, "#38");
			Assert.AreEqual ("System.Data.PropertyCollection", col.ExtendedProperties.ToString (), "#39");
			Assert.AreEqual (-1, col.MaxLength, "#40");
			Assert.AreEqual (1, col.Ordinal, "#41");
			Assert.AreEqual ("", col.Prefix, "#42");
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
			
			stream.Dispose ();
				
                }
                
		[Test]
		public void XmlSchemaTest6 ()
		{
			MakeParentTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable.WriteXmlSchema (stream);
			stream.Dispose ();

			DataTable table = new DataTable ();
			DataSet ds = new DataSet ();
			ds.Tables.Add (table);
			
			stream = new FileStream (fileName1, FileMode.Open);
			try {
				table.ReadXmlSchema (stream);
				Assert.Fail ("#1 Exception was expected");
			} catch (Exception e) {
				Assert.AreEqual ("System.ArgumentException", e.GetType ().ToString (), "#2");
			}
			stream.Dispose ();
		}
		
		
		[Test]
		public void XmlSchemaTest7 ()
		{
			string fileName2 = String.Empty;
			DataTable table = new DataTable ();
			
			try {
				table.ReadXmlSchema (fileName2);
				Assert.Fail ("#1 Exception was expected");
			} catch (Exception e) {
				Assert.AreEqual ("System.ArgumentException", e.GetType ().ToString (), "#2");	
			}	
		}

		[Test]
		public void XmlSchemaTest8 ()
		{
			MakeParentTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable.WriteXmlSchema (stream);
			stream.Close ();
		
			//Create a table and define the schema partially
			DataTable table = new DataTable ();
			table.Columns.Add (new DataColumn (parentTable.Columns [0].ColumnName, typeof (int)));

			//ReadXmlSchema will not read any schema in this case
			table.ReadXmlSchema (fileName1);

			Assert.AreEqual ("", table.TableName, "");
			Assert.AreEqual (1, table.Columns.Count, "");
			Assert.AreEqual (0, table.Constraints.Count, "");
		}

		[Test]
		public void XmlSchemaTest9 ()
		{
			MakeParentTable ();
			FileStream stream = new FileStream (fileName1, FileMode.Create);
			parentTable.WriteXmlSchema (stream);
			stream.Close ();
		
			//Create a table and define the full schema 
			DataTable table = new DataTable ();
			table.Columns.Add (new DataColumn (parentTable.Columns [0].ColumnName, typeof (int)));
			table.Columns.Add (new DataColumn (parentTable.Columns [1].ColumnName, typeof (string)));
			table.Columns.Add (new DataColumn (parentTable.Columns [2].ColumnName, typeof (int)));

			//ReadXmlSchema will not read any schema in this case
			table.ReadXmlSchema (fileName1);

			Assert.AreEqual ("", table.TableName, "");
			Assert.AreEqual (3, table.Columns.Count, "");
			Assert.AreEqual (0, table.Constraints.Count, "");
		}
	}
}
#endif

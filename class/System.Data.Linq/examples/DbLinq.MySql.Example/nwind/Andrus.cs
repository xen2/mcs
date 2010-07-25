//#########################################################################
//generated by MySqlMetal on 2008-Feb-11 - extracted from server localhost.
//#########################################################################

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;
//using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using DBLinq.Linq;
using DBLinq.Linq.Mapping;
using DataContext = DBLinq.vendor.mysql.MysqlDataContext;


namespace andrusDB
{
    
	/// <summary>
	/// This class represents MySql database Andrus.
	/// </summary>
	public partial class Andrus : DataContext
	{
		public Andrus(string connStr) 
			: base(connStr)
		{
		}
		public Andrus(System.Data.IDbConnection connection) 
			: base(connection)
		{
		}
	
		//these fields represent tables in database and are
		//ordered - parent tables first, child tables next. Do not change the order.
		public Table<T2> T2s { get { return base.GetTable<T2>("T2s"); } }
		public Table<Tcompositepk> Tcompositepks { get { return base.GetTable<Tcompositepk>("Tcompositepks"); } }
		public Table<T1> T1s { get { return base.GetTable<T1>("T1s"); } }
		public Table<Char_Pk> Char_Pks { get { return base.GetTable<Char_Pk>("Char_Pks"); } }
		public Table<Employee> Employees { get { return base.GetTable<Employee>("Employees"); } }
	
		
	}
	
	
	
	[Table(Name = "t2")]
	public partial class T2 : IModified
	{
		
		protected int? _f1;
		protected int? _f2;
	
		
		public T2()
		{
		}
		
	
		#region properties - accessors
	
		[Column(Storage = "_f1", Name = "f1", DbType = "int", CanBeNull = true)]
		[DebuggerNonUserCode]
		public int? f1
		{
		    get { return _f1; }
		    set { _f1 = value; IsModified = true; }
		}
		
	
		[Column(Storage = "_f2", Name = "f2", DbType = "int", CanBeNull = true)]
		[DebuggerNonUserCode]
		public int? f2
		{
		    get { return _f2; }
		    set { _f2 = value; IsModified = true; }
		}
		
	#endregion
		#warning L189 table t2 has no primary key. Multiple c# objects will refer to the same row.

		#region childtables
		#endregion
		#region parenttables
		#endregion
	
		public bool IsModified { get; set; }
	}
	
	
	
	[Table(Name = "tcompositepk")]
	public partial class Tcompositepk : IModified
	{
		
		protected int _f1;
		protected string _f2;
		protected int? _f3;
	
		
		public Tcompositepk()
		{
		}
		
	
		#region properties - accessors
	
		[Column(Storage = "_f1", Name = "f1", DbType = "int", IsPrimaryKey = true)]
		[DebuggerNonUserCode]
		public int f1
		{
		    get { return _f1; }
		    set { _f1 = value; IsModified = true; }
		}
		
	
		[Column(Storage = "_f2", Name = "f2", DbType = "varchar(5)", IsPrimaryKey = true)]
		[DebuggerNonUserCode]
		public string f2
		{
		    get { return _f2; }
		    set { _f2 = value; IsModified = true; }
		}
		
	
		[Column(Storage = "_f3", Name = "f3", DbType = "int", CanBeNull = true)]
		[DebuggerNonUserCode]
		public int? f3
		{
		    get { return _f3; }
		    set { _f3 = value; IsModified = true; }
		}
		
	#endregion
		
		#region GetHashCode(),Equals() - uses column $fieldID to look up objects in liveObjectMap
		//TODO: move this logic our of user code, into a generated class
		public override int GetHashCode()
		{
			return _f1.GetHashCode() ^ (_f2 == null ? 0 : _f2.GetHashCode());
		}
		public override bool Equals(object obj)
		{
			Tcompositepk o2 = obj as Tcompositepk;
			if(o2==null)
				return false;
			return _f1 == o2._f1 && object.Equals(_f2, o2._f2);
		}
		#endregion
	
		#region childtables
		#endregion
		#region parenttables
		#endregion
	
		public bool IsModified { get; set; }
	}
	
	
	
	[Table(Name = "t1")]
	public partial class T1 : IModified
	{
		
		protected int _private;
	
		
		public T1()
		{
		}
		
	
		#region properties - accessors
	
		[Column(Storage = "_private", Name = "private", DbType = "int", IsPrimaryKey = true)]
		[DebuggerNonUserCode]
		public int private_
		{
		    get { return _private; }
		    set { _private = value; IsModified = true; }
		}
		
	#endregion
		
		#region GetHashCode(),Equals() - uses column $fieldID to look up objects in liveObjectMap
		//TODO: move this logic our of user code, into a generated class
		public override int GetHashCode()
		{
			return _private.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			T1 o2 = obj as T1;
			if(o2==null)
				return false;
			return _private == o2._private;
		}
		#endregion
	
		#region childtables
		#endregion
		#region parenttables
		#endregion
	
		public bool IsModified { get; set; }
	}
	
	
	
	[Table(Name = "char_pk")]
	public partial class Char_Pk : IModified
	{
		
		protected string _col1;
		protected int? _val1;
	
		
		public Char_Pk()
		{
		}
		
	
		#region properties - accessors
	
		[Column(Storage = "_col1", Name = "col1", DbType = "char(1)", IsPrimaryKey = true)]
		[DebuggerNonUserCode]
		public string col1
		{
		    get { return _col1; }
		    set { _col1 = value; IsModified = true; }
		}
		
	
		[Column(Storage = "_val1", Name = "val1", DbType = "int", CanBeNull = true)]
		[DebuggerNonUserCode]
		public int? val1
		{
		    get { return _val1; }
		    set { _val1 = value; IsModified = true; }
		}
		
	#endregion
		
		#region GetHashCode(),Equals() - uses column $fieldID to look up objects in liveObjectMap
		//TODO: move this logic our of user code, into a generated class
		public override int GetHashCode()
		{
			return (_col1 == null ? 0 : _col1.GetHashCode());
		}
		public override bool Equals(object obj)
		{
			Char_Pk o2 = obj as Char_Pk;
			if(o2==null)
				return false;
			return object.Equals(_col1, o2._col1);
		}
		#endregion
	
		#region childtables
		#endregion
		#region parenttables
		#endregion
	
		public bool IsModified { get; set; }
	}
	
	
	
	[Table(Name = "employee")]
	public partial class Employee : IModified
	{
		[DBLinq.Linq.Mapping.AutoGenId] 
		protected int _employeeID;
		protected int _employeeType;
		protected string _employeeName;
		protected DateTime? _startDate;
	
		
		public Employee()
		{
		}
		
	
		#region properties - accessors
	
		[Column(Storage = "_employeeID", Name = "employeeID", DbType = "int", IsPrimaryKey = true, IsDbGenerated = true)]
		[DebuggerNonUserCode]
		public int employeeID
		{
		    get { return _employeeID; }
		    set { _employeeID = value; IsModified = true; }
		}
		
	
		[Column(Storage = "_employeeType", Name = "employeeType", DbType = "int", CanBeNull = false)]
		[DebuggerNonUserCode]
		public int employeeType
		{
		    get { return _employeeType; }
		    set { _employeeType = value; IsModified = true; }
		}
		
	
		[Column(Storage = "_employeeName", Name = "employeeName", DbType = "varchar(99)", CanBeNull = true)]
		[DebuggerNonUserCode]
		public string employeeName
		{
		    get { return _employeeName; }
		    set { _employeeName = value; IsModified = true; }
		}
		
	
		[Column(Storage = "_startDate", Name = "startDate", DbType = "date", CanBeNull = true)]
		[DebuggerNonUserCode]
		public DateTime? startDate
		{
		    get { return _startDate; }
		    set { _startDate = value; IsModified = true; }
		}
		
	#endregion
		
		#region GetHashCode(),Equals() - uses column $fieldID to look up objects in liveObjectMap
		//TODO: move this logic our of user code, into a generated class
		public override int GetHashCode()
		{
			return _employeeID.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			Employee o2 = obj as Employee;
			if(o2==null)
				return false;
			return _employeeID == o2._employeeID;
		}
		#endregion
	
		#region childtables
		#endregion
		#region parenttables
		#endregion
	
		public bool IsModified { get; set; }
	}
	
}

//
// System.Management.Instrumentation.ManagedNameAttribute
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Class | 
			AttributeTargets.Struct |
			AttributeTargets.Method | 
			AttributeTargets.Property |
			AttributeTargets.Field)]
        public class ManagedNameAttribute : Attribute {
		public ManagedNameAttribute (string name)
		{
			_name = name;
		}

		public string Name {
			get {
				return _name;
			}
		}

		private string _name;
	}
}

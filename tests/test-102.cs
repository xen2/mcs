using System;
using System.Reflection;

[assembly: AssemblyTitle ("Foo")]
[assembly: AssemblyVersion ("1.0.2")]

namespace N1 {
		
	[AttributeUsage (AttributeTargets.All)]
	public class MineAttribute : Attribute {

		string name;
		
		public MineAttribute (string s)
		{
			name = s;
		}
	}
	
	public class Foo {	
		
		int i;
		
		[return : Mine ("Foo")]	
		public static int Main ()
		{
			return 0;
		}
	}
}

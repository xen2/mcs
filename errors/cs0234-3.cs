// cs0234-3.cs: The type or namespace name `Type' does not exist in the namespace `MonoTests.System' (are you missing an assembly reference?)
// Line: 12

using System;

namespace MonoTests.System
{
	public class Test
	{
		public static void Main ()
		{
			Console.WriteLine (System.Type.GetType ("System.String"));
		}
	}
}



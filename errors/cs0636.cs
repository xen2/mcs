// CS0636: The FieldOffset attribute can only be placed on members of types marked with the StructLayout(LayoutKind.Explicit)

using System;
using System.Runtime.InteropServices;

namespace cs0636 {
	struct GValue {
		public string name;
		[ FieldOffset (4) ] public int value;
	}
	
	class Tests {
		public static void Main () {
		}
	}
}

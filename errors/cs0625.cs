// CS0625: Instance field of type marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute.

using System;
using System.Runtime.InteropServices;

namespace cs0625 {
	[StructLayout(LayoutKind.Explicit)]
	struct GValue {
		public string name;
		[ FieldOffset (4) ] public int value;
	}
	
	class Tests {
		public static void Main () {
		}
	}
}

// cs0196.cs: A pointer must be indexed by only one value
// line: 10
// Compiler options: -unsafe
using System;

unsafe class ZZ {
	static void Main () {
		int *p = null;

		if (p [10,4] == 4)
			return;
	}
}

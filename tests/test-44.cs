//
// This test shows that the current way in which we handle blocks
// and statements is broken.  The b [q,w] code is only executed 10
// times instead of a 100
//
using System;

class X {

	static int dob (int [,]b)
	{
		int total = 0;
		
		foreach (int i in b)
			total += i;

		return total;
	}
	
	static void Main ()
	{
		int [,] b = new int [10,10];

		for (int q = 0; q < 10; q++)
			for (int w = 0; w < 10; w++)
				b [q,w] = q * 10 + w;

		if (dob (b) != 4950)
			return 1;

		return 0;
	}
}
	

// cs0159-2.cs: No such label `case 20:' within the scope of the goto statement
// Line: 13

class y {
	enum X { A = 1, B = 1, C = 1 }

	static void Main ()
	{
		int x = 1;

		switch (x){
			case 1: break;
			case 2: goto case 20;
		}
	}
}
		

// cs0157-2.cs: Control can not leave the body of a finally clause
// Line: 11

class T {
	static void Main ()
	{
		while (true) { 
			try {
				System.Console.WriteLine ("trying");
			} finally {
				break;
			}
		}
	}
}
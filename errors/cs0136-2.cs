// cs0136-2.cs: A local variable named `j' cannot be declared in this scope because it would give a different meaning to `j', which is already used in a `method argument' scope to denote something else
// Line: 5
class X {
	public static void Bar (int j, params int [] args)
	{
		foreach (int j in args)
			;
	}
}

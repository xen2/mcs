// cs0038-1.cs: Cannot access a nonstatic member of outer type `A.n' via nested type `C.N'
// Line: 12

class A {
	protected int n = 0;
}

class B : A { }

class C : B {
	class N {
		internal int foo () { return n; }
	}
	public static int Main () {
		N a = new N ();
		return a.foo ();
	}
}

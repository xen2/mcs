//
// This tests that conversions from Enum and ValueType to structs
// are treated as unboxing conversions, and the `unbox' opcode
// is emitted. #52569.
//

enum Foo { Bar }
class T {
	static int Main ()
	{
		System.Enum e = Foo.Bar;
		System.ValueType vt1 = Foo.Bar, vt2 = 1;
		
		if (((Foo) e) != Foo.Bar)
			return 1;
		
		if (((Foo) vt1) != Foo.Bar)
			return 2;
		
		if (((int) vt2) != 1)
			return 3;
		
		return 0;
	}
}
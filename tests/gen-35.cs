// May use a constructed type as constraint.

class Test<T>
{ }

class Foo<T>
	where T : Test<T>
{ }

class X
{
	static void Main ()
	{
		Foo<int> foo;
	}
}

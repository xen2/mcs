public class X
{
	public static void Foo (Stack stack)
	{
		stack.Hello<string> ("Hello World");
	}

	static void Main ()
	{
		Stack stack = new Stack ();
		Foo (stack);
	}
}

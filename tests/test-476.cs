#if false
#line hahaha
#error
#pragma warning disable 3005 // wrong directive on csc 1.x
	public class Foo
	{
	}
#pragma warning restore // wrong directive on csc 1.x

#region // blank -> no error
#endregion

#endif // of funky directives

public class Test
{
	public static void Main ()
	{
	}
}


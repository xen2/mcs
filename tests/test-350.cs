using System;

public class A
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class BAttribute : Attribute
    {
    }
}


[A.B()]
public class C
{
	static void Main () {}
}

// CS0449: The `class' or `struct' constraint must be the first constraint specified
// Line: 8

interface I
{
}

public class C<T> where T : I, class
{
}
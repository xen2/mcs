// cs0110.cs: The evaluation of the constant value for `E.c' involves a circular definition
// Line: 6

enum E
{
	a = b,
	b = c,
	c = a
}

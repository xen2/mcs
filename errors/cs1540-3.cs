// cs1540.cs: Cannot access protected member `A.n' via a qualifier of type `B'; the qualifier must be of type `C' (or derived from it)
// Line: 19

class A
{
        protected int n;
}
 
class B : A
{
}
 
class C : A
{
        static B b = new B ();
 
        static void Main ()
        {
                b.n = 1;
        }
}

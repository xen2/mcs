// cs0111-9.cs: `C.this[int]' is already defined. Rename this member or use different parameter types
// Line: 6

class C
{
    bool this [int i] { get { return false; } }
    bool this [int i] { get { return true; } }
}

// cs0619-22.cs: `AA' is obsolete: `'
// Line: 12

[System.Obsolete ("", true)]
class AA
{
}

class B {
        public bool Foo (object b)
        {
                return b is AA;
        }
}
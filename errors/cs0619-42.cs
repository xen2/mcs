// cs0619-42.cs: `Error.member' is obsolete: `Obsolete member'
// Line: 8
// Compiler options: -reference:CS0619-42-lib.dll

class A: Error {
	public A () {
		string s = Filename;
	}
	
	public override string Filename {
		set {
		}
	}
}
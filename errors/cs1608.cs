// cs1608.cs: The RequiredAttribute attribute is not permitted on C# types
// Line: 6

using System.Runtime.CompilerServices;

[RequiredAttribute (typeof (object))]
class ClassMain {
}


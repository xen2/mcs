// CS1570: XML documentation comment on `Testing.Test.PublicField2' is not well-formed XML markup ('summary' is expected  Line 3, position 4.)
// Line: 19
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	public class Test
	{
		/// <summary>
		/// comment for public field
		/// </invalid>
		public string PublicField2;
	}
}


// cs0118-4.cs: `x.a.B' is a `property' but a `type' was expected
// Line: 9

using System;

namespace x
{
	class a
	{
		bool B { set {} }
		
		void Test (B b) {}
	}
}
//
// TODOAttribute.cs
//
// Author:
//   Ravi Pratap (ravi@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	/// <summary>
	///   The TODO attribute is used to flag all incomplete bits in our class libraries
	/// </summary>
	///
	/// <remarks>
	///   Use this to decorate any element which you think is not complete
	/// </remarks>
	[AttributeUsage (AttributeTargets.All)]
	internal class MonoTODOAttribute : Attribute {

		// No methods.
	}
}

//
// TODOAttribute.cs
//
// Author:
//   Ravi Pratap (ravi@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace Microsoft.VisualBasic.CompilerServices {

	/// <summary>
	///   The TODO attribute is used to flag all incomplete bits in our class libraries
	/// </summary>
	///
	/// <remarks>
	///   Use this to decorate any element which you think is not complete
	/// </remarks>
	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	public class MonoTODOAttribute : Attribute {

		private string comment;
		
		public MonoTODOAttribute ()
		{}

		public MonoTODOAttribute (string comment)
		{
			this.comment = comment;
		}

		public string Comment
		{
			get { return comment; }
		}
	}
}

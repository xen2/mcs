//
// OptionCompareAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//   Martin Adoue (martin@cwanet.com)
//
// (C) 2002 Ximian Inc.
//
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.VisualBasic.CompilerServices {
	[MonoTODO]
	[EditorBrowsable(EditorBrowsableState.Never)] 
	[AttributeUsage(AttributeTargets.Parameter)] 
	[StructLayoutAttribute(LayoutKind.Auto)] 
	sealed public class OptionCompareAttribute : Attribute {
		// Declarations
		// Constructors
		// Properties
		// Methods
		// Events
		public OptionCompareAttribute()
		{
			//FIXME: should this do something?
			throw new NotImplementedException(); 
		}
	};

}

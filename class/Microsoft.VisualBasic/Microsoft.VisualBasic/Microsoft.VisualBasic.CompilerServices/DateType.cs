//
// DateType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic.CompilerServices {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class DateType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.DateTime FromString (System.String Value) { return System.DateTime.MinValue;}
		public static System.DateTime FromString (System.String Value, System.Globalization.CultureInfo culture) { return System.DateTime.MinValue;}
		public static System.DateTime FromObject (System.Object Value) { return System.DateTime.MinValue;}
		// Events
	};
}

//
// VBFixedArrayAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[System.AttributeUsageAttribute(System.AttributeTargets.Field)] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class VBFixedArrayAttribute : System.Attribute {
		// Declarations
		// Constructors
		VBFixedArrayAttribute(System.Int32 UpperBound1) {}
		VBFixedArrayAttribute(System.Int32 UpperBound1, System.Int32 UpperBound2) {}
		// Properties
		public System.Int32 Length { get {return 0;} }
		public System.Int32[] Bounds { get {return null;} }
		// Methods
		// Events
	};
}

//
// JSFunctionAttribute.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;

namespace Microsoft.JScript {

	[AttributeUsage (AttributeTargets.Method |AttributeTargets.Constructor)]
	public class JSFunctionAttribute : Attribute {

		public JSFunctionAttribute (JSFunctionAttributeEnum value)
		{
			throw new NotImplementedException ();
		}

		public JSFunctionAttribute (JSFunctionAttributeEnum value, JSBuiltin builinFunction)
		{
			throw new NotImplementedException ();
		}

		public JSFunctionAttributeEnum GetAttributeValue ()
		{
			throw new NotImplementedException ();
		}
	}
}
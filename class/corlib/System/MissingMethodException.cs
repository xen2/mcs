using System;
using System.Runtime.Serialization;
using System.Globalization;

namespace System {
	public class MissingMethodException : MissingMemberException {
		public MissingMethodException ()
			: base (Locale.GetText ("A missing method exception has occurred."))
		{
		}

		public MissingMethodException (string message)
			: base (message)
		{
		}

		protected MissingMethodException (SerializationInfo info, StreamingContext context)
			: base (info, context) {
		}
		
		public MissingMethodException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}



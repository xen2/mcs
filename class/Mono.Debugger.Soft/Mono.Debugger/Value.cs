using System;
using System.Collections.Generic;

namespace Mono.Debugger
{
	public abstract class Value : Mirror {

		// FIXME: Add a 'Value' field

		internal Value (VirtualMachine vm, long id) : base (vm, id) {
		}
	}
}


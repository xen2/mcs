//
// System.Web.UI.PersistenceModeAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.All)]
	public sealed class PersistenceModeAttribute : Attribute
	{
		PersistenceMode mode;
		
		public PersistenceModeAttribute (PersistenceMode mode)
		{
			this.mode = mode;
		}

		public static readonly PersistenceModeAttribute Attribute =
						new PersistenceModeAttribute (PersistenceMode.Attribute);

		public static readonly PersistenceModeAttribute Default =
						new PersistenceModeAttribute (PersistenceMode.Attribute);

		public static readonly PersistenceModeAttribute EncodedInnerDefaultProperty =
						new PersistenceModeAttribute (PersistenceMode.EncodedInnerDefaultProperty);

		public static readonly PersistenceModeAttribute InnerDefaultProperty =
						new PersistenceModeAttribute (PersistenceMode.InnerDefaultProperty);

		public static readonly PersistenceModeAttribute InnerProperty =
						new PersistenceModeAttribute (PersistenceMode.InnerProperty);
		
		public PersistenceMode Mode {
			get { return mode; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is PersistenceModeAttribute))
				return false;

			return ((PersistenceModeAttribute) obj).mode == mode;
		}

		public override int GetHashCode ()
		{
			return (int) mode;
		}

		public override bool IsDefaultAttribute ()
		{
			return (mode == PersistenceMode.Attribute);
		}
	}
}
	

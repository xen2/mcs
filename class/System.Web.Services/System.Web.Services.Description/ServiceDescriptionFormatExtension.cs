// 
// System.Web.Services.Description.ServiceDescriptionFormatExtension.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public abstract class ServiceDescriptionFormatExtension {

		#region Fields
		
		bool handled;
		object parent;
		bool required;

		#endregion // Fields

		#region Constructors

		protected ServiceDescriptionFormatExtension () 
		{
			handled = false;
			parent = null;
			required = false;	
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public bool Handled {
			get { return handled; }
			set { handled = value; }
		}

		[XmlIgnore]
		public object Parent {
			get { return parent; }
		}

		[DefaultValue (false)]
		[XmlAttribute ("required", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
		public bool Required {	
			get { return required; }
			set { required = value; }
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (object value)
		{
			parent = value; 
		}
		
		#endregion // Methods
	}
}

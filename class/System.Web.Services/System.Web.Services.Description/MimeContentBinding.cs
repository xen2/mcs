// 
// System.Web.Services.Description.MimeContentBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class MimeContentBinding : ServiceDescriptionFormatExtension {

		#region Fields

		public const string Namespace = "http://schemas.xmlsoap.org/wsdl/mime/";
		string part;
		string type;

		#endregion // Fields

		#region Constructors
		
		public MimeContentBinding ()
		{
			part = String.Empty;
			type = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties
	
		public string Part {
			get { return part; }
			set { part = value; }
		}

		public string Type {
			get { return type; }
			set { type = value; }
		}

		#endregion // Properties
	}
}

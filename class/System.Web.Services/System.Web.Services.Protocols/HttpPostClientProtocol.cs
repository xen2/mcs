// 
// System.Web.Services.Protocols.HttpPostClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Net;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class HttpPostClientProtocol : HttpSimpleClientProtocol {

		#region Constructors

		public HttpPostClientProtocol () 
		{
			TypeStub = (HttpSimpleTypeStubInfo) TypeStubManager.GetTypeStub (GetType(), typeof (HttpPostMethodStubInfo));
		}
		
		#endregion // Constructors

		#region Methods

		protected override WebRequest GetWebRequest (Uri uri)
		{
			if (null == uri)
				throw new InvalidOperationException ("The uri parameter is a null reference.");
			if (String.Empty == uri.ToString ())
				throw new InvalidOperationException ("The uri parameter has a length of zero.");

			WebRequest request = WebRequest.Create (uri);
			request.Method = "POST";
			return request;
		}

		#endregion // Methods
	}
}

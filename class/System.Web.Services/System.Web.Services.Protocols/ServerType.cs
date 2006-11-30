// 
// ServerType.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Description;

namespace System.Web.Services.Protocols
{
	//
	// This class has information abou a web service. Through providess
	// access to the TypeStubInfo instances for each protocol.
	//
#if NET_2_0
	public
#else
	internal
#endif
	class ServerType // It was LogicalTypeInfo until Mono 1.2.
	{
		LogicalMethodInfo[] logicalMethods;

		internal string WebServiceName;
		internal string WebServiceNamespace;
		internal string WebServiceAbstractNamespace;
		internal string Description;
		internal Type Type;
		bool useEncoded;

		TypeStubInfo soapProtocol;
		TypeStubInfo httpGetProtocol;
		TypeStubInfo httpPostProtocol;
		
		public ServerType (Type t)
		{
			this.Type = t;

			object [] o = Type.GetCustomAttributes (typeof (WebServiceAttribute), false);
			if (o.Length == 1){
				WebServiceAttribute a = (WebServiceAttribute) o [0];
				WebServiceName = (a.Name != string.Empty) ? a.Name : Type.Name;
				WebServiceNamespace = (a.Namespace != string.Empty) ? a.Namespace : WebServiceAttribute.DefaultNamespace;
				Description = a.Description;
			} else {
				WebServiceName = Type.Name;
				WebServiceNamespace = WebServiceAttribute.DefaultNamespace;
			}
			
			// Determine the namespaces for literal and encoded schema types
			
			useEncoded = false;
			
			o = t.GetCustomAttributes (typeof(SoapDocumentServiceAttribute), true);
			if (o.Length > 0) {
				SoapDocumentServiceAttribute at = (SoapDocumentServiceAttribute) o[0];
				useEncoded = (at.Use == SoapBindingUse.Encoded);
			}
			else if (t.GetCustomAttributes (typeof(SoapRpcServiceAttribute), true).Length > 0)
				useEncoded = true;
			
			string sep = WebServiceNamespace.EndsWith ("/") ? "" : "/";

			WebServiceAbstractNamespace = WebServiceNamespace + sep + "AbstractTypes";

			MethodInfo [] type_methods = Type.GetMethods (BindingFlags.Instance | BindingFlags.Public);
			logicalMethods = LogicalMethodInfo.Create (type_methods, LogicalMethodTypes.Sync);
		}
		
		internal LogicalMethodInfo[] LogicalMethods
		{
			get { return logicalMethods; }
		}
		
		internal TypeStubInfo GetTypeStub (string protocolName)
		{
			lock (this)
			{
				switch (protocolName)
				{
					case "Soap": 
						if (soapProtocol == null) soapProtocol = CreateTypeStubInfo (typeof(SoapTypeStubInfo));
						return soapProtocol;
					case "HttpGet":
						if (httpGetProtocol == null) httpGetProtocol = CreateTypeStubInfo (typeof(HttpGetTypeStubInfo));
						return httpGetProtocol;
					case "HttpPost":
						if (httpPostProtocol == null) httpPostProtocol = CreateTypeStubInfo (typeof(HttpPostTypeStubInfo));
						return httpPostProtocol;
				}
			}
			throw new InvalidOperationException ("Protocol " + protocolName + " not supported");
		}
		
		TypeStubInfo CreateTypeStubInfo (Type type)
		{
			TypeStubInfo tsi = (TypeStubInfo) Activator.CreateInstance (type, new object[] {this});
			tsi.Initialize ();
			return tsi;
		}
		
		internal string GetWebServiceLiteralNamespace (string baseNamespace)
		{
			if (useEncoded) {
				string sep = baseNamespace.EndsWith ("/") ? "" : "/";
				return baseNamespace + sep + "literalTypes";
			}
			else
				return baseNamespace;
		}

		internal string GetWebServiceEncodedNamespace (string baseNamespace)
		{
			if (useEncoded)
				return baseNamespace;
			else {
				string sep = baseNamespace.EndsWith ("/") ? "" : "/";
				return baseNamespace + sep + "encodedTypes";
			}
		}

		internal string GetWebServiceNamespace (string baseNamespace, SoapBindingUse use)
		{
			if (use == SoapBindingUse.Literal) return GetWebServiceLiteralNamespace (baseNamespace);
			else return GetWebServiceEncodedNamespace (baseNamespace);
		}
		
	}
}

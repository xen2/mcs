//
// ClientCredentialsElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	public class ClientCredentialsElement
		 : BehaviorExtensionElement
	{
		// Static Fields
		ConfigurationPropertyCollection _properties;

		// Properties

		public override Type BehaviorType {
			get { return typeof (ClientCredentials); }
		}

		[ConfigurationProperty ("clientCertificate",
			 Options = ConfigurationPropertyOptions.None)]
		public X509InitiatorCertificateClientElement ClientCertificate {
			get { return (X509InitiatorCertificateClientElement) base ["clientCertificate"]; }
		}

		[ConfigurationProperty ("httpDigest",
			 Options = ConfigurationPropertyOptions.None)]
		public HttpDigestClientElement HttpDigest {
			get { return (HttpDigestClientElement) base ["httpDigest"]; }
		}

		[ConfigurationProperty ("issuedToken",
			 Options = ConfigurationPropertyOptions.None)]
		public IssuedTokenClientElement IssuedToken {
			get { return (IssuedTokenClientElement) base ["issuedToken"]; }
		}

		[ConfigurationProperty ("peer",
			 Options = ConfigurationPropertyOptions.None)]
		public PeerCredentialElement Peer {
			get { return (PeerCredentialElement) base ["peer"]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = new ConfigurationPropertyCollection ();
					_properties.Add (new ConfigurationProperty ("clientCertificate", typeof (X509InitiatorCertificateClientElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("httpDigest", typeof (HttpDigestClientElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("issuedToken", typeof (IssuedTokenClientElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("peer", typeof (PeerCredentialElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("serviceCertificate", typeof (X509RecipientCertificateClientElement), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("supportInteractive", typeof (bool), "true", new BooleanConverter (), null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("type", typeof (string), String.Empty, new StringConverter (), new StringValidator (0, int.MaxValue, null), ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("windows", typeof (WindowsClientElement), null, null, null, ConfigurationPropertyOptions.None));
				}
				return _properties;
			}
		}

		[ConfigurationProperty ("serviceCertificate",
			 Options = ConfigurationPropertyOptions.None)]
		public X509RecipientCertificateClientElement ServiceCertificate {
			get { return (X509RecipientCertificateClientElement) base ["serviceCertificate"]; }
		}

		[ConfigurationProperty ("supportInteractive",
			DefaultValue = true,
			 Options = ConfigurationPropertyOptions.None)]
		public bool SupportInteractive {
			get { return (bool) base ["supportInteractive"]; }
			set { base ["supportInteractive"] = value; }
		}

		[ConfigurationProperty ("type",
			 DefaultValue = "",
			 Options = ConfigurationPropertyOptions.None)]
		[StringValidator (MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		public string Type {
			get { return (string) base ["type"]; }
			set { base ["type"] = value; }
		}

		[ConfigurationProperty ("windows",
			 Options = ConfigurationPropertyOptions.None)]
		public WindowsClientElement Windows {
			get { return (WindowsClientElement) base ["windows"]; }
		}

		[MonoTODO]
		protected internal override object CreateBehavior () {
			throw new NotImplementedException ();
		}

	}

}

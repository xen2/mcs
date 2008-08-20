//
// X509RecipientCertificateClientElement.cs
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
	[MonoTODO]
	public sealed partial class X509RecipientCertificateClientElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty authentication;
		static ConfigurationProperty default_certificate;
		static ConfigurationProperty scoped_certificates;

		static X509RecipientCertificateClientElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			authentication = new ConfigurationProperty ("authentication",
				typeof (X509ServiceCertificateAuthenticationElement), null, null/* FIXME: get converter for X509ServiceCertificateAuthenticationElement*/, null,
				ConfigurationPropertyOptions.None);

			default_certificate = new ConfigurationProperty ("defaultCertificate",
				typeof (X509DefaultServiceCertificateElement), null, null/* FIXME: get converter for X509DefaultServiceCertificateElement*/, null,
				ConfigurationPropertyOptions.None);

			scoped_certificates = new ConfigurationProperty ("scopedCertificates",
				typeof (X509ScopedServiceCertificateElementCollection), null, null/* FIXME: get converter for X509ScopedServiceCertificateElementCollection*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (authentication);
			properties.Add (default_certificate);
			properties.Add (scoped_certificates);
		}

		public X509RecipientCertificateClientElement ()
		{
		}


		// Properties

		[ConfigurationProperty ("authentication",
			 Options = ConfigurationPropertyOptions.None)]
		public X509ServiceCertificateAuthenticationElement Authentication {
			get { return (X509ServiceCertificateAuthenticationElement) base [authentication]; }
		}

		[ConfigurationProperty ("defaultCertificate",
			 Options = ConfigurationPropertyOptions.None)]
		public X509DefaultServiceCertificateElement DefaultCertificate {
			get { return (X509DefaultServiceCertificateElement) base [default_certificate]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("scopedCertificates",
			 Options = ConfigurationPropertyOptions.None)]
		public X509ScopedServiceCertificateElementCollection ScopedCertificates {
			get { return (X509ScopedServiceCertificateElementCollection) base [scoped_certificates]; }
		}


	}

}

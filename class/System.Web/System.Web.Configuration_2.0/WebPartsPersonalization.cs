//
// System.Web.Configuration.WebPartsPersonalization
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class WebPartsPersonalization : ConfigurationElement
	{
		static ConfigurationProperty authorizationProp;
		static ConfigurationProperty defaultProviderProp;
		static ConfigurationProperty providersProp;
		static ConfigurationPropertyCollection properties;

		static WebPartsPersonalization ()
		{
			authorizationProp = new ConfigurationProperty ("authorization", typeof (WebPartsPersonalizationAuthorization));
			defaultProviderProp = new ConfigurationProperty ("defaultProvider", typeof (string), "AspNetSqlPersonalizationProvider");
			providersProp = new ConfigurationProperty ("providers", typeof (ProviderSettingsCollection));
			properties = new ConfigurationPropertyCollection ();

			properties.Add (authorizationProp);
			properties.Add (defaultProviderProp);
			properties.Add (providersProp);
		}

		[ConfigurationProperty ("authorization")]
		public WebPartsPersonalizationAuthorization Authorization {
			get { return (WebPartsPersonalizationAuthorization) base [authorizationProp];}
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("defaultProvider", DefaultValue = "AspNetSqlPersonalizationProvider")]
		public string DefaultProvider {
			get { return (string) base [defaultProviderProp];}
			set { base[defaultProviderProp] = value; }
		}

		[ConfigurationProperty ("providers")]
		public ProviderSettingsCollection Providers {
			get { return (ProviderSettingsCollection) base [providersProp];}
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif


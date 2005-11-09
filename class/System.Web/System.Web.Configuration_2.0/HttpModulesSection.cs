//
// System.Web.Configuration.HttpModulesSection
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

#if NET_2_0

using System;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class HttpModulesSection: ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty modulesProp;

		public HttpModulesSection ()
		{
			properties = new ConfigurationPropertyCollection ();
			modulesProp = new ConfigurationProperty ("", typeof (HttpModuleActionCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
			properties.Add (modulesProp);
		}

		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public HttpModuleActionCollection Modules {
			get {
				return (HttpModuleActionCollection) base [modulesProp];
			}
		}

		[MonoTODO]
		protected override ConfigurationPropertyCollection Properties {
			get {
				return properties;
			}
		}

		/* stolen from the 1.0 S.W.Config ModulesConfiguration.cs */
		[MonoTODO]
		internal HttpModuleCollection LoadModules (HttpApplication app)
		{
			HttpModuleCollection coll = new HttpModuleCollection ();
			foreach (HttpModuleAction item in Modules){
				Type type = Type.GetType (item.Type);
				if (type == null) {
					/* XXX should we throw here? */
					continue;
				}
				IHttpModule module = (IHttpModule) Activator.CreateInstance (type);
				module.Init (app);
				coll.AddModule (item.Name, module);
			}

			return coll;
		}

	}
}

#endif

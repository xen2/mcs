//
// System.Web.Configuration.HttpModuleAction
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
	public sealed class HttpModuleAction: ConfigurationElement
	{
		static ConfigurationPropertyCollection _properties;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty typeProp;

		[MonoTODO]
		static HttpModuleAction ()
		{
			/* XXX fill in _properties */
		}

		[MonoTODO]
		public HttpModuleAction (string name, string type)
		{
			Name = name;
			Type = type;
		}
#if notyet
		[MonoTODO]
		protected override ConfigurationElementProperty ElementProperty {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		[StringValidator (MinLength = 1)]
		public string Name {
			get { return (string)base[nameProp]; }
			set { base[nameProp] = value; }
		}

		[ConfigurationProperty ("type", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string Type {
			get { return (string)base[typeProp]; }
			set { base[typeProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get {
				return _properties;
			}
		}
	}
}

#endif

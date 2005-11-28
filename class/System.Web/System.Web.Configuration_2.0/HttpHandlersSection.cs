//
// System.Web.Configuration.HttpHandlersSection
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
	public sealed class HttpHandlersSection: ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty handlersProp;

		static HttpHandlersSection ()
		{
			handlersProp = new ConfigurationProperty ("", typeof (HttpHandlerActionCollection), null,
								  null, PropertyHelper.DefaultValidator,
								  ConfigurationPropertyOptions.IsDefaultCollection);


			properties = new ConfigurationPropertyCollection ();

			properties.Add (handlersProp);
		}

		public HttpHandlersSection ()
		{
		}

		[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
		public HttpHandlerActionCollection Handlers {
			get { return (HttpHandlerActionCollection) base[handlersProp]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

#region CompatabilityCode
		internal object LocateHandler (string verb, string filepath)
		{
			int top = Handlers.Count;

			for (int i = 0; i < top; i++){
				HttpHandlerAction handler = (HttpHandlerAction) Handlers [i];

				string[] verbs = handler.Verbs;
				if (verbs == null){
					if (handler.PathMatches (filepath))
						return handler.GetHandlerInstance ();
					continue;
				}

				for (int j = verbs.Length; j > 0; ){
					j--;
					if (verbs [j] != verb)
						continue;
					if (handler.PathMatches (filepath))
						return handler.GetHandlerInstance ();
				}
			}

			return null;
		}
#endregion
	}
}

#endif

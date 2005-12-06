//
// System.Web.HttpCookie.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Text;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Web {

	[Flags]
	internal enum CookieFlags : byte {
		Secure = 1,
		HttpOnly = 2
	}
	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpCookie {

		string path = "/";
		string domain;
		DateTime expires = DateTime.MinValue;
		string name;
		CookieFlags flags = 0;
		NameValueCollection values;

		[Obsolete]
		internal HttpCookie (string name, string value, string path, DateTime expires)
		{
			this.name = name;
			this.values = new CookieNVC();
			this.Value = value;
			this.path = path;
			this.expires = expires;
		}

		public HttpCookie (string name)
		{
			this.name = name;
			values = new CookieNVC();
			Value = "";
		}

		public HttpCookie (string name, string value)
		  : this (name)
		{
			Value = value;
		}

		internal BaseResponseHeader GetCookieHeader ()
		{
			StringBuilder builder = new StringBuilder ("");

			builder.Append (name);
			builder.Append ("=");
			builder.Append (Value);

			if (domain != null) {
				builder.Append ("; domain=");
				builder.Append (domain);
			}
	       
			if (path != null) {
				builder.Append ("; path=");
				builder.Append (path);
			}

			if (expires != DateTime.MinValue) {
				builder.Append ("; expires=");
				builder.Append (expires.ToUniversalTime().ToString("r"));
			}

			if ((flags & CookieFlags.Secure) != 0) {
				builder.Append ("; secure");
			}

			if ((flags & CookieFlags.HttpOnly) != 0){
				builder.Append ("; HttpOnly");
			}

			return new UnknownResponseHeader ("Set-Cookie", builder.ToString());
		}

		public string Domain {
			get {
				return domain;
			}
			set {
				domain = value;
			}
		}

		public DateTime Expires {
			get {
				return expires;
			}
			set {
				expires = value;
			}
		}

		public bool HasKeys {
			get {
				return values.HasKeys();
			}
		}


		public string this [ string key ] {
			get {
				return values [ key ];
			}
			set {
				values [ key ] = value;
			}
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public string Path {
			get {
				return path;
			}
			set {
				path = value;
			}
		}

		public bool Secure {
			get {
				return (flags & CookieFlags.Secure) == CookieFlags.Secure;
			}
			set {
				if (value)
					flags |= CookieFlags.Secure;
				else
					flags &= ~CookieFlags.Secure;
			}
		}

		public string Value {
			get {
				return values.ToString ();
			}
			set {
				values.Clear ();
				
				if (value != null && value != "") {
					string [] components = value.Split ('&');
					foreach (string kv in components){
						int pos = kv.IndexOf ('=');
						if (pos == -1){
							values.Add (null, kv);
						} else {
							string key = kv.Substring (0, pos);
							string val = kv.Substring (pos+1);
							
							values.Add (key, val);
						}
					}
				}
			}
		}

		public NameValueCollection Values {
			get {
				return values;
			}
		}

#if NET_2_0
		public bool HttpOnly {
			get {
				return (flags & CookieFlags.HttpOnly) == CookieFlags.HttpOnly;
			}

			set {
				flags |= CookieFlags.HttpOnly;
			}
		}
#endif

		/*
		 * simple utility class that just overrides ToString
		 * to get the desired behavior for
		 * HttpCookie.Values
		 */
		class CookieNVC : NameValueCollection
		{
			public override string ToString ()
			{
				StringBuilder builder = new StringBuilder ("");

				bool first_key = true;
				foreach (string key in Keys) {
					if (!first_key)
						builder.Append ("&");

					bool first_val = true;
					foreach (string v in GetValues (key)) {
						if (!first_val)
							builder.Append ("&");

						if (key != null) {
							builder.Append (key);
							builder.Append ("=");
						}
						builder.Append (v);
						first_val = false;
					}
					first_key = false;
				}

				return builder.ToString();
			}

			/* MS's implementation has the interesting quirk that if you do:
			 * cookie.Values[null] = "foo"
			 * it clears out the rest of the values.
			 */
			public override void Set (string name, string value)
			{
				if (this.IsReadOnly)
					throw new NotSupportedException ("Collection is read-only");

				if (name == null)
					Clear();

				base.Set (name, value);
			}
		}
	}

}

//
// X509SecurityTokenProvider.cs
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
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace System.IdentityModel.Selectors
{
	public class X509SecurityTokenProvider : SecurityTokenProvider, IDisposable
	{
		X509Certificate2 cert;
		X509Store store;

		public X509SecurityTokenProvider (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			cert = certificate;
		}

		public X509SecurityTokenProvider (StoreLocation storeLocation,
			StoreName storeName, X509FindType findType, object findValue)
		{
			if (findValue == null)
				throw new ArgumentNullException ("findValue");

			store = new X509Store (storeName, storeLocation);
			store.Open (OpenFlags.ReadOnly);
			foreach (X509Certificate2 hit in store.Certificates.Find (findType, findValue, true)) {
				if (cert != null)
					throw new SecurityTokenException ("X509SecurityTokenProvider does not allow such certificate specification that indicates more than one certificate. Use more specific find value.");
				cert = hit;
			}
		}

		public X509Certificate2 Certificate {
			get { return cert; }
		}

		public void Dispose ()
		{
			if (store != null)
				store.Close ();
		}

		protected override SecurityToken GetTokenCore (TimeSpan timeout)
		{
			return new X509SecurityToken (cert);
		}
	}
}

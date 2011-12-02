//
// HttpHeadersTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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
using NUnit.Framework;
using System.Net.Http.Headers;
using System.Linq;

namespace MonoTests.System.Net.Http.Headers
{
	[TestFixture]
	public class HttpHeadersTest
	{
		HttpHeaders headers;

		class HttpHeadersMock : HttpHeaders
		{
		}

		[SetUp]
		public void Setup ()
		{
			headers = new HttpHeadersMock ();
		}

		[Test]
		public void Add ()
		{
			headers.Add ("aa", "value");
			headers.Add ("aa", "value");
			headers.Add ("Expires", (string) null);
		}

		[Test]
		public void Add_InvalidArguments ()
		{
			try {
				headers.Add (null, "value");
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				headers.Add ("", "value");
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Clear ()
		{
			headers.Add ("aa", "value");
			headers.Clear ();
		}

		[Test]
		public void GetValues ()
		{
			headers.Add ("aa", "v");
			headers.Add ("aa", "v");

			var r = headers.GetValues ("aa").ToList ();
			Assert.AreEqual ("v", r[0], "#1");
			Assert.AreEqual ("v", r[1], "#2");
		}

		[Test]
		public void GetValues_Invalid ()
		{
			try {
				headers.GetValues (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				headers.GetValues ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				headers.GetValues ("x");
				Assert.Fail ("#3");
			} catch (InvalidOperationException) {
			}
		}
	}
}

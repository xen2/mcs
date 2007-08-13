//
// ArgumentExceptionTest.cs - Unit tests for
//	System.ArgumentException
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2007 Gert Driesen
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

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class ArgumentExceptionTest
	{
		[Test] // ctor (string, string)
		public void Constructor4 ()
		{
			ArgumentException ae = new ArgumentException ("MSG", (string) null);
			Assert.IsNotNull (ae.Message, "#A1");
			Assert.AreEqual ("MSG", ae.Message, "#A2");
			Assert.IsNull (ae.ParamName, "#A3");

			ae = new ArgumentException ("MSG", string.Empty);
			Assert.IsNotNull (ae.Message, "#B1");
			Assert.AreEqual ("MSG", ae.Message, "#B2");
			Assert.IsNotNull (ae.ParamName, "#B3");
			Assert.AreEqual (string.Empty, ae.ParamName, "#B4");

			ae = new ArgumentException ("MSG", "PARAM");
			Assert.IsNotNull (ae.Message, "#C1");
			Assert.IsTrue (ae.Message.StartsWith ("MSG"), "#C2");
			Assert.IsTrue (ae.Message.IndexOf (Environment.NewLine) != -1, "#C3");
			Assert.IsTrue (ae.Message.IndexOf ("PARAM") != -1, "#C4");
			Assert.IsNotNull (ae.ParamName, "#C5");
			Assert.AreEqual ("PARAM", ae.ParamName, "#C6");

			ae = new ArgumentException ("MSG", " \t ");
			Assert.IsNotNull (ae.Message, "#D1");
			Assert.IsTrue (ae.Message.StartsWith ("MSG"), "#D2");
			Assert.IsTrue (ae.Message.IndexOf (Environment.NewLine) != -1, "#D3");
			Assert.IsTrue (ae.Message.IndexOf (" \t ") != -1, "#D4");
			Assert.IsNotNull (ae.ParamName, "#D5");
			Assert.AreEqual (" \t ", ae.ParamName, "#D6");
		}
	}
}

//
// I18N.CJK.Test.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//

using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace MonoTests.I18N.CJK
{
	[TestFixture]
	public class TestCJK
	{
		void AssertEncode (string utf8file, string decfile, int codepage)
		{
			string decoded = null;
			byte [] encoded = null;
			using (StreamReader sr = new StreamReader (utf8file,
				Encoding.UTF8)) {
				decoded = sr.ReadToEnd ();
			}
			using (FileStream fs = File.OpenRead (decfile)) {
				encoded = new byte [fs.Length];
				fs.Read (encoded, 0, (int) fs.Length);
			}
			Encoding enc = Encoding.GetEncoding (codepage);
			byte [] actual;

			// simple string case
			//Assert.AreEqual (encoded.Length,
			//	enc.GetByteCount (decoded),
			//	"GetByteCount(string)");
			actual = enc.GetBytes (decoded);
			Assert.AreEqual (encoded, actual,
				"GetBytes(string)");

			// simple char[] case
			Assert.AreEqual (encoded.Length,
				enc.GetByteCount (decoded.ToCharArray (), 0, decoded.Length),
				"GetByteCount(char[], 0, len)");
			actual = enc.GetBytes (decoded.ToCharArray (), 0, decoded.Length);
			Assert.AreEqual (encoded, actual,
				"GetBytes(char[], 0, len)");
		}

		void AssertDecode (string utf8file, string decfile, int codepage)
		{
			string decoded = null;
			byte [] encoded = null;
			using (StreamReader sr = new StreamReader (utf8file,
				Encoding.UTF8)) {
				decoded = sr.ReadToEnd ();
			}
			using (FileStream fs = File.OpenRead (decfile)) {
				encoded = new byte [fs.Length];
				fs.Read (encoded, 0, (int) fs.Length);
			}
			Encoding enc = Encoding.GetEncoding (codepage);
			char [] actual;

			Assert.AreEqual (decoded.Length,
				enc.GetCharCount (encoded, 0, encoded.Length),
				"GetCharCount(byte[], 0, len)");
			actual = enc.GetChars (encoded, 0, encoded.Length);
			Assert.AreEqual (decoded.ToCharArray (), actual,
				"GetChars(byte[], 0, len)");
		}

		#region Chinese

		// GB2312

		[Test]
		public void CP936_Encode ()
		{
			AssertEncode ("Test/texts/chinese-utf8.txt", "Test/texts/chinese-936.txt", 936);
		}

		[Test]
		public void CP936_Decode ()
		{
			AssertDecode ("Test/texts/chinese-utf8.txt", "Test/texts/chinese-936.txt", 936);
		}

		// BIG5

		[Test]
		public void CP950_Encode ()
		{
			AssertEncode ("Test/texts/chinese2-utf8.txt", "Test/texts/chinese2-950.txt", 950);
		}

		[Test]
		public void CP950_Decode ()
		{
			AssertDecode ("Test/texts/chinese2-utf8.txt", "Test/texts/chinese2-950.txt", 950);
		}

		// GB18030

		[Test]
		public void CP54936_Encode ()
		{
			AssertEncode ("Test/texts/chinese-utf8.txt", "Test/texts/chinese-54936.txt", 54936);
		}

		[Test]
		public void CP54936_Decode ()
		{
			AssertDecode ("Test/texts/chinese-utf8.txt", "Test/texts/chinese-54936.txt", 54936);
		}

		#endregion

		#region Japanese

		// Shift_JIS

		[Test]
		public void CP932_Encode ()
		{
			AssertEncode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-932.txt", 932);
		}

		[Test]
		public void CP932_Decode ()
		{
			AssertDecode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-932.txt", 932);
		}

		// EUC-JP

		[Test]
		public void CP51932_Encode ()
		{
			AssertEncode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-51932.txt", 51932);
		}

		[Test]
		public void CP51932_Decode ()
		{
			AssertDecode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-51932.txt", 51932);
		}

		// ISO-2022-JP

		[Test]
		public void CP50220_Encode ()
		{
			AssertEncode ("Test/texts/japanese2-utf8.txt", "Test/texts/japanese2-50220.txt", 50220);
		}

		[Test]
		public void CP50220_Decode ()
		{
			AssertDecode ("Test/texts/japanese2-utf8.txt", "Test/texts/japanese2-50220.txt", 50220);
		}

		[Test]
		public void CP50221_Encode ()
		{
			AssertEncode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-50221.txt", 50221);
		}

		[Test]
		public void CP50221_Decode ()
		{
			AssertDecode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-50221.txt", 50221);
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")] // MS is buggy here
#endif
		public void CP50222_Encode ()
		{
			AssertEncode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-50222.txt", 50222);
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")] // MS is buggy here
#endif
		public void CP50222_Decode ()
		{
			AssertDecode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-50222.txt", 50222);
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")] // MS bug
#endif
		public void Bug77723 ()
		{
			GetBytesAllSingleChars (51932);
		}

		[Test]
		public void Bug77724 ()
		{
			GetBytesAllSingleChars (932);
		}

		[Test]
		public void Bug77307 ()
		{
			GetBytesAllSingleChars (54936);
		}

		void GetBytesAllSingleChars (int enc)
		{
			Encoding e = Encoding.GetEncoding (enc);
			for (int i = 0; i < 0x10000; i++)
				e.GetBytes (new char [] { (char)i });
		}

		void GetCharsAllBytePairs (int enc)
		{
			Encoding e = Encoding.GetEncoding (enc);
			byte [] bytes = new byte [2];
			for (int i0 = 0; i0 < 0x100; i0++) {
				bytes [0] = (byte) i0;
				for (int i1 = 0; i1 < 0x100; i1++) {
					bytes [1] = (byte) i1;
					e.GetChars (bytes);
				}
			}
		}

		[Test]
		public void Bug77222 ()
		{
			GetCharsAllBytePairs (51932);
		}

		[Test]
		public void Bug77238 ()
		{
			GetCharsAllBytePairs (936);
		}

		[Test]
		public void Bug77306 ()
		{
			GetCharsAllBytePairs (54936);
		}

		[Test]
		public void Bug77298 ()
		{
			GetCharsAllBytePairs (949);
		}

		[Test]
		public void Bug77274 ()
		{
			GetCharsAllBytePairs (950);
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")] // MS bug
#endif
		public void Encoder54936Refresh ()
		{
			Encoding e = Encoding.GetEncoding ("gb18030");
			Encoder d = e.GetEncoder ();
			byte [] bytes;

			bytes = new byte [4];
			Assert.AreEqual (0, d.GetBytes (new char [] {'\uD800'}, 0, 1, bytes, 0, false), "#1");
			Assert.AreEqual (new byte [] {00, 00, 00, 00},
				bytes, "#2");

			bytes = new byte [4];
			Assert.AreEqual (4, d.GetBytes (new char [] {'\uDC00'}, 0, 1, bytes, 0, true), "#3");
			Assert.AreEqual (new byte [] {0x90, 0x30, 0x81, 0x30},
				bytes, "#4");

			bytes = new byte [4];
			Assert.AreEqual (1, d.GetBytes (new char [] {'\uD800'}, 0, 1, bytes, 0, true), "#5");
			Assert.AreEqual (new byte [] {0x3F, 00, 00, 00},
				bytes, "#6");
		}

#if NET_2_0
		[Test]
		public void Decoder932Refresh ()
		{
			Encoding e = Encoding.GetEncoding (932);
			Decoder d = e.GetDecoder ();
			char [] chars;

			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0x81}, 0, 1, chars, 0, false), "#1");
			Assert.AreEqual (new char [] {'\0'}, chars, "#2");

			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0x81}, 0, 1, chars, 0, true), "#3");
			Assert.AreEqual (new char [] {'\uFF1D'}, chars, "#4");

			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0x81}, 0, 1, chars, 0, true), "#5");
			Assert.AreEqual (new char [] {'\u30FB'}, chars, "#6");
		}

		[Test]
		public void Decoder51932Refresh ()
		{
			Encoding e = Encoding.GetEncoding (51932);
			Decoder d = e.GetDecoder ();
			char [] chars;

			// invalid one
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0x81}, 0, 1, chars, 0, false), "#0.1");
			Assert.AreEqual (new char [] {'\u30FB'}, chars, "#0.2");

			// incomplete
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0xA1}, 0, 1, chars, 0, false), "#1");
			Assert.AreEqual (new char [] {'\0'}, chars, "#2");

			// became complete
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0xA1}, 0, 1, chars, 0, true), "#3");
			Assert.AreEqual (new char [] {'\u3000'}, chars, "#4");

			// incomplete but refreshed
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0xA1}, 0, 1, chars, 0, true), "#5");
			Assert.AreEqual (new char [] {'\u30FB'}, chars, "#6");
		}

		[Test]
		public void Decoder936Refresh ()
		{
			Encoding e = Encoding.GetEncoding (936);
			Decoder d = e.GetDecoder ();
			char [] chars;

			// incomplete
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0xB0}, 0, 1, chars, 0, false), "#1");
			Assert.AreEqual (new char [] {'\0'}, chars, "#2");

			// became complete
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0xA1}, 0, 1, chars, 0, false), "#3");
			Assert.AreEqual (new char [] {'\u554A'}, chars, "#4");

			// incomplete but refreshed
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0xB0}, 0, 1, chars, 0, true), "#5");
			Assert.AreEqual (new char [] {'?'}, chars, "#6");
		}

		[Test]
		public void Decoder949Refresh ()
		{
			Encoding e = Encoding.GetEncoding (949);
			Decoder d = e.GetDecoder ();
			char [] chars;

			// incomplete
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0x81}, 0, 1, chars, 0, false), "#1");
			Assert.AreEqual (new char [] {'\0'}, chars, "#2");

			// became complete
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0x41}, 0, 1, chars, 0, false), "#3");
			Assert.AreEqual (new char [] {'\uAC02'}, chars, "#4");

			// incomplete but refreshed
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0x81}, 0, 1, chars, 0, true), "#5");
			Assert.AreEqual (new char [] {'?'}, chars, "#6");
		}

		[Test]
		public void Decoder950Refresh ()
		{
			Encoding e = Encoding.GetEncoding (950);
			Decoder d = e.GetDecoder ();
			char [] chars;

			// incomplete
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0xF9}, 0, 1, chars, 0, false), "#1");
			Assert.AreEqual (new char [] {'\0'}, chars, "#2");

			// became complete
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0x40}, 0, 1, chars, 0, false), "#3");
			Assert.AreEqual (new char [] {'\u7E98'}, chars, "#4");

			// incomplete but refreshed
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0xF9}, 0, 1, chars, 0, true), "#5");
			Assert.AreEqual (new char [] {'?'}, chars, "#6");
		}
#endif


		[Test]
		public void Decoder51932NoRefresh ()
		{
			Encoding e = Encoding.GetEncoding (51932);
			Decoder d = e.GetDecoder ();
			char [] chars;

			// incomplete
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0xA1}, 0, 1, chars, 0), "#1");
			Assert.AreEqual (new char [] {'\0'}, chars, "#2");

			// became complete
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0xA1}, 0, 1, chars, 0), "#3");
			Assert.AreEqual (new char [] {'\u3000'}, chars, "#4");

			// incomplete but refreshed
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0xA1}, 0, 1, chars, 0), "#5");
			Assert.AreEqual (new char [] {'\0'}, chars, "#6");
		}

		[Test]
		public void Decoder936NoRefresh ()
		{
			Encoding e = Encoding.GetEncoding (936);
			Decoder d = e.GetDecoder ();
			char [] chars;

			// incomplete
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0xB0}, 0, 1, chars, 0), "#1");
			Assert.AreEqual (new char [] {'\0'}, chars, "#2");

			// became complete
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0xA1}, 0, 1, chars, 0), "#3");
			Assert.AreEqual (new char [] {'\u554A'}, chars, "#4");

			// incomplete but refreshed
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0xB0}, 0, 1, chars, 0), "#5");
			Assert.AreEqual (new char [] {'\0'}, chars, "#6");
		}

		[Test]
		public void Decoder949NoRefresh ()
		{
			Encoding e = Encoding.GetEncoding (949);
			Decoder d = e.GetDecoder ();
			char [] chars;

			// incomplete
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0x81}, 0, 1, chars, 0), "#1");
			Assert.AreEqual (new char [] {'\0'}, chars, "#2");

			// became complete
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0x41}, 0, 1, chars, 0), "#3");
			Assert.AreEqual (new char [] {'\uAC02'}, chars, "#4");

			// incomplete but refreshed
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0x81}, 0, 1, chars, 0), "#5");
			Assert.AreEqual (new char [] {'\0'}, chars, "#6");
		}

		[Test]
		public void Decoder950NoRefresh ()
		{
			Encoding e = Encoding.GetEncoding (950);
			Decoder d = e.GetDecoder ();
			char [] chars;

			// incomplete
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0xF9}, 0, 1, chars, 0), "#1");
			Assert.AreEqual (new char [] {'\0'}, chars, "#2");

			// became complete
			chars = new char [1];
			Assert.AreEqual (1, d.GetChars (new byte [] {0x40}, 0, 1, chars, 0), "#3");
			Assert.AreEqual (new char [] {'\u7E98'}, chars, "#4");

			// incomplete but refreshed
			chars = new char [1];
			Assert.AreEqual (0, d.GetChars (new byte [] {0xF9}, 0, 1, chars, 0), "#5");
			Assert.AreEqual (new char [] {'\0'}, chars, "#6");
		}

		[Test]
		public void HandleObsoletedESCJ () // bug #398273
		{
			byte [] b = new byte [] {0x64, 0x6f, 0x6e, 0x1b, 0x24, 0x42, 0x21, 0x47, 0x1b, 0x28, 0x4a, 0x74};
			string s = Encoding.GetEncoding ("ISO-2022-JP").GetString (b);
			Assert.AreEqual ("don\u2019t", s);

		}
		#endregion

		#region Korean

		[Test]
		public void CP949_Encode ()
		{
			AssertEncode ("Test/texts/korean-utf8.txt", "Test/texts/korean-949.txt", 949);
		}

		[Test]
		public void CP949_Decode ()
		{
			AssertDecode ("Test/texts/korean-utf8.txt", "Test/texts/korean-949.txt", 949);
		}

		#endregion
	}
}

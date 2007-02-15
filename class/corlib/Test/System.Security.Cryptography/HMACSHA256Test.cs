//
// HMACSHA256Test.cs - NUnit Test Cases for HMACSHA256
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2006, 2007 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	public class HS256 : HMACSHA256 {

		public int BlockSize {
			get { return base.BlockSizeValue; }
			set { base.BlockSizeValue = value; }
		}
	}

	// References:
	// a.	The HMAC-SHA-256-128 Algorithm and Its Use With IPsec
	//	http://www.ietf.org/proceedings/02jul/I-D/draft-ietf-ipsec-ciph-sha-256-01.txt

	[TestFixture]
	public class HMACSHA256Test : KeyedHashAlgorithmTest {

		protected HMACSHA256 algo;

		[SetUp]
		protected override void SetUp () 
		{
			algo = new HMACSHA256 ();
			algo.Key = new byte [8];
			hash = algo;
		}

		// the hash algorithm only exists as a managed implementation
		public override bool ManagedHashImplementation {
			get { return true; }
		}

		[Test]
		public void Constructors () 
		{
			algo = new HMACSHA256 ();
			Assert.IsNotNull (algo, "HMACSHA256 ()");

			byte[] key = new byte [8];
			algo = new HMACSHA256 (key);
			Assert.IsNotNull (algo, "HMACSHA256 (key)");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Null () 
		{
			new HMACSHA256 (null);
		}

		[Test]
		public void Invariants () 
		{
			Assert.IsTrue (algo.CanReuseTransform, "HMACSHA256.CanReuseTransform");
			Assert.IsTrue (algo.CanTransformMultipleBlocks, "HMACSHA256.CanTransformMultipleBlocks");
			Assert.AreEqual ("SHA256", algo.HashName, "HMACSHA256.HashName");
			Assert.AreEqual (256, algo.HashSize, "HMACSHA256.HashSize");
			Assert.AreEqual (1, algo.InputBlockSize, "HMACSHA256.InputBlockSize");
			Assert.AreEqual (1, algo.OutputBlockSize, "HMACSHA256.OutputBlockSize");
			Assert.AreEqual ("System.Security.Cryptography.HMACSHA256", algo.ToString (), "HMACSHA256.ToString()"); 
		}

		[Test]
		public void BlockSize ()
		{
			HS256 hmac = new HS256 ();
			Assert.AreEqual (64, hmac.BlockSize, "BlockSizeValue");
		}

		public void Check (string testName, byte[] key, byte[] data, byte[] result) 
		{
			string classTestName = "HMACSHA256-" + testName;
			CheckA (testName, key, data, result);
			CheckB (testName, key, data, result);
			CheckC (testName, key, data, result);
			CheckD (testName, key, data, result);
			CheckE (testName, key, data, result);
		}

		public void CheckA (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACSHA256 ();
			algo.Key = key;
			byte[] hmac = algo.ComputeHash (data);
			Assert.AreEqual (result, hmac, testName + "a1");
			Assert.AreEqual (result, algo.Hash, testName + "a2");
		}

		public void CheckB (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACSHA256 ();
			algo.Key = key;
			byte[] hmac = algo.ComputeHash (data, 0, data.Length);
			Assert.AreEqual (result, hmac, testName + "b1");
			Assert.AreEqual (result, algo.Hash, testName + "b2");
		}
	
		public void CheckC (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACSHA256 ();
			algo.Key = key;
			MemoryStream ms = new MemoryStream (data);
			byte[] hmac = algo.ComputeHash (ms);
			Assert.AreEqual (result, hmac, testName + "c1");
			Assert.AreEqual (result, algo.Hash, testName + "c2");
		}

		public void CheckD (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACSHA256 ();
			algo.Key = key;
			// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
			algo.TransformFinalBlock (data, 0, data.Length);
			Assert.AreEqual (result, algo.Hash, testName + "d");
		}

		public void CheckE (string testName, byte[] key, byte[] data, byte[] result) 
		{
			algo = new HMACSHA256 ();
			algo.Key = key;
			byte[] copy = new byte [data.Length];
			// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
			for (int i=0; i < data.Length - 1; i++)
				algo.TransformBlock (data, i, 1, copy, i);
			algo.TransformFinalBlock (data, data.Length - 1, 1);
			Assert.AreEqual (result, algo.Hash, testName + "e");
		}

		[Test]
		// Test Case #1: HMAC-SHA-256 with 3-byte input and 32-byte key
		public void HMACSHA256_TC1 () 
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20 };
			byte[] data = Encoding.Default.GetBytes ("abc");
			byte[] digest = { 0xa2, 0x1b, 0x1f, 0x5d, 0x4c, 0xf4, 0xf7, 0x3a, 0x4d, 0xd9, 0x39, 0x75, 0x0f, 0x7a, 0x06, 0x6a, 0x7f, 0x98, 0xcc, 0x13, 0x1c, 0xb1, 0x6a, 0x66, 0x92, 0x75, 0x90, 0x21, 0xcf, 0xab, 0x81, 0x81 };
			Check ("HMACSHA256-TC1", key, data, digest);
		}

		[Test]
		// HMAC-SHA-256 with 56-byte input and 32-byte key
		public void HMACSHA256_TC2 () 
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20 };
			byte[] data = Encoding.Default.GetBytes ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq");
			byte[] digest = { 0x10, 0x4f, 0xdc, 0x12, 0x57, 0x32, 0x8f, 0x08, 0x18, 0x4b, 0xa7, 0x31, 0x31, 0xc5, 0x3c, 0xae, 0xe6, 0x98, 0xe3, 0x61, 0x19, 0x42, 0x11, 0x49, 0xea, 0x8c, 0x71, 0x24, 0x56, 0x69, 0x7d, 0x30 };
			Check ("HMACSHA256-TC2", key, data, digest);
		}

		[Test]
		// Test Case #3: HMAC-SHA-256 with 112-byte (multi-block) input	and 32-byte key
		public void HMACSHA256_TC3 () 
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20 };
			byte[] data = Encoding.Default.GetBytes ("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopqabcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq");
			byte[] digest = { 0x47, 0x03, 0x05, 0xfc, 0x7e, 0x40, 0xfe, 0x34, 0xd3, 0xee, 0xb3, 0xe7, 0x73, 0xd9, 0x5a, 0xab, 0x73, 0xac, 0xf0, 0xfd, 0x06, 0x04, 0x47, 0xa5, 0xeb, 0x45, 0x95, 0xbf, 0x33, 0xa9, 0xd1, 0xa3 };
			Check ("HMACSHA256-TC3", key, data, digest);
		}

		[Test]
		// Test Case #4:  HMAC-SHA-256 with 8-byte input and 32-byte key
		public void HMACSHA256_TC4 () 
		{
			byte[] key = { 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b };
			byte[] data = Encoding.Default.GetBytes ("Hi There");
			byte[] digest = { 0x19, 0x8a, 0x60, 0x7e, 0xb4, 0x4b, 0xfb, 0xc6, 0x99, 0x03, 0xa0, 0xf1, 0xcf, 0x2b, 0xbd, 0xc5, 0xba, 0x0a, 0xa3, 0xf3, 0xd9, 0xae, 0x3c, 0x1c, 0x7a, 0x3b, 0x16, 0x96, 0xa0, 0xb6, 0x8c, 0xf7 };
			Check ("HMACSHA256-TC4", key, data, digest);
		}

		[Test]
		// Test Case #5:  HMAC-SHA-256 with 28-byte input and 4-byte key
		public void HMACSHA256_TC5 () 
		{
			byte[] key = Encoding.Default.GetBytes ("Jefe");
			byte[] data = Encoding.Default.GetBytes ("what do ya want for nothing?");
			byte[] digest = { 0x5b, 0xdc, 0xc1, 0x46, 0xbf, 0x60, 0x75, 0x4e, 0x6a, 0x04, 0x24, 0x26, 0x08, 0x95, 0x75, 0xc7, 0x5a, 0x00, 0x3f, 0x08, 0x9d, 0x27, 0x39, 0x83, 0x9d, 0xec, 0x58, 0xb9, 0x64, 0xec, 0x38, 0x43 };
			Check ("HMACSHA256-TC5", key, data, digest);
		}

		[Test]
		// Test Case #6: HMAC-SHA-256 with 50-byte input and 32-byte key
		public void HMACSHA256_TC6 () 
		{
			byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa };
			byte[] data = new byte [50];
			for (int i = 0; i < data.Length; i++)
				data[i] = 0xdd;
			byte[] digest = { 0xcd, 0xcb, 0x12, 0x20, 0xd1, 0xec, 0xcc, 0xea, 0x91, 0xe5, 0x3a, 0xba, 0x30, 0x92, 0xf9, 0x62, 0xe5, 0x49, 0xfe, 0x6c, 0xe9, 0xed, 0x7f, 0xdc, 0x43, 0x19, 0x1f, 0xbd, 0xe4, 0x5c, 0x30, 0xb0 };
			Check ("HMACSHA256-TC6", key, data, digest);
		}

		[Test]
		// Test Case #7: HMAC-SHA-256 with 50-byte input and 37-byte key
		public void HMACSHA256_TC7 () 
		{
			byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25 };
			byte[] data = new byte [50];
			for (int i = 0; i < data.Length; i++)
				data[i] = 0xcd;
			byte[] digest = { 0xd4, 0x63, 0x3c, 0x17, 0xf6, 0xfb, 0x8d, 0x74, 0x4c, 0x66, 0xde, 0xe0, 0xf8, 0xf0, 0x74, 0x55, 0x6e, 0xc4, 0xaf, 0x55, 0xef, 0x07, 0x99, 0x85, 0x41, 0x46, 0x8e, 0xb4, 0x9b, 0xd2, 0xe9, 0x17 };
			Check ("HMACSHA256-TC7", key, data, digest);
		}

		[Test]
		// Test Case #8: HMAC-SHA-256 with 20-byte input and 32-byte key
		public void HMACSHA256_TC8 () 
		{
			byte[] key = { 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c };
			byte[] data = Encoding.Default.GetBytes ("Test With Truncation");
			byte[] digest = { 0x75, 0x46, 0xaf, 0x01, 0x84, 0x1f, 0xc0, 0x9b, 0x1a, 0xb9, 0xc3, 0x74, 0x9a, 0x5f, 0x1c, 0x17, 0xd4, 0xf5, 0x89, 0x66, 0x8a, 0x58, 0x7b, 0x27, 0x00, 0xa9, 0xc9, 0x7c, 0x11, 0x93, 0xcf, 0x42 };
			Check ("HMACSHA256-TC8", key, data, digest);
		}

		[Test]
		// Test Case #9: HMAC-SHA-256 with 54-byte input and 80-byte key
		public void HMACSHA256_TC9 () 
		{
			byte[] key = new byte [80];
			for (int i = 0; i < key.Length; i++)
				key[i] = 0xaa;
			byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key - Hash Key First");
			byte[] digest = { 0x69, 0x53, 0x02, 0x5e, 0xd9, 0x6f, 0x0c, 0x09, 0xf8, 0x0a, 0x96, 0xf7, 0x8e, 0x65, 0x38, 0xdb, 0xe2, 0xe7, 0xb8, 0x20, 0xe3, 0xdd, 0x97, 0x0e, 0x7d, 0xdd, 0x39, 0x09, 0x1b, 0x32, 0x35, 0x2f };
			Check ("HMACSHA256-TC9", key, data, digest);
		}

		[Test]
		// Test Case #10: HMAC-SHA-256 with 73-byte (multi-block) input and 80-byte key
		public void HMACSHA256_TC10 () 
		{
			byte[] key = new byte [80];
			for (int i = 0; i < key.Length; i++)
				key[i] = 0xaa;
			byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key and Larger Than One Block-Size Data");
			byte[] digest = { 0x63, 0x55, 0xac, 0x22, 0xe8, 0x90, 0xd0, 0xa3, 0xc8, 0x48, 0x1a, 0x5c, 0xa4, 0x82, 0x5b, 0xc8, 0x84, 0xd3, 0xe7, 0xa1, 0xff, 0x98, 0xa2, 0xfc, 0x2a, 0xc7, 0xd8, 0xe0, 0x64, 0xc3, 0xb2, 0xe6 };
			Check ("HMACSHA256-TC10", key, data, digest);
		}
	}
}

#endif

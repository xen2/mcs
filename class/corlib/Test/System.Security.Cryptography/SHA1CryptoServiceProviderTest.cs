//
// SHA1CryptoServiceProviderTest.cs - NUnit Test Cases for SHA1CryptoServiceProvider
//
// Author:
//		Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

// References:
// a.	FIPS PUB 180-1: Secure Hash Standard
//	http://csrc.nist.gov/publications/fips/fips180-1/fip180-1.txt

// we inherit from SHA1Test because all SHA1 implementation must return the 
// same results (hence should run a common set of unit tests).
public class SHA1CryptoServiceProviderTest : SHA1Test {
	public SHA1CryptoServiceProviderTest () : base ("System.Security.Cryptography.SHA1CryptoServiceProvider testsuite") {}
	public SHA1CryptoServiceProviderTest (string name) : base (name) {}

	protected override void SetUp () 
	{
		hash = new SHA1CryptoServiceProvider ();
	}

	protected override void TearDown () {}

	public static new ITest Suite 
	{
		get { 
			return new TestSuite (typeof (SHA1CryptoServiceProviderTest)); 
		}
	}

	public override void TestCreate () 
	{
		// no need to repeat this test
	}

	// none of those values changes for a particuliar implementation of SHA1
	public override void TestStaticInfo ()
	{
		// test all values static for SHA1
		base.TestStaticInfo();
		string className = hash.ToString ();
		AssertEquals (className + ".CanReuseTransform", true, hash.CanReuseTransform);
		AssertEquals (className + ".CanTransformMultipleBlocks", true, hash.CanTransformMultipleBlocks);
		AssertEquals (className + ".ToString()", "System.Security.Cryptography.SHA1CryptoServiceProvider", className);
	}

	public void TestSHA1CSPforFIPSCompliance () 
	{
		SHA1 sha = (SHA1)hash;
		// First test, we hash the string "abc"
		FIPS186_Test1 (sha);
                // Second test, we hash the string "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"
		FIPS186_Test2 (sha);
		// Third test, we hash 1,000,000 times the character "a"
		FIPS186_Test3 (sha);
	}

	// LAMESPEC / MSBUG: Under Windows an Initialize is required after 
	// TransformFinalBlock/Hash or SHA1CryptoServiceProvider will still return 
	// the previous Hash. SHA1Managed behavior's is different as it will return
	// a bad Hash if Initialize isn't called.
	// FIXME: Do we want to duplicate this bad behaviour ?
/*	public void TestInitialize () 
	{
		byte[] expectedDEF = { 0x58, 0x9c, 0x22, 0x33, 0x5a, 0x38, 0x1f, 0x12, 0x2d, 0x12, 0x92, 0x25, 0xf5, 0xc0, 0xba, 0x30, 0x56, 0xed, 0x58, 0x11 };
		string className = hash.ToString ();
		// hash abc
		byte[] inputABC = Encoding.Default.GetBytes ("abc");
		hash.TransformFinalBlock (inputABC, 0, inputABC.Length);
		byte[] resultABC = hash.Hash;
		// hash def
		byte[] inputDEF = Encoding.Default.GetBytes ("def");
		byte[] resultDEF = hash.ComputeHash (inputDEF); 
		// result(abc) == result(def) -> forgot to initialize
		AssertEquals (className + ".Initialize ABC=DEF", resultABC, resultDEF);
		hash.Initialize ();
		resultDEF = hash.ComputeHash (inputDEF);
		AssertEquals (className + ".Initialize DEF ok", expectedDEF, resultDEF);
	}*/
}

}

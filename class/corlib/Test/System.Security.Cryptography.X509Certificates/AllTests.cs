//
// TestSuite.System.Security.Cryptography.X509Certificates.AllTests.cs
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.X509Certificates {
	/// <summary>
	///   Combines all available X.509 unit tests into one test suite.
	/// </summary>
	public class AllTests : TestCase {
                public AllTests(string name) : base(name) {}
                
        	// because most crypto stuff works with byte[] buffers
        	static public void AssertEquals (string msg, byte[] array1, byte[] array2)
        	{
        		if ((array1 == null) && (array2 == null))
        			return;
        		if (array1 == null)
        			Fail (msg + " -> First array is NULL");
        		if (array2 == null)
        			Fail (msg + " -> Second array is NULL");
        
        		bool a = (array1.Length == array2.Length);
        		if (a) {
        			for (int i = 0; i < array1.Length; i++) {
        				if (array1 [i] != array2 [i]) {
        					a = false;
        					break;
        				}
        			}
        		}
        		msg += " -> Expected " + BitConverter.ToString (array1, 0);
        		msg += " is different than " + BitConverter.ToString (array2, 0);
        		Assert (msg, a);
        	}

                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite ();
                                suite.AddTest (X509CertificateTest.Suite);
                                return suite;
                        }
                }
        }
}

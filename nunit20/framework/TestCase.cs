#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright � 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright � 2000-2003 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright � 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright � 2000-2003 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Core
{
	using System;
	using System.Collections;

	/// <summary>
	/// Summary description for TestCase.
	/// </summary>
	public abstract class TestCase : Test
	{
		public TestCase(string path, string name) : base(path, name)
		{}

		public override int CountTestCases 
		{
			get { return 1; }
		}

		private TestSuite suite;

		public TestSuite Suite 
		{
			get { return suite; }
			set { suite = value; }
		}

		public override TestResult Run(EventListener listener)
		{
			TestCaseResult testResult = new TestCaseResult(this);

			listener.TestStarted(this);
			
			long startTime = DateTime.Now.Ticks;

			Run(testResult);

			long stopTime = DateTime.Now.Ticks;

			double time = ((double)(stopTime - startTime)) / (double)TimeSpan.TicksPerSecond;

			testResult.Time = time;

			listener.TestFinished(testResult);
	
			return testResult;
		}

		public override bool IsSuite
		{
			get { return false; }
		}

		public override bool IsFixture
		{
			get { return false; }
		}

		public override bool IsTestCase
		{
			get { return true; }
		}

		public override ArrayList Tests
		{
			get { return null; }
		}

		public abstract void Run(TestCaseResult result);

	}
}

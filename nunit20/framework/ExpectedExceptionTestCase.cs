#region Copyright (c) 2002, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Philip A. Craig
/************************************************************************************
'
' Copyright � 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov
' Copyright � 2000-2002 Philip A. Craig
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
' Portions Copyright � 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' or Copyright � 2000-2002 Philip A. Craig
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
	using System.Diagnostics;
	using System.Reflection;

	/// <summary>
	/// Summary description for ExpectedExceptionTestCase.
	/// </summary>
	public class ExpectedExceptionTestCase : TemplateTestCase
	{
		private Type expectedException;

		public ExpectedExceptionTestCase(object fixture, MethodInfo info, Type expectedException)
			: base(fixture, info)
		{
			this.expectedException = expectedException;
		}

		protected override internal void ProcessException(Exception exception, TestCaseResult testResult)
		{
			if (expectedException.Equals(exception.GetType()))
			{
				testResult.Success();
			}
			else
			{
				string message = "Expected: " + expectedException.Name + " but was " + exception.GetType().Name;
				testResult.Failure(message, exception.StackTrace);
			}

			return;
		}

		protected override internal void ProcessNoException(TestCaseResult testResult)
		{
			testResult.Failure(expectedException.Name + " was expected", null);
		}
	}
}

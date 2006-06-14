//
// UsingTaskTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

using System;
using System.Collections;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class UsingTaskTest {
		
		string		binPath;
		Engine		engine;
		Project		project;
		
		[SetUp]
		public void SetUp ()
		{
			binPath = "../../tools/xbuild/xbuild";
		}
		
		[Test]
		public void TestAssemblyFile1 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/SimpleTask.dll'
						TaskName='SimpleTask'
						Condition='true'
					/>
				</Project>
			";

			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			
			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();
			
			UsingTask ut = (UsingTask) en.Current;
			
			Assert.AreEqual ("Test/resources/SimpleTask.dll", ut.AssemblyFile, "A1");
			Assert.IsNull (ut.AssemblyName, "A2");
			Assert.AreEqual ("true", ut.Condition, "A3");
			Assert.AreEqual (false, ut.IsImported, "A4");
			Assert.AreEqual ("SimpleTask", ut.TaskName, "A5");
		}

		[Test]
		public void TestAssemblyFile2 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/SimpleTask.dll'
						TaskName='SimpleTask'
					/>
				</Project>
			";

			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			
			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();
			
			UsingTask ut = (UsingTask) en.Current;
			
			Assert.AreEqual ("Test/resources/SimpleTask.dll", ut.AssemblyFile, "A1");
			Assert.IsNull (ut.AssemblyName, "A2");
			Assert.AreEqual (null, ut.Condition, "A3");
			Assert.AreEqual (false, ut.IsImported, "A4");
			Assert.AreEqual ("SimpleTask", ut.TaskName, "A5");
		}

		[Test]
		// NOTE: quite hacky test, it works because MSBuild doesn't check type of task at loading
		public void TestAssemblyName ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyName='System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
						TaskName='System.Uri'
						Condition='true'
					/>
				</Project>
			";

			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			
			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();
			
			UsingTask ut = (UsingTask) en.Current;
			
			Assert.AreEqual ("System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", ut.AssemblyName, "A1");
			Assert.IsNull (ut.AssemblyFile, "A2");
			Assert.AreEqual ("true", ut.Condition, "A3");
			Assert.AreEqual (false, ut.IsImported, "A4");
			Assert.AreEqual ("System.Uri", ut.TaskName, "A5");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException),
			"The required attribute \"TaskName\" is missing from element <UsingTask>.  ")]
		public void TestTaskName ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/SimpleTask.dll'
					/>
				</Project>
			";

			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException),
			"A <UsingTask> element must contain either the \"AssemblyName\" attribute or the \"AssemblyFile\" attribute (but not both).  ")]
		public void TestAssemblyNameOrAssemblyFile1 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						TaskName='SimpleTask'
					/>
				</Project>
			";

			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException),
			"A <UsingTask> element must contain either the \"AssemblyName\" attribute or the \"AssemblyFile\" attribute (but not both).  ")]
		public void TestAssemblyNameOrAssemblyFile2 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						TaskName='SimpleTask'
						AssemblyFile='A'
						AssemblyName='B'
					/>
				</Project>
			";

			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}
	}
}

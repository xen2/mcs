//
// FileTest.cs: Test cases for System.IO.File
//
// Author: 
//     Duncan Mak (duncan@ximian.com)
//     Ville Palo (vi64pa@kolumbus.fi)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Threading;

namespace MonoTests.System.IO
{
	public class FileTest : TestCase
	{
		protected override void SetUp ()
		{
                	Thread.CurrentThread.CurrentCulture = new CultureInfo ("EN-us");
			DeleteFile ("resources" + Path.DirectorySeparatorChar + "creationTime");                	
                	DeleteFile ("resources" + Path.DirectorySeparatorChar + "lastAccessTime");
                	DeleteFile ("resources" + Path.DirectorySeparatorChar + "lastWriteTime");                	

		        File.Delete ("resources" + Path.DirectorySeparatorChar + "baz");
		        File.Delete ("resources" + Path.DirectorySeparatorChar + "bar");
		        File.Delete ("resources" + Path.DirectorySeparatorChar + "foo");
		}

		protected override void TearDown ()
		{
			DeleteFile ("resources" + Path.DirectorySeparatorChar + "creationTime");
                	DeleteFile ("resources" + Path.DirectorySeparatorChar + "lastAccessTime");
                	DeleteFile ("resources" + Path.DirectorySeparatorChar + "lastWriteTime");                        

		        File.Delete ("resources" + Path.DirectorySeparatorChar + "baz");
		        File.Delete ("resources" + Path.DirectorySeparatorChar + "bar");
		        File.Delete ("resources" + Path.DirectorySeparatorChar + "foo");
		}

		public void TestExists ()
		{
			int i = 0;
			try {
				Assert ("null filename should not exist", !File.Exists (null));
				i++;
				Assert ("empty filename should not exist", !File.Exists (""));
				i++;
				Assert ("whitespace filename should not exist", !File.Exists ("  \t\t  \t \n\t\n \n"));
				i++;
				Assert ("File resources" + Path.DirectorySeparatorChar + "AFile.txt should exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "AFile.txt"));
				i++;
				Assert ("File resources" + Path.DirectorySeparatorChar + "doesnotexist should not exist", !File.Exists ("resources" + Path.DirectorySeparatorChar + "doesnotexist"));
			} catch (Exception e) {
				Fail ("Unexpected exception at i = " + i + ". e=" + e);
			}
		}

		public void TestCreate ()
		{
			FileStream stream;

			/* exception test: File.Create(null) */
			try {
				stream = File.Create (null);
				Fail ("File.Create(null) should throw ArgumentNullException");
			} catch (ArgumentNullException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Create(null) unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Create("") */
			try {
				stream = File.Create ("");
				Fail ("File.Create('') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Create('') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Create(" ") */
			try {
				stream = File.Create (" ");
				Fail ("File.Create(' ') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Create(' ') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Create(directory_not_found) */
			try {
				stream = File.Create ("directory_does_not_exist" + Path.DirectorySeparatorChar + "foo");
				Fail ("File.Create(directory_does_not_exist) should throw DirectoryNotFoundException");
			} catch (DirectoryNotFoundException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Create(directory_does_not_exist) unexpected exception caught: e=" + e.ToString());
			}


			/* positive test: create resources/foo */
			try {
				stream = File.Create ("resources" + Path.DirectorySeparatorChar + "foo");
				Assert ("File should exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "foo"));
				stream.Close ();
			} catch (Exception e) {
				Fail ("File.Create(resources/foo) unexpected exception caught: e=" + e.ToString());
			}

			/* positive test: repeat test above again to test for overwriting file */
			try {
				stream = File.Create ("resources" + Path.DirectorySeparatorChar + "foo");
				Assert ("File should exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "foo"));
				stream.Close ();
			} catch (Exception e) {
				Fail ("File.Create(resources/foo) unexpected exception caught: e=" + e.ToString());
			}
		}

		public void TestCopy ()
		{
			/* exception test: File.Copy(null, b) */
			try {
				File.Copy (null, "b");
				Fail ("File.Copy(null, 'b') should throw ArgumentNullException");
			} catch (ArgumentNullException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Copy(null, 'b') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Copy(a, null) */
			try {
				File.Copy ("a", null);
				Fail ("File.Copy('a', null) should throw ArgumentNullException");
			} catch (ArgumentNullException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Copy('a', null) unexpected exception caught: e=" + e.ToString());
			}


			/* exception test: File.Copy("", b) */
			try {
				File.Copy ("", "b");
				Fail ("File.Copy('', 'b') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Copy('', 'b') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Copy(a, "") */
			try {
				File.Copy ("a", "");
				Fail ("File.Copy('a', '') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Copy('a', '') unexpected exception caught: e=" + e.ToString());
			}


			/* exception test: File.Copy(" ", b) */
			try {
				File.Copy (" ", "b");
				Fail ("File.Copy(' ', 'b') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Copy(' ', 'b') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Copy(a, " ") */
			try {
				File.Copy ("a", " ");
				Fail ("File.Copy('a', ' ') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Copy('a', ' ') unexpected exception caught: e=" + e.ToString());
			}


			/* exception test: File.Copy(doesnotexist, b) */
			try {
				File.Copy ("doesnotexist", "b");
				Fail ("File.Copy('doesnotexist', 'b') should throw FileNotFoundException");
			} catch (FileNotFoundException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Copy('doesnotexist', 'b') unexpected exception caught: e=" + e.ToString());
			}

			/* positive test: copy resources/AFile.txt to resources/bar */
			try {
				File.Delete ("resources" + Path.DirectorySeparatorChar + "bar");
				File.Copy ("resources" + Path.DirectorySeparatorChar + "AFile.txt", "resources" + Path.DirectorySeparatorChar + "bar");
				Assert ("File AFile.txt should still exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "AFile.txt"));
				Assert ("File bar should exist after File.Copy", File.Exists ("resources" + Path.DirectorySeparatorChar + "bar"));
			} catch (Exception e) {
				Fail ("#1 File.Copy('resources/AFile.txt', 'resources/bar') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Copy(resources/AFile.txt, resources/bar) (default is overwrite == false) */
			try {
				File.Copy ("resources" + Path.DirectorySeparatorChar + "AFile.txt", "resources" + Path.DirectorySeparatorChar + "bar");
				Fail ("File.Copy('resources/AFile.txt', 'resources/bar') should throw IOException");
			} catch (IOException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("#2 File.Copy('resources/AFile.txt', 'resources/bar') unexpected exception caught: e=" + e.ToString());
			}


			/* positive test: copy resources/AFile.txt to resources/bar, overwrite */
			try {
				Assert ("File bar should exist before File.Copy", File.Exists ("resources" + Path.DirectorySeparatorChar + "bar"));
				File.Copy ("resources" + Path.DirectorySeparatorChar + "AFile.txt", "resources" + Path.DirectorySeparatorChar + "bar", true);
				Assert ("File AFile.txt should still exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "AFile.txt"));
				Assert ("File bar should exist after File.Copy", File.Exists ("resources" + Path.DirectorySeparatorChar + "bar"));
			} catch (Exception e) {
				Fail ("File.Copy('resources/AFile.txt', 'resources/bar', true) unexpected exception caught: e=" + e.ToString());
			}


		}
		
		public void TestDelete ()
		{

			/* exception test: File.Delete(null) */
			try {
				File.Delete (null);
				Fail ("File.Delete(null) should throw ArgumentNullException");
			} catch (ArgumentNullException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Delete(null) unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Delete("") */
			try {
				File.Delete ("");
				Fail ("File.Delete('') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Delete('') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Delete(" ") */
			try {
				File.Delete (" ");
				Fail ("File.Delete(' ') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Delete(' ') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Delete(directory_not_found) */
			try {
				File.Delete ("directory_does_not_exist" + Path.DirectorySeparatorChar + "foo");
				Fail ("File.Delete(directory_does_not_exist) should throw DirectoryNotFoundException");
			} catch (DirectoryNotFoundException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Delete(directory_does_not_exist) unexpected exception caught: e=" + e.ToString());
			}

			if (!File.Exists ("resources" + Path.DirectorySeparatorChar + "foo")) {
				FileStream f = File.Create("resources" + Path.DirectorySeparatorChar + "foo");
				f.Close();
			}

                        Assert ("File resources" + Path.DirectorySeparatorChar + "foo should exist for TestDelete to succeed", File.Exists ("resources" + Path.DirectorySeparatorChar + "foo"));
                        try {
                                File.Delete ("resources" + Path.DirectorySeparatorChar + "foo");
                        } catch (Exception e) {
                                Fail ("Unable to delete resources" + Path.DirectorySeparatorChar + "foo: e=" + e.ToString());
                        }
			Assert ("File resources" + Path.DirectorySeparatorChar + "foo should not exist after File.Delete", !File.Exists ("resources" + Path.DirectorySeparatorChar + "foo"));
		}


		public void TestMove ()
		{
			/* exception test: File.Move(null, b) */
			try {
				File.Move (null, "b");
				Fail ("File.Move(null, 'b') should throw ArgumentNullException");
			} catch (ArgumentNullException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Move(null, 'b') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Move(a, null) */
			try {
				File.Move ("a", null);
				Fail ("File.Move('a', null) should throw ArgumentNullException");
			} catch (ArgumentNullException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Move('a', null) unexpected exception caught: e=" + e.ToString());
			}


			/* exception test: File.Move("", b) */
			try {
				File.Move ("", "b");
				Fail ("File.Move('', 'b') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Move('', 'b') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Move(a, "") */
			try {
				File.Move ("a", "");
				Fail ("File.Move('a', '') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Move('a', '') unexpected exception caught: e=" + e.ToString());
			}


			/* exception test: File.Move(" ", b) */
			try {
				File.Move (" ", "b");
				Fail ("File.Move(' ', 'b') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Move(' ', 'b') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Move(a, " ") */
			try {
				File.Move ("a", " ");
				Fail ("File.Move('a', ' ') should throw ArgumentException");
			} catch (ArgumentException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Move('a', ' ') unexpected exception caught: e=" + e.ToString());
			}


			/* exception test: File.Move(doesnotexist, b) */
			try {
				File.Move ("doesnotexist", "b");
				Fail ("File.Move('doesnotexist', 'b') should throw FileNotFoundException");
			} catch (FileNotFoundException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Move('doesnotexist', 'b') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Move(resources/foo, doesnotexist/b) */
			File.Copy("resources" + Path.DirectorySeparatorChar + "AFile.txt", "resources" + Path.DirectorySeparatorChar + "foo", true);
			try {
				File.Move ("resources" + Path.DirectorySeparatorChar + "foo", "doesnotexist" + Path.DirectorySeparatorChar + "b");
				Fail ("File.Move('resources/foo', 'b') should throw DirectoryNotFoundException");
			} catch (DirectoryNotFoundException) {
				// do nothing, this is what we expect
			} catch (FileNotFoundException) {
				// LAMESPEC
				// do nothing, this is (kind of) what we expect
			} catch (Exception e) {
				Fail ("File.Move('resources/foo', 'doesnotexist/b') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Move(doesnotexist/foo, b) */
			try {
				File.Move ("doesnotexist" + Path.DirectorySeparatorChar + "foo", "b");
				Fail ("File.Move('doesnotexist/foo', 'b') should throw DirectoryNotFoundException");
			} catch (DirectoryNotFoundException) {
				// do nothing, this is what we expect
			} catch (FileNotFoundException) {
				// LAMESPEC
				// do nothing, this is (kind of) what we expect
			} catch (Exception e) {
				Fail ("File.Move('doesnotexist/foo', 'b') unexpected exception caught: e=" + e.ToString());
			}

			/* exception test: File.Move(resources/foo, resources) */
			try {
				File.Move ("resources" + Path.DirectorySeparatorChar + "foo", "resources");
				Fail ("File.Move('resources/foo', 'resources') should throw IOException");
			} catch (IOException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("File.Move('resources/foo', 'resources') unexpected exception caught: e=" + e.ToString());
			}

			/* positive test: File.Move(a, a) shouldn't throw exception */
			try {
				File.Move ("resources" + Path.DirectorySeparatorChar + "foo", "resources" + Path.DirectorySeparatorChar + "foo");
			} catch (Exception e) {
				Fail ("File.Move('doesnotexist/foo', 'b') unexpected exception caught: e=" + e.ToString());
			}

			if (!File.Exists ("resources" + Path.DirectorySeparatorChar + "bar")) {
				FileStream f = File.Create("resources" + Path.DirectorySeparatorChar + "bar");
				f.Close();
			}
			
			Assert ("File resources" + Path.DirectorySeparatorChar + "bar should exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "bar"));
			File.Move ("resources" + Path.DirectorySeparatorChar + "bar", "resources" + Path.DirectorySeparatorChar + "baz");
			Assert ("File resources" + Path.DirectorySeparatorChar + "bar should not exist", !File.Exists ("resources" + Path.DirectorySeparatorChar + "bar"));
			Assert ("File resources" + Path.DirectorySeparatorChar + "baz should exist", File.Exists ("resources" + Path.DirectorySeparatorChar + "baz"));
		}

		public void TestOpen ()
		{
                        try {
                                FileStream stream = File.Open ("resources" + Path.DirectorySeparatorChar + "AFile.txt", FileMode.Open);
				stream.Close ();
                        } catch (Exception e) {
                                Fail ("Unable to open resources" + Path.DirectorySeparatorChar + "AFile.txt: e=" + e.ToString());
                        }

                        /* Exception tests */
			try {
				FileStream stream = File.Open ("filedoesnotexist", FileMode.Open);
				Fail ("File 'filedoesnotexist' should not exist");
			} catch (FileNotFoundException) {
				// do nothing, this is what we expect
			} catch (Exception e) {
				Fail ("Unexpect exception caught: e=" + e.ToString());
			}
		}

		[ExpectedException(typeof(IOException))]
		public void TestGetCreationTime ()
		{
			string path = "resources" + Path.DirectorySeparatorChar + "baz";
			FileStream stream = File.Create (path);
			Assert ("GetCreationTime incorrect", (DateTime.Now - File.GetCreationTime (path)).TotalSeconds < 10);

			// Test nonexistent files
			string path2 = "resources" + Path.DirectorySeparatorChar + "filedoesnotexist";

			// should throw an exception
			File.GetCreationTime (path2);
		}

                [Test]
                public void CreationTime ()
                {
                        string path = "resources" + Path.DirectorySeparatorChar + "creationTime";                	
                        if (File.Exists (path))
                        	File.Delete (path);
                        	
                       	FileStream stream = File.Create (path);
                	stream.Close ();                	
                	
                	File.SetCreationTime (path, new DateTime (2002, 4, 6, 4, 6, 4));
                	DateTime time = File.GetCreationTime (path);
                	Assertion.AssertEquals ("test#01", 2002, time.Year);
                	Assertion.AssertEquals ("test#02", 4, time.Month);
                	Assertion.AssertEquals ("test#03", 6, time.Day);
                	Assertion.AssertEquals ("test#04", 4, time.Hour);
                	Assertion.AssertEquals ("test#05", 4, time.Second);
                	
                	time = File.GetCreationTimeUtc (path);
                	Assertion.AssertEquals ("test#06", 2002, time.Year);
                	Assertion.AssertEquals ("test#07", 4, time.Month);
                	Assertion.AssertEquals ("test#08", 6, time.Day);
                	Assertion.AssertEquals ("test#09", 1, time.Hour);
                	Assertion.AssertEquals ("test#10", 4, time.Second);                	

                	File.SetCreationTimeUtc (path, new DateTime (2002, 4, 6, 4, 6, 4));
                	time = File.GetCreationTimeUtc (path);
                	Assertion.AssertEquals ("test#11", 2002, time.Year);
                	Assertion.AssertEquals ("test#12", 4, time.Month);
                	Assertion.AssertEquals ("test#13", 6, time.Day);
                	Assertion.AssertEquals ("test#14", 4, time.Hour);
                	Assertion.AssertEquals ("test#15", 4, time.Second);
                	
                	time = File.GetCreationTime (path);
                	Assertion.AssertEquals ("test#16", 2002, time.Year);
                	Assertion.AssertEquals ("test#17", 4, time.Month);
                	Assertion.AssertEquals ("test#18", 6, time.Day);
                	Assertion.AssertEquals ("test#19", 7, time.Hour);
                	Assertion.AssertEquals ("test#20", 4, time.Second);
                }

                [Test]
                public void LastAccessTime ()
                {
                        string path = "resources" + Path.DirectorySeparatorChar + "lastAccessTime";                	
                        if (File.Exists (path))
                        	File.Delete (path);
                        	
                       	FileStream stream = File.Create (path);
                	stream.Close ();                	
                	
                	File.SetLastAccessTime (path, new DateTime (2002, 4, 6, 4, 6, 4));
                	DateTime time = File.GetLastAccessTime (path);
                	Assertion.AssertEquals ("test#01", 2002, time.Year);
                	Assertion.AssertEquals ("test#02", 4, time.Month);
                	Assertion.AssertEquals ("test#03", 6, time.Day);
                	Assertion.AssertEquals ("test#04", 4, time.Hour);
                	Assertion.AssertEquals ("test#05", 4, time.Second);
                	
                	time = File.GetLastAccessTimeUtc (path);
                	Assertion.AssertEquals ("test#06", 2002, time.Year);
                	Assertion.AssertEquals ("test#07", 4, time.Month);
                	Assertion.AssertEquals ("test#08", 6, time.Day);
                	Assertion.AssertEquals ("test#09", 1, time.Hour);
                	Assertion.AssertEquals ("test#10", 4, time.Second);                	

                	File.SetLastAccessTimeUtc (path, new DateTime (2002, 4, 6, 4, 6, 4));
                	time = File.GetLastAccessTimeUtc (path);
                	Assertion.AssertEquals ("test#11", 2002, time.Year);
                	Assertion.AssertEquals ("test#12", 4, time.Month);
                	Assertion.AssertEquals ("test#13", 6, time.Day);
                	Assertion.AssertEquals ("test#14", 4, time.Hour);
                	Assertion.AssertEquals ("test#15", 4, time.Second);
                	
                	time = File.GetLastAccessTime (path);
                	Assertion.AssertEquals ("test#16", 2002, time.Year);
                	Assertion.AssertEquals ("test#17", 4, time.Month);
                	Assertion.AssertEquals ("test#18", 6, time.Day);
                	Assertion.AssertEquals ("test#19", 7, time.Hour);
                	Assertion.AssertEquals ("test#20", 4, time.Second);
                }

                [Test]
                public void LastWriteTime ()
                {
                        string path = "resources" + Path.DirectorySeparatorChar + "lastWriteTime";                	
                        if (File.Exists (path))
                        	File.Delete (path);
                        	
                       	FileStream stream = File.Create (path);
                	stream.Close ();                	
                	
                	File.SetLastWriteTime (path, new DateTime (2002, 4, 6, 4, 6, 4));
                	DateTime time = File.GetLastWriteTime (path);
                	Assertion.AssertEquals ("test#01", 2002, time.Year);
                	Assertion.AssertEquals ("test#02", 4, time.Month);
                	Assertion.AssertEquals ("test#03", 6, time.Day);
                	Assertion.AssertEquals ("test#04", 4, time.Hour);
                	Assertion.AssertEquals ("test#05", 4, time.Second);
                	
                	time = File.GetLastWriteTimeUtc (path);
                	Assertion.AssertEquals ("test#06", 2002, time.Year);
                	Assertion.AssertEquals ("test#07", 4, time.Month);
                	Assertion.AssertEquals ("test#08", 6, time.Day);
                	Assertion.AssertEquals ("test#09", 1, time.Hour);
                	Assertion.AssertEquals ("test#10", 4, time.Second);                	

                	File.SetLastWriteTimeUtc (path, new DateTime (2002, 4, 6, 4, 6, 4));
                	time = File.GetLastWriteTimeUtc (path);
                	Assertion.AssertEquals ("test#11", 2002, time.Year);
                	Assertion.AssertEquals ("test#12", 4, time.Month);
                	Assertion.AssertEquals ("test#13", 6, time.Day);
                	Assertion.AssertEquals ("test#14", 4, time.Hour);
                	Assertion.AssertEquals ("test#15", 4, time.Second);
                	
                	time = File.GetLastWriteTime (path);
                	Assertion.AssertEquals ("test#16", 2002, time.Year);
                	Assertion.AssertEquals ("test#17", 4, time.Month);
                	Assertion.AssertEquals ("test#18", 6, time.Day);
                	Assertion.AssertEquals ("test#19", 7, time.Hour);
                	Assertion.AssertEquals ("test#20", 4, time.Second);
                }

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]	
		public void GetCreationTimeException1 ()
		{
			File.GetCreationTime (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetCreationTimeException2 ()
		{
			File.GetCreationTime ("");
		}
	
		[Test]
		[ExpectedException(typeof(IOException))]
		public void GetCreationTimeException3 ()
		{
                        string path = "resources" + Path.DirectorySeparatorChar + "GetCreationTimeException3";                	
			DeleteFile (path);		
			File.GetCreationTime (path);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetCreationTimeException4 ()
		{
			File.GetCreationTime ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetCreationTimeException5 ()
		{
			File.GetCreationTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]	
		public void GetCreationTimeUtcException1 ()
		{
			File.GetCreationTimeUtc (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetCreationTimeUtcException2 ()
		{
			File.GetCreationTimeUtc ("");
		}
	
		[Test]
		[ExpectedException(typeof(IOException))]
		public void GetCreationTimeUtcException3 ()
		{
                        string path = "resources" + Path.DirectorySeparatorChar + "GetCreationTimeUtcException3";                	
			DeleteFile (path);		
			File.GetCreationTimeUtc (path);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetCreationTimeUtcException4 ()
		{
			File.GetCreationTimeUtc ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetCreationTimeUtcException5 ()
		{
			File.GetCreationTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]	
		public void GetLastAccessTimeException1 ()
		{
			File.GetLastAccessTime (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastAccessTimeException2 ()
		{
			File.GetLastAccessTime ("");
		}
	
		[Test]
		[ExpectedException(typeof(IOException))]
		public void GetLastAccessTimeException3 ()
		{
                        string path = "resources" + Path.DirectorySeparatorChar + "GetLastAccessTimeException3";                	
			DeleteFile (path);		
			File.GetLastAccessTime (path);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastAccessTimeException4 ()
		{
			File.GetLastAccessTime ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastAccessTimeException5 ()
		{
			File.GetLastAccessTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]	
		public void GetLastAccessTimeUtcException1 ()
		{
			File.GetLastAccessTimeUtc (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastAccessTimeUtcException2 ()
		{
			File.GetLastAccessTimeUtc ("");
		}
	
		[Test]
		[ExpectedException(typeof(IOException))]
		public void GetLastAccessTimeUtcException3 ()
		{
                        string path = "resources" + Path.DirectorySeparatorChar + "GetLastAccessTimeUtcException3";                	
			DeleteFile (path);			
			File.GetLastAccessTimeUtc (path);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastAccessTimeUtcException4 ()
		{
			File.GetLastAccessTimeUtc ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastAccessTimeUtcException5 ()
		{
			File.GetLastAccessTimeUtc (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]	
		public void GetLastWriteTimeException1 ()
		{
			File.GetLastWriteTime (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastWriteTimeException2 ()
		{
			File.GetLastWriteTime ("");
		}
	
		[Test]
		[ExpectedException(typeof(IOException))]
		public void GetLastWriteTimeException3 ()
		{
                        string path = "resources" + Path.DirectorySeparatorChar + "GetLastAccessTimeUtcException3";                	
			DeleteFile (path);			
			File.GetLastWriteTime (path);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastWriteTimeException4 ()
		{
			File.GetLastWriteTime ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastWriteTimeException5 ()
		{
			File.GetLastWriteTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]	
		public void GetLastWriteTimeUtcException1 ()
		{
			File.GetLastWriteTimeUtc (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastWriteTimeUtcException2 ()
		{
			File.GetLastAccessTimeUtc ("");
		}
	
		[Test]
		[ExpectedException(typeof(IOException))]
		public void GetLastWriteTimeUtcException3 ()
		{
                        string path = "resources" + Path.DirectorySeparatorChar + "GetLastWriteTimeUtcException3";
			DeleteFile (path);
			File.GetLastAccessTimeUtc (path);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastWriteTimeUtcException4 ()
		{
			File.GetLastAccessTimeUtc ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]	
		public void GetLastWriteTimeUtcException5 ()
		{
			File.GetLastAccessTimeUtc (Path.InvalidPathChars [0].ToString ());
		}		

		private void DeleteFile (string path)
		{
			if (File.Exists (path))
				File.Delete (path);
		}
	}
}

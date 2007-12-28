// ThreadTest.cs - NUnit Test Cases for the System.Threading.Thread class
//
// Authors
//	Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Eduardo Garcia Cebollero.
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Threading
{
	// These tests seem to hang the 2.0 framework. So they are disabled for now
	// Don't reenable them until you can run a few thousand times on an SMP box.
	[Category ("NotWorking")]
	public class ThreadedPrincipalTest
	{
		public static void NoPrincipal () 
		{
#if !TARGET_JVM // AppDomain.SetPrincipalPolicy not supported for TARGET_JVM
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
#endif
			IPrincipal p = Thread.CurrentPrincipal;
			Assert.IsNull (p, "#1");

			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("mono"), null);
			Assert.IsNotNull (Thread.CurrentPrincipal, "#2");

			Thread.CurrentPrincipal = null;
			Assert.IsNull (Thread.CurrentPrincipal, "#3");
			// in this case we can return to null
		}

#if !TARGET_JVM // AppDomain.SetPrincipalPolicy not supported for TARGET_JVM
		public static void UnauthenticatedPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.UnauthenticatedPrincipal);
			IPrincipal p = Thread.CurrentPrincipal;
			Assert.IsNotNull (p, "#1");
			Assert.IsTrue ((p is GenericPrincipal), "#2");
			Assert.AreEqual (String.Empty, p.Identity.Name, "#3");
			Assert.AreEqual (String.Empty, p.Identity.AuthenticationType, "#4");
			Assert.IsFalse (p.Identity.IsAuthenticated, "#5");

			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("mono"), null);
			Assert.IsNotNull (Thread.CurrentPrincipal, "#6");

			Thread.CurrentPrincipal = null;
			Assert.IsNotNull (Thread.CurrentPrincipal, "#7");
			// in this case we can't return to null
		}

		public static void WindowsPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.WindowsPrincipal);
			IPrincipal p = Thread.CurrentPrincipal;
			Assert.IsNotNull (p, "#1");
			Assert.IsTrue ((p is WindowsPrincipal), "#2");
			Assert.IsNotNull (p.Identity.Name, "#3");
			Assert.IsNotNull (p.Identity.AuthenticationType, "#4");
			Assert.IsTrue (p.Identity.IsAuthenticated, "#5");

			// note: we can switch from a WindowsPrincipal to a GenericPrincipal
			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("mono"), null);
			Assert.IsNotNull (Thread.CurrentPrincipal, "#6");

			Thread.CurrentPrincipal = null;
			Assert.IsNotNull (Thread.CurrentPrincipal, "#7");
			// in this case we can't return to null
		}
#endif // TARGET_JVM

		public static void CopyOnNewThread ()
		{
			Assert.IsNotNull (Thread.CurrentPrincipal, "#1");
			Assert.AreEqual ("good", Thread.CurrentPrincipal.Identity.Name, "#2");
		}
	}

	[TestFixture]
	public class ThreadTest
	{
		static bool is_win32;

		static ThreadTest ()
		{
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Win32NT:
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
			case PlatformID.WinCE:
				is_win32 = true;
				break;
			}
		}

		//Some Classes to test as threads
		private class C1Test
		{
			public int cnt;
			public Thread thread1;
			public bool endm1;
			public bool endm2;

			public C1Test()
			{
				thread1 = (Thread)null;
				this.cnt = 0;
				endm1 = endm2 = false;
			}
			
			public void TestMethod()
			{
				while (cnt < 10)
				{
					cnt++;
				}
				endm1 = true;
			}
			public void TestMethod2()
			{
				if (!(thread1==(Thread)null) )
				{
					thread1.Join();
				}
				endm2 = true;
			}
		}

		private class C2Test
		{
			public int cnt;
			public bool run = false;
			
			public C2Test()
			{
				this.cnt = 0;
			}

			public void TestMethod()
			{
				run = true;
				while (true)
				{
					if (cnt < 1000)
						cnt++;
					else
						cnt = 0;
				}
			}
		}
		
		private class C3Test
		{
			public C1Test sub_class;
			public Thread sub_thread;

			public C3Test()
			{
				sub_class = new C1Test();
				sub_thread = new Thread(new ThreadStart(sub_class.TestMethod));
			}

			public void TestMethod1()
			{
				sub_thread.Start();
				Thread.Sleep (100);
				sub_thread.Abort();
			}
		}
		
		private class C4Test
		{
			public C1Test class1;
			public C1Test class2;
			public Thread thread1;
			public Thread thread2;
			public bool T1ON ;
			public bool T2ON ;

			public C4Test()
			{
				T1ON = false;
				T2ON = false;
				class1 = new C1Test();
				class2 = new C1Test();
				thread1 = new Thread(new ThreadStart(class1.TestMethod));
				thread2 = new Thread(new ThreadStart(class2.TestMethod));
			}

			public void TestMethod1()
			{
				thread1.Start();
				TestUtil.WaitForAlive (thread1, "wait1");
				T1ON = true;
				thread2.Start();
				TestUtil.WaitForAlive (thread2, "wait2");
				T2ON = true;
				thread1.Abort();
				TestUtil.WaitForNotAlive (thread1, "wait3");
				T1ON = false;
				thread2.Abort();
				TestUtil.WaitForNotAlive (thread2, "wait4");
				T2ON = false;
			}
			
			public void TestMethod2()
			{
				thread1.Start();
				thread1.Join();
			}
		}

		[Test]
		public void TestCtor1()
		{
			C1Test test1 = new C1Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
		}

		[Test] // bug #325566
		public void GetHashCodeTest ()
		{
			C1Test test1 = new C1Test ();
			Thread tA = new Thread (new ThreadStart (test1.TestMethod));
			int hA1 = tA.GetHashCode ();
#if NET_2_0
			Assert.IsTrue (hA1 > 0, "#A1");
#endif
			tA.Start ();
			int hA2 = tA.GetHashCode ();
			Assert.AreEqual (hA1, hA2, "#A2");
			tA.Join ();
			int hA3 = tA.GetHashCode ();
			Assert.AreEqual (hA1, hA3, "#A3");
#if NET_2_0
			Assert.AreEqual (hA1, tA.ManagedThreadId, "#A4");
#endif

			test1 = new C1Test ();
			Thread tB = new Thread (new ThreadStart (test1.TestMethod));
			int hB1 = tB.GetHashCode ();
#if NET_2_0
			Assert.IsTrue (hB1 > 0, "#B1");
#endif
			tB.Start ();
			int hB2 = tB.GetHashCode ();
			Assert.AreEqual (hB1, hB2, "#B2");
			tB.Join ();
			int hB3 = tB.GetHashCode ();
			Assert.AreEqual (hB1, hB3, "#B3");
#if NET_2_0
			Assert.AreEqual (hB1, tB.ManagedThreadId, "#B4");
#endif
			Assert.IsFalse (hA2 == hB2, "#B5");
		}

#if NET_2_0
		[Test] // bug #82700
		public void ManagedThreadId ()
		{
			C1Test test1 = new C1Test ();
			Thread t1 = new Thread (new ThreadStart (test1.TestMethod));
			int mtA1 = t1.ManagedThreadId;
			t1.Start ();
			int mtA2 = t1.ManagedThreadId;
			t1.Join ();
			int mtA3 = t1.ManagedThreadId;
			Assert.AreEqual (mtA1, mtA2, "#A1");
			Assert.AreEqual (mtA2, mtA3, "#A2");

			test1 = new C1Test ();
			Thread t2 = new Thread (new ThreadStart (test1.TestMethod));
			int mtB1 = t2.ManagedThreadId;
			t2.Start ();
			int mtB2 = t2.ManagedThreadId;
			t2.Join ();
			int mtB3 = t2.ManagedThreadId;
			Assert.AreEqual (mtB1, mtB2, "#B1");
			Assert.AreEqual (mtB2, mtB3, "#B2");
			Assert.IsFalse (mtB1 == mtA1, "#B3");
		}
#endif

		[Test]
		[Category ("NotDotNet")] // it hangs.
		public void TestStart()
		{
			if (is_win32)
				Assert.Fail ("This test fails on Win32. The test should be fixed.");
		{
			C1Test test1 = new C1Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
			TestThread.Join();
			Assert.AreEqual (10, test1.cnt, "#1");
		}
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
			TestThread.Abort();
			try {
				TestThread.Start();
				Assert.Fail ("#2");
			} catch (ThreadStateException) {
			}
		}
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
			while (!test1.run) {
			}
			bool started = (TestThread.ThreadState == ThreadState.Running);
			Assert.AreEqual (started, test1.run, "#15 Thread Is not in the correct state: ");
			TestThread.Abort();
		}
		}

		[Test]
		public void TestApartmentState ()
		{
			if (is_win32)
				Assert.Fail ("This test fails on mono on win32. Our runtime should be fixed.");

			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			Assert.AreEqual (ApartmentState.Unknown, TestThread.ApartmentState, "#1");
			TestThread.Start();
			TestUtil.WaitForAlive (TestThread, "wait5");
#if NET_2_0
			Assert.AreEqual (ApartmentState.MTA, TestThread.ApartmentState, "#2");
#else
			Assert.AreEqual (ApartmentState.Unknown, TestThread.ApartmentState, "#3");
#endif
			TestThread.Abort();
		}

		[Test]
		public void TestPriority1()
		{
			if (is_win32)
				Assert.Fail ("This test fails on mono on Win32. Our runtime should be fixed.");

			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Priority=ThreadPriority.BelowNormal;
				ThreadPriority after = TestThread.Priority;
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait7");
				ThreadPriority before = TestThread.Priority;
				Assert.AreEqual (before, after, "#41 Unexpected Priority Change: ");
			} finally {
				TestThread.Abort();
			}
		}

		[Test]
		[Category ("NotDotNet")] // on MS, Thread is still in AbortRequested state when Start is invoked
		public void AbortUnstarted ()
		{
			C2Test test1 = new C2Test();
			Thread th = new Thread (new ThreadStart (test1.TestMethod));
			th.Abort ();
			th.Start ();
		}

		[Test]
		[Category ("NotDotNet")] // on MS, ThreadState is immediately Stopped after Abort
		[Category ("NotWorking")] // this is a MonoTODO -> no support for Priority
		public void TestPriority2()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				Assert.AreEqual (ThreadPriority.Normal, TestThread.Priority, "#42 Incorrect Priority in New thread: ");
				TestThread.Start();
				TestUtil.WaitForAliveOrStop (TestThread, "wait8");
				Assert.AreEqual (ThreadPriority.Normal, TestThread.Priority, "#43 Incorrect Priority in Started thread: ");
			} finally {
				TestThread.Abort();
			}
			Assert.AreEqual (ThreadPriority.Normal, TestThread.Priority, "#44 Incorrect Priority in Aborted thread: ");
		}

		[Test]
		[Category ("NotWorking")] // this is a MonoTODO -> no support for Priority
		public void TestPriority3()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Start();
				TestThread.Priority = ThreadPriority.Lowest;
				Assert.AreEqual (ThreadPriority.Lowest, TestThread.Priority, "#45A Incorrect Priority:");
				TestThread.Priority = ThreadPriority.BelowNormal;
				Assert.AreEqual (ThreadPriority.BelowNormal, TestThread.Priority, "#45B Incorrect Priority:");
				TestThread.Priority = ThreadPriority.Normal;
				Assert.AreEqual (ThreadPriority.Normal, TestThread.Priority, "#45C Incorrect Priority:");
				TestThread.Priority = ThreadPriority.AboveNormal;
				Assert.AreEqual (ThreadPriority.AboveNormal, TestThread.Priority, "#45D Incorrect Priority:");
				TestThread.Priority = ThreadPriority.Highest;
				Assert.AreEqual (ThreadPriority.Highest, TestThread.Priority, "#45E Incorrect Priority:");
			}
			finally {
				TestThread.Abort();
			}
		}

		[Test]
		public void TestIsBackground1 ()
		{
			if (is_win32)
				Assert.Fail ("This test fails on mono on Win32. Our runtime should be fixed.");

			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait9");
				bool state = TestThread.IsBackground;
				Assert.IsFalse (state, "#51 IsBackground not set at the default state: ");
			} finally {
				TestThread.Abort();
			}
		}

		[Test]
		[Category ("NotDotNet")] // on MS, ThreadState is immediately Stopped after Abort
		public void TestIsBackground2 ()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.IsBackground = true;
			try {
				TestThread.Start();
			} finally {
				TestThread.Abort();
			}
			Assert.IsTrue (TestThread.IsBackground, "#52 Is Background Changed to Start ");
		}

		[Test]
		public void TestName()
		{
			if (is_win32)
				Assert.Fail ("This test fails on mono on Win32. Our runtime should be fixed.");

			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait10");
				string name = TestThread.Name;
				Assert.IsNull (name, "#61 Name set when mustn't be set: ");
				string newname = "Testing....";
				TestThread.Name = newname;
				Assert.AreEqual (newname, TestThread.Name, "#62 Name not set when must be set: ");
			} finally {
				TestThread.Abort();
			}
		}

		[Test]
		//[Category("NotDotNet")]
		public void TestNestedThreads1()
		{
			C3Test test1 = new C3Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod1));
			try {
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait11");
			} finally {
				TestThread.Abort();
			}
		}

		[Test]
		public void TestNestedThreads2()
		{
			C4Test test1 = new C4Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod1));
			try {
				TestThread.Start();
			} finally {
				TestThread.Abort();
			}
		}

		[Test]
		public void TestJoin1()
		{
			C1Test test1 = new C1Test();
			C1Test test2 = new C1Test();
			Thread thread1 = new Thread(new ThreadStart(test1.TestMethod));
			Thread thread2 = new Thread(new ThreadStart(test1.TestMethod2));
			try {
				thread1.Start();
				thread2.Start();
				thread2.Join();
			} finally {
				thread1.Abort();
				thread2.Abort();
			}
		}

		[Test]
		public void TestThreadState()
		{
			if (is_win32)
				Assert.Fail ("This test fails on mono on Win32. Our runtime should be fixed.");

			//TODO: Test The rest of the possible transitions
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			Assert.AreEqual (ThreadState.Unstarted, TestThread.ThreadState, "#101 Wrong Thread State");
			try {
				TestThread.Start();
				//while(!TestThread.IsAlive); //In the MS Documentation this is not necessary
											  //but in the MS SDK it is
				Assert.IsTrue (TestThread.ThreadState == ThreadState.Running || (TestThread.ThreadState & ThreadState.Unstarted) != 0,
					"#102 Wrong Thread State: " + TestThread.ThreadState.ToString ());
			} finally {
				TestThread.Abort();
			}
			
			TestUtil.WaitForNotAlive (TestThread, "wait12");
			// Docs say state will be Stopped, but Aborted happens sometimes (?)
			Assert.IsTrue ((ThreadState.Stopped & TestThread.ThreadState) != 0 || (ThreadState.Aborted & TestThread.ThreadState) != 0,
				"#103 Wrong Thread State: " + TestThread.ThreadState.ToString ());
		}

		[Test]
		[Ignore ("see comment below.")]
		public void CurrentPrincipal_PrincipalPolicy_NoPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.NoPrincipal));
			try {
				t.Start ();
				t.Join ();
			} catch {
				t.Abort ();
			}
		}

#if !TARGET_JVM // AppDomain.SetPrincipalPolicy not supported for TARGET_JVM
		[Test]
		[Ignore ("see comment below.")]
		public void CurrentPrincipal_PrincipalPolicy_UnauthenticatedPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.UnauthenticatedPrincipal));
			try {
				t.Start ();
				t.Join ();
			} catch {
				t.Abort ();
			}
		}

		[Test]
		public void CurrentPrincipal_PrincipalPolicy_WindowsPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.WindowsPrincipal));
			try {
				t.Start ();
				t.Join ();
			} catch {
				t.Abort ();
			}
		}
#endif // TARGET_JVM
		
		[Test]
		public void IPrincipal_CopyOnNewThread () 
		{
			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("bad"), null);
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.CopyOnNewThread));
			try {
				Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("good"), null);
				t.Start ();
				t.Join ();
			} catch {
				t.Abort ();
			}
		}

		int counter = 0;

		[Test]
		public void TestSuspend ()
		{
			Thread t = new Thread (new ThreadStart (DoCount));
			t.IsBackground = true;
			t.Start ();
			
			CheckIsRunning ("t1", t);
			
			t.Suspend ();
			WaitSuspended ("t2", t);
			
			CheckIsNotRunning ("t3", t);
			
			t.Resume ();
			WaitResumed ("t4", t);
			
			CheckIsRunning ("t5", t);
			
			t.Abort ();
			TestUtil.WaitForNotAlive (t, "wait13");
			CheckIsNotRunning ("t6", t);
		}

		[Test]
		[Category("NotDotNet")] // On MS, ThreadStateException is thrown on Abort: "Thread is suspended; attempting to abort"
		public void TestSuspendAbort ()
		{
			if (is_win32)
				Assert.Fail ("This test fails on Win32. The test should be fixed.");

			Thread t = new Thread (new ThreadStart (DoCount));
			t.IsBackground = true;
			t.Start ();
			
			CheckIsRunning ("t1", t);
			
			t.Suspend ();
			WaitSuspended ("t2", t);
			
			CheckIsNotRunning ("t3", t);
			
			t.Abort ();
			
			int n=0;
			while (t.IsAlive && n < 200) {
				Thread.Sleep (10);
				n++;
			}

			Assert.IsTrue (n < 200, "Timeout while waiting for abort");
			
			CheckIsNotRunning ("t6", t);
		}
		
		void CheckIsRunning (string s, Thread t)
		{
			int c = counter;
			Thread.Sleep (100);
			Assert.IsTrue (counter > c, s);
		}
		
		void CheckIsNotRunning (string s, Thread t)
		{
			int c = counter;
			Thread.Sleep (100);
			Assert.AreEqual (counter, c, s);
		}
		
		void WaitSuspended (string s, Thread t)
		{
			int n=0;
			ThreadState state = t.ThreadState;
			while ((state & ThreadState.Suspended) == 0) {
				Assert.IsTrue ((state & ThreadState.SuspendRequested) != 0, s + ": expected SuspendRequested state");
				Thread.Sleep (10);
				n++;
				Assert.IsTrue (n < 100, s + ": failed to suspend");
				state = t.ThreadState;
			}
			Assert.IsTrue ((state & ThreadState.SuspendRequested) == 0, s + ": SuspendRequested state not expected");
		}
		
		void WaitResumed (string s, Thread t)
		{
			int n=0;
			while ((t.ThreadState & ThreadState.Suspended) != 0) {
				Thread.Sleep (10);
				n++;
				Assert.IsTrue (n < 100, s + ": failed to resume");
			}
		}
		
		public void DoCount ()
		{
			while (true) {
				counter++;
				Thread.Sleep (1);
			}
		}
	}

	[TestFixture]
	public class ThreadStateTest {
		void Start ()
		{
		}

		[Test] // bug #81720
		public void IsBackGround ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			Assert.AreEqual (ThreadState.Unstarted, t1.ThreadState, "#A1");
			Assert.IsFalse (t1.IsBackground, "#A2");
			t1.Start ();
			t1.Join ();
			Assert.AreEqual (ThreadState.Stopped, t1.ThreadState, "#A3");

			try {
				bool isBackGround = t1.IsBackground;
				Assert.Fail ("#A4: " + isBackGround.ToString ());
			} catch (ThreadStateException ex) {
				Assert.AreEqual (typeof (ThreadStateException), ex.GetType (), "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNotNull (ex.Message, "#A7");
			}

			Thread t2 = new Thread (new ThreadStart (Start));
			Assert.AreEqual (ThreadState.Unstarted, t2.ThreadState, "#B1");
			t2.IsBackground = true;
			Assert.AreEqual (ThreadState.Unstarted | ThreadState.Background, t2.ThreadState, "#B2");
			Assert.IsTrue (t2.IsBackground, "#B3");
			t2.Start ();
			t2.Join ();
			Assert.AreEqual (ThreadState.Stopped, t2.ThreadState, "#B4");

			try {
				bool isBackGround = t2.IsBackground;
				Assert.Fail ("#B5: " + isBackGround.ToString ());
			} catch (ThreadStateException ex) {
				Assert.AreEqual (typeof (ThreadStateException), ex.GetType (), "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
				Assert.IsNotNull (ex.Message, "#B8");
			}
		}
	}

	[TestFixture]
	public class ThreadApartmentTest
	{
		void Start ()
		{
		}

		[Test] // bug #81658
		public void ApartmentState_StoppedThread ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			t1.Start ();
			t1.Join ();
			try {
				ApartmentState state = t1.ApartmentState;
				Assert.Fail ("#A1: " + state.ToString ());
			} catch (ThreadStateException ex) {
				Assert.AreEqual (typeof (ThreadStateException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Thread t2 = new Thread (new ThreadStart (Start));
			t2.IsBackground = true;
			t2.Start ();
			t2.Join ();
			try {
				ApartmentState state = t2.ApartmentState;
				Assert.Fail ("#B1: " + state.ToString ());
			} catch (ThreadStateException ex) {
				Assert.AreEqual (typeof (ThreadStateException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void ApartmentState_BackGround ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			t1.IsBackground = true;
			Assert.AreEqual (ApartmentState.Unknown, t1.ApartmentState, "#1");
			t1.ApartmentState = ApartmentState.STA;
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "#2");
		}

		[Test]
		public void TestApartmentState ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			Thread t2 = new Thread (new ThreadStart (Start));
			Thread t3 = new Thread (new ThreadStart (Start));

			Assert.AreEqual (ApartmentState.Unknown, t1.ApartmentState, "Thread1 Default");
			Assert.AreEqual (ApartmentState.Unknown, t2.ApartmentState, "Thread2 Default");
			Assert.AreEqual (ApartmentState.Unknown, t3.ApartmentState, "Thread3 Default");

			t1.ApartmentState = ApartmentState.STA;
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "Thread1 Set Once");
			t1.ApartmentState = ApartmentState.MTA;
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "Thread1 Set Twice");

			t2.ApartmentState = ApartmentState.MTA;
			Assert.AreEqual (ApartmentState.MTA, t2.ApartmentState, "Thread2 Set Once");
			t2.ApartmentState = ApartmentState.STA;
			Assert.AreEqual (ApartmentState.MTA, t2.ApartmentState, "Thread2 Set Twice");

			bool exception_occured = false;
			try {
				t3.ApartmentState = ApartmentState.Unknown;
			}
			catch (Exception) {
				exception_occured = true;
			}
			Assert.AreEqual (ApartmentState.Unknown, t3.ApartmentState, "Thread3 Set Invalid");
#if NET_2_0
			Assert.IsFalse (exception_occured, "Thread3 Set Invalid Exception Occured");
#else
			Assert.IsTrue (exception_occured, "Thread3 Set Invalid Exception Occured");
#endif

			t1.Start ();
			exception_occured = false;
			try {
				t1.ApartmentState = ApartmentState.STA;
			}
			catch (Exception) {
				exception_occured = true;
			}
			Assert.IsTrue (exception_occured, "Thread1 Started Invalid Exception Occured");
		}
	}

	public class TestUtil
	{
		public static void WaitForNotAlive (Thread t, string s)
		{
			WhileAlive (t, true, s);
		}
		
		public static void WaitForAlive (Thread t, string s)
		{
			WhileAlive (t, false, s);
		}
		
		public static bool WaitForAliveOrStop (Thread t, string s)
		{
			return WhileAliveOrStop (t, false, s);
		}
		
		public static void WhileAlive (Thread t, bool alive, string s)
		{
			DateTime ti = DateTime.Now;
			while (t.IsAlive == alive) {
				if ((DateTime.Now - ti).TotalSeconds > 10) {
					if (alive) Assert.Fail ("Timeout while waiting for not alive state. " + s);
					else Assert.Fail ("Timeout while waiting for alive state. " + s);
				}
			}
		}

		public static bool WhileAliveOrStop (Thread t, bool alive, string s)
		{
			DateTime ti = DateTime.Now;
			while (t.IsAlive == alive) {
				if (t.ThreadState == ThreadState.Stopped)
					return false;

				if ((DateTime.Now - ti).TotalSeconds > 10) {
					if (alive) Assert.Fail ("Timeout while waiting for not alive state. " + s);
					else Assert.Fail ("Timeout while waiting for alive state. " + s);
				}
			}

			return true;
		}
	}
}

//
// AttributeTest.cs - NUnit Test Cases for the System.Attribute class
//
// Authors:
// 	Duco Fijma (duco@lorentz.xs4all.nl)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
//	(C) 2002 Duco Fijma
//	(c) 2004 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Reflection;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System
{
	using MonoTests.System.AttributeTestInternals;

	namespace AttributeTestInternals
	{
		[AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
		internal class MyCustomAttribute : Attribute
		{
			private string _info;

			public MyCustomAttribute (string info)
			{
				_info = info;
			}

			public string Info {
				get {
					return _info;
				}
			}
		}

		[AttributeUsage (AttributeTargets.Class)]
		internal class YourCustomAttribute : Attribute
		{
			private int _value;

			public YourCustomAttribute (int value)
			{
				_value = value;
			}

			public int Value {
				get {
					return _value;
				}
			}
		}

		[AttributeUsage (AttributeTargets.Class)]
		internal class UnusedAttribute : Attribute
		{
		}

		[MyCustomAttribute ("MyBaseClass"), YourCustomAttribute (37)]
		internal class MyClass
		{
			int Value {
				get { return 42; }
			}

			public static void ParamsMethod(params object[] args)
			{
			}
		}

		[MyCustomAttribute ("MyDerivedClass")]
		internal class MyDerivedClass : MyClass
		{
			public void Do ()
			{
			}
		}
	}

	[TestFixture]
	public class AttributeTest
	{
		[Test]
		public void TestIsDefined ()
		{
			Assert.IsTrue (Attribute.IsDefined (typeof(MyDerivedClass), typeof(MyCustomAttribute)), "#1");
			Assert.IsTrue (Attribute.IsDefined (typeof (MyDerivedClass), typeof (YourCustomAttribute)), "#2");
			Assert.IsFalse (Attribute.IsDefined (typeof (MyDerivedClass), typeof (UnusedAttribute)), "#3");
			Assert.IsTrue (Attribute.IsDefined (typeof (MyDerivedClass), typeof (MyCustomAttribute), true), "#4");
			Assert.IsTrue (Attribute.IsDefined (typeof (MyDerivedClass), typeof (YourCustomAttribute), true), "#5");
			Assert.IsFalse (Attribute.IsDefined (typeof (MyDerivedClass), typeof (UnusedAttribute), false), "#6");
			Assert.IsTrue (Attribute.IsDefined (typeof (MyDerivedClass), typeof (MyCustomAttribute), false), "#7");
			Assert.IsFalse (Attribute.IsDefined (typeof (MyDerivedClass), typeof (YourCustomAttribute), false), "#8");
			Assert.IsFalse (Attribute.IsDefined (typeof (MyDerivedClass), typeof (UnusedAttribute), false), "#9");
			Assert.IsTrue (Attribute.IsDefined (typeof (MyClass).GetMethod ("ParamsMethod").GetParameters () [0], typeof (ParamArrayAttribute), false), "#10");
		}

		[Test]
		public void IsDefined_PropertyInfo ()
		{
			PropertyInfo pi = typeof (TestBase).GetProperty ("PropBase3");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute)), "#A1");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute), false), "#A2");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute), true), "#A3");
			Assert.IsFalse (Attribute.IsDefined (pi, typeof (ComVisibleAttribute)), "#A4");
			Assert.IsFalse (Attribute.IsDefined (pi, typeof (ComVisibleAttribute), false), "#A5");
			Assert.IsFalse (Attribute.IsDefined (pi, typeof (ComVisibleAttribute), true), "#A6");

			pi = typeof (TestBase).GetProperty ("PropBase2");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute)), "#C1");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute), false), "#C2");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute), true), "#C3");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (ComVisibleAttribute)), "#C4");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (ComVisibleAttribute), false), "#C5");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (ComVisibleAttribute), true), "#C6");

			pi = typeof (TestSub).GetProperty ("PropBase2");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute)), "#D1");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute), false), "#D2");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute), true), "#D3");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (ComVisibleAttribute)), "#D4");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (ComVisibleAttribute), false), "#D5");
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (ComVisibleAttribute), true), "#D6");
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")] // bug #81797
#endif
		public void IsDefined_PropertyInfo_Override ()
		{
			PropertyInfo pi = typeof (TestSub).GetProperty ("PropBase3");
#if NET_2_0
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute)), "#B1");
#else
			Assert.IsFalse (Attribute.IsDefined (pi, typeof (PropTestAttribute)), "#B1");
#endif
			Assert.IsFalse (Attribute.IsDefined (pi, typeof (PropTestAttribute), false), "#B2");
#if NET_2_0
			Assert.IsTrue (Attribute.IsDefined (pi, typeof (PropTestAttribute), true), "#B3");
#else
			Assert.IsFalse (Attribute.IsDefined (pi, typeof (PropTestAttribute), true), "#B3");
#endif
			Assert.IsFalse (Attribute.IsDefined (pi, typeof (ComVisibleAttribute)), "#B4");
			Assert.IsFalse (Attribute.IsDefined (pi, typeof (ComVisibleAttribute), false), "#B5");
			Assert.IsFalse (Attribute.IsDefined (pi, typeof (ComVisibleAttribute), true), "#B6");
		}

		[Test]
		public void TestGetCustomAttribute ()
		{
			int i = 1;
			Type t = typeof(MyDerivedClass);
			try {
				Assert.AreEqual  ("MyDerivedClass", ((MyCustomAttribute) (Attribute.GetCustomAttribute (typeof(MyDerivedClass), typeof(MyCustomAttribute), false))).Info, "#1");
				i++;
				Assert.IsNull (((YourCustomAttribute) (Attribute.GetCustomAttribute (typeof(MyDerivedClass), typeof(YourCustomAttribute), false))), "#2");
				i++;
				Assert.AreEqual ("MyDerivedClass", ((MyCustomAttribute) (Attribute.GetCustomAttribute (typeof(MyDerivedClass), typeof(MyCustomAttribute)))).Info, "#3");
				i++;
				Assert.IsNotNull (Attribute.GetCustomAttribute (t, typeof(YourCustomAttribute)), "#4");
				i++;
				Assert.AreEqual (37, ((YourCustomAttribute) (Attribute.GetCustomAttribute (t, typeof(YourCustomAttribute)))).Value, "#5");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception thrown at i=" + i + " with t=" + t + ". e=" + e);
			}
		}

		[Test]
		public void GetCustomAttribute_PropertyInfo ()
		{
			PropertyInfo pi = typeof (TestBase).GetProperty ("PropBase3");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute)), "#A1");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute), false), "#A2");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute), true), "#A3");
			Assert.IsNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute)), "#A4");
			Assert.IsNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute), false), "#A5");
			Assert.IsNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute), true), "#A6");

			pi = typeof (TestBase).GetProperty ("PropBase2");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute)), "#C1");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute), false), "#C2");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute), true), "#C3");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute)), "#C4");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute), false), "#C5");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute), true), "#C6");

			pi = typeof (TestSub).GetProperty ("PropBase2");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute)), "#D1");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute), false), "#D2");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute), true), "#D3");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute)), "#D4");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute), false), "#D5");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute), true), "#D6");
		}

		[Test]
		[Category ("NotWorking")] // bug #81797
		public void GetCustomAttribute_PropertyInfo_Override ()
		{
			PropertyInfo pi = typeof (TestSub).GetProperty ("PropBase3");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute)), "#B1");
			Assert.IsNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute), false), "#B2");
			Assert.IsNotNull (Attribute.GetCustomAttribute (pi,
				typeof (PropTestAttribute), true), "#B3");
			Assert.IsNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute)), "#B4");
			Assert.IsNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute), false), "#B5");
			Assert.IsNull (Attribute.GetCustomAttribute (pi,
				typeof (ComVisibleAttribute), true), "#B6");
		}

		/* Test for bug 54518 */
		[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
		public class PropTestAttribute : Attribute
		{
			public PropTestAttribute ()
			{
			}
		}

		public class TestBase
		{
			[PropTest]
			public int PropBase1 {
				get { return 0; }
				set { }
			}

			[PropTest]
			[ComVisible (false)]
			public string PropBase2 {
				get { return ""; }
				set { }
			}

			[PropTest]
			public virtual string PropBase3 {
				get { return ""; }
				set { }
			}
		}

		public class TestSub : TestBase
		{
			[PropTest]
			public int PropSub1 {
				get { return 0; }
				set { }
			}

			[PropTest]
			public string PropSub2 {
				get { return ""; }
				set { }
			}

			public override string PropBase3 {
				get { return ""; }
				set { }
			}
		}

		[Test]
		public void GetCustomAttributes_Element_Null ()
		{
			// 
			// Assembly
			// 

			try {
				Attribute.GetCustomAttributes ((Assembly) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("element", ex.ParamName, "#A6");
			}

			try {
				Attribute.GetCustomAttributes ((Assembly) null, false);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("element", ex.ParamName, "#B6");
			}

			try {
				Attribute.GetCustomAttributes ((Assembly) null, typeof (PropTestAttribute));
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("element", ex.ParamName, "#C6");
			}

			try {
				Attribute.GetCustomAttributes ((Assembly) null, typeof (PropTestAttribute), false);
				Assert.Fail ("#D1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
				Assert.AreEqual ("element", ex.ParamName, "#D6");
			}

			// 
			// MemberInfo
			// 

			try {
				Attribute.GetCustomAttributes ((MemberInfo) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("element", ex.ParamName, "#A6");
			}

			try {
				Attribute.GetCustomAttributes ((MemberInfo) null, false);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("element", ex.ParamName, "#B6");
			}

			try {
				Attribute.GetCustomAttributes ((MemberInfo) null, typeof (PropTestAttribute));
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("element", ex.ParamName, "#C6");
			}

			try {
				Attribute.GetCustomAttributes ((MemberInfo) null, typeof (PropTestAttribute), false);
				Assert.Fail ("#D1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
				Assert.AreEqual ("element", ex.ParamName, "#D6");
			}

			// 
			// Module
			// 

			try {
				Attribute.GetCustomAttributes ((Module) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("element", ex.ParamName, "#A6");
			}

			try {
				Attribute.GetCustomAttributes ((Module) null, false);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("element", ex.ParamName, "#B6");
			}

			try {
				Attribute.GetCustomAttributes ((Module) null, typeof (PropTestAttribute));
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("element", ex.ParamName, "#C6");
			}

			try {
				Attribute.GetCustomAttributes ((Module) null, typeof (PropTestAttribute), false);
				Assert.Fail ("#D1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
				Assert.AreEqual ("element", ex.ParamName, "#D6");
			}

			// 
			// ParameterInfo
			// 

			try {
				Attribute.GetCustomAttributes ((ParameterInfo) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("element", ex.ParamName, "#A6");
			}

			try {
				Attribute.GetCustomAttributes ((ParameterInfo) null, false);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("element", ex.ParamName, "#B6");
			}

			try {
				Attribute.GetCustomAttributes ((ParameterInfo) null, typeof (PropTestAttribute));
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNotNull (ex.ParamName, "#C5");
				Assert.AreEqual ("element", ex.ParamName, "#C6");
			}

			try {
				Attribute.GetCustomAttributes ((ParameterInfo) null, typeof (PropTestAttribute), false);
				Assert.Fail ("#D1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNotNull (ex.ParamName, "#D5");
				Assert.AreEqual ("element", ex.ParamName, "#D6");
			}
		}

		[Test]
		public void GetCustomAttributes_PropertyInfo ()
		{
			object[] attrs;
			PropertyInfo pi;

			pi = typeof (TestBase).GetProperty ("PropBase3");
			attrs = Attribute.GetCustomAttributes (pi);
			Assert.AreEqual (1, attrs.Length, "#A1");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#A2");
			attrs = Attribute.GetCustomAttributes (pi, false);
			Assert.AreEqual (1, attrs.Length, "#A3");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#A4");
			attrs = Attribute.GetCustomAttributes (pi, true);
			Assert.AreEqual (1, attrs.Length, "#A5");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#A6");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute));
			Assert.AreEqual (1, attrs.Length, "#A7");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#A8");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#A9");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#A10");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#A11");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#A12");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute));
			Assert.AreEqual (0, attrs.Length, "#A13");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#A14");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#A15");

			pi = typeof (TestBase).GetProperty ("PropBase2");
			attrs = Attribute.GetCustomAttributes (pi);
			Assert.AreEqual (2, attrs.Length, "#C1");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (PropTestAttribute)), "#C2");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (ComVisibleAttribute)), "#C3");
			attrs = Attribute.GetCustomAttributes (pi, false);
			Assert.AreEqual (2, attrs.Length, "#C4");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (PropTestAttribute)), "#C5");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (ComVisibleAttribute)), "#C6");
			attrs = Attribute.GetCustomAttributes (pi, true);
			Assert.AreEqual (2, attrs.Length, "#C7");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (PropTestAttribute)), "#C8");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (ComVisibleAttribute)), "#C9");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute));
			Assert.AreEqual (1, attrs.Length, "#C10");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#C11");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#C12");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#C13");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#C14");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#C15");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute));
			Assert.AreEqual (1, attrs.Length, "#C16");
			Assert.AreEqual (typeof (ComVisibleAttribute), attrs [0].GetType (), "#C17");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#C18");
			Assert.AreEqual (typeof (ComVisibleAttribute), attrs [0].GetType (), "#C19");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#C20");
			Assert.AreEqual (typeof (ComVisibleAttribute), attrs [0].GetType (), "#C21");

			pi = typeof (TestSub).GetProperty ("PropBase2");
			attrs = Attribute.GetCustomAttributes (pi);
			Assert.AreEqual (2, attrs.Length, "#D1");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (PropTestAttribute)), "#D2");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (ComVisibleAttribute)), "#D3");
			attrs = Attribute.GetCustomAttributes (pi, false);
			Assert.AreEqual (2, attrs.Length, "#D4");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (PropTestAttribute)), "#D5");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (ComVisibleAttribute)), "#D6");
			attrs = Attribute.GetCustomAttributes (pi, true);
			Assert.AreEqual (2, attrs.Length, "#D7");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (PropTestAttribute)), "#D8");
			Assert.AreEqual (1, GetAttributeCount (attrs, typeof (ComVisibleAttribute)), "#D9");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute));
			Assert.AreEqual (1, attrs.Length, "#D10");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#D11");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#D12");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#D13");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#D14");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#D15");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute));
			Assert.AreEqual (1, attrs.Length, "#D16");
			Assert.AreEqual (typeof (ComVisibleAttribute), attrs [0].GetType (), "#D17");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#D18");
			Assert.AreEqual (typeof (ComVisibleAttribute), attrs [0].GetType (), "#D19");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#D20");
			Assert.AreEqual (typeof (ComVisibleAttribute), attrs [0].GetType (), "#D21");

			pi = typeof (TestSub).GetProperty ("PropSub1");
			attrs = Attribute.GetCustomAttributes (pi);
			Assert.AreEqual (1, attrs.Length, "#E1");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#E2");
			attrs = Attribute.GetCustomAttributes (pi, false);
			Assert.AreEqual (1, attrs.Length, "#E3");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#E4");
			attrs = Attribute.GetCustomAttributes (pi, true);
			Assert.AreEqual (1, attrs.Length, "#E5");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#E6");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute));
			Assert.AreEqual (1, attrs.Length, "#E7");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#E8");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#E9");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#E10");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#E11");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#E12");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute));
			Assert.AreEqual (0, attrs.Length, "#E13");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#E14");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#D15");
		}

		[Test]
		[Category ("NotWorking")] // bug #81797
		public void GetCustomAttributes_PropertyInfo_Override ()
		{
			object [] attrs;
			PropertyInfo pi;

			pi = typeof (TestSub).GetProperty ("PropBase3");
			attrs = Attribute.GetCustomAttributes (pi);
			Assert.AreEqual (1, attrs.Length, "#B1");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#B2");
			attrs = Attribute.GetCustomAttributes (pi, false);
			Assert.AreEqual (0, attrs.Length, "#B3");
			attrs = Attribute.GetCustomAttributes (pi, true);
			Assert.AreEqual (1, attrs.Length, "#B4");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#B5");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute));
			Assert.AreEqual (1, attrs.Length, "#B6");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#B7");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#B8");
			attrs = Attribute.GetCustomAttributes (pi, typeof (PropTestAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#B9");
			Assert.AreEqual (typeof (PropTestAttribute), attrs [0].GetType (), "#B10");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute));
			Assert.AreEqual (0, attrs.Length, "#B11");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#B12");
			attrs = Attribute.GetCustomAttributes (pi, typeof (ComVisibleAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#B13");
		}

		[Test]
		public void GetCustomAttributeOK ()
		{
			Attribute attribute = Attribute.GetCustomAttribute (typeof(ClassA),
				typeof(DerivedTestCustomAttributeInherit));
			Assert.IsNotNull (attribute);
		}

		[Test]
		[ExpectedException (typeof(AmbiguousMatchException))]
		public void GetCustomAttributeAmbiguous ()
		{
			Attribute.GetCustomAttribute (typeof(ClassA), typeof(TestCustomAttribute));
		}

		[Test]
		public void GetCustomAttributeNull ()
		{
			Attribute attribute = Attribute.GetCustomAttribute (typeof(ClassA),
				typeof(DerivedTestCustomAttributeMultipleInherit));
			Assert.IsNull (attribute);
		}

		[Test]
		public void GetCustomAttributesTypeNoInherit ()
		{
			object[] attributes;

			attributes = Attribute.GetCustomAttributes (typeof(ClassA), false);
			Assert.AreEqual (3, attributes.Length, "#A1");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(TestCustomAttribute)), "#A2");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(DerivedTestCustomAttributeMultiple)), "#A3");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(DerivedTestCustomAttributeInherit)), "#A4");

			attributes = Attribute.GetCustomAttributes (typeof(ClassB), false);
			Assert.AreEqual (4, attributes.Length, "#B1");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(TestCustomAttribute)), "#B2");
			Assert.AreEqual (2, GetAttributeCount (attributes,
				typeof(DerivedTestCustomAttributeMultiple)), "#B3");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(DerivedTestCustomAttributeMultipleInherit)), "#B4");
		}

		[Test]
		public void GetCustomAttributesTypeInherit ()
		{
			object[] attributes;

			attributes = Attribute.GetCustomAttributes (typeof(ClassA), true);
			Assert.AreEqual (3, attributes.Length, "#A1");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(TestCustomAttribute)), "#A2");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(DerivedTestCustomAttributeMultiple)), "#A3");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(DerivedTestCustomAttributeInherit)), "#A4");

			attributes = Attribute.GetCustomAttributes (typeof(ClassB), true);
			Assert.AreEqual (5, attributes.Length, "#B1");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(TestCustomAttribute)), "#B2");
			Assert.AreEqual (2, GetAttributeCount (attributes,
				typeof(DerivedTestCustomAttributeMultiple)), "#B3");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(DerivedTestCustomAttributeInherit)), "#B4");
			Assert.AreEqual (1, GetAttributeCount (attributes,
				typeof(DerivedTestCustomAttributeMultipleInherit)), "#B5");
		}

		[Test]
		public void TestEquality ()
		{
			MyCustomAttribute a = new MyCustomAttribute ("one");
			MyCustomAttribute b = new MyCustomAttribute ("two");
			MyCustomAttribute c = new MyCustomAttribute ("one");
			MyCustomAttribute d = a;
			
			Assert.IsTrue (a.Equals (c), "#1");
			Assert.IsTrue (c.Equals (a), "#2");
			Assert.IsFalse (c.Equals (b), "#3");
			Assert.IsFalse (b.Equals (a), "#4");
			Assert.IsFalse (b.Equals (c), "#5");
			Assert.IsTrue (a.Equals (a), "#6");
			Assert.IsTrue (a.Equals (d), "#7");
			Assert.IsFalse (a.Equals (null), "#8");
		}

		private int GetAttributeCount (object[] attributes, Type attributeType)
		{
			int counter = 0;

			foreach (Attribute attribute in attributes) {
				if (attribute.GetType () == attributeType)
					counter++;
			}

			return counter;
		}

		[AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = true)]
		private class TestCustomAttribute : Attribute
		{
		}

		[AttributeUsage (AttributeTargets.All, AllowMultiple = true, Inherited = false)]
		private class DerivedTestCustomAttributeMultiple : TestCustomAttribute
		{
		}

		[AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = true)]
		private class DerivedTestCustomAttributeInherit : TestCustomAttribute
		{
		}

		[AttributeUsage (AttributeTargets.All, AllowMultiple = true, Inherited = true)]
		private class DerivedTestCustomAttributeMultipleInherit : TestCustomAttribute
		{
		}

		[TestCustomAttribute]
		[DerivedTestCustomAttributeMultiple]
		[DerivedTestCustomAttributeInherit]
		private class ClassA
		{
		}

		[TestCustomAttribute ()]
		[DerivedTestCustomAttributeMultiple ()]
		[DerivedTestCustomAttributeMultiple ()]
		[DerivedTestCustomAttributeMultipleInherit ()]
		private class ClassB : ClassA
		{
		}

		[TestCustomAttribute ()]
		[DerivedTestCustomAttributeMultiple ()]
		[DerivedTestCustomAttributeMultipleInherit ()]
		private class ClassC : ClassB
		{
		}
	}
}

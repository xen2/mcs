//
// Tests for System.Web.UI.ClientScriptManagerTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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


#if NET_2_0

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using System.Text;
using System.Threading;

namespace MonoTests.System.Web.UI.WebControls
{

	public class MyPage : Page, ICallbackEventHandler
	{
		#region ICallbackEventHandler Members

		public string GetCallbackResult ()
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public void RaiseCallbackEvent (string eventArgument)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		#endregion
	}

	[TestFixture]
	public class ClientScriptManagerTest
	{
		[TestFixtureSetUp]
		public void Set_Up ()
		{
#if DOT_NET
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.EventValidationTest1.aspx", "EventValidationTest1.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.EventValidationTest2.aspx", "EventValidationTest2.aspx");
#else
			WebTest.CopyResource (GetType (), "EventValidationTest1.aspx", "EventValidationTest1.aspx");
			WebTest.CopyResource (GetType (), "EventValidationTest2.aspx", "EventValidationTest2.aspx");
#endif
		}

		[SetUp]
		public void SetupTestCase ()
		{
			Thread.Sleep (100);
		}

		[Test]
		[Category("NotWorking")]
		public void ClientScriptManager_GetCallbackEventReference_1 ()
		{
			MyPage p = new MyPage ();
			ClientScriptManager cs = p.ClientScript;
			StringBuilder context1 = new StringBuilder ();
			context1.Append ("function ReceiveServerData1(arg, context)");
			context1.Append ("{");
			context1.Append ("Message1.innerText =  arg;");
			context1.Append ("value1 = arg;");
			context1.Append ("}");

			// Define callback references.
			String cbReference = cs.GetCallbackEventReference (p, "arg",
			    "ReceiveServerData1", context1.ToString ());
			Assert.AreEqual ("WebForm_DoCallback('__Page',arg,ReceiveServerData1,function ReceiveServerData1(arg, context){Message1.innerText =  arg;value1 = arg;},null,false)", cbReference, "GetCallbackEventReferenceFail1");
		}

		[Test]
		[Category ("NotWorking")]
		public void ClientScriptManager_GetCallbackEventReference_2 ()
		{
			MyPage p = new MyPage ();
			ClientScriptManager cs = p.ClientScript;
			StringBuilder context1 = new StringBuilder ();
			context1.Append ("function ReceiveServerData1(arg, context)");
			context1.Append ("{");
			context1.Append ("Message1.innerText =  arg;");
			context1.Append ("value1 = arg;");
			context1.Append ("}");

			// Define callback references.
			String cbReference = cs.GetCallbackEventReference (p, "arg",
			    "ReceiveServerData1", context1.ToString (), true);
			Assert.AreEqual ("WebForm_DoCallback('__Page',arg,ReceiveServerData1,function ReceiveServerData1(arg, context){Message1.innerText =  arg;value1 = arg;},null,true)", cbReference, "GetCallbackEventReferenceFail2");
		}

		[Test]
		[Category ("NotWorking")]
		public void ClientScriptManager_GetCallbackEventReference_3 ()
		{
			MyPage p = new MyPage ();
			ClientScriptManager cs = p.ClientScript;
			StringBuilder context1 = new StringBuilder ();
			context1.Append ("function ReceiveServerData1(arg, context)");
			context1.Append ("{");
			context1.Append ("Message1.innerText =  arg;");
			context1.Append ("value1 = arg;");
			context1.Append ("}");

			// Define callback references.
			String cbReference = cs.GetCallbackEventReference (p, "arg",
			    "ReceiveServerData1", context1.ToString (), "ErrorCallback", false);
			Assert.AreEqual ("WebForm_DoCallback('__Page',arg,ReceiveServerData1,function ReceiveServerData1(arg, context){Message1.innerText =  arg;value1 = arg;},ErrorCallback,false)", cbReference, "GetCallbackEventReferenceFail3");
		}

		[Test]
		[Category ("NotWorking")]
		public void ClientScriptManager_GetPostBackEventReference_1 ()
		{
			MyPage p = new MyPage ();
			ClientScriptManager cs = p.ClientScript;
			String result = cs.GetPostBackEventReference (new PostBackOptions (p, "args"));
			Assert.AreEqual ("__doPostBack('__Page','args')", result, "GetPostBackEventReference#1");
		}

		[Test]
		public void ClientScriptManager_GetPostBackEventReference_2 ()
		{
			MyPage p = new MyPage ();
			ClientScriptManager cs = p.ClientScript;
			String result = cs.GetPostBackEventReference (p, "args");
			Assert.AreEqual ("__doPostBack('__Page','args')", result, "GetPostBackEventReference#2");
		}

		[Test]
		public void ClientScriptManager_GetPostBackClientHyperlink ()
		{
			MyPage p = new MyPage ();
			ClientScriptManager cs = p.ClientScript;
			String hyperlink = cs.GetPostBackClientHyperlink (p, "args");
			Assert.AreEqual ("javascript:__doPostBack('__Page','args')", hyperlink, "GetPostBackClientHyperlink");
		}

		[Test]
		[Category("NunitWeb")]
		[Category ("NotWorking")]
		public void ClientScriptManager_GetWebResourceUrl ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (GetWebResourceUrlLoad)).Run();
		}

		public static void GetWebResourceUrlLoad (Page p)
		{
			ClientScriptManager cs = p.ClientScript;
			String cbReference = cs.GetWebResourceUrl (typeof (MonoTests.System.Web.UI.WebControls.ClientScriptManagerTest), "ClientScript.js");
			if (cbReference.IndexOf("/NunitWeb/WebResource.axd?")<0)
				Assert.Fail ("GetWebResourceUrlFail");
		}

		[Test]
		[Category ("NotWorking")]
		public void ClientScriptManager_IsClientScriptBlockRegistered ()
		{
			Page p = new Page ();
			ClientScriptManager cs = p.ClientScript;
			String csname2 = "ButtonClickScript";
			Type cstype = p.GetType ();
			StringBuilder cstext2 = new StringBuilder ();
			cstext2.Append ("<script type=text/javascript> function DoClick() {");
			cstext2.Append ("alert('Text from client script.')} </");
			cstext2.Append ("script>");
			cs.RegisterClientScriptBlock (cstype, csname2, cstext2.ToString ());
			Assert.AreEqual (true, cs.IsClientScriptBlockRegistered (csname2), "ClientScriptBlockRegisterFail#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void ClientScriptManager_IsRegisterClientScriptInclude ()
		{
			Page p = new Page ();
			String csname = "ButtonClickScript";
			String csurl = "ClientScript.js";
			Type cstype = p.GetType ();
			ClientScriptManager cs = p.ClientScript;
			cs.RegisterClientScriptInclude (cstype, csname, csurl);
			bool registry = cs.IsClientScriptIncludeRegistered (csname);
			Assert.AreEqual (true, registry, "RegisterClientScriptIncludeFail");
		}

		[Test]
		[Category ("NotWorking")]
		public void ClientScriptManager_IsRegisterOnSubmitStatement ()
		{
			Page p = new Page ();
			String csname = "ButtonClickScript";
			Type cstype = p.GetType ();
			ClientScriptManager cs = p.ClientScript;
			cs.RegisterClientScriptInclude (cstype, csname, "document.write('Text from OnSubmit statement');");
			bool registry = cs.IsClientScriptIncludeRegistered (csname);
			Assert.AreEqual (true, registry, "RegisterClientScriptIncludeFail");
		}

		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void ClientScriptManager_RegisterOnSubmitStatement ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (RegisterOnSubmitStatement));
			string html = t.Run ();
			if (html.IndexOf ("WebForm_OnSubmit()") < 0)
				Assert.Fail ("RegisterOnSubmitStatement");
		}

		public static void RegisterOnSubmitStatement (Page p)
		{
			String csname = "OnSubmitScript";
			Type cstype = p.GetType ();
			ClientScriptManager cs = p.ClientScript;
			String cstext = "document.write('Text from OnSubmit statement');";
			cs.RegisterOnSubmitStatement (cstype, csname, cstext);
		}

		[Test]
		[Category ("NunitWeb")]
		public void ClientScriptManager_RegisterClientScriptInclude ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (RegisterClientScriptInclude));
			string html = t.Run ();
			if (html.IndexOf ("script_include.js") < 0)
				Assert.Fail ("RegisterClientScriptIncludeFail");
		}

		public static void RegisterClientScriptInclude (Page p)
		{
			String csname = "ButtonClickScript";
			String csurl = "script_include.js";
			Type cstype = p.GetType ();
			ClientScriptManager cs = p.ClientScript;
			cs.RegisterClientScriptInclude (cstype, csname, csurl);
		}

		[Test]
		[Category("NunitWeb")]
		public void ClientScriptManager_ClientScriptBlockRegister()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ClientScriptBlockRegister));
			string html = t.Run ();
			if( html.IndexOf("DoClick()") < 0)
				Assert.Fail ("ClientScriptBlockRegisterFail#2");
		}

		public static void ClientScriptBlockRegister (Page p)
		{
			ClientScriptManager cs = p.ClientScript;
			String csname2 = "ButtonClickScript";
			Type cstype = p.GetType ();
			StringBuilder cstext2 = new StringBuilder ();
			cstext2.Append ("<script type=text/javascript> function DoClick() {");
			cstext2.Append ("alert('Text from client script.')} </");
			cstext2.Append ("script>");
			cs.RegisterClientScriptBlock (cstype, csname2, cstext2.ToString ());
		}

		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void ClientScriptManager_IsRegisterStartupScript ()
		{
			Page p = new Page ();
			String csname1 = "PopupScript";
			Type cstype = p.GetType ();
			ClientScriptManager cs = p.ClientScript;
			String cstext1 = "alert('Hello World');";
			cs.RegisterStartupScript (cstype, csname1, cstext1);
			Assert.AreEqual (true,cs.IsStartupScriptRegistered(csname1) , "StartupScriptRegisteredFail"); 	
		}

		[Test]
		[Category ("NunitWeb")]
		public void ClientScriptManager_RegisterStartupScript ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (RegisterStartupScript));
			string html = t.Run ();
			if (html.IndexOf ("alert('Hello World');") < 0)
				Assert.Fail ("RegisterStartupScriptFail#1");
		}

		public static void RegisterStartupScript (Page p)
		{
			String csname1 = "PopupScript";
			Type cstype = p.GetType ();
			ClientScriptManager cs = p.ClientScript;
			String cstext1 = "alert('Hello World');";
			cs.RegisterStartupScript (cstype, csname1, cstext1,true);
		}

		[Test]
		[Category ("NunitWeb")]
		public void ClientScriptManager_RegisterArrayDeclaration ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (RegisterArrayDeclaration));
			string html = t.Run ();
			if (html.IndexOf ("var MyArray =  new Array(\"1\", \"2\", \"text\");") < 0)
				Assert.Fail ("RegisterArrayDeclarationFail#1");
		}

		public static void RegisterArrayDeclaration (Page p)
		{
			Type cstype = p.GetType ();
			ClientScriptManager cs = p.ClientScript;

			String arrName = "MyArray";
			String arrValue = "\"1\", \"2\", \"text\"";

			// Register the array with the Page class.
			cs.RegisterArrayDeclaration (arrName, arrValue);
		}

		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void ClientScriptManager_RegisterExpandAttribute ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (RegisterExpandAttribute));
			string html = t.Run ();
			if (html.IndexOf ("Message.title = \"New title from client script.\"") < 0)
				Assert.Fail ("RegisterExpandAttributeFail");
		}

		public static void RegisterExpandAttribute (Page p)
		{
			ClientScriptManager cs = p.ClientScript;
			cs.RegisterExpandoAttribute ("Message", "title", "New title from client script.", true);
		}

		[Test]
		[Category ("NunitWeb")]
		public void ClientScriptManager_RegisterHiddenField ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (RegisterHiddenField));
			string html = t.Run ();
			if (html.IndexOf ("<input type=\"hidden\" name=\"MyHiddenField\" id=\"MyHiddenField\" value=\"3\" />") < 0)
				Assert.Fail ("RegisterHiddenFieldFail");
		}

		public static void RegisterHiddenField (Page p)
		{
			ClientScriptManager cs = p.ClientScript;
			// Define the hidden field name and initial value.
			String hiddenName = "MyHiddenField";
			String hiddenValue = "3";
			// Register the hidden field with the Page class.
			cs.RegisterHiddenField (hiddenName, hiddenValue);
		}

		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void ClientScriptManager_RegisterForEventValidation_1 ()
		{
			WebTest t = new WebTest ("EventValidationTest1.aspx");
			string html = t.Run ();
			
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("__CALLBACKID");
			fr.Controls.Add ("__CALLBACKPARAM");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["__CALLBACKID"].Value = "__Page";
			t.Request = fr;
			html = t.Run ();

			if(html.IndexOf("Correct event raised callback.")<0)
				Assert.Fail ("RegisterForEventValidationFail#1");
		}

		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void ClientScriptManager_RegisterForEventValidation_2 ()
		{
			WebTest t = new WebTest ("EventValidationTest2.aspx");
			string html = t.Run ();
			
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("__CALLBACKID");
			fr.Controls.Add ("__CALLBACKPARAM");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["__CALLBACKID"].Value = "__Page";
			t.Request = fr;
			html = t.Run ();

			if (html.IndexOf ("Incorrect event raised callback.") < 0)
				Assert.Fail ("RegisterForEventValidationFail#2");
		}

		// Expected Exceptions
				
		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ClientScriptManager_RegisterForEventValidationException ()
		{
		// TODO --> No RegisterForEventValidation Method	
		//        Page p = new Page ();
		//        ClientScriptManager cs = p.ClientScript;
		//        cs.RegisterForEventValidation ("ID", "args");
		}

		
		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentException))]
		public void ClientScriptManager_ValidateEventException_1 ()
		{
		// TODO --> No ValidateEvent Method
		//        Page p = new Page ();
		//        ClientScriptManager cs = p.ClientScript;
		//        cs.ValidateEvent ("Exception");
		}

		
		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentException))]
		public void ClientScriptManager_ValidateEventException_2 ()
		{
		// TODO --> No ValidateEvent Method		
		//        Page p = new Page ();
		//        ClientScriptManager cs = p.ClientScript;
		//        cs.ValidateEvent ("Exception", "args");
		}


		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientScriptManager_IsRegisterStartupScriptException ()
		{
			Page p = new Page ();
			String csname1 = "PopupScript";
			ClientScriptManager cs = p.ClientScript;
			cs.RegisterStartupScript (null, csname1, "");
		}
		
		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientScriptManager_RegisterOnSubmitStatementException ()
		{
			Page p = new Page ();
			String csname = "OnSubmitScript";
			ClientScriptManager cs = p.ClientScript;
			String cstext = "document.write('Text from OnSubmit statement');";
			cs.RegisterOnSubmitStatement (null, csname, cstext);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientScriptManager_RegisterClientScriptIncludeException_1 ()
		{
			Page p = new Page ();
			String csname = "ButtonClickScript";
			Type cstype = p.GetType ();
			String csurl = "";
			ClientScriptManager cs = p.ClientScript;
			cs.RegisterClientScriptInclude (null, csname, csurl);
			bool registry = cs.IsClientScriptIncludeRegistered (csname);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientScriptManager_RegisterClientScriptIncludeException_2 ()
		{
			Page p = new Page ();
			String csname = "ButtonClickScript";
			String csurl = "ClientScript.js";
			ClientScriptManager cs = p.ClientScript;
			cs.RegisterClientScriptInclude (null, csname, csurl);
			bool registry = cs.IsClientScriptIncludeRegistered (csname);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientScriptManager_ClientScriptBlockRegisterException_2 ()
		{
			Page p = new Page ();
			ClientScriptManager cs = p.ClientScript;
			String csname2 = "ButtonClickScript";
			cs.RegisterClientScriptBlock (null, csname2, "");
			Assert.AreEqual (true, cs.IsClientScriptBlockRegistered (csname2), "ClientScriptBlockRegisterFail");
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientScriptManager_GetWebResourceUrlException_1 ()
		{
			Page p = new Page ();
			ClientScriptManager cs = p.ClientScript;
			String cbReference = cs.GetWebResourceUrl (null, "test");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientScriptManager_GetWebResourceUrlException_2 ()
		{
			Page p = new Page ();
			ClientScriptManager cs = p.ClientScript;
			String cbReference = cs.GetWebResourceUrl (typeof (ClientScriptManagerTest), "");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ClientScriptManager_GetCallbackEventReferenceException_1 ()
		{
			Page p = new Page ();
			ClientScriptManager cs = p.ClientScript;

			// Define callback references.
			String cbReference = cs.GetCallbackEventReference (p, "arg",
			    "ReceiveServerData1", "");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ClientScriptManager_GetCallbackEventReferenceException_2 ()
		{
			Page p = new Page ();
			ClientScriptManager cs = p.ClientScript;

			// Define callback references.
			String cbReference = cs.GetCallbackEventReference (null, "arg",
			    "ReceiveServerData1", "");
		}

		[TestFixtureTearDown]
		public void Unload()
		{
			WebTest.Unload();
		}
	}
}
#endif



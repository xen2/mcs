//
// Tests for System.Web.UI.WebControls.View.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
//

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{


	class PokerMultiView : MultiView
	{
		public PokerMultiView ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveControlState ();
		}

		public void LoadState (object o)
		{
			LoadControlState (o);
		}

		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			Render (tw);
			return sw.ToString ();
		}

		public void DoAddParsedSubObject (object e)
		{
			AddParsedSubObject (e);
		}

		public void DoOnActiveViewChanged (EventArgs e)
		{
			base.OnActiveViewChanged (e);
		}

		public void DoBubbleEvent (object source, EventArgs e)
		{
			OnBubbleEvent (source, e);
		}

		public void AddViewCtrl (View v)
		{
			this.Controls.Add (v);
		}
	}

	[TestFixture]
	public class MultiViewTest
	{

		[Test]
		public void MultiView_DefaultProperties ()
		{
			PokerMultiView pmw = new PokerMultiView ();
			Assert.AreEqual (0, pmw.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (-1, pmw.ActiveViewIndex, "ActiveViewIndex");
			Assert.AreEqual (0, pmw.Views.Count, "DefaultZeroViews");
			Assert.AreEqual (PokerMultiView.NextViewCommandName, "NextView", "DefaultNextViewCommandName");
			Assert.AreEqual (PokerMultiView.PreviousViewCommandName, "PrevView", "DefaultPrevViewCommandName");
			Assert.AreEqual (PokerMultiView.SwitchViewByIDCommandName, "SwitchViewByID", "SwitchViewByIDCommandName");
			Assert.AreEqual (PokerMultiView.SwitchViewByIndexCommandName, "SwitchViewByIndex", "SwitchViewByIndexCommandName");
		}

		[Test]
		[Category ("NotWorking")] // NotImplementedException on Mono
		public void MultiView_NotWorkingDefaultProperties ()
		{
			PokerMultiView pmw = new PokerMultiView ();
			Assert.IsTrue (pmw.EnableTheming, "EnableTheming"); 

		}

		[Test]
		public void MultiView_AddViews ()
		{
			PokerMultiView pmv = new PokerMultiView ();
			View v1 = new View ();
			pmv.Controls.Add (v1);			
			Assert.AreEqual (1, pmv.Views.Count, "ViewsCount");
			Assert.AreEqual (-1, pmv.ActiveViewIndex, "ActiveViewIndex");
		}



		[Test]
		[Category ("NotWorking")] // ActiveIndex property assigning fails to work in Mono
		public void MultiView_ActiveIndex ()
		{
			PokerMultiView pmv = new PokerMultiView ();
			View myView = new View ();
			Assert.AreEqual (-1, pmv.ActiveViewIndex, "ActiveViewIndexDefault");
			pmv.ActiveViewIndex = 0;
			Assert.AreEqual (0, pmv.ActiveViewIndex, "ActiveViewIndexChange");
			pmv.Controls.Remove (myView);
			Assert.AreEqual (0, pmv.Controls.Count, "ControlsCount");
			Assert.AreEqual (0, pmv.ActiveViewIndex, "ActiveViewIndexRemove");
		}



		[Test]
		[Category ("NotWorking")] // SetActiveView method fails to work in Mono
		public void MultiView_SetActiveView ()
		{
			PokerMultiView pmw = new PokerMultiView ();
			PokerView pv1 = new PokerView ();
			pmw.Controls.Add (pv1);
			pmw.SetActiveView (pv1);
			Assert.AreEqual (pv1, pmw.GetActiveView (), "GetActiveView");
			Assert.AreEqual (1, pmw.Controls.Count, "MultiViewControlsCount");

		}

		[Test]
		[Category ("NotWorking")] // This test fails on side effect of set ActiveViewIndex 
		public void MultiView_RemoveViewControlEvent ()
		{
			PokerMultiView pmv = new PokerMultiView ();
			View pv1 = new View ();
			View pv2 = new View ();
			View pv3 = new View ();
			pmv.Controls.Add (pv1);
			pmv.Controls.Add (pv2);
			pmv.Controls.Add (pv3);
			pmv.SetActiveView (pv1);
			Assert.AreEqual (0, pmv.ActiveViewIndex, "MultiViewActiveView");
			Assert.AreEqual (3, pmv.Controls.Count, "MultiViewControlsCount1");
			pmv.Controls.Remove (pv1);
			Assert.AreEqual (2, pmv.Controls.Count, "MultiViewControlsCount2");
			// Protected method MultiView RemovedControl has changed active view to next 
			Assert.AreSame (pv2, pmv.GetActiveView (), "EventRemovedControl");

		}

		[Test]
		public void MultiView_AddParsedSubObject ()
		{
			PokerMultiView pmv = new PokerMultiView ();
			View v1 = new View ();
			pmv.DoAddParsedSubObject (v1);
			Assert.AreEqual (1, pmv.Controls.Count, "AddParsedSubObjectSuccssed");
		}


		[Test]
		public void MultiView_CreateControlCollection ()
		{
			PokerMultiView pmv = new PokerMultiView ();
			Assert.IsNotNull (pmv.Views, "CreatingViewCollection");
		}


		[Test]
		public void MultiView_Render ()
		{
			PokerMultiView b = new PokerMultiView ();
			string html = b.Render ();
			Assert.AreEqual (b.Render (), string.Empty, "DefaultRender");
		}

		[Test]
		public void MultiView_ButtonRender ()
		{
			PokerMultiView m = new PokerMultiView ();
			PokerView v = new PokerView ();
			Button b = new Button ();
			b.ID = "test";
			v.Controls.Add (b);
			m.Controls.Add (v);
			m.SetActiveView (v);
			string html = m.Render ();
			Assert.AreEqual ("<input type=\"submit\" name=\"test\" value=\"\" id=\"test\" />", html, "ButtonRender");			
		}

		[Test]
		public void MultiView_SomeViewsButtonRender ()
		{
			PokerMultiView m = new PokerMultiView ();
			View v = new View ();
			View v1 = new View ();
			Button b = new Button ();
			Button b1 = new Button ();
			b1.ID = "test1";
			b.ID = "test";
			v.Controls.Add (b);
			v1.Controls.Add (b1);
			m.Controls.Add (v);
			m.Controls.Add (v1);
			m.SetActiveView (v);
			Assert.AreEqual (m.Render (), "<input type=\"submit\" name=\"test\" value=\"\" id=\"test\" />", "ViewWithButtonRender");
			m.SetActiveView (v1);
			Assert.AreEqual (m.Render (), "<input type=\"submit\" name=\"test1\" value=\"\" id=\"test1\" />", "ChangeViewButtonRender");

		}

		[Test]
		[Category ("NotWorking")] // This test fails on side effect of set ActiveViewIndex 
		public void MultiView_ControlState ()
		{
			PokerMultiView pmv = new PokerMultiView ();
			View v1 = new View ();
			View v2 = new View ();
			View v3 = new View ();
			pmv.AddViewCtrl (v1);
			pmv.AddViewCtrl (v2);
			pmv.AddViewCtrl (v3);
			pmv.SetActiveView (v1);
			Assert.AreEqual (v1, pmv.GetActiveView (), "BeforeLoadState");
			object state = pmv.SaveState ();
			pmv.SetActiveView (v2);
			Assert.AreEqual (1, pmv.ActiveViewIndex, "AftreSetActiveViewChanged");
			pmv.LoadState (state);
			Assert.AreEqual (0, pmv.ActiveViewIndex, "AftreLoadState");

		}


		// Events Stuff

		private bool OnActiveChanged;

		private void OnActiveViewChangedHandler (object sender, EventArgs e)
		{
			OnActiveChanged = true;
		}

		private void ResetEvents ()
		{
			OnActiveChanged = false;
		}

		[Test]
		public void MultiView_Events ()
		{
			PokerMultiView pmv = new PokerMultiView ();
			pmv.ActiveViewChanged += new EventHandler (OnActiveViewChangedHandler);
			Assert.AreEqual (false, OnActiveChanged, "OnActiveChanged");
			pmv.DoOnActiveViewChanged (new EventArgs ());
			Assert.AreEqual (true, OnActiveChanged, "AfterOnActiveChanged");

		}

		[Test]
		[Category ("NotWorking")] // This test fails on side effect of set ActiveViewIndex 
		public void MultiView_OnBubbleEvent ()
		{
			Page myPage = new Page ();
			PokerMultiView pmv = new PokerMultiView ();
			View v1 = new View ();
			View v2 = new View ();
			pmv.Controls.Add (v1);
			pmv.Controls.Add (v2);
			pmv.ActiveViewIndex = 0;
			// Command NextView
			CommandEventArgs ceaNext = new CommandEventArgs ("NextView", null);
			pmv.DoBubbleEvent (this, ceaNext);
			Assert.AreEqual (1, pmv.ActiveViewIndex, "BubbleEventNext ");
			// Command PrevView
			CommandEventArgs ceaPrev = new CommandEventArgs ("PrevView", null);
			pmv.DoBubbleEvent (this, ceaPrev);
			Assert.AreEqual (0, pmv.ActiveViewIndex, "BubbleEventPrev");
			// Command SwitchViewByIndex
			CommandEventArgs ceaSwitch = new CommandEventArgs ("SwitchViewByIndex", "1");
			pmv.DoBubbleEvent (this, ceaSwitch);
			Assert.AreEqual (1, pmv.ActiveViewIndex, "BubbleSwitchViewByIndex");
			// Command SwitchViewByID
			v1.ID = "v1";
			myPage.Controls.Add (pmv);    // FindControl inherited from control & Page must exist
			CommandEventArgs ceaSwitchViewByID = new CommandEventArgs ("SwitchViewByID", "v1");
			pmv.DoBubbleEvent (this, ceaSwitchViewByID);
			Assert.AreEqual (0, pmv.ActiveViewIndex, "SwitchViewByID");
		}

		[Test]
		[Category ("NotWorking")] // On setting a wrong index, the ArgumentOutOfRangeException exception must be thrown 
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void MultiView_IndexOutRange ()
		{
			PokerMultiView pmw = new PokerMultiView ();
			View pv1 = new View ();
			pmw.Controls.Add (pv1);
			pmw.SetActiveView (pv1);
			pmw.ActiveViewIndex = 7;

		}

		[Test]
		[Category ("NotWorking")] // On attempting add to MultiViewControlCollection not View Control, the HttpException must be thrown 
		[ExpectedException (typeof (HttpException))]
		public void MultiView_AddParsedSubObjectExeption ()
		{
			PokerMultiView pmv = new PokerMultiView ();
			Button b1 = new Button ();
			pmv.DoAddParsedSubObject (b1);
		}

		[Test]
		public void MultiView_AddParsedSubObjectExeption2 ()
		{
			PokerMultiView pmv = new PokerMultiView ();
			LiteralControl l1 = new LiteralControl ("literal");
			pmv.DoAddParsedSubObject (l1);
		}
	}
}
#endif
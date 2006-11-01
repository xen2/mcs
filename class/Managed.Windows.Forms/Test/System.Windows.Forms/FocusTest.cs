//
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//      Jackson Harper  (jackson@ximian.com)
//

using System;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class FocusTest {

		public class ControlPoker : Button {

			internal bool directed_select_called;

			public ControlPoker ()
			{
			}

			public ControlPoker (string text)
			{
				Text = text;
			}

			public void _Select (bool directed, bool forward)
			{
				Select (directed, forward);
			}

			protected override void Select (bool directed, bool forward)
			{
				directed_select_called = true;
				base.Select (directed, forward);
			}

		}

		private ControlPoker [] flat_controls;

		public class ContainerPoker : ContainerControl {

			public ContainerPoker (string s)
			{
				Text = s;
			}

			public void _Select (bool directed, bool forward)
			{
				Select (directed, forward);
			}

			public override string ToString ()
			{
				return String.Concat (GetType (), " ", Text);
			}
		}

		public class GroupBoxPoker: GroupBox {

			public GroupBoxPoker (string s)
			{
				Text = s;
			}

			public void _Select (bool directed, bool forward)
			{
				Select (directed, forward);
			}

			public override string ToString ()
			{
				return String.Concat (GetType (), " ", Text);
			}
		}

		[SetUp]
		protected virtual void SetUp ()
		{
			flat_controls = null;

			flat_controls = new ControlPoker [] {
				new ControlPoker (), new ControlPoker (), new ControlPoker ()
			};

			for (int i = 0; i < flat_controls.Length; i++)
				flat_controls [i].Text = i.ToString ();
		}

		[Test]
		public void ControlSelectNextFlatTest ()
		{
			Form form = new Form ();

			form.Controls.AddRange (flat_controls);
			form.Show ();

			Assert.IsTrue (flat_controls [0].Focused, "sanity-1");
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "sanity-2");

			form.SelectNextControl (flat_controls [0], true, false, false, false);
			Assert.IsFalse (flat_controls [0].Focused, "A1");
			Assert.IsTrue (flat_controls [1].Focused, "A2");
			Assert.IsFalse (flat_controls [2].Focused, "A3");
			Assert.AreEqual (flat_controls [1], form.ActiveControl, "A4");

			form.SelectNextControl (flat_controls [1], true, false, false, false);
			Assert.IsFalse (flat_controls [0].Focused, "A5");
			Assert.IsFalse (flat_controls [1].Focused, "A6");
			Assert.IsTrue (flat_controls [2].Focused, "A7");
			Assert.AreEqual (flat_controls [2], form.ActiveControl, "A8");

			// Can't select anymore because we aren't wrapping
			form.SelectNextControl (flat_controls [2], true, false, false, false);
			Assert.IsFalse (flat_controls [0].Focused, "A9");
			Assert.IsFalse (flat_controls [1].Focused, "A10");
			Assert.IsTrue (flat_controls [2].Focused, "A11");
			Assert.AreEqual (flat_controls [2], form.ActiveControl, "A12");

			form.SelectNextControl (flat_controls [2], true, false, false, true);
			Assert.IsTrue (flat_controls [0].Focused, "A13");
			Assert.IsFalse (flat_controls [1].Focused, "A14");
			Assert.IsFalse (flat_controls [2].Focused, "A15");
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A16");
			form.Dispose ();
		}

		[Test]
		public void SelectNextControlNullTest ()
		{
			Form form = new Form ();

			form.Show ();
			form.Controls.AddRange (flat_controls);

			form.SelectNextControl (null, true, false, false, false);
			Assert.IsTrue (flat_controls [0].Focused, "A1");
			Assert.IsFalse (flat_controls [1].Focused, "A2");
			Assert.IsFalse (flat_controls [2].Focused, "A3");
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A4");

			form.SelectNextControl (null, true, false, false, false);
			Assert.IsTrue (flat_controls [0].Focused, "A5");
			Assert.IsFalse (flat_controls [1].Focused, "A6");
			Assert.IsFalse (flat_controls [2].Focused, "A7");
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A8");
			form.Dispose ();
		}

		[Test]
		public void SelectControlTest ()
		{
			Form form = new Form ();

			form.Show ();
			form.Controls.AddRange (flat_controls);

			flat_controls [0]._Select (false, false);
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A1");

			flat_controls [0]._Select (true, false);
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A2");

			flat_controls [0]._Select (true, true);
			Assert.AreEqual (flat_controls [0], form.ActiveControl, "A3");
			form.Dispose ();
		}

		[Test]
		public void EnsureDirectedSelectUsed ()
		{
			Form form = new Form ();

			form.Show ();
			form.Controls.AddRange (flat_controls);

			form.SelectNextControl (null, true, false, false, false);
			Assert.IsTrue (flat_controls [0].directed_select_called, "A1");
			form.Dispose ();
		}

		[Test]
		public void ContainerSelectDirectedForward ()
		{
			Form form = new Form ();
			ContainerPoker cp = new ContainerPoker ("container-a");
			
			form.Show ();
			form.Controls.Add (cp);

			cp.Controls.AddRange (flat_controls);

			cp._Select (true, true);
			Assert.IsTrue (flat_controls [0].Focused, "A1");
			Assert.IsFalse (flat_controls [1].Focused, "A2");
			Assert.IsFalse (flat_controls [2].Focused, "A3");
			Assert.AreEqual (flat_controls [0], cp.ActiveControl, "A4");
			Assert.AreEqual (cp, form.ActiveControl, "A5");

			// Should select the first one again
			cp._Select (true, true);
			Assert.IsTrue (flat_controls [0].Focused, "A6");
			Assert.IsFalse (flat_controls [1].Focused, "A7");
			Assert.IsFalse (flat_controls [2].Focused, "A8");
			Assert.AreEqual (flat_controls [0], cp.ActiveControl, "A9");
			Assert.AreEqual (cp, form.ActiveControl, "A10");
			form.Dispose ();
		}

		[Test]
		public void ContainerSelectDirectedBackward ()
		{
			Form form = new Form ();
			ContainerPoker cp = new ContainerPoker ("container-a");
			
			form.Show ();
			form.Controls.Add (cp);

			cp.Controls.AddRange (flat_controls);

			cp._Select (true, false);
			Assert.IsFalse (flat_controls [0].Focused, "A1");
			Assert.IsFalse (flat_controls [1].Focused, "A2");
			Assert.IsTrue (flat_controls [2].Focused, "A3");
			Assert.AreEqual (flat_controls [2], cp.ActiveControl, "A4");
			Assert.AreEqual (cp, form.ActiveControl, "A5");

			// Should select the first one again
			cp._Select (true, false);
			Assert.IsFalse (flat_controls [0].Focused, "A6");
			Assert.IsFalse (flat_controls [1].Focused, "A7");
			Assert.IsTrue (flat_controls [2].Focused, "A8");
			Assert.AreEqual (flat_controls [2], cp.ActiveControl, "A9");
			Assert.AreEqual (cp, form.ActiveControl, "A10");
			form.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		public void ContainerSelectUndirectedForward ()
		{
			Form form = new Form ();
			ContainerPoker cp = new ContainerPoker ("container-a");
			
			form.Show ();
			form.Controls.Add (cp);

			cp.Controls.AddRange (flat_controls);

			cp._Select (false, true);
			Assert.IsFalse (flat_controls [0].Focused, "A1");
			Assert.IsFalse (flat_controls [1].Focused, "A2");
			Assert.IsFalse (flat_controls [2].Focused, "A3");
			Assert.AreEqual (null, cp.ActiveControl, "A4");
			Assert.AreEqual (cp, form.ActiveControl, "A5");
			form.Dispose ();
		}

		[Test]
		public void GetNextControlFromForm ()
		{
			Form form = new Form ();
			ContainerPoker con_a = new ContainerPoker ("container-a");
			ContainerPoker con_b = new ContainerPoker ("container-b");
			ContainerPoker con_c = new ContainerPoker ("container-c");
			ControlPoker [] ctrls_a = new ControlPoker [] {
				new ControlPoker (), new ControlPoker (), new ControlPoker ()
			};
			ControlPoker [] ctrls_b = new ControlPoker [] {
				new ControlPoker (), new ControlPoker (), new ControlPoker ()
			};
			ControlPoker [] ctrls_c = new ControlPoker [] {
				new ControlPoker (), new ControlPoker (), new ControlPoker ()
			};

			con_a.Controls.AddRange (ctrls_a);
			con_b.Controls.AddRange (ctrls_b);
			con_c.Controls.AddRange (ctrls_c);
			
			form.Controls.Add (con_a);
			form.Controls.Add (con_b);
			form.Controls.Add (con_c);

			form.Show ();

			// top level movement, 
			Assert.AreEqual (form.GetNextControl (null, true), con_a, "null-1");
			Assert.AreEqual (form.GetNextControl (null, false), con_c, "null-2");

			Assert.AreEqual (form.GetNextControl (form, true), con_a, "form-1");
			Assert.AreEqual (form.GetNextControl (form, false), con_c, "form-2");
			
			Assert.AreEqual (form.GetNextControl (con_a, true), con_b, "container-1");
			Assert.AreEqual (form.GetNextControl (con_a, false), null, "container-2");
			Assert.AreEqual (form.GetNextControl (con_b, true), con_c, "container-3");
			Assert.AreEqual (form.GetNextControl (con_b, false), con_a, "container-4");
			Assert.AreEqual (form.GetNextControl (con_c, true), null, "container-5");
			Assert.AreEqual (form.GetNextControl (con_c, false), con_b, "container-6");

			Assert.AreEqual (form.GetNextControl (ctrls_a [0], true), ctrls_a [1], "ctrls-a-1");
			Assert.AreEqual (form.GetNextControl (ctrls_a [0], false), con_a, "ctrls-a-2");
			Assert.AreEqual (form.GetNextControl (ctrls_a [1], true), ctrls_a [2], "ctrls-a-3");
			Assert.AreEqual (form.GetNextControl (ctrls_a [1], false), ctrls_a [0], "ctrls-a-4");
			Assert.AreEqual (form.GetNextControl (ctrls_a [2], true), con_b, "ctrls-a-5");
			Assert.AreEqual (form.GetNextControl (ctrls_a [2], false), ctrls_a [1], "ctrls-a-6");

			Assert.AreEqual (form.GetNextControl (ctrls_b [0], true), ctrls_b [1], "ctrls-b-1");
			Assert.AreEqual (form.GetNextControl (ctrls_b [0], false), con_b, "ctrls-b-2");
			Assert.AreEqual (form.GetNextControl (ctrls_b [1], true), ctrls_b [2], "ctrls-b-3");
			Assert.AreEqual (form.GetNextControl (ctrls_b [1], false), ctrls_b [0], "ctrls-b-4");
			Assert.AreEqual (form.GetNextControl (ctrls_b [2], true), con_c, "ctrls-b-5");
			Assert.AreEqual (form.GetNextControl (ctrls_b [2], false), ctrls_b [1], "ctrls-b-6");

			Assert.AreEqual (form.GetNextControl (ctrls_c [0], true), ctrls_c [1], "ctrls-c-1");
			Assert.AreEqual (form.GetNextControl (ctrls_c [0], false), con_c, "ctrls-c-2");
			Assert.AreEqual (form.GetNextControl (ctrls_c [1], true), ctrls_c [2], "ctrls-c-3");
			Assert.AreEqual (form.GetNextControl (ctrls_c [1], false), ctrls_c [0], "ctrls-c-4");
			Assert.AreEqual (form.GetNextControl (ctrls_c [2], true), null, "ctrls-c-5");
			Assert.AreEqual (form.GetNextControl (ctrls_c [2], false), ctrls_c [1], "ctrls-c-6");
			form.Dispose ();
		}

		[Test]
		public void GetNextControlFromContainerA ()
		{
			Form form = new Form ();
			ContainerPoker con_a = new ContainerPoker ("container-a");
			ContainerPoker con_b = new ContainerPoker ("container-b");
			ContainerPoker con_c = new ContainerPoker ("container-c");
			ControlPoker [] ctrls_a = new ControlPoker [] {
				new ControlPoker ("ctrls-a-0"), new ControlPoker ("ctrls-a-1"), new ControlPoker ("ctrls-a-2")
			};
			ControlPoker [] ctrls_b = new ControlPoker [] {
				new ControlPoker ("ctrls-b-0"), new ControlPoker ("ctrls-b-1"), new ControlPoker ("ctrls-b-2")
			};
			ControlPoker [] ctrls_c = new ControlPoker [] {
				new ControlPoker ("ctrls-c-0"), new ControlPoker ("ctrls-c-1"), new ControlPoker ("ctrls-c-2")
			};

			con_a.Controls.AddRange (ctrls_a);
			con_b.Controls.AddRange (ctrls_b);
			con_c.Controls.AddRange (ctrls_c);
			
			form.Controls.Add (con_a);
			form.Controls.Add (con_b);
			form.Controls.Add (con_c);

			form.Show ();

			// top level movement, 
			Assert.AreEqual (con_a.GetNextControl (null, true), ctrls_a [0], "null-1");
			Assert.AreEqual (con_a.GetNextControl (null, false), ctrls_a [2], "null-2");

			Assert.AreEqual (con_a.GetNextControl (form, true), ctrls_a [0], "form-1");
			Assert.AreEqual (con_a.GetNextControl (form, false), ctrls_a [2], "form-2");
			
			Assert.AreEqual (con_a.GetNextControl (con_a, true), ctrls_a [0], "container-1");
			Assert.AreEqual (con_a.GetNextControl (con_a, false), ctrls_a [2], "container-2");
			Assert.AreEqual (con_a.GetNextControl (con_b, true), ctrls_a [0], "container-3");
			Assert.AreEqual (con_a.GetNextControl (con_b, false), ctrls_a [2], "container-4");
			Assert.AreEqual (con_a.GetNextControl (con_c, true), ctrls_a [0], "container-5");
			Assert.AreEqual (con_a.GetNextControl (con_c, false), ctrls_a [2], "container-6");

			Assert.AreEqual (con_a.GetNextControl (ctrls_a [0], true), ctrls_a [1], "ctrls-a-1");
			Assert.AreEqual (con_a.GetNextControl (ctrls_a [0], false), null, "ctrls-a-2");
			Assert.AreEqual (con_a.GetNextControl (ctrls_a [1], true), ctrls_a [2], "ctrls-a-3");
			Assert.AreEqual (con_a.GetNextControl (ctrls_a [1], false), ctrls_a [0], "ctrls-a-4");
			Assert.AreEqual (con_a.GetNextControl (ctrls_a [2], true), null, "ctrls-a-5");
			Assert.AreEqual (con_a.GetNextControl (ctrls_a [2], false), ctrls_a [1], "ctrls-a-6");

			Assert.AreEqual (con_a.GetNextControl (ctrls_b [0], true), ctrls_a [0], "ctrls-b-1");
			Assert.AreEqual (con_a.GetNextControl (ctrls_b [0], false), ctrls_a [2], "ctrls-b-2");
			Assert.AreEqual (con_a.GetNextControl (ctrls_b [1], true), ctrls_a [0], "ctrls-b-3");
			Assert.AreEqual (con_a.GetNextControl (ctrls_b [1], false), ctrls_a [2], "ctrls-b-4");
			Assert.AreEqual (con_a.GetNextControl (ctrls_b [2], true), ctrls_a [0], "ctrls-b-5");
			Assert.AreEqual (con_a.GetNextControl (ctrls_b [2], false), ctrls_a [2], "ctrls-b-6");

			Assert.AreEqual (con_a.GetNextControl (ctrls_c [0], true), ctrls_a [0], "ctrls-c-1");
			Assert.AreEqual (con_a.GetNextControl (ctrls_c [0], false), ctrls_a [2], "ctrls-c-2");
			Assert.AreEqual (con_a.GetNextControl (ctrls_c [1], true), ctrls_a [0], "ctrls-c-3");
			Assert.AreEqual (con_a.GetNextControl (ctrls_c [1], false), ctrls_a [2], "ctrls-c-4");
			Assert.AreEqual (con_a.GetNextControl (ctrls_c [2], true), ctrls_a [0], "ctrls-c-5");
			Assert.AreEqual (con_a.GetNextControl (ctrls_c [2], false), ctrls_a [2], "ctrls-c-6");
			form.Dispose ();
		}

		[Test]
		public void GetNextControlFromContainerB ()
		{
			Form form = new Form ();
			ContainerPoker con_a = new ContainerPoker ("container-a");
			ContainerPoker con_b = new ContainerPoker ("container-b");
			ContainerPoker con_c = new ContainerPoker ("container-c");
			ControlPoker [] ctrls_a = new ControlPoker [] {
				new ControlPoker ("ctrls-a-0"), new ControlPoker ("ctrls-a-1"), new ControlPoker ("ctrls-a-2")
			};
			ControlPoker [] ctrls_b = new ControlPoker [] {
				new ControlPoker ("ctrls-b-0"), new ControlPoker ("ctrls-b-1"), new ControlPoker ("ctrls-b-2")
			};
			ControlPoker [] ctrls_c = new ControlPoker [] {
				new ControlPoker ("ctrls-c-0"), new ControlPoker ("ctrls-c-1"), new ControlPoker ("ctrls-c-2")
			};

			con_a.Controls.AddRange (ctrls_a);
			con_b.Controls.AddRange (ctrls_b);
			con_c.Controls.AddRange (ctrls_c);
			
			form.Controls.Add (con_a);
			form.Controls.Add (con_b);
			form.Controls.Add (con_c);

			form.Show ();

			// top level movement
			Assert.AreEqual (con_b.GetNextControl (null, true), ctrls_b [0], "null-1");
			Assert.AreEqual (con_b.GetNextControl (null, false), ctrls_b [2], "null-2");

			Assert.AreEqual (con_b.GetNextControl (form, true), ctrls_b [0], "form-1");
			Assert.AreEqual (con_b.GetNextControl (form, false), ctrls_b [2], "form-2");
			
			Assert.AreEqual (con_b.GetNextControl (con_a, true), ctrls_b [0], "container-1");
			Assert.AreEqual (con_b.GetNextControl (con_a, false), ctrls_b [2], "container-2");
			Assert.AreEqual (con_b.GetNextControl (con_b, true), ctrls_b [0], "container-3");
			Assert.AreEqual (con_b.GetNextControl (con_b, false), ctrls_b [2], "container-4");
			Assert.AreEqual (con_b.GetNextControl (con_c, true), ctrls_b [0], "container-5");
			Assert.AreEqual (con_b.GetNextControl (con_c, false), ctrls_b [2], "container-6");

			Assert.AreEqual (con_b.GetNextControl (ctrls_a [0], true), ctrls_b [0], "ctrls-a-1");
			Assert.AreEqual (con_b.GetNextControl (ctrls_a [0], false), ctrls_b [2], "ctrls-a-2");
			Assert.AreEqual (con_b.GetNextControl (ctrls_a [1], true), ctrls_b [0], "ctrls-a-3");
			Assert.AreEqual (con_b.GetNextControl (ctrls_a [1], false), ctrls_b [2], "ctrls-a-4");
			Assert.AreEqual (con_b.GetNextControl (ctrls_a [2], true), ctrls_b [0], "ctrls-a-5");
			Assert.AreEqual (con_b.GetNextControl (ctrls_a [2], false), ctrls_b [2], "ctrls-a-6");

			Assert.AreEqual (con_b.GetNextControl (ctrls_b [0], true), ctrls_b [1], "ctrls-b-1");
			Assert.AreEqual (con_b.GetNextControl (ctrls_b [0], false), null, "ctrls-b-2");
			Assert.AreEqual (con_b.GetNextControl (ctrls_b [1], true), ctrls_b [2], "ctrls-b-3");
			Assert.AreEqual (con_b.GetNextControl (ctrls_b [1], false), ctrls_b [0], "ctrls-b-4");
			Assert.AreEqual (con_b.GetNextControl (ctrls_b [2], true), null, "ctrls-b-5");
			Assert.AreEqual (con_b.GetNextControl (ctrls_b [2], false), ctrls_b [1], "ctrls-b-6");

			Assert.AreEqual (con_b.GetNextControl (ctrls_c [0], true), ctrls_b [0], "ctrls-c-1");
			Assert.AreEqual (con_b.GetNextControl (ctrls_c [0], false), ctrls_b [2], "ctrls-c-2");
			Assert.AreEqual (con_b.GetNextControl (ctrls_c [1], true), ctrls_b [0], "ctrls-c-3");
			Assert.AreEqual (con_b.GetNextControl (ctrls_c [1], false), ctrls_b [2], "ctrls-c-4");
			Assert.AreEqual (con_b.GetNextControl (ctrls_c [2], true), ctrls_b [0], "ctrls-c-5");
			Assert.AreEqual (con_b.GetNextControl (ctrls_c [2], false), ctrls_b [2], "ctrls-c-6");
			form.Dispose ();
		}

		[Test]
		public void GetNextControlFromContainerC ()
		{
			Form form = new Form ();
			ContainerPoker con_a = new ContainerPoker ("container-a");
			ContainerPoker con_b = new ContainerPoker ("container-b");
			ContainerPoker con_c = new ContainerPoker ("container-c");
			ControlPoker [] ctrls_a = new ControlPoker [] {
				new ControlPoker ("ctrls-a-0"), new ControlPoker ("ctrls-a-1"), new ControlPoker ("ctrls-a-2")
			};
			ControlPoker [] ctrls_b = new ControlPoker [] {
				new ControlPoker ("ctrls-b-0"), new ControlPoker ("ctrls-b-1"), new ControlPoker ("ctrls-b-2")
			};
			ControlPoker [] ctrls_c = new ControlPoker [] {
				new ControlPoker ("ctrls-c-0"), new ControlPoker ("ctrls-c-1"), new ControlPoker ("ctrls-c-2")
			};

			con_a.Controls.AddRange (ctrls_a);
			con_b.Controls.AddRange (ctrls_b);
			con_c.Controls.AddRange (ctrls_c);
			
			form.Controls.Add (con_a);
			form.Controls.Add (con_b);
			form.Controls.Add (con_c);

			form.Show ();

			// top level movement, 
			Assert.AreEqual (con_c.GetNextControl (null, true), ctrls_c [0], "null-1");
			Assert.AreEqual (con_c.GetNextControl (null, false), ctrls_c [2], "null-2");

			Assert.AreEqual (con_c.GetNextControl (form, true), ctrls_c [0], "form-1");
			Assert.AreEqual (con_c.GetNextControl (form, false), ctrls_c [2], "form-2");
			
			Assert.AreEqual (con_c.GetNextControl (con_a, true), ctrls_c [0], "container-1");
			Assert.AreEqual (con_c.GetNextControl (con_a, false), ctrls_c [2], "container-2");
			Assert.AreEqual (con_c.GetNextControl (con_b, true), ctrls_c [0], "container-3");
			Assert.AreEqual (con_c.GetNextControl (con_b, false), ctrls_c [2], "container-4");
			Assert.AreEqual (con_c.GetNextControl (con_c, true), ctrls_c [0], "container-5");
			Assert.AreEqual (con_c.GetNextControl (con_c, false), ctrls_c [2], "container-6");

			Assert.AreEqual (con_c.GetNextControl (ctrls_a [0], true), ctrls_c [0], "ctrls-a-1");
			Assert.AreEqual (con_c.GetNextControl (ctrls_a [0], false), ctrls_c [2], "ctrls-a-2");
			Assert.AreEqual (con_c.GetNextControl (ctrls_a [1], true), ctrls_c [0], "ctrls-a-3");
			Assert.AreEqual (con_c.GetNextControl (ctrls_a [1], false), ctrls_c [2], "ctrls-a-4");
			Assert.AreEqual (con_c.GetNextControl (ctrls_a [2], true), ctrls_c [0], "ctrls-a-5");
			Assert.AreEqual (con_c.GetNextControl (ctrls_a [2], false), ctrls_c [2], "ctrls-a-6");

			Assert.AreEqual (con_c.GetNextControl (ctrls_b [0], true), ctrls_c [0], "ctrls-b-1");
			Assert.AreEqual (con_c.GetNextControl (ctrls_b [0], false), ctrls_c [2], "ctrls-b-2");
			Assert.AreEqual (con_c.GetNextControl (ctrls_b [1], true), ctrls_c [0], "ctrls-b-3");
			Assert.AreEqual (con_c.GetNextControl (ctrls_b [1], false), ctrls_c [2], "ctrls-b-4");
			Assert.AreEqual (con_c.GetNextControl (ctrls_b [2], true), ctrls_c [0], "ctrls-b-5");
			Assert.AreEqual (con_c.GetNextControl (ctrls_b [2], false), ctrls_c [2], "ctrls-b-6");

			Assert.AreEqual (con_c.GetNextControl (ctrls_c [0], true), ctrls_c [1], "ctrls-c-1");
			Assert.AreEqual (con_c.GetNextControl (ctrls_c [0], false), null, "ctrls-c-2");
			Assert.AreEqual (con_c.GetNextControl (ctrls_c [1], true), ctrls_c [2], "ctrls-c-3");
			Assert.AreEqual (con_c.GetNextControl (ctrls_c [1], false), ctrls_c [0], "ctrls-c-4");
			Assert.AreEqual (con_c.GetNextControl (ctrls_c [2], true), null, "ctrls-c-5");
			Assert.AreEqual (con_c.GetNextControl (ctrls_c [2], false), ctrls_c [1], "ctrls-c-6");
			form.Dispose ();
		}

		[Test]
		public void GetNextControl2FromForm ()
		{
			Form form = new Form ();
			ContainerPoker con_a = new ContainerPoker ("container-a");
			ContainerPoker con_b = new ContainerPoker ("container-b");
			ContainerPoker con_c = new ContainerPoker ("container-c");
			ControlPoker [] ctrls_a = new ControlPoker [] {
				new ControlPoker ("ctrls-a-0"), new ControlPoker ("ctrls-a-1"), new ControlPoker ("ctrls-a-2")
			};
			ControlPoker ctrl_b = new ControlPoker ("ctrl-b");
			
			con_a.Controls.AddRange (ctrls_a);
			
			form.Controls.Add (con_a);
			form.Controls.Add (ctrl_b);

			form.Show ();

			// top level movement, 
			Assert.AreEqual (form.GetNextControl (null, true), con_a, "null-1");
			Assert.AreEqual (form.GetNextControl (null, false), ctrl_b, "null-2");

			Assert.AreEqual (form.GetNextControl (form, true), con_a, "form-1");
			Assert.AreEqual (form.GetNextControl (form, false), ctrl_b, "form-2");

			Assert.AreEqual (form.GetNextControl (con_a, true), ctrl_b, "con-a-1");
			Assert.AreEqual (form.GetNextControl (con_a, false), null, "con-a-2");

			Assert.AreEqual (form.GetNextControl (ctrl_b, true), null, "ctrl-b-1");
			Assert.AreEqual (form.GetNextControl (ctrl_b, false), con_a, "ctrl-b-2");

			Assert.AreEqual (form.GetNextControl (ctrls_a [0], true), ctrls_a [1], "ctrl-a-1");
			Assert.AreEqual (form.GetNextControl (ctrls_a [0], false), con_a, "ctrl-a-2");
			Assert.AreEqual (form.GetNextControl (ctrls_a [1], true), ctrls_a [2], "ctrl-a-1");
			Assert.AreEqual (form.GetNextControl (ctrls_a [1], false), ctrls_a [0], "ctrl-a-2");
			Assert.AreEqual (form.GetNextControl (ctrls_a [2], true), ctrl_b, "ctrl-a-1");
			Assert.AreEqual (form.GetNextControl (ctrls_a [2], false), ctrls_a [1], "ctrl-a-2");
			form.Dispose();
		}

		[Test]
		public void GetNextControlFlat ()
		{
			Form form = new Form ();

			form.Controls.AddRange (flat_controls);
			form.Show ();

			Assert.AreEqual (form.GetNextControl (null, true), flat_controls [0], "form-1");
			Assert.AreEqual (form.GetNextControl (null, false), flat_controls [2], "form-2");
			Assert.AreEqual (form.GetNextControl (flat_controls [0], true), flat_controls [1], "form-3");
			Assert.AreEqual (form.GetNextControl (flat_controls [0], false), null, "form-4");
			Assert.AreEqual (form.GetNextControl (flat_controls [1], true), flat_controls [2], "form-5");
			Assert.AreEqual (form.GetNextControl (flat_controls [1], false), flat_controls [0], "form-6");
			Assert.AreEqual (form.GetNextControl (flat_controls [2], true), null, "form-7");
			Assert.AreEqual (form.GetNextControl (flat_controls [2], false), flat_controls [1],"form-8");

			
			Assert.AreEqual (flat_controls [0].GetNextControl (null, true), null, "ctrls-0-1");
			Assert.AreEqual (flat_controls [0].GetNextControl (null, false), null, "ctrls-0-2");
			Assert.AreEqual (flat_controls [0].GetNextControl (flat_controls [0], true), null, "ctrls-0-3");
			Assert.AreEqual (flat_controls [0].GetNextControl (flat_controls [0], false), null, "ctrls-0-4");
			Assert.AreEqual (flat_controls [0].GetNextControl (flat_controls [1], true), null, "ctrls-0-5");
			Assert.AreEqual (flat_controls [0].GetNextControl (flat_controls [1], false), null, "ctrls-0-6");
			Assert.AreEqual (flat_controls [0].GetNextControl (flat_controls [2], true), null, "ctrls-0-7");
			Assert.AreEqual (flat_controls [0].GetNextControl (flat_controls [2], false), null,"ctrls-0-8");
			form.Dispose ();
		}

		[Test]
		public void GetNextGroupBoxControlFlat ()
		{
			Form form = new Form ();
			GroupBoxPoker gbp = new GroupBoxPoker ("group-box");

			gbp.Controls.AddRange (flat_controls);
			form.Controls.Add (gbp);
			form.Show ();

			Assert.AreEqual (form.GetNextControl (null, true), gbp, "form-1");
			Assert.AreEqual (form.GetNextControl (null, false), flat_controls [2], "form-2");

			Assert.AreEqual (form.GetNextControl (gbp, true), flat_controls [0], "gb-1");
			Assert.AreEqual (form.GetNextControl (gbp, false), null, "gb-2");

			Assert.AreEqual (gbp.GetNextControl (null, true), flat_controls [0], "gb-3");
			Assert.AreEqual (gbp.GetNextControl (null, false), flat_controls [2], "gb-4");
			Assert.AreEqual (gbp.GetNextControl (gbp, true), flat_controls [0], "gb-5");
			Assert.AreEqual (gbp.GetNextControl (gbp, false), flat_controls [2], "gb-6");

			Assert.AreEqual (form.GetNextControl (flat_controls [0], true), flat_controls [1], "form-ctrls-0-forward");
			Assert.AreEqual (form.GetNextControl (flat_controls [0], false), gbp, "form-ctrls-0-backward");
			Assert.AreEqual (form.GetNextControl (flat_controls [1], true), flat_controls [2], "form-ctrls-1-forward");
			Assert.AreEqual (form.GetNextControl (flat_controls [1], false), flat_controls [0], "form-ctrls-1-backward");
			Assert.AreEqual (form.GetNextControl (flat_controls [2], true), null, "form-ctrls-2-forward");
			Assert.AreEqual (form.GetNextControl (flat_controls [2], false), flat_controls [1],"form-ctrls-2-backward");

			Assert.AreEqual (gbp.GetNextControl (flat_controls [0], true), flat_controls [1], "gbp-ctrls-0-forward");
			Assert.AreEqual (gbp.GetNextControl (flat_controls [0], false), null, "gbp-ctrls-0-backward");
			Assert.AreEqual (gbp.GetNextControl (flat_controls [1], true), flat_controls [2], "gbp-ctrls-1-forward");
			Assert.AreEqual (gbp.GetNextControl (flat_controls [1], false), flat_controls [0], "gbp-ctrls-1-backward");
			Assert.AreEqual (gbp.GetNextControl (flat_controls [2], true), null, "gbp-ctrls-2-forward");
			Assert.AreEqual (gbp.GetNextControl (flat_controls [2], false), flat_controls [1],"gbp-ctrls-2-backward");		
			form.Dispose ();
		}

		[Test]
		public void GetNextControlFromTabControl ()
		{
			Form form = new Form ();
			TabControl tab = new TabControl ();
			TabPage page1 = new TabPage ("page one");
			TabPage page2 = new TabPage ("page two");

			tab.TabPages.Add (page1);
			tab.TabPages.Add (page2);

			form.Controls.Add (tab);
			form.Show ();

			Assert.AreEqual (form.GetNextControl (null, true), tab, "form-1");
			Assert.AreEqual (form.GetNextControl (null, false), page2, "form-2");

			Assert.AreEqual (form.GetNextControl (tab, true), page1, "tab-1");
			Assert.AreEqual (form.GetNextControl (tab, false), null, "tab-2");

			Assert.AreEqual (form.GetNextControl (page1, true), page2, "page-one-1");
			Assert.AreEqual (form.GetNextControl (page1, false), tab, "page-one-2");

			Assert.AreEqual (form.GetNextControl (page2, true), null, "page-two-1");
			Assert.AreEqual (form.GetNextControl (page2, false), page1, "page-two-2");
			form.Dispose ();
		}

		[Test]
		public void GetNextControlFromTabControl2 () {
			Form form = new Form ();
			TabControl tab = new TabControl ();
			
			TabPage page1 = new TabPage ("page one");
			page1.Controls.AddRange (flat_controls);

			TabPage page2 = new TabPage ("page two");

			tab.TabPages.Add (page1);

			tab.TabPages.Add (page2);

			form.Controls.Add (tab);
			form.Show ();

			Assert.AreEqual (form.GetNextControl (null, true), tab, "form-1");
			Assert.AreEqual (form.GetNextControl (null, false), page2, "form-2");

			Assert.AreEqual (form.GetNextControl (tab, true), page1, "tab-1");
			Assert.AreEqual (form.GetNextControl (tab, false), null, "tab-2");

			Assert.AreEqual (form.GetNextControl (page1, true), flat_controls [0], "page-one-1");
			Assert.AreEqual (form.GetNextControl (page1, false), tab, "page-one-2");

			Assert.AreEqual (form.GetNextControl (page2, true), null, "page-two-1");
			Assert.AreEqual (form.GetNextControl (page2, false), flat_controls [2], "page-two-2");

			Assert.AreEqual (form.GetNextControl (flat_controls [0], false), page1, "form-ctrls-0-backward");
			Assert.AreEqual (form.GetNextControl (flat_controls [2], true), page2, "form-ctrls-2-forward");

			Assert.AreEqual (tab.GetNextControl (null, true), page1, "tab-null-forward");
			Assert.AreEqual (tab.GetNextControl (page1, false), null, "tab-page1-backward");

			Assert.AreEqual (tab.GetNextControl (flat_controls [0], false), page1, "tab-ctrls-0-backward");
			Assert.AreEqual (tab.GetNextControl (flat_controls [2], true), page2, "tab-ctrls-2-forward");

			Assert.AreEqual (page1.GetNextControl (flat_controls [0], true), flat_controls [1], "page1-ctrls-0-forward");
			Assert.AreEqual (page1.GetNextControl (flat_controls [0], false), null, "page1-ctrls-0-backward");
			Assert.AreEqual (page1.GetNextControl (flat_controls [1], true), flat_controls [2], "page1-ctrls-1-forward");
			Assert.AreEqual (page1.GetNextControl (flat_controls [1], false), flat_controls [0], "page1-ctrls-1-backward");
			Assert.AreEqual (page1.GetNextControl (flat_controls [2], true), null, "page1-ctrls-2-forward");
			Assert.AreEqual (page1.GetNextControl (flat_controls [2], false), flat_controls [1],"page1-ctrls-2-backward");
			form.Dispose ();
		}

		[Test]
		public void GetNextControlTabIndex ()
		{
			Form form = new Form ();
			ControlPoker [] ctrls = new ControlPoker [5];

			for (int i = 0; i < 5; i++) {
				ctrls [i] = new ControlPoker ();
				ctrls [i].TabIndex = i;
				ctrls [i].Text = "ctrl " + i;
			}

			form.Controls.AddRange (ctrls);
			form.Show ();

			Assert.AreEqual (form.GetNextControl (null, true), ctrls [0], "A1");
			Assert.AreEqual (form.GetNextControl (null, false), ctrls [4], "A2");

			Assert.AreEqual (form.GetNextControl (ctrls [0], true), ctrls [1], "A3");
			Assert.AreEqual (form.GetNextControl (ctrls [0], false), null, "A4");

			Assert.AreEqual (form.GetNextControl (ctrls [1], true), ctrls [2], "A5");
			Assert.AreEqual (form.GetNextControl (ctrls [1], false), ctrls [0], "A6");

			Assert.AreEqual (form.GetNextControl (ctrls [2], true), ctrls [3], "A7");
			Assert.AreEqual (form.GetNextControl (ctrls [2], false), ctrls [1], "A8");

			Assert.AreEqual (form.GetNextControl (ctrls [3], true), ctrls [4], "A9");
			Assert.AreEqual (form.GetNextControl (ctrls [3], false), ctrls [2], "A10");

			Assert.AreEqual (form.GetNextControl (ctrls [4], true), null, "A11");
			Assert.AreEqual (form.GetNextControl (ctrls [4], false), ctrls [3], "A12");

			form.Dispose ();
		}

		[Test]
		public void GetNextControlDuplicateTabIndex ()
		{
			Form form = new Form ();
			ControlPoker [] ctrls = new ControlPoker [5];

			for (int i = 0; i < 5; i++) {
				ctrls [i] = new ControlPoker ();
				ctrls [i].TabIndex = i;
				ctrls [i].Text = "ctrl " + i;
			}

			ctrls [3].TabIndex = 2;

			form.Controls.AddRange (ctrls);
			form.Show ();

			Assert.AreEqual (form.GetNextControl (null, true), ctrls [0], "A1");
			Assert.AreEqual (form.GetNextControl (null, false), ctrls [4], "A2");

			Assert.AreEqual (form.GetNextControl (ctrls [0], true), ctrls [1], "A3");
			Assert.AreEqual (form.GetNextControl (ctrls [0], false), null, "A4");

			Assert.AreEqual (form.GetNextControl (ctrls [1], true), ctrls [2], "A5");
			Assert.AreEqual (form.GetNextControl (ctrls [1], false), ctrls [0], "A6");

			Assert.AreEqual (form.GetNextControl (ctrls [2], true), ctrls [3], "A7");
			Assert.AreEqual (form.GetNextControl (ctrls [2], false), ctrls [1], "A8");

			Assert.AreEqual (form.GetNextControl (ctrls [3], true), ctrls [4], "A9");
			Assert.AreEqual (form.GetNextControl (ctrls [3], false), ctrls [2], "A10");

			Assert.AreEqual (form.GetNextControl (ctrls [4], true), null, "A11");
			Assert.AreEqual (form.GetNextControl (ctrls [4], false), ctrls [3], "A12");

			form.Dispose ();
		}

		[Test]
		public void GetNextControlComposite ()
		{
			Form form = new Form ();
			ControlPoker a = new ControlPoker ("a");
			ControlPoker b = new ControlPoker ("b");
			ControlPoker c = new ControlPoker ("c");

			form.Controls.Add (a);
			form.Controls.Add (b);
			b.Controls.Add (c);

			form.Show ();

			Assert.AreEqual (form.GetNextControl (a, true), b, "form-1");
			Assert.AreEqual (form.GetNextControl (a, false), null, "form-2");

			form.Dispose ();
		}

		[Test]
		public void FocusSetsActive ()
		{
			Form form = new Form ();

			form.Controls.AddRange (flat_controls);
			form.Show ();

			Assert.AreEqual (form.ActiveControl, flat_controls [0], "A1");

			flat_controls [1].Focus ();

			Assert.AreEqual (form.ActiveControl, flat_controls [1], "A2");

			form.Dispose ();
		}
	}

}


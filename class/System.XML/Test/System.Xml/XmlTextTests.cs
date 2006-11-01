//
// System.Xml.XmlTextWriterTests
//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Kral Ferch
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlTextTests : Assertion
	{
		XmlDocument document;
		XmlText text;
		bool inserted;
		bool inserting;
		bool changed;
		bool changing;
		bool removed;
		bool removing;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
		}

		private void EventNodeInserted(Object sender, XmlNodeChangedEventArgs e)
		{
			inserted = true;
		}

		private void EventNodeInserting (Object sender, XmlNodeChangedEventArgs e)
		{
			inserting = true;
		}

		private void EventNodeChanged(Object sender, XmlNodeChangedEventArgs e)
		{
			changed = true;
		}

		private void EventNodeChanging (Object sender, XmlNodeChangedEventArgs e)
		{
			changing = true;
		}

		private void EventNodeRemoved(Object sender, XmlNodeChangedEventArgs e)
		{
			removed = true;
		}

		private void EventNodeRemoving (Object sender, XmlNodeChangedEventArgs e)
		{
			removing = true;
		}

		[Test]
		public void InnerAndOuterXml ()
		{
			text = document.CreateTextNode ("&<>\"'");
			AssertEquals (String.Empty, text.InnerXml);
			AssertEquals ("&amp;&lt;&gt;\"'", text.OuterXml);
		}
		
		[Test]
		public void SplitText ()
		{
			document.LoadXml ("<root>test text.</root>");
			document.NodeInserted += new XmlNodeChangedEventHandler(EventNodeInserted);
			document.NodeChanged += new XmlNodeChangedEventHandler(EventNodeChanged);
			document.NodeRemoved += new XmlNodeChangedEventHandler(EventNodeRemoved);
			XmlText t = document.DocumentElement.FirstChild as XmlText;
			t.SplitText (5);
			AssertNotNull (t.NextSibling);
			AssertEquals ("test ", t.Value);
			AssertEquals ("text.", t.NextSibling.Value);
			Assert (changed);
			Assert (inserted);
			Assert (!removed);
		}
	}
}

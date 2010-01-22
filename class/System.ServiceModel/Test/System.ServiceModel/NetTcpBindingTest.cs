//
// NetTcpBindingTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class NetTcpBindingTest
	{
		[Test]
		public void DefaultValues ()
		{
			var n = new NetTcpBinding ();
			Assert.AreEqual (HostNameComparisonMode.StrongWildcard, n.HostNameComparisonMode, "#1");
			Assert.AreEqual (10, n.ListenBacklog, "#2");
			Assert.AreEqual (false, n.PortSharingEnabled, "#3");

			var tr = n.CreateBindingElements ().Find<TcpTransportBindingElement> ();
			Assert.IsNotNull (tr, "#tr1");
			Assert.AreEqual (false, tr.TeredoEnabled, "#tr2");
			Assert.AreEqual ("net.tcp", tr.Scheme, "#tr3");

			Assert.IsFalse (n.TransactionFlow, "#4");
			var tx = n.CreateBindingElements ().Find<TransactionFlowBindingElement> ();
			Assert.IsNotNull (tx, "#tx1");
		}

		[Test]
		public void BufferedConnection ()
		{
			var host = new ServiceHost (typeof (Foo));
			var bindingsvc = new CustomBinding (new BinaryMessageEncodingBindingElement (), new TcpTransportBindingElement ());
			host.AddServiceEndpoint (typeof (IFoo), bindingsvc, "net.tcp://localhost/");
			host.Open (TimeSpan.FromSeconds (5));
			try {
				var bindingcli = new NetTcpBinding () { TransactionFlow = false };
				bindingcli.Security.Mode = SecurityMode.None;
				var cli = new ChannelFactory<IFooClient> (bindingcli, new EndpointAddress ("net.tcp://localhost/")).CreateChannel ();
				Assert.AreEqual (5, cli.Add (1, 4));
				Assert.AreEqual ("monkey science", cli.Join ("monkey", "science"));
			} finally {
				host.Close (TimeSpan.FromSeconds (5));
				var t = new TcpListener (808);
				t.Start ();
				t.Stop ();
			}
			Assert.IsTrue (Foo.AddCalled, "#1");
			Assert.IsTrue (Foo.JoinCalled, "#2");
		}

		[Test]
		public void StreamedConnection ()
		{
			var host = new ServiceHost (typeof (Foo));
			var bindingsvc = new CustomBinding (new BinaryMessageEncodingBindingElement (), new TcpTransportBindingElement () { TransferMode = TransferMode.Streamed });
			host.AddServiceEndpoint (typeof (IFoo), bindingsvc, "net.tcp://localhost:37564/");
			host.Open (TimeSpan.FromSeconds (5));
			try {
				var bindingcli = new NetTcpBinding () { TransactionFlow = false };
				bindingcli.TransferMode = TransferMode.Streamed;
				bindingcli.Security.Mode = SecurityMode.None;
				var cli = new ChannelFactory<IFooClient> (bindingcli, new EndpointAddress ("net.tcp://localhost:37564/")).CreateChannel ();
				Assert.AreEqual (5, cli.Add (1, 4));
				Assert.AreEqual ("monkey science", cli.Join ("monkey", "science"));
			} finally {
				host.Close (TimeSpan.FromSeconds (5));
				var t = new TcpListener (37564);
				t.Start ();
				t.Stop ();
			}
			Assert.IsTrue (Foo.AddCalled, "#1");
			Assert.IsTrue (Foo.JoinCalled, "#2");
		}

		[ServiceContract]
		public interface IFoo
		{
			[OperationContract]
			int Add (short s, int i);
			[OperationContract]
			string Join (string s1, string s2);
		}

		public interface IFooClient : IFoo, IClientChannel
		{
		}

		public class Foo : IFoo
		{
			public static bool AddCalled;
			public static bool JoinCalled;

			public int Add (short s, int i)
			{
				AddCalled = true;
				return s + i;
			}

			public string Join (string s1, string s2)
			{
				JoinCalled = true;
				return s1 + " " + s2;
			}
		}
	}
}

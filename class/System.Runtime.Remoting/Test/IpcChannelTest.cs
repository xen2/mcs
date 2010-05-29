//
// MonoTests.Remoting.IpcChannelTests.cs
//
// Authors:
// 	Robert Jordan (robertj@gmx.net)
//

#if NET_2_0

using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	[TestFixture]
	public class IpcChannelTest
	{
		[Test]
		public void Bug81653 ()
		{
			IpcClientChannel c = new IpcClientChannel ();
			ChannelDataStore cd = new ChannelDataStore (new string[] { "foo" });
			string objectUri;
			c.CreateMessageSink (null, cd, out objectUri);
		}

		[Test]
		public void Bug609381 ()
		{
			string portName = "ipc" + Guid.NewGuid ().ToString ("N");
			string objUri = "ipcserver609381.rem";
			string url = String.Format ("ipc://{0}/{1}", portName, objUri);

			IpcChannel serverChannel = new IpcChannel (portName);
			ChannelServices.RegisterChannel (serverChannel);

			RemotingServices.Marshal (new Server (), objUri);

			Server client = (Server) RemotingServices.Connect (typeof (Server), url);
			
			int count = 10 * 1024 * 1024;
			byte[] sendBuf = new byte[count];
			sendBuf [sendBuf.Length - 1] = 41;
			
			byte[] recvBuf = client.Send (sendBuf);

			Assert.IsNotNull (recvBuf);
			Assert.AreNotSame (sendBuf, recvBuf);
			Assert.AreEqual (count, recvBuf.Length);
			Assert.AreEqual (42, recvBuf [recvBuf.Length - 1]);

			sendBuf = null;
			recvBuf = null;

			ChannelServices.UnregisterChannel (serverChannel);
		}

		class Server : MarshalByRefObject
		{
			public byte[] Send (byte[] payload)
			{
				payload [payload.Length - 1]++;
				return payload;
			}
		}
	}
}

#endif

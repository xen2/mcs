//
// System.Runtime.Remoting.Channels.Simple.SimpleClientTransportSink.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Channels.Simple
{

	internal class SimpleClientTransportSink : IClientChannelSink
	{
		string host;
		string object_uri;
		int port;
		
		TcpClient tcpclient;
		
		public SimpleClientTransportSink (string url)
		{
			host = SimpleChannel.ParseSimpleURL (url, out object_uri, out port);
			tcpclient = new TcpClient ();
		}

		public IDictionary Properties
		{
			get {
				return null;
			}
		}

		public IClientChannelSink NextChannelSink
		{
			get {
				// we are the last one
				return null;
			}
		}

		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack, IMessage msg,
						 ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();			
		}

		public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack,
						  object state, ITransportHeaders headers,
						  Stream stream)
		{
			throw new NotImplementedException ();
		}

		public Stream GetRequestStream (IMessage msg, ITransportHeaders headers)
		{
			// no direct access to the stream
			return null;
		}
		
		public void ProcessMessage (IMessage msg,
					    ITransportHeaders requestHeaders,
					    Stream requestStream,
					    out ITransportHeaders responseHeaders,
					    out Stream responseStream)
		{
			// get a network stream
			tcpclient.Connect (host, port);
			Stream network_stream = tcpclient.GetStream ();

			// send the message
			SimpleMessageFormat.SendMessageStream (network_stream, (MemoryStream)requestStream,
							       SimpleMessageFormat.MessageType.Request,
							       object_uri);
			
			// read the response fro the network an copy it to a memory stream
			SimpleMessageFormat.MessageType msg_type;
			string uri;
			MemoryStream mem_stream = SimpleMessageFormat.ReceiveMessageStream (network_stream, out msg_type, out uri);

			// close the stream
			tcpclient.Close ();

			switch (msg_type) {
			case SimpleMessageFormat.MessageType.Response:
				//fixme: read response message
				responseHeaders = null;
			
				responseStream = mem_stream;
		
				break;
			default:
				throw new Exception ("unknown response mesage header");
			}
		}
			
	}
}	

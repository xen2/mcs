//
// Mono.Data.Tds.Protocol.TdsComm.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Mono.Data.Tds.Protocol {
        internal sealed class TdsComm
	{
		#region Fields

		NetworkStream stream;
		int packetSize;
		TdsPacketType packetType = TdsPacketType.None;
		bool connReset;
		Encoding encoder;

		string dataSource;
		int commandTimeout;

		byte[] outBuffer;
		int outBufferLength;
		int nextOutBufferIndex = 0;

		byte[] inBuffer;
		int inBufferLength;
		int inBufferIndex = 0;

		static int headerLength = 8;

		byte[] tmpBuf = new byte[8];
		byte[] resBuffer = new byte[256];

		int packetsSent = 0;
		int packetsReceived = 0;

		Socket socket;
		TdsVersion tdsVersion;

		#endregion // Fields
		
		#region Constructors

		public TdsComm (string dataSource, int port, int packetSize, int timeout, TdsVersion tdsVersion)
		{
			this.packetSize = packetSize;
			this.tdsVersion = tdsVersion;
			this.dataSource = dataSource;

			outBuffer = new byte[packetSize];
			inBuffer = new byte[packetSize];

			outBufferLength = packetSize;
			inBufferLength = packetSize;

			IPEndPoint endPoint;
			
			try {
#if NET_2_0
				IPAddress ip;
				if(IPAddress.TryParse(this.dataSource, out ip)) {
					endPoint = new IPEndPoint(ip, port);
				} else {
					IPHostEntry hostEntry = Dns.GetHostEntry (this.dataSource);
					endPoint = new IPEndPoint(hostEntry.AddressList [0], port);
				}
#else
				IPHostEntry hostEntry = Dns.Resolve (this.dataSource);
				endPoint = new IPEndPoint (hostEntry.AddressList [0], port);
#endif
			} catch (SocketException e) {
				throw new TdsInternalException ("Server does not exist or connection refused.", e);
			}

			try {
				socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IAsyncResult ares = socket.BeginConnect (endPoint, null, null);
				if (timeout > 0 && !ares.IsCompleted && !ares.AsyncWaitHandle.WaitOne (timeout * 1000))
					throw Tds.CreateTimeoutException (dataSource, "Open()");
				socket.EndConnect (ares);
				try {
					// MS sets these socket option
					socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
				} catch (SocketException) {
					// Some platform may throw an exception, so
					// eat all socket exception, yeaowww! 
				}
				
				// Let the stream own the socket and take the pleasure of closing it
				stream = new NetworkStream (socket, true);
			} catch (SocketException e) {
				if (socket != null) {
					try {
						Socket s = socket;
						socket = null;
						s.Close ();
					} catch {}
				}
				throw new TdsInternalException ("Server does not exist or connection refused.", e);
			}
			if (!socket.Connected)
				throw new TdsInternalException ("Server does not exist or connection refused.", null);
		}
		
		#endregion // Constructors
		
		#region Properties

		public int CommandTimeout {
			get { return commandTimeout; }
			set { commandTimeout = value; }
		}

		internal Encoding Encoder {
			get { return encoder; }
			set { encoder = value; }
		}
		
		public int PacketSize {
			get { return packetSize; }
			set { packetSize = value; }
		}
		
		#endregion // Properties
		
		#region Methods

		public byte[] Swap(byte[] toswap) {
			byte[] ret = new byte[toswap.Length];
			for(int i = 0; i < toswap.Length; i++)
				ret [toswap.Length - i - 1] = toswap[i];

			return ret;
		}

		public void Append (object o)
		{
			if (o == null || o == DBNull.Value) {
				Append ((byte)0);
				return ;
			}
			switch (Type.GetTypeCode (o.GetType ())) {
			case TypeCode.Byte :
				Append ((byte) o);
				return;
			case TypeCode.Boolean:
				if ((bool)o == true)
					Append ((byte)1);
				else
					Append ((byte)0);
				return;
			case TypeCode.Object :
				if (o is byte[])
					Append ((byte[]) o);
				return;
			case TypeCode.Int16 :
				Append ((short) o);
				return;
			case TypeCode.Int32 :
				Append ((int) o);
				return;
			case TypeCode.String :
				Append ((string) o);
				return;
			case TypeCode.Double :
				Append ((double) o);
				return;
			case TypeCode.Single :
				Append ((float) o);
				return;
			case TypeCode.Int64 :
				Append ((long) o);
				return;
			case TypeCode.Decimal:
				Append ((decimal) o, 17);
				return;
			case TypeCode.DateTime:
				Append ((DateTime) o, 8);
				return;
			}
			throw new InvalidOperationException (String.Format ("Object Type :{0} , not being appended", o.GetType ()));
		}

		public void Append (byte b)
		{
			if (nextOutBufferIndex == outBufferLength) {
				SendPhysicalPacket (false);
				nextOutBufferIndex = headerLength;
			}
			Store (nextOutBufferIndex, b);
			nextOutBufferIndex++;
		}	

		public void Append (DateTime t, int bytes)
		{
			DateTime epoch = new DateTime (1900,1,1);
			
			TimeSpan span = t - epoch;
			int days = span.Days ;
			int val = 0;	

			if (bytes == 8) {
				long ms = (span.Hours * 3600 + span.Minutes * 60 + span.Seconds)*1000L + (long)span.Milliseconds;
				val = (int) ((ms*300)/1000);
				Append ((int) days);
				Append ((int) val);
			} else if (bytes ==4) {
				val = span.Hours * 60 + span.Minutes;
				Append ((ushort) days);
				Append ((short) val);
			} else {
				throw new Exception ("Invalid No of bytes");
			}
		}

		public void Append (byte[] b)
		{
			Append (b, b.Length, (byte) 0);
		}		

		public void Append (byte[] b, int len, byte pad)
		{
			int i = 0;
			for ( ; i < b.Length && i < len; i++)
			    Append (b[i]);

			for ( ; i < len; i++)
			    Append (pad);
		}	

		public void Append (short s)
		{
			if(!BitConverter.IsLittleEndian)
				Append (Swap (BitConverter.GetBytes(s)));
			else 
				Append (BitConverter.GetBytes (s));
		}

		public void Append (ushort s)
		{
			if(!BitConverter.IsLittleEndian)
				Append (Swap (BitConverter.GetBytes(s)));
			else 
				Append (BitConverter.GetBytes (s));
		}

		public void Append (int i)
		{
			if(!BitConverter.IsLittleEndian)
				Append (Swap (BitConverter.GetBytes(i)));
			else
				Append (BitConverter.GetBytes (i));
		}

		public void Append (string s)
		{
			if (tdsVersion < TdsVersion.tds70) 
				Append (encoder.GetBytes (s));
			else 
				foreach (char c in s)
					if(!BitConverter.IsLittleEndian)
						Append (Swap (BitConverter.GetBytes (c)));
					else
						Append (BitConverter.GetBytes (c));
		}	

		// Appends with padding
		public byte[] Append (string s, int len, byte pad)
		{
			if (s == null)
				return new byte[0];

			byte[] result = encoder.GetBytes (s);
			Append (result, len, pad);
			return result;
		}

		public void Append (double value)
		{
			if (!BitConverter.IsLittleEndian)
				Append (Swap (BitConverter.GetBytes (value)));
			else
				Append (BitConverter.GetBytes (value));
		}

		public void Append (float value)
		{
			if (!BitConverter.IsLittleEndian)
				Append (Swap (BitConverter.GetBytes (value)));
			else
				Append (BitConverter.GetBytes (value));
		}

		public void Append (long l)
		{
			if (!BitConverter.IsLittleEndian)
				Append (Swap (BitConverter.GetBytes (l)));
			else
				Append (BitConverter.GetBytes (l));
		}

		public void Append (decimal d, int bytes)
		{
			int[] arr = Decimal.GetBits (d);
			byte sign =  (d > 0 ? (byte)1 : (byte)0);
			Append (sign) ;
			Append (arr[0]);
			Append (arr[1]);
			Append (arr[2]);
			Append ((int)0);
		}

		public void Close ()
		{
			connReset = false;
			socket = null;
			stream.Close ();
		}

		public bool IsConnected () 
		{
			return socket != null && socket.Connected && !(socket.Poll (0, SelectMode.SelectRead) && socket.Available == 0);
		}
		
		public byte GetByte ()
		{
			byte result;

			if (inBufferIndex >= inBufferLength) {
				// out of data, read another physical packet.
				GetPhysicalPacket ();
			}
			result = inBuffer[inBufferIndex++];
			return result;
		}

		public byte[] GetBytes (int len, bool exclusiveBuffer)
		{
			byte[] result = null;
			int i;

			// Do not keep an internal result buffer larger than 16k.
			// This would unnecessarily use up memory.
			if (exclusiveBuffer || len > 16384)
				result = new byte[len];
			else
			{
				if (resBuffer.Length < len)
					resBuffer = new byte[len];
				result = resBuffer;
			}

			for (i = 0; i<len; )
			{
				if (inBufferIndex >= inBufferLength)
					GetPhysicalPacket ();

				int avail = inBufferLength - inBufferIndex;
				avail = avail>len-i ? len-i : avail;

				System.Array.Copy (inBuffer, inBufferIndex, result, i, avail);
				i += avail;
				inBufferIndex += avail;
			}

			return result;
		}

		public string GetString (int len)
		{
			if (tdsVersion == TdsVersion.tds70) 
				return GetString (len, true);
			else
				return GetString (len, false);
		}

		public string GetString (int len, bool wide)
		{
			if (wide) {
				char[] chars = new char[len];
				for (int i = 0; i < len; ++i) {
					int lo = ((byte) GetByte ()) & 0xFF;
					int hi = ((byte) GetByte ()) & 0xFF;
					chars[i] = (char) (lo | ( hi << 8));
				}
				return new String (chars);
			}
			else {
				byte[] result = new byte[len];
				Array.Copy (GetBytes (len, false), result, len);
				return (encoder.GetString (result));
			}
		}

		public int GetNetShort ()
		{
			byte[] tmp = new byte[2];
			tmp[0] = GetByte ();
			tmp[1] = GetByte ();
			return Ntohs (tmp, 0);
		}

		public short GetTdsShort ()
		{
			byte[] input = new byte[2];

			for (int i = 0; i < 2; i += 1)
				input[i] = GetByte ();
			if(!BitConverter.IsLittleEndian)
				return (BitConverter.ToInt16 (Swap (input), 0));
			else
				return (BitConverter.ToInt16 (input, 0));
		}


		public int GetTdsInt ()
		{
			byte[] input = new byte[4];
			for (int i = 0; i < 4; i += 1) {
				input[i] = GetByte ();
			}
			if(!BitConverter.IsLittleEndian)
				return (BitConverter.ToInt32 (Swap (input), 0));
			else
				return (BitConverter.ToInt32 (input, 0));
		}

		public long GetTdsInt64 ()
		{
			byte[] input = new byte[8];
			for (int i = 0; i < 8; i += 1)
				input[i] = GetByte ();
			if(!BitConverter.IsLittleEndian)
				return (BitConverter.ToInt64 (Swap (input), 0));
			else
				return (BitConverter.ToInt64 (input, 0));
		}

		private void GetPhysicalPacket ()
		{
                        int dataLength = GetPhysicalPacketHeader ();
                        GetPhysicalPacketData (dataLength);
		}

                private int GetPhysicalPacketHeader ()
                {
                        int nread = 0;

			// read the header
			while (nread < 8)
				nread += stream.Read (tmpBuf, nread, 8 - nread);

			TdsPacketType packetType = (TdsPacketType) tmpBuf[0];
			if (packetType != TdsPacketType.Logon && packetType != TdsPacketType.Query && packetType != TdsPacketType.Reply) 
			{
				throw new Exception (String.Format ("Unknown packet type {0}", tmpBuf[0]));
			}

			// figure out how many bytes are remaining in this packet.
			int len = Ntohs (tmpBuf, 2) - 8;
			if (len >= inBuffer.Length) 
				inBuffer = new byte[len];

			if (len < 0) {
				throw new Exception (String.Format ("Confused by a length of {0}", len));
			}
                        
                        return len;

                }
                
                private void GetPhysicalPacketData (int length)
                {
                        // now get the data
			int nread = 0;

			while (nread < length) {
				nread += stream.Read (inBuffer, nread, length - nread);
			}

			packetsReceived++;

			// adjust the bookkeeping info about the incoming buffer
			inBufferLength = length;
			inBufferIndex = 0;
                }
                

		private static int Ntohs (byte[] buf, int offset)
		{
			int lo = ((int) buf[offset + 1] & 0xff);
			int hi = (((int) buf[offset] & 0xff ) << 8);

			return hi | lo;
			// return an int since we really want an _unsigned_
		}		

		public byte Peek ()
		{
			// If out of data, read another physical packet.
			if (inBufferIndex >= inBufferLength)
				GetPhysicalPacket ();

			return inBuffer[inBufferIndex];
		}

		public bool Poll (int seconds, SelectMode selectMode)
		{
			return Poll (socket, seconds, selectMode);
		}

		private bool Poll (Socket s, int seconds, SelectMode selectMode)
		{
			long uSeconds = seconds * 1000000;
			bool bState = false;

			while (uSeconds > (long) Int32.MaxValue) {
				bState = s.Poll (Int32.MaxValue, selectMode);
				if (bState) 
					return true;
				uSeconds -= Int32.MaxValue;
			}
			return s.Poll ((int) uSeconds, selectMode);
		}

		internal void ResizeOutBuf (int newSize)
		{
			if (newSize != outBufferLength) {
				byte[] newBuf = new byte [newSize];
				Array.Copy (outBuffer, 0, newBuf, 0, newSize);
				outBufferLength = newSize;
				outBuffer = newBuf;
			}
		}

		public bool ResetConnection {
			get { return connReset; }
			set { connReset = value; }
		}

		public void SendPacket ()
		{
			// Reset connection flag is only valid for SQLBatch/RPC/DTC messages
			if (packetType != TdsPacketType.Query && packetType != TdsPacketType.RPC)
				connReset = false;
			
			SendPhysicalPacket (true);
			nextOutBufferIndex = 0;
			packetType = TdsPacketType.None;
			// Reset connection-reset flag to false - as any exception would anyway close 
			// the whole connection
			connReset = false;
		}
		
		private void SendPhysicalPacket (bool isLastSegment)
		{
			if (nextOutBufferIndex > headerLength || packetType == TdsPacketType.Cancel) {
				byte status =  (byte) ((isLastSegment ? 0x01 : 0x00) | (connReset ? 0x08 : 0x00)); 
				// packet type
				Store (0, (byte) packetType);
				Store (1, status);
				Store (2, (short) nextOutBufferIndex );
				Store (4, (byte) 0);
				Store (5, (byte) 0);
				Store (6, (byte) (tdsVersion == TdsVersion.tds70 ? 0x1 : 0x0));
				Store (7, (byte) 0);

				stream.Write (outBuffer, 0, nextOutBufferIndex);
				stream.Flush ();
				packetsSent++;
			}
		}
		
		public void Skip (long i)
		{
			for ( ; i > 0; i--)
				GetByte ();
		}

		public void StartPacket (TdsPacketType type)
		{
			if (type != TdsPacketType.Cancel && inBufferIndex != inBufferLength)
				inBufferIndex = inBufferLength;

			packetType = type;
			nextOutBufferIndex = headerLength;
		}

		private void Store (int index, byte value)
		{
			outBuffer[index] = value;
		}		

		private void Store (int index, short value)
		{
			outBuffer[index] = (byte) (((byte) (value >> 8)) & 0xff);
			outBuffer[index + 1] = (byte) (((byte) (value >> 0)) & 0xff);
		}

		#endregion // Methods
#if NET_2_0
                #region Async Methods

                public IAsyncResult BeginReadPacket (AsyncCallback callback, object stateObject)
		{
                        TdsAsyncResult ar = new TdsAsyncResult (callback, stateObject);

                        stream.BeginRead (tmpBuf, 0, 8, new AsyncCallback(OnReadPacketCallback), ar);
                        return ar;
		}
                
                /// <returns>Packet size in bytes</returns>
                public int EndReadPacket (IAsyncResult ar)
                {
                        if (!ar.IsCompleted)
                                ar.AsyncWaitHandle.WaitOne ();
                        return (int) ((TdsAsyncResult) ar).ReturnValue;
                }
                

                public void OnReadPacketCallback (IAsyncResult socketAsyncResult)
                {
                        TdsAsyncResult ar = (TdsAsyncResult) socketAsyncResult.AsyncState;
                        int nread = stream.EndRead (socketAsyncResult);
                        
                        while (nread < 8)
                                nread += stream.Read (tmpBuf, nread, 8 - nread);

			TdsPacketType packetType = (TdsPacketType) tmpBuf[0];
			if (packetType != TdsPacketType.Logon && packetType != TdsPacketType.Query && packetType != TdsPacketType.Reply) 
			{
				throw new Exception (String.Format ("Unknown packet type {0}", tmpBuf[0]));
			}

			// figure out how many bytes are remaining in this packet.
			int len = Ntohs (tmpBuf, 2) - 8;

			if (len >= inBuffer.Length) 
				inBuffer = new byte[len];

			if (len < 0) {
				throw new Exception (String.Format ("Confused by a length of {0}", len));
			}

                        GetPhysicalPacketData (len);
                        int value = len + 8;
                        ar.ReturnValue = ((object)value); // packet size
                        ar.MarkComplete ();
                }
                
                #endregion // Async Methods
#endif // NET_2_0

	}

}

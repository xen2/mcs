// System.Net.Sockets.Socket.cs
//
// Authors:
//	Phillip Pearson (pp@myelin.co.nz)
//	Dick Porter <dick@ximian.com>
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2001, 2002 Phillip Pearson and Ximian, Inc.
//    http://www.myelin.co.nz
// (c) 2004 Novell, Inc. (http://www.novell.com)
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
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Net.Configuration;

namespace System.Net.Sockets 
{
	public class Socket : IDisposable 
	{
		enum SocketOperation {
			Accept,
			Connect,
			Receive,
			ReceiveFrom,
			Send,
			SendTo,
			UsedInManaged1,
			UsedInManaged2,
			UsedInProcess,
			UsedInConsole2
		}

		[StructLayout (LayoutKind.Sequential)]
		private sealed class SocketAsyncResult: IAsyncResult 
		{
			/* Same structure in the runtime */
			public Socket Sock;
			public IntPtr handle;
			object state;
			AsyncCallback callback;
			WaitHandle waithandle;

			Exception delayedException;

			public EndPoint EndPoint;	// Connect,ReceiveFrom,SendTo
			public byte [] Buffer;		// Receive,ReceiveFrom,Send,SendTo
			public int Offset;		// Receive,ReceiveFrom,Send,SendTo
			public int Size;		// Receive,ReceiveFrom,Send,SendTo
			public SocketFlags SockFlags;	// Receive,ReceiveFrom,Send,SendTo

			// Return values
			Socket acc_socket;
			int total;

			bool completed_sync;
			bool completed;
			public bool blocking;
			internal int error;
			SocketOperation operation;
			public object ares;

			public SocketAsyncResult (Socket sock, object state, AsyncCallback callback, SocketOperation operation)
			{
				this.Sock = sock;
				this.blocking = sock.blocking;
				this.handle = sock.socket;
				this.state = state;
				this.callback = callback;
				this.operation = operation;
				SockFlags = SocketFlags.None;
			}

			public void CheckIfThrowDelayedException ()
			{
				if (delayedException != null)
					throw delayedException;

				if (error != 0) {
					throw new SocketException (error);
				}
			}

			void CompleteAllOnDispose (Queue queue)
			{
				object [] pending = queue.ToArray ();
				queue.Clear ();

				WaitCallback cb;
				for (int i = 0; i < pending.Length; i++) {
					SocketAsyncResult ares = (SocketAsyncResult) pending [i];
					cb = new WaitCallback (ares.CompleteDisposed);
					ThreadPool.QueueUserWorkItem (cb, null);
				}
			}

			void CompleteDisposed (object unused)
			{
				Complete ();
			}

			public void Complete ()
			{
				if (operation != SocketOperation.Receive && Sock.disposed)
					delayedException = new ObjectDisposedException (Sock.GetType ().ToString ());

				IsCompleted = true;

				Queue queue = null;
				if (operation == SocketOperation.Receive || operation == SocketOperation.ReceiveFrom) {
					queue = Sock.readQ;
				} else if (operation == SocketOperation.Send || operation == SocketOperation.SendTo) {
					queue = Sock.writeQ;
				}

				if (queue != null) {
					SocketAsyncCall sac = null;
					SocketAsyncResult req = null;
					lock (queue) {
						queue.Dequeue (); // remove ourselves
						if (queue.Count > 0) {
							req = (SocketAsyncResult) queue.Peek ();
							if (!Sock.disposed) {
								Worker worker = new Worker (req);
								sac = GetDelegate (worker, req.operation);
							} else {
								CompleteAllOnDispose (queue);
							}
						}
					}

					if (sac != null)
						sac.BeginInvoke (null, req);
				}

				if (callback != null)
					callback (this);
			}

			SocketAsyncCall GetDelegate (Worker worker, SocketOperation op)
			{
				switch (op) {
				case SocketOperation.Receive:
					return new SocketAsyncCall (worker.Receive);
				case SocketOperation.ReceiveFrom:
					return new SocketAsyncCall (worker.ReceiveFrom);
				case SocketOperation.Send:
					return new SocketAsyncCall (worker.Send);
				case SocketOperation.SendTo:
					return new SocketAsyncCall (worker.SendTo);
				default:
					return null; // never happens
				}
			}

			public void Complete (bool synch)
			{
				completed_sync = synch;
				Complete ();
			}

			public void Complete (int total)
			{
				this.total = total;
				Complete ();
			}

			public void Complete (Exception e, bool synch)
			{
				completed_sync = synch;
				delayedException = e;
				Complete ();
			}

			public void Complete (Exception e)
			{
				delayedException = e;
				Complete ();
			}

			public void Complete (Socket s)
			{
				acc_socket = s;
				Complete ();
			}

			public object AsyncState {
				get {
					return state;
				}
			}

			public WaitHandle AsyncWaitHandle {
				get {
					lock (this) {
						if (waithandle == null)
							waithandle = new ManualResetEvent (completed);
					}

					return waithandle;
				}
				set {
					waithandle=value;
				}
			}

			public bool CompletedSynchronously {
				get {
					return(completed_sync);
				}
			}

			public bool IsCompleted {
				get {
					return(completed);
				}
				set {
					completed=value;
					lock (this) {
						if (waithandle != null && value) {
							((ManualResetEvent) waithandle).Set ();
						}
					}
				}
			}
			
			public Socket Socket {
				get {
					return acc_socket;
				}
			}

			public int Total {
				get { return total; }
				set { total = value; }
			}
		}

		private sealed class Worker 
		{
			SocketAsyncResult result;

			public Worker (SocketAsyncResult ares)
			{
				this.result = ares;
			}

			public void Accept ()
			{
				Socket acc_socket = null;
				try {
					acc_socket = result.Sock.Accept ();
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				result.Complete (acc_socket);
			}

			public void Connect ()
			{
				try {
					if (!result.Sock.Blocking) {
						int success;
						result.Sock.Poll (-1, SelectMode.SelectWrite, out success);
						if (success == 0) {
							result.Sock.connected = true;
						} else {
							result.Complete (new SocketException (success));
							return;
						}
					} else {
						result.Sock.Connect (result.EndPoint);
						result.Sock.connected = true;
					}
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				result.Complete ();
			}

			public void Receive ()
			{
				// Actual recv() done in the runtime
				result.Complete ();
			}

			public void ReceiveFrom ()
			{
				int total = 0;
				try {
					total = result.Sock.ReceiveFrom_nochecks (result.Buffer,
									 result.Offset,
									 result.Size,
									 result.SockFlags,
									 ref result.EndPoint);
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				result.Complete (total);
			}

			int send_so_far;

			void UpdateSendValues (int last_sent)
			{
				if (result.error == 0) {
					send_so_far += last_sent;
					result.Offset += last_sent;
					result.Size -= last_sent;
				}
			}

			public void Send ()
			{
				// Actual send() done in the runtime
				if (result.error == 0) {
					UpdateSendValues (result.Total);
					if (result.Sock.disposed) {
						result.Complete ();
						return;
					}

					if (result.Size > 0) {
						SocketAsyncCall sac = new SocketAsyncCall (this.Send);
						sac.BeginInvoke (null, result);
						return; // Have to finish writing everything. See bug #74475.
					}
					result.Total = send_so_far;
				}
				result.Complete ();
			}

			public void SendTo ()
			{
				int total = 0;
				try {
					total = result.Sock.SendTo_nochecks (result.Buffer,
								    result.Offset,
								    result.Size,
								    result.SockFlags,
								    result.EndPoint);

					UpdateSendValues (total);
					if (result.Size > 0) {
						SocketAsyncCall sac = new SocketAsyncCall (this.SendTo);
						sac.BeginInvoke (null, result);
						return; // Have to finish writing everything. See bug #74475.
					}
					result.Total = send_so_far;
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				result.Complete ();
			}
		}
			
		/* the field "socket" is looked up by name by the runtime */
		private IntPtr socket;
		private AddressFamily address_family;
		private SocketType socket_type;
		private ProtocolType protocol_type;
		internal bool blocking=true;
		private Queue readQ = new Queue (2);
		private Queue writeQ = new Queue (2);

		delegate void SocketAsyncCall ();
		/*
		 *	These two fields are looked up by name by the runtime, don't change
		 *  their name without also updating the runtime code.
		 */
		private static int ipv4Supported = -1, ipv6Supported = -1;

		/* When true, the socket was connected at the time of
		 * the last IO operation
		 */
		private bool connected=false;
		/* true if we called Close_internal */
		private bool closed;
		internal bool disposed;
		

		/* Used in LocalEndPoint and RemoteEndPoint if the
		 * Mono.Posix assembly is available
		 */
		private static object unixendpoint=null;
		private static Type unixendpointtype=null;
		

		static void AddSockets (ArrayList sockets, IList list, string name)
		{
			if (list != null) {
				foreach (Socket sock in list) {
					if (sock == null) // MS throws a NullRef
						throw new ArgumentNullException (name, "Contains a null element");
					sockets.Add (sock);
				}
			}

			sockets.Add (null);
		}
#if !TARGET_JVM
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Select_internal (ref Socket [] sockets,
							int microSeconds,
							out int error);
#endif
		public static void Select (IList checkRead, IList checkWrite, IList checkError, int microSeconds)
		{
			ArrayList list = new ArrayList ();
			AddSockets (list, checkRead, "checkRead");
			AddSockets (list, checkWrite, "checkWrite");
			AddSockets (list, checkError, "checkError");

			if (list.Count == 3) {
				throw new ArgumentNullException ("checkRead, checkWrite, checkError",
								 "All the lists are null or empty.");
			}

			int error;
			/*
			 * The 'sockets' array contains: READ socket 0-n, null,
			 * 				 WRITE socket 0-n, null,
			 *				 ERROR socket 0-n, null
			 */
			Socket [] sockets = (Socket []) list.ToArray (typeof (Socket));
			Select_internal (ref sockets, microSeconds, out error);

			if (error != 0)
				throw new SocketException (error);

			if (checkRead != null)
				checkRead.Clear ();

			if (checkWrite != null)
				checkWrite.Clear ();

			if (checkError != null)
				checkError.Clear ();

			if (sockets == null)
				return;

			int mode = 0;
			int count = sockets.Length;
			IList currentList = checkRead;
			for (int i = 0; i < count; i++) {
				Socket sock = sockets [i];
				if (sock == null) { // separator
					currentList = (mode == 0) ? checkWrite : checkError;
					mode++;
					continue;
				}

				if (currentList != null) {
					if (currentList == checkWrite) {
						if ((int)sock.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error) == 0) {
							sock.connected = true;
						}
					}
					
					currentList.Add (sock);
				}
			}
		}

		static Socket() {
			Assembly ass;
			
			try {
				ass = Assembly.Load (Consts.AssemblyMono_Posix);
			} catch (FileNotFoundException) {
				return;
			}
				
			unixendpointtype=ass.GetType("Mono.Posix.UnixEndPoint");

			/* The endpoint Create() method is an instance
			 * method :-(
			 */
			Type[] arg_types=new Type[1];
			arg_types[0]=typeof(string);
			ConstructorInfo cons=unixendpointtype.GetConstructor(arg_types);

			object[] args=new object[1];
			args[0]="nothing";

			unixendpoint=cons.Invoke(args);
		}

		// private constructor used by Accept, which already
		// has a socket handle to use
		private Socket(AddressFamily family, SocketType type,
			       ProtocolType proto, IntPtr sock) {
			address_family=family;
			socket_type=type;
			protocol_type=proto;
			
			socket=sock;
			connected=true;
		}
#if !TARGET_JVM
		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern IntPtr Socket_internal(AddressFamily family,
						      SocketType type,
						      ProtocolType proto,
						      out int error);
#endif		
		public Socket(AddressFamily family, SocketType type,
			      ProtocolType proto) {
			address_family=family;
			socket_type=type;
			protocol_type=proto;
			
			int error;
			
			socket=Socket_internal(family, type, proto, out error);
			if (error != 0) {
				throw new SocketException (error);
			}
		}

		public AddressFamily AddressFamily {
			get {
				return(address_family);
			}
		}
#if !TARGET_JVM
		// Returns the amount of data waiting to be read on socket
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Available_internal(IntPtr socket,
							     out int error);
#endif
		
		public int Available {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				int ret, error;
				
				ret = Available_internal(socket, out error);

				if (error != 0) {
					throw new SocketException (error);
				}

				return(ret);
			}
		}
#if !TARGET_JVM
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Blocking_internal(IntPtr socket,
							     bool block,
							     out int error);
#endif
		public bool Blocking {
			get {
				return(blocking);
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				int error;
				
				Blocking_internal(socket, value, out error);

				if (error != 0) {
					throw new SocketException (error);
				}
				
				blocking=value;
			}
		}

		public bool Connected {
			get {
				return(connected);
			}
		}

		public IntPtr Handle {
			get {
				return(socket);
			}
		}
#if !TARGET_JVM
		// Returns the local endpoint details in addr and port
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static SocketAddress LocalEndPoint_internal(IntPtr socket, out int error);
#endif
		// Wish:  support non-IP endpoints.
		public EndPoint LocalEndPoint {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				SocketAddress sa;
				int error;
				
				sa=LocalEndPoint_internal(socket, out error);

				if (error != 0) {
					throw new SocketException (error);
				}

				if(sa.Family==AddressFamily.InterNetwork || sa.Family==AddressFamily.InterNetworkV6) {
					// Stupidly, EndPoint.Create() is an
					// instance method
					return new IPEndPoint(0, 0).Create(sa);
				} else if (sa.Family==AddressFamily.Unix &&
					   unixendpoint!=null) {
					return((EndPoint)unixendpointtype.InvokeMember("Create", BindingFlags.InvokeMethod|BindingFlags.Instance|BindingFlags.Public, null, unixendpoint, new object[] {sa}));
				} else {
					throw new NotImplementedException();
				}
			}
		}

		public ProtocolType ProtocolType {
			get {
				return(protocol_type);
			}
		}

		// Returns the remote endpoint details in addr and port
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static SocketAddress RemoteEndPoint_internal(IntPtr socket, out int error);

		//
		// Wish: Support non-IP endpoints
		//
		public EndPoint RemoteEndPoint {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				SocketAddress sa;
				int error;
				
				sa=RemoteEndPoint_internal(socket, out error);

				if (error != 0) {
					throw new SocketException (error);
				}

				if(sa.Family==AddressFamily.InterNetwork || sa.Family==AddressFamily.InterNetworkV6 ) {
					// Stupidly, EndPoint.Create() is an
					// instance method
					return new IPEndPoint(0, 0).Create(sa);
				} else if (sa.Family==AddressFamily.Unix &&
					   unixendpoint!=null) {
					return((EndPoint)unixendpointtype.InvokeMember("Create", BindingFlags.InvokeMethod|BindingFlags.Instance|BindingFlags.Public, null, unixendpoint, new object[] {sa}));
				} else {
					throw new NotImplementedException();
				}
			}
		}

		public SocketType SocketType {
			get {
				return(socket_type);
			}
		}

		public static bool SupportsIPv4 {
			get {
				CheckProtocolSupport();
				return ipv4Supported == 1;
			}
		}

		public static bool SupportsIPv6 {
			get {
				CheckProtocolSupport();
				return ipv6Supported == 1;
			}
		}

#if NET_2_0
		public int SendTimeout {
			get {
				return (int)GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendTimeout);
			}
			set {
				SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendTimeout, value);
			}
		}

		public int ReceiveTimeout {
			get {
				return (int)GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveTimeout);
			}
			set {
				SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveTimeout, value);
			}
		}

		public bool NoDelay {
			get {
				return (int)(GetSocketOption (
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay)) != 0;
			}

			set {
				SetSocketOption (
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay, value ? 1 : 0);
			}
		}
#endif

		internal static void CheckProtocolSupport()
		{
			if(ipv4Supported == -1) {
				try  {
					Socket tmp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					tmp.Close();

					ipv4Supported = 1;
				}
				catch {
					ipv4Supported = 0;
				}
			}

			if(ipv6Supported == -1) {
#if NET_2_0 && CONFIGURATION_DEP
				SettingsSection config;
				config = (SettingsSection) System.Configuration.ConfigurationManager.GetSection ("system.net/settings");
				if (config != null)
					ipv6Supported = config.Ipv6.Enabled ? -1 : 0;
#else
				NetConfig config = (NetConfig)System.Configuration.ConfigurationSettings.GetConfig("system.net/settings");

				if(config != null)
					ipv6Supported = config.ipv6Enabled?-1:0;
#endif
				if(ipv6Supported != 0) {
					try {
						Socket tmp = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
						tmp.Close();

						ipv6Supported = 1;
					}
					catch { }
				}
			}
		}

		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr Accept_internal(IntPtr sock,
							     out int error);

		Thread blocking_thread;
		public Socket Accept() {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error = 0;
			IntPtr sock = (IntPtr) (-1);
			blocking_thread = Thread.CurrentThread;
			try {
				sock = Accept_internal(socket, out error);
			} catch (ThreadAbortException) {
				if (disposed) {
					Thread.ResetAbort ();
					error = (int) SocketError.Interrupted;
				}
			} finally {
				blocking_thread = null;
			}

			if (error != 0) {
				throw new SocketException (error);
			}
			
			Socket accepted = new Socket(this.AddressFamily,
						     this.SocketType,
						     this.ProtocolType, sock);

			accepted.Blocking = this.Blocking;
			return(accepted);
		}

		public IAsyncResult BeginAccept(AsyncCallback callback,
						object state) {

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.Accept);
			Worker worker = new Worker (req);
			SocketAsyncCall sac = new SocketAsyncCall (worker.Accept);
			sac.BeginInvoke (null, req);
			return(req);
		}

		public IAsyncResult BeginConnect(EndPoint end_point,
						 AsyncCallback callback,
						 object state) {

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (end_point == null)
				throw new ArgumentNullException ("end_point");

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.Connect);
			req.EndPoint = end_point;

			// Bug #75154: Connect() should not succeed for .Any addresses.
			if (end_point is IPEndPoint) {
				IPEndPoint ep = (IPEndPoint) end_point;
				if (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any)) {
					req.Complete (new SocketException ((int) SocketError.AddressNotAvailable), true);
					return req;
				}
			}

			int error = 0;
			if (!blocking) {
				SocketAddress serial = end_point.Serialize ();
				Connect_internal (socket, serial, out error);
				if (error == 0) {
					// succeeded synch
					connected = true;
					req.Complete (true);
				} else if (error != (int) SocketError.InProgress && error != (int) SocketError.WouldBlock) {
					// error synch
					connected = false;
					req.Complete (new SocketException (error), true);
				}
			}

			if (blocking || error == (int) SocketError.InProgress || error == (int) SocketError.WouldBlock) {
				// continue asynch
				connected = false;
				Worker worker = new Worker (req);
				SocketAsyncCall sac = new SocketAsyncCall (worker.Connect);
				sac.BeginInvoke (null, req);
			}

			return(req);
		}

		public IAsyncResult BeginReceive(byte[] buffer, int offset,
						 int size,
						 SocketFlags socket_flags,
						 AsyncCallback callback,
						 object state) {

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (size < 0 || offset + size > buffer.Length)
				throw new ArgumentOutOfRangeException ("size");

			SocketAsyncResult req;
			lock (readQ) {
				req = new SocketAsyncResult (this, state, callback, SocketOperation.Receive);
				req.Buffer = buffer;
				req.Offset = offset;
				req.Size = size;
				req.SockFlags = socket_flags;
				readQ.Enqueue (req);
				if (readQ.Count == 1) {
					Worker worker = new Worker (req);
					SocketAsyncCall sac = new SocketAsyncCall (worker.Receive);
					sac.BeginInvoke (null, req);
				}
			}

			return req;
		}

		public IAsyncResult BeginReceiveFrom(byte[] buffer, int offset,
						     int size,
						     SocketFlags socket_flags,
						     ref EndPoint remote_end,
						     AsyncCallback callback,
						     object state) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset must be >= 0");

			if (size < 0)
				throw new ArgumentOutOfRangeException ("size must be >= 0");

			if (offset + size > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset + size exceeds the buffer length");

			SocketAsyncResult req;
			lock (readQ) {
				req = new SocketAsyncResult (this, state, callback, SocketOperation.ReceiveFrom);
				req.Buffer = buffer;
				req.Offset = offset;
				req.Size = size;
				req.SockFlags = socket_flags;
				req.EndPoint = remote_end;
				readQ.Enqueue (req);
				if (readQ.Count == 1) {
					Worker worker = new Worker (req);
					SocketAsyncCall sac = new SocketAsyncCall (worker.ReceiveFrom);
					sac.BeginInvoke (null, req);
				}
			}
			return req;
		}

		public IAsyncResult BeginSend (byte[] buffer, int offset, int size, SocketFlags socket_flags,
					       AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset must be >= 0");

			if (size < 0)
				throw new ArgumentOutOfRangeException ("size must be >= 0");

			if (offset + size > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset + size exceeds the buffer length");

			SocketAsyncResult req;
			lock (writeQ) {
				req = new SocketAsyncResult (this, state, callback, SocketOperation.Send);
				req.Buffer = buffer;
				req.Offset = offset;
				req.Size = size;
				req.SockFlags = socket_flags;
				writeQ.Enqueue (req);
				if (writeQ.Count == 1) {
					Worker worker = new Worker (req);
					SocketAsyncCall sac = new SocketAsyncCall (worker.Send);
					sac.BeginInvoke (null, req);
				}
			}
			return req;
		}

		public IAsyncResult BeginSendTo(byte[] buffer, int offset,
						int size,
						SocketFlags socket_flags,
						EndPoint remote_end,
						AsyncCallback callback,
						object state) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset must be >= 0");

			if (size < 0)
				throw new ArgumentOutOfRangeException ("size must be >= 0");

			if (offset + size > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset + size exceeds the buffer length");

			SocketAsyncResult req;
			lock (writeQ) {
				req = new SocketAsyncResult (this, state, callback, SocketOperation.SendTo);
				req.Buffer = buffer;
				req.Offset = offset;
				req.Size = size;
				req.SockFlags = socket_flags;
				req.EndPoint = remote_end;
				writeQ.Enqueue (req);
				if (writeQ.Count == 1) {
					Worker worker = new Worker (req);
					SocketAsyncCall sac = new SocketAsyncCall (worker.SendTo);
					sac.BeginInvoke (null, req);
				}
			}
			return req;
		}

		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Bind_internal(IntPtr sock,
							 SocketAddress sa,
							 out int error);

		public void Bind(EndPoint local_end) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if(local_end==null) {
				throw new ArgumentNullException("local_end");
			}
			
			int error;
			
			Bind_internal(socket, local_end.Serialize(),
				      out error);

			if (error != 0) {
				throw new SocketException (error);
			}
		}

		// Closes the socket
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Close_internal(IntPtr socket,
							  out int error);
		
		public void Close() {
			((IDisposable) this).Dispose ();
		}

		// Connects to the remote address
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Connect_internal(IntPtr sock,
							    SocketAddress sa,
							    out int error);

		public void Connect(EndPoint remote_end) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if(remote_end==null) {
				throw new ArgumentNullException("remote_end");
			}

			if (remote_end is IPEndPoint) {
				IPEndPoint ep = (IPEndPoint) remote_end;
				if (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any))
					throw new SocketException ((int) SocketError.AddressNotAvailable);
			}

			SocketAddress serial = remote_end.Serialize ();
			int error = 0;

			blocking_thread = Thread.CurrentThread;
			try {
				Connect_internal (socket, serial, out error);
			} catch (ThreadAbortException) {
				if (disposed) {
					Thread.ResetAbort ();
					error = (int) SocketError.Interrupted;
				}
			} finally {
				blocking_thread = null;
			}

			if (error != 0) {
				throw new SocketException (error);
			}
			
			connected=true;
		}
		
		public Socket EndAccept(IAsyncResult result) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (result == null)
				throw new ArgumentNullException ("result");

			SocketAsyncResult req = result as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne();

			req.CheckIfThrowDelayedException();
			return req.Socket;
		}

		public void EndConnect(IAsyncResult result) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (result == null)
				throw new ArgumentNullException ("result");

			SocketAsyncResult req = result as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne();

			req.CheckIfThrowDelayedException();
		}

		public int EndReceive(IAsyncResult result) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (result == null)
				throw new ArgumentNullException ("result");

			SocketAsyncResult req = result as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne();

			req.CheckIfThrowDelayedException();
			return req.Total;
		}

		public int EndReceiveFrom(IAsyncResult result,
					  ref EndPoint end_point) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (result == null)
				throw new ArgumentNullException ("result");

			SocketAsyncResult req = result as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne();

 			req.CheckIfThrowDelayedException();
			end_point = req.EndPoint;
			return req.Total;
		}

		public int EndSend(IAsyncResult result) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (result == null)
				throw new ArgumentNullException ("result");

			SocketAsyncResult req = result as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne();

			req.CheckIfThrowDelayedException();
			return req.Total;
		}

		public int EndSendTo(IAsyncResult result) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (result == null)
				throw new ArgumentNullException ("result");

			SocketAsyncResult req = result as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne();

			req.CheckIfThrowDelayedException();
			return req.Total;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void GetSocketOption_obj_internal(IntPtr socket, SocketOptionLevel level, SocketOptionName name, out object obj_val, out int error);
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void GetSocketOption_arr_internal(IntPtr socket, SocketOptionLevel level, SocketOptionName name, ref byte[] byte_val, out int error);

		public object GetSocketOption (SocketOptionLevel level, SocketOptionName name)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			object obj_val;
			int error;
			
			GetSocketOption_obj_internal(socket, level, name,
						     out obj_val, out error);

			if (error != 0) {
				throw new SocketException (error);
			}
			
			if(name==SocketOptionName.Linger) {
				return((LingerOption)obj_val);
			} else if (name==SocketOptionName.AddMembership ||
				   name==SocketOptionName.DropMembership) {
				return((MulticastOption)obj_val);
			} else if (obj_val is int) {
				return((int)obj_val);
			} else {
				return(obj_val);
			}
		}

		public void GetSocketOption (SocketOptionLevel level, SocketOptionName name, byte [] opt_value)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			
			GetSocketOption_arr_internal(socket, level, name,
						     ref opt_value, out error);

			if (error != 0) {
				throw new SocketException (error);
			}
		}

		public byte [] GetSocketOption (SocketOptionLevel level, SocketOptionName name, int length)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			byte[] byte_val=new byte[length];
			int error;
			
			GetSocketOption_arr_internal(socket, level, name,
						     ref byte_val, out error);

			if (error != 0) {
				throw new SocketException (error);
			}

			return(byte_val);
		}

		// See Socket.IOControl, WSAIoctl documentation in MSDN. The
		// common options between UNIX and Winsock are FIONREAD,
		// FIONBIO and SIOCATMARK. Anything else will depend on the
		// system.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int WSAIoctl (IntPtr sock, int ioctl_code,
					    byte [] input, byte [] output,
					    out int error);

		public int IOControl (int ioctl_code, byte [] in_value, byte [] out_value)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			int result = WSAIoctl (socket, ioctl_code, in_value,
					       out_value, out error);

			if (error != 0) {
				throw new SocketException (error);
			}
			
			if (result == -1)
				throw new InvalidOperationException ("Must use Blocking property instead.");

			return result;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Listen_internal(IntPtr sock,
							   int backlog,
							   out int error);

		public void Listen (int backlog)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			
			Listen_internal(socket, backlog, out error);

			if (error != 0) {
				throw new SocketException (error);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Poll_internal (IntPtr socket, SelectMode mode, int timeout, out int error);

		public bool Poll (int time_us, SelectMode mode)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (mode != SelectMode.SelectRead &&
			    mode != SelectMode.SelectWrite &&
			    mode != SelectMode.SelectError)
				throw new NotSupportedException ("'mode' parameter is not valid.");

			int error;
			bool result = Poll_internal (socket, mode, time_us, out error);
			if (error != 0)
				throw new SocketException (error);

			if (mode == SelectMode.SelectWrite && result == true) {
				/* Update the connected state; for
				 * non-blocking Connect()s this is
				 * when we can find out that the
				 * connect succeeded.
				 */
				if ((int)GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error) == 0) {
					connected = true;
				}
			}
			
			return result;
		}

		/* This overload is needed as the async Connect method
		 * also needs to check the socket error status, but
		 * getsockopt(..., SO_ERROR) clears the error.
		 */
		private bool Poll (int time_us, SelectMode mode,
				   out int socket_error)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (mode != SelectMode.SelectRead &&
			    mode != SelectMode.SelectWrite &&
			    mode != SelectMode.SelectError)
				throw new NotSupportedException ("'mode' parameter is not valid.");

			int error;
			bool result = Poll_internal (socket, mode, time_us, out error);
			if (error != 0)
				throw new SocketException (error);

			socket_error = (int)GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
			
			if (mode == SelectMode.SelectWrite && result == true) {
				/* Update the connected state; for
				 * non-blocking Connect()s this is
				 * when we can find out that the
				 * connect succeeded.
				 */
				if (socket_error == 0) {
					connected = true;
				}
			}
			
			return result;
		}
		
		public int Receive (byte [] buf)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			SocketError error;

			int ret = Receive_nochecks (buf, 0, buf.Length, SocketFlags.None, out error);
			
			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Receive (byte [] buf, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			SocketError error;

			int ret = Receive_nochecks (buf, 0, buf.Length, flags, out error);
			
			if (error != SocketError.Success) {
				if (error == SocketError.WouldBlock && blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException ((int) error, "Operation timed out.");
				throw new SocketException ((int) error);
			}

			return ret;
		}

		public int Receive (byte [] buf, int size, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			if (size < 0 || size > buf.Length)
				throw new ArgumentOutOfRangeException ("size");

			SocketError error;

			int ret = Receive_nochecks (buf, 0, size, flags, out error);
			
			if (error != SocketError.Success) {
				if (error == SocketError.WouldBlock && blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException ((int) error, "Operation timed out.");
				throw new SocketException ((int) error);
			}

			return ret;
		}

		public int Receive (byte [] buf, int offset, int size, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			if (offset < 0 || offset > buf.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (size < 0 || offset + size > buf.Length)
				throw new ArgumentOutOfRangeException ("size");
			
			SocketError error;

			int ret = Receive_nochecks (buf, offset, size, flags, out error);
			
			if (error != SocketError.Success) {
				if (error == SocketError.WouldBlock && blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException ((int) error, "Operation timed out.");
				throw new SocketException ((int) error);
			}

			return ret;
		}

#if NET_2_0
		public int Receive (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			if (offset < 0 || offset > buf.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (size < 0 || offset + size > buf.Length)
				throw new ArgumentOutOfRangeException ("size");
			
			return Receive_nochecks (buf, offset, size, flags, out error);
		}
#endif

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Receive_internal(IntPtr sock,
							   byte[] buffer,
							   int offset,
							   int count,
							   SocketFlags flags,
							   out int error);

		int Receive_nochecks (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			int nativeError;
			int ret = Receive_internal (socket, buf, offset, size, flags, out nativeError);
			error = (SocketError) nativeError;
			if (error != SocketError.Success && error != SocketError.WouldBlock && error != SocketError.InProgress)
				connected = false;
			else
				connected = true;
			
			return ret;
		}
		
		public int ReceiveFrom (byte [] buf, ref EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			return ReceiveFrom_nochecks (buf, 0, buf.Length, SocketFlags.None, ref remote_end);
		}

		public int ReceiveFrom (byte [] buf, SocketFlags flags, ref EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");


			return ReceiveFrom_nochecks (buf, 0, buf.Length, flags, ref remote_end);
		}

		public int ReceiveFrom (byte [] buf, int size, SocketFlags flags,
					ref EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			if (size < 0 || size > buf.Length)
				throw new ArgumentOutOfRangeException ("size");

			return ReceiveFrom_nochecks (buf, 0, size, flags, ref remote_end);
		}


		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int RecvFrom_internal(IntPtr sock,
							    byte[] buffer,
							    int offset,
							    int count,
							    SocketFlags flags,
							    ref SocketAddress sockaddr,
							    out int error);

		public int ReceiveFrom (byte [] buf, int offset, int size, SocketFlags flags,
					ref EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			if (offset < 0 || offset > buf.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (size < 0 || offset + size > buf.Length)
				throw new ArgumentOutOfRangeException ("size");

			return ReceiveFrom_nochecks (buf, offset, size, flags, ref remote_end);
		}

		int ReceiveFrom_nochecks (byte [] buf, int offset, int size, SocketFlags flags,
					  ref EndPoint remote_end)
		{
			SocketAddress sockaddr = remote_end.Serialize();
			int cnt, error;

			cnt = RecvFrom_internal (socket, buf, offset, size, flags, ref sockaddr, out error);

			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress)
					connected = false;
				else if (err == SocketError.WouldBlock && blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException (error, "Operation timed out.");

				throw new SocketException (error);
			}

			connected = true;

			// If sockaddr is null then we're a connection
			// oriented protocol and should ignore the
			// remote_end parameter (see MSDN
			// documentation for Socket.ReceiveFrom(...) )
			
			if ( sockaddr != null ) {
				// Stupidly, EndPoint.Create() is an
				// instance method
				remote_end = remote_end.Create (sockaddr);
			}

			return cnt;
		}

		public int Send (byte [] buf)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			SocketError error;

			int ret = Send_nochecks (buf, 0, buf.Length, SocketFlags.None, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buf, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			SocketError error;

			int ret = Send_nochecks (buf, 0, buf.Length, flags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buf, int size, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			if (size < 0 || size > buf.Length)
				throw new ArgumentOutOfRangeException ("size");

			SocketError error;

			int ret = Send_nochecks (buf, 0, size, flags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buf, int offset, int size, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buf.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (size < 0 || offset + size > buf.Length)
				throw new ArgumentOutOfRangeException ("size");

			SocketError error;

			int ret = Send_nochecks (buf, offset, size, flags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

#if NET_2_0
		public int Send (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buf.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (size < 0 || offset + size > buf.Length)
				throw new ArgumentOutOfRangeException ("size");

			return Send_nochecks (buf, offset, size, flags, out error);
		}
#endif

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Send_internal(IntPtr sock,
							byte[] buf, int offset,
							int count,
							SocketFlags flags,
							out int error);

		int Send_nochecks (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			if (size == 0) {
				error = SocketError.Success;
				return 0;
			}

			int nativeError;

			int ret = Send_internal (socket, buf, offset, size, flags, out nativeError);

			error = (SocketError)nativeError;

			if (error != SocketError.Success && error != SocketError.WouldBlock && error != SocketError.InProgress)
				connected = false;
			else
				connected = true;

			return ret;
		}

		public int SendTo (byte [] buffer, EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			return SendTo_nochecks (buffer, 0, buffer.Length, SocketFlags.None, remote_end);
		}

		public int SendTo (byte [] buffer, SocketFlags flags, EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");
				
			return SendTo_nochecks (buffer, 0, buffer.Length, flags, remote_end);
		}

		public int SendTo (byte [] buffer, int size, SocketFlags flags, EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			if (size < 0 || size > buffer.Length)
				throw new ArgumentOutOfRangeException ("size");

			return SendTo_nochecks (buffer, 0, size, flags, remote_end);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int SendTo_internal(IntPtr sock,
							  byte[] buffer,
							  int offset,
							  int count,
							  SocketFlags flags,
							  SocketAddress sa,
							  out int error);

		public int SendTo (byte [] buffer, int offset, int size, SocketFlags flags,
				   EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remote_end == null)
				throw new ArgumentNullException("remote_end");

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (size < 0 || offset + size > buffer.Length)
				throw new ArgumentOutOfRangeException ("size");

			return SendTo_nochecks (buffer, offset, size, flags, remote_end);
		}

		int SendTo_nochecks (byte [] buffer, int offset, int size, SocketFlags flags,
				   EndPoint remote_end)
		{
			SocketAddress sockaddr = remote_end.Serialize ();

			int ret, error;

			ret = SendTo_internal (socket, buffer, offset, size, flags, sockaddr, out error);

			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress)
					connected = false;

				throw new SocketException (error);
			}

			connected = true;

			return ret;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void SetSocketOption_internal (IntPtr socket, SocketOptionLevel level,
								     SocketOptionName name, object obj_val,
								     byte [] byte_val, int int_val,
								     out int error);

		public void SetSocketOption (SocketOptionLevel level, SocketOptionName name, byte[] opt_value)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			
			SetSocketOption_internal(socket, level, name, null,
						 opt_value, 0, out error);

			if (error != 0) {
				throw new SocketException (error);
			}
		}

		public void SetSocketOption (SocketOptionLevel level, SocketOptionName name, int opt_value)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			
			SetSocketOption_internal(socket, level, name, null,
						 null, opt_value, out error);

			if (error != 0) {
				throw new SocketException (error);
			}
		}

		public void SetSocketOption (SocketOptionLevel level, SocketOptionName name, object opt_value)
		{

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if(opt_value==null) {
				throw new ArgumentNullException();
			}
			
			int error;
			/* From MS documentation on SetSocketOption: "For an
			 * option with a Boolean data type, specify a nonzero
			 * value to enable the option, and a zero value to
			 * disable the option."
			 * Booleans are only handled in 2.0
			 */

			if (opt_value is System.Boolean) {
#if NET_2_0
				bool bool_val = (bool) opt_value;
				int int_val = (bool_val) ? 1 : 0;

				SetSocketOption_internal (socket, level, name, null, null, int_val, out error);
#else
				throw new ArgumentException ("Use an integer 1 (true) or 0 (false) instead of a boolean.", "opt_value");
#endif
			} else {
				SetSocketOption_internal (socket, level, name, opt_value, null, 0, out error);
			}

			if (error != 0)
				throw new SocketException (error);
		}

#if NET_2_0
		public void SetSocketOption (SocketOptionLevel level, SocketOptionName name, bool optionValue)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			int int_val = (optionValue) ? 1 : 0;
			SetSocketOption_internal (socket, level, name, null, null, int_val, out error);
			if (error != 0)
				throw new SocketException (error);
		}
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Shutdown_internal(IntPtr socket, SocketShutdown how, out int error);
		
		public void Shutdown (SocketShutdown how)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			
			Shutdown_internal(socket, how, out error);

			if (error != 0) {
				throw new SocketException (error);
			}
		}

		public override int GetHashCode ()
		{ 
			return (int) socket; 
		}

		protected virtual void Dispose (bool explicitDisposing)
		{
			if (disposed)
				return;

			disposed = true;
			connected = false;
			if ((int) socket != -1) {
				int error;
				closed = true;
				IntPtr x = socket;
				socket = (IntPtr) (-1);
				Close_internal (x, out error);
				if (blocking_thread != null) {
					blocking_thread.Abort ();
					blocking_thread = null;
				}

				if (error != 0)
					throw new SocketException (error);
			}
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
		~Socket () {
			Dispose(false);
		}
	}
}


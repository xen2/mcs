//
// System.Net.WebConnection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.IO;
using System.Collections;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace System.Net
{
	enum ReadState
	{
		None,
		Status,
		Headers,
		Content
	}
	
	class WebConnection
	{
		ServicePoint sPoint;
		Stream nstream;
		Socket socket;
		WebExceptionStatus status;
		WebConnectionGroup group;
		bool busy;
		WaitOrTimerCallback initConn;
		bool keepAlive;
		byte [] buffer;
		static AsyncCallback readDoneDelegate = new AsyncCallback (ReadDone);
		EventHandler abortHandler;
		ReadState readState;
		internal WebConnectionData Data;
		WebConnectionStream prevStream;
		bool chunkedRead;
		ChunkStream chunkStream;
		AutoResetEvent waitForContinue;
		AutoResetEvent goAhead;
		bool waitingForContinue;
		Queue queue;
		bool reused;
		int position;

		bool ssl;
		bool certsAvailable;
		static bool sslCheck;
		static Type sslStream;
		static PropertyInfo piCRL;
		static PropertyInfo piClient;
		static PropertyInfo piServer;

		public WebConnection (WebConnectionGroup group, ServicePoint sPoint)
		{
			this.group = group;
			this.sPoint = sPoint;
			buffer = new byte [4096];
			readState = ReadState.None;
			Data = new WebConnectionData ();
			initConn = new WaitOrTimerCallback (InitConnection);
			abortHandler = new EventHandler (Abort);
			goAhead = new AutoResetEvent (true);
			queue = new Queue (1);
		}

		public void Connect ()
		{
			lock (this) {
				if (socket != null && socket.Connected && status == WebExceptionStatus.Success) {
					reused = true;
					return;
				}

				reused = false;
				if (socket != null) {
					socket.Close();
					socket = null;
				}
				
				chunkStream = null;
				IPHostEntry hostEntry = sPoint.HostEntry;

				if (hostEntry == null) {
					status = sPoint.UsesProxy ? WebExceptionStatus.ProxyNameResolutionFailure :
								    WebExceptionStatus.NameResolutionFailure;
					return;
				}

				foreach (IPAddress address in hostEntry.AddressList) {
					socket = new Socket (address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					try {
						socket.Connect (new IPEndPoint(address, sPoint.Address.Port));
						status = WebExceptionStatus.Success;
						break;
					} catch (SocketException) {
						socket.Close();
						socket = null;
						status = WebExceptionStatus.ConnectFailure;
					}
				}
			}
		}

		bool CreateStream (HttpWebRequest request)
		{
			try {
				NetworkStream serverStream = new NetworkStream (socket, false);
				if (request.RequestUri.Scheme == Uri.UriSchemeHttps) {
					ssl = true;
					if (!sslCheck) {
						lock (typeof (WebConnection)) {
							sslCheck = true;
							// HttpsClientStream is an internal glue class in Mono.Security.dll
							sslStream = Type.GetType ("Mono.Security.Protocol.Tls.HttpsClientStream, Mono.Security, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false);
							if (sslStream != null) {
								piClient = sslStream.GetProperty ("SelectedClientCertificate");
								piServer = sslStream.GetProperty ("ServerCertificate");
							}
						}
					}
					if (sslStream == null)
						throw new NotSupportedException ("Missing Mono.Security.dll assembly. Support for SSL/TLS is unavailable.");

					object[] args = new object [4] { serverStream, request.RequestUri.Host, request.ClientCertificates, request };
					nstream = (Stream) Activator.CreateInstance (sslStream, args);

					// we also need to set ServicePoint.Certificate 
					// and ServicePoint.ClientCertificate but this can
					// only be done later (after handshake - which is
					// done only after a read operation).
				}
				else
					nstream = serverStream;
			} catch (Exception) {
				status = WebExceptionStatus.ConnectFailure;
				return false;
			}

			return true;
		}
		
		void HandleError (WebExceptionStatus st, Exception e)
		{
			status = st;
			lock (this) {
				busy = false;
				if (st == WebExceptionStatus.RequestCanceled)
					Data = new WebConnectionData ();

				status = st;
			}

			if (e == null) { // At least we now where it comes from
				try {
					throw new Exception (new System.Diagnostics.StackTrace ().ToString ());
				} catch (Exception e2) {
					e = e2;
				}
			}

			if (Data != null && Data.request != null)
				Data.request.SetResponseError (st, e);

			Close (true);
		}
		
		internal bool WaitForContinue (byte [] headers, int offset, int size)
		{
			waitingForContinue = sPoint.SendContinue;
			if (waitingForContinue && waitForContinue == null)
				waitForContinue = new AutoResetEvent (false);

			Write (headers, offset, size);
			if (!waitingForContinue)
				return false;

			bool result = waitForContinue.WaitOne (2000, false);
			waitingForContinue = false;
			if (result) {
				sPoint.SendContinue = true;
				if (Data.request.ExpectContinue)
					Data.request.DoContinueDelegate (Data.StatusCode, Data.Headers);
			} else {
				sPoint.SendContinue = false;
			}

			return result;
		}
		
		static void ReadDone (IAsyncResult result)
		{
			WebConnection cnc = (WebConnection) result.AsyncState;
			WebConnectionData data = cnc.Data;
			Stream ns = cnc.nstream;
			if (ns == null) {
				cnc.Close (true);
				return;
			}

			int nread = -1;
			try {
				nread = ns.EndRead (result);
			} catch (Exception e) {
				cnc.status = WebExceptionStatus.ReceiveFailure;
				cnc.HandleError (cnc.status, e);
				return;
			}

			if (nread == 0) {
				cnc.status = WebExceptionStatus.ReceiveFailure;
				cnc.HandleError (cnc.status, null);
				return;
			}

			if (nread < 0) {
				cnc.HandleError (WebExceptionStatus.ServerProtocolViolation, null);
				return;
			}

			//Console.WriteLine (System.Text.Encoding.Default.GetString (cnc.buffer, 0, nread + cnc.position));
			int pos = -1;
			nread += cnc.position;
			if (cnc.readState == ReadState.None) { 
				Exception exc = null;
				try {
					pos = cnc.GetResponse (cnc.buffer, nread);
					if (pos != -1 && data.StatusCode == 100) {
						cnc.readState = ReadState.None;
						InitRead (cnc);
						cnc.sPoint.SendContinue = true;
						if (cnc.waitingForContinue) {
							cnc.waitForContinue.Set ();
						} else if (data.request.ExpectContinue) {
							data.request.DoContinueDelegate (data.StatusCode, data.Headers);
						}

						return;
					}
				} catch (Exception e) {
					exc = e;
				}

				if (exc != null) {
					cnc.HandleError (WebExceptionStatus.ServerProtocolViolation, exc);
					return;
				}
			}

			if (cnc.readState != ReadState.Content) {
				int est = nread * 2;
				int max = (est < cnc.buffer.Length) ? cnc.buffer.Length : est;
				byte [] newBuffer = new byte [max];
				Buffer.BlockCopy (cnc.buffer, 0, newBuffer, 0, nread);
				cnc.buffer = newBuffer;
				cnc.position = nread;
				cnc.readState = ReadState.None;
				InitRead (cnc);
				return;
			}

			cnc.position = 0;

			WebConnectionStream stream = new WebConnectionStream (cnc);

			string contentType = data.Headers ["Transfer-Encoding"];
			cnc.chunkedRead = (contentType != null && contentType.ToLower ().IndexOf ("chunked") != -1);
			if (!cnc.chunkedRead) {
				stream.ReadBuffer = cnc.buffer;
				stream.ReadBufferOffset = pos;
				stream.ReadBufferSize = nread;
			} else if (cnc.chunkStream == null) {
				cnc.chunkStream = new ChunkStream (cnc.buffer, pos, nread, data.Headers);
			} else {
				cnc.chunkStream.ResetBuffer ();
				cnc.chunkStream.Write (cnc.buffer, pos, nread);
			}

			data.stream = stream;
			
			lock (cnc) {
				if (cnc.queue.Count > 0)
					stream.ReadAll ();
				else
				{
					cnc.prevStream = stream;
					stream.CheckComplete ();
				}
			}
			
			data.request.SetResponseData (data);
		}

		internal void GetCertificates () 
		{
			// here the SSL negotiation have been done
			X509Certificate client = (X509Certificate) piClient.GetValue (nstream, null);
			X509Certificate server = (X509Certificate) piServer.GetValue (nstream, null);
			sPoint.SetCertificates (client, server);
			certsAvailable = (server != null);
		}
		
		static void InitRead (object state)
		{
			WebConnection cnc = (WebConnection) state;
			Stream ns = cnc.nstream;

			try {
				int size = cnc.buffer.Length - cnc.position;
				ns.BeginRead (cnc.buffer, cnc.position, size, readDoneDelegate, cnc);
			} catch (Exception e) {
				cnc.HandleError (WebExceptionStatus.ReceiveFailure, e);
			}
		}
		
		int GetResponse (byte [] buffer, int max)
		{
			int pos = 0;
			string line = null;
			bool lineok = false;
			bool isContinue = false;
			do {
				if (readState == ReadState.None) {
					lineok = ReadLine (buffer, ref pos, max, ref line);
					if (!lineok)
						return -1;

					readState = ReadState.Status;

					string [] parts = line.Split (' ');
					if (parts.Length < 2)
						return -1;

					if (String.Compare (parts [0], "HTTP/1.1", true) == 0) {
						Data.Version = HttpVersion.Version11;
						sPoint.SetVersion (HttpVersion.Version11);
					} else {
						Data.Version = HttpVersion.Version10;
						sPoint.SetVersion (HttpVersion.Version10);
					}

					Data.StatusCode = (int) UInt32.Parse (parts [1]);
					if (parts.Length >= 3)
						Data.StatusDescription = String.Join (" ", parts, 2, parts.Length - 2);
					else
						Data.StatusDescription = "";

					if (pos >= max)
						return pos;
				}

				if (readState == ReadState.Status) {
					readState = ReadState.Headers;
					Data.Headers = new WebHeaderCollection ();
					ArrayList headers = new ArrayList ();
					bool finished = false;
					while (!finished) {
						if (ReadLine (buffer, ref pos, max, ref line) == false)
							break;
					
						if (line == null) {
							// Empty line: end of headers
							finished = true;
							continue;
						}
					
						if (line.Length > 0 && (line [0] == ' ' || line [0] == '\t')) {
							int count = headers.Count - 1;
							if (count < 0)
								break;

							string prev = (string) headers [count] + line;
							headers [count] = prev;
						} else {
							headers.Add (line);
						}
					}

					if (!finished)
						return -1;

					foreach (string s in headers)
						Data.Headers.Add (s);

					if (Data.StatusCode == (int) HttpStatusCode.Continue) {
						sPoint.SendContinue = true;
						if (pos >= max)
							return pos;
						if (Data.request.ExpectContinue)
							Data.request.DoContinueDelegate (Data.StatusCode, Data.Headers);
						readState = ReadState.None;
						isContinue = true;
					}
					else {
						readState = ReadState.Content;
						return pos;
					}
				}
			} while (isContinue == true);

			return -1;
		}
		
		void InitConnection (object state, bool notUsed)
		{
			HttpWebRequest request = (HttpWebRequest) state;

			if (status == WebExceptionStatus.RequestCanceled) {
				busy = false;
				Data = new WebConnectionData ();
				goAhead.Set ();
				SendNext ();
				return;
			}

			keepAlive = request.KeepAlive;
			Data = new WebConnectionData ();
			Data.request = request;

			Connect ();
			if (status != WebExceptionStatus.Success) {
				request.SetWriteStreamError (status);
				Close (true);
				return;
			}
			
			if (!CreateStream (request)) {
				request.SetWriteStreamError (status);
				Close (true);
				return;
			}

			readState = ReadState.None;
			request.SetWriteStream (new WebConnectionStream (this, request));
			InitRead (this);
		}
		
		internal EventHandler SendRequest (HttpWebRequest request)
		{
			lock (this) {
				if (prevStream != null && socket != null && socket.Connected) {
					prevStream.ReadAll ();
					prevStream = null;
				}

				if (!busy) {
					busy = true;
					ThreadPool.RegisterWaitForSingleObject (goAhead, initConn,
										request, -1, true);
				} else {
					queue.Enqueue (request);
				}
			}

			return abortHandler;
		}
		
		void SendNext ()
		{
			lock (this) {
				if (queue.Count > 0) {
					prevStream = null;
					SendRequest ((HttpWebRequest) queue.Dequeue ());
				}
			}
		}

		internal void NextRead ()
		{
			lock (this) {
				busy = false;
				string header = (sPoint.UsesProxy) ? "Proxy-Connection" : "Connection";
				string cncHeader = (Data.Headers != null) ? Data.Headers [header] : null;
				bool keepAlive = (Data.Version == HttpVersion.Version11);
				if (cncHeader != null) {
					cncHeader = cncHeader.ToLower ();
					keepAlive = (this.keepAlive && cncHeader.IndexOf ("keep-alive") != -1);
				}

				if ((socket != null && !socket.Connected) ||
				   (!keepAlive || (cncHeader != null && cncHeader.IndexOf ("close") != -1))) {
					Close (false);
				}

				goAhead.Set ();
				if (queue.Count > 0) {
					prevStream = null;
					SendRequest ((HttpWebRequest) queue.Dequeue ());
				}
			}
		}
		
		static bool ReadLine (byte [] buffer, ref int start, int max, ref string output)
		{
			bool foundCR = false;
			StringBuilder text = new StringBuilder ();

			int c = 0;
			while (start < max) {
				c = (int) buffer [start++];

				if (c == '\n') {			// newline
					if ((text.Length > 0) && (text [text.Length - 1] == '\r'))
						text.Length--;

					foundCR = false;
					break;
				} else if (foundCR) {
					text.Length--;
					break;
				}

				if (c == '\r')
					foundCR = true;
					

				text.Append ((char) c);
			}

			if (c != '\n' && c != '\r')
				return false;

			if (text.Length == 0) {
				output = null;
				return (c == '\n' || c == '\r');
			}

			if (foundCR)
				text.Length--;

			output = text.ToString ();
			return true;
		}

		internal IAsyncResult BeginRead (byte [] buffer, int offset, int size, AsyncCallback cb, object state)
		{
			if (nstream == null)
				return null;

			IAsyncResult result = null;
			if (!chunkedRead || chunkStream.WantMore) {
				try {
					result = nstream.BeginRead (buffer, offset, size, cb, state);
				} catch (Exception) {
					status = WebExceptionStatus.ReceiveFailure;
					throw;
				}
			}

			if (chunkedRead) {
				WebAsyncResult wr = new WebAsyncResult (null, null, buffer, offset, size);
				wr.InnerAsyncResult = result;
				return wr;
			}

			return result;
		}
		
		internal int EndRead (IAsyncResult result)
		{
			if (nstream == null)
				return 0;

			if (chunkedRead) {
				WebAsyncResult wr = (WebAsyncResult) result;
				int nbytes = 0;
				if (wr.InnerAsyncResult != null)
					nbytes = nstream.EndRead (wr.InnerAsyncResult);

				chunkStream.WriteAndReadBack (wr.Buffer, wr.Offset, wr.Size, ref nbytes);
				if (nbytes == 0 && chunkStream.WantMore) {
					nbytes = nstream.Read (wr.Buffer, wr.Offset, wr.Size);		
					chunkStream.WriteAndReadBack (wr.Buffer, wr.Offset, wr.Size, ref nbytes);
				}
				return nbytes;
			}

			return nstream.EndRead (result);
		}

		internal IAsyncResult BeginWrite (byte [] buffer, int offset, int size, AsyncCallback cb, object state)
		{
			IAsyncResult result = null;
			if (nstream == null)
				return null;

			try {
				result = nstream.BeginWrite (buffer, offset, size, cb, state);
			} catch (Exception) {
				status = WebExceptionStatus.SendFailure;
				throw;
			}

			return result;
		}

		internal void EndWrite (IAsyncResult result)
		{
			if (nstream != null)
				nstream.EndWrite (result);
		}

		internal int Read (byte [] buffer, int offset, int size)
		{
			if (nstream == null)
				return 0;

			int result = 0;
			try {
				if (!chunkedRead || chunkStream.WantMore)
					result = nstream.Read (buffer, offset, size);

				if (chunkedRead)
					chunkStream.WriteAndReadBack (buffer, offset, size, ref result);
			} catch (Exception e) {
				status = WebExceptionStatus.ReceiveFailure;
				HandleError (status, e);
			}

			return result;
		}

		internal void Write (byte [] buffer, int offset, int size)
		{
			if (nstream == null)
				return;

			try {
				nstream.Write (buffer, offset, size);
				// here SSL handshake should have been done
				if (ssl && !certsAvailable) {
					GetCertificates ();
				}
			} catch (Exception) {
			}
		}

		internal bool TryReconnect ()
		{
			lock (this) {
				if (!reused) {
					HandleError (WebExceptionStatus.SendFailure, null);
					return false;
				}

				Close (false);
				reused = false;
				Connect ();
				if (status != WebExceptionStatus.Success) {
					HandleError (WebExceptionStatus.SendFailure, null);
					return false;
				}
			
				if (!CreateStream (Data.request)) {
					HandleError (WebExceptionStatus.SendFailure, null);
					return false;
				}
			}
			return true;
		}

		void Close (bool sendNext)
		{
			lock (this) {
				busy = false;
				if (nstream != null) {
					try {
						nstream.Close ();
					} catch {}
					nstream = null;
				}

				if (socket != null) {
					try {
						socket.Close ();
					} catch {}
					socket = null;
				}

				if (sendNext) {
					goAhead.Set ();
					SendNext ();
				}
			}
		}

		void Abort (object sender, EventArgs args)
		{
			HandleError (WebExceptionStatus.RequestCanceled, null);
		}

		internal bool Busy {
			get { lock (this) return busy; }
		}
		
		internal bool Connected {
			get {
				lock (this) {
					return (socket != null && socket.Connected);
				}
			}
		}
		
		~WebConnection ()
		{
			Close (false);
		}
	}
}


//
// System.Net.EndPointListener
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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

#if NET_2_0 && SECURITY_DEP

using System.IO;
using System.Net.Sockets;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Mono.Security.Authenticode;

namespace System.Net {
	sealed class EndPointListener
	{
		IPEndPoint endpoint;
		Socket sock;
		ReaderWriterLock plock;
		Hashtable prefixes;  // Dictionary <ListenerPrefix, HttpListener>
		ArrayList unhandled; // List<ListenerPrefix> unhandled; host = '*'
		ArrayList all;       // List<ListenerPrefix> all;  host = '+'
		X509Certificate2 cert;
		AsymmetricAlgorithm key;
		bool secure;
		Hashtable unregistered;

		public EndPointListener (IPAddress addr, int port, bool secure)
		{
			if (secure) {
				this.secure = secure;
				LoadCertificateAndKey (addr, port);
			}

			endpoint = new IPEndPoint (addr, port);
			sock = new Socket (addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			sock.Bind (endpoint);
			sock.Listen (500);
			SocketAsyncEventArgs args = new SocketAsyncEventArgs ();
			args.UserToken = this;
			args.Completed += OnAccept;
			sock.AcceptAsync (args);
			prefixes = new Hashtable ();
			plock = new ReaderWriterLock ();
			unregistered = Hashtable.Synchronized (new Hashtable ());
		}

		void LoadCertificateAndKey (IPAddress addr, int port)
		{
			// Actually load the certificate
			try {
				string dirname = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
				string path = Path.Combine (dirname, ".mono");
				path = Path.Combine (path, "httplistener");
				string cert_file = Path.Combine (path, String.Format ("{0}.cer", port));
				string pvk_file = Path.Combine (path, String.Format ("{0}.pvk", port));
				cert = new X509Certificate2 (cert_file);
				key = PrivateKey.CreateFromFile (pvk_file).RSA;
			} catch {
				// ignore errors
			}
		}

		static void OnAccept (object sender, EventArgs e)
		{
			SocketAsyncEventArgs args = (SocketAsyncEventArgs) e;
			EndPointListener epl = (EndPointListener) args.UserToken;
			Socket accepted = null;
			if (args.SocketError == SocketError.Success) {
				accepted = args.AcceptSocket;
				args.AcceptSocket = null;
			}

			try {
				if (epl.sock != null)
					epl.sock.AcceptAsync (args);
			} catch {
				if (accepted != null) {
					try {
						accepted.Close ();
					} catch {}
					accepted = null;
				}
			} 

			if (accepted == null)
				return;

			if (epl.secure && (epl.cert == null || epl.key == null)) {
				accepted.Close ();
				return;
			}
			HttpConnection conn = new HttpConnection (accepted, epl, epl.secure, epl.cert, epl.key);
			epl.unregistered [conn] = conn;
			conn.BeginReadRequest ();
		}

		internal void RemoveConnection (HttpConnection conn)
		{
			unregistered.Remove (conn);
		}

		public bool BindContext (HttpListenerContext context)
		{
			HttpListenerRequest req = context.Request;
			ListenerPrefix prefix;
			HttpListener listener = SearchListener (req.Url, out prefix);
			if (listener == null)
				return false;

			context.Listener = listener;
			context.Connection.Prefix = prefix;
			listener.RegisterContext (context);
			return true;
		}

		public void UnbindContext (HttpListenerContext context)
		{
			if (context == null || context.Request == null)
				return;

			context.Listener.UnregisterContext (context);
		}

		HttpListener SearchListener (Uri uri, out ListenerPrefix prefix)
		{
			prefix = null;
			if (uri == null)
				return null;

			string host = uri.Host;
			int port = uri.Port;
			string path = HttpUtility.UrlDecode (uri.AbsolutePath);
			string path_slash = path [path.Length - 1] == '/' ? path : path + "/";
			
			HttpListener best_match = null;
			int best_length = -1;

			try {
				plock.AcquireReaderLock (-1);
				if (host != null && host != "") {
					foreach (ListenerPrefix p in prefixes.Keys) {
						string ppath = p.Path;
						if (ppath.Length < best_length)
							continue;

						if (p.Host != host || p.Port != port)
							continue;

						if (path.StartsWith (ppath) || path_slash.StartsWith (ppath)) {
							best_length = ppath.Length;
							best_match = (HttpListener) prefixes [p];
							prefix = p;
						}
					}
					if (best_length != -1)
						return best_match;
				}

				best_match = MatchFromList (host, path, unhandled, out prefix);
				if (best_match != null)
					return best_match;

				best_match = MatchFromList (host, path, all, out prefix);
				if (best_match != null)
					return best_match;
			} finally {
				try {
					plock.ReleaseReaderLock ();
				} catch {}
			}
			return null;
		}

		HttpListener MatchFromList (string host, string path, ArrayList list, out ListenerPrefix prefix)
		{
			prefix = null;
			if (list == null)
				return null;

			HttpListener best_match = null;
			int best_length = -1;
			
			foreach (ListenerPrefix p in list) {
				string ppath = p.Path;
				if (ppath.Length < best_length)
					continue;

				if (path.StartsWith (ppath)) {
					best_length = ppath.Length;
					best_match = p.Listener;
					prefix = p;
				}
			}

			return best_match;
		}

		void AddSpecial (ArrayList coll, ListenerPrefix prefix)
		{
			if (coll == null)
				return;

			try {
				plock.AcquireReaderLock (-1);
				foreach (ListenerPrefix p in coll) {
					if (p.Path == prefix.Path) //TODO: code
						throw new HttpListenerException (400, "Prefix already in use.");
				}
				plock.UpgradeToWriterLock (-1);
				coll.Add (prefix);
			} finally {
				try {
					plock.ReleaseReaderLock (); // This releases the writer lock if held.
				} catch { }
			}
		}

		void RemoveSpecial (ArrayList coll, ListenerPrefix prefix)
		{
			if (coll == null)
				return;

			try {
				plock.AcquireReaderLock (-1);
				int c = coll.Count;
				for (int i = 0; i < c; i++) {
					ListenerPrefix p = (ListenerPrefix) coll [i];
					if (p.Path == prefix.Path) {
						plock.UpgradeToWriterLock (-1);
						coll.RemoveAt (i);
						CheckIfRemove ();
						return;
					}
				}
			} finally {
				try {
					plock.ReleaseReaderLock (); // Releases the writer lock if held
				} catch {}
			}
		}

		// Writer lock held when calling (could use just reader)
		void CheckIfRemove ()
		{
			if (prefixes.Count > 0)
				return;

			if (unhandled != null && unhandled.Count > 0)
				return;

			if (all != null && all.Count > 0)
				return;

			EndPointManager.RemoveEndPoint (this, endpoint);
		}

		public void Close ()
		{
			sock.Close ();
			lock (unregistered) {
				foreach (HttpConnection c in unregistered.Keys)
					c.Close (true);
				unregistered.Clear ();
			}
		}

		public void AddPrefix (ListenerPrefix prefix, HttpListener listener)
		{
			if (prefix.Host == "*") {
				if (unhandled == null)
					unhandled = new ArrayList ();

				prefix.Listener = listener;
				AddSpecial (unhandled, prefix);
				return;
			}

			if (prefix.Host == "+") {
				if (all == null)
					all = new ArrayList ();
				prefix.Listener = listener;
				AddSpecial (all, prefix);
				return;
			}

			try { 
				plock.AcquireReaderLock (-1);
				if (prefixes.ContainsKey (prefix)) {
					HttpListener other = (HttpListener) prefixes [prefix];
					if (other != listener) // TODO: code.
						throw new HttpListenerException (400, "There's another listener for " + prefix);
					return;
				}
				plock.UpgradeToWriterLock (-1);
				prefixes [prefix] = listener;
			} finally {
				try {
					plock.ReleaseReaderLock ();
				} catch {}
			}
		}

		public void RemovePrefix (ListenerPrefix prefix, HttpListener listener)
		{
			if (prefix.Host == "*") {
				RemoveSpecial (unhandled, prefix);
				return;
			}

			if (prefix.Host == "+") {
				RemoveSpecial (all, prefix);
				return;
			}

			try {
				plock.AcquireReaderLock (-1);
				if (prefixes.ContainsKey (prefix)) {
					plock.UpgradeToWriterLock (-1);
					prefixes.Remove (prefix);
					CheckIfRemove ();
				}
			} finally {
				try {
					plock.ReleaseReaderLock ();
				} catch {}
			}
		}
	}
}
#endif


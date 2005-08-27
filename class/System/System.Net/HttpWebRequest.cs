//
// System.Net.HttpWebRequest
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Lawrence Pit
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
using System.Collections;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace System.Net 
{
	[Serializable]
	public class HttpWebRequest : WebRequest, ISerializable
	{
		Uri requestUri;
		Uri actualUri;
		bool hostChanged;
		bool allowAutoRedirect = true;
		bool allowBuffering = true;
		X509CertificateCollection certificates;
		string connectionGroup;
		long contentLength = -1;
		HttpContinueDelegate continueDelegate;
		CookieContainer cookieContainer;
		ICredentials credentials;
		bool haveResponse;		
		bool haveRequest;
		bool requestSent;
		WebHeaderCollection webHeaders = new WebHeaderCollection (true);
		bool keepAlive = true;
		int maxAutoRedirect = 50;
		string mediaType = String.Empty;
		string method = "GET";
		string initialMethod = "GET";
		bool pipelined = true;
		bool preAuthenticate;
		bool usedPreAuth;
		Version version = HttpVersion.Version11;
		Version actualVersion;
		IWebProxy proxy;
		bool sendChunked;
		ServicePoint servicePoint;
		int timeout = 100000;
		
		WebConnectionStream writeStream;
		HttpWebResponse webResponse;
		WebAsyncResult asyncWrite;
		WebAsyncResult asyncRead;
		EventHandler abortHandler;
		bool aborted;
		bool gotRequestStream;
		int redirects;
		bool expectContinue;
		bool authCompleted;
		byte[] bodyBuffer;
		int bodyBufferLength;
		bool getResponseCalled;
		Exception saved_exc;
		object locker = new object ();
#if NET_1_1
		int maxResponseHeadersLength;
		static int defaultMaxResponseHeadersLength;
		int readWriteTimeout = 300000; // ms
		
		// Constructors
		static HttpWebRequest ()
		{
			NetConfig config = ConfigurationSettings.GetConfig ("system.net/settings") as NetConfig;
			defaultMaxResponseHeadersLength = 64 * 1024;
			if (config != null) {
				int x = config.MaxResponseHeadersLength;
				if (x != -1)
					x *= 64;

				defaultMaxResponseHeadersLength = x;
			}
		}
#endif
		
		internal HttpWebRequest (Uri uri) 
		{
			this.requestUri = uri;
			this.actualUri = uri;
			this.proxy = GlobalProxySelection.Select;
		}		
		
		protected HttpWebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			SerializationInfo info = serializationInfo;

			requestUri = (Uri) info.GetValue ("requestUri", typeof (Uri));
			actualUri = (Uri) info.GetValue ("actualUri", typeof (Uri));
			allowAutoRedirect = info.GetBoolean ("allowAutoRedirect");
			allowBuffering = info.GetBoolean ("allowBuffering");
			certificates = (X509CertificateCollection) info.GetValue ("certificates", typeof (X509CertificateCollection));
			connectionGroup = info.GetString ("connectionGroup");
			contentLength = info.GetInt64 ("contentLength");
			webHeaders = (WebHeaderCollection) info.GetValue ("webHeaders", typeof (WebHeaderCollection));
			keepAlive = info.GetBoolean ("keepAlive");
			maxAutoRedirect = info.GetInt32 ("maxAutoRedirect");
			mediaType = info.GetString ("mediaType");
			method = info.GetString ("method");
			initialMethod = info.GetString ("initialMethod");
			pipelined = info.GetBoolean ("pipelined");
			version = (Version) info.GetValue ("version", typeof (Version));
			proxy = (IWebProxy) info.GetValue ("proxy", typeof (IWebProxy));
			sendChunked = info.GetBoolean ("sendChunked");
			timeout = info.GetInt32 ("timeout");
			redirects = info.GetInt32 ("redirects");
		}
		
		// Properties
		
		public string Accept {
			get { return webHeaders ["Accept"]; }
			set {
				CheckRequestStarted ();
				webHeaders.RemoveAndAdd ("Accept", value);
			}
		}
		
		public Uri Address {
			get { return actualUri; }
		}
		
		public bool AllowAutoRedirect {
			get { return allowAutoRedirect; }
			set { this.allowAutoRedirect = value; }
		}
		
		public bool AllowWriteStreamBuffering {
			get { return allowBuffering; }
			set { allowBuffering = value; }
		}
		
		internal bool InternalAllowBuffering {
			get {
				return (allowBuffering && (method == "PUT" || method == "POST"));
			}
		}
		
		public X509CertificateCollection ClientCertificates {
			get {
				if (certificates == null)
					certificates = new X509CertificateCollection ();

				return certificates;
			}
		}
		
		public string Connection {
			get { return webHeaders ["Connection"]; }
			set {
				CheckRequestStarted ();
				string val = value;
				if (val != null) 
					val = val.Trim ().ToLower ();

				if (val == null || val.Length == 0) {
					webHeaders.RemoveInternal ("Connection");
					return;
				}

				if (val == "keep-alive" || val == "close") 
					throw new ArgumentException ("Keep-Alive and Close may not be set with this property");

				if (keepAlive && val.IndexOf ("keep-alive") == -1)
					value = value + ", Keep-Alive";
				
				webHeaders.RemoveAndAdd ("Connection", value);
			}
		}		
		
		public override string ConnectionGroupName { 
			get { return connectionGroup; }
			set { connectionGroup = value; }
		}
		
		public override long ContentLength { 
			get { return contentLength; }
			set { 
				CheckRequestStarted ();
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "Content-Length must be >= 0");
					
				contentLength = value;
			}
		}
		
		internal long InternalContentLength {
			set { contentLength = value; }
		}
		
		public override string ContentType { 
			get { return webHeaders ["Content-Type"]; }
			set {
				if (value == null || value.Trim().Length == 0) {
					webHeaders.RemoveInternal ("Content-Type");
					return;
				}
				webHeaders.RemoveAndAdd ("Content-Type", value);
			}
		}
		
		public HttpContinueDelegate ContinueDelegate {
			get { return continueDelegate; }
			set { continueDelegate = value; }
		}
		
		public CookieContainer CookieContainer {
			get { return cookieContainer; }
			set { cookieContainer = value; }
		}
		
		public override ICredentials Credentials { 
			get { return credentials; }
			set { credentials = value; }
		}
		
		public string Expect {
			get { return webHeaders ["Expect"]; }
			set {
				CheckRequestStarted ();
				string val = value;
				if (val != null)
					val = val.Trim ().ToLower ();

				if (val == null || val.Length == 0) {
					webHeaders.RemoveInternal ("Expect");
					return;
				}

				if (val == "100-continue")
					throw new ArgumentException ("100-Continue cannot be set with this property.",
								     "value");
				webHeaders.RemoveAndAdd ("Expect", value);
			}
		}
		
		public bool HaveResponse {
			get { return haveResponse; }
		}
		
		public override WebHeaderCollection Headers { 
			get { return webHeaders; }
			set {
				CheckRequestStarted ();
				WebHeaderCollection newHeaders = new WebHeaderCollection (true);
				int count = value.Count;
				for (int i = 0; i < count; i++) 
					newHeaders.Add (value.GetKey (i), value.Get (i));

				webHeaders = newHeaders;
			}
		}
		
		public DateTime IfModifiedSince {
			get { 
				string str = webHeaders ["If-Modified-Since"];
				if (str == null)
					return DateTime.Now;
				try {
					return MonoHttpDate.Parse (str);
				} catch (Exception) {
					return DateTime.Now;
				}
			}
			set {
				CheckRequestStarted ();
				// rfc-1123 pattern
				webHeaders.SetInternal ("If-Modified-Since", 
					value.ToUniversalTime ().ToString ("r", null));
				// TODO: check last param when using different locale
			}
		}

		public bool KeepAlive {		
			get {
				return keepAlive;
			}
			set {
				keepAlive = value;
			}
		}
		
		public int MaximumAutomaticRedirections {
			get { return maxAutoRedirect; }
			set {
				if (value <= 0)
					throw new ArgumentException ("Must be > 0", "value");

				maxAutoRedirect = value;
			}			
		}

#if NET_1_1
		[MonoTODO ("Use this")]
		public int MaximumResponseHeadersLength {
			get { return maxResponseHeadersLength; }
			set { maxResponseHeadersLength = value; }
		}

		[MonoTODO ("Use this")]
		public static int DefaultMaximumResponseHeadersLength {
			get { return defaultMaxResponseHeadersLength; }
			set { defaultMaxResponseHeadersLength = value; }
		}

		public
#else
		internal
#endif
		int ReadWriteTimeout {
			get { return readWriteTimeout; }
			set {
				if (requestSent)
					throw new InvalidOperationException ("The request has already been sent.");

				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "Must be >= -1");

				readWriteTimeout = value;
			}
		}
		
		public string MediaType {
			get { return mediaType; }
			set { 
				mediaType = value;
			}
		}
		
		public override string Method { 
			get { return this.method; }
			set { 
				if (value == null || value.Trim () == "")
					throw new ArgumentException ("not a valid method");

				method = value;
			}
		}
		
		public bool Pipelined {
			get { return pipelined; }
			set { pipelined = value; }
		}		
		
		public override bool PreAuthenticate { 
			get { return preAuthenticate; }
			set { preAuthenticate = value; }
		}
		
		public Version ProtocolVersion {
			get { return version; }
			set { 
				if (value != HttpVersion.Version10 && value != HttpVersion.Version11)
					throw new ArgumentException ("value");

				version = value; 
			}
		}
		
		public override IWebProxy Proxy { 
			get { return proxy; }
			set { 
				CheckRequestStarted ();
				if (value == null)
					throw new ArgumentNullException ("value");

				proxy = value;
				servicePoint = null; // we may need a new one
			}
		}
		
		public string Referer {
			get { return webHeaders ["Referer"]; }
			set {
				CheckRequestStarted ();
				if (value == null || value.Trim().Length == 0) {
					webHeaders.RemoveInternal ("Referer");
					return;
				}
				webHeaders.SetInternal ("Referer", value);
			}
		}

		public override Uri RequestUri { 
			get { return requestUri; }
		}
		
		public bool SendChunked {
			get { return sendChunked; }
			set {
				CheckRequestStarted ();
				sendChunked = value;
			}
		}
		
		public ServicePoint ServicePoint {
			get { return GetServicePoint (); }
		}
		
		public override int Timeout { 
			get { return timeout; }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");

				timeout = value;
			}
		}
		
		public string TransferEncoding {
			get { return webHeaders ["Transfer-Encoding"]; }
			set {
				CheckRequestStarted ();
				string val = value;
				if (val != null)
					val = val.Trim ().ToLower ();

				if (val == null || val.Length == 0) {
					webHeaders.RemoveInternal ("Transfer-Encoding");
					return;
				}

				if (val == "chunked")
					throw new ArgumentException ("Chunked encoding must be set with the SendChunked property");

				if (!sendChunked)
					throw new ArgumentException ("SendChunked must be True", "value");

				webHeaders.RemoveAndAdd ("Transfer-Encoding", value);
			}
		}
		
		public string UserAgent {
			get { return webHeaders ["User-Agent"]; }
			set { webHeaders.SetInternal ("User-Agent", value); }
		}

#if NET_1_1
		[MonoTODO]
		public bool UnsafeAuthenticatedConnectionSharing
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif

		internal bool GotRequestStream {
			get { return gotRequestStream; }
		}

		internal bool ExpectContinue {
			get { return expectContinue; }
			set { expectContinue = value; }
		}
		
		internal Uri AuthUri {
			get { return actualUri; }
		}
		
		internal bool ProxyQuery {
			get { return servicePoint.UsesProxy && !servicePoint.UseConnect; }
		}
		
		// Methods
		
		internal ServicePoint GetServicePoint ()
		{
			lock (locker) {
				if (hostChanged || servicePoint == null) {
					servicePoint = ServicePointManager.FindServicePoint (actualUri, proxy);
					hostChanged = false;
				}
			}

			return servicePoint;
		}
		
		public void AddRange (int range)
		{
			AddRange ("bytes", range);
		}
		
		public void AddRange (int from, int to)
		{
			AddRange ("bytes", from, to);
		}
		
		public void AddRange (string rangeSpecifier, int range)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			string value = webHeaders ["Range"];
			if (value == null || value.Length == 0) 
				value = rangeSpecifier + "=";
			else if (value.ToLower ().StartsWith (rangeSpecifier.ToLower () + "="))
				value += ",";
			else
				throw new InvalidOperationException ("rangeSpecifier");
			webHeaders.RemoveAndAdd ("Range", value + range + "-");	
		}
		
		public void AddRange (string rangeSpecifier, int from, int to)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			if (from < 0 || to < 0 || from > to)
				throw new ArgumentOutOfRangeException ();			
			string value = webHeaders ["Range"];
			if (value == null || value.Length == 0) 
				value = rangeSpecifier + "=";
			else if (value.ToLower ().StartsWith (rangeSpecifier.ToLower () + "="))
				value += ",";
			else
				throw new InvalidOperationException ("rangeSpecifier");
			webHeaders.RemoveAndAdd ("Range", value + from + "-" + to);	
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		void CommonChecks (bool putpost)
		{
			if (method == null)
				throw new ProtocolViolationException ("Method is null.");

			if (putpost && ((!keepAlive || (contentLength == -1 && !sendChunked)) && !allowBuffering))
				throw new ProtocolViolationException ("Content-Length not set");

			string transferEncoding = TransferEncoding;
			if (!sendChunked && transferEncoding != null && transferEncoding.Trim () != "")
				throw new ProtocolViolationException ("SendChunked should be true.");
		}

		public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			if (aborted)
				throw new WebException ("The request was previosly aborted.");

			bool send = !(method == "GET" || method == "CONNECT" || method == "HEAD");
			if (method == null || !send)
				throw new ProtocolViolationException ("Cannot send data when method is: " + method);

			CommonChecks (send);
			
			lock (locker)
			{
				if (asyncWrite != null) {
					throw new InvalidOperationException ("Cannot re-call start of asynchronous " +
								"method while a previous call is still in progress.");
				}
	
				asyncWrite = new WebAsyncResult (this, callback, state);
				initialMethod = method;
				if (haveRequest) {
					if (writeStream != null) {
						asyncWrite.SetCompleted (true, writeStream);
						asyncWrite.DoCallback ();
						return asyncWrite;
					}
				}
				
				gotRequestStream = true;
				WebAsyncResult result = asyncWrite;
				if (!requestSent) {
					requestSent = true;
					redirects = 0;
					servicePoint = GetServicePoint ();
					abortHandler = servicePoint.SendRequest (this, connectionGroup);
				}
				return result;
			}
		}

		public override Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			WebAsyncResult result = asyncResult as WebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult");

			asyncWrite = result;
			result.WaitUntilComplete ();

			Exception e = result.Exception;
			if (e != null)
				throw e;

			return result.WriteStream;
		}
		
		public override Stream GetRequestStream()
		{
			IAsyncResult asyncResult = BeginGetRequestStream (null, null);
			asyncWrite = (WebAsyncResult) asyncResult;
			if (!asyncResult.AsyncWaitHandle.WaitOne (timeout, false)) {
				Abort ();
				throw new WebException ("The request timed out", WebExceptionStatus.Timeout);
			}

			return EndGetRequestStream (asyncResult);
		}
		
		public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			bool send = (method == "PUT" || method == "POST");
			if (send) {
				if ((!KeepAlive || (ContentLength == -1 && !SendChunked)) && !AllowWriteStreamBuffering)
					throw new ProtocolViolationException ("Content-Length not set");
			}

			CommonChecks (send);
			Monitor.Enter (this);
			getResponseCalled = true;
			if (asyncRead != null && !haveResponse) {
				Monitor.Exit (this);
				throw new InvalidOperationException ("Cannot re-call start of asynchronous " +
							"method while a previous call is still in progress.");
			}

			asyncRead = new WebAsyncResult (this, callback, state);
			initialMethod = method;
			if (haveResponse) {
				if (webResponse != null) {
					Exception saved = saved_exc;
					Monitor.Exit (this);
					if (saved == null) {
						asyncRead.SetCompleted (true, webResponse);
					} else {
						asyncRead.SetCompleted (true, saved);
					}
					asyncRead.DoCallback ();
					return asyncRead;
				}
			}
			
			if (!requestSent) {
				requestSent = true;
				redirects = 0;
				servicePoint = GetServicePoint ();
				abortHandler = servicePoint.SendRequest (this, connectionGroup);
			}

			Monitor.Exit (this);
			return asyncRead;
		}
		
		public override WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			WebAsyncResult result = asyncResult as WebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			if (!result.WaitUntilComplete (timeout, false)) {
				Abort ();
				throw new WebException("The request timed out", WebExceptionStatus.Timeout);
			}

			if (result.GotException)
				throw result.Exception;

			return result.Response;
		}

		public override WebResponse GetResponse()
		{
			WebAsyncResult result = (WebAsyncResult) BeginGetResponse (null, null);
			return EndGetResponse (result);
		}
		
		public override void Abort ()
		{
			haveResponse = true;
			aborted = true;
			if (asyncWrite != null) {
				WebAsyncResult r = asyncWrite;
				WebException wexc = new WebException ("Aborted.", WebExceptionStatus.RequestCanceled); 
				r.SetCompleted (false, wexc);
				r.DoCallback ();
				asyncWrite = null;
			}			

			if (asyncRead != null) {
				WebAsyncResult r = asyncRead;
				WebException wexc = new WebException ("Aborted.", WebExceptionStatus.RequestCanceled); 
				r.SetCompleted (false, wexc);
				r.DoCallback ();
				asyncRead = null;
			}			

			if (abortHandler != null) {
				try {
					abortHandler (this, EventArgs.Empty);
				} catch (Exception) {}
				abortHandler = null;
			}

			if (writeStream != null) {
				try {
					writeStream.Close ();
					writeStream = null;
				} catch {}
			}

			if (webResponse != null) {
				try {
					webResponse.Close ();
					webResponse = null;
				} catch {}
			}
		}		
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			SerializationInfo info = serializationInfo;

			info.AddValue ("requestUri", requestUri, typeof (Uri));
			info.AddValue ("actualUri", actualUri, typeof (Uri));
			info.AddValue ("allowAutoRedirect", allowAutoRedirect);
			info.AddValue ("allowBuffering", allowBuffering);
			info.AddValue ("certificates", certificates, typeof (X509CertificateCollection));
			info.AddValue ("connectionGroup", connectionGroup);
			info.AddValue ("contentLength", contentLength);
			info.AddValue ("webHeaders", webHeaders, typeof (WebHeaderCollection));
			info.AddValue ("keepAlive", keepAlive);
			info.AddValue ("maxAutoRedirect", maxAutoRedirect);
			info.AddValue ("mediaType", mediaType);
			info.AddValue ("method", method);
			info.AddValue ("initialMethod", initialMethod);
			info.AddValue ("pipelined", pipelined);
			info.AddValue ("version", version, typeof (Version));
			info.AddValue ("proxy", proxy, typeof (IWebProxy));
			info.AddValue ("sendChunked", sendChunked);
			info.AddValue ("timeout", timeout);
			info.AddValue ("redirects", redirects);
		}
		
		void CheckRequestStarted () 
		{
			if (requestSent)
				throw new InvalidOperationException ("request started");
		}

		internal void DoContinueDelegate (int statusCode, WebHeaderCollection headers)
		{
			if (continueDelegate != null)
				continueDelegate (statusCode, headers);
		}
		
		bool Redirect (WebAsyncResult result, HttpStatusCode code)
		{
			redirects++;
			Exception e = null;
			string uriString = null;

			switch (code) {
			case HttpStatusCode.Ambiguous: // 300
				e = new WebException ("Ambiguous redirect.");
				break;
			case HttpStatusCode.MovedPermanently: // 301
			case HttpStatusCode.Redirect: // 302
			case HttpStatusCode.TemporaryRedirect: // 307
				/* MS follows the redirect for POST too
				if (method != "GET" && method != "HEAD") // 10.3
					return false;
				*/

				uriString = webResponse.Headers ["Location"];
				break;
			case HttpStatusCode.SeeOther: //303
				method = "GET";
				uriString = webResponse.Headers ["Location"];
				break;
			case HttpStatusCode.NotModified: // 304
				return false;
			case HttpStatusCode.UseProxy: // 305
				e = new NotImplementedException ("Proxy support not available.");
				break;
			case HttpStatusCode.Unused: // 306
			default:
				e = new ProtocolViolationException ("Invalid status code: " + (int) code);
				break;
			}

			if (e != null)
				throw e;

			if (uriString == null)
				throw new WebException ("No Location header found for " + (int) code,
							WebExceptionStatus.ProtocolError);

			Uri prev = actualUri;
			try {
				actualUri = new Uri (actualUri, uriString);
			} catch (Exception) {
				throw new WebException (String.Format ("Invalid URL ({0}) for {1}",
									uriString, (int) code),
									WebExceptionStatus.ProtocolError);
			}

			hostChanged = (actualUri.Scheme != prev.Scheme || actualUri.Host != prev.Host ||
					actualUri.Port != prev.Port);
			return true;
		}

		string GetHeaders ()
		{
			bool continue100 = false;
			if (contentLength != -1) {
				continue100 = true;
				webHeaders.SetInternal ("Content-Length", contentLength.ToString ());
				webHeaders.RemoveInternal ("Transfer-Encoding");
			} else if (sendChunked) {
				continue100 = true;
				webHeaders.RemoveAndAdd ("Transfer-Encoding", "chunked");
				webHeaders.RemoveInternal ("Content-Length");
			}

			if (actualVersion == HttpVersion.Version11 && continue100 &&
			    servicePoint.SendContinue) { // RFC2616 8.2.3
				webHeaders.RemoveAndAdd ("Expect" , "100-continue");
				expectContinue = true;
			} else {
				webHeaders.RemoveInternal ("Expect");
				expectContinue = false;
			}

			string connectionHeader = (ProxyQuery) ? "Proxy-Connection" : "Connection";
			webHeaders.RemoveInternal ((!ProxyQuery) ? "Proxy-Connection" : "Connection");
			bool spoint10 = (servicePoint.ProtocolVersion == null ||
					 servicePoint.ProtocolVersion == HttpVersion.Version10);

			if (keepAlive && (version == HttpVersion.Version10 || spoint10)) {
				webHeaders.RemoveAndAdd (connectionHeader, "keep-alive");
			} else if (!keepAlive && version == HttpVersion.Version11) {
				webHeaders.RemoveAndAdd (connectionHeader, "close");
			}

			webHeaders.SetInternal ("Host", actualUri.Authority);
			if (cookieContainer != null) {
				string cookieHeader = cookieContainer.GetCookieHeader (requestUri);
				if (cookieHeader != "")
					webHeaders.SetInternal ("Cookie", cookieHeader);
			}

			if (!usedPreAuth && preAuthenticate)
				DoPreAuthenticate ();

			return webHeaders.ToString ();
		}

		void DoPreAuthenticate ()
		{
			webHeaders.RemoveInternal ("Proxy-Authorization");
			webHeaders.RemoveInternal ("Authorization");
			bool isProxy = (proxy != null && !proxy.IsBypassed (actualUri));
			ICredentials creds = (!isProxy || credentials != null) ? credentials : proxy.Credentials;
			Authorization auth = AuthenticationManager.PreAuthenticate (this, creds);
			if (auth == null)
				return;

			string authHeader = (isProxy && credentials == null) ? "Proxy-Authorization" : "Authorization";
			webHeaders [authHeader] = auth.Message;
			usedPreAuth = true;
		}
		
		internal void SetWriteStreamError (WebExceptionStatus status)
		{
			if (aborted)
				return;

			WebAsyncResult r = asyncWrite;
			if (r == null)
				r = asyncRead;

			if (r != null) {
				r.SetCompleted (false, new WebException ("Error: " + status, status));
				r.DoCallback ();
			}
		}

		internal void SendRequestHeaders ()
		{
			StringBuilder req = new StringBuilder ();
			string query;
			if (!ProxyQuery) {
				query = actualUri.PathAndQuery;
			} else if (actualUri.IsDefaultPort) {
				query = String.Format ("{0}://{1}{2}",  actualUri.Scheme,
									actualUri.Host,
									actualUri.PathAndQuery);
			} else {
				query = String.Format ("{0}://{1}:{2}{3}", actualUri.Scheme,
									   actualUri.Host,
									   actualUri.Port,
									   actualUri.PathAndQuery);
			}
			
			if (servicePoint.ProtocolVersion != null && servicePoint.ProtocolVersion < version) {
				actualVersion = servicePoint.ProtocolVersion;
			} else {
				actualVersion = version;
			}

			req.AppendFormat ("{0} {1} HTTP/{2}.{3}\r\n", method, query,
								actualVersion.Major, actualVersion.Minor);
			req.Append (GetHeaders ());
			string reqstr = req.ToString ();
			byte [] bytes = Encoding.UTF8.GetBytes (reqstr);
			writeStream.SetHeaders (bytes, 0, bytes.Length);
		}

		internal void SetWriteStream (WebConnectionStream stream)
		{
			if (aborted)
				return;
			
			writeStream = stream;
			if (bodyBuffer != null) {
				webHeaders.RemoveInternal ("Transfer-Encoding");
				contentLength = bodyBufferLength;
				writeStream.SendChunked = false;
			}
			
			SendRequestHeaders ();

			haveRequest = true;
			
			if (bodyBuffer != null) {
				// The body has been written and buffered. The request "user"
				// won't write it again, so we must do it.
				writeStream.Write (bodyBuffer, 0, bodyBufferLength);
				bodyBuffer = null;
				writeStream.Close ();
			} else if (method == "PUT" || method == "POST") {
				if (getResponseCalled && !writeStream.RequestWritten)
					writeStream.WriteRequest ();
			}

			if (asyncWrite != null) {
				asyncWrite.SetCompleted (false, stream);
				asyncWrite.DoCallback ();
				asyncWrite = null;
			}
		}

		internal void SetResponseError (WebExceptionStatus status, Exception e, string where)
		{
			if (aborted)
				return;
			string msg = String.Format ("Error getting response stream ({0}): {1}", where, status);
			WebAsyncResult r = asyncRead;
			if (r == null)
				r = asyncWrite;

			if (r != null) {
				WebException wexc;
				if (e is WebException) {
					wexc = (WebException) e;
				} else {
					wexc = new WebException (msg, e, status, null); 
				}
				r.SetCompleted (false, wexc);
				r.DoCallback ();
				asyncRead = null;
				asyncWrite = null;
			}
		}

		void CheckSendError (WebConnectionData data)
		{
			// Got here, but no one called GetResponse
			if (data.StatusCode < 400)
				return;

			if (writeStream != null && asyncRead == null && !writeStream.CompleteRequestWritten) {
				// The request has not been completely sent and we got here!
				// We should probably just close and cause an error in any case,
				saved_exc = new WebException (data.StatusDescription, null, WebExceptionStatus.ProtocolError, webResponse); 
				webResponse.ReadAll ();
			}
		}

		internal void SetResponseData (WebConnectionData data)
		{
			if (aborted) {
				if (data.stream != null)
					data.stream.Close ();
				return;
			}

			WebException wexc = null;
			try {
				webResponse = new HttpWebResponse (actualUri, method, data, cookieContainer);
				haveResponse = true;
			} catch (Exception e) {
				wexc = new WebException (e.Message, e, WebExceptionStatus.ProtocolError, null); 
				if (data.stream != null)
					data.stream.Close ();
			}

			if (wexc == null && (method == "POST" || method == "PUT")) {
				lock (locker) {
					CheckSendError (data);
					if (saved_exc != null)
						wexc = (WebException) saved_exc;
				}
			}

			WebAsyncResult r = asyncRead;
			if (r != null) {
				if (wexc != null) {
					r.SetCompleted (false, wexc);
					r.DoCallback ();
					return;
				}

				bool redirected;
				try {
					redirected = CheckFinalStatus (r);
					if (!redirected) {
						r.SetCompleted (false, webResponse);
						r.DoCallback ();
					} else {
						if (webResponse != null) {
							webResponse.Close ();
							webResponse = null;
						}
						haveResponse = false;
						webResponse = null;
						r.Reset ();
						servicePoint = GetServicePoint ();
						abortHandler = servicePoint.SendRequest (this, connectionGroup);
					}
				} catch (WebException wexc2) {
					r.SetCompleted (false, wexc2);
					r.DoCallback ();
					return;
				} catch (Exception ex) {
					wexc = new WebException (ex.Message, ex, WebExceptionStatus.ProtocolError, null); 
					r.SetCompleted (false, wexc);
					r.DoCallback ();
					return;
				}
			}
		}

		bool CheckAuthorization (WebResponse response, HttpStatusCode code)
		{
			authCompleted = false;
			if (code == HttpStatusCode.Unauthorized && credentials == null)
				return false;

			bool isProxy = (code == HttpStatusCode.ProxyAuthenticationRequired);
			if (isProxy && (proxy == null || proxy.Credentials == null))
				return false;

			string authHeader = response.Headers [(isProxy) ? "Proxy-Authenticate" : "WWW-Authenticate"];
			if (authHeader == null)
				return false;

			ICredentials creds = (!isProxy) ? credentials : proxy.Credentials;
			Authorization auth = AuthenticationManager.Authenticate (authHeader, this, creds);
			if (auth == null)
				return false;

			webHeaders [(isProxy) ? "Proxy-Authorization" : "Authorization"] = auth.Message;
			authCompleted = auth.Complete;
			return true;
		}

		// Returns true if redirected
		bool CheckFinalStatus (WebAsyncResult result)
		{
			if (result.GotException)
				throw result.Exception;

			Exception throwMe = result.Exception;
			bodyBuffer = null;

			HttpWebResponse resp = result.Response;
			WebExceptionStatus protoError = WebExceptionStatus.ProtocolError;
			HttpStatusCode code = 0;
			if (throwMe == null && webResponse != null) {
				code  = webResponse.StatusCode;
				if (!authCompleted && ((code == HttpStatusCode.Unauthorized && credentials != null) ||
							code == HttpStatusCode.ProxyAuthenticationRequired)) {
					if (!usedPreAuth && CheckAuthorization (webResponse, code)) {
						// Keep the written body, so it can be rewritten in the retry
						if (InternalAllowBuffering) {
							bodyBuffer = writeStream.WriteBuffer;
							bodyBufferLength = writeStream.WriteBufferLength;
							webResponse.Close ();
							return true;
						} else if (method != "PUT" && method != "POST") {
							webResponse.Close ();
							return true;
						}
						
						writeStream.InternalClose ();
						writeStream = null;
						webResponse.Close ();
						webResponse = null;

						throw new WebException ("This request requires buffering " +
									"of data for authentication or " +
									"redirection to be sucessful.");
					}
				}

				if ((int) code >= 400) {
					string err = String.Format ("The remote server returned an error: ({0}) {1}.",
								    (int) code, webResponse.StatusDescription);
					throwMe = new WebException (err, null, protoError, webResponse);
					webResponse.ReadAll ();
				} else if ((int) code == 304 && allowAutoRedirect) {
					string err = String.Format ("The remote server returned an error: ({0}) {1}.",
								    (int) code, webResponse.StatusDescription);
					throwMe = new WebException (err, null, protoError, webResponse);
				} else if ((int) code >= 300 && allowAutoRedirect && redirects > maxAutoRedirect) {
					throwMe = new WebException ("Max. redirections exceeded.", null,
								    protoError, webResponse);
					webResponse.ReadAll ();
				}
			}

			if (throwMe == null) {
				bool b = false;
				int c = (int) code;
				if (allowAutoRedirect && c >= 300)
					b = Redirect (result, code);

				if (resp != null && c >= 300 && c != 304)
					resp.ReadAll ();

				return b;
			}

			if (writeStream != null) {
				writeStream.InternalClose ();
				writeStream = null;
			}

			webResponse = null;

			throw throwMe;
		}
	}
}


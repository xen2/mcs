//
// System.Web.HttpApplication.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//    
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
// The Application Processing Pipeline.
// 
//     The Http application pipeline implemented in this file is a
//     beautiful thing.  The application pipeline invokes a number of
//     hooks at various stages of the processing of a request.  These
//     hooks can be either synchronous or can be asynchronous.
//     
//     The pipeline must ensure that every step is completed before
//     moving to the next step.  A trivial thing for synchronous
//     hooks, but asynchronous hooks introduce an extra layer of
//     complexity: when the hook is invoked, the thread must
//     relinquish its control so that the thread can be reused in
//     another operation while waiting.
//
//     To implement this functionality we used C# iterators manually;
//     we drive the pipeline by executing the various hooks from the
//     `RunHooks' routine which is an enumerator that will yield the
//     value `false' if execution must proceed or `true' if execution
//     must be stopped.
//
//     By yielding values we can suspend execution of RunHooks.
//
//     Special attention must be given to `in_begin' and `must_yield'
//     variables.  These are used in the case that an async hook
//     completes synchronously as its important to not yield in that
//     case or we would hang.
//    
//     Many of Mono modules used to be declared async, but they would
//     actually be completely synchronous, this might resurface in the
//     future with other modules.
//
// TODO:
//    Events Disposed
//

using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Web.UI;

#if TARGET_J2EE
using Mainsoft.Web;
#endif
	
namespace System.Web {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ToolboxItem(false)]
	public class HttpApplication : IHttpAsyncHandler, IHttpHandler, IComponent, IDisposable {
		object this_lock = new object();

		internal static readonly string [] BinDirs = {"Bin", "bin"};
		
		HttpContext context;
		HttpSessionState session;
		ISite isite;

		// The source, and the exposed API (cache).
		HttpModuleCollection modcoll;

		string assemblyLocation;

		//
		// The factory for the handler currently running.
		//
		IHttpHandlerFactory factory;

		//
		// Whether the thread culture is to be auto-set.
		// Used only in the 2.0 profile, always false for 1.x
		//
		bool autoCulture;
		bool autoUICulture;
		
		//
		// Whether the pipeline should be stopped
		//
		bool stop_processing;

		//
		// The Pipeline
		//
		IEnumerator pipeline;

		// To flag when we are done processing a request from BeginProcessRequest.
		ManualResetEvent done;

		// The current IAsyncResult for the running async request handler in the pipeline
		AsyncRequestState begin_iar;

		// Tracks the current AsyncInvocation being dispatched
		AsyncInvoker current_ai;

		EventHandlerList events;

		// Culture and IPrincipal
		CultureInfo app_culture;
		CultureInfo appui_culture;
		CultureInfo prev_app_culture;
		CultureInfo prev_appui_culture;
		IPrincipal prev_user;

		static string binDirectory;
		
#if NET_2_0
#if TARGET_J2EE
		const string initialization_exception_key = "System.Web.HttpApplication.initialization_exception";
		static Exception initialization_exception {
			get { return (Exception) AppDomain.CurrentDomain.GetData (initialization_exception_key); }
			set { AppDomain.CurrentDomain.SetData (initialization_exception_key, value); }
		}
#else
		static Exception initialization_exception;
#endif
		bool removeConfigurationFromCache;
#endif
		bool fullInitComplete = false;
		
		//
		// These are used to detect the case where the EndXXX method is invoked
		// from within the BeginXXXX delegate, so we detect whether we kick the
		// pipeline from here, or from the the RunHook routine
		//
		bool must_yield;
		bool in_begin;
		
		public HttpApplication ()
		{
			done = new ManualResetEvent (false);
		}
		
		internal void InitOnce (bool full_init)
		{
			lock (this_lock) {
				if (modcoll != null)
					return;

				if (full_init)
					events = null; // discard all the event handlers registered
						       // so far (e.g. in Application_Start)
				
#if NET_2_0
				HttpModulesSection modules;
				modules = (HttpModulesSection) WebConfigurationManager.GetSection ("system.web/httpModules", HttpRuntime.AppDomainAppVirtualPath);
#else
				ModulesConfiguration modules;

				modules = (ModulesConfiguration) HttpContext.GetAppConfig ("system.web/httpModules");
#endif

				HttpContext saved = HttpContext.Current;
				HttpContext.Current = new HttpContext (new System.Web.Hosting.SimpleWorkerRequest ("/", "", new StringWriter()));
				modcoll = modules.LoadModules (this);
				HttpContext.Current = saved;

				if (full_init) {
					HttpApplicationFactory.AttachEvents (this);
					Init ();
					fullInitComplete = true;
				}
			}
		}

		internal string AssemblyLocation {
			get {
				if (assemblyLocation == null)
					assemblyLocation = GetType ().Assembly.Location;
				return assemblyLocation;
			}
		}

#if NET_2_0
		internal static Exception InitializationException {
			get { return initialization_exception; }
		}
#endif

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpApplicationState Application {
			get {
				return HttpApplicationFactory.ApplicationState;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpContext Context {
			get {
				return context;
			}
		}
					 
		protected EventHandlerList Events {
			get {
				if (events == null)
					events = new EventHandlerList ();

				return events;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpModuleCollection Modules {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
			get {
				lock (this_lock) {
					if (modcoll == null)
						modcoll = new HttpModuleCollection ();
				}
				
				return modcoll;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpRequest Request {
			get {
				if (context == null)
					throw new HttpException (Locale.GetText ("No context is available."));

				if (false == HttpApplicationFactory.ContextAvailable)
					throw new HttpException (Locale.GetText ("Request is not available in this context."));

				return context.Request;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpResponse Response {
			get {
				if (context == null)
					throw new HttpException (Locale.GetText ("No context is available."));

				if (false == HttpApplicationFactory.ContextAvailable)
					throw new HttpException (Locale.GetText ("Response is not available in this context."));

				return context.Response;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpServerUtility Server {
			get {
				if (context != null)
					return context.Server;

				//
				// This is so we can get the Server and call a few methods
				// which are not context sensitive, see HttpServerUtilityTest
				//
				return new HttpServerUtility ((HttpContext) null);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpSessionState Session {
			get {
				// Only used for Session_End
				if (session != null)
					return session;

				if (context == null)
					throw new HttpException (Locale.GetText ("No context is available."));
				return context.Session;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		public ISite Site {
#else
		public virtual ISite Site {
#endif
			get {
				return isite;
			}

			set {
				isite = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IPrincipal User {
			get {
				if (context == null)
					throw new HttpException (Locale.GetText ("No context is available."));
				if (context.User == null)
					throw new HttpException (Locale.GetText ("No currently authenticated user."));
				
				return context.User;
			}
		}
		
		public virtual event EventHandler Disposed;
		public virtual event EventHandler Error;

		static object PreSendRequestHeadersEvent = new object ();
		public event EventHandler PreSendRequestHeaders
		{
			add { AddEventHandler (PreSendRequestHeadersEvent, value); }
			remove { RemoveEventHandler (PreSendRequestHeadersEvent, value); }
		}
		
		internal void TriggerPreSendRequestHeaders ()
		{
			EventHandler handler = Events [PreSendRequestHeadersEvent] as EventHandler;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}

		static object PreSendRequestContentEvent = new object ();
		public event EventHandler PreSendRequestContent
		{
			add { AddEventHandler (PreSendRequestContentEvent, value); }
			remove { RemoveEventHandler (PreSendRequestContentEvent, value); }
		}
		
		internal void TriggerPreSendRequestContent ()
		{
			EventHandler handler = Events [PreSendRequestContentEvent] as EventHandler;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}

		static object AcquireRequestStateEvent = new object ();
		public event EventHandler AcquireRequestState
		{
			add { AddEventHandler (AcquireRequestStateEvent, value); }
			remove { RemoveEventHandler (AcquireRequestStateEvent, value); }
		}
		
		public void AddOnAcquireRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			AcquireRequestState += new EventHandler (invoker.Invoke);
		}

		static object AuthenticateRequestEvent = new object ();
		public event EventHandler AuthenticateRequest
		{
			add { AddEventHandler (AuthenticateRequestEvent, value); }
			remove { RemoveEventHandler (AuthenticateRequestEvent, value); }
		}
		
		public void AddOnAuthenticateRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			AuthenticateRequest += new EventHandler (invoker.Invoke);
		}

		static object AuthorizeRequestEvent = new object ();
		public event EventHandler AuthorizeRequest
		{
			add { AddEventHandler (AuthorizeRequestEvent, value); }
			remove { RemoveEventHandler (AuthorizeRequestEvent, value); }
		}
		
		public void AddOnAuthorizeRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			AuthorizeRequest += new EventHandler (invoker.Invoke);
		}

		static object BeginRequestEvent = new object ();
		public event EventHandler BeginRequest
		{
			add { AddEventHandler (BeginRequestEvent, value); }
			remove { RemoveEventHandler (BeginRequestEvent, value); }
		}
		
		public void AddOnBeginRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			BeginRequest += new EventHandler (invoker.Invoke);
		}

		static object EndRequestEvent = new object ();
		public event EventHandler EndRequest
		{
			add { AddEventHandler (EndRequestEvent, value); }
			remove { RemoveEventHandler (EndRequestEvent, value); }
		}
		
		public void AddOnEndRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			EndRequest += new EventHandler (invoker.Invoke);
		}

		static object PostRequestHandlerExecuteEvent = new object ();
		public event EventHandler PostRequestHandlerExecute
		{
			add { AddEventHandler (PostRequestHandlerExecuteEvent, value); }
			remove { RemoveEventHandler (PostRequestHandlerExecuteEvent, value); }
		}
		
		public void AddOnPostRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			PostRequestHandlerExecute += new EventHandler (invoker.Invoke);
		}

		static object PreRequestHandlerExecuteEvent = new object ();
		public event EventHandler PreRequestHandlerExecute
		{
			add { AddEventHandler (PreRequestHandlerExecuteEvent, value); }
			remove { RemoveEventHandler (PreRequestHandlerExecuteEvent, value); }
		}
		
		public void AddOnPreRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			PreRequestHandlerExecute += new EventHandler (invoker.Invoke);
		}

		static object ReleaseRequestStateEvent = new object ();
		public event EventHandler ReleaseRequestState
		{
			add { AddEventHandler (ReleaseRequestStateEvent, value); }
			remove { RemoveEventHandler (ReleaseRequestStateEvent, value); }
		}
		
		public void AddOnReleaseRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			ReleaseRequestState += new EventHandler (invoker.Invoke);
		}

		static object ResolveRequestCacheEvent = new object ();
		public event EventHandler ResolveRequestCache
		{
			add { AddEventHandler (ResolveRequestCacheEvent, value); }
			remove { RemoveEventHandler (ResolveRequestCacheEvent, value); }
		}
		
		public void AddOnResolveRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			ResolveRequestCache += new EventHandler (invoker.Invoke);
		}

		static object UpdateRequestCacheEvent = new object ();
		public event EventHandler UpdateRequestCache
		{
			add { AddEventHandler (UpdateRequestCacheEvent, value); }
			remove { RemoveEventHandler (UpdateRequestCacheEvent, value); }
		}
		
		public void AddOnUpdateRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh);
			UpdateRequestCache += new EventHandler (invoker.Invoke);
		}

#if NET_2_0
		static object PostAuthenticateRequestEvent = new object ();
		public event EventHandler PostAuthenticateRequest
		{
			add { AddEventHandler (PostAuthenticateRequestEvent, value); }
			remove { RemoveEventHandler (PostAuthenticateRequestEvent, value); }
		}
		
		public void AddOnPostAuthenticateRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostAuthenticateRequestAsync (bh, eh, null);
		}
			
		public void AddOnPostAuthenticateRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostAuthenticateRequest += new EventHandler (invoker.Invoke);
		}

		static object PostAuthorizeRequestEvent = new object ();
		public event EventHandler PostAuthorizeRequest
		{
			add { AddEventHandler (PostAuthorizeRequestEvent, value); }
			remove { RemoveEventHandler (PostAuthorizeRequestEvent, value); }
		}
		
		public void AddOnPostAuthorizeRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostAuthorizeRequestAsync (bh, eh, null);
		}
		
		public void AddOnPostAuthorizeRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostAuthorizeRequest += new EventHandler (invoker.Invoke);
		}
		
		static object PostResolveRequestCacheEvent = new object ();
		public event EventHandler PostResolveRequestCache
		{
			add { AddEventHandler (PostResolveRequestCacheEvent, value); }
			remove { RemoveEventHandler (PostResolveRequestCacheEvent, value); }
		}
		
		public void AddOnPostResolveRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostResolveRequestCacheAsync (bh, eh, null);
		}
		
		public void AddOnPostResolveRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostResolveRequestCache += new EventHandler (invoker.Invoke);
		}

		static object PostMapRequestHandlerEvent = new object ();
		public event EventHandler PostMapRequestHandler
		{
			add { AddEventHandler (PostMapRequestHandlerEvent, value); }
			remove { RemoveEventHandler (PostMapRequestHandlerEvent, value); }
		}
		
		public void AddOnPostMapRequestHandlerAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostMapRequestHandlerAsync (bh, eh, null);
		}
		
		public void AddOnPostMapRequestHandlerAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostMapRequestHandler += new EventHandler (invoker.Invoke);
		}

		static object PostAcquireRequestStateEvent = new object ();
		public event EventHandler PostAcquireRequestState
		{
			add { AddEventHandler (PostAcquireRequestStateEvent, value); }
			remove { RemoveEventHandler (PostAcquireRequestStateEvent, value); }
		}
		
		public void AddOnPostAcquireRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostAcquireRequestStateAsync (bh, eh, null);
		}
		
		public void AddOnPostAcquireRequestStateAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostAcquireRequestState += new EventHandler (invoker.Invoke);
		}

		static object PostReleaseRequestStateEvent = new object ();
		public event EventHandler PostReleaseRequestState
		{
			add { AddEventHandler (PostReleaseRequestStateEvent, value); }
			remove { RemoveEventHandler (PostReleaseRequestStateEvent, value); }
		}
		
		public void AddOnPostReleaseRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostReleaseRequestStateAsync (bh, eh, null);
		}
		
		public void AddOnPostReleaseRequestStateAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostReleaseRequestState += new EventHandler (invoker.Invoke);
		}

		static object PostUpdateRequestCacheEvent = new object ();
		public event EventHandler PostUpdateRequestCache
		{
			add { AddEventHandler (PostUpdateRequestCacheEvent, value); }
			remove { RemoveEventHandler (PostUpdateRequestCacheEvent, value); }
		}
		
		public void AddOnPostUpdateRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostUpdateRequestCacheAsync (bh, eh, null);
		}
		
		public void AddOnPostUpdateRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostUpdateRequestCache += new EventHandler (invoker.Invoke);
		}

		//
		// The new overloads that take a data parameter
		//
		public void AddOnAcquireRequestStateAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			AcquireRequestState += new EventHandler (invoker.Invoke);
		}

		public void AddOnAuthenticateRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			AuthenticateRequest += new EventHandler (invoker.Invoke);
		}

		public void AddOnAuthorizeRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			AuthorizeRequest += new EventHandler (invoker.Invoke);
		}

		public void AddOnBeginRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			BeginRequest += new EventHandler (invoker.Invoke);
		}

		public void AddOnEndRequestAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			EndRequest += new EventHandler (invoker.Invoke);
		}
		
		public void AddOnPostRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PostRequestHandlerExecute += new EventHandler (invoker.Invoke);
		}

		public void AddOnPreRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			PreRequestHandlerExecute += new EventHandler (invoker.Invoke);
		}

		public void AddOnReleaseRequestStateAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			ReleaseRequestState += new EventHandler (invoker.Invoke);
		}

		public void AddOnResolveRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			ResolveRequestCache += new EventHandler (invoker.Invoke);
		}

		public void AddOnUpdateRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh, object data)
		{
			AsyncInvoker invoker = new AsyncInvoker (bh, eh, data);
			UpdateRequestCache += new EventHandler (invoker.Invoke);
		}

		// Added in 2.0 SP1
		// They are for use with the IIS7 integrated mode, but have been added for
		// compatibility
		static object LogRequestEvent = new object ();
		public event EventHandler LogRequest
		{
			add { AddEventHandler (LogRequestEvent, value); }
			remove { RemoveEventHandler (LogRequestEvent, value); }
		}
		
		public void AddOnLogRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnLogRequestAsync (bh, eh, null);
		}
		
		public void AddOnLogRequestAsync (BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
		{
			AsyncInvoker invoker = new AsyncInvoker (beginHandler, endHandler, state);
			LogRequest += new EventHandler (invoker.Invoke);
		}

		static object MapRequestHandlerEvent = new object ();
		public event EventHandler MapRequestHandler
		{
			add { AddEventHandler (MapRequestHandlerEvent, value); }
			remove { RemoveEventHandler (MapRequestHandlerEvent, value); }
		}
		
		public void AddOnMapRequestHandlerAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnMapRequestHandlerAsync (bh, eh, null);
		}

		public void AddOnMapRequestHandlerAsync (BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
		{
			AsyncInvoker invoker = new AsyncInvoker (beginHandler, endHandler, state);
			MapRequestHandler += new EventHandler (invoker.Invoke);
		}

		static object PostLogRequestEvent = new object ();
		public event EventHandler PostLogRequest
		{
			add { AddEventHandler (PostLogRequestEvent, value); }
			remove { RemoveEventHandler (PostLogRequestEvent, value); }
		}
		
		public void AddOnPostLogRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			AddOnPostLogRequestAsync (bh, eh, null);
		}

		public void AddOnPostLogRequestAsync (BeginEventHandler beginHandler, EndEventHandler endHandler, object state)
		{
			AsyncInvoker invoker = new AsyncInvoker (beginHandler, endHandler, state);
			PostLogRequest += new EventHandler (invoker.Invoke);
		}

#endif
		
		internal event EventHandler DefaultAuthentication;

		void AddEventHandler (object key, EventHandler handler)
		{
			if (fullInitComplete)
				return;

			Events.AddHandler (key, handler);
		}

		void RemoveEventHandler (object key, EventHandler handler)
		{
			if (fullInitComplete)
				return;

			Events.RemoveHandler (key, handler);
		}
		
		//
		// Bypass all the event on the Http pipeline and go directly to EndRequest
		//
		public void CompleteRequest ()
		{
			stop_processing = true;
		}

		internal bool RequestCompleted {
			set { stop_processing = value; }
		}

		internal void DisposeInternal ()
		{
			Dispose ();
			lock (this_lock) {
				if (modcoll != null) {
					for (int i = modcoll.Count - 1; i >= 0; i--) {
						modcoll.Get (i).Dispose ();
					}
					modcoll = null;
				}
			}
			
			if (Disposed != null)
				Disposed (this, EventArgs.Empty);
			
			done.Close ();
			done = null;
		}
		
		public virtual void Dispose ()
		{
		}

		public virtual string GetVaryByCustomString (HttpContext context, string custom)
		{
			if (custom == null) // Sigh
				throw new NullReferenceException ();

			if (0 == String.Compare (custom, "browser", true, CultureInfo.InvariantCulture))
				return context.Request.Browser.Type;

			return null;
		}

		//
		// If we catch an error, queue this error
		//
		void ProcessError (Exception e)
		{
			bool first = context.Error == null;
			context.AddError (e);
			if (first){
				if (Error != null){
					try {
						Error (this, EventArgs.Empty);
					} catch (ThreadAbortException taex){
						context.ClearError ();
						if (FlagEnd.Value == taex.ExceptionState)
							// This happens on Redirect() or End()
							Thread.ResetAbort ();
						else
							// This happens on Thread.Abort()
							context.AddError (taex);
					} catch (Exception ee){
						context.AddError (ee);
					}
				}
			}
			stop_processing = true;
#if NET_2_0
			// we want to remove configuration from the cache in case of 
			// invalid resource not exists to prevent DOS attack.
			HttpException httpEx = e as HttpException;
			if (httpEx != null && httpEx.GetHttpCode () == 404) {
				removeConfigurationFromCache = true;
			}
#endif
		}
		
		//
		// Ticks the clock: next step on the pipeline.
		//
		internal void Tick ()
		{
			try {
#if TARGET_J2EE
				if (context.Error is UnifyRequestException) {
					Exception ex = context.Error.InnerException;
					context.ClearError ();
					vmw.common.TypeUtils.Throw (ex);
				}
				try {
#endif		
				if (pipeline.MoveNext ()){
					if ((bool)pipeline.Current)
						PipelineDone ();
				}
#if TARGET_J2EE
				}
				catch (Exception ex) {
					if (ex is ThreadAbortException && 
						((ThreadAbortException) ex).ExceptionState == FlagEnd.Value)
						throw;
					if (context.WorkerRequest is IHttpUnifyWorkerRequest) {
						context.ClearError ();
						context.AddError (new UnifyRequestException (ex));
						return;
					}
					else
						throw;
				}
#endif
			} catch (ThreadAbortException taex) {
				object obj = taex.ExceptionState;
				Thread.ResetAbort ();
				stop_processing = true;
				if (obj is StepTimeout)
					ProcessError (new HttpException ("The request timed out."));
				else {
					context.ClearError ();
					if (FlagEnd.Value != obj)
						context.AddError (taex);
				}

				PipelineDone ();
			} catch (Exception e) {
				stop_processing = true;
				ProcessError (e);
				PipelineDone ();
			}
		}

		void Resume ()
		{
			if (in_begin)
				must_yield = false;
			else
				Tick ();
		}
		
		//
		// Invoked when our async callback called from RunHooks completes,
		// we restart the pipeline here.
		//
		void async_callback_completed_cb (IAsyncResult ar)
		{
			if (current_ai.end != null){
				try {
					current_ai.end (ar);
				} catch (Exception e) {
					ProcessError (e);
				}
			}

			Resume ();
		}

		void async_handler_complete_cb (IAsyncResult ar)
		{
			IHttpAsyncHandler async_handler = ((IHttpAsyncHandler) ar.AsyncState);

			try {
				async_handler.EndProcessRequest (ar);
			} catch (Exception e){
				ProcessError (e);
			}
			
			Resume ();
		}
		
		//
		// This enumerator yields whether processing must be stopped:
		//    true:  processing of the pipeline must be stopped
		//    false: processing of the pipeline must not be stopped
		//
		IEnumerable RunHooks (Delegate list)
		{
			Delegate [] delegates = list.GetInvocationList ();

			foreach (EventHandler d in delegates){
				if (d.Target != null && (d.Target is AsyncInvoker)){
					current_ai = (AsyncInvoker) d.Target;

					try {
						must_yield = true;
						in_begin = true;
						context.BeginTimeoutPossible ();
						current_ai.begin (this, EventArgs.Empty, async_callback_completed_cb, current_ai.data);
					} finally {
						in_begin = false;
						context.EndTimeoutPossible ();
					}

					//
					// If things are still moving forward, yield this
					// thread now
					//
					if (must_yield)
						yield return stop_processing;
					else if (stop_processing)
						yield return true;
				} else {
					try {
						context.BeginTimeoutPossible ();
						d (this, EventArgs.Empty);
					} finally {
						context.EndTimeoutPossible ();
					}
					if (stop_processing)
						yield return true;
				}
			}
		}

		static void FinalErrorWrite (HttpResponse response, string error)
		{
			try {
				response.Write (error);
				response.Flush (true);
			} catch {
				response.Close ();
			}
		}

		void OutputPage ()
		{
			if (context.Error == null){
				try {
					context.Response.Flush (true);
				} catch (Exception e){
					context.AddError (e);
				}
			}

			Exception error = context.Error;
			if (error != null){
				HttpResponse response = context.Response;

				if (!response.HeadersSent){
					response.ClearHeaders ();
					response.ClearContent ();

					if (error is HttpException){
						response.StatusCode = ((HttpException)error).GetHttpCode ();
					} else {
						error = new HttpException ("", error);
						response.StatusCode = 500;
					}
					HttpException httpEx = (HttpException) error;
					if (!RedirectCustomError (ref httpEx))
						FinalErrorWrite (response, httpEx.GetHtmlErrorMessage ());
					else
						response.Flush (true);
				} else {
					if (!(error is HttpException))
						error = new HttpException ("", error);
					FinalErrorWrite (response, ((HttpException) error).GetHtmlErrorMessage ());
				}
			}
			
		}
		
		//
		// Invoked at the end of the pipeline execution
		//
		void PipelineDone ()
		{
			try {
				EventHandler handler = Events [EndRequestEvent] as EventHandler;
				if (handler != null)
					handler (this, EventArgs.Empty);
			} catch (Exception e){
				ProcessError (e);
			}

			try {
				OutputPage ();
			} catch (Exception e) {
				Console.WriteLine ("Internal error: OutputPage threw an exception " + e);
			} finally {
				context.WorkerRequest.EndOfRequest();
				if (factory != null && context.Handler != null){
					factory.ReleaseHandler (context.Handler);
					context.Handler = null;
					factory = null;
				}
#if NET_2_0
				context.PopHandler ();
#endif
				// context = null; -> moved to PostDone
				pipeline = null;
				current_ai = null;
			}
			PostDone ();

			if (begin_iar != null)
				begin_iar.Complete ();
			else
				done.Set ();
		}
		
		//
		// Events fired as described in `Http Runtime Support, HttpModules,
		// Handling Public Events'
		//
		IEnumerator Pipeline ()
		{
			Delegate eventHandler;
			if (stop_processing)
				yield return true;

			eventHandler = Events [BeginRequestEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;

			eventHandler = Events [AuthenticateRequestEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;

			if (DefaultAuthentication != null)
				foreach (bool stop in RunHooks (DefaultAuthentication))
					yield return stop;

#if NET_2_0
			eventHandler = Events [PostAuthenticateRequestEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;
#endif
			eventHandler = Events [AuthorizeRequestEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;
#if NET_2_0
			eventHandler = Events [PostAuthorizeRequestEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;
#endif

			eventHandler = Events [ResolveRequestCacheEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;

#if NET_2_0
			eventHandler = Events [PostResolveRequestCacheEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;

			// As per http://msdn2.microsoft.com/en-us/library/bb470252(VS.90).aspx
			eventHandler = Events [MapRequestHandlerEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;
#endif
			
			// Obtain the handler for the request.
			IHttpHandler handler = null;
			try {
				handler = GetHandler (context, context.Request.CurrentExecutionFilePath);
				context.Handler = handler;
#if NET_2_0
				context.PushHandler (handler);
#endif
			} catch (FileNotFoundException fnf){
#if TARGET_JVM
				Console.WriteLine ("$$$$$$$$$$:Sys.Web Pipeline");
				Console.WriteLine (fnf.ToString ());
#endif
				if (context.Request.IsLocal)
					ProcessError (new HttpException (404, String.Format ("File not found {0}", fnf.FileName), fnf, context.Request.FilePath));
				else
					ProcessError (new HttpException (404, "File not found: " + Path.GetFileName (fnf.FileName), context.Request.FilePath));
			} catch (DirectoryNotFoundException dnf){
				if (!context.Request.IsLocal)
					dnf = null; // Do not "leak" real path information
				ProcessError (new HttpException (404, "Directory not found", dnf));
			} catch (Exception e) {
				ProcessError (e);
			}

			if (stop_processing)
				yield return true;

#if NET_2_0
			eventHandler = Events [PostMapRequestHandlerEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;
#endif

			eventHandler = Events [AcquireRequestStateEvent];
			if (eventHandler != null){
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;
			}
			
#if NET_2_0
			eventHandler = Events [PostAcquireRequestStateEvent];
			if (eventHandler != null){
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;
			}
#endif
			
			//
			// From this point on, we need to ensure that we call
			// ReleaseRequestState, so the code below jumps to
			// `release:' to guarantee it rather than yielding.
			//
			eventHandler = Events [PreRequestHandlerExecuteEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					if (stop)
						goto release;
			
				
#if TARGET_J2EE
		processHandler:
			bool doProcessHandler = false;
#endif
			
#if NET_2_0
			IHttpHandler ctxHandler = context.Handler;
			if (ctxHandler != null && handler != ctxHandler) {
				context.PopHandler ();
				handler = ctxHandler;
				context.PushHandler (handler);
			}
#endif

			try {
				context.BeginTimeoutPossible ();
				if (handler != null){
					IHttpAsyncHandler async_handler = handler as IHttpAsyncHandler;
					
					if (async_handler != null){
						must_yield = true;
						in_begin = true;
						async_handler.BeginProcessRequest (context, async_handler_complete_cb, handler);
					} else {
						must_yield = false;
						handler.ProcessRequest (context);
#if TARGET_J2EE
						IHttpExtendedHandler extHandler=handler as IHttpExtendedHandler;
						doProcessHandler = extHandler != null && !extHandler.IsCompleted;
#endif
					}
				}
				if (context.Error != null)
					throw new TargetInvocationException(context.Error);
			} finally {
				in_begin = false;
				context.EndTimeoutPossible ();
			}
#if TARGET_J2EE
			if (doProcessHandler) {
				yield return false;
				goto processHandler;
			}
#endif
			if (must_yield)
				yield return stop_processing;
			else if (stop_processing)
				goto release;
			
			// These are executed after the application has returned
			
			eventHandler = Events [PostRequestHandlerExecuteEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					if (stop)
						goto release;
			
		release:
			eventHandler = Events [ReleaseRequestStateEvent];
			if (eventHandler != null){
#pragma warning disable 168
				foreach (bool stop in RunHooks (eventHandler)) {
					//
					// Ignore the stop signal while release the state
					//
					
				}
#pragma warning restore 168
			}
			
			if (stop_processing)
				yield return true;

#if NET_2_0
			eventHandler = Events [PostReleaseRequestStateEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;
#endif

			if (context.Error == null)
				context.Response.DoFilter (true);

			eventHandler = Events [UpdateRequestCacheEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;

#if NET_2_0
			eventHandler = Events [PostUpdateRequestCacheEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;

			eventHandler = Events [LogRequestEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;

			eventHandler = Events [PostLogRequestEvent];
			if (eventHandler != null)
				foreach (bool stop in RunHooks (eventHandler))
					yield return stop;
#endif
			PipelineDone ();
		}


		internal CultureInfo GetThreadCulture (HttpRequest request, CultureInfo culture, bool isAuto)
		{
#if NET_2_0
			if (!isAuto)
				return culture;
			CultureInfo ret = null;
			string[] languages = request.UserLanguages;
			try {
				if (languages != null && languages.Length > 0)
					ret = CultureInfo.CreateSpecificCulture (languages[0]);
			} catch {
			}
			
			if (ret == null)
				ret = culture;
			
			return ret;
#else
			return culture;
#endif
		}


		void PreStart ()
		{
#if NET_2_0
			GlobalizationSection cfg;
			cfg = (GlobalizationSection) WebConfigurationManager.GetSection ("system.web/globalization");
			app_culture = cfg.GetCulture ();
			autoCulture = cfg.IsAutoCulture;
			appui_culture = cfg.GetUICulture ();
			autoUICulture = cfg.IsAutoUICulture;
#else
			GlobalizationConfiguration cfg;
			cfg = GlobalizationConfiguration.GetInstance (null);
			if (cfg != null) {
				app_culture = cfg.Culture;
				appui_culture = cfg.UICulture;
			}
#endif

#if !TARGET_J2EE
			context.StartTimeoutTimer ();
#endif
			Thread th = Thread.CurrentThread;
			if (app_culture != null) {
				prev_app_culture = th.CurrentCulture;
				CultureInfo new_app_culture = GetThreadCulture (Request, app_culture, autoCulture);
				if (!new_app_culture.Equals (CultureInfo.InvariantCulture))
					th.CurrentCulture = new_app_culture;
			}

			if (appui_culture != null) {
				prev_appui_culture = th.CurrentUICulture;
				CultureInfo new_app_culture = GetThreadCulture (Request, appui_culture, autoUICulture);
				if (!new_app_culture.Equals (CultureInfo.InvariantCulture))
					th.CurrentUICulture = new_app_culture;
			}

#if !TARGET_JVM
			prev_user = Thread.CurrentPrincipal;
#endif
		}

		void PostDone ()
		{
#if NET_2_0
			if (removeConfigurationFromCache) {
				WebConfigurationManager.RemoveConfigurationFromCache (context);
				removeConfigurationFromCache = false;
			}
#endif
			Thread th = Thread.CurrentThread;
#if !TARGET_JVM
			if (Thread.CurrentPrincipal != prev_user)
				Thread.CurrentPrincipal = prev_user;
#endif
			if (prev_appui_culture != null && prev_appui_culture != th.CurrentUICulture)
				th.CurrentUICulture = prev_appui_culture;
			if (prev_app_culture != null && prev_app_culture != th.CurrentCulture)
				th.CurrentCulture = prev_app_culture;

#if !TARGET_J2EE
			if (context == null)
				context = HttpContext.Current;
			context.StopTimeoutTimer ();
#endif
			context = null;
			session = null;
			HttpContext.Current = null;
		}

		void Start (object x)
		{
			CultureInfo[] cultures = x as CultureInfo[];
			if (cultures != null && cultures.Length == 2) {
				Thread ct = Thread.CurrentThread;
				ct.CurrentCulture = cultures [0];
				ct.CurrentUICulture = cultures [1];
			}
			
			try {
				InitOnce (true);
			} catch (Exception e) {
#if NET_2_0
				initialization_exception = e;
#endif
				FinalErrorWrite (context.Response, new HttpException ("", e).GetHtmlErrorMessage ());
				PipelineDone ();
				return;
			}

			HttpContext.Current = Context;
			PreStart ();
			pipeline = Pipeline ();
			Tick ();
		}

		const string HANDLER_CACHE = "@@HttpHandlerCache@@";

		internal static Hashtable GetHandlerCache ()
		{
			Cache cache = HttpRuntime.InternalCache;
			Hashtable ret = cache [HANDLER_CACHE] as Hashtable;

			if (ret == null) {
				ret = new Hashtable ();
				cache.Insert (HANDLER_CACHE, ret);
			}

			return ret;
		}
		
		internal static void ClearHandlerCache ()
		{
			Hashtable cache = GetHandlerCache ();
			cache.Clear ();
		}
		
		internal object LocateHandler (string verb, string url)
		{
			Hashtable cache = GetHandlerCache ();
			string id = String.Concat (verb, url);
			object ret = cache [id];

			if (ret != null)
				return ret;
			
#if NET_2_0
			HttpHandlersSection	httpHandlersSection = (HttpHandlersSection) WebConfigurationManager.GetSection ("system.web/httpHandlers");
			ret = httpHandlersSection.LocateHandler (verb, url);
#else
			HandlerFactoryConfiguration factory_config = (HandlerFactoryConfiguration) HttpContext.GetAppConfig ("system.web/httpHandlers");
			ret = factory_config.LocateHandler (verb, url);
#endif

			cache [id] = ret;
			return ret;
		}

		internal IHttpHandler GetHandler (HttpContext context, string url)
		{
			return GetHandler (context, url, false);
		}
		
		// Used by HttpServerUtility.Execute
		internal IHttpHandler GetHandler (HttpContext context, string url, bool ignoreContextHandler)
		{
			if (!ignoreContextHandler && context.Handler != null)
				return context.Handler;
			
			HttpRequest request = context.Request;
			string verb = request.RequestType;
			
			IHttpHandler handler = null;
			object o = LocateHandler (verb, url);
			
			factory = o as IHttpHandlerFactory;
			
			if (factory == null) {
				handler = (IHttpHandler) o;
			} else {
				handler = factory.GetHandler (context, verb, url, request.MapPath (url));
			}

			return handler;
		}
		
		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			begin_iar = null;
			this.context = context;
			done.Reset ();

			Start (null);
			done.WaitOne ();
		}

		//
		// This is used by FireOnAppStart, when we init the application
		// as the context is required to be set at that point (the user
		// might call methods that require it on that hook).
		//
		internal void SetContext (HttpContext context)
		{
			this.context = context;
		}

		internal void SetSession (HttpSessionState session)
		{
			this.session = session;
		}

		IAsyncResult IHttpAsyncHandler.BeginProcessRequest (HttpContext context, AsyncCallback cb, object extraData)
		{
			this.context = context;
			done.Reset ();
			
			begin_iar = new AsyncRequestState (done, cb, extraData);

			CultureInfo[] cultures = new CultureInfo [2];
			cultures [0] = Thread.CurrentThread.CurrentCulture;
			cultures [1] = Thread.CurrentThread.CurrentUICulture;
			
#if TARGET_JVM
			if (true)
#else
			if (Thread.CurrentThread.IsThreadPoolThread)
#endif
				Start (null);
			else
				ThreadPool.QueueUserWorkItem (new WaitCallback (Start), cultures);
			
			return begin_iar;
		}

		void IHttpAsyncHandler.EndProcessRequest (IAsyncResult result)
		{
#if TARGET_J2EE
			if (result == null)
				result = begin_iar;
#endif
			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne ();
			begin_iar = null;
		}

		public virtual void Init ()
		{
		}

		bool IHttpHandler.IsReusable {
			get {
				return true;
			}
		}
		
#region internals
		internal void ClearError ()
		{
			context.ClearError ();
		}

		bool RedirectErrorPage (string error_page)
		{
			if (context.Request.QueryString ["aspxerrorpath"] != null)
				return false;

			Response.Redirect (error_page + "?aspxerrorpath=" + Request.Path, false);
			return true;
		}
							
		bool RedirectCustomError (ref HttpException httpEx)
		{
			try {
			if (!context.IsCustomErrorEnabledUnsafe)
				return false;
			
#if NET_2_0
			CustomErrorsSection config = (CustomErrorsSection)WebConfigurationManager.GetSection ("system.web/customErrors");
#else
			CustomErrorsConfig config = null;
			try {
				config = (CustomErrorsConfig) context.GetConfig ("system.web/customErrors");
			} catch { }
#endif
			
			if (config == null) {
				if (context.ErrorPage != null)
					return RedirectErrorPage (context.ErrorPage);
				
				return false;
			}
			
#if NET_2_0
			CustomError err = config.Errors [context.Response.StatusCode.ToString()];
			string redirect = err == null ? null : err.Redirect;
#else
			string redirect =  config [context.Response.StatusCode];
#endif
			if (redirect == null) {
				redirect = context.ErrorPage;
				if (redirect == null)
					redirect = config.DefaultRedirect;
			}
			
			if (redirect == null)
				return false;
			
			return RedirectErrorPage (redirect);
			}
			catch (Exception ex) {
				httpEx = new HttpException (500, "", ex);
				return false;
			}
		}
#endregion		
		internal static string BinDirectory
		{
			get {
				if (binDirectory == null) {
					AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
					string baseDir = setup.ApplicationBase;
					string bindir;
					
					foreach (string dir in BinDirs) {
						bindir = Path.Combine (baseDir, dir);
						if (!Directory.Exists (bindir))
							continue;
						binDirectory = bindir;
						break;
					}
				}

				return binDirectory;
			}
		}

		internal static string[] BinDirectoryAssemblies
		{
			get {
				ArrayList binDlls = null;
				string[] dlls;
				
				string bindir = BinDirectory;
				if (bindir != null) {
					binDlls = new ArrayList ();
					dlls = Directory.GetFiles (bindir, "*.dll");
					binDlls.AddRange (dlls);
				}

				if (binDlls == null)
					return new string[] {};
				
				return (string[]) binDlls.ToArray (typeof (string));
			}
		}
					
		internal static Type LoadType (string typeName)
		{
			return LoadType (typeName, false);
		}
		
		internal static Type LoadType (string typeName, bool throwOnMissing)
		{
			Type type = Type.GetType (typeName);
			if (type != null)
				return type;

#if !TARGET_JVM
			Assembly [] assemblies = AppDomain.CurrentDomain.GetAssemblies ();
			foreach (Assembly ass in assemblies) {
				type = ass.GetType (typeName, false);
				if (type != null)
					return type;
			}

#if NET_2_0
			IList tla = System.Web.Compilation.BuildManager.TopLevelAssemblies;
			if (tla != null && tla.Count > 0) {
				foreach (Assembly asm in tla) {
					if (asm == null)
						continue;
					type = asm.GetType (typeName, false);
					if (type != null)
						return type;
				}
			}
#endif

			type = LoadTypeFromBin (typeName);
			if (type != null)
				return type;
#endif
			if (throwOnMissing)
				throw new TypeLoadException (String.Format ("Type '{0}' cannot be found", typeName));
			
			return null;
		}

		internal static Type LoadTypeFromBin (string typeName)
		{
			Type type = null;
			
			foreach (string s in BinDirectoryAssemblies) {
				Assembly binA = Assembly.LoadFrom (s);
				type = binA.GetType (typeName, false);
				if (type == null)
					continue;
				
				return type;
			}

			return null;
		}
	}

	//
	// Based on Fritz' Onion's AsyncRequestState class for asynchronous IHttpAsyncHandlers
	// 
	class AsyncRequestState : IAsyncResult {
		AsyncCallback cb;
		object cb_data;
		bool completed;
		ManualResetEvent complete_event = null;
		
		internal AsyncRequestState (ManualResetEvent complete_event, AsyncCallback cb, object cb_data)
		{
			this.cb = cb;
			this.cb_data = cb_data;
			this.complete_event = complete_event;
		}

		internal void Complete ()
		{
			completed = true;
			try {
				//
				// TODO: if this throws an error, we have no way of reporting it
				// Not really too bad, since the only failure might be
				// `HttpRuntime.request_processed'.   
				//
				if (cb != null)
					cb (this);
			} catch {
			}
			
			complete_event.Set ();
		}

		public object AsyncState {
			get {
				return cb_data;
			}
		}

		public bool CompletedSynchronously {
			get {
				return false;
			}
		}

		public bool IsCompleted {
			get {
				return completed;
			}
		}

		public WaitHandle AsyncWaitHandle {
			get {
				return complete_event;
			}
		}
	}

#region Helper classes
	
	//
	// A wrapper to keep track of begin/end pairs
	//
	class AsyncInvoker {
		public BeginEventHandler begin;
		public EndEventHandler end;
		public object data;
		
		public AsyncInvoker (BeginEventHandler bh, EndEventHandler eh, object d)
		{
			begin = bh;
			end = eh;
			data = d;
		}

		public AsyncInvoker (BeginEventHandler bh, EndEventHandler eh)
		{
			begin = bh;
			end = eh;
		}
		
		public void Invoke (object sender, EventArgs e)
		{
			throw new Exception ("This is just a dummy");
		}
	}
#endregion
}

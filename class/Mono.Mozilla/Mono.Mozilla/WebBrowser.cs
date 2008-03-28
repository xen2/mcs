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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

#undef debug

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla
{
	internal class WebBrowser : IWebBrowser
	{
		private bool loaded;
		private DOM.Document document;
		
		internal DOM.Navigation navigation;
		internal Platform platform;
		internal Platform enginePlatform;
		internal Callback callbacks;
		private EventHandlerList events;
		private EventHandlerList domEvents;

		private string statusText;

		private bool streamingMode;
		
		public WebBrowser (Platform platform)
		{
			this.platform = platform;
			callbacks = new Callback(this);
			loaded = Base.Init (this, platform);
		}

		public bool Load (IntPtr handle, int width, int height)
		{
			Base.Bind (this, handle, width, height);
			return loaded;
		}

		public void Shutdown ()
		{
			Base.Shutdown (this);
		}
		
		internal void Reset ()
		{
			this.document = null;
			this.DomEvents.Dispose ();
			this.domEvents = null;
		}

		public bool Initialized {
			get { return this.loaded; }
		}
		
		public IWindow Window {
			get {
				if (Navigation != null) {
					nsIWebBrowserFocus webBrowserFocus = (nsIWebBrowserFocus) (navigation.navigation);
					nsIDOMWindow window;
					webBrowserFocus.getFocusedWindow (out window);
					return new DOM.Window (this, window) as IWindow;
				}
				return null;
			}
		}

		public IDocument Document {
			get {
				if (Navigation != null && document == null) {
					document = navigation.Document;
				}
				return document as IDocument;
			}
		}

		public INavigation Navigation {
			get {
				if (navigation == null) {
					
					nsIWebNavigation webNav = Base.GetWebNavigation (this);
					navigation = new DOM.Navigation (this, webNav);
				}
				return navigation as INavigation;
			}
		}
		
		public string StatusText {
			get { return statusText; }
		}

		internal EventHandlerList DomEvents {
			get {
				if (domEvents == null)
					domEvents = new EventHandlerList();

				return domEvents;
			}
		}

		internal EventHandlerList Events {
			get {
				if (events == null)
					events = new EventHandlerList();

				return events;
			}
		}

		#region Layout
		public void FocusIn (FocusOption focus)
		{
			Base.Focus (this, focus);
		}
		public void FocusOut ()
		{
			Base.Blur (this);
		}

		public void Activate ()
		{
			Base.Activate (this);
		}
		public void Deactivate ()
		{
			Base.Deactivate (this);
		}

		public void Resize (int width, int height)
		{
			Base.Resize (this, width, height);			
		}
		

		public void Render (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			string html = System.Text.ASCIIEncoding.UTF8.GetString (data);
			Render (html);
		}

		public void Render (string html)
		{
			Render (html, "file:///", "text/html");
		}

		
		nsIServiceManager servMan;
		IntPtr ioServicePtr;
		nsIIOService ioService;
		
		public void Render (string html, string uri, string contentType)
		{
			nsIWebBrowserStream stream;
			
			if (Navigation != null) {
				stream = (nsIWebBrowserStream) navigation.navigation;
			} else
				throw new Mono.WebBrowser.Exception (Mono.WebBrowser.Exception.ErrorCodes.Navigation);

			
			if (servMan == null)
				servMan = Base.gluezilla_getServiceManager ();
			
			
			if (ioServicePtr == IntPtr.Zero)
				servMan.getServiceByContractID ("@mozilla.org/network/io-service;1", typeof (nsIIOService).GUID, out ioServicePtr);
			if (ioServicePtr == IntPtr.Zero)
				throw new Mono.WebBrowser.Exception (Mono.WebBrowser.Exception.ErrorCodes.IOService);

			
			if (ioService == null) {
				try {
					ioService = (nsIIOService)Marshal.GetObjectForIUnknown (ioServicePtr);
				} catch (System.Exception ex) {
					throw new Mono.WebBrowser.Exception (Mono.WebBrowser.Exception.ErrorCodes.IOService, ex);
				}
			}

			AsciiString asciiUri = new AsciiString (uri);
			nsIURI ret;
			ioService.newURI (asciiUri.Handle, null, null, out ret);

			AsciiString ctype = new AsciiString(contentType);

			HandleRef han = ctype.Handle;

			stream.openStream (ret, han);

			IntPtr native_html = Marshal.StringToHGlobalAnsi (html);
			stream.appendToStream (native_html, (uint)html.Length);
			Marshal.FreeHGlobal (native_html);

			stream.closeStream ();

		}
		
				
		internal void AttachEvent (INode node, string eve, EventHandler handler) {
			string key = String.Intern (node.GetHashCode() + ":" + eve);
			Console.Error.WriteLine ("Event Attached: " + key);
			DomEvents.AddHandler (key, handler);
		}

		internal void DetachEvent (INode node, string eve, EventHandler handler) {
			string key = String.Intern (node.GetHashCode() + ":" + eve);
			Console.Error.WriteLine ("Event Detached: " + key);
			DomEvents.RemoveHandler (key, handler);
		}
		
		
		#endregion

		#region Events
		internal static object KeyDownEvent = new object ();
		internal static object KeyPressEvent = new object ();
		internal static object KeyUpEvent = new object ();
		internal static object MouseClickEvent = new object ();
		internal static object MouseDoubleClickEvent = new object ();
		internal static object MouseDownEvent = new object ();
		internal static object MouseEnterEvent = new object ();
		internal static object MouseLeaveEvent = new object ();
		internal static object MouseMoveEvent = new object ();
		internal static object MouseUpEvent = new object ();
		internal static object FocusEvent = new object ();
		internal static object BlurEvent = new object ();
		internal static object CreateNewWindowEvent = new object ();
		internal static object AlertEvent = new object ();
		internal static object TransferringEvent = new object ();
		internal static object DocumentCompletedEvent = new object ();
		internal static object CompletedEvent = new object ();
		internal static object LoadEvent = new object ();
		internal static object UnloadEvent = new object ();
		
		public event NodeEventHandler KeyDown
		{
			add { Events.AddHandler (KeyDownEvent, value); }
			remove { Events.RemoveHandler (KeyDownEvent, value); }
		}

		public event NodeEventHandler KeyPress
		{
			add { Events.AddHandler (KeyPressEvent, value); }
			remove { Events.RemoveHandler (KeyPressEvent, value); }
		}
		public event NodeEventHandler KeyUp
		{
			add { Events.AddHandler (KeyUpEvent, value); }
			remove { Events.RemoveHandler (KeyUpEvent, value); }
		}
		public event NodeEventHandler MouseClick
		{
			add { Events.AddHandler (MouseClickEvent, value); }
			remove { Events.RemoveHandler (MouseClickEvent, value); }
		}
		public event NodeEventHandler MouseDoubleClick
		{
			add { Events.AddHandler (MouseDoubleClickEvent, value); }
			remove { Events.RemoveHandler (MouseDoubleClickEvent, value); }
		}
		public event NodeEventHandler MouseDown
		{
			add { Events.AddHandler (MouseDownEvent, value); }
			remove { Events.RemoveHandler (MouseDownEvent, value); }
		}
		public event NodeEventHandler MouseEnter
		{
			add { Events.AddHandler (MouseEnterEvent, value); }
			remove { Events.RemoveHandler (MouseEnterEvent, value); }
		}
		public event NodeEventHandler MouseLeave
		{
			add { Events.AddHandler (MouseLeaveEvent, value); }
			remove { Events.RemoveHandler (MouseLeaveEvent, value); }
		}
		public event NodeEventHandler MouseMove
		{
			add { Events.AddHandler (MouseMoveEvent, value); }
			remove { Events.RemoveHandler (MouseMoveEvent, value); }
		}
		public event NodeEventHandler MouseUp
		{
			add { Events.AddHandler (MouseUpEvent, value); }
			remove { Events.RemoveHandler (MouseUpEvent, value); }
		}
		public event EventHandler Focus
		{
			add { Events.AddHandler (FocusEvent, value); }
			remove { Events.RemoveHandler (FocusEvent, value); }
		}
		public event EventHandler Blur
		{
			add { Events.AddHandler (BlurEvent, value); }
			remove { Events.RemoveHandler (BlurEvent, value); }
		}
		public event CreateNewWindowEventHandler CreateNewWindow
		{
			add { Events.AddHandler (CreateNewWindowEvent, value); }
			remove { Events.RemoveHandler (CreateNewWindowEvent, value); }
		}
		public event AlertEventHandler Alert
		{
			add { Events.AddHandler (AlertEvent, value); }
			remove { Events.RemoveHandler (AlertEvent, value); }
		}
		public event EventHandler Transferring
		{
			add { Events.AddHandler (TransferringEvent, value); }
			remove { Events.RemoveHandler (TransferringEvent, value); }
		}
		public event EventHandler DocumentCompleted
		{
			add { Events.AddHandler (DocumentCompletedEvent, value); }
			remove { Events.RemoveHandler (DocumentCompletedEvent, value); }
		}
		public event EventHandler Completed
		{
			add { Events.AddHandler (CompletedEvent, value); }
			remove { Events.RemoveHandler (CompletedEvent, value); }
		}
		public event EventHandler Loaded
		{
			add { Events.AddHandler (LoadEvent, value); }
			remove { Events.RemoveHandler (LoadEvent, value); }
		}
		public event EventHandler Unloaded
		{
			add { Events.AddHandler (UnloadEvent, value); }
			remove { Events.RemoveHandler (UnloadEvent, value); }
		}



		internal static object GenericEvent = new object ();
		public event EventHandler Generic
		{
			add { Events.AddHandler (GenericEvent, value); }
			remove { Events.RemoveHandler (GenericEvent, value); }
		}

		#endregion


	}
}

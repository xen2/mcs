// 
// System.Web.Services.Protocols.SoapHttpClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Net;
using System.Web;
using System.Reflection;
using System.Web.Services;
using System.Diagnostics;

namespace System.Web.Services.Protocols {
	public class SoapHttpClientProtocol : HttpWebClientProtocol {

		#region Constructors

		public SoapHttpClientProtocol () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		protected IAsyncResult BeginInvoke (string methodName, object[] parameters, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Discover ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected object[] EndInvoke (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		protected override WebRequest GetWebRequest (Uri uri)
		{
			WebRequest request = WebRequest.Create (uri);
			request.Method = "POST";

			return request;
		}

		//
		// Just for debugging
		//
		void DumpStackFrames ()
		{
			StackTrace st = new StackTrace ();

			for (int i = 0; i < st.FrameCount; i++){
				StackFrame sf = st.GetFrame (i);
				Console.WriteLine ("At frame: {0} {1}", i, sf.GetMethod ().Name);
			}
		}
		
		//
		// The `method_name' should be the name of our invoker, this is only used
		// for sanity checks, nothing else
		//
		MethodInfo GetCallerMethod (string method_name)
		{
			MethodInfo mi;
#if StackFrameWorks
			StackFrame stack_trace = new StackFrame (5, false);
			mi = (MethodInfo) stack_frame.GetMethod ();

#else
			//
			// Temporary hack: look for a type which is not this type
			//
			StackTrace st = new StackTrace ();
			mi = null;
			for (int i = 0; i < st.FrameCount; i++){
				StackFrame sf = st.GetFrame (i);
				mi = (MethodInfo) sf.GetMethod ();
				if (mi.DeclaringType != typeof (SoapHttpClientProtocol))
					break;
			}
#endif
			//
			// A few sanity checks, just in case the code moves around later
			//
			if (!mi.DeclaringType.IsSubclassOf (typeof (System.Web.Services.Protocols.SoapHttpClientProtocol)))
				throw new Exception ("We are pointing to the wrong method (T=" + mi.DeclaringType + ")");

			if (mi.DeclaringType == typeof (System.Web.Services.Protocols.SoapHttpClientProtocol))
				throw new Exception ("We are pointing to the wrong method (we are pointing to our Invoke)");

			if (mi.Name != method_name)
				throw new Exception ("The method we point to is: " + mi.Name);

			return mi;
		}
		
		SoapClientMessage CreateMessage (string method_name, object [] parameters)
		{
			MethodInfo mi = GetCallerMethod (method_name);
			object [] attributes = mi.GetCustomAttributes (typeof (System.Web.Services.Protocols.SoapDocumentMethodAttribute), false);
			SoapDocumentMethodAttribute sma = (SoapDocumentMethodAttribute) attributes [0];

			Console.WriteLine ("SMAA:    " + sma.Action);
			Console.WriteLine ("Binding: " + sma.Binding);
			Console.WriteLine ("OneWay:  " + sma.OneWay);
			Console.WriteLine ("PStyle:  " + sma.ParameterStyle);
			Console.WriteLine ("REN:     " + sma.RequestElementName);
			Console.WriteLine ("REN:     " + sma.RequestElementName);
			
			SoapClientMessage message = new SoapClientMessage (
				this, sma, new LogicalMethodInfo (mi), sma.OneWay, Url);

			return message;
		}
		
		void SendMessage (WebRequest request, SoapClientMessage message)
		{
			WebHeaderCollection headers = request.Headers;
			headers.Add ("SOAPAction", message.Action);

			// create envelope
			// create body
			// serialize arguments
			
		}
		
		protected object[] Invoke (string method_name, object[] parameters)
		{
			SoapClientMessage message = CreateMessage (method_name, parameters);
			WebRequest request = GetWebRequest (uri);
			Stream s = request.GetRequestStream ();


			try {
				SendMessage (request, message);
			} finally {
				s.Close ();
			}

			return null;
		}

		#endregion // Methods
	}
}

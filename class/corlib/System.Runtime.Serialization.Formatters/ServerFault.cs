//
// System.Runtime.Serialization.Formatters.ServerFault.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
// 	   Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//
// 2002 (C) Copyright, Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Remoting.Metadata;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization.Formatters {

	[Serializable]
	[SoapType()]
	public sealed class ServerFault
	{
		string exceptionType;
		string message;
		string stackTrace;
		Exception exception;

		public ServerFault (string exceptionType, string message,
				  string stackTrace)
		{
			this.exceptionType = exceptionType;
			this.message = message;
			this.stackTrace = stackTrace;
		}

		public string ExceptionType {
			get { return exceptionType; }
			set { exceptionType = value; }
		}

		public string ExceptionMessage {
			get { return message; }
			set { message = value; }
		}

		public string StackTrace {
			get { return stackTrace; }
			set { stackTrace = value; }
		}
	}
}

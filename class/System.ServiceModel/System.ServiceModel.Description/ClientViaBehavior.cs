//
// ClientViaBehavior.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Description
{
	public class ClientViaBehavior : IEndpointBehavior
	{
		Uri uri;

		public ClientViaBehavior (Uri uri)
		{
			this.uri = uri;
		}

		public Uri Uri {
			get { return uri; }
			set { uri = value; }
		}

		void IEndpointBehavior.AddBindingParameters (ServiceEndpoint endpoint,
			BindingParameterCollection parameters)
		{
			throw new NotImplementedException ();
		}

		void IEndpointBehavior.ApplyDispatchBehavior (ServiceEndpoint endpoint,
			EndpointDispatcher dispatcher)
		{
			throw new NotImplementedException ();
		}

		void IEndpointBehavior.ApplyClientBehavior (
			ServiceEndpoint endpoint, ClientRuntime behavior)
		{
			behavior.Via = Uri;
		}

		void IEndpointBehavior.Validate (ServiceEndpoint endpoint)
		{
			throw new NotImplementedException ();
		}
	}
}

//
// EndpointDispatcher.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security.Tokens;
using System.Text;

namespace System.ServiceModel.Dispatcher
{
	public class EndpointDispatcher
	{
		EndpointAddress address;
		string contract_name, contract_ns;
		ChannelDispatcher channel_dispatcher;
		MessageFilter address_filter;
		MessageFilter contract_filter;
		int filter_priority;
		DispatchRuntime dispatch_runtime;

		// Umm, this API is ugly, since it or its members will
		// anyways require ServiceEndpoint, those arguments are
		// likely to be replaced by ServiceEndpoint (especially
		// considering about possible EndpointAddress inconsistency).
		public EndpointDispatcher (EndpointAddress address,
			string contractName, string contractNamespace)
		{
			if (contractName == null)
				throw new ArgumentNullException ("contractName");
			if (contractNamespace == null)
				throw new ArgumentNullException ("contractNamespace");
			if (address == null)
				throw new ArgumentNullException ("address");

			this.address = address;
			contract_name = contractName;
			contract_ns = contractNamespace;

			dispatch_runtime = new DispatchRuntime (this);
		}

		public DispatchRuntime DispatchRuntime {
			get { return dispatch_runtime; }
		}

		public string ContractName {
			get { return contract_name; }
		}

		public string ContractNamespace {
			get { return contract_ns; }
		}

		public ChannelDispatcher ChannelDispatcher {
			get { return channel_dispatcher; }
			internal set { channel_dispatcher = value; }
		}

		public MessageFilter AddressFilter {
			get { return address_filter; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				address_filter = value;
			}
		}

		public MessageFilter ContractFilter {
			get { return contract_filter ?? (contract_filter = new MatchAllMessageFilter ()); }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				contract_filter = value;
			}
		}

		public EndpointAddress EndpointAddress {
			get { return address; }
		}

		public int FilterPriority {
			get { return filter_priority; }
			set { filter_priority = value; }
		}
	}
}

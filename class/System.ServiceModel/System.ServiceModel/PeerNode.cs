//
// PeerNode.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005,2009 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel.Channels;

namespace System.ServiceModel
{
	public abstract class PeerNode : IOnlineStatus
	{
		internal PeerNode (string meshId, int port)
		{
			MeshId = meshId;
			Port = port;
		}

		public event EventHandler Offline;
		public event EventHandler Online;

		public bool IsOnline { get; internal set; }

		internal string MeshId { get; private set; }

		internal int NodeId { get; set; }

		internal abstract bool IsOpen { get; }

		public int Port { get; private set; }

		public abstract PeerMessagePropagationFilter MessagePropagationFilter { get; set; }

		internal abstract void Open (TimeSpan timeout);

		public void RefreshConnection ()
		{
		}

		public override string ToString ()
		{
			return String.Format ("MeshId: {0}, Node ID: {1}, Online: {2}, Opened:{3}, Port: {4}", MeshId, NodeId, IsOnline, IsOpen, Port);
		}

		internal void SetOnline ()
		{
			IsOnline = true;
			if (Online != null)
				Online (this, EventArgs.Empty);
		}

		internal void SetOffline ()
		{
			IsOnline = false;
			if (Offline != null)
				Offline (this, EventArgs.Empty);
		}
	}

	internal class PeerNodeImpl : PeerNode
	{
		internal PeerNodeImpl (PeerResolver resolver, string meshId, int port)
			: base (meshId, port)
		{
			this.resolver = resolver;
		}

		PeerResolver resolver;
		ICollection<PeerNodeAddress> addresses;
		object registered_id;

		// FIXME: implement
		public override PeerMessagePropagationFilter MessagePropagationFilter { get; set; }

		internal override bool IsOpen {
			get { return registered_id != null; }
		}

		internal override void Open (TimeSpan timeout)
		{
			DateTime startTime = DateTime.Now;

			int maxAddresses = 3; // FIXME: get it from somewhere

			// FIXME: not sure how I should handle addresses
			int idx = 0;
			foreach (var addr in resolver.Resolve (MeshId, maxAddresses, timeout)) {
				idx++;
				registered_id = resolver.Register (MeshId, addr, timeout - (DateTime.Now - startTime));
				NodeId = idx;
				SetOnline ();
				break;
			}
		}
	}
}

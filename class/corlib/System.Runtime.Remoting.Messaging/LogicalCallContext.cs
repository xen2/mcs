//
// System.Runtime.Remoting.Messaging.LogicalCallContext.cs
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System.Collections;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[MonoTODO]
	[Serializable]
	public sealed class LogicalCallContext : ISerializable, ICloneable
	{
		Hashtable _data;
		CallContextRemotingData _remotingData = new CallContextRemotingData();

		internal LogicalCallContext ()
		{
		}

		internal LogicalCallContext (SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "__RemotingData")
					_remotingData = (CallContextRemotingData) entry.Value;
				else
					SetData (entry.Name, entry.Value);
			}
		}

		public bool HasInfo
		{
			get
			{
				return (_data != null && _data.Count > 0);
			}
		}

		public void FreeNamedDataSlot (string name)
		{
			if (_data != null)
				_data.Remove (name);
		}

		public object GetData (string name)
		{
			if (_data != null) return _data [name];
			else return null;
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("__RemotingData", _remotingData);
			if (_data != null)
			{
				foreach (DictionaryEntry de in _data)
					info.AddValue ((string)de.Key, de.Value);
			}
		}

		public void SetData (string name, object data)
		{
			if (_data == null) _data = new Hashtable ();
			_data [name] = data;
		}

		public object Clone ()
		{
			LogicalCallContext nc = new LogicalCallContext ();
			nc._remotingData = (CallContextRemotingData) _remotingData.Clone ();
			if (_data != null)
			{
				nc._data = new Hashtable ();
				foreach (DictionaryEntry de in _data)
					nc._data [de.Key] = de.Value;
			}
			return nc;
		}

		internal Hashtable Datastore
		{
			get { return _data; }
		}
	}


	internal class CallContextRemotingData : ICloneable
	{
		string _logicalCallID;

		public string LogicalCallID
		{
			get { return _logicalCallID; }
			set { _logicalCallID = value; }
		}

		public object Clone ()
		{
			CallContextRemotingData data = new CallContextRemotingData ();
			data._logicalCallID = _logicalCallID;
			return data;
		}
}
}


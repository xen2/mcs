using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Simple;
using System.IO;


// compile with:
// csc -r:../../lib/System.Runtime.Remoting.dll example.cs

class Test : MarshalByRefObject {

	public int test_function (int a, bool b)
	{
		Console.WriteLine ("test function called: " + b);
		return a + 1;
	}
	
	static int Main () {

		Test t1 = new Test ();
		ObjRef myref = RemotingServices.Marshal (t1, "/test");
		Console.WriteLine ("OBJREF: " + myref.URI);
		
		string url = "simple://localhost:8000/test";
		string uri;
		
		IChannelReceiver svr_chnl = new SimpleServerChannel (8000);
		
		IChannel chnl = new SimpleClientChannel ();

		Console.WriteLine ("Channel name: " + chnl.ChannelName);
		Console.WriteLine ("Channel priority: " + chnl.ChannelPriority);
		Console.WriteLine ("URI: " + chnl.Parse (url, out uri));
		Console.WriteLine ("URI: " + uri);
		
		ChannelServices.RegisterChannel (chnl);

		Test tp = (Test)RemotingServices.Connect (typeof (Test), url);

		int res = tp.test_function (4, true);

		Console.WriteLine ("RESULT: " + res);
		
		svr_chnl.StopListening (null);
		
		return 0;
	}
}

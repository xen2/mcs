//
// SystemEvents.cs
//
// Author:
//  Johannes Roith (johannes@jroith.de)
//
// (C) 2002 Johannes Roith
//

namespace Microsoft.Win32 {
	/// <summary>
	/// </summary>
public sealed class SystemEvents : System.EventArgs{

	[MonoTODO]
	public static void InvokeOnEventsThread(System.Delegate method)
	{
		throw new System.NotImplementedException ();
	}



	[MonoTODO]
	public static event System.EventHandler DisplaySettingsChanged {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
		
	[MonoTODO]
	public static event System.EventHandler EventsThreadShutdown {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
	
	[MonoTODO]
	public static event System.EventHandler InstalledFontsChanged {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
	
	[MonoTODO]
	public static event System.EventHandler LowMemory {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
	
	[MonoTODO]
	public static event System.EventHandler PaletteChanged {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
	
	[MonoTODO]
	public static event System.EventHandler PowerModeChanged {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
	
	[MonoTODO]
		public static event SessionEndedEventHandler SessionEnded {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
	
	[MonoTODO]
	public static event System.EventHandler SessionEnding {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	

	[MonoTODO]
	public static event System.EventHandler TimeChanged {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
	
	[MonoTODO]
	public static event System.EventHandler TimerElapsed {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
	
	[MonoTODO]
	public static event System.EventHandler UserPreferenceChanged {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	
	
	[MonoTODO]
	public static event System.EventHandler UserPreferenceChanging {
		add 	{ throw new System.NotImplementedException ();}
		remove 	{ throw new System.NotImplementedException ();}
	}
	

}


}

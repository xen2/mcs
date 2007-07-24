//
// System.Diagnostics.Process.cs
//
// Authors:
// 	Dick Porter (dick@ximian.com)
// 	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2003 Andreas Nahr
// (c) 2004,2005,2006 Novell, Inc. (http://www.novell.com)
//

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

using System.IO;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Collections;
using System.Threading;

namespace System.Diagnostics {

	[DefaultEvent ("Exited"), DefaultProperty ("StartInfo")]
	[Designer ("System.Diagnostics.Design.ProcessDesigner, " + Consts.AssemblySystem_Design)]
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class Process : Component 
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct ProcInfo 
		{
			public IntPtr process_handle;
			/* If thread_handle is ever needed for
			 * something, take out the CloseHandle() in
			 * the Start_internal icall in
			 * mono/metadata/process.c
			 */
			public IntPtr thread_handle;
			public int pid; // Contains -GetLastError () on failure.
			public int tid;
			public string [] envKeys;
			public string [] envValues;
		};
		
		IntPtr process_handle;
		int pid;
		bool enableRaisingEvents;
		bool already_waiting;
		ISynchronizeInvoke synchronizingObject;
		EventHandler exited_event;
		IntPtr stdout_rd;
		IntPtr stderr_rd;
		
		/* Private constructor called from other methods */
		private Process(IntPtr handle, int id) {
			process_handle=handle;
			pid=id;
		}
		
		public Process ()
		{
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Base process priority.")]
		public int BasePriority {
			get {
				return(0);
			}
		}

		void StartExitCallbackIfNeeded ()
		{
			bool start = (!already_waiting && enableRaisingEvents && exited_event != null);
			if (start && process_handle != IntPtr.Zero && !HasExited) {
				WaitOrTimerCallback cb = new WaitOrTimerCallback (CBOnExit);
				ProcessWaitHandle h = new ProcessWaitHandle (process_handle);
				ThreadPool.RegisterWaitForSingleObject (h, cb, this, -1, true);
				already_waiting = true;
			}
		}

		[DefaultValue (false), Browsable (false)]
		[MonitoringDescription ("Check for exiting of the process to raise the apropriate event.")]
		public bool EnableRaisingEvents {
			get {
				return enableRaisingEvents;
			}
			set { 
				bool prev = enableRaisingEvents;
				enableRaisingEvents = value;
				if (enableRaisingEvents && !prev)
					StartExitCallbackIfNeeded ();
			}

		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int ExitCode_internal(IntPtr handle);

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The exit code of the process.")]
		public int ExitCode {
			get {
				if (process_handle == IntPtr.Zero)
					throw new InvalidOperationException ("Process has not been started.");

				int code = ExitCode_internal (process_handle);
				if (code == 259)
					throw new InvalidOperationException ("The process must exit before " +
									"getting the requested information.");

				return code;
			}
		}

		/* Returns the process start time in Windows file
		 * times (ticks from DateTime(1/1/1601 00:00 GMT))
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static long ExitTime_internal(IntPtr handle);
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The exit time of the process.")]
		public DateTime ExitTime {
			get {
				if (process_handle == IntPtr.Zero)
					throw new InvalidOperationException ("Process has not been started.");

				if (!HasExited)
					throw new InvalidOperationException ("The process must exit before " +
									"getting the requested information.");

				return(DateTime.FromFileTime(ExitTime_internal(process_handle)));
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("Handle for this process.")]
		public IntPtr Handle {
			get {
				return(process_handle);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Handles for this process.")]
		public int HandleCount {
			get {
				return(0);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("Determines if the process is still running.")]
		public bool HasExited {
			get {
				if (process_handle == IntPtr.Zero)
					throw new InvalidOperationException ("Process has not been started.");
					
				int exitcode = ExitCode_internal (process_handle);

				if(exitcode==259) {
					/* STILL_ACTIVE */
					return(false);
				} else {
					return(true);
				}
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Process identifier.")]
		public int Id {
			get {
				if (pid == 0) {
					throw new InvalidOperationException ("Process ID has not been set.");
				}

				return(pid);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The name of the computer running the process.")]
		public string MachineName {
			get {
				return("localhost");
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The main module of the process.")]
		public ProcessModule MainModule {
			get {
				return(this.Modules[0]);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The handle of the main window of the process.")]
		public IntPtr MainWindowHandle {
			get {
				return((IntPtr)0);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The title of the main window of the process.")]
		public string MainWindowTitle {
			get {
				return("null");
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetWorkingSet_internal(IntPtr handle, out int min, out int max);
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool SetWorkingSet_internal(IntPtr handle, int min, int max, bool use_min);

		/* LAMESPEC: why is this an IntPtr not a plain int? */
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum working set for this process.")]
		public IntPtr MaxWorkingSet {
			get {
				if(HasExited) {
					throw new InvalidOperationException("The process " + ProcessName + " (ID " + Id + ") has exited");
				}
				
				int min;
				int max;
				bool ok=GetWorkingSet_internal(process_handle, out min, out max);
				if(ok==false) {
					throw new Win32Exception();
				}
				
				return((IntPtr)max);
			}
			set {
				if(HasExited) {
					throw new InvalidOperationException("The process " + ProcessName + " (ID " + Id + ") has exited");
				}
				
				bool ok=SetWorkingSet_internal(process_handle, 0, value.ToInt32(), false);
				if(ok==false) {
					throw new Win32Exception();
				}
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The minimum working set for this process.")]
		public IntPtr MinWorkingSet {
			get {
				if(HasExited) {
					throw new InvalidOperationException("The process " + ProcessName + " (ID " + Id + ") has exited");
				}
				
				int min;
				int max;
				bool ok=GetWorkingSet_internal(process_handle, out min, out max);
				if(ok==false) {
					throw new Win32Exception();
				}
				
				return((IntPtr)min);
			}
			set {
				if(HasExited) {
					throw new InvalidOperationException("The process " + ProcessName + " (ID " + Id + ") has exited");
				}
				
				bool ok=SetWorkingSet_internal(process_handle, value.ToInt32(), 0, true);
				if(ok==false) {
					throw new Win32Exception();
				}
			}
		}

		/* Returns the list of process modules.  The main module is
		 * element 0.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern ProcessModule[] GetModules_internal();

		private ProcessModuleCollection module_collection;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The modules that are loaded as part of this process.")]
		public ProcessModuleCollection Modules {
			get {
				if(module_collection==null) {
					module_collection=new ProcessModuleCollection(GetModules_internal());
				}

				return(module_collection);
			}
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use NonpagedSystemMemorySize64")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of bytes that are not pageable.")]
		public int NonpagedSystemMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use PagedMemorySize64")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of bytes that are paged.")]
		public int PagedMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use PagedSystemMemorySize64")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of paged system memory in bytes.")]
		public int PagedSystemMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use PeakPagedMemorySize64")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of paged memory used by this process.")]
		public int PeakPagedMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use PeakVirtualMemorySize64")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of virtual memory used by this process.")]
		public int PeakVirtualMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use PeakWorkingSet64")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of system memory used by this process.")]
		public int PeakWorkingSet {
			get {
				return(0);
			}
		}

#if NET_2_0
		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of bytes that are not pageable.")]
		public long NonpagedSystemMemorySize64 {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The number of bytes that are paged.")]
		public long PagedMemorySize64 {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of paged system memory in bytes.")]
		public long PagedSystemMemorySize64 {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of paged memory used by this process.")]
		public long PeakPagedMemorySize64 {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of virtual memory used by this process.")]
		public long PeakVirtualMemorySize64 {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The maximum amount of system memory used by this process.")]
		public long PeakWorkingSet64 {
			get {
				return(0);
			}
		}
#endif

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Process will be of higher priority while it is actively used.")]
		public bool PriorityBoostEnabled {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The relative process priority.")]
		public ProcessPriorityClass PriorityClass {
			get {
				return(ProcessPriorityClass.Normal);
			}
			set {
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of memory exclusively used by this process.")]
		public int PrivateMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of processing time spent in the OS core for this process.")]
		public TimeSpan PrivilegedProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string ProcessName_internal(IntPtr handle);
		
		private string process_name=null;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The name of this process.")]
		public string ProcessName {
			get {
				if(process_name==null) {
					process_name=ProcessName_internal(process_handle);
					/* If process_name is _still_
					 * null, assume the process
					 * has exited
					 */
					if(process_name==null) {
						throw new SystemException("The process has exited");
					}
					
					/* Strip the suffix (if it
					 * exists) simplistically
					 * instead of removing any
					 * trailing \.???, so we dont
					 * get stupid results on sane
					 * systems
					 */
					if(process_name.EndsWith(".exe") ||
					   process_name.EndsWith(".bat") ||
					   process_name.EndsWith(".com")) {
						process_name=process_name.Substring(0, process_name.Length-4);
					}
				}
				return(process_name);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Allowed processor that can be used by this process.")]
		public IntPtr ProcessorAffinity {
			get {
				return((IntPtr)0);
			}
			set {
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("Is this process responsive.")]
		public bool Responding {
			get {
				return(false);
			}
		}

		private StreamReader error_stream=null;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The standard error stream of this process.")]
		public StreamReader StandardError {
			get {
				if (error_stream == null) {
					throw new InvalidOperationException("Standard error has not been redirected");
				}
#if NET_2_0
				if ((async_mode & AsyncModes.AsyncError) != 0)
					throw new InvalidOperationException ("Cannot mix asynchronous and synchonous reads.");

				async_mode |= AsyncModes.SyncError;
#endif

				return(error_stream);
			}
		}

		private StreamWriter input_stream=null;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The standard input stream of this process.")]
		public StreamWriter StandardInput {
			get {
				if (input_stream == null) {
					throw new InvalidOperationException("Standard input has not been redirected");
				}

				return(input_stream);
			}
		}

		private StreamReader output_stream=null;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The standard output stream of this process.")]
		public StreamReader StandardOutput {
			get {
				if (output_stream == null) {
					throw new InvalidOperationException("Standard output has not been redirected");
				}
#if NET_2_0
				if ((async_mode & AsyncModes.AsyncOutput) != 0)
					throw new InvalidOperationException ("Cannot mix asynchronous and synchonous reads.");

				async_mode |= AsyncModes.SyncOutput;
#endif

				return(output_stream);
			}
		}

		private ProcessStartInfo start_info=null;
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content), Browsable (false)]
		[MonitoringDescription ("Information for the start of this process.")]
		public ProcessStartInfo StartInfo {
			get {
				if(start_info==null) {
					start_info=new ProcessStartInfo();
				}
				
				return(start_info);
			}
			set {
				if(value==null) {
					throw new ArgumentException("value is null");
				}
				
				start_info=value;
			}
		}

		/* Returns the process start time in Windows file
		 * times (ticks from DateTime(1/1/1601 00:00 GMT))
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static long StartTime_internal(IntPtr handle);
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The time this process started.")]
		public DateTime StartTime {
			get {
				return(DateTime.FromFileTime(StartTime_internal(process_handle)));
			}
		}

		[DefaultValue (null), Browsable (false)]
		[MonitoringDescription ("The object that is used to synchronize event handler calls for this process.")]
		public ISynchronizeInvoke SynchronizingObject {
			get { return synchronizingObject; }
			set { synchronizingObject = value; }
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		[MonitoringDescription ("The number of threads of this process.")]
		public ProcessThreadCollection Threads {
			get {
				return(null);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The total CPU time spent for this process.")]
		public TimeSpan TotalProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The CPU time spent for this process in user mode.")]
		public TimeSpan UserProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use VirtualMemorySize64")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of virtual memory currently used for this process.")]
		public int VirtualMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use WorkingSet64")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of physical memory currently used for this process.")]
		public int WorkingSet {
			get {
				return(0);
			}
		}

#if NET_2_0
		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of virtual memory currently used for this process.")]
		public long VirtualMemorySize64 {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The amount of physical memory currently used for this process.")]
		public long WorkingSet64 {
			get {
				return(0);
			}
		}
#endif

		public void Close()
		{
			Dispose (true);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Kill_internal (IntPtr handle, int signo);

		/* int kill -> 1 KILL, 2 CloseMainWindow */
		bool Close (int signo)
		{
			if (process_handle == IntPtr.Zero)
				throw new SystemException ("No process to kill.");

			int exitcode = ExitCode_internal (process_handle);
			if (exitcode != 259)
				throw new InvalidOperationException ("The process already finished.");

			return Kill_internal (process_handle, signo);
		}

		public bool CloseMainWindow ()
		{
			return Close (2);
		}

		[MonoTODO]
		public static void EnterDebugMode() {
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr GetProcess_internal(int pid);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int GetPid_internal();

		public static Process GetCurrentProcess()
		{
			int pid = GetPid_internal();
			IntPtr proc = GetProcess_internal(pid);
			
			if (proc == IntPtr.Zero) {
				throw new SystemException("Can't find current process");
			}

			return (new Process (proc, pid));
		}

		public static Process GetProcessById(int processId)
		{
			IntPtr proc = GetProcess_internal(processId);
			
			if (proc == IntPtr.Zero) {
				throw new ArgumentException ("Can't find process with ID " + processId.ToString ());
			}

			return (new Process (proc, processId));
		}

		[MonoTODO ("There is no support for retrieving process information from a remote machine")]
		public static Process GetProcessById(int processId, string machineName) {
			if (machineName == null)
				throw new ArgumentNullException ("machineName");

			if (!IsLocalMachine (machineName))
				throw new NotImplementedException ();

			return GetProcessById (processId);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int[] GetProcesses_internal();

		public static Process[] GetProcesses()
		{
			int [] pids = GetProcesses_internal ();
			ArrayList proclist = new ArrayList ();
			
			for (int i = 0; i < pids.Length; i++) {
				try {
					proclist.Add (GetProcessById (pids [i]));
				} catch (SystemException) {
					/* The process might exit
					 * between
					 * GetProcesses_internal and
					 * GetProcessById
					 */
				}
			}

			return ((Process []) proclist.ToArray (typeof (Process)));
		}

		[MonoTODO ("There is no support for retrieving process information from a remote machine")]
		public static Process[] GetProcesses(string machineName) {
			if (machineName == null)
				throw new ArgumentNullException ("machineName");

			if (!IsLocalMachine (machineName))
				throw new NotImplementedException ();

			return GetProcesses ();
		}

		public static Process[] GetProcessesByName(string processName)
		{
			Process [] procs = GetProcesses();
			ArrayList proclist = new ArrayList();
			
			for (int i = 0; i < procs.Length; i++) {
				/* Ignore case */
				if (String.Compare (processName,
						    procs [i].ProcessName,
						    true) == 0) {
					proclist.Add (procs [i]);
				}
			}

			return ((Process[]) proclist.ToArray (typeof(Process)));
		}

		[MonoTODO]
		public static Process[] GetProcessesByName(string processName, string machineName) {
			throw new NotImplementedException();
		}

		public void Kill ()
		{
			Close (1);
		}

		[MonoTODO]
		public static void LeaveDebugMode() {
		}

		public void Refresh ()
	    	{
			// FIXME: should refresh any cached data we might have about
			// the process (currently we have none).
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool ShellExecuteEx_internal(ProcessStartInfo startInfo,
								   ref ProcInfo proc_info);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool CreateProcess_internal(ProcessStartInfo startInfo,
								  IntPtr stdin,
								  IntPtr stdout,
								  IntPtr stderr,
								  ref ProcInfo proc_info);

		private static bool Start_shell (ProcessStartInfo startInfo,
						 Process process)
		{
			ProcInfo proc_info=new ProcInfo();
			bool ret;

			if (startInfo.RedirectStandardInput ||
			    startInfo.RedirectStandardOutput ||
			    startInfo.RedirectStandardError) {
				throw new InvalidOperationException ("UseShellExecute must be false when redirecting I/O.");
			}

			if (startInfo.HaveEnvVars) {
				throw new InvalidOperationException ("UseShellExecute must be false in order to use environment variables.");
			}

			ret = ShellExecuteEx_internal (startInfo,
						       ref proc_info);
			if (!ret) {
				throw new Win32Exception (-proc_info.pid);
			}

			process.process_handle = proc_info.process_handle;
			process.pid = proc_info.pid;

			process.StartExitCallbackIfNeeded ();

			return(ret);
		}

		private static bool Start_noshell (ProcessStartInfo startInfo,
						   Process process)
		{
			if (Path.IsPathRooted (startInfo.FileName) && !File.Exists (startInfo.FileName))
				throw new FileNotFoundException  ("Executable not found: " + startInfo.FileName);
			ProcInfo proc_info=new ProcInfo();
			IntPtr stdin_rd, stdin_wr;
			IntPtr stdout_wr;
			IntPtr stderr_wr;
			bool ret;
			MonoIOError error;

			if (startInfo.HaveEnvVars) {
				string [] strs = new string [startInfo.EnvironmentVariables.Count];
				startInfo.EnvironmentVariables.Keys.CopyTo (strs, 0);
				proc_info.envKeys = strs;

				strs = new string [startInfo.EnvironmentVariables.Count];
				startInfo.EnvironmentVariables.Values.CopyTo (strs, 0);
				proc_info.envValues = strs;
			}

			if (startInfo.RedirectStandardInput == true) {
				ret = MonoIO.CreatePipe (out stdin_rd,
						         out stdin_wr);
				if (ret == false) {
					throw new IOException ("Error creating standard input pipe");
				}
			} else {
				stdin_rd = MonoIO.ConsoleInput;
				/* This is required to stop the
				 * &$*£ing stupid compiler moaning
				 * that stdin_wr is unassigned, below.
				 */
				stdin_wr = (IntPtr)0;
			}

			if (startInfo.RedirectStandardOutput == true) {
				IntPtr out_rd;
				ret = MonoIO.CreatePipe (out out_rd,
						         out stdout_wr);

				process.stdout_rd = out_rd;
				if (ret == false) {
					if (startInfo.RedirectStandardInput == true) {
						MonoIO.Close (stdin_rd, out error);
						MonoIO.Close (stdin_wr, out error);
					}

					throw new IOException ("Error creating standard output pipe");
				}
			} else {
				process.stdout_rd = (IntPtr)0;
				stdout_wr = MonoIO.ConsoleOutput;
			}

			if (startInfo.RedirectStandardError == true) {
				IntPtr err_rd;
				ret = MonoIO.CreatePipe (out err_rd,
						         out stderr_wr);

				process.stderr_rd = err_rd;
				if (ret == false) {
					if (startInfo.RedirectStandardInput == true) {
						MonoIO.Close (stdin_rd, out error);
						MonoIO.Close (stdin_wr, out error);
					}
					if (startInfo.RedirectStandardOutput == true) {
						MonoIO.Close (process.stdout_rd, out error);
						MonoIO.Close (stdout_wr, out error);
					}
					
					throw new IOException ("Error creating standard error pipe");
				}
			} else {
				process.stderr_rd = (IntPtr)0;
				stderr_wr = MonoIO.ConsoleError;
			}
			
			ret = CreateProcess_internal (startInfo,
						      stdin_rd, stdout_wr, stderr_wr,
						      ref proc_info);
			if (!ret) {
				if (startInfo.RedirectStandardInput == true) {
					MonoIO.Close (stdin_rd, out error);
					MonoIO.Close (stdin_wr, out error);
				}

				if (startInfo.RedirectStandardOutput == true) {
					MonoIO.Close (process.stdout_rd, out error);
					MonoIO.Close (stdout_wr, out error);
				}

				if (startInfo.RedirectStandardError == true) {
					MonoIO.Close (process.stderr_rd, out error);
					MonoIO.Close (stderr_wr, out error);
				}

				throw new Win32Exception (-proc_info.pid, 
					"ApplicationName='"+startInfo.FileName+
					"', CommandLine='"+startInfo.Arguments+
					"', CurrentDirectory='"+startInfo.WorkingDirectory+
					"', PATH='"+startInfo.EnvironmentVariables["PATH"]+"'");
			}

			process.process_handle = proc_info.process_handle;
			process.pid = proc_info.pid;
			
			if (startInfo.RedirectStandardInput == true) {
				MonoIO.Close (stdin_rd, out error);
				process.input_stream = new StreamWriter (new FileStream (stdin_wr, FileAccess.Write, true), ConsoleEncoding.InputEncoding);
				process.input_stream.AutoFlush = true;
			}

			if (startInfo.RedirectStandardOutput == true) {
				MonoIO.Close (stdout_wr, out error);
				process.output_stream = new StreamReader (new FileStream (process.stdout_rd, FileAccess.Read, true), ConsoleEncoding.OutputEncoding);
			}

			if (startInfo.RedirectStandardError == true) {
				MonoIO.Close (stderr_wr, out error);
				process.error_stream = new StreamReader (new FileStream (process.stderr_rd, FileAccess.Read, true), ConsoleEncoding.OutputEncoding);
			}

			process.StartExitCallbackIfNeeded ();

			return(ret);
		}

		private static bool Start_common (ProcessStartInfo startInfo,
						  Process process)
		{
			if(startInfo.FileName == null ||
			   startInfo.FileName == "") {
				throw new InvalidOperationException("File name has not been set");
			}
			
			if (startInfo.UseShellExecute) {
				return (Start_shell (startInfo, process));
			} else {
				return (Start_noshell (startInfo, process));
			}
		}
		
		public bool Start() {
			bool ret;

			if (process_handle != IntPtr.Zero) {
				Process_free_internal (process_handle);
				process_handle = IntPtr.Zero;
			}
			ret=Start_common(start_info, this);
			
			return(ret);
		}

		public static Process Start(ProcessStartInfo startInfo) {
			Process process=new Process();
			bool ret;

			process.StartInfo = startInfo;
			ret=Start_common(startInfo, process);
			
			if(ret==true) {
				return(process);
			} else {
				return(null);
			}
		}

		public static Process Start(string fileName) {
                       return Start(new ProcessStartInfo(fileName));
		}

		public static Process Start(string fileName,
					    string arguments) {
                       return Start(new ProcessStartInfo(fileName, arguments));
		}

		public override string ToString() {
			return(base.ToString() +
			       " (" + this.ProcessName + ")");
		}

		/* Waits up to ms milliseconds for process 'handle' to
		 * exit.  ms can be <0 to mean wait forever.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool WaitForExit_internal(IntPtr handle, int ms);

		public void WaitForExit ()
		{
			WaitForExit (-1);
		}

		public bool WaitForExit(int milliseconds) {
			int ms = milliseconds;
			if (ms == int.MaxValue)
				ms = -1;

#if NET_2_0
			DateTime start = DateTime.UtcNow;
			if (async_output != null && !async_output.IsCompleted) {
				if (false == async_output.WaitHandle.WaitOne (ms, false))
					return false; // Timed out

				if (ms >= 0) {
					DateTime now = DateTime.UtcNow;
					ms -= (int) (now - start).TotalMilliseconds;
					if (ms <= 0)
						return false;
					start = now;
				}
			}

			if (async_error != null && !async_error.IsCompleted) {
				if (false == async_error.WaitHandle.WaitOne (ms, false))
					return false; // Timed out

				if (ms >= 0) {
					ms -= (int) (DateTime.UtcNow - start).TotalMilliseconds;
					if (ms <= 0)
						return false;
				}
			}
#endif
			return WaitForExit_internal (process_handle, ms);
		}

		[MonoTODO]
		public bool WaitForInputIdle() {
			return(false);
		}

		[MonoTODO]
		public bool WaitForInputIdle(int milliseconds) {
			return(false);
		}

		private static bool IsLocalMachine (string machineName)
		{
			if (machineName == "." || machineName.Length == 0)
				return true;

			return (string.Compare (machineName, Environment.MachineName, true) == 0);
		}

#if NET_2_0
		public event DataReceivedEventHandler OutputDataReceived;
		public event DataReceivedEventHandler ErrorDataReceived;

		void OnOutputDataReceived (string str)
		{
			if (OutputDataReceived != null)
				OutputDataReceived (this, new DataReceivedEventArgs (str));
		}

		void OnErrorDataReceived (string str)
		{
			if (ErrorDataReceived != null)
				ErrorDataReceived (this, new DataReceivedEventArgs (str));
		}

		[Flags]
		enum AsyncModes {
			NoneYet = 0,
			SyncOutput = 1,
			SyncError = 1 << 1,
			AsyncOutput = 1 << 2,
			AsyncError = 1 << 3
		}

		[StructLayout (LayoutKind.Sequential)]
		sealed class ProcessAsyncReader
		{
			/*
			   The following fields match those of SocketAsyncResult.
			   This is so that changes needed in the runtime to handle
			   asynchronous reads are trivial
			*/
			/* DON'T shuffle fields around. DON'T remove fields */
			public object Sock;
			public IntPtr handle;
			public object state;
			public AsyncCallback callback;
			public ManualResetEvent wait_handle;

			public Exception delayedException;

			public object EndPoint;
			byte [] buffer = new byte [4196];
			public int Offset;
			public int Size;
			public int SockFlags;

			public object acc_socket;
			public int total;
			public bool completed_sync;
			bool completed;
			bool err_out; // true -> stdout, false -> stderr
			internal int error;
			public int operation = 8; // MAGIC NUMBER: see Socket.cs:AsyncOperation
			public object ares;


			// These fields are not in SocketAsyncResult
			Process process;
			Stream stream;
			StringBuilder sb = new StringBuilder ();
			public AsyncReadHandler ReadHandler;

			public ProcessAsyncReader (Process process, IntPtr handle, bool err_out)
			{
				this.process = process;
				this.handle = handle;
				stream = new FileStream (handle, FileAccess.Read, false);
				this.ReadHandler = new AsyncReadHandler (AddInput);
				this.err_out = err_out;
			}

			public void AddInput ()
			{
				lock (this) {
					int nread = stream.Read (buffer, 0, buffer.Length);
					if (nread == 0) {
						completed = true;
						if (wait_handle != null)
							wait_handle.Set ();
						Flush (true);
						return;
					}

					try {
						sb.Append (Encoding.Default.GetString (buffer, 0, nread));
					} catch {
						// Just in case the encoding fails...
						for (int i = 0; i < nread; i++) {
							sb.Append ((char) buffer [i]);
						}
					}

					Flush (false);
					ReadHandler.BeginInvoke (null, this);
				}
			}

			void Flush (bool last)
			{
				if (sb.Length == 0 ||
				    (err_out && process.output_canceled) ||
				    (!err_out && process.error_canceled))
					return;

				string total = sb.ToString ();
				sb.Length = 0;
				string [] strs = total.Split ('\n');
				int len = strs.Length;
				if (len == 0)
					return;

				for (int i = 0; i < len - 1; i++) {
					if (err_out)
						process.OnOutputDataReceived (strs [i]);
					else
						process.OnErrorDataReceived (strs [i]);
				}

				string end = strs [len - 1];
				if (last || (len == 1 && end == "")) {
					if (err_out) {
						process.OnOutputDataReceived (end);
					} else {
						process.OnErrorDataReceived (end);
					}
				} else {
					sb.Append (end);
				}
			}

			public bool IsCompleted {
				get { return completed; }
			}

			public WaitHandle WaitHandle {
				get {
					lock (this) {
						if (wait_handle == null)
							wait_handle = new ManualResetEvent (completed);
						return wait_handle;
					}
				}
			}
		}

		AsyncModes async_mode;
		bool output_canceled;
		bool error_canceled;
		ProcessAsyncReader async_output;
		ProcessAsyncReader async_error;
		delegate void AsyncReadHandler ();

		[ComVisibleAttribute(false)] 
		public void BeginOutputReadLine ()
		{
			if (process_handle == IntPtr.Zero || output_stream == null || StartInfo.RedirectStandardOutput == false)
				throw new InvalidOperationException ("Standard output has not been redirected or process has not been started.");

			if ((async_mode & AsyncModes.SyncOutput) != 0)
				throw new InvalidOperationException ("Cannot mix asynchronous and synchonous reads.");

			async_mode |= AsyncModes.AsyncOutput;
			output_canceled = false;
			if (async_output == null) {
				async_output = new ProcessAsyncReader (this, stdout_rd, true);
				async_output.ReadHandler.BeginInvoke (null, async_output);
			}
		}

		[ComVisibleAttribute(false)] 
		public void CancelOutputRead ()
		{
			if (process_handle == IntPtr.Zero || output_stream == null || StartInfo.RedirectStandardOutput == false)
				throw new InvalidOperationException ("Standard output has not been redirected or process has not been started.");

			if ((async_mode & AsyncModes.SyncOutput) != 0)
				throw new InvalidOperationException ("OutputStream is not enabled for asynchronous read operations.");

			if (async_output == null)
				throw new InvalidOperationException ("No async operation in progress.");

			output_canceled = true;
		}

		[ComVisibleAttribute(false)] 
		public void BeginErrorReadLine ()
		{
			if (process_handle == IntPtr.Zero || error_stream == null || StartInfo.RedirectStandardError == false)
				throw new InvalidOperationException ("Standard error has not been redirected or process has not been started.");

			if ((async_mode & AsyncModes.SyncError) != 0)
				throw new InvalidOperationException ("Cannot mix asynchronous and synchonous reads.");

			async_mode |= AsyncModes.AsyncError;
			error_canceled = false;
			if (async_error == null) {
				async_error = new ProcessAsyncReader (this, stderr_rd, false);
				async_error.ReadHandler.BeginInvoke (null, async_error);
			}
		}

		[ComVisibleAttribute(false)] 
		public void CancelErrorRead ()
		{
			if (process_handle == IntPtr.Zero || output_stream == null || StartInfo.RedirectStandardOutput == false)
				throw new InvalidOperationException ("Standard output has not been redirected or process has not been started.");

			if ((async_mode & AsyncModes.SyncOutput) != 0)
				throw new InvalidOperationException ("OutputStream is not enabled for asynchronous read operations.");

			if (async_error == null)
				throw new InvalidOperationException ("No async operation in progress.");

			error_canceled = true;
		}
#endif

		[Category ("Behavior")]
		[MonitoringDescription ("Raised when this process exits.")]
		public event EventHandler Exited {
			add {
				if (process_handle != IntPtr.Zero && HasExited) {
					value.BeginInvoke (null, null, null, null);
				} else {
					exited_event = (EventHandler) Delegate.Combine (exited_event, value);
					if (exited_event != null)
						StartExitCallbackIfNeeded ();
				}
			}
			remove {
				exited_event = (EventHandler) Delegate.Remove (exited_event, value);
			}
		}

		// Closes the system process handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Process_free_internal(IntPtr handle);
		
		private bool disposed = false;
		
		protected override void Dispose(bool disposing) {
			// Check to see if Dispose has already been called.
			if(this.disposed == false) {
				this.disposed=true;
				// If this is a call to Dispose,
				// dispose all managed resources.
				if(disposing) {
					// Do stuff here
				}
				
				// Release unmanaged resources

				lock(this) {
					if(process_handle!=IntPtr.Zero) {
						
						Process_free_internal(process_handle);
						process_handle=IntPtr.Zero;
					}

					if (input_stream != null) {
						input_stream.Close();
						input_stream = null;
					}

					if (output_stream != null) {
						output_stream.Close();
						output_stream = null;
					}

					if (error_stream != null) {
						error_stream.Close();
						error_stream = null;
					}
				}
			}
			base.Dispose (disposing);
		}

		~Process ()
		{
			Dispose (false);
		}

		static void CBOnExit (object state, bool unused)
		{
			Process p = (Process) state;
			p.OnExited ();
		}

		protected void OnExited() 
		{
			if (exited_event == null)
				return;

			if (synchronizingObject == null) {
				foreach (EventHandler d in exited_event.GetInvocationList ()) {
					try {
						d (this, EventArgs.Empty);
					} catch {}
				}
				return;
			}
			
			object [] args = new object [] {this, EventArgs.Empty};
			synchronizingObject.BeginInvoke (exited_event, args);
		}

		class ProcessWaitHandle : WaitHandle
		{
			public ProcessWaitHandle (IntPtr handle)
			{
				Handle = handle;
			}

			protected override void Dispose (bool explicitDisposing)
			{
				// Do nothing, we don't own the handle and we won't close it.
			}
		}

		class ConsoleEncoding
		{
			[DllImport ("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
			private static extern int GetConsoleCP ();
			[DllImport ("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
			private static extern int GetConsoleOutputCP ();

			static bool RunningOnWindows {
				get {
					return ((int) Environment.OSVersion.Platform != 4 &&
#if NET_2_0
						Environment.OSVersion.Platform != PlatformID.Unix);
#else
						(int) Environment.OSVersion.Platform != 128);
#endif
				}
			}

			public static Encoding InputEncoding {
				get {
					if(!RunningOnWindows) {
						return Encoding.Default;
					}

#if !NET_2_0
					try {
						return Encoding.GetEncoding (GetConsoleCP ());
					} catch {
						return Encoding.GetEncoding (28591);
					}
#else
					return Console.InputEncoding;
#endif
				}
			}

			public static Encoding OutputEncoding {
				get {
					if(!RunningOnWindows) {
						return Encoding.Default;
					}

#if !NET_2_0
					try {
						return Encoding.GetEncoding (GetConsoleOutputCP ());
					} catch {
						return Encoding.GetEncoding (28591);
					}
#else
					return Console.OutputEncoding;
#endif
				}
			}
		}
	}
}


//
// System.ServiceProcess.ServiceController 
//
// Authors:
//	Marek Safar (marek.safar@seznam.cz)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005, Marek Safar
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
// TODO: check if there's more information to cache (eg. status)
// Start / Stop / ...

using System;
using System.ComponentModel;
using System.Globalization;
#if NET_2_0
using System.Runtime.InteropServices;
#endif
using System.Threading;

namespace System.ServiceProcess
{
	[Designer("System.ServiceProcess.Design.ServiceControllerDesigner, " + Consts.AssemblySystem_Design)]
	public class ServiceController : Component
	{
		private string _name;
		private string _serviceName = string.Empty;
		private string _machineName;
		private string _displayName = string.Empty;
		private readonly ServiceControllerImpl _impl;
		private ServiceController [] _dependentServices;
		private ServiceController [] _servicesDependedOn;

		[MonoTODO ("No unix implementation")]
		public ServiceController ()
		{
			_machineName = ".";
			_name = string.Empty;
			_impl = CreateServiceControllerImpl (this);
		}

		[MonoTODO ("No unix implementation")]
		public ServiceController (string name) : this (name, ".")
		{
		}

		[MonoTODO ("No unix implementation")]
		public ServiceController (string name, string machineName)
		{
			if (name == null || name.Length == 0)
				throw new ArgumentException (string.Format (CultureInfo.CurrentCulture,
					"Invalid value {0} for parameter name.", name));

			ValidateMachineName (machineName);

			_machineName = machineName;
			_name = name;
			_impl = CreateServiceControllerImpl (this);
		}

		[MonoTODO ("No unix implementation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("")]
		public bool CanPauseAndContinue {
			get {
#if NET_2_0
				ValidateServiceName (ServiceName);
#endif
				return _impl.CanPauseAndContinue;
			}
		}

		[MonoTODO ("No unix implementation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("")]
		public bool CanShutdown {
			get
			{
#if NET_2_0
				ValidateServiceName (ServiceName);
#endif
				return _impl.CanShutdown;
			}
		}

		[MonoTODO ("No unix implementation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("")]
		public bool CanStop {
			get
			{
#if NET_2_0
				ValidateServiceName (ServiceName);
#endif
				return _impl.CanStop;
			}
		}

		[MonoTODO ("No unix implementation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("")]
		public ServiceController [] DependentServices {
			get
			{
#if NET_2_0
				ValidateServiceName (ServiceName);
#endif
				if (_dependentServices == null)
					_dependentServices = _impl.DependentServices;
				return _dependentServices;
			}
		}

		[MonoTODO ("No unix implementation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[ServiceProcessDescription ("")]
		public string DisplayName {
			get {
				if (_displayName.Length == 0 && (_serviceName.Length > 0 || _name.Length > 0))
					_displayName = _impl.DisplayName;
				return _displayName;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (_displayName == value)
					return;

				_displayName = value;

				// if display name is modified, then we also need to force a
				// new lookup of the corresponding service name
				_serviceName = string.Empty;

				// you'd expect the DependentServices and ServiceDependedOn cache
				// to be cleared too, but the MS implementation doesn't do this
				//
				// bug submitted:
				// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762

				// release any handles and clear cache
				Close ();
			}
		}

		[MonoTODO ("No unix implementation")]
		[Browsable (false)]
		[DefaultValue (".")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("")]
		public string MachineName {
			get {
				return _machineName;
			}
			set {
				ValidateMachineName (value);

				if (_machineName == value)
					return;

				_machineName = value;

				// you'd expect the DependentServices and ServiceDependedOn cache
				// to be cleared too, but the MS implementation doesn't do this
				//
				// bug submitted:
				// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762

				// release any handles and clear cache
				Close ();
			}
		}

		[MonoTODO ("No unix implementation")]
		[DefaultValue ("")]
		[ReadOnly (true)]
		[RecommendedAsConfigurable (true)]
		[ServiceProcessDescription ("")]
		public string ServiceName {
			get {
				if (_serviceName.Length == 0 && (_displayName.Length > 0 || _name.Length > 0))
					_serviceName = _impl.ServiceName;
				return _serviceName;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (_serviceName == value)
					return;

#if NET_2_0
				ValidateServiceName (value);
#endif

				_serviceName = value;

				// if service name is modified, then we also need to force a
				// new lookup of the corresponding display name
				_displayName = string.Empty;

				// you'd expect the DependentServices and ServiceDependedOn cache
				// to be cleared too, but the MS implementation doesn't do this
				//
				// bug submitted:
				// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=201762

				// release any handles and clear cache
				Close ();
			}
		}

		[MonoTODO ("No unix implementation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("")]
		public ServiceController [] ServicesDependedOn {
			get
			{
#if NET_2_0
				ValidateServiceName (ServiceName);
#endif
				if (_servicesDependedOn == null)
					_servicesDependedOn = _impl.ServicesDependedOn;
				return _servicesDependedOn;
			}
		}

#if NET_2_0
		[MonoTODO]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SafeHandle ServiceHandle 
		{
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		[MonoTODO ("No unix implementation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("")]
		public ServiceType ServiceType {
			get
			{
#if NET_2_0
				ValidateServiceName (ServiceName);
#endif
				return _impl.ServiceType;
			}
		}

		[MonoTODO ("No unix implementation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ServiceProcessDescription ("")]
		public ServiceControllerStatus Status {
			get
			{
#if NET_2_0
				ValidateServiceName (ServiceName);
#endif
				return _impl.Status;
			}
		}

		[MonoTODO ("No unix implementation")]
		public void Close () 
		{
			_impl.Close ();
		}

		[MonoTODO ("No unix implementation")]
		public void Continue ()
		{
#if NET_2_0
			ValidateServiceName (ServiceName);
#endif
			_impl.Continue ();
		}

		[MonoTODO ("No unix implementation")]
		protected override void Dispose (bool disposing)
		{
			_impl.Dispose (disposing);
			base.Dispose (disposing);
		}

		[MonoTODO ("No unix implementation")]
		public void ExecuteCommand (int command)
		{
#if NET_2_0
			ValidateServiceName (ServiceName);
#endif
			_impl.ExecuteCommand (command);
		}

		[MonoTODO ("No unix implementation")]
		public static ServiceController[] GetDevices ()
		{
			return GetDevices (".");
		}

		[MonoTODO ("No unix implementation")]
		public static ServiceController[] GetDevices (string machineName)
		{
			ValidateMachineName (machineName);

			using (ServiceController sc = new ServiceController ("dummy", machineName)) {
				ServiceControllerImpl impl = CreateServiceControllerImpl (sc);
				return impl.GetDevices ();
			}
		}

		[MonoTODO ("No unix implementation")]
		public static ServiceController[] GetServices ()
		{
			return GetServices (".");
		}

		[MonoTODO ("No unix implementation")]
		public static ServiceController[] GetServices (string machineName)
		{
			ValidateMachineName (machineName);

			using (ServiceController sc = new ServiceController ("dummy", machineName)) {
				ServiceControllerImpl impl = CreateServiceControllerImpl (sc);
				return impl.GetServices ();
			}
		}

		[MonoTODO ("No unix implementation")]
		public void Pause ()
		{
#if NET_2_0
			ValidateServiceName (ServiceName);
#endif
			_impl.Pause ();
		}

		[MonoTODO ("No unix implementation")]
		public void Refresh ()
		{
			// MSDN: this method also sets the  ServicesDependedOn and 
			// DependentServices properties to a null reference
			//
			// I assume they wanted to say that the cache for these properties
			// is cleared. Verified by unit tests.
			_dependentServices = null;
			_servicesDependedOn = null;
			_impl.Refresh ();
		}

		[MonoTODO ("No unix implementation")]
		public void Start () 
		{
			Start (new string [0]);
		}

		[MonoTODO ("No unix implementation")]
		public void Start (string [] args)
		{
#if NET_2_0
			ValidateServiceName (ServiceName);
#endif
			_impl.Start (args);
		}

		[MonoTODO ("No unix implementation")]
		public void Stop ()
		{
#if NET_2_0
			ValidateServiceName (ServiceName);
#endif
			_impl.Stop ();
		}

		[MonoTODO ("No unix implementation")]
		public void WaitForStatus (ServiceControllerStatus desiredStatus)
		{
			WaitForStatus (desiredStatus, TimeSpan.MaxValue);
		}

		[MonoTODO ("No unix implementation")]
		public void WaitForStatus (ServiceControllerStatus desiredStatus, TimeSpan timeout)
		{
#if NET_2_0
			ValidateServiceName (ServiceName);
#endif

			DateTime start = DateTime.Now;
			while (Status != desiredStatus) {
				if (timeout  < (DateTime.Now - start))
					throw new TimeoutException ("Time out has expired and the"
						+ " operation has not been completed.");
				Thread.Sleep (100);
				// force refresh of status
				Refresh ();
			}
		}

		internal string Name {
			get {
				return _name;
			}
			set {
				_name = value;
			}
		}

		internal string InternalDisplayName {
			get {
				return _displayName;
			}
			set {
				_displayName = value;
			}
		}

		internal string InternalServiceName {
			get {
				return _serviceName;
			}
			set {
				_serviceName = value;
			}
		}

#if NET_2_0
		private static void ValidateServiceName (string serviceName)
		{
			if (serviceName.Length == 0 || serviceName.Length > 80)
				throw new ArgumentException (string.Format (CultureInfo.CurrentCulture,
					"Service name {0} contains invalid characters, is empty"
					+ " or is too long (max length = 80).", serviceName));
		}
#endif

		private static void ValidateMachineName (string machineName)
		{
			if (machineName == null || machineName.Length == 0)
				throw new ArgumentException (string.Format (CultureInfo.CurrentCulture,
					"MachineName value {0} is invalid.", machineName));
		}

		private static ServiceControllerImpl CreateServiceControllerImpl (ServiceController serviceController)
		{
#if NET_2_0
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
#else
			if ((int) Environment.OSVersion.Platform == 128) {
#endif
				return new UnixServiceController (serviceController);
			} else {
				return new Win32ServiceController (serviceController);
			}
		}
	}
}

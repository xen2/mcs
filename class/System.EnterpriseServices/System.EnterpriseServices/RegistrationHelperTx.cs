// 
// System.EnterpriseServices.RegistrationHelperTx.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[Guid("9e31421c-2f15-4f35-ad20-66fb9d4cd428")]
	[TransactionAttribute (TransactionOption.RequiresNew)]
	public sealed class RegistrationHelperTx : ServicedComponent {

		#region Constructors

		[MonoTODO]
		public RegistrationHelperTx ()
		{
		}

		#endregion

		#region Methods

		[MonoTODO]
		protected internal override void Activate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void Deactivate ()
		{
			throw new NotImplementedException ();
		}

		public void InstallAssembly (string assembly, ref string application, ref string tlb, InstallationFlags installFlags, object sync)
		{
			InstallAssembly (assembly, ref application, null, ref tlb, installFlags, sync);
		}

		[MonoTODO]
		public void InstallAssembly (string assembly, ref string application, string partition, ref string tlb, InstallationFlags installFlags, object sync)
		{
			throw new NotImplementedException ();
		}

#if NET_1_1
		[MonoTODO]
		public void InstallAssemblyFromConfig ([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig, object sync)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public bool IsInTransaction ()
		{
			throw new NotImplementedException ();
		}

		public void UninstallAssembly (string assembly, string application, object sync)
		{
			UninstallAssembly (assembly, application, null, sync);
		}

		[MonoTODO]
		public void UninstallAssembly (string assembly, string application, string partition, object sync)
		{
			throw new NotImplementedException ();
		}

#if NET_1_1
		[MonoTODO]
		public void UninstallAssemblyFromConfig ([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig, object sync)
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion // Methods
	}
}

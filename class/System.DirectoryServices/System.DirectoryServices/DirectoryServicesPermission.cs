/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.,  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

//
// System.DirectoryServices.DirectoryEntry.cs
//
// Copyright (C) 2004  Novell Inc.
//
// Stub implementation written by Raja R Harinath <rharinath@novell.com>
//

using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices
{
	public class DirectoryServicesPermission : ResourcePermissionBase
	{
		DirectoryServicesPermissionEntryCollection entries;

		[MonoTODO]
		public DirectoryServicesPermission ()
		{ throw new NotImplementedException ("System.DirectoryServices.DirectoryServicesPermission()"); }

		[MonoTODO]
		public DirectoryServicesPermission (DirectoryServicesPermissionEntry[] entries)
		{
			this.entries = new DirectoryServicesPermissionEntryCollection ();
			this.entries.AddRange (entries);
		}

 		[MonoTODO]
 		public DirectoryServicesPermission (PermissionState ps)
 		{ throw new NotImplementedException ("System.DirectoryServices.DirectoryServicesPermission(permission_state)"); }

		[MonoTODO]
		public DirectoryServicesPermission (DirectoryServicesPermissionAccess access, string path)
		{
			entries = new DirectoryServicesPermissionEntryCollection ();
			entries.Add (new DirectoryServicesPermissionEntry (access, path));
		}

		public DirectoryServicesPermissionEntryCollection PermissionEntries
		{
			[MonoTODO]
			get { return entries; }
		}
	}
}


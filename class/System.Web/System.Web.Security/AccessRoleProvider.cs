//
// System.Web.Security.AccessRoleProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.Security {
	public class AccessRoleProvider {
		
		[MonoTODO]
		public void AddUsersToRoles (string [] usernames, string [] rolenames)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void CreateRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void DeleteRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string [] GetAllRoles ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string [] GetRolesForUser (string username)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string [] GetUsersInRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void Initialize (string name, NameValueCollection config)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool IsUserInRole (string username, string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveUsersFromRoles (string [] usernames, string [] rolenames)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool RoleExists (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string ApplicationName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual string Description {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual string Name {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif


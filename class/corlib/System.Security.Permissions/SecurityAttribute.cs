//------------------------------------------------------------------------------
// 
// System.Security.Permissions.SecurityAttribute.cs 
//
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
// 
// Author:         Nick Drochak, ndrochak@gol.com
// Created:        2002-01-06 
//
//------------------------------------------------------------------------------

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Security;

namespace System.Security.Permissions {
	[System.AttributeUsage(
		System.AttributeTargets.Assembly 
		| System.AttributeTargets.Class 
		| System.AttributeTargets.Struct 
		| System.AttributeTargets.Constructor 
		| System.AttributeTargets.Method, 
		AllowMultiple=true, 
		Inherited=false)
	]

	[Serializable]
	public abstract class SecurityAttribute : Attribute {

		private SecurityAction m_Action;
		private bool m_Unrestricted;

		public SecurityAttribute (SecurityAction action) 
		{
			Action = action;
		}

		public abstract IPermission CreatePermission ();

		public bool Unrestricted {
			get {
				return m_Unrestricted;
			}
			set {
				m_Unrestricted = value;
			}
		}

		public SecurityAction Action {
			get {
				return m_Action;
			}
			set {
				if (!SecurityAction.IsDefined(typeof(SecurityAction), value)) {
					throw new System.ArgumentException();
				}
				m_Action = value;
			}
		}
	} // public abstract class SecurityAttribute
}  // namespace System.Security.Permissions

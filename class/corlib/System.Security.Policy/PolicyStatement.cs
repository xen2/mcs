//
// System.Security.Policy.PolicyStatement
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//

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

using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
	public sealed class PolicyStatement : ISecurityEncodable, ISecurityPolicyEncodable {
		public PolicyStatement (PermissionSet perms) :
			this (perms, PolicyStatementAttribute.Nothing)
		{
		}

		public PolicyStatement (PermissionSet perms, PolicyStatementAttribute attrs) {
			this.perms = perms;
			this.attrs = attrs;
		}
		
		public PermissionSet PermissionSet {
			get { return perms; }
			set { perms = value; }
		}
		
		public PolicyStatementAttribute Attributes {
			get { return attrs; }
			set { attrs = value; }
		}

		public string AttributeString {
			get { return attrs.ToString ("F"); }
		}

		public PolicyStatement Copy ()
		{
			return new PolicyStatement (perms, attrs);
		}

		// ISecurityEncodable

		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}

		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			SecurityElement permissions = e.SearchForChildByTag ("PermissionSet");

			string attributes = e.Attribute ("Attributes");

			if (attributes != null)
				attrs = (PolicyStatementAttribute) Enum.Parse (
					typeof (PolicyStatementAttribute), attributes);
				
			perms = new PermissionSet (PermissionState.None);
			perms.FromXml (permissions);
		}
		
		public SecurityElement ToXml ()
		{
			return ToXml (null);
		}

		public SecurityElement ToXml (PolicyLevel level)
		{
			SecurityElement element = new SecurityElement ("PolicyStatement");
			element.AddAttribute ("version", "1");

			if (attrs != PolicyStatementAttribute.Nothing)
				element.AddAttribute ("Attributes", attrs.ToString ());
			
			element.AddChild (perms.ToXml ());

			return element;
		}

		private PermissionSet perms;
		private PolicyStatementAttribute attrs;
	}
}

//
// System.Web.Configuration.RootProfilePropertySettingsCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	[ConfigurationCollection (typeof (ProfilePropertySettings), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class RootProfilePropertySettingsCollection : ProfilePropertySettingsCollection
	{
		static ConfigurationPropertyCollection properties;

		ProfileGroupSettingsCollection groupSettings;
		
		static RootProfilePropertySettingsCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public RootProfilePropertySettingsCollection ()
		{
			groupSettings = new ProfileGroupSettingsCollection ();
		}		
		
		public override bool Equals (object obj)
		{
			RootProfilePropertySettingsCollection col = obj as RootProfilePropertySettingsCollection;
			if (col == null)
				return false;

			if (GetType () != col.GetType ())
				return false;

			if (Count != col.Count)
				return false;

			for (int n = 0; n < Count; n++) {
				if (!BaseGet (n).Equals (col.BaseGet (n)))
					return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			int code = 0;
			for (int n = 0; n < Count; n++)
				code += BaseGet (n).GetHashCode ();
			return code;
		}

		protected override bool AllowClear {
			get { return true; }
		}
		
		[ConfigurationProperty ("group")]
		public ProfileGroupSettingsCollection GroupSettings {
			get { return groupSettings; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		protected override bool ThrowOnDuplicate {
			get { return true; }
		}

		// Why override?
		protected override bool IsModified ()
		{
			return base.IsModified ();
		}

		// Why override?
		protected override void ResetModified ()
		{
			base.ResetModified ();
		}
		
		protected override void Reset (ConfigurationElement parentElement)
		{
			base.Reset (parentElement);

			RootProfilePropertySettingsCollection root = (RootProfilePropertySettingsCollection) parentElement;
			if (root == null)
				return;

			GroupSettings.ResetInternal (root.GroupSettings);
		}
	}
}

#endif

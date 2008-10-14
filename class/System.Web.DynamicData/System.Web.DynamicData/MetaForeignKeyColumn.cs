//
// MetaForeignKeyColumn.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class MetaForeignKeyColumn : MetaColumn
	{
		internal MetaForeignKeyColumn ()
		{
		}

		[MonoTODO]
		public ReadOnlyCollection<string> ForeignKeyNames { get; private set; }

		[MonoTODO]
		public bool IsPrimaryKeyInThisTable { get; private set; }

		[MonoTODO]
		public MetaTable ParentTable { get; private set; }

		[MonoTODO]
		public void ExtractForeignKey (IDictionary dictionary, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetForeignKeyDetailsPath (object row)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetForeignKeyPath (string action, object row)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetForeignKeyPath (string action, object row, string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetForeignKeyString (object row)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IList<object> GetForeignKeyValues (object row)
		{
			throw new NotImplementedException ();
		}
	}
}

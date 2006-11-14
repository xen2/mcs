//
// System.Web.Compilation.AggregateCacheDependency
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace System.Web.Caching 
{
	public sealed class AggregateCacheDependency : CacheDependency
	{
		private List <CacheDependency> dependencies;
		
		public AggregateCacheDependency ()
		{
			dependencies = new List <CacheDependency> ();
		}

		public void Add (params CacheDependency [] dependencies)
		{
			if (dependencies == null)
				throw new ArgumentNullException ("dependencies");
			foreach (CacheDependency dep in dependencies)
				if (dep == null || dep.IsUsed)
					throw new InvalidOperationException ("Cache dependency already in use");

			bool somethingChanged = false;
			lock (dependencies) {
				this.dependencies.AddRange (dependencies);
				foreach (CacheDependency dep in dependencies)
					if (dep.HasChanged) {
						somethingChanged = true;
						break;
					}
			}
			base.Start = DateTime.UtcNow;
			if (somethingChanged)
				base.SignalDependencyChanged ();
		}

		public override string GetUniqueID ()
		{
			if (dependencies == null)
				return null;
			
			StringBuilder sb = new StringBuilder ();
			lock (dependencies) {
				string depid = null;
				foreach (CacheDependency dep in dependencies) {
					depid = dep.GetUniqueID ();
					if (depid == null || depid.Length == 0)
						return null;
					sb.AppendFormat ("{0};", depid);
				}
			}
			return sb.ToString ();
		}
	}
}
#endif


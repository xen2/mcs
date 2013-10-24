//
// ProjectItem.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// (C) 2011 Leszek Ciesielski
// Copyright (C) 2011,2013 Xamarin Inc.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Construction;
using System.IO;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Evaluation
{
	[DebuggerDisplay ("{ItemType}={EvaluatedInclude} [{UnevaluatedInclude}] #DirectMetadata={DirectMetadataCount}")]
	public class ProjectItem
	{
		internal ProjectItem (Project project, ProjectItemElement xml, string evaluatedInclude)
		{
			this.project = project;
			this.xml = xml;
			if (project.ItemDefinitions.ContainsKey (ItemType))
				foreach (var md in project.ItemDefinitions [ItemType].Metadata)
					metadata.Add (md);
			foreach (var item in xml.Metadata)
				metadata.Add (new ProjectMetadata (project, ItemType, metadata, m => metadata.Remove (m), item));
			this.evaluated_include = evaluatedInclude;
			is_imported = project.ProjectCollection.OngoingImports.Any ();			
		}
		
		readonly Project project;
		readonly ProjectItemElement xml;
		readonly List<ProjectMetadata> metadata = new List<ProjectMetadata> ();
		readonly bool is_imported;
		readonly string evaluated_include;

		public ProjectMetadata GetMetadata (string name)
		{
			return metadata.FirstOrDefault (m => m.Name == name);
		}
		
		static readonly char [] path_sep = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
		
		static readonly Dictionary<string,Func<ProjectItem,string>> well_known_metadata = new Dictionary<string, Func<ProjectItem,string>> {
				{"FullPath", p => Path.Combine (p.Project.GetFullPath (p.evaluated_include)) },
				{"RootDir", p => Path.GetPathRoot (p.Project.GetFullPath (p.evaluated_include)) },
				{"Filename", p => Path.GetFileNameWithoutExtension (p.evaluated_include) },
				{"Extension", p => Path.GetExtension (p.evaluated_include) },
				{"RelativeDir", p => {
					var idx = p.evaluated_include.LastIndexOfAny (path_sep);
					return idx < 0 ? string.Empty : p.evaluated_include.Substring (0, idx + 1); }
					},
				{"Directory", p => {
					var fp = p.Project.GetFullPath (p.evaluated_include);
					return Path.GetDirectoryName (fp).Substring (Path.GetPathRoot (fp).Length); }
					},
				// FIXME: implement RecursiveDir: Microsoft.Build.BuildEngine.DirectoryScanner would be reusable with some changes.
				{"Identity", p => p.EvaluatedInclude },
				{"ModifiedTime", p => new FileInfo (p.Project.GetFullPath (p.evaluated_include)).LastWriteTime.ToString ("yyyy-MM-dd HH:mm:ss.fffffff") },
				{"CreatedTime", p => new FileInfo (p.Project.GetFullPath (p.evaluated_include)).CreationTime.ToString ("yyyy-MM-dd HH:mm:ss.fffffff") },
				{"AccessedTime", p => new FileInfo (p.Project.GetFullPath (p.evaluated_include)).LastAccessTime.ToString ("yyyy-MM-dd HH:mm:ss.fffffff") },
				};

		public string GetMetadataValue (string name)
		{
			var wellKnown = well_known_metadata.FirstOrDefault (p => p.Key.Equals (name, StringComparison.OrdinalIgnoreCase));
			if (wellKnown.Value != null)
				return wellKnown.Value (this);
			var m = GetMetadata (name);
			return m != null ? m.EvaluatedValue : string.Empty;
		}

		public bool HasMetadata (string name)
		{
			return GetMetadata (name) != null;
		}

		public bool RemoveMetadata (string name)
		{
			var m = GetMetadata (name);
			if (m == null)
				return false;
			return metadata.Remove (m);
		}

		public void Rename (string name)
		{
			throw new NotImplementedException ();
		}

		public ProjectMetadata SetMetadataValue (string name, string unevaluatedValue)
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<ProjectMetadata> DirectMetadata {
			get {
				var list = new List<ProjectMetadata> ();
				foreach (var xm in xml.Metadata)
					yield return new ProjectMetadata (project, ItemType, list, p => list.Remove (p), xm);
			}
		}

		public int DirectMetadataCount {
			get { return xml.Metadata.Count; }
		}

		public string EvaluatedInclude {
			get { return evaluated_include; }
		}

		public bool IsImported {
			get { return is_imported; }
		}

		public string ItemType {
			get { return Xml.ItemType; }
			set { Xml.ItemType = value; }
		}

		public ICollection<ProjectMetadata> Metadata {
			get { return metadata; }
		}

		public int MetadataCount {
			get { return metadata.Count; }
		}

		public Project Project {
			get { return project; }
		}

		public string UnevaluatedInclude {
			get { return Xml.Include; }
			set { Xml.Include = value; }
		}

		public ProjectItemElement Xml {
			get { return xml; }
		}		
	}
}

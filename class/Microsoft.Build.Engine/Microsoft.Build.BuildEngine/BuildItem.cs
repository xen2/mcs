//
// BuildItem.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class BuildItem {

		XmlElement	itemElement;
		string		finalItemSpec;
		bool		isImported;
		string		itemInclude;
		string		name;
		BuildItemGroup	parentItemGroup;
		//string		recursiveDir;
		IDictionary	evaluatedMetadata;
		IDictionary	unevaluatedMetadata;

		private BuildItem ()
		{
		}
		
		public BuildItem (string itemName, ITaskItem taskItem)
		{
			this.name = itemName;
			this.finalItemSpec = taskItem.ItemSpec;
			this.itemInclude = Utilities.Escape (taskItem.ItemSpec);
			this.evaluatedMetadata = (Hashtable) taskItem.CloneCustomMetadata ();
			this.unevaluatedMetadata = (Hashtable) taskItem.CloneCustomMetadata ();
		}

		public BuildItem (string itemName, string itemInclude)
		{
			if (itemInclude == null)
				throw new ArgumentNullException ("itemInclude");
			if (itemInclude == String.Empty)
				throw new ArgumentException ("Parameter \"itemInclude\" cannot have zero length.");

			this.name = itemName;
			this.finalItemSpec = itemInclude;
			this.unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			this.evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}
		
		internal BuildItem (XmlElement itemElement, BuildItemGroup parentItemGroup)
		{
			this.isImported = false;
			this.unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			this.evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			this.parentItemGroup = parentItemGroup;
			BindToXml (itemElement);
		}
		
		private BuildItem (BuildItem parent)
		{
			this.isImported = parent.isImported;
			this.name = parent.name;
			this.parentItemGroup = parent.parentItemGroup;
			this.unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable (parent.unevaluatedMetadata);
			this.evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable (parent.evaluatedMetadata);
		}
		
		public void CopyCustomMetadataTo (BuildItem destinationItem)
		{
			foreach (DictionaryEntry de in unevaluatedMetadata)
				destinationItem.SetMetadata ((string) de.Key, (string) de.Value);
		}
		
		[MonoTODO]
		public BuildItem Clone ()
		{
			return (BuildItem) this.MemberwiseClone ();
		}

		public string GetEvaluatedMetadata (string metadataName)
		{
			if (evaluatedMetadata.Contains (metadataName))
				return (string) evaluatedMetadata [metadataName];
			else
				return String.Empty;
		}

		public string GetMetadata (string metadataName)
		{
			if (ReservedNameUtils.IsReservedMetadataName (metadataName))
				return ReservedNameUtils.GetReservedMetadata (FinalItemSpec, metadataName);
			else if (unevaluatedMetadata.Contains (metadataName))
				return (string) unevaluatedMetadata [metadataName];
			else
				return String.Empty;
		}
		
		public bool HasMetadata (string metadataName)
		{
			return evaluatedMetadata.Contains (metadataName);
		}

		public void RemoveMetadata (string metadataName)
		{
			if (metadataName == null)
				throw new ArgumentNullException ("metadataName");
			
			if (ReservedNameUtils.IsReservedMetadataName (metadataName))
				throw new ArgumentException ("Can't remove reserved metadata.");
			
			if (evaluatedMetadata.Contains (metadataName))
				evaluatedMetadata.Remove (metadataName);
			
			if (unevaluatedMetadata.Contains (metadataName))
				unevaluatedMetadata.Remove (metadataName);
		}

		public void SetMetadata (string metadataName,
					 string metadataValue)
		{
			SetMetadata (metadataName, metadataValue, false);
		}
		
		public void SetMetadata (string metadataName,
					 string metadataValue,
					 bool treatMetadataValueAsLiteral)
		{
			if (metadataName == null)
				throw new ArgumentNullException ("metadataName");
			
			if (metadataValue == null)
				throw new ArgumentNullException ("metadataValue");
			
			if (ReservedNameUtils.IsReservedMetadataName (metadataName))
				throw new ArgumentException ("Can't modify reserved metadata.");
			
			RemoveMetadata (metadataName);
			
			evaluatedMetadata.Add (metadataName, metadataValue);
				
			if (treatMetadataValueAsLiteral) {	
				unevaluatedMetadata.Add (metadataName, Utilities.Escape (metadataValue));
			} else
				unevaluatedMetadata.Add (metadataName, metadataValue);
		}
		
		private void BindToXml (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
			
			this.itemElement = xmlElement;
			this.name = xmlElement.Name;
			
			if (Include == String.Empty)
				throw new InvalidProjectFileException ("Item must have Include attribute.");
			
			foreach (XmlElement xe in xmlElement.ChildNodes) {
				this.SetMetadata (xe.Name, xe.InnerText);
			}
		}

		internal void Evaluate ()
		{
			DirectoryScanner directoryScanner;
			OldExpression includeExpr, excludeExpr;
			string includes, excludes;

			includeExpr = new OldExpression (parentItemGroup.Project);
			includeExpr.ParseSource (Include);
			excludeExpr = new OldExpression (parentItemGroup.Project);
			excludeExpr.ParseSource (Exclude);
			
			includes = (string) includeExpr.ConvertTo (typeof (string));
			excludes = (string) excludeExpr.ConvertTo (typeof (string));

			this.finalItemSpec = includes;
			
			directoryScanner = new DirectoryScanner ();
			
			directoryScanner.Includes = includes;
			directoryScanner.Excludes = excludes;
			if (parentItemGroup.Project.FullFileName != String.Empty) {
				directoryScanner.BaseDirectory = new DirectoryInfo (Path.GetDirectoryName (parentItemGroup.Project.FullFileName));
			} else {
				directoryScanner.BaseDirectory = new DirectoryInfo (Directory.GetCurrentDirectory ());
			}
			
			directoryScanner.Scan ();
			
			foreach (string matchedFile in directoryScanner.MatchedFilenames) {
				AddEvaluatedItem (matchedFile);
			}
		}
		
		private void AddEvaluatedItem (string itemSpec)
		{
			Project project = this.parentItemGroup.Project;
			
			BuildItem bi = new BuildItem (this);
			bi.finalItemSpec = itemSpec;
			project.EvaluatedItems.AddItem (bi);
			if (project.EvaluatedItemsByName.Contains (bi.name) == false) {
				BuildItemGroup big = new BuildItemGroup (null, project);
				project.EvaluatedItemsByName.Add (bi.name, big);
				big.AddItem (bi);
			} else {
				((BuildItemGroup) project.EvaluatedItemsByName [bi.name]).AddItem (bi);
			}
		}
		
		internal string ConvertToString (OldExpression transform)
		{
			return GetItemSpecFromTransform (transform);
		}
		
		internal ITaskItem ConvertToITaskItem (OldExpression transform)
		{
			TaskItem taskItem;
			taskItem = new TaskItem (GetItemSpecFromTransform (transform), evaluatedMetadata);
			return taskItem;
		}

		private string GetItemSpecFromTransform (OldExpression transform)
		{
			StringBuilder sb;
		
			if (transform == null)
				return finalItemSpec;
			else {
				sb = new StringBuilder ();
				foreach (object o in transform.Collection) {
					if (o is string) {
						sb.Append ((string)o);
					} else if (o is PropertyReference) {
						sb.Append (((PropertyReference)o).ConvertToString ());
					} else if (o is ItemReference) {
						sb.Append (((ItemReference)o).ConvertToString ());
					} else if (o is MetadataReference) {
						sb.Append (GetMetadata (((MetadataReference)o).MetadataName));
					}
				}
				return sb.ToString ();
			}
		}

		public string Condition {
			get {
				if (FromXml)
					return itemElement.GetAttribute ("Condition");
				else
					return String.Empty;
			}
			set {
				if (FromXml)
					itemElement.SetAttribute ("Condition", value);
			}
		}

		public string Exclude {
			get {
				if (FromXml)
					return itemElement.GetAttribute ("Exclude");
				else
					return String.Empty;
			}
			set {
				if (FromXml)
					itemElement.SetAttribute ("Exclude", value);
			}
		}

		public string FinalItemSpec {
			get { return finalItemSpec; }
		}

		public string Include {
			get {
				if (FromXml)
					return itemElement.GetAttribute ("Include");
				else if (itemInclude != null)
					return itemInclude;
				else
					return finalItemSpec;
			}
			set {
				if (FromXml)
					itemElement.SetAttribute ("Include", value);
			}
		}

		public bool IsImported {
			get { return isImported; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		internal bool FromXml {
			get {
				return itemElement != null;
			}
		}
	}
}

#endif

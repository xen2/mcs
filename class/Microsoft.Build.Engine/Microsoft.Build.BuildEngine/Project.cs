//
// Project.cs: Project class
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Build.Framework;
using Mono.XBuild.Framework;

namespace Microsoft.Build.BuildEngine {
	public class Project {
	
		bool				buildEnabled;
		//IDictionary			conditionedProperties;
		Dictionary <string, List <string>>	conditionedProperties;
		string[]			defaultTargets;
		Encoding			encoding;
		BuildItemGroup			evaluatedItems;
		BuildItemGroup			evaluatedItemsIgnoringCondition;
		Dictionary <string, BuildItemGroup>	evaluatedItemsByName;
		Dictionary <string, BuildItemGroup>	evaluatedItemsByNameIgnoringCondition;
		BuildPropertyGroup		evaluatedProperties;
		string				firstTargetName;
		string				fullFileName;
		BuildPropertyGroup		globalProperties;
		GroupingCollection		groups;
		bool				isDirty;
		bool				isValidated;
		BuildItemGroupCollection	itemGroups;
		ImportCollection		imports;
		string				initialTargets;
		Engine				parentEngine;
		BuildPropertyGroupCollection	propertyGroups;
		string				schemaFile;
		TaskDatabase			taskDatabase;
		TargetCollection		targets;
		DateTime			timeOfLastDirty;
		UsingTaskCollection		usingTasks;
		XmlDocument			xmlDocument;
		bool				unloaded;

		static XmlNamespaceManager	manager;
		static string ns = "http://schemas.microsoft.com/developer/msbuild/2003";

		public Project ()
			: this (Engine.GlobalEngine)
		{
		}

		public Project (Engine engine)
		{
			parentEngine  = engine;
			xmlDocument = new XmlDocument ();
			groups = new GroupingCollection ();
			imports = new ImportCollection (this);
			usingTasks = new UsingTaskCollection (this);
			itemGroups = new BuildItemGroupCollection (groups);
			propertyGroups = new BuildPropertyGroupCollection (groups);
			targets = new TargetCollection (this);
			
			taskDatabase = new TaskDatabase ();
			if (engine.DefaultTasksRegistered) {
				taskDatabase.CopyTasks (engine.DefaultTasks);
			}
			
			globalProperties = new BuildPropertyGroup ();
			fullFileName = String.Empty;

			foreach (BuildProperty bp in parentEngine.GlobalProperties) {
				GlobalProperties.AddProperty (bp.Clone (true));
			}

			// You can evaluate an empty project.
			Evaluate ();
		}

		[MonoTODO]
		public void AddNewImport (string importLocation,
					  string importCondition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public BuildItem AddNewItem (string itemName,
					     string itemInclude)
		{
			return AddNewItem (itemName, itemInclude, false);
		}
		
		[MonoTODO]
		public BuildItem AddNewItem (string itemName,
					     string itemInclude,
					     bool treatItemIncludeAsLiteral)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public BuildItemGroup AddNewItemGroup ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public BuildPropertyGroup AddNewPropertyGroup (bool insertAtEndOfProject)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void AddNewUsingTaskFromAssemblyFile (string taskName,
							     string assemblyFile)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void AddNewUsingTaskFromAssemblyName (string taskName,
							     string assemblyName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool Build ()
		{
			return true;
		}
		
		[MonoTODO]
		public bool Build (string targetName)
		{
			return Build (new string [1] { targetName });
		}
		
		[MonoTODO]
		public bool Build (string[] targetNames)
		{
			return Build (targetNames, null);
		}
		
		[MonoTODO]
		public bool Build (string[] targetNames,
				   IDictionary targetOutputs)
		{
			return Build (targetNames, targetOutputs, BuildSettings.None);
		}
		
		[MonoTODO]
		public bool Build (string[] targetNames,
				   IDictionary targetOutputs,
				   BuildSettings buildFlags)
		
		{
			CheckUnloaded ();
			
			if (targetNames.Length == 0) {
				if (defaultTargets != null && defaultTargets.Length != 0)
					targetNames = defaultTargets;
				else if (firstTargetName != null)
					targetNames = new string [1] { firstTargetName};
				else
					return false;
			}
			
			foreach (string target in targetNames) {
				if (!targets.Exists (target))
					throw new InvalidOperationException (String.Format ("Target {0} does not exist.", target));
				
				if (!targets [target].Build ())
					return false;
			}
				
			return true;
		}

		[MonoTODO]
		public string[] GetConditionedPropertyValues (string propertyName)
		{
			if (conditionedProperties.ContainsKey (propertyName))
				return conditionedProperties [propertyName].ToArray ();
			else
				return new string [0];
		}

		public BuildItemGroup GetEvaluatedItemsByName (string itemName)
		{
			if (evaluatedItemsByName.ContainsKey (itemName))
				return evaluatedItemsByName [itemName];
			else
				return null;
		}

		public BuildItemGroup GetEvaluatedItemsByNameIgnoringCondition (string itemName)
		{
			if (evaluatedItemsByNameIgnoringCondition.ContainsKey (itemName))
				return evaluatedItemsByNameIgnoringCondition [itemName];
			else
				return null;
		}

		public string GetEvaluatedProperty (string propertyName)
		{
			return (string) evaluatedProperties [propertyName];
		}

		public string GetProjectExtensions (string id)
		{
			if (id == null || id == String.Empty)
				return String.Empty;

			XmlNode node = xmlDocument.SelectSingleNode (String.Format ("/tns:Project/tns:ProjectExtensions/tns:{0}", id), XmlNamespaceManager);

			// FIXME: InnerXml or InnerText?
			if (node == null)
				return String.Empty;
			else
				return node.InnerXml;
		}


		public void Load (string projectFileName)
		{
			this.fullFileName = Path.GetFullPath (projectFileName);
			try {
				DoLoad (new StreamReader (projectFileName));
			} catch {
				Console.WriteLine ("Failure to load: {0}", projectFileName);
			}
		}
		
		[MonoTODO]
		public void Load (TextReader textReader)
		{
			fullFileName = String.Empty;
			DoLoad (textReader);
		}

		public void LoadXml (string projectXml)
		{
			fullFileName = String.Empty;
			DoLoad (new StringReader (projectXml));
		}


		public void MarkProjectAsDirty ()
		{
			isDirty = true;
		}

		[MonoTODO]
		public void RemoveAllItemGroups ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAllPropertyGroups ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveItem (BuildItem itemToRemove)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveItemGroup (BuildItemGroup itemGroupToRemove)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		// NOTE: does not modify imported projects
		public void RemoveItemGroupsWithMatchingCondition (string matchingCondition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveItemsByName (string itemName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemovePropertyGroup (BuildPropertyGroup propertyGroupToRemove)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		// NOTE: does not modify imported projects
		public void RemovePropertyGroupsWithMatchingCondition (string matchCondition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetBuildStatus ()
		{
			throw new NotImplementedException ();
		}

		public void Save (string projectFileName)
		{
			Save (projectFileName, Encoding.Default);
		}

		public void Save (string projectFileName, Encoding encoding)
		{
			xmlDocument.Save (projectFileName);
		}

		public void Save (TextWriter outTextWriter)
		{
			xmlDocument.Save (outTextWriter);
		}

		[MonoTODO]
		public void SetImportedProperty (string propertyName,
						 string propertyValue,
						 string condition,
						 Project importProject)
		{
			SetImportedProperty (propertyName, propertyValue, condition, importProject,
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup);
		}

		[MonoTODO]
		public void SetImportedProperty (string propertyName,
						 string propertyValue,
						 string condition,
						 Project importedProject,
						 PropertyPosition position)
		{
			SetImportedProperty (propertyName, propertyValue, condition, importedProject,
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup, false);
		}

		[MonoTODO]
		public void SetImportedProperty (string propertyName,
						 string propertyValue,
						 string condition,
						 Project importedProject,
						 PropertyPosition position,
						 bool treatPropertyValueAsLiteral)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetProjectExtensions (string id, string xmlText)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetProperty (string propertyName,
					 string propertyValue)
		{
			SetProperty (propertyName, propertyValue, "true",
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup, false);
		}

		[MonoTODO]
		public void SetProperty (string propertyName,
					 string propertyValue,
					 string condition)
		{
			SetProperty (propertyName, propertyValue, condition,
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup);
		}

		[MonoTODO]
		public void SetProperty (string propertyName,
					 string propertyValue,
					 string condition,
					 PropertyPosition position)
		{
			SetProperty (propertyName, propertyValue, condition,
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup, false);
		}

		[MonoTODO]
		public void SetProperty (string propertyName,
					 string propertyValue,
					 string condition,
					 PropertyPosition position,
					 bool treatPropertyValueAsLiteral)
		{
			throw new NotImplementedException ();
		}

		internal void Unload ()
		{
			unloaded = true;
		}

		internal void CheckUnloaded ()
		{
			if (unloaded)
				throw new InvalidOperationException ("This project object is no longer valid.");
		}
				
		// Does the actual loading.
		private void DoLoad (TextReader textReader)
		{
			ParentEngine.RemoveLoadedProject (this);

			XmlReaderSettings settings = new XmlReaderSettings ();

			if (SchemaFile != null) {
				settings.Schemas.Add (null, SchemaFile);
				settings.ValidationType = ValidationType.Schema;
				settings.ValidationEventHandler += new ValidationEventHandler (ValidationCallBack);
			}

			XmlReader xmlReader = XmlReader.Create (textReader, settings);
			xmlDocument.Load (xmlReader);

			if (xmlDocument.DocumentElement.GetAttribute ("xmlns") != ns) {
				throw new InvalidProjectFileException (
					@"The default XML namespace of the project must be the MSBuild XML namespace." + 
					" If the project is authored in the MSBuild 2003 format, please add " +
					"xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" to the <Project> element. " +
					"If the project has been authored in the old 1.0 or 1.2 format, please convert it to MSBuild 2003 format.  ");
			}
			ProcessXml ();
			ParentEngine.AddLoadedProject (this);
		}

		private void ProcessXml ()
		{
			XmlElement xmlElement = xmlDocument.DocumentElement;
			if (xmlElement.Name != "Project")
				throw new InvalidProjectFileException ("Invalid root element.");
			if (xmlElement.GetAttributeNode ("DefaultTargets") != null)
				defaultTargets = xmlElement.GetAttribute ("DefaultTargets").Split (';');
			else
				defaultTargets = new string [0];
			
			ProcessElements (xmlElement, null);
			
			isDirty = false;
			Evaluate ();
		}
		
		internal void ProcessElements (XmlElement rootElement, ImportedProject ip)
		{
			foreach (XmlNode xn in rootElement.ChildNodes) {
				if (xn is XmlElement) {
					XmlElement xe = (XmlElement) xn;
					switch (xe.Name) {
					case "ProjectExtensions":
						AddProjectExtensions (xe);
						break;
					case "Warning":
					case "Message":
					case "Error":
						AddMessage (xe);
						break;
					case "Target":
						AddTarget (xe, ip);
						break;
					case "UsingTask":
						AddUsingTask (xe, ip);
						break;
					case "Import":
						AddImport (xe, ip);
						break;
					case "ItemGroup":
						AddItemGroup (xe);
						break;
					case "PropertyGroup":
						AddPropertyGroup (xe);
						break;
					case  "Choose":
						AddChoose (xe);
						break;
					default:
						throw new InvalidProjectFileException ("Invalid element in project file.");
					}
				}
			}
		}
		
		internal void Evaluate ()
		{
			evaluatedItems = new BuildItemGroup (null, this);
			evaluatedItemsIgnoringCondition = new BuildItemGroup (null, this);
			evaluatedItemsByName = new Dictionary <string, BuildItemGroup> (StringComparer.InvariantCultureIgnoreCase);
			evaluatedItemsByNameIgnoringCondition = new Dictionary <string, BuildItemGroup> (StringComparer.InvariantCultureIgnoreCase);
			evaluatedProperties = new BuildPropertyGroup ();

			InitializeProperties ();

			// FIXME: questionable order of evaluation
			
			foreach (Import import in Imports)
				import.Evaluate ();
			
			foreach (BuildPropertyGroup bpg in PropertyGroups) {
				if (bpg.Condition == String.Empty)
					bpg.Evaluate ();
				else {
					ConditionExpression ce = ConditionParser.ParseCondition (bpg.Condition);
					if (ce.BoolEvaluate (this))
						bpg.Evaluate ();
				}
			}
			
			foreach (BuildItemGroup big in ItemGroups) {
				if (big.Condition == String.Empty)
					big.Evaluate ();
				else {
					ConditionExpression ce = ConditionParser.ParseCondition (big.Condition);
					if (ce.BoolEvaluate (this))
						big.Evaluate ();
				}
			}

			
			foreach (UsingTask usingTask in UsingTasks)
				usingTask.Evaluate ();
		}

		private void InitializeProperties ()
		{
			BuildProperty bp;

			foreach (BuildProperty gp in GlobalProperties) {
				bp = new BuildProperty (gp.Name, gp.Value, PropertyType.Global);
				EvaluatedProperties.AddProperty (bp);
			}
			
			foreach (DictionaryEntry de in Environment.GetEnvironmentVariables ()) {
				bp = new BuildProperty ((string) de.Key, (string) de.Value, PropertyType.Environment);
				EvaluatedProperties.AddProperty (bp);
			}

			bp = new BuildProperty ("MSBuildBinPath", parentEngine.BinPath, PropertyType.Reserved);
			EvaluatedProperties.AddProperty (bp);
		}
		
		private void AddProjectExtensions (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
		}
		
		private void AddMessage (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
		}
		
		private void AddTarget (XmlElement xmlElement, ImportedProject importedProject)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
			Target target = new Target (xmlElement, this);
			targets.AddTarget (target);
			if (importedProject == null) {
				target.IsImported = false;
				if (firstTargetName == null)
					firstTargetName = target.Name;
			} else
				target.IsImported = true;
		}
		
		private void AddUsingTask (XmlElement xmlElement, ImportedProject importedProject)
		{
			UsingTask usingTask;

			usingTask = new UsingTask (xmlElement, this, importedProject);
			UsingTasks.Add (usingTask);
		}
		
		private void AddImport (XmlElement xmlElement, ImportedProject importingProject)
		{
			Import import;
			
			import = new Import (xmlElement, this, importingProject);
			Imports.Add (import);
		}
		
		private void AddItemGroup (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
			BuildItemGroup big = new BuildItemGroup (xmlElement, this);
			ItemGroups.Add (big);
			//big.Evaluate ();
		}
		
		private void AddPropertyGroup (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
			BuildPropertyGroup bpg = new BuildPropertyGroup (xmlElement, this);
			PropertyGroups.Add (bpg);
			//bpg.Evaluate ();
		}
		
		private void AddChoose (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
				
			BuildChoose bc = new BuildChoose (xmlElement, this);
			groups.Add (bc);
			
		}
		
		private static void ValidationCallBack (object sender, ValidationEventArgs e)
		{
			Console.WriteLine ("Validation Error: {0}", e.Message);
		}
		
		public bool BuildEnabled {
			get {
				return buildEnabled;
			}
			set {
				buildEnabled = value;
			}
		}

		public Encoding Encoding {
			get { return encoding; }
		}

		public string DefaultTargets {
			get {
				return xmlDocument.DocumentElement.GetAttribute ("DefaultTargets");
			}
			set {
				xmlDocument.DocumentElement.SetAttribute ("DefaultTargets", value);
				defaultTargets = value.Split (';');
			}
		}

		public BuildItemGroup EvaluatedItems {
			get { return evaluatedItems; }
		}

		public BuildItemGroup EvaluatedItemsIgnoringCondition {
			get { return evaluatedItemsIgnoringCondition; }
		}
		
		internal IDictionary EvaluatedItemsByName {
			get { return evaluatedItemsByName; }
		}
		
		internal IDictionary EvaluatedItemsByNameIgnoringCondition {
			get { return evaluatedItemsByNameIgnoringCondition; }
		}

		public BuildPropertyGroup EvaluatedProperties {
			get { return evaluatedProperties; }
		}

		public string FullFileName {
			get { return fullFileName; }
			set { fullFileName = value; }
		}

		public BuildPropertyGroup GlobalProperties {
			get { return globalProperties; }
			set {
				if (value == null) {
					throw new ArgumentNullException ("value");
				}
				if (value.FromXml) {
					throw new InvalidOperationException ("Can't do that.");
				}
				globalProperties = value;
			}
		}

		public bool IsDirty {
			get { return isDirty; }
		}

		public bool IsValidated {
			get { return isValidated; }
			set { isValidated = value; }
		}

		public BuildItemGroupCollection ItemGroups {
			get { return itemGroups; }
		}
		
		public ImportCollection Imports {
			get { return imports; }
		}
		
		public string InitialTargets {
			get { return initialTargets; }
			set { initialTargets = value; }
		}

		public Engine ParentEngine {
			get { return parentEngine; }
		}

		public BuildPropertyGroupCollection PropertyGroups {
			get { return propertyGroups; }
		}

		public string SchemaFile {
			get { return schemaFile; }
			set { schemaFile = value; }
		}

		public TargetCollection Targets {
			get { return targets; }
		}

		public DateTime TimeOfLastDirty {
			get { return timeOfLastDirty; }
		}
		
		public UsingTaskCollection UsingTasks {
			get { return usingTasks; }
		}

		[MonoTODO]
		public string Xml {
			get { return xmlDocument.InnerXml; }
		}
		
		internal static XmlNamespaceManager XmlNamespaceManager {
			get {
				if (manager == null) {
					manager = new XmlNamespaceManager (new NameTable ());
					manager.AddNamespace ("tns", ns);
				}
				
				return manager;
			}
		}
		
		internal TaskDatabase TaskDatabase {
			get { return taskDatabase; }
		}
		
		internal XmlDocument XmlDocument {
			get { return xmlDocument; }
		}
		
		internal static string XmlNamespace {
			get { return ns; }
		}
	}
}

#endif

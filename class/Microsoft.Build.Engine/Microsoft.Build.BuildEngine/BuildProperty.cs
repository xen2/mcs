//
// BuildProperty.cs: Represents a property
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
using System.Text;
using System.Xml;

using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	public class BuildProperty {
	
		XmlElement	propertyElement;
		string		finalValue;
		bool		isImported;
		string		value;
		string		name;
		Project		parentProject;
		PropertyType	propertyType;

		BuildProperty ()
		{
		}

		public BuildProperty (string propertyName, string propertyValue)
			: this (propertyName, propertyValue, PropertyType.Normal)
		{
			if (propertyName == null)
				throw new ArgumentNullException ("propertyName");
			if (propertyValue == null)
				throw new ArgumentNullException ("propertyValue");
		}

		internal BuildProperty (string propertyName,
				string propertyValue, PropertyType propertyType)
		{
			this.name = propertyName;
			this.value = propertyValue;
			this.finalValue = propertyValue;
			this.propertyType = propertyType;
			this.isImported = false;
		}

		internal BuildProperty (Project parentProject, XmlElement propertyElement)
		{
			if (propertyElement == null)
				throw new ArgumentNullException ("propertyElement");

			this.propertyElement = propertyElement;
			this.propertyType = PropertyType.Normal;
			this.parentProject = parentProject;
			this.name = propertyElement.Name;
			this.value = Utilities.UnescapeFromXml (propertyElement.InnerXml);
			this.isImported = false;
		}

		[MonoTODO]
		public BuildProperty Clone (bool deepClone)
		{
			if (deepClone) {
				if (FromXml) 
					throw new NotImplementedException ();
				else
					return (BuildProperty) this.MemberwiseClone ();
			} else {
				if (FromXml)
					throw new NotImplementedException ();
				else
					throw new InvalidOperationException ("A shallow clone of this object cannot be created.");
			}
		}

		public static explicit operator string (BuildProperty propertyToCast)
		{
			if (propertyToCast == null)
				return String.Empty;
			else
				return propertyToCast.ToString ();
		}

		public override string ToString ()
		{
			if (finalValue != null)
				return finalValue;
			else
				return Value;
		}

		internal void Evaluate ()
		{
			BuildProperty evaluated = new BuildProperty (Name, Value);

			Expression exp = new Expression ();
			exp.Parse (Value, false, false);
			evaluated.finalValue = (string) exp.ConvertTo (parentProject, typeof (string));

			parentProject.EvaluatedProperties.AddProperty (evaluated);
		}

		internal string ConvertToString (Project project)
		{
			Expression exp = new Expression ();
			exp.Parse (Value, true, false);

			return (string) exp.ConvertTo (project, typeof (string));
		}

		internal ITaskItem[] ConvertToITaskItemArray (Project project)
		{
			Expression exp = new Expression ();
			exp.Parse (Value, true, false);

			return (ITaskItem[]) exp.ConvertTo (project, typeof (ITaskItem[]));
		}

		internal bool FromXml {
			get {
				return propertyElement != null;
			}
		}
	
		public string Condition {
			get {
				if (FromXml)
					return propertyElement.GetAttribute ("Condition");
				else
					return String.Empty;
			}
			set {
				if (FromXml)
					propertyElement.SetAttribute ("Condition", value);
				else
					throw new InvalidOperationException ("Cannot set a condition on an object not represented by an XML element in the project file.");
			}
		}

		public string FinalValue {
			get {
				if (finalValue == null)
					return this.@value;
				else
					return finalValue;
			}
		}
		
		public bool IsImported {
			get { return isImported; }
		}

		public string Name {
			get { return name; }
		}

		public string Value {
			get {
				return value;
			}
			set {
				this.@value = value;
				if (FromXml) {
					propertyElement.InnerXml = value;
				} else {
					finalValue = value;
				}
			}
		}

		internal PropertyType PropertyType {
			get {
				return propertyType;
			}
		}

		internal XmlElement XmlElement {
			get { return propertyElement; }
		}
	}

	internal enum PropertyType {
		Reserved,
		Global,
		Normal,
		Environment
	}
}

#endif

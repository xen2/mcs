//
// System.Xml.XmlEntityReference.cs
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Xml
{
	public class XmlEntityReference : XmlLinkedNode
	{
		string entityName;
		
		// Constructor
		protected internal XmlEntityReference (string name, XmlDocument doc)
			: base (doc)
		{
			entityName = name;
		}

		// Properties
		[MonoTODO]
		public override string BaseURI {
			get { return null; }
		}

		public override bool IsReadOnly {
			get { return true; } 
		}

		public override string LocalName {
			get { return entityName; } // name of the entity referenced.
		}

		public override string Name {
			get { return entityName; } // name of the entity referenced.
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.EntityReference; }
		}

		public override string Value {
			get { return null; } // always return null here.
		}

		// Methods
		[MonoTODO]
		public override XmlNode CloneNode (bool deep)
		{
			return null;
		}

		[MonoTODO]
		public override void WriteContentTo (XmlWriter w)
		{
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter w)
		{
		}
	}
}

//
// System.Xml.XmlAttribute
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;

namespace System.Xml
{
	public class XmlElement : XmlLinkedNode
	{
		#region Fields

		private XmlAttributeCollection attributes;
		private XmlLinkedNode lastLinkedChild;
		private string localName;
		private string namespaceURI;
		private string prefix;

		#endregion

		#region Constructor

		protected internal XmlElement (
			string prefix, 
			string localName, 
			string namespaceURI, 
			XmlDocument doc) : base (doc)
		{
			this.prefix = prefix;
			this.localName = localName;
			this.namespaceURI = namespaceURI;

			attributes = new XmlAttributeCollection (this);
		}

		#endregion

		#region Properties

		public override XmlAttributeCollection Attributes {
			get { 
				return attributes; 
			}
		}

		public virtual bool HasAttributes {
			get { 
				return attributes.Count > 0; 
			}
		}

		[MonoTODO]
		public override string InnerText {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public override string InnerXml {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public bool IsEmpty	{
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		internal override XmlLinkedNode LastLinkedChild {
			get	{
				return lastLinkedChild;
			}

			set {
				lastLinkedChild = value;
			}
		}
		
		public override string LocalName 
		{
			get { 
				return localName; 
			}
		}

		public override string Name {
			get { 
				return prefix != String.Empty ? prefix + ":" + localName : localName; 
			}
		}

		public override string NamespaceURI {
			get { 
				return namespaceURI; 
			}
		}

		[MonoTODO]
		public override XmlNode NextSibling {
			get { 
				return base.NextSibling; 
			}
		}

		public override XmlNodeType NodeType {
			get { 
				return XmlNodeType.Element; 
			}
		}

		[MonoTODO]
		public override XmlDocument OwnerDocument {
			get { 
				return base.OwnerDocument; 
			}
		}

		public override string Prefix {
			get { 
				return prefix; 
			}
		}

		#endregion

		#region Methods
		
		[MonoTODO]
		public override XmlNode CloneNode (bool deep)
		{
			if (deep) {
				XmlNode node = ParentNode.FirstChild;

				while ((node != null) && (node.HasChildNodes)) {
					//
					// XmlNode.CloneNode says we should also clone the attributes,
					// does the NextSibling.CloneNode do the Right Thing?
					//
					AppendChild (node.NextSibling.CloneNode (false));
					node = node.NextSibling;
				}

				return node;
			} else {
				XmlNode node =  new XmlElement (prefix, localName, namespaceURI,
								OwnerDocument);

				//
				// XmlNode.CloneNode says 'Clones the element node, its attributes
				// including its default attributes.'
				//
				for (int i = 0; i < node.Attributes.Count; i++)
					node.AppendChild (node.Attributes [i].CloneNode (false));

				return node;
			}
		}

		[MonoTODO]
		public virtual string GetAttribute (string name)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (name);
			return attributeNode != null ? attributeNode.Value : String.Empty;
		}

		[MonoTODO]
		public virtual string GetAttribute (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute GetAttributeNode (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute GetAttributeNode (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNodeList GetElementsByTagName (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNodeList GetElementsByTagName (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool HasAttribute (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool HasAttribute (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Don't remove default attributes.")]
		public override void RemoveAll ()
		{
			// Remove the child nodes.
			base.RemoveAll ();

			// Remove all attributes.
			attributes.RemoveAll ();
		}

		[MonoTODO]
		public virtual void RemoveAllAttributes ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveAttribute (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveAttribute (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode RemoveAttributeAt (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute RemoveAttributeNode (XmlAttribute oldAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute RemoveAttributeNode (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetAttribute (string name, string value)
		{
			XmlAttribute attribute = OwnerDocument.CreateAttribute (name);
			attribute.SetOwnerElement (this);
			attribute.Value = value;
			Attributes.SetNamedItem (attribute);
		}

		[MonoTODO]
		public virtual void SetAttribute (string localName, string namespaceURI, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute SetAttributeNode (XmlAttribute newAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute SetAttributeNode (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		public override void WriteContentTo (XmlWriter w)
		{
			foreach(XmlNode childNode in ChildNodes)
				childNode.WriteTo(w);
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteStartElement(LocalName);

			foreach(XmlNode attributeNode in Attributes)
				attributeNode.WriteTo(w);

			WriteContentTo(w);

			w.WriteEndElement();
		}

		#endregion
	}
}

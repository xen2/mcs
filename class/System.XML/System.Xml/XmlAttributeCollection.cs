//
// System.Xml.XmlAttributeCollection
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Collections;

namespace System.Xml
{
	public class XmlAttributeCollection : XmlNamedNodeMap, ICollection
	{
		internal XmlAttributeCollection (XmlNode parent) : base (parent)
		{
		}

		bool ICollection.IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
		public virtual XmlAttribute this [string name] {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
		public virtual XmlAttribute this [int i] {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
		public virtual XmlAttribute this [string localName, string namespaceURI] {
			get {
				throw new NotImplementedException ();
			}
		}

		object ICollection.SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual XmlAttribute Append (XmlAttribute node) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (XmlAttribute [] array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute InsertAfter (XmlAttribute newNode, XmlAttribute refNode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute InsertBefore (XmlAttribute newNode, XmlAttribute refNode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute Prepend (XmlAttribute node) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute Remove (XmlAttribute node) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveAll () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlAttribute RemoveAt (int i) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override XmlNode SetNamedItem (XmlNode node)
		{
			return base.SetNamedItem (node);
		}
	}
}

//
// EncryptionMethod.cs - EncryptionMethod implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptionMethod
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public class EncryptionMethod {

		#region Fields

		string algorithm;
		int keySize;

		#endregion // Fields
	
		#region Constructors

		public EncryptionMethod ()
		{
			KeyAlgorithm = null;
		}

		public EncryptionMethod (string strAlgorithm)
		{
			KeyAlgorithm = strAlgorithm;
		}

		#endregion // Constructors

		#region Properties

		public string KeyAlgorithm {
			get { return algorithm; }
			set { algorithm = value; }
		}

		public int KeySize {
			get { return keySize; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("The key size should be a non negative integer.");
				keySize = value; 
			}
		}

		#endregion // Properties

		#region Methods

		public XmlElement GetXml ()
		{
			return GetXml (new XmlDocument ());
		}

		internal XmlElement GetXml (XmlDocument document)
		{
			XmlElement xel = document.CreateElement (XmlEncryption.ElementNames.EncryptionMethod, XmlEncryption.NamespaceURI);

			if (KeySize != 0) {
				XmlElement xks = document.CreateElement (XmlEncryption.ElementNames.KeySize, XmlEncryption.NamespaceURI);
				xks.InnerText = String.Format ("{0}", keySize);
				xel.AppendChild (xks);
			}

			if (KeyAlgorithm != null)
				xel.SetAttribute (XmlEncryption.AttributeNames.Algorithm, KeyAlgorithm);
			return xel;
		}

		public void LoadXml (XmlElement value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if ((value.LocalName != XmlEncryption.ElementNames.EncryptionMethod) || (value.NamespaceURI != XmlEncryption.NamespaceURI))
				throw new CryptographicException ("Malformed EncryptionMethod element.");
			else {
				KeyAlgorithm = null;
				foreach (XmlNode n in value.ChildNodes) {
					if (n is XmlWhitespace)
						continue;
					switch (n.LocalName) {
					case XmlEncryption.ElementNames.KeySize:
						KeySize = Int32.Parse (n.InnerText);
						break;
					}
				}
				if (value.HasAttribute (XmlEncryption.AttributeNames.Algorithm))
					KeyAlgorithm = value.Attributes [XmlEncryption.AttributeNames.Algorithm].Value;
			}
		}

		#endregion // Methods
	}
}

#endif

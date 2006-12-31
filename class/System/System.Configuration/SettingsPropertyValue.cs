//
// System.Web.UI.WebControls.SettingsPropertyValue.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
#if (XML_DEP)
using System.Xml;
using System.Xml.Serialization;
#endif

namespace System.Configuration
{

	public class SettingsPropertyValue
	{
		public SettingsPropertyValue (SettingsProperty property)
		{
			this.property = property;
			needPropertyValue = true;
		}

		public bool Deserialized {
			get {
				return deserialized;
			}
			set {
				deserialized = true;
			}
		}

		public bool IsDirty {
			get {
				return dirty;
			}
			set {
				dirty = value;
			}
		}

		public string Name {
			get {
				return property.Name;
			}
		}

		public SettingsProperty Property {
			get {
				return property;
			}
		}

		public object PropertyValue {
			get {
				if (needPropertyValue) {
					propertyValue = GetDeserializedValue ();
					if (propertyValue == null) {
						if (!property.PropertyType.IsAssignableFrom (property.DefaultValue.GetType ())) {
							TypeConverter converter = TypeDescriptor.GetConverter (property.PropertyType);
							propertyValue = converter.ConvertFrom (property.DefaultValue);
						}
						else {
							propertyValue = property.DefaultValue;
						}
						defaulted = true;
					}
					needPropertyValue = false;
				}

#if notyet
				/* LAMESPEC: the msdn2 docs say that
				 * for object types this
				 * pessimistically sets Dirty == true.
				 * tests, however, point out that that
				 * is not the case. */
				if (!property.PropertyType.IsValueType)
					dirty = true;
#endif

				return propertyValue;
			}
			set {
				propertyValue = value;
				dirty = true;
				needPropertyValue = false;
				needSerializedValue = true;
				defaulted = false;
			}
		}

		[MonoTODO ("string type converter?")]
		public object SerializedValue {
			get {
				if (needSerializedValue) {
					needSerializedValue = false;

					switch (property.SerializeAs)
					{
					case SettingsSerializeAs.String:
						/* the docs say use a string type converter.. this means what? */
						serializedValue = propertyValue.ToString();
						break;
#if (XML_DEP)
					case SettingsSerializeAs.Xml:
						XmlSerializer serializer = new XmlSerializer (propertyValue.GetType());
						StringWriter w = new StringWriter();

						serializer.Serialize (w, propertyValue);
						serializedValue = w.ToString();
						break;
#endif
					case SettingsSerializeAs.Binary:
						BinaryFormatter bf = new BinaryFormatter ();
						MemoryStream ms = new MemoryStream ();
						bf.Serialize (ms, propertyValue);
						serializedValue = ms.ToArray();
						break;
					default:
						serializedValue = null;
						break;
					}

				}

				return serializedValue;
			}
			set {
				serializedValue = value;
				needPropertyValue = true;
			}
		}

		public bool UsingDefaultValue {
			get {
				return defaulted;
			}
		}

		private object GetDeserializedValue ()
		{
			if (serializedValue == null)
				return null;

			object deserializedObject = null;

			try {
				switch (property.SerializeAs) {
					case SettingsSerializeAs.String:
						if (((string) serializedValue).Length > 0)
							deserializedObject = TypeDescriptor.GetConverter (property.PropertyType).ConvertFromString ((string) serializedValue);
						break;
#if (XML_DEP)
					case SettingsSerializeAs.Xml:
						XmlSerializer serializer = new XmlSerializer (property.PropertyType);
						StringReader str = new StringReader ((string) serializedValue);
						deserializedObject = serializer.Deserialize (XmlReader.Create (str));
						break;
#endif
					case SettingsSerializeAs.Binary:
						BinaryFormatter bf = new BinaryFormatter ();
						MemoryStream ms = new MemoryStream ((byte []) serializedValue);
						deserializedObject = bf.Deserialize (ms);
						break;
				}
			}
			catch (Exception e) {
				if (property.ThrowOnErrorDeserializing)
					throw e;
			}

			return deserializedObject;
		}

		SettingsProperty property;
		object propertyValue;
		object serializedValue;
		bool needSerializedValue;
		bool needPropertyValue;
		bool dirty;
		bool defaulted = false;
		bool deserialized;
	}

}

#endif

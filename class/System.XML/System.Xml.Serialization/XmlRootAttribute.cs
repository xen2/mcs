//
// XmlRootAttribute.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlRootAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Enum | AttributeTargets.Interface |
		AttributeTargets.ReturnValue)]
	public class XmlRootAttribute : Attribute
	{
		private string dataType;
		private string elementName;
		private bool isNullable;
		private string ns;

		public XmlRootAttribute ()
		{
		
		}
		public XmlRootAttribute (string elementName)
		{
			ElementName = elementName;
		}

		public string DataType 
		{
			get { 
				return dataType; 
			} 
			set { 
				dataType = value; 
			}
		}
		public string ElementName 
		{
			get { 
				return elementName; 
			}
			set { 
				elementName = value; 
			}
		}
		public bool IsNullable 
		{
			get { 
				return isNullable; 
			}
			set { 
				isNullable = value; 
			}
		}
		public string Namespace 
		{
			get { 
				return ns; 
			} 
			set { 
				ns = value; 
			}
		}
		
		internal bool InternalEquals (XmlRootAttribute other)
		{
			if (other == null) return false;
			return (dataType == other.dataType && elementName == other.elementName &&
				    isNullable == other.isNullable && ns == other.ns);
		}
	}
}

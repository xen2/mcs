//
// System.Web.Configuration.CustomErrorsConfigHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Xml;

namespace System.Web.Configuration
{
	enum CustomErrorMode
	{
		RemoteOnly,
		On,
		Off
	}
	
	class CustomErrorsConfig
	{
		string defaultRedirect;
		CustomErrorMode mode;
		Hashtable redirects;
		string configFilePath;

		public CustomErrorsConfig (object parent, object context)
		{
			if (parent != null) {
				CustomErrorsConfig p = (CustomErrorsConfig) parent;
				mode = p.mode;
				defaultRedirect = p.defaultRedirect;
				if (p.redirects != null && p.redirects.Count > 0) {
					redirects = new Hashtable ();
					foreach (DictionaryEntry entry in p.redirects)
						redirects [entry.Key] = entry.Value;
				}
			}

			configFilePath = Path.GetDirectoryName ((string) context);
		}

		public string DefaultRedirect {
			get { return defaultRedirect; }
			set { defaultRedirect = value; }
		}

		public CustomErrorMode Mode {
			get { return mode; }
			set { mode = value; }
		}

		public string ConfigFilePath {
			get { return configFilePath; }
		}
		
		public string this [int statusCode] {
			get {
				if (redirects == null)
					return null;
					
				return (string) redirects [statusCode];
			}

			set {
				if (redirects == null)
					redirects = new Hashtable ();

				// Overrides any previous setting for statusCode even in the same file
				redirects [statusCode] = value;
			}
		}
	}
	
	class CustomErrorsConfigHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object context, XmlNode section)
		{
			CustomErrorsConfig config = new CustomErrorsConfig (parent, context);
			
			string defaultRedirect = AttValue ("defaultRedirect", section);
			if (defaultRedirect != null)
				config.DefaultRedirect = defaultRedirect;

			string mode = AttValue ("mode", section);
			if (mode != null) {
				switch (mode) {
				case "On":
					config.Mode = CustomErrorMode.On;
					break;
				case "Off":
					config.Mode = CustomErrorMode.Off;
					break;
				case "RemoteOnly":
					config.Mode = CustomErrorMode.RemoteOnly;
					break;
				default:
					ThrowException ("Invalid value for 'mode': " + mode, section);
					break;
				}
			}
			
			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			if (!section.HasChildNodes)
				return config;

			XmlNodeList children = section.ChildNodes;
			foreach (XmlNode child in children) {
				XmlNodeType ntype = child.NodeType;
				if (ntype == XmlNodeType.Whitespace || ntype == XmlNodeType.Comment)
					continue;

				if (ntype != XmlNodeType.Element)
					ThrowException ("Only elements allowed", child);

				if (child.Name != "error")
					ThrowException ("Unrecognized node: " + child.Name, child);

				string statusCode = AttValue ("statusCode", child, false, false);
				string redirect = AttValue ("redirect", child, false, false);
				int code = 0;
				try {
					code = Int32.Parse (statusCode);
				} catch {
					ThrowException ("Unable to parse 'statusCode': " + statusCode, child);
				}

				if (code < 100 || code >= 1000)
					ThrowException ("Invalid value for 'statusCode': " + code, child);

				config [code] = redirect;
			}

			return config;
		}

		// To save some typing...
		static string AttValue (string name, XmlNode node, bool optional, bool allowEmpty)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, optional, allowEmpty);
		}

		static string AttValue (string name, XmlNode node, bool optional)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, optional);
		}

		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
		//
	}
}


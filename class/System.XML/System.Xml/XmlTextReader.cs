//
// System.Xml.XmlTextReader
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Adam Treat (manyoso@yahoo.com)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
//

// FIXME:
//   This can only parse basic XML: elements, attributes, processing
//   instructions, and comments are OK.
//
//   It barfs on DOCTYPE declarations.
//     => No barfing, but parsing is incomplete.
//        DTD nodes are not still created.
//
//   There's also no checking being done for validity.
//
//   More checking should be done for well-formedness.
//
//   NameTables aren't being used everywhere yet.
//
//   Some thought needs to be given to performance. There's too many
//   strings being allocated.
//
//   Some of the MoveTo methods haven't been implemented yet.
//
//   xml:space, xml:lang, and xml:base aren't being tracked.
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Xml
{
	public class XmlTextReader : XmlReader, IXmlLineInfo
	{
		WhitespaceHandling whitespaceHandling = WhitespaceHandling.All;
		#region Constructors

		protected XmlTextReader ()
		{
		}

		public XmlTextReader (Stream input)
			: this (new StreamReader (input))
		{
		}

		public XmlTextReader (string url)
			: this(url, new NameTable ())
		{
		}

		public XmlTextReader (TextReader input)
			: this (input, new NameTable ())
		{
		}

		protected XmlTextReader (XmlNameTable nt)
			: this (String.Empty, null, XmlNodeType.None, null)
		{
		}

		public XmlTextReader (Stream input, XmlNameTable nt)
			: this(new StreamReader (input), nt)
 		{
		}

		public XmlTextReader (string url, Stream input)
			: this (url, new StreamReader (input))
		{
		}

		public XmlTextReader (string url, TextReader input)
			: this (url, input, new NameTable ())
		{
		}

		[MonoTODO("Non-filename-url must be supported. Waiting for WebClient")]
		public XmlTextReader (string url, XmlNameTable nt)
			// : this(url, new StreamReader ((Stream)new XmlUrlResolver ().GetEntity (new Uri (url), null, typeof(Stream))), nt)
			: this (url, new StreamReader (url), nt)
		{
		}

		public XmlTextReader (TextReader input, XmlNameTable nt)
			: this(String.Empty, input, nt)
		{
		}

		public XmlTextReader (Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (String.Empty, new StreamReader (xmlFragment), fragType, context)
		{
		}

		public XmlTextReader (string url, Stream input, XmlNameTable nt)
			: this (url, new StreamReader (input), nt)
		{
		}

		public XmlTextReader (string url, TextReader input, XmlNameTable nt)
			: this (url, input, XmlNodeType.Document, new XmlParserContext (nt, new XmlNamespaceManager (nt), null, XmlSpace.None))
		{
		}

		[MonoTODO("TODO as same as private XmlTextReader(TextReader, XmlNodeType, XmlParserContext)")]
		public XmlTextReader (string xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (String.Empty, new StringReader (xmlFragment), fragType, context)
		{
		}

		// TODO still remains as described at head of this file,
		// but it might not be TODO of the constructors...
		XmlTextReader (string url, TextReader fragment, XmlNodeType fragType, XmlParserContext context)
		{
			this.SetReaderContext(url, context);
			this.SetReaderFragment(fragment, fragType);
		}

		#endregion

		#region Properties

		public override int AttributeCount
		{
			get { return attributes.Count; }
		}

		public override string BaseURI
		{
			get { return parserContext.BaseURI; }
		}

		public override int Depth
		{
			get {
				return elementDepth;
			}
		}

		public Encoding Encoding
		{
			get { return parserContext.Encoding; }
		}

		public override bool EOF
		{
			get
			{
				return
					readState == ReadState.EndOfFile ||
					readState == ReadState.Closed;
			}
		}

		public override bool HasValue
		{
			get { return value != String.Empty;	}
		}

		public override bool IsDefault
		{
			get
			{
				// XmlTextReader does not expand default attributes.
				return false;
			}
		}

		public override bool IsEmptyElement
		{
			get { return isEmptyElement; }
		}

		public override string this [int i]
		{
			get { return GetAttribute (i); }
		}

		public override string this [string name]
		{
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string namespaceName]
		{
			get { return GetAttribute (localName, namespaceName); }
		}

		public int LineNumber
		{
			get { return line; }
		}

		public int LinePosition
		{
			get { return column; }
		}

		public override string LocalName
		{
			get { return localName; }
		}

		public override string Name
		{
			get { return name; }
		}

		[MonoTODO]
		public bool Namespaces
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public override string NamespaceURI
		{
			get { return namespaceURI; }
		}

		public override XmlNameTable NameTable
		{
			get { return parserContext.NameTable; }
		}

		public override XmlNodeType NodeType
		{
			get { return nodeType; }
		}

		[MonoTODO]
		public bool Normalization
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public override string Prefix
		{
			get { return prefix; }
		}

		public override char QuoteChar
		{
			get {
				// value string holds attribute quotation char.
				if (NodeType == XmlNodeType.Attribute)
					return value [0];
				else
					return '"';
			}
		}

		public override ReadState ReadState
		{
			get { return readState; }
		}

		public override string Value
		{
			get {
				if(NodeType == XmlNodeType.Attribute)
					return UnescapeAttributeValue(value);
				else
					return value;
			}
		}

		public WhitespaceHandling WhitespaceHandling
		{
			get { return whitespaceHandling; }
			set { whitespaceHandling = value; }
		}

		[MonoTODO]
		public override string XmlLang
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlResolver XmlResolver
		{
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override XmlSpace XmlSpace
		{
			get { throw new NotImplementedException (); }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override void Close ()
		{
			readState = ReadState.Closed;
		}

		public override string GetAttribute (int i)
		{
			if (i > attributes.Count)
				throw new ArgumentOutOfRangeException ("i is smaller than AttributeCount");
			else
				return UnescapeAttributeValue (attributes [orderedAttributes [i]] as string);
		}

		public override string GetAttribute (string name)
		{
			return attributes.ContainsKey (name) ?
				UnescapeAttributeValue (attributes [name] as string) : String.Empty;
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			foreach (DictionaryEntry entry in attributes)
			{
				string thisName = entry.Key as string;

				int indexOfColon = thisName.IndexOf (':');

				if (indexOfColon != -1) {
					string thisLocalName = thisName.Substring (indexOfColon + 1);

					if (localName == thisLocalName) {
						string thisPrefix = thisName.Substring (0, indexOfColon);
						string thisNamespaceURI = LookupNamespace (thisPrefix);

						if (namespaceURI == thisNamespaceURI)
							return attributes.ContainsKey (thisName) ?
								UnescapeAttributeValue (attributes [thisName] as string) : String.Empty;
					}
				} else if (localName == "xmlns" && namespaceURI == "http://www.w3.org/2000/xmlns/" && thisName == "xmlns")
					return attributes.ContainsKey (thisName) ? 
						UnescapeAttributeValue (attributes [thisName] as string) : String.Empty;
			}

			return String.Empty;
		}

		[MonoTODO]
		public TextReader GetRemainder ()
		{
			throw new NotImplementedException ();
		}

		bool IXmlLineInfo.HasLineInfo ()
		{
			return true;
		}

		public override string LookupNamespace (string prefix)
		{
			return parserContext.NamespaceManager.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int i)
		{
			MoveToElement ();

			if (attributes == null || orderedAttributes.Count < i || i < 0)
				throw new ArgumentOutOfRangeException ("attribute index out of range.");

			string name = orderedAttributes [i] as string;
			string value = attributes [name] as string;
			SetProperties (
				XmlNodeType.Attribute, // nodeType
				name, // name
				false, // isEmptyElement
				value, // value
				false // clearAttributes
				);
			attributeValuePos = 0;
		}

		public override bool MoveToAttribute (string name)
		{
			MoveToElement ();
			bool match = false;

			if (attributes == null)
				return false;

			if (orderedAttributesEnumerator == null) {
				SaveProperties ();
				orderedAttributesEnumerator = orderedAttributes.GetEnumerator ();
			}

			while (orderedAttributesEnumerator.MoveNext ()) {
				if(name == orderedAttributesEnumerator.Current as string) {
					match = true;
					break;
				}
			}

			if (match) {
				string value = attributes [name] as string;
				SetProperties (
					XmlNodeType.Attribute, // nodeType
					name, // name
					false, // isEmptyElement
					value, // value
					false // clearAttributes
				);
				attributeValuePos = 0;
			}

			return match;
		}

		[MonoTODO]
		public override bool MoveToAttribute (string localName, string namespaceName)
		{
			throw new NotImplementedException ();
		}

		public override bool MoveToElement ()
		{
			if (orderedAttributesEnumerator != null) {
				orderedAttributesEnumerator = null;
				RestoreProperties ();
				return true;
			}

			return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			MoveToElement ();
			return MoveToNextAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			if (attributes == null)
				return false;

			if (orderedAttributesEnumerator == null) {
				SaveProperties ();
				orderedAttributesEnumerator = orderedAttributes.GetEnumerator ();
			}

			if (orderedAttributesEnumerator.MoveNext ()) {
				string name = orderedAttributesEnumerator.Current as string;
				string value = attributes [name] as string;
				SetProperties (
					XmlNodeType.Attribute, // nodeType
					name, // name
					false, // isEmptyElement
					value, // value
					false // clearAttributes
				);
				attributeValuePos = 0;
				return true;
			}

			return false;
		}

		public override bool Read ()
		{
			bool more = false;

			readState = ReadState.Interactive;

			more = ReadContent ();

			return more;
		}

		public override bool ReadAttributeValue ()
		{
			// 'attributeString' holds real string value (without their
			// quotation characters).
			//
			// 'attributeValuePos' holds current position
			// of 'attributeString' while iterating ReadAttribute().
			// It may be:
			//   -1 if ReadAttributeValue() has already finished.
			//    0 if ReadAttributeValue() ready to start reading.
			//   >0 if ReadAttributeValue() already got 1 or more values
			//
			// local 'refPosition' holds the position on the 
			// attributeString which may be used next time.

			if (attributeValuePos < 0) {
				SetProperties (XmlNodeType.None,
					String.Empty,
					false,
					String.Empty,
					false);
				return false;
			}

			// If not started, then initialize attributeString when parsing is at start.
			if (attributeValuePos == 0)
				attributeString =
					value.Substring (1, value.Length - 2);

			returnEntityReference = false;
			value = String.Empty;
			int refPosition;
			int loop = 0;

			do {
				refPosition = attributeString.IndexOf ('&', attributeValuePos);
				if (refPosition < 0) {
					// Reached to the end of value string.
					value += attributeString.Substring (attributeValuePos);
					attributeValuePos = -1;
					break;
				} else if (refPosition == attributeValuePos) {
					string parsed = ReadAttributeValueReference ();
					if (parsed != null)
						value += parsed;
					else {
						// Found that an entity reference starts from this point.
						// reset position to after '&'.
						attributeValuePos = refPosition;
						if (value.Length <= 0) {
							int endNamePos = attributeString.IndexOf (";", attributeValuePos);
							value = attributeString.Substring (attributeValuePos+1, endNamePos - attributeValuePos - 1);
							attributeValuePos += value.Length + 2;
							returnEntityReference = true;
						}
						break;
					}
				} else {
					value += attributeString.Substring (attributeValuePos,
						refPosition - attributeValuePos);
					attributeValuePos = refPosition;
					continue;
				}
			} while (++loop > 0);

			if (returnEntityReference)
				SetProperties (XmlNodeType.EntityReference,
					value,
					false,
					String.Empty,
					false);
			else
				SetProperties (XmlNodeType.Text,
					"#text",
					false,
					value,
					false);

			return true;
		}

		[MonoTODO]
		public int ReadBase64 (byte [] buffer, int offset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ReadBinHex (byte [] buffer, int offset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ReadChars (char [] buffer, int offset, int length)
		{
			throw new NotImplementedException ();
		}

		public override string ReadInnerXml ()
		{
			if (readState != ReadState.Interactive)
				return String.Empty;

			switch (NodeType) {
			case XmlNodeType.Attribute:
				return value.Substring (1, value.Length - 2);
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return String.Empty;

				int startDepth = depth;

				innerXmlBuilder.Length = 0;
				do {
					ReadContent ();
					if (NodeType != XmlNodeType.EndElement || depth + 1 > startDepth)
						innerXmlBuilder.Append (currentTag);
				} while (depth >= startDepth);

				string xml = innerXmlBuilder.ToString ();
				innerXmlBuilder.Length = 0;
				return xml;
			case XmlNodeType.None:
				// MS document is incorrect. Seems not to progress.
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}

		public override string ReadOuterXml ()
		{
			if (readState != ReadState.Interactive)
				return String.Empty;

			switch (NodeType) {
			case XmlNodeType.Attribute:
				// strictly incompatible with MS... (it holds spaces attribute between name, value and "=" char (very trivial).
				return String.Format ("{0}={1}{2}{1}", Name, QuoteChar, ReadInnerXml ());
			case XmlNodeType.Element:
				bool isEmpty = IsEmptyElement;
				string startTag = currentTag.ToString ();
				string name = Name;

				if (NodeType == XmlNodeType.Element && !isEmpty)
					return String.Format ("{0}{1}</{2}>", startTag, ReadInnerXml (), name);
				else
					return currentTag.ToString ();
			case XmlNodeType.None:
				// MS document is incorrect. Seems not to progress.
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}

		[MonoTODO]
		public override string ReadString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetState ()
		{
			throw new NotImplementedException ();
		}

		public override void ResolveEntity ()
		{
			// XmlTextReaders don't resolve entities.
			throw new InvalidOperationException ("XmlTextReader cannot resolve external entities.");
		}

		#endregion

		#region Internals
		internal string publicId;
		internal string systemId;

		internal void SetReaderContext (string url, XmlParserContext context)
		{
			parserContext = context;
			parserContext.BaseURI = url;
			Init ();
		}

		internal void SetReaderFragment(TextReader fragment, XmlNodeType fragType)
		{
			this.reader = fragment;
			StreamReader sr = fragment as StreamReader;
			if (sr != null)
				can_seek = sr.BaseStream.CanSeek;
			else
				can_seek = fragment != null && fragment.Peek () != -1;

			if (fragType == XmlNodeType.Attribute)
				value = "''";
			else if (fragType == XmlNodeType.DocumentFragment)
				allowMultipleRoot = true;
/*	for future use
			switch(fragType)
			{
			case XmlNodeType.Attribute:	// attribute content
				parserContext.InputState = XmlParserInputState.AttributeValue;
				break;
			case XmlNodeType.DocumentFragment:	// element content
				parserContext.InputState = XmlParserInputState.Content;
				break;
			case XmlNodeType.Element:	// one element
				parserContext.InputState = XmlParserInputState.StartTag;
				break;
			case XmlNodeType.Document:	// document content
				parserContext.InputState = XmlParserInputState.Start;
				break;
			default:
				throw new InvalidOperationException("setting this xml node type not allowed.");
			}
*/
		}
		#endregion

		#region Privates

		private XmlParserContext parserContext;

		private TextReader reader;
		private ReadState readState;

		private int depth;
		private int elementDepth;
		private bool depthDown;

		private bool popScope;
		private Stack elementStack;
		private bool haveEnteredDocument;
		private bool allowMultipleRoot = false;

		private XmlNodeType nodeType;
		private string name;
		private string prefix;
		private string localName;
		private string namespaceURI;
		private bool isEmptyElement;
		private string value;

		private XmlNodeType saveNodeType;
		private string saveName;
		private string savePrefix;
		private string saveLocalName;
		private string saveNamespaceURI;
		private bool saveIsEmptyElement;

		private Hashtable attributes;
		private ArrayList orderedAttributes;
		private IEnumerator orderedAttributesEnumerator;

		private bool returnEntityReference;
		private string entityReferenceName;

		private char [] nameBuffer;
		private int nameLength;
		private int nameCapacity;
		private const int initialNameCapacity = 256;

		private char [] valueBuffer;
		private int valueLength;
		private int valueCapacity;
		private const int initialValueCapacity = 8192;

		private StringBuilder currentTag; // A buffer for ReadContent for ReadOuterXml
		private int line = 1;
		private int column = 1;
		private bool has_peek;
		private bool can_seek;
		private int peek_char;

		private string attributeString = String.Empty;
		private int attributeValuePos;
		// This should be only referenced(used) by ReadInnerXml(). Kind of flyweight pattern.
		private StringBuilder innerXmlBuilder;

		private XmlException ReaderError(string message)
		{
			return new XmlException(message, LineNumber, LinePosition);
		}
		private void Init ()
		{
			readState = ReadState.Initial;

			depth = 0;
			depthDown = false;

			popScope = false;
			elementStack = new Stack();
			haveEnteredDocument = false;

			nodeType = XmlNodeType.None;
			name = String.Empty;
			prefix = String.Empty;
			localName = string.Empty;
			isEmptyElement = false;
			value = String.Empty;

			attributes = new Hashtable ();
			orderedAttributes = new ArrayList ();
			orderedAttributesEnumerator = null;

			returnEntityReference = false;
			entityReferenceName = String.Empty;

			nameBuffer = new char [initialNameCapacity];
			nameLength = 0;
			nameCapacity = initialNameCapacity;

			valueBuffer = new char [initialValueCapacity];
			valueLength = 0;
			valueCapacity = initialValueCapacity;

			currentTag = new StringBuilder ();
			innerXmlBuilder = new StringBuilder ();
		}

		// Use this method rather than setting the properties
		// directly so that all the necessary properties can
		// be changed in harmony with each other. Maybe the
		// fields should be in a seperate class to help enforce
		// this.
		private void SetProperties (
			XmlNodeType nodeType,
			string name,
			bool isEmptyElement,
			string value,
			bool clearAttributes)
		{
			this.nodeType = nodeType;
			this.name = name;
			this.isEmptyElement = isEmptyElement;
			this.value = value;
			this.elementDepth = depth;

			if (clearAttributes)
				ClearAttributes ();

			int indexOfColon = name.IndexOf (':');

			if (indexOfColon == -1) {
				prefix = String.Empty;
				localName = name;
			} else {
				prefix = name.Substring (0, indexOfColon);
				localName = name.Substring (indexOfColon + 1);
			}

			namespaceURI = LookupNamespace (prefix);
		}

		private void SaveProperties ()
		{
			saveNodeType = nodeType;
			saveName = name;
			savePrefix = prefix;
			saveLocalName = localName;
			saveNamespaceURI = namespaceURI;
			saveIsEmptyElement = isEmptyElement;
			// An element's value is always String.Empty.
		}

		private void RestoreProperties ()
		{
			nodeType = saveNodeType;
			name = saveName;
			prefix = savePrefix;
			localName = saveLocalName;
			namespaceURI = saveNamespaceURI;
			isEmptyElement = saveIsEmptyElement;
			value = String.Empty;
		}

		private void AddAttribute (string name, string value)
		{
			attributes.Add (name, value);
			orderedAttributes.Add (name);
		}

		private void ClearAttributes ()
		{
			if (attributes.Count > 0) {
				attributes.Clear ();
				orderedAttributes.Clear ();
			}

			orderedAttributesEnumerator = null;
		}

		private int PeekChar ()
		{
			if (can_seek)
				return reader.Peek ();

			if (has_peek)
				return peek_char;

			peek_char = reader.Read ();
			has_peek = true;
			return peek_char;
		}

		private int ReadChar ()
		{
			int ch;
			if (has_peek) {
				ch = peek_char;
				has_peek = false;
			} else {
				ch = reader.Read ();
			}

			if (ch == '\n') {
				line++;
				column = 1;
			} else {
				column++;
			}
			currentTag.Append ((char) ch);
			return ch;
		}

		// This should really keep track of some state so
		// that it's not possible to have more than one document
		// element or text outside of the document element.
		private bool ReadContent ()
		{
			currentTag.Length = 0;
			if (popScope) {
				parserContext.NamespaceManager.PopScope ();
				popScope = false;
			}

			if (returnEntityReference) {
				SetEntityReferenceProperties ();
			} else {
    			switch (PeekChar ())
				{
				case '<':
					ReadChar ();
					ReadTag ();
					break;
				case '\r':
					if (whitespaceHandling == WhitespaceHandling.All ||
					    whitespaceHandling == WhitespaceHandling.Significant)
						return ReadWhitespace ();

					ReadChar ();
					return ReadContent ();
				case '\n':
					if (whitespaceHandling == WhitespaceHandling.All ||
					    whitespaceHandling == WhitespaceHandling.Significant)
						return ReadWhitespace ();

					ReadChar ();
					return ReadContent ();
				case ' ':
					if (whitespaceHandling == WhitespaceHandling.All ||
					    whitespaceHandling == WhitespaceHandling.Significant)
						return ReadWhitespace ();

					SkipWhitespace ();
					return ReadContent ();
				case -1:
					readState = ReadState.EndOfFile;
					SetProperties (
						XmlNodeType.None, // nodeType
						String.Empty, // name
						false, // isEmptyElement
						String.Empty, // value
						true // clearAttributes
					);
					break;
				default:
					ReadText (true);
					break;
				}
			}
			return this.ReadState != ReadState.EndOfFile;
		}

		private void SetEntityReferenceProperties ()
		{
			SetProperties (
				XmlNodeType.EntityReference, // nodeType
				entityReferenceName, // name
				false, // isEmptyElement
				String.Empty, // value
				true // clearAttributes
			);

			returnEntityReference = false;
			entityReferenceName = String.Empty;
		}

		// The leading '<' has already been consumed.
		private void ReadTag ()
		{
			switch (PeekChar ())
			{
			case '/':
				ReadChar ();
				ReadEndTag ();
				break;
			case '?':
				ReadChar ();
				ReadProcessingInstruction ();
				break;
			case '!':
				ReadChar ();
				ReadDeclaration ();
				break;
			default:
				ReadStartTag ();
				break;
			}
		}

		// The leading '<' has already been consumed.
		private void ReadStartTag ()
		{
			parserContext.NamespaceManager.PushScope ();

			string name = ReadName ();
			if (haveEnteredDocument && elementStack.Count == 0 && !allowMultipleRoot)
				throw ReaderError("document has terminated, cannot open new element");

			haveEnteredDocument = true;
			SkipWhitespace ();

			bool isEmptyElement = false;

			ClearAttributes ();

			if (XmlConstructs.IsNameStart (PeekChar ()))
				ReadAttributes ();

			if (PeekChar () == '/') {
				ReadChar ();
				isEmptyElement = true;
				depthDown = true;
				popScope = true;
			}
			else
				elementStack.Push(name);

			Expect ('>');

			SetProperties (
				XmlNodeType.Element, // nodeType
				name, // name
				isEmptyElement, // isEmptyElement
				String.Empty, // value
				false // clearAttributes
			);

			if (!depthDown)
				++depth;
			else
				depthDown = false;

		}

		// The reader is positioned on the first character
		// of the element's name.
		private void ReadEndTag ()
		{
			string name = ReadName ();
			if (elementStack.Count == 0)
				throw ReaderError("closing element without matching opening element");
			if ((string)elementStack.Pop() != name)
				throw ReaderError("unmatched closing element");

			SkipWhitespace ();
			Expect ('>');

			--depth;

			SetProperties (
				XmlNodeType.EndElement, // nodeType
				name, // name
				false, // isEmptyElement
				String.Empty, // value
				true // clearAttributes
			);

			popScope = true;
		}

		private void AppendNameChar (int ch)
		{
			CheckNameCapacity ();
			nameBuffer [nameLength++] = (char)ch;
		}

		private void CheckNameCapacity ()
		{
			if (nameLength == nameCapacity) {
				nameCapacity = nameCapacity * 2;
				char [] oldNameBuffer = nameBuffer;
				nameBuffer = new char [nameCapacity];
				Array.Copy (oldNameBuffer, nameBuffer, nameLength);
			}
		}

		private string CreateNameString ()
		{
			return new String (nameBuffer, 0, nameLength);
		}

		private void AppendValueChar (int ch)
		{
			CheckValueCapacity ();
			valueBuffer [valueLength++] = (char)ch;
		}

		private void CheckValueCapacity ()
		{
			if (valueLength == valueCapacity) {
				valueCapacity = valueCapacity * 2;
				char [] oldValueBuffer = valueBuffer;
				valueBuffer = new char [valueCapacity];
				Array.Copy (oldValueBuffer, valueBuffer, valueLength);
			}
		}

		private string CreateValueString ()
		{
			return new String (valueBuffer, 0, valueLength);
		}

		// The reader is positioned on the first character
		// of the text.
		private void ReadText (bool cleanValue)
		{
			if (cleanValue)
				valueLength = 0;

			int ch = PeekChar ();

			while (ch != '<' && ch != -1) {
				if (ch == '&') {
					ReadChar ();
					if (ReadReference (false))
						break;
				} else
					AppendValueChar (ReadChar ());

				ch = PeekChar ();
			}

			if (returnEntityReference && valueLength == 0) {
				SetEntityReferenceProperties ();
			} else {
				SetProperties (
					XmlNodeType.Text, // nodeType
					String.Empty, // name
					false, // isEmptyElement
					CreateValueString (), // value
					true // clearAttributes
				);
			}
		}

		// The leading '&' has already been consumed.
		// Returns true if the entity reference isn't a simple
		// character reference or one of the predefined entities.
		// This allows the ReadText method to break so that the
		// next call to Read will return the EntityReference node.
		private bool ReadReference (bool ignoreEntityReferences)
		{
			if (PeekChar () == '#') {
				ReadChar ();
				ReadCharacterReference ();
			} else
				ReadEntityReference (ignoreEntityReferences);

			return returnEntityReference;
		}

		private void ReadCharacterReference ()
		{
			int value = 0;

			if (PeekChar () == 'x') {
				ReadChar ();

				while (PeekChar () != ';' && PeekChar () != -1) {
					int ch = ReadChar ();

					if (ch >= '0' && ch <= '9')
						value = (value << 4) + ch - '0';
					else if (ch >= 'A' && ch <= 'F')
						value = (value << 4) + ch - 'A' + 10;
					else if (ch >= 'a' && ch <= 'f')
						value = (value << 4) + ch - 'a' + 10;
					else
						throw ReaderError (
							String.Format (
								"invalid hexadecimal digit: {0} (#x{1:X})",
								(char)ch,
								ch));
				}
			} else {
				while (PeekChar () != ';' && PeekChar () != -1) {
					int ch = ReadChar ();

					if (ch >= '0' && ch <= '9')
						value = value * 10 + ch - '0';
					else
						throw ReaderError (
							String.Format (
								"invalid decimal digit: {0} (#x{1:X})",
								(char)ch,
								ch));
				}
			}

			ReadChar (); // ';'

			AppendValueChar (value);
		}

		private void ReadEntityReference (bool ignoreEntityReferences)
		{
			nameLength = 0;

			int ch = PeekChar ();

			while (ch != ';' && ch != -1) {
				AppendNameChar (ReadChar ());
				ch = PeekChar ();
			}

			Expect (';');

			string name = CreateNameString ();

			switch (name)
			{
				case "lt":
					AppendValueChar ('<');
					break;
				case "gt":
					AppendValueChar ('>');
					break;
				case "amp":
					AppendValueChar ('&');
					break;
				case "apos":
					AppendValueChar ('\'');
					break;
				case "quot":
					AppendValueChar ('"');
					break;
				default:
					if (ignoreEntityReferences) {
						AppendValueChar ('&');

						foreach (char ch2 in name) {
							AppendValueChar (ch2);
						}

						AppendValueChar (';');
					} else {
						returnEntityReference = true;
						entityReferenceName = name;
					}
					break;
			}
		}

		// The reader is positioned on the first character of
		// the attribute name.
		private void ReadAttributes ()
		{
			do {
				string name = ReadName ();
				SkipWhitespace ();
				Expect ('=');
				SkipWhitespace ();
				string value = ReadAttribute ();
				SkipWhitespace ();

				if (name == "xmlns")
					parserContext.NamespaceManager.AddNamespace (String.Empty, UnescapeAttributeValue (value));
				else if (name.StartsWith ("xmlns:"))
					parserContext.NamespaceManager.AddNamespace (name.Substring (6), UnescapeAttributeValue (value));

				AddAttribute (name, value);
			} while (PeekChar () != '/' && PeekChar () != '>' && PeekChar () != -1);
		}

		// The reader is positioned on the quote character.
		// *Keeps quote char* to value to get_QuoteChar() correctly.
		private string ReadAttribute ()
		{
			valueLength = 0;

			int quoteChar = ReadChar ();

			if (quoteChar != '\'' && quoteChar != '\"')
				throw ReaderError ("an attribute value was not quoted");

			AppendValueChar (quoteChar);

			while (PeekChar () != quoteChar) {
				int ch = ReadChar ();

				switch (ch)
				{
				case '<':
					throw ReaderError ("attribute values cannot contain '<'");
				case -1:
					throw ReaderError ("unexpected end of file in an attribute value");
				default:
					AppendValueChar (ch);
					break;
				}
			}

			ReadChar (); // quoteChar
			AppendValueChar (quoteChar);

			return CreateValueString ();
		}

		// The reader is positioned on the first character
		// of the target.
		//
		// Now it also reads XmlDeclaration, this method name became improper...
		private void ReadProcessingInstruction ()
		{
			string target = ReadName ();
			SkipWhitespace ();

			valueLength = 0;

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == '?' && PeekChar () == '>') {
					ReadChar ();
					break;
				}

				AppendValueChar ((char)ch);
			}

/* for future use
			if(target == "xml") && parserContext.InputState != XmlParserInputState.Start)
				throw new XmlException("Xml declaration is not allowed here.");
			else {
				parserContext.InputState = XmlParserInputState.DTD; //for future use
			}
*/
			SetProperties (
				target == "xml" ?
				XmlNodeType.XmlDeclaration :
				XmlNodeType.ProcessingInstruction, // nodeType
				target, // name
				false, // isEmptyElement
				CreateValueString (), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character after
		// the leading '<!'.
		private void ReadDeclaration ()
		{
			int ch = PeekChar ();

			switch (ch)
			{
			case '-':
				Expect ("--");
				ReadComment ();
				break;
			case '[':
				ReadChar ();
				Expect ("CDATA[");
				ReadCDATA ();
				break;
			case 'D':
				Expect ("DOCTYPE");
				ReadDoctypeDecl ();
				break;
			}
		}

		// The reader is positioned on the first character after
		// the leading '<!--'.
		private void ReadComment ()
		{
			valueLength = 0;

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == '-' && PeekChar () == '-') {
					ReadChar ();

					if (PeekChar () != '>')
						throw ReaderError ("comments cannot contain '--'");

					ReadChar ();
					break;
				}

				AppendValueChar ((char)ch);
			}

			SetProperties (
				XmlNodeType.Comment, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				CreateValueString (), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character after
		// the leading '<![CDATA['.
		private void ReadCDATA ()
		{
			valueLength = 0;

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == ']' && PeekChar () == ']') {
					ch = ReadChar (); // ']'

					if (PeekChar () == '>') {
						ReadChar (); // '>'
						break;
					} else {
						AppendValueChar (']');
						AppendValueChar (']');
						ch = ReadChar ();
					}
				}

				AppendValueChar ((char)ch);
			}

			SetProperties (
				XmlNodeType.CDATA, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				CreateValueString (), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character after
		// the leading '<!DOCTYPE'.
		private void ReadDoctypeDecl ()
		{
			string doctypeName = null;
			string publicId = String.Empty;
			string systemId = String.Empty;

			SkipWhitespace ();
			doctypeName = ReadName ();
			SkipWhitespace ();
			switch(PeekChar ())
			{
			case 'S':
				systemId = ReadSystemLiteral (true);
				break;
			case 'P':
				publicId = ReadPubidLiteral ();
				SkipWhitespace ();
				systemId = ReadSystemLiteral (false);
				break;
			}
			SkipWhitespace ();


			if(PeekChar () == '[')
			{
				// read markupdecl etc. or end of decl
				ReadChar ();
				int startPos = currentTag.Length;
				do {
					ReadDTDInternalSubset ();
				} while (nodeType != XmlNodeType.None);
				int endPos = currentTag.Length - 1;
				parserContext.InternalSubset = currentTag.ToString (startPos, endPos - startPos);
			}
			// end of DOCTYPE decl.
			SkipWhitespace ();
			Expect ('>');

			// set properties for <!DOCTYPE> node
			SetProperties (
				XmlNodeType.DocumentType, // nodeType
				doctypeName, // name
				false, // isEmptyElement
				parserContext.InternalSubset, // value
				true // clearAttributes
				);
		}

		// Read any one of following:
		//   elementdecl, AttlistDecl, EntityDecl, NotationDecl,
		//   PI, Comment, Parameter Entity, or doctype termination char(']')
		//
		// returns a node of some nodeType or null, setting nodeType.
		//	 (if None then ']' was found.)
		private void ReadDTDInternalSubset()
		{
			SkipWhitespace ();
			switch(ReadChar ())
			{
			case ']':
				nodeType = XmlNodeType.None;
				break;
			case '%':
				string peName = ReadName ();
				Expect (';');
				nodeType = XmlNodeType.EntityReference;	// It's chating a bit;-)
				break;
			case '<':
				switch(ReadChar ())
				{
				case '?':
					ReadProcessingInstruction ();
					break;
				case '!':
					switch(ReadChar ())
					{
					case '-':
						Expect ('-');
						ReadComment ();
						break;
					case 'E':
						switch(ReadChar ())
						{
						case 'N':
							Expect ("TITY");
							ReadEntityDecl ();
							break;
						case 'L':
							Expect ("EMENT");
							ReadElementDecl ();
							break;
						default:
							throw ReaderError ("Syntax Error after '<!E' (ELEMENT or ENTITY must be found)");
						}
						break;
					case 'A':
						Expect ("TTLIST");
						ReadAttListDecl ();
						break;
					case 'N':
						Expect ("OTATION");
						ReadNotationDecl ();
						break;
					default:
						throw ReaderError ("Syntax Error after '<!' characters.");
					}
					break;
				default:
					throw ReaderError ("Syntax Error after '<' character.");
				}
				break;
			default:
				throw ReaderError ("Syntax Error inside doctypedecl markup.");
			}
		}

		// The reader is positioned on the head of the name.
		private void ReadElementDecl()
		{
			while(ReadChar () != '>');
		}

		private void ReadEntityDecl()
		{
			while(ReadChar () != '>');
		}

		private void ReadAttListDecl()
		{
			while(ReadChar () != '>');
		}

		private void ReadNotationDecl()
		{
			while(ReadChar () != '>');
		}
		
		// The reader is positioned on the first 'S' of "SYSTEM".
		private string ReadSystemLiteral (bool expectSYSTEM)
		{
			if(expectSYSTEM)
				Expect ("SYSTEM");
			SkipWhitespace ();
			int quoteChar = ReadChar ();	// apos or quot
			int startPos = currentTag.Length;
			int c = 0;
			while(c != quoteChar) {
				c = ReadChar ();
				if(c < 0) throw ReaderError ("Unexpected end of stream in ExternalID.");
			}
			return currentTag.ToString (startPos, currentTag.Length - 1 - startPos);
		}

		private string ReadPubidLiteral()
		{
			Expect ("PUBLIC");
			SkipWhitespace ();
			int quoteChar = ReadChar ();
			int startPos = currentTag.Length;
			int c = 0;
			while(c != quoteChar)
			{
				c = ReadChar ();
				if(c < 0) throw ReaderError ("Unexpected end of stream in ExternalID.");
				if(c != quoteChar && !XmlConstructs.IsPubid (c))
					throw ReaderError("character '" + (char)c + "' not allowed for PUBLIC ID");
			}
			ReadChar();	// skips quoteChar
			return currentTag.ToString (startPos, currentTag.Length - 1 - startPos);
		}

		// The reader is positioned on the first character
		// of the name.
		private string ReadName ()
		{
			if (!XmlConstructs.IsNameStart (PeekChar ()))
				throw ReaderError ("a name did not start with a legal character");

			nameLength = 0;

			AppendNameChar (ReadChar ());

			while (XmlConstructs.IsName (PeekChar ())) {
				AppendNameChar (ReadChar ());
			}

			return CreateNameString ();
		}

		// Read the next character and compare it against the
		// specified character.
		private void Expect (int expected)
		{
			int ch = ReadChar ();

			if (ch != expected) {
				throw ReaderError (
					String.Format (
						"expected '{0}' ({1:X}) but found '{2}' ({3:X})",
						(char)expected,
						expected,
						(char)ch,
						ch));
			}
		}

		private void Expect (string expected)
		{
			int len = expected.Length;
			for(int i=0; i< len; i++)
				Expect (expected[i]);
		}

		// Does not consume the first non-whitespace character.
		private void SkipWhitespace ()
		{
			//FIXME: Should not skip if whitespaceHandling == WhiteSpaceHandling.None
			while (XmlConstructs.IsSpace (PeekChar ()))
				ReadChar ();
		}

		private bool ReadWhitespace ()
		{
			valueLength = 0;
			int ch = PeekChar ();
			do {
				AppendValueChar (ReadChar ());
			} while ((ch = PeekChar ()) != -1 && XmlConstructs.IsSpace (ch));

			if (ch != -1 && ch != '<')
				ReadText (false);
			else
				SetProperties (XmlNodeType.Whitespace,
					       String.Empty,
					       false,
					       CreateValueString (),
					       true);

			return (PeekChar () != -1);
		}

		// read entity reference from attribute string and if parsable then return the value.
		private string ReadAttributeValueReference ()
		{
			int endEntityPosition = attributeString.IndexOf(';',
				attributeValuePos);
			string entityName = attributeString.Substring (attributeValuePos + 1,
				endEntityPosition - attributeValuePos - 1);

			attributeValuePos = endEntityPosition + 1;

			if(entityName [0] == '#') {
				char c;
				// character entity
				if(entityName [1] == 'x') {
					// hexadecimal
					c = (char) int.Parse ("0" + entityName.Substring (2),
						System.Globalization.NumberStyles.HexNumber);
				} else {
					// decimal
					c = (char) int.Parse (entityName.Substring (1));
				}
				return c.ToString();
			}
			else {
				switch(entityName)
				{
				case "lt": return "<";
				case "gt": return ">";
				case "amp": return "&";
				case "quot": return "\"";
				case "apos": return "'";
				default: return null;
				}
			}
		}

		private string UnescapeAttributeValue (string unresolved)
		{
			if(unresolved == null) return null;
			StringBuilder resolved = new StringBuilder();
			int pos = 0;

			// trim start/end edge of quotation character.
			unresolved = unresolved.Substring (1, unresolved.Length - 2);

			int next = unresolved.IndexOf ('&');
			if(next < 0)
				return unresolved;

			while(next >= 0) {
				if(pos < next)
					resolved.Append (unresolved.Substring (pos, next - pos));// - 1);
				int endPos = unresolved.IndexOf (';', next+1);
				string entityName =
					unresolved.Substring (next + 1, endPos - next - 1);
				if(entityName [0] == '#') {
					char c;
					// character entity
					if(entityName [1] == 'x') {
						// hexadecimal
						c = (char) int.Parse ("0" + entityName.Substring (2),
							System.Globalization.NumberStyles.HexNumber);
					} else {
						// decimal
						c = (char) int.Parse (entityName.Substring (1));
					}
					resolved.Append (c);
				} else {
					switch(entityName) {
					case "lt": resolved.Append ("<"); break;
					case "gt": resolved.Append (">"); break;
					case "amp": resolved.Append ("&"); break;
					case "quot": resolved.Append ("\""); break;
					case "apos": resolved.Append ("'"); break;
					// With respect to "Value", MS document is helpless
					// and the implemention returns inconsistent value
					// (e.g. XML: "&ent; &amp;ent;" ---> Value: "&ent; &ent;".)
					default: resolved.Append ("&" + entityName + ";"); break;
					}
				}
				pos = endPos + 1;
				if(pos > unresolved.Length)
					break;
				next = unresolved.IndexOf('&', pos);
			}
			resolved.Append (unresolved.Substring(pos));

			return resolved.ToString();
		}

		#endregion
	}
}

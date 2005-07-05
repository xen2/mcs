//
//
// There are two kind of sort keys : which are computed and which are laid out
// as an indexed array. Computed sort keys are:
//
//	- Surrogate
//	- PrivateUse
//
// Also, for composite characters it should prepare different index table.
//
// Though it is possible to "compute" level 3 weights, they are still dumped
// to an array to avoid execution cost.
//

//
// * sortkey getter signature
//
//	int GetSortKey (string s, int index, SortKeyBuffer buf)
//	Stores sort key for corresponding character element into buf and
//	returns the length of the consumed _source_ character element in s.
//
// * character length to consume
//
//	If there are characters whose primary weight is 0, they are consumed
//	and considered as a part of the character element.
//
#define Binary

using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Mono.Globalization.Unicode
{
	internal class MSCompatSortKeyTableGenerator
	{
		public static void Main (string [] args)
		{
			new MSCompatSortKeyTableGenerator ().Run (args);
		}

		const int DecompositionWide = 1; // fixed
		const int DecompositionSub = 2; // fixed
		const int DecompositionSmall = 3;
		const int DecompositionIsolated = 4;
		const int DecompositionInitial = 5;
		const int DecompositionFinal = 6;
		const int DecompositionMedial = 7;
		const int DecompositionNoBreak = 8;
		const int DecompositionVertical = 9;
		const int DecompositionFraction = 0xA;
		const int DecompositionFont = 0xB;
		const int DecompositionSuper = 0xC; // fixed
		const int DecompositionFull = 0xE;
		const int DecompositionNarrow = 0xD;
		const int DecompositionCircle = 0xF;
		const int DecompositionSquare = 0x10;
		const int DecompositionCompat = 0x11;
		const int DecompositionCanonical = 0x12;

		TextWriter Result = Console.Out;

		byte [] fillIndex = new byte [256]; // by category
		CharMapEntry [] map = new CharMapEntry [char.MaxValue + 1];

		char [] specialIgnore = new char [] {
			'\u3099', '\u309A', '\u309B', '\u309C', '\u0BCD',
			'\u0E47', '\u0E4C', '\uFF9E', '\uFF9F'
			};

		// FIXME: need more love (as always)
		char [] alphabets = new char [] {'A', 'B', 'C', 'D', 'E', 'F',
			'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q',
			'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
			'\u0292', '\u01BE', '\u0298'};
		byte [] alphaWeights = new byte [] {
			2, 9, 0xA, 0x1A, 0x21,
			0x23, 0x25, 0x2C, 0x32, 0x35,
			0x36, 0x48, 0x51, 0x70, 0x7C,
			0x7E, 0x89, 0x8A, 0x91, 0x99,
			0x9F, 0xA2, 0xA4, 0xA6, 0xA7,
			0xA9, 0xAA, 0xB3, 0xB4};

		bool [] isSmallCapital = new bool [char.MaxValue + 1];
		bool [] isUppercase = new bool [char.MaxValue + 1];

		byte [] decompType = new byte [char.MaxValue + 1];
		int [] decompIndex = new int [char.MaxValue + 1];
		int [] decompLength = new int [char.MaxValue + 1];
		int [] decompValues;
		decimal [] decimalValue = new decimal [char.MaxValue + 1];

		byte [] diacritical = new byte [char.MaxValue + 1];

		string [] diacritics = new string [] {
			// LATIN
			"WITH ACUTE;", "WITH GRAVE;", " DOT ABOVE;", " MIDDLE DOT;",
			"WITH CIRCUMFLEX;", "WITH DIAERESIS;", "WITH CARON;", "WITH BREVE;",
			" DIALYTIKA AND TONOS;", "WITH MACRON;", "WITH TILDE;", " RING ABOVE;",
			" OGONEK;", " CEDILLA;",
			//
			" DOUBLE ACUTE;", " ACUTE AND DOT ABOVE;",
			" STROKE;", " CIRCUMFLEX AND ACUTE;",
			" DIAERESIS AND ACUTE;", "WITH CIRCUMFLEX AND GRAVE;", " L SLASH;",
			" DIAERESIS AND GRAVE;",
			" BREVE AND ACUTE;",
			" CARON AND DOT ABOVE;", " BREVE AND GRAVE;",
			" MACRON AND ACUTE;",
			" MACRON AND GRAVE;",
			//
			" DIAERESIS AND CARON", " DOT ABOVE AND MACRON", " TILDE AND ACUTE",
			" RING ABOVE AND ACUTE",
			" DIAERESIS AND MACRON", " CEDILLA AND ACUTE", " MACRON AND DIAERESIS",
			" CIRCUMFLEX AND TILDE",
			" TILDE AND DIAERESIS",
			" STROKE AND ACUTE",
			" BREVE AND TILDE",
			" CEDILLA AND BREVE",
			" OGONEK AND MACRON",
			//
			" HOOK;", "LEFT HOOK;", " WITH HOOK ABOVE;",
			" DOUBLE GRAVE;",
			" INVERTED BREVE",
			" PRECEDED BY APOSTROPHE",
			" HORN;",
			" LINE BELOW;", " CIRCUMFLEX AND HOOK ABOVE",
			" PALATAL HOOK",
			" DOT BELOW;",
			" RETROFLEX;", "DIAERESIS BELOW",
			" RING BELOW",
			" CIRCUMFLEX BELOW", "HORN AND ACUTE",
			" BREVE BELOW;", " HORN AND GRAVE",
			" TILDE BELOW",
			" DOT BELOW AND DOT ABOVE",
			" RIGHT HALF RING", " HORN AND TILDE",
			" CIRCUMFLEX AND DOT BELOW",
			" BREVE AND DOT BELOW",
			" DOT BELOW AND MACRON",
			" HORN AND HOOK ABOVE",
			" HORN AND DOT",
			// CIRCLED, PARENTHESIZED and so on
			"CIRCLED DIGIT", "CIRCLED NUMBER", "CIRCLED LATIN",
			"CIRCLED KATAKANA", "CIRCLED SANS-SERIF",
			"PARENTHESIZED DIGIT", "PARENTHESIZED NUMBER", "PARENTHESIZED LATIN",
			};
		byte [] diacriticWeights = new byte [] {
			// LATIN.
			0xE, 0xF, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
			0x17, 0x19, 0x1A, 0x1B, 0x1C,
			//
			0x1D, 0x1D, 0x1E, 0x1E, 0x1F, 0x1F, 0x1F,
			0x20, 0x21, 0x22, 0x22, 0x23, 0x24,
			//
			0x25, 0x25, 0x25, 0x26, 0x28, 0x28, 0x28,
			0x29, 0x2A, 0x2B, 0x2C, 0x2F, 0x30,
			//
			0x43, 0x43, 0x43, 0x44, 0x46, 0x48,
			0x52, 0x55, 0x55, 0x57, 0x58, 0x59, 0x59, 0x5A,
			0x60, 0x60, 0x61, 0x61, 0x63, 0x68, 
			0x69, 0x69, 0x6A, 0x6D, 0x6E,
			0x95, 0xAA,
			// CIRCLED, PARENTHESIZED and so on.
			0xEE, 0xEE, 0xEE, 0xEE, 0xEE,
			0xF3, 0xF3, 0xF3
			};

		int [] numberSecondaryWeightBounds = new int [] {
			0x660, 0x680, 0x6F0, 0x700, 0x960, 0x970,
			0x9E0, 0x9F0, 0x9F4, 0xA00, 0xA60, 0xA70,
			0xAE0, 0xAF0, 0xB60, 0xB70, 0xBE0, 0xC00,
			0xC60, 0xC70, 0xCE0, 0xCF0, 0xD60, 0xD70,
			0xE50, 0xE60, 0xED0, 0xEE0
			};

		char [] orderedCyrillic;
		char [] orderedGurmukhi;
		char [] orderedGujarati;
		char [] orderedGeorgian;
		char [] orderedThaana;

		static readonly char [] orderedTamilConsonants = new char [] {
			// based on traditional Tamil consonants, except for
			// Grantha (where Microsoft breaks traditionalism).
			// http://www.angelfire.com/empire/thamizh/padanGaL
			'\u0B95', '\u0B99', '\u0B9A', '\u0B9E', '\u0B9F',
			'\u0BA3', '\u0BA4', '\u0BA8', '\u0BAA', '\u0BAE',
			'\u0BAF', '\u0BB0', '\u0BB2', '\u0BB5', '\u0BB4',
			'\u0BB3', '\u0BB1', '\u0BA9', '\u0B9C', '\u0BB8',
			'\u0BB7', '\u0BB9'};

		// cp -> character name (only for some characters)
		ArrayList sortableCharNames = new ArrayList ();

		// cp -> arrow value (int)
		ArrayList arrowValues = new ArrayList ();

		// cp -> box value (int)
		ArrayList boxValues = new ArrayList ();

		// cp -> level1 value
		Hashtable arabicLetterPrimaryValues = new Hashtable ();
		Hashtable cyrillicLetterPrimaryValues = new Hashtable ();

		// letterName -> cp
		Hashtable arabicNameMap = new Hashtable ();
		Hashtable cyrillicNameMap = new Hashtable ();

		// cp -> Hashtable [decompType] -> cp
		Hashtable nfkdMap = new Hashtable ();

		// Latin letter -> ArrayList [int]
		Hashtable latinMap = new Hashtable ();

		ArrayList jisJapanese = new ArrayList ();
		ArrayList nonJisJapanese = new ArrayList ();

		ushort [] cjkJA = new ushort [char.MaxValue +1];// - 0x4E00];
		ushort [] cjkCHS = new ushort [char.MaxValue +1];// - 0x3100];
		ushort [] cjkCHT = new ushort [char.MaxValue +1];// - 0x4E00];
		ushort [] cjkKO = new ushort [char.MaxValue +1];// - 0x4E00];
		byte [] cjkKOlv2 = new byte [char.MaxValue +1];// - 0x4E00];

		byte [] ignorableFlags = new byte [char.MaxValue + 1];

		static double [] unicodeAge = new double [char.MaxValue + 1];

		ArrayList tailorings = new ArrayList ();

		void Run (string [] args)
		{
			string dirname = args.Length == 0 ? "downloaded" : args [0];
			ParseSources (dirname);
			Console.Error.WriteLine ("parse done.");

			ModifyParsedValues ();
			GenerateCore ();
			Console.Error.WriteLine ("generation done.");
			Serialize ();
			Console.Error.WriteLine ("serialization done.");
/*
StreamWriter sw = new StreamWriter ("agelog.txt");
for (int i = 0; i < char.MaxValue; i++) {
bool shouldBe = false;
switch (Char.GetUnicodeCategory ((char) i)) {
case UnicodeCategory.Format: case UnicodeCategory.OtherNotAssigned:
	shouldBe = true; break;
}
if (unicodeAge [i] >= 3.1)
	shouldBe = true;
//if (IsIgnorable (i) != shouldBe)
sw.WriteLine ("{1} {2} {3} {0:X04} {4} {5}", i, unicodeAge [i], IsIgnorable (i), IsIgnorableSymbol (i), char.GetUnicodeCategory ((char) i), IsIgnorable (i) != shouldBe ? '!' : ' ');
}
sw.Close ();
*/
		}

		byte [] CompressArray (byte [] source, CodePointIndexer i)
		{
			return (byte []) CodePointIndexer.CompressArray  (
				source, typeof (byte), i);
		}

		ushort [] CompressArray (ushort [] source, CodePointIndexer i)
		{
			return (ushort []) CodePointIndexer.CompressArray  (
				source, typeof (ushort), i);
		}

		void Serialize ()
		{
			// Tailorings
			SerializeTailorings ();

			byte [] categories = new byte [map.Length];
			byte [] level1 = new byte [map.Length];
			byte [] level2 = new byte [map.Length];
			byte [] level3 = new byte [map.Length];
			ushort [] widthCompat = new ushort [map.Length];
			for (int i = 0; i < map.Length; i++) {
				categories [i] = map [i].Category;
				level1 [i] = map [i].Level1;
				level2 [i] = map [i].Level2;
				level3 [i] = ComputeLevel3Weight ((char) i);
				switch (decompType [i]) {
				case DecompositionNarrow:
				case DecompositionWide:
				case DecompositionSuper:
				case DecompositionSub:
					// they are always 1 char
					widthCompat [i] = (ushort) decompValues [decompIndex [i]];
					break;
				}
			}

			// compress
			ignorableFlags = CompressArray (ignorableFlags,
				MSCompatUnicodeTableUtil.Ignorable);
			categories = CompressArray (categories,
				MSCompatUnicodeTableUtil.Category);
			level1 = CompressArray (level1, 
				MSCompatUnicodeTableUtil.Level1);
			level2 = CompressArray (level2, 
				MSCompatUnicodeTableUtil.Level2);
			level3 = CompressArray (level3, 
				MSCompatUnicodeTableUtil.Level3);
			widthCompat = (ushort []) CodePointIndexer.CompressArray (
				widthCompat, typeof (ushort),
				MSCompatUnicodeTableUtil.WidthCompat);
			cjkCHS = CompressArray (cjkCHS,
				MSCompatUnicodeTableUtil.CjkCHS);
			cjkCHT = CompressArray (cjkCHT,
				MSCompatUnicodeTableUtil.Cjk);
			cjkJA = CompressArray (cjkJA,
				MSCompatUnicodeTableUtil.Cjk);
			cjkKO = CompressArray (cjkKO,
				MSCompatUnicodeTableUtil.Cjk);
			cjkKOlv2 = CompressArray (cjkKOlv2,
				MSCompatUnicodeTableUtil.Cjk);

			// Ignorables
			Result.WriteLine ("internal static readonly byte [] ignorableFlags = new byte [] {");
#if Binary
			MemoryStream ms = new MemoryStream ();
			BinaryWriter binary = new BinaryWriter (ms);
			binary.Write (ignorableFlags.Length);
#endif
			for (int i = 0; i < ignorableFlags.Length; i++) {
				byte value = ignorableFlags [i];
				if (value < 10)
					Result.Write ("{0},", value);
				else
					Result.Write ("0x{0:X02},", value);
#if Binary
				binary.Write (value);
#endif
				if ((i & 0xF) == 0xF)
					Result.WriteLine ("// {0:X04}", i - 0xF);
			}
			Result.WriteLine ("};");
			Result.WriteLine ();

			// Primary category
			Result.WriteLine ("internal static readonly byte [] categories = new byte [] {");
#if Binary
			binary.Write (categories.Length);
#endif
			for (int i = 0; i < categories.Length; i++) {
				byte value = categories [i];
				if (value < 10)
					Result.Write ("{0},", value);
				else
					Result.Write ("0x{0:X02},", value);
#if Binary
				binary.Write (value);
#endif
				if ((i & 0xF) == 0xF)
					Result.WriteLine ("// {0:X04}", i - 0xF);
			}
			Result.WriteLine ("};");
			Result.WriteLine ();

			// Primary weight value
			Result.WriteLine ("internal static readonly byte [] level1 = new byte [] {");
#if Binary
			binary.Write (level1.Length);
#endif
			for (int i = 0; i < level1.Length; i++) {
				byte value = level1 [i];
				if (value < 10)
					Result.Write ("{0},", value);
				else
					Result.Write ("0x{0:X02},", value);
#if Binary
				binary.Write (value);
#endif
				if ((i & 0xF) == 0xF)
					Result.WriteLine ("// {0:X04}", i - 0xF);
			}
			Result.WriteLine ("};");
			Result.WriteLine ();

			// Secondary weight
			Result.WriteLine ("internal static readonly byte [] level2 = new byte [] {");
#if Binary
			binary.Write (level2.Length);
#endif
			for (int i = 0; i < level2.Length; i++) {
				byte value = level2 [i];
				if (value < 10)
					Result.Write ("{0},", value);
				else
					Result.Write ("0x{0:X02},", value);
#if Binary
				binary.Write (value);
#endif
				if ((i & 0xF) == 0xF)
					Result.WriteLine ("// {0:X04}", i - 0xF);
			}
			Result.WriteLine ("};");
			Result.WriteLine ();

			// Thirtiary weight
			Result.WriteLine ("internal static readonly byte [] level3 = new byte [] {");
#if Binary
			binary.Write (level3.Length);
#endif
			for (int i = 0; i < level3.Length; i++) {
				byte value = level3 [i];
				if (value < 10)
					Result.Write ("{0},", value);
				else
					Result.Write ("0x{0:X02},", value);
#if Binary
				binary.Write (value);
#endif
				if ((i & 0xF) == 0xF)
					Result.WriteLine ("// {0:X04}", i - 0xF);
			}
			Result.WriteLine ("};");
			Result.WriteLine ();

			// Width insensitivity mappings
			// (for now it is more lightweight than dumping the
			// entire NFKD table).
			Result.WriteLine ("internal static readonly ushort [] widthCompat = new ushort [] {");
#if Binary
			binary.Write (widthCompat.Length);
#endif
			for (int i = 0; i < widthCompat.Length; i++) {
				ushort value = widthCompat [i];
				if (value < 10)
					Result.Write ("{0},", value);
				else
					Result.Write ("0x{0:X02},", value);
#if Binary
				binary.Write (value);
#endif
				if ((i & 0xF) == 0xF)
					Result.WriteLine ("// {0:X04}", i - 0xF);
			}
			Result.WriteLine ("};");
			Result.WriteLine ();
#if Binary
			using (FileStream fs = File.Create ("../collation.core.bin")) {
				byte [] array = ms.ToArray ();
				fs.Write (array, 0, array.Length);
			}
#endif

			// CJK
			SerializeCJK ("cjkCHS", cjkCHS, char.MaxValue);
			SerializeCJK ("cjkCHT", cjkCHT, 0x9FB0);
			SerializeCJK ("cjkJA", cjkJA, 0x9FB0);
			SerializeCJK ("cjkKO", cjkKO, 0x9FB0);
			SerializeCJK ("cjkKOlv2", cjkKOlv2, 0x9FB0);
		}

		void SerializeCJK (string name, ushort [] cjk, int max)
		{
			int offset = 0;//char.MaxValue - cjk.Length;
			Result.WriteLine ("static ushort [] {0} = new ushort [] {{", name);
#if Binary
			MemoryStream ms = new MemoryStream ();
			BinaryWriter binary = new BinaryWriter (ms);
#endif
			for (int i = 0; i < cjk.Length; i++) {
				if (i + offset == max)
					break;
				ushort value = cjk [i];
				if (value < 10)
					Result.Write ("{0},", value);
				else
					Result.Write ("0x{0:X04},", value);
#if Binary
				binary.Write (value);
#endif
				if ((i & 0xF) == 0xF)
					Result.WriteLine ("// {0:X04}", i - 0xF + offset);
			}
			Result.WriteLine ("};");
			Result.WriteLine ();
#if Binary
			using (FileStream fs = File.Create (String.Format ("../collation.{0}.bin", name))) {
				byte [] array = ms.ToArray ();
				fs.Write (array, 0, array.Length);
			}
#endif
		}

		void SerializeCJK (string name, byte [] cjk, int max)
		{
			int offset = 0;//char.MaxValue - cjk.Length;
			Result.WriteLine ("static byte [] {0} = new byte [] {{", name);
#if Binary
			MemoryStream ms = new MemoryStream ();
			BinaryWriter binary = new BinaryWriter (ms);
#endif
			for (int i = 0; i < cjk.Length; i++) {
				if (i + offset == max)
					break;
				byte value = cjk [i];
				if (value < 10)
					Result.Write ("{0},", value);
				else
					Result.Write ("0x{0:X02},", value);
#if Binary
				binary.Write (value);
#endif
				if ((i & 0xF) == 0xF)
					Result.WriteLine ("// {0:X04}", i - 0xF + offset);
			}
			Result.WriteLine ("};");
			Result.WriteLine ();
#if Binary
			using (FileStream fs = File.Create (String.Format ("../collation.{0}.bin", name))) {
				byte [] array = ms.ToArray ();
				fs.Write (array, 0, array.Length);
			}
#endif
		}

		void SerializeTailorings ()
		{
			Hashtable indexes = new Hashtable ();
			Hashtable counts = new Hashtable ();
			Result.WriteLine ("static char [] tailorings = new char [] {");
			int count = 0;
#if Binary
			MemoryStream ms = new MemoryStream ();
			BinaryWriter binary = new BinaryWriter (ms);
#endif
			foreach (Tailoring t in tailorings) {
				if (t.Alias != 0)
					continue;
				Result.Write ("/*{0}*/", t.LCID);
				indexes.Add (t.LCID, count);
				char [] values = t.ItemToCharArray ();
				counts.Add (t.LCID, values.Length);
				foreach (char c in values) {
					Result.Write ("'\\x{0:X}', ", (int) c);
					if (++count % 16 == 0)
						Result.WriteLine (" // {0:X04}", count - 16);
#if Binary
					binary.Write ((ushort) c);
#endif
				}
			}
			Result.WriteLine ("};");

			Result.WriteLine ("static TailoringInfo [] tailoringInfos = new TailoringInfo [] {");
#if Binary
			byte [] rawdata = ms.ToArray ();
			ms = new MemoryStream ();
			binary = new BinaryWriter (ms);
			binary.Write (tailorings.Count);
#endif
			foreach (Tailoring t in tailorings) {
				int target = t.Alias != 0 ? t.Alias : t.LCID;
				if (!indexes.ContainsKey (target)) {
					throw new Exception (String.Format ("WARNING: no corresponding definition for tailoring alias. From {0} to {1}", t.LCID, t.Alias));
					continue;
				}
				int idx = (int) indexes [target];
				int cnt = (int) counts [target];
				bool french = t.FrenchSort;
				if (t.Alias != 0)
					foreach (Tailoring t2 in tailorings)
						if (t2.LCID == t.LCID)
							french = t2.FrenchSort;
				Result.WriteLine ("new TailoringInfo ({0}, 0x{1:X}, {2}, {3}), ", t.LCID, idx, cnt, french ? "true" : "false");
#if Binary
				binary.Write (t.LCID);
				binary.Write (idx);
				binary.Write (cnt);
				binary.Write (french);
#endif
			}
			Result.WriteLine ("};");
#if Binary
			binary.Write ((byte) 0xFF);
			binary.Write ((byte) 0xFF);
			binary.Write (rawdata.Length / 2);
			binary.Write (rawdata, 0, rawdata.Length);


			using (FileStream fs = File.Create ("../collation.tailoring.bin")) {
				byte [] array = ms.ToArray ();
				fs.Write (array, 0, array.Length);
			}
#endif
		}

		#region Parse

		void ParseSources (string dirname)
		{
			string unidata =
				dirname + "/UnicodeData.txt";
			string derivedCoreProps = 
				dirname + "/DerivedCoreProperties.txt";
			string scripts = 
				dirname + "/Scripts.txt";
			string cp932 = 
				dirname + "/CP932.TXT";
			string derivedAge = 
				dirname + "/DerivedAge.txt";
			string chXML = dirname + "/common/collation/zh.xml";
			string jaXML = dirname + "/common/collation/ja.xml";
			string koXML = dirname + "/common/collation/ko.xml";

			ParseDerivedAge (derivedAge);

			FillIgnorables ();

			ParseJISOrder (cp932); // in prior to ParseUnidata()
			ParseUnidata (unidata);
			ParseDerivedCoreProperties (derivedCoreProps);
			ParseScripts (scripts);
			ParseCJK (chXML, jaXML, koXML);

			ParseTailorings ("mono-tailoring-source.txt");
		}

		void ParseTailorings (string filename)
		{
			Tailoring t = null;
			int line = 0;
			using (StreamReader sr = new StreamReader (filename)) {
				try {
					while (sr.Peek () >= 0) {
						line++;
						ProcessTailoringLine (ref t,
							sr.ReadLine ().Trim ());
					}
				} catch (Exception) {
					Console.Error.WriteLine ("ERROR at line {0}", line);
					throw;
				}
			}
		}

		// For now this is enough.
		string ParseTailoringSourceValue (string s)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < s.Length; i++) {
				if (s.StartsWith ("\\u")) {
					sb.Append ((char) int.Parse (
						s.Substring (2, 4), NumberStyles.HexNumber),
						1);
					i += 5;
				}
			else
				sb.Append (s [i]);
			}
			return sb.ToString ();
		}

		void ProcessTailoringLine (ref Tailoring t, string s)
		{
			int idx = s.IndexOf ('#');
			if (idx > 0)
				s = s.Substring (0, idx).Trim ();
			if (s.Length == 0 || s [0] == '#')
				return;
			if (s [0] == '@') {
				idx = s.IndexOf ('=');
				if (idx > 0)
					t = new Tailoring (
						int.Parse (s.Substring (1, idx - 1)),
						int.Parse (s.Substring (idx + 1)));
				else
					t = new Tailoring (int.Parse (s.Substring (1)));
				tailorings.Add (t);
				return;
			}
			if (s.StartsWith ("*FrenchSort")) {
				t.FrenchSort = true;
				return;
			}
			string d = "*Diacritical";
			if (s.StartsWith (d)) {
				idx = s.IndexOf ("->");
				t.AddDiacriticalMap (
					byte.Parse (s.Substring (d.Length, idx - d.Length).Trim (),
						NumberStyles.HexNumber),
					byte.Parse (s.Substring (idx + 2).Trim (),
						NumberStyles.HexNumber));
				return;
			}
			idx = s.IndexOf (':');
			if (idx > 0) {
				string source = s.Substring (0, idx).Trim ();
				string [] l = s.Substring (idx + 1).Trim ().Split (' ');
				byte [] b = new byte [4];
				for (int i = 0; i < 4; i++) {
					if (l [i] == "*")
						b [i] = 0;
					else
						b [i] = byte.Parse (l [i],
							NumberStyles.HexNumber);
				}
				t.AddSortKeyMap (ParseTailoringSourceValue (source),
					b);
			}
			idx = s.IndexOf ('=');
			if (idx > 0)
				t.AddReplacementMap (
					ParseTailoringSourceValue (
						s.Substring (0, idx).Trim ()),
					ParseTailoringSourceValue (
						s.Substring (idx + 1).Trim ()));
		}

		void ParseDerivedAge (string filename)
		{
			using (StreamReader file =
				new StreamReader (filename)) {
				while (file.Peek () >= 0) {
					string s = file.ReadLine ();
					int idx = s.IndexOf ('#');
					if (idx >= 0)
						s = s.Substring (0, idx);
					idx = s.IndexOf (';');
					if (idx < 0)
						continue;

					string cpspec = s.Substring (0, idx);
					idx = cpspec.IndexOf ("..");
					NumberStyles nf = NumberStyles.HexNumber |
						NumberStyles.AllowTrailingWhite;
					int cp = int.Parse (idx < 0 ? cpspec : cpspec.Substring (0, idx), nf);
					int cpEnd = idx < 0 ? cp : int.Parse (cpspec.Substring (idx + 2), nf);
					string value = s.Substring (cpspec.Length + 1).Trim ();

					// FIXME: use index
					if (cp > char.MaxValue)
						continue;

					double v = double.Parse (value);
					for (int i = cp; i <= cpEnd; i++)
						unicodeAge [i] = v;
				}
			}
			unicodeAge [0] = double.MaxValue; // never be supported
		}

		void ParseUnidata (string filename)
		{
			ArrayList decompValues = new ArrayList ();
			using (StreamReader unidata =
				new StreamReader (filename)) {
				for (int line = 1; unidata.Peek () >= 0; line++) {
					try {
						ProcessUnidataLine (unidata.ReadLine (), decompValues);
					} catch (Exception) {
						Console.Error.WriteLine ("**** At line " + line);
						throw;
					}
				}
			}
			this.decompValues = (int [])
				decompValues.ToArray (typeof (int));
		}
		
		void ProcessUnidataLine (string s, ArrayList decompValues)
		{
			int idx = s.IndexOf ('#');
			if (idx >= 0)
				s = s.Substring (0, idx);
			idx = s.IndexOf (';');
			if (idx < 0)
				return;
			int cp = int.Parse (s.Substring (0, idx), NumberStyles.HexNumber);
			string [] values = s.Substring (idx + 1).Split (';');

			// FIXME: use index
			if (cp > char.MaxValue)
				return;
			if (IsIgnorable (cp))
				return;

			string name = values [0];

			// isSmallCapital
			if (s.IndexOf ("SMALL CAPITAL") > 0)
				isSmallCapital [cp] = true;

			// latin mapping by character name
			if (s.IndexOf ("LATIN") > 0) {
				int lidx = s.IndexOf ("LETTER DOTLESS ");
				int offset = lidx + 15;
				if (lidx < 0) {
					lidx = s.IndexOf ("LETTER TURNED ");
					offset = lidx + 14;
				}
				if (lidx < 0) {
					lidx = s.IndexOf ("LETTER ");
					offset = lidx + 7;
				}
				char c = lidx > 0 ? s [offset] : char.MinValue;
				char n = s [offset + 1];
				char target = char.MinValue;
				if ('A' <= c && c <= 'Z' &&
					(n == ' ') || n == ';') {
					target = c;
				// FIXME: they are still not working fine.
				if (s.Substring (offset).StartsWith ("OI;")) // 01A2,01A3
					target = 'O';
				if (s.Substring (offset).StartsWith ("ALPHA"))
					target = 'A';
				if (target != char.MinValue);
					ArrayList entry = (ArrayList) latinMap [c];
					if (entry == null) {
						entry = new ArrayList ();
						latinMap [c] = entry;
					}
					entry.Add (cp);
				}
			}

			// Arrow names
			if (0x2000 <= cp && cp < 0x3000) {
				int value = 0;
				// SPECIAL CASES. FIXME: why?
				switch (cp) {
				case 0x21C5: value = -1; break; // E2
				case 0x261D: value = 1; break;
				case 0x27A6: value = 3; break;
				case 0x21B0: value = 7; break;
				case 0x21B1: value = 3; break;
				case 0x21B2: value = 7; break;
				case 0x21B4: value = 5; break;
				case 0x21B5: value = 7; break;
				case 0x21B9: value = -1; break; // E1
				case 0x21CF: value = 7; break;
				case 0x21D0: value = 3; break;
				}
				string [] arrowTargets = new string [] {
					"",
					"UPWARDS",
					"NORTH EAST",
					"RIGHTWARDS",
					"SOUTH EAST",
					"DOWNWARDS",
					"SOUTH WEST",
					"LEFTWARDS",
					"NORTH WEST",
					};
				if (value == 0)
					for (int i = 1; value == 0 && i < arrowTargets.Length; i++)
						if (s.IndexOf (arrowTargets [i]) > 0 &&
							s.IndexOf ("BARB " + arrowTargets [i]) < 0 &&
							s.IndexOf (" OVER") < 0
						)
							value = i;
				if (value > 0)
					arrowValues.Add (new DictionaryEntry (
						cp, value));
			}

			// Box names
			if (0x2500 <= cp && cp < 0x25B0) {
				int value = 0;
				// flags:
				// up:1 down:2 right:4 left:8 vert:16 horiz:32
				// [h,rl] [r] [l]
				// [v,ud] [u] [d]
				// [dr] [dl] [ur] [ul]
				// [vr,udr] [vl,vdl]
				// [hd,rld] [hu,rlu]
				// [hv,udrl,rlv,udh]
				ArrayList flags = new ArrayList (new int [] {
					32, 8 + 4, 8, 4,
					16, 1 + 2, 1, 2,
					4 + 2, 8 + 2, 4 + 1, 8 + 1,
					16 + 4, 1 + 2 + 4, 16 + 8, 1 + 2 + 8,
					32 + 2, 4 + 8 + 2, 32 + 1, 4 + 8 + 1,
					16 + 32, 1 + 2 + 4 + 8, 4 + 8 + 16, 1 + 2 + 32
					});
				byte [] offsets = new byte [] {
					0, 0, 1, 2,
					3, 3, 4, 5,
					6, 7, 8, 9,
					10, 10, 11, 11,
					12, 12, 13, 13,
					14, 14, 14, 14};
				if (s.IndexOf ("BOX DRAWINGS ") > 0) {
					int flag = 0;
					if (s.IndexOf (" UP") > 0)
						flag |= 1;
					if (s.IndexOf (" DOWN") > 0)
						flag |= 2;
					if (s.IndexOf (" RIGHT") > 0)
						flag |= 4;
					if (s.IndexOf (" LEFT") > 0)
						flag |= 8;
					if (s.IndexOf (" VERTICAL") > 0)
						flag |= 16;
					if (s.IndexOf (" HORIZONTAL") > 0)
						flag |= 32;

					int fidx = flags.IndexOf (flag);
					value = fidx < 0 ? fidx : offsets [fidx];
				} else if (s.IndexOf ("BLOCK") > 0) {
					if (s.IndexOf ("ONE EIGHTH") > 0)
						value = 0x12;
					else if (s.IndexOf ("ONE QUARTER") > 0)
						value = 0x13;
					else if (s.IndexOf ("THREE EIGHTHS") > 0)
						value = 0x14;
					else if (s.IndexOf ("HALF") > 0)
						value = 0x15;
					else if (s.IndexOf ("FIVE EIGHTHS") > 0)
						value = 0x16;
					else if (s.IndexOf ("THREE QUARTERS") > 0)
						value = 0x17;
					else if (s.IndexOf ("SEVEN EIGHTHS") > 0)
						value = 0x18;
					else
						value = 0x19;
				}
				if (value >= 0)
					boxValues.Add (new DictionaryEntry (
						cp, value));
			}

			// For some characters store the name and sort later
			// to determine sorting.
			if (0x2100 <= cp && cp <= 0x213F &&
				Char.IsSymbol ((char) cp))
				sortableCharNames.Add (
					new DictionaryEntry (cp, values [0]));
			else if (0x3380 <= cp && cp <= 0x33DD)
				sortableCharNames.Add (new DictionaryEntry (
					cp, values [0].Substring (7)));

			// diacritical weights by character name
if (diacritics.Length != diacriticWeights.Length)
throw new Exception (String.Format ("Should not happen. weights are {0} while labels are {1}", diacriticWeights.Length, diacritics.Length));
			for (int d = 0; d < diacritics.Length; d++) {
				if (s.IndexOf (diacritics [d]) > 0) {
					diacritical [cp] |= diacriticWeights [d];
					continue;
				}
				// also process "COMBINING blah" here
				// For now it is limited to cp < 0x0370
//				if (cp < 0x0300 || cp >= 0x0370)
//					continue;
				string tmp = diacritics [d].TrimEnd (';');
				if (tmp.IndexOf ("WITH ") == 0)
					tmp = tmp.Substring (4);
				tmp = String.Concat ("COMBINING", (tmp [0] != ' ' ? " " : ""), tmp);
				if (values [0] == tmp)
					diacritical [cp] = (byte) (diacriticWeights [d] - 2);
if (values [0] == tmp) Console.Error.WriteLine ("======= {2:X04} : '{0}' / '{1}'", values [0], tmp, cp);
			}
			// Two-step grep required for it.
			if (s.IndexOf ("FULL STOP") > 0 &&
				(s.IndexOf ("DIGIT") > 0 || s.IndexOf ("NUMBER") > 0))
				diacritical [cp] |= 0xF4;

			// Cyrillic letter name
			if (0x0430 <= cp && cp <= 0x0486 &&
				Char.IsLetter ((char) cp)) {
				byte value = (byte) (cyrillicNameMap.Count * 3 + 0x06);
				// Get primary letter name i.e.
				// XXX part of CYRILLIC LETTER XXX yyy
				// e.g. "IZHITSA" for "IZHITSA DOUBLE GRAVE".
				string letterName =
					values [0].Substring (values [0].IndexOf ("LETTER ") + 7);
				int tmpIdx = letterName.IndexOf (' ');
				letterName = tmpIdx < 0 ? letterName : letterName.Substring (0, tmpIdx);
//Console.Error.WriteLine ("Arabic name for {0:X04} is {1}", cp, letterName);
				if (cyrillicNameMap.ContainsKey (letterName))
					value = (byte) cyrillicLetterPrimaryValues [cyrillicNameMap [letterName]];
				else
					cyrillicNameMap [letterName] = cp;

				cyrillicLetterPrimaryValues [cp] = value;
			}

			// Arabic letter name
			if (0x0621 <= cp && cp <= 0x064A &&
				Char.GetUnicodeCategory ((char) cp)
				== UnicodeCategory.OtherLetter) {
				byte value = (byte) (arabicNameMap.Count * 4 + 0x0B);
				switch (cp) {
				case 0x0621:
				case 0x0624:
				case 0x0626:
					// hamza, waw, yeh ... special cases.
					value = 0x07;
					break;
				case 0x0649:
				case 0x064A:
					value = 0x77; // special cases.
					break;
				default:
					// Get primary letter name i.e.
					// XXX part of ARABIC LETTER XXX yyy
					// e.g. that of "TEH MARBUTA" is "TEH".
					string letterName =
						(cp == 0x0640) ?
						// 0x0640 is special: it does
						// not start with ARABIC LETTER
						values [0] :
						values [0].Substring (14);
					int tmpIdx = letterName.IndexOf (' ');
					letterName = tmpIdx < 0 ? letterName : letterName.Substring (0, tmpIdx);
//Console.Error.WriteLine ("Arabic name for {0:X04} is {1}", cp, letterName);
					if (arabicNameMap.ContainsKey (letterName))
						value = (byte) arabicLetterPrimaryValues [arabicNameMap [letterName]];
					else
						arabicNameMap [letterName] = cp;
					break;
				}
				arabicLetterPrimaryValues [cp] = value;
			}

			// Japanese square letter
			if (0x3300 <= cp && cp <= 0x3357)
				if (!ExistsJIS (cp))
					nonJisJapanese.Add (new NonJISCharacter (cp, values [0]));

			// normalizationType
			string decomp = values [4];
			idx = decomp.IndexOf ('<');
			if (idx >= 0) {
				switch (decomp.Substring (idx + 1, decomp.IndexOf ('>') - 1)) {
				case "full":
					decompType [cp] = DecompositionFull;
					break;
				case "sub":
					decompType [cp] = DecompositionSub;
					break;
				case "super":
					decompType [cp] = DecompositionSuper;
					break;
				case "small":
					decompType [cp] = DecompositionSmall;
					break;
				case "isolated":
					decompType [cp] = DecompositionIsolated;
					break;
				case "initial":
					decompType [cp] = DecompositionInitial;
					break;
				case "final":
					decompType [cp] = DecompositionFinal;
					break;
				case "medial":
					decompType [cp] = DecompositionMedial;
					break;
				case "noBreak":
					decompType [cp] = DecompositionNoBreak;
					break;
				case "compat":
					decompType [cp] = DecompositionCompat;
					break;
				case "fraction":
					decompType [cp] = DecompositionFraction;
					break;
				case "font":
					decompType [cp] = DecompositionFont;
					break;
				case "circle":
					decompType [cp] = DecompositionCircle;
					break;
				case "square":
					decompType [cp] = DecompositionSquare;
					break;
				case "wide":
					decompType [cp] = DecompositionWide;
					break;
				case "narrow":
					decompType [cp] = DecompositionNarrow;
					break;
				case "vertical":
					decompType [cp] = DecompositionVertical;
					break;
				default:
					throw new Exception ("Support NFKD type : " + decomp);
				}
			}
			else
				decompType [cp] = DecompositionCanonical;
			decomp = idx < 0 ? decomp : decomp.Substring (decomp.IndexOf ('>') + 2);
			if (decomp.Length > 0) {

				string [] velems = decomp.Split (' ');
				int didx = decompValues.Count;
				decompIndex [cp] = didx;
				foreach (string v in velems)
					decompValues.Add (int.Parse (v, NumberStyles.HexNumber));
				decompLength [cp] = velems.Length;

				// [decmpType] -> this_cp
				int targetCP = (int) decompValues [didx];
				// for "(x)" it specially maps to 'x' .
				// FIXME: check if it is sane
				if (velems.Length == 3 &&
					(int) decompValues [didx] == '(' &&
					(int) decompValues [didx + 2] == ')')
					targetCP = (int) decompValues [didx + 1];
				// special: 0x215F "1/"
				else if (cp == 0x215F)
					targetCP = '1';
				else if (velems.Length > 1 &&
					(targetCP < 0x4C00 || 0x9FBB < targetCP))
					// skip them, except for CJK ideograph compat
					targetCP = 0;

				if (targetCP != 0) {
					Hashtable entry = (Hashtable) nfkdMap [targetCP];
					if (entry == null) {
						entry = new Hashtable ();
						nfkdMap [targetCP] = entry;
					}
					entry [(byte) decompType [cp]] = cp;
				}
			}
			// numeric values
			if (values [5].Length > 0)
				decimalValue [cp] = decimal.Parse (values [5]);
			else if (values [6].Length > 0)
				decimalValue [cp] = decimal.Parse (values [6]);
			else if (values [7].Length > 0) {
				string decstr = values [7];
				idx = decstr.IndexOf ('/');
				if (cp == 0x215F) // special. "1/"
					decimalValue [cp] = 0x1;
				else if (idx > 0)
					// m/n
					decimalValue [cp] = 
						decimal.Parse (decstr.Substring (0, idx))
						/ decimal.Parse (decstr.Substring (idx + 1));
				else if (decstr [0] == '(' &&
					decstr [decstr.Length - 1] == ')')
					// (n)
					decimalValue [cp] =
						decimal.Parse (decstr.Substring (1, decstr.Length - 2));
				else if (decstr [decstr.Length - 1] == '.')
					// n.
					decimalValue [cp] =
						decimal.Parse (decstr.Substring (0, decstr.Length - 1));
				else
					decimalValue [cp] = decimal.Parse (decstr);
			}
		}

		void ParseDerivedCoreProperties (string filename)
		{
			// IsUppercase
			using (StreamReader file =
				new StreamReader (filename)) {
				for (int line = 1; file.Peek () >= 0; line++) {
					try {
						ProcessDerivedCorePropLine (file.ReadLine ());
					} catch (Exception) {
						Console.Error.WriteLine ("**** At line " + line);
						throw;
					}
				}
			}
		}

		void ProcessDerivedCorePropLine (string s)
		{
			int idx = s.IndexOf ('#');
			if (idx >= 0)
				s = s.Substring (0, idx);
			idx = s.IndexOf (';');
			if (idx < 0)
				return;
			string cpspec = s.Substring (0, idx);
			idx = cpspec.IndexOf ("..");
			NumberStyles nf = NumberStyles.HexNumber |
				NumberStyles.AllowTrailingWhite;
			int cp = int.Parse (idx < 0 ? cpspec : cpspec.Substring (0, idx), nf);
			int cpEnd = idx < 0 ? cp : int.Parse (cpspec.Substring (idx + 2), nf);
			string value = s.Substring (cpspec.Length + 1).Trim ();

			// FIXME: use index
			if (cp > char.MaxValue)
				return;

			switch (value) {
			case "Uppercase":
				for (int x = cp; x <= cpEnd; x++)
					isUppercase [x] = true;
				break;
			}
		}

		void ParseScripts (string filename)
		{
			ArrayList cyrillic = new ArrayList ();
			ArrayList gurmukhi = new ArrayList ();
			ArrayList gujarati = new ArrayList ();
			ArrayList georgian = new ArrayList ();
			ArrayList thaana = new ArrayList ();

			using (StreamReader file =
				new StreamReader (filename)) {
				while (file.Peek () >= 0) {
					string s = file.ReadLine ();
					int idx = s.IndexOf ('#');
					if (idx >= 0)
						s = s.Substring (0, idx);
					idx = s.IndexOf (';');
					if (idx < 0)
						continue;

					string cpspec = s.Substring (0, idx);
					idx = cpspec.IndexOf ("..");
					NumberStyles nf = NumberStyles.HexNumber |
						NumberStyles.AllowTrailingWhite;
					int cp = int.Parse (idx < 0 ? cpspec : cpspec.Substring (0, idx), nf);
					int cpEnd = idx < 0 ? cp : int.Parse (cpspec.Substring (idx + 2), nf);
					string value = s.Substring (cpspec.Length + 1).Trim ();

					// FIXME: use index
					if (cp > char.MaxValue)
						continue;

					switch (value) {
					case "Cyrillic":
						for (int x = cp; x <= cpEnd; x++)
							if (!IsIgnorable (x))
								cyrillic.Add ((char) x);
						break;
					case "Gurmukhi":
						for (int x = cp; x <= cpEnd; x++)
							if (!IsIgnorable (x))
								gurmukhi.Add ((char) x);
						break;
					case "Gujarati":
						for (int x = cp; x <= cpEnd; x++)
							if (!IsIgnorable (x))
								gujarati.Add ((char) x);
						break;
					case "Georgian":
						for (int x = cp; x <= cpEnd; x++)
							if (!IsIgnorable (x))
								georgian.Add ((char) x);
						break;
					case "Thaana":
						for (int x = cp; x <= cpEnd; x++)
							if (!IsIgnorable (x))
								thaana.Add ((char) x);
						break;
					}
				}
			}
			cyrillic.Sort (UCAComparer.Instance);
			gurmukhi.Sort (UCAComparer.Instance);
			gujarati.Sort (UCAComparer.Instance);
			georgian.Sort (UCAComparer.Instance);
			thaana.Sort (UCAComparer.Instance);
			orderedCyrillic = (char []) cyrillic.ToArray (typeof (char));
			orderedGurmukhi = (char []) gurmukhi.ToArray (typeof (char));
			orderedGujarati = (char []) gujarati.ToArray (typeof (char));
			orderedGeorgian = (char []) georgian.ToArray (typeof (char));
			orderedThaana = (char []) thaana.ToArray (typeof (char));
		}

		void ParseJISOrder (string filename)
		{
			int line = 1;
			try {
				using (StreamReader file =
					new StreamReader (filename)) {
					for (;file.Peek () >= 0; line++)
						ProcessJISOrderLine (file.ReadLine ());
				}
			} catch (Exception) {
				Console.Error.WriteLine ("---- line {0}", line);
				throw;
			}
		}

		char [] ws = new char [] {'\t', ' '};

		void ProcessJISOrderLine (string s)
		{
			int idx = s.IndexOf ('#');
			if (idx >= 0)
				s = s.Substring (0, idx).Trim ();
			if (s.Length == 0)
				return;
			idx = s.IndexOfAny (ws);
			if (idx < 0)
				return;
			// They start with "0x" so cut them out.
			int jis = int.Parse (s.Substring (2, idx - 2), NumberStyles.HexNumber);
			int cp = int.Parse (s.Substring (idx).Trim ().Substring (2), NumberStyles.HexNumber);
			jisJapanese.Add (new JISCharacter (cp, jis));
		}

		void ParseCJK (string zhXML, string jaXML, string koXML)
		{
			XmlDocument doc = new XmlDocument ();
			doc.XmlResolver = null;
			int v;
			string s;
			string category;
			int offset;
			ushort [] arr;

			// Chinese Simplified
			category = "chs";
			arr = cjkCHS;
			offset = 0;//char.MaxValue - arr.Length;
			doc.Load (zhXML);
			s = doc.SelectSingleNode ("/ldml/collations/collation[@type='pinyin']/rules/pc").InnerText;
			v = 0x8008;
			foreach (char c in s) {
				if (c < '\u3100')
					Console.Error.WriteLine ("---- warning: for {0} {1:X04} is omitted which should be {2:X04}", category, (int) c, v);
				else {
					arr [(int) c - offset] = (ushort) v++;
					if (v % 256 == 0)
						v += 2;
				}
			}

			// Chinese Traditional
			category = "cht";
			arr = cjkCHT;
			offset = 0;//char.MaxValue - arr.Length;
			s = doc.SelectSingleNode ("/ldml/collations/collation[@type='stroke']/rules/pc").InnerText;
			v = 0x8002;
			foreach (char c in s) {
				if (c < '\u4E00')
					Console.Error.WriteLine ("---- warning: for {0} {1:X04} is omitted which should be {2:X04}", category, (int) c, v);
				else {
					arr [(int) c - offset] = (ushort) v++;
					if (v % 256 == 0)
						v += 2;
				}
			}

			// Japanese
			category = "ja";
			arr = cjkJA;
			offset = 0;//char.MaxValue - arr.Length;
			doc.Load (jaXML);
			s = doc.SelectSingleNode ("/ldml/collations/collation/rules/pc").InnerText;
			v = 0x8008;
			foreach (char c in s) {
				if (c < '\u4E00')
					Console.Error.WriteLine ("---- warning: for {0} {1:X04} is omitted which should be {2:X04}", category, (int) c, v);
				else {
					arr [(int) c - offset] = (ushort) v++;
					if (v % 256 == 0)
						v += 2;
				}
			}

			// Korean
			// Korean weight is somewhat complex. It first shifts
			// Hangul category from 52-x to 80-x (they are anyways
			// computed). CJK ideographs are placed at secondary
			// weight, like XX YY 01 zz 01, where XX and YY are
			// corresponding "reset" value and zz is 41,43,45...
			//
			// Unlike chs,cht and ja, Korean value is a combined
			// ushort which is computed as category
			//
			category = "ko";
			arr = cjkKO;
			offset = 0;//char.MaxValue - arr.Length;
			doc.Load (koXML);
			foreach (XmlElement reset in doc.SelectNodes ("/ldml/collations/collation/rules/reset")) {
				XmlElement sc = (XmlElement) reset.NextSibling;
				// compute "category" and "level 1" for the 
				// target "reset" Hangle syllable
				char rc = reset.InnerText [0];
				int ri = ((int) rc - 0xAC00) + 1;
				ushort p = (ushort)
					((ri / 254) * 256 + (ri % 254) + 2);
				// Place the characters after the target.
				s = sc.InnerText;
				v = 0x41;
				foreach (char c in s) {
					arr [(int) c - offset] = p;
					cjkKOlv2 [(int) c - offset] = (byte) v;
					v += 2;
				}
			}
		}

		#endregion

		#region Generation

		void FillIgnorables ()
		{
			for (int i = 0; i <= char.MaxValue; i++) {
				if (Char.GetUnicodeCategory ((char) i) ==
					UnicodeCategory.OtherNotAssigned)
					continue;
				if (IsIgnorable (i))
					ignorableFlags [i] |= 1;
				if (IsIgnorableSymbol (i))
					ignorableFlags [i] |= 2;
				if (IsIgnorableNonSpacing (i))
					ignorableFlags [i] |= 4;
			}
		}

		void ModifyParsedValues ()
		{
			// number, secondary weights
			byte weight = 0x38;
			int [] numarr = numberSecondaryWeightBounds;
			for (int i = 0; i < numarr.Length; i += 2, weight++)
				for (int cp = numarr [i]; cp < numarr [i + 1]; cp++)
					if (Char.IsNumber ((char) cp))
						diacritical [cp] = weight;

			// Modify some decomposition equivalence
			decompType [0xFE31] = 0;
			decompIndex [0xFE31] = 0;
			decompLength [0xFE31] = 0;
			decompType [0xFE32] = 0;
			decompIndex [0xFE32] = 0;
			decompLength [0xFE32] = 0;

			// Korean parens numbers
			for (int i = 0x3200; i <= 0x321C; i++)
				diacritical [i] = 0xA;
			for (int i = 0x3260; i <= 0x327B; i++)
				diacritical [i] = 0xC;

			// Update name part of named characters
			for (int i = 0; i < sortableCharNames.Count; i++) {
				DictionaryEntry de =
					(DictionaryEntry) sortableCharNames [i];
				int cp = (int) de.Key;
				string renamed = null;
				switch (cp) {
				case 0x2101: renamed = "A_1"; break;
				case 0x33C3: renamed = "A_2"; break;
				case 0x2105: renamed = "C_1"; break;
				case 0x2106: renamed = "C_2"; break;
				case 0x211E: renamed = "R1"; break;
				case 0x211F: renamed = "R2"; break;
				// Remove some of them!
				case 0x2103:
				case 0x2109:
				case 0x2116:
				case 0x2117:
				case 0x2118:
				case 0x2125:
				case 0x2127:
				case 0x2129:
				case 0x212E:
				case 0x2132:
					sortableCharNames.RemoveAt (i);
					i--;
					continue;
				}
				if (renamed != null)
					sortableCharNames [i] =
						new DictionaryEntry (cp, renamed);
			}
		}

		void GenerateCore ()
		{
			UnicodeCategory uc;

			#region Specially ignored // 01
			// This will raise "Defined" flag up.
			foreach (char c in specialIgnore)
				map [(int) c] = new CharMapEntry (0, 0, 0);
			#endregion


			#region Variable weights
			// Controls : 06 03 - 06 3D
			fillIndex [6] = 3;
			for (int i = 0; i < 65536; i++) {
				if (IsIgnorable (i))
					continue;
				char c = (char) i;
				uc = Char.GetUnicodeCategory (c);
				// NEL is whitespace but not ignored here.
				if (uc == UnicodeCategory.Control &&
					!Char.IsWhiteSpace (c) || c == '\u0085')
					AddCharMap (c, 6, 1);
			}

			// Apostrophe 06 80
			fillIndex [6] = 0x80;
			AddCharMapGroup ('\'', 6, 1, 0);
			AddCharMap ('\uFE63', 6, 1);

			// Hyphen/Dash : 06 81 - 06 90
			for (int i = 0; i < char.MaxValue; i++) {
				if (!IsIgnorable (i) &&
					Char.GetUnicodeCategory ((char) i) ==
					UnicodeCategory.DashPunctuation) {
					AddCharMapGroup2 ((char) i, 6, 1, 0);
					if (i == 0x2011) {
						// SPECIAL: add 2027 and 2043
						// Maybe they are regarded the 
						// same hyphens in "central"
						// position.
						AddCharMap ('\u2027', 6, 1);
						AddCharMap ('\u2043', 6, 1);
					}
				}
			}

			// Arabic variable weight chars 06 A0 -
			fillIndex [6] = 0xA0;
			// vowels
			for (int i = 0x64B; i <= 0x650; i++)
				AddArabicCharMap ((char) i);
			// sukun
			AddCharMapGroup ('\u0652', 6, 1, 0);
			// shadda
			AddCharMapGroup ('\u0651', 6, 1, 0);
			#endregion


			#region Nonspacing marks // 01
			// FIXME: 01 03 - 01 B6 ... annoyance :(

			// Combining diacritical marks: 01 DC -

			fillIndex [0x1] = 0x41;
			for (int i = 0x030E; i <= 0x0326; i++)
				if (!IsIgnorable (i))
					AddCharMap ((char) i, 0x1, 1);
			for (int i = 0x0329; i <= 0x0334; i++)
				if (!IsIgnorable (i))
					AddCharMap ((char) i, 0x1, 1);
			for (int i = 0x0339; i <= 0x0341; i++)
				if (!IsIgnorable (i))
					AddCharMap ((char) i, 0x1, 1);
			fillIndex [0x1] = 0x72;
			for (int i = 0x0346; i <= 0x0348; i++)
				if (!IsIgnorable (i))
					AddCharMap ((char) i, 0x1, 1);
			for (int i = 0x02BE; i <= 0x02BF; i++)
				if (!IsIgnorable (i))
					AddCharMap ((char) i, 0x1, 1);
			for (int i = 0x02C1; i <= 0x02C5; i++)
				if (!IsIgnorable (i))
					AddCharMap ((char) i, 0x1, 1);
			for (int i = 0x02CE; i <= 0x02CF; i++)
				if (!IsIgnorable (i))
					AddCharMap ((char) i, 0x1, 1);
			for (int i = 0x02D1; i <= 0x02D3; i++)
				if (!IsIgnorable (i))
					AddCharMap ((char) i, 0x1, 1);
			AddCharMap ('\u02DE', 0x1, 1);
			for (int i = 0x02E4; i <= 0x02E9; i++)
				if (!IsIgnorable (i))
					AddCharMap ((char) i, 0x1, 1);

			// FIXME: needs more love here (it should eliminate
			// all the hacky code above).
			for (int i = 0x0300; i < 0x0370; i++)
				if (!IsIgnorable (i) && diacritical [i] != 0
					/* especiall here*/ && !map [i].Defined)
					map [i] = new CharMapEntry (
						0x1, 0x1, diacritical [i]);

			// LAMESPEC: It should not stop at '\u20E1'. There are
			// a few more characters (that however results in 
			// overflow of level 2 unless we start before 0xDD).
			fillIndex [0x1] = 0xDC;
			for (int i = 0x20d0; i <= 0x20e1; i++)
				AddCharMap ((char) i, 0x1, 1);
			#endregion


			#region Whitespaces // 07 03 -
			fillIndex [0x7] = 0x2;
			AddCharMap (' ', 0x7, 2);
			AddCharMap ('\u00A0', 0x7, 1);
			for (int i = 9; i <= 0xD; i++)
				AddCharMap ((char) i, 0x7, 1);
			for (int i = 0x2000; i <= 0x200B; i++)
				AddCharMap ((char) i, 0x7, 1);

			fillIndex [0x7] = 0x17;
			AddCharMapGroup ('\u2028', 0x7, 1, 0);
			AddCharMapGroup ('\u2029', 0x7, 1, 0);

			// Characters which used to represent layout control.
			// LAMESPEC: Windows developers seem to have thought 
			// that those characters are kind of whitespaces,
			// while they aren't.
			AddCharMap ('\u2422', 0x7, 1, 0); // blank symbol
			AddCharMap ('\u2423', 0x7, 1, 0); // open box
			#endregion

			// category 09 - continued symbols from 08
			fillIndex [0x9] = 2;
			// misc tech mark
			for (int cp = 0x2300; cp <= 0x237A; cp++)
				AddCharMap ((char) cp, 0x9, 1, 0);

			// arrows
			byte [] arrowLv2 = new byte [] {0, 3, 3, 3, 3, 3, 3, 3, 3};
			foreach (DictionaryEntry de in arrowValues) {
				int idx = (int) de.Value;
				int cp = (int) de.Key;
				if (map [cp].Defined)
					continue;
				fillIndex [0x9] = (byte) (0xD8 + idx);
				AddCharMapGroup ((char) cp, 0x9, 0, arrowLv2 [idx]);
				arrowLv2 [idx]++;
			}
			// boxes
			byte [] boxLv2 = new byte [128];
			for (int i = 0; i < boxLv2.Length; i++)
				boxLv2 [i] = 3;
			foreach (DictionaryEntry de in boxValues) {
				int cp = (int) de.Key;
				int idx = (int) de.Value;
				if (map [cp].Defined)
					continue;
				fillIndex [0x9] = (byte) (0xE5 + idx);
				AddCharMapGroup ((char) cp, 0x9, 0, boxLv2 [idx]);
				boxLv2 [idx]++;
			}
			// Some special characters (slanted)
			fillIndex [0x9] = 0xF4;
			AddCharMap ('\u2571', 0x9, 3);
			AddCharMap ('\u2572', 0x9, 3);
			AddCharMap ('\u2573', 0x9, 3);

			// FIXME: implement 0A
			#region Symbols
			fillIndex [0xA] = 2;
			// byte currency symbols
			for (int cp = 0; cp < 0x100; cp++) {
				uc = Char.GetUnicodeCategory ((char) cp);
				if (!IsIgnorable (cp) &&
					uc == UnicodeCategory.CurrencySymbol &&
					cp != '$')
					AddCharMapGroup ((char) cp, 0xA, 1, 0);
			}
			// byte other symbols
			for (int cp = 0; cp < 0x100; cp++) {
				if (cp == 0xA6)
					continue; // SPECIAL: skip FIXME: why?
				uc = Char.GetUnicodeCategory ((char) cp);
				if (!IsIgnorable (cp) &&
					uc == UnicodeCategory.OtherSymbol)
					AddCharMapGroup ((char) cp, 0xA, 1, 0);
			}

			fillIndex [0xA] = 0x2F; // FIXME: it won't be needed
			for (int cp = 0x2600; cp <= 0x2613; cp++)
				AddCharMap ((char) cp, 0xA, 1, 0);
			// Dingbats
			for (int cp = 0x2620; cp <= 0x2770; cp++)
				if (Char.IsSymbol ((char) cp))
					AddCharMap ((char) cp, 0xA, 1, 0);
			// OCR
			for (int i = 0x2440; i < 0x2460; i++)
				AddCharMap ((char) i, 0xA, 1, 0);

			#endregion

			#region Numbers // 0C 02 - 0C E1
			fillIndex [0xC] = 2;

			// 9F8 : Bengali "one less than the denominator"
			AddCharMap ('\u09F8', 0xC, 1);

			ArrayList numbers = new ArrayList ();
			for (int i = 0; i < 65536; i++)
				if (!IsIgnorable (i) &&
					Char.IsNumber ((char) i) &&
					(i < 0x3190 || 0x32C0 < i)) // they are CJK characters
					numbers.Add (i);

			ArrayList numberValues = new ArrayList ();
			foreach (int i in numbers)
				numberValues.Add (new DictionaryEntry (i, decimalValue [(char) i]));
			numberValues.Sort (DecimalDictionaryValueComparer.Instance);

//foreach (DictionaryEntry de in numberValues)
//Console.Error.WriteLine ("****** number {0:X04} : {1} {2}", de.Key, de.Value, decompType [(int) de.Key]);

			decimal prevValue = -1;
			foreach (DictionaryEntry de in numberValues) {
				int cp = (int) de.Key;
				decimal currValue = (decimal) de.Value;
				bool addnew = false;
				if (prevValue < currValue &&
					prevValue - (int) prevValue == 0 &&
					prevValue >= 1) {

					addnew = true;
					// Process Hangzhou and Roman numbers

					// There are some SPECIAL cases.
					if (currValue != 4) // no increment for 4
						fillIndex [0xC]++;

					int xcp;
					if (currValue <= 10) {
						xcp = (int) prevValue + 0x2170 - 1;
						AddCharMap ((char) xcp, 0xC, 0, diacritical [xcp]);
						xcp = (int) prevValue + 0x2160 - 1;
						AddCharMap ((char) xcp, 0xC, 0, diacritical [xcp]);
						fillIndex [0xC] += 2;
						xcp = (int) prevValue + 0x3021 - 1;
						AddCharMap ((char) xcp, 0xC, 0, diacritical [xcp]);
						fillIndex [0xC]++;
					}
					else if (currValue == 11)
						fillIndex [0xC]++;
				}
				if (prevValue < currValue)
					prevValue = currValue;
				if (map [cp].Defined)
					continue;
				// HangZhou and Roman are add later 
				// (code is above)
				else if (0x3021 <= cp && cp < 0x302A
					|| 0x2160 <= cp && cp < 0x216A
					|| 0x2170 <= cp && cp < 0x217A)
					continue;

				if (cp ==  0x215B) // FIXME: why?
					fillIndex [0xC] += 2;
				else if (cp == 0x3021) // FIXME: why?
					fillIndex [0xC]++;
				AddCharMapGroup ((char) cp, 0xC, 0, diacritical [cp]);
				if (addnew || cp <= '9') {
					int mod = (int) currValue - 1;
					int xcp;
					if (1 <= currValue && currValue <= 10) {
						xcp = mod + 0x2776;
						AddCharMap ((char) xcp, 0xC, 0, diacritical [xcp]);
						xcp = mod + 0x2780;
						AddCharMap ((char) xcp, 0xC, 0, diacritical [xcp]);
						xcp = mod + 0x278A;
						AddCharMap ((char) xcp, 0xC, 0, diacritical [xcp]);
					}
					if (1 <= currValue && currValue <= 20) {
						xcp = mod + 0x2460;
						AddCharMap ((char) xcp, 0xC, 0, diacritical [xcp]);
						xcp = mod + 0x2474;
						AddCharMap ((char) xcp, 0xC, 0, diacritical [xcp]);
						xcp = mod + 0x2488;
						AddCharMap ((char) xcp, 0xC, 0, diacritical [xcp]);
					}
				}

				if (cp != 0x09E7 && cp != 0x09EA)
					fillIndex [0xC]++;

				// Add special cases that are not regarded as 
				// numbers in UnicodeCategory speak.
				if (cp == '5') {
					// TONE FIVE
					AddCharMapGroup ('\u01BD', 0xC, 0, 0);
					AddCharMapGroup ('\u01BC', 0xC, 1, 0);
				}
				else if (cp == '6') // FIXME: why?
					fillIndex [0xC]++;
			}

			// 221E: infinity
			fillIndex [0xC] = 0xFF;
			AddCharMap ('\u221E', 0xC, 1);
			#endregion

			#region Letters and NonSpacing Marks (general)

			// ASCII Latin alphabets
			for (int i = 0; i < alphabets.Length; i++)
				AddAlphaMap (alphabets [i], 0xE, alphaWeights [i]);


			// non-ASCII Latin alphabets
			// FIXME: there is no such characters that are placed
			// *after* "alphabets" array items. This is nothing
			// more than a hack that creates dummy weight for
			// primary characters.
			for (int i = 0x0080; i < 0x0300; i++) {
				if (!Char.IsLetter ((char) i))
					continue;
				// For those Latin Letters which has NFKD are
				// not added as independent primary character.
				if (decompIndex [i] != 0)
					continue;
				// SPECIAL CASES:
				// 1.some alphabets have primarily
				//   equivalent ASCII alphabets.
				// 2.some have independent primary weights,
				//   but inside a-to-z range.
				// 3.there are some expanded characters that
				//   are not part of Unicode Standard NFKD.
				// 4. some characters are letter in IsLetter
				//   but not in sortkeys (maybe unicode version
				//   difference caused it).
				switch (i) {
				// 1. skipping them does not make sense
//				case 0xD0: case 0xF0: case 0x131: case 0x138:
//				case 0x184: case 0x185: case 0x186: case 0x189:
//				case 0x18D: case 0x18E: case 0x18F: case 0x190:
//				case 0x194: case 0x195: case 0x196: case 0x19A:
//				case 0x19B: case 0x19C:
				// 2. skipping them does not make sense
//				case 0x14A: // Ng
//				case 0x14B: // ng
				// 3.
				case 0xC6: // AE
				case 0xE6: // ae
				case 0xDE: // Icelandic Thorn
				case 0xFE: // Icelandic Thorn
				case 0xDF: // German ss
				case 0xFF: // German ss
				// 4.
				case 0x1C0: case 0x1C1: case 0x1C2: case 0x1C3:
				// not classified yet
//				case 0x1A6: case 0x1A7: case 0x1A8: case 0x1A9:
//				case 0x1AA: case 0x1B1: case 0x1B7: case 0x1B8:
//				case 0x1B9: case 0x1BA: case 0x1BB: case 0x1BF:
//				case 0x1DD:
					continue;
				}
				AddCharMapGroup ((char) i, 0xE, 1, 0);
			}

			// Greek and Coptic
			fillIndex [0xF] = 02;
			for (int i = 0x0380; i < 0x0390; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0xF, 1);
			fillIndex [0xF] = 02;
			for (int i = 0x0391; i < 0x03CF; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0xF, 1);
			fillIndex [0xF] = 0x40;
			for (int i = 0x03D0; i < 0x0400; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0xF, 1);

			// Cyrillic - character name order
			fillIndex [0x10] = 0x6;
//*
for (int i = 0; i < orderedCyrillic.Length; i++)
Console.Error.WriteLine ("----- {0:x04}", (int) orderedCyrillic [i]);

			// table which is moslty from UCA DUCET.
			for (int i = 0; i < orderedCyrillic.Length; i++) {
				char c = Char.ToUpper (orderedCyrillic [i], CultureInfo.InvariantCulture);
				if (!IsIgnorable ((int) c) &&
					c <= '\u045C' &&
					Char.IsLetter (c)) {
					AddLetterMap (c, 0x10, 0);
					fillIndex [0x10] += 3;
				}
			}
			/*
			for (int i = 0x0460; i < 0x0481; i++) {
				if (Char.IsLetter ((char) i)) {
					AddLetterMap ((char) i, 0x10, 0);
					fillIndex [0x10] += 3;
				}
			}
			*/
/*
			for (int i = 0x0400; i <= 0x0486; i++) {
				if (!Char.IsLetter ((char) i)) {
//					AddCharMap ((char) i, 0x1, 1);
					continue;
				}
				if (!cyrillicLetterPrimaryValues.ContainsKey (i)) {
					Console.Error.WriteLine ("no value for {0:x04}", i);
					continue;
				}
				fillIndex [0x10] = 
					(byte) cyrillicLetterPrimaryValues [i];
				AddLetterMap ((char) i, 0x10, 0);
			}
*/

			// Armenian
			fillIndex [0x11] = 0x3;
			for (int i = 0x0531; i < 0x0586; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0x11, 1);

			// Hebrew
			// -Letters
			fillIndex [0x12] = 0x3;
			for (int i = 0x05D0; i < 0x05FF; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0x12, 1);
			// -Accents
			fillIndex [0x1] = 0x3;
			for (int i = 0x0591; i <= 0x05C2; i++)
				if (i != 0x05BE)
					AddCharMap ((char) i, 0x1, 1);

			// Arabic
			fillIndex [0x1] = 0x8E;
			fillIndex [0x13] = 0x3;
			for (int i = 0x0621; i <= 0x064A; i++) {
				// Abjad
				if (Char.GetUnicodeCategory ((char) i)
					!= UnicodeCategory.OtherLetter) {
					// FIXME: arabic nonspacing marks are
					// in different order.
					AddCharMap ((char) i, 0x1, 1);
					continue;
				}
//				map [i] = new CharMapEntry (0x13,
//					(byte) arabicLetterPrimaryValues [i], 1);
				fillIndex [0x13] = 
					(byte) arabicLetterPrimaryValues [i];
				AddLetterMap ((char) i, 0x13, 0);
			}
			fillIndex [0x13] = 0x84;
			for (int i = 0x0674; i < 0x06D6; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0x13, 1);

			// Devanagari
			// FIXME: it does seem straight codepoint mapping.
			fillIndex [0x14] = 04;
			for (int i = 0x0901; i < 0x0905; i++)
				if (!IsIgnorable (i))
					AddLetterMap ((char) i, 0x14, 2);
			fillIndex [0x14] = 0xB;
			for (int i = 0x0905; i < 0x093A; i++) {
				if (i == 0x0928)
					AddCharMap ('\u0929', 0x14, 0, 8);
				if (i == 0x0930)
					AddCharMap ('\u0931', 0x14, 0, 8);
				if (i == 0x0933)
					AddCharMap ('\u0934', 0x14, 0, 8);
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0x14, 4);
				if (i == 0x090B)
					AddCharMap ('\u0960', 0x14, 4);
				if (i == 0x090C)
					AddCharMap ('\u0961', 0x14, 4);
			}
			fillIndex [0x14] = 0xDA;
			for (int i = 0x093E; i < 0x0945; i++)
				if (!IsIgnorable (i))
					AddLetterMap ((char) i, 0x14, 2);
			fillIndex [0x14] = 0xEC;
			for (int i = 0x0945; i < 0x094F; i++)
				if (!IsIgnorable (i))
					AddLetterMap ((char) i, 0x14, 2);

			// Bengali
			// -Letters
			fillIndex [0x15] = 02;
			for (int i = 0x0980; i < 0x9FF; i++) {
				if (IsIgnorable (i))
					continue;
				if (i == 0x09E0)
					fillIndex [0x15] = 0x3B;
				switch (Char.GetUnicodeCategory ((char) i)) {
				case UnicodeCategory.NonSpacingMark:
				case UnicodeCategory.DecimalDigitNumber:
				case UnicodeCategory.OtherNumber:
					continue;
				}
				AddLetterMap ((char) i, 0x15, 1);
			}
			// -Signs
			fillIndex [0x1] = 0x3;
			for (int i = 0x0981; i < 0x0A00; i++)
				if (Char.GetUnicodeCategory ((char) i) ==
					UnicodeCategory.NonSpacingMark)
					AddCharMap ((char) i, 0x1, 1);

			// Gurmukhi. orderedGurmukhi is from UCA
			// FIXME: it does not look equivalent to UCA.
			fillIndex [0x16] = 04;
			fillIndex [0x1] = 3;
			for (int i = 0; i < orderedGurmukhi.Length; i++) {
				char c = orderedGurmukhi [i];
				if (IsIgnorable ((int) c))
					continue;
				if (IsIgnorableNonSpacing (c)) {
					AddLetterMap (c, 0x1, 1);
					continue;
				}
				if (c == '\u0A3C' || c == '\u0A4D' ||
					'\u0A66' <= c && c <= '\u0A71')
					continue;
				// SPECIAL CASE: U+A38 = U+A36 at primary level (why?)
				byte shift = 4;
				if (c == '\u0A36' || c == '\u0A16' || c == '\u0A17' || c == '\u0A5B' || c == '\u0A5E')
					shift = 0;
				AddLetterMap (c, 0x16, shift);
			}

			// Gujarati. orderedGujarati is from UCA
			fillIndex [0x17] = 0x4;
			// nonspacing marks
			map [0x0A4D] = new CharMapEntry (1, 0, 0x3);
			map [0x0ABD] = new CharMapEntry (1, 0, 0x3);
			map [0x0A3C] = new CharMapEntry (1, 0, 0x4);
			map [0x0A71] = new CharMapEntry (1, 0, 0x6);
			map [0x0ABC] = new CharMapEntry (1, 0, 0xB);
			map [0x0A70] = new CharMapEntry (1, 0, 0xE);
			// letters go first.
			for (int i = 0; i < orderedGujarati.Length; i++) {
				// SPECIAL CASE
				char c = orderedGujarati [i];
				if (Char.IsLetter (c)) {
					// SPECIAL CASES
					if (c == '\u0AB3' || c == '\u0A32')
						continue;
					if (c == '\u0A33') {
						AddCharMap ('\u0A32', 0x17, 0);
						AddCharMap ('\u0A33', 0x17, 4, 4);
						continue;
					}
					if (c == '\u0A8B')
						AddCharMap ('\u0AE0', 0x17, 0, 5);
					AddCharMap (c, 0x17, 4);

					if (c == '\u0AB9')
						AddCharMap ('\u0AB3', 0x17, 6);
				}
			}
			// non-letters
			byte gujaratiShift = 4;
			fillIndex [0x17] = 0xC0;
			for (int i = 0; i < orderedGujarati.Length; i++) {
				char c = orderedGujarati [i];
				if (fillIndex [0x17] == 0xCC)
					gujaratiShift = 3;
				if (!Char.IsLetter (c)) {
					// SPECIAL CASES
					if (c == '\u0A82')
						AddCharMap ('\u0A81', 0x17, 2);
					if (c == '\u0AC2')
						fillIndex [0x17]++;
					AddLetterMap (c, 0x17, gujaratiShift);
				}
			}

			// Oriya
			fillIndex [0x1] = 03;
			fillIndex [0x18] = 02;
			for (int i = 0x0B00; i < 0x0B7F; i++) {
				switch (Char.GetUnicodeCategory ((char) i)) {
				case UnicodeCategory.NonSpacingMark:
				case UnicodeCategory.DecimalDigitNumber:
					AddLetterMap ((char) i, 0x1, 1);
					continue;
				}
				AddLetterMap ((char) i, 0x18, 1);
			}

			// Tamil
			fillIndex [0x19] = 2;
			AddCharMap ('\u0BD7', 0x19, 0);
			fillIndex [0x19] = 0xA;
			// vowels
			for (int i = 0x0B82; i <= 0x0B94; i++)
				if (!IsIgnorable ((char) i))
					AddCharMap ((char) i, 0x19, 2);
			// special vowel
			fillIndex [0x19] = 0x28;
			// The array for Tamil consonants is a constant.
			// Windows have almost similar sequence to TAM from
			// tamilnet but a bit different in Grantha.
			for (int i = 0; i < orderedTamilConsonants.Length; i++)
				AddLetterMap (orderedTamilConsonants [i], 0x19, 4);
			// combining marks
			fillIndex [0x19] = 0x82;
			for (int i = 0x0BBE; i < 0x0BCD; i++)
				if (Char.GetUnicodeCategory ((char) i) ==
					UnicodeCategory.SpacingCombiningMark
					|| i == 0x0BC0)
					AddLetterMap ((char) i, 0x19, 2);

			// Telugu
			fillIndex [0x1A] = 0x4;
			for (int i = 0x0C00; i < 0x0C62; i++) {
				if (i == 0x0C55 || i == 0x0C56)
					continue; // skip
				AddCharMap ((char) i, 0x1A, 3);
				char supp = (i == 0x0C0B) ? '\u0C60':
					i == 0x0C0C ? '\u0C61' : char.MinValue;
				if (supp == char.MinValue)
					continue;
				AddCharMap (supp, 0x1A, 3);
			}

			// Kannada
			fillIndex [0x1B] = 4;
			for (int i = 0x0C80; i < 0x0CE5; i++) {
				if (i == 0x0CD5 || i == 0x0CD6)
					continue; // ignore
				if (i == 0x0CB1 || i == 0x0CB3 || i == 0x0CDE)
					continue; // shift after 0xCB9
				AddCharMap ((char) i, 0x1B, 3);
				if (i == 0x0CB9) {
					// SPECIAL CASES: but why?
					AddCharMap ('\u0CB1', 0x1B, 3); // RRA
					AddCharMap ('\u0CB3', 0x1B, 3); // LLA
					AddCharMap ('\u0CDE', 0x1B, 3); // FA
				}
				if (i == 0x0CB2)
					AddCharMap ('\u0CE1', 0x1B, 3); // vocalic LL
			}
			
			// Malayalam
			fillIndex [0x1C] = 2;
			for (int i = 0x0D02; i < 0x0D61; i++)
				// FIXME: I avoided MSCompatUnicodeTable usage
				// here (it results in recursion). So check if
				// using NonSpacingMark makes sense or not.
				if (Char.GetUnicodeCategory ((char) i) != UnicodeCategory.NonSpacingMark)
//				if (!MSCompatUnicodeTable.IsIgnorable ((char) i))
					AddCharMap ((char) i, 0x1C, 1);

			// Thai ... note that it breaks 0x1E wall after E2B!
			// Also, all Thai characters have level 2 value 3.
			fillIndex [0x1E] = 2;
			for (int i = 0xE40; i <= 0xE44; i++)
				AddCharMap ((char) i, 0x1E, 1, 3);
			for (int i = 0xE01; i < 0xE2B; i++)
				AddCharMap ((char) i, 0x1E, 6, 3);
			fillIndex [0x1F] = 5;
			for (int i = 0xE2B; i < 0xE30; i++)
				AddCharMap ((char) i, 0x1F, 6, 3);
			fillIndex [0x1F] = 0x1E;
			for (int i = 0xE30; i < 0xE3B; i++)
				AddCharMap ((char) i, 0x1F, 1, 3);
			// some Thai characters remains.
			char [] specialThai = new char [] {'\u0E45', '\u0E46',
				'\u0E4E', '\u0E4F', '\u0E5A', '\u0E5B'};
			foreach (char c in specialThai)
				AddCharMap (c, 0x1F, 1);

			// Lao
			fillIndex [0x1F] = 2;
			for (int i = 0xE80; i < 0xEDF; i++)
				if (Char.IsLetter ((char) i))
					AddCharMap ((char) i, 0x1F, 1);

			// Georgian. orderedGeorgian is from UCA DUCET.
			fillIndex [0x21] = 5;
			for (int i = 0; i < orderedGeorgian.Length; i++) {
				char c = orderedGeorgian [i];
				if (map [(int) c].Defined)
					continue;
				AddCharMap (c, 0x21, 0);
				if (c < '\u10F6')
					AddCharMap ((char) (c - 0x30), 0x21, 0, 0x12);
				fillIndex [0x21] += 5;
			}

			// Japanese Kana.
			fillIndex [0x22] = 2;
			int kanaOffset = 0x3041;
			byte [] kanaLines = new byte [] {2, 2, 2, 2, 1, 3, 1, 2, 1};

			for (int gyo = 0; gyo < 9; gyo++) {
				for (int dan = 0; dan < 5; dan++) {
					if (gyo == 7 && dan % 2 == 1) {
						// 'ya'-gyo
						fillIndex [0x22]++;
						kanaOffset -= 2; // There is no space for yi and ye.
						continue;
					}
					int cp = kanaOffset + dan * kanaLines [gyo];
					// small lines (a-gyo, ya-gyo)
					if (gyo == 0 || gyo == 7) {
						AddKanaMap (cp, 1); // small
						AddKanaMap (cp + 1, 1);
					}
					else
						AddKanaMap (cp, kanaLines [gyo]);
					fillIndex [0x22]++;

					if (cp == 0x3061) {
						// add small 'Tsu' (before normal one)
						AddKanaMap (0x3063, 1);
						kanaOffset++;
					}
				}
				fillIndex [0x22] += 3;
				kanaOffset += 5 * kanaLines [gyo];
			}

			// Wa-gyo is almost special, so I just manually add.
			AddLetterMap ((char) 0x308E, 0x22, 0);
			AddLetterMap ((char) (0x308E + 0x60), 0x22, 0);
			AddLetterMap ((char) 0x308F, 0x22, 0);
			AddLetterMap ((char) (0x308F + 0x60), 0x22, 0);
			fillIndex [0x22]++;
			AddLetterMap ((char) 0x3090, 0x22, 0);
			AddLetterMap ((char) (0x3090 + 0x60), 0x22, 0);
			fillIndex [0x22] += 2;
			// no "Wu" in Japanese.
			AddLetterMap ((char) 0x3091, 0x22, 0);
			AddLetterMap ((char) (0x3091 + 0x60), 0x22, 0);
			fillIndex [0x22]++;
			AddLetterMap ((char) 0x3092, 0x22, 0);
			AddLetterMap ((char) (0x3092 + 0x60), 0x22, 0);
			// Nn
			fillIndex [0x22] = 0x80;
			AddLetterMap ((char) 0x3093, 0x22, 0);
			AddLetterMap ((char) (0x3093 + 0x60), 0x22, 0);

			// JIS Japanese square chars.
			fillIndex [0x22] = 0x97;
			jisJapanese.Sort (JISComparer.Instance);
			foreach (JISCharacter j in jisJapanese)
				if (0x3300 <= j.CP && j.CP <= 0x3357)
					AddCharMap ((char) j.CP, 0x22, 1);
			// non-JIS Japanese square chars.
			nonJisJapanese.Sort (NonJISComparer.Instance);
			foreach (NonJISCharacter j in nonJisJapanese)
				AddCharMap ((char) j.CP, 0x22, 1);

			// Bopomofo
			fillIndex [0x23] = 0x02;
			for (int i = 0x3105; i <= 0x312C; i++)
				AddCharMap ((char) i, 0x23, 1);

			// Estrangela: ancient Syriac
			fillIndex [0x24] = 0x0B;
			// FIXME: is 0x71E really alternative form?
			ArrayList syriacAlternatives = new ArrayList (
				new int [] {0x714, 0x716, 0x71C, 0x71E, 0x724, 0x727});
			for (int i = 0x0710; i <= 0x072C; i++) {
				if (i == 0x0711) // NonSpacingMark
					continue;
				if (syriacAlternatives.Contains (i))
					continue;
				AddCharMap ((char) i, 0x24, 4);
				// FIXME: why?
				if (i == 0x721)
					fillIndex [0x24]++;
			}
			foreach (int cp in syriacAlternatives)
				map [cp] = new CharMapEntry (0x24,
					(byte) (map [cp - 1].Level1 + 2),
					0);
			// FIXME: Syriac NonSpacingMark should go here.

			// Thaana
			// FIXME: it turned out that it does not look like UCA
			fillIndex [0x24] = 0x6E;
			for (int i = 0; i < orderedThaana.Length; i++) {
				char c = orderedThaana [i];
				if (IsIgnorableNonSpacing ((int) c))
					continue;
				AddCharMap (c, 0x24, 2);
				if (c == '\u0782') // SPECIAL CASE: why?
					fillIndex [0x24] += 2;
			}
			#endregion

			// FIXME: Add more culture-specific letters (that are
			// not supported in Windows collation) here.

			// Surrogate ... they are computed.

			#region Hangul
			// Hangul.
			//
			// Unlike UCA Windows Hangul sequence mixes Jongseong
			// with Choseong sequence as well as Jungseong,
			// adjusted to have the same primary weight for the
			// same base character. So it is impossible to compute
			// those sort keys.
			//
			// Here I introduce an ordered sequence of mixed
			// 'commands' and 'characters' that is similar to
			// LDML text:
			//	- ',' increases primary weight.
			//	- [A B] means a range, increasing index
			//	- {A B} means a range, without increasing index
			//	- '=' is no operation (it means the characters 
			//	  of both sides have the same weight).
			//	- '>' inserts a Hangul Syllable block that 
			//	  contains 0x251 characters.
			//	- '<' decreases the index
			//	- '0'-'9' means skip count
			//	- whitespaces are ignored
			//

			string hangulSequence =
			+ "\u1100=\u11A8 > \u1101=\u11A9 >"
			+ "\u11C3, \u11AA, \u11C4, \u1102=\u11AB >"
			+ "<{\u1113 \u1116}, \u3165,"
				+ "\u11C5, \u11C6=\u3166,, \u11C7, \u11C8,"
				+ "\u11AC, \u11C9, \u11AD, \u1103=\u11AE  >"
			+ "<\u1117, \u11CA, \u1104, \u11CB > \u1105 >"
			+ "<{\u1118 \u111B}, \u11B0, [\u11CC \u11D0], \u11B1,"
				+ "[\u11D1 \u11D2], \u11B2,"
				+ "[\u11D3 \u11D5], \u11B3,"
				+ "[\u11D6 \u11D7], \u11B4, \u11B5,"
				+ "\u11B6=\u11D8, \u3140,, \u11D9, \u1106=\u11B7 >"
			+ "<{\u111C \u111D}, [\u11DA \u11E2], \u1107=\u11B8 >"
			+ "<{\u111E \u1120}, \u3172,, \u3173, \u11E3, \u1108 >"
			+ "<{\u1121 \u112C}, \u3144 \u11B9, \u3174, \u3175,,,, "
				+ "\u3176,, \u3177, [\u11E4 \u11E6] \u3178,"
				+ "\u3179, \u1109=\u11BA,,, \u3214=\u3274 <>"
			+ "<{\u112D \u1133}, \u11E7 \u317A, \u317B, \u317C "
				+ "[\u11E8 \u11E9],, \u11EA \u317D,, \u110A=\u11BB,,, >"
			+ "<{\u1134 \u1140}, \u317E,,,,,,, \u11EB,"
				+ "\u110B=\u11BC, [\u1161 \u11A2], \u1160 >"
			+ "<{\u1141 \u114C}, \u11EE, \u11EC, \u11ED,,,,, "
				+ "\u11F1,, \u11F2,,,"
				+ "\u11EF,,, \u11F0, \u110C=\u11BD,, >"
			+ "<\u114D, \u110D,,  >"
			+ "<{\u114E \u1151},, \u110E=\u11BE,,  >"
			+ "<{\u1152 \u1155},,, \u110F=\u11BF >"
			+ "\u1110=\u11C0 > \u1111=\u11C1 >"
			+ "<\u1156=\u1157, \u11F3, \u11F4, \u1112=\u11C2 >"
			+ "<\u1158=\u1159=\u115F, \u3185, \u11F9,"
				+ "[\u11F5 \u11F8]"
			;

			byte hangulCat = 0x52;
			fillIndex [hangulCat] = 0x2;

			int syllableBlock = 0;
			for (int n = 0; n < hangulSequence.Length; n++) {
				char c = hangulSequence [n];
				int start, end;
				if (Char.IsWhiteSpace (c))
					continue;
				switch (c) {
				case '=':
					break; // NOP
				case ',':
					IncrementSequentialIndex (ref hangulCat);
					break;
				case '<':
					if (fillIndex [hangulCat] == 2)
						throw new Exception ("FIXME: handle it correctly (yes it is hacky, it is really unfortunate).");
					fillIndex [hangulCat]--;
					break;
				case '>':
					IncrementSequentialIndex (ref hangulCat);
					for (int l = 0; l < 0x15; l++)
						for (int v = 0; v < 0x1C; v++) {
							AddCharMap (
								(char) (0xAC00 + syllableBlock * 0x1C * 0x15 + l * 0x1C + v), hangulCat, 0);
							IncrementSequentialIndex (ref hangulCat);
						}
					syllableBlock++;
					break;
				case '[':
					start = hangulSequence [n + 1];
					end = hangulSequence [n + 3];
					for (int i = start; i <= end; i++) {
						AddCharMap ((char) i, hangulCat, 0);
						if (end > i)
							IncrementSequentialIndex (ref hangulCat);
					}
					n += 4; // consumes 5 characters for this operation
					break;
				case '{':
					start = hangulSequence [n + 1];
					end = hangulSequence [n + 3];
					for (int i = start; i <= end; i++)
						AddCharMap ((char) i, hangulCat, 0);
					n += 4; // consumes 5 characters for this operation
					break;
				default:
					AddCharMap (c, hangulCat, 0);
					break;
				}
			}

			// Some Jamo NFKD.
			for (int i = 0x3200; i < 0x3300; i++) {
				if (IsIgnorable (i) || map [i].Defined)
					continue;
				int ch = 0;
				// w/ bracket
				if (decompLength [i] == 4 &&
					decompValues [decompIndex [i]] == '(')
					ch = decompIndex [i] + 1;
				// circled
				else if (decompLength [i] == 2 &&
					decompValues [decompIndex [i] + 1] == '\u1161')
					ch = decompIndex [i];
				else if (decompLength [i] == 1)
					ch = decompIndex [i];
				else
					continue;
				ch = decompValues [ch];
				if (ch < 0x1100 || 0x1200 < ch &&
					ch < 0xAC00 || 0xD800 < ch)
					continue;

				// SPECIAL CASE ?
				int offset = i < 0x3260 ? 1 : 0;
				if (0x326E <= i && i <= 0x3273)
					offset = 1;

				map [i] = new CharMapEntry (map [ch].Category,
					(byte) (map [ch].Level1 + offset),
					map [ch].Level2);
//					Console.Error.WriteLine ("Jamo {0:X04} -> {1:X04}", i, decompValues [decompIndex [i] + 1]);
			}


			#endregion

			// Letterlike characters and CJK compatibility square
			sortableCharNames.Sort (StringDictionaryValueComparer.Instance);
			int [] counts = new int ['Z' - 'A' + 1];
			char [] namedChars = new char [sortableCharNames.Count];
			int nCharNames = 0;
			foreach (DictionaryEntry de in sortableCharNames) {
				counts [((string) de.Value) [0] - 'A']++;
				namedChars [nCharNames++] = (char) ((int) de.Key);
			}
			nCharNames = 0; // reset
			for (int a = 0; a < counts.Length; a++) {
				fillIndex [0xE] = (byte) (alphaWeights [a + 1] - counts [a]);
				for (int i = 0; i < counts [a]; i++)
//Console.Error.WriteLine ("---- {0:X04} : {1:x02} / {2} {3}", (int) namedChars [nCharNames], fillIndex [0xE], ((DictionaryEntry) sortableCharNames [nCharNames]).Value, Char.GetUnicodeCategory (namedChars [nCharNames]));
					AddCharMap (namedChars [nCharNames++], 0xE, 1);
			}

			// CJK unified ideograph.
			byte cjkCat = 0x9E;
			fillIndex [cjkCat] = 0x2;
			for (int cp = 0x4E00; cp <= 0x9FBB; cp++)
				if (!IsIgnorable (cp))
					AddCharMapGroupCJK ((char) cp, ref cjkCat);
			// CJK Extensions goes here.
			// LAMESPEC: With this Windows style CJK layout, it is
			// impossible to add more CJK ideograph i.e. 0x9FA6-
			// 0x9FBB can never be added w/o breaking compat.
			for (int cp = 0xF900; cp <= 0xFA2D; cp++)
				if (!IsIgnorable (cp))
					AddCharMapGroupCJK ((char) cp, ref cjkCat);

			// PrivateUse ... computed.
			// remaining Surrogate ... computed.

			#region Special "biggest" area (FF FF)
			fillIndex [0xFF] = 0xFF;
			char [] specialBiggest = new char [] {
				'\u3005', '\u3031', '\u3032', '\u309D',
				'\u309E', '\u30FC', '\u30FD', '\u30FE',
				'\uFE7C', '\uFE7D', '\uFF70'};
			foreach (char c in specialBiggest)
				AddCharMap (c, 0xFF, 0);
			#endregion

			#region 07 - ASCII non-alphanumeric + 3001, 3002 // 07
			// non-alphanumeric ASCII except for: + - < = > '
			for (int i = 0x21; i < 0x7F; i++) {
				if (Char.IsLetterOrDigit ((char) i)
					|| "+-<=>'".IndexOf ((char) i) >= 0)
					continue; // they are not added here.
					AddCharMapGroup2 ((char) i, 0x7, 1, 0);
				// Insert 3001 after ',' and 3002 after '.'
				if (i == 0x2C)
					AddCharMapGroup2 ('\u3001', 0x7, 1, 0);
				else if (i == 0x2E) {
					fillIndex [0x7]--;
					AddCharMapGroup2 ('\u3002', 0x7, 1, 0);
				}
				else if (i == 0x3A)
					AddCharMap ('\uFE30', 0x7, 1, 0);
			}
			#endregion

			#region 07 - Punctuations and something else
			for (int i = 0xA0; i < char.MaxValue; i++) {
				if (IsIgnorable (i))
					continue;

				// FIXME: actually this reset should not be done
				// but here I put for easy goal.
				if (i == 0x0700)
					fillIndex [0x7] = 0xE2;

				// SPECIAL CASES:
				switch (i) {
				case 0xAB: // 08
				case 0xB7: // 0A
				case 0xBB: // 08
				case 0x2329: // 09
				case 0x232A: // 09
					continue;
				}

				switch (Char.GetUnicodeCategory ((char) i)) {
				case UnicodeCategory.OtherPunctuation:
				case UnicodeCategory.ClosePunctuation:
				case UnicodeCategory.OpenPunctuation:
				case UnicodeCategory.InitialQuotePunctuation:
				case UnicodeCategory.FinalQuotePunctuation:
				case UnicodeCategory.ModifierSymbol:
					// SPECIAL CASES: // 0xA
					if (0x2020 <= i && i <= 0x2042)
						continue;
					AddCharMapGroup ((char) i, 0x7, 1, 0);
					break;
				default:
					if (i == 0xA6) // SPECIAL CASE. FIXME: why?
						goto case UnicodeCategory.OtherPunctuation;
					break;
				}
			}
			// Control pictures
			for (int i = 0x2400; i <= 0x2421; i++)
				AddCharMap ((char) i, 0x7, 1, 0);
			#endregion

			// FIXME: for 07 xx we need more love.

			// Characters w/ diacritical marks (NFKD)
			for (int i = 0; i <= char.MaxValue; i++) {
				if (map [i].Defined || IsIgnorable (i))
					continue;
				if (decompIndex [i] == 0)
					continue;

				int start = decompIndex [i];
				int primaryChar = decompValues [start];
				int secondary = 0;
				bool skip = false;
				int length = decompLength [i];
				// special processing for parenthesized ones.
				if (length == 3 &&
					decompValues [start] == '(' &&
					decompValues [start + 2] == ')') {
					primaryChar = decompValues [start + 1];
					length = 1;
				}

				if (map [primaryChar].Level1 == 0)
					continue;

				for (int l = 1; l < length; l++) {
					int c = decompValues [start + l];
					if (map [c].Level1 != 0)
						skip = true;
					secondary += diacritical [c];
				}
				if (skip)
					continue;
				map [i] = new CharMapEntry (
					map [primaryChar].Category,
					map [primaryChar].Level1,
					(byte) secondary);
				
			}

			// category 08 - symbols
			fillIndex [0x8] = 2;
			// Here Windows mapping is not straightforward. It is
			// not based on computation but seems manual sorting.
			AddCharMapGroup ('+', 0x8, 1, 0); // plus
			AddCharMapGroup ('\u2212', 0x8, 1, 0); // minus
			AddCharMapGroup ('\u229D', 0x8, 1, 0); // minus
			AddCharMapGroup ('\u2297', 0x8, 1, 0); // mul
			AddCharMapGroup ('\u2044', 0x8, 1, 0); // div
			AddCharMapGroup ('\u2215', 0x8, 1, 0); // div
			AddCharMapGroup ('\u2217', 0x8, 1, 0); // mul
			AddCharMapGroup ('\u2218', 0x8, 1, 0); // ring
			AddCharMapGroup ('\u2219', 0x8, 1, 0); // bullet
			AddCharMapGroup ('\u2213', 0x8, 1, 0); // minus-or-plus
			AddCharMapGroup ('\u003C', 0x8, 1, 0); // <
			AddCharMapGroup ('\u227A', 0x8, 1, 0); // precedes relation
			AddCharMapGroup ('\u22B0', 0x8, 1, 0); // precedes under relation

			for (int cp = 0; cp < 0x2300; cp++) {
				if (cp == 0x200)
					cp = 0x2200; // skip to 2200
				if (cp == 0xAC) // SPECIAL CASE: skip
					continue;
				if (!map [cp].Defined &&
//					Char.GetUnicodeCategory ((char) cp) ==
//					UnicodeCategory.MathSymbol)
					Char.IsSymbol ((char) cp))
					AddCharMapGroup ((char) cp, 0x8, 1, 0);
				// SPECIAL CASES: no idea why Windows sorts as such
				switch (cp) {
				case 0x3E:
					AddCharMap ('\u227B', 0x8, 1, 0);
					AddCharMap ('\u22B1', 0x8, 1, 0);
					break;
				case 0xB1:
					AddCharMapGroup ('\u00AB', 0x8, 1, 0);
					AddCharMapGroup ('\u226A', 0x8, 1, 0);
					AddCharMapGroup ('\u00BB', 0x8, 1, 0);
					AddCharMapGroup ('\u226B', 0x8, 1, 0);
					break;
				case 0xF7:
					AddCharMap ('\u01C0', 0x8, 1, 0);
					AddCharMap ('\u01C1', 0x8, 1, 0);
					AddCharMap ('\u01C2', 0x8, 1, 0);
					break;
				}
			}

			#region Level2 adjustment
			// Arabic Hamzah
			diacritical [0x624] = 0x5;
			diacritical [0x626] = 0x7;
			diacritical [0x622] = 0x9;
			diacritical [0x623] = 0xA;
			diacritical [0x625] = 0xB;
			diacritical [0x649] = 0x5; // 'alif maqs.uurah
			diacritical [0x64A] = 0x7; // Yaa'

			for (int i = 0; i < char.MaxValue; i++) {
				byte mod = 0;
				byte cat = map [i].Category;
				switch (cat) {
				case 0xE: // Latin diacritics
				case 0x22: // Japanese: circled characters
					mod = diacritical [i];
					break;
				case 0x13: // Arabic
					if (diacritical [i] == 0)
						mod = 0x8; // default for arabic
					break;
				}
				if (0x52 <= cat && cat <= 0x7F) // Hangul
					mod = diacritical [i];
				if (mod > 0)
					map [i] = new CharMapEntry (
						cat, map [i].Level1, mod);
			}
			#endregion

			// FIXME: this is hack but those NonSpacingMark 
			// characters and still undefined are likely to
			// be nonspacing.
			for (int i = 0; i < char.MaxValue; i++)
				if (!map [i].Defined &&
					!IsIgnorable (i) &&
					Char.GetUnicodeCategory ((char) i) ==
					UnicodeCategory.NonSpacingMark)
					AddCharMap ((char) i, 1, 1);

			// FIXME: this is hack but those Symbol characters
			// are likely to fall into 0xA category.
			for (int i = 0; i < char.MaxValue; i++)
				if (!map [i].Defined &&
					!IsIgnorable (i) &&
					Char.IsSymbol ((char) i))
					AddCharMap ((char) i, 0xA, 1);
		}

		private void IncrementSequentialIndex (ref byte hangulCat)
		{
			fillIndex [hangulCat]++;
			if (fillIndex [hangulCat] == 0) { // overflown
				hangulCat++;
				fillIndex [hangulCat] = 0x2;
			}
		}

		// Reset fillIndex to fixed value and call AddLetterMap().
		private void AddAlphaMap (char c, byte category, byte alphaWeight)
		{
			fillIndex [category] = alphaWeight;
			AddLetterMap (c, category, 0);

			ArrayList al = latinMap [c] as ArrayList;
			if (al == null)
				return;

			foreach (int cp in al)
				AddLetterMap ((char) cp, category, 0);
		}

		private void AddKanaMap (int i, byte voices)
		{
			for (byte b = 0; b < voices; b++) {
				char c = (char) (i + b);
				byte arg = (byte) (b > 0 ? b + 2 : 0);
				// Hiragana
				AddLetterMapCore (c, 0x22, 0, arg);
				// Katakana
				AddLetterMapCore ((char) (c + 0x60), 0x22, 0, arg);
			}
		}

		private void AddLetterMap (char c, byte category, byte updateCount)
		{
			AddLetterMapCore (c, category, updateCount, 0);
		}

		private void AddLetterMapCore (char c, byte category, byte updateCount, byte level2)
		{
			char c2;
			// <small> updates index
			c2 = ToSmallForm (c);
			if (c2 != c)
				AddCharMapGroup (c2, category, updateCount, level2);
			c2 = Char.ToLower (c, CultureInfo.InvariantCulture);
			if (c2 != c && !map [(int) c2].Defined)
				AddLetterMapCore (c2, category, 0, level2);
			bool doUpdate = true;
			if (IsIgnorable ((int) c) || map [(int) c].Defined)
				doUpdate = false;
			else
				AddCharMapGroup (c, category, 0, level2);
			if (doUpdate)
				fillIndex [category] += updateCount;
		}

		private bool AddCharMap (char c, byte category, byte increment)
		{
			return AddCharMap (c, category, increment, 0);
		}
		
		private bool AddCharMap (char c, byte category, byte increment, byte alt)
		{
			if (IsIgnorable ((int) c) || map [(int) c].Defined)
				return false; // do nothing
			map [(int) c] = new CharMapEntry (category,
				category == 1 ? alt : fillIndex [category],
				category == 1 ? fillIndex [category] : alt);
			fillIndex [category] += increment;
			return true;
		}

		private void AddCharMapGroupTail (char c, byte category, byte updateCount)
		{
			char c2 = ToSmallFormTail (c);
			if (c2 != c)
				AddCharMap (c2, category, updateCount, 0);
			// itself
			AddCharMap (c, category, updateCount, 0);
			// <full>
			c2 = ToFullWidthTail (c);
			if (c2 != c)
				AddCharMapGroupTail (c2, category, updateCount);
		}

		//
		// Adds characters to table in the order below 
		// (+ increases weight):
		//	(<small> +)
		//	itself
		//	<fraction>
		//	<full> | <super> | <sub>
		//	<circle> | <wide> (| <narrow>)
		//	+
		//	(vertical +)
		//
		// level2 is fixed (does not increase).
		int [] sameWeightItems = new int [] {
			DecompositionFraction,
			DecompositionFull,
			DecompositionSuper,
			DecompositionSub,
			DecompositionCircle,
			DecompositionWide,
			DecompositionNarrow,
			};
		private void AddCharMapGroup (char c, byte category, byte updateCount, byte level2)
		{
			if (map [(int) c].Defined)
				return;

			char small = char.MinValue;
			char vertical = char.MinValue;
			Hashtable nfkd = (Hashtable) nfkdMap [(int) c];
			if (nfkd != null) {
				object smv = nfkd [(byte) DecompositionSmall];
				if (smv != null)
					small = (char) ((int) smv);
				object vv = nfkd [(byte) DecompositionVertical];
				if (vv != null)
					vertical = (char) ((int) vv);
			}

			// <small> updates index
			if (small != char.MinValue)
				AddCharMap (small, category, updateCount);

			// itself
			AddCharMap (c, category, 0, level2);

			if (nfkd != null) {
				foreach (int weight in sameWeightItems) {
					object wv = nfkd [(byte) weight];
					if (wv != null)
						AddCharMap ((char) ((int) wv), category, 0, level2);
				}
			}

			// update index here.
			fillIndex [category] += updateCount;

			if (vertical != char.MinValue)
				AddCharMap (vertical, category, updateCount, level2);
		}

		private void AddCharMapCJK (char c, ref byte category)
		{
			AddCharMap (c, category, 0, 0);
			IncrementSequentialIndex (ref category);

			// Special. I wonder why but Windows skips 9E F9.
			if (category == 0x9E && fillIndex [category] == 0xF9)
				IncrementSequentialIndex (ref category);
		}

		private void AddCharMapGroupCJK (char c, ref byte category)
		{
			AddCharMapCJK (c, ref category);

			// LAMESPEC: see below.
			if (c == '\u5B78') {
				AddCharMapCJK ('\u32AB', ref category);
				AddCharMapCJK ('\u323B', ref category);
			}
			if (c == '\u52DE') {
				AddCharMapCJK ('\u3298', ref category);
				AddCharMapCJK ('\u3238', ref category);
			}
			if (c == '\u5BEB')
				AddCharMapCJK ('\u32A2', ref category);
			if (c == '\u91AB')
				// Especially this mapping order totally does
				// not make sense to me.
				AddCharMapCJK ('\u32A9', ref category);

			Hashtable nfkd = (Hashtable) nfkdMap [(int) c];
			if (nfkd == null)
				return;
			for (byte weight = 0; weight <= 0x12; weight++) {
				object wv = nfkd [weight];
				if (wv == null)
					continue;
				int w = (int) wv;

				// Special: they are ignored in this area.
				// FIXME: check if it is sane
				if (0xF900 <= w && w <= 0xFAD9)
					continue;
				// LAMESPEC: on Windows some of CJK characters
				// in 3200-32B0 are incorrectly mapped. They
				// mix Chinise and Japanese Kanji when
				// ordering those characters.
				switch (w) {
				case 0x32A2: case 0x3298: case 0x3238:
				case 0x32A9: case 0x323B: case 0x32AB:
					continue;
				}

				AddCharMapCJK ((char) w, ref category);
			}
		}

		// For now it is only for 0x7 category.
		private void AddCharMapGroup2 (char c, byte category, byte updateCount, byte level2)
		{
			char small = char.MinValue;
			char vertical = char.MinValue;
			Hashtable nfkd = (Hashtable) nfkdMap [(int) c];
			if (nfkd != null) {
				object smv = nfkd [(byte) DecompositionSmall];
				if (smv != null)
					small = (char) ((int) smv);
				object vv = nfkd [(byte) DecompositionVertical];
				if (vv != null)
					vertical = (char) ((int) vv);
			}

			// <small> updates index
			if (small != char.MinValue)
				// SPECIAL CASE excluded (FIXME: why?)
				if (small != '\u2024')
					AddCharMap (small, category, updateCount);

			// itself
			AddCharMap (c, category, updateCount, level2);

			// Since nfkdMap is problematic to have two or more
			// NFKD to an identical character, here I iterate all.
			for (int c2 = 0; c2 < char.MaxValue; c2++) {
				if (decompLength [c2] == 1 &&
					(int) (decompValues [decompIndex [c2]]) == (int) c) {
					switch (decompType [c2]) {
					case DecompositionCompat:
						AddCharMap ((char) c2, category, updateCount, level2);
						break;
					}
				}
			}

			if (vertical != char.MinValue)
				// SPECIAL CASE excluded (FIXME: why?)
				if (vertical != '\uFE33' && vertical != '\uFE34')
					AddCharMap (vertical, category, updateCount, level2);
		}

		private void AddArabicCharMap (char c)
		{
			byte category = 6;
			byte updateCount = 1;
			byte level2 = 0;

			// itself
			AddCharMap (c, category, 0, level2);

			// Since nfkdMap is problematic to have two or more
			// NFKD to an identical character, here I iterate all.
			for (int c2 = 0; c2 < char.MaxValue; c2++) {
				if (decompLength [c2] == 0)
					continue;
				int idx = decompIndex [c2] + decompLength [c2] - 1;
				if ((int) (decompValues [idx]) == (int) c)
					AddCharMap ((char) c2, category,
						0, level2);
			}
			fillIndex [category] += updateCount;
		}

		char ToFullWidth (char c)
		{
			return ToDecomposed (c, DecompositionFull, false);
		}

		char ToFullWidthTail (char c)
		{
			return ToDecomposed (c, DecompositionFull, true);
		}

		char ToSmallForm (char c)
		{
			return ToDecomposed (c, DecompositionSmall, false);
		}

		char ToSmallFormTail (char c)
		{
			return ToDecomposed (c, DecompositionSmall, true);
		}

		char ToDecomposed (char c, byte d, bool tail)
		{
			if (decompType [(int) c] != d)
				return c;
			int idx = decompIndex [(int) c];
			if (tail)
				idx += decompLength [(int) c] - 1;
			return (char) decompValues [idx];
		}

		bool ExistsJIS (int cp)
		{
			foreach (JISCharacter j in jisJapanese)
				if (j.CP == cp)
					return true;
			return false;
		}

		#endregion

		#region Level 3 properties (Case/Width)

		private byte ComputeLevel3Weight (char c)
		{
			byte b = ComputeLevel3WeightRaw (c);
			return b > 0 ? (byte) (b + 2) : b;
		}

		private byte ComputeLevel3WeightRaw (char c) // add 2 for sortkey value
		{
			// CJK compat
			if ('\u3192' <= c && c <= '\u319F')
				return 0;
			// Korean
			if ('\u11A8' <= c && c <= '\u11F9')
				return 2;
			if ('\uFFA0' <= c && c <= '\uFFDC')
				return 4;
			if ('\u3130' <= c && c <= '\u3164')
				return 5;
			if ('\u3165' <= c && c <= '\u318E')
				return 4;
			// numbers
			if ('\u2776' <= c && c <= '\u277F')
				return 4;
			if ('\u2780' <= c && c <= '\u2789')
				return 8;
			if ('\u2776' <= c && c <= '\u2793')
				return 0xC;
			if ('\u2160' <= c && c <= '\u216F')
				return 0x18;
			if ('\u2181' <= c && c <= '\u2182')
				return 0x18;
			// Arabic
			if ('\u2135' <= c && c <= '\u2138')
				return 4;
			if ('\uFE80' <= c && c < '\uFE8E') {
				// 2(Isolated)/8(Final)/0x18(Medial)
				switch (decompType [(int) c]) {
				case DecompositionIsolated:
					return 2;
				case DecompositionFinal:
					return 8;
				case DecompositionMedial:
					return 0x18;
				}
			}

			// actually I dunno the reason why they have weights.
			switch (c) {
			case '\u01BC':
				return 0x10;
			case '\u06A9':
				return 0x20;
			case '\u06AA':
				return 0x28;
			}

			byte ret = 0;
			switch (c) {
			case '\u03C2':
			case '\u2104':
			case '\u212B':
				ret |= 8;
				break;
			case '\uFE42':
				ret |= 0xC;
				break;
			}

			// misc
			switch (decompType [(int) c]) {
			case DecompositionWide: // <wide>
			case DecompositionSub: // <sub>
			case DecompositionSuper: // <super>
				ret |= decompType [(int) c];
				break;
			}
			if (isSmallCapital [(int) c]) // grep "SMALL CAPITAL"
				ret |= 8;
			if (isUppercase [(int) c]) // DerivedCoreProperties
				ret |= 0x10;

			return ret;
		}

		#endregion

		#region IsIgnorable
/*
		static bool IsIgnorable (int i)
		{
			if (unicodeAge [i] >= 3.1)
				return true;
			switch (char.GetUnicodeCategory ((char) i)) {
			case UnicodeCategory.OtherNotAssigned:
			case UnicodeCategory.Format:
				return true;
			}
			return false;
		}
*/

		// FIXME: In the future use DerivedAge.txt to examine character
		// versions and set those ones that have higher version than
		// 1.0 as ignorable.
		static bool IsIgnorable (int i)
		{
			switch (i) {
			case 0:
			// I guess, those characters are added between
			// Unicode 1.0 (LCMapString) and Unicode 3.1
			// (UnicodeCategory), so they used to be 
			// something like OtherNotAssigned as of Unicode 1.1.
			case 0x2df: case 0x387:
			case 0x3d7: case 0x3d8: case 0x3d9:
			case 0x3f3: case 0x3f4: case 0x3f5: case 0x3f6:
			case 0x400: case 0x40d: case 0x450: case 0x45d:
			case 0x587: case 0x58a: case 0x5c4: case 0x640:
			case 0x653: case 0x654: case 0x655: case 0x66d:
			case 0xb56:
			case 0x1e9b: case 0x202f: case 0x20ad:
			case 0x20ae: case 0x20af:
			case 0x20e2: case 0x20e3:
			case 0x2139: case 0x213a: case 0x2183:
			case 0x2425: case 0x2426: case 0x2619:
			case 0x2670: case 0x2671: case 0x3007:
			case 0x3190: case 0x3191:
			case 0xfffc: case 0xfffd:
				return true;
			// exceptional characters filtered by the 
			// following conditions. Originally those exceptional
			// ranges are incorrect (they should not be ignored)
			// and most of those characters are unfortunately in
			// those ranges.
			case 0x4d8: case 0x4d9:
			case 0x4e8: case 0x4e9:
			case 0x70F:
			case 0x3036: case 0x303f:
			case 0x337b: case 0xfb1e:
				return false;
			}

			if (
				// The whole Sinhala characters.
				0x0D82 <= i && i <= 0x0DF4
				// The whole Tibetan characters.
				|| 0x0F00 <= i && i <= 0x0FD1
				// The whole Myanmar characters.
				|| 0x1000 <= i && i <= 0x1059
				// The whole Etiopic, Cherokee, 
				// Canadian Syllablic, Ogham, Runic,
				// Tagalog, Hanunoo, Philippine,
				// Buhid, Tagbanwa, Khmer and Mongorian
				// characters.
				|| 0x1200 <= i && i <= 0x1DFF
				// Greek extension characters.
				|| 0x1F00 <= i && i <= 0x1FFF
				// The whole Braille characters.
				|| 0x2800 <= i && i <= 0x28FF
				// CJK radical characters.
				|| 0x2E80 <= i && i <= 0x2EF3
				// Kangxi radical characters.
				|| 0x2F00 <= i && i <= 0x2FD5
				// Ideographic description characters.
				|| 0x2FF0 <= i && i <= 0x2FFB
				// Bopomofo letter and final
				|| 0x31A0 <= i && i <= 0x31B7
				// White square with quadrant characters.
				|| 0x25F0 <= i && i <= 0x25F7
				// Ideographic telegraph symbols.
				|| 0x32C0 <= i && i <= 0x32CB
				|| 0x3358 <= i && i <= 0x3370
				|| 0x33E0 <= i && i <= 0x33FF
				// The whole YI characters.
				|| 0xA000 <= i && i <= 0xA48C
				|| 0xA490 <= i && i <= 0xA4C6
				// American small ligatures
				|| 0xFB13 <= i && i <= 0xFB17
				// hebrew, arabic, variation selector.
				|| 0xFB1D <= i && i <= 0xFE2F
				// Arabic ligatures.
				|| 0xFEF5 <= i && i <= 0xFEFC
				// FIXME: why are they excluded?
				|| 0x01F6 <= i && i <= 0x01F9
				|| 0x0218 <= i && i <= 0x0233
				|| 0x02A9 <= i && i <= 0x02AD
				|| 0x02EA <= i && i <= 0x02EE
				|| 0x0349 <= i && i <= 0x036F
				|| 0x0488 <= i && i <= 0x048F
				|| 0x04D0 <= i && i <= 0x04FF
				|| 0x0500 <= i && i <= 0x050F // actually it matters only for 2.0
				|| 0x06D6 <= i && i <= 0x06ED
				|| 0x06FA <= i && i <= 0x06FE
				|| 0x2048 <= i && i <= 0x204D
				|| 0x20e4 <= i && i <= 0x20ea
				|| 0x213C <= i && i <= 0x214B
				|| 0x21EB <= i && i <= 0x21FF
				|| 0x22F2 <= i && i <= 0x22FF
				|| 0x237B <= i && i <= 0x239A
				|| 0x239B <= i && i <= 0x23CF
				|| 0x24EB <= i && i <= 0x24FF
				|| 0x2596 <= i && i <= 0x259F
				|| 0x25F8 <= i && i <= 0x25FF
				|| 0x2672 <= i && i <= 0x2689
				|| 0x2768 <= i && i <= 0x2775
				|| 0x27d0 <= i && i <= 0x27ff
				|| 0x2900 <= i && i <= 0x2aff
				|| 0x3033 <= i && i <= 0x303F
				|| 0x31F0 <= i && i <= 0x31FF
				|| 0x3250 <= i && i <= 0x325F
				|| 0x32B1 <= i && i <= 0x32BF
				|| 0x3371 <= i && i <= 0x337B
				|| 0xFA30 <= i && i <= 0xFA6A
			)
				return true;

			UnicodeCategory uc = Char.GetUnicodeCategory ((char) i);
			switch (uc) {
			case UnicodeCategory.PrivateUse:
			case UnicodeCategory.Surrogate:
				return false;
			// ignored by nature
			case UnicodeCategory.Format:
			case UnicodeCategory.OtherNotAssigned:
				return true;
			default:
				return false;
			}
		}

		// To check IsIgnorable sanity, try the driver below under MS.NET.

		/*
		public static void Main ()
		{
			for (int i = 0; i <= char.MaxValue; i++)
				Dump (i, IsIgnorable (i));
		}

		static void Dump (int i, bool ignore)
		{
			switch (Char.GetUnicodeCategory ((char) i)) {
			case UnicodeCategory.PrivateUse:
			case UnicodeCategory.Surrogate:
				return; // check nothing
			}

			string s1 = "";
			string s2 = new string ((char) i, 10);
			int ret = CultureInfo.InvariantCulture.CompareInfo.Compare (s1, s2, CompareOptions.IgnoreCase);
			if ((ret == 0) == ignore)
				return;
			Console.WriteLine ("{0} : {1:x} {2}", ignore ? "o" : "x", i, Char.GetUnicodeCategory ((char) i));
		}
		*/
		#endregion // IsIgnorable

		#region IsIgnorableSymbol
		static bool IsIgnorableSymbol (int i)
		{
			if (IsIgnorable (i))
				return true;

			switch (i) {
			// *Letter
			case 0x00b5: case 0x01C0: case 0x01C1:
			case 0x01C2: case 0x01C3: case 0x01F6:
			case 0x01F7: case 0x01F8: case 0x01F9:
			case 0x02D0: case 0x02EE: case 0x037A:
			case 0x03D7: case 0x03F3:
			case 0x0400: case 0x040d:
			case 0x0450: case 0x045d:
			case 0x048C: case 0x048D:
			case 0x048E: case 0x048F:
			case 0x0587: case 0x0640: case 0x06E5:
			case 0x06E6: case 0x06FA: case 0x06FB:
			case 0x06FC: case 0x093D: case 0x0950:
			case 0x1E9B: case 0x2139: case 0x3006:
			case 0x3033: case 0x3034: case 0x3035:
			case 0xFE7E: case 0xFE7F:
			// OtherNumber
			case 0x16EE: case 0x16EF: case 0x16F0:
			// LetterNumber
			case 0x2183: // ROMAN NUMERAL REVERSED ONE HUNDRED
			case 0x3007: // IDEOGRAPHIC NUMBER ZERO
			case 0x3038: // HANGZHOU NUMERAL TEN
			case 0x3039: // HANGZHOU NUMERAL TWENTY
			case 0x303a: // HANGZHOU NUMERAL THIRTY
			// OtherSymbol
			case 0x2117:
			case 0x327F:
				return true;
			// ModifierSymbol
			case 0x02B9: case 0x02BA: case 0x02C2:
			case 0x02C3: case 0x02C4: case 0x02C5:
			case 0x02C8: case 0x02CC: case 0x02CD:
			case 0x02CE: case 0x02CF: case 0x02D2:
			case 0x02D3: case 0x02D4: case 0x02D5:
			case 0x02D6: case 0x02D7: case 0x02DE:
			case 0x02E5: case 0x02E6: case 0x02E7:
			case 0x02E8: case 0x02E9:
			case 0x309B: case 0x309C:
			// OtherPunctuation
			case 0x055A: // American Apos
			case 0x05C0: // Hebrew Punct
			case 0x0E4F: // Thai FONGMAN
			case 0x0E5A: // Thai ANGKHANKHU
			case 0x0E5B: // Thai KHOMUT
			// CurencySymbol
			case 0x09F2: // Bengali Rupee Mark
			case 0x09F3: // Bengali Rupee Sign
			// MathSymbol
			case 0x221e: // INF.
			// OtherSymbol
			case 0x0482:
			case 0x09FA:
			case 0x0B70:
				return false;
			}

			// *Letter
			if (0xFE70 <= i && i < 0xFE7C // ARABIC LIGATURES B
#if NET_2_0
				|| 0x0501 <= i && i <= 0x0510 // CYRILLIC KOMI
				|| 0xFA30 <= i && i < 0xFA70 // CJK COMPAT
#endif
			)
				return true;

			UnicodeCategory uc = Char.GetUnicodeCategory ((char) i);
			switch (uc) {
			case UnicodeCategory.Surrogate:
				return false; // inconsistent

			case UnicodeCategory.SpacingCombiningMark:
			case UnicodeCategory.EnclosingMark:
			case UnicodeCategory.NonSpacingMark:
			case UnicodeCategory.PrivateUse:
				// NonSpacingMark
				if (0x064B <= i && i <= 0x0652) // Arabic
					return true;
				return false;

			case UnicodeCategory.Format:
			case UnicodeCategory.OtherNotAssigned:
				return true;

			default:
				bool use = false;
				// OtherSymbols
				if (
					// latin in a circle
					0x249A <= i && i <= 0x24E9
					|| 0x2100 <= i && i <= 0x2132
					// Japanese
					|| 0x3196 <= i && i <= 0x31A0
					// Korean
					|| 0x3200 <= i && i <= 0x321C
					// Chinese/Japanese
					|| 0x322A <= i && i <= 0x3243
					// CJK
					|| 0x3260 <= i && i <= 0x32B0
					|| 0x32D0 <= i && i <= 0x3357
					|| 0x337B <= i && i <= 0x33DD
				)
					use = !Char.IsLetterOrDigit ((char) i);
				if (use)
					return false;

				// This "Digit" rule is mystery.
				// It filters some symbols out.
				if (Char.IsLetterOrDigit ((char) i))
					return false;
				if (Char.IsNumber ((char) i))
					return false;
				if (Char.IsControl ((char) i)
					|| Char.IsSeparator ((char) i)
					|| Char.IsPunctuation ((char) i))
					return true;
				if (Char.IsSymbol ((char) i))
					return true;

				// FIXME: should check more
				return false;
			}
		}

		// To check IsIgnorableSymbol sanity, try the driver below under MS.NET.
/*
		public static void Main ()
		{
			CompareInfo ci = CultureInfo.InvariantCulture.CompareInfo;
			for (int i = 0; i <= char.MaxValue; i++) {
				UnicodeCategory uc = Char.GetUnicodeCategory ((char) i);
				if (uc == UnicodeCategory.Surrogate)
					continue;

				bool ret = IsIgnorableSymbol (i);

				string s1 = "TEST ";
				string s2 = "TEST " + (char) i;

				int result = ci.Compare (s1, s2, CompareOptions.IgnoreSymbols);

				if (ret != (result == 0))
					Console.WriteLine ("{0} : {1:x}[{2}]({3})",
						ret ? "should not ignore" :
							"should ignore",
						i,(char) i, uc);
			}
		}
*/
		#endregion

		#region NonSpacing
		static bool IsIgnorableNonSpacing (int i)
		{
			if (IsIgnorable (i))
				return true;

			switch (i) {
			case 0x02C8: case 0x02DE: case 0x0559: case 0x055A:
			case 0x05C0: case 0x0ABD: case 0x0CD5: case 0x0CD6:
			case 0x309B: case 0x309C: case 0xFF9E: case 0xFF9F:
				return true;
			case 0x02D0: case 0x0670: case 0x0901: case 0x0902:
			case 0x094D: case 0x0962: case 0x0963: case 0x0A41:
			case 0x0A42: case 0x0A47: case 0x0A48: case 0x0A4B:
			case 0x0A4C: case 0x0A81: case 0x0A82: case 0x0B82:
			case 0x0BC0: case 0x0CBF: case 0x0CC6: case 0x0CCC:
			case 0x0CCD: case 0x0E4E:
				return false;
			}

			if (0x02b9 <= i && i <= 0x02c5
				|| 0x02cc <= i && i <= 0x02d7
				|| 0x02e4 <= i && i <= 0x02ef
				|| 0x20DD <= i && i <= 0x20E0
			)
				return true;

			if (0x064B <= i && i <= 0x00652
				|| 0x0941 <= i && i <= 0x0948
				|| 0x0AC1 <= i && i <= 0x0ACD
				|| 0x0C3E <= i && i <= 0x0C4F
				|| 0x0E31 <= i && i <= 0x0E3F
			)
				return false;

			return Char.GetUnicodeCategory ((char) i) ==
				UnicodeCategory.NonSpacingMark;
		}

		// We can reuse IsIgnorableSymbol testcode 
		// for IsIgnorableNonSpacing.
		#endregion
	}

	struct CharMapEntry
	{
		public byte Category;
		public byte Level1;
		public byte Level2; // It is always single byte.
		public bool Defined;

		public CharMapEntry (byte category, byte level1, byte level2)
		{
			Category = category;
			Level1 = level1;
			Level2 = level2;
			Defined = true;
		}
	}

	class JISCharacter
	{
		public readonly int CP;
		public readonly int JIS;

		public JISCharacter (int cp, int cpJIS)
		{
			CP = cp;
			JIS = cpJIS;
		}
	}

	class JISComparer : IComparer
	{
		public static readonly JISComparer Instance =
			new JISComparer ();

		public int Compare (object o1, object o2)
		{
			JISCharacter j1 = (JISCharacter) o1;
			JISCharacter j2 = (JISCharacter) o2;
			return j1.JIS - j2.JIS;
		}
	}

	class NonJISCharacter
	{
		public readonly int CP;
		public readonly string Name;

		public NonJISCharacter (int cp, string name)
		{
			CP = cp;
			Name = name;
		}
	}

	class NonJISComparer : IComparer
	{
		public static readonly NonJISComparer Instance =
			new NonJISComparer ();

		public int Compare (object o1, object o2)
		{
			NonJISCharacter j1 = (NonJISCharacter) o1;
			NonJISCharacter j2 = (NonJISCharacter) o2;
			return string.CompareOrdinal (j1.Name, j2.Name);
		}
	}

	class DecimalDictionaryValueComparer : IComparer
	{
		public static readonly DecimalDictionaryValueComparer Instance
			= new DecimalDictionaryValueComparer ();

		private DecimalDictionaryValueComparer ()
		{
		}

		public int Compare (object o1, object o2)
		{
			DictionaryEntry e1 = (DictionaryEntry) o1;
			DictionaryEntry e2 = (DictionaryEntry) o2;
			// FIXME: in case of 0, compare decomposition categories
			int ret = Decimal.Compare ((decimal) e1.Value, (decimal) e2.Value);
			if (ret != 0)
				return ret;
			int i1 = (int) e1.Key;
			int i2 = (int) e2.Key;
			return i1 - i2;
		}
	}

	class StringDictionaryValueComparer : IComparer
	{
		public static readonly StringDictionaryValueComparer Instance
			= new StringDictionaryValueComparer ();

		private StringDictionaryValueComparer ()
		{
		}

		public int Compare (object o1, object o2)
		{
			DictionaryEntry e1 = (DictionaryEntry) o1;
			DictionaryEntry e2 = (DictionaryEntry) o2;
			int ret = String.Compare ((string) e1.Value, (string) e2.Value);
			if (ret != 0)
				return ret;
			int i1 = (int) e1.Key;
			int i2 = (int) e2.Key;
			return i1 - i2;
		}
	}

	class UCAComparer : IComparer
	{
		public static readonly UCAComparer Instance
			= new UCAComparer ();

		private UCAComparer ()
		{
		}

		public int Compare (object o1, object o2)
		{
			char i1 = (char) o1;
			char i2 = (char) o2;

			int l1 = CollationElementTable.GetSortKeyCount (i1);
			int l2 = CollationElementTable.GetSortKeyCount (i2);
			int l = l1 > l2 ? l2 : l1;

			for (int i = 0; i < l; i++) {
				SortKeyValue k1 = CollationElementTable.GetSortKey (i1, i);
				SortKeyValue k2 = CollationElementTable.GetSortKey (i2, i);
				int v = k1.Primary - k2.Primary;
				if (v != 0)
					return v;
				v = k1.Secondary - k2.Secondary;
				if (v != 0)
					return v;
				v = k1.Thirtiary - k2.Thirtiary;
				if (v != 0)
					return v;
				v = k1.Quarternary - k2.Quarternary;
				if (v != 0)
					return v;
			}
			return l1 - l2;
		}
	}

	class Tailoring
	{
		int lcid;
		int alias;
		bool frenchSort;
		ArrayList items = new ArrayList ();

		public Tailoring (int lcid)
			: this (lcid, 0)
		{
		}

		public Tailoring (int lcid, int alias)
		{
			this.lcid = lcid;
			this.alias = alias;
		}

		public int LCID {
			get { return lcid; }
		}

		public int Alias {
			get { return alias; }
		}

		public bool FrenchSort {
			get { return frenchSort; }
			set { frenchSort = value; }
		}

		public void AddDiacriticalMap (byte target, byte replace)
		{
			items.Add (new DiacriticalMap (target, replace));
		}

		public void AddSortKeyMap (string source, byte [] sortkey)
		{
			items.Add (new SortKeyMap (source, sortkey));
		}

		public void AddReplacementMap (string source, string replace)
		{
			items.Add (new ReplacementMap (source, replace));
		}

		public char [] ItemToCharArray ()
		{
			ArrayList al = new ArrayList ();
			foreach (ITailoringMap m in items)
				al.AddRange (m.ToCharArray ());
			return al.ToArray (typeof (char)) as char [];
		}

		interface ITailoringMap
		{
			char [] ToCharArray ();
		}

		class DiacriticalMap : ITailoringMap
		{
			public readonly byte Target;
			public readonly byte Replace;

			public DiacriticalMap (byte target, byte replace)
			{
				Target = target;
				Replace = replace;
			}

			public char [] ToCharArray ()
			{
				char [] ret = new char [3];
				ret [0] = (char) 02; // kind:DiacriticalMap
				ret [1] = (char) Target;
				ret [2] = (char) Replace;
				return ret;
			}
		}

		class SortKeyMap : ITailoringMap
		{
			public readonly string Source;
			public readonly byte [] SortKey;

			public SortKeyMap (string source, byte [] sortkey)
			{
				Source = source;
				SortKey = sortkey;
			}

			public char [] ToCharArray ()
			{
				char [] ret = new char [Source.Length + 7];
				ret [0] = (char) 01; // kind:SortKeyMap
				for (int i = 0; i < Source.Length; i++)
					ret [i + 1] = Source [i];
				// null terminate
				for (int i = 0; i < 4; i++)
					ret [i + Source.Length + 2] = (char) SortKey [i];
				return ret;
			}
		}

		class ReplacementMap : ITailoringMap
		{
			public readonly string Source;
			public readonly string Replace;

			public ReplacementMap (string source, string replace)
			{
				Source = source;
				Replace = replace;
			}

			public char [] ToCharArray ()
			{
				char [] ret = new char [Source.Length + Replace.Length + 3];
				ret [0] = (char) 03; // kind:ReplaceMap
				int pos = 1;
				for (int i = 0; i < Source.Length; i++)
					ret [pos++] = Source [i];
				// null terminate
				pos++;
				for (int i = 0; i < Replace.Length; i++)
					ret [pos++] = Replace [i];
				// null terminate
				return ret;
			}
		}
	}
}

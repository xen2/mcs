//
// SimpleCollator.cs : the core collator implementation
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

//
// Here's a summary for supporting contractions, expansions and diacritical 
// remappings.
//
// Diacritical mapping is a simple tailoring rule that "remaps" diacritical
// weight value from one to another. For now it applies to all range of
// characters, but at some stage we might need to limit the remapping ranges.
//
// A Contraction consists of a string (actually char[]) and a sortkey entry
// (i.e. byte[]). It indicates that a sequence of characters is interpreted
// as a single character that has the mapped sortkey value. There is no
// character which goes across "special rules". When the collator encountered
// such a sequence of characters, it just returns the sortkey value without
// further replacement.
//
// Since it is still likely to happen that a contraction sequence matches
// other character than the identical sequence (depending on CompareOptions
// and of course, defined sortkey value itself), comparison cannot be done
// at source char[] level.
//
// In IndexOf(), the first character in the target string or the target char
// itself is turned into sortkey bytes. If the character has a contraction and
// that is sortkey map, then it is used instead. If the contraction exists and
// that is replacement map, then the first character of the replacement string
// is searched instead. IndexOf() always searches only for the top character,
// and if it returned negative value, then it returns -1. Otherwise, it then
// tries IsPrefix() from that location. If it returns true, then it returns
// the index.
//
// LAMESPEC: IndexOf() is lame as a whole API. It never matches in the middle
// of expansion and there is no proper way to return such indexes within
// a single int return value.
//
// For example, try below in .NET:
//	IndexOf("\u00E6", "a")
//	IndexOf("\u00E6", "e")
//


using System;
using System.Collections;
using System.Globalization;

using Uni = Mono.Globalization.Unicode.MSCompatUnicodeTable;
using UUtil = Mono.Globalization.Unicode.MSCompatUnicodeTableUtil;
using COpt = System.Globalization.CompareOptions;

namespace Mono.Globalization.Unicode
{
	internal class SimpleCollator
	{
		struct PreviousInfo
		{
			public int Code;
			public byte [] SortKey;

			public PreviousInfo (bool dummy)
			{
				Code = -1;
				SortKey = null;
			}
		}

		struct Escape
		{
			public string Source;
			public int Index;
			public int Start;
			public int End;
			public int Optional;
		}

		static SimpleCollator invariant =
			new SimpleCollator (CultureInfo.InvariantCulture);

		readonly TextInfo textInfo; // for ToLower().
		readonly bool frenchSort;
		unsafe readonly byte* cjkCatTable;
		unsafe readonly byte* cjkLv1Table;
		readonly CodePointIndexer cjkIndexer;
		unsafe readonly byte* cjkLv2Table;
		readonly CodePointIndexer cjkLv2Indexer;
		readonly int lcid;
		readonly Contraction [] contractions;
		readonly Level2Map [] level2Maps;

		// This flag marks characters as "unsafe", where the character
		// could be used as part of a contraction (whose length > 1).
		readonly byte [] unsafeFlags;

		const int UnsafeFlagLength = 0x300 / 8;

//		readonly byte [] contractionFlags = new byte [16];

		// This array is internally used inside IndexOf() to store
		// "no need to check" ASCII characters.
		//
		// Now that it should be thread safe, this array is allocated
		// at every time.
//		byte [] checkedFlags = new byte [128 / 8];

		#region .ctor() and split functions

		public SimpleCollator (CultureInfo culture)
		{
			lcid = culture.LCID;
			textInfo = culture.TextInfo;

			unsafe {
				SetCJKTable (culture, ref cjkIndexer,
					ref cjkCatTable, ref cjkLv1Table,
					ref cjkLv2Indexer, ref cjkLv2Table);
			}

			// Get tailoring info
			TailoringInfo t = null;
			for (CultureInfo ci = culture; ci.LCID != 127; ci = ci.Parent) {
				t = Uni.GetTailoringInfo (ci.LCID);
				if (t != null)
					break;
			}
			if (t == null) // then use invariant
				t = Uni.GetTailoringInfo (127);

			frenchSort = t.FrenchSort;
			Uni.BuildTailoringTables (culture, t, ref contractions,
				ref level2Maps);
			unsafeFlags = new byte [UnsafeFlagLength];
			foreach (Contraction c in contractions) {
				if (c.Source.Length > 1)
					foreach (char ch in c.Source)
						unsafeFlags [(int) ch / 8 ]
							|= (byte) (1 << ((int) ch & 7));
//				for (int i = 0; i < c.Source.Length; i++) {
//					int ch = c.Source [i];
//					if (ch > 127)
//						continue;
//					contractionFlags [ch / 8] |= (byte) (1 << (ch & 7));
//				}
			}
			if (lcid != 127)
				foreach (Contraction c in invariant.contractions) {
					if (c.Source.Length > 1)
						foreach (char ch in c.Source)
							unsafeFlags [(int) ch / 8 ] 
								|= (byte) (1 << ((int) ch & 7));
//					for (int i = 0; i < c.Source.Length; i++) {
//						int ch = c.Source [i];
//						if (ch > 127)
//							continue;
//						contractionFlags [ch / 8] |= (byte) (1 << (ch & 7));
//					}
				}

			// FIXME: Since tailorings are mostly for latin
			// (and in some cases Cyrillic) characters, it would
			// be much better for performance to store "start 
			// indexes" for > 370 (culture-specific letters).

/*
// dump tailoring table
Console.WriteLine ("******** building table for {0} : c - {1} d - {2}",
culture.LCID, contractions.Length, level2Maps.Length);
foreach (Contraction c in contractions) {
foreach (char cc in c.Source)
Console.Write ("{0:X4} ", (int) cc);
Console.WriteLine (" -> '{0}'", c.Replacement);
}
*/
		}

		unsafe private void SetCJKTable (
			CultureInfo culture, ref CodePointIndexer cjkIndexer,
			ref byte* catTable, ref byte* lv1Table,
			ref CodePointIndexer lv2Indexer, ref byte* lv2Table)
		{
			string name = GetNeutralCulture (culture).Name;

			Uni.FillCJK (name, ref cjkIndexer, ref catTable,
				ref lv1Table, ref lv2Indexer, ref lv2Table);
		}

		static CultureInfo GetNeutralCulture (CultureInfo info)
		{
			CultureInfo ret = info;
			while (ret.Parent != null && ret.Parent.LCID != 127)
				ret = ret.Parent;
			return ret;
		}

		#endregion

		unsafe byte Category (int cp)
		{
			if (cp < 0x3000 || cjkCatTable == null)
				return Uni.Category (cp);
			int idx = cjkIndexer.ToIndex (cp);
			return idx < 0 ? Uni.Category (cp) : cjkCatTable [idx];
		}

		unsafe byte Level1 (int cp)
		{
			if (cp < 0x3000 || cjkLv1Table == null)
				return Uni.Level1 (cp);
			int idx = cjkIndexer.ToIndex (cp);
			return idx < 0 ? Uni.Level1 (cp) : cjkLv1Table [idx];
		}

		unsafe byte Level2 (int cp, ExtenderType ext)
		{
			if (ext == ExtenderType.Buggy)
				return 5;
			else if (ext == ExtenderType.Conditional)
				return 0;

			if (cp < 0x3000 || cjkLv2Table == null)
				return Uni.Level2 (cp);
			int idx = cjkLv2Indexer.ToIndex (cp);
			byte ret = idx < 0 ? (byte) 0 : cjkLv2Table [idx];
			if (ret != 0)
				return ret;
			ret = Uni.Level2 (cp);
			if (level2Maps.Length == 0)
				return ret;
			for (int i = 0; i < level2Maps.Length; i++) {
				if (level2Maps [i].Source == ret)
					return level2Maps [i].Replace;
				else if (level2Maps [i].Source > ret)
					break;
			}
			return ret;
		}

		static bool IsHalfKana (int cp, COpt opt)
		{
			return (opt & COpt.IgnoreWidth) != 0 ||
				Uni.IsHalfWidthKana ((char) cp);
		}

		Contraction GetContraction (string s, int start, int end)
		{
//			int si = s [start];
//			if (si < 128 && (contractionFlags [si / 8] & (1 << (si & 7))) == 0)
//				return null;
			Contraction c = GetContraction (s, start, end, contractions);
			if (c != null || lcid == 127)
				return c;
			return GetContraction (s, start, end, invariant.contractions);
		}

		Contraction GetContraction (string s, int start, int end, Contraction [] clist)
		{
			for (int i = 0; i < clist.Length; i++) {
				Contraction ct = clist [i];
				int diff = ct.Source [0] - s [start];
				if (diff > 0)
					return null; // it's already sorted
				else if (diff < 0)
					continue;
				char [] chars = ct.Source;
				if (end - start < chars.Length)
					continue;
				bool match = true;
				for (int n = 0; n < chars.Length; n++)
					if (s [start + n] != chars [n]) {
						match = false;
						break;
					}
				if (match)
					return ct;
			}
			return null;
		}

		Contraction GetTailContraction (string s, int start, int end)
		{
//			int si = s [end - 1];
//			if (si < 128 && (contractionFlags [si / 8] & (1 << (si & 7))) == 0)
//				return null;
			Contraction c = GetTailContraction (s, start, end, contractions);
			if (c != null || lcid == 127)
				return c;
			return GetTailContraction (s, start, end, invariant.contractions);
		}

		Contraction GetTailContraction (string s, int start, int end, Contraction [] clist)
		{
			for (int i = 0; i < clist.Length; i++) {
				Contraction ct = clist [i];
				int diff = ct.Source [0] - s [end];
				if (diff > 0)
					return null; // it's already sorted
				else if (diff < 0)
					continue;
				char [] chars = ct.Source;
				if (start - end + 1 < chars.Length)
					continue;
				bool match = true;
				int offset = start - chars.Length + 1;
				for (int n = 0; n < chars.Length; n++)
					if (s [offset + n] != chars [n]) {
						match = false;
						break;
					}
				if (match)
					return ct;
			}
			return null;
		}

		Contraction GetContraction (char c)
		{
			Contraction ct = GetContraction (c, contractions);
			if (ct != null || lcid == 127)
				return ct;
			return GetContraction (c, invariant.contractions);
		}

		Contraction GetContraction (char c, Contraction [] clist)
		{
			for (int i = 0; i < clist.Length; i++) {
				Contraction ct = clist [i];
				if (ct.Source [0] > c)
					return null; // it's already sorted
				if (ct.Source [0] == c && ct.Source.Length == 1)
					return ct;
			}
			return null;
		}

		int FilterOptions (int i, COpt opt)
		{
			if ((opt & COpt.IgnoreWidth) != 0) {
				int x = Uni.ToWidthCompat (i);
				if (x != 0)
					i = x;
			}
			if ((opt & COpt.IgnoreCase) != 0)
				i = textInfo.ToLower ((char) i);
			if ((opt & COpt.IgnoreKanaType) != 0)
				i = Uni.ToKanaTypeInsensitive (i);
			return i;
		}

		enum ExtenderType {
			None,
			Simple,
			Voiced,
			Conditional,
			Buggy,
		}

		ExtenderType GetExtenderType (int i)
		{
			// LAMESPEC: Windows expects true for U+3005, but 
			// sometimes it does not represent to repeat just
			// one character.
			// Windows also expects true for U+3031 and U+3032,
			// but they should *never* repeat one character.

			// U+2015 becomes an extender only when it is Japanese
			if (i == 0x2015)
				return lcid == 16 ? ExtenderType.Conditional :
					ExtenderType.None;

			if (i < 0x3005 || i > 0xFF70)
				return ExtenderType.None;
			if (i >= 0xFE7C) {
				switch (i) {
				case 0xFE7C:
				case 0xFE7D:
					return ExtenderType.Simple;
				case 0xFF70:
					return ExtenderType.Conditional;
				case 0xFF9E:
				case 0xFF9F:
					return ExtenderType.Voiced;
				}
			}
			if (i > 0x30FE)
				return ExtenderType.None;
			switch (i) {
			case 0x3005: // LAMESPEC: see above.
				return ExtenderType.Buggy;
			case 0x3031: // LAMESPEC: see above.
			case 0x3032: // LAMESPEC: see above.
			case 0x309D:
			case 0x30FD:
				return ExtenderType.Simple;
			case 0x309E:
			case 0x30FE:
				return ExtenderType.Voiced;
			case 0x30FC:
				return ExtenderType.Conditional;
			default:
				return ExtenderType.None;
			}
		}

		static byte ToDashTypeValue (ExtenderType ext, COpt opt)
		{
			if ((opt & COpt.IgnoreNonSpace) != 0) // LAMESPEC: huh, why?
				return 3;
			switch (ext) {
			case ExtenderType.None:
				return 3;
			case ExtenderType.Conditional:
				return 5;
			default:
				return 4;
			}
		}

		int FilterExtender (int i, ExtenderType ext, COpt opt)
		{
			if (ext == ExtenderType.Conditional &&
				Uni.HasSpecialWeight ((char) i)) {
				bool half = IsHalfKana ((char) i, opt);
				bool katakana = !Uni.IsHiragana ((char) i);
				switch (Level1 (i) & 7) {
				case 2:
					return half ? 0xFF71 : katakana ? 0x30A2 : 0x3042;
				case 3:
					return half ? 0xFF72 : katakana ? 0x30A4 : 0x3044;
				case 4:
					return half ? 0xFF73 : katakana ? 0x30A6 : 0x3046;
				case 5:
					return half ? 0xFF74 : katakana ? 0x30A8 : 0x3048;
				case 6:
					return half ? 0xFF75 : katakana ? 0x30AA : 0x304A;
				}
			}
			return i;
		}

		static bool IsIgnorable (int i, COpt opt)
		{
			return Uni.IsIgnorable (i, (byte) (1 +
				((opt & COpt.IgnoreSymbols) != 0 ? 2 : 0) +
				((opt & COpt.IgnoreNonSpace) != 0 ? 4 : 0)));
			
		}

		bool IsSafe (int i)
		{
			return i / 8 >= unsafeFlags.Length ? true : (unsafeFlags [i / 8] & (1 << (i % 8))) == 0;
		}

		#region GetSortKey()

		public SortKey GetSortKey (string s)
		{
			return GetSortKey (s, CompareOptions.None);
		}

		public SortKey GetSortKey (string s, CompareOptions options)
		{
			return GetSortKey (s, 0, s.Length, options);
		}

		public SortKey GetSortKey (string s, int start, int length, CompareOptions options)
		{
			SortKeyBuffer buf = new SortKeyBuffer (lcid);
			buf.Initialize (options, lcid, s, frenchSort);
			int end = start + length;
			GetSortKey (s, start, end, buf, options);
			return buf.GetResultAndReset ();
		}

		void GetSortKey (string s, int start, int end,
			SortKeyBuffer buf, CompareOptions opt)
		{
			PreviousInfo prev = new PreviousInfo (false);

			for (int n = start; n < end; n++) {
				int i = s [n];

				ExtenderType ext = GetExtenderType (i);
				if (ext != ExtenderType.None) {
					i = FilterExtender (prev.Code, ext, opt);
					if (i >= 0)
						FillSortKeyRaw (i, ext, buf, opt);
					else if (prev.SortKey != null) {
						byte [] b = prev.SortKey;
						buf.AppendNormal (
							b [0],
							b [1],
							b [2] != 1 ? b [2] : Level2 (i, ext),
							b [3] != 1 ? b [3] : Uni.Level3 (i));
					}
					// otherwise do nothing.
					// (if the extender is the first char
					// in the string, then just ignore.)
					continue;
				}

				if (IsIgnorable (i, opt))
					continue;
				i = FilterOptions (i, opt);

				Contraction ct = GetContraction (s, n, end);
				if (ct != null) {
					if (ct.Replacement != null) {
						GetSortKey (ct.Replacement, 0, ct.Replacement.Length, buf, opt);
					} else {
						byte [] b = ct.SortKey;
						buf.AppendNormal (
							b [0],
							b [1],
							b [2] != 1 ? b [2] : Level2 (i, ext),
							b [3] != 1 ? b [3] : Uni.Level3 (i));
						prev.SortKey = b;
						prev.Code = -1;
					}
					n += ct.Source.Length - 1;
				}
				else {
					if (!Uni.IsIgnorableNonSpacing (i))
						prev.Code = i;
					FillSortKeyRaw (i, ExtenderType.None, buf, opt);
				}
			}
		}

		void FillSortKeyRaw (int i, ExtenderType ext,
			SortKeyBuffer buf, CompareOptions opt)
		{
			if (0x3400 <= i && i <= 0x4DB5) {
				int diff = i - 0x3400;
				buf.AppendCJKExtension (
					(byte) (0x10 + diff / 254),
					(byte) (diff % 254 + 2));
				return;
			}

			UnicodeCategory uc = char.GetUnicodeCategory ((char) i);
			switch (uc) {
			case UnicodeCategory.PrivateUse:
				int diff = i - 0xE000;
				buf.AppendNormal (
					(byte) (0xE5 + diff / 254),
					(byte) (diff % 254 + 2),
					0,
					0);
				return;
			case UnicodeCategory.Surrogate:
				FillSurrogateSortKeyRaw (i, buf);
				return;
			}

			byte level2 = Level2 (i, ext);
			if (Uni.HasSpecialWeight ((char) i)) {
				byte level1 = Level1 (i);
				buf.AppendKana (
					Category (i),
					level1,
					level2,
					Uni.Level3 (i),
					Uni.IsJapaneseSmallLetter ((char) i),
					ToDashTypeValue (ext, opt),
					!Uni.IsHiragana ((char) i),
					IsHalfKana ((char) i, opt)
					);
				if ((opt & COpt.IgnoreNonSpace) == 0 && ext == ExtenderType.Voiced)
					// Append voice weight
					buf.AppendNormal (1, 1, 1, 0);
			}
			else
				buf.AppendNormal (
					Category (i),
					Level1 (i),
					level2,
					Uni.Level3 (i));
		}

		void FillSurrogateSortKeyRaw (int i, SortKeyBuffer buf)
		{
			int diffbase = 0;
			int segment = 0;
			byte lower = 0;

			if (i < 0xD840) {
				diffbase = 0xD800;
				segment = 0x41;
				lower = (byte) ((i == 0xD800) ? 0x3E : 0x3F);
			} else if (0xD840 <= i && i < 0xD880) {
				diffbase = 0xD840;
				segment = 0xF2;
				lower = 0x3E;
			} else if (0xDB80 <= i && i < 0xDC00) {
				diffbase = 0xDB80 - 0x40;
				segment = 0xFE;
				lower = 0x3E;
			} else {
				diffbase = 0xDC00 - 0xF8 + 2;
				segment = 0x41;
				lower = 0x3F;
			}
			int diff = i - diffbase;

			buf.AppendNormal (
				(byte) (segment + diff / 254),
				(byte) (diff % 254 + 2),
				lower,
				lower);
		}

		#endregion

		#region Compare()

		public int Compare (string s1, string s2)
		{
			return Compare (s1, s2, CompareOptions.None);
		}

		public int Compare (string s1, string s2, CompareOptions options)
		{
			return Compare (s1, 0, s1.Length, s2, 0, s2.Length, options);
		}

		private int CompareOrdinal (string s1, int idx1, int len1,
			string s2, int idx2, int len2)
		{
			int min = len1 < len2 ? len1 : len2;
			int end1 = idx1 + min;
			int end2 = idx2 + min;
			if (idx1 < 0 || idx2 < 0 || end1 > s1.Length || end2 > s2.Length)
				throw new SystemException (String.Format ("CompareInfo Internal Error: Should not happen. {0} {1} {2} {3} {4} {5}", idx1, idx2, len1, len2, s1.Length, s2.Length));
			for (int i1 = idx1, i2 = idx2;
				i1 < end1 && i2 < end2; i1++, i2++)
				if (s1 [i1] != s2 [i2])
					return s1 [i1] - s2 [i2];
			return len1 == len2 ? 0 :
				len1 == min ? - 1 : 1;
		}

		public int Compare (string s1, int idx1, int len1,
			string s2, int idx2, int len2, CompareOptions options)
		{
			// quick equality check
			if (idx1 == idx2 && len1 == len2 &&
				Object.ReferenceEquals (s1, s2))
				return 0;
			// FIXME: this should be done inside Compare() at
			// any time.
//			int ord = CompareOrdinal (s1, idx1, len1, s2, idx2, len2);
//			if (ord == 0)
//				return 0;
			if (options == CompareOptions.Ordinal)
				return CompareOrdinal (s1, idx1, len1, s2, idx2, len2);

#if false // stable easy version, depends on GetSortKey().
			SortKey sk1 = GetSortKey (s1, idx1, len1, options);
			SortKey sk2 = GetSortKey (s2, idx2, len2, options);
			byte [] d1 = sk1.KeyData;
			byte [] d2 = sk2.KeyData;
			int len = d1.Length > d2.Length ? d2.Length : d1.Length;
			for (int i = 0; i < len; i++)
				if (d1 [i] != d2 [i])
					return d1 [i] < d2 [i] ? -1 : 1;
			return d1.Length == d2.Length ? 0 : d1.Length < d2.Length ? -1 : 1;
#else
			PreviousInfo prev1 = new PreviousInfo (false);
			byte [] sk1 = new byte [4];
			byte [] sk2 = new byte [4];
			bool dummy, dummy2;
			int ret = CompareInternal (options, s1, idx1, len1, s2, idx2, len2, out dummy, out dummy2, true, ref prev1, sk1, sk2);
			return ret == 0 ? 0 : ret < 0 ? -1 : 1;
#endif
		}

		int CompareInternal (COpt opt, string s1, int idx1, int len1, string s2,
			int idx2, int len2,
			out bool targetConsumed, out bool sourceConsumed,
			bool skipHeadingExtenders, ref PreviousInfo prev1,
			byte [] charSortKey, byte [] charSortKey2)
		{
			int start1 = idx1;
			int start2 = idx2;
			int end1 = idx1 + len1;
			int end2 = idx2 + len2;
			targetConsumed = false;
			sourceConsumed = false;
			PreviousInfo prev2 = new PreviousInfo (false);

			// It holds final result that comes from the comparison
			// at level 2 or lower. Even if Compare() found the
			// difference at level 2 or lower, it still has to
			// continue level 1 comparison. FinalResult is used
			// when there was no further differences.
			int finalResult = 0;
			// It holds the comparison level to do. It starts from
			// 5, and becomes 1 when we do primary-only comparison.
			int currentLevel = 5;

			int lv5At1 = -1;
			int lv5At2 = -1;
			int lv5Value1 = 0;
			int lv5Value2 = 0;

			// Skip heading extenders
			if (skipHeadingExtenders) {
				for (; idx1 < end1; idx1++)
					if (GetExtenderType (s1 [idx1]) == ExtenderType.None)
						break;
				for (; idx2 < end2; idx2++)
					if (GetExtenderType (s2 [idx2]) == ExtenderType.None)
						break;
			}

			ExtenderType ext1 = ExtenderType.None;
			ExtenderType ext2 = ExtenderType.None;

			int quickCheckPos1 = idx1;
			int quickCheckPos2 = idx2;
			bool stringSort = (opt & COpt.StringSort) != 0;
			bool ignoreNonSpace = (opt & COpt.IgnoreNonSpace) != 0;
			Escape escape1 = new Escape ();
			Escape escape2 = new Escape ();

			while (true) {
				for (; idx1 < end1; idx1++)
					if (!IsIgnorable (s1 [idx1], opt))
						break;
				for (; idx2 < end2; idx2++)
					if (!IsIgnorable (s2 [idx2], opt))
						break;

				if (idx1 >= end1) {
					if (escape1.Source == null)
						break;
					s1 = escape1.Source;
					start1 = escape1.Start;
					idx1 = escape1.Index;
					end1 = escape1.End;
					quickCheckPos1 = escape1.Optional;
					escape1.Source = null;
					continue;
				}
				if (idx2 >= end2) {
					if (escape2.Source == null)
						break;
					s2 = escape2.Source;
					start2 = escape2.Start;
					idx2 = escape2.Index;
					end2 = escape2.End;
					quickCheckPos2 = escape2.Optional;
					escape2.Source = null;
					continue;
				}
#if true
// If comparison is unstable, then this part is one of the most doubtful part.
// Here we run quick codepoint comparison and run back to "stable" area to
// compare characters.

				// Strictly to say, even the first character
				// could be compared here, but it messed
				// backward step, so I just avoided mess.
				if (quickCheckPos1 < idx1 && quickCheckPos2 < idx2) {
					while (idx1 < end1 && idx2 < end2 &&
						s1 [idx1] == s2 [idx2]) {
						idx1++;
						idx2++;
					}
					if (idx1 == end1 || idx2 == end2)
						continue; // check replacement

					int backwardEnd1 = quickCheckPos1;
					int backwardEnd2 = quickCheckPos2;
					quickCheckPos1 = idx1;
					quickCheckPos2 = idx2;

					idx1--;
					idx2--;

					for (;idx1 > backwardEnd1; idx1--)
						if (Category (s1 [idx1]) != 1)
							break;
					for (;idx2 > backwardEnd2; idx2--)
						if (Category (s2 [idx2]) != 1)
							break;
					for (;idx1 > backwardEnd1; idx1--)
						if (IsSafe (s1 [idx1]))
							break;
					for (;idx2 > backwardEnd2; idx2--)
						if (IsSafe (s2 [idx2]))
							break;
				}
#endif

				int cur1 = idx1;
				int cur2 = idx2;
				byte [] sk1 = null;
				byte [] sk2 = null;
				int i1 = FilterOptions (s1 [idx1], opt);
				int i2 = FilterOptions (s2 [idx2], opt);
				bool special1 = false;
				bool special2 = false;

				// If current character is an expander, then
				// repeat the previous character.
				ext1 = GetExtenderType (i1);
				if (ext1 != ExtenderType.None) {
					if (prev1.Code < 0) {
						if (prev1.SortKey == null) {
							// nothing to extend
							idx1++;
							continue;
						}
						sk1 = prev1.SortKey;
					}
					else
						i1 = FilterExtender (prev1.Code, ext1, opt);
				}
				ext2 = GetExtenderType (i2);
				if (ext2 != ExtenderType.None) {
					if (prev2.Code < 0) {
						if (prev2.SortKey == null) {
							// nothing to extend
							idx2++;
							continue;
						}
						sk2 = prev2.SortKey;
					}
					else
						i2 = FilterExtender (prev2.Code, ext2, opt);
				}

				byte cat1 = Category (i1);
				byte cat2 = Category (i2);

				// Handle special weight characters
				if (!stringSort && currentLevel > 4) {
					if (cat1 == 6) {
						lv5At1 = escape1.Source != null ?
							escape1.Index - escape1.Start :
							cur1 - start1;
						// here Windows has a bug that it does
						// not consider thirtiary weight.
						lv5Value1 = Level1 (i1) << 8 + Uni.Level3 (i1);
						prev1.Code = i1;
						idx1++;
					}
					if (cat2 == 6) {
						lv5At2 = escape2.Source != null ?
							escape2.Index - escape2.Start :
							cur2 - start2;
						// here Windows has a bug that it does
						// not consider thirtiary weight.
						lv5Value2 = Level1 (i2) << 8 + Uni.Level3 (i2);
						prev2.Code = i2;
						idx2++;
					}
					if (cat1 == 6 || cat2 == 6) {
						currentLevel = 4;
						continue;
					}
				}

				Contraction ct1 = null;
				if (ext1 == ExtenderType.None)
					ct1 = GetContraction (s1, idx1, end1);

				int offset1 = 1;
				if (sk1 != null)
					offset1 = 1;
				else if (ct1 != null) {
					offset1 = ct1.Source.Length;
					if (ct1.SortKey != null) {
						sk1 = charSortKey;
						for (int i = 0; i < ct1.SortKey.Length; i++)
							sk1 [i] = ct1.SortKey [i];
						prev1.Code = -1;
						prev1.SortKey = sk1;
					}
					else if (escape1.Source == null) {
						escape1.Source = s1;
						escape1.Start = start1;
						escape1.Index = cur1 + ct1.Source.Length;
						escape1.End = end1;
						escape1.Optional = quickCheckPos1;
						s1 = ct1.Replacement;
						idx1 = 0;
						start1 = 0;
						end1 = s1.Length;
						quickCheckPos1 = 0;
						continue;
					}
				}
				else {
					sk1 = charSortKey;
					sk1 [0] = cat1;
					sk1 [1] = Level1 (i1);
					if (!ignoreNonSpace && currentLevel > 1)
						sk1 [2] = Level2 (i1, ext1);
					if (currentLevel > 2)
						sk1 [3] = Uni.Level3 (i1);
					if (currentLevel > 3)
						special1 = Uni.HasSpecialWeight ((char) i1);
					if (cat1 > 1)
						prev1.Code = i1;
				}

				Contraction ct2 = null;
				if (ext2 == ExtenderType.None)
					ct2 = GetContraction (s2, idx2, end2);

				if (sk2 != null)
					idx2++;
				else if (ct2 != null) {
					idx2 += ct2.Source.Length;
					if (ct2.SortKey != null) {
						sk2 = charSortKey2;
						for (int i = 0; i < ct2.SortKey.Length; i++)
							sk2 [i] = ct2.SortKey [i];
						prev2.Code = -1;
						prev2.SortKey = sk2;
					}
					else if (escape2.Source == null) {
						escape2.Source = s2;
						escape2.Start = start2;
						escape2.Index = cur2 + ct2.Source.Length;
						escape2.End = end2;
						escape2.Optional = quickCheckPos2;
						s2 = ct2.Replacement;
						idx2 = 0;
						start2 = 0;
						end2 = s2.Length;
						quickCheckPos2 = 0;
						continue;
					}
				}
				else {
					sk2 = charSortKey2;
					sk2 [0] = cat2;
					sk2 [1] = Level1 (i2);
					if (!ignoreNonSpace && currentLevel > 1)
						sk2 [2] = Level2 (i2, ext2);
					if (currentLevel > 2)
						sk2 [3] = Uni.Level3 (i2);
					if (currentLevel > 3)
						special2 = Uni.HasSpecialWeight ((char) i2);
					if (cat2 > 1)
						prev2.Code = i2;
					idx2++;
				}

				// Add offset here so that it does not skip
				// idx1 while s2 has a replacement.
				idx1 += offset1;

				// add diacritical marks in s1 here
				if (!ignoreNonSpace) {
					while (idx1 < end1) {
						if (Category (s1 [idx1]) != 1)
							break;
						if (sk1 [2] == 0)
							sk1 [2] = 2;
						sk1 [2] = (byte) (sk1 [2] + 
							Level2 (s1 [idx1], ExtenderType.None));
						idx1++;
					}

					// add diacritical marks in s2 here
					while (idx2 < end2) {
						if (Category (s2 [idx2]) != 1)
							break;
						if (sk2 [2] == 0)
							sk2 [2] = 2;
						sk2 [2] = (byte) (sk2 [2] + 
							Level2 (s2 [idx2], ExtenderType.None));
						idx2++;
					}
				}

				int ret = sk1 [0] - sk2 [0];
				ret = ret != 0 ? ret : sk1 [1] - sk2 [1];
				if (ret != 0)
					return ret;
				if (currentLevel == 1)
					continue;
				if (!ignoreNonSpace) {
					ret = sk1 [2] - sk2 [2];
					if (ret != 0) {
						finalResult = ret;
						currentLevel = frenchSort ? 2 : 1;
						continue;
					}
				}
				if (currentLevel == 2)
					continue;
				ret = sk1 [3] - sk2 [3];
				if (ret != 0) {
					finalResult = ret;
					currentLevel = 2;
					continue;
				}
				if (currentLevel == 3)
					continue;
				if (special1 != special2) {
					finalResult = special1 ? 1 : -1;
					currentLevel = 3;
					continue;
				}
				if (special1) {
					ret = CompareFlagPair (
						!Uni.IsJapaneseSmallLetter ((char) i1),
						!Uni.IsJapaneseSmallLetter ((char) i2));
					ret = ret != 0 ? ret :
						ToDashTypeValue (ext1, opt) -
						ToDashTypeValue (ext2, opt);
					ret = ret != 0 ? ret : CompareFlagPair (
						Uni.IsHiragana ((char) i1),
						Uni.IsHiragana ((char) i2));
					ret = ret != 0 ? ret : CompareFlagPair (
						!IsHalfKana ((char) i1, opt),
						!IsHalfKana ((char) i2, opt));
					if (ret != 0) {
						finalResult = ret;
						currentLevel = 3;
						continue;
					}
				}
			}

			// If there were only level 3 or lower differences,
			// then we still have to find diacritical differences
			// if any.
			if (!ignoreNonSpace &&
				finalResult != 0 && currentLevel > 2) {
				while (idx1 < end1 && idx2 < end2) {
					if (!Uni.IsIgnorableNonSpacing (s1 [idx1]))
						break;
					if (!Uni.IsIgnorableNonSpacing (s2 [idx2]))
						break;
					finalResult = Level2 (FilterOptions (s1 [idx1], opt), ext1) - Level2 (FilterOptions (s2 [idx2], opt), ext2);
					if (finalResult != 0)
						break;
					idx1++;
					idx2++;
					// they should work only for the first character
					ext1 = ExtenderType.None;
					ext2 = ExtenderType.None;
				}
			}
			if (currentLevel == 1 && finalResult != 0) {
				while (idx1 < end1) {
					if (Uni.IsIgnorableNonSpacing (s1 [idx1]))
						idx1++;
					else
						break;
				}
				while (idx2 < end2) {
					if (Uni.IsIgnorableNonSpacing (s2 [idx2]))
						idx2++;
					else
						break;
				}
			}
			// we still have to handle level 5
			if (finalResult == 0) {
				finalResult = lv5At1 - lv5At2;
				if (finalResult == 0)
					finalResult = lv5Value1 - lv5Value2;
			}
			if (finalResult == 0) {
				if (idx2 == end2)
					targetConsumed = true;
				if (idx1 == end1)
					sourceConsumed = true;
			}
			return idx1 != end1 ? 1 : idx2 == end2 ? finalResult : -1;
		}

		int CompareFlagPair (bool b1, bool b2)
		{
			return b1 == b2 ? 0 : b1 ? 1 : -1;
		}

		#endregion

		#region IsPrefix() and IsSuffix()

		public bool IsPrefix (string src, string target, CompareOptions opt)
		{
			return IsPrefix (src, target, 0, src.Length, opt);
		}

		public bool IsPrefix (string s, string target, int start, int length, CompareOptions opt)
		{
			if (target.Length == 0)
				return true;
			PreviousInfo prev = new PreviousInfo (false);
			byte [] sk1 = new byte [4];
			byte [] sk2 = new byte [4];
			return IsPrefix (opt, s, target, start, length, true, ref prev, sk1, sk2);
		}

		bool IsPrefix (COpt opt, string s, string target, int start, int length, bool skipHeadingExtenders, ref PreviousInfo prev, byte [] sk1, byte [] sk2)
		{
			bool consumed, dummy;
			CompareInternal (opt, s, start, length,
				target, 0, target.Length,
				out consumed, out dummy, skipHeadingExtenders,
				ref prev, sk1, sk2);
			return consumed;
		}

		// IsSuffix()

		public bool IsSuffix (string src, string target, CompareOptions opt)
		{
			return IsSuffix (src, target, src.Length - 1, src.Length, opt);
		}

		public bool IsSuffix (string s, string target, int start, int length, CompareOptions opt)
		{
			if (target.Length == 0)
				return true;
			int last = LastIndexOf (s, target, start, length, opt);
			return last >= 0 && Compare (s, last, s.Length - last, target, 0, target.Length, opt) == 0;
/*
			// quick check : simple codepoint comparison
			if (s.Length >= target.Length) {
				int si = start;
				int se = start - length;
				for (int i = target.Length - 1; si >= se && i >= 0; i--, si--)
					if (s [si] != target [i])
						break;
				if (si == start + target.Length)
					return true;
			}

			PreviousInfo prev = new PreviousInfo (false);
			byte [] sk1 = new byte [4];
			byte [] sk2 = new byte [4];
			return IsSuffix (opt, s, target, start, length, ref prev, sk1, sk2);
*/
		}

		bool IsSuffix (COpt opt, string s, string t, int start, int length, ref PreviousInfo prev, byte [] sk1, byte [] sk2)
		{
			int tstart = 0;
			for (;tstart < t.Length; tstart++)
				if (!IsIgnorable (t [tstart], opt))
					break;
			if (tstart == t.Length)
				return true; // as if target is String.Empty.

#if false
// This is still not working. If it is not likely to get working, then just remove it.
			int si = start;
			int send = start - length;
			int ti = t.Length - 1;
			int tend = -1;

			int sStep = start + 1;
			int tStep = t.Length;
			bool sourceConsumed, targetConsumed;
			while (true) {
				for (; send < si; si--)
					if (!IsIgnorable (s [si]))
						break;
				for (; tend < ti; ti--)
					if (!IsIgnorable (t [ti]))
						break;
				if (tend == ti)
					break;
				for (; send < si; si--)
					if (IsSafe (s [si]))
						break;
				for (; tend < ti; ti--)
					if (IsSafe (t [ti]))
						break;
Console.WriteLine ("==== {0} {1} {2} {3} {4} {5} {6} {7} {8}", s, si, send, length, t, ti, tstart, sStep - si, tStep - ti);
				if (CompareInternal (opt, s, si, sStep - si,
					t, ti, tStep - ti,
					out targetConsumed, out sourceConsumed,
					true) != 0)
					return false;
				if (send == si)
					return false;
				sStep = si;
				tStep = ti;
				si--;
				ti--;
			}
			return true;
#else
			// FIXME: it is not efficient for very long target.
			bool sourceConsumed, targetConsumed;
			int mismatchCount = 0;
			for (int i = 0; i < length; i++) {
				prev = new PreviousInfo (false); // prev.Reset();

				int ret = CompareInternal (opt, s, start - i, i + 1,
					t, tstart, t.Length - tstart,
					out targetConsumed,
					out sourceConsumed, true, ref prev,
					sk1, sk2);
				if (ret == 0)
					return true;
				if (!sourceConsumed && targetConsumed)
					return false;
				if (!targetConsumed) {
					mismatchCount++;
					if (mismatchCount > Uni.MaxExpansionLength)
						// The largest length of an
						// expansion is 3, so if the
						// target was not consumed more
						// than 3 times, then the tail
						// character does not match.
						return false;
				}
			}
			return false;
#endif
		}

		#endregion

		#region IndexOf() / LastIndexOf()

		public int IndexOf (string s, string target, CompareOptions opt)
		{
			return IndexOf (s, target, 0, s.Length, opt);
		}

		public int IndexOf (string s, string target, int start, int length, CompareOptions opt)
		{
			PreviousInfo prev = new PreviousInfo (false);
			byte [] checkedFlags = s.Length > 50 ? new byte [16] : null;
			byte [] targetSortKey = new byte [4];
			byte [] sk1 = new byte [4];
			byte [] sk2 = new byte [4];

			return IndexOf (opt, s, target, start, length,
				checkedFlags, targetSortKey, ref prev, sk1, sk2);
		}

		public int IndexOf (string s, char target, CompareOptions opt)
		{
			return IndexOf (s, target, 0, s.Length, opt);
		}

		public int IndexOf (string s, char target, int start, int length, CompareOptions opt)
		{
			PreviousInfo prev = new PreviousInfo (false);
			byte [] sk1 = new byte [4];
			byte [] sk2 = new byte [4];
			byte [] checkedFlags = s.Length > 50 ? new byte [16] : null;
			byte [] targetSortKey = new byte [4];

			// If target is contraction, then use string search.
			Contraction ct = GetContraction (target);
			if (ct != null) {
				if (ct.Replacement != null)
					return IndexOf (opt, s, ct.Replacement,
						start, length, checkedFlags, targetSortKey, ref prev, sk1, sk2);
				else
					return IndexOfSortKey (opt, s, start, length, ct.SortKey, char.MinValue, -1, true, checkedFlags, ref prev, sk1);
			} else {
				int ti = FilterOptions ((int) target, opt);
				targetSortKey [0] = Category (ti);
				targetSortKey [1] = Level1 (ti);
				if ((opt & COpt.IgnoreNonSpace) == 0)
					targetSortKey [2] =
						Level2 (ti, ExtenderType.None);
				targetSortKey [3] = Uni.Level3 (ti);
				return IndexOfSortKey (opt, s, start, length,
					targetSortKey, target, ti,
					!Uni.HasSpecialWeight ((char) ti), checkedFlags, ref prev, sk1);
			}
		}

		// Searches target byte[] keydata
		int IndexOfSortKey (COpt opt, string s, int start, int length, byte [] sortkey, char target, int ti, bool noLv4, byte [] checkedFlags, ref PreviousInfo prev, byte [] sk)
		{
			int end = start + length;
			int idx = start;

			while (idx < end) {
				int cur = idx;
				if (MatchesForward (opt, s, ref idx, end, ti, sortkey, noLv4, checkedFlags, ref prev, sk))
					return cur;
			}
			return -1;
		}

		// Searches string. Search head character (or keydata when
		// the head is contraction sortkey) and try IsPrefix().
		int IndexOf (COpt opt, string s, string target, int start, int length, byte [] checkedFlags, byte [] targetSortKey, ref PreviousInfo prev, byte [] sk1, byte [] sk2)
		{
			int tidx = 0;
			for (; tidx < target.Length; tidx++)
				if (!IsIgnorable (target [tidx], opt))
					break;
			if (tidx == target.Length)
				return start;
			Contraction ct = GetContraction (target, tidx, target.Length - tidx);
			string replace = ct != null ? ct.Replacement : null;
			byte [] sk = replace == null ? targetSortKey : null;
			bool noLv4 = true;
			char tc = char.MinValue;
			int ti = -1;
			if (ct != null && sk != null) {
				for (int i = 0; i < ct.SortKey.Length; i++)
					sk [i] = ct.SortKey [i];
			} else if (sk != null) {
				tc = target [tidx];
				ti = FilterOptions (target [tidx], opt);
				sk [0] = Category (ti);
				sk [1] = Level1 (ti);
				if ((opt & COpt.IgnoreNonSpace) == 0)
					sk [2] = Level2 (ti, ExtenderType.None);
				sk [3] = Uni.Level3 (ti);
				noLv4 = !Uni.HasSpecialWeight ((char) ti);
			}
			if (sk != null) {
				for (tidx++; tidx < target.Length; tidx++) {
					if (Category (target [tidx]) != 1)
						break;
					if (sk [2] == 0)
						sk [2] = 2;
					sk [2] = (byte) (sk [2] + Level2 (target [tidx], ExtenderType.None));
				}
			}

			do {
				int idx = 0;
				if (replace != null)
					idx = IndexOf (opt, s, replace, start, length, checkedFlags, targetSortKey, ref prev, sk1, sk2);
				else
					idx = IndexOfSortKey (opt, s, start, length, sk, tc, ti, noLv4, checkedFlags, ref prev, sk1);
				if (idx < 0)
					return -1;
				length -= idx - start;
				start = idx;
				if (IsPrefix (opt, s, target, start, length, false, ref prev, sk1, sk2))
					return idx;
				Contraction cts = GetContraction (s, start, length);
				if (cts != null) {
					start += cts.Source.Length;
					length -= cts.Source.Length;
				} else {
					start++;
					length--;
				}
			} while (length > 0);
			return -1;
		}

		//
		// There are the same number of IndexOf() related methods,
		// with the same functionalities for each.
		//

		public int LastIndexOf (string s, string target, CompareOptions opt)
		{
			return LastIndexOf (s, target, s.Length - 1, s.Length, opt);
		}

		public int LastIndexOf (string s, string target, int start, int length, CompareOptions opt)
		{
			PreviousInfo prev = new PreviousInfo (false);
			byte [] sk1 = new byte [4];
			byte [] sk2 = new byte [4];
			byte [] checkedFlags = s.Length > 50 ? new byte [16] : null;
			byte [] targetSortKey = new byte [4];
			return LastIndexOf (opt, s, target, start, length,
				checkedFlags, targetSortKey, ref prev, sk1, sk2);
		}

		public int LastIndexOf (string s, char target, CompareOptions opt)
		{
			return LastIndexOf (s, target, s.Length - 1, s.Length, opt);
		}

		public int LastIndexOf (string s, char target, int start, int length, CompareOptions opt)
		{
			PreviousInfo prev = new PreviousInfo (false);
			byte [] sk1 = new byte [4];
			byte [] sk2 = new byte [4];
			byte [] checkedFlags = s.Length > 50 ? new byte [16] : null;
			byte [] targetSortKey = new byte [4];

			// If target is contraction, then use string search.
			Contraction ct = GetContraction (target);
			if (ct != null) {
				if (ct.Replacement != null)
					return LastIndexOf (opt, s,
						ct.Replacement, start, length,
						checkedFlags, targetSortKey, ref prev, sk1, sk2);
				else
					return LastIndexOfSortKey (opt, s, start,
						start, length, ct.SortKey,
						char.MinValue, -1, true,
						checkedFlags, ref prev, sk1);
			}
			else {
				int ti = FilterOptions ((int) target, opt);
				targetSortKey [0] = Category (ti);
				targetSortKey [1] = Level1 (ti);
				if ((opt & COpt.IgnoreNonSpace) == 0)
					targetSortKey [2] = Level2 (ti, ExtenderType.None);
				targetSortKey [3] = Uni.Level3 (ti);
				return LastIndexOfSortKey (opt, s, start, start,
					length, targetSortKey, target,
					ti, !Uni.HasSpecialWeight ((char) ti),
					checkedFlags, ref prev, sk1);
			}
		}

		// Searches target byte[] keydata
		int LastIndexOfSortKey (COpt opt, string s, int start, int orgStart, int length, byte [] sortkey, char target, int ti, bool noLv4, byte [] checkedFlags, ref PreviousInfo prev, byte [] sk)
		{
			int end = start - length;
			int idx = start;
			while (idx > end) {
				int cur = idx;
				if (MatchesBackward (opt, s, ref idx, end, orgStart,
					ti, sortkey, noLv4, checkedFlags, ref prev, sk))
					return cur;
			}
			return -1;
		}

		// Searches string. Search head character (or keydata when
		// the head is contraction sortkey) and try IsPrefix().
		int LastIndexOf (COpt opt, string s, string target, int start, int length, byte [] checkedFlags, byte [] targetSortKey, ref PreviousInfo prev, byte [] sk1, byte [] sk2)
		{
			int orgStart = start;
			int tidx = 0;
			for (; tidx < target.Length; tidx++)
				if (!IsIgnorable (target [tidx], opt))
					break;
			if (tidx == target.Length)
				return start;
			Contraction ct = GetContraction (target, tidx, target.Length - tidx);
			string replace = ct != null ? ct.Replacement : null;
			byte [] sk = replace == null ? targetSortKey : null;

			bool noLv4 = true;
			char tc = char.MinValue;
			int ti = -1;
			if (ct != null && sk != null) {
				for (int i = 0; i < ct.SortKey.Length; i++)
					sk [i] = ct.SortKey [i];
			} else if (sk != null) {
				tc = target [tidx];
				ti = FilterOptions (target [tidx], opt);
				sk [0] = Category (ti);
				sk [1] = Level1 (ti);
				if ((opt & COpt.IgnoreNonSpace) == 0)
					sk [2] = Level2 (ti, ExtenderType.None);
				sk [3] = Uni.Level3 (ti);
				noLv4 = !Uni.HasSpecialWeight ((char) ti);
			}
			if (sk != null) {
				for (tidx++; tidx < target.Length; tidx++) {
					if (Category (target [tidx]) != 1)
						break;
					if (sk [2] == 0)
						sk [2] = 2;
					sk [2] = (byte) (sk [2] + Level2 (target [tidx], ExtenderType.None));
				}
			}

			do {
				int idx = 0;

				if (replace != null)
					idx = LastIndexOf (opt, s, replace,
						start, length, checkedFlags,
						targetSortKey, ref prev, sk1, sk2);
				else
					idx = LastIndexOfSortKey (opt, s, start, orgStart, length, sk, tc, ti, noLv4, checkedFlags, ref prev, sk1);
				if (idx < 0)
					return -1;
				length -= start - idx;
				start = idx;

				if (IsPrefix (opt, s, target, idx, orgStart - idx + 1, false, ref prev, sk1, sk2)) {
					for (;idx < orgStart; idx++)
						if (!IsIgnorable (s [idx], opt))
							break;
					return idx;
				}
				Contraction cts = GetContraction (s, idx, orgStart - idx + 1);
				if (cts != null) {
					start -= cts.Source.Length;
					length -= cts.Source.Length;
				} else {
					start--;
					length--;
				}
			} while (length > 0);
			return -1;
		}

		private bool MatchesForward (COpt opt, string s, ref int idx, int end, int ti, byte [] sortkey, bool noLv4, byte [] checkedFlags, ref PreviousInfo prev, byte [] sk)
		{
			int si = s [idx];
			if (checkedFlags != null && si < 128 && (checkedFlags [si / 8] & (1 << (si % 8))) != 0) {
				idx++;
				return false;
			}
			ExtenderType ext = GetExtenderType (s [idx]);
			Contraction ct = null;
			if (MatchesForwardCore (opt, s, ref idx, end, ti, sortkey, noLv4, ext, ref ct, checkedFlags, ref prev, sk))
				return true;
			if (checkedFlags != null && ct == null && ext == ExtenderType.None && si < 128) {
				checkedFlags [si / 8] |= (byte) (1 << (si % 8));
			}
			return false;
		}

		private bool MatchesForwardCore (COpt opt, string s, ref int idx, int end, int ti, byte [] sortkey, bool noLv4, ExtenderType ext, ref Contraction ct, byte [] checkedFlags, ref PreviousInfo prev, byte [] charSortKey)
		{
			bool ignoreNonSpace = (opt & COpt.IgnoreNonSpace) != 0;
			int si = -1;
			if (ext == ExtenderType.None)
				ct = GetContraction (s, idx, end);
			else if (prev.Code < 0) {
				if (prev.SortKey == null) {
					idx++;
					return false;
				}
				charSortKey = prev.SortKey;
			}
			else
				si = FilterExtender (prev.Code, ext, opt);
			// if lv4 exists, it never matches contraction
			if (ct != null) {
				idx += ct.Source.Length;
				if (!noLv4)
					return false;
				if (ct.SortKey != null) {
					for (int i = 0; i < sortkey.Length; i++)
						charSortKey [i] = sortkey [i];
					prev.Code = -1;
					prev.SortKey = charSortKey;
				} else {
					// Here is the core of LAMESPEC
					// described at the top of the source.
					int dummy = 0;
					return MatchesForward (opt, ct.Replacement, ref dummy,
						ct.Replacement.Length, ti, sortkey, noLv4, checkedFlags, ref prev, charSortKey);
				}
			} else {
				if (si < 0)
					si = FilterOptions (s [idx], opt);
				idx++;
				charSortKey [0] = Category (si);
				bool noMatch = false;
				if (sortkey [0] == charSortKey [0])
					charSortKey [1] = Level1 (si);
				else
					noMatch = true;
				if (!ignoreNonSpace && sortkey [1] == charSortKey [1])
					charSortKey [2] = Level2 (si, ext);
				else if (!ignoreNonSpace)
					noMatch = true;
				if (noMatch) {
					for (; idx < end; idx++) {
						if (Category (s [idx]) != 1)
							break;
					}
					return false;
				}
				charSortKey [3] = Uni.Level3 (si);
				if (charSortKey [0] != 1)
					prev.Code = si;
			}
			for (; idx < end; idx++) {
				if (Category (s [idx]) != 1)
					break;
				if (ignoreNonSpace)
					continue;
				if (charSortKey [2] == 0)
						charSortKey [2] = 2;
					charSortKey [2] = (byte) (charSortKey [2]
						+ Level2 (s [idx], ExtenderType.None));
			}

			return MatchesPrimitive (opt, charSortKey, si, ext, sortkey, ti, noLv4);
		}

		private bool MatchesPrimitive (COpt opt, byte [] source, int si, ExtenderType ext, byte [] target, int ti, bool noLv4)
		{
			bool ignoreNonSpace = (opt & COpt.IgnoreNonSpace) != 0;
			if (source [0] != target [0] ||
				source [1] != target [1] ||
				(!ignoreNonSpace && source [2] != target [2]) ||
				source [3] != target [3])
				return false;
			if (noLv4 && (si < 0 || !Uni.HasSpecialWeight ((char) si)))
				return true;
			else if (noLv4)
				return false;
			// Since target can never be an extender, if the source
			// is an expander and it matters, then they never match.
			if (!ignoreNonSpace && ext == ExtenderType.Conditional)
				return false;
			if (Uni.IsJapaneseSmallLetter ((char) si) !=
				Uni.IsJapaneseSmallLetter ((char) ti) ||
				ToDashTypeValue (ext, opt) !=
				// FIXME: we will have to specify correct value for target
				ToDashTypeValue (ExtenderType.None, opt) ||
				!Uni.IsHiragana ((char) si) !=
				!Uni.IsHiragana ((char) ti) ||
				IsHalfKana ((char) si, opt) !=
				IsHalfKana ((char) ti, opt))
				return false;
			return true;
		}

		private bool MatchesBackward (COpt opt, string s, ref int idx, int end, int orgStart, int ti, byte [] sortkey, bool noLv4, byte [] checkedFlags, ref PreviousInfo prev, byte [] sk)
		{
			int si = s [idx];
			if (checkedFlags != null && si < 128 && (checkedFlags [si / 8] & (1 << (si % 8))) != 0) {
				idx--;
				return false;
			}
			ExtenderType ext = GetExtenderType (s [idx]);
			Contraction ct = null;
			if (MatchesBackwardCore (opt, s, ref idx, end, orgStart, ti, sortkey, noLv4, ext, ref ct, checkedFlags, ref prev, sk))
				return true;
			if (checkedFlags != null && ct == null && ext == ExtenderType.None && si < 128) {
				checkedFlags [si / 8] |= (byte) (1 << (si % 8));
			}
			return false;
		}

		private bool MatchesBackwardCore (COpt opt, string s, ref int idx, int end, int orgStart, int ti, byte [] sortkey, bool noLv4, ExtenderType ext, ref Contraction ct, byte [] checkedFlags, ref PreviousInfo prev, byte [] charSortKey)
		{
			bool ignoreNonSpace = (opt & COpt.IgnoreNonSpace) != 0;
			int cur = idx;
			int si = -1;
			// To handle extenders in source, we need to
			// check next _primary_ character.
			if (ext != ExtenderType.None) {
				byte diacritical = 0;
				for (int tmp = 0; ; tmp--) {
					if (tmp < 0) // heading extender
						return false;
					if (IsIgnorable (s [tmp], opt))
						continue;
					int tmpi = FilterOptions (s [tmp], opt);
					byte category = Category (tmpi);
					if (category == 1) {
						diacritical = Level2 (tmpi, ExtenderType.None);
						continue;
					}
					si = FilterExtender (tmpi, ext, opt);
					charSortKey [0] = category;
					charSortKey [1] = Level1 (si);
					if (!ignoreNonSpace)
						charSortKey [2] = Level2 (si, ext);
					charSortKey [3] = Uni.Level3 (si);
					if (ext != ExtenderType.Conditional &&
						diacritical != 0)
						charSortKey [2] =
							(charSortKey [2] == 0) ?
							(byte) (diacritical + 2) :
							diacritical;
					break;
				}
				idx--;
			}
			if (ext == ExtenderType.None)
				ct = GetContraction (s, idx, end);
			// if lv4 exists, it never matches contraction
			if (ct != null) {
				idx -= ct.Source.Length;
				if (!noLv4)
					return false;
				if (ct.SortKey != null) {
					for (int i = 0; i < sortkey.Length; i++)
						charSortKey [i] = sortkey [i];
					prev.Code = -1;
					prev.SortKey = charSortKey;
				} else {
					// Here is the core of LAMESPEC
					// described at the top of the source.
					int dummy = ct.Replacement.Length - 1;
					return MatchesBackward (opt, 
						ct.Replacement, ref dummy,
						dummy, -1, ti, sortkey, noLv4,
						checkedFlags, ref prev, charSortKey);
				}
			} else if (ext == ExtenderType.None) {
				if (si < 0)
					si = FilterOptions (s [idx], opt);
				idx--;
				bool noMatch = false;
				charSortKey [0] = Category (si);
				if (charSortKey [0] == sortkey [0])
					charSortKey [1] = Level1 (si);
				else
					noMatch = true;
				if (!ignoreNonSpace && charSortKey [1] == sortkey [1])
					charSortKey [2] = Level2 (si, ext);
				else if (!ignoreNonSpace)
					noMatch = true;
				if (noMatch)
					return false;
				charSortKey [3] = Uni.Level3 (si);
				if (charSortKey [0] != 1)
					prev.Code = si;
			}
			if (ext == ExtenderType.None) {
				for (int tmp = cur + 1; tmp < orgStart; tmp++) {
					if (Category (s [tmp]) != 1)
						break;
					if (ignoreNonSpace)
						continue;
					if (charSortKey [2] == 0)
							charSortKey [2] = 2;
					charSortKey [2] = (byte) (charSortKey [2]
						+ Level2 (s [tmp], ExtenderType.None));
				}
			}
			return MatchesPrimitive (opt, charSortKey, si, ext, sortkey, ti, noLv4);
		}
		#endregion
	}
}

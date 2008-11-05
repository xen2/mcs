//
// UCD.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.
//

//
// Unicode table generator for eglib.
// Note that this code is only for Unicode 5.1.0 or earlier.
// (regarding character ranges)
//
// Some premises:
// - lower-band (0000-FFFF) characters never has case mapping to higher-band
//   characters. Hence, simple upper/lower mapping is divided into 16-bit and
//   32-bit tables.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Mono.Globalization.Unicode
{
	public class Driver
	{
		public static void Main (string [] args)
		{
			TextWriter w = Console.Out;
			w.NewLine = "\n";

			w.WriteLine (@"/*
DO NOT MODIFY THIS FILE DIRECTLY.

This file is automatically generated by {0}.exe.
The source for this generator should be in Mono repository
(mcs/class/corlib/Mono.Globalization.Unicode directory).
*/

#ifndef __UNICODE_DATA_H
#define __UNICODE_DATA_H

#include <glib.h>

", Assembly.GetEntryAssembly ().GetName ().Name);
			var ud = new UnicodeData5_1_0 ();
			var ucd = ud.ParseFile (args [0]);
			var ucg = new UnicodeDataCodeGeneratorC5_1_0 (ud, w);
			ucg.GenerateStructures ();
			w.WriteLine ();
			ucg.GenerateUnicodeCategoryListC (ucd);
			w.WriteLine ();
			ucg.GenerateSimpleCaseMappingListC (ucd);
			w.WriteLine ();
			ucg.GenerateSimpleTitlecaseMappingListC (ucd);
			w.WriteLine (@"
#endif
");
		}
	}

	public class UnicodeData5_1_0 : UnicodeData
	{
		public override CodePointRange [] SimpleCases {
			get { return simple_cases; }
		}

		public override CodePointRange [] CategoryRanges {
			get { return category_ranges; }
		}

		static readonly CodePointRange [] simple_cases = {
			new CodePointRange (0x0040, 0x0600),
			new CodePointRange (0x1000, 0x10D0),
			new CodePointRange (0x1D00, 0x2000),
			new CodePointRange (0x2100, 0x21C0),
			new CodePointRange (0x2480, 0x2500),
			new CodePointRange (0x2C00, 0x2D80),
			new CodePointRange (0xA640, 0xA7C0),
			new CodePointRange (0xFF20, 0xFF80),
			new CodePointRange (0x10400, 0x10480),
			};

		static readonly CodePointRange [] category_ranges = {
			new CodePointRange (0x0000, 0x3400),
			// 3400-4DB5: OtherLetter
			new CodePointRange (0x4DC0, 0x4E00),
			// 4E00-9FC3: OtherLetter
			new CodePointRange (0xA000, 0xAA80),
			// AC00-D7A3: OtherLetter
			// D800-DFFF: OtherSurrogate
			// E000-F8FF: OtherPrivateUse
			new CodePointRange (0xF900, 0x10000),
			new CodePointRange (0x10000, 0x104C0),
			new CodePointRange (0x10800, 0x10A80),
			new CodePointRange (0x12000, 0x12480),
			new CodePointRange (0x1D000, 0x1D800),
			new CodePointRange (0x1F000, 0x1F0C0),
			// 20000-2A6D6 OtherLetter
			new CodePointRange (0x2F800, 0x2FA40),
			new CodePointRange (0xE0000, 0xE0200),
			// F0000-FFFFD OtherPrivateUse
			// 100000-10FFFD OtherPrivateUse
			};
	}

	public abstract class UnicodeData
	{
		public abstract CodePointRange [] SimpleCases { get; }

		public abstract CodePointRange [] CategoryRanges { get; }

		public virtual UcdCharacterProperty [] ParseFile (string file)
		{
			var d = new List<KeyValuePair<int,UcdCharacterProperty>> ();

			using (TextReader r = File.OpenText (file)) {
				while (r.Peek () >= 0) {
					var l = r.ReadLine ();
					if (l.Length > 0 && l [0] != '#') {
						var u = Parse (l);
						d.Add (new KeyValuePair<int,UcdCharacterProperty> (u.Codepoint, u));
					}
				}
			}
			var list = new List<UcdCharacterProperty> ();
			foreach (var p in d)
				list.Add (p.Value);
			return list.ToArray ();
		}

		UcdCharacterProperty Parse (string line)
		{
			string [] tokens = line.Split (';');
			string [] decomp = tokens [5].Length > 0 ? tokens [5].Split (' ') : null;
			string decomp_type = decomp != null && decomp [0] [0] == '<' ? decomp [0] : null;
			if (decomp_type != null) {
				for (int i = 1; i < decomp.Length; i++)
					decomp [i - 1] = decomp [i];
				Array.Resize (ref decomp, decomp.Length - 1);
			}

			return new UcdCharacterProperty () {
				Codepoint = int.Parse (tokens [0], NumberStyles.HexNumber),
				Name = tokens [1],
				Category = ParseUnicodeCategory (tokens [2]),
				CanonicalCombiningClass = tokens [3].Length > 0 ? (byte?) byte.Parse (tokens [3]) : null,
				BidiClass = tokens [4].Length > 0 ? (UcdBidiClass) Enum.Parse (typeof (UcdBidiClass), tokens [4]) : UcdBidiClass.None,
				DecompositionType = decomp_type != null ? ParseDecompositionType (decomp_type) : UcdDecompositionType.None,
				DecompositionMapping = decomp != null ? Array.ConvertAll<string,int> (decomp, dv => int.Parse (dv, NumberStyles.HexNumber)) : null,
				DecimalDigitValue = tokens [6],
				DigitValue = tokens [7],
				NumericValue = tokens [8],
				BidiMirrored = (tokens [9] == "Y"),
				Unicode1Name = tokens [10],
				IsoComment = tokens [11],
				SimpleUppercaseMapping = tokens [12].Length > 0 ? int.Parse (tokens [12], NumberStyles.HexNumber) : 0,
				SimpleLowercaseMapping = tokens [13].Length > 0 ? int.Parse (tokens [13], NumberStyles.HexNumber) : 0,
				SimpleTitlecaseMapping = tokens [14].Length > 0 ? int.Parse (tokens [14], NumberStyles.HexNumber) : 0,
				};
		}

		UcdDecompositionType ParseDecompositionType (string s)
		{
			switch (s) {
			case "<font>":
				return UcdDecompositionType.Font;
			case "<noBreak>":
				return UcdDecompositionType.NoBreak;
			case "<initial>":
				return UcdDecompositionType.Initial;
			case "<medial>":
				return UcdDecompositionType.Medial;
			case "<final>":
				return UcdDecompositionType.Final;
			case "<isolated>":
				return UcdDecompositionType.Isolated;
			case "<circle>":
				return UcdDecompositionType.Circle;
			case "<super>":
				return UcdDecompositionType.Super;
			case "<sub>":
				return UcdDecompositionType.Sub;
			case "<vertical>":
				return UcdDecompositionType.Vertical;
			case "<wide>":
				return UcdDecompositionType.Wide;
			case "<narrow>":
				return UcdDecompositionType.Narrow;
			case "<small>":
				return UcdDecompositionType.Small;
			case "<square>":
				return UcdDecompositionType.Square;
			case "<fraction>":
				return UcdDecompositionType.Fraction;
			case "<compat>":
				return UcdDecompositionType.Compat;
			}
			throw new ArgumentException (String.Format ("Unexpected decomposition type '{0}'", s));
		}

		UnicodeCategory ParseUnicodeCategory (string s)
		{
			switch (s) {
			case "Lu":
				return UnicodeCategory.UppercaseLetter;
			case "Ll":
				return UnicodeCategory.LowercaseLetter;
			case "Lt":
				return UnicodeCategory.TitlecaseLetter;
			case "Lm":
				return UnicodeCategory.ModifierLetter;
			case "Lo":
				return UnicodeCategory.OtherLetter;
			case "Mn":
				return UnicodeCategory.NonSpacingMark;
			case "Mc":
				return UnicodeCategory.SpacingCombiningMark;
			case "Me":
				return UnicodeCategory.EnclosingMark;
			case "Nd":
				return UnicodeCategory.DecimalDigitNumber;
			case "Nl":
				return UnicodeCategory.LetterNumber;
			case "No":
				return UnicodeCategory.OtherNumber;
			case "Pc":
				return UnicodeCategory.ConnectorPunctuation;
			case "Pd":
				return UnicodeCategory.DashPunctuation;
			case "Ps":
				return UnicodeCategory.OpenPunctuation;
			case "Pe":
				return UnicodeCategory.ClosePunctuation;
			case "Pi":
				return UnicodeCategory.InitialQuotePunctuation;
			case "Pf":
				return UnicodeCategory.FinalQuotePunctuation;
			case "Po":
				return UnicodeCategory.OtherPunctuation;
			case "Sm":
				return UnicodeCategory.MathSymbol;
			case "Sc":
				return UnicodeCategory.CurrencySymbol;
			case "Sk":
				return UnicodeCategory.ModifierSymbol;
			case "So":
				return UnicodeCategory.OtherSymbol;
			case "Zs":
				return UnicodeCategory.SpaceSeparator;
			case "Zl":
				return UnicodeCategory.LineSeparator;
			case "Zp":
				return UnicodeCategory.ParagraphSeparator;
			case "Cc":
				return UnicodeCategory.Control;
			case "Cf":
				return UnicodeCategory.Format;
			case "Cs":
				return UnicodeCategory.Surrogate;
			case "Co":
				return UnicodeCategory.PrivateUse;
			case "Cn":
				return UnicodeCategory.OtherNotAssigned;
			}
			throw new ArgumentException (String.Format ("Unexpected category {0}", s));
		}
	}

	public class UnicodeDataCodeGeneratorC5_1_0
	{
		UnicodeData catalog;
		TextWriter w;

		public UnicodeDataCodeGeneratorC5_1_0 (UnicodeData catalog, TextWriter writer)
		{
			this.catalog = catalog;
			w = writer;
		}

		public void GenerateStructures ()
		{
			w.WriteLine ("/* ======== Structures ======== */");
			w.WriteLine (@"typedef struct {
	guint32 codepoint;
	guint32 upper;
	guint32 title;
} SimpleTitlecaseMapping;");
			w.WriteLine (@"typedef struct {
	guint32 start;
	guint32 end;
} CodePointRange;");
			w.WriteLine (@"typedef struct {
	guint32 upper;
	guint32 lower;
} SimpleCaseMapping;");
		}

		void GenerateCodePointRanges (string name, CodePointRange [] ranges)
		{
			w.WriteLine ("static const guint8 {0}_count = {1};", name, ranges.Length);
			w.WriteLine ("static const CodePointRange {0} [] = {{", name);
			foreach (var cpr in ranges)
				w.WriteLine ("{{0x{0:X06}, 0x{1:X06}}},", cpr.Start, cpr.End);
			w.WriteLine ("{0, 0}};");
		}

		public void GenerateUnicodeCategoryListC (UcdCharacterProperty [] ucd)
		{
			w.WriteLine ("/* ======== Unicode Categories ======== */");
			GenerateCodePointRanges ("unicode_category_ranges", catalog.CategoryRanges);

			int table = 0;
			foreach (var cpr in catalog.CategoryRanges) {
				w.WriteLine ("const GUnicodeType unicode_category_table{0} [] = {{", table);
				w.WriteLine ("\t/* ==== {0:X}-{1:X} ==== */", cpr.Start, cpr.End);
				w.Write ("\t");
				int cp = cpr.Start;
				foreach (var ucp in ucd) {
					if (ucp.Codepoint >= cpr.End)
						break;
					if (ucp.Codepoint < cp)
						continue;
					while (cp < ucp.Codepoint) {
						w.Write ("0,");
						if (++cp % 16 == 0)
//							w.Write ("\n/* ==== {0:X} ==== */\n\t", cp);
							w.Write ("\n\t", cp);
					}
					w.Write ((int) ToGUnicodeCategory (ucp.Category));
					w.Write (',');
					if (++cp % 16 == 0)
//						w.Write ("\n/* ==== {0:X} ==== */\n\t", cp);
						w.Write ("\n\t", cp);
					if (cp >= cpr.End)
						break;
				}
				w.WriteLine ("0};");
				table++;
			}

			w.WriteLine ("static const GUnicodeType *unicode_category [{0}]  = {{", catalog.CategoryRanges.Length);
			for (int i = 0, end = catalog.CategoryRanges.Length; i < end; i++)
				w.WriteLine ("\tunicode_category_table{0}{1}", i, i + 1 < end ? "," : String.Empty);
			w.WriteLine ("};");
		}

		public void GenerateSimpleTitlecaseMappingListC (UcdCharacterProperty [] ucd)
		{
			w.WriteLine ("static const SimpleTitlecaseMapping simple_titlecase_mapping [] = {");
			int count = 0;
			foreach (var ucp in ucd) {
				if (ucp.SimpleUppercaseMapping == ucp.SimpleTitlecaseMapping)
					continue;
				if (count > 0)
					w.WriteLine (',');
				w.Write ("\t{{0x{0:X06}, 0x{1:X06}, 0x{2:X06}}}", ucp.Codepoint, ucp.SimpleUppercaseMapping, ucp.SimpleTitlecaseMapping);
				count++;
			}
			w.WriteLine ();
			w.WriteLine ("};");
			w.WriteLine ("static const guint8 simple_titlecase_mapping_count = {0};", count);
		}

		public void GenerateSimpleCaseMappingListC (UcdCharacterProperty [] ucd)
		{
			GenerateCodePointRanges ("simple_case_map_ranges", catalog.SimpleCases);
			GenerateSimpleCaseMappingListC (ucd, true, true);
			GenerateSimpleCaseMappingListC (ucd, true, false);
			GenerateSimpleCaseMappingListC (ucd, false, true);
			GenerateSimpleCaseMappingListC (ucd, false, false);
		}

		void GenerateSimpleCaseMappingListC (UcdCharacterProperty [] ucd, bool upper, bool small)
		{
			int nTable = 0;
			foreach (var cpr in catalog.SimpleCases) {
				if (small && cpr.Start > 0xFFFF)
					break;
				if (!small && cpr.Start < 0x10000)
					continue;

				w.WriteLine ("static const {0} simple_{1}_case_mapping_{2}_table{3} [] = {{", small ? "guint16" : "guint32", upper ? "upper" : "lower", small ? "lowarea" : "higharea", nTable);


				w.WriteLine ("\t/* ==== {0:X}-{1:X} ==== */", cpr.Start, cpr.End);
				w.Write ("\t");
				int cp = cpr.Start;
				foreach (var ucp in ucd) {
					if (ucp.Codepoint >= cpr.End)
						break;
					if (ucp.Codepoint < cp)
						continue;
					while (cp < ucp.Codepoint) {
						w.Write ("0,");
						if (++cp % 16 == 0)
							w.WriteLine ();
					}
					int v = upper ? ucp.SimpleUppercaseMapping : ucp.SimpleLowercaseMapping;
					if (v != 0)
						w.Write ("0x{0:X},", v);
					else
						w.Write ("0,");

					if (++cp % 16 == 0) {
						w.WriteLine ();
						w.Write ("\t");
					}
					if (cp >= cpr.End)
						break;
				}
				w.WriteLine ("0};");

				nTable++;
			}

			w.WriteLine ("static const {0} *simple_{1}_case_mapping_{2} [] = {{", small ? "guint16" : "guint32", upper ? "upper" : "lower", small ? "lowarea" : "higharea");

			for (int i = 0; i < nTable; i++) {
				if (i > 0)
					w.WriteLine (",");
				w.Write ("\tstatic const guint8 simple_{0}_case_mapping_{1}_table{2}", upper ? "upper" : "lower", small ? "lowarea" : "higharea", i);
			}

			w.WriteLine ("};");
			w.WriteLine ();
		}

		enum GUnicodeType
		{
			G_UNICODE_CONTROL,
			G_UNICODE_FORMAT,
			G_UNICODE_UNASSIGNED,
			G_UNICODE_PRIVATE_USE,
			G_UNICODE_SURROGATE,
			G_UNICODE_LOWERCASE_LETTER,
			G_UNICODE_MODIFIER_LETTER,
			G_UNICODE_OTHER_LETTER,
			G_UNICODE_TITLECASE_LETTER,
			G_UNICODE_UPPERCASE_LETTER,
			G_UNICODE_COMBINING_MARK,
			G_UNICODE_ENCLOSING_MARK,
			G_UNICODE_NON_SPACING_MARK,
			G_UNICODE_DECIMAL_NUMBER,
			G_UNICODE_LETTER_NUMBER,
			G_UNICODE_OTHER_NUMBER,
			G_UNICODE_CONNECT_PUNCTUATION,
			G_UNICODE_DASH_PUNCTUATION,
			G_UNICODE_CLOSE_PUNCTUATION,
			G_UNICODE_FINAL_PUNCTUATION,
			G_UNICODE_INITIAL_PUNCTUATION,
			G_UNICODE_OTHER_PUNCTUATION,
			G_UNICODE_OPEN_PUNCTUATION,
			G_UNICODE_CURRENCY_SYMBOL,
			G_UNICODE_MODIFIER_SYMBOL,
			G_UNICODE_MATH_SYMBOL,
			G_UNICODE_OTHER_SYMBOL,
			G_UNICODE_LINE_SEPARATOR,
			G_UNICODE_PARAGRAPH_SEPARATOR,
			G_UNICODE_SPACE_SEPARATOR
		}

		GUnicodeType ToGUnicodeCategory (UnicodeCategory v)
		{
			switch (v) {
			case UnicodeCategory.UppercaseLetter:
				return GUnicodeType.G_UNICODE_UPPERCASE_LETTER;
			case UnicodeCategory.LowercaseLetter:
				return GUnicodeType.G_UNICODE_LOWERCASE_LETTER;
			case UnicodeCategory.TitlecaseLetter:
				return GUnicodeType.G_UNICODE_TITLECASE_LETTER;
			case UnicodeCategory.ModifierLetter:
				return GUnicodeType.G_UNICODE_MODIFIER_LETTER;
			case UnicodeCategory.OtherLetter:
				return GUnicodeType.G_UNICODE_OTHER_LETTER;
			case UnicodeCategory.NonSpacingMark:
				return GUnicodeType.G_UNICODE_NON_SPACING_MARK;
			case UnicodeCategory.SpacingCombiningMark:
				return GUnicodeType.G_UNICODE_COMBINING_MARK;
			case UnicodeCategory.EnclosingMark:
				return GUnicodeType.G_UNICODE_ENCLOSING_MARK;
			case UnicodeCategory.DecimalDigitNumber:
				return GUnicodeType.G_UNICODE_DECIMAL_NUMBER;
			case UnicodeCategory.LetterNumber:
				return GUnicodeType.G_UNICODE_LETTER_NUMBER;
			case UnicodeCategory.OtherNumber:
				return GUnicodeType.G_UNICODE_OTHER_NUMBER;
			case UnicodeCategory.ConnectorPunctuation:
				return GUnicodeType.G_UNICODE_CONNECT_PUNCTUATION;
			case UnicodeCategory.DashPunctuation:
				return GUnicodeType.G_UNICODE_DASH_PUNCTUATION;
			case UnicodeCategory.OpenPunctuation:
				return GUnicodeType.G_UNICODE_OPEN_PUNCTUATION;
			case UnicodeCategory.ClosePunctuation:
				return GUnicodeType.G_UNICODE_CLOSE_PUNCTUATION;
			case UnicodeCategory.InitialQuotePunctuation:
				return GUnicodeType.G_UNICODE_INITIAL_PUNCTUATION;
			case UnicodeCategory.FinalQuotePunctuation:
				return GUnicodeType.G_UNICODE_FINAL_PUNCTUATION;
			case UnicodeCategory.OtherPunctuation:
				return GUnicodeType.G_UNICODE_OTHER_PUNCTUATION;
			case UnicodeCategory.MathSymbol:
				return GUnicodeType.G_UNICODE_MATH_SYMBOL;
			case UnicodeCategory.CurrencySymbol:
				return GUnicodeType.G_UNICODE_CURRENCY_SYMBOL;
			case UnicodeCategory.ModifierSymbol:
				return GUnicodeType.G_UNICODE_MODIFIER_SYMBOL;
			case UnicodeCategory.OtherSymbol:
				return GUnicodeType.G_UNICODE_OTHER_SYMBOL;
			case UnicodeCategory.SpaceSeparator:
				return GUnicodeType.G_UNICODE_SPACE_SEPARATOR;
			case UnicodeCategory.LineSeparator:
				return GUnicodeType.G_UNICODE_LINE_SEPARATOR;
			case UnicodeCategory.ParagraphSeparator:
				return GUnicodeType.G_UNICODE_PARAGRAPH_SEPARATOR;
			case UnicodeCategory.Control:
				return GUnicodeType.G_UNICODE_CONTROL;
			case UnicodeCategory.Format:
				return GUnicodeType.G_UNICODE_FORMAT;
			case UnicodeCategory.Surrogate:
				return GUnicodeType.G_UNICODE_SURROGATE;
			case UnicodeCategory.PrivateUse:
				return GUnicodeType.G_UNICODE_PRIVATE_USE;
			case UnicodeCategory.OtherNotAssigned:
				return GUnicodeType.G_UNICODE_UNASSIGNED;
			}
			throw new ArgumentException (String.Format ("Unexpected category {0}", v));
		}
	}

	public class CodePointRange
	{
		public CodePointRange (int start, int end)
		{
			Start = start;
			End = end;
		}

		public int Start { get; set; }
		public int End { get; set; }
	}

	public class UcdCharacterProperty
	{
		public int Codepoint { get; set; }
		public string Name { get; set; }
		public UnicodeCategory Category { get; set; }
		public byte? CanonicalCombiningClass { get; set; }
		public UcdBidiClass BidiClass { get; set; }
		public UcdDecompositionType DecompositionType { get; set; }
		public int [] DecompositionMapping { get; set; }
		public string DecimalDigitValue { get; set; }
		public string DigitValue { get; set; }
		public string NumericValue { get; set; }
		public bool BidiMirrored { get; set; }
		public string Unicode1Name { get; set; }
		public string IsoComment { get; set; }
		public int SimpleUppercaseMapping { get; set; }
		public int SimpleLowercaseMapping { get; set; }
		public int SimpleTitlecaseMapping { get; set; }
	}

	public enum UcdBidiClass
	{
		None,
		L,
		LRE,
		LRO,
		R,
		AL,
		RLE,
		RLO,
		PDF,
		EN,
		ES,
		ET,
		AN,
		CS,
		NSM,
		BN,
		B,
		S,
		WS,
		ON
	}

	public enum UcdDecompositionType
	{
		None,
		Font,
		NoBreak,
		Initial,
		Medial,
		Final,
		Isolated,
		Circle,
		Super,
		Sub,
		Vertical,
		Wide,
		Narrow,
		Small,
		Square,
		Fraction,
		Compat
	}
}

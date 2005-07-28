using System;
using System.Globalization;
using System.Text;

namespace Mono.Globalization.Unicode
{
	internal /*static*/ class NormalizationTableUtil
	{
		public static readonly CodePointIndexer prop;
		public static readonly CodePointIndexer map;
		public static readonly CodePointIndexer Combining;

		static NormalizationTableUtil ()
		{
			int [] propStarts = new int [] {
				0, 0xAC00, 0xF900, 0x1D100,
				0x2f800, 0x2fa10
				};
			int [] propEnds = new int [] {
				0x3400, 0xD7AF, 0x10000, 0x1D800,
				0x2f810, 0x2fa20
				};
			int [] mapStarts = new int [] {
				0, 0xF900, 0x1d150, 0x2f800
				};
			int [] mapEnds = new int [] {
				0x3400, 0x10000, 0x1d800, 0x2fb00
				};
			int [] combiningStarts = new int [] {
				0x0290, 0x0480, 0x0590, 0x0930, 0x09B0,
				0x0A30, 0x0AB0, 0x0B30, 0x0BC0, 0x0C40,
				0x0CB0, 0x0D40, 0x0DC0, 0x0E30, 0x0EB0,
				0x0F00, 0x1030, 0x1350, 0x1710, 0x17D0,
				0x18A0, 0x1930, 0x1A10, 0x1DC0, 0x20D0,
				0x3020, 0x3090, 0xA800, 0xFB10, 0xFE20,
				0x10A00, 0x1D160, 0x1D240
				};
			int [] combiningEnds = new int [] {
				0x0360, 0x0490, 0x0750, 0x0960, 0x09D0,
				0x0A50, 0x0AD0, 0x0B50, 0x0BD0, 0x0C60,
				0x0CD0, 0x0D50, 0x0DD0, 0x0E50, 0x0ED0,
				0x0FD0, 0x1040, 0x1360, 0x1740, 0x17E0,
				0x18B0, 0x1940, 0x1A20, 0x1DD0, 0x20F0,
				0x3030, 0x30A0, 0xA810, 0xFB20, 0xFE30,
				0x10A40, 0x1D1B0, 0x1D250
				};

			prop = new CodePointIndexer (propStarts, propEnds, 0, 0);
			map = new CodePointIndexer (mapStarts, mapEnds, 0, 0);
			Combining = new CodePointIndexer (combiningStarts,
				combiningEnds, 0, 0);
		}

		public static int PropIdx (int cp)
		{
			return prop.ToIndex (cp);
		}

		public static int PropCP (int index)
		{
			return prop.ToCodePoint (index);
		}

		public static int PropCount { get { return prop.TotalCount; } }

		public static int MapIdx (int cp)
		{
			return map.ToIndex (cp);
		}

		public static int MapCP (int index)
		{
			return map.ToCodePoint (index);
		}

		public static int CbIdx (int cp)
		{
			return Combining.ToIndex (cp);
		}

		public static int CbCP (int index)
		{
			return Combining.ToCodePoint (index);
		}

		public static int MapCount { get { return map.TotalCount; } }
	}
}

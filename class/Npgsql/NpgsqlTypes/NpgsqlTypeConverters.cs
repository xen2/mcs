// NpgsqlTypes.NpgsqlTypesHelper.cs
//
// Author:
//	Glen Parker <glenebob@nwlink.com>
//
//	Copyright (C) 2004 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

// This file provides data type converters between PostgreSQL representations
// and .NET objects.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace NpgsqlTypes
{
	/// <summary>
	/// Provide event handlers to convert all native supported basic data types from their backend
	/// text representation to a .NET object.
	/// </summary>
	internal abstract class BasicBackendToNativeTypeConverter
	{
		private static readonly String[] DateFormats = new String[] { "yyyy-MM-dd", };

		private static readonly String[] TimeFormats =
			new String[]
				{
					"HH:mm:ss.ffffff", "HH:mm:ss", "HH:mm:ss.ffffffzz", "HH:mm:sszz", "HH:mm:ss.fffff", "HH:mm:ss.ffff", "HH:mm:ss.fff"
					, "HH:mm:ss.ff", "HH:mm:ss.f", "HH:mm:ss.fffffzz", "HH:mm:ss.ffffzz", "HH:mm:ss.fffzz", "HH:mm:ss.ffzz",
					"HH:mm:ss.fzz",
				};

		private static readonly String[] DateTimeFormats =
			new String[]
				{
					"yyyy-MM-dd HH:mm:ss.ffffff", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.ffffffzz", "yyyy-MM-dd HH:mm:sszz",
					"yyyy-MM-dd HH:mm:ss.fffff", "yyyy-MM-dd HH:mm:ss.ffff", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.ff",
					"yyyy-MM-dd HH:mm:ss.f", "yyyy-MM-dd HH:mm:ss.fffffzz", "yyyy-MM-dd HH:mm:ss.ffffzz", "yyyy-MM-dd HH:mm:ss.fffzz",
					"yyyy-MM-dd HH:mm:ss.ffzz", "yyyy-MM-dd HH:mm:ss.fzz",
				};

		/// <summary>
		/// Binary data.
		/// </summary>
		internal static Object ToBinary(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			Int32 octalValue = 0;
			Int32 byteAPosition = 0;
			Int32 byteAStringLength = BackendData.Length;
			MemoryStream ms = new MemoryStream();

			while (byteAPosition < byteAStringLength)
			{
				// The IsDigit is necessary in case we receive a \ as the octal value and not
				// as the indicator of a following octal value in decimal format.
				// i.e.: \201\301P\A
				if (BackendData[byteAPosition] == '\\')
				{
					if (byteAPosition + 1 == byteAStringLength)
					{
						octalValue = '\\';
						byteAPosition++;
					}
					else if (Char.IsDigit(BackendData[byteAPosition + 1]))
					{
                        octalValue = Convert.ToByte(BackendData.Substring(byteAPosition + 1, 3), 8);
                        //octalValue = (Byte.Parse(BackendData[byteAPosition + 1].ToString()) << 6);
                        //octalValue |= (Byte.Parse(BackendData[byteAPosition + 2].ToString()) << 3);
                        //octalValue |= Byte.Parse(BackendData[byteAPosition + 3].ToString());
						byteAPosition += 4;
					}
					else
					{
						octalValue = '\\';
						byteAPosition += 2;
					}
				}
				else
				{
					octalValue = (Byte)BackendData[byteAPosition];
					byteAPosition++;
				}


				ms.WriteByte((Byte)octalValue);
			}

			return ms.ToArray();
		}

		/// <summary>
		/// Convert a postgresql boolean to a System.Boolean.
		/// </summary>
		internal static Object ToBoolean(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize,
										 Int32 TypeModifier)
		{
			return (BackendData.ToLower() == "t" ? true : false);
		}


		/// <summary>
		/// Convert a postgresql bit to a System.Boolean.
		/// </summary>
		internal static Object ToBit(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			return (BackendData.ToLower() == "1" ? true : false);
		}

		/// <summary>
		/// Convert a postgresql datetime to a System.DateTime.
		/// </summary>
		internal static Object ToDateTime(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize,
										  Int32 TypeModifier)
		{
			// Get the date time parsed in all expected formats for timestamp.

			// First check for special values infinity and -infinity.

			if (BackendData == "infinity")
			{
				return DateTime.MaxValue;
			}

			if (BackendData == "-infinity")
			{
				return DateTime.MinValue;
			}

			return
				DateTime.ParseExact(BackendData, DateTimeFormats, DateTimeFormatInfo.InvariantInfo,
									DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces);
		}

		/// <summary>
		/// Convert a postgresql date to a System.DateTime.
		/// </summary>
		internal static Object ToDate(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			return
				DateTime.ParseExact(BackendData, DateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces);
		}

		/// <summary>
		/// Convert a postgresql time to a System.DateTime.
		/// </summary>
		internal static Object ToTime(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			return
				DateTime.ParseExact(BackendData, TimeFormats, DateTimeFormatInfo.InvariantInfo,
									DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces);
		}

		/// <summary>
		/// Convert a postgresql money to a System.Decimal.
		/// </summary>
		internal static Object ToMoney(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			// It's a number with a $ on the beginning...
			return Convert.ToDecimal(BackendData.Substring(1, BackendData.Length - 1), CultureInfo.InvariantCulture);
		}
	}

	/// <summary>
	/// Provide event handlers to convert the basic native supported data types from
	/// native form to backend representation.
	/// </summary>
	internal abstract class BasicNativeToBackendTypeConverter
	{
		/// <summary>
		/// Binary data.
		/// </summary>
		internal static String ToBinary(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			Byte[] byteArray = (Byte[])NativeData;
			int len = byteArray.Length;
			char[] res = new char[len * 5];

			for (int i = 0, o = 0; i < len; ++i, o += 5)
			{
				byte item = byteArray[i];
				res[o] = res[o + 1] = '\\';
				res[o + 2] = (char)('0' + (7 & (item >> 6)));
				res[o + 3] = (char)('0' + (7 & (item >> 3)));
				res[o + 4] = (char)('0' + (7 & item));
			}

			return new String(res);
		}

		/// <summary>
		/// Convert to a postgresql boolean.
		/// </summary>
		internal static String ToBoolean(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			return ((bool)NativeData) ? "TRUE" : "FALSE";
		}

		/// <summary>
		/// Convert to a postgresql bit.
		/// </summary>
		internal static String ToBit(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			// Convert boolean values to bit or convert int32 values to bit - odd values are 1 and
			// even numbers are 0.
			if (NativeData is Boolean)
			{
				return ((Boolean)NativeData) ? "1" : "0";
			}
			else
			{
				return (((Int32)NativeData) % 2 == 1) ? "1" : "0";
			}
		}

		/// <summary>
		/// Convert to a postgresql timestamp.
		/// </summary>
		internal static String ToDateTime(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			if (!(NativeData is DateTime))
			{
				return ExtendedNativeToBackendTypeConverter.ToTimeStamp(TypeInfo, NativeData);
			}
			if (DateTime.MaxValue.Equals(NativeData))
			{
				return "infinity";
			}
			if (DateTime.MinValue.Equals(NativeData))
			{
				return "-infinity";
			}
			return ((DateTime)NativeData).ToString("yyyy-MM-dd HH:mm:ss.ffffff", DateTimeFormatInfo.InvariantInfo);
		}

		/// <summary>
		/// Convert to a postgresql date.
		/// </summary>
		internal static String ToDate(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			if (!(NativeData is DateTime))
			{
				return ExtendedNativeToBackendTypeConverter.ToDate(TypeInfo, NativeData);
			}
			return ((DateTime)NativeData).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
		}

		/// <summary>
		/// Convert to a postgresql time.
		/// </summary>
		internal static String ToTime(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			if (!(NativeData is DateTime))
			{
				return ExtendedNativeToBackendTypeConverter.ToTime(TypeInfo, NativeData);
			}
			else
			{
				return ((DateTime)NativeData).ToString("HH:mm:ss.ffffff", DateTimeFormatInfo.InvariantInfo);
			}
		}

		/// <summary>
		/// Convert to a postgres money.
		/// </summary>
		internal static String ToMoney(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			return "$" + ((IFormattable)NativeData).ToString(null, CultureInfo.InvariantCulture.NumberFormat);
		}
	}


	/// <summary>
	/// Provide event handlers to convert extended native supported data types from their backend
	/// text representation to a .NET object.
	/// </summary>
	internal abstract class ExtendedBackendToNativeTypeConverter
	{
		private static readonly Regex pointRegex = new Regex(@"\((-?\d+.?\d*),(-?\d+.?\d*)\)");
		private static readonly Regex boxlsegRegex = new Regex(@"\((-?\d+.?\d*),(-?\d+.?\d*)\),\((-?\d+.?\d*),(-?\d+.?\d*)\)");
		private static readonly Regex pathpolygonRegex = new Regex(@"\((-?\d+.?\d*),(-?\d+.?\d*)\)");
		private static readonly Regex circleRegex = new Regex(@"<\((-?\d+.?\d*),(-?\d+.?\d*)\),(\d+.?\d*)>");


		/// <summary>
		/// Convert a postgresql point to a System.NpgsqlPoint.
		/// </summary>
		internal static Object ToPoint(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			Match m = pointRegex.Match(BackendData);

			return
				new NpgsqlPoint(Single.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
								Single.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
		}

		/// <summary>
		/// Convert a postgresql point to a System.RectangleF.
		/// </summary>
		internal static Object ToBox(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			Match m = boxlsegRegex.Match(BackendData);

			return
				new NpgsqlBox(
					new NpgsqlPoint(Single.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
									Single.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)),
					new NpgsqlPoint(Single.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
									Single.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)));
		}

		/// <summary>
		/// LDeg.
		/// </summary>
		internal static Object ToLSeg(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			Match m = boxlsegRegex.Match(BackendData);

			return
				new NpgsqlLSeg(
					new NpgsqlPoint(Single.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
									Single.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)),
					new NpgsqlPoint(Single.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
									Single.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)));
		}

		/// <summary>
		/// Path.
		/// </summary>
		internal static Object ToPath(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			Match m = pathpolygonRegex.Match(BackendData);
			Boolean open = (BackendData[0] == '[');
			List<NpgsqlPoint> points = new List<NpgsqlPoint>();

			while (m.Success)
			{
				if (open)
				{
					points.Add(
						new NpgsqlPoint(
							Single.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
							Single.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)));
				}
				else
				{
					// Here we have to do a little hack, because as of 2004-08-11 mono cvs version, the last group is returned with
					// a trailling ')' only when the last character of the string is a ')' which is the case for closed paths
					// returned by backend. This gives parsing exception when converting to single. 
					// I still don't know if this is a bug in mono or in my regular expression.
					// Check if there is this character and remove it.

					String group2 = m.Groups[2].ToString();
					if (group2.EndsWith(")"))
					{
						group2 = group2.Remove(group2.Length - 1, 1);
					}

					points.Add(
						new NpgsqlPoint(
							Single.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
							Single.Parse(group2, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)));
				}

				m = m.NextMatch();
			}

			NpgsqlPath result = new NpgsqlPath(points.ToArray());
			result.Open = open;
			return result;
		}

		/// <summary>
		/// Polygon.
		/// </summary>
		internal static Object ToPolygon(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize,
										 Int32 TypeModifier)
		{
			Match m = pathpolygonRegex.Match(BackendData);
			List<NpgsqlPoint> points = new List<NpgsqlPoint>();

			while (m.Success)
			{
				// Here we have to do a little hack, because as of 2004-08-11 mono cvs version, the last group is returned with
				// a trailling ')' only when the last character of the string is a ')' which is the case for closed paths
				// returned by backend. This gives parsing exception when converting to single. 
				// I still don't know if this is a bug in mono or in my regular expression.
				// Check if there is this character and remove it.

				String group2 = m.Groups[2].ToString();
				if (group2.EndsWith(")"))
				{
					group2 = group2.Remove(group2.Length - 1, 1);
				}

				points.Add(
					new NpgsqlPoint(Single.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
									Single.Parse(group2, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)));


				m = m.NextMatch();
			}

			return new NpgsqlPolygon(points);
		}

		/// <summary>
		/// Circle.
		/// </summary>
		internal static Object ToCircle(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			Match m = circleRegex.Match(BackendData);
			return
				new NpgsqlCircle(
					new NpgsqlPoint(Single.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
									Single.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)),
					Single.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
		}

		/// <summary>
		/// Inet.
		/// </summary>
		internal static Object ToInet(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
		{
			return new NpgsqlInet(BackendData);
		}
		
        internal static Object ToGuid(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            return new Guid(BackendData);
        }

		/// <summary>
		/// interval
		/// </summary>
		internal static object ToInterval(NpgsqlBackendTypeInfo typeInfo, String backendData, Int16 typeSize,
										  Int32 typeModifier)
		{
			return NpgsqlInterval.Parse(backendData);
		}

		internal static object ToTime(NpgsqlBackendTypeInfo typeInfo, String backendData, Int16 typeSize, Int32 typeModifier)
		{
			return NpgsqlTime.Parse(backendData);
		}

		internal static object ToTimeTZ(NpgsqlBackendTypeInfo typeInfo, String backendData, Int16 typeSize, Int32 typeModifier)
		{
			return NpgsqlTimeTZ.Parse(backendData);
		}

		internal static object ToDate(NpgsqlBackendTypeInfo typeInfo, String backendData, Int16 typeSize, Int32 typeModifier)
		{
			return NpgsqlDate.Parse(backendData);
		}

		internal static object ToTimeStamp(NpgsqlBackendTypeInfo typeInfo, String backendData, Int16 typeSize,
										   Int32 typeModifier)
		{
			return NpgsqlTimeStamp.Parse(backendData);
		}

		internal static object ToTimeStampTZ(NpgsqlBackendTypeInfo typeInfo, String backendData, Int16 typeSize,
											 Int32 typeModifier)
		{
			return NpgsqlTimeStampTZ.Parse(backendData);
		}
	}

	/// <summary>
	/// Provide event handlers to convert extended native supported data types from
	/// native form to backend representation.
	/// </summary>
	internal abstract class ExtendedNativeToBackendTypeConverter
	{
		/// <summary>
		/// Point.
		/// </summary>
		internal static String ToPoint(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			if (NativeData is NpgsqlPoint)
			{
				NpgsqlPoint P = (NpgsqlPoint)NativeData;
				return String.Format(CultureInfo.InvariantCulture, "({0},{1})", P.X, P.Y);
			}
			else
			{
				throw new InvalidCastException("Unable to cast data to NpgsqlPoint type");
			}
		}

		/// <summary>
		/// Box.
		/// </summary>
		internal static String ToBox(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			/*if (NativeData.GetType() == typeof(Rectangle)) {
                Rectangle       R = (Rectangle)NativeData;
                return String.Format(CultureInfo.InvariantCulture, "({0},{1}),({2},{3})", R.Left, R.Top, R.Left + R.Width, R.Top + R.Height);
            } else if (NativeData.GetType() == typeof(RectangleF)) {
                RectangleF      R = (RectangleF)NativeData;
                return String.Format(CultureInfo.InvariantCulture, "({0},{1}),({2},{3})", R.Left, R.Top, R.Left + R.Width, R.Top + R.Height);*/

			if (NativeData is NpgsqlBox)
			{
				NpgsqlBox box = (NpgsqlBox)NativeData;
				return
					String.Format(CultureInfo.InvariantCulture, "({0},{1}),({2},{3})", box.LowerLeft.X, box.LowerLeft.Y,
								  box.UpperRight.X, box.UpperRight.Y);
			}
			else
			{
				throw new InvalidCastException("Unable to cast data to Rectangle type");
			}
		}

		/// <summary>
		/// LSeg.
		/// </summary>
		internal static String ToLSeg(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			NpgsqlLSeg S = (NpgsqlLSeg)NativeData;
			return String.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", S.Start.X, S.Start.Y, S.End.X, S.End.Y);
		}

		/// <summary>
		/// Open path.
		/// </summary>
		internal static String ToPath(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			StringBuilder B = null;
			try
			{
	B =new StringBuilder();

			foreach (NpgsqlPoint P in ((NpgsqlPath)NativeData))
			{
				B.AppendFormat(CultureInfo.InvariantCulture, "{0}({1},{2})", (B.Length > 0 ? "," : ""), P.X, P.Y);
			}

			return String.Format("[{0}]", B);
			}
			finally
			{
				B = null;

			}
		
		}

		/// <summary>
		/// Polygon.
		/// </summary>
		internal static String ToPolygon(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			StringBuilder B = new StringBuilder();

			foreach (NpgsqlPoint P in ((NpgsqlPolygon)NativeData))
			{
				B.AppendFormat(CultureInfo.InvariantCulture, "{0}({1},{2})", (B.Length > 0 ? "," : ""), P.X, P.Y);
			}

			return String.Format("({0})", B);
		}

		/// <summary>
		/// Circle.
		/// </summary>
		internal static String ToCircle(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			NpgsqlCircle C = (NpgsqlCircle)NativeData;
			return String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", C.Center.X, C.Center.Y, C.Radius);
		}

		/// <summary>
		/// Convert to a postgres inet.
		/// </summary>
		internal static String ToIPAddress(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			if (NativeData is NpgsqlInet)
			{
				return ((NpgsqlInet)NativeData).ToString();
			}
			return NativeData.ToString();

		}

		/// <summary>
		/// Convert to a postgres interval
		/// </summary>
		internal static String ToInterval(NpgsqlNativeTypeInfo TypeInfo, Object NativeData)
		{
			return
				((NativeData is TimeSpan)
					? ((NpgsqlInterval)(TimeSpan)NativeData).ToString()
					: ((NpgsqlInterval)NativeData).ToString());
		}

		internal static string ToTime(NpgsqlNativeTypeInfo typeInfo, object nativeData)
		{
			if (nativeData is DateTime)
			{
				return BasicNativeToBackendTypeConverter.ToTime(typeInfo, nativeData);
			}
			NpgsqlTime time;
			if (nativeData is TimeSpan)
			{
				time = (NpgsqlTime)(TimeSpan)nativeData;
			}
			else
			{
				time = (NpgsqlTime)nativeData;
			}
			return time.ToString();
		}

		internal static string ToTimeTZ(NpgsqlNativeTypeInfo typeInfo, object nativeData)
		{
			if (nativeData is DateTime)
			{
				return BasicNativeToBackendTypeConverter.ToTime(typeInfo, nativeData);
			}
			NpgsqlTimeTZ time;
			if (nativeData is TimeSpan)
			{
				time = (NpgsqlTimeTZ)(TimeSpan)nativeData;
			}
			else
			{
				time = (NpgsqlTimeTZ)nativeData;
			}
			return time.ToString();
		}

		internal static string ToDate(NpgsqlNativeTypeInfo typeInfo, object nativeData)
		{
			if (nativeData is DateTime)
			{
				return BasicNativeToBackendTypeConverter.ToDate(typeInfo, nativeData);
			}
			else
			{
				return nativeData.ToString();
			}
		}

		internal static string ToTimeStamp(NpgsqlNativeTypeInfo typeInfo, object nativeData)
		{
			if (nativeData is DateTime)
			{
				return BasicNativeToBackendTypeConverter.ToDateTime(typeInfo, nativeData);
			}
			else
			{
				return nativeData.ToString();
			}
		}
	}
}
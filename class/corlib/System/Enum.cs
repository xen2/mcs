//
// System.Enum.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
	internal struct MonoEnumInfo {
		internal Type utype;
		internal Array values;
		internal string[] names;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void get_enum_info (Type enumType, out MonoEnumInfo info);
		
		internal static void GetInfo (Type enumType, out MonoEnumInfo info) {
			get_enum_info (enumType, out info);
			Array.Sort (info.values, info.names);
		}
	};

	[MonoTODO]
	public abstract class Enum : ValueType, IComparable {

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern object get_value ();
		
		public static Array GetValues (Type enumType) {
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.values;
		}
		public static string[] GetNames (Type enumType) {
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.names;
		}
		public static string GetName( Type enumType, object value) {
			MonoEnumInfo info;
			int i;
			MonoEnumInfo.GetInfo (enumType, out info);
			for (i = 0; i < info.values.Length; ++i) {				
				if (value.Equals (info.values.GetValue (i)))
					return info.names [i];
			}
			return null;
		}
		public static bool IsDefined( Type enumType, object value) {
			return GetName (enumType, value) != null;
		}
		public static Type GetUnderlyingType( Type enumType) {
			MonoEnumInfo info;
			MonoEnumInfo.GetInfo (enumType, out info);
			return info.utype;
		}

		/// <summary>
		///   Compares the enum value with another enum value of the same type.
		/// </summary>
		///
		/// <remarks>
		///   
		int IComparable.CompareTo (object obj)
		{
			if (obj == null)
				return 1;

			object value1, value2;

			value1 = this.get_value ();

			if (obj is Enum)
				value2 = ((Enum)obj).get_value();
			else
				value2 = obj;

			return ((IComparable)value1).CompareTo (value2);
		}
		
		public override string ToString ()
		{
			return GetName (this.GetType(), this.get_value ());
		}

		public string ToString (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		public string ToString (String format)
		{
			throw new NotImplementedException ();
		}

		public string ToString (String format, IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		public static object ToObject(Type enumType, byte value)
		{
			return ToObject (enumType, (object)value);
		}
		
		public static object ToObject(Type enumType, short value)
		{
			return ToObject (enumType, (object)value);
		}
		public static object ToObject(Type enumType, int value)
		{
			return ToObject (enumType, (object)value);
		}
		public static object ToObject(Type enumType, long value)
		{
			return ToObject (enumType, (object)value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern object ToObject(Type enumType, object value);

		[CLSCompliant(false)]
		public static object ToObject(Type enumType, sbyte value)
		{
			return ToObject (enumType, (object)value);
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, ushort value)
		{
			return ToObject (enumType, (object)value);
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, uint value)
		{
			return ToObject (enumType, (object)value);
		}
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, ulong value)
		{
			return ToObject (enumType, (object)value);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is Enum))
				return false;

			object v1 = this.get_value ();
			object v2 = ((Enum)obj).get_value ();

			return v1.Equals (v2);
		}
	}
}

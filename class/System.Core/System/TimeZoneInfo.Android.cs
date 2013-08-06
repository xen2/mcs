/*
 * System.TimeZoneInfo Android Support
 *
 * Author(s)
 * 	Jonathan Pryor  <jpryor@novell.com>
 * 	The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#if (INSIDE_CORLIB && MONODROID)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System {

	interface IAndroidTimeZoneDB {
		IEnumerable<string>   GetAvailableIds ();
		byte[]                GetTimeZoneData (string id);
	}

	[StructLayout (LayoutKind.Sequential, Pack=1)]
	unsafe struct AndroidTzDataHeader {
		public fixed byte signature [12];
		public int        indexOffset;
		public int        dataOffset;
		public int        zoneTabOffset;
	}

	[StructLayout (LayoutKind.Sequential, Pack=1)]
	unsafe struct AndroidTzDataEntry {
		public fixed byte id [40];
		public int        byteOffset;
		public int        length;
		public int        rawUtcOffset;
	}

	/*
	 * Android v4.3 Timezone support infrastructure.
	 *
	 * This is a C# port of libcore.util.ZoneInfoDB:
	 *
	 *    https://android.googlesource.com/platform/libcore/+/master/luni/src/main/java/libcore/util/ZoneInfoDB.java
	 *
	 * This is needed in order to read Android v4.3 tzdata files.
	 */
	sealed class AndroidTzData : IAndroidTimeZoneDB {

		internal static readonly string[] Paths = new string[]{
			Environment.GetEnvironmentVariable ("ANDROID_DATA") + "/misc/zoneinfo/tzdata",
			Environment.GetEnvironmentVariable ("ANDROID_ROOT") + "/usr/share/zoneinfo/tzdata",
		};

		string    tzdataPath;
		Stream    data;
		string    version;
		string    zoneTab;

		string[]  ids;
		int[]     byteOffsets;
		int[]     lengths;

		public AndroidTzData (params string[] paths)
		{
			foreach (var path in paths)
				if (LoadData (path)) {
					tzdataPath = path;
					return;
				}

			Console.Error.WriteLine ("Couldn't find any tzdata!");
			tzdataPath  = "/";
			version     = "missing";
			zoneTab     = "# Emergency fallback data.\n";
			ids         = new[]{ "GMT" };
		}

		public string Version {
			get {return version;}
		}

		public string ZoneTab {
			get {return zoneTab;}
		}

		bool LoadData (string path)
		{
			if (!File.Exists (path))
				return false;
			try {
				data = File.OpenRead (path);
			} catch (IOException) {
				return false;
			} catch (UnauthorizedAccessException) {
				return false;
			}

			try {
				ReadHeader ();
				return true;
			} catch (Exception e) {
				Console.Error.WriteLine ("tzdata file \"{0}\" was present but invalid: {1}", path, e);
			}
			return false;
		}

		unsafe void ReadHeader ()
		{
			int size   = System.Math.Max (Marshal.SizeOf (typeof (AndroidTzDataHeader)), Marshal.SizeOf (typeof (AndroidTzDataEntry)));
			var buffer = new byte [size];
			var header = ReadAt<AndroidTzDataHeader>(0, buffer);

			header.indexOffset    = NetworkToHostOrder (header.indexOffset);
			header.dataOffset     = NetworkToHostOrder (header.dataOffset);
			header.zoneTabOffset  = NetworkToHostOrder (header.zoneTabOffset);

			sbyte* s = (sbyte*) header.signature;
			string magic = new string (s, 0, 6, Encoding.ASCII);
			if (magic != "tzdata" || header.signature [11] != 0) {
				var b = new StringBuilder ();
				b.Append ("bad tzdata magic:");
				for (int i = 0; i < 12; ++i) {
					b.Append (" ").Append (((byte) s [i]).ToString ("x2"));
				}
				throw new InvalidOperationException ("bad tzdata magic: " + b.ToString ());
			}

			version = new string (s, 6, 5, Encoding.ASCII);

			ReadIndex (header.indexOffset, header.dataOffset, buffer);
			ReadZoneTab (header.zoneTabOffset, checked ((int) data.Length) - header.zoneTabOffset);
		}

		unsafe T ReadAt<T> (long position, byte[] buffer)
			where T : struct
		{
			int size = Marshal.SizeOf (typeof (T));
			if (buffer.Length < size)
				throw new InvalidOperationException ("Internal error: buffer too small");

			data.Position = position;
			int r;
			if ((r = data.Read (buffer, 0, size)) < size)
				throw new InvalidOperationException (
						string.Format ("Error reading '{0}': read {1} bytes, expected {2}", tzdataPath, r, size));

			fixed (byte* b = buffer)
				return (T) Marshal.PtrToStructure ((IntPtr) b, typeof (T));
		}

		static int NetworkToHostOrder (int value)
		{
			if (!BitConverter.IsLittleEndian)
				return value;

			return
				(((value >> 24) & 0xFF) |
				 ((value >> 08) & 0xFF00) |
				 ((value << 08) & 0xFF0000) |
				 ((value << 24)));
		}

		unsafe void ReadIndex (int indexOffset, int dataOffset, byte[] buffer)
		{
			int indexSize   = dataOffset - indexOffset;
			int entryCount  = indexSize / Marshal.SizeOf (typeof (AndroidTzDataEntry));
			int entrySize   = Marshal.SizeOf (typeof (AndroidTzDataEntry));

			byteOffsets   = new int [entryCount];
			ids           = new string [entryCount];
			lengths       = new int [entryCount];

			for (int i = 0; i < entryCount; ++i) {
				var entry = ReadAt<AndroidTzDataEntry>(indexOffset + (entrySize*i), buffer);
				var p     = (sbyte*) entry.id;

				byteOffsets [i]   = NetworkToHostOrder (entry.byteOffset) + dataOffset;
				ids [i]           = new string (p, 0, GetStringLength (p, 40), Encoding.ASCII);
				lengths [i]       = NetworkToHostOrder (entry.length);

				if (lengths [i] < Marshal.SizeOf (typeof (AndroidTzDataHeader)))
					throw new InvalidOperationException ("Length in index file < sizeof(tzhead)");
			}
		}

		static unsafe int GetStringLength (sbyte* s, int maxLength)
		{
			int len;
			for (len = 0; len < maxLength; len++, s++) {
				if (*s == 0)
					break;
			}
			return len;
		}

		unsafe void ReadZoneTab (int zoneTabOffset, int zoneTabSize)
		{
			byte[] zoneTab = new byte [zoneTabSize];

			data.Position = zoneTabOffset;

			int r;
			if ((r = data.Read (zoneTab, 0, zoneTab.Length)) < zoneTab.Length)
				throw new InvalidOperationException (
						string.Format ("Error reading zonetab: read {0} bytes, expected {1}", r, zoneTabSize));

			this.zoneTab = Encoding.ASCII.GetString (zoneTab, 0, zoneTab.Length);
		}

		public IEnumerable<string> GetAvailableIds ()
		{
			return ids;
		}

		public byte[] GetTimeZoneData (string id)
		{
			int i = Array.BinarySearch (ids, id, StringComparer.Ordinal);
			if (i < 0)
				return null;

			int offset = byteOffsets [i];
			int length = lengths [i];
			var buffer = new byte [length];

			lock (data) {
				data.Position = offset;
				int r;
				if ((r = data.Read (buffer, 0, buffer.Length)) < buffer.Length)
					throw new InvalidOperationException (
							string.Format ("Unable to fully read from file '{0}' at offset {1} length {2}; read {3} bytes expected {4}.",
								tzdataPath, offset, length, r, buffer.Length));
			}

			return buffer;
		}
	}

	partial class TimeZoneInfo {

		/*
		 * Android < v4.3 Timezone support infrastructure.
		 *
		 * This is a C# port of org.apache.harmony.luni.internal.util.ZoneInfoDB:
		 *
		 *    http://android.git.kernel.org/?p=platform/libcore.git;a=blob;f=luni/src/main/java/org/apache/harmony/luni/internal/util/ZoneInfoDB.java;h=3e7bdc3a952b24da535806d434a3a27690feae26;hb=HEAD
		 *
		 * From the ZoneInfoDB source:
		 *
		 *    However, to conserve disk space the data for all time zones are 
		 *    concatenated into a single file, and a second file is used to indicate 
		 *    the starting position of each time zone record.  A third file indicates
		 *    the version of the zoneinfo databse used to generate the data.
		 *
		 * which succinctly describes why we can't just use the LIBC implementation in
		 * TimeZoneInfo.cs -- the "standard Unixy" directory structure is NOT used.
		 */
		sealed class ZoneInfoDB : IAndroidTimeZoneDB {
			const int TimeZoneNameLength  = 40;
			const int TimeZoneIntSize     = 4;

			internal static readonly string ZoneDirectoryName  = Environment.GetEnvironmentVariable ("ANDROID_ROOT") + "/usr/share/zoneinfo/";

			const    string ZoneFileName       = "zoneinfo.dat";
			const    string IndexFileName      = "zoneinfo.idx";
			const    string DefaultVersion     = "2007h";
			const    string VersionFileName    = "zoneinfo.version";

			readonly string    zoneRoot;
			readonly string    version;
			readonly string[]  names;
			readonly int[]     starts;
			readonly int[]     lengths;
			readonly int[]     offsets;

			public ZoneInfoDB (string zoneInfoDB = null)
			{
				zoneRoot = zoneInfoDB ?? ZoneDirectoryName;
				try {
					version = ReadVersion (Path.Combine (zoneRoot, VersionFileName));
				} catch {
					version = DefaultVersion;
				}

				try {
					ReadDatabase (Path.Combine (zoneRoot, IndexFileName), out names, out starts, out lengths, out offsets);
				} catch {
					names   = new string [0];
					starts  = new int [0];
					lengths = new int [0];
					offsets = new int [0];
				}
			}

			static string ReadVersion (string path)
			{
				using (var file = new StreamReader (path, Encoding.GetEncoding ("iso-8859-1"))) {
					return file.ReadToEnd ().Trim ();
				}
			}

			void ReadDatabase (string path, out string[] names, out int[] starts, out int[] lengths, out int[] offsets)
			{
				using (var file = File.OpenRead (path)) {
					var nbuf = new byte [TimeZoneNameLength];

					int numEntries = (int) (file.Length / (TimeZoneNameLength + 3*TimeZoneIntSize));

					char[]  namebuf = new char [TimeZoneNameLength];

					names   = new string [numEntries];
					starts  = new int [numEntries];
					lengths = new int [numEntries];
					offsets = new int [numEntries];

					for (int i = 0; i < numEntries; ++i) {
						Fill (file, nbuf, nbuf.Length);
						int namelen;
						for (namelen = 0; namelen < nbuf.Length; ++namelen) {
							if (nbuf [namelen] == '\0')
								break;
							namebuf [namelen] = (char) (nbuf [namelen] & 0xFF);
						}

						names   [i] = new string (namebuf, 0, namelen);
						starts  [i] = ReadInt32 (file, nbuf);
						lengths [i] = ReadInt32 (file, nbuf);
						offsets [i] = ReadInt32 (file, nbuf);
					}
				}
			}

			static void Fill (Stream stream, byte[] nbuf, int required)
			{
				int read, offset = 0;
				while (offset < required && (read = stream.Read (nbuf, offset, required - offset)) > 0)
					offset += read;
				if (read != required)
					throw new EndOfStreamException ("Needed to read " + required + " bytes; read " + read + " bytes");
			}

			// From java.io.RandomAccessFioe.readInt(), as we need to use the same
			// byte ordering as Java uses.
			static int ReadInt32 (Stream stream, byte[] nbuf)
			{
				Fill (stream, nbuf, 4);
				return ((nbuf [0] & 0xff) << 24) + ((nbuf [1] & 0xff) << 16) +
					((nbuf [2] & 0xff) << 8) + (nbuf [3] & 0xff);
			}

			internal string Version {
				get {return version;}
			}

			public IEnumerable<string> GetAvailableIds ()
			{
				return GetAvailableIds (0, false);
			}

			IEnumerable<string> GetAvailableIds (int rawOffset)
			{
				return GetAvailableIds (rawOffset, true);
			}

			IEnumerable<string> GetAvailableIds (int rawOffset, bool checkOffset)
			{
				for (int i = 0; i < offsets.Length; ++i) {
					if (!checkOffset || offsets [i] == rawOffset)
						yield return names [i];
				}
			}

			public byte[] GetTimeZoneData (string id)
			{
				int start, length;
				using (var stream = GetTimeZoneData (id, out start, out length)) {
					if (stream == null)
						return null;
					byte[] buf = new byte [length];
					Fill (stream, buf, buf.Length);
					return buf;
				}
			}

			FileStream GetTimeZoneData (string name, out int start, out int length)
			{
				var f = new FileInfo (Path.Combine (zoneRoot, name));
				if (f.Exists) {
					start   = 0;
					length  = (int) f.Length;
					return f.OpenRead ();
				}

				start = length = 0;

				int i = Array.BinarySearch (names, name, StringComparer.Ordinal);
				if (i < 0)
					return null;

				start   = starts [i];
				length  = lengths [i];

				var stream = File.OpenRead (Path.Combine (zoneRoot, ZoneFileName));
				stream.Seek (start, SeekOrigin.Begin);

				return stream;
			}
		}

		static class AndroidTimeZones {

			static IAndroidTimeZoneDB db;

			static AndroidTimeZones ()
			{
				db = GetDefaultTimeZoneDB ();
			}

			static IAndroidTimeZoneDB GetDefaultTimeZoneDB ()
			{
				foreach (var p in AndroidTzData.Paths)
					if (File.Exists (p))
						return new AndroidTzData (AndroidTzData.Paths);
				if (Directory.Exists (ZoneInfoDB.ZoneDirectoryName))
					return new ZoneInfoDB ();
				return null;
			}

			internal static IEnumerable<string> GetAvailableIds ()
			{
				return db == null
					? new string [0]
					: db.GetAvailableIds ();
			}

			static TimeZoneInfo _GetTimeZone (string name)
			{
				if (db == null)
					return null;
				byte[] buffer = db.GetTimeZoneData (name);
				if (buffer == null)
					return null;
				return TimeZoneInfo.ParseTZBuffer (name, buffer, buffer.Length);
			}

			internal static TimeZoneInfo GetTimeZone (string id)
			{
				if (id != null) {
					if (id == "GMT" || id == "UTC")
						return new TimeZoneInfo (id, TimeSpan.FromSeconds (0), id, id, id, null, true);
					if (id.StartsWith ("GMT"))
						return new TimeZoneInfo (id,
								TimeSpan.FromSeconds (ParseNumericZone (id)),
								id, id, id, null, true);
				}

				try {
					return _GetTimeZone (id);
				} catch (Exception) {
					return null;
				}
			}

			static int ParseNumericZone (string name)
			{
				if (name == null || !name.StartsWith ("GMT") || name.Length <= 3)
					return 0;

				int sign;
				if (name [3] == '+')
					sign = 1;
				else if (name [3] == '-')
					sign = -1;
				else
					return 0;

				int where;
				int hour = 0;
				bool colon = false;
				for (where = 4; where < name.Length; where++) {
					char c = name [where];

					if (c == ':') {
						where++;
						colon = true;
						break;
					}

					if (c >= '0' && c <= '9')
						hour = hour * 10 + c - '0';
					else
						return 0;
				}

				int min = 0;
				for (; where < name.Length; where++) {
					char c = name [where];

					if (c >= '0' && c <= '9')
						min = min * 10 + c - '0';
					else
						return 0;
				}

				if (colon)
					return sign * (hour * 60 + min) * 60;
				else if (hour >= 100)
					return sign * ((hour / 100) * 60 + (hour % 100)) * 60;
				else
					return sign * (hour * 60) * 60;
			}

			static readonly object _lock = new object ();

			static TimeZoneInfo defaultZone;
			internal static TimeZoneInfo Default {
				get {
					lock (_lock) {
						if (defaultZone != null)
							return defaultZone;
						return defaultZone = GetTimeZone (GetDefaultTimeZoneName ());
					}
				}
			}

			// <sys/system_properties.h>
			[DllImport ("/system/lib/libc.so")]
			static extern int __system_property_get (string name, StringBuilder value);

			const int MaxPropertyNameLength   = 32; // <sys/system_properties.h>
			const int MaxPropertyValueLength  = 92; // <sys/system_properties.h>

			static string GetDefaultTimeZoneName ()
			{
				var buf = new StringBuilder (MaxPropertyValueLength + 1);
				int n = __system_property_get ("persist.sys.timezone", buf);
				if (n > 0)
					return buf.ToString ();
				return null;
			}

#if SELF_TEST
			/*
			 * Compile:
			 *    mcs  /out:tzi.exe /unsafe "/d:INSIDE_CORLIB;MONODROID;NET_4_0;LIBC;SELF_TEST" System/TimeZone*.cs ../../build/common/Consts.cs ../Mono.Options/Mono.Options/Options.cs
			 * Prep:
			 *    mkdir -p usr/share/zoneinfo
			 *    android_root=`adb shell echo '$ANDROID_ROOT' | tr -d "\r"`
			 *    adb pull $android_root/usr/share/zoneinfo usr/share/zoneinfo
			 * Run:
			 *    ANDROID_ROOT=`pwd` mono tzi.exe
			 */
			static void Main (string[] args)
			{
				Func<IAndroidTimeZoneDB> c = () => GetDefaultTimeZoneDB ();
				Mono.Options.OptionSet p = null;
				p = new Mono.Options.OptionSet () {
					{ "T=", "Create AndroidTzData from {PATH}.", v => {
							c = () => new AndroidTzData (v);
					} },
					{ "Z=", "Create ZoneInfoDB from {DIR}.", v => {
							c = () => new ZoneInfoDB (v);
					} },
					{ "help", "Show this message and exit", v => {
							p.WriteOptionDescriptions (Console.Out);
							Environment.Exit (0);
					} },
				};
				p.Parse (args);
				AndroidTimeZones.db = c ();
				Console.WriteLine ("DB type: {0}", AndroidTimeZones.db.GetType ().FullName);
				foreach (var id in GetAvailableIds ()) {
					Console.Write ("name={0,-40}", id);
					try {
						TimeZoneInfo zone = _GetTimeZone (id);
						if (zone != null)
							Console.Write (" {0}", zone);
						else {
							Console.Write (" ERROR:null");
						}
					} catch (Exception e) {
						Console.WriteLine ();
						Console.Write ("ERROR: {0}", e);
					}
					Console.WriteLine ();
				}
			}
#endif
		}
	}
}

#endif // MONODROID


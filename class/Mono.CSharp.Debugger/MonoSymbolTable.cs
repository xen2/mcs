//
// System.Diagnostics.SymbolStore/MonoSymbolTable.cs
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Text;
using System.IO;

//
// Parts which are actually written into the symbol file are marked with
//
//         #region This is actually written to the symbol file
//         #endregion
//
// Please do not modify these regions without previously talking to me.
//
// All changes to the file format must be synchronized in several places:
//
// a) The fields in these regions (and their order) must match the actual
//    contents of the symbol file.
//
//    This helps people to understand the symbol file format without reading
//    too much source code, ie. you look at the appropriate region and then
//    you know what's actually in the file.
//
//    It is also required to help me enforce b).
//
// b) The regions must be kept in sync with the unmanaged code in
//    mono/metadata/debug-mono-symfile.h
//
// When making changes to the file format, you must also increase two version
// numbers:
//
// i)  OffsetTable.Version in this file.
// ii) MONO_SYMBOL_FILE_VERSION in mono/metadata/debug-mono-symfile.h
//
// After doing so, recompile everything, including the debugger.  Symbol files
// with different versions are incompatible to each other and the debugger and
// the runtime enfore this, so you need to recompile all your assemblies after
// changing the file format.
//

namespace Mono.CSharp.Debugger
{
	public struct OffsetTable
	{
		public const int  Version = 35;
		public const long Magic   = 0x45e82623fd7fa614;

		#region This is actually written to the symbol file
		public int TotalFileSize;
		public int DataSectionOffset;
		public int DataSectionSize;
		public int SourceCount;
		public int SourceTableOffset;
		public int SourceTableSize;
		public int MethodCount;
		public int MethodTableOffset;
		public int MethodTableSize;
		public int TypeCount;
		#endregion

		internal OffsetTable (BinaryReader reader)
		{
			TotalFileSize = reader.ReadInt32 ();
			DataSectionOffset = reader.ReadInt32 ();
			DataSectionSize = reader.ReadInt32 ();
			SourceCount = reader.ReadInt32 ();
			SourceTableOffset = reader.ReadInt32 ();
			SourceTableSize = reader.ReadInt32 ();
			MethodCount = reader.ReadInt32 ();
			MethodTableOffset = reader.ReadInt32 ();
			MethodTableSize = reader.ReadInt32 ();
			TypeCount = reader.ReadInt32 ();
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (TotalFileSize);
			bw.Write (DataSectionOffset);
			bw.Write (DataSectionSize);
			bw.Write (SourceCount);
			bw.Write (SourceTableOffset);
			bw.Write (SourceTableSize);
			bw.Write (MethodCount);
			bw.Write (MethodTableOffset);
			bw.Write (MethodTableSize);
			bw.Write (TypeCount);
		}

		public override string ToString ()
		{
			return String.Format (
				"OffsetTable [{0} - {1}:{2} - {3}:{4}:{5} - {6}:{7}:{8} - {9}]",
				TotalFileSize, DataSectionOffset, DataSectionSize, SourceCount,
				SourceTableOffset, SourceTableSize, MethodCount, MethodTableOffset,
				MethodTableSize, TypeCount);
		}
	}

	public struct LineNumberEntry
	{
		#region This is actually written to the symbol file
		public readonly int Row;
		public readonly int Offset;
		#endregion

		public LineNumberEntry (int row, int offset)
		{
			this.Row = row;
			this.Offset = offset;
		}

		public static LineNumberEntry Null = new LineNumberEntry (0, 0);

		internal LineNumberEntry (BinaryReader reader)
		{
			Row = reader.ReadInt32 ();
			Offset = reader.ReadInt32 ();
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Row);
			bw.Write (Offset);
		}

		private class OffsetComparerClass : IComparer
		{
			public int Compare (object a, object b)
			{
				LineNumberEntry l1 = (LineNumberEntry) a;
				LineNumberEntry l2 = (LineNumberEntry) b;

				if (l1.Offset < l2.Offset)
					return -1;
				else if (l1.Offset > l2.Offset)
					return 1;
				else
					return 0;
			}
		}

		private class RowComparerClass : IComparer
		{
			public int Compare (object a, object b)
			{
				LineNumberEntry l1 = (LineNumberEntry) a;
				LineNumberEntry l2 = (LineNumberEntry) b;

				if (l1.Row < l2.Row)
					return -1;
				else if (l1.Row > l2.Row)
					return 1;
				else
					return 0;
			}
		}

		public static readonly IComparer OffsetComparer = new OffsetComparerClass ();
		public static readonly IComparer RowComparer = new RowComparerClass ();

		public override string ToString ()
		{
			return String.Format ("[Line {0}:{1}]", Row, Offset);
		}
	}

	public class LexicalBlockEntry
	{
		public int Index;
		#region This is actually written to the symbol file
		public int StartOffset;
		public int EndOffset;
		#endregion

		public LexicalBlockEntry (int index, int start_offset)
		{
			this.Index = index;
			this.StartOffset = start_offset;
		}

		internal LexicalBlockEntry (int index, BinaryReader reader)
		{
			this.Index = index;
			this.StartOffset = reader.ReadInt32 ();
			this.EndOffset = reader.ReadInt32 ();
		}

		public void Close (int end_offset)
		{
			this.EndOffset = end_offset;
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (StartOffset);
			bw.Write (EndOffset);
		}

		public override string ToString ()
		{
			return String.Format ("[LexicalBlock {0}:{1}]", StartOffset, EndOffset);
		}
	}

	public struct LocalVariableEntry
	{
		#region This is actually written to the symbol file
		public readonly string Name;
		public readonly FieldAttributes Attributes;
		public readonly byte[] Signature;
		public readonly int BlockIndex;
		#endregion

		public LocalVariableEntry (string Name, FieldAttributes Attributes, byte[] Signature,
					   int BlockIndex)
		{
			this.Name = Name;
			this.Attributes = Attributes;
			this.Signature = Signature;
			this.BlockIndex = BlockIndex;
		}

		internal LocalVariableEntry (BinaryReader reader)
		{
			int name_length = reader.ReadInt32 ();
			byte[] name = reader.ReadBytes (name_length);
			Name = Encoding.UTF8.GetString (name);
			Attributes = (FieldAttributes) reader.ReadInt32 ();
			int sig_length = reader.ReadInt32 ();
			Signature = reader.ReadBytes (sig_length);
			BlockIndex = reader.ReadInt32 ();
		}

		internal void Write (MonoSymbolFile file, BinaryWriter bw)
		{
			file.WriteString (bw, Name);
			bw.Write ((int) Attributes);
			bw.Write ((int) Signature.Length);
			bw.Write (Signature);
			bw.Write (BlockIndex);
		}

		public override string ToString ()
		{
			return String.Format ("[LocalVariable {0}:{1}]", Name, Attributes);
		}
	}

	public class SourceFileEntry
	{
		#region This is actually written to the symbol file
		public readonly int Index;
		int Count;
		int NamespaceCount;
		int NameOffset;
		int MethodOffset;
		int NamespaceTableOffset;
		#endregion

		MonoSymbolFile file;
		string file_name;
		ArrayList methods;
		ArrayList namespaces;
		bool creating;

		public static int Size {
			get { return 24; }
		}

		internal SourceFileEntry (MonoSymbolFile file, string file_name)
		{
			this.file = file;
			this.file_name = file_name;
			this.Index = file.AddSource (this);

			creating = true;
			methods = new ArrayList ();
			namespaces = new ArrayList ();
		}

		public void DefineMethod (MethodBase method, int token, LocalVariableEntry[] locals,
					  LineNumberEntry[] lines, LexicalBlockEntry[] blocks,
					  int start, int end, int namespace_id)
		{
			if (!creating)
				throw new InvalidOperationException ();

			MethodEntry entry = new MethodEntry (
				file, this, method, token, locals, lines, blocks, start, end, namespace_id);

			methods.Add (entry);
			file.AddMethod (entry);
		}

		public int DefineNamespace (string name, string[] using_clauses, int parent)
		{
			if (!creating)
				throw new InvalidOperationException ();

			int index = file.GetNextNamespaceIndex ();
			NamespaceEntry ns = new NamespaceEntry (name, index, using_clauses, parent);
			namespaces.Add (ns);
			return index;
		}

		internal void WriteData (BinaryWriter bw)
		{
			NameOffset = (int) bw.BaseStream.Position;
			file.WriteString (bw, file_name);

			ArrayList list = new ArrayList ();
			foreach (MethodEntry entry in methods)
				list.Add (entry.Write (file, bw));
			list.Sort ();
			Count = list.Count;

			MethodOffset = (int) bw.BaseStream.Position;
			foreach (MethodSourceEntry method in list)
				method.Write (bw);

			NamespaceCount = namespaces.Count;
			NamespaceTableOffset = (int) bw.BaseStream.Position;
			foreach (NamespaceEntry ns in namespaces)
				ns.Write (file, bw);
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Index);
			bw.Write (Count);
			bw.Write (NamespaceCount);
			bw.Write (NameOffset);
			bw.Write (MethodOffset);
			bw.Write (NamespaceTableOffset);
		}

		internal SourceFileEntry (MonoSymbolFile file, BinaryReader reader)
		{
			this.file = file;

			Index = reader.ReadInt32 ();
			Count = reader.ReadInt32 ();
			NamespaceCount = reader.ReadInt32 ();
			NameOffset = reader.ReadInt32 ();
			MethodOffset = reader.ReadInt32 ();
			NamespaceTableOffset = reader.ReadInt32 ();

			file_name = file.ReadString (NameOffset);
		}

		public string FileName {
			get { return file_name; }
		}

		public MethodSourceEntry[] Methods {
			get {
				if (creating)
					throw new InvalidOperationException ();

				BinaryReader reader = file.BinaryReader;
				int old_pos = (int) reader.BaseStream.Position;

				reader.BaseStream.Position = MethodOffset;
				ArrayList list = new ArrayList ();
				for (int i = 0; i < Count; i ++)
					list.Add (new MethodSourceEntry (reader));
				reader.BaseStream.Position = old_pos;

				MethodSourceEntry[] retval = new MethodSourceEntry [Count];
				list.CopyTo (retval, 0);
				return retval;
			}
		}

		public override string ToString ()
		{
			return String.Format ("SourceFileEntry ({0}:{1}:{2})",
					      Index, file_name, Count);
		}
	}

	public struct MethodSourceEntry : IComparable
	{
		#region This is actually written to the symbol file
		public readonly int Index;
		public readonly int FileOffset;
		public readonly int StartRow;
		public readonly int EndRow;
		#endregion

		public MethodSourceEntry (int index, int file_offset, int start, int end)
		{
			this.Index = index;
			this.FileOffset = file_offset;
			this.StartRow = start;
			this.EndRow = end;
		}

		internal MethodSourceEntry (BinaryReader reader)
		{
			Index = reader.ReadInt32 ();
			FileOffset = reader.ReadInt32 ();
			StartRow = reader.ReadInt32 ();
			EndRow = reader.ReadInt32 ();
		}

		public static int Size {
			get { return 16; }
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Index);
			bw.Write (FileOffset);
			bw.Write (StartRow);
			bw.Write (EndRow);
		}

		public int CompareTo (object obj)
		{
			MethodSourceEntry method = (MethodSourceEntry) obj;

			if (method.StartRow < StartRow)
				return -1;
			else if (method.StartRow > StartRow)
				return 1;
			else
				return 0;
		}

		public override string ToString ()
		{
			return String.Format ("MethodSourceEntry ({0}:{1}:{2}:{3})",
					      Index, FileOffset, StartRow, EndRow);
		}
	}

	public struct MethodIndexEntry
	{
		#region This is actually written to the symbol file
		public readonly int FileOffset;
		public readonly int FullNameOffset;
		public readonly int Token;
		#endregion

		public static int Size {
			get { return 12; }
		}

		public MethodIndexEntry (int offset, int name_offset, int token)
		{
			this.FileOffset = offset;
			this.FullNameOffset = name_offset;
			this.Token = token;
		}

		internal MethodIndexEntry (BinaryReader reader)
		{
			FileOffset = reader.ReadInt32 ();
			FullNameOffset = reader.ReadInt32 ();
			Token = reader.ReadInt32 ();
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (FileOffset);
			bw.Write (FullNameOffset);
			bw.Write (Token);
		}

		public override string ToString ()
		{
			return String.Format ("MethodIndexEntry ({0}:{1}:{2:x})",
					      FileOffset, FullNameOffset, Token);
		}
	}

	public class MethodEntry
	{
		#region This is actually written to the symbol file
		public readonly int SourceFileIndex;
		public readonly int Token;
		public readonly int StartRow;
		public readonly int EndRow;
		public readonly int ClassTypeIndex;
		public readonly int NumParameters;
		public readonly int NumLocals;
		public readonly int NumLineNumbers;
		public readonly int NamespaceID;
		public readonly bool LocalNamesAmbiguous;

		int NameOffset;
		int FullNameOffset;
		int TypeIndexTableOffset;
		int LocalVariableTableOffset;
		int LineNumberTableOffset;
		int NumLexicalBlocks;
		int LexicalBlockTableOffset;
		#endregion

		int file_offset;
		string name;
		string full_name;

		public readonly int Index;
		public readonly SourceFileEntry SourceFile;
		public readonly LineNumberEntry[] LineNumbers;
		public readonly int[] ParamTypeIndices;
		public readonly int[] LocalTypeIndices;
		public readonly LocalVariableEntry[] Locals;
		public readonly Type[] LocalTypes;
		public readonly LexicalBlockEntry[] LexicalBlocks;

		public readonly MonoSymbolFile SymbolFile;

		public static int Size {
			get { return 52; }
		}

		public string Name {
			get { return name; }
		}

		public string FullName {
			get { return full_name; }
		}

		public MethodBase MethodBase {
			get { return MonoDebuggerSupport.GetMethod (SymbolFile.Assembly, Token); }
		}

		internal MethodEntry (MonoSymbolFile file, BinaryReader reader, int index)
		{
			this.SymbolFile = file;
			this.Index = index;
			SourceFileIndex = reader.ReadInt32 ();
			Token = reader.ReadInt32 ();
			StartRow = reader.ReadInt32 ();
			EndRow = reader.ReadInt32 ();
			ClassTypeIndex = reader.ReadInt32 ();
			NumParameters = reader.ReadInt32 ();
			NumLocals = reader.ReadInt32 ();
			NumLineNumbers = reader.ReadInt32 ();
			NameOffset = reader.ReadInt32 ();
			FullNameOffset = reader.ReadInt32 ();
			TypeIndexTableOffset = reader.ReadInt32 ();
			LocalVariableTableOffset = reader.ReadInt32 ();
			LineNumberTableOffset = reader.ReadInt32 ();
			NumLexicalBlocks = reader.ReadInt32 ();
			LexicalBlockTableOffset = reader.ReadInt32 ();
			NamespaceID = reader.ReadInt32 ();
			LocalNamesAmbiguous = reader.ReadInt32 () != 0;

			name = file.ReadString (NameOffset);
			full_name = file.ReadString (FullNameOffset);

			SourceFile = file.GetSourceFile (SourceFileIndex);

			if (LineNumberTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = LineNumberTableOffset;

				LineNumbers = new LineNumberEntry [NumLineNumbers];

				for (int i = 0; i < NumLineNumbers; i++)
					LineNumbers [i] = new LineNumberEntry (reader);

				reader.BaseStream.Position = old_pos;
			}

			if (LocalVariableTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = LocalVariableTableOffset;

				Locals = new LocalVariableEntry [NumLocals];
				LocalTypes = new Type [NumLocals];

				Assembly ass = file.Assembly;

				for (int i = 0; i < NumLocals; i++) {
					Locals [i] = new LocalVariableEntry (reader);
					LocalTypes [i] = MonoDebuggerSupport.GetLocalTypeFromSignature (
						ass, Locals [i].Signature);
				}

				reader.BaseStream.Position = old_pos;
			}

			if (TypeIndexTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = TypeIndexTableOffset;

				ParamTypeIndices = new int [NumParameters];
				LocalTypeIndices = new int [NumLocals];

				for (int i = 0; i < NumParameters; i++)
					ParamTypeIndices [i] = reader.ReadInt32 ();
				for (int i = 0; i < NumLocals; i++)
					LocalTypeIndices [i] = reader.ReadInt32 ();

				reader.BaseStream.Position = old_pos;
			}

			if (LexicalBlockTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = LexicalBlockTableOffset;

				LexicalBlocks = new LexicalBlockEntry [NumLexicalBlocks];
				for (int i = 0; i < NumLexicalBlocks; i++)
					LexicalBlocks [i] = new LexicalBlockEntry (i, reader);

				reader.BaseStream.Position = old_pos;
			}
		}

		internal MethodEntry (MonoSymbolFile file, SourceFileEntry source, MethodBase method,
				      int token, LocalVariableEntry[] locals, LineNumberEntry[] lines,
				      LexicalBlockEntry[] blocks, int start_row, int end_row,
				      int namespace_id)
		{
			this.SymbolFile = file;
			Index = file.GetNextMethodIndex ();

			Token = token;
			SourceFileIndex = source.Index;
			SourceFile = source;
			StartRow = start_row;
			EndRow = end_row;
			NamespaceID = namespace_id;
			LexicalBlocks = blocks;
			NumLexicalBlocks = LexicalBlocks.Length;

			LineNumbers = BuildLineNumberTable (lines);
			NumLineNumbers = LineNumbers.Length;

			ParameterInfo[] parameters = method.GetParameters ();
			if (parameters == null)
				parameters = new ParameterInfo [0];
			
			if (parameters.Length == 0)
				full_name = method.DeclaringType.FullName + "." + method.Name + "()";
			else if (parameters.Length == 1)
				full_name = method.DeclaringType.FullName + "." + method.Name + "(" + parameters [0].ParameterType.FullName +  ")";
			else if (parameters.Length == 2)
				full_name = method.DeclaringType.FullName + "." + method.Name + "(" + parameters [0].ParameterType.FullName + "," + parameters [1].ParameterType.FullName + ")";
			else {
				StringBuilder sb = new StringBuilder ();
				sb.Append (method.DeclaringType.FullName);
				sb.Append (".");
				sb.Append (method.Name);
				sb.Append ("(");
				for (int i = 0; i < parameters.Length; i++) {
					if (i > 0)
						sb.Append (",");
					sb.Append (parameters [i].ParameterType.FullName);
				}
				sb.Append (")");
				full_name = sb.ToString ();
			}

			name = method.Name;
			
			NumParameters = parameters.Length;
			ParamTypeIndices = new int [NumParameters];
			for (int i = 0; i < NumParameters; i++)
				ParamTypeIndices [i] = file.DefineType (parameters [i].ParameterType);

			NumLocals = locals.Length;
			Locals = locals;

			if (NumLocals <= 32) {
				// Most of the time, the O(n^2) factor is actually
				// less than the cost of allocating the hash table,
				// 32 is a rough number obtained through some testing.
				
				for (int i = 0; i < NumLocals; i ++) {
					string nm = locals [i].Name;
					
					for (int j = i + 1; j < NumLocals; j ++) {
						if (locals [j].Name == nm) {
							LocalNamesAmbiguous = true;
							goto locals_check_done;
						}
					}
				}
			locals_check_done :
				;
			} else {
				Hashtable local_names = new Hashtable ();
				foreach (LocalVariableEntry local in locals) {
					if (local_names.Contains (local.Name)) {
						LocalNamesAmbiguous = true;
						break;
					}
					local_names.Add (local.Name, local);
				}
			}

			LocalTypeIndices = new int [NumLocals];
			for (int i = 0; i < NumLocals; i++)
				LocalTypeIndices [i] = file.GetNextTypeIndex ();

			ClassTypeIndex = file.DefineType (method.ReflectedType);
		}

		// BuildLineNumberTable() eliminates duplicate line numbers and ensures
		// we aren't going "backwards" since this would counfuse the runtime's
		// debugging code (and the debugger).
		//
		// In the line number table, the "offset" field most be strictly
		// monotonic increasing; that is, the next entry must not have an offset
		// which is equal to or less than the current one.
		//
		// The most common case is that our input (ie. the line number table as
		// we get it from mcs) contains several entries with the same offset
		// (and different line numbers) - but it may also happen that the offset
		// is decreasing (this can be considered as an exception, such lines will
		// simply be discarded).
		LineNumberEntry[] BuildLineNumberTable (LineNumberEntry[] line_numbers)
		{
			ArrayList list = new ArrayList ();
			int last_offset = -1;
			int last_row = -1;

			for (int i = 0; i < line_numbers.Length; i++) {
				LineNumberEntry line = (LineNumberEntry) line_numbers [i];

				if (line.Offset > last_offset) {
					if (last_row >= 0)
						list.Add (new LineNumberEntry (last_row, last_offset));
					last_row = line.Row;
					last_offset = line.Offset;
				} else if (line.Row > last_row) {
					last_row = line.Row;
				}
			}

			if (last_row >= 0)
				list.Add (new LineNumberEntry (last_row, last_offset));

			LineNumberEntry[] retval = new LineNumberEntry [list.Count];
			list.CopyTo (retval, 0);
			return retval;
		}

		internal MethodSourceEntry Write (MonoSymbolFile file, BinaryWriter bw)
		{
			NameOffset = (int) bw.BaseStream.Position;
			file.WriteString (bw, name);

			FullNameOffset = (int) bw.BaseStream.Position;
			file.WriteString (bw, full_name);

			TypeIndexTableOffset = (int) bw.BaseStream.Position;

			for (int i = 0; i < NumParameters; i++)
				bw.Write (ParamTypeIndices [i]);
			for (int i = 0; i < NumLocals; i++)
				bw.Write (LocalTypeIndices [i]);

			LocalVariableTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < NumLocals; i++)
				Locals [i].Write (file, bw);
			file.LocalCount += NumLocals;

			LineNumberTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < NumLineNumbers; i++)
				LineNumbers [i].Write (bw);
			file.LineNumberCount += NumLineNumbers;

			LexicalBlockTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < NumLexicalBlocks; i++)
				LexicalBlocks [i].Write (bw);
			file_offset = (int) bw.BaseStream.Position;

			bw.Write (SourceFileIndex);
			bw.Write (Token);
			bw.Write (StartRow);
			bw.Write (EndRow);
			bw.Write (ClassTypeIndex);
			bw.Write (NumParameters);
			bw.Write (NumLocals);
			bw.Write (NumLineNumbers);
			bw.Write (NameOffset);
			bw.Write (FullNameOffset);
			bw.Write (TypeIndexTableOffset);
			bw.Write (LocalVariableTableOffset);
			bw.Write (LineNumberTableOffset);
			bw.Write (NumLexicalBlocks);
			bw.Write (LexicalBlockTableOffset);
			bw.Write (NamespaceID);
			bw.Write (LocalNamesAmbiguous ? 1 : 0);

			return new MethodSourceEntry (Index, file_offset, StartRow, EndRow);
		}

		internal void WriteIndex (BinaryWriter bw)
		{
			new MethodIndexEntry (file_offset, FullNameOffset, Token).Write (bw);
		}

		public override string ToString ()
		{
			return String.Format ("[Method {0}:{1}:{2}:{3}:{4} - {7}:{8}:{9}:{10} - {5} - {6}]",
					      Index, Token, SourceFileIndex, StartRow, EndRow,
					      SourceFile, FullName, ClassTypeIndex, NumParameters,
					      NumLocals, NumLineNumbers);
		}
	}

	public struct NamespaceEntry
	{
		#region This is actually written to the symbol file
		public readonly string Name;
		public readonly int Index;
		public readonly int Parent;
		public readonly string[] UsingClauses;
		#endregion

		public NamespaceEntry (string name, int index, string[] using_clauses, int parent)
		{
			this.Name = name;
			this.Index = index;
			this.Parent = parent;
			this.UsingClauses = using_clauses != null ? using_clauses : new string [0];
		}

		internal void Write (MonoSymbolFile file, BinaryWriter bw)
		{
			file.WriteString (bw, Name);
			bw.Write (Index);
			bw.Write (Parent);
			bw.Write (UsingClauses.Length);
			foreach (string uc in UsingClauses)
				file.WriteString (bw, uc);
		}

		public override string ToString ()
		{
			return String.Format ("[Namespace {0}:{1}:{2}]", Name, Index, Parent);
		}
	}
}

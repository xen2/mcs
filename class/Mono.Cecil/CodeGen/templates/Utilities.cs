//
// Utilities.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Generated by /CodeGen/cecil-gen.rb do not edit
// <%=Time.now%>
//
// (C) 2005 Jb Evain
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

namespace Mono.Cecil.Metadata {

	using System;
	using System.Collections;
	using System.IO;

	class Utilities {

		Utilities ()
		{
		}

		public static int ReadCompressedInteger (byte [] data, int pos, out int start)
		{
			int integer = 0;
			start = pos;
			if ((data [pos] & 0x80) == 0) {
				integer = data [pos];
				start++;
			} else if ((data [pos] & 0x40) == 0) {
				integer = (data [start] & ~0x80) << 8;
				integer |= data [pos + 1];
				start += 2;
			} else {
				integer = (data [start] & ~0xc0) << 24;
				integer |= data [pos + 1] << 16;
				integer |= data [pos + 2] << 8;
				integer |= data [pos + 3];
				start += 4;
			}
			return integer;
		}

		public static int WriteCompressedInteger (BinaryWriter writer, int value)
		{
			if (value < 0x80)
				writer.Write ((byte) value);
			else if (value < 0x4000) {
				writer.Write ((byte) (0x80 | (value >> 8)));
				writer.Write ((byte) (value & 0xff));
			} else {
				writer.Write ((byte) ((value >> 24) | 0xc0));
				writer.Write ((byte) ((value >> 16) & 0xff));
				writer.Write ((byte) ((value >> 8) & 0xff));
				writer.Write ((byte) (value & 0xff));
			}
			return (int) writer.BaseStream.Position;
		}

		public static MetadataToken GetMetadataToken (CodedIndex cidx, uint data)
		{
			uint rid = 0;
			switch (cidx) {
<% $coded_indexes.each { |ci| %>			case CodedIndex.<%=ci.name%> :
				rid = data >> <%=ci.size%>;
				switch (data & <%=(2 ** ci.size.to_i - 1).to_s%>) {
<%	ci.tables.each { |tbl|
		name = tbl.name
		if (name == "DeclSecurity")
			name = "Permission"
		elsif (name == "StandAloneSig")
			name = "Signature"
		end
%>				case <%=tbl.tag%> :
					return new MetadataToken (TokenType.<%=name%>, rid);
<%	}
%>				default :
					return MetadataToken.Zero;
				}
<% } %>			default :
				return MetadataToken.Zero;
			}
		}

		public static uint CompressMetadataToken (CodedIndex cidx, MetadataToken token)
		{
			uint ret = 0;
			if (token.RID == 0)
				return ret;
			switch (cidx) {
<% $coded_indexes.each { |ci| %>			case CodedIndex.<%=ci.name%> :
				ret = token.RID << <%=ci.size%>;
				switch (token.TokenType) {
<%	ci.tables.each { |tbl|
		name = tbl.name
		if (name == "DeclSecurity")
			name = "Permission"
		elsif (name == "StandAloneSig")
			name = "Signature"
		end
%>				case TokenType.<%=name%> :
					return ret | <%=tbl.tag%>;
<%	}
%>				default :
					throw new MetadataFormatException("Non valid Token for <%=ci.name%>");
				}
<% } %>			default :
				throw new MetadataFormatException ("Non valid CodedIndex");
			}
		}

		internal static Type GetCorrespondingTable (TokenType t)
		{
			switch (t) {
			case TokenType.Assembly :
				return typeof (AssemblyTable);
			case TokenType.AssemblyRef :
				return typeof (AssemblyRefTable);
			case TokenType.CustomAttribute :
				return typeof (CustomAttributeTable);
			case TokenType.Event :
				return typeof (EventTable);
			case TokenType.ExportedType :
				return typeof (ExportedTypeTable);
			case TokenType.Field :
				return typeof (FieldTable);
			case TokenType.File :
				return typeof (FileTable);
			case TokenType.InterfaceImpl :
				return typeof (InterfaceImplTable);
			case TokenType.MemberRef :
				return typeof (MemberRefTable);
			case TokenType.Method :
				return typeof (MethodTable);
			case TokenType.Module :
				return typeof (ModuleTable);
			case TokenType.ModuleRef :
				return typeof (ModuleRefTable);
			case TokenType.Param :
				return typeof (ParamTable);
			case TokenType.Permission :
				return typeof (DeclSecurityTable);
			case TokenType.Property :
				return typeof (PropertyTable);
			case TokenType.Signature :
				return typeof (StandAloneSigTable);
			case TokenType.TypeDef :
				return typeof (TypeDefTable);
			case TokenType.TypeRef :
				return typeof (TypeRefTable);
			case TokenType.TypeSpec :
				return typeof (TypeSpecTable);
			default :
				return null;
			}
		}

		internal delegate int TableRowCounter (int rid);

		internal static int GetCodedIndexSize (CodedIndex ci, TableRowCounter rowCounter, int [] codedIndexCache)
		{
			int bits = 0, max = 0, index = (int) ci;
			if (codedIndexCache [index] != 0)
				return codedIndexCache [index];

			int res = 0;
			int [] rids;
			switch (ci) {
<% $coded_indexes.each { |ci| %>			case CodedIndex.<%=ci.name%> :
				bits = <%=ci.size%>;
				rids = new int [<%=ci.tables.length%>];
<%	ci.tables.each_with_index { |tbl, i|
%>				rids [<%=i%>] = <%=tbl.name%>Table.RId;
<%	}
%>				break;
<% } %>			default :
				throw new MetadataFormatException ("Non valid CodedIndex");
			}

			for (int i = 0; i < rids.Length; i++) {
				int rows = rowCounter (rids [i]);
				if (rows > max) max = rows;
			}

			res = max < (1 << (16 - bits)) ? 2 : 4;
			codedIndexCache [index] = res;
			return res;
		}
	}
}

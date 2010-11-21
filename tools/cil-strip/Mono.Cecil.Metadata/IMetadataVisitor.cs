//
// IMetadataVisitor.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Generated by /CodeGen/cecil-gen.rb do not edit
// Thu Feb 22 14:39:38 CET 2007
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

	internal interface IMetadataVisitor {
		void VisitMetadataRoot (MetadataRoot root);
		void VisitMetadataRootHeader (MetadataRoot.MetadataRootHeader header);
		void VisitMetadataStreamCollection (MetadataStreamCollection streams);
		void VisitMetadataStream (MetadataStream stream);
		void VisitMetadataStreamHeader (MetadataStream.MetadataStreamHeader header);
		void VisitGuidHeap (GuidHeap heap);
		void VisitStringsHeap (StringsHeap heap);
		void VisitTablesHeap (TablesHeap heap);
		void VisitBlobHeap (BlobHeap heap);
		void VisitUserStringsHeap (UserStringsHeap heap);

		void TerminateMetadataRoot (MetadataRoot root);
	}

	internal interface IMetadataTableVisitor {
		void VisitTableCollection (TableCollection coll);

		void VisitAssemblyTable (AssemblyTable table);
		void VisitAssemblyOSTable (AssemblyOSTable table);
		void VisitAssemblyProcessorTable (AssemblyProcessorTable table);
		void VisitAssemblyRefTable (AssemblyRefTable table);
		void VisitAssemblyRefOSTable (AssemblyRefOSTable table);
		void VisitAssemblyRefProcessorTable (AssemblyRefProcessorTable table);
		void VisitClassLayoutTable (ClassLayoutTable table);
		void VisitConstantTable (ConstantTable table);
		void VisitCustomAttributeTable (CustomAttributeTable table);
		void VisitDeclSecurityTable (DeclSecurityTable table);
		void VisitEventTable (EventTable table);
		void VisitEventMapTable (EventMapTable table);
		void VisitEventPtrTable (EventPtrTable table);
		void VisitExportedTypeTable (ExportedTypeTable table);
		void VisitFieldTable (FieldTable table);
		void VisitFieldLayoutTable (FieldLayoutTable table);
		void VisitFieldMarshalTable (FieldMarshalTable table);
		void VisitFieldPtrTable (FieldPtrTable table);
		void VisitFieldRVATable (FieldRVATable table);
		void VisitFileTable (FileTable table);
		void VisitGenericParamTable (GenericParamTable table);
		void VisitGenericParamConstraintTable (GenericParamConstraintTable table);
		void VisitImplMapTable (ImplMapTable table);
		void VisitInterfaceImplTable (InterfaceImplTable table);
		void VisitManifestResourceTable (ManifestResourceTable table);
		void VisitMemberRefTable (MemberRefTable table);
		void VisitMethodTable (MethodTable table);
		void VisitMethodImplTable (MethodImplTable table);
		void VisitMethodPtrTable (MethodPtrTable table);
		void VisitMethodSemanticsTable (MethodSemanticsTable table);
		void VisitMethodSpecTable (MethodSpecTable table);
		void VisitModuleTable (ModuleTable table);
		void VisitModuleRefTable (ModuleRefTable table);
		void VisitNestedClassTable (NestedClassTable table);
		void VisitParamTable (ParamTable table);
		void VisitParamPtrTable (ParamPtrTable table);
		void VisitPropertyTable (PropertyTable table);
		void VisitPropertyMapTable (PropertyMapTable table);
		void VisitPropertyPtrTable (PropertyPtrTable table);
		void VisitStandAloneSigTable (StandAloneSigTable table);
		void VisitTypeDefTable (TypeDefTable table);
		void VisitTypeRefTable (TypeRefTable table);
		void VisitTypeSpecTable (TypeSpecTable table);

		void TerminateTableCollection (TableCollection coll);
		IMetadataRowVisitor GetRowVisitor();
}

	internal interface IMetadataRowVisitor {
		void VisitRowCollection (RowCollection coll);

		void VisitAssemblyRow (AssemblyRow row);
		void VisitAssemblyOSRow (AssemblyOSRow row);
		void VisitAssemblyProcessorRow (AssemblyProcessorRow row);
		void VisitAssemblyRefRow (AssemblyRefRow row);
		void VisitAssemblyRefOSRow (AssemblyRefOSRow row);
		void VisitAssemblyRefProcessorRow (AssemblyRefProcessorRow row);
		void VisitClassLayoutRow (ClassLayoutRow row);
		void VisitConstantRow (ConstantRow row);
		void VisitCustomAttributeRow (CustomAttributeRow row);
		void VisitDeclSecurityRow (DeclSecurityRow row);
		void VisitEventRow (EventRow row);
		void VisitEventMapRow (EventMapRow row);
		void VisitEventPtrRow (EventPtrRow row);
		void VisitExportedTypeRow (ExportedTypeRow row);
		void VisitFieldRow (FieldRow row);
		void VisitFieldLayoutRow (FieldLayoutRow row);
		void VisitFieldMarshalRow (FieldMarshalRow row);
		void VisitFieldPtrRow (FieldPtrRow row);
		void VisitFieldRVARow (FieldRVARow row);
		void VisitFileRow (FileRow row);
		void VisitGenericParamRow (GenericParamRow row);
		void VisitGenericParamConstraintRow (GenericParamConstraintRow row);
		void VisitImplMapRow (ImplMapRow row);
		void VisitInterfaceImplRow (InterfaceImplRow row);
		void VisitManifestResourceRow (ManifestResourceRow row);
		void VisitMemberRefRow (MemberRefRow row);
		void VisitMethodRow (MethodRow row);
		void VisitMethodImplRow (MethodImplRow row);
		void VisitMethodPtrRow (MethodPtrRow row);
		void VisitMethodSemanticsRow (MethodSemanticsRow row);
		void VisitMethodSpecRow (MethodSpecRow row);
		void VisitModuleRow (ModuleRow row);
		void VisitModuleRefRow (ModuleRefRow row);
		void VisitNestedClassRow (NestedClassRow row);
		void VisitParamRow (ParamRow row);
		void VisitParamPtrRow (ParamPtrRow row);
		void VisitPropertyRow (PropertyRow row);
		void VisitPropertyMapRow (PropertyMapRow row);
		void VisitPropertyPtrRow (PropertyPtrRow row);
		void VisitStandAloneSigRow (StandAloneSigRow row);
		void VisitTypeDefRow (TypeDefRow row);
		void VisitTypeRefRow (TypeRefRow row);
		void VisitTypeSpecRow (TypeSpecRow row);

		void TerminateRowCollection (RowCollection coll);
	}
}

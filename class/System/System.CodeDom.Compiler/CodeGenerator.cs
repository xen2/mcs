//
// System.CodeDom.Compiler.CodeGenerator.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001-2003 Ximian, Inc.
//

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

using System.Globalization;
using System.CodeDom;
using System.Reflection;
using System.IO;
using System.Collections;
	
namespace System.CodeDom.Compiler {

	public abstract class CodeGenerator : ICodeGenerator
	{
		private IndentedTextWriter output;
		private CodeGeneratorOptions options;
		private CodeTypeMember currentMember;
		private CodeTypeDeclaration currentType;

		//
		// Constructors
		//
		protected CodeGenerator()
		{
		}

		//
		// Properties
		//
#if NET_2_0
		protected CodeTypeDeclaration CurrentClass {
			get {
				return currentType;
			}
		}
#endif

		protected CodeTypeMember CurrentMember {
			get {
				return currentMember;
			}
		}
		
		protected string CurrentMemberName {
			get {
				if (currentMember == null)
					return "<% unknown %>";
				return currentMember.Name;
			}
		}

		protected string CurrentTypeName {
			get {
				if (currentType == null)
					return "<% unknown %>";
				return currentType.Name;
			}
		}
		
		protected int Indent {
			get {
				return output.Indent;
			}
			set {
				output.Indent = value;
			}
		}
		
		protected bool IsCurrentClass {
			get {
				if (currentType == null)
					return false;
				return currentType.IsClass && !(currentType is CodeTypeDelegate);
			}
		}

		protected bool IsCurrentDelegate {
			get {
				return currentType is CodeTypeDelegate;
			}
		}

		protected bool IsCurrentEnum {
			get {
				if (currentType == null)
					return false;
				return currentType.IsEnum;
			}
		}

		protected bool IsCurrentInterface {
			get {
				if (currentType == null)
					return false;
				return currentType.IsInterface;
			}
		}

		protected bool IsCurrentStruct {
			get {
				if (currentType == null)
					return false;
				return currentType.IsStruct;
			}
		}

		protected abstract string NullToken {
			get;
		}
		
		
		protected CodeGeneratorOptions Options {
			get {
				return options;
			}
		}
			
		protected TextWriter Output {
			get {
				return output;
			}
		}

		//
		// Methods
		//
		protected virtual void ContinueOnNewLine (string st)
		{
			output.WriteLine (st);
		}

		/*
		 * Code Generation methods
		 */
		protected abstract void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression e);
		protected abstract void GenerateArrayCreateExpression (CodeArrayCreateExpression e);
		protected abstract void GenerateArrayIndexerExpression (CodeArrayIndexerExpression e);
		protected abstract void GenerateAssignStatement (CodeAssignStatement s);
		protected abstract void GenerateAttachEventStatement (CodeAttachEventStatement s);
		protected abstract void GenerateAttributeDeclarationsStart (CodeAttributeDeclarationCollection attributes);
		protected abstract void GenerateAttributeDeclarationsEnd (CodeAttributeDeclarationCollection attributes);
		protected abstract void GenerateBaseReferenceExpression (CodeBaseReferenceExpression e);

		protected virtual void GenerateBinaryOperatorExpression (CodeBinaryOperatorExpression e)
		{
			output.Write ('(');
			GenerateExpression (e.Left);
			output.Write (' ');
			OutputOperator (e.Operator);
			output.Write (' ');
			GenerateExpression (e.Right);
			output.Write (')');
		}

		protected abstract void GenerateCastExpression (CodeCastExpression e);
#if NET_2_0
		[MonoTODO]
		public virtual void GenerateCodeFromMember (CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
		{
			throw new NotImplementedException ();
		}
#endif
		protected abstract void GenerateComment (CodeComment comment);

		protected virtual void GenerateCommentStatement (CodeCommentStatement statement)
		{
			GenerateComment (statement.Comment);
		}

		protected virtual void GenerateCommentStatements (CodeCommentStatementCollection statements)
		{
			foreach (CodeCommentStatement comment in statements)
				GenerateCommentStatement (comment);
		}

		protected virtual void GenerateCompileUnit (CodeCompileUnit compileUnit)
		{
			GenerateCompileUnitStart (compileUnit);

			CodeAttributeDeclarationCollection attributes = compileUnit.AssemblyCustomAttributes;
			if (attributes.Count != 0) {
				foreach (CodeAttributeDeclaration att in attributes) {
					GenerateAttributeDeclarationsStart (attributes);
					output.Write ("assembly: ");
					OutputAttributeDeclaration (att);
					GenerateAttributeDeclarationsEnd (attributes);
				}
				output.WriteLine ();
			}

			foreach (CodeNamespace ns in compileUnit.Namespaces)
				GenerateNamespace (ns);

			GenerateCompileUnitEnd (compileUnit);
		}

		protected virtual void GenerateCompileUnitEnd (CodeCompileUnit compileUnit)
		{
#if NET_2_0
			if (compileUnit.EndDirectives.Count > 0)
				GenerateDirectives (compileUnit.EndDirectives);
#endif
		}

		protected virtual void GenerateCompileUnitStart (CodeCompileUnit compileUnit)
		{
#if NET_2_0
			if (compileUnit.StartDirectives.Count > 0) {
				GenerateDirectives (compileUnit.StartDirectives);
				Output.WriteLine ();
			}
#endif
		}

		protected abstract void GenerateConditionStatement (CodeConditionStatement s);
		protected abstract void GenerateConstructor (CodeConstructor x, CodeTypeDeclaration d);

		protected virtual void GenerateDecimalValue (Decimal d)
		{
			Output.Write (d.ToString (CultureInfo.InvariantCulture));
		}
#if NET_2_0
		[MonoTODO]
		protected virtual void GenerateDefaultValueExpression (CodeDefaultValueExpression e)
		{
			throw new NotImplementedException ();
		}
#endif
		protected abstract void GenerateDelegateCreateExpression (CodeDelegateCreateExpression e);
		protected abstract void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression e);

		protected virtual void GenerateDirectionExpression (CodeDirectionExpression e)
		{
			OutputDirection (e.Direction);
			output.Write (' ');
			GenerateExpression (e.Expression);
		}

		protected virtual void GenerateDoubleValue (Double d)
		{
			Output.Write (d.ToString (CultureInfo.InvariantCulture));
		}

		protected abstract void GenerateEntryPointMethod (CodeEntryPointMethod m, CodeTypeDeclaration d);
		protected abstract void GenerateEvent (CodeMemberEvent ev, CodeTypeDeclaration d);
		protected abstract void GenerateEventReferenceExpression (CodeEventReferenceExpression e);

		protected void GenerateExpression (CodeExpression e)
		{
			if (e == null)
				throw new ArgumentNullException ("e");

			CodeArgumentReferenceExpression argref = e as CodeArgumentReferenceExpression;
			if (argref != null) {
				GenerateArgumentReferenceExpression (argref);
				return;
			}
			CodeArrayCreateExpression mkarray = e as CodeArrayCreateExpression;
			if (mkarray != null) {
				GenerateArrayCreateExpression (mkarray);
				return;
			}
			CodeArrayIndexerExpression arrayidx = e as CodeArrayIndexerExpression;
			if (arrayidx != null) {
				GenerateArrayIndexerExpression (arrayidx);
				return;
			}
			CodeBaseReferenceExpression baseref = e as CodeBaseReferenceExpression;
			if (baseref != null) {
				GenerateBaseReferenceExpression (baseref);
				return;
			}
			CodeBinaryOperatorExpression binary = e as CodeBinaryOperatorExpression;
			if (binary != null) {
				GenerateBinaryOperatorExpression (binary);
				return;
			}
			CodeCastExpression cast = e as CodeCastExpression;
			if (cast != null) {
				GenerateCastExpression (cast);
				return;
			}
			CodeDelegateCreateExpression mkdel = e as CodeDelegateCreateExpression;
			if (mkdel != null) {
				GenerateDelegateCreateExpression (mkdel);
				return;
			}
			CodeDelegateInvokeExpression delinvoke = e as CodeDelegateInvokeExpression;
			if (delinvoke != null) {
				GenerateDelegateInvokeExpression (delinvoke);
				return;
			}
			CodeDirectionExpression direction = e as CodeDirectionExpression;
			if (direction != null) {
				GenerateDirectionExpression (direction);
				return;
			}
			CodeEventReferenceExpression eventref = e as CodeEventReferenceExpression;
			if ( eventref != null ) {
				GenerateEventReferenceExpression( eventref );
				return;
			}
			CodeFieldReferenceExpression fieldref = e as CodeFieldReferenceExpression;
			if (fieldref != null) {
				GenerateFieldReferenceExpression (fieldref);
				return;
			}
			CodeIndexerExpression idx = e as CodeIndexerExpression;
			if (idx != null) {
				GenerateIndexerExpression (idx);
				return;
			}
			CodeMethodInvokeExpression methodinv = e as CodeMethodInvokeExpression;
			if (methodinv != null) {
				GenerateMethodInvokeExpression (methodinv);
				return;
			}
			CodeMethodReferenceExpression methodref = e as CodeMethodReferenceExpression;
			if (methodref != null) {
				GenerateMethodReferenceExpression (methodref);
				return;
			}
			CodeObjectCreateExpression objref = e as CodeObjectCreateExpression;
			if (objref != null) {
				GenerateObjectCreateExpression (objref);
				return;
			}
			CodeParameterDeclarationExpression param = e as CodeParameterDeclarationExpression;
			if (param != null) {
				GenerateParameterDeclarationExpression (param);
				return;
			}
			CodePrimitiveExpression primitive = e as CodePrimitiveExpression;
			if (primitive != null) {
				GeneratePrimitiveExpression (primitive);
				return;
			}
			CodePropertyReferenceExpression propref = e as CodePropertyReferenceExpression;
			if (propref != null) {
				GeneratePropertyReferenceExpression (propref);
				return;
			}
			CodePropertySetValueReferenceExpression propset = e as CodePropertySetValueReferenceExpression;
			if (propset != null) {
				GeneratePropertySetValueReferenceExpression (propset);
				return;
			}
			CodeSnippetExpression snippet = e as CodeSnippetExpression;
			if (snippet != null) {
				GenerateSnippetExpression (snippet);
				return;
			}
			CodeThisReferenceExpression thisref = e as CodeThisReferenceExpression;
			if (thisref != null) {
				GenerateThisReferenceExpression (thisref);
				return;
			}
			CodeTypeOfExpression typeOf = e as CodeTypeOfExpression;
			if (typeOf != null) {
				GenerateTypeOfExpression (typeOf);
				return;
			}
			CodeTypeReferenceExpression typeref = e as CodeTypeReferenceExpression;
			if (typeref != null) {
				GenerateTypeReferenceExpression (typeref);
				return;
			}
			CodeVariableReferenceExpression varref = e as CodeVariableReferenceExpression;
			if (varref != null) {
				GenerateVariableReferenceExpression (varref);
				return;
			}

			throw new ArgumentException ("Element type " + e + " is not supported.", "e");
		}

		protected abstract void GenerateExpressionStatement (CodeExpressionStatement statement);
		protected abstract void GenerateField (CodeMemberField f);
		protected abstract void GenerateFieldReferenceExpression (CodeFieldReferenceExpression e);
		protected abstract void GenerateGotoStatement (CodeGotoStatement statement);
		protected abstract void GenerateIndexerExpression (CodeIndexerExpression e);
		protected abstract void GenerateIterationStatement (CodeIterationStatement s);
		protected abstract void GenerateLabeledStatement (CodeLabeledStatement statement);
		protected abstract void GenerateLinePragmaStart (CodeLinePragma p);
		protected abstract void GenerateLinePragmaEnd (CodeLinePragma p);
		protected abstract void GenerateMethod (CodeMemberMethod m, CodeTypeDeclaration d);
		protected abstract void GenerateMethodInvokeExpression (CodeMethodInvokeExpression e);
		protected abstract void GenerateMethodReferenceExpression (CodeMethodReferenceExpression e);
		protected abstract void GenerateMethodReturnStatement (CodeMethodReturnStatement e);

		protected virtual void GenerateNamespace (CodeNamespace ns)
		{
			foreach (CodeCommentStatement statement in ns.Comments)
				GenerateCommentStatement (statement);

			GenerateNamespaceStart (ns);

			foreach (CodeNamespaceImport import in ns.Imports) {
				if (import.LinePragma != null)
					GenerateLinePragmaStart (import.LinePragma);

				GenerateNamespaceImport (import);

				if (import.LinePragma != null)
					GenerateLinePragmaEnd (import.LinePragma);
			}

			output.WriteLine();

			GenerateTypes (ns);

			GenerateNamespaceEnd (ns);
		}

		protected abstract void GenerateNamespaceStart (CodeNamespace ns);
		protected abstract void GenerateNamespaceEnd (CodeNamespace ns);
		protected abstract void GenerateNamespaceImport (CodeNamespaceImport i);
		protected void GenerateNamespaceImports (CodeNamespace e)
		{
			foreach (CodeNamespaceImport import in e.Imports) {
				if (import.LinePragma != null)
					GenerateLinePragmaStart (import.LinePragma);

				GenerateNamespaceImport (import);

				if (import.LinePragma != null)
					GenerateLinePragmaEnd (import.LinePragma);
			}
		}

		protected void GenerateNamespaces (CodeCompileUnit e)
		{
			foreach (CodeNamespace ns in e.Namespaces)
				GenerateNamespace (ns);
		}

		protected abstract void GenerateObjectCreateExpression (CodeObjectCreateExpression e);

		protected virtual void GenerateParameterDeclarationExpression (CodeParameterDeclarationExpression e)
		{
			if (e.CustomAttributes != null && e.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (e.CustomAttributes);
			OutputDirection (e.Direction);
			OutputType (e.Type);
			output.Write (' ');
			output.Write (e.Name);
		}

		protected virtual void GeneratePrimitiveExpression (CodePrimitiveExpression e)
		{
			if (e.Value == null) {
				output.Write (NullToken);
				return;
			}

			Type type = e.Value.GetType ();
			if (type == typeof (bool)) {
				output.Write (e.Value.ToString ().ToLower (CultureInfo.InvariantCulture));
			} else if (type == typeof (char)) {
				output.Write ("'" + e.Value.ToString () + "'");
			} else if (type == typeof (string)) {
				output.Write (QuoteSnippetString ((string) e.Value));
			} else if (type == typeof (float)) {
				GenerateSingleFloatValue((float) e.Value);
			} else if (type == typeof (double)) {
				GenerateDoubleValue((double) e.Value);
			} else if (type == typeof (decimal)) {
				this.GenerateDecimalValue((decimal) e.Value);
			} else if (type == typeof (byte) || type == typeof (short) || 
				type == typeof (int) || type == typeof (long)) {
				// All of these should be IFormatable, I am just being safe/slow 
				IFormattable formattable = e.Value as IFormattable;
				if (formattable != null) {
					output.Write (formattable.ToString (null, CultureInfo.InvariantCulture));
				} else {
					output.Write (e.Value.ToString ());
				}
			} else {
				throw new ArgumentException (string.Format(CultureInfo.InvariantCulture,
					"Invalid Primitive Type: {0}. Only CLS compliant primitive " +
					"types can be used. Consider using CodeObjectCreateExpression.",
					type.FullName));
			}
		}

		protected abstract void GenerateProperty (CodeMemberProperty p, CodeTypeDeclaration d);
		protected abstract void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression e);
		protected abstract void GeneratePropertySetValueReferenceExpression (CodePropertySetValueReferenceExpression e);
		protected abstract void GenerateRemoveEventStatement (CodeRemoveEventStatement statement);

		protected virtual void GenerateSingleFloatValue (Single s)
		{
			output.Write (s.ToString(CultureInfo.InvariantCulture));
		}

		protected virtual void GenerateSnippetCompileUnit (CodeSnippetCompileUnit e)
		{
			if (e.LinePragma != null)
				GenerateLinePragmaStart (e.LinePragma);

			output.WriteLine (e.Value);

			if (e.LinePragma != null)
				GenerateLinePragmaEnd (e.LinePragma);

		}

		protected abstract void GenerateSnippetExpression (CodeSnippetExpression e);
		protected abstract void GenerateSnippetMember (CodeSnippetTypeMember m);
		protected virtual void GenerateSnippetStatement (CodeSnippetStatement s)
		{
			output.WriteLine (s.Value);
		}

		protected void GenerateStatement (CodeStatement s)
		{
			bool handled = false;

#if NET_2_0
			if (s.StartDirectives.Count > 0)
				GenerateDirectives (s.StartDirectives);
#endif
			if (s.LinePragma != null)
				GenerateLinePragmaStart (s.LinePragma);

			CodeAssignStatement assign = s as CodeAssignStatement;
			if (assign != null) {
				GenerateAssignStatement (assign);
				handled = true;
			}
			CodeAttachEventStatement attach = s as CodeAttachEventStatement;
			if (attach != null) {
				GenerateAttachEventStatement (attach);
				handled = true;
			}
			CodeCommentStatement comment = s as CodeCommentStatement;
			if (comment != null) {
				GenerateCommentStatement (comment);
				handled = true;
			}
			CodeConditionStatement condition = s as CodeConditionStatement;
			if (condition != null) {
				GenerateConditionStatement (condition);
				handled = true;
			}
			CodeExpressionStatement expression = s as CodeExpressionStatement;
			if (expression != null) {
				GenerateExpressionStatement (expression);
				handled = true;
			}
			CodeGotoStatement gotostmt = s as CodeGotoStatement;
			if (gotostmt != null) {
				GenerateGotoStatement (gotostmt);
				handled = true;
			}
			CodeIterationStatement iteration = s as CodeIterationStatement;
			if (iteration != null) {
				GenerateIterationStatement (iteration);
				handled = true;
			}
			CodeLabeledStatement label = s as CodeLabeledStatement;
			if (label != null) {
				GenerateLabeledStatement (label);
				handled = true;
			}
			CodeMethodReturnStatement returnstmt = s as CodeMethodReturnStatement;
			if (returnstmt != null) {
				GenerateMethodReturnStatement (returnstmt);
				handled = true;
			}
			CodeRemoveEventStatement remove = s as CodeRemoveEventStatement;
			if (remove != null) {
				GenerateRemoveEventStatement (remove);
				handled = true;
			}
			CodeSnippetStatement snippet = s as CodeSnippetStatement;
			if (snippet != null) {
#if NET_2_0
				int indent = Indent;
				try {
					Indent = 0;
					GenerateSnippetStatement (snippet);
				} finally {
					Indent = indent;
				}
#else
				GenerateSnippetStatement (snippet);
#endif
				handled = true;
			}
			CodeThrowExceptionStatement exception = s as CodeThrowExceptionStatement;
			if (exception != null) {
				GenerateThrowExceptionStatement (exception);
				handled = true;
			}
			CodeTryCatchFinallyStatement trycatch = s as CodeTryCatchFinallyStatement;
			if (trycatch != null) {
				GenerateTryCatchFinallyStatement (trycatch);
				handled = true;
			}
			CodeVariableDeclarationStatement declaration = s as CodeVariableDeclarationStatement;
			if (declaration != null) {
				GenerateVariableDeclarationStatement (declaration);
				handled = true;
			}

			if (!handled)
				throw new ArgumentException ("Element type " + s + " is not supported.");

			if (s.LinePragma != null)
				GenerateLinePragmaEnd (s.LinePragma);

#if NET_2_0
			if (s.EndDirectives.Count > 0)
				GenerateDirectives (s.EndDirectives);
#endif			
		}

		protected void GenerateStatements (CodeStatementCollection c)
		{
			foreach (CodeStatement statement in c)
				GenerateStatement (statement);
		}

		protected abstract void GenerateThisReferenceExpression (CodeThisReferenceExpression e);
		protected abstract void GenerateThrowExceptionStatement (CodeThrowExceptionStatement s);
		protected abstract void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement s);
		protected abstract void GenerateTypeEnd (CodeTypeDeclaration declaration);
		protected abstract void GenerateTypeConstructor (CodeTypeConstructor constructor);

		protected virtual void GenerateTypeOfExpression (CodeTypeOfExpression e)
		{
			output.Write ("typeof(");
			OutputType (e.Type);
			output.Write (")");
		}

		protected virtual void GenerateTypeReferenceExpression (CodeTypeReferenceExpression e)
		{
			OutputType (e.Type);
		}

		protected void GenerateTypes (CodeNamespace e)
		{
			foreach (CodeTypeDeclaration type in e.Types) {
				if (options.BlankLinesBetweenMembers)
					output.WriteLine ();

				GenerateType (type);
			}
		}

		protected abstract void GenerateTypeStart (CodeTypeDeclaration declaration);
		protected abstract void GenerateVariableDeclarationStatement (CodeVariableDeclarationStatement e);
		protected abstract void GenerateVariableReferenceExpression (CodeVariableReferenceExpression e);

		//
		// Other members
		//
		
		/*
		 * Output Methods
		 */
		protected virtual void OutputAttributeArgument (CodeAttributeArgument argument)
		{
			string name = argument.Name;
			if ((name != null) && (name.Length > 0)) {
				output.Write (name);
				output.Write ('=');
			}
			GenerateExpression (argument.Value);
		}

		private void OutputAttributeDeclaration (CodeAttributeDeclaration attribute)
		{
			output.Write (attribute.Name.Replace ('+', '.'));
			output.Write ('(');
			IEnumerator enumerator = attribute.Arguments.GetEnumerator();
			if (enumerator.MoveNext()) {
				CodeAttributeArgument argument = (CodeAttributeArgument)enumerator.Current;
				OutputAttributeArgument (argument);
				
				while (enumerator.MoveNext()) {
					output.Write (',');
					argument = (CodeAttributeArgument)enumerator.Current;
					OutputAttributeArgument (argument);
				}
			}
			output.Write (')');
		}

		protected virtual void OutputAttributeDeclarations (CodeAttributeDeclarationCollection attributes)
		{
			GenerateAttributeDeclarationsStart (attributes);
			
			IEnumerator enumerator = attributes.GetEnumerator();
			if (enumerator.MoveNext()) {
				CodeAttributeDeclaration attribute = (CodeAttributeDeclaration)enumerator.Current;

				OutputAttributeDeclaration (attribute);
				
				while (enumerator.MoveNext()) {
					attribute = (CodeAttributeDeclaration)enumerator.Current;

					output.WriteLine (',');
					OutputAttributeDeclaration (attribute);
				}
			}

			GenerateAttributeDeclarationsEnd (attributes);
		}

		protected virtual void OutputDirection (FieldDirection direction)
		{
			switch (direction) {
			case FieldDirection.In:
				//output.Write ("in ");
				break;
			case FieldDirection.Out:
				output.Write ("out ");
				break;
			case FieldDirection.Ref:
				output.Write ("ref ");
				break;
			}
		}

		protected virtual void OutputExpressionList (CodeExpressionCollection expressions)
		{
			OutputExpressionList (expressions, false);
		}

		protected virtual void OutputExpressionList (CodeExpressionCollection expressions,
							     bool newLineBetweenItems)
		{
			++Indent;
			IEnumerator enumerator = expressions.GetEnumerator();
			if (enumerator.MoveNext()) {
				CodeExpression expression = (CodeExpression)enumerator.Current;

				GenerateExpression (expression);
				
				while (enumerator.MoveNext()) {
					expression = (CodeExpression)enumerator.Current;
					
					output.Write (',');
					if (newLineBetweenItems)
						output.WriteLine ();
					else
						output.Write (' ');
					
					GenerateExpression (expression);
				}
			}
			--Indent;
		}

		protected virtual void OutputFieldScopeModifier (MemberAttributes attributes)
		{
			if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New)
				output.Write ("new ");

			switch (attributes & MemberAttributes.ScopeMask) {
			case MemberAttributes.Static:
				output.Write ("static ");
				break;
			case MemberAttributes.Const:
				output.Write ("const ");
				break;
			}
		}

		protected virtual void OutputIdentifier (string ident)
		{
			output.Write (ident);
		}

		protected virtual void OutputMemberAccessModifier (MemberAttributes attributes)
		{
			switch (attributes & MemberAttributes.AccessMask) {
				case MemberAttributes.Assembly:
					output.Write ("internal ");
					break;
				case MemberAttributes.FamilyAndAssembly:
#if NET_2_0
					output.Write ("internal "); 
#else
					output.Write ("/*FamANDAssem*/ internal "); 
#endif
					break;
				case MemberAttributes.Family:
					output.Write ("protected ");
					break;
				case MemberAttributes.FamilyOrAssembly:
					output.Write ("protected internal ");
					break;
				case MemberAttributes.Private:
					output.Write ("private ");
					break;
				case MemberAttributes.Public:
					output.Write ("public ");
					break;
			}
		}

		protected virtual void OutputMemberScopeModifier (MemberAttributes attributes)
		{
#if NET_2_0
			if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New)
				output.Write( "new " );
#endif

			switch (attributes & MemberAttributes.ScopeMask) {
				case MemberAttributes.Abstract:
					output.Write ("abstract ");
					break;
				case MemberAttributes.Final:
					// Do nothing
					break;
				case MemberAttributes.Static:
					output.Write ("static ");
					break;
				case MemberAttributes.Override:
					output.Write ("override ");
					break;
				default:
					//
					// FUNNY! if the scope value is
					// rubbish (0 or >Const), and access
					// is public or protected, make it
					// "virtual".
					//
					// i'm not sure whether this is 100%
					// correct, but it seems to be MS
					// behavior.
					//
					// On .NET 2.0, internal members
					// are also marked "virtual".
					//
					MemberAttributes access = attributes & MemberAttributes.AccessMask;
					if (access == MemberAttributes.Public || access == MemberAttributes.Family)
						output.Write ("virtual ");
					break;
			}
		}
				
		protected virtual void OutputOperator (CodeBinaryOperatorType op)
		{
			switch (op) {
			case CodeBinaryOperatorType.Add:
				output.Write ("+");
				break;
			case CodeBinaryOperatorType.Subtract:
				output.Write ("-");
				break;
			case CodeBinaryOperatorType.Multiply:
				output.Write ("*");
				break;
			case CodeBinaryOperatorType.Divide:
				output.Write ("/");
				break;
			case CodeBinaryOperatorType.Modulus:
				output.Write ("%");
				break;
			case CodeBinaryOperatorType.Assign:
				output.Write ("=");
				break;
			case CodeBinaryOperatorType.IdentityInequality:
				output.Write ("!=");
				break;
			case CodeBinaryOperatorType.IdentityEquality:
				output.Write ("==");
				break;
			case CodeBinaryOperatorType.ValueEquality:
				output.Write ("==");
				break;
			case CodeBinaryOperatorType.BitwiseOr:
				output.Write ("|");
				break;
			case CodeBinaryOperatorType.BitwiseAnd:
				output.Write ("&");
				break;
			case CodeBinaryOperatorType.BooleanOr:
				output.Write ("||");
				break;
			case CodeBinaryOperatorType.BooleanAnd:
				output.Write ("&&");
				break;
			case CodeBinaryOperatorType.LessThan:
				output.Write ("<");
				break;
			case CodeBinaryOperatorType.LessThanOrEqual:
				output.Write ("<=");
				break;
			case CodeBinaryOperatorType.GreaterThan:
				output.Write (">");
				break;
			case CodeBinaryOperatorType.GreaterThanOrEqual:
				output.Write (">=");
				break;
			}
		}

		protected virtual void OutputParameters (CodeParameterDeclarationExpressionCollection parameters)
		{
			bool first = true;
			foreach (CodeParameterDeclarationExpression expr in parameters) {
				if (first)
					first = false;
				else
					output.Write (", ");
				GenerateExpression (expr);
			}
		}

		protected abstract void OutputType (CodeTypeReference t);

		protected virtual void OutputTypeAttributes (TypeAttributes attributes,
							     bool isStruct,
							     bool isEnum)
		{
			switch (attributes & TypeAttributes.VisibilityMask) {
			case TypeAttributes.NotPublic:
				// private by default
				break; 

			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				output.Write ("public ");
				break;

			case TypeAttributes.NestedPrivate:
				output.Write ("private ");
				break;
			}

			if (isStruct)
				output.Write ("struct ");

			else if (isEnum)
				output.Write ("enum ");
			else {
				if ((attributes & TypeAttributes.Interface) != 0) 
					output.Write ("interface ");
				else if (currentType is CodeTypeDelegate)
					output.Write ("delegate ");
				else {
					if ((attributes & TypeAttributes.Sealed) != 0)
						output.Write ("sealed ");
					if ((attributes & TypeAttributes.Abstract) != 0)
						output.Write ("abstract ");
					
					output.Write ("class ");
				}
			}
		}

		protected virtual void OutputTypeNamePair (CodeTypeReference type,
							   string name)
		{
			OutputType (type);
			output.Write (' ');
			output.Write (name);
		}

		protected abstract string QuoteSnippetString (string value);

		/*
		 * ICodeGenerator
		 */
		protected abstract string CreateEscapedIdentifier (string value);
		string ICodeGenerator.CreateEscapedIdentifier (string value)
		{
			return CreateEscapedIdentifier (value);
		}

		protected abstract string CreateValidIdentifier (string value);
		string ICodeGenerator.CreateValidIdentifier (string value)
		{
			return CreateValidIdentifier (value);
		}

		private void InitOutput (TextWriter output, CodeGeneratorOptions options)
		{
			if (options == null)
				options = new CodeGeneratorOptions ();
				
			this.output = new IndentedTextWriter (output, options.IndentString);
			this.options = options;
		}

		void ICodeGenerator.GenerateCodeFromCompileUnit (CodeCompileUnit compileUnit,
								 TextWriter output,
								 CodeGeneratorOptions options)
		{
			InitOutput (output, options);

			if (compileUnit is CodeSnippetCompileUnit) {
				GenerateSnippetCompileUnit ((CodeSnippetCompileUnit) compileUnit);
			} else {
				GenerateCompileUnit (compileUnit);
			}
		}

		void ICodeGenerator.GenerateCodeFromExpression (CodeExpression expression,
								TextWriter output,
								CodeGeneratorOptions options)
		{
			InitOutput (output, options);
			GenerateExpression (expression);
		}

		void ICodeGenerator.GenerateCodeFromNamespace (CodeNamespace ns,
							       TextWriter output, 
							       CodeGeneratorOptions options)
		{
			InitOutput (output, options);
			GenerateNamespace (ns);
		}

		void ICodeGenerator.GenerateCodeFromStatement (CodeStatement statement,
							       TextWriter output, 
							       CodeGeneratorOptions options)
		{
			InitOutput (output, options);
			GenerateStatement (statement);
		}

		void ICodeGenerator.GenerateCodeFromType (CodeTypeDeclaration type,
							  TextWriter output,
							  CodeGeneratorOptions options)
		{
			InitOutput (output, options);
			GenerateType (type);
		}

		private void GenerateType (CodeTypeDeclaration type)
		{
			this.currentType = type;

#if NET_2_0
			if (type.StartDirectives.Count > 0)
				GenerateDirectives (type.StartDirectives);
#endif
			foreach (CodeCommentStatement statement in type.Comments)
				GenerateCommentStatement (statement);

			if (type.LinePragma != null)
				GenerateLinePragmaStart (type.LinePragma);

			GenerateTypeStart (type);

			CodeTypeMember[] members = new CodeTypeMember[type.Members.Count];
			type.Members.CopyTo (members, 0);

#if NET_2_0
			if (!Options.VerbatimOrder)
#endif
 {
				int[] order = new int[members.Length];
				for (int n = 0; n < members.Length; n++)
					order[n] = Array.IndexOf (memberTypes, members[n].GetType ()) * members.Length + n;

				Array.Sort (order, members);
			}

			// WARNING: if anything is missing in the foreach loop and you add it, add the type in
			// its corresponding place in CodeTypeMemberComparer class (below)

			CodeTypeDeclaration subtype = null;
			foreach (CodeTypeMember member in members) {
				CodeTypeMember prevMember = this.currentMember;
				this.currentMember = member;

				if (prevMember != null && subtype == null) {
					if (prevMember.LinePragma != null)
						GenerateLinePragmaEnd (prevMember.LinePragma);
#if NET_2_0
					if (prevMember.EndDirectives.Count > 0)
						GenerateDirectives (prevMember.EndDirectives);
#endif
				}

				if (options.BlankLinesBetweenMembers)
					output.WriteLine ();

				subtype = member as CodeTypeDeclaration;
				if (subtype != null) {
					GenerateType (subtype);
					this.currentType = type;
					continue;
				}

#if NET_2_0
				if (currentMember.StartDirectives.Count > 0)
					GenerateDirectives (currentMember.StartDirectives);
#endif
				foreach (CodeCommentStatement statement in member.Comments)
					GenerateCommentStatement (statement);

				if (member.LinePragma != null)
					GenerateLinePragmaStart (member.LinePragma);

				CodeMemberEvent eventm = member as CodeMemberEvent;
				if (eventm != null) {
					GenerateEvent (eventm, type);
					continue;
				}
				CodeMemberField field = member as CodeMemberField;
				if (field != null) {
					GenerateField (field);
					continue;
				}
				CodeEntryPointMethod epmethod = member as CodeEntryPointMethod;
				if (epmethod != null) {
					GenerateEntryPointMethod (epmethod, type);
					continue;
				}
				CodeTypeConstructor typeCtor = member as CodeTypeConstructor;
				if (typeCtor != null) {
					GenerateTypeConstructor (typeCtor);
					continue;
				}
				CodeConstructor ctor = member as CodeConstructor;
				if (ctor != null) {
					GenerateConstructor (ctor, type);
					continue;
				}
				CodeMemberMethod method = member as CodeMemberMethod;
				if (method != null) {
					GenerateMethod (method, type);
					continue;
				}
				CodeMemberProperty property = member as CodeMemberProperty;
				if (property != null) {
					GenerateProperty (property, type);
					continue;
				}
				CodeSnippetTypeMember snippet = member as CodeSnippetTypeMember;
				if (snippet != null) {
					GenerateSnippetMember (snippet);
					continue;
				}

				this.currentMember = prevMember;
			}

			// Hack because of previous continue usage
			if (currentMember != null && !(currentMember is CodeTypeDeclaration)) {
				if (currentMember.LinePragma != null)
					GenerateLinePragmaEnd (currentMember.LinePragma);
#if NET_2_0
				if (currentMember.EndDirectives.Count > 0)
					GenerateDirectives (currentMember.EndDirectives);
#endif
			}

			this.currentType = type;
			GenerateTypeEnd (type);

			if (type.LinePragma != null)
				GenerateLinePragmaEnd (type.LinePragma);

#if NET_2_0
			if (type.EndDirectives.Count > 0)
				GenerateDirectives (type.EndDirectives);
#endif
		}

		protected abstract string GetTypeOutput (CodeTypeReference type);

		string ICodeGenerator.GetTypeOutput (CodeTypeReference type)
		{
			return GetTypeOutput (type);
		}

		protected abstract bool IsValidIdentifier (string value);

		bool ICodeGenerator.IsValidIdentifier (string value)
		{
			return IsValidIdentifier (value);
		}

		public static bool IsValidLanguageIndependentIdentifier (string value)
		{
			if (value == null)
				return false;
			if (value.Equals (string.Empty))
				return false;

			switch (char.GetUnicodeCategory (value[0])) {
			case UnicodeCategory.LetterNumber:
			case UnicodeCategory.LowercaseLetter:
			case UnicodeCategory.TitlecaseLetter:
			case UnicodeCategory.UppercaseLetter:
			case UnicodeCategory.OtherLetter:
			case UnicodeCategory.ModifierLetter:
			case UnicodeCategory.ConnectorPunctuation:
				break;
			default:
				return false;
			}

			for (int x = 1; x < value.Length; ++x) {
				switch (char.GetUnicodeCategory (value[x])) {
				case UnicodeCategory.LetterNumber:
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.UppercaseLetter:
				case UnicodeCategory.OtherLetter:
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.ConnectorPunctuation:
				case UnicodeCategory.DecimalDigitNumber:
				case UnicodeCategory.NonSpacingMark:
				case UnicodeCategory.SpacingCombiningMark:
				case UnicodeCategory.Format:
					break;
				default:
					return false;
				}
			}

			return true;
		}

		protected abstract bool Supports (GeneratorSupport supports);

		bool ICodeGenerator.Supports (GeneratorSupport value)
		{
			return Supports (value);
		}

		protected virtual void ValidateIdentifier (string value)
		{
			if (!(IsValidIdentifier (value)))
				throw new ArgumentException ("Identifier is invalid", "value");
		}

		[MonoTODO]
		public static void ValidateIdentifiers (CodeObject e)
		{
			throw new NotImplementedException();
		}

		void ICodeGenerator.ValidateIdentifier (string value)
		{
			ValidateIdentifier (value);
		}

		// The position in the array determines the order in which those
		// kind of CodeTypeMembers are generated. Less is more ;-)
		static Type [] memberTypes = {	typeof (CodeMemberField),
						typeof (CodeSnippetTypeMember),
						typeof (CodeTypeConstructor),
						typeof (CodeConstructor),
						typeof (CodeMemberProperty),
						typeof (CodeMemberEvent),
						typeof (CodeMemberMethod),
						typeof (CodeTypeDeclaration),
						typeof (CodeEntryPointMethod)
					};

#if NET_2_0
		protected virtual void GenerateDirectives (CodeDirectiveCollection directives)
		{
		}
#endif
	}
}

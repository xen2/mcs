//
// System.CodeDOM.Compiler CodeGenerator class
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM.Compiler {

	using System.CodeDOM;
	using System.Reflection;
	using System.IO;
	
	public abstract class CodeGenerator : ICodeGenerator {
		protected TextWriter output;
		
		protected virtual  void ContinueOnNewLine (string st);

		void CodeGenerator ()
		{
			indent = 8;
		}
		
		//
		// Code Generation methods
		//
		protected abstract void GenerateArrayCreateExpression (CodeArrayCreateExpression e);
		protected abstract void GenerateAssignStatement (CodeAssignStatement s);
		protected abstract void GenerateAttachEventStatement (CodeAttachEventStatement s);
		protected abstract void GenerateAttributeBlockStart (CodeAttributeBlock a);
		protected abstract void GenerateAttributeBlockEnd (CodeAttributeBlock a);
		protected abstract void GenerateBaseReferenceExpression (CodeBaseReferenceExpression e);
		protected abstract void GenerateBinaryOperatorExpression (CodeBinaryOperatorExpression x);
		protected abstract void GenerateCastExpression (CodeCastExpression x);
		protected abstract void GenerateClassConstructor (CodeClassConstructor x);
		protected abstract void GenerateClasses (CodeNamespace x);
		protected abstract void GenerateClassEnd (CodeClass x);
		protected abstract void GenerateClassStart (CodeClass x);
		protected abstract void GenerateCommentStatement (CodeCommentStatement x);
		protected abstract void GenerateConstructor (CodeConstructor x, CodeClass c);
		protected abstract void GenerateDelegateCreateExpression (CodeDelegateCreateExpression x);
		protected abstract void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression x);
		protected abstract void GenerateDelegateInvokeStatement (CodeDelegateInvokeStatement x);
		protected abstract void GenerateDetachEventStatement (CodeDetachEventStatement x);
		protected abstract void GenerateEvent (CodeMemberEvent x, CodeClass c);
		protected abstract void GenerateExpression (CodeExpression x);
		protected abstract void GenerateField (CodeMemberField x);
		protected abstract void GenerateFieldReferenceExpression (CodeFieldReferenceExpression x);
		protected abstract void GenerateForLoopStatement (CodeForLoopStatement x);
		protected abstract void GenerateIfStatement (CodeIfStatement x);
		protected abstract void GenerateIndexerExpression (CodeIndexerExpression x);
		protected abstract void GenerateLinePragmaStart (CodeLinePragma x);
		protected abstract void GenerateLinePragmaEnd (CodeLinePragma x);
		protected abstract void GenerateLiteralExpression (CodeLiteralExpression x);
		protected abstract void GenerateLiteralMember (CodeLiteralClassMember x);
		protected abstract void GenerateLiteralNamespace (CodeLiteralNamespace x);
		protected abstract void GenerateLiteralStatement (CodeLiteralStatement x);
		protected abstract void GenerateMethod (CodeMemberMethod m, CodeClass c);
		protected abstract void GenerateMethodInvokeExpression (CodeMethodInvokeExpression x);
		protected abstract void GenerateMethodInvokeStatement (CodeMethodInvokeStatement x);
		protected abstract void GenerateMethodReturnStatement (CodeMethodReturnStatement x);
		protected abstract void GenerateNamespace (CodeNamespace x);
		protected abstract void GenerateNamespaceStart (CodeNamespace x);
		protected abstract void GenerateNamespaceEnd (CodeNamespace x);
		protected abstract void GenerateNamespaceImport (CodeNamespaceImport i);
		protected abstract void GenerateNamespaceImports (CodeNamespace i);
		protected abstract void GenerateObjectCreateExpression (CodeObjectCreateExpression x);
		protected abstract void GenerateParameterDeclarationExpression (CodeParameterDeclarationExpression x);
		protected abstract void GeneratePrimitiveExpression (CodePrimitiveExpression x);
		protected abstract void GenerateProperty (CodeMemberProperty e, CodeClass c);
		protected abstract void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression x);
		protected abstract void GenerateStatement (CodeStatement x);
		protected abstract void GenerateStatementCollection (CodeStatementCollection x);
		protected abstract void GenerateThisReferenceExpression (CodeThisReferenceExpression x);
		protected abstract void GenerateThrowExceptionStatement (CodeThrowExceptionStatement x);
		protected abstract void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement x);
		protected abstract void GenerateTypeOfExpression (CodeTypeOfExpression x);
		protected abstract void GenerateTypeReferenceExpression (CodeTypeReferenceExpression x);
		protected abstract void GenerateVariableDeclarationStatement (CodeVariableDeclarationStatement x);

		//
		// Other members
		//
		protected abstract string GetNullToken ()
		protected int GetTypeArrayRank (string type);
		protected string GetTypeBaseType (string type);

		protected abstract bool IsValidIdentifier (string value);

		public static bool IsValidLanguateIndependentIdentifier (string value)
		{
			/* FIXME: implement */
			return true;
		}

		//
		// Output functions
		//
		protected virtual void OutputAttributeArgument (CodeAttributeArgument arg);
		protected virtual void OutputAttributeBlock (CodeAttributeBlock a);
		protected virtual void OutputDirection (FieldDirection dir);
		protected virtual void OutputExpressionList (CodeExpressionCollection c);
		protected virtual void OutputExpressionLIst (CodeExpressionCollection c, bool useNewlines);
		protected virtual void OutputFieldScopeModifier (MemberAttributes attrs);
		protected virtual void OutputIdentifier (string ident);
		protected virtual void OutputMemberAccessModifier (MemberAttributes attrs);
		protected virtual void OutputMemberScopeModifier (MemberAttributes attrs);
		protected virtual void OutputOperator (CodeBinaryOperatorType op);
		protected virtual void OutputParameters (CodeParameterDeclarationExpressionCollection pars);
		protected virtual void OutputType (string typeRef);
		protected virtual void OutputTypeAttributes (TypeAttributes attrs);
		protected virtual void OutputTypeNamePair (string typeRef, string name);

		protected abstract string void QuoteLiteralString (string value);

		protected virtual bool ValidateIdentifier (string value);

		//
		// Properties
		//
		string currentClassname;
		protected string CurrentClassName {
			get {
				return currentClassName;
			}
		}

		protected CodeClassMember CurrentMember {
			get {
				return codeClassMember;
			}
		}

		protected string CurrentMemberName {
			get {
				return codeClassMember.Name;
			}
		}

		int indent;
		protected int Indent {
			get {
				return indent;
			}

			set {
				indent = value;
			}
		}

		//
		// This concept seems broken, I should not really be using
		// a flag, I should be "probing" what is being generated.
		// at least the Start/End
		// functions should not be abstract, or abstract could
		// have an implementation piece?
		//
		bool isCurrentClass;
		protected bool IsCurrentClass {
			get {
				return isCurrentClass;
			}
		}

		bool isCurrentDelegate;
		protected bool IsCurrentDelegate {
			get {
				return isCurrentDelegate;
			}
		}
		bool isCurrentEnum;
		protected bool IsCurrentEnum {
			get {
				return isCurrentEnum;
			}
		}
		bool isCurrentInterface;
		protected bool IsCurrentInterface {
			get {
				return isCurrentInterface;
			}
		}

		bool isCurrentStruct
		protected bool IsCurrentStruct {
			get {
				return isCurrentStruct;
			}
		}

		protected TextWriter Output {
			get {
				return output;
			}

			set {
				output = value;
			}
		}
	}
}

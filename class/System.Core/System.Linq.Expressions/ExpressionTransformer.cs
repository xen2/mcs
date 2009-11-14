//
// ExpressionTransformer.cs
//
// Authors:
//	Roei Erez (roeie@mainsoft.com)
//  Jb Evain (jbevain@novell.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq.Expressions {

	abstract class ExpressionTransformer {

		public Expression Transform (Expression expression)
		{
			return Visit (expression);
		}

		protected virtual Expression Visit (Expression exp)
		{
			if (exp == null) return exp;

			switch (exp.NodeType) {
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
			case ExpressionType.Not:
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
			case ExpressionType.ArrayLength:
			case ExpressionType.Quote:
			case ExpressionType.TypeAs:
			case ExpressionType.UnaryPlus:
				return this.VisitUnary ((UnaryExpression) exp);
			case ExpressionType.Add:
			case ExpressionType.AddChecked:
			case ExpressionType.Subtract:
			case ExpressionType.SubtractChecked:
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyChecked:
			case ExpressionType.Divide:
			case ExpressionType.Power:
			case ExpressionType.Modulo:
			case ExpressionType.And:
			case ExpressionType.AndAlso:
			case ExpressionType.Or:
			case ExpressionType.OrElse:
			case ExpressionType.LessThan:
			case ExpressionType.LessThanOrEqual:
			case ExpressionType.GreaterThan:
			case ExpressionType.GreaterThanOrEqual:
			case ExpressionType.Equal:
			case ExpressionType.NotEqual:
			case ExpressionType.Coalesce:
			case ExpressionType.ArrayIndex:
			case ExpressionType.RightShift:
			case ExpressionType.LeftShift:
			case ExpressionType.ExclusiveOr:
				return this.VisitBinary ((BinaryExpression) exp);
			case ExpressionType.TypeIs:
				return this.VisitTypeIs ((TypeBinaryExpression) exp);
			case ExpressionType.Conditional:
				return this.VisitConditional ((ConditionalExpression) exp);
			case ExpressionType.Constant:
				return this.VisitConstant ((ConstantExpression) exp);
			case ExpressionType.Parameter:
				return this.VisitParameter ((ParameterExpression) exp);
			case ExpressionType.MemberAccess:
				return this.VisitMemberAccess ((MemberExpression) exp);
			case ExpressionType.Call:
				return this.VisitMethodCall ((MethodCallExpression) exp);
			case ExpressionType.Lambda:
				return this.VisitLambda ((LambdaExpression) exp);
			case ExpressionType.New:
				return this.VisitNew ((NewExpression) exp);
			case ExpressionType.NewArrayInit:
			case ExpressionType.NewArrayBounds:
				return this.VisitNewArray ((NewArrayExpression) exp);
			case ExpressionType.Invoke:
				return this.VisitInvocation ((InvocationExpression) exp);
			case ExpressionType.MemberInit:
				return this.VisitMemberInit ((MemberInitExpression) exp);
			case ExpressionType.ListInit:
				return this.VisitListInit ((ListInitExpression) exp);
			default:
				throw new Exception (string.Format ("Unhandled expression type: '{0}'", exp.NodeType));
			}
		}

		protected virtual MemberBinding VisitBinding (MemberBinding binding)
		{
			switch (binding.BindingType) {
			case MemberBindingType.Assignment:
				return this.VisitMemberAssignment ((MemberAssignment) binding);
			case MemberBindingType.MemberBinding:
				return this.VisitMemberMemberBinding ((MemberMemberBinding) binding);
			case MemberBindingType.ListBinding:
				return this.VisitMemberListBinding ((MemberListBinding) binding);
			default:
				throw new Exception (string.Format ("Unhandled binding type '{0}'", binding.BindingType));
			}
		}

		protected virtual ElementInit VisitElementInitializer (ElementInit initializer)
		{
			ReadOnlyCollection<Expression> arguments = this.VisitExpressionList (initializer.Arguments);
			if (arguments != initializer.Arguments) return Expression.ElementInit (initializer.AddMethod, arguments);
			return initializer;
		}

		protected virtual Expression VisitUnary (UnaryExpression u)
		{
			Expression operand = this.Visit (u.Operand);
			if (operand != u.Operand) return Expression.MakeUnary (u.NodeType, operand, u.Type, u.Method);
			return u;
		}

		protected virtual Expression VisitBinary (BinaryExpression b)
		{
			Expression left = this.Visit (b.Left);
			Expression right = this.Visit (b.Right);
			Expression conversion = this.Visit (b.Conversion);
			if (left != b.Left || right != b.Right || conversion != b.Conversion) {
				if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null) {
					return Expression.Coalesce (left, right, conversion as LambdaExpression);
				} else {
					return Expression.MakeBinary (b.NodeType, left, right, b.IsLiftedToNull, b.Method);
				}
			}
			return b;
		}

		protected virtual Expression VisitTypeIs (TypeBinaryExpression b)
		{
			Expression expr = this.Visit (b.Expression);
			if (expr != b.Expression) {
				return Expression.TypeIs (expr, b.TypeOperand);
			}
			return b;
		}

		protected virtual Expression VisitConstant (ConstantExpression c)
		{
			return c;
		}

		protected virtual Expression VisitConditional (ConditionalExpression c)
		{
			Expression test = this.Visit (c.Test);
			Expression ifTrue = this.Visit (c.IfTrue);
			Expression ifFalse = this.Visit (c.IfFalse);
			if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse) {
				return Expression.Condition (test, ifTrue, ifFalse);
			}
			return c;
		}

		protected virtual Expression VisitParameter (ParameterExpression p)
		{
			return p;
		}

		protected virtual Expression VisitMemberAccess (MemberExpression m)
		{
			Expression exp = this.Visit (m.Expression);
			if (exp != m.Expression) {
				return Expression.MakeMemberAccess (exp, m.Member);
			}
			return m;
		}

		protected virtual Expression VisitMethodCall (MethodCallExpression m)
		{
			Expression obj = this.Visit (m.Object);
			IEnumerable<Expression> args = this.VisitExpressionList (m.Arguments);
			if (obj != m.Object || args != m.Arguments) {
				return Expression.Call (obj, m.Method, args);
			}
			return m;
		}

		protected virtual ReadOnlyCollection<Expression> VisitExpressionList (ReadOnlyCollection<Expression> original)
		{
			var list = VisitList (original, Visit);
			if (list == null) return original;

			return new ReadOnlyCollection<Expression> (list);
		}

		protected virtual MemberAssignment VisitMemberAssignment (MemberAssignment assignment)
		{
			Expression e = this.Visit (assignment.Expression);
			if (e != assignment.Expression) return Expression.Bind (assignment.Member, e);
			return assignment;
		}

		protected virtual MemberMemberBinding VisitMemberMemberBinding (MemberMemberBinding binding)
		{
			IEnumerable<MemberBinding> bindings = this.VisitBindingList (binding.Bindings);
			if (bindings != binding.Bindings) return Expression.MemberBind (binding.Member, bindings);
			return binding;
		}

		protected virtual MemberListBinding VisitMemberListBinding (MemberListBinding binding)
		{
			IEnumerable<ElementInit> initializers = this.VisitElementInitializerList (binding.Initializers);
			if (initializers != binding.Initializers) return Expression.ListBind (binding.Member, initializers);
			return binding;
		}

		protected virtual IEnumerable<MemberBinding> VisitBindingList (ReadOnlyCollection<MemberBinding> original)
		{
			return VisitList (original, VisitBinding);
		}

		protected virtual IEnumerable<ElementInit> VisitElementInitializerList (ReadOnlyCollection<ElementInit> original)
		{
			return VisitList (original, VisitElementInitializer);
		}

		private IList<TElement> VisitList<TElement> (ReadOnlyCollection<TElement> original, Func<TElement, TElement> visit)
		{
			List<TElement> list = null;
			for (int i = 0, n = original.Count; i < n; i++) {
				TElement element = visit (original [i]);
				if (list != null) {
					list.Add (element);
				} else if (!EqualityComparer<TElement>.Default.Equals (element, original [i])) {
					list = new List<TElement> (n);
					for (int j = 0; j < i; j++) {
						list.Add (original [j]);
					}
					list.Add (element);
				}
			}
			if (list != null)
				return list;

			return original;
		}

		protected virtual Expression VisitLambda (LambdaExpression lambda)
		{
			Expression body = this.Visit (lambda.Body);
			if (body != lambda.Body) return Expression.Lambda (lambda.Type, body, lambda.Parameters);
			return lambda;
		}

		protected virtual NewExpression VisitNew (NewExpression nex)
		{
			IEnumerable<Expression> args = this.VisitExpressionList (nex.Arguments);
			if (args != nex.Arguments) {
				if (nex.Members != null)
					return Expression.New (nex.Constructor, args, nex.Members);
				else
					return Expression.New (nex.Constructor, args);
			}
			return nex;
		}

		protected virtual Expression VisitMemberInit (MemberInitExpression init)
		{
			NewExpression n = this.VisitNew (init.NewExpression);
			IEnumerable<MemberBinding> bindings = this.VisitBindingList (init.Bindings);
			if (n != init.NewExpression || bindings != init.Bindings) return Expression.MemberInit (n, bindings);
			return init;
		}

		protected virtual Expression VisitListInit (ListInitExpression init)
		{
			NewExpression n = this.VisitNew (init.NewExpression);
			IEnumerable<ElementInit> initializers = this.VisitElementInitializerList (init.Initializers);
			if (n != init.NewExpression || initializers != init.Initializers) return Expression.ListInit (n, initializers);
			return init;
		}

		protected virtual Expression VisitNewArray (NewArrayExpression na)
		{
			IEnumerable<Expression> exprs = this.VisitExpressionList (na.Expressions);
			if (exprs != na.Expressions) {
				if (na.NodeType == ExpressionType.NewArrayInit) {
					return Expression.NewArrayInit (na.Type.GetElementType (), exprs);
				} else {
					return Expression.NewArrayBounds (na.Type.GetElementType (), exprs);
				}
			}
			return na;
		}

		protected virtual Expression VisitInvocation (InvocationExpression iv)
		{
			IEnumerable<Expression> args = this.VisitExpressionList (iv.Arguments);
			Expression expr = this.Visit (iv.Expression);
			if (args != iv.Arguments || expr != iv.Expression) return Expression.Invoke (expr, args);
			return iv;
		}
	}
}

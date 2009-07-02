/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
#if CODEPLEX_40
using System.Dynamic.Utils;
#else
using Microsoft.Scripting.Utils;
#endif
using System.Reflection;

#if CODEPLEX_40
namespace System.Linq.Expressions {
#else
namespace Microsoft.Linq.Expressions {
#endif
    /// <summary>
    /// Represents an expression that applies a delegate or lambda expression to a list of argument expressions.
    /// </summary>
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(Expression.InvocationExpressionProxy))]
#endif
    public sealed class InvocationExpression : Expression, IArgumentProvider {
        private IList<Expression> _arguments;
        private readonly Expression _lambda;
        private readonly Type _returnType;

        internal InvocationExpression(Expression lambda, IList<Expression> arguments, Type returnType) {
            _lambda = lambda;
            _arguments = arguments;
            _returnType = returnType;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        public sealed override Type Type {
            get { return _returnType; }
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Invoke; }
        }

        /// <summary>
        /// Gets the delegate or lambda expression to be applied.
        /// </summary>
        public Expression Expression {
            get { return _lambda; }
        }

        /// <summary>
        /// Gets the arguments that the delegate or lambda expression is applied to.
        /// </summary>
        public ReadOnlyCollection<Expression> Arguments {
            get { return ReturnReadOnly(ref _arguments); }
        }

        Expression IArgumentProvider.GetArgument(int index) {
            return _arguments[index];
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return _arguments.Count;
            }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitInvocation(this);
        }

        internal InvocationExpression Rewrite(Expression lambda, Expression[] arguments) {
            Debug.Assert(lambda != null);
            Debug.Assert(arguments == null || arguments.Length == _arguments.Count);

            return Expression.Invoke(lambda, arguments ?? _arguments);
        }

        internal LambdaExpression LambdaOperand {
            get {
                return (_lambda.NodeType == ExpressionType.Quote)
                    ? (LambdaExpression)((UnaryExpression)_lambda).Operand
                    : (_lambda as LambdaExpression);
            }
        }
    }

    public partial class Expression {

        ///<summary>
        ///Creates an <see cref="T:Microsoft.Linq.Expressions.InvocationExpression" /> that 
        ///applies a delegate or lambda expression to a list of argument expressions.
        ///</summary>
        ///<returns>
        ///An <see cref="T:Microsoft.Linq.Expressions.InvocationExpression" /> that 
        ///applies the specified delegate or lambda expression to the provided arguments.
        ///</returns>
        ///<param name="expression">
        ///An <see cref="T:Microsoft.Linq.Expressions.Expression" /> that represents the delegate
        ///or lambda expression to be applied.
        ///</param>
        ///<param name="arguments">
        ///An array of <see cref="T:Microsoft.Linq.Expressions.Expression" /> objects
        ///that represent the arguments that the delegate or lambda expression is applied to.
        ///</param>
        ///<exception cref="T:System.ArgumentNullException">
        ///<paramref name="expression" /> is null.</exception>
        ///<exception cref="T:System.ArgumentException">
        ///<paramref name="expression" />.Type does not represent a delegate type or an <see cref="T:Microsoft.Linq.Expressions.Expression`1" />.-or-The <see cref="P:Microsoft.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the delegate represented by <paramref name="expression" />.</exception>
        ///<exception cref="T:System.InvalidOperationException">
        ///<paramref name="arguments" /> does not contain the same number of elements as the list of parameters for the delegate represented by <paramref name="expression" />.</exception>
        public static InvocationExpression Invoke(Expression expression, params Expression[] arguments) {
            return Invoke(expression, (IEnumerable<Expression>)arguments);
        }

        ///<summary>
        ///Creates an <see cref="T:Microsoft.Linq.Expressions.InvocationExpression" /> that 
        ///applies a delegate or lambda expression to a list of argument expressions.
        ///</summary>
        ///<returns>
        ///An <see cref="T:Microsoft.Linq.Expressions.InvocationExpression" /> that 
        ///applies the specified delegate or lambda expression to the provided arguments.
        ///</returns>
        ///<param name="expression">
        ///An <see cref="T:Microsoft.Linq.Expressions.Expression" /> that represents the delegate
        ///or lambda expression to be applied.
        ///</param>
        ///<param name="arguments">
        ///An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Microsoft.Linq.Expressions.Expression" /> objects
        ///that represent the arguments that the delegate or lambda expression is applied to.
        ///</param>
        ///<exception cref="T:System.ArgumentNullException">
        ///<paramref name="expression" /> is null.</exception>
        ///<exception cref="T:System.ArgumentException">
        ///<paramref name="expression" />.Type does not represent a delegate type or an <see cref="T:Microsoft.Linq.Expressions.Expression`1" />.-or-The <see cref="P:Microsoft.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the delegate represented by <paramref name="expression" />.</exception>
        ///<exception cref="T:System.InvalidOperationException">
        ///<paramref name="arguments" /> does not contain the same number of elements as the list of parameters for the delegate represented by <paramref name="expression" />.</exception>
        public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> arguments) {
            RequiresCanRead(expression, "expression");

            var args = arguments.ToReadOnly();
            var mi = GetInvokeMethod(expression);
            ValidateArgumentTypes(mi, ExpressionType.Invoke, ref args);
            return new InvocationExpression(expression, args, mi.ReturnType);
        }

        /// <summary>
        /// Gets the delegate's Invoke method; used by InvocationExpression.
        /// </summary>
        /// <param name="expression">The expression to be invoked.</param>
        internal static MethodInfo GetInvokeMethod(Expression expression) {
            Type delegateType = expression.Type;
            if (delegateType == typeof(Delegate)) {
                throw Error.ExpressionTypeNotInvocable(delegateType);
            } else if (!typeof(Delegate).IsAssignableFrom(expression.Type)) {
                Type exprType = TypeUtils.FindGenericType(typeof(Expression<>), expression.Type);
                if (exprType == null) {
                    throw Error.ExpressionTypeNotInvocable(expression.Type);
                }
                delegateType = exprType.GetGenericArguments()[0];
            }

            return delegateType.GetMethod("Invoke");
        }
    }
}

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


#if CODEPLEX_40
using System.Dynamic.Utils;
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;
#endif

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    /// <summary>
    /// Represents the binary dynamic operation at the call site, providing the binding semantic and the details about the operation.
    /// </summary>
    public abstract class BinaryOperationBinder : DynamicMetaObjectBinder {
        private ExpressionType _operation;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperationBinder"/> class.
        /// </summary>
        /// <param name="operation">The binary operation kind.</param>
        protected BinaryOperationBinder(ExpressionType operation) {
            ContractUtils.Requires(OperationIsValid(operation), "operation");
            _operation = operation;
        }

        /// <summary>
        /// The result type of the operation.
        /// </summary>
        public override sealed Type ReturnType {
            get { return typeof(object); }
        }

        /// <summary>
        /// The binary operation kind.
        /// </summary>
        public ExpressionType Operation {
            get {
                return _operation;
            }
        }

        /// <summary>
        /// Performs the binding of the binary dynamic operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic binary operation.</param>
        /// <param name="arg">The right hand side operand of the dynamic binary operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg) {
            return FallbackBinaryOperation(target, arg, null);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the binary dynamic operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic binary operation.</param>
        /// <param name="arg">The right hand side operand of the dynamic binary operation.</param>
        /// <param name="errorSuggestion">The binding result in case the binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion);

        /// <summary>
        /// Performs the binding of the dynamic binary operation.
        /// </summary>
        /// <param name="target">The target of the dynamic operation.</param>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.Requires(args.Length == 1, "args");

            var arg0 = args[0];
            ContractUtils.RequiresNotNull(arg0, "args");

            return target.BindBinaryOperation(this, arg0);
        }

        // this is a standard DynamicMetaObjectBinder
        internal override sealed bool IsStandardBinder {
            get {
                return true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool OperationIsValid(ExpressionType operation) {
            switch (operation) {
                #region Generated Binary Operation Binder Validator

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_binop_validator from: generate_tree.py

                case ExpressionType.Add:
                case ExpressionType.And:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:

                // *** END GENERATED CODE ***

                #endregion

                case ExpressionType.Extension:
                    return true;

                default:
                    return false;
            }
        }
    }
}

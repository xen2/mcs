//
// conversion.cs: various routines for implementing conversions.
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Ravi Pratap (ravi@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc.
//

namespace Mono.CSharp {
	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;

	//
	// A container class for all the conversion operations
	//
	public class Convert {
		//
		// This is used to prettify the code: a null argument is allowed
		// for ImplicitStandardConversion as long as it is known that
		// no anonymous method will play a role.
		//
		// FIXME: renamed from `const' to `static' to allow bootstraping from older
		// versions of the compiler that could not cope with this construct.
		//
		public static EmitContext ConstantEC = null;
		
		static public void Error_CannotConvertType (Location loc, Type source, Type target)
		{
			Report.Error (30, loc, "Cannot convert type '" +
				      TypeManager.CSharpName (source) + "' to '" +
				      TypeManager.CSharpName (target) + "'");
		}

		static EmptyExpression MyEmptyExpr;
		static public Expression ImplicitReferenceConversion (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (expr_type == null && expr.eclass == ExprClass.MethodGroup){
				// if we are a method group, emit a warning

				expr.Emit (null);
			}

			if (expr_type == TypeManager.void_type)
				return null;
			
			//
			// notice that it is possible to write "ValueType v = 1", the ValueType here
			// is an abstract class, and not really a value type, so we apply the same rules.
			//
			if (target_type == TypeManager.object_type) {
				//
				// A pointer type cannot be converted to object
				// 
				if (expr_type.IsPointer)
					return null;

				if (expr_type.IsValueType)
					return new BoxedCast (expr);
				if (expr_type.IsClass || expr_type.IsInterface || expr_type == TypeManager.enum_type){
					if (expr_type == TypeManager.anonymous_method_type)
						return null;
					return new EmptyCast (expr, target_type);
				}

				return null;
			} else if (target_type == TypeManager.value_type) {
				if (expr_type.IsValueType)
					return new BoxedCast (expr);
				if (expr_type == TypeManager.null_type)
					return new NullCast (expr, target_type);

				return null;
			} else if (expr_type.IsSubclassOf (target_type)) {
				//
				// Special case: enumeration to System.Enum.
				// System.Enum is not a value type, it is a class, so we need
				// a boxing conversion
				//
				if (expr_type.IsEnum)
					return new BoxedCast (expr);

				return new EmptyCast (expr, target_type);
			}

			// This code is kind of mirrored inside ImplicitStandardConversionExists
			// with the small distinction that we only probe there
			//
			// Always ensure that the code here and there is in sync

			// from the null type to any reference-type.
			if (expr_type == TypeManager.null_type){
				if (target_type.IsPointer)
					return new EmptyCast (NullPointer.Null, target_type);
					
				if (!target_type.IsValueType)
					return new NullCast (expr, target_type);
			}

			// from any class-type S to any interface-type T.
			if (target_type.IsInterface) {
				if (target_type != TypeManager.iconvertible_type &&
				    expr_type.IsValueType && (expr is Constant) &&
				    !(expr is IntLiteral || expr is BoolLiteral ||
				      expr is FloatLiteral || expr is DoubleLiteral ||
				      expr is LongLiteral || expr is CharLiteral ||
				      expr is StringLiteral || expr is DecimalLiteral ||
				      expr is UIntLiteral || expr is ULongLiteral)) {
					return null;
				}

				if (TypeManager.ImplementsInterface (expr_type, target_type)){
					if (expr_type.IsClass)
						return new EmptyCast (expr, target_type);
					else if (expr_type.IsValueType)
						return new BoxedCast (expr, target_type);
					else
						return new EmptyCast (expr, target_type);
				}
			}

			// from any interface type S to interface-type T.
			if (expr_type.IsInterface && target_type.IsInterface) {
				if (TypeManager.ImplementsInterface (expr_type, target_type))
					return new EmptyCast (expr, target_type);
				else
					return null;
			}
				
			// from an array-type S to an array-type of type T
			if (expr_type.IsArray && target_type.IsArray) {
				if (expr_type.GetArrayRank () == target_type.GetArrayRank ()) {

					Type expr_element_type = TypeManager.GetElementType (expr_type);

					if (MyEmptyExpr == null)
						MyEmptyExpr = new EmptyExpression ();
						
					MyEmptyExpr.SetType (expr_element_type);
					Type target_element_type = TypeManager.GetElementType (target_type);

					if (!expr_element_type.IsValueType && !target_element_type.IsValueType)
						if (ImplicitStandardConversionExists (ConstantEC, MyEmptyExpr,
										      target_element_type))
							return new EmptyCast (expr, target_type);
				}
			}
				
			// from an array-type to System.Array
			if (expr_type.IsArray && target_type == TypeManager.array_type)
				return new EmptyCast (expr, target_type);
				
			// from any delegate type to System.Delegate
			if ((expr_type == TypeManager.delegate_type ||
			     expr_type.IsSubclassOf (TypeManager.delegate_type)) &&
			    target_type == TypeManager.delegate_type)
				return new EmptyCast (expr, target_type);
					
			// from any array-type or delegate type into System.ICloneable.
			if (expr_type.IsArray ||
			    expr_type == TypeManager.delegate_type ||
			    expr_type.IsSubclassOf (TypeManager.delegate_type))
				if (target_type == TypeManager.icloneable_type)
					return new EmptyCast (expr, target_type);
				
			return null;
		}

		//
		// Tests whether an implicit reference conversion exists between expr_type
		// and target_type
		//
		static bool ImplicitReferenceConversionExists (Expression expr, Type target_type)
		{
			if (target_type.IsValueType)
				return false;

			Type expr_type = expr.Type;

			//
			// This is the boxed case.
			//
			if (target_type == TypeManager.object_type) {
				if (expr_type.IsClass || expr_type.IsValueType ||
				    expr_type.IsInterface || expr_type == TypeManager.enum_type)
					if (target_type != TypeManager.anonymous_method_type)
						return true;

				return false;
			} else if (expr_type.IsSubclassOf (target_type)) 
				return true;

			// Please remember that all code below actually comes
			// from ImplicitReferenceConversion so make sure code remains in sync
				
			// from any class-type S to any interface-type T.
			if (target_type.IsInterface) {
				if (target_type != TypeManager.iconvertible_type &&
				    expr_type.IsValueType && (expr is Constant) &&
				    !(expr is IntLiteral || expr is BoolLiteral ||
				      expr is FloatLiteral || expr is DoubleLiteral ||
				      expr is LongLiteral || expr is CharLiteral ||
				      expr is StringLiteral || expr is DecimalLiteral ||
				      expr is UIntLiteral || expr is ULongLiteral)) {
					return false;
				}
				
				if (TypeManager.ImplementsInterface (expr_type, target_type))
					return true;
			}
				
			// from any interface type S to interface-type T.
			if (expr_type.IsInterface && target_type.IsInterface)
				if (TypeManager.ImplementsInterface (expr_type, target_type))
					return true;
				
			// from an array-type S to an array-type of type T
			if (expr_type.IsArray && target_type.IsArray) {
				if (expr_type.GetArrayRank () == target_type.GetArrayRank ()) {
						
					Type expr_element_type = expr_type.GetElementType ();

					if (MyEmptyExpr == null)
						MyEmptyExpr = new EmptyExpression ();
						
					MyEmptyExpr.SetType (expr_element_type);
					Type target_element_type = TypeManager.GetElementType (target_type);
						
					if (!expr_element_type.IsValueType && !target_element_type.IsValueType)
						if (ImplicitStandardConversionExists (ConstantEC, MyEmptyExpr,
										      target_element_type))
							return true;
				}
			}
				
			// from an array-type to System.Array
			if (expr_type.IsArray && (target_type == TypeManager.array_type))
				return true;
				
			// from any delegate type to System.Delegate
			if ((expr_type == TypeManager.delegate_type ||
			     expr_type.IsSubclassOf (TypeManager.delegate_type)) &&
			    target_type == TypeManager.delegate_type)
				if (target_type.IsAssignableFrom (expr_type))
					return true;
					
			// from any array-type or delegate type into System.ICloneable.
			if (expr_type.IsArray ||
			    expr_type == TypeManager.delegate_type ||
			    expr_type.IsSubclassOf (TypeManager.delegate_type))
				if (target_type == TypeManager.icloneable_type)
					return true;
				
			// from the null type to any reference-type.
			if (expr_type == TypeManager.null_type){
				if (target_type.IsPointer)
					return true;
			
				if (!target_type.IsValueType)
					return true;
			}
			return false;
		}

		/// <summary>
		///   Implicit Numeric Conversions.
		///
		///   expr is the expression to convert, returns a new expression of type
		///   target_type or null if an implicit conversion is not possible.
		/// </summary>
		static public Expression ImplicitNumericConversion (EmitContext ec, Expression expr,
								    Type target_type, Location loc)
		{
			Type expr_type = expr.Type;

			//
			// Attempt to do the implicit constant expression conversions

			if (expr is Constant){
				if (expr is IntConstant){
					Expression e;
					
					e = TryImplicitIntConversion (target_type, (IntConstant) expr);
					
					if (e != null)
						return e;
				} else if (expr is LongConstant && target_type == TypeManager.uint64_type){
					//
					// Try the implicit constant expression conversion
					// from long to ulong, instead of a nice routine,
					// we just inline it
					//
					long v = ((LongConstant) expr).Value;
					if (v >= 0)
						return new ULongConstant ((ulong) v);
				} 
			}
			
 			Type real_target_type = target_type;

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double, decimal
				//
				if (real_target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double, decimal
				// 
				if ((real_target_type == TypeManager.short_type) ||
				    (real_target_type == TypeManager.ushort_type) ||
				    (real_target_type == TypeManager.int32_type) ||
				    (real_target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);

				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr);
				
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double, decimal
				// 
				if (real_target_type == TypeManager.int32_type)
					return new EmptyCast (expr, target_type);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr);
				
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double, decimal
				//
				if (real_target_type == TypeManager.uint32_type)
					return new EmptyCast (expr, target_type);

				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double, decimal
				//
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double, decimal
				//
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long/ulong to float, double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to float, double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double, decimal
				// 
				if ((real_target_type == TypeManager.ushort_type) ||
				    (real_target_type == TypeManager.int32_type) ||
				    (real_target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);
				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr);
			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			}

			return null;
		}


		/// <summary>
		///  Same as ImplicitStandardConversionExists except that it also looks at
		///  implicit user defined conversions - needed for overload resolution
		/// </summary>
		public static bool ImplicitConversionExists (EmitContext ec, Expression expr, Type target_type)
		{
			if (ImplicitStandardConversionExists (ec, expr, target_type))
				return true;

			Expression dummy = ImplicitUserConversion (ec, expr, target_type, Location.Null);

			if (dummy != null)
				return true;

			return false;
		}

		public static bool ImplicitUserConversionExists (EmitContext ec, Type source, Type target)
		{
			Expression dummy = ImplicitUserConversion (
				ec, new EmptyExpression (source), target, Location.Null);
			return dummy != null;
		}

		/// <summary>
		///  Determines if a standard implicit conversion exists from
		///  expr_type to target_type
		///
		///  ec should point to a real EmitContext if expr.Type is TypeManager.anonymous_method_type.
		/// </summary>
		public static bool ImplicitStandardConversionExists (EmitContext ec, Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (expr_type == TypeManager.void_type)
				return false;

			if (expr_type == target_type)
				return true;


			// First numeric conversions 

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double, decimal
				//
				if ((target_type == TypeManager.int32_type) || 
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type)  ||
				    (target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double, decimal
				// 
				if ((target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
	
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, double, float, decimal
				// 
				if ((target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, double, float, decimal
				//
				if ((target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, double, float, decimal
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, double, float, decimal
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if ((expr_type == TypeManager.uint64_type) ||
				   (expr_type == TypeManager.int64_type)) {
				//
				// From long/ulong to double, float, decimal
				//
				if ((target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, ulong, long, float, double, decimal
				// 
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;

			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (target_type == TypeManager.double_type)
					return true;
			}	
			
			if (expr.eclass == ExprClass.MethodGroup){
				if (TypeManager.IsDelegateType (target_type) && RootContext.Version != LanguageVersion.ISO_1){
					MethodGroupExpr mg = expr as MethodGroupExpr;
					if (mg != null){
						//
						// This should not happen frequently, so we can create an object
						// to test compatibility
						//
						Expression c = ImplicitDelegateCreation.Create (
							ec, mg, target_type, true, Location.Null);
						return c != null;
					}
				}
			}
			
			if (ImplicitReferenceConversionExists (expr, target_type))
				return true;

			//
			// Implicit Constant Expression Conversions
			//
			if (expr is IntConstant){
				int value = ((IntConstant) expr).Value;

				if (target_type == TypeManager.sbyte_type){
					if (value >= SByte.MinValue && value <= SByte.MaxValue)
						return true;
				} else if (target_type == TypeManager.byte_type){
					if (value >= 0 && value <= Byte.MaxValue)
						return true;
				} else if (target_type == TypeManager.short_type){
					if (value >= Int16.MinValue && value <= Int16.MaxValue)
						return true;
				} else if (target_type == TypeManager.ushort_type){
					if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
						return true;
				} else if (target_type == TypeManager.uint32_type){
					if (value >= 0)
						return true;
				} else if (target_type == TypeManager.uint64_type){
					 //
					 // we can optimize this case: a positive int32
					 // always fits on a uint64.  But we need an opcode
					 // to do it.
					 //
					if (value >= 0)
						return true;
				}
				
				if (value == 0 && expr is IntLiteral && TypeManager.IsEnumType (target_type))
					return true;
			}

			if (expr is LongConstant && target_type == TypeManager.uint64_type){
				//
				// Try the implicit constant expression conversion
				// from long to ulong, instead of a nice routine,
				// we just inline it
				//
				long v = ((LongConstant) expr).Value;
				if (v >= 0)
					return true;
			}
			
			if ((target_type == TypeManager.enum_type ||
			     target_type.IsSubclassOf (TypeManager.enum_type)) &&
			     expr is IntLiteral){
				IntLiteral i = (IntLiteral) expr;

				if (i.Value == 0)
					return true;
			}

			//
			// If `expr_type' implements `target_type' (which is an iface)
			// see TryImplicitIntConversion
			// 
			if (target_type.IsInterface && target_type.IsAssignableFrom (expr_type))
				return true;

			if (target_type == TypeManager.void_ptr_type && expr_type.IsPointer)
				return true;

			if (expr_type == TypeManager.anonymous_method_type){
				if (!TypeManager.IsDelegateType (target_type))
					return false;

				AnonymousMethod am = (AnonymousMethod) expr;

				Expression conv = am.Compatible (ec, target_type, true);
				if (conv != null)
					return true;
			}

			return false;
		}

		/// <summary>
		///  Finds "most encompassed type" according to the spec (13.4.2)
		///  amongst the methods in the MethodGroupExpr
		/// </summary>
		static Type FindMostEncompassedType (EmitContext ec, ArrayList types)
		{
			Type best = null;

			if (types.Count == 0)
				return null;

			if (types.Count == 1)
				return (Type) types [0];

			EmptyExpression expr = EmptyExpression.Grab ();

			foreach (Type t in types) {
				if (best == null) {
					best = t;
					continue;
				}

				expr.SetType (t);
				if (ImplicitStandardConversionExists (ec, expr, best))
					best = t;
			}

			expr.SetType (best);
			foreach (Type t in types) {
				if (best == t)
					continue;
				if (!ImplicitStandardConversionExists (ec, expr, t)) {
					best = null;
					break;
				}
			}

			EmptyExpression.Release (expr);

			return best;
		}
	
		/// <summary>
		///  Finds "most encompassing type" according to the spec (13.4.2)
		///  amongst the types in the given set
		/// </summary>
		static Type FindMostEncompassingType (EmitContext ec, ArrayList types)
		{
			Type best = null;

			if (types.Count == 0)
				return null;

			if (types.Count == 1)
				return (Type) types [0];

			EmptyExpression expr = EmptyExpression.Grab ();

			foreach (Type t in types) {
				if (best == null) {
					best = t;
					continue;
				}

				expr.SetType (best);
				if (ImplicitStandardConversionExists (ec, expr, t))
					best = t;
			}

			foreach (Type t in types) {
				if (best == t)
					continue;
				expr.SetType (t);
				if (!ImplicitStandardConversionExists (ec, expr, best)) {
					best = null;
					break;
				}
			}

			EmptyExpression.Release (expr);

			return best;
		}

		/// <summary>
		///   Finds the most specific source Sx according to the rules of the spec (13.4.4)
		///   by making use of FindMostEncomp* methods. Applies the correct rules separately
		///   for explicit and implicit conversion operators.
		/// </summary>
		static public Type FindMostSpecificSource (EmitContext ec, IList list,
							   Expression source, bool apply_explicit_conv_rules,
							   Location loc)
		{
			ArrayList src_types_set = new ArrayList ();
			
			//
			// If any operator converts from S then Sx = S
			//
			Type source_type = source.Type;
			foreach (MethodBase mb in list){
				ParameterData pd = TypeManager.GetParameterData (mb);
				Type param_type = pd.ParameterType (0);

				if (param_type == source_type)
					return param_type;

				src_types_set.Add (param_type);
			}
			
			//
			// Explicit Conv rules
			//
			if (apply_explicit_conv_rules) {
				ArrayList candidate_set = new ArrayList ();

				foreach (Type param_type in src_types_set){
					if (ImplicitStandardConversionExists (ec, source, param_type))
						candidate_set.Add (param_type);
				}

				if (candidate_set.Count != 0)
					return FindMostEncompassedType (ec, candidate_set);
			}

			//
			// Final case
			//
			if (apply_explicit_conv_rules)
				return FindMostEncompassingType (ec, src_types_set);
			else
				return FindMostEncompassedType (ec, src_types_set);
		}
		
		/// <summary>
		///  Finds the most specific target Tx according to section 13.4.4
		/// </summary>
		static public Type FindMostSpecificTarget (EmitContext ec, IList list,
							   Type target, bool apply_explicit_conv_rules,
							   Location loc)
		{
			ArrayList tgt_types_set = new ArrayList ();
			
			//
			// If any operator converts to T then Tx = T
			//
			foreach (MethodInfo mi in list){
				Type ret_type = mi.ReturnType;
				if (ret_type == target)
					return ret_type;

				tgt_types_set.Add (ret_type);
			}

			//
			// Explicit conv rules
			//
			if (apply_explicit_conv_rules) {
				ArrayList candidate_set = new ArrayList ();

				EmptyExpression expr = EmptyExpression.Grab ();

				foreach (Type ret_type in tgt_types_set){
					expr.SetType (ret_type);
					
					if (ImplicitStandardConversionExists (ec, expr, target))
						candidate_set.Add (ret_type);
				}

				EmptyExpression.Release (expr);

				if (candidate_set.Count != 0)
					return FindMostEncompassingType (ec, candidate_set);
			}
			
			//
			// Okay, final case !
			//
			if (apply_explicit_conv_rules)
				return FindMostEncompassedType (ec, tgt_types_set);
			else 
				return FindMostEncompassingType (ec, tgt_types_set);
		}
		
		/// <summary>
		///  User-defined Implicit conversions
		/// </summary>
		static public Expression ImplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, false);
		}

		/// <summary>
		///  User-defined Explicit conversions
		/// </summary>
		static public Expression ExplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, true);
		}

		static void AddConversionOperators (EmitContext ec, ArrayList list, 
						    Expression source, Type target_type, 
						    bool look_for_explicit,
						    MethodGroupExpr mg)
		{
			if (mg == null)
				return;

			Type source_type = source.Type;
			EmptyExpression expr = EmptyExpression.Grab ();
			foreach (MethodInfo m in mg.Methods) {
				ParameterData pd = TypeManager.GetParameterData (m);
				Type return_type = m.ReturnType;
				Type arg_type = pd.ParameterType (0);

				if (source_type != arg_type) {
					if (!ImplicitStandardConversionExists (ec, source, arg_type)) {
						if (!look_for_explicit)
							continue;
						expr.SetType (arg_type);
						if (!ImplicitStandardConversionExists (ec, expr, source_type))
							continue;
					}
				}

				if (target_type != return_type) {
					expr.SetType (return_type);
					if (!ImplicitStandardConversionExists (ec, expr, target_type)) {
						if (!look_for_explicit)
							continue;
						expr.SetType (target_type);
						if (!ImplicitStandardConversionExists (ec, expr, return_type))
							continue;
					}
				}

				list.Add (m);
			}

			EmptyExpression.Release (expr);
		}

		/// <summary>
		///   Computes the list of the user-defined conversion
		///   operators from source_type to target_type.  `look_for_explicit'
		///   controls whether we should also include the list of explicit
		///   operators
		/// </summary>
		static IList GetConversionOperators (EmitContext ec,
						     Expression source, Type target_type,
						     Location loc, bool look_for_explicit)
		{
			ArrayList ret = new ArrayList (4);

			Type source_type = source.Type;

			if (source_type != TypeManager.decimal_type) {
				AddConversionOperators (ec, ret, source, target_type, look_for_explicit,
					Expression.MethodLookup (
						ec, source_type, "op_Implicit", loc) as MethodGroupExpr);
				if (look_for_explicit) {
					AddConversionOperators (ec, ret, source, target_type, look_for_explicit,
						Expression.MethodLookup (
							ec, source_type, "op_Explicit", loc) as MethodGroupExpr);
				}
			}

			if (target_type != TypeManager.decimal_type) {
				AddConversionOperators (ec, ret, source, target_type, look_for_explicit,
					Expression.MethodLookup (
						ec, target_type, "op_Implicit", loc) as MethodGroupExpr);
				if (look_for_explicit) {
					AddConversionOperators (ec, ret, source, target_type, look_for_explicit,
						Expression.MethodLookup (
							ec, target_type, "op_Explicit", loc) as MethodGroupExpr);
				}
			}

			return ret;
		}

		static DoubleHash explicit_conv = new DoubleHash (100);
		static DoubleHash implicit_conv = new DoubleHash (100);

		/// <summary>
		///   User-defined conversions
		/// </summary>
		static public Expression UserDefinedConversion (EmitContext ec, Expression source,
								Type target, Location loc,
								bool look_for_explicit)
		{
			Type source_type = source.Type;
			MethodInfo method = null;
			Type most_specific_source = null;
			Type most_specific_target = null;

			object o;
			DoubleHash hash = look_for_explicit ? explicit_conv : implicit_conv;

			if (!(source is Constant) && hash.Lookup (source_type, target, out o)) {
				method = (MethodInfo) o;
				if (method != null) {
					ParameterData pd = TypeManager.GetParameterData (method);
					most_specific_source = pd.ParameterType (0);
					most_specific_target = method.ReturnType;
				}
			} else {
				IList ops = GetConversionOperators (ec, source, target, loc, look_for_explicit);
				if (ops == null || ops.Count == 0) {
					method = null;
					goto skip;
				}
					
				most_specific_source = FindMostSpecificSource (ec, ops, source, look_for_explicit, loc);
				if (most_specific_source == null) {
					method = null;
					goto skip;
				}
				
				most_specific_target = FindMostSpecificTarget (ec, ops, target, look_for_explicit, loc);
				if (most_specific_target == null) {
					method = null;
					goto skip;
				}
				
				int count = 0;
				
				foreach (MethodInfo m in ops) {
					ParameterData pd = TypeManager.GetParameterData (m);
					
					if (pd.ParameterType (0) == most_specific_source &&
					    m.ReturnType == most_specific_target) {
						method = m;
						count++;
					}
				}
				if (count > 1)
					method = null;
				
			skip:
				if (!(source is Constant))
					hash.Insert (source_type, target, method);
			}

			if (method == null)
				return null;
			
			//
			// This will do the conversion to the best match that we
			// found.  Now we need to perform an implict standard conversion
			// if the best match was not the type that we were requested
			// by target.
			//
			if (look_for_explicit)
				source = ExplicitConversionStandard (ec, source, most_specific_source, loc);
			else
				source = ImplicitConversionStandard (ec, source, most_specific_source, loc);

			if (source == null)
				return null;

			Expression e;
			e =  new UserCast (method, source, loc);
			if (e.Type != target){
				if (!look_for_explicit)
					e = ImplicitConversionStandard (ec, e, target, loc);
				else
					e = ExplicitConversionStandard (ec, e, target, loc);
			}

			return e;
		}
		
		/// <summary>
		///   Converts implicitly the resolved expression `expr' into the
		///   `target_type'.  It returns a new expression that can be used
		///   in a context that expects a `target_type'. 
		/// </summary>
		static public Expression ImplicitConversion (EmitContext ec, Expression expr,
							     Type target_type, Location loc)
		{
			Expression e;

			if (target_type == null)
				throw new Exception ("Target type is null");

			e = ImplicitConversionStandard (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = ImplicitUserConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			return null;
		}

		
		/// <summary>
		///   Attempts to apply the `Standard Implicit
		///   Conversion' rules to the expression `expr' into
		///   the `target_type'.  It returns a new expression
		///   that can be used in a context that expects a
		///   `target_type'.
		///
		///   This is different from `ImplicitConversion' in that the
		///   user defined implicit conversions are excluded. 
		/// </summary>
		static public Expression ImplicitConversionStandard (EmitContext ec, Expression expr,
								     Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression e;

			if (expr.eclass == ExprClass.MethodGroup){
				if (!TypeManager.IsDelegateType (target_type)){
					return null;
				}
				
				//
				// Only allow anonymous method conversions on post ISO_1
				//
				if (RootContext.Version != LanguageVersion.ISO_1){
					MethodGroupExpr mg = expr as MethodGroupExpr;
					if (mg != null)
						return ImplicitDelegateCreation.Create (
							ec, mg, target_type, false, loc);
				}
			}

			if (expr_type == target_type && expr_type != TypeManager.null_type)
				return expr;

			e = ImplicitNumericConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = ImplicitReferenceConversion (expr, target_type);
			if (e != null)
				return e;
			
			if ((target_type == TypeManager.enum_type ||
			     target_type.IsSubclassOf (TypeManager.enum_type)) &&
			    expr is IntLiteral){
				IntLiteral i = (IntLiteral) expr;
				
				if (i.Value == 0)
					return new EnumConstant ((Constant) expr, target_type);
			}

			if (ec.InUnsafe) {
				if (expr_type.IsPointer){
					if (target_type == TypeManager.void_ptr_type)
						return new EmptyCast (expr, target_type);

					//
					// yep, comparing pointer types cant be done with
					// t1 == t2, we have to compare their element types.
					//
					if (target_type.IsPointer){
						if (TypeManager.GetElementType(target_type) == TypeManager.GetElementType(expr_type))
							return expr;
					}
				}
				
				if (target_type.IsPointer) {
					if (expr_type == TypeManager.null_type)
						return new EmptyCast (NullPointer.Null, target_type);

					if (expr_type == TypeManager.void_ptr_type)
						return new EmptyCast (expr, target_type);
				}
			}

			if (expr_type == TypeManager.anonymous_method_type){
				if (!TypeManager.IsDelegateType (target_type)){
					Report.Error (1660, loc,
						"Cannot convert anonymous method block to type `{0}' because it is not a delegate type",
						TypeManager.CSharpName (target_type));
					return null;
				}

				AnonymousMethod am = (AnonymousMethod) expr;
				int errors = Report.Errors;

				Expression conv = am.Compatible (ec, target_type, false);
				if (conv != null)
					return conv;
				
				//
				// We return something instead of null, to avoid
				// the duplicate error, since am.Compatible would have
				// reported that already
				//
				if (errors != Report.Errors)
					return new EmptyCast (expr, target_type);
			}
			
			return null;
		}

		/// <summary>
		///   Attempts to perform an implicit constant conversion of the IntConstant
		///   into a different data type using casts (See Implicit Constant
		///   Expression Conversions)
		/// </summary>
		static public Expression TryImplicitIntConversion (Type target_type, IntConstant ic)
		{
			int value = ic.Value;

			if (target_type == TypeManager.sbyte_type){
				if (value >= SByte.MinValue && value <= SByte.MaxValue)
					return new SByteConstant ((sbyte) value);
			} else if (target_type == TypeManager.byte_type){
				if (value >= Byte.MinValue && value <= Byte.MaxValue)
					return new ByteConstant ((byte) value);
			} else if (target_type == TypeManager.short_type){
				if (value >= Int16.MinValue && value <= Int16.MaxValue)
					return new ShortConstant ((short) value);
			} else if (target_type == TypeManager.ushort_type){
				if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
					return new UShortConstant ((ushort) value);
			} else if (target_type == TypeManager.uint32_type){
				if (value >= 0)
					return new UIntConstant ((uint) value);
			} else if (target_type == TypeManager.uint64_type){
				//
				// we can optimize this case: a positive int32
				// always fits on a uint64.  But we need an opcode
				// to do it.
				//
				if (value >= 0)
					return new ULongConstant ((ulong) value);
			} else if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) value);
			else if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) value);
			
			if (value == 0 && ic is IntLiteral && TypeManager.IsEnumType (target_type)){
				Type underlying = TypeManager.EnumToUnderlying (target_type);
				Constant e = (Constant) ic;
				
				//
				// Possibly, we need to create a different 0 literal before passing
				// to EnumConstant
				//
				if (underlying == TypeManager.int64_type)
					e = new LongLiteral (0);
				else if (underlying == TypeManager.uint64_type)
					e = new ULongLiteral (0);

				return new EnumConstant (e, target_type);
			}

			//
			// If `target_type' is an interface and the type of `ic' implements the interface
			// e.g. target_type is IComparable, IConvertible, IFormattable
			//
			if (target_type.IsInterface && target_type.IsAssignableFrom (ic.Type))
				return new BoxedCast (ic);

			return null;
		}

		static public void Error_CannotImplicitConversion (Location loc, Type source, Type target)
		{
			if (source.Name == target.Name){
				Report.ExtraInformation (loc,
					 String.Format (
						"The type {0} has two conflicting definitions, one comes from {1} and the other from {2}",
						source.Name, source.Assembly.FullName, target.Assembly.FullName));
							 
			}
			Report.Error (29, loc, "Cannot implicitly convert type {0} to `{1}'",
				      source == TypeManager.anonymous_method_type ?
				      "anonymous method" : "`" + TypeManager.CSharpName (source) + "'",
				      TypeManager.CSharpName (target));
		}

		/// <summary>
		///   Attempts to implicitly convert `source' into `target_type', using
		///   ImplicitConversion.  If there is no implicit conversion, then
		///   an error is signaled
		/// </summary>
		static public Expression ImplicitConversionRequired (EmitContext ec, Expression source,
								     Type target_type, Location loc)
		{
			Expression e;
			
			e = ImplicitConversion (ec, source, target_type, loc);
			if (e != null)
				return e;

			if (source is DoubleLiteral) {
				if (target_type == TypeManager.float_type) {
					Error_664 (loc, "float", "f");
					return null;
				}
				if (target_type == TypeManager.decimal_type) {
					Error_664 (loc, "decimal", "m");
					return null;
				}
			}

			source.Error_ValueCannotBeConverted (loc, target_type);
			return null;
		}

		static void Error_664 (Location loc, string type, string suffix) {
			Report.Error (664, loc,
				"Literal of type double cannot be implicitly converted to type `{0}'. Add suffix `{1}' to create a literal of this type",
				type, suffix);
		}

		/// <summary>
		///   Performs the explicit numeric conversions
		/// </summary>
		static Expression ExplicitNumericConversion (EmitContext ec, Expression expr, Type target_type, Location loc)
		{
			Type expr_type = expr.Type;

			//
			// If we have an enumeration, extract the underlying type,
			// use this during the comparison, but wrap around the original
			// target_type
			//
			Type real_target_type = target_type;

			if (TypeManager.IsEnumType (real_target_type))
				real_target_type = TypeManager.EnumToUnderlying (real_target_type);

			if (ImplicitStandardConversionExists (ec, expr, real_target_type)){
 				Expression ce = ImplicitConversionStandard (ec, expr, real_target_type, loc);

				if (real_target_type != target_type)
					return new EmptyCast (ce, target_type);
				return ce;
			}
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to byte, ushort, uint, ulong, char
				//
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U1);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_CH);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to sbyte and char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U1_I1);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U1_CH);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to sbyte, byte, ushort, uint, ulong, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U1);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_CH);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to sbyte, byte, short, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_I2);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_CH);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to sbyte, byte, short, ushort, uint, ulong, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_CH);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to sbyte, byte, short, ushort, int, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_I4);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_CH);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long to sbyte, byte, short, ushort, int, uint, ulong, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_CH);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to sbyte, byte, short, ushort, int, uint, long, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_CH);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to sbyte, byte, short
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.CH_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.CH_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.CH_I2);
			} else if (expr_type == TypeManager.float_type){
				//
				// From float to sbyte, byte, short,
				// ushort, int, uint, long, ulong, char
				// or decimal
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_I8);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_CH);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr, true);
			} else if (expr_type == TypeManager.double_type){
				//
				// From double to sbyte, byte, short,
				// ushort, int, uint, long, ulong,
				// char, float or decimal
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I8);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_CH);
				if (real_target_type == TypeManager.float_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (ec, expr, true);
			} else if (expr_type == TypeManager.decimal_type) {
				//
				// From decimal to sbyte, byte, short, ushort, int, uint,
				// long, ulong, char, double or float
				//
				if (real_target_type == TypeManager.sbyte_type ||
				    real_target_type == TypeManager.byte_type ||
				    real_target_type == TypeManager.short_type ||
				    real_target_type == TypeManager.ushort_type ||
				    real_target_type == TypeManager.int32_type ||
				    real_target_type == TypeManager.uint32_type ||
				    real_target_type == TypeManager.int64_type ||
				    real_target_type == TypeManager.uint64_type ||
				    real_target_type == TypeManager.char_type ||
				    real_target_type == TypeManager.double_type ||
				    real_target_type == TypeManager.float_type)
					return new CastFromDecimal (ec, expr, target_type);
			}
			return null;
		}

		/// <summary>
		///  Returns whether an explicit reference conversion can be performed
		///  from source_type to target_type
		/// </summary>
		public static bool ExplicitReferenceConversionExists (Type source_type, Type target_type)
		{
			bool target_is_value_type = target_type.IsValueType;
			
			if (source_type == target_type)
				return true;
			
			//
			// From object to any reference type
			//
			if (source_type == TypeManager.object_type && !target_is_value_type)
				return true;
					
			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (target_type.IsSubclassOf (source_type))
				return true;

			//
			// From any interface type S to any interface T provided S is not derived from T
			//
			if (source_type.IsInterface && target_type.IsInterface){
				if (!target_type.IsSubclassOf (source_type))
					return true;
			}
			    
			//
			// From any class type S to any interface T, provided S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed &&
			    !TypeManager.ImplementsInterface (source_type, target_type))
				return true;

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface &&
			    (!target_type.IsSealed || TypeManager.ImplementsInterface (target_type, source_type)))
				return true;
			
			
			// From an array type S with an element type Se to an array type T with an 
			// element type Te provided all the following are true:
			//     * S and T differe only in element type, in other words, S and T
			//       have the same number of dimensions.
			//     * Both Se and Te are reference types
			//     * An explicit referenc conversions exist from Se to Te
			//
			if (source_type.IsArray && target_type.IsArray) {
				if (source_type.GetArrayRank () == target_type.GetArrayRank ()) {
					
					Type source_element_type = TypeManager.GetElementType (source_type);
					Type target_element_type = TypeManager.GetElementType (target_type);
					
					if (!source_element_type.IsValueType && !target_element_type.IsValueType)
						if (ExplicitReferenceConversionExists (source_element_type,
										       target_element_type))
							return true;
				}
			}
			

			// From System.Array to any array-type
			if (source_type == TypeManager.array_type &&
			    target_type.IsArray){
				return true;
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type &&
			    target_type.IsSubclassOf (TypeManager.delegate_type))
				return true;

			//
			// From ICloneable to Array or Delegate types
			//
			if (source_type == TypeManager.icloneable_type &&
			    (target_type == TypeManager.array_type ||
			     target_type == TypeManager.delegate_type))
				return true;
			
			return false;
		}

		/// <summary>
		///   Implements Explicit Reference conversions
		/// </summary>
		static Expression ExplicitReferenceConversion (Expression source, Type target_type)
		{
			Type source_type = source.Type;
			bool target_is_value_type = target_type.IsValueType;

			//
			// From object to any reference type
			//
			if (source_type == TypeManager.object_type && !target_is_value_type)
				return new ClassCast (source, target_type);

			//
			// Unboxing conversion.
			//
			if (((source_type == TypeManager.enum_type &&
				!(source is EmptyCast)) ||
				source_type == TypeManager.value_type) && target_is_value_type)
				return new UnboxCast (source, target_type);

			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (target_type.IsSubclassOf (source_type))
				return new ClassCast (source, target_type);

			//
			// From any interface type S to any interface T provided S is not derived from T
			//
			if (source_type.IsInterface && target_type.IsInterface){
				if (TypeManager.ImplementsInterface (source_type, target_type))
					return null;
				else
					return new ClassCast (source, target_type);
			}
			    
			//
			// From any class type S to any interface T, provides S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed) {
				if (TypeManager.ImplementsInterface (source_type, target_type))
					return null;
				else
					return new ClassCast (source, target_type);
				
			}

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface) {
				if (!target_type.IsSealed || TypeManager.ImplementsInterface (target_type, source_type)) {
					if (target_type.IsClass)
						return new ClassCast (source, target_type);
					else
						return new UnboxCast (source, target_type);
				}

				return null;
			}
			
			// From an array type S with an element type Se to an array type T with an 
			// element type Te provided all the following are true:
			//     * S and T differe only in element type, in other words, S and T
			//       have the same number of dimensions.
			//     * Both Se and Te are reference types
			//     * An explicit referenc conversions exist from Se to Te
			//
			if (source_type.IsArray && target_type.IsArray) {
				if (source_type.GetArrayRank () == target_type.GetArrayRank ()) {
					
					Type source_element_type = TypeManager.GetElementType (source_type);
					Type target_element_type = TypeManager.GetElementType (target_type);
					
					if (!source_element_type.IsValueType && !target_element_type.IsValueType)
						if (ExplicitReferenceConversionExists (source_element_type,
										       target_element_type))
							return new ClassCast (source, target_type);
				}
			}
			

			// From System.Array to any array-type
			if (source_type == TypeManager.array_type &&
			    target_type.IsArray) {
				return new ClassCast (source, target_type);
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type &&
			    target_type.IsSubclassOf (TypeManager.delegate_type))
				return new ClassCast (source, target_type);

			//
			// From ICloneable to Array or Delegate types
			//
			if (source_type == TypeManager.icloneable_type &&
			    (target_type == TypeManager.array_type ||
			     target_type == TypeManager.delegate_type))
				return new ClassCast (source, target_type);
			
			return null;
		}
		
		/// <summary>
		///   Performs an explicit conversion of the expression `expr' whose
		///   type is expr.Type to `target_type'.
		/// </summary>
		static public Expression ExplicitConversion (EmitContext ec, Expression expr,
							     Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Type original_expr_type = expr_type;

			if (expr_type.IsSubclassOf (TypeManager.enum_type)){
				if (target_type == TypeManager.enum_type ||
				    target_type == TypeManager.object_type) {
					if (expr is EnumConstant)
						expr = ((EnumConstant) expr).Child;
					// We really need all these casts here .... :-(
					expr = new BoxedCast (new EmptyCast (expr, expr_type));
					return new EmptyCast (expr, target_type);
				} else if ((expr_type == TypeManager.enum_type) && target_type.IsValueType &&
					   target_type.IsSubclassOf (TypeManager.enum_type))
					return new UnboxCast (expr, target_type);

				//
				// Notice that we have kept the expr_type unmodified, which is only
				// used later on to 
				if (expr is EnumConstant)
					expr = ((EnumConstant) expr).Child;
				else
					expr = new EmptyCast (expr, TypeManager.EnumToUnderlying (expr_type));
				expr_type = expr.Type;
			}

			Expression ne = ImplicitConversionStandard (ec, expr, target_type, loc);

			if (ne != null)
				return ne;

			ne = ExplicitNumericConversion (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			//
			// Unboxing conversion.
			//
			if (expr_type == TypeManager.object_type && target_type.IsValueType)
				return new UnboxCast (expr, target_type);

			//
			// Skip the ExplicitReferenceConversion because we can not convert
			// from Null to a ValueType, and ExplicitReference wont check against
			// null literal explicitly
			//
			if (expr_type != TypeManager.null_type){
				ne = ExplicitReferenceConversion (expr, target_type);
				if (ne != null)
					return ne;
			}

			if (ec.InUnsafe){
				if (target_type.IsPointer){
					if (expr_type.IsPointer)
						return new EmptyCast (expr, target_type);
					
					if (expr_type == TypeManager.sbyte_type ||
					    expr_type == TypeManager.short_type ||
					    expr_type == TypeManager.int32_type ||
					    expr_type == TypeManager.int64_type)
						return new OpcodeCast (expr, target_type, OpCodes.Conv_I);

					if (expr_type == TypeManager.ushort_type ||
					    expr_type == TypeManager.uint32_type ||
					    expr_type == TypeManager.uint64_type ||
					    expr_type == TypeManager.byte_type)
						return new OpcodeCast (expr, target_type, OpCodes.Conv_U);
				}
				if (expr_type.IsPointer){
					Expression e = null;
					
					if (target_type == TypeManager.sbyte_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
					else if (target_type == TypeManager.byte_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
					else if (target_type == TypeManager.short_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
					else if (target_type == TypeManager.ushort_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
					else if (target_type == TypeManager.int32_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
					else if (target_type == TypeManager.uint32_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
					else if (target_type == TypeManager.uint64_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
					else if (target_type == TypeManager.int64_type){
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
					}

					if (e != null){
						Expression ci, ce;

						ci = ImplicitConversionStandard (ec, e, target_type, loc);

						if (ci != null)
							return ci;

						ce = ExplicitNumericConversion (ec, e, target_type, loc);
						if (ce != null)
							return ce;
						//
						// We should always be able to go from an uint32
						// implicitly or explicitly to the other integral
						// types
						//
						throw new Exception ("Internal compiler error");
					}
				}
			}

			ne = ExplicitUserConversion (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			if (expr is Constant)
				expr.Error_ValueCannotBeConverted (loc, target_type);
			else
				Error_CannotConvertType (loc, original_expr_type, target_type);
			return null;
		}

		/// <summary>
		///   Same as ExplicitConversion, only it doesn't include user defined conversions
		/// </summary>
		static public Expression ExplicitConversionStandard (EmitContext ec, Expression expr,
								  Type target_type, Location l)
		{
			Expression ne = ImplicitConversionStandard (ec, expr, target_type, l);

			if (ne != null)
				return ne;

			ne = ExplicitNumericConversion (ec, expr, target_type, l);
			if (ne != null)
				return ne;

			ne = ExplicitReferenceConversion (expr, target_type);
			if (ne != null)
				return ne;

			Error_CannotConvertType (l, expr.Type, target_type);
			return null;
		}
	}
}

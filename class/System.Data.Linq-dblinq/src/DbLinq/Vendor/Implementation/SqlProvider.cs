﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

#if MONO_STRICT
using System.Data.Linq.Sql;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sql;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

using DbLinq.Util;

namespace DbLinq.Vendor.Implementation
{
#if MONO_STRICT
    internal
#else
    public
#endif
 class SqlProvider : ISqlProvider
    {
        /// <summary>
        /// Builds an insert clause
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="inputColumns">Columns to be inserted</param>
        /// <param name="inputValues">Values to be inserted into columns</param>
        /// <returns></returns>
        public virtual SqlStatement GetInsert(SqlStatement table, IList<SqlStatement> inputColumns, IList<SqlStatement> inputValues)
        {
            if (inputColumns.Count == 0)
                return SqlStatement.Empty;

            var insertBuilder = new SqlStatementBuilder("INSERT INTO ");
            insertBuilder.Append(table);
            insertBuilder.AppendFormat(" ({0})", SqlStatement.Join(", ", inputColumns));
            insertBuilder.Append(" VALUES");
            insertBuilder.AppendFormat(" ({0})", SqlStatement.Join(", ", inputValues));
            return insertBuilder.ToSqlStatement();
        }

        /// <summary>
        /// Builds the statements that gets back the IDs for the inserted statement
        /// </summary>
        /// <param name="outputParameters">Expected output parameters</param>
        /// <param name="outputExpressions">Expressions (to help generate output parameters)</param>
        /// <returns></returns>
        public virtual SqlStatement GetInsertIds(IList<SqlStatement> outputParameters, IList<SqlStatement> outputExpressions)
        {
            return "SELECT @@IDENTITY";
        }

        /// <summary>
        /// Builds an update clause
        /// </summary>
        /// <param name="table"></param>
        /// <param name="inputColumns">Columns to be inserted</param>
        /// <param name="inputValues">Values to be inserted into columns</param>
        /// <param name="outputParameters">Expected output parameters</param>
        /// <param name="outputExpressions">Expressions (to help generate output parameters)</param>
        /// <param name="inputPKColumns">PK columns for reference</param>
        /// <param name="inputPKValues">PK values for reference</param>
        /// <returns></returns>
        public SqlStatement GetUpdate(SqlStatement table, IList<SqlStatement> inputColumns,
            IList<SqlStatement> inputValues,
            IList<SqlStatement> outputParameters, IList<SqlStatement> outputExpressions,
            IList<SqlStatement> inputPKColumns, IList<SqlStatement> inputPKValues)
        {
            if (inputColumns.Count == 0)
                return SqlStatement.Empty;

            var updateBuilder = new SqlStatementBuilder("UPDATE ");
            updateBuilder.Append(table);
            updateBuilder.Append(" SET ");
            for (int inputIndex = 0; inputIndex < inputColumns.Count; inputIndex++)
            {
                if (inputIndex > 0)
                    updateBuilder.Append(", ");
                updateBuilder.AppendFormat("{0} = {1}", inputColumns[inputIndex], inputValues[inputIndex]);
            }
            updateBuilder.Append(" WHERE ");
            for (int pkIndex = 0; pkIndex < inputPKColumns.Count; pkIndex++)
            {
                if (pkIndex > 0)
                    updateBuilder.Append(" AND ");
                updateBuilder.AppendFormat("{0} = {1}", inputPKColumns[pkIndex], inputPKValues[pkIndex]);
            }
            return updateBuilder.ToSqlStatement();
        }

        /// <summary>
        /// Builds a delete clause
        /// </summary>
        /// <param name="table"></param>
        /// <param name="inputPKColumns">PK columns for reference</param>
        /// <param name="inputPKValues">PK values for reference</param>
        /// <returns></returns>
        public SqlStatement GetDelete(SqlStatement table, IList<SqlStatement> inputPKColumns, IList<SqlStatement> inputPKValues)
        {
            if (inputPKColumns.Count == 0)
                return SqlStatement.Empty;

            var deleteBuilder = new SqlStatementBuilder("DELETE FROM ");
            deleteBuilder.Append(table);
            deleteBuilder.Append(" WHERE ");
            for (int pkIndex = 0; pkIndex < inputPKColumns.Count; pkIndex++)
            {
                if (pkIndex > 0)
                    deleteBuilder.Append(" AND ");
                deleteBuilder.AppendFormat("{0} = {1}", inputPKColumns[pkIndex], inputPKValues[pkIndex]);
            }
            return deleteBuilder.ToSqlStatement();
        }

        public string NewLine
        {
            get { return Environment.NewLine; }
        }
        /// <summary>
        /// Converts a constant value to a literal representation
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        public virtual SqlStatement GetLiteral(object literal)
        {
            if (literal == null)
                return GetNullLiteral();
            if (literal is string)
                return GetLiteral((string)literal);
            if (literal is char)
                return GetLiteral(literal.ToString());
            if (literal is DateTime)
                return GetLiteral(literal.ToString());
            if (literal.GetType().IsArray)
                return GetLiteral((Array)literal);
            return Convert.ToString(literal, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a standard operator to an expression
        /// </summary>
        /// <param name="operationType"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual SqlStatement GetLiteral(ExpressionType operationType, IList<SqlStatement> p)
        {
            switch (operationType)
            {
            case ExpressionType.Add:
                return GetLiteralAdd(p[0], p[1]);
            case ExpressionType.AddChecked:
                return GetLiteralAddChecked(p[0], p[1]);
            case ExpressionType.And:
                return GetLiteralAnd(p[0], p[1]);
            case ExpressionType.AndAlso:
                return GetLiteralAndAlso(p[0], p[1]);
            case ExpressionType.ArrayLength:
                return GetLiteralArrayLength(p[0], p[1]);
            case ExpressionType.ArrayIndex:
                return GetLiteralArrayIndex(p[0], p[1]);
            case ExpressionType.Call:
                return GetLiteralCall(p[0]);
            case ExpressionType.Coalesce:
                return GetLiteralCoalesce(p[0], p[1]);
            case ExpressionType.Conditional:
                return GetLiteralConditional(p[0], p[1], p[2]);
            //case ExpressionType.Constant:
            //break;
            case ExpressionType.Divide:
                return GetLiteralDivide(p[0], p[1]);
            case ExpressionType.Equal:
                return GetLiteralEqual(p[0], p[1]);
            case ExpressionType.ExclusiveOr:
                return GetLiteralExclusiveOr(p[0], p[1]);
            case ExpressionType.GreaterThan:
                return GetLiteralGreaterThan(p[0], p[1]);
            case ExpressionType.GreaterThanOrEqual:
                return GetLiteralGreaterThanOrEqual(p[0], p[1]);
            //case ExpressionType.Invoke:
            //break;
            //case ExpressionType.Lambda:
            //break;
            case ExpressionType.LeftShift:
                return GetLiteralLeftShift(p[0], p[1]);
            case ExpressionType.LessThan:
                return GetLiteralLessThan(p[0], p[1]);
            case ExpressionType.LessThanOrEqual:
                return GetLiteralLessThanOrEqual(p[0], p[1]);
            //case ExpressionType.ListInit:
            //break;
            //case ExpressionType.MemberAccess:
            //    break;
            //case ExpressionType.MemberInit:
            //    break;
            case ExpressionType.Modulo:
                return GetLiteralModulo(p[0], p[1]);
            case ExpressionType.Multiply:
                return GetLiteralMultiply(p[0], p[1]);
            case ExpressionType.MultiplyChecked:
                return GetLiteralMultiplyChecked(p[0], p[1]);
            case ExpressionType.Negate:
                return GetLiteralNegate(p[0]);
            case ExpressionType.UnaryPlus:
                return GetLiteralUnaryPlus(p[0]);
            case ExpressionType.NegateChecked:
                return GetLiteralNegateChecked(p[0]);
            //case ExpressionType.New:
            //    break;
            //case ExpressionType.NewArrayInit:
            //    break;
            //case ExpressionType.NewArrayBounds:
            //    break;
            case ExpressionType.Not:
                return GetLiteralNot(p[0]);
            case ExpressionType.NotEqual:
                return GetLiteralNotEqual(p[0], p[1]);
            case ExpressionType.Or:
                return GetLiteralOr(p[0], p[1]);
            case ExpressionType.OrElse:
                return GetLiteralOrElse(p[0], p[1]);
            //case ExpressionType.Parameter:
            //    break;
            case ExpressionType.Power:
                return GetLiteralPower(p[0], p[1]);
            //case ExpressionType.Quote:
            //    break;
            case ExpressionType.RightShift:
                return GetLiteralRightShift(p[0], p[1]);
            case ExpressionType.Subtract:
                return GetLiteralSubtract(p[0], p[1]);
            case ExpressionType.SubtractChecked:
                return GetLiteralSubtractChecked(p[0], p[1]);
            //case ExpressionType.TypeAs:
            //    break;
            //case ExpressionType.TypeIs:
            //    break;
            }
            throw new ArgumentException(operationType.ToString());
        }


        /// <summary>
        /// Converts a special expression type to literal
        /// </summary>
        /// <param name="operationType"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual SqlStatement GetLiteral(SpecialExpressionType operationType, IList<SqlStatement> p)
        {
            switch (operationType) // SETuse
            {
            case SpecialExpressionType.IsNull:
                return GetLiteralIsNull(p[0]);
            case SpecialExpressionType.IsNotNull:
                return GetLiteralIsNotNull(p[0]);
            case SpecialExpressionType.Concat:
                return GetLiteralStringConcat(p[0], p[1]);
            case SpecialExpressionType.Count:
                return GetLiteralCount(p[0]);
            case SpecialExpressionType.Like:
                return GetLiteralLike(p[0], p[1]);
            case SpecialExpressionType.Min:
                return GetLiteralMin(p[0]);
            case SpecialExpressionType.Max:
                return GetLiteralMax(p[0]);
            case SpecialExpressionType.Sum:
                return GetLiteralSum(p[0]);
            case SpecialExpressionType.Average:
                return GetLiteralAverage(p[0]);
            case SpecialExpressionType.StringLength:
                return GetLiteralStringLength(p[0]);
            case SpecialExpressionType.ToUpper:
                return GetLiteralStringToUpper(p[0]);
            case SpecialExpressionType.ToLower:
                return GetLiteralStringToLower(p[0]);
            case SpecialExpressionType.In:
                return GetLiteralIn(p[0], p[1]);
            case SpecialExpressionType.Substring:
                if (p.Count > 2)
                    return GetLiteralSubString(p[0], p[1], p[2]);
                return GetLiteralSubString(p[0], p[1]);
            case SpecialExpressionType.Trim:
            case SpecialExpressionType.LTrim:
            case SpecialExpressionType.RTrim:
                return GetLiteralTrim(p[0]);
            case SpecialExpressionType.StringInsert:
                return GetLiteralStringInsert(p[0], p[1], p[2]);
            case SpecialExpressionType.Replace:
                return GetLiteralStringReplace(p[0], p[1], p[2]);
            case SpecialExpressionType.Remove:
                if (p.Count > 2)
                    return GetLiteralStringRemove(p[0], p[1], p[2]);
                return GetLiteralStringRemove(p[0], p[1]);
            case SpecialExpressionType.IndexOf:
                if (p.Count == 2)
                    return GetLiteralStringIndexOf(p[0], p[1]);
                else if (p.Count == 3)
                    return GetLiteralStringIndexOf(p[0], p[1], p[2]);
                else if (p.Count == 4)
                    return GetLiteralStringIndexOf(p[0], p[1], p[2], p[3]);
                break;
            case SpecialExpressionType.Year:
            case SpecialExpressionType.Month:
            case SpecialExpressionType.Day:
            case SpecialExpressionType.Hour:
            case SpecialExpressionType.Minute:
            case SpecialExpressionType.Second:
            case SpecialExpressionType.Millisecond:
                return GetLiteralDateTimePart(p[0], operationType);
            case SpecialExpressionType.DateDiffInMilliseconds:
                return GetLiteralDateDiff(p[0], p[1]);
            case SpecialExpressionType.Abs:
                return GetLiteralMathAbs(p[0]);
            case SpecialExpressionType.Exp:
                return GetLiteralMathExp(p[0]);
            case SpecialExpressionType.Floor:
                return GetLiteralMathFloor(p[0]);
            case SpecialExpressionType.Ln:
                return GetLiteralMathLn(p[0]);

            case SpecialExpressionType.Log:
                if (p.Count == 1)
                    return GetLiteralMathLog(p[0]);
                else
                    return GetLiteralMathLog(p[0], p[1]);
            case SpecialExpressionType.Pow:
                return GetLiteralMathPow(p[0], p[1]);
            case SpecialExpressionType.Round:
                return GetLiteralMathRound(p[0]);
            case SpecialExpressionType.Sign:
                return GetLiteralMathSign(p[0]);
            case SpecialExpressionType.Sqrt:
                return GetLiteralMathSqrt(p[0]);

            }
            throw new ArgumentException(operationType.ToString());
        }

        protected virtual SqlStatement GetLiteralMathSqrt(SqlStatement p)
        {
            return SqlStatement.Format("SQRT({0})", p);
        }

        protected virtual SqlStatement GetLiteralMathSign(SqlStatement p)
        {
            return SqlStatement.Format("SIGN({0})", p);
        }

        protected virtual SqlStatement GetLiteralMathRound(SqlStatement p)
        {
            return SqlStatement.Format("ROUND({0})", p);
        }

        protected virtual SqlStatement GetLiteralMathPow(SqlStatement p, SqlStatement p_2)
        {
            return SqlStatement.Format("POW({0},{1})", p, p_2);
        }

        protected virtual SqlStatement GetLiteralMathLog(SqlStatement p)
        {
            return SqlStatement.Format("LOG({0})", p);
        }

        protected virtual SqlStatement GetLiteralMathLog(SqlStatement p, SqlStatement p_2)
        {
            return SqlStatement.Format("LOG({0},{1})", p, p_2);
        }

        protected virtual SqlStatement GetLiteralMathLn(SqlStatement p)
        {
            return SqlStatement.Format("LN({0})", p);
        }

        protected virtual SqlStatement GetLiteralMathFloor(SqlStatement p)
        {
            return SqlStatement.Format("FLOOR({0})", p);
        }

        protected virtual SqlStatement GetLiteralMathExp(SqlStatement p)
        {
            return SqlStatement.Format("EXP({0})", p);
        }

        protected virtual SqlStatement GetLiteralMathAbs(SqlStatement p)
        {
            return SqlStatement.Format("ABS({0})", p);
        }

        /// <summary>
        /// It should return a int with de difference in milliseconds between two dates.
        /// It is used in a lot of tasks, ie: operations of timespams ej: timespam.Minutes or timespam.TotalMinutes
        /// </summary>
        /// <remarks>
        /// In the implementation you should pay atention in overflows inside the database engine, since a difference of dates in milliseconds
        /// maybe deliver a very big integer int. Ie: sqlServer provider  has to do some tricks with castings for implementing such requeriments.
        /// </remarks>
        /// <param name="dateA"></param>
        /// <param name="dateB"></param>
        /// <returns></returns>
        protected virtual SqlStatement GetLiteralDateDiff(SqlStatement dateA, SqlStatement dateB)
        {
            return SqlStatement.Format("DATEDIFF(MILLISECOND,{0},{1})", dateA, dateB);
        }


        protected virtual SqlStatement GetLiteralDateTimePart(SqlStatement dateExpression, SpecialExpressionType operationType)
        {
            return SqlStatement.Format("EXTRACT({0} FROM {1})", operationType.ToString().ToUpper(), dateExpression);
        }

        /// <summary>
        /// This function should return the first index of the string 'searchString' in a string 'baseString'. 
        /// This function is usually used in others methods of this sqlprovider.
        /// </summary>
        /// <remarks>
        /// In the impleementation you should pay atention that in some database engines the indexes of arrays or strings are shifted one unit.
        /// ie: in .NET stringExpression.Substring(2,2) should be translated as SUBSTRING (stringExpression, 3 , 2) since the first element in sqlserver in a string has index=1
        /// </remarks>
        /// <param name="baseString"></param>
        /// <param name="searchString"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual SqlStatement GetLiteralStringIndexOf(SqlStatement baseString, SqlStatement searchString, SqlStatement startIndex, SqlStatement count)
        {
            var translatedStartIndex = GetLiteralAdd(startIndex, "1");
            //SUBSTRING(baseString,StartIndex+1)
            var substring = GetLiteralSubString(baseString, translatedStartIndex, count);

            //STRPOS(SUBSTRING(baseString,StartIndex),searchString)---> range 1:n , 0 => doesn't exist
            var substringIndexOf = SqlStatement.Format("STRPOS({0},{1})", substring, searchString);

            //(STRPOS(SUBSTRING(baseString,StartIndex),searchString)+ startIndex+1)*STRPOS(SUBSTRING(baseString,StartIndex),searchString)
            // range 1:m, m>n, 0=>doesn't exist
            var literalRangeShifted = GetLiteralMultiply(GetLiteralAdd(substringIndexOf, translatedStartIndex), substringIndexOf);

            return GetLiteralSubtract(literalRangeShifted, "1");
        }

        /// <summary>
        /// This function should return the first index of the string 'searchString' in a string 'baseString' but starting in 'the startIndex' index . This can be a problem since most of database
        /// engines doesn't have such overload of SUBSTR, the base implementation do it in a pretty complex with the goal of be most generic syntax as possible using a set of primitives(SUBSTRING(X,X,X) and STRPOS(X,X),+ , *).
        /// This function is usually used in others methods of this sqlprovider.
        /// </summary>
        /// <remarks>
        /// In the impleementation you should pay atention that in some database engines the indexes of arrays or strings are shifted one unit.
        /// ie: in .NET stringExpression.Substring(2,2) should be translated as SUBSTRING (stringExpression, 3 , 2) since the first element in sqlserver in a SqlStatement has index=1
        protected virtual SqlStatement GetLiteralStringIndexOf(SqlStatement baseString, SqlStatement searchString, SqlStatement startIndex)
        {
            //SUBSTRING(baseString,StartIndex)
            var substring = GetLiteralSubString(baseString, startIndex);

            //STRPOS(SUBSTRING(baseString,StartIndex),searchString)---> range 1:n , 0 => doesn't exist
            var substringIndexOf = SqlStatement.Format("STRPOS({0},{1})", substring, searchString);

            //(STRPOS(SUBSTRING(baseString,StartIndex),searchString)+ startIndex)*STRPOS(SUBSTRING(baseString,StartIndex),searchString)
            // range 1:m, m>n, 0=>doesn't exist
            var literalRangeShifted = GetLiteralMultiply(GetLiteralAdd(substringIndexOf, startIndex), substringIndexOf);

            return GetLiteralSubtract(literalRangeShifted, "1");
        }

        protected virtual SqlStatement GetLiteralStringIndexOf(SqlStatement baseString, SqlStatement searchString)
        {
            return GetLiteralSubtract(SqlStatement.Format("STRPOS({0},{1})", baseString, searchString), "1");
        }

        protected virtual SqlStatement GetLiteralStringRemove(SqlStatement baseString, SqlStatement startIndex, SqlStatement count)
        {
            return GetLiteralStringConcat(
                    GetLiteralSubString(baseString, "1", startIndex),
                    GetLiteralSubString(baseString, GetLiteralAdd(startIndex, count).ToString(), GetLiteralStringLength(baseString)));
        }

        protected virtual SqlStatement GetLiteralStringRemove(SqlStatement baseString, SqlStatement startIndex)
        {
            return GetLiteralSubString(baseString, "1", startIndex);
        }

        protected SqlStatement GetLiteralStringReplace(SqlStatement stringExpresision, SqlStatement searchString, SqlStatement replacementstring)
        {
            return SqlStatement.Format("REPLACE({0},{1},{2})", stringExpresision, searchString, replacementstring);
        }

        protected virtual SqlStatement GetLiteralStringInsert(SqlStatement stringExpression, SqlStatement position, SqlStatement insertString)
        {

            return this.GetLiteralStringConcat(
                            this.GetLiteralStringConcat(
                                            GetLiteralSubString(stringExpression, "1", position),
                                            insertString),
                            this.GetLiteralSubString(stringExpression, GetLiteralAdd(position, "1")));
        }


        /// <summary>
        /// Returns an operation between two SELECT clauses (UNION, UNION ALL, etc.)
        /// </summary>
        /// <param name="selectOperator"></param>
        /// <param name="selectA"></param>
        /// <param name="selectB"></param>
        /// <returns></returns>
        public virtual SqlStatement GetLiteral(SelectOperatorType selectOperator, SqlStatement selectA, SqlStatement selectB)
        {
            switch (selectOperator)
            {
            case SelectOperatorType.Union:
                return GetLiteralUnion(selectA, selectB);
            case SelectOperatorType.UnionAll:
                return GetLiteralUnionAll(selectA, selectB);
            case SelectOperatorType.Intersection:
                return GetLiteralIntersect(selectA, selectB);
            case SelectOperatorType.Exception:
                return GetLiteralExcept(selectA, selectB);
            default:
                throw new ArgumentOutOfRangeException(selectOperator.ToString());
            }
        }

        /// <summary>
        /// Places the expression into parenthesis
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public virtual SqlStatement GetParenthesis(SqlStatement a)
        {
            return SqlStatement.Format("({0})", a);
        }

        /// <summary>
        /// Returns a column related to a table.
        /// Ensures about the right case
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual string GetColumn(string table, string column)
        {
            return string.Format("{0}.{1}", table, GetColumn(column));
        }

        /// <summary>
        /// Returns a column related to a table.
        /// Ensures about the right case
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetColumn(string column)
        {
            return GetSafeNamePart(column);
        }

        /// <summary>
        /// Returns a table alias
        /// Ensures about the right case
        /// </summary>
        /// <param name="table"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public virtual string GetTableAsAlias(string table, string alias)
        {
            return string.Format("{0} {1}", GetTable(table), GetTableAlias(alias));
        }

        /// <summary>
        /// Returns a table alias
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public virtual string GetTable(string table)
        {
            // we use the full version, since the table name may include the schema
            return GetSafeName(table);
        }

        /// <summary>
        /// Joins a list of table selection to make a FROM clause
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        public virtual SqlStatement GetFromClause(SqlStatement[] tables)
        {
            if (tables.Length == 0)
                return SqlStatement.Empty;
            return SqlStatement.Format("FROM {0}", SqlStatement.Join(", ", tables));
        }

        /// <summary>
        /// Concatenates all join clauses
        /// </summary>
        /// <param name="joins"></param>
        /// <returns></returns>
        public virtual SqlStatement GetJoinClauses(SqlStatement[] joins)
        {
            if (joins.Length == 0)
                return SqlStatement.Empty;
            var space = " ";
            return space + SqlStatement.Join(NewLine + space, joins);
        }

        /// <summary>
        /// Returns an INNER JOIN syntax
        /// </summary>
        /// <param name="joinedTable"></param>
        /// <param name="joinExpression"></param>
        /// <returns></returns>
        public virtual SqlStatement GetInnerJoinClause(SqlStatement joinedTable, SqlStatement joinExpression)
        {
            return SqlStatement.Format("INNER JOIN {0} ON {1}", joinedTable, joinExpression);
        }

        /// <summary>
        /// Returns a LEFT JOIN syntax
        /// </summary>
        /// <param name="joinedTable"></param>
        /// <param name="joinExpression"></param>
        /// <returns></returns>
        public virtual SqlStatement GetLeftOuterJoinClause(SqlStatement joinedTable, SqlStatement joinExpression)
        {
            return SqlStatement.Format("LEFT JOIN {0} ON {1}", joinedTable, joinExpression);
        }

        /// <summary>
        /// Returns a RIGHT JOIN syntax
        /// </summary>
        /// <param name="joinedTable"></param>
        /// <param name="joinExpression"></param>
        /// <returns></returns>
        public virtual SqlStatement GetRightOuterJoinClause(SqlStatement joinedTable, SqlStatement joinExpression)
        {
            return SqlStatement.Format("RIGHT JOIN {0} ON {1}", joinedTable, joinExpression);
        }

        /// <summary>
        /// Joins a list of conditions to make a WHERE clause
        /// </summary>
        /// <param name="wheres"></param>
        /// <returns></returns>
        public virtual SqlStatement GetWhereClause(SqlStatement[] wheres)
        {
            if (wheres.Length == 0)
                return SqlStatement.Empty;
            return SqlStatement.Format("WHERE {0}", SqlStatement.Join(" AND ", wheres));
        }

        /// <summary>
        /// Joins a list of conditions to make a HAVING clause
        /// </summary>
        /// <param name="havings"></param>
        /// <returns></returns>
        public virtual SqlStatement GetHavingClause(SqlStatement[] havings)
        {
            if (havings.Length == 0)
                return SqlStatement.Empty;
            return SqlStatement.Format("HAVING {0}", SqlStatement.Join(" AND ", havings));
        }

        /// <summary>
        /// Joins a list of operands to make a SELECT clause
        /// </summary>
        /// <param name="selects"></param>
        /// <returns></returns>
        public virtual SqlStatement GetSelectClause(SqlStatement[] selects)
        {
            if (selects.Length == 0)
                return SqlStatement.Empty;
            return SqlStatement.Format("SELECT {0}", SqlStatement.Join(", ", selects));
        }

        /// <summary>
        /// Returns all table columns (*)
        /// </summary>
        /// <returns></returns>
        public virtual string GetColumns()
        {
            return "*";
        }

        /// <summary>
        /// Returns a literal parameter name
        /// </summary>
        /// <returns></returns>
        public virtual string GetParameterName(string nameBase)
        {
            return string.Format(":{0}", nameBase);
        }

        /// <summary>
        /// Returns a valid alias syntax for the given table
        /// </summary>
        /// <param name="nameBase"></param>
        /// <returns></returns>
        public virtual string GetTableAlias(string nameBase)
        {
            return string.Format("{0}$", nameBase);
        }

        protected virtual SqlStatement GetLiteralAdd(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} + {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralAddChecked(SqlStatement a, SqlStatement b)
        {
            return GetLiteralAdd(a, b);
        }

        protected virtual SqlStatement GetLiteralAnd(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} AND {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralAndAlso(SqlStatement a, SqlStatement b)
        {
            return GetLiteralAnd(a, b);
        }

        protected virtual SqlStatement GetLiteralArrayLength(SqlStatement a, SqlStatement b)
        {
            throw new NotImplementedException();
        }

        protected virtual SqlStatement GetLiteralArrayIndex(SqlStatement a, SqlStatement b)
        {
            throw new NotImplementedException();
        }

        protected virtual SqlStatement GetLiteralCall(SqlStatement a)
        {
            throw new NotImplementedException();
        }

        protected virtual SqlStatement GetLiteralCoalesce(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("COALESCE({0}, {1})", a, b);
        }

        protected virtual SqlStatement GetLiteralConditional(SqlStatement a, SqlStatement b, SqlStatement c)
        {
            throw new NotImplementedException();
        }

        public virtual SqlStatement GetLiteralConvert(SqlStatement a, Type newType)
        {
            return a;
        }

        protected virtual SqlStatement GetLiteralDivide(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} / {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralEqual(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} = {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralExclusiveOr(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} XOR {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralGreaterThan(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} > {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralGreaterThanOrEqual(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} >= {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralLeftShift(SqlStatement a, SqlStatement b)
        {
            throw new NotImplementedException();
        }

        protected virtual SqlStatement GetLiteralLessThan(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} < {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralLessThanOrEqual(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} <= {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralModulo(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} % {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralMultiply(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} * {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralMultiplyChecked(SqlStatement a, SqlStatement b)
        {
            return GetLiteralMultiply(a, b);
        }

        protected virtual SqlStatement GetLiteralNegate(SqlStatement a)
        {
            return SqlStatement.Format("-{0}", a);
        }

        protected virtual SqlStatement GetLiteralUnaryPlus(SqlStatement a)
        {
            return SqlStatement.Format("+{0}", a);
        }

        protected virtual SqlStatement GetLiteralNegateChecked(SqlStatement a)
        {
            return GetLiteralNegate(a);
        }

        protected virtual SqlStatement GetLiteralNot(SqlStatement a)
        {
            return SqlStatement.Format("NOT {0}", a);
        }

        protected virtual SqlStatement GetLiteralNotEqual(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} <> {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralOr(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} OR {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralOrElse(SqlStatement a, SqlStatement b)
        {
            return GetLiteralOr(a, b);
        }

        protected virtual SqlStatement GetLiteralPower(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("POWER ({0}, {1})", a, b);
        }

        protected virtual SqlStatement GetLiteralRightShift(SqlStatement a, SqlStatement b)
        {
            throw new NotImplementedException();
        }

        protected virtual SqlStatement GetLiteralSubtract(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} - {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralSubtractChecked(SqlStatement a, SqlStatement b)
        {
            return GetLiteralSubtract(a, b);
        }

        protected virtual SqlStatement GetLiteralIsNull(SqlStatement a)
        {
            return SqlStatement.Format("{0} IS NULL", a);
        }

        protected virtual SqlStatement GetLiteralIsNotNull(SqlStatement a)
        {
            return SqlStatement.Format("{0} IS NOT NULL", a);
        }

        protected virtual SqlStatement GetLiteralStringConcat(SqlStatement a, SqlStatement b)
        {
            // for some vendors, it is "CONCAT(a,b)"
            return SqlStatement.Format("{0} || {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralStringLength(SqlStatement a)
        {
            return SqlStatement.Format("CHARACTER_LENGTH({0})", a);
        }

        protected virtual SqlStatement GetLiteralStringToUpper(SqlStatement a)
        {
            return SqlStatement.Format("UCASE({0})", a);
        }

        protected virtual SqlStatement GetLiteralStringToLower(SqlStatement a)
        {
            return SqlStatement.Format("LCASE({0})", a);
        }


        protected virtual SqlStatement GetLiteralTrim(SqlStatement a)
        {
            return SqlStatement.Format("TRIM({0})", a);
        }

        protected virtual SqlStatement GetLiteralLTrim(SqlStatement a)
        {
            return SqlStatement.Format("LTRIM({0})", a);
        }

        protected virtual SqlStatement GetLiteralRTrim(SqlStatement a)
        {
            return SqlStatement.Format("RTRIM({0})", a);
        }

        protected virtual SqlStatement GetLiteralSubString(SqlStatement baseString, SqlStatement startIndex, SqlStatement count)
        {
            //in standard sql base SqlStatement index is 1 instead 0
            return SqlStatement.Format("SUBSTR({0}, {1}, {2})", baseString, GetLiteralAdd(startIndex, "1"), count);
        }

        protected virtual SqlStatement GetLiteralSubString(SqlStatement baseString, SqlStatement startIndex)
        {
            //in standard sql base SqlStatement index is 1 instead 0
            return SqlStatement.Format("SUBSTR({0}, {1})", baseString, GetLiteralAdd(startIndex, "1"));
        }

        protected virtual SqlStatement GetLiteralLike(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} LIKE {1}", a, b);
        }

        protected virtual SqlStatement GetLiteralCount(SqlStatement a)
        {
            return SqlStatement.Format("COUNT({0})", a);
        }

        protected virtual SqlStatement GetLiteralMin(SqlStatement a)
        {
            return SqlStatement.Format("MIN({0})", a);
        }

        protected virtual SqlStatement GetLiteralMax(SqlStatement a)
        {
            return SqlStatement.Format("MAX({0})", a);
        }

        protected virtual SqlStatement GetLiteralSum(SqlStatement a)
        {
            return SqlStatement.Format("SUM({0})", a);
        }

        protected virtual SqlStatement GetLiteralAverage(SqlStatement a)
        {
            return SqlStatement.Format("AVG({0})", a);
        }

        protected virtual SqlStatement GetLiteralIn(SqlStatement a, SqlStatement b)
        {
            return SqlStatement.Format("{0} IN {1}", a, b);
        }

        protected virtual SqlStatement GetNullLiteral()
        {
            return "NULL";
        }

        /// <summary>
        /// Returns a LIMIT clause around a SELECT clause
        /// </summary>
        /// <param name="select">SELECT clause</param>
        /// <param name="limit">limit value (number of columns to be returned)</param>
        /// <returns></returns>
        public virtual SqlStatement GetLiteralLimit(SqlStatement select, SqlStatement limit)
        {
            return SqlStatement.Format("{0} LIMIT {1}", select, limit);
        }

        /// <summary>
        /// Returns a LIMIT clause around a SELECT clause, with offset
        /// </summary>
        /// <param name="select">SELECT clause</param>
        /// <param name="limit">limit value (number of columns to be returned)</param>
        /// <param name="offset">first row to be returned (starting from 0)</param>
        /// <param name="offsetAndLimit">limit+offset</param>
        /// <returns></returns>
        public virtual SqlStatement GetLiteralLimit(SqlStatement select, SqlStatement limit, SqlStatement offset, SqlStatement offsetAndLimit)
        {
            // default SQL syntax: LIMIT limit OFFSET offset
            return SqlStatement.Format("{0} LIMIT {1} OFFSET {2}", select, limit, offset);
        }

        protected virtual string GetLiteral(string str)
        {
            return string.Format("'{0}'", str.Replace("'", "''"));
        }

        protected virtual SqlStatement GetLiteral(Array array)
        {
            var listItems = new List<SqlStatement>();
            foreach (object o in array)
                listItems.Add(GetLiteral(o));
            return SqlStatement.Format("({0})", SqlStatement.Join(", ", listItems.ToArray()));
        }

        /// <summary>
        /// Returns an ORDER criterium
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public virtual SqlStatement GetOrderByColumn(SqlStatement expression, bool descending)
        {
            if (!descending)
                return expression;
            return SqlStatement.Format("{0} DESC", expression);
        }

        /// <summary>
        /// Joins a list of conditions to make a ORDER BY clause
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public virtual SqlStatement GetOrderByClause(SqlStatement[] orderBy)
        {
            if (orderBy.Length == 0)
                return SqlStatement.Empty;
            return SqlStatement.Format("ORDER BY {0}", SqlStatement.Join(", ", orderBy));
        }

        /// <summary>
        /// Joins a list of conditions to make a GROUP BY clause
        /// </summary>
        /// <param name="groupBy"></param>
        /// <returns></returns>
        public virtual SqlStatement GetGroupByClause(SqlStatement[] groupBy)
        {
            if (groupBy.Length == 0)
                return SqlStatement.Empty;
            return SqlStatement.Format("GROUP BY {0}", SqlStatement.Join(", ", groupBy));
        }

        protected virtual SqlStatement GetLiteralUnion(SqlStatement selectA, SqlStatement selectB)
        {
            return SqlStatement.Format("{0}{2}UNION{2}{1}", selectA, selectB, NewLine);
        }

        protected virtual SqlStatement GetLiteralUnionAll(SqlStatement selectA, SqlStatement selectB)
        {
            return SqlStatement.Format("{0}{2}UNION ALL{2}{1}", selectA, selectB, NewLine);
        }

        protected virtual SqlStatement GetLiteralIntersect(SqlStatement selectA, SqlStatement selectB)
        {
            return SqlStatement.Format("{0}{2}INTERSECT{2}{1}", selectA, selectB, NewLine);
        }

        protected virtual SqlStatement GetLiteralExcept(SqlStatement selectA, SqlStatement selectB)
        {
            return SqlStatement.Format("{0}{2}EXCEPT{2}{1}", selectA, selectB, NewLine);
        }

        public virtual string GetSafeName(string name)
        {
            string[] nameParts = name.Split('.');
            for (int index = 0; index < nameParts.Length; index++)
            {
                nameParts[index] = GetSafeNamePart(nameParts[index]);
            }
            return string.Join(".", nameParts);
        }

        protected virtual string GetSafeNamePart(string namePart)
        {
            if (IsMadeSafe(namePart))
                return namePart;
            if (IsNameSafe(namePart) && IsNameCaseSafe(namePart))
                return namePart;
            return MakeNameSafe(namePart);
        }

        protected virtual bool IsMadeSafe(string namePart)
        {
            var l = namePart.Length;
            if (l < 2)
                return false;
            return namePart[0] == SafeNameStartQuote && namePart[l - 1] == SafeNameEndQuote;
        }

        protected virtual bool IsNameCaseSafe(string namePart)
        {
            foreach (char c in namePart)
            {
                if (char.IsLower(c))
                    return false;
            }
            return true;
        }

        protected virtual char SafeNameStartQuote { get { return '"'; } }
        protected virtual char SafeNameEndQuote { get { return '"'; } }

        protected virtual string MakeNameSafe(string namePart)
        {
            return namePart.Enquote(SafeNameStartQuote, SafeNameEndQuote);
        }

        /// <summary>
        /// Determines if a given field is dangerous (related to a SQL keyword or containing problematic characters)
        /// </summary>
        protected virtual bool IsNameSafe(string name)
        {
            var nameL = name.ToLower();
            switch (nameL)
            {
            case "user":
            case "bit":
            case "int":
            case "smallint":
            case "tinyint":
            case "mediumint":

            case "float":
            case "double":
            case "real":
            case "decimal":
            case "numeric":

            case "blob":
            case "text":
            case "char":
            case "varchar":

            case "date":
            case "time":
            case "datetime":
            case "timestamp":
            case "year":

                return false;
            default:
                return !name.Contains(' ');
            }
        }

        private static readonly Regex _fieldIdentifierEx = new Regex(@"\[(?<var>[\w.]+)\]",
                                                                     RegexOptions.Singleline |
                                                                     RegexOptions.ExplicitCapture |
                                                                     RegexOptions.Compiled);

        public virtual string GetSafeQuery(string sqlString)
        {
            if (sqlString == null)
                return null;
            return _fieldIdentifierEx.Replace(sqlString, delegate(Match e)
            {
                var field = e.Groups[1].Value;
                var safeField = GetSafeNamePart(field);
                return safeField;
            });
        }
    }
}

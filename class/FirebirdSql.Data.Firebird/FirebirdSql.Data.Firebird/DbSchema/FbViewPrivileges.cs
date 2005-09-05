/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Data;
using System.Globalization;
using System.Text;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbViewPrivileges : FbDbSchema
	{
		#region Constructors

		public FbViewPrivileges() : base("ViewPrivileges")
		{
		}

		#endregion

		#region Protected Methods

		protected override StringBuilder GetCommandText(object[] restrictions)
		{
			StringBuilder sql = new StringBuilder();
			StringBuilder where = new StringBuilder();

			sql.Append(
				@"SELECT " +
					"null AS VIEW_CATALOG, " +
					"null AS VIEW_SCHEMA, " +
					"priv.rdb$relation_name AS VIEW_NAME, " +
					"priv.rdb$user AS GRANTEE, " +
					"priv.rdb$grantor AS GRANTOR, " +
					"priv.rdb$privilege AS PRIVILEGE, " +
					"priv.rdb$grant_option AS WITH_GRANT " +
				"FROM " +
					"rdb$user_privileges priv " +
					"left join rdb$relations rel ON priv.rdb$relation_name = rel.rdb$relation_name");

			where.Append("priv.rdb$object_type = 0");
			where.Append(" AND rel.rdb$view_source IS NOT NULL");

			if (restrictions != null)
			{
				int index = 0;

				/* VIEW_CATALOG	*/
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* VIEW_SCHEMA */
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* VIEW_NAME */
				if (restrictions.Length >= 3 && restrictions[2] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, " AND priv.rdb$relation_name = @p{0}", index++);
				}

				/* GRANTOR */
				if (restrictions.Length >= 4 && restrictions[3] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, " AND priv.rdb$grantor = @p{0}", index++);
				}

				/* GRANTEE */
				if (restrictions.Length >= 5 && restrictions[4] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, " AND priv.rdb$user = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY priv.rdb$relation_name, priv.rdb$user");

			return sql;
		}

		#endregion
	}
}
#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System.Data;
using System.Data.SqlClient;
using DotNetNuke.Common.Utilities;
using PetaPoco;

namespace DotNetNuke.Data.PetaPoco
{
    public static class PetaPocoHelper
    {
        #region Public Methods

        public static void ExecuteNonQuery(string connectionString, CommandType type, string sql, params object[] args)
        {
			ExecuteNonQuery(connectionString, type, Null.NullInteger, sql, args);
        }

        public static void ExecuteNonQuery(string connectionString, CommandType type, int timeout, string sql, params object[] args)
		{
            using (var database = new Database(connectionString, "System.Data.SqlClient") { EnableAutoSelect = false })
            { 
                if (type == CommandType.StoredProcedure)
                {
                    sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
                }

                if (timeout > 0)
                {
                    database.CommandTimeout = timeout;
                }

                database.Execute(sql, args);
            }
		}

        public static void BulkInsert(string connectionString, string procedureName, string tableParameterName, DataTable dataTable)
        {
            BulkInsert(connectionString, Null.NullInteger, procedureName, tableParameterName, dataTable);
        }

        public static void BulkInsert(string connectionString, int timeout, string procedureName, string tableParameterName, DataTable dataTable)
		{
            if (dataTable.Rows.Count > 0)
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(procedureName, con))
                {
                    if (!tableParameterName.StartsWith("@"))
                        tableParameterName = "@" + tableParameterName;

                    if (timeout > 0)
                        cmd.CommandTimeout = timeout;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue(tableParameterName, dataTable);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

		public static IDataReader ExecuteReader(string connectionString, CommandType type, string sql, params object[] args)
		{
			return ExecuteReader(connectionString, type, Null.NullInteger, sql, args);
		}

        public static IDataReader ExecuteReader(string connectionString, CommandType type, int timeout, string sql, params object[] args)
        {
            var database = new Database(connectionString, "System.Data.SqlClient") { EnableAutoSelect = false };

            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

			if (timeout > 0)
			{
				database.CommandTimeout = timeout;
			}
            return database.ExecuteReader(sql, args);
        }

        public static T ExecuteScalar<T>(string connectionString, CommandType type, string sql, params object[] args)
        {
			return ExecuteScalar<T>(connectionString, type, Null.NullInteger, sql, args);
        }

		public static T ExecuteScalar<T>(string connectionString, CommandType type, int timeout, string sql, params object[] args)
		{
            using (var database = new Database(connectionString, "System.Data.SqlClient") { EnableAutoSelect = false })
            {
                if (type == CommandType.StoredProcedure)
                {
                    sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
                }

                if (timeout > 0)
                {
                    database.CommandTimeout = timeout;
                }

                return database.ExecuteScalar<T>(sql, args);
            }
		}

        // ReSharper disable once InconsistentNaming
        public static void ExecuteSQL(string connectionString, string sql)
        {
            ExecuteSQL(connectionString, sql, Null.NullInteger);
        }

        // ReSharper disable once InconsistentNaming
		public static void ExecuteSQL(string connectionString, string sql, int timeout)
		{
            using (var database = new Database(connectionString, "System.Data.SqlClient") { EnableAutoSelect = false })
            {
                if (timeout > 0)
                {
                    database.CommandTimeout = timeout;
                }

                database.Execute(sql);
            }
		}

        #endregion
    }
}

#region Copyright
// 
// DotNetNuke® - https://www.dnnsoftware.com
// Copyright (c) 2002-2014
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

#region usings

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DotNetNuke.Data.PetaPoco;

#endregion

namespace DotNetNuke.Data
{
    public class SqlDatabaseConnectionProvider : DatabaseConnectionProvider
    {
        public override int ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (commandTimeout > 0)
                    command.CommandTimeout = commandTimeout;

                connection.Open();
                try
                {
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public override void ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string procedure, object[] commandParameters)
        {
            PetaPocoHelper.ExecuteNonQuery(connectionString, commandType, commandTimeout, procedure, commandParameters);
        }

        public override IDataReader ExecuteSql(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
            //return SqlHelper.ExecuteReader(ConnectionString, CommandType, Query);
            return PetaPocoHelper.ExecuteReader(connectionString, commandType, commandTimeout, query);
        }

        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters)
        {
            return PetaPocoHelper.ExecuteReader(connectionString, commandType, commandTimeout, procedureName, commandParameters);
        }

        public override T ExecuteScalar<T>(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters)
        {
            return PetaPocoHelper.ExecuteScalar<T>(connectionString, commandType, commandTimeout, procedureName, commandParameters);
        }

        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable)
        {
            PetaPocoHelper.BulkInsert(connectionString, commandTimeout, procedureName, tableParameterName, dataTable);
        }

        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters)
        {
            PetaPocoHelper.BulkInsert(connectionString, procedureName, tableParameterName, dataTable, commandTimeout, commandParameters);
        }
    }
}

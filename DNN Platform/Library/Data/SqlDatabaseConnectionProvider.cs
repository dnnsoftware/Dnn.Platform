// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

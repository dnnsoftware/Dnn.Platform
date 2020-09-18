// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data.Fakes
{
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Data;

    internal class FakeDbConnectionProvider : DatabaseConnectionProvider
    {
        public override int ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
            throw new System.NotImplementedException();
        }

        public override void ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string procedure, object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSql(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, int commandTimeout, string procedureName,
            params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable)
        {
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable,
            Dictionary<string, object> commandParameters)
        {
            throw new System.NotImplementedException();
        }
    }
}

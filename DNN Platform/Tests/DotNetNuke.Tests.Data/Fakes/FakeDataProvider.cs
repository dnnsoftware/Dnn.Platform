// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data.Fakes
{
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Data;

    internal class FakeDataProvider : DataProvider
    {
        public FakeDataProvider(Dictionary<string, string> settings)
        {
            this.Settings = settings;
        }

        public override bool IsConnectionValid
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Dictionary<string, string> Settings { get; }

        public override void ExecuteNonQuery(string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override void ExecuteNonQuery(int timeoutSec, string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable)
        {
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec)
        {
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec,
            Dictionary<string, object> commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(int timeoutSec, string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(int timeoutSec, string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQL(string sql)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQL(string sql, int timeoutSec)
        {
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string script)
        {
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string script, int timeoutSec)
        {
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string connectionString, string sql)
        {
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string connectionString, string sql, int timeoutSec)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec, out string errorMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}

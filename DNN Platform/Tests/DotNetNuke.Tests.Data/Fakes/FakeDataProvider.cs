﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Data;

using DotNetNuke.Data;

namespace DotNetNuke.Tests.Data.Fakes
{
    internal class FakeDataProvider : DataProvider
    {
        public FakeDataProvider(Dictionary<string, string> settings )
        {
            Settings = settings;
        }

        #region Overrides of DataProvider

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

        #endregion
    }
}

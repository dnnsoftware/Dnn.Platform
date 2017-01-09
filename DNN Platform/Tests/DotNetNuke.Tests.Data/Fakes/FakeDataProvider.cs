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

        public override void ExecuteNonQuery(int timeout, string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable)
        {
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeout)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(int timeout, string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQL(string sql)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQL(string sql, int timeout)
        {
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string script)
        {
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string script, int timeout)
        {
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string connectionString, string sql)
        {
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string connectionString, string sql, int timeout)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeout)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeout, out string errorMessage)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}

// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region usings

using System.Collections.Generic;
using System.Data;

using DotNetNuke.ComponentModel;

#endregion

namespace DotNetNuke.Data
{
    public abstract class DatabaseConnectionProvider
    {
        #region Shared/Static Methods

        public static DatabaseConnectionProvider Instance()
        {
            return ComponentFactory.GetComponent<DatabaseConnectionProvider>();
        }

        #endregion

        #region Public Methods

        public abstract int ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string query);

        public abstract void ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string procedure, object[] commandParameters);

        public abstract IDataReader ExecuteSql(string connectionString, CommandType commandType, int commandTimeout, string query);

        public abstract IDataReader ExecuteReader(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters);

        public abstract T ExecuteScalar<T>(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters);

        public abstract void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable);

        public abstract void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters);

        #endregion
    }
}

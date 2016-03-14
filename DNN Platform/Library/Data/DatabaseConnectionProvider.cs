#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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

using System;
using System.Linq;

#endregion

namespace DotNetNuke.Data
{
    using System.Data;

    using DotNetNuke.ComponentModel;

    public abstract class DatabaseConnectionProvider
    {
        #region Shared/Static Methods

        public static DatabaseConnectionProvider Instance()
        {
            return ComponentFactory.GetComponent<DatabaseConnectionProvider>();
        }

        #endregion

        #region Public Properties

        public virtual string Query { get; set; }

        public virtual int CommandTimeout { get; set; }

        public virtual string ConnectionString { get; set; }

        public virtual CommandType CommandType { get; set; }

        #endregion

        #region Public Methods

        public abstract void ExecuteCommand();

        public abstract int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText);

        public abstract void ExecuteNonQuery(string connectionString, CommandType commandType, string procedure, object[] commandParameters);

        public abstract IDataReader ExecuteReader(string connectionString, CommandType commandType, string procedureName, params object[] commandParameters);

        public abstract T ExecuteScalar<T>(string connectionString, CommandType commandType, string procedureName, params object[] commandParameters);

        #endregion
    }
}

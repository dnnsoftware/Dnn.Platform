// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public interface IDataContext : IDisposable
    {
        void BeginTransaction();

        void Commit();

        void Execute(CommandType type, string sql, params object[] args);

        IEnumerable<T> ExecuteQuery<T>(CommandType type, string sql, params object[] args);

        T ExecuteScalar<T>(CommandType type, string sql, params object[] args);

        T ExecuteSingleOrDefault<T>(CommandType type, string sql, params object[] args);

        IRepository<T> GetRepository<T>()
            where T : class;

        void RollbackTransaction();
    }
}

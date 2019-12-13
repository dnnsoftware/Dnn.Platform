using System;
using System.Collections.Generic;
using System.Data;

namespace DotNetNuke.Data
{
    public interface IDataContext : IDisposable
    {
        void BeginTransaction();
        void Commit();

        void Execute(CommandType type, string sql, params object[] args);
        IEnumerable<T> ExecuteQuery<T>(CommandType type, string sql, params object[] args);
        T ExecuteScalar<T>(CommandType type, string sql, params object[] args);
        T ExecuteSingleOrDefault<T>(CommandType type, string sql, params object[] args);

        IRepository<T> GetRepository<T>() where T : class;
        void RollbackTransaction();
    }
}

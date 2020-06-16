// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SqlConsole.Components
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    public class SqlQueryController : ServiceLocator<ISqlQueryController, SqlQueryController>, ISqlQueryController
    {
        public void AddQuery(SqlQuery query)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                rep.Insert(query);
            }
        }

        public void DeleteQuery(SqlQuery query)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                rep.Delete(query);
            }
        }

        public IEnumerable<SqlQuery> GetQueries()
        {
            IEnumerable<SqlQuery> queries;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                queries = rep.Get();
            }
            return queries;
        }

        public SqlQuery GetQuery(int id)
        {
            SqlQuery query;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                query = rep.GetById(id);
            }
            return query;
        }

        public SqlQuery GetQuery(string name)
        {
            List<SqlQuery> queries;
            SqlQuery query = null;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                queries = rep.Find("where name = @0", name).ToList();
                if (queries != null && queries.Count > 0)
                    query = queries.ElementAt(0);
            }
            return query;
        }

        public void UpdateQuery(SqlQuery query)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<SqlQuery>();
                rep.Update(query);
            }
        }

        public IList<string> GetConnections()
        {
            IList<string> connections = new List<string>();
            foreach (ConnectionStringSettings connection in ConfigurationManager.ConnectionStrings)
            {
                if (connection.Name.ToLowerInvariant() != "localmysqlserver" && connection.Name.ToLowerInvariant() != "localsqlserver")
                {
                    connections.Add(connection.Name);
                }
            }

            return connections;
        }

        protected override Func<ISqlQueryController> GetFactory()
        {
            return () => new SqlQueryController();
        }
    }
}

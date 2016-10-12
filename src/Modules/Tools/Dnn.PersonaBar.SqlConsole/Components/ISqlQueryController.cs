using System.Collections.Generic;

namespace Dnn.PersonaBar.SqlConsole.Components
{
    public interface ISqlQueryController
    {
        void AddQuery(SqlQuery query);
        void DeleteQuery(SqlQuery query);
        IEnumerable<SqlQuery> GetQueries();
        SqlQuery GetQuery(int id);
        SqlQuery GetQuery(string name);
        void UpdateQuery(SqlQuery query);
        IList<string> GetConnections();
    }
}
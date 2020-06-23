// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SqlConsole.Components
{
    using System.Collections.Generic;

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

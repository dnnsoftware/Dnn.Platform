// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

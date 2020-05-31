// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    public static class StoredProcedureStatsHelper
    {
        public static int GetStoredProcedureExecutionCount(string storedProcedureName)
        {
            var query = string.Format(
                @"SELECT ISNULL(SUM(execution_count),0)
                  FROM sys.dm_exec_procedure_stats
                  WHERE OBJECT_NAME(object_id, database_id) = '{{objectQualifier}}{0}'", storedProcedureName);
            return DatabaseHelper.ExecuteScalar<int>(query);
        }
    }
}

// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

namespace DotNetNuke.Tests.Integration.Framework.Helpers
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

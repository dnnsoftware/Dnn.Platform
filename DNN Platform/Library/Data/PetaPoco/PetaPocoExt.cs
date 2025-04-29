// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco;

using System;
using System.Data;

using global::PetaPoco;

public static class PetaPocoExt
{
    public static IDataReader ExecuteReader(this Database database, string sql, params object[] args)
    {
        IDataReader reader;
        try
        {
            database.OpenSharedConnection();
            sql = NormalizeSql(sql, args);

            using (IDbCommand command = database.CreateCommand(database.Connection, sql, args))
            {
                reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                database.OnExecutedCommand(command);
            }
        }
        catch (Exception exception)
        {
            if (database.OnException(exception))
            {
                throw;
            }

            reader = null;
        }

        return reader;
    }

    /// <summary>Escapes the "@" character if the arguments are empty and the sql string contains any "@" characters.</summary>
    /// <param name="sql">Sql string to normalize.</param>
    /// <param name="args">Sql command arguments.</param>
    /// <returns>Normalized sql string.</returns>
    private static string NormalizeSql(string sql, object[] args)
    {
        const string ReplaceFrom = "@";
        const string ReplaceTo = "@@";

        if (
            (args != null && args.Length != 0) ||
            string.IsNullOrWhiteSpace(sql))
        {
            return sql;
        }

        return sql.Replace(ReplaceFrom, ReplaceTo);
    }
}

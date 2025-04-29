// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.IO;
using System.Linq;

using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.FileHelpers;
using Cake.Frosting;

using Microsoft.Data.SqlClient;

/// <summary>A cake task to reset the local dev database.</summary>
public sealed class ResetDatabase : FrostingTask<Context>
{
    private const string ScriptsPath = @".\Build\Tasks\sql\";

    private static readonly string[] GoStatement = { "\r\nGO\r\n", "\nGO\n", "\nGO\r\n", "\r\nGO\n", };

    /// <inheritdoc/>
    public override void Run(Context context)
    {
        var script = ReplaceScriptVariables(context, LoadScript(context, "db-connections-drop"));
        ExecuteScript(context, script);
        script = ReplaceScriptVariables(context, LoadScript(context, "create-db"));
        ExecuteScript(context, script);
        if (context.Settings.DnnSqlUsername != string.Empty)
        {
            script = ReplaceScriptVariables(context, LoadScript(context, "add-db-user"));
            ExecuteScript(context, script);
        }
    }

    private static string LoadScript(ICakeContext context, string scriptName)
    {
        var script = scriptName + ".local.sql";
        if (!System.IO.File.Exists(ScriptsPath + script))
        {
            script = scriptName + ".sql";
        }

        return context.FileReadText(ScriptsPath + script);
    }

    private static string ReplaceScriptVariables(Context context, string script)
    {
        var dbPath = context.FileSystem.GetDirectory(context.Settings.DatabasePath);
        dbPath.Create();
        var fullDbPath = Path.GetFullPath(dbPath.Path.FullPath);

        return script.Replace("{DBName}", context.Settings.DnnDatabaseName)
            .Replace("{DBPath}", fullDbPath)
            .Replace("{DBLogin}", context.Settings.DnnSqlUsername);
    }

    private static bool ExecuteScript(Context context, string scriptStatement)
    {
        try
        {
            using (var connection = new SqlConnection(context.Settings.SaConnectionString))
            {
                connection.Open();
                foreach (var cmd in scriptStatement.Split(GoStatement, StringSplitOptions.RemoveEmptyEntries))
                {
                    var command = new SqlCommand(cmd, connection);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
        catch (Exception err)
        {
            context.Error(err);
            return false;
        }

        return true;
    }
}

// Database tasks for your local DNN development site

using System;
using System.Data.SqlClient;
using Cake.Common.Diagnostics;
using Cake.Frosting;
using Dnn.CakeUtils;

public sealed class ResetDatabase : FrostingTask<Context>
{
    public override void Run(Context context)
    {

        var script = ReplaceScriptVariables(context, LoadScript("db-connections-drop"));
        ExecuteScript(context, script);
        script = ReplaceScriptVariables(context, LoadScript("create-db"));
        ExecuteScript(context, script);
        if (context.Settings.DnnSqlUsername != "")
        {
            script = ReplaceScriptVariables(context, LoadScript("add-db-user"));
            ExecuteScript(context, script);
        }
    }

    private const string ScriptsPath = @".\Build\Cake\sql\";

    private static readonly string[] GoStatement = {"\r\nGO\r\n", "\nGO\n", "\nGO\r\n", "\r\nGO\n",};

    private string LoadScript(string scriptName)
    {
        var script = scriptName + ".local.sql";
        if (!System.IO.File.Exists(ScriptsPath + script))
        {
            script = scriptName + ".sql";
        }

        return Utilities.ReadFile(ScriptsPath + script);
    }

    private string ReplaceScriptVariables(Context context, string script)
    {
        return script
            .Replace("{DBName}", context.Settings.DnnDatabaseName)
            .Replace("{DBPath}", context.Settings.DatabasePath)
            .Replace("{DBLogin}", context.Settings.DnnSqlUsername);
    }

    private bool ExecuteScript(Context context, string scriptStatement)
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

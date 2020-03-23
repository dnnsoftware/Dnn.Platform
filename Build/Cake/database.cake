// Database tasks for your local DNN development site
using System.Data.SqlClient;

Task("ResetDatabase")
  .Does(() =>
    {
        var script = ReplaceScriptVariables(LoadScript("db-connections-drop"));
        ExecuteScript(script);
        script = ReplaceScriptVariables(LoadScript("create-db"));
        ExecuteScript(script);
        if (Settings.DnnSqlUsername != "") {
            script = ReplaceScriptVariables(LoadScript("add-db-user"));
            ExecuteScript(script);
        }
    });

public const string ScriptsPath = @".\Build\Cake\sql\";

private static readonly string[] GoStatement = {
    "\r\nGO\r\n",
    "\nGO\n",
    "\nGO\r\n",
    "\r\nGO\n",
};

public string LoadScript(string scriptName) {
    var script = scriptName + ".local.sql";
    if (!System.IO.File.Exists(ScriptsPath + script)) {
        script = scriptName + ".sql";
    }
    return Utilities.ReadFile(ScriptsPath + script);
}

public string ReplaceScriptVariables(string script) {
    return script
        .Replace("{DBName}", Settings.DnnDatabaseName)
        .Replace("{DBPath}", Settings.DatabasePath)
        .Replace("{DBLogin}", Settings.DnnSqlUsername);
}

public bool ExecuteScript(string scriptStatement)
{
    try
    {
        using (var connection = new SqlConnection(Settings.SaConnectionString))
        {
            connection.Open();
            foreach (var cmd in scriptStatement.Split(GoStatement, StringSplitOptions.RemoveEmptyEntries)) {
                var command = new SqlCommand(cmd, connection);
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
    catch (Exception err){
		Error(err);
        return false;
    }
    return true;
}

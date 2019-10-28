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

public bool ExecuteScript(string ScriptStatement)
{
    try
    {
        using (var connection = new System.Data.SqlClient.SqlConnection(Settings.SaConnectionString))
        {
            connection.Open();
            foreach (var cmd in ScriptStatement.Split(new string[] {"\r\nGO\r\n"}, StringSplitOptions.RemoveEmptyEntries)) {
                var command = new System.Data.SqlClient.SqlCommand(cmd, connection);
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
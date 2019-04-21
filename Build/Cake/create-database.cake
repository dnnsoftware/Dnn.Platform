
string connectionString = @"server=(localdb)\MSSQLLocalDB";

Task("CreateDatabase")
  .Does(() => 
    {
    var deleteScript = "if db_id('Dnn_Platform') is not null DROP DATABASE Dnn_Platform;";
    
    Information("Dropping LocalDb: {0}", ExecuteSqlScript(deleteScript));
    
    var createDbScript = string.Format(@"
        CREATE DATABASE
            [Dnn_Platform]
        ON PRIMARY (
           NAME=Dnn_data,
           FILENAME = '{0}\Dnn_Platform.mdf'
        )
        LOG ON (
            NAME=Dnn_log,
            FILENAME = '{0}\Dnn_Platform.ldf'
        )",
        tempDir
    );
	var createDbStatus = ExecuteSqlScript(createDbScript);
    Information("Created LocalDb: {0}", createDbStatus);

	if (createDbStatus) 
	{
		connectionString = @"server=(localdb)\MSSQLLocalDB;Database=Dnn_Platform;Trusted_Connection=True;";
    
		var schemaScriptName = XmlPeek("./Website/Install/DotNetNuke.install.config.resources", "/dotnetnuke/scripts/script[@name='Schema']");
		var dataScriptName = XmlPeek("./Website/Install/DotNetNuke.install.config.resources", "/dotnetnuke/scripts/script[@name='Data']");
		var schemaVersion = XmlPeek("./Website/Install/DotNetNuke.install.config.resources", "/dotnetnuke/version");

		//#####################################################################
		//run initial schema first
		//#####################################################################
		var fileContents = System.IO.File.ReadAllText("./Website/Providers/DataProviders/SqlDataProvider/" + schemaScriptName.ToString() + ".SqlDataProvider");

		var sqlDelimiterRegex = new System.Text.RegularExpressions.Regex("(?<=(?:[^\\w]+|^))GO(?=(?: |\\t)*?(?:\\r?\\n|$))", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
		string[] sqlStatements = sqlDelimiterRegex.Split(fileContents);
		foreach (string statement in sqlStatements)
		{
			ExecuteSqlScript(statement);
		}            
		Information("Initial Schema for v{0}", schemaVersion);
    
		//#####################################################################
		//populate with data next
		//#####################################################################
		fileContents = System.IO.File.ReadAllText("./Website/Providers/DataProviders/SqlDataProvider/" + dataScriptName.ToString() + ".SqlDataProvider");

		sqlStatements = sqlDelimiterRegex.Split(fileContents);
		foreach (string statement in sqlStatements)
		{
			Information("Test Data: {1}", schemaVersion, ExecuteSqlScript(statement));
		}

		var createDummyPortalStatement = "INSERT [dbo].[dnn_Portals] ([ExpiryDate], [UserRegistration], [BannerAdvertising], [AdministratorId], [Currency], [HostFee], [HostSpace], [AdministratorRoleId], [RegisteredRoleId], [GUID], [PaymentProcessor], [ProcessorUserId], [ProcessorPassword], [SiteLogHistory], [DefaultLanguage], [TimezoneOffset], [HomeDirectory], [PageQuota], [UserQuota], [CreatedByUserID], [CreatedOnDate], [LastModifiedByUserID], [LastModifiedOnDate], [PortalGroupID]) VALUES (NULL, 1, 0, 1, N'USD', 0.0000, 0, 0, 1, N'97debbc9-4643-4bd9-b0a0-b14170b38b0f', N'PayPal', NULL, NULL, 0, N'en-US', -8, N'Portals/0', 0, 0, -1, CAST(N'2015-02-05 14:49:37.873' AS DateTime), 1, CAST(N'2015-10-13 11:08:13.513' AS DateTime), -1)";

		Information("Test Portal: {1}", schemaVersion, ExecuteSqlScript(createDummyPortalStatement));

		//#####################################################################
		//now get all other SqlDataProvider files and run those....
		//#####################################################################
		var files = GetFiles("./Website/Providers/DataProviders/SqlDataProvider/*.SqlDataProvider");
    
		var currentFileToProcess = string.Empty;
    
			foreach(var file in files)
			{
			currentFileToProcess = file.GetFilenameWithoutExtension().ToString();
			var fileBits = currentFileToProcess.Split('.');
        
			int firstBit;
			int secondBit;
			int thirdBit;
        
			if (int.TryParse(fileBits[0], out firstBit) && int.TryParse(fileBits[1], out secondBit) && int.TryParse(fileBits[2], out thirdBit)) {
				var schemaVersionBits = schemaVersion.Split('.');
            
				int schemaFirstBit = int.Parse(schemaVersionBits[0]);
				int schemaSecondBit = int.Parse(schemaVersionBits[1]);
				int schemaThirdBit = int.Parse(schemaVersionBits[2]);
            
				if ((firstBit == schemaFirstBit && (secondBit >= schemaSecondBit && thirdBit >= schemaThirdBit)) || firstBit > schemaFirstBit){
					Information("Updated to v{0}", currentFileToProcess);
                
					fileContents = System.IO.File.ReadAllText(file.ToString());

					sqlStatements = sqlDelimiterRegex.Split(fileContents);
					foreach (string statement in sqlStatements)
					{
						var statementSuccess = ExecuteSqlScript(statement);
					}        
				}
                
			} 
		}
	
	}
	else {
		Information("An Error has occured. Please review and try again.");
	}

    
});  


public bool ExecuteSqlScript(string ScriptStatement)
{
    try
    {
        using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
        {
            connection.Open();

            var command = new System.Data.SqlClient.SqlCommand(ScriptStatement.Replace("{databaseOwner}", "dbo.").Replace("{objectQualifier}", "dnn_"), connection);
            command.ExecuteNonQuery();
            
            connection.Close();
        }
    }
    catch (Exception err){
		Error(err);

        return false;
    }
    
    return true;
}
// Tasks to help you create and maintain a local DNN development site.
// Note these tasks depend on the correct settings in your settings file.

Task("BuildToTempFolder")
    .IsDependentOn("SetVersion")
    .IsDependentOn("UpdateDnnManifests")
    .IsDependentOn("ResetDatabase")
    .IsDependentOn("PreparePackaging")
	.IsDependentOn("OtherPackages")
	.IsDependentOn("ExternalExtensions")
    .Does(() => 
    {
    });

Task("ResetDevSite")
	.IsDependentOn("BuildToTempFolder")
	.IsDependentOn("CopyToDevSite")
	.IsDependentOn("CopyWebConfigToDevSite")
    .Does(() => 
    {
    });

Task("CopyToDevSite")
    .Does(() => {
        CleanDirectory(Settings.WebsitePath);
        var files = GetFilesByPatterns(websiteFolder, new string[] {"**/*"}, packagingPatterns.installExclude);
        files.Add(GetFilesByPatterns(websiteFolder, packagingPatterns.installInclude));
        Information("Copying {0} files to {1}", files.Count, Settings.WebsitePath);
        CopyFiles(files, Settings.WebsitePath, true);
    });

Task("CopyWebConfigToDevSite")
    .Does(() => {
        var conf = Utilities.ReadFile("./Website/web.config");
        var transFile = "./Build/Cake/webconfig-transform.local.xsl";
        if (!FileExists(transFile)) transFile = "./Build/Cake/webconfig-transform.xsl";
        var trans = Utilities.ReadFile(transFile);
        trans = trans
            .Replace("{ConnectionString}", Settings.DnnConnectionString)
            .Replace("{DbOwner}", Settings.DbOwner)
            .Replace("{ObjectQualifier}", Settings.ObjectQualifier);
        var res = XmlTransform(trans, conf);
        var webConfig = File(System.IO.Path.Combine(Settings.WebsitePath, "web.config"));
        FileWriteText(webConfig, res);
    });
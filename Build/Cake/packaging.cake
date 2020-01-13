// The tasks create the various DNN release packages (Install, Upgrade, Deploy and Symbols)

using Dnn.CakeUtils;

public class PackagingPatterns {
    public string[] installExclude {get; set;}
    public string[] installInclude {get; set;}
    public string[] upgradeExclude {get; set;}
    public string[] symbolsInclude {get; set;}
    public string[] symbolsExclude {get; set;}
}

PackagingPatterns packagingPatterns;

var sqlDataProviderExists = false;

Task("PreparePackaging")
	.IsDependentOn("CopyWebsite")
	.IsDependentOn("Build")
	.IsDependentOn("CopyWebConfig")
	.IsDependentOn("CopyWebsiteBinFolder")
    .Does(() =>
	{
        packagingPatterns = Newtonsoft.Json.JsonConvert.DeserializeObject<PackagingPatterns>(Utilities.ReadFile("./Build/Cake/packaging.json"));
		// Various fixes
		CopyFile("./DNN Platform/Components/DataAccessBlock/bin/Microsoft.ApplicationBlocks.Data.dll", websiteFolder + "bin/Microsoft.ApplicationBlocks.Data.dll");
		CopyFiles("./DNN Platform/Components/Lucene.Net.Contrib/bin/Lucene.Net.Contrib.Analyzers.*", websiteFolder + "bin/");
		CopyFile("./DNN Platform/Library/bin/PetaPoco.dll", websiteFolder + "bin/PetaPoco.dll");
	});

Task("CopyWebsite")
	.IsDependentOn("CleanWebsite")
    .IsDependentOn("GenerateSqlDataProvider")
	.Does(() =>
	{
		CopyFiles(GetFiles("./DNN Platform/Website/**/*"), websiteFolder, true);
	});

Task("CopyWebsiteBinFolder")
    .Does(() =>
	{
		CopyFiles(GetFiles("./DNN Platform/Website/bin/**/*"), websiteFolder + "bin/", true);
	});

Task("CopyWebConfig")
    .Does(() =>
	{
		CopyFile(websiteFolder + "release.config", websiteFolder + "web.config");
	});

Task("CreateInstall")
	.IsDependentOn("PreparePackaging")
	.IsDependentOn("OtherPackages")
	.IsDependentOn("ExternalExtensions")
	.Does(() =>
	{
        CreateDirectory(artifactsFolder);
        var files = GetFilesByPatterns(websiteFolder, new string[] {"**/*"}, packagingPatterns.installExclude);
        files.Add(GetFilesByPatterns(websiteFolder, packagingPatterns.installInclude));
        Information("Zipping {0} files for Install zip", files.Count);
        var packageZip = string.Format(artifactsFolder + "DNN_Platform_{0}_Install.zip", GetBuildNumber());
        Zip(websiteFolder, packageZip, files);
	});

Task("CreateUpgrade")
	.IsDependentOn("PreparePackaging")
	.IsDependentOn("OtherPackages")
	.IsDependentOn("ExternalExtensions")
	.Does(() =>
	{
        CreateDirectory(artifactsFolder);
		var excludes = new string[packagingPatterns.installExclude.Length + packagingPatterns.upgradeExclude.Length];
		packagingPatterns.installExclude.CopyTo(excludes, 0);
		packagingPatterns.upgradeExclude.CopyTo(excludes, packagingPatterns.installExclude.Length);
        var files = GetFilesByPatterns(websiteFolder, new string[] {"**/*"}, excludes);
		files.Add(GetFiles("./Website/Install/Module/DNNCE_Website.Deprecated_*_Install.zip"));
        Information("Zipping {0} files for Upgrade zip", files.Count);
        var packageZip = string.Format(artifactsFolder + "DNN_Platform_{0}_Upgrade.zip", GetBuildNumber());
        Zip(websiteFolder, packageZip, files);
	});
    
Task("CreateDeploy")
	.IsDependentOn("PreparePackaging")
	.IsDependentOn("OtherPackages")
	.IsDependentOn("ExternalExtensions")
	.Does(() =>
	{
        CreateDirectory(artifactsFolder);
        var packageZip = string.Format(artifactsFolder + "DNN_Platform_{0}_Deploy.zip", GetBuildNumber());
		var deployFolder = "./DotNetNuke/";
		var deployDir = Directory(deployFolder);
		System.IO.Directory.Move(websiteDir.Path.FullPath, deployDir.Path.FullPath);
        var files = GetFilesByPatterns(deployFolder, new string[] {"**/*"}, packagingPatterns.installExclude);
        files.Add(GetFilesByPatterns(deployFolder, packagingPatterns.installInclude));
        Zip("", packageZip, files);
    	Dnn.CakeUtils.Compression.AddFilesToZip(packageZip, "./Build/Deploy", GetFiles("./Build/Deploy/*"), true);
		System.IO.Directory.Move(deployDir.Path.FullPath, websiteDir.Path.FullPath);
	});

Task("CreateSymbols")
	.IsDependentOn("PreparePackaging")
	.IsDependentOn("OtherPackages")
	.IsDependentOn("ExternalExtensions")
	.Does(() =>
	{
        CreateDirectory(artifactsFolder);
        var packageZip = string.Format(artifactsFolder + "DNN_Platform_{0}_Symbols.zip", GetBuildNumber());
        Zip("./Build/Symbols/", packageZip, GetFiles("./Build/Symbols/*"));
		// Fix for WebUtility symbols missing from bin folder
		CopyFiles(GetFiles("./DNN Platform/DotNetNuke.WebUtility/bin/DotNetNuke.WebUtility.*"), websiteFolder + "bin/");
        var files = GetFilesByPatterns(websiteFolder, packagingPatterns.symbolsInclude, packagingPatterns.symbolsExclude);
		var resFile = Dnn.CakeUtils.Compression.ZipToBytes(websiteFolder.TrimEnd('/'), files);
		Dnn.CakeUtils.Compression.AddBinaryFileToZip(packageZip, resFile, "Resources.zip", true);
	});

Task("GenerateSqlDataProvider")
	.IsDependentOn("SetVersion")
	.Does(() => {
		var fileName = GetTwoDigitsVersionNumber().Substring(0,8) + ".SqlDataProvider";
		var filePath = "./Dnn Platform/Website/Providers/DataProviders/SqlDataProvider/" + fileName;
		if (System.IO.File.Exists(filePath))
		{
			sqlDataProviderExists = true;
			return;
		}
		sqlDataProviderExists = false;
		
		using (System.IO.StreamWriter file = 
            new System.IO.StreamWriter(filePath, true))
        {
			file.WriteLine("/************************************************************/");
			file.WriteLine("/*****              SqlDataProvider                     *****/");
			file.WriteLine("/*****                                                  *****/");
			file.WriteLine("/*****                                                  *****/");
			file.WriteLine("/***** Note: To manually execute this script you must   *****/");
			file.WriteLine("/*****       perform a search and replace operation     *****/");
			file.WriteLine("/*****       for {databaseOwner} and {objectQualifier}  *****/");
			file.WriteLine("/*****                                                  *****/");
			file.WriteLine("/************************************************************/");
        }
	});

private void RevertSqlDataProvider(){
	var fileName = GetTwoDigitsVersionNumber() + ".SqlDataProvider";
	var filePath = "./Dnn Platform/Website/Providers/DataProviders/SqlDataProvider/" + fileName;
	if (!sqlDataProviderExists && System.IO.File.Exists(filePath))
	{
		System.IO.File.Delete(filePath);
	}
}



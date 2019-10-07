using Dnn.CakeUtils;

public class PackagingPatterns {
    public string[] installExclude {get; set;}
    public string[] installInclude {get; set;}
    public string[] upgradeExclude {get; set;}
}

PackagingPatterns packagingPatterns;

Task("PreparePackaging")
	.IsDependentOn("CopyWebsite")
	.IsDependentOn("CompileSource")
	.IsDependentOn("CopyWebConfig")
    .Does(() =>
	{
        packagingPatterns = Newtonsoft.Json.JsonConvert.DeserializeObject<PackagingPatterns>(Utilities.ReadFile("./Build/Cake/packaging.json"));
	});

Task("CopyWebsite")
	.IsDependentOn("CleanWebsite")
    .Does(() =>
	{
		CopyFiles(GetFiles("./DNN Platform/Website/**/*"), websiteFolder, true);
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
        Information(files.Count);
        var packageZip = string.Format(artifactsFolder + "DNN_Platform_{0}_Install.zip", GetProductVersion());
        Zip(websiteFolder, packageZip, files);
	});

Task("CreateUpgrade")
	.IsDependentOn("PreparePackaging")
	.IsDependentOn("OtherPackages")
	.Does(() =>
	{
	});
    
Task("CreateSymbols")
	.IsDependentOn("PreparePackaging")
	.IsDependentOn("OtherPackages")
	.Does(() =>
	{
	});

Task("CreateDeploy")
	.IsDependentOn("PreparePackaging")
	.IsDependentOn("OtherPackages")
	.Does(() =>
	{
	});

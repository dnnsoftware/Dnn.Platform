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
        Information("Zipping {0} files for Install zip", files.Count);
        var packageZip = string.Format(artifactsFolder + "DNN_Platform_{0}_Install.zip", GetProductVersion());
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
        var packageZip = string.Format(artifactsFolder + "DNN_Platform_{0}_Upgrade.zip", GetProductVersion());
        Zip(websiteFolder, packageZip, files);
	});
    
Task("CreateDeploy")
	.IsDependentOn("PreparePackaging")
	.IsDependentOn("OtherPackages")
	.IsDependentOn("ExternalExtensions")
	.Does(() =>
	{
        CreateDirectory(artifactsFolder);
        var packageZip = string.Format(artifactsFolder + "DNN_Platform_{0}_Deploy.zip", GetProductVersion());
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
	.Does(() =>
	{
	});

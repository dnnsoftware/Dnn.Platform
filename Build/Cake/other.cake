// Packaging of 3rd party stuff that is in our repository

using System.Collections.Generic;

public class OtherPackage {
    public string name {get; set;}
    public string folder {get; set;}
    public string destination {get; set;}
    public string version {get; set;}
    public string extension {get; set;} = "zip";
    public string[] excludes {get;set;} = new string[] {};
}

Task("OtherPackages")
    .IsDependentOn("UpdateDnnManifests")
    .IsDependentOn("Newtonsoft")
    .Does(() =>
	{
        List<OtherPackage> otherPackages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OtherPackage>>(Utilities.ReadFile("./Build/Cake/other.json"));
        foreach (var op in otherPackages) {
            PackageOtherPackage(op);
        }
	});

Task("Newtonsoft")
    .Does(() =>
	{
    	var packageZip = string.Format("{0}Install/Module/Newtonsoft.Json_10.00.03_Install.zip", websiteFolder);
    	Dnn.CakeUtils.Compression.AddFilesToZip(packageZip, "DNN Platform/Components/Newtonsoft", GetFiles("DNN Platform/Components/Newtonsoft/*"), false);
    	Dnn.CakeUtils.Compression.AddFilesToZip(packageZip, "Website", GetFiles("Website/bin/Newtonsoft.Json.dll"), true);
	});

private void PackageOtherPackage(OtherPackage package) {
    var srcFolder = "./" + package.folder;
    var files = package.excludes.Length == 0 ?
        GetFiles(srcFolder + "**/*") :
        GetFilesByPatterns(srcFolder, new string[] {"**/*"}, package.excludes);
    var version = package.version;
    if (version == "auto") {
        version = GetProductVersion();
    }
    CreateDirectory(package.destination);
	var packageZip = string.Format("{0}{1}/{2}_{3}_Install.{4}", websiteFolder, package.destination, package.name, version, package.extension);
    Information("Packaging {0}", packageZip);
	Zip(srcFolder, packageZip, files);
}


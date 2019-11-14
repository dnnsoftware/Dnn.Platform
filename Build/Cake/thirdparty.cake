// Packaging of 3rd party stuff that is in our repository

using System.Collections.Generic;

public class OtherPackage {
    public string name {get; set;}
    public string folder {get; set;}
    public string destination {get; set;}
    public string extension {get; set;} = "zip";
    public string[] excludes {get;set;} = new string[] {};
}

Task("OtherPackages")
    .IsDependentOn("UpdateDnnManifests")
    .IsDependentOn("Newtonsoft")
    .Does(() =>
	{
        List<OtherPackage> otherPackages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OtherPackage>>(Utilities.ReadFile("./Build/Cake/thirdparty.json"));
        foreach (var op in otherPackages) {
            PackageOtherPackage(op);
        }
	});

Task("Newtonsoft")
    .Does(() =>
	{
        var version = "00.00.00";
        foreach (var assy in GetFiles(websiteFolder + "bin/Newtonsoft.Json.dll")) {
            version = System.Diagnostics.FileVersionInfo.GetVersionInfo(assy.FullPath).FileVersion;
        }
    	var packageZip = string.Format("{0}Install/Module/Newtonsoft.Json_{1}_Install.zip", websiteFolder, version);
        Zip("./DNN Platform/Components/Newtonsoft", packageZip, GetFiles("./DNN Platform/Components/Newtonsoft/*"));
    	Dnn.CakeUtils.Compression.AddFilesToZip(packageZip, "Website", GetFiles(websiteFolder + "bin/Newtonsoft.Json.dll"), true);
	});

private void PackageOtherPackage(OtherPackage package) {
    var srcFolder = "./" + package.folder;
    var files = package.excludes.Length == 0 ?
        GetFiles(srcFolder + "**/*") :
        GetFilesByPatterns(srcFolder, new string[] {"**/*"}, package.excludes);
    var version = "00.00.00";
    foreach (var dnn in GetFiles(srcFolder + "**/*.dnn")) {
        version = XmlPeek(dnn, "dotnetnuke/packages/package/@version");
    }
    CreateDirectory(package.destination);
	var packageZip = string.Format("{0}{1}/{2}_{3}_Install.{4}", websiteFolder, package.destination, package.name, version, package.extension);
    Information("Packaging {0}", packageZip);
	Zip(srcFolder, packageZip, files);
}


// Packaging of 3rd party stuff that is in our repository

using System.Collections.Generic;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Xml;
using Cake.Frosting;
using Dnn.CakeUtils;
using Newtonsoft.Json;

public class OtherPackage {
    public string name {get; set;}
    public string folder {get; set;}
    public string destination {get; set;}
    public string extension {get; set;} = "zip";
    public string[] excludes {get;set;} = new string[] {};
}

[Dependency(typeof(PackageNewtonsoft))]
public sealed class OtherPackages : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        List<OtherPackage> otherPackages =
            JsonConvert.DeserializeObject<List<OtherPackage>>(
                Utilities.ReadFile("./Build/Cake/thirdparty.json"));
        foreach (var op in otherPackages)
        {
            PackageOtherPackage(context, op);
        }
    }

    private void PackageOtherPackage(Context context, OtherPackage package) {
        var srcFolder = "./" + package.folder;
        var files = package.excludes.Length == 0 ?
            context.GetFiles(srcFolder + "**/*") :
            context.GetFilesByPatterns(srcFolder, new string[] {"**/*"}, package.excludes);
        var version = "00.00.00";
        foreach (var dnn in context.GetFiles(srcFolder + "**/*.dnn")) {
            version = context.XmlPeek(dnn, "dotnetnuke/packages/package/@version");
        }
        context.CreateDirectory(package.destination);
        var packageZip =
            $"{context.websiteFolder}{package.destination}/{package.name}_{version}_Install.{package.extension}";
        context.Information("Packaging {0}", packageZip);
        context.Zip(srcFolder, packageZip, files);
    }
}

public sealed class PackageNewtonsoft : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var version = "00.00.00";
        foreach (var assy in context.GetFiles(context.websiteFolder + "bin/Newtonsoft.Json.dll"))
        {
            version = System.Diagnostics.FileVersionInfo.GetVersionInfo(assy.FullPath).FileVersion;
        }

        var packageZip = $"{context.websiteFolder}Install/Module/Newtonsoft.Json_{version}_Install.zip";
        context.Zip("./DNN Platform/Components/Newtonsoft", packageZip, context.GetFiles("./DNN Platform/Components/Newtonsoft/*"));
        Dnn.CakeUtils.Compression.AddFilesToZip(packageZip, "Website",
            context.GetFiles(context.websiteFolder + "bin/Newtonsoft.Json.dll"), true);
    }
}

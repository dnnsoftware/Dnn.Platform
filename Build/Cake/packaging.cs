// The tasks create the various DNN release packages (Install, Upgrade, Deploy and Symbols)

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Frosting;
using Dnn.CakeUtils;
using Newtonsoft.Json;

public class PackagingPatterns
{
    public string[] installExclude { get; set; }
    public string[] installInclude { get; set; }
    public string[] upgradeExclude { get; set; }
    public string[] symbolsInclude { get; set; }
    public string[] symbolsExclude { get; set; }
}


[Dependency(typeof(CopyWebsite))]
[Dependency(typeof(Build))]
[Dependency(typeof(CopyWebConfig))]
[Dependency(typeof(CopyWebsiteBinFolder))]
public sealed class PreparePackaging : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.packagingPatterns =
            JsonConvert.DeserializeObject<PackagingPatterns>(
                Utilities.ReadFile("./Build/Cake/packaging.json"));
        // Various fixes
        context.CopyFile("./DNN Platform/Components/DataAccessBlock/bin/Microsoft.ApplicationBlocks.Data.dll",
            context.websiteFolder + "bin/Microsoft.ApplicationBlocks.Data.dll");
        context.CopyFiles("./DNN Platform/Components/Lucene.Net.Contrib/bin/Lucene.Net.Contrib.Analyzers.*",
            context.websiteFolder + "bin/");
        context.CopyFile("./DNN Platform/Library/bin/PetaPoco.dll", context.websiteFolder + "bin/PetaPoco.dll");
    }
}

[Dependency(typeof(CleanWebsite))]
[Dependency(typeof(GenerateSqlDataProvider))]
public sealed class CopyWebsite : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CopyFiles(context.GetFiles("./DNN Platform/Website/**/*"), context.websiteFolder, true);
    }
}

public sealed class CopyWebsiteBinFolder : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CopyFiles(context.GetFiles("./DNN Platform/Website/bin/**/*"), context.websiteFolder + "bin/", true);
    }
}

public sealed class CopyWebConfig : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CopyFile(context.websiteFolder + "release.config", context.websiteFolder + "web.config");
    }
}

[Dependency(typeof(PreparePackaging))]
[Dependency(typeof(OtherPackages))]
public sealed class CreateInstall : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CreateDirectory(context.artifactsFolder);
        var files = context.GetFilesByPatterns(context.websiteFolder, new string[] { "**/*" }, context.packagingPatterns.installExclude);
        files.Add(context.GetFilesByPatterns(context.websiteFolder, context.packagingPatterns.installInclude));
        context.Information("Zipping {0} files for Install zip", files.Count);
        var packageZip = string.Format(context.artifactsFolder + "DNN_Platform_{0}_Install.zip", context.GetBuildNumber());
        context.Zip(context.websiteFolder, packageZip, files);
    }
}

[Dependency(typeof(PreparePackaging))]
[Dependency(typeof(OtherPackages))]
public sealed class CreateUpgrade : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CreateDirectory(context.artifactsFolder);
        var excludes = new string[context.packagingPatterns.installExclude.Length + context.packagingPatterns.upgradeExclude.Length];
        context.packagingPatterns.installExclude.CopyTo(excludes, 0);
        context.packagingPatterns.upgradeExclude.CopyTo(excludes, context.packagingPatterns.installExclude.Length);
        var files = context.GetFilesByPatterns(context.websiteFolder, new string[] { "**/*" }, excludes);
        files.Add(context.GetFiles("./Website/Install/Module/DNNCE_Website.Deprecated_*_Install.zip"));
        context.Information("Zipping {0} files for Upgrade zip", files.Count);
        var packageZip = string.Format(context.artifactsFolder + "DNN_Platform_{0}_Upgrade.zip", context.GetBuildNumber());
        context.Zip(context.websiteFolder, packageZip, files);
    }
}

[Dependency(typeof(PreparePackaging))]
[Dependency(typeof(OtherPackages))]
public sealed class CreateDeploy : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CreateDirectory(context.artifactsFolder);
        var packageZip = string.Format(context.artifactsFolder + "DNN_Platform_{0}_Deploy.zip", context.GetBuildNumber());
        var deployFolder = "./DotNetNuke/";
        var deployDir = context.Directory(deployFolder);
        System.IO.Directory.Move(context.websiteDir.Path.FullPath, deployDir.Path.FullPath);
        var files = context.GetFilesByPatterns(deployFolder, new string[] { "**/*" }, context.packagingPatterns.installExclude);
        files.Add(context.GetFilesByPatterns(deployFolder, context.packagingPatterns.installInclude));
        context.Zip("", packageZip, files);
        Dnn.CakeUtils.Compression.AddFilesToZip(packageZip, "./Build/Deploy", context.GetFiles("./Build/Deploy/*"), true);
        System.IO.Directory.Move(deployDir.Path.FullPath, context.websiteDir.Path.FullPath);
    }
}

[Dependency(typeof(PreparePackaging))]
[Dependency(typeof(OtherPackages))]
public sealed class CreateSymbols : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CreateDirectory(context.artifactsFolder);
        var packageZip = string.Format(context.artifactsFolder + "DNN_Platform_{0}_Symbols.zip", context.GetBuildNumber());
        context.Zip("./Build/Symbols/", packageZip, context.GetFiles("./Build/Symbols/*"));
        // Fix for WebUtility symbols missing from bin folder
        context.CopyFiles(context.GetFiles("./DNN Platform/DotNetNuke.WebUtility/bin/DotNetNuke.WebUtility.*"),
            context.websiteFolder + "bin/");
        var files = context.GetFilesByPatterns(context.websiteFolder, context.packagingPatterns.symbolsInclude,
            context.packagingPatterns.symbolsExclude);
        var resFile = Dnn.CakeUtils.Compression.ZipToBytes(context.websiteFolder.TrimEnd('/'), files);
        Dnn.CakeUtils.Compression.AddBinaryFileToZip(packageZip, resFile, "Resources.zip", true);
    }
}

[Dependency(typeof(SetVersion))]
public sealed class GenerateSqlDataProvider : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var fileName = context.GetTwoDigitsVersionNumber().Substring(0, 8) + ".SqlDataProvider";
        var filePath = "./Dnn Platform/Website/Providers/DataProviders/SqlDataProvider/" + fileName;
        if (System.IO.File.Exists(filePath))
        {
            context.sqlDataProviderExists = true;
            return;
        }

        context.sqlDataProviderExists = false;

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
    }
}

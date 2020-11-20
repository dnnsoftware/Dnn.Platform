// Tasks to help you create and maintain a local DNN development site.
// Note these tasks depend on the correct settings in your settings file.

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Xml;
using Cake.FileHelpers;
using Cake.Frosting;
using Dnn.CakeUtils;

[Dependency(typeof(SetVersion))]
[Dependency(typeof(UpdateDnnManifests))]
[Dependency(typeof(ResetDatabase))]
[Dependency(typeof(PreparePackaging))]
[Dependency(typeof(OtherPackages))]
public sealed class BuildToTempFolder : FrostingTask<Context>
{
}

[Dependency(typeof(BuildToTempFolder))]
[Dependency(typeof(CopyToDevSite))]
[Dependency(typeof(CopyWebConfigToDevSite))]
public sealed class ResetDevSite : FrostingTask<Context>
{
}

public sealed class CopyToDevSite : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CleanDirectory(context.Settings.WebsitePath);
        var files = context.GetFilesByPatterns(context.websiteFolder, new string[] {"**/*"},
            context.packagingPatterns.installExclude);
        files.Add(context.GetFilesByPatterns(context.websiteFolder, context.packagingPatterns.installInclude));
        context.Information("Copying {0} files to {1}", files.Count, context.Settings.WebsitePath);
        context.CopyFiles(files, context.Settings.WebsitePath, true);

    }
}

public sealed class CopyWebConfigToDevSite : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var conf = Utilities.ReadFile("./Website/web.config");
        var transFile = "./Build/Cake/webconfig-transform.local.xsl";
        if (!context.FileExists(transFile)) transFile = "./Build/Cake/webconfig-transform.xsl";
        var trans = Utilities.ReadFile(transFile);
        trans = trans
            .Replace("{ConnectionString}", context.Settings.DnnConnectionString)
            .Replace("{DbOwner}", context.Settings.DbOwner)
            .Replace("{ObjectQualifier}", context.Settings.ObjectQualifier);
        var res = context.XmlTransform(trans, conf);
        var webConfig = context.File(System.IO.Path.Combine(context.Settings.WebsitePath, "web.config"));
        context.FileWriteText(webConfig, res);

    }
}

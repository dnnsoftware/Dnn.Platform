// These tasks will set the right version for manifests and assemblies. Note you can
// control this by using custom settings

using System;
using System.IO;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.GitVersion;
using Cake.FileHelpers;
using Cake.Frosting;
using Cake.XdtTransform;
using Dnn.CakeUtils;
using Newtonsoft.Json;

[Dependency(typeof(SetVersion))]
public sealed class BuildServerSetVersion : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        Console.WriteLine($"##vso[build.updatebuildnumber]{context.version.FullSemVer}.{context.buildId}");
    }
}

public sealed class SetVersion : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        if (context.Settings.Version == "auto")
        {
            context.version = context.GitVersion();
            context.buildNumber = context.version.LegacySemVerPadded;
        }
        else
        {
            context.version = new GitVersion();
            var assemblyInfo = new AssemblyInfo("SolutionInfo.cs");
            var requestedVersion = context.Settings.Version == "off"
                ? assemblyInfo.GetVersion()
                : new System.Version(context.Settings.Version);
            context.version.Major = requestedVersion.Major;
            context.version.Minor = requestedVersion.Minor;
            context.version.Patch = requestedVersion.Build;
            context.version.InformationalVersion = requestedVersion.ToString(3) + " Custom build";
            context.version.MajorMinorPatch = requestedVersion.ToString(3);
            context.version.FullSemVer = requestedVersion.ToString(3);
            if (requestedVersion.Revision != -1)
            {
                context.version.CommitsSinceVersionSource = requestedVersion.Revision;
                context.version.InformationalVersion = requestedVersion.ToString(4) + " Custom build";
            }

            context.buildNumber = requestedVersion.ToString(3);
        }

        context.Information(JsonConvert.SerializeObject(context.version));
        if (context.Settings.Version != "off")
        {
            Dnn.CakeUtils.Utilities.UpdateAssemblyInfoVersion(
                new System.Version(context.version.Major, context.version.Minor, context.version.Patch,
                    context.version.CommitsSinceVersionSource ?? 0),
                context.version.InformationalVersion, "SolutionInfo.cs");
        }

        context.Information("Informational Version : " + context.version.InformationalVersion);
        context.productVersion = context.version.MajorMinorPatch;
        context.Information("Product Version : " + context.productVersion);
        context.Information("Build Number : " + context.buildNumber);
        context.Information("The build Id is : " + context.buildId);
    }
}

[Dependency(typeof(SetVersion))]
[Dependency(typeof(SetPackageVersions))]
public sealed class UpdateDnnManifests : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        foreach (var file in context.GetFilesByPatterns(".", new string[] {"**/*.dnn"}, context.unversionedManifests))
        {
            if (context.Settings.Version == "off") return;
            context.Information("Transforming: " + file);
            var transformFile = context.File(Path.GetTempFileName());
            context.FileAppendText(transformFile, GetXdtTransformation(context));
            context.XdtTransformConfig(file, transformFile, file);
        }
    }

    public static string GetVersionString(Context context)
    {
        return $"{context.version.Major:00}.{context.version.Minor:00}.{context.version.Patch:00}";
    }

    public string GetXdtTransformation(Context context)
    {
        return $@"<?xml version=""1.0""?>
<dotnetnuke xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
  <packages>
    <package version=""{GetVersionString(context)}"" 
             xdt:Transform=""SetAttributes(version)"" />
  </packages>
</dotnetnuke>";
    }
}

[Dependency(typeof(SetVersion))]
public sealed class SetPackageVersions : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        if (context.Settings.Version == "off") return;
        var packages = context.GetFiles("./Dnn.AdminExperience/ClientSide/*.Web/package.json");
        packages.Add(context.GetFiles("./Dnn.AdminExperience/ClientSide/Dnn.React.Common/package.json"));
        packages.Add(context.GetFiles("./Dnn.AdminExperience/ClientSide/*.Web/**/_exportables/package.json"));
        packages.Add(context.GetFiles("./DNN Platform/Modules/ResourceManager/ResourceManager.Web/package.json"));

        // Set all package.json in Admin Experience to the current version and to consume the current (local) version of dnn-react-common.
        foreach (var file in packages)
        {
            context.Information($"Updating {file.ToString()} to version {context.version.FullSemVer}");
            context.ReplaceRegexInFiles(file.ToString(), @"""version"": "".*""", $@"""version"": ""{context.version.FullSemVer}""");
            context.ReplaceRegexInFiles(file.ToString(), @"""@dnnsoftware\/dnn-react-common"": "".*""",
                $@"""@dnnsoftware/dnn-react-common"": ""{context.version.FullSemVer}""");
        }
    }
}

[Dependency(typeof(SetVersion))]
public sealed class GenerateChecksum : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.Information("Generating default.aspx checksum...");
        var sourceFile = "./Dnn Platform/Website/Default.aspx";
        var destFile = "./Dnn.AdminExperience/Dnn.PersonaBar.Extensions/Components/Security/Resources/sums.resources";
        var hash = CalculateSha(sourceFile);
        var content = $@"<checksums>
  <sum name=""Default.aspx"" version=""{context.version.MajorMinorPatch}"" type=""Platform"" sum=""{hash}"" />
</checksums>";
        System.IO.File.WriteAllText(destFile, content);
    }

    private static string CalculateSha(string filename)
    {
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            using (var stream = System.IO.File.OpenRead(filename))
            {
                var hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public string GetXdtTransformation(Context context)
    {
        return $@"<?xml version=""1.0""?>
<dotnetnuke xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
  <packages>
    <package version=""{UpdateDnnManifests.GetVersionString(context)}"" 
             xdt:Transform=""SetAttributes(version)"" />
  </packages>
</dotnetnuke>";
    }
}

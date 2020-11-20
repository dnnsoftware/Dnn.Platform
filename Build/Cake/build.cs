using System;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Core.IO;
using Cake.FileHelpers;
using Cake.Frosting;
using Dnn.CakeUtils;
using Newtonsoft.Json;

public class Context : FrostingContext
{
    public Context(ICakeContext context)
        : base(context)
    { 
        try
        {
            //////////////////////////////////////////////////////////////////////
            // ARGUMENTS
            //////////////////////////////////////////////////////////////////////

            this.target = context.Argument("target", "Default");
            context.Information($"Target: {target}");
            this.configuration = context.Argument("configuration", "Release");
            context.Information($"Configuration: {configuration}");

            //////////////////////////////////////////////////////////////////////
            // PREPARATION
            //////////////////////////////////////////////////////////////////////

            // Define directories.
            this.tempFolder = "./Temp/";
            this.tempDir = context.Directory(tempFolder);
            context.Information($"TempDir: {tempDir}");

            this.artifactsFolder = "./Artifacts/";
            this.artifactsDir = context.Directory(this.artifactsFolder);
            context.Information($"ArtifactsDir: {artifactsDir}");
            
            this.websiteFolder = "./Website/";
            this.websiteDir = context.Directory(this.websiteFolder);
            context.Information($"WebsiteDir: {websiteDir}");

            // Global information variables
            this.isRunningInCI = false;

            this.dnnSolutionPath = "./DNN_Platform.sln";
            this.connectionString = @"server=(localdb)\MSSQLLocalDB";
            
            this.sqlDataProviderExists = false;

            var settingsFile = "settings.local.json";
            this.Settings = this.LoadSettings(settingsFile);
            WriteSettings(settingsFile);

            this.buildId = context.EnvironmentVariable("BUILD_BUILDID") ?? "0";
            context.Information($"BuildId: {buildId}");
            this.buildNumber = "";
            this.productVersion = "";

            this.unversionedManifests = context.FileReadLines("./Cake/unversionedManifests.txt");
        }
        catch (Exception exc)
        {
            this.Error(exc);
            throw;
        }
    }

    public string[] unversionedManifests { get; set; }

    public string productVersion { get; set; }

    public string buildNumber { get; set; }

    public string buildId { get; set; }

    public bool sqlDataProviderExists { get; set; }

    public string connectionString { get; set; }

    public string dnnSolutionPath { get; set; }

    public bool isRunningInCI { get; set; }

    public ConvertableDirectoryPath websiteDir { get; set; }

    public string websiteFolder { get; set; }

    public ConvertableDirectoryPath artifactsDir { get; set; }

    public string artifactsFolder { get; set; }

    public ConvertableDirectoryPath tempDir { get; set; }

    public string tempFolder { get; set; }

    public string configuration { get; set; }

    public string target { get; set; }

    private void WriteSettings(string settingsFile)
    {
        using (var sw = new System.IO.StreamWriter(settingsFile))
        {
            sw.WriteLine(JsonConvert.SerializeObject(Settings, Formatting.Indented));
        }
    }

    public PackagingPatterns packagingPatterns { get; set; }

    public LocalSettings Settings { get; set; }
    public GitVersion version { get; set; }

    
    public string GetBuildNumber()
    {
        return buildNumber;
    }

    public string GetTwoDigitsVersionNumber(){
        var fullVer = GetBuildNumber().Split('-')[0]; // Gets rid of the -unstable, -beta, etc.
        var numbers = fullVer.Split('.');
        for (int i=0; i < numbers.Length; i++)
        {
            if (numbers[i].Length < 2)
            {
                numbers[i] = "0" + numbers[i];
            }
        }
        return String.Join(".", numbers);
    }

    public string GetProductVersion()
    {
        return productVersion;
    }

    private LocalSettings LoadSettings(string settingsFile) {
        if (System.IO.File.Exists(settingsFile)) {
            return JsonConvert.DeserializeObject<LocalSettings>(Utilities.ReadFile(settingsFile));
        } else {
            return new LocalSettings();
        }
    }
}

//////////////////////////////////////////////////////////////////////
// SETUP/TEARDOWN
//////////////////////////////////////////////////////////////////////

public sealed class Lifetime : FrostingLifetime<Context>
{
    public override void Setup(Context context)
    {
        context.isRunningInCI = context.HasEnvironmentVariable("TF_BUILD");
        context.Information("Is Running in CI : {0}", context.isRunningInCI);
        if (context.Settings.Version == "auto" && !context.isRunningInCI)
        {
            // Temporarily commit all changes to prevent checking in scripted changes like versioning.
            Git(context, "add .");
            Git(context, "commit --allow-empty -m 'backup'");
        }
    }

    public override void Teardown(Context context, ITeardownContext info)
    {
        if (context.Settings.Version == "auto" && !context.isRunningInCI)
        {
            // Undoes the script changes to all tracked files.
            Git(context, "reset --hard");
            
            // Undoes the setup commit keeping file states as before this build script ran.
            Git(context, "reset HEAD^");
        }
    }

    private static void Git(ICakeContext context, string arguments)
    {
        context.Information($"git ${arguments}");
        using var process = context.StartAndReturnProcess("git", new ProcessSettings {Arguments = arguments});
        process.WaitForExit();
    }
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

public sealed class CleanWebsite : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CleanDirectory(context.websiteDir);

    }
}

public sealed class CleanTemp : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CleanDirectory(context.tempDir);

    }
}

public sealed class CleanArtifacts : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.CleanDirectory(context.artifactsDir);

    }
}

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

[Dependency(typeof(CleanArtifacts))]
[Dependency(typeof(UpdateDnnManifests))]
[Dependency(typeof(CreateInstall))]
[Dependency(typeof(CreateUpgrade))]
[Dependency(typeof(CreateDeploy))]
[Dependency(typeof(CreateSymbols))]
public sealed class Default : FrostingTask<Context>
{
}

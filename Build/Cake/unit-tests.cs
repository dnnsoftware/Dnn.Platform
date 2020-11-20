using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.VSTest;
using Cake.Common.Tools.VSWhere;
using Cake.Common.Tools.VSWhere.Latest;
using Cake.Core.Diagnostics;
using Cake.Frosting;

[Dependency(typeof(UpdateDnnManifests))]
[Dependency(typeof(RestoreNuGetPackages))]
public sealed class EnsureAllProjectsBuilt : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.MSBuild("DNN_Platform.sln",
            new MSBuildSettings
            {
                Verbosity = Verbosity.Minimal,
                ToolVersion = MSBuildToolVersion.VS2017,
                Configuration = context.configuration
            });
    }
}

[Dependency(typeof(EnsureAllProjectsBuilt))]
public sealed class UnitTests : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var testAssemblies = context.GetFiles($@"**\bin\{context.configuration}\DotNetNuke.Tests.*.dll");
        testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Data.dll");
        testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Integration.dll");
        testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Utilities.dll");
        testAssemblies -= context.GetFiles(@"**\DotNetNuke.Tests.Urls.dll");

        foreach (var file in testAssemblies)
        {
            context.VSTest(file.FullPath,
                FixToolPath(context, new VSTestSettings()
                {
                    Logger = $"trx;LogFileName={file.GetFilename()}.xml",
                    Parallel = true,
                    EnableCodeCoverage = true,
                    FrameworkVersion = VSTestFrameworkVersion.NET45,
                    TestAdapterPath = @"tools\NUnitTestAdapter.2.1.1\tools"
                }));
        }
    }

// https://github.com/cake-build/cake/issues/1522
    VSTestSettings FixToolPath(Context context, VSTestSettings settings)
    {
// #tool vswhere
        settings.ToolPath =
            context.VSWhereLatest(new VSWhereLatestSettings {Requires = "Microsoft.VisualStudio.PackageGroup.TestTools.Core"})
                .CombineWithFilePath(context.File(@"Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"));
        return settings;
    }
}


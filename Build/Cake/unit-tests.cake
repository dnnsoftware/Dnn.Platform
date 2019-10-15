
Task("EnsureAllProjectsBuilt")
  .IsDependentOn("UpdateDnnManifests")
  .IsDependentOn("Restore-NuGet-Packages")
  .Does(() =>
  {
    MSBuild("DNN_Platform.sln", new MSBuildSettings {
      Verbosity = Verbosity.Minimal,
      ToolVersion = MSBuildToolVersion.VS2017,
      Configuration = configuration
    });
  });

Task("UnitTests")
  .IsDependentOn("EnsureAllProjectsBuilt")
  .Does(() =>
  {
    var testAssemblies = GetFiles($@"**\bin\{configuration}\DotNetNuke.Tests.*.dll");
    testAssemblies -= GetFiles(@"**\DotNetNuke.Tests.Data.dll");
    testAssemblies -= GetFiles(@"**\DotNetNuke.Tests.Integration.dll");
    testAssemblies -= GetFiles(@"**\DotNetNuke.Tests.Utilities.dll");
    testAssemblies -= GetFiles(@"**\DotNetNuke.Tests.Urls.dll");

    foreach (var file in testAssemblies) {
      VSTest(file.FullPath, FixToolPath(new VSTestSettings() {
        Logger = $"trx;LogFileName={file.GetFilename()}.xml",
        Parallel = true,
        EnableCodeCoverage = true,
        FrameworkVersion = VSTestFrameworkVersion.NET45,
        TestAdapterPath = @"tools\NUnitTestAdapter.2.1.1\tools"
      }));
    }
  });

// https://github.com/cake-build/cake/issues/1522
VSTestSettings FixToolPath(VSTestSettings settings)
{
    #tool vswhere
    settings.ToolPath =
        VSWhereLatest(new VSWhereLatestSettings { Requires = "Microsoft.VisualStudio.PackageGroup.TestTools.Core" })
        .CombineWithFilePath(File(@"Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"));
    return settings;
}

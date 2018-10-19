#tool "nuget:?package=Microsoft.TestPlatform&version=15.7.0"
#tool "nuget:?package=NUnitTestAdapter&version=2.1.1"

Task("EnsureAllProjectsBuilt")
  .IsDependentOn("CompileSource")
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
    var testAssemblies = GetFiles($@"**\bin\{configuration}\*test*.dll");
    testAssemblies -= GetFiles(@"**\*TestAdapter.dll");
    testAssemblies -= GetFiles(@"**\*Integration*.dll");
    testAssemblies -= GetFiles(@"**\DotNetNuke.Tests.Data.dll");
    testAssemblies -= GetFiles(@"**\DotNetNuke.Tests.Utilities.dll");
  
    foreach(var file in testAssemblies) {

        VSTest(file.FullPath, new VSTestSettings() { 
          Logger = $"trx;LogFileName={file.GetFilename()}.xml",
          Parallel = true,
          EnableCodeCoverage = true,
          FrameworkVersion = VSTestFrameworkVersion.NET45,
          TestAdapterPath = @"tools\NUnitTestAdapter.2.1.1\tools"
        });
    }
  });

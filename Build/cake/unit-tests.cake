var testAssemblies = GetFiles("**\*test*.dll");
testAssemblies -= GetFiles("**\*TestAdapter.dll");
testAssemblies -= GetFiles("**\obj\**");
testAssemblies -= GetFiles("**\*Integration*.dll");
testAssemblies -= GetFiles("**\DotNetNuke.Tests.Data.dll");
testAssemblies -= GetFiles("**\DotNetNuke.Tests.Utilities.dll");

Task("UnitTests")
  .IsDependentOn("CompileSources")
  .Does(() => 
  {
    VsTest(testAssemblies, new VSTestSettings() { 
      Logger = VSTestLogger.Trx,
      Parallel = true,
      EnableCodeCoverage = true,
      FrameworkVersion = VSTestFrameworkVersion.NET45
    });
  });

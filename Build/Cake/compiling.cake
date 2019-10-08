// Main solution
var dnnSolutionPath = "./DNN_Platform.sln";
var buildSettings = new MSBuildSettings()
.SetConfiguration(configuration)
.UseToolVersion(MSBuildToolVersion.VS2017)
.SetPlatformTarget(PlatformTarget.MSIL)
.WithTarget("Build");

Task("CompileSource")
    .IsDependentOn("CleanWebsite")
    .IsDependentOn("UpdateDnnManifests")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
	{
		MSBuild(dnnSolutionPath, settings => settings.WithTarget("Clean"));
		MSBuild(dnnSolutionPath, buildSettings);
	});

Task("Restore-NuGet-Packages")
    .Does(() =>
	{
		NuGetRestore(dnnSolutionPath);
	});

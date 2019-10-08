// Main solution
var dnnSolutionPath = "./DNN_Platform.sln";

Task("CompileSource")
    .IsDependentOn("CleanWebsite")
    .IsDependentOn("UpdateDnnManifests")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
	{
		var buildSettings = new MSBuildSettings()
			.SetConfiguration(configuration)
			.UseToolVersion(MSBuildToolVersion.VS2017)
			.SetPlatformTarget(PlatformTarget.MSIL)
			.WithTarget("Rebuild")
			.SetMaxCpuCount(4);
		MSBuild(dnnSolutionPath, settings => settings.WithTarget("Clean"));
		MSBuild(dnnSolutionPath, buildSettings);
	});

Task("Restore-NuGet-Packages")
    .Does(() =>
	{
		NuGetRestore(dnnSolutionPath);
	});

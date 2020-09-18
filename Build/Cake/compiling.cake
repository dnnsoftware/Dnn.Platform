// This tasks kicks off MS Build (just as in Visual Studio)

var dnnSolutionPath = "./DNN_Platform.sln";

Task("Build")
    .IsDependentOn("CleanWebsite")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
	{
		var buildSettings = new MSBuildSettings()
			.SetConfiguration(configuration)
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

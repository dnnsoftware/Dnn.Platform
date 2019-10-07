Task("CompileSource")
    .IsDependentOn("CleanWebsite")
    .IsDependentOn("UpdateDnnManifests")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
	{
		MSBuild(dnnSolutionPath, settings => settings.WithTarget("Clean"));
		MSBuild(dnnSolutionPath, buildSettings);
	});

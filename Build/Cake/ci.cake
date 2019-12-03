Task("BuildAll")
    .IsDependentOn("CleanArtifacts")
    .IsDependentOn("UpdateDnnManifests")
	.IsDependentOn("CreateInstall")
	.IsDependentOn("CreateUpgrade")
    .IsDependentOn("CreateDeploy")
    .IsDependentOn("CreateSymbols")
    .IsDependentOn("CreateNugetPackages")
    .Does(() =>
	{
	});

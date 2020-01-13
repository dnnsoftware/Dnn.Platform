// This is the task CI will use to build release packages

Task("BuildAll")
    .IsDependentOn("CleanArtifacts")
    .IsDependentOn("GenerateChecksum")
    .IsDependentOn("SetPackageVersions")
	.IsDependentOn("CreateInstall")
	.IsDependentOn("CreateUpgrade")
    .IsDependentOn("CreateDeploy")
    .IsDependentOn("CreateSymbols")
    .IsDependentOn("CreateNugetPackages")
    .Does(() =>
	{
        RevertSqlDataProvider();
	});

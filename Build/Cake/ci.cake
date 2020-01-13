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

#addin nuget:?package=Cake.XdtTransform&version=0.18.1&loaddependencies=true
#addin nuget:?package=Cake.FileHelpers&version=3.2.0
#addin nuget:?package=Cake.Powershell&version=0.4.8

#addin nuget:?package=Dnn.CakeUtils&version=1.1.0
#tool "nuget:?package=GitVersion.CommandLine&version=5.0.1"
#tool "nuget:?package=Microsoft.TestPlatform&version=15.7.0"
#tool "nuget:?package=NUnitTestAdapter&version=2.1.1"

#load "local:?path=Build/Cake/compiling.cake"
#load "local:?path=Build/Cake/create-database.cake"
#load "local:?path=Build/Cake/external.cake"
#load "local:?path=Build/Cake/nuget.cake"
#load "local:?path=Build/Cake/packaging.cake"
#load "local:?path=Build/Cake/testing.cake"
#load "local:?path=Build/Cake/thirdparty.cake"
#load "local:?path=Build/Cake/unit-tests.cake"
#load "local:?path=Build/Cake/version.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var targetBranchCk = Argument("CkBranch", "development");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var tempFolder = "./Temp/";
var tempDir = Directory(tempFolder);
var artifactsFolder = "./Artifacts/";
var artifactsDir = Directory(artifactsFolder);
var websiteFolder = "./Website/";
var websiteDir = Directory(websiteFolder);

// Define versioned files (manifests) to backup and revert on build
var manifestFiles = GetFiles("./**/*.dnn");
manifestFiles.Add(GetFiles("./SolutionInfo.cs"));

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("CleanWebsite")
    .Does(() =>
	{
		CleanDirectory(websiteDir);
	});

Task("CleanTemp")
    .Does(() =>
	{
		CleanDirectory(tempDir);
	});
    
Task("CleanArtifacts")
    .Does(() =>
	{
		CleanDirectory(artifactsDir);
	});

Task("Build")
	.IsDependentOn("CompileSource")
    .Does(() =>
	{
	});
    
Task("BuildWithDatabase")
    .IsDependentOn("CleanArtifacts")
	.IsDependentOn("CompileSource")
	.IsDependentOn("CreateInstall")
	.IsDependentOn("CreateUpgrade")
	.IsDependentOn("CreateDeploy")
    .IsDependentOn("CreateSymbols")
    .IsDependentOn("CreateDatabase")
    .Does(() =>
	{
	});
    
Task("BuildInstallUpgradeOnly")
    .IsDependentOn("CleanArtifacts")
	.IsDependentOn("CompileSource")
	.IsDependentOn("CreateInstall")
	.IsDependentOn("CreateUpgrade")
    .Does(() =>
	{
	});

Task("BuildAll")
    .IsDependentOn("CleanArtifacts")
	.IsDependentOn("BackupManifests")
	.IsDependentOn("CreateInstall")
	.IsDependentOn("CreateUpgrade")
    .IsDependentOn("CreateDeploy")
    .IsDependentOn("CreateSymbols")
    .IsDependentOn("CreateNugetPackages")
	.IsDependentOn("RestoreManifests")
    .Does(() =>
	{
	});

Task("BackupManifests")
	.Does( () => {		
		Zip("./", "manifestsBackup.zip", manifestFiles);
	});

Task("RestoreManifests")	
	.Does( () => {
		DeleteFiles(manifestFiles);
		Unzip("./manifestsBackup.zip", "./");
		DeleteFiles("./manifestsBackup.zip");
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("BuildAll");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

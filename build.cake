#addin nuget:?package=Cake.XdtTransform&version=0.18.1&loaddependencies=true
#addin nuget:?package=Cake.FileHelpers&version=3.2.0
#addin nuget:?package=Cake.Powershell&version=0.4.8

#addin nuget:?package=Dnn.CakeUtils
#tool "nuget:?package=GitVersion.CommandLine&version=5.0.1"
#tool "nuget:?package=Microsoft.TestPlatform&version=15.7.0"
#tool "nuget:?package=NUnitTestAdapter&version=2.1.1"

#load "local:?path=Build/Cake/external.cake"
#load "local:?path=Build/Cake/version.cake"
#load "local:?path=Build/Cake/create-database.cake"
#load "local:?path=Build/Cake/unit-tests.cake"
#load "local:?path=Build/Cake/packaging.cake"
#load "local:?path=Build/Cake/other.cake"

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

// Main solution
var dnnSolutionPath = "./DNN_Platform.sln";
var buildSettings = new MSBuildSettings()
.SetConfiguration(configuration)
.UseToolVersion(MSBuildToolVersion.VS2017)
.SetPlatformTarget(PlatformTarget.MSIL)
.WithTarget("Build");

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

Task("Restore-NuGet-Packages")
    .Does(() =>
	{
		NuGetRestore("./DNN_Platform.sln");
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

Task("Test")
    .IsDependentOn("CleanArtifacts")
	.IsDependentOn("BackupManifests")
	.IsDependentOn("CompileSource")
	.IsDependentOn("ExternalExtensions")
	.IsDependentOn("CreateInstall")
	.IsDependentOn("RestoreManifests")
    .Does(() =>
	{
	});

Task("BuildAll")
    .IsDependentOn("CleanArtifacts")
	.IsDependentOn("BackupManifests")
	.IsDependentOn("CompileSource")
	.IsDependentOn("ExternalExtensions")
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

Task("CompileSource")
    .IsDependentOn("CleanWebsite")
    .IsDependentOn("UpdateDnnManifests")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
	{
		MSBuild(dnnSolutionPath, settings => settings.WithTarget("Clean"));
		MSBuild(dnnSolutionPath, buildSettings);
	});

/*
Task("CompileSource")
    .IsDependentOn("CleanWebsite")
    .IsDependentOn("UpdateDnnManifests")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
	{
		MSBuild(createCommunityPackages, c =>
		{
			c.Configuration = configuration;
			c.WithProperty("BUILD_NUMBER", GetProductVersion());
			c.Targets.Add("CompileSource");
		});
	});

Task("CreateInstall")
	.IsDependentOn("CompileSource")
	.Does(() =>
	{
		CreateDirectory("./Artifacts");
	
		MSBuild(createCommunityPackages, c =>
		{
			c.Configuration = configuration;
			c.WithProperty("BUILD_NUMBER", GetProductVersion());
			c.Targets.Add("CreateInstall");
		});
	});

Task("CreateUpgrade")
	.IsDependentOn("CompileSource")
	.Does(() =>
	{
		CreateDirectory("./Artifacts");
	
		MSBuild(createCommunityPackages, c =>
		{
			c.Configuration = configuration;
			c.WithProperty("BUILD_NUMBER", GetProductVersion());
			c.Targets.Add("CreateUpgrade");
		});
	});
    
Task("CreateSymbols")
	.IsDependentOn("CompileSource")
	.Does(() =>
	{
		CreateDirectory("./Artifacts");
	
		MSBuild(createCommunityPackages, c =>
		{
			c.Configuration = configuration;
			c.WithProperty("BUILD_NUMBER", GetProductVersion());
			c.Targets.Add("CreateSymbols");
		});
	});

Task("CreateDeploy")
	.IsDependentOn("CompileSource")
	.Does(() =>
	{
		CreateDirectory("./Artifacts");
		
		MSBuild(createCommunityPackages, c =>
		{
			c.Configuration = configuration;
			c.WithProperty("BUILD_NUMBER", GetProductVersion());
			c.Targets.Add("CreateDeploy");
		});
	});
*/

Task("CreateNugetPackages")
	.IsDependentOn("CompileSource")
	.Does(() =>
	{
		//look for solutions and start building them
		var nuspecFiles = GetFiles("./Build/Tools/NuGet/DotNetNuke.*.nuspec");
	
		Information("Found {0} nuspec files.", nuspecFiles.Count);

		//basic nuget package configuration
		var nuGetPackSettings = new NuGetPackSettings
		{
			Version = GetBuildNumber(),
			OutputDirectory = @"./Artifacts/",
			IncludeReferencedProjects = true,
			Properties = new Dictionary<string, string>
			{
				{ "Configuration", "Release" }
			}
		};
	
		//loop through each nuspec file and create the package
		foreach (var spec in nuspecFiles){
			var specPath = spec.ToString();

			Information("Starting to pack: {0}", specPath);
			NuGetPack(specPath, nuGetPackSettings);
		}


	});

Task("Run-Unit-Tests")
    .IsDependentOn("CompileSource")
    .Does(() =>
	{
		NUnit3("./src/**/bin/" + configuration + "/*.Test*.dll", new NUnit3Settings {
			NoResults = false
			});
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

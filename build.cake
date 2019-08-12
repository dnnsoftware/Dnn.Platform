#addin nuget:?package=Cake.XdtTransform&version=0.18.1&loaddependencies=true
#addin nuget:?package=Cake.FileHelpers&version=3.2.0
#addin nuget:?package=Cake.Powershell&version=0.4.8

#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"
#tool "nuget:?package=Microsoft.TestPlatform&version=15.7.0"
#tool "nuget:?package=NUnitTestAdapter&version=2.1.1"

#load "local:?path=Build/Cake/version.cake"
#load "local:?path=Build/Cake/create-database.cake"
#load "local:?path=Build/Cake/unit-tests.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var createCommunityPackages = "./Build/BuildScripts/CreateCommunityPackages.build";

var targetBranchCk = Argument("CkBranch", "development");
var targetBranchCdf = Argument("CdfBranch", "dnn");
var targetBranchCp = Argument("CpBranch", "development");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/");
var artifactDir = Directory("./Artifacts/");
var tempDir = Directory("./Temp/");
var buildDirFullPath = System.IO.Path.GetFullPath(buildDir.ToString()) + "\\";

// Define versioned files (manifests) to backup and revert on build
var manifestFiles = GetFiles("./**/*.dnn");
manifestFiles.Add(GetFiles("./SolutionInfo.cs"));

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
	{
		CleanDirectory(buildDir);
		CleanDirectory(tempDir);
	});
    
Task("CleanArtifacts")
    .Does(() =>
	{
		CleanDirectory(artifactDir);
	});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
	{
		NuGetRestore("./DNN_Platform.sln");
	});

Task("Build")
    .IsDependentOn("CleanArtifacts")
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

Task("ExternalExtensions")
.IsDependentOn("Clean")
    .Does(() =>
	{
        Information("CK:'{0}', CDF:'{1}', CP:'{2}'", targetBranchCk, targetBranchCdf, targetBranchCp);

    
		Information("Downloading External Extensions to {0}", buildDirFullPath);

        
        
		//ck
		DownloadFile("https://github.com/DNN-Connect/CKEditorProvider/archive/" + targetBranchCk + ".zip", buildDirFullPath + "ckeditor.zip");
	
		//cdf
		DownloadFile("https://github.com/dnnsoftware/ClientDependency/archive/" + targetBranchCdf + ".zip", buildDirFullPath + "clientdependency.zip");

		//pb
        Information("Downloading: {0}", "https://github.com/dnnsoftware/Dnn.AdminExperience/archive/" + targetBranchCp + ".zip");
		DownloadFile("https://github.com/dnnsoftware/Dnn.AdminExperience/archive/" + targetBranchCp + ".zip", buildDirFullPath + "Dnn.AdminExperience.zip");

		Information("Decompressing: {0}", "CK Editor");
		Unzip(buildDirFullPath + "ckeditor.zip", buildDirFullPath + "Providers/");

		Information("Decompressing: {0}", "CDF");
		Unzip(buildDirFullPath + "clientdependency.zip", buildDirFullPath + "Modules");
	
		Information("Decompressing: {0}", "Admin Experience");
		Unzip(buildDirFullPath + "Dnn.AdminExperience.zip", tempDir);


		//look for solutions and start building them
		var externalSolutions = GetFiles("./src/**/*.sln");
	
		Information("Found {0} solutions.", externalSolutions.Count);
	
		foreach (var solution in externalSolutions){
			var solutionPath = solution.ToString();
		
			//cdf contains two solutions, we only want the dnn solution
			if (solutionPath.Contains("ClientDependency-dnn") && !solutionPath.EndsWith(".DNN.sln")) {
				Information("Ignoring Solution File: {0}", solutionPath);
				continue;
			}
			else {
				Information("Processing Solution File: {0}", solutionPath);
			}
		
			Information("Starting NuGetRestore: {0}", solutionPath);
			NuGetRestore(solutionPath);

			Information("Starting to Build: {0}", solutionPath);
			MSBuild(solutionPath, settings => settings.SetConfiguration(configuration));
		}


		externalSolutions = GetFiles("./" + tempDir.ToString() + "/**/*.sln");

		Information("Found {0} solutions.", externalSolutions.Count);
	
		foreach (var solution in externalSolutions){
			var solutionPath = solution.ToString();
		
			Information("Processing Solution File: {0}", solutionPath);
			Information("Starting NuGetRestore: {0}", solutionPath);
			NuGetRestore(solutionPath);

			Information("Starting to Build: {0}", solutionPath);
			MSBuild(solutionPath, settings => settings.SetConfiguration(configuration));
		}
	
	
		//grab all install zips and copy to staging directory

		var fileCounter = 0;

		fileCounter = GetFiles("./src/Providers/**/*_Install.zip").Count;
		Information("Copying {1} Artifacts from {0}", "Providers", fileCounter);
		CopyFiles("./src/Providers/**/*_Install.zip", "./Website/Install/Provider/");

		fileCounter = GetFiles("./src/Modules/**/*_Install.zip").Count;
		Information("Copying {1} Artifacts from {0}", "Modules", fileCounter);
		CopyFiles("./src/Modules/**/*_Install.zip", "./Website/Install/Module/");

		//CDF is handled with nuget, and because the version isn't updated in git this builds as an "older" version and fails.
		//fileCounter = GetFiles("./src/Modules/ClientDependency-dnn/ClientDependency.Core/bin/Release/ClientDependency.Core.*").Count;
		//Information("Copying {1} Artifacts from {0}", "CDF", fileCounter);
		//CopyFiles("./src/Modules/ClientDependency-dnn/ClientDependency.Core/bin/Release/ClientDependency.Core.*", "./Website/bin");
	
		var files = GetFiles("./" + tempDir.ToString() + "/*/Website/Install/Module/*_Install.zip");
		Information("Copying {1} Artifacts from {0}", "AdminExperience", files.Count);
		CopyFiles(files, "./Website/Install/Module/");
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
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

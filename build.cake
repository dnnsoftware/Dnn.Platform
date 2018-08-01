#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var createCommunityPackages = "./Build/BuildScripts/CreateCommunityPackages.build";
var buildNumber = Argument("buildNumber", "9.2.2");;

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/");
var artifactDir = Directory("./Artifacts/");
var tempDir = "C:\\temp\\x\\";

var buildDirFullPath = System.IO.Path.GetFullPath(buildDir.ToString()) + "\\";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
	{
		CleanDirectory(buildDir);
		CleanDirectory(artifactDir);
		CleanDirectory(tempDir);
	});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
	{
		NuGetRestore("./DNN_Platform.sln");
	});

Task("Build")
	.IsDependentOn("CompileSource")
	
	.IsDependentOn("CreateInstall")
	.IsDependentOn("CreateUpgrade")
	.IsDependentOn("CreateDeploy")

    .Does(() =>
	{
	
	
	});

Task("BuildAll")
	.IsDependentOn("CompileSource")

	.IsDependentOn("ExternalExtensions")

	.IsDependentOn("CreateInstall")
	.IsDependentOn("CreateUpgrade")
	.IsDependentOn("CreateDeploy")

    .Does(() =>
	{
	
	
	});

Task("CompileSource")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
	{
		MSBuild(createCommunityPackages, c =>
		{
			c.Configuration = configuration;
			c.WithProperty("BUILD_NUMBER", buildNumber);
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
			c.WithProperty("BUILD_NUMBER", buildNumber);
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
			c.WithProperty("BUILD_NUMBER", buildNumber);
			c.Targets.Add("CreateUpgrade");
		});
	});

Task("CreateSource")
	.Does(() =>
	{
		CreateDirectory("./Artifacts");
		CleanDirectory("./src/Projects/");
	
		using (var process = StartAndReturnProcess("git", new ProcessSettings{Arguments = "clean -xdf --exclude=tools/cake/**"}))
		{
			process.WaitForExit();
			Information("Git Clean Exit code: {0}", process.GetExitCode());
		};
	
		MSBuild(createCommunityPackages, c =>
		{
			c.Configuration = configuration;
			c.WithProperty("BUILD_NUMBER", buildNumber);
			c.Targets.Add("CreateSource");
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
			c.WithProperty("BUILD_NUMBER", buildNumber);
			c.Targets.Add("CreateDeploy");
		});
	});

Task("ExternalExtensions")
    .Does(() =>
	{
		CreateDirectory("./src/Projects");
		CreateDirectory("./src/Projects/Providers");
		CreateDirectory("./src/Projects/Modules");

		Information("Downloading External Extensions to {0}", buildDirFullPath);

		//ck
		DownloadFile("https://github.com/DNN-Connect/CKEditorProvider/archive/development.zip", buildDirFullPath + "ckeditor.zip");
	
		//cdf
		DownloadFile("https://github.com/dnnsoftware/ClientDependency/archive/dnn.zip", buildDirFullPath + "clientdependency.zip");

		//pb
		DownloadFile("https://github.com/dnnsoftware/Dnn.AdminExperience.Library/archive/development.zip", buildDirFullPath + "Dnn.AdminExperience.Library.zip");
		DownloadFile("https://github.com/dnnsoftware/Dnn.AdminExperience.Extensions/archive/development.zip", buildDirFullPath + "Dnn.AdminExperience.Extensions.zip");
		DownloadFile("https://github.com/dnnsoftware/Dnn.EditBar/archive/development.zip", buildDirFullPath + "Dnn.EditBar.zip");
	

		Information("Decompressing: {0}", "CK Editor");
		Unzip(buildDirFullPath + "ckeditor.zip", buildDirFullPath + "Providers/");

		Information("Decompressing: {0}", "CDF");
		Unzip(buildDirFullPath + "clientdependency.zip", buildDirFullPath + "Modules");
	
		Information("Decompressing: {0}", "PersonaBar Library");
		Unzip(buildDirFullPath + "Dnn.AdminExperience.Library.zip", tempDir);

		Information("Decompressing: {0}", "PersonaBar Extension");
		Unzip(buildDirFullPath + "Dnn.AdminExperience.Extensions.zip", tempDir);

		Information("Decompressing: {0}", "EditBar");
		Unzip(buildDirFullPath + "Dnn.EditBar.zip", tempDir);
	

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


		externalSolutions = GetFiles("c:\\temp\\x\\**\\*.sln");
	
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
	
		fileCounter = GetFiles("C:\\temp\\x\\*\\Website\\Install\\Module\\*_Install.zip").Count;
		Information("Copying {1} Artifacts from {0}", "PersonaBar", fileCounter);
		CopyFiles("C:\\temp\\x\\*\\Website\\Install\\Module\\*_Install.zip", "./Website/Install/Module/");
	
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


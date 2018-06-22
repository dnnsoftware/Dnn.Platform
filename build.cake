#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var createCommunityPackages = "./Build/BuildScripts/CreateCommunityPackages.build";
var buildNumber = Argument("buildNumber", "9.2.1");;

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/");
var artifactDir = Directory("./Artifacts/");
var tempDir = Directory("c:\\temp\\x\\");

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
	.IsDependentOn("ExternalExtensions")
	.IsDependentOn("CreateInstall")
	.IsDependentOn("CreateUpgrade")
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

Task("ExternalExtensions")
    .Does(() =>
{
	CreateDirectory("./src/Projects");
	CreateDirectory("./src/Projects/Providers");
	CreateDirectory("./src/Projects/Modules");

	//CKEditor
	DownloadFile("https://github.com/DNN-Connect/CKEditorProvider/archive/development.zip", "./src/ckeditor.zip");
	Unzip("./src/ckeditor.zip", "./src/Projects/Providers/");
	
	//cdf
	DownloadFile("https://github.com/dnnsoftware/ClientDependency/archive/dnn.zip", "./src/clientdependency.zip");
	Unzip("./src/clientdependency.zip", "./src/Modules/");
	
	//pb
	DownloadFile("https://github.com/dnnsoftware/Dnn.AdminExperience.Library/archive/development.zip", "./src/Dnn.AdminExperience.Library.zip");
	DownloadFile("https://github.com/dnnsoftware/Dnn.AdminExperience.Extensions/archive/development.zip", "./src/Dnn.AdminExperience.Extensions.zip");
	DownloadFile("https://github.com/dnnsoftware/Dnn.EditBar/archive/development.zip", "./src/Dnn.EditBar.zip");
	
	//todo: path too long, java requirement, verify output
	Unzip("./src/Dnn.AdminExperience.Library.zip", "c:\\temp\\x");
	Unzip("./src/Dnn.AdminExperience.Extensions.zip", "c:\\temp\\x");
	Unzip("./src/Dnn.EditBar.zip", "c:\\temp\\x");
	
	//CopyDirectory("c:\\temp\\x", "./src/Projects/");
	
	var externalSolutions = GetFiles("./src/Projects/**/*.sln");
	
	Information("Found {0} solutions.", externalSolutions.Count);
	
	foreach (var solution in externalSolutions){
		var solutionPath = solution.ToString();
		
		//cdf contains two solutions, we only want the dnn solution
		if (solutionPath.Contains("ClientDependency-dnn") && !solutionPath.EndsWith(".DNN.sln")) {
			continue;
		}
		
		Information("File: {0}", solutionPath);
		NuGetRestore(solutionPath);
		MSBuild(solutionPath, settings => settings.SetConfiguration(configuration));
	}
	
	externalSolutions = GetFiles("c:/temp/x/**/*.sln");
	
	Information("Found {0} solutions.", externalSolutions.Count);
	
	foreach (var solution in externalSolutions){
		var solutionPath = solution.ToString();
		
		Information("File: {0}", solutionPath);
		
		NuGetRestore(solutionPath);
		MSBuild(solutionPath, settings => settings.SetConfiguration(configuration));
	}
	
	//grab all install zips and copy to staging directory
	CopyFiles("./src/Projects/Providers/**/*_Install.zip", "./Website/Install/Provider/");
	CopyFiles("./src/Projects/Modules/**/*_Install.zip", "./Website/Install/Module/");
	
	
	//update cdf to latest build
	CopyFiles("./src/Projects/ClientDependency-dnn/ClientDependency.Core/bin/Release/ClientDependency.Core.*", "./Website/bin");
	
	//grab all the personabar extensions
	CopyFiles("c:/temp/x/**/*_Install.zip", "./Website/Install/Module/");
	
});

Task("CreateInstall")
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
	
	using (var process = StartAndReturnProcess("git", new ProcessSettings{Arguments = "clean -xdf"}))
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


Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        NoResults = true
        });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);


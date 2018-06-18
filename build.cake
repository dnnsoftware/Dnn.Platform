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


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
	CleanDirectory(artifactDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./DNN_Platform.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
	.IsDependentOn("CompileSource")
    .Does(() =>
{
	
	
});

Task("CompileSource")
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
	.IsDependentOn("Clean")
    .Does(() =>
{
	CreateDirectory("./src/Downloads");
	CreateDirectory("./src/Projects");

	DownloadFile("https://github.com/DNN-Connect/CKEditorProvider/archive/development.zip", "./src/Downloads/ckeditor.zip");
	Unzip("./src/Downloads/ckeditor.zip", "./src/Projects/");
	
	
	var externalSolutions = GetFiles("./src/Projects/**/*.sln");
	
	Information("Found {0} solutions.", externalSolutions.Count);
	
	foreach (var solution in externalSolutions){
		Information("File: {0}", solution);
		NuGetRestore(solution);
		MSBuild(solution, settings => settings.SetConfiguration(configuration));
		
		CopyFiles("./src/Projects/**/*_Install.zip", artifactDir);
	}
	
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


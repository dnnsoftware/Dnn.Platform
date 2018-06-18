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
	.IsDependentOn("ExternalExtensions")
	.IsDependentOn("CreateInstall")
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
    .Does(() =>
{
	CreateDirectory("./src/Downloads");
	CreateDirectory("./src/Projects");

	//CKEditor
	DownloadFile("https://github.com/DNN-Connect/CKEditorProvider/archive/development.zip", "./src/Downloads/ckeditor.zip");
	Unzip("./src/Downloads/ckeditor.zip", "./src/Projects/");
	
	//cdf
	DownloadFile("https://github.com/dnnsoftware/ClientDependency/archive/dnn.zip", "./src/Downloads/clientdependency.zip");
	Unzip("./src/Downloads/clientdependency.zip", "./src/Projects/");
	
	//pb
	DownloadFile("https://github.com/dnnsoftware/Dnn.AdminExperience.Library/archive/development.zip", "./src/Downloads/AdminExperience.Library.zip");
	DownloadFile("https://github.com/dnnsoftware/Dnn.AdminExperience.Extensions/archive/development.zip", "./src/Downloads/AdminExperience.Extensions.zip");
	DownloadFile("https://github.com/dnnsoftware/Dnn.EditBar/archive/development.zip", "./src/Downloads/EditBar.zip");
	
	//todo: path too long, java requirement, verify output
	//Unzip("./src/Downloads/AdminExperience.Library.zip", "./src/Projects/");
	//Unzip("./src/Downloads/AdminExperience.Extensions.zip", "./src/Projects/");
	//Unzip("./src/Downloads/EditBar.zip", "./src/Projects/");

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
	
	//grab all install zips and copy to staging directory
	CopyFiles("./src/Projects/**/*_Install.zip", artifactDir);
	
	//update cdf to latest build
	CopyFiles("./src/Projects/ClientDependency-dnn/ClientDependency.Core/bin/Release/ClientDependency.Core.*", "./Website/bin");
	
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


#addin nuget:?package=Cake.XdtTransform&version=0.18.1&loaddependencies=true
#addin nuget:?package=Cake.FileHelpers&version=3.2.0
#addin nuget:?package=Cake.Powershell&version=0.4.8

#addin nuget:?package=Dnn.CakeUtils&version=1.1.6
#tool "nuget:?package=GitVersion.CommandLine&version=5.0.1"
#tool "nuget:?package=Microsoft.TestPlatform&version=15.7.0"
#tool "nuget:?package=NUnitTestAdapter&version=2.1.1"

#load "local:?path=Build/Cake/ci.cake"
#load "local:?path=Build/Cake/compiling.cake"
#load "local:?path=Build/Cake/create-database.cake"
#load "local:?path=Build/Cake/database.cake"
#load "local:?path=Build/Cake/devsite.cake"
#load "local:?path=Build/Cake/external.cake"
#load "local:?path=Build/Cake/nuget.cake"
#load "local:?path=Build/Cake/packaging.cake"
#load "local:?path=Build/Cake/settings.cake"
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

// Global information variables
bool isRunningInCI = false;

//////////////////////////////////////////////////////////////////////
// SETUP/TEARDOWN
//////////////////////////////////////////////////////////////////////

// Executed BEFORE the first task.
Setup(context =>
{
	isRunningInCI = context.HasEnvironmentVariable("TF_BUILD");
	Information("Is Running in CI : {0}", isRunningInCI);
	if(Settings.Version == "auto" && !isRunningInCI){
		// Temporarelly commit all changes to prevent checking in scripted changes like versioning.
		StartPowershellScript("git add .");
		StartPowershellScript("git commit -m 'backup'");	
	}
});

// Executed AFTER the last task even if any task fails.
Teardown(context =>
{
	if(Settings.Version == "auto" && !isRunningInCI){
		// Undoes the script changes to all tracked files.
		StartPowershellScript("git reset --hard");
		// Undoes the setup commit keeping file states as before this build script ran.
		StartPowershellScript("git reset HEAD^");
	}
});

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

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("BuildAll");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

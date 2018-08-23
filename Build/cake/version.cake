#addin Cake.XdtTransform
#addin "Cake.FileHelpers"
#tool "nuget:?package=GitVersion.CommandLine"

GitVersion version;

Task("GitVersion")
  .Does(() => {
    version = GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = @"SolutionInfo.cs"
    });
  });

Task("UpdateDnnManifests")
  .IsDependentOn("CalculateVersion")
  .DoesForEach(GetFiles("**/*.dnn"), (file) => 
  { 
    var transformFile = File(System.IO.Path.GetTempFileName());
    FileAppendText(transformFile, GetXdtTransformation());
    XdtTransformConfig(file, transformFile, file);
});

public string GetBuildNumber()
{
    return version.MajorMinorPatch;
}

public string GetXdtTransformation()
{
    var versionString = $"{version.Major.ToString("00")}.{version.Minor.ToString("00")}.{version.Patch.ToString("00")}";

    return $@"<?xml version=""1.0""?>
<dotnetnuke xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
  <packages>
    <package version=""{versionString}"" 
             xdt:Transform=""SetAttributes(version)""
             xdt:Locator=""Condition(not[@version])"" />
  </packages>
</dotnetnuke>";
}
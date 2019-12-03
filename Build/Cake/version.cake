// These tasks are meant for our CI build process. They set the versions of the assemblies and manifests to the version found on Github.

GitVersion version;
var buildId = EnvironmentVariable("BUILD_BUILDID") ?? "0";
var buildNumber = "";
var productVersion = "";

var unversionedManifests = new string[] {
  "DNN Platform/Components/Microsoft.*/**/*.dnn",
  "DNN Platform/Components/Newtonsoft/*.dnn",
  "DNN Platform/JavaScript Libraries/**/*.dnn",
  "Temp/**/*.dnn"
};

Task("BuildServerSetVersion")
  .IsDependentOn("SetVersion")
  .Does(() => {
    StartPowershellScript($"Write-Host ##vso[build.updatebuildnumber]{version.FullSemVer}.{buildId}");
});

Task("SetVersion")
  .Does(() => {
    version = GitVersion();
    Information(Newtonsoft.Json.JsonConvert.SerializeObject(version));
    Dnn.CakeUtils.Utilities.UpdateAssemblyInfoVersion(new System.Version(version.Major, version.Minor, version.Patch, version.CommitsSinceVersionSource != null ? (int)version.CommitsSinceVersionSource : 0), version.InformationalVersion, "SolutionInfo.cs");
    Information("Informational Version : " + version.InformationalVersion);
    buildNumber = version.LegacySemVerPadded;
    productVersion = version.MajorMinorPatch;
    Information("Product Version : " + productVersion);
    Information("Build Number : " + buildNumber);
    Information("The build Id is : " + buildId);
});

Task("UpdateDnnManifests")
  .IsDependentOn("SetVersion")
  .DoesForEach(GetFilesByPatterns(".", new string[] {"**/*.dnn"}, unversionedManifests), (file) => 
  { 
    Information("Transforming: " + file);
    var transformFile = File(System.IO.Path.GetTempFileName());
    FileAppendText(transformFile, GetXdtTransformation());
    XdtTransformConfig(file, transformFile, file);
});

public string GetBuildNumber()
{
    return buildNumber;
}

public string GetProductVersion()
{
    return productVersion;
}

public string GetXdtTransformation()
{
    var versionString = $"{version.Major.ToString("00")}.{version.Minor.ToString("00")}.{version.Patch.ToString("00")}";

    return $@"<?xml version=""1.0""?>
<dotnetnuke xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
  <packages>
    <package version=""{versionString}"" 
             xdt:Transform=""SetAttributes(version)"" />
  </packages>
</dotnetnuke>";
}

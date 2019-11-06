GitVersion version;
var buildId = EnvironmentVariable("BUILD_BUILDID") ?? "0";
var buildNumber = "";
var productVersion = "";

Task("BuildServerSetVersion")
  .IsDependentOn("SetVersion")
  .Does(() => {
    StartPowershellScript($"Write-Host ##vso[build.updatebuildnumber]{version.FullSemVer}.{buildId}");
});

Task("SetVersion")
  .Does(() => {
    Information("Local Settings Version is : " + Settings.Version);
    if (Settings.Version == "auto") {
      version = GitVersion();
      Information(Newtonsoft.Json.JsonConvert.SerializeObject(version));
      Dnn.CakeUtils.Utilities.UpdateAssemblyInfoVersion(new System.Version(version.Major, version.Minor, version.Patch, version.CommitsSinceVersionSource != null ? (int)version.CommitsSinceVersionSource : 0), version.InformationalVersion, "SolutionInfo.cs");
      Information("Informational Version : " + version.InformationalVersion);
      buildNumber = version.LegacySemVerPadded;
      productVersion = version.MajorMinorPatch;
    } else {
      var v = new System.Version(Settings.Version);
      if (v.Revision == -1) {
        v = new System.Version(Settings.Version + ".0");
      }
      Dnn.CakeUtils.Utilities.UpdateAssemblyInfoVersion(v, Settings.Version + " - Custom local build", "SolutionInfo.cs");
      buildNumber = Settings.Version;
      productVersion = v.ToString(3);
    }
    Information("Product Version : " + productVersion);
    Information("Build Number : " + buildNumber);
    Information("The build Id is : " + buildId);
});

Task("UpdateDnnManifests")
  .IsDependentOn("SetVersion")
  .DoesForEach(GetFiles("**/*.dnn"), (file) => 
  { 
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
             xdt:Transform=""SetAttributes(version)""
             xdt:Locator=""Condition([not(@version)])"" />
  </packages>
</dotnetnuke>";
}

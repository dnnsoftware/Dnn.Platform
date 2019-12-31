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
    if (Settings.Version == "auto") {
      version = GitVersion();
      buildNumber = version.LegacySemVerPadded;
    } else {
      version = new GitVersion();
      var requestedVersion = new System.Version(Settings.Version);
      version.Major = requestedVersion.Major;
      version.Minor = requestedVersion.Minor;
      version.Patch = requestedVersion.Build;
      version.InformationalVersion = requestedVersion.ToString(3) + " Custom build";
      version.MajorMinorPatch = requestedVersion.ToString(3);
      if (requestedVersion.Revision != -1) {
        version.CommitsSinceVersionSource = requestedVersion.Revision;
        version.InformationalVersion = requestedVersion.ToString(4) + " Custom build";
      }
    }
    Information(Newtonsoft.Json.JsonConvert.SerializeObject(version));
    Dnn.CakeUtils.Utilities.UpdateAssemblyInfoVersion(new System.Version(version.Major, version.Minor, version.Patch, version.CommitsSinceVersionSource != null ? (int)version.CommitsSinceVersionSource : 0), version.InformationalVersion, "SolutionInfo.cs");
    Information("Informational Version : " + version.InformationalVersion);
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

public string GetTwoDigitsVersionNumber(){
    var fullVer = GetBuildNumber().Split('-')[0]; // Gets rid of the -unstable, -beta, etc.
    var numbers = fullVer.Split('.');
    for (int i=0; i < numbers.Length; i++)
    {
      if (numbers[i].Length < 2)
      {
        numbers[i] = "0" + numbers[i];
      }
    }
    return String.Join(".", numbers);
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

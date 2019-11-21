GitVersion version;
var buildId = EnvironmentVariable("BUILD_BUILDID") ?? "0";

Task("BuildServerSetVersion")
  .IsDependentOn("GitVersion")
  .Does(() => {
    StartPowershellScript($"Write-Host ##vso[build.updatebuildnumber]{version.FullSemVer}.{buildId}");
});

Task("GitVersion")
  .Does(() => {
    Information("Local Settings Version is : " + Settings.Version);
    if (Settings.Version == "auto") {
      version = GitVersion(new GitVersionSettings {
          UpdateAssemblyInfo = true,
          UpdateAssemblyInfoFilePath = @"SolutionInfo.cs"
      });
      Information(Newtonsoft.Json.JsonConvert.SerializeObject(version));
    } else {
      version = new GitVersion();
      var v = new System.Version(Settings.Version);
      version.AssemblySemFileVer = Settings.Version.ToString();
      version.Major = v.Major;
      version.Minor = v.Minor;
      version.Patch = v.Build;
      version.Patch = v.Revision;
      version.FullSemVer = v.ToString();
      version.InformationalVersion = v.ToString() + "-custom";
      FileAppendText("SolutionInfo.cs", string.Format("[assembly: AssemblyVersion(\"{0}\")]\r\n", v.ToString(3)));
      FileAppendText("SolutionInfo.cs", string.Format("[assembly: AssemblyFileVersion(\"{0}\")]\r\n", version.FullSemVer));
      FileAppendText("SolutionInfo.cs", string.Format("[assembly: AssemblyInformationalVersion(\"{0}\")]\r\n", version.InformationalVersion));
    }
    Information("AssemblySemFileVer : " + version.AssemblySemFileVer);
    Information("Manifests Version String : " + $"{version.Major.ToString("00")}.{version.Minor.ToString("00")}.{version.Patch.ToString("00")}");
    Information("The full sevVer is : " + version.FullSemVer);
    Information("The build Id is : " + buildId);
});

Task("UpdateDnnManifests")
  .IsDependentOn("GitVersion")
  .DoesForEach(GetFiles("**/*.dnn"), (file) => 
  { 
    var transformFile = File(System.IO.Path.GetTempFileName());
    FileAppendText(transformFile, GetXdtTransformation());
    XdtTransformConfig(file, transformFile, file);
});

public string GetBuildNumber()
{
    return version.LegacySemVerPadded;
}

public string GetProductVersion()
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
             xdt:Locator=""Condition([not(@version)])"" />
  </packages>
</dotnetnuke>";
}

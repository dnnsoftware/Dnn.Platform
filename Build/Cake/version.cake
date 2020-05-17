// These tasks will set the right version for manifests and assemblies. Note you can
// control this by using custom settings

GitVersion version;
var buildId = EnvironmentVariable("BUILD_BUILDID") ?? "0";
var buildNumber = "";
var productVersion = "";

var unversionedManifests = FileReadLines("./Build/Cake/unversionedManifests.txt");

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
      var assemblyInfo = new AssemblyInfo("SolutionInfo.cs");
      var requestedVersion = Settings.Version == "off" ? 
        assemblyInfo.GetVersion():
        new System.Version(Settings.Version);
      version.Major = requestedVersion.Major;
      version.Minor = requestedVersion.Minor;
      version.Patch = requestedVersion.Build;
      version.InformationalVersion = requestedVersion.ToString(3) + " Custom build";
      version.MajorMinorPatch = requestedVersion.ToString(3);
      version.FullSemVer = requestedVersion.ToString(3);
      if (requestedVersion.Revision != -1) {
        version.CommitsSinceVersionSource = requestedVersion.Revision;
        version.InformationalVersion = requestedVersion.ToString(4) + " Custom build";
      }
      buildNumber = requestedVersion.ToString(3);
    }
    Information(Newtonsoft.Json.JsonConvert.SerializeObject(version));
    if (Settings.Version != "off")
      Dnn.CakeUtils.Utilities.UpdateAssemblyInfoVersion(new System.Version(version.Major, version.Minor, version.Patch, version.CommitsSinceVersionSource != null ? (int)version.CommitsSinceVersionSource : 0), version.InformationalVersion, "SolutionInfo.cs");
    Information("Informational Version : " + version.InformationalVersion);
    productVersion = version.MajorMinorPatch;
    Information("Product Version : " + productVersion);
    Information("Build Number : " + buildNumber);
    Information("The build Id is : " + buildId);
});

Task("UpdateDnnManifests")
  .IsDependentOn("SetVersion")
  .IsDependentOn("SetPackageVersions")
  .DoesForEach(GetFilesByPatterns(".", new string[] {"**/*.dnn"}, unversionedManifests), (file) => 
  { 
    if (Settings.Version == "off") return;
    Information("Transforming: " + file);
    var transformFile = File(System.IO.Path.GetTempFileName());
    FileAppendText(transformFile, GetXdtTransformation());
    XdtTransformConfig(file, transformFile, file);
});

Task("SetPackageVersions")
  .IsDependentOn("SetVersion")
  .Does(() => {
    if (Settings.Version == "off") return;
    var packages = GetFiles("./Dnn.AdminExperience/ClientSide/*.Web/package.json");
    packages.Add(GetFiles("./Dnn.AdminExperience/ClientSide/Dnn.React.Common/package.json"));
    packages.Add(GetFiles("./Dnn.AdminExperience/ClientSide/*.Web/**/_exportables/package.json"));

    // Set all package.json in Admin Experience to the current version and to consume the current (local) version of dnn-react-common.
    foreach(var file in packages){
      Information($"Updating {file.ToString()} to version {version.FullSemVer}");
      ReplaceRegexInFiles(file.ToString(), @"""version"": "".*""", $@"""version"": ""{version.FullSemVer}""");
      ReplaceRegexInFiles(file.ToString(), @"""@dnnsoftware\/dnn-react-common"": "".*""", $@"""@dnnsoftware/dnn-react-common"": ""{version.FullSemVer}""");
    }
  });

Task("GenerateChecksum")
.IsDependentOn("SetVersion")
.Does(() => {
  Information("Generating default.aspx checksum...");
  var sourceFile = "./Dnn Platform/Website/Default.aspx";
  var destFile = "./Dnn.AdminExperience/Dnn.PersonaBar.Extensions/Components/Security/Resources/sums.resources";
  var hash = CalculateSha(sourceFile);
  var content = $@"<checksums>
  <sum name=""Default.aspx"" version=""{version.MajorMinorPatch}"" type=""Platform"" sum=""{hash}"" />
</checksums>";
  System.IO.File.WriteAllText(destFile, content);
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

private string GetVersionString()
{
  return $"{version.Major.ToString("00")}.{version.Minor.ToString("00")}.{version.Patch.ToString("00")}";
}

public string GetXdtTransformation()
{
    return $@"<?xml version=""1.0""?>
<dotnetnuke xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
  <packages>
    <package version=""{GetVersionString()}"" 
             xdt:Transform=""SetAttributes(version)"" />
  </packages>
</dotnetnuke>";
}

static string CalculateSha(string filename)
{
    using (var sha = System.Security.Cryptography.SHA256.Create())
    {
        using (var stream = System.IO.File.OpenRead(filename))
        {
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
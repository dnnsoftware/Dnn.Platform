public class LocalSettings {
    public string WebsitePath {get; set;} = "";
    public string WebsiteUrl {get; set;} = "";
    public string SaConnectionString {get; set;} = "server=(local);Trusted_Connection=True;";
    public string DnnConnectionString {get; set;} = "";
    public string DbOwner {get; set;} = "dbo";
    public string ObjectQualifier {get; set;} = "";
    public string DnnDatabaseName {get; set;} = "Dnn_Platform";
    public string DnnSqlUsername {get; set;} = "";
    public string DatabasePath {get; set;} = "";
    public string Version {get; set;} = "auto";
}

LocalSettings Settings;

public void LoadSettings() {
    var settingsFile = "settings.local.json";
    if (System.IO.File.Exists(settingsFile)) {
        Settings = Newtonsoft.Json.JsonConvert.DeserializeObject<LocalSettings>(Utilities.ReadFile(settingsFile));
    } else {
        Settings = new LocalSettings();
    }
    using (var sw = new System.IO.StreamWriter(settingsFile)) {
        sw.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(Settings, Newtonsoft.Json.Formatting.Indented));
    }
}

LoadSettings();

Task("CreateSettings")
    .Does(() =>
	{
		// Doesn't need to do anything as it's done automatically
	});

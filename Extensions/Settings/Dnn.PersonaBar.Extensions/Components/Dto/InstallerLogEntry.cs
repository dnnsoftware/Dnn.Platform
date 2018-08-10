using DotNetNuke.Services.Installer.Log;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class InstallerLogEntry
    {
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
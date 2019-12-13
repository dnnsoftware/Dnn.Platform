#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.ConfigConsole.Services.Dto
{
    [JsonObject]
    public class ConfigFileDto
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("fileContent")]
        public string FileContent { get; set; }
    }
}

#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class ModuleFolderDto
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("isSpecial")]
        public bool IsSpecial { get; set; }

        [JsonProperty("specialType")]
        public string SpecialType { get; set; }
    }
}

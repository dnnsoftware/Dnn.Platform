#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class DeletePackageDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("deleteFiles")]
        public bool DeleteFiles { get; set; }
    }
}

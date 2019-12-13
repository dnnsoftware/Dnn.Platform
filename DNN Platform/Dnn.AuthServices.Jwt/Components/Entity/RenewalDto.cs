using Newtonsoft.Json;

namespace Dnn.AuthServices.Jwt.Components.Entity
{
    [JsonObject]
    public class RenewalDto
    {
        [JsonProperty("rtoken")]
        public string RenewalToken;
    }
}

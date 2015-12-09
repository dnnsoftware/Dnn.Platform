using Newtonsoft.Json;

namespace Dnn.AuthServices.Jwt.Internal
{
    [JsonObject]
    public class LoginResultData
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("sessionToken")]
        public string SessionToken { get; set; }
        [JsonProperty("renewalToken")]
        public string RenewalToken { get; set; }
    }
}

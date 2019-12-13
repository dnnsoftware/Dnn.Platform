using Newtonsoft.Json;

namespace Dnn.AuthServices.Jwt.Components.Entity
{
    /// <summary>
    /// Structure used for the Login to obtain a Json Web Token (JWT).
    /// </summary>
    [JsonObject]
    public struct LoginData
    {
        [JsonProperty("u")]
        public string Username;
        [JsonProperty("p")]
        public string Password;
    }
}

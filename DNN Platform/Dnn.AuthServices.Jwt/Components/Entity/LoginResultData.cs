// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Components.Entity
{
    using Newtonsoft.Json;

    [JsonObject]
    public class LoginResultData
    {
        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("renewalToken")]
        public string RenewalToken { get; set; }

        [JsonIgnore]
        public string Error { get; set; }
    }
}

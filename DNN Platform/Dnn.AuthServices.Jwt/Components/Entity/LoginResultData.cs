// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Newtonsoft.Json;

namespace Dnn.AuthServices.Jwt.Components.Entity
{
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

// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
        [JsonProperty("a")]
        public string Audience;
    }
}

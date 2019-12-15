// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

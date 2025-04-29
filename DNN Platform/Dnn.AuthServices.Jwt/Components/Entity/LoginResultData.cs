// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Components.Entity;

using Newtonsoft.Json;

/// <summary>Represents information about a login result.</summary>
[JsonObject]
public class LoginResultData
{
    /// <summary>Gets or sets the id of the user.</summary>
    [JsonProperty("userId")]
    public int UserId { get; set; }

    /// <summary>Gets or sets the user display name.</summary>
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }

    /// <summary>Gets or sets the access token.</summary>
    [JsonProperty("accessToken")]
    public string AccessToken { get; set; }

    /// <summary>Gets or sets the renewal token.</summary>
    [JsonProperty("renewalToken")]
    public string RenewalToken { get; set; }

    /// <summary>Gets or sets any error message.</summary>
    [JsonIgnore]
    public string Error { get; set; }
}

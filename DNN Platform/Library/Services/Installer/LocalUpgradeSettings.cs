// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer;

using Newtonsoft.Json;

/// <summary>
/// Represents settings for a local upgrade, including minimum DNN version and excluded upgrade files.
/// </summary>
[JsonObject]
public class LocalUpgradeSettings
{
    /// <summary>
    /// Gets or sets the minimum DNN version required for the upgrade.
    /// </summary>
    [JsonProperty("minimumDnnVersion")]
    public string MinimumDnnVersion { get; set; }

    /// <summary>
    /// Gets or sets the list of upgrade exclusions.
    /// </summary>
    [JsonProperty("upgradeExclude")]
    public string[] UpgradeExclude { get; set; }
}

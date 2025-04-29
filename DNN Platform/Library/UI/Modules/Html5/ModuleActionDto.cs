// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules.Html5;

using Newtonsoft.Json;

public class ModuleActionDto
{
    [JsonProperty("controlkey")]
    public string ControlKey { get; set; }

    [JsonProperty("icon")]
    public string Icon { get; set; }

    [JsonProperty("localresourcefile")]
    public string LocalResourceFile { get; set; }

    [JsonProperty("securitysccesslevel")]
    public string SecurityAccessLevel { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("titlekey")]
    public string TitleKey { get; set; }

    [JsonProperty("script")]
    public string Script { get; set; }
}

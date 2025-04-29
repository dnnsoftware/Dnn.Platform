// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens;

using System.Collections.Generic;

using Newtonsoft.Json;

/// <summary>Data-transfer-object for javascript file registration.</summary>
public class JavaScriptDto
{
    /// <summary>Gets or sets the name of an installed javascript library.</summary>
    [JsonProperty("jsname")]
    public string JsName { get; set; }

    /// <summary>Gets or sets the path to the javascript file.</summary>
    [JsonProperty("path")]
    public string Path { get; set; }

    /// <summary>Gets or sets the priority of the javascript file.</summary>
    [JsonProperty("priority")]
    public int Priority { get; set; }

    /// <summary>Gets or sets the name of the provider to use. Examples: <c>DnnPageHeaderProvider</c>, <c>DnnBodyProvider</c>, <c>DnnFormBottomProvider</c>.</summary>
    [JsonProperty("provider")]
    public string Provider { get; set; }

    /// <summary>Gets or sets the specific version to use. Examples: <c>Latest</c>, <c>LatestMajor</c>, <c>LatestMinor</c>, <c>Exact</c>c>.</summary>
    [JsonProperty("specific")]
    public string Specific { get; set; }

    /// <summary>Gets or sets the version number to load.</summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>Gets or sets the HTML attributes to include in the script tag.</summary>
    [JsonProperty("htmlAttributes")]
    public IDictionary<string, string> HtmlAttributes { get; set; }
}

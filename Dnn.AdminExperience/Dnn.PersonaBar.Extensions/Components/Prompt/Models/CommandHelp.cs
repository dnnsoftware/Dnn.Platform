// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models;

using System;
using System.Collections.Generic;

using DotNetNuke.Internal.SourceGenerators;

using Newtonsoft.Json;

[Serializable]
[JsonObject]
[DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project.")]
public partial class CommandHelp
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "description")]
    public string Description { get; set; }

    [JsonProperty(PropertyName = "options")]
    public IEnumerable<CommandOption> Options { get; set; }

    [JsonProperty(PropertyName = "resultHtml")]
    public string ResultHtml { get; set; }

    [JsonProperty(PropertyName = "error")]
    public string Error { get; set; }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models;

using System;

using DotNetNuke.Internal.SourceGenerators;

using Newtonsoft.Json;

[Serializable]
[JsonObject]
[DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project.")]
public partial class Command
{
    public string Key { get; set; }

    public string Name { get; set; }

    public string Category { get; set; }

    public string Description { get; set; }

    public string Version { get; set; }

    [JsonIgnore]
    public Type CommandType { get; set; }
}

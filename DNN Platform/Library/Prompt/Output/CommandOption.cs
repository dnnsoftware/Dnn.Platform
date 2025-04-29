// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt;

using System;

using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;

/// <summary>This is used in the <see cref="CommandHelp"/> to send a list of command parameters to the client for explanatory help.</summary>
[Serializable]
[JsonObject]
public class CommandOption : ICommandOption
{
    /// <summary>Gets or sets name of the parameter.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets a value indicating whether is flag required or not.</summary>
    public bool Required { get; set; }

    /// <summary>Gets or sets default value of the flag.</summary>
    public string DefaultValue { get; set; }

    /// <summary>Gets or sets resource key of the description of flag.</summary>
    public string Description { get; set; }
}

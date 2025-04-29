// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Prompt;

/// <summary>This is used in the <see cref="ICommandHelp"/> to send a list of command parameters to the client for explanatory help.</summary>
public interface ICommandOption
{
    /// <summary>Gets or sets the name of the parameter.</summary>
    string Name { get; set; }

    /// <summary>Gets or sets the description of what this parameter is for.</summary>
    string Description { get; set; }

    /// <summary>Gets or sets the default serialized value if it is specified.</summary>
    string DefaultValue { get; set; }

    /// <summary>Gets or sets a value indicating whether the parameter is required.</summary>
    bool Required { get; set; }
}

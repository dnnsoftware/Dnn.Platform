// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Settings;

/// <summary>The configuration setting.</summary>
public interface IConfigurationSetting
{
    /// <summary>Gets or sets a value indicating whether if the configuration setting is secure.</summary>
    bool IsSecure { get; set; }

    /// <summary>Gets or sets the configuration key.</summary>
    string Key { get; set; }

    /// <summary>Gets or sets the configuraiton value.</summary>
    string Value { get; set; }
}

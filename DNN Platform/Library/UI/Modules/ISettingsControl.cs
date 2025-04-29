// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules;

/// <summary>ISettingsControl provides a common Interface for Module Settings Controls.</summary>
public interface ISettingsControl : IModuleControl
{
    /// <summary>Loads the module settings.</summary>
    void LoadSettings();

    /// <summary>Updates the module settings.</summary>
    void UpdateSettings();
}

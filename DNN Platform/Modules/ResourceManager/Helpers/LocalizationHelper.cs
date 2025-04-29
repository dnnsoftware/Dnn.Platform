// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Helpers;

using Dnn.Modules.ResourceManager.Components;

using DotNetNuke.Services.Localization;

/// <summary>Localization helper methods.</summary>
internal class LocalizationHelper
{
    private const string ResourceFile = "~/" + Constants.ModulePath + "/App_LocalResources/ResourceManager.resx";

    /// <summary>Gets a localized resource string from the resource manager resource file.</summary>
    /// <param name="key">The localization key to get.</param>
    /// <returns>A string containing the localized text.</returns>
    public static string GetString(string key)
    {
        return Localization.GetString(key, ResourceFile);
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Components;

using System.Collections.Generic;

using Dnn.Modules.ResourceManager.Components.Models;

/// <summary>Provides localization services.</summary>
public interface ILocalizationController
{
    /// <summary>Gets the culture name.</summary>
    string CultureName { get; }

    /// <summary>Gets a timestamp for a given resource file.</summary>
    /// <param name="resourceFile">The resource file for which to get the timestamp.</param>
    /// <param name="localization">The localizatio information.</param>
    /// <returns>A long representing the resource file timestamp.</returns>
    long GetResxTimeStamp(string resourceFile, Localization localization);

    /// <summary>Gets a localized dictionary from a resource file.</summary>
    /// <param name="resourceFile">The resource file from which to generate a dictionary.</param>
    /// <param name="culture">The culture to get.</param>
    /// <param name="localization">The localization information.</param>
    /// <returns>
    /// A <see cref="Dictionary{Tkey, TValue}"/> where the key is a string representing
    /// the localization key and the value is a string containing the localized text.
    /// </returns>
    Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture, Localization localization);

    /// <summary>Returns a dictionary of localized keys for a resource file for a given culture.</summary>
    /// <param name="resourceFile">
    /// The relative file path of the main resource file,e.g.
    /// ~/DesktopModules/SocialLibrary/App_LocalResources/CmxResources.resx .
    /// </param>
    /// <param name="culture">The culture for which this dictionay is requested.</param>
    /// <remarks>This API does not fallback to any other DNN resource files.</remarks>
    /// <returns>
    /// A <see cref="Dictionary{TKey, TValue}"/> where the key is a string representing
    /// the localization key and the value is a string containing the localized text.
    /// </returns>
    Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture);
}

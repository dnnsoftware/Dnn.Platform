// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings;

public interface ISettingsRepository<T>
    where T : class
{
    /// <summary>
    /// Retrieve module settings. This can optionally include TabModule settings, Portal settings and
    /// host settings as well. Note the result is cached.
    /// </summary>
    /// <param name="moduleContext">Your module's context.</param>
    /// <returns>Your settings class.</returns>
    T GetSettings(ModuleInfo moduleContext);

    /// <summary>
    /// Retrieve portal/host settings. This will ignore Module and TabModule settings.
    /// Note the result is cached.
    /// </summary>
    /// <param name="portalId">The portal ID for which to retrieve the settings.</param>
    /// <returns>Your settings class.</returns>
    T GetSettings(int portalId);

    /// <summary>
    /// Save your module's settings. This can optionally include TabModule settings, Portal settings and
    /// host settings as well.
    /// </summary>
    /// <param name="moduleContext">Your module's context.</param>
    /// <param name="settings">Your settings class.</param>
    void SaveSettings(ModuleInfo moduleContext, T settings);

    /// <summary>
    /// Save your settings. Module and TabModule settings will be ignored. Only Portal settings and
    /// host settings will be saved.
    /// </summary>
    /// <param name="portalId">Your portal Id for these settings.</param>
    /// <param name="settings">Your settings class.</param>
    void SaveSettings(int portalId, T settings);
}

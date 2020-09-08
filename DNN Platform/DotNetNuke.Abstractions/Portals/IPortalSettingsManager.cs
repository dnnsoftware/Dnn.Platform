// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Portals
{
    using DotNetNuke.Abstractions.Settings;

    /// <summary>
    /// Manages instantiating the correct
    /// <see cref="ISettingsService"/> for adding or updating
    /// Portal Settings.
    /// </summary>
    public interface IPortalSettingsManager
    {
        /// <summary>
        /// Get the Portal Settings for a specific Portal.
        /// </summary>
        /// <param name="portalId">The portalId to retrieve portal settings for.</param>
        /// <returns>The portal settings service implementation.</returns>
        ISettingsService GetPortalSettings(int portalId);

        /// <summary>
        /// Get the Portal Settings for a specific Portal.
        /// </summary>
        /// <param name="portalId">The portalId to retrieve portal settings for.</param>
        /// <param name="cultureCode">The culture code to retrieve the portal settings for.</param>
        /// <returns>The portal settings service implementation.</returns>
        ISettingsService GetPortalSettings(int portalId, string cultureCode);

        /// <summary>
        /// Deletes all portal settings by portal id and for a given
        /// language (Null: all languages and neutral settings).
        /// </summary>
        /// <param name="portalId">The portalId to delete settings for.</param>
        void DeleteAllSettings(int portalId);

        /// <summary>
        /// Deletes all portal settings by portal id and for a given
        /// language (Null: all languages and neutral settings).
        /// </summary>
        /// <param name="portalId">The portalId to delete settings for.</param>
        /// <param name="cultureCode">The culture code to delete settings for.</param>
        void DeleteAllSettings(int portalId, string cultureCode);
    }
}

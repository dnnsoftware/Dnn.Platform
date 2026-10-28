// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.Settings
{
    /// <summary>Reads and writes raw setting values for all sites (host) or for one site (portal).</summary>
    public interface ISettingsStore
    {
        /// <summary>Gets a setting value.</summary>
        /// <param name="portalId">The site's portal ID, or <see cref="EditorSettingsRepository.AllSites"/> for the host setting.</param>
        /// <param name="key">The setting name.</param>
        /// <returns>The value, or <see langword="null"/> or empty if it is not set.</returns>
        string Get(int portalId, string key);

        /// <summary>Sets a setting value; an empty value clears the setting.</summary>
        /// <param name="portalId">The site's portal ID, or <see cref="EditorSettingsRepository.AllSites"/> for the host setting.</param>
        /// <param name="key">The setting name.</param>
        /// <param name="value">The value.</param>
        void Set(int portalId, string key, string value);
    }
}

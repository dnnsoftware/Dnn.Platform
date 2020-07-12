// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Controllers
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface of HostController.
    /// </summary>
    /// <example>
    /// <code lang="C#">
    /// public static bool CheckUpgrade
    /// {
    ///     get
    ///     {
    ///         return HostController.Instance.GetBoolean("CheckUpgrade", true);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="HostController"/>
    public interface IHostController
    {
        /// <summary>
        /// Gets the setting value for the specific key.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <returns>host setting as a boolean.</returns>
        bool GetBoolean(string key);

        /// <summary>
        /// Gets the setting value for the specific key.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <param name="defaultValue">Default value returned if the setting is not found or not compatible with the requested type.</param>
        /// <returns>host setting or the provided default value as a <see cref="bool"/>.</returns>
        bool GetBoolean(string key, bool defaultValue);

        /// <summary>
        /// Gets the setting value for the specific key.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <param name="defaultValue">Default value returned if the setting is not found or not compatible with the requested type.</param>
        /// <returns>Host setting or the provided default value as a <see cref="double"/>.</returns>
        double GetDouble(string key, double defaultValue);

        /// <summary>
        /// Gets the setting value for the specific key.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <returns>Host setting as a <see cref="double"/>.</returns>
        double GetDouble(string key);

        /// <summary>
        /// Gets the setting value for the specific key.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <returns>Host setting as an <see cref="int"/>.</returns>
        int GetInteger(string key);

        /// <summary>
        /// Gets the setting value for the specific key.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <param name="defaultValue">Default value returned if the setting is not found or not compatible with the requested type.</param>
        /// <returns>Host setting or provided default value as a <see cref="int"/>.</returns>
        int GetInteger(string key, int defaultValue);

        /// <summary>
        /// Gets the host settings.
        /// </summary>
        /// <returns>Host settings as a <see cref="Dictionary{TKey, TValue}"/>.</returns>
        Dictionary<string, ConfigurationSetting> GetSettings();

        /// <summary>
        /// Gets the host settings.
        /// </summary>
        /// <returns>Host settings as a <see cref="Dictionary{TKey, TValue}"/>.</returns>
        Dictionary<string, string> GetSettingsDictionary();

        /// <summary>
        /// Gets an encrypted host setting as a <see cref="string"/>.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <param name="passPhrase">The passPhrase used to decrypt the setting value.</param>
        /// <returns>The setting value as a <see cref="string"/>.</returns>
        string GetEncryptedString(string key, string passPhrase);

        /// <summary>
        /// Gets the setting value for a specific key.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <returns>The setting value as a <see cref="string"/>.</returns>
        string GetString(string key);

        /// <summary>
        /// Gets the setting value for a specific key.
        /// </summary>
        /// <param name="key">The seeting key string.</param>
        /// <param name="defaultValue"></param>
        /// <returns>Default value returned if the setting is not found.</returns>
        string GetString(string key, string defaultValue);

        /// <summary>
        /// Updates the specified settings.
        /// </summary>
        /// <param name="settings">The settings to update.</param>
        void Update(Dictionary<string, string> settings);

        /// <summary>
        /// Updates the specified config.
        /// </summary>
        /// <param name="config">The configuration setting.</param>
        void Update(ConfigurationSetting config);

        /// <summary>
        /// Updates the specified config, ontionally clearing the cache.
        /// </summary>
        /// <param name="config">The configuaration setting.</param>
        /// <param name="clearCache">If set to <c>true</c>, will clear the cache after updating the setting.</param>
        void Update(ConfigurationSetting config, bool clearCache);

        /// <summary>
        /// Updates the setting for a specific key.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <param name="value">The value to update.</param>
        /// <param name="clearCache">If set to <c>true</c>, will clear the cache after updating the setting.</param>
        void Update(string key, string value, bool clearCache);

        /// <summary>
        /// Updates the setting for a specific key.
        /// </summary>
        /// <param name="key">The setting key string.</param>
        /// <param name="value">The value to update.</param>
        void Update(string key, string value);

        /// <summary>
        /// Takes in a <see cref="string"/> value, encrypts it with a FIPS compliant algorithm and stores.
        /// </summary>
        /// <param name="key">host settings key.</param>
        /// <param name="value">host settings value.</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption.</param>
        void UpdateEncryptedString(string key, string value, string passPhrase);

        /// <summary>
        /// Increments the Client Resource Manager (CRM) version to bust local cache.
        /// </summary>
        /// <param name="includeOverridingPortals">If true also forces a CRM version increment on portals that have non-default settings for CRM.</param>
        void IncrementCrmVersion(bool includeOverridingPortals);
    }
}

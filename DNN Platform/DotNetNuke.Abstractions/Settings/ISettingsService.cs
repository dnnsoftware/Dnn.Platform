// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Settings
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The settings service defines APIs for
    /// retrieving or saving standard DNN settings.
    /// </summary>
    public interface ISettingsService
    {
        string this[string key] { get; set; }

        T Get<T>(string key)
            where T : IConvertible;

        T GetEncrypted<T>(string key)
            where T : IConvertible;

        T GetEncrypted<T>(string key, string passPhrase)
            where T : IConvertible;

        string GetEncrypted(string key);

        string GetEncrypted(string key, string passPhrase);

        /// <summary>
        /// Deletes the setting.
        /// </summary>
        /// <param name="key">The setting key to delete.</param>
        void Delete(string key);

        /// <summary>
        /// Deletes the setting.
        /// </summary>
        /// <param name="key">The setting key to delete.</param>
        /// <param name="clearCache">Clears the cache if true.</param>
        void Delete(string key, bool clearCache);

        void DeleteAll();

        void DeleteAll(bool clearCache);

        /// <summary>
        /// Updates the setting for a specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void Update(string key, string value);

        void UpdateEncrypted(string key, string value);
        void UpdateEncrypted(string key, string value, string passPhrase);

        void Update(string key, string value, bool clearCache);
        void UpdateEncrypted(string key, string value, bool clearCache);
        void UpdateEncrypted(string key, string value, string passPhrase, bool clearCache);
    }
}

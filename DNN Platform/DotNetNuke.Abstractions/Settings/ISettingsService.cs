// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Settings
{
    using System;

    /// <summary>
    /// The settings service defines APIs for
    /// retrieving or saving standard DNN settings.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets or sets the setting key value pair via string indexer.
        /// </summary>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The value of the indexer.</returns>
        string this[string key] { get; set; }

        /// <summary>
        /// Gets the setting value converted to the generic type.
        /// </summary>
        /// <typeparam name="T">
        /// The type to convert the retrieved value to.
        /// This should implement <see cref="IConvertible"/>.
        /// </typeparam>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The value of the retrieved key.</returns>
        T Get<T>(string key)
            where T : IConvertible;

        /// <summary>
        /// Gets the encrypted value converted to the generic type.
        /// </summary>
        /// <typeparam name="T">
        /// The type to convert the retrieved value to.
        /// This should implement <see cref="IConvertible"/>.
        /// </typeparam>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The decrypted value of the retrieved key.</returns>
        T GetEncrypted<T>(string key)
            where T : IConvertible;

        /// <summary>
        /// Gets the encrypted value converted to the generic type.
        /// </summary>
        /// <typeparam name="T">
        /// The type to convert the retrieved value to.
        /// This should implement <see cref="IConvertible"/>.
        /// </typeparam>
        /// <param name="key">The key to retrieve.</param>
        /// <param name="passPhrase">The passpPhrase to use for decrypting the value.</param>
        /// <returns>The decrypted value of the retrieved key.</returns>
        T GetEncrypted<T>(string key, string passPhrase)
            where T : IConvertible;

        /// <summary>
        /// Gets the encrypted value as a string.
        /// </summary>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The decrypted value of the retrieved key.</returns>
        string GetEncrypted(string key);

        /// <summary>
        /// Gets the encrypted value as a string.
        /// </summary>
        /// <param name="key">The key to retrieve.</param>
        /// <param name="passPhrase">The passpPhrase to use for decrypting the value.</param>
        /// <returns>The decrypted value of the retrieved key.</returns>
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

        /// <summary>
        /// Deletes all settings and clears the cache.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Deletes all settings.
        /// </summary>
        /// <param name="clearCache">If true the cache will be cleared.</param>
        void DeleteAll(bool clearCache);

        /// <summary>
        /// Adds or updates the setting for a specified key.
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The value to set.</param>
        void Update(string key, string value);

        /// <summary>
        /// Adds or updates the encrypted setting for the
        /// specified key using the default encryption key.
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The unencrypted setting value to set.</param>
        void UpdateEncrypted(string key, string value);

        /// <summary>
        /// Adds or updates the encrypted setting for the
        /// specified key using the default encryption key.
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The unencrypted setting value to set.</param>
        /// <param name="passPhrase">The encryptiong key or pass pharse to use for this setting.</param>
        void UpdateEncrypted(string key, string value, string passPhrase);

        /// <summary>
        /// Adds or updates the setting for a specified key.
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="clearCache">If true the cache will be cleared.</param>
        void Update(string key, string value, bool clearCache);

        /// <summary>
        /// Adds or updates the encrypted setting for the
        /// specified key using the default encryption key.
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The unencrypted setting value to set.</param>
        /// <param name="clearCache">If true the cache will be cleared.</param>
        void UpdateEncrypted(string key, string value, bool clearCache);

        /// <summary>
        /// Adds or updates the encrypted setting for the
        /// specified key using the default encryption key.
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The unencrypted setting value to set.</param>
        /// <param name="passPhrase">The encryptiong key or pass pharse to use for this setting.</param>
        /// <param name="clearCache">If true the cache will be cleared.</param>
        void UpdateEncrypted(string key, string value, string passPhrase, bool clearCache);
    }
}

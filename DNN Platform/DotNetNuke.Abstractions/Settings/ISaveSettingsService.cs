// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Settings
{
    /// <summary>
    /// The Save Settings service is used by implementations of
    /// <see cref="ISettingsService"/>. It is not currently designed
    /// to be injected via Dependency Injection. Please use the
    /// <see cref="ISettingsService"/>.
    /// </summary>
    public interface ISaveSettingsService
    {
        /// <summary>
        /// Update the current setting with a new value. If there is no
        /// setting key found it will create a new setting.
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The new setting value.</param>
        /// <param name="clearCache">If true the cache will be cleared.</param>
        void Update(string key, string value, bool clearCache);

        /// <summary>
        /// Update the current setting with a new encrypted value.
        /// If there is no setting key found it will create a
        /// new setting.
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The new setting value.</param>
        /// <param name="passPhrase">The encryption salt key or pass phrase.</param>
        /// <param name="clearCache">If true the cache will be cleared.</param>
        void UpdateEncrypted(string key, string value, string passPhrase, bool clearCache);

        /// <summary>
        /// Update the current setting with a new encrypted value.
        /// If there is no setting key found it will create a
        /// new setting. This function will use the default encryption
        /// salt key or (pass phrase).
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The new setting value.</param>
        /// <param name="clearCache">If true the cache will be cleared.</param>
        void UpdateEncrypted(string key, string value, bool clearCache);
    }
}

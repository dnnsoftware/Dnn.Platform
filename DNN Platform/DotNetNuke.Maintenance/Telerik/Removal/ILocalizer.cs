// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    /// <summary>Utility interface to support simple localization.</summary>
    internal interface ILocalizer
    {
        /// <summary>
        /// Looks for the specified key in the resources file and
        /// returns the translated value if found, or null otherwise.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The localized string associated to the provided key.</returns>
        string Localize(string key);

        /// <summary>
        /// Looks for the specified key in the resources file.
        /// If the key is found, it returns the translated value
        /// after filling the placeholders in it with the supplied arguments.
        /// If the key is not found, this method returns null.
        /// </summary>
        /// <param name="formatKey">
        /// The key to search for, which should yield a string with format placeholders in it.
        /// </param>
        /// <param name="args">An array of arguments to fill the format placeholders with.</param>
        /// <returns>
        /// The localized string associated with the provided key,
        /// after filling all placeholders in it with the supplied arguments.
        /// </returns>
        string LocalizeFormat(string formatKey, params object[] args);
    }
}

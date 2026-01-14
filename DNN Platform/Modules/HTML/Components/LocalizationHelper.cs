// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// Provides helper methods for retrieving localization values for HTML module content workflow states.
    /// </summary>
    public class LocalizationHelper
    {
        /// <summary>
        /// Returns all possible localization values for a given key across all available languages.
        /// </summary>
        /// <param name="key">The content workflow state key to localize.</param>
        /// <returns>A list of localized values for the specified key in all available languages.</returns>
        public List<string> StateLocalizations(string key)
        {
            return this.GetLanguages().Select(language => Localization.GetString(key, Localization.GlobalResourceFile, language.Key)).ToList();
        }

        /// <summary>
        /// Gets a dictionary of all available languages in the system, keyed by culture code.
        /// </summary>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> where the key is the culture code and the value is a <see cref="Locale"/> object.
        /// </returns>
        private Dictionary<string, Locale> GetLanguages()
        {
            return CBO.FillDictionary("CultureCode", DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetLanguages"), new Dictionary<string, Locale>(StringComparer.OrdinalIgnoreCase));
        }
    }
}

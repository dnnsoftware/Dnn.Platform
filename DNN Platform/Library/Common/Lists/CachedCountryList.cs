// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Lists
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;

    /// <summary>Provides access to country list with caching.</summary>
    [Serializable]
    public class CachedCountryList : Dictionary<string, CachedCountryList.Country>
    {
        /// <summary>Initializes a new instance of the <see cref="CachedCountryList"/> class.</summary>
        /// <param name="locale">This value is not currently used.</param>
        public CachedCountryList(string locale)
            : base()
        {
            // TODO: locale is unused here, this as it stands is not localizable. See https://www.dnnsoftware.com/community-blog/cid/155072/new-list-localization-in-dnn-733
            foreach (ListEntryInfo li in new ListController().GetListEntryInfoItems("Country"))
            {
                string text = li.Text;
                Country c = new Country
                {
                    Id = li.EntryID,
                    Code = li.Value,
                    FullName = $"{text} ({li.Value})",
                    Name = text,
                };
                c.NormalizedFullName = c.FullName.NormalizeString();
                this.Add(li.Value, c);
            }
        }

        /// <summary>Gets the country list.</summary>
        /// <param name="locale">Which locale to use for the country names.</param>
        /// <returns>A cached list of countries.</returns>
        public static CachedCountryList GetCountryList(string locale)
        {
            CachedCountryList res = null;
            try
            {
                res = (CachedCountryList)DotNetNuke.Common.Utilities.DataCache.GetCache(CacheKey(locale));
            }
            catch (Exception)
            {
                // do nothing here.
            }

            if (res == null)
            {
                res = new CachedCountryList(locale);
                DotNetNuke.Common.Utilities.DataCache.SetCache(CacheKey(locale), res);
            }

            return res;
        }

        /// <summary>Gets the cache key for a country list in the specified culture.</summary>
        /// <param name="locale">The locale to use for the country names.</param>
        /// <returns>The cache key string.</returns>
        public static string CacheKey(string locale)
        {
            return $"CountryList:{locale}";
        }

        /// <summary>Represents a country.</summary>
        [Serializable]
        public struct Country
        {
            /// <summary>The country id.</summary>
            public int Id;

            /// <summary>The country name.</summary>
            /// <example>United States.</example>
            public string Name;

            /// <summary>The country code.</summary>
            /// <example>US.</example>
            public string Code;

            /// <summary>The country name and code.</summary>
            /// <example>United States (US).</example>
            public string FullName;

            /// <summary>The country name and code with diacritics (accents) removed.</summary>
            public string NormalizedFullName;
        }
    }
}

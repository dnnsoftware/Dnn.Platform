// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Lists
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common.Utilities;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides access to country list with caching.</summary>
    [Serializable]
    public class CachedCountryList : Dictionary<string, CachedCountryList.Country>
    {
        /// <summary>Initializes a new instance of the <see cref="CachedCountryList"/> class.</summary>
        /// <param name="locale">This value is not currently used.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public CachedCountryList(string locale)
            : this(Globals.GetCurrentServiceProvider().GetRequiredService<ListController>())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CachedCountryList"/> class.</summary>
        /// <param name="listController">The list controller.</param>
        public CachedCountryList(ListController listController)
        {
            // TODO: locale is unused here, this as it stands is not localizable. See https://www.dnnsoftware.com/community-blog/cid/155072/new-list-localization-in-dnn-733
            foreach (ListEntryInfo li in listController.GetListEntryInfoItems("Country"))
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
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public static CachedCountryList GetCountryList(string locale)
            => GetCountryList(Globals.GetCurrentServiceProvider().GetRequiredService<ListController>());

        /// <summary>Gets the country list.</summary>
        /// <param name="listController">The list controller.</param>
        /// <returns>A cached list of countries.</returns>
        public static CachedCountryList GetCountryList(ListController listController)
        {
            CachedCountryList res = null;
            try
            {
                res = (CachedCountryList)DataCache.GetCache(CacheKey("none"));
            }
            catch (Exception)
            {
                // do nothing here.
            }

            if (res == null)
            {
                res = new CachedCountryList(listController);
                DataCache.SetCache(CacheKey("none"), res);
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
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public int Id;

            /// <summary>The country name.</summary>
            /// <example>United States.</example>
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string Name;

            /// <summary>The country code.</summary>
            /// <example>US.</example>
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string Code;

            /// <summary>The country name and code.</summary>
            /// <example>United States (US).</example>
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string FullName;

            /// <summary>The country name and code with diacritics (accents) removed.</summary>
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string NormalizedFullName;
        }
    }
}

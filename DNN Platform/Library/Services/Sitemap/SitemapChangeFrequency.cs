// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Sitemap
{
    /// <summary>How frequently a page is likely to change.</summary>
    /// <seealso href="https://www.sitemaps.org/protocol.html#changefreqdef" />
    public enum SitemapChangeFrequency
    {
        /// <summary>A document that changes each time it is accessed.</summary>
        Always = 0,

        /// <summary>A page likely to change every hour.</summary>
        Hourly = 1,

        /// <summary>A page likely to change every day.</summary>
        Daily = 2,

        /// <summary>A page likely to change every week.</summary>
        Weekly = 3,

        /// <summary>A page likely to change every month.</summary>
        Monthly = 4,

        /// <summary>A page likely to change every year.</summary>
        Yearly = 5,

        /// <summary>An archived URL that never changes.</summary>
        Never = 6,
    }
}

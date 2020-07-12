// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Entities.Portals;

    public interface ILocalization
    {
        /// <summary>
        /// Inspect the browser supplied language headers and find the best match from the supplied list.
        /// </summary>
        /// <param name="cultureCodes">The codes to search for a match.</param>
        /// <param name="fallback">The code to return if no good match is found.</param>
        /// <returns>Best matching culture code, or fallback if no good match found.</returns>
        string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes, string fallback);

        /// <summary>
        /// Inspect the browser supplied language headers and find the best match from the supplied list.
        /// </summary>
        /// <param name="cultureCodes">The codes to search for a match.</param>
        /// <returns>Best matching culture code, or SystemLocale if no good match found.</returns>
        string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes);

        /// <summary>
        /// Detects the current language for the request.
        /// The order in which the language is being detect is:
        ///         1. QueryString
        ///         2. Cookie
        ///         3. User profile (if request is authenticated)
        ///         4. Browser preference (if portal has this option enabled)
        ///         5. Portal default
        ///         6. System default (en-US)
        ///     At any point, if a valid language is detected nothing else should be done.
        /// </summary>
        /// <param name="portalSettings">Current PortalSettings.</param>
        /// <returns>A valid CultureInfo.</returns>
        CultureInfo GetPageLocale(PortalSettings portalSettings);

        /// <summary>
        ///   Sets the culture codes on the current Thread.
        /// </summary>
        /// <param name = "cultureInfo">Culture Info for the current page.</param>
        /// <param name = "portalSettings">The current portalSettings.</param>
        /// <remarks>
        ///   This method will configure the Thread culture codes.  Any page which does not derive from PageBase should
        ///   be sure to call this method in OnInit to ensure localiztion works correctly.  See the TelerikDialogHandler for an example.
        /// </remarks>
        void SetThreadCultures(CultureInfo cultureInfo, PortalSettings portalSettings);
    }
}

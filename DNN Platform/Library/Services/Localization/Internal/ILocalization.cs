#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Services.Localization.Internal
{
    public interface ILocalization
    {
        /// <summary>
        /// Inspect the browser supplied language headers and find the best match from the supplied list
        /// </summary>
        /// <param name="cultureCodes">The codes to search for a match</param>
        /// <param name="fallback">The code to return if no good match is found</param>
        /// <returns>Best matching culture code, or fallback if no good match found</returns>
        string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes, string fallback);

        /// <summary>
        /// Inspect the browser supplied language headers and find the best match from the supplied list
        /// </summary>
        /// <param name="cultureCodes">The codes to search for a match</param>
        /// <returns>Best matching culture code, or SystemLocale if no good match found</returns>
        string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes);

        /// <summary>
        /// Detects the current language for the request.
        /// The order in which the language is being detect is:
        ///		1. QueryString
        ///		2. Cookie
        ///		3. User profile (if request is authenticated)
        ///		4. Browser preference (if portal has this option enabled)
        ///		5. Portal default
        ///		6. System default (en-US)
        ///	At any point, if a valid language is detected nothing else should be done
        /// </summary>
        /// <param name="portalSettings">Current PortalSettings</param>
        /// <returns>A valid CultureInfo</returns>
        CultureInfo GetPageLocale(PortalSettings portalSettings);

        /// <summary>
        ///   Sets the culture codes on the current Thread
        /// </summary>
        /// <param name = "cultureInfo">Culture Info for the current page</param>
        /// <param name = "portalSettings">The current portalSettings</param>
        /// <remarks>
        ///   This method will configure the Thread culture codes.  Any page which does not derive from PageBase should
        ///   be sure to call this method in OnInit to ensure localiztion works correctly.
        /// </remarks>
        void SetThreadCultures(CultureInfo cultureInfo, PortalSettings portalSettings);
    }
}
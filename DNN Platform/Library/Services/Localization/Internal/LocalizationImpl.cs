#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Services.Localization.Internal
{
    internal class LocalizationImpl : ILocalization
    {
        public string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes, string fallback)
        {
            if(cultureCodes == null)
            {
                throw new ArgumentException("cultureCodes cannot be null");
            }

            if (fallback == null)
            {
                throw new ArgumentException("fallback cannot be null");
            }

            var values = cultureCodes.ToList();
            
            foreach (string langHeader in HttpContextSource.Current.Request.UserLanguages ?? new string[0])
            {
                string lang = langHeader;
                //strip any ;q=xx
                lang = lang.Split(';')[0];

                //check for exact match e.g. de-DE == de-DE
                if (lang.Contains('-'))
                {
                    var match = values.FirstOrDefault(x => x == lang);
                    if(match != null)
                    {
                        return match;
                    }
                }

                //only keep the initial language value
                if (lang.Length > 1)
                {
                    lang = lang.Substring(0, 2);

                    //check for language match e.g. en-GB == en-US because en == en
                    var match = values.FirstOrDefault(x => x.StartsWith(lang));
                    if (match != null)
                    {
                        return match;
                    }
                }
            }

            return fallback;
        }

        public string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes)
        {
            return BestCultureCodeBasedOnBrowserLanguages(cultureCodes, Localization.SystemLocale);
        }

        public CultureInfo GetPageLocale(PortalSettings portalSettings)
        {
            return Localization.GetPageLocale(portalSettings);
        }

        public void SetThreadCultures(CultureInfo cultureInfo, PortalSettings portalSettings)
        {
            Localization.SetThreadCultures(cultureInfo, portalSettings);
        }
    }
}
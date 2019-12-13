﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

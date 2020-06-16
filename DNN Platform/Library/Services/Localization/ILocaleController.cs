// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System.Collections.Generic;
    using System.Globalization;

    public interface ILocaleController
    {
        bool CanDeleteLanguage(int languageId);

        List<CultureInfo> GetCultures(Dictionary<string, Locale> locales);

        Locale GetCurrentLocale(int PortalId);

        Locale GetDefaultLocale(int portalId);

        Locale GetLocale(string code);

        Locale GetLocale(int portalID, string code);

        Locale GetLocale(int languageID);

        Locale GetLocaleOrCurrent(int portalID, string code);

        Dictionary<string, Locale> GetLocales(int portalID);

        Dictionary<string, Locale> GetPublishedLocales(int portalID);

        bool IsEnabled(ref string localeCode, int portalId);

        void UpdatePortalLocale(Locale locale);

        bool IsDefaultLanguage(string code);

        void ActivateLanguage(int portalid, string cultureCode, bool publish);

        void PublishLanguage(int portalid, string cultureCode, bool publish);
    }
}

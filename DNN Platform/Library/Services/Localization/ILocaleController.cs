#region Usings

using System.Collections.Generic;
using System.Globalization;

#endregion

namespace DotNetNuke.Services.Localization
{
    public interface ILocaleController
    {
        bool CanDeleteLanguage(int languageId);

        List<CultureInfo> GetCultures(Dictionary<string, Locale> locales);

        Locale GetCurrentLocale(int PortalId);

        Locale GetDefaultLocale(int portalId);

        Locale GetLocale(string code);

        Locale GetLocale(int portalID, string code);

        Locale GetLocale(int languageID);

        Dictionary<string, Locale> GetLocales(int portalID);

        Dictionary<string, Locale> GetPublishedLocales(int portalID);

        bool IsEnabled(ref string localeCode, int portalId);

        void UpdatePortalLocale(Locale locale);

        bool IsDefaultLanguage(string code);

        void ActivateLanguage(int portalid, string cultureCode, bool publish);

        void PublishLanguage(int portalid, string cultureCode, bool publish);
    }
}

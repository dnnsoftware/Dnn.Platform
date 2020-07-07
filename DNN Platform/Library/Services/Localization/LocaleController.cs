// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>
    /// LocaleContrller provides method to manage all pages with localization content.
    /// </summary>
    /// <remarks>
    /// Content localization in DotNetNuke will allow you to easily manage your web pages in a primary language
    /// and then utilize translators to keep the content synchronized in multiple secondary languages.
    /// Whether you are maintaining your site in a single language or dozens of languages, the content localization system
    /// will help guide your content editors and translators through the process.  Although content localization required
    /// extensive changes to the core platform, we have been able to add this new feature while still improving overall system performance.
    /// </remarks>
    public class LocaleController : ComponentBase<ILocaleController, LocaleController>, ILocaleController
    {
        public static bool IsValidCultureName(string name)
        {
            return
                CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Any(c => c.Name == name);
        }

        /// <summary>
        /// Determines whether the language can be delete.
        /// </summary>
        /// <param name="languageId">The language id.</param>
        /// <returns>
        ///   <c>true</c> if the language can be delete; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDeleteLanguage(int languageId)
        {
            return PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType.Equals("CoreLanguagePack", StringComparison.OrdinalIgnoreCase))
                        .Select(package => LanguagePackController.GetLanguagePackByPackage(package.PackageID))
                        .All(languagePack => languagePack.LanguageID != languageId);
        }

        /// <summary>
        /// Gets the cultures from local list.
        /// </summary>
        /// <param name="locales">The locales.</param>
        /// <returns>culture list.</returns>
        public List<CultureInfo> GetCultures(Dictionary<string, Locale> locales)
        {
            return locales.Values.Select(locale => new CultureInfo(locale.Code)).ToList();
        }

        /// <summary>
        /// Gets the current locale for current request to the portal.
        /// </summary>
        /// <param name="PortalId">The portal id.</param>
        /// <returns></returns>
        public Locale GetCurrentLocale(int PortalId)
        {
            Locale locale = null;

            if (HttpContext.Current != null && !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["language"]))
            {
                locale = this.GetLocale(HttpContext.Current.Request.QueryString["language"]);
            }

            return locale ?? ((PortalId == Null.NullInteger)
                                ? this.GetLocale(Localization.SystemLocale)
                                : this.GetDefaultLocale(PortalId));
        }

        /// <summary>
        /// Gets the default locale of the portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns></returns>
        public Locale GetDefaultLocale(int portalId)
        {
            var portal = PortalController.Instance.GetPortal(portalId);
            Locale locale = null;
            if (portal != null)
            {
                Dictionary<string, Locale> locales = this.GetLocales(portal.PortalID);
                if (locales != null && locales.ContainsKey(portal.DefaultLanguage))
                {
                    locale = locales[portal.DefaultLanguage];
                }
            }

            return locale ?? this.GetLocale(Localization.SystemLocale);
        }

        /// <summary>
        /// Gets the locale by code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Locale GetLocale(string code)
        {
            return this.GetLocale(Null.NullInteger, code);
        }

        /// <summary>
        /// Gets the locale included in the portal.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Locale GetLocale(int portalID, string code)
        {
            Dictionary<string, Locale> dicLocales = this.GetLocales(portalID);
            Locale locale = null;

            if (dicLocales != null)
            {
                dicLocales.TryGetValue(code, out locale);
            }

            return locale;
        }

        /// <summary>
        /// Gets the locale included in the portal if culture code is not null or empty
        /// or else gets the current locale for current request to the portal.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Locale GetLocaleOrCurrent(int portalID, string code)
        {
            return string.IsNullOrEmpty(code)
                       ? LocaleController.Instance.GetCurrentLocale(portalID) : LocaleController.Instance.GetLocale(portalID, code);
        }

        /// <summary>
        /// Gets the locale.
        /// </summary>
        /// <param name="languageID">The language ID.</param>
        /// <returns></returns>
        public Locale GetLocale(int languageID)
        {
            Dictionary<string, Locale> dicLocales = this.GetLocales(Null.NullInteger);

            return (from kvp in dicLocales where kvp.Value.LanguageId == languageID select kvp.Value).FirstOrDefault();
        }

        /// <summary>
        /// Gets the locales.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns></returns>
        public Dictionary<string, Locale> GetLocales(int portalID)
        {
            if (Globals.Status != Globals.UpgradeStatus.Error)
            {
                Dictionary<string, Locale> locales;
                if (Globals.Status != Globals.UpgradeStatus.Install)
                {
                    string cacheKey = string.Format(DataCache.LocalesCacheKey, portalID);
                    locales = CBO.GetCachedObject<Dictionary<string, Locale>>(new CacheItemArgs(cacheKey, DataCache.LocalesCacheTimeOut, DataCache.LocalesCachePriority, portalID), GetLocalesCallBack, true);
                }
                else
                {
                    locales = CBO.FillDictionary("CultureCode", DataProvider.Instance().GetLanguages(), new Dictionary<string, Locale>(StringComparer.OrdinalIgnoreCase));
                }

                return locales;
            }

            return null;
        }

        /// <summary>
        /// Gets the published locales.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns></returns>
        public Dictionary<string, Locale> GetPublishedLocales(int portalID)
        {
            return this.GetLocales(portalID).Where(kvp => kvp.Value.IsPublished).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Determines whether the specified locale code is enabled.
        /// </summary>
        /// <param name="localeCode">The locale code.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns>
        ///   <c>true</c> if the specified locale code is enabled; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEnabled(ref string localeCode, int portalId)
        {
            try
            {
                bool enabled = false;
                Dictionary<string, Locale> dicLocales = this.GetLocales(portalId);

                // if ((!dicLocales.ContainsKey(localeCode)))
                string locale = localeCode;
                if (dicLocales.FirstOrDefault(x => x.Key.ToLower() == locale.ToLower()).Key == null)
                {
                    // if localecode is neutral (en, es,...) try to find a locale that has the same language
                    if (localeCode.IndexOf("-", StringComparison.Ordinal) == -1)
                    {
                        foreach (string strLocale in dicLocales.Keys)
                        {
                            if (strLocale.Split('-')[0].ToLower() == localeCode.ToLower())
                            {
                                // set the requested _localecode to the full locale
                                localeCode = strLocale;
                                enabled = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    enabled = true;
                }

                return enabled;
            }
            catch (Exception ex)
            {
                // item could not be retrieved  or error
                Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Updates the portal locale.
        /// </summary>
        /// <param name="locale">The locale.</param>
        public void UpdatePortalLocale(Locale locale)
        {
            DataProvider.Instance().UpdatePortalLanguage(locale.PortalId, locale.LanguageId, locale.IsPublished, UserController.Instance.GetCurrentUserInfo().UserID);
            DataCache.RemoveCache(string.Format(DataCache.LocalesCacheKey, locale.PortalId));
        }

        /// <summary>
        /// Determines the language whether is default language.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>
        ///   <c>true</c> if the language is default language; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDefaultLanguage(string code)
        {
            bool returnValue = code == PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage;
            return returnValue;
        }

        /// <summary>
        /// Activates the language without publishing it.
        /// </summary>
        /// <param name="portalid">The portalid.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="publish">if set to <c>true</c> will publishthe language.</param>
        public void ActivateLanguage(int portalid, string cultureCode, bool publish)
        {
            Dictionary<string, Locale> enabledLanguages = Instance.GetLocales(portalid);
            Locale enabledlanguage;
            if (enabledLanguages.TryGetValue(cultureCode, out enabledlanguage))
            {
                enabledlanguage.IsPublished = publish;
                Instance.UpdatePortalLocale(enabledlanguage);
            }
        }

        /// <summary>
        /// Publishes the language.
        /// </summary>
        /// <param name="portalid">The portalid.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="publish">if set to <c>true</c> will publishthe language.</param>
        public void PublishLanguage(int portalid, string cultureCode, bool publish)
        {
            Dictionary<string, Locale> enabledLanguages = Instance.GetLocales(portalid);
            Locale enabledlanguage;
            if (enabledLanguages.TryGetValue(cultureCode, out enabledlanguage))
            {
                enabledlanguage.IsPublished = publish;
                Instance.UpdatePortalLocale(enabledlanguage);
                if (publish)
                {
                    // only publish tabs if we actually need to do that
                    // we cannot "unpublish"
                    TabController.Instance.PublishTabs(TabController.GetTabsBySortOrder(portalid, cultureCode, false));
                }
            }
        }

        private static object GetLocalesCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            Dictionary<string, Locale> locales = CBO.FillDictionary("CultureCode", portalID > Null.NullInteger
                                                                       ? DataProvider.Instance().GetLanguagesByPortal(portalID)
                                                                       : DataProvider.Instance().GetLanguages(), new Dictionary<string, Locale>(StringComparer.OrdinalIgnoreCase));
            return locales;
        }
    }
}

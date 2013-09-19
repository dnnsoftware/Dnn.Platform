#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
#region Usings

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

#endregion

namespace DotNetNuke.Services.Localization
{
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
        #region Private Shared Methods

        private static object GetLocalesCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            Dictionary<string, Locale> locales = CBO.FillDictionary("CultureCode", portalID > Null.NullInteger
                                                                       ? DataProvider.Instance().GetLanguagesByPortal(portalID)
                                                                       : DataProvider.Instance().GetLanguages(), new Dictionary<string, Locale>());
            return locales;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether the language can be delete.
        /// </summary>
        /// <param name="languageId">The language id.</param>
        /// <returns>
        ///   <c>true</c> if the language can be delete; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDeleteLanguage(int languageId)
        {
            return PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "CoreLanguagePack")
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
                locale = GetLocale(HttpContext.Current.Request.QueryString["language"]);
            }
            return locale ?? ((PortalId == Null.NullInteger)
                                ? GetLocale(Localization.SystemLocale)
                                : GetDefaultLocale(PortalId));
        }

        /// <summary>
        /// Gets the default locale of the portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns></returns>
        public Locale GetDefaultLocale(int portalId)
        {
            PortalInfo portal = new PortalController().GetPortal(portalId);
            Locale locale = null;
            if (portal != null)
            {
                Dictionary<string, Locale> locales = GetLocales(portal.PortalID);
                if (locales != null && locales.ContainsKey(portal.DefaultLanguage))
                {
                    locale = locales[portal.DefaultLanguage];
                }
            }
            return locale ?? (GetLocale(Localization.SystemLocale));
        }

        /// <summary>
        /// Gets the locale by code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Locale GetLocale(string code)
        {
            return GetLocale(Null.NullInteger, code);
        }

        /// <summary>
        /// Gets the locale included in the portal.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Locale GetLocale(int portalID, string code)
        {
            Dictionary<string, Locale> dicLocales = GetLocales(portalID);
            Locale locale = null;

            if (dicLocales != null)
            {
                dicLocales.TryGetValue(code, out locale);
            }

            return locale;
        }

        /// <summary>
        /// Gets the locale.
        /// </summary>
        /// <param name="languageID">The language ID.</param>
        /// <returns></returns>
        public Locale GetLocale(int languageID)
        {
            Dictionary<string, Locale> dicLocales = GetLocales(Null.NullInteger);

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
                    locales = CBO.FillDictionary("CultureCode", DataProvider.Instance().GetLanguages(), new Dictionary<string, Locale>());
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
            return GetLocales(portalID).Where(kvp => kvp.Value.IsPublished).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
                Dictionary<string, Locale> dicLocales = GetLocales(portalId);

                if ((!dicLocales.ContainsKey(localeCode)))
                {
                    //if localecode is neutral (en, es,...) try to find a locale that has the same language
                    if (localeCode.IndexOf("-", StringComparison.Ordinal) == -1)
                    {
                        foreach (string strLocale in dicLocales.Keys)
                        {
                            if (strLocale.Split('-')[0] == localeCode)
                            {
                                //set the requested _localecode to the full locale
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
                //item could not be retrieved  or error
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
            DataProvider.Instance().UpdatePortalLanguage(locale.PortalId, locale.LanguageId, locale.IsPublished, UserController.GetCurrentUserInfo().UserID);
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
            bool returnValue = code == PortalController.GetCurrentPortalSettings().DefaultLanguage;
            return returnValue;
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
                    var tabCtrl = new TabController();
                    tabCtrl.PublishTabs(TabController.GetTabsBySortOrder(portalid, cultureCode, false));
                }
            }
        }

        #endregion
    }
}
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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Installer.Packages;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>LocaleController provides method to manage all pages with localization content.</summary>
    /// <remarks>
    /// Content localization in DotNetNuke will allow you to easily manage your web pages in a primary language
    /// and then utilize translators to keep the content synchronized in multiple secondary languages.
    /// Whether you are maintaining your site in a single language or dozens of languages, the content localization system
    /// will help guide your content editors and translators through the process.  Although content localization required
    /// extensive changes to the core platform, we have been able to add this new feature while still improving overall system performance.
    /// </remarks>
    public class LocaleController : ComponentBase<ILocaleController, LocaleController>, ILocaleController
    {
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly DataProvider dataProvider;
        private readonly IPortalController portalController;
        private readonly ITabController tabController;
        private readonly IUserController userController;
        private readonly IPackageController packageController;

        /// <summary>Initializes a new instance of the <see cref="LocaleController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public LocaleController()
            : this(null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LocaleController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="tabController">The tab controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="packageController">The package controller.</param>
        public LocaleController(IHostSettings hostSettings, IApplicationStatusInfo appStatus, DataProvider dataProvider, IPortalController portalController, ITabController tabController, IUserController userController, IPackageController packageController)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
            this.userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
            this.packageController = packageController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPackageController>();
        }

        public static bool IsValidCultureName(string name)
        {
            return
                CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Any(c => c.Name == name);
        }

        /// <summary>Determines whether the language can be deleted.</summary>
        /// <param name="languageId">The language ID.</param>
        /// <returns><see langword="true"/> if the language can be deleted; otherwise, <see langword="false"/>.</returns>
        public bool CanDeleteLanguage(int languageId)
        {
            return this.packageController.GetExtensionPackages(Null.NullInteger, p => p.PackageType.Equals("CoreLanguagePack", StringComparison.OrdinalIgnoreCase))
                        .Select(package => LanguagePackController.GetLanguagePackByPackage(package.PackageID))
                        .All(languagePack => languagePack.LanguageID != languageId);
        }

        /// <summary>Gets the cultures from local list.</summary>
        /// <param name="locales">The locales.</param>
        /// <returns>culture list.</returns>
        public List<CultureInfo> GetCultures(Dictionary<string, Locale> locales)
        {
            return locales.Values.Select(locale => new CultureInfo(locale.Code)).ToList();
        }

        /// <summary>Gets the current locale for current request to the portal.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>A <see cref="Locale"/> instance.</returns>
        public Locale GetCurrentLocale(int portalId)
        {
            Locale locale = null;

            if (HttpContext.Current != null && !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["language"]))
            {
                locale = this.GetLocale(HttpContext.Current.Request.QueryString["language"]);
            }

            return locale ?? ((portalId == Null.NullInteger)
                                ? this.GetLocale(Localization.SystemLocale)
                                : this.GetDefaultLocale(portalId));
        }

        /// <summary>Gets the default locale of the portal.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>A <see cref="Locale"/> instance.</returns>
        public Locale GetDefaultLocale(int portalId)
        {
            IPortalInfo portal = this.portalController.GetPortal(portalId);
            Locale locale = null;
            if (portal != null)
            {
                var locales = this.GetLocales(portal.PortalId);
                if (locales != null && locales.TryGetValue(portal.DefaultLanguage, out var locale1))
                {
                    locale = locale1;
                }
            }

            return locale ?? this.GetLocale(Localization.SystemLocale);
        }

        /// <summary>Gets the locale by code.</summary>
        /// <param name="code">The code.</param>
        /// <returns>A <see cref="Locale"/> instance or <see langword="null"/>.</returns>
        public Locale GetLocale(string code)
        {
            return this.GetLocale(Null.NullInteger, code);
        }

        /// <summary>Gets the locale included in the portal.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="code">The code.</param>
        /// <returns>A <see cref="Locale"/> instance or <see langword="null"/>.</returns>
        public Locale GetLocale(int portalID, string code)
        {
            Dictionary<string, Locale> dicLocales = this.GetLocales(portalID);
            Locale locale = null;

            dicLocales?.TryGetValue(code, out locale);

            return locale;
        }

        /// <summary>Gets the locale included in the portal if culture code is not null or empty or else gets the current locale for current request to the portal.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="code">The code.</param>
        /// <returns>A <see cref="Locale"/> instance or <see langword="null"/>.</returns>
        public Locale GetLocaleOrCurrent(int portalID, string code)
        {
            return string.IsNullOrEmpty(code)
                       ? this.GetCurrentLocale(portalID) : this.GetLocale(portalID, code);
        }

        /// <summary>Gets the locale.</summary>
        /// <param name="languageID">The language ID.</param>
        /// <returns>A <see cref="Locale"/> instance or <see langword="null"/>.</returns>
        public Locale GetLocale(int languageID)
        {
            Dictionary<string, Locale> dicLocales = this.GetLocales(Null.NullInteger);

            return (from kvp in dicLocales where kvp.Value.LanguageId == languageID select kvp.Value).FirstOrDefault();
        }

        /// <summary>Gets the locales.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping from culture code to <see cref="Locale"/>, or <see langword="null"/>.</returns>
        public Dictionary<string, Locale> GetLocales(int portalID)
        {
            if (this.appStatus.Status == UpgradeStatus.Error)
            {
                return null;
            }

            if (this.appStatus.Status == UpgradeStatus.Install)
            {
                return CBO.FillDictionary(
                    "CultureCode",
                    this.dataProvider.GetLanguages(),
                    new Dictionary<string, Locale>(StringComparer.OrdinalIgnoreCase));
            }

            var cacheArgs = new CacheItemArgs(
                string.Format(DataCache.LocalesCacheKey, portalID),
                DataCache.LocalesCacheTimeOut,
                DataCache.LocalesCachePriority,
                portalID,
                this.dataProvider);
            return CBO.GetCachedObject<Dictionary<string, Locale>>(
                this.hostSettings,
                cacheArgs,
                GetLocalesCallBack,
                true);
        }

        /// <summary>Gets the published locales.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping from culture code to <see cref="Locale"/>.</returns>
        public Dictionary<string, Locale> GetPublishedLocales(int portalID)
        {
            return this.GetLocales(portalID).Where(kvp => kvp.Value.IsPublished).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>Determines whether the specified locale code is enabled.</summary>
        /// <param name="localeCode">The locale code.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns><see langword="true"/> if the specified locale code is enabled; otherwise, <see langword="false"/>.</returns>
        public bool IsEnabled(ref string localeCode, int portalId)
        {
            try
            {
                bool enabled = false;
                Dictionary<string, Locale> dicLocales = this.GetLocales(portalId);

                // if ((!dicLocales.ContainsKey(localeCode)))
                string locale = localeCode;
                if (dicLocales.FirstOrDefault(x => string.Equals(x.Key, locale, StringComparison.OrdinalIgnoreCase)).Key == null)
                {
                    // if localecode is neutral (en, es,...) try to find a locale that has the same language
                    if (localeCode.IndexOf("-", StringComparison.Ordinal) == -1)
                    {
                        foreach (string strLocale in dicLocales.Keys)
                        {
                            if (string.Equals(strLocale.Split('-')[0], localeCode, StringComparison.OrdinalIgnoreCase))
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

        /// <summary>Updates the portal locale.</summary>
        /// <param name="locale">The locale.</param>
        public void UpdatePortalLocale(Locale locale)
        {
            this.dataProvider.UpdatePortalLanguage(locale.PortalId, locale.LanguageId, locale.IsPublished, this.userController.GetCurrentUserInfo().UserID);
            DataCache.RemoveCache(string.Format(DataCache.LocalesCacheKey, locale.PortalId));
        }

        /// <summary>Determines the language whether is default language.</summary>
        /// <param name="code">The code.</param>
        /// <returns><see langword="true"/> if the language is default language; otherwise, <see langword="false"/>.</returns>
        public bool IsDefaultLanguage(string code)
        {
            return code == this.portalController.GetCurrentSettings().DefaultLanguage;
        }

        /// <summary>Activates the language without publishing it.</summary>
        /// <param name="portalid">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="publish">if set to <see langword="true"/> will publish the language, otherwise will "un-publish".</param>
        public void ActivateLanguage(int portalid, string cultureCode, bool publish)
        {
            Dictionary<string, Locale> enabledLanguages = this.GetLocales(portalid);
            if (enabledLanguages.TryGetValue(cultureCode, out var enabledLanguage))
            {
                enabledLanguage.IsPublished = publish;
                this.UpdatePortalLocale(enabledLanguage);
            }
        }

        /// <summary>Publishes the language.</summary>
        /// <param name="portalid">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="publish">if set to <see langword="true"/> will publish the language.</param>
        public void PublishLanguage(int portalid, string cultureCode, bool publish)
        {
            Dictionary<string, Locale> enabledLanguages = this.GetLocales(portalid);
            if (enabledLanguages.TryGetValue(cultureCode, out var enabledLanguage))
            {
                enabledLanguage.IsPublished = publish;
                this.UpdatePortalLocale(enabledLanguage);
                if (publish)
                {
                    // only publish tabs if we actually need to do that
                    // we cannot "unpublish"
                    this.tabController.PublishTabs(TabController.GetTabsBySortOrder(portalid, cultureCode, false));
                }
            }
        }

        private static object GetLocalesCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var dataProvider = (DataProvider)cacheItemArgs.ParamList[1];
            var languages = portalId > Null.NullInteger
                ? dataProvider.GetLanguagesByPortal(portalId)
                : dataProvider.GetLanguages();
            return CBO.FillDictionary(
                "CultureCode",
                languages,
                new Dictionary<string, Locale>(StringComparer.OrdinalIgnoreCase));
        }
    }
}

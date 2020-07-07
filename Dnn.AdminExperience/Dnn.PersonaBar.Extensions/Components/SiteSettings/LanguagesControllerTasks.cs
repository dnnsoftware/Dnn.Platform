// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Dnn.PersonaBar.SiteSettings.Services.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    internal class LanguagesControllerTasks
    {
        private const string LocalResourcesFile = "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.SiteSettings/App_LocalResources/SiteSettings.resx";
        private const string LocalizationProgressFile = "PersonaBarLocalizationProgress.txt";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguagesControllerTasks));

        public static void LocalizeSitePages(LocalizationProgress progress, int portalId, bool translatePages, string defaultLanguage)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var languageCount = LocaleController.Instance.GetLocales(portalId).Count;
                    var languageCounter = 0;

                    var pageList = GetPages(portalId).Where(p => string.IsNullOrEmpty(p.CultureCode)).ToList();

                    if (translatePages)
                    {
                        // populate default language
                        ProcessLanguage(pageList, LocaleController.Instance.GetLocale(defaultLanguage),
                            defaultLanguage, languageCounter, languageCount, progress);
                    }
                    PublishLanguage(defaultLanguage, portalId, true);

                    PortalController.UpdatePortalSetting(portalId, "ContentLocalizationEnabled", "True");

                    if (translatePages)
                    {
                        // populate other languages
                        pageList = GetPages(portalId).Where(p => p.CultureCode == defaultLanguage).ToList();

                        foreach (var locale in LocaleController.Instance.GetLocales(portalId).Values.Where(l => l.Code != defaultLanguage))
                        {
                            languageCounter++;

                            //add translator role
                            Localization.AddTranslatorRole(portalId, locale);

                            //populate pages
                            ProcessLanguage(pageList, locale, defaultLanguage, languageCounter, languageCount, progress);

                            //Map special pages
                            PortalController.Instance.MapLocalizedSpecialPages(portalId, locale.Code);
                        }
                    }

                    //clear portal cache
                    DataCache.ClearPortalCache(portalId, true);
                    progress.Reset();
                    SaveProgressToFile(progress);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Logger.Error(ex);
                        progress.Reset().Error = ex.ToString();
                        SaveProgressToFile(progress);
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
            });
        }

        public static void LocalizeLanguagePages(LocalizationProgress progress, int portalId, string cultureCode, string defaultLocale)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var pageList = GetTabsToLocalize(portalId, cultureCode, defaultLocale);
                    var locale = LocaleController.Instance.GetLocale(cultureCode);

                    //add translator role
                    Localization.AddTranslatorRole(portalId, locale);

                    //populate pages
                    ProcessLanguage(pageList, locale, defaultLocale, 0, 1, progress);

                    //Map special pages
                    PortalController.Instance.MapLocalizedSpecialPages(portalId, locale.Code);

                    //clear portal cache
                    DataCache.ClearPortalCache(portalId, true);
                    progress.Reset();
                    SaveProgressToFile(progress);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Logger.Error(ex);
                        progress.Reset().Error = ex.ToString();
                        SaveProgressToFile(progress);
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
            });
        }

        internal static LocalizationProgress ReadProgressFile()
        {
            var path = Path.Combine(Globals.ApplicationMapPath, "App_Data", LocalizationProgressFile);
#if true
            var text = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<LocalizationProgress>(text);
#else
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 256))
            {
                var bytes = new byte[file.Length];
                file.Read(bytes, 0, bytes.Length);
                var text = Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject<LocalizationProgress>(text);
            }
#endif
        }

        private static IList<TabInfo> GetTabsToLocalize(int portalId, string code, string defaultLocale)
        {
            var results = new List<TabInfo>();
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId)
                .Where(kvp => (kvp.Value.CultureCode == defaultLocale || string.IsNullOrEmpty(kvp.Value.CultureCode))
                && !kvp.Value.IsDeleted && !kvp.Value.IsSystem && !kvp.Value.TabPath.StartsWith("//Admin"));

            foreach (var kvp in portalTabs)
            {
                if (kvp.Value.LocalizedTabs.Count == 0)
                {
                    results.Add(kvp.Value);
                }
                else
                {
                    var tabLocalizedInCulture = kvp.Value.LocalizedTabs.Any(localizedTab => localizedTab.Value.CultureCode == code);
                    if (!tabLocalizedInCulture)
                    {
                        results.Add(kvp.Value);
                    }
                }
            }

            return results;
        }

        private static void ProcessLanguage(ICollection<TabInfo> pageList, Locale locale,
            string defaultLocale, int languageCount, int totalLanguages, LocalizationProgress progress)
        {
            progress.PrimaryTotal = totalLanguages;
            progress.PrimaryValue = languageCount;

            var total = pageList.Count;
            if (total == 0)
            {
                progress.SecondaryTotal = 0;
                progress.SecondaryValue = 0;
                progress.SecondaryPercent = 100;
            }

            for (var i = 0; i < total; i++)
            {
                var currentTab = pageList.ElementAt(i);
                var stepNo = i + 1;

                progress.SecondaryTotal = total;
                progress.SecondaryValue = stepNo;
                progress.SecondaryPercent = Convert.ToInt32((float)stepNo / total * 100);
                progress.PrimaryPercent =
                    Convert.ToInt32((languageCount + (float)stepNo / total) / totalLanguages * 100);

                progress.CurrentOperationText = string.Format(Localization.GetString(
                    "ProcessingPage", LocalResourcesFile), locale.Code, stepNo, total, currentTab.TabName);

                progress.TimeEstimated = (total - stepNo) * 100;

                SaveProgressToFile(progress);

                if (locale.Code == defaultLocale || string.IsNullOrEmpty(locale.Code))
                {
                    TabController.Instance.LocalizeTab(currentTab, locale, true);
                }
                else
                {
                    if (currentTab.IsNeutralCulture)
                    {
                        TabController.Instance.LocalizeTab(currentTab, LocaleController.Instance.GetLocale(defaultLocale), true);
                    }
                    TabController.Instance.CreateLocalizedCopy(currentTab, locale, false);
                }
            }
        }

        private static void SaveProgressToFile(LocalizationProgress progress)
        {
            var path = Path.Combine(Globals.ApplicationMapPath, "App_Data", LocalizationProgressFile);
            var text = JsonConvert.SerializeObject(progress);
#if false
            // this could have file locking issues from multiple threads
            File.WriteAllText(path, text);
#else
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 256))
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                file.Write(bytes, 0, bytes.Length);
                file.Flush();
            }
#endif
        }

        private static IList<TabInfo> GetPages(int portalId)
        {
            return (
                from kvp in TabController.Instance.GetTabsByPortal(portalId)
                where !kvp.Value.TabPath.StartsWith("//Admin")
                      && !kvp.Value.IsDeleted
                      && !kvp.Value.IsSystem
                select kvp.Value
                ).ToList();
        }

        private static void PublishLanguage(string cultureCode, int portalId, bool publish)
        {
            var enabledLanguages = LocaleController.Instance.GetLocales(portalId);
            Locale enabledlanguage;
            if (enabledLanguages.TryGetValue(cultureCode, out enabledlanguage))
            {
                enabledlanguage.IsPublished = publish;
                LocaleController.Instance.UpdatePortalLocale(enabledlanguage);
            }
        }
    }
}

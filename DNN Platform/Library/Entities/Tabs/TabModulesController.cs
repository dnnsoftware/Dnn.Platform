// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.Skins;

    public class TabModulesController : ServiceLocator<ITabModulesController, TabModulesController>, ITabModulesController
    {
        public ArrayList GetTabModules(TabInfo tab)
        {
            var objPaneModules = new Dictionary<string, int>();

            var modules = GetModules(tab);
            var configuredModules = new ArrayList();
            foreach (var configuringModule in modules)
            {
                ConfigureModule(configuringModule, tab);

                if (objPaneModules.ContainsKey(configuringModule.PaneName) == false)
                {
                    objPaneModules.Add(configuringModule.PaneName, 0);
                }

                configuringModule.PaneModuleCount = 0;
                if (!configuringModule.IsDeleted)
                {
                    objPaneModules[configuringModule.PaneName] = objPaneModules[configuringModule.PaneName] + 1;
                    configuringModule.PaneModuleIndex = objPaneModules[configuringModule.PaneName] - 1;
                }

                configuredModules.Add(configuringModule);
            }

            foreach (ModuleInfo module in configuredModules)
            {
                module.PaneModuleCount = objPaneModules[module.PaneName];
            }

            return configuredModules;
        }

        public Dictionary<int, string> GetTabModuleSettingsByName(string settingName)
        {
            var portalId = PortalSettings.Current.PortalId;
            var dataProvider = DataProvider.Instance();
            var cacheKey = string.Format(DataCache.TabModuleSettingsNameCacheKey, portalId, settingName);
            var cachedItems = CBO.GetCachedObject<Dictionary<int, string>>(
                new CacheItemArgs(cacheKey, DataCache.TabModuleCacheTimeOut, DataCache.TabModuleCachePriority),
                c =>
                {
                    using (var dr = dataProvider.GetTabModuleSettingsByName(portalId, settingName))
                    {
                        var result = new Dictionary<int, string>();
                        while (dr.Read())
                        {
                            result[dr.GetInt32(0)] = dr.GetString(1);
                        }

                        return result;
                    }
                });

            return cachedItems;
        }

        public IList<int> GetTabModuleIdsBySetting(string settingName, string expectedValue)
        {
            var items = this.GetTabModuleSettingsByName(settingName);
            var matches = items.Where(e => e.Value.Equals(expectedValue, StringComparison.CurrentCultureIgnoreCase));
            var keyValuePairs = matches as KeyValuePair<int, string>[] ?? matches.ToArray();
            if (keyValuePairs.Any())
            {
                return keyValuePairs.Select(kpv => kpv.Key).ToList();
            }

            // this is fallback in case a new value was added but not in the cache yet
            var dataProvider = DataProvider.Instance();
            using (var dr = dataProvider.GetTabModuleIdsBySettingNameAndValue(PortalSettings.Current.PortalId, settingName, expectedValue))
            {
                var result = new List<int>();
                while (dr.Read())
                {
                    result.Add(dr.GetInt32(0));
                }

                return result;
            }
        }

        protected override Func<ITabModulesController> GetFactory()
        {
            return () => new TabModulesController();
        }

        private static void ConfigureModule(ModuleInfo cloneModule, TabInfo tab)
        {
            if (Null.IsNull(cloneModule.StartDate))
            {
                cloneModule.StartDate = DateTime.MinValue;
            }

            if (Null.IsNull(cloneModule.EndDate))
            {
                cloneModule.EndDate = DateTime.MaxValue;
            }

            if (string.IsNullOrEmpty(cloneModule.ContainerSrc))
            {
                cloneModule.ContainerSrc = tab.ContainerSrc;
            }

            cloneModule.ContainerSrc = SkinController.FormatSkinSrc(cloneModule.ContainerSrc, PortalSettings.Current);
            cloneModule.ContainerPath = SkinController.FormatSkinPath(cloneModule.ContainerSrc);
        }

        private static IEnumerable<ModuleInfo> GetModules(TabInfo tab)
        {
            int urlVersion;
            if (TabVersionUtils.TryGetUrlVersion(out urlVersion))
            {
                return TabVersionBuilder.Instance.GetVersionModules(tab.TabID, urlVersion);
            }

            if (Globals.IsEditMode())
            {
                return TabVersionBuilder.Instance.GetUnPublishedVersionModules(tab.TabID);
            }

            return TabVersionBuilder.Instance.GetCurrentModules(tab.TabID);
        }
    }
}

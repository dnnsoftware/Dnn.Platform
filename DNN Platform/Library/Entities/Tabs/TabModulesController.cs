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

namespace DotNetNuke.Entities.Tabs
{
    public class TabModulesController: ServiceLocator<ITabModulesController, TabModulesController>, ITabModulesController
    {
        #region Public Methods

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

        public Dictionary<int,string> GetTabModuleSettingsByName(string settingName)
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
            var items = GetTabModuleSettingsByName(settingName);
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

        #endregion

        #region Private Methods
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
            if (String.IsNullOrEmpty(cloneModule.ContainerSrc))
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
        #endregion

        protected override Func<ITabModulesController> GetFactory()
        {
            return ()  => new TabModulesController();
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Caching;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Search.Entities;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// Search Result Controller for Module Crawler.
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public class ModuleResultController : BaseResultController
    {
        private const string ModuleByIdCacheKey = "ModuleById{0}";
        private const CacheItemPriority ModuleByIdCachePriority = CacheItemPriority.Normal;
        private const int ModuleByIdCacheTimeOut = 20;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleResultController));

        private static Hashtable _moduleSearchControllers = new Hashtable();
        private static object _threadLock = new object();

        public override bool HasViewPermission(SearchResult searchResult)
        {
            var viewable = false;
            if (searchResult.ModuleId > 0)
            {
                // Get All related tabIds from moduleId (while minimizing DB access; using caching)
                var moduleId = searchResult.ModuleId;

                // The next call has over 30% performance enhancement over the above one
                var tabModules = TabController.Instance.GetTabsByPortal(searchResult.PortalId).Values
                    .SelectMany(tabinfo => tabinfo.ChildModules.Where(kv => kv.Key == moduleId)).Select(m => m.Value);

                foreach (ModuleInfo module in tabModules)
                {
                    var tab = TabController.Instance.GetTab(module.TabID, searchResult.PortalId, false);
                    if (this.ModuleIsAvailable(tab, module) && !tab.IsDeleted && !tab.DisableLink && TabPermissionController.CanViewPage(tab))
                    {
                        // Check If authorised to View Module
                        if (ModulePermissionController.CanViewModule(module) && this.HasModuleSearchPermission(module, searchResult))
                        {
                            // Verify against search document permissions
                            if (string.IsNullOrEmpty(searchResult.Permissions) || PortalSecurity.IsInRoles(searchResult.Permissions))
                            {
                                viewable = true;
                                if (string.IsNullOrEmpty(searchResult.Url))
                                {
                                    searchResult.Url = this.GetModuleSearchUrl(module, searchResult);
                                    if (string.IsNullOrEmpty(searchResult.Url))
                                    {
                                        searchResult.Url = TestableGlobals.Instance.NavigateURL(module.TabID, string.Empty,
                                                                               searchResult.QueryString);
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                viewable = true;
            }

            return viewable;
        }

        // Returns the URL to the first instance of the module the user has access to view
        public override string GetDocUrl(SearchResult searchResult)
        {
            if (!string.IsNullOrEmpty(searchResult.Url))
            {
                return searchResult.Url;
            }

            var url = Localization.GetString("SEARCH_NoLink");

            // Get All related tabIds from moduleId
            var tabModules = GetModuleTabs(searchResult.ModuleId);

            foreach (ModuleInfo module in tabModules)
            {
                var tab = TabController.Instance.GetTab(module.TabID, searchResult.PortalId, false);
                if (TabPermissionController.CanViewPage(tab) && ModulePermissionController.CanViewModule(module))
                {
                    try
                    {
                        url = this.GetModuleSearchUrl(module, searchResult);

                        if (string.IsNullOrEmpty(url))
                        {
                            var portalSettings = new PortalSettings(searchResult.PortalId);
                            portalSettings.PortalAlias =
                                PortalAliasController.Instance.GetPortalAlias(portalSettings.DefaultPortalAlias);
                            url = TestableGlobals.Instance.NavigateURL(module.TabID, portalSettings, string.Empty,
                                                      searchResult.QueryString);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    break;
                }
            }

            return url;
        }

        private static ArrayList GetModuleTabs(int moduleID)
        {
            // no manual clearing of the cache exists; let is just expire
            var cacheKey = string.Format(ModuleByIdCacheKey, moduleID);
            return CBO.GetCachedObject<ArrayList>(
                new CacheItemArgs(cacheKey, ModuleByIdCacheTimeOut, ModuleByIdCachePriority, moduleID),
                (args) => CBO.FillCollection(DataProvider.Instance().GetModule(moduleID, Null.NullInteger), typeof(ModuleInfo)));
        }

        private bool HasModuleSearchPermission(ModuleInfo module, SearchResult searchResult)
        {
            var canView = true;

            var moduleSearchController = this.GetModuleSearchController(module);
            if (moduleSearchController != null)
            {
                canView = moduleSearchController.HasViewPermission(searchResult);
            }

            return canView;
        }

        private string GetModuleSearchUrl(ModuleInfo module, SearchResult searchResult)
        {
            var url = string.Empty;
            var moduleSearchController = this.GetModuleSearchController(module);
            if (moduleSearchController != null)
            {
                url = moduleSearchController.GetDocUrl(searchResult);
            }

            return url;
        }

        private IModuleSearchResultController GetModuleSearchController(ModuleInfo module)
        {
            if (string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass))
            {
                return null;
            }

            if (!_moduleSearchControllers.ContainsKey(module.DesktopModule.BusinessControllerClass))
            {
                lock (_threadLock)
                {
                    if (!_moduleSearchControllers.ContainsKey(module.DesktopModule.BusinessControllerClass))
                    {
                        var controller = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass, module.DesktopModule.BusinessControllerClass) as IModuleSearchResultController;
                        _moduleSearchControllers.Add(module.DesktopModule.BusinessControllerClass, controller);
                    }
                }
            }

            return _moduleSearchControllers[module.DesktopModule.BusinessControllerClass] as IModuleSearchResultController;
        }

        private bool ModuleIsAvailable(TabInfo tab, ModuleInfo module)
        {
            return this.GetModules(tab).Any(m => m.ModuleID == module.ModuleID && !m.IsDeleted);
        }

        private IEnumerable<ModuleInfo> GetModules(TabInfo tab)
        {
            int urlVersion;
            if (TabVersionUtils.TryGetUrlVersion(out urlVersion))
            {
                return TabVersionBuilder.Instance.GetVersionModules(tab.TabID, urlVersion);
            }

            return TabVersionBuilder.Instance.GetCurrentModules(tab.TabID);
        }
    }
}

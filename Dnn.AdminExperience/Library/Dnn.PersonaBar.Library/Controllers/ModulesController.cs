// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Dnn.PersonaBar.Library.Helper;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    public class ModulesController : ServiceLocator<IModulesController, ModulesController>, IModulesController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModulesController));
        private IContentVerifier _contentVerifier;

        public ModulesController()
            : this(new ContentVerifier())
        {
        }

        public ModulesController(IContentVerifier contentVerifier)
        {
            this._contentVerifier = contentVerifier;
        }

        public List<ModuleInfo> AddNewModule(PortalSettings portalSettings, string title, int desktopModuleId, int tabId, string paneName, int position, int permissionType, string align, out KeyValuePair<HttpStatusCode, string> message)
        {
            message = default(KeyValuePair<HttpStatusCode, string>);
            var page = TabController.Instance.GetTab(tabId, portalSettings.PortalId);
            if (page == null)
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(Localization.GetString("Prompt_PageNotFound", Constants.LocalResourcesFile), tabId));
                return null;
            }

            if (!TabPermissionController.CanManagePage(page))
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("Prompt_InsufficientPermissions", Constants.LocalResourcesFile));
                return null;
            }

            var moduleList = new List<ModuleInfo>();

            foreach (var objModuleDefinition in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
            {
                var objModule = new ModuleInfo();
                objModule.Initialize(portalSettings.PortalId);

                objModule.PortalID = portalSettings.PortalId;
                objModule.TabID = tabId;
                objModule.ModuleOrder = position;
                objModule.ModuleTitle = string.IsNullOrEmpty(title) ? objModuleDefinition.FriendlyName : title;
                objModule.PaneName = paneName;
                objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                if (objModuleDefinition.DefaultCacheTime > 0)
                {
                    objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
                    if (portalSettings.DefaultModuleId > Null.NullInteger &&
                        portalSettings.DefaultTabId > Null.NullInteger)
                    {
                        var defaultModule = ModuleController.Instance.GetModule(
                            portalSettings.DefaultModuleId,
                            portalSettings.DefaultTabId, true);
                        if (defaultModule != null)
                        {
                            objModule.CacheTime = defaultModule.CacheTime;
                        }
                    }
                }

                ModuleController.Instance.InitialModulePermission(objModule, objModule.TabID, permissionType);

                if (portalSettings.ContentLocalizationEnabled)
                {
                    var defaultLocale = LocaleController.Instance.GetDefaultLocale(portalSettings.PortalId);

                    // check whether original tab is exists, if true then set culture code to default language,
                    // otherwise set culture code to current.
                    objModule.CultureCode =
                        TabController.Instance.GetTabByCulture(objModule.TabID, portalSettings.PortalId, defaultLocale) !=
                        null
                            ? defaultLocale.Code
                            : portalSettings.CultureCode;
                }
                else
                {
                    objModule.CultureCode = Null.NullString;
                }

                objModule.AllTabs = false;
                objModule.Alignment = align;

                ModuleController.Instance.AddModule(objModule);
                moduleList.Add(objModule);

                // Set position so future additions to page can operate correctly
                position = ModuleController.Instance.GetTabModule(objModule.TabModuleID).ModuleOrder + 1;
            }

            return moduleList;
        }

        public ModuleInfo CopyModule(PortalSettings portalSettings, int moduleId, int sourcePageId, int targetPageId, string pane, bool includeSettings, out KeyValuePair<HttpStatusCode, string> message, bool moveBahaviour = false)
        {
            var sourceModule = this.GetModule(portalSettings, moduleId, sourcePageId, out message);

            if (sourceModule == null)
            {
                return null;
            }

            var targetPage = TabController.Instance.GetTab(targetPageId, portalSettings.PortalId);

            message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(Localization.GetString("Prompt_PageNotFound", Constants.LocalResourcesFile), targetPageId));

            if (targetPage == null)
            {
                return null;
            }

            var currentPortalSetting = PortalController.Instance.GetCurrentPortalSettings();

            if (this._contentVerifier.IsContentExistsForRequestedPortal(targetPage.PortalID, portalSettings))
            {
                try
                {
                    if (moveBahaviour)
                    {
                        ModuleController.Instance.MoveModule(sourceModule.ModuleID, sourceModule.TabID, targetPage.TabID, pane);
                    }
                    else
                    {
                        ModuleController.Instance.CopyModule(sourceModule, targetPage, pane, includeSettings);
                    }

                    ModuleController.Instance.ClearCache(targetPageId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.InternalServerError, Localization.GetString(moveBahaviour ? "Prompt_ErrorWhileMoving" : "Prompt_ErrorWhileCopying"));
                }

                // get the new module
                return ModuleController.Instance.GetModule(sourceModule.ModuleID, targetPageId, true);
            }
            else
            {
                return null;
            }
        }

        public void DeleteModule(PortalSettings portalSettings, int moduleId, int pageId, out KeyValuePair<HttpStatusCode, string> message)
        {
            var module = this.GetModule(portalSettings, moduleId, pageId, out message);

            if (module != null)
            {
                try
                {
                    ModuleController.Instance.DeleteTabModule(pageId, moduleId, true);
                    ModuleController.Instance.ClearCache(pageId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.InternalServerError, string.Format(Localization.GetString("Prompt_FailedtoDeleteModule", Constants.LocalResourcesFile), moduleId));
                }
            }
        }

        public ModuleInfo GetModule(PortalSettings portalSettings, int moduleId, int? pageId, out KeyValuePair<HttpStatusCode, string> message)
        {
            message = default(KeyValuePair<HttpStatusCode, string>);
            if (pageId.HasValue)
            {
                var module = ModuleController.Instance.GetModule(moduleId, pageId.Value, true);

                if (module != null)
                {
                    var currentPortal = PortalController.Instance.GetCurrentPortalSettings();

                    if (this._contentVerifier.IsContentExistsForRequestedPortal(module.PortalID, portalSettings, true))
                    {
                        return module;
                    }
                }
                else
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(Localization.GetString("Prompt_ModuleNotFound", Constants.LocalResourcesFile), moduleId, pageId));
                    return null;
                }
            }
            else
            {
                var modules = ModuleController.Instance.GetAllTabsModulesByModuleID(moduleId);
                if (modules != null && modules.Count != 0)
                {
                    return modules[0] as ModuleInfo;
                }
            }

            message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(Localization.GetString("Prompt_NoModule", Constants.LocalResourcesFile), moduleId));
            return null;
        }

        public IEnumerable<ModuleInfo> GetModules(PortalSettings portalSettings, bool? deleted, out int total, string moduleName = null, string moduleTitle = null,
            int? pageId = null, int pageIndex = 0, int pageSize = 10)
        {
            pageIndex = pageIndex < 0 ? 0 : pageIndex;
            pageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
            moduleName = moduleName?.Replace("*", string.Empty);
            moduleTitle = moduleTitle?.Replace("*", string.Empty);
            var modules = ModuleController.Instance.GetModules(portalSettings.PortalId)
                    .Cast<ModuleInfo>().Where(ModulePermissionController.CanViewModule);
            if (!string.IsNullOrEmpty(moduleName))
            {
                modules = modules.Where(module => module.DesktopModule.ModuleName.IndexOf(moduleName, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrEmpty(moduleTitle))
            {
                modules = modules.Where(module => module.ModuleTitle.IndexOf(moduleTitle, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Return only deleted modules with matching criteria.
            if (pageId.HasValue && pageId.Value > 0)
            {
                modules = modules.Where(x => x.TabID == pageId.Value);
            }

            if (deleted.HasValue)
            {
                modules = modules.Where(module => module.IsDeleted == deleted);
            }

            // Get distincts.
            modules = modules.GroupBy(x => x.ModuleID).Select(group => group.First()).OrderBy(x => x.ModuleID);
            var moduleInfos = modules as IList<ModuleInfo> ?? modules.ToList();
            total = moduleInfos.Count;
            return moduleInfos.Skip(pageIndex * pageSize).Take(pageSize);
        }

        protected override Func<IModulesController> GetFactory()
        {
            return () => new ModulesController();
        }
    }
}

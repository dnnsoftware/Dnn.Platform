// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Abstractions.Prompt;
using DotNetNuke.Abstractions.Users;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Prompt;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DotNetNuke.Entities.Modules.Prompt
{
    /// <summary>
    /// This is a (Prompt) Console Command. You should not reference this class directly. It is to be used solely through Prompt.
    /// </summary>
    [ConsoleCommand("add-module", Constants.CommandCategoryKeys.Modules, "Prompt_AddModule_Description")]
    public class AddModule : ConsoleCommand
    {
        public override string LocalResourceFile => Constants.DefaultPromptResourceFile;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AddModule));

        [ConsoleCommandParameter("name", "Prompt_AddModule_FlagModuleName", true)]
        public string ModuleName { get; set; }

        [ConsoleCommandParameter("pageid", "Prompt_AddModule_FlagPageId", true)]
        public int PageId { get; set; }    // the page on which to add the module

        [ConsoleCommandParameter("pane", "Prompt_AddModule_FlagPane", "ContentPane")]
        public string Pane { get; set; }

        [ConsoleCommandParameter("title", "Prompt_AddModule_FlagModuleTitle")]
        public string ModuleTitle { get; set; }   // title for the new module. defaults to friendly name

        public override void Initialize(string[] args, IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            ParseParameters(this);
        }

        public override IConsoleResultModel Run()
        {
            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(ModuleName, PortalId);
                if (desktopModule == null)
                {
                    return new ConsoleErrorResultModel(string.Format(LocalizeString("Prompt_DesktopModuleNotFound"), ModuleName));
                }

                var message = new KeyValuePair<HttpStatusCode, string>();
                var page = TabController.Instance.GetTab(PageId, PortalSettings.PortalId);
                if (page == null)
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(Localization.GetString("Prompt_PageNotFound", LocalResourceFile), PageId));
                    return null;
                }
                if (!TabPermissionController.CanManagePage(page))
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("Prompt_InsufficientPermissions", LocalResourceFile));
                    return null;
                }

                var moduleList = new List<ModuleInfo>();

                foreach (var objModuleDefinition in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModule.DesktopModuleID).Values)
                {
                    var objModule = new ModuleInfo();
                    objModule.Initialize(PortalSettings.PortalId);

                    objModule.PortalID = PortalSettings.PortalId;
                    objModule.TabID = PageId;
                    objModule.ModuleOrder = 0;
                    objModule.ModuleTitle = string.IsNullOrEmpty(ModuleTitle) ? objModuleDefinition.FriendlyName : ModuleTitle;
                    objModule.PaneName = Pane;
                    objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                    if (objModuleDefinition.DefaultCacheTime > 0)
                    {
                        objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
                        if (PortalSettings.DefaultModuleId > Null.NullInteger &&
                            PortalSettings.DefaultTabId > Null.NullInteger)
                        {
                            var defaultModule = ModuleController.Instance.GetModule(PortalSettings.DefaultModuleId,
                                PortalSettings.DefaultTabId, true);
                            if (defaultModule != null)
                            {
                                objModule.CacheTime = defaultModule.CacheTime;
                            }
                        }
                    }

                    ModuleController.Instance.InitialModulePermission(objModule, objModule.TabID, 0);

                    if (PortalSettings.ContentLocalizationEnabled)
                    {
                        var defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalSettings.PortalId);
                        //check whether original tab is exists, if true then set culture code to default language,
                        //otherwise set culture code to current.
                        objModule.CultureCode =
                            TabController.Instance.GetTabByCulture(objModule.TabID, PortalSettings.PortalId, defaultLocale) !=
                            null
                                ? defaultLocale.Code
                                : PortalSettings.CultureCode;
                    }
                    else
                    {
                        objModule.CultureCode = Null.NullString;
                    }
                    objModule.AllTabs = false;
                    objModule.Alignment = null;

                    ModuleController.Instance.AddModule(objModule);
                    moduleList.Add(objModule);

                    // Set position so future additions to page can operate correctly
                    var position = ModuleController.Instance.GetTabModule(objModule.TabModuleID).ModuleOrder + 1;
                }

                if (moduleList == null)
                {
                    return new ConsoleErrorResultModel(message.Value);
                }
                if (moduleList.Count == 0)
                    return new ConsoleErrorResultModel(LocalizeString("Prompt_NoModulesAdded"));
                var modules = moduleList.Select(newModule => ModuleController.Instance.GetTabModule(newModule.TabModuleID)).ToList();

                return new ConsoleResultModel(string.Format(LocalizeString("Prompt_ModuleAdded"), modules.Count, moduleList.Count == 1 ? string.Empty : "s")) { Data = modules, Records = modules.Count };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(LocalizeString("Prompt_AddModuleError"));
            }
        }

    }
}

﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;

    using ModulesControllerLibrary = Dnn.PersonaBar.Library.Controllers.ModulesController;

    [ConsoleCommand("add-module", Constants.ModulesCategory, "Prompt_AddModule_Description")]
    public class AddModule : ConsoleCommandBase
    {
        [FlagParameter("name", "Prompt_AddModule_FlagModuleName", "String", true)]
        private const string FlagModuleName = "name";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AddModule));

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("pageid", "Prompt_AddModule_FlagPageId", "Integer", true)]
        private const string FlagPageId = "pageid";

        [FlagParameter("pane", "Prompt_AddModule_FlagPane", "String", "ContentPane")]
        private const string FlagPane = "pane";

        [FlagParameter("title", "Prompt_AddModule_FlagModuleTitle", "String")]
        private const string FlagModuleTitle = "title";

        private string ModuleName { get; set; }
        private int PageId { get; set; }    // the page on which to add the module
        private string Pane { get; set; }
        private string ModuleTitle { get; set; }   // title for the new module. defaults to friendly name

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.ModuleName = this.GetFlagValue(FlagModuleName, "Module Name", string.Empty, true, true);
            this.PageId = this.GetFlagValue(FlagPageId, "Page Id", -1, true, false, true);
            this.ModuleTitle = this.GetFlagValue(FlagModuleTitle, "Module Title", string.Empty);
            this.Pane = this.GetFlagValue(FlagPane, "Pane", "ContentPane");
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(this.ModuleName, this.PortalId);
                if (desktopModule == null)
                {
                    return new ConsoleErrorResultModel(string.Format(this.LocalizeString("Prompt_DesktopModuleNotFound"), this.ModuleName));
                }

                KeyValuePair<HttpStatusCode, string> message;
                var addedModules = ModulesControllerLibrary.Instance.AddNewModule(
                    this.PortalSettings,
                    this.ModuleTitle,
                    desktopModule.DesktopModuleID,
                    this.PageId,
                    this.Pane,
                    0,
                    0,
                    null,
                    out message
                    );
                if (addedModules == null)
                {
                    return new ConsoleErrorResultModel(message.Value);
                }
                if (addedModules.Count == 0)
                    return new ConsoleErrorResultModel(this.LocalizeString("Prompt_NoModulesAdded"));
                var modules = addedModules.Select(newModule => ModuleInstanceModel.FromDnnModuleInfo(ModuleController.Instance.GetTabModule(newModule.TabModuleID))).ToList();

                return new ConsoleResultModel(string.Format(this.LocalizeString("Prompt_ModuleAdded"), modules.Count, modules.Count == 1 ? string.Empty : "s")) { Data = modules, Records = modules.Count };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_AddModuleError"));
            }
        }
    }
}

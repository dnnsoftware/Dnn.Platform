using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("add-module", "Adds a new module instance to a page", new[] { "name", "pageid", "pane", "title" })]
    public class AddModule : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AddModule));

        private const string FlagModuleName = "name";
        private const string FlagPageId = "pageid";
        private const string FlagPane = "pane";
        private const string FlagModuleTitle = "title";

        private string ModuleName { get; set; }
        private int PageId { get; set; }    // the page on which to add the module
        private string Pane { get; set; }
        private string ModuleTitle { get; set; }   // title for the new module. defaults to friendly name

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagModuleName))
            {
                ModuleName = Flag(FlagModuleName);
            }
            else if (args.Length >= 2 && !IsFlag(args[1]))
            {
                // assume first argument is the module name
                ModuleName = args[1];
            }
            if (string.IsNullOrEmpty(ModuleName))
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_MainParamRequired", Constants.LocalResourcesFile), "Module Name", FlagModuleName);
            }

            if (HasFlag(FlagPageId))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagPageId), out tmpId))
                {
                    PageId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile), FlagPageId);
                }
            }
            else
            {
                // Page ID is required
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagRequired", Constants.LocalResourcesFile), FlagPageId);
            }

            if (HasFlag(FlagModuleTitle))
                ModuleTitle = Flag(FlagModuleTitle);

            if (HasFlag(FlagPane))
                Pane = Flag(FlagPane);
            if (string.IsNullOrEmpty(Pane))
                Pane = "ContentPane";

            if (PageId <= 0)
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagPageId);
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {

            // get the desktop module id from the module name
            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(ModuleName, PortalId);
                if (desktopModule == null)
                {
                    return new ConsoleErrorResultModel(string.Format(Localization.GetString("Prompt_DesktopModuleNotFound", Constants.LocalResourcesFile), ModuleName));
                }

                KeyValuePair<HttpStatusCode, string> message;
                var addedModules = ModulesController.Instance.AddNewModule(PortalSettings, ModuleTitle, desktopModule.DesktopModuleID, PageId, Pane, 0, 0, null, out message);
                if (addedModules == null)
                {
                    return new ConsoleErrorResultModel(message.Value);
                }
                if (addedModules.Count == 0)
                    return new ConsoleErrorResultModel(Localization.GetString("Prompt_NoModulesAdded", Constants.LocalResourcesFile));
                var modules = addedModules.Select(newModule => ModuleInstanceModel.FromDnnModuleInfo(ModuleController.Instance.GetTabModule(newModule.TabModuleID))).ToList();

                return new ConsoleResultModel(string.Format(Localization.GetString("Prompt_ModuleAdded", Constants.LocalResourcesFile), modules.Count, modules.Count == 1 ? string.Empty : "s")) { Data = modules, Records = modules.Count };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(Localization.GetString("Prompt_AddModuleError", Constants.LocalResourcesFile));
            }
        }
    }
}
using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Module
{
    [ConsoleCommand("copy-module", "Copies the module specified", new string[] { "id" })]
    public class CopyModule : BaseConsoleCommand, IConsoleCommand
    {

        private const string FLAG_ID = "id";
        private const string FLAG_PAGEID = "pageid";
        private const string FLAG_TOPAGEID = "topageid";
        private const string FLAG_PANE = "pane";

        private const string FLAG_INCLUDESETTINGS = "includesettings";
        public string ValidationMessage { get; private set; }
        protected int? ModuleId { get; private set; }
        protected int? PageId { get; private set; }
        protected int? TargetPageId { get; private set; }
        protected string Pane { get; private set; }
        protected bool? IncludeSettings { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            if (HasFlag(FLAG_ID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_ID), out tmpId))
                {
                    ModuleId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("The --{0} flag must be an integer; ", FLAG_ID);
                }
            }
            else
            {
                // attempt to get it as the first argument
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    int tmpId = 0;
                    if (int.TryParse(args[1], out tmpId))
                    {
                        ModuleId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat("The Module ID is required. Please use the --{0} flag or pass it as the first argument after the command name; ", FLAG_ID);
                    }
                }
            }

            if (HasFlag(FLAG_PAGEID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_PAGEID), out tmpId))
                {
                    PageId = tmpId;
                }
            }
            else
            {
                // Assume it's on the current Page
                PageId = base.TabId;
            }

            if (HasFlag(FLAG_TOPAGEID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_TOPAGEID), out tmpId))
                {
                    TargetPageId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("--{0} must be an integer; ", FLAG_TOPAGEID);
                }
            }
            else
            {
                sbErrors.AppendFormat("--{0} is required; ", FLAG_TOPAGEID);
            }

            if (HasFlag(FLAG_PANE))
                Pane = Flag(FLAG_PANE);
            if (string.IsNullOrEmpty(Pane))
                Pane = "ContentPane";

            if (HasFlag(FLAG_INCLUDESETTINGS))
            {
                bool tmpBool = false;
                if (bool.TryParse(Flag(FLAG_INCLUDESETTINGS), out tmpBool))
                {
                    IncludeSettings = tmpBool;
                }
                else
                {
                    sbErrors.AppendFormat("--{0} must be a valid boolean (true/false) value; ", FLAG_INCLUDESETTINGS);
                }
            }
            if (!IncludeSettings.HasValue)
            {
                IncludeSettings = true;
            }

            if (ModuleId.HasValue && ModuleId <= 0)
            {
                sbErrors.Append("The Module's ID must be greater than 0; ");
            }
            if (PageId.HasValue && PageId <= 0)
            {
                sbErrors.Append("The source Page ID must be greater than 0; ");
            }
            if (TargetPageId.HasValue && TargetPageId <= 0)
            {
                sbErrors.Append("The target Page ID must be greater than 0; ");
            }
            if (PageId == TargetPageId)
            {
                sbErrors.Append("The source Page ID and target Page ID cannot be the same; ");
            }
            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            List<ModuleInfoModel> lst = new List<ModuleInfoModel>();

            var moduleToBeCopied = ModuleController.Instance.GetModule((int)ModuleId, (int)PageId, true);
            var targetTab = TabController.Instance.GetTab((int)TargetPageId, PortalId);

            if (targetTab == null)
            {
                return new ConsoleErrorResultModel(string.Format("Could not load Target Page. No page found in the portal with ID '{0}'", TargetPageId));
            }


            if (moduleToBeCopied != null)
            {
                try
                {
                    ModuleController.Instance.CopyModule(moduleToBeCopied, targetTab, Pane, (bool)IncludeSettings);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return new ConsoleErrorResultModel("An error occurred while copying the module. See the DNN Event Viewer for Details.");
                }
                // get the new module
                dynamic copiedModule = ModuleController.Instance.GetModule(moduleToBeCopied.ModuleID, (int)TargetPageId, true);
                lst.Add(ModuleInfoModel.FromDnnModuleInfo(copiedModule));
            }
            else
            {
                return new ConsoleResultModel(string.Format("No module found with ID '{0}'", ModuleId));
            }

            return new ConsoleResultModel("Successfully copied the module") { data = lst };
        }

    }
}
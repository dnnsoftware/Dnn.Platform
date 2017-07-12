using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using System.Collections.Generic;
using System.Text;
using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Module
{
    [ConsoleCommand("move-module", "Copies the module specified", new string[] { "id" })]
    public class MoveModule : ConsoleCommandBase
    {

        private const string FLAG_ID = "id";
        private const string FLAG_PAGEID = "pageid";
        private const string FLAG_TOPAGEID = "topageid";
        private const string FLAG_PANE = "pane";
        private const string FLAG_INCLUDESETTINGS = "includesettings";


        public int? ModuleId { get; private set; }
        public int? PageId { get; private set; }
        public int? TargetPageId { get; private set; }
        public string Pane { get; private set; }
        public bool? IncludeSettings { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
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

        public override ConsoleResultModel Run()
        {
            List<ModuleInfoModel> lst = new List<ModuleInfoModel>();

            var moduleIToBeMoved = ModuleController.Instance.GetModule((int)ModuleId, (int)PageId, true);
            var targetTab = TabController.Instance.GetTab((int)TargetPageId, PortalId);

            if (targetTab == null)
            {
                return new ConsoleErrorResultModel(string.Format("Could not load Target Page. No page found in the portal with ID '{0}'", TargetPageId));
            }


            if (moduleIToBeMoved != null)
            {
                try
                {
                    ModuleController.Instance.MoveModule((int)ModuleId, (int)PageId, (int)TargetPageId, Pane);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return new ConsoleErrorResultModel("An error occurred while moving the module. See the DNN Event Viewer for Details.");
                }
                // get the new module
                var movedModule = ModuleController.Instance.GetModule(moduleIToBeMoved.ModuleID, (int)TargetPageId, true);
                lst.Add(ModuleInfoModel.FromDnnModuleInfo(movedModule));
            }
            else
            {
                return new ConsoleResultModel(string.Format("No module found with ID '{0}'", ModuleId));
            }

            return new ConsoleResultModel("Successfully copied the module") { Data = lst };
        }

    }
}
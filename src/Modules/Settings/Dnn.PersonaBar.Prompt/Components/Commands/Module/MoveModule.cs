using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("move-module", "Copies the module specified", new[] { "id" })]
    public class MoveModule : ConsoleCommandBase
    {

        private const string FlagId = "id";
        private const string FlagPageid = "pageid";
        private const string FlagTopageid = "topageid";
        private const string FlagPane = "pane";
        private const string FlagIncludesettings = "includesettings";


        public int? ModuleId { get; private set; }
        public int? PageId { get; private set; }
        public int? TargetPageId { get; private set; }
        public string Pane { get; private set; }
        public bool? IncludeSettings { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagId), out tmpId))
                {
                    ModuleId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("The --{0} flag must be an integer; ", FlagId);
                }
            }
            else
            {
                // attempt to get it as the first argument
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    var tmpId = 0;
                    if (int.TryParse(args[1], out tmpId))
                    {
                        ModuleId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat("The Module ID is required. Please use the --{0} flag or pass it as the first argument after the command name; ", FlagId);
                    }
                }
            }

            if (HasFlag(FlagPageid))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagPageid), out tmpId))
                {
                    PageId = tmpId;
                }
            }
            else
            {
                // Assume it's on the current Page
                PageId = TabId;
            }

            if (HasFlag(FlagTopageid))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagTopageid), out tmpId))
                {
                    TargetPageId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("--{0} must be an integer; ", FlagTopageid);
                }
            }
            else
            {
                sbErrors.AppendFormat("--{0} is required; ", FlagTopageid);
            }

            if (HasFlag(FlagPane))
                Pane = Flag(FlagPane);
            if (string.IsNullOrEmpty(Pane))
                Pane = "ContentPane";

            if (HasFlag(FlagIncludesettings))
            {
                var tmpBool = false;
                if (bool.TryParse(Flag(FlagIncludesettings), out tmpBool))
                {
                    IncludeSettings = tmpBool;
                }
                else
                {
                    sbErrors.AppendFormat("--{0} must be a valid boolean (true/false) value; ", FlagIncludesettings);
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
            var lst = new List<ModuleInfoModel>();

            var moduleIToBeMoved = ModuleController.Instance.GetModule((int)ModuleId, (int)PageId, true);
            var targetTab = TabController.Instance.GetTab((int)TargetPageId, PortalId);

            if (targetTab == null)
            {
                return new ConsoleErrorResultModel(
                    $"Could not load Target Page. No page found in the portal with ID '{TargetPageId}'");
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
                return new ConsoleResultModel($"No module found with ID '{ModuleId}'");
            }

            return new ConsoleResultModel("Successfully copied the module") { Data = lst };
        }

    }
}
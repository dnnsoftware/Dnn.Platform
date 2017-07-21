using System.Collections.Generic;
using System.Net;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
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

        private int? ModuleId { get; set; }
        private int? PageId { get; set; }
        private int? TargetPageId { get; set; }
        private string Pane { get; set; }

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
            if (!ModuleId.HasValue || !PageId.HasValue || !TargetPageId.HasValue)
                return new ConsoleErrorResultModel("Insufficient parameters");
            KeyValuePair<HttpStatusCode, string> message;
            var movedModule = ModulesController.Instance.CopyModule(PortalSettings, ModuleId.Value, PageId.Value, TargetPageId.Value, Pane, true, out message, true);
            if (movedModule == null && !string.IsNullOrEmpty(message.Value))
                return new ConsoleErrorResultModel(message.Value);
            lst.Add(ModuleInfoModel.FromDnnModuleInfo(movedModule));
            return new ConsoleResultModel("Successfully moved the module") { Data = lst };
        }
    }
}
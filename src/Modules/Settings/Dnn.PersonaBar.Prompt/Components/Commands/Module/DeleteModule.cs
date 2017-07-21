using System.Collections.Generic;
using System.Net;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("delete-module", "Delete a module instance", new[] { "id", "pageid" })]
    public class DeleteModule : ConsoleCommandBase
    {
        private const string FlagId = "id";
        private const string FlagPageid = "pageid";

        private int? ModuleId { get; set; }
        private int? PageId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagId), out tmpId))
                {
                    ModuleId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile), FlagId);
                }
            }
            else if (args.Length >= 2 && !IsFlag(args[1]))
            {
                int tmpId;
                if (int.TryParse(args[1], out tmpId))
                {
                    ModuleId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat(
                        Localization.GetString("Prompt_MainParamRequired", Constants.LocalResourcesFile), "Module Id",
                        FlagId);
                }
            }
            else
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_MainParamRequired", Constants.LocalResourcesFile), "Module Id", FlagId);
            }


            if (HasFlag(FlagPageid))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagPageid), out tmpId))
                {
                    if (tmpId > 0)
                    {
                        PageId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagPageid);
                    }
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile), FlagPageid);
                }
            }
            else
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagRequired", Constants.LocalResourcesFile), FlagPageid);
            }

            if (ModuleId <= 0)
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagId);
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            if (!ModuleId.HasValue || !PageId.HasValue) return new ConsoleErrorResultModel("Insufficient parameters");
            KeyValuePair<HttpStatusCode, string> message;
            ModulesController.Instance.DeleteModule(ModuleId.Value, PageId.Value, out message);
            return string.IsNullOrEmpty(message.Value)
                ? new ConsoleResultModel(Localization.GetString("Prompt_ModuleDeleted", Constants.LocalResourcesFile))
                : new ConsoleErrorResultModel(message.Value);
        }
    }
}
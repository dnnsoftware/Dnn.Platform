using System.Collections.Generic;
using System.Net;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("get-module", "Gets module information for module specified", new[] { "id" })]
    public class GetModule : ConsoleCommandBase
    {

        private const string FlagId = "id";

        private const string FlagPageid = "pageid";

        private int ModuleId { get; set; }
        private int PageId { get; set; }

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
                    sbErrors.AppendFormat(Localization.GetString("Prompt_MainParamRequired", Constants.LocalResourcesFile), "Module Id", FlagId);
                }
            }
            else
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_MainParamRequired", Constants.LocalResourcesFile), "Module Id", FlagId);
            }


            if (HasFlag(FlagPageid))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagPageid), out tmpId))
                {
                    PageId = tmpId;
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
            if (PageId <= 0)
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagPageid);
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<ModuleInfoModel>();
            KeyValuePair<HttpStatusCode, string> message;
            var moduleInfo = ModulesController.Instance.GetModule(ModuleId, PageId, out message);
            if (moduleInfo == null && !string.IsNullOrEmpty(message.Value))
            {
                return new ConsoleErrorResultModel(message.Value);
            }
            lst.Add(ModuleInfoModel.FromDnnModuleInfo(moduleInfo));
            return new ConsoleResultModel { Data = lst };
        }
    }
}
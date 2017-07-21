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
    [ConsoleCommand("copy-module", "Copies the module specified", new[] { "id", "pageid", "topageid", "pane" })]
    public class CopyModule : ConsoleCommandBase
    {
        private const string FlagId = "id";
        private const string FlagPageid = "pageid";
        private const string FlagTopageid = "topageid";
        private const string FlagPane = "pane";

        private const string FlagIncludesettings = "includesettings";

        private int ModuleId { get; set; }
        private int PageId { get; set; }
        private int TargetPageId { get; set; }
        private string Pane { get; set; }
        private bool? IncludeSettings { get; set; }

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
                int tmpId;
                if (int.TryParse(Flag(FlagPageid), out tmpId))
                {
                    PageId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile),
                        FlagPageid);
                }
            }
            else
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagRequired", Constants.LocalResourcesFile), FlagPageid);
            }

            if (HasFlag(FlagTopageid))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagTopageid), out tmpId))
                {
                    TargetPageId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile), FlagTopageid);
                }
            }
            else
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagRequired", Constants.LocalResourcesFile), FlagTopageid);
            }

            if (HasFlag(FlagPane))
                Pane = Flag(FlagPane);
            if (string.IsNullOrEmpty(Pane))
                Pane = "ContentPane";

            if (HasFlag(FlagIncludesettings))
            {
                bool tmpBool;
                if (bool.TryParse(Flag(FlagIncludesettings), out tmpBool))
                {
                    IncludeSettings = tmpBool;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotValidBool", Constants.LocalResourcesFile), FlagIncludesettings);
                }
            }
            if (!IncludeSettings.HasValue)
            {
                IncludeSettings = true;
            }

            if (ModuleId <= 0)
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagId);
            }
            if (PageId <= 0)
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagPageid);
            }
            if (TargetPageId <= 0)
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagTopageid);
            }
            if (PageId == TargetPageId)
            {
                sbErrors.Append(Localization.GetString("Prompt_SourceAndTargetPagesAreSame", Constants.LocalResourcesFile));
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<ModuleInfoModel>();
            KeyValuePair<HttpStatusCode, string> message;
            var copiedModule = ModulesController.Instance.CopyModule(PortalSettings, ModuleId, PageId, TargetPageId, Pane, IncludeSettings ?? true, out message);
            if (copiedModule == null && !string.IsNullOrEmpty(message.Value))
                return new ConsoleErrorResultModel(message.Value);
            lst.Add(ModuleInfoModel.FromDnnModuleInfo(copiedModule));
            return new ConsoleResultModel(Localization.GetString("Prompt_ModuleCopied", Constants.LocalResourcesFile)) { Data = lst };
        }
    }
}
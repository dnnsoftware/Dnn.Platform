using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("restore-module", "Restores a module from the DNN recycle bin", new[]
    {
        "id",
        "pageid"
    })]
    public class RestoreModule : ConsoleCommandBase
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
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile),
                        FlagId);
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
                        Localization.GetString("Prompt_MainParamRequired", Constants.LocalResourcesFile), "Module ID",
                        FlagId);
                }
            }

            if (HasFlag(FlagPageid))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagPageid), out tmpId))
                {
                    if (tmpId > 0)
                    {
                        PageId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat(
                            Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile),
                            FlagPageid);
                    }
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagNotInt", Constants.LocalResourcesFile),
                        FlagPageid);
                }
            }
            else
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagRequired", Constants.LocalResourcesFile),
                    FlagPageid);
            }

            if (ModuleId <= 0)
            {
                sbErrors.AppendFormat(
                    Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagId);
            }
            if (PageId <= 0)
            {
                sbErrors.AppendFormat(
                    Localization.GetString("Prompt_FlagNotPositiveInt", Constants.LocalResourcesFile), FlagPageid);
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            string message;
            var restored = RecyclebinController.Instance.RestoreModule(ModuleId, PageId, out message);
            return !restored
                ? new ConsoleErrorResultModel(message)
                : new ConsoleResultModel(
                    string.Format(
                        Localization.GetString("Prompt_ModuleRestoredSuccessfully", Constants.LocalResourcesFile),
                        ModuleId));
        }
    }
}
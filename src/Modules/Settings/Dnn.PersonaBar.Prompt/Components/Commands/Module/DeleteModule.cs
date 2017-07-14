using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    [ConsoleCommand("delete-module", "Delete a module instance", new[] { "id", "pageid" })]
    public class DeleteModule : ConsoleCommandBase
    {
        private const string FlagId = "id";
        private const string FlagPageid = "pageid";


        public int? ModuleId { get; private set; }
        public int? PageId { get; private set; }

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
                    sbErrors.AppendFormat("The --{0} flag must be an integer", FlagId);
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
                        sbErrors.AppendFormat("The Module ID is required. Please use the --{0} flag or pass it as the first argument after the command name", FlagId);
                    }
                }
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
                        sbErrors.AppendFormat("The --{0} flag value must be greater than 0", FlagPageid);
                    }
                }
                else
                {
                    sbErrors.AppendFormat("The --{0} flag value must be an integer", FlagPageid);
                }
            }

            if (ModuleId.HasValue && ModuleId <= 0)
            {
                sbErrors.Append("The Module's ID must be greater than 0");
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<ModuleInfoModel>();


            var results = ModuleController.Instance.GetAllTabsModulesByModuleID((int)ModuleId);
            if (results != null)
            {
                var module = (ModuleInfo)results[0];
                if (PageId.HasValue)
                {
                    // we can do a soft Delete
                    ModuleController.Instance.DeleteTabModule((int)PageId, (int)ModuleId, true);
                    return new ConsoleResultModel($"Module {ModuleId} sent to Recycle Bin");
                }
                else
                {
                    ModuleController.Instance.DeleteModule((int)ModuleId);
                    DataCache.ClearModuleCache(module.TabID);
                    return new ConsoleResultModel($"Module {ModuleId} permanently deleted");
                }
            }
            else
            {
                return new ConsoleResultModel($"No module found with ID '{ModuleId}'");
            }


        }

    }
}
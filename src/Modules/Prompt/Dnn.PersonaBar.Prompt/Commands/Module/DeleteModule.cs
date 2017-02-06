using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Module
{
    [ConsoleCommand("delete-module", "Delete a module instance", new string[] { "id", "pageid" })]
    public class DeleteModule : ConsoleCommandBase, IConsoleCommand
    {
        private const string FLAG_ID = "id";
        private const string FLAG_PAGEID = "pageid";

        public string ValidationMessage { get; private set; }
        public int? ModuleId { get; private set; }
        public int? PageId { get; private set; }

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
                    sbErrors.AppendFormat("The --{0} flag must be an integer", FLAG_ID);
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
                        sbErrors.AppendFormat("The Module ID is required. Please use the --{0} flag or pass it as the first argument after the command name", FLAG_ID);
                    }
                }
            }

            if (HasFlag(FLAG_PAGEID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_PAGEID), out tmpId))
                {
                    if (tmpId > 0)
                    {
                        PageId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat("The --{0} flag value must be greater than 0", FLAG_PAGEID);
                    }
                }
                else
                {
                    sbErrors.AppendFormat("The --{0} flag value must be an integer", FLAG_PAGEID);
                }
            }

            if (ModuleId.HasValue && ModuleId <= 0)
            {
                sbErrors.Append("The Module's ID must be greater than 0");
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


            var results = ModuleController.Instance.GetAllTabsModulesByModuleID((int)ModuleId);
            if (results != null)
            {
                if (PageId.HasValue)
                {
                    // we can do a soft Delete
                    ModuleController.Instance.DeleteTabModule((int)PageId, (int)ModuleId, true);
                    return new ConsoleResultModel(string.Format("Module {0} sent to Recycle Bin", ModuleId));
                }
                else
                {
                    ModuleController.Instance.DeleteModule((int)ModuleId);
                    return new ConsoleResultModel(string.Format("Module {0} permanently deleted", ModuleId));
                }
            }
            else
            {
                return new ConsoleResultModel(string.Format("No module found with ID '{0}'", ModuleId));
            }


        }

    }
}
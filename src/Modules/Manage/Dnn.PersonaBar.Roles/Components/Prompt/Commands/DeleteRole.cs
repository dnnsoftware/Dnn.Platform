using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("delete-role", "Deletes the specified DNN security role for this portal")]
    public class DeleteRole : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DeleteRole));
        private const string FlagId = "id";


        public int RoleId { get; private set; } = Convert.ToInt32(Globals.glbRoleNothing);

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);

            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                var tmp = 0;
                if (int.TryParse(Flag(FlagId), out tmp))
                {
                    RoleId = tmp;
                }
                else
                {
                    sbErrors.Append(Localization.GetString("Prompt_RoleIdNotInt", Constants.LocalResourcesFile));
                }
            }
            else
            {
                // assume it's the first argument
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    var tmp = 0;
                    if (int.TryParse(args[1], out tmp))
                    {
                        RoleId = tmp;
                    }
                }
            }

            if (RoleId == Convert.ToInt32(Globals.glbRoleNothing))
            {
                sbErrors.Append(Localization.GetString("Prompt_RoleIdIsRequired", Constants.LocalResourcesFile));
            }
            else if (RoleId <= 0)
            {
                // validate it's > 0
                sbErrors.AppendFormat(Localization.GetString("Prompt_RoleIdNegative", Constants.LocalResourcesFile));
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> message;
                var roleName = RolesController.Instance.DeleteRole(PortalSettings, RoleId, out message);
                return !string.IsNullOrEmpty(roleName)
                    ? new ConsoleResultModel($"{Localization.GetString("DeleteRole.Message", Constants.LocalResourcesFile)} '{roleName}' ({RoleId})") { Records = 1 }
                    : new ConsoleErrorResultModel(message.Value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(Localization.GetString("DeleteRole.Error", Constants.LocalResourcesFile));
            }
        }
    }
}
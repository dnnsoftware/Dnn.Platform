using System;
using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("delete-role", Constants.RolesCategory, "Prompt_DeleteRole_Description")]
    public class DeleteRole : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DeleteRole));
        [FlagParameter("id", "Prompt_DeleteRole_FlagId", "Integer", true)]
        private const string FlagId = "id";

        public int RoleId { get; private set; } = Convert.ToInt32(Globals.glbRoleNothing);

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            RoleId = GetFlagValue(FlagId, "Role Id", -1, true, true, true);
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> message;
                var roleName = RolesController.Instance.DeleteRole(PortalSettings, RoleId, out message);
                return !string.IsNullOrEmpty(roleName)
                    ? new ConsoleResultModel($"{LocalizeString("DeleteRole.Message")} '{roleName}' ({RoleId})") { Records = 1 }
                    : new ConsoleErrorResultModel(message.Value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(LocalizeString("DeleteRole.Error"));
            }
        }
    }
}
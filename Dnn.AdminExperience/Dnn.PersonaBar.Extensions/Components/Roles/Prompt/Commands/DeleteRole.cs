// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
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

    [ConsoleCommand("delete-role", Constants.RolesCategory, "Prompt_DeleteRole_Description")]
    public class DeleteRole : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_DeleteRole_FlagId", "Integer", true)]
        private const string FlagId = "id";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DeleteRole));

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public int RoleId { get; private set; } = Convert.ToInt32(Globals.glbRoleNothing);

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.RoleId = this.GetFlagValue(FlagId, "Role Id", -1, true, true, true);
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> message;
                var roleName = RolesController.Instance.DeleteRole(this.PortalSettings, this.RoleId, out message);
                return !string.IsNullOrEmpty(roleName)
                    ? new ConsoleResultModel($"{this.LocalizeString("DeleteRole.Message")} '{roleName}' ({this.RoleId})") { Records = 1 }
                    : new ConsoleErrorResultModel(message.Value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(this.LocalizeString("DeleteRole.Error"));
            }
        }
    }
}

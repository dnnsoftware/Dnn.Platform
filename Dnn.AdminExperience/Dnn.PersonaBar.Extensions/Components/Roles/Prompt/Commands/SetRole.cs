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
    using Dnn.PersonaBar.Roles.Components.Prompt.Exceptions;
    using Dnn.PersonaBar.Roles.Components.Prompt.Models;
    using Dnn.PersonaBar.Roles.Services.DTO;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;

    [ConsoleCommand("set-role", Constants.RolesCategory, "Prompt_SetRole_Description")]
    public class SetRole : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_SetRole_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("public", "Prompt_SetRole_FlagIsPublic", "Boolean")]
        private const string FlagIsPublic = "public";

        [FlagParameter("autoassign", "Prompt_SetRole_FlagAutoAssign", "Boolean")]
        private const string FlagAutoAssign = "autoassign";

        [FlagParameter("name", "Prompt_SetRole_FlagRoleName", "String")]
        private const string FlagRoleName = "name";

        [FlagParameter("description", "Prompt_SetRole_FlagDescription", "String")]
        private const string FlagDescription = "description";

        [FlagParameter("status", "Prompt_SetRole_FlagStatus", "Boolean")]
        private const string FlagStatus = "status";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SetRole));

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool? IsPublic { get; set; }
        public bool? AutoAssign { get; set; }
        public RoleStatus? Status { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.RoleId = this.GetFlagValue(FlagId, "Role Id", -1, true, true, true);
            this.RoleName = this.GetFlagValue(FlagRoleName, "Role Name", string.Empty);
            this.Description = this.GetFlagValue(FlagDescription, "Description", string.Empty);
            this.IsPublic = this.GetFlagValue<bool?>(FlagIsPublic, "Is Public", null);
            this.AutoAssign = this.GetFlagValue<bool?>(FlagAutoAssign, "Auto Assign", null);
            var status = this.GetFlagValue(FlagStatus, "Status", string.Empty);
            switch (status)
            {
                case "pending":
                    this.Status = RoleStatus.Pending;
                    break;
                case "approved":
                    this.Status = RoleStatus.Approved;
                    break;
                case "disabled":
                    this.Status = RoleStatus.Disabled;
                    break;
                default:
                    this.Status = null;
                    break;
            }

            if ((string.IsNullOrEmpty(this.RoleName)) && string.IsNullOrEmpty(this.Description) && !this.IsPublic.HasValue && !this.AutoAssign.HasValue && string.IsNullOrEmpty(status))
            {
                this.AddMessage(this.LocalizeString("Prompt_NothingToUpdate"));
            }
        }

        public override ConsoleResultModel Run()
        {
            try
            {

                var existingRole = RoleController.Instance.GetRoleById(this.PortalId, this.RoleId);
                if (existingRole.IsSystemRole)
                {
                    throw new SetRoleException("Cannot modify System Roles.");
                }
                var roleDto = new RoleDto
                {
                    Id = this.RoleId,
                    Name = !string.IsNullOrEmpty(this.RoleName) ? this.RoleName : existingRole?.RoleName,
                    Description = !string.IsNullOrEmpty(this.Description) ? this.Description : existingRole?.Description,
                    AutoAssign = this.AutoAssign ?? existingRole?.AutoAssignment ?? false,
                    IsPublic = this.IsPublic ?? existingRole?.IsPublic ?? false,
                    GroupId = existingRole?.RoleGroupID ?? -1,
                    IsSystem = existingRole?.IsSystemRole ?? false,
                    SecurityMode = existingRole?.SecurityMode ?? SecurityMode.SecurityRole,
                    Status = this.Status ?? (existingRole?.Status ?? RoleStatus.Approved)
                };
                KeyValuePair<HttpStatusCode, string> message;
                var success = RolesController.Instance.SaveRole(this.PortalSettings, roleDto, false, out message);
                if (!success) return new ConsoleErrorResultModel(message.Value);

                var lstResults = new List<RoleModel>
                {
                    new RoleModel(RoleController.Instance.GetRoleById(this.PortalId, roleDto.Id))
                };
                return new ConsoleResultModel(this.LocalizeString("RoleUpdated.Message")) { Data = lstResults, Records = lstResults.Count };
            }
            catch (SetRoleException se)
            {
                Logger.Error(se);
                return new ConsoleErrorResultModel(this.LocalizeString("RoleUpdated.SystemRoleError"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(this.LocalizeString("RoleUpdated.Error"));
            }
        }
    }
}

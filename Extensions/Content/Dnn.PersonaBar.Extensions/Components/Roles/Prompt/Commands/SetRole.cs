using System;
using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Roles.Components.Prompt.Models;
using Dnn.PersonaBar.Roles.Components.Prompt.Exceptions;
using Dnn.PersonaBar.Roles.Services.DTO;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("set-role", Constants.RolesCategory, "Prompt_SetRole_Description")]
    public class SetRole : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SetRole));
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


        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool? IsPublic { get; set; }
        public bool? AutoAssign { get; set; }
        public RoleStatus? Status { get; set; }


        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            RoleId = GetFlagValue(FlagId, "Role Id", -1, true, true, true);
            RoleName = GetFlagValue(FlagRoleName, "Role Name", string.Empty);
            Description = GetFlagValue(FlagDescription, "Description", string.Empty);
            IsPublic = GetFlagValue<bool?>(FlagIsPublic, "Is Public", null);
            AutoAssign = GetFlagValue<bool?>(FlagAutoAssign, "Auto Assign", null);
            var status = GetFlagValue(FlagStatus, "Status", string.Empty);
            switch (status)
            {
                case "pending":
                    Status = RoleStatus.Pending;
                    break;
                case "approved":
                    Status = RoleStatus.Approved;
                    break;
                case "disabled":
                    Status = RoleStatus.Disabled;
                    break;
                default:
                    Status = null;
                    break;
            }
            
            if ((string.IsNullOrEmpty(RoleName)) && string.IsNullOrEmpty(Description) && !IsPublic.HasValue && !AutoAssign.HasValue && string.IsNullOrEmpty(status))
            {
                AddMessage(LocalizeString("Prompt_NothingToUpdate"));
            }
        }

        public override ConsoleResultModel Run()
        {
            try
            {

                var existingRole = RoleController.Instance.GetRoleById(PortalId, RoleId);
                if (existingRole.IsSystemRole)
                {
                    throw new SetRoleException("Cannot modify System Roles.");
                }
                var roleDto = new RoleDto
                {
                    Id = RoleId,
                    Name = !string.IsNullOrEmpty(RoleName) ? RoleName : existingRole?.RoleName,
                    Description = !string.IsNullOrEmpty(Description) ? Description : existingRole?.Description,
                    AutoAssign = AutoAssign ?? existingRole?.AutoAssignment ?? false,
                    IsPublic = IsPublic ?? existingRole?.IsPublic ?? false,
                    GroupId = existingRole?.RoleGroupID ?? -1,
                    IsSystem = existingRole?.IsSystemRole ?? false,
                    SecurityMode = existingRole?.SecurityMode ?? SecurityMode.SecurityRole,
                    Status = Status ?? (existingRole?.Status ?? RoleStatus.Approved)
                };
                KeyValuePair<HttpStatusCode, string> message;
                var success = RolesController.Instance.SaveRole(PortalSettings, roleDto, false, out message);
                if (!success) return new ConsoleErrorResultModel(message.Value);

                var lstResults = new List<RoleModel>
                {
                    new RoleModel(RoleController.Instance.GetRoleById(PortalId, roleDto.Id))
                };
                return new ConsoleResultModel(LocalizeString("RoleUpdated.Message")) { Data = lstResults, Records = lstResults.Count };
            }
            catch (SetRoleException se)
            {
                Logger.Error(se);
                return new ConsoleErrorResultModel(LocalizeString("RoleUpdated.SystemRoleError"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(LocalizeString("RoleUpdated.Error"));
            }
        }
    }
}
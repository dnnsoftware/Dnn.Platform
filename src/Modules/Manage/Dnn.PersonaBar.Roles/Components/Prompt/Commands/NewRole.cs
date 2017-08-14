using System;
using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Roles.Components.Prompt.Models;
using Dnn.PersonaBar.Roles.Services.DTO;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("new-role", Constants.RolesCategory, "Prompt_NewRole_Description")]
    public class NewRole : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NewRole));

        [FlagParameter("public", "Prompt_NewRole_FlagIsPublic", "Boolean", "false")]
        private const string FlagIsPublic = "public";
        [FlagParameter("autoassign", "Prompt_NewRole_FlagAutoAssign", "Boolean", "false")]
        private const string FlagAutoAssign = "autoassign";
        [FlagParameter("name", "Prompt_NewRole_FlagRoleName", "String", true)]
        private const string FlagRoleName = "name";
        [FlagParameter("description", "Prompt_NewRole_FlagDescription", "String")]
        private const string FlagDescription = "description";
        [FlagParameter("status", "Prompt_NewRole_FlagStatus", "Boolean", "approved")]
        private const string FlagStatus = "status";


        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public bool AutoAssign { get; set; }
        public RoleStatus Status { get; set; }


        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            RoleName = GetFlagValue(FlagRoleName, "Rolename", string.Empty, true, true);
            Description = GetFlagValue(FlagDescription, "Description", string.Empty);
            IsPublic = GetFlagValue(FlagIsPublic, "Is Public", false);
            AutoAssign = GetFlagValue(FlagAutoAssign, "Auto Assign", false, true);
            var status = GetFlagValue(FlagStatus, "Status", "approved");
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
                    AddMessage(string.Format(LocalizeString("Prompt_InvalidRoleStatus"), FlagStatus));
                    break;
            }
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var lstResults = new List<RoleModel>();
                var roleDto = new RoleDto
                {
                    Id = Null.NullInteger,
                    Description = Description,
                    Status = Status,
                    Name = RoleName,
                    AutoAssign = AutoAssign,
                    IsPublic = IsPublic,
                    GroupId = -1,
                    IsSystem = false,
                    SecurityMode = SecurityMode.SecurityRole
                };
                KeyValuePair<HttpStatusCode, string> message;
                var success = RolesController.Instance.SaveRole(PortalSettings, roleDto, false, out message);
                if (!success) return new ConsoleErrorResultModel(message.Value);

                lstResults.Add(new RoleModel(RoleController.Instance.GetRoleById(PortalId, roleDto.Id)));
                return new ConsoleResultModel(LocalizeString("RoleAdded.Message")) { Data = lstResults, Records = lstResults.Count };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(LocalizeString("RoleAdded.Error"));
            }

        }
    }
}
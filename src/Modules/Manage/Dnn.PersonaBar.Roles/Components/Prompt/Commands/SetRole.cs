using System;
using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Roles.Components.Prompt.Models;
using Dnn.PersonaBar.Roles.Services.DTO;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("set-role", "Update a DNN security role with new data", new[]{
        "id",
        "name",
        "description",
        "public"
    })]
    public class SetRole : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SetRole));
        private const string FlagId = "id";
        private const string FlagIsPublic = "public";
        private const string FlagAutoAssign = "autoassign";
        private const string FlagRoleName = "name";
        private const string FlagDescription = "description";


        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool? IsPublic { get; set; }
        public bool? AutoAssign { get; set; }


        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            RoleId = GetFlagValue(FlagId, "Role Id", -1, true, true, true);
            RoleName = GetFlagValue(FlagRoleName, "Role Name", string.Empty);
            Description = GetFlagValue(FlagDescription, "Description", string.Empty);
            IsPublic = GetFlagValue<bool?>(FlagIsPublic, "Is Public", null);
            AutoAssign = GetFlagValue<bool?>(FlagAutoAssign, "Auto Assign", null);

            if (string.IsNullOrEmpty(RoleName) && string.IsNullOrEmpty(Description) && !IsPublic.HasValue && !AutoAssign.HasValue)
            {
                AddMessage(LocalizeString("Prompt_NothingToUpdate"));
            }
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var existingRole = RoleController.Instance.GetRoleById(PortalId, RoleId);
                var roleDto = new RoleDto
                {
                    Id = RoleId,
                    Name = !string.IsNullOrEmpty(RoleName) ? RoleName : existingRole?.RoleName,
                    Description = !string.IsNullOrEmpty(Description) ? Description : existingRole?.Description,
                    AutoAssign = AutoAssign ?? existingRole?.AutoAssignment ?? false,
                    IsPublic = IsPublic ?? existingRole?.IsPublic ?? false,
                    GroupId = existingRole?.RoleGroupID ?? -1,
                    IsSystem = existingRole?.IsSystemRole ?? false,
                    SecurityMode = existingRole?.SecurityMode ?? SecurityMode.SecurityRole
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
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(LocalizeString("RoleUpdated.Error"));
            }
        }
    }
}
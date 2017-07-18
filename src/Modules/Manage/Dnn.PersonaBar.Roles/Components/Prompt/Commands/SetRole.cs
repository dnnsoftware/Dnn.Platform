using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Roles.Components.Prompt.Models;
using Dnn.PersonaBar.Roles.Services.DTO;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;

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
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SetRole));
        private const string FlagId = "id";
        private const string FlagIsPublic = "public";
        private const string FlagAutoAssign = "autoassign";
        private const string FlagRoleName = "name";
        private const string FlagDescription = "description";


        public int? RoleId { get; private set; }
        public string RoleName { get; private set; }
        public string Description { get; private set; }
        public bool? IsPublic { get; private set; }
        public bool? AutoAssign { get; private set; }


        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);

            var sbErrors = new StringBuilder();
            if (HasFlag(FlagId))
            {
                int tmpId;
                if (!int.TryParse(Flag(FlagId), out tmpId))
                {
                    sbErrors.Append(Localization.GetString("Prompt_RoleIdNotInt", Constants.LocalResourcesFile));
                }
                else
                {
                    RoleId = tmpId;
                }
            }
            else
            {
                int tempId;
                if (!int.TryParse(args[1], out tempId))
                {
                    sbErrors.Append(Localization.GetString("Prompt_RoleIdIsRequired", Constants.LocalResourcesFile));
                }
                else
                {
                    RoleId = tempId;
                }
            }

            if (HasFlag(FlagRoleName))
                RoleName = Flag(FlagRoleName);
            if (HasFlag(FlagDescription))
                Description = Flag(FlagDescription);

            if (HasFlag(FlagIsPublic))
            {
                bool tmpPublic;
                if (!bool.TryParse(Flag(FlagIsPublic), out tmpPublic))
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_UnableToParseBool", Constants.LocalResourcesFile), FlagIsPublic, Flag(FlagIsPublic));
                }
                else
                {
                    IsPublic = tmpPublic;
                }
            }

            if (HasFlag(FlagAutoAssign))
            {
                bool tmpAuto;
                if (!bool.TryParse(Flag(FlagAutoAssign), out tmpAuto))
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_UnableToParseBool", Constants.LocalResourcesFile), FlagAutoAssign, Flag(FlagAutoAssign));
                }
                else
                {
                    AutoAssign = tmpAuto;
                }
            }

            if (RoleName == null && Description == null && !IsPublic.HasValue && !AutoAssign.HasValue)
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_NothingToUpdate", Constants.LocalResourcesFile));
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var existingRole = RoleController.Instance.GetRoleById(PortalId, (int)RoleId);
                var roleDto = new RoleDto
                {
                    Id = (int)RoleId,
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
                return new ConsoleResultModel(Localization.GetString("RoleUpdated.Message", Constants.LocalResourcesFile)) { Data = lstResults };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(Localization.GetString("RoleUpdated.Error", Constants.LocalResourcesFile));
            }
        }
    }
}
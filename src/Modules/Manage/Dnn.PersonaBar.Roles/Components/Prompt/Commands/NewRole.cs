using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("new-role", "Creates a new DNN security roles in the portal.", new[]{
        "name",
        "description",
        "public",
        "autoassign"
    })]
    public class NewRole : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NewRole));

        private const string FlagIsPublic = "public";
        private const string FlagAutoAssign = "autoassign";
        private const string FlagRoleName = "name";
        private const string FlagDescription = "description";
        private const string FlagStatus = "status";


        public string RoleName { get; private set; }
        public string Description { get; private set; }
        public bool IsPublic { get; private set; }
        public bool AutoAssign { get; private set; }
        public RoleStatus Status { get; private set; }


        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagRoleName))
            {
                if (string.IsNullOrEmpty(Flag(FlagRoleName)))
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagEmpty", Constants.LocalResourcesFile), FlagRoleName);
                }
                else
                {
                    // non-empty roles flag.
                    RoleName = Flag(FlagRoleName);
                }
            }
            else
            {
                // role name is assumed to be the first argument
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    RoleName = args[1];
                }
            }

            if (string.IsNullOrEmpty(RoleName))
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_RoleNameRequired", Constants.LocalResourcesFile), FlagRoleName);
            }

            if (HasFlag(FlagDescription))
            {
                Description = Flag(FlagDescription);
            }
            if (Description == null)
                Description = string.Empty;

            if (HasFlag(FlagIsPublic))
            {
                bool tmpPublic;
                if (bool.TryParse(Flag(FlagIsPublic), out tmpPublic))
                {
                    IsPublic = tmpPublic;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_UnableToParseBool", Constants.LocalResourcesFile), FlagIsPublic, Flag(FlagIsPublic));
                }
            }

            if (HasFlag(FlagAutoAssign))
            {
                bool tmpAutoAssign;
                if (bool.TryParse(Flag(FlagAutoAssign), out tmpAutoAssign))
                {
                    AutoAssign = tmpAutoAssign;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_UnableToParseBool", Constants.LocalResourcesFile), FlagAutoAssign, Flag(FlagAutoAssign));
                }
            }

            if (HasFlag(FlagStatus))
            {
                var status = Flag(FlagStatus).ToLower();
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
                        sbErrors.AppendFormat(Localization.GetString("Prompt_InvalidRoleStatus", Constants.LocalResourcesFile), Flag(FlagStatus), FlagStatus);
                        break;
                }
            }
            else
            {
                // default to 'approved'
                Status = RoleStatus.Approved;
            }
            ValidationMessage = sbErrors.ToString();
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
                return new ConsoleResultModel(Localization.GetString("RoleAdded.Message", Constants.LocalResourcesFile)) { Data = lstResults };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(Localization.GetString("RoleAdded.Error", Constants.LocalResourcesFile));
            }

        }
    }
}
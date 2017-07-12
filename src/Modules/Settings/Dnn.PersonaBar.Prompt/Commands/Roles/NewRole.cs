using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Prompt.Commands.Roles
{
    [ConsoleCommand("new-role", "Creates a new DNN security roles in the portal.", new[]{
        "name",
        "description",
        "public",
        "autoassign"
    })]
    public class NewRole : ConsoleCommandBase
    {

        private const string FlagIsPublic = "public";
        private const string FlagAutoAssign = "autoassign";
        private const string FlagRoleName = "name";
        private const string FlagDescription = "description";
        private const string FlagStatus = "status";


        public string RoleName { get; private set; }
        public string Description { get; private set; }
        public bool? IsPublic { get; private set; }
        public bool? AutoAssign { get; private set; }
        public RoleStatus Status { get; private set; }


        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagRoleName))
            {
                if (string.IsNullOrEmpty(Flag(FlagRoleName)))
                {
                    sbErrors.AppendFormat("--{0} cannot be empty; ", FlagRoleName);
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
                sbErrors.AppendFormat("You must specify a name for the role as the first argument or by using the --{0} flag. " + "Names with spaces And special characters should be enclosed in double quotes.", FlagRoleName);
            }

            if (HasFlag(FlagDescription))
            {
                Description = Flag(FlagDescription);
            }
            if (Description == null)
                Description = string.Empty;

            if (HasFlag(FlagIsPublic))
            {
                var tmpPublic = false;
                if (bool.TryParse(Flag(FlagIsPublic), out tmpPublic))
                {
                    IsPublic = tmpPublic;
                }
                else
                {
                    sbErrors.AppendFormat("Unable to parse the --{0} flag value '{1}'. Value should be True or False; ", FlagIsPublic, Flag(FlagIsPublic));
                }
            }
            if (!IsPublic.HasValue)
                IsPublic = false;

            if (HasFlag(FlagAutoAssign))
            {
                var tmpAutoAssign = false;
                if (bool.TryParse(Flag(FlagAutoAssign), out tmpAutoAssign))
                {
                    AutoAssign = tmpAutoAssign;
                }
                else
                {
                    sbErrors.AppendFormat("Unable to parse the --{0} flag value '{1}'. Value should be True or False; ", FlagAutoAssign, Flag(FlagAutoAssign));
                }
            }
            if (!AutoAssign.HasValue)
                AutoAssign = false;

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
                        sbErrors.AppendFormat("Invalid value '{0}' passed for --{1}. Expecting 'pending', 'approved', or 'disabled'", Flag(FlagStatus), FlagStatus);
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

            var sbErrors = new StringBuilder();

            try
            {
                var lstResults = new List<RoleModel>();

                // only act if role doesn't yet exist
                if (Prompt.Utilities.RoleExists(RoleName, PortalId))
                {
                    return new ConsoleErrorResultModel(
                        $"Cannot create role: A role with the name '{RoleName}' already exists.");
                }

                var newRole = Prompt.Utilities.CreateRole(RoleName, PortalId, Status, Description, (bool)IsPublic, (bool)AutoAssign);
                if (newRole != null)
                {
                    lstResults.Add(new RoleModel(newRole));
                }

                return new ConsoleResultModel("Role successfully created.") { Data = lstResults };
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return new ConsoleErrorResultModel("An error occurred while trying to create the role. Please see the event viewer for details.");
            }

        }
    }
}
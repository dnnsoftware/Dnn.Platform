using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Role
{
    [ConsoleCommand("new-role", "Creates a new DNN security roles in the portal.", new string[]{
        "name",
        "description",
        "public",
        "autoassign"
    })]
    public class NewRole : BaseConsoleCommand, IConsoleCommand
    {

        private const string FLAG_IS_PUBLIC = "public";
        private const string FLAG_AUTO_ASSIGN = "autoassign";
        private const string FLAG_ROLE_NAME = "name";
        private const string FLAG_DESCRIPTION = "description";

        public string ValidationMessage { get; private set; }
        public string RoleName { get; private set; }
        public string Description { get; private set; }
        public bool? IsPublic { get; private set; }
        public bool? AutoAssign { get; private set; }


        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            if (HasFlag(FLAG_ROLE_NAME))
            {
                if (string.IsNullOrEmpty(Flag(FLAG_ROLE_NAME)))
                {
                    sbErrors.AppendFormat("--{0} cannot be empty; ", FLAG_ROLE_NAME);
                }
                else
                {
                    // non-empty roles flag.
                    RoleName = Flag(FLAG_ROLE_NAME);
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
                sbErrors.AppendFormat("You must specify a name for the role as the first argument or by using the --{0} flag. " + "Names with spaces And special characters should be enclosed in double quotes.", FLAG_ROLE_NAME);
            }

            if (HasFlag(FLAG_DESCRIPTION))
            {
                Description = Flag(FLAG_DESCRIPTION);
            }
            if (Description == null)
                Description = string.Empty;

            if (HasFlag(FLAG_IS_PUBLIC))
            {
                bool tmpPublic = false;
                if (bool.TryParse(Flag(FLAG_IS_PUBLIC), out tmpPublic))
                {
                    IsPublic = tmpPublic;
                }
                else
                {
                    sbErrors.AppendFormat("Unable to parse the --{0} flag value '{1}'. Value should be True or False; ", FLAG_IS_PUBLIC, Flag(FLAG_IS_PUBLIC));
                }
            }
            if (!IsPublic.HasValue)
                IsPublic = false;

            if (HasFlag(FLAG_AUTO_ASSIGN))
            {
                bool tmpAutoAssign = false;
                if (bool.TryParse(Flag(FLAG_AUTO_ASSIGN), out tmpAutoAssign))
                {
                    AutoAssign = tmpAutoAssign;
                }
                else
                {
                    sbErrors.AppendFormat("Unable to parse the --{0} flag value '{1}'. Value should be True or False; ", FLAG_AUTO_ASSIGN, Flag(FLAG_AUTO_ASSIGN));
                }
            }
            if (!AutoAssign.HasValue)
                AutoAssign = false;

            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {

            StringBuilder sbErrors = new StringBuilder();

            try
            {
                List<RoleModel> lstResults = new List<RoleModel>();

                // only act if role doesn't yet exist
                if (Prompt.Utilities.RoleExists(RoleName, PortalId))
                {
                    return new ConsoleErrorResultModel(string.Format("Cannot create role: A role with the name '{0}' already exists.", RoleName));
                }

                var newRole = Prompt.Utilities.CreateRole(RoleName, PortalId, Description, (bool)IsPublic, (bool)AutoAssign);
                if (newRole != null)
                {
                    lstResults.Add(new RoleModel(newRole));
                }

                return new ConsoleResultModel("Role successfully created.") { data = lstResults };
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return new ConsoleErrorResultModel("An error occurred while trying to create the role. Please see the event viewer for details.");
            }

        }
    }
}
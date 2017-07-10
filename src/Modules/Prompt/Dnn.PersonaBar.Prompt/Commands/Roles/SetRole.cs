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
    [ConsoleCommand("set-role", "Update a DNN security role with new data", new string[]{
        "id",
        "name",
        "description",
        "public"
    })]
    public class SetRole : ConsoleCommandBase, IConsoleCommand
    {

        private const string FLAG_ID = "id";
        private const string FLAG_IS_PUBLIC = "public";
        private const string FLAG_AUTO_ASSIGN = "autoassign";
        private const string FLAG_ROLE_NAME = "name";
        private const string FLAG_DESCRIPTION = "description";

        public string ValidationMessage { get; private set; }
        public int? RoleId { get; private set; }
        public string RoleName { get; private set; }
        public string Description { get; private set; }
        public bool? IsPublic { get; private set; }
        public bool? AutoAssign { get; private set; }


        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);

            StringBuilder sbErrors = new StringBuilder();
            if (HasFlag(FLAG_ID))
            {
                int tmpId = 0;
                if (!int.TryParse(Flag(FLAG_ID), out tmpId))
                {
                    sbErrors.AppendFormat("The --{0} flag's value must be a valid numeric Role ID; ", FLAG_ID);
                }
                else
                {
                    RoleId = tmpId;
                }
            }
            else
            {
                int tempId = 0;
                if (!int.TryParse(args[1], out tempId))
                {
                    sbErrors.AppendFormat("No valid Role ID was passed. Pass it using the --{0} flag or as the first argument; ", FLAG_ID);
                }
                else
                {
                    RoleId = tempId;
                }
            }

            if (HasFlag(FLAG_ROLE_NAME))
                RoleName = Flag(FLAG_ROLE_NAME);
            if (HasFlag(FLAG_DESCRIPTION))
                Description = Flag(FLAG_DESCRIPTION);

            if (HasFlag(FLAG_IS_PUBLIC))
            {
                bool tmpPublic = false;
                if (!bool.TryParse(Flag(FLAG_IS_PUBLIC), out tmpPublic))
                {
                    sbErrors.AppendFormat("You must pass True or False for the --{0} flag; ", FLAG_IS_PUBLIC);
                }
                else
                {
                    IsPublic = tmpPublic;
                }
            }

            if (HasFlag(FLAG_AUTO_ASSIGN))
            {
                bool tmpAuto = false;
                if (!bool.TryParse(Flag(FLAG_AUTO_ASSIGN), out tmpAuto))
                {
                    sbErrors.AppendFormat("You must pass True or False for the --{0} flag; ", FLAG_AUTO_ASSIGN);
                }
                else
                {
                    AutoAssign = tmpAuto;
                }
            }

            if (RoleName == null && Description == null && (!IsPublic.HasValue) && (!AutoAssign.HasValue))
            {
                sbErrors.AppendFormat("Nothing to Update! Tell me what to update with flags like --{0} --{1} --{2} --{3}, etc.", FLAG_ROLE_NAME, FLAG_DESCRIPTION, FLAG_IS_PUBLIC, FLAG_AUTO_ASSIGN);
            }
            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            StringBuilder sbMessage = new StringBuilder();
            try
            {
                var role = Prompt.Utilities.GetRoleById((int)RoleId, PortalId);
                if (role == null)
                {
                    return new ConsoleErrorResultModel(string.Format("Unable to find a role with the ID of '{0}'", RoleId));
                }

                // Do not modify any system roles
                if (role.IsSystemRole)
                {
                    return new ConsoleErrorResultModel("System roles cannot be modified through Prompt.");
                }

                if (RoleName != null)
                    role.RoleName = RoleName;
                if (Description != null)
                    role.Description = Description;
                if (IsPublic.HasValue)
                    role.IsPublic = (bool)IsPublic;
                if (AutoAssign.HasValue)
                    role.AutoAssignment = (bool)AutoAssign;

                // update the role
                var updatedRole = Prompt.Utilities.UpdateRole(role);

                var lstResults = new List<RoleModel>();
                lstResults.Add(new RoleModel(updatedRole));
                return new ConsoleResultModel("Role has been updated") { data = lstResults };
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return new ConsoleErrorResultModel("An unexpected error has occurred, please see the Event Viewer for more details");
        }


    }
}
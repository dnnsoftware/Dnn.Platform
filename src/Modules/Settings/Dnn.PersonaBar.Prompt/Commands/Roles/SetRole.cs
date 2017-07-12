using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Commands.Roles
{
    [ConsoleCommand("set-role", "Update a DNN security role with new data", new[]{
        "id",
        "name",
        "description",
        "public"
    })]
    public class SetRole : ConsoleCommandBase
    {

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
                var tmpId = 0;
                if (!int.TryParse(Flag(FlagId), out tmpId))
                {
                    sbErrors.AppendFormat("The --{0} flag's value must be a valid numeric Role ID; ", FlagId);
                }
                else
                {
                    RoleId = tmpId;
                }
            }
            else
            {
                var tempId = 0;
                if (!int.TryParse(args[1], out tempId))
                {
                    sbErrors.AppendFormat("No valid Role ID was passed. Pass it using the --{0} flag or as the first argument; ", FlagId);
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
                var tmpPublic = false;
                if (!bool.TryParse(Flag(FlagIsPublic), out tmpPublic))
                {
                    sbErrors.AppendFormat("You must pass True or False for the --{0} flag; ", FlagIsPublic);
                }
                else
                {
                    IsPublic = tmpPublic;
                }
            }

            if (HasFlag(FlagAutoAssign))
            {
                var tmpAuto = false;
                if (!bool.TryParse(Flag(FlagAutoAssign), out tmpAuto))
                {
                    sbErrors.AppendFormat("You must pass True or False for the --{0} flag; ", FlagAutoAssign);
                }
                else
                {
                    AutoAssign = tmpAuto;
                }
            }

            if (RoleName == null && Description == null && (!IsPublic.HasValue) && (!AutoAssign.HasValue))
            {
                sbErrors.AppendFormat("Nothing to Update! Tell me what to update with flags like --{0} --{1} --{2} --{3}, etc.", FlagRoleName, FlagDescription, FlagIsPublic, FlagAutoAssign);
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var sbMessage = new StringBuilder();
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
                return new ConsoleResultModel("Role has been updated") { Data = lstResults };
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return new ConsoleErrorResultModel("An unexpected error has occurred, please see the Event Viewer for more details");
        }


    }
}
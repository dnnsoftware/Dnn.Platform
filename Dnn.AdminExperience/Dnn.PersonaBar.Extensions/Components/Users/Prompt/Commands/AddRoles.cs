// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Roles.Components;
    using Dnn.PersonaBar.Users.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    using Constants = Dnn.PersonaBar.Users.Components.Constants;

    [ConsoleCommand("add-roles", Constants.UsersCategory, "Prompt_AddRoles_Description")]
    public class AddRoles : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_AddRoles_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("roles", "Prompt_AddRoles_FlagRoles", "String", true)]
        private const string FlagRoles = "roles";

        [FlagParameter("start", "Prompt_AddRoles_FlagStart", "DateTime")]
        private const string FlagStart = "start";

        [FlagParameter("end", "Prompt_AddRoles_FlagEnd", "DateTime")]
        private const string FlagEnd = "end";

        private IUserValidator _userValidator;
        private IUsersController _usersController;
        private IRolesController _rolesController;

        public AddRoles() : this(new UserValidator(), UsersController.Instance, RolesController.Instance)
        {
        }

        public AddRoles(IUserValidator userValidator, IUsersController userController, IRolesController rolesController)
        {
            this._userValidator = userValidator;
            this._usersController = userController;
            this._rolesController = rolesController;
        }

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int UserId { get; set; }
        private string Roles { get; set; }
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            this.UserId = this.GetFlagValue(FlagId, "User Id", -1, true, true, true);
            this.Roles = this.GetFlagValue(FlagRoles, "Roles", string.Empty, true);
            this.StartDate = this.GetFlagValue<DateTime?>(FlagStart, "Start Date", null);
            this.EndDate = this.GetFlagValue<DateTime?>(FlagEnd, "End Date", null);
            // validate end date is beyond the start date
            if (this.StartDate.HasValue && this.EndDate.HasValue)
            {
                if (this.EndDate < this.StartDate)
                {
                    this.AddMessage(this.LocalizeString("Prompt_StartDateGreaterThanEnd") + " ");
                }
            }
        }

        public override ConsoleResultModel Run()
        {
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;

            this.checkRoles();

            if ((errorResultModel = this._userValidator.ValidateUser(this.UserId, this.PortalSettings, this.User, out userInfo)) != null)
            {
                return errorResultModel;
            }

            try
            {
                this._usersController.AddUserToRoles(this.User, userInfo.UserID, userInfo.PortalID, this.Roles, ",", this.StartDate, this.EndDate);
                int totalRoles;
                var userRoles = this._usersController.GetUserRoles(userInfo, "", out totalRoles).Select(UserRoleModel.FromDnnUserRoleInfo).ToList();
                return new ConsoleResultModel(string.Empty) { Data = userRoles, Output = "Total Roles: " + totalRoles, Records = userRoles.Count };
            }
            catch (Exception ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }

        private void checkRoles()
        {
            IList<string> rolesFilter = new List<string>();
            if (!string.IsNullOrWhiteSpace(this.Roles))
            {
                this.Roles.Split(',').ToList().ForEach((role) => rolesFilter.Add(role.Trim()));
            }
            if (rolesFilter.Count() > 0)
            {
                IList<RoleInfo> foundRoles = this._rolesController.GetRolesByNames(this.PortalSettings, -1, rolesFilter);
                HashSet<string> foundRolesNames = new HashSet<string>(foundRoles.Select(role => role.RoleName));
                HashSet<string> roleFiltersSet = new HashSet<string>(rolesFilter);
                roleFiltersSet.ExceptWith(foundRolesNames);

                int notFoundCount = roleFiltersSet.Count();

                if (notFoundCount > 0)
                {
                    throw new Exception(string.Format(this.LocalizeString("Prompt_AddRoles_NotFound"), notFoundCount > 1 ? "s" : "", string.Join(",", roleFiltersSet)));
                }
            }
        }
    }
}

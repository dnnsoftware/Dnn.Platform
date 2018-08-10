using System;
using System.Linq;
using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using Dnn.PersonaBar.Roles.Components;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("add-roles", Constants.UsersCategory, "Prompt_AddRoles_Description")]
    public class AddRoles : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

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

        private int UserId { get; set; }
        private string Roles { get; set; }
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }

        public AddRoles() : this(new UserValidator(), UsersController.Instance, RolesController.Instance)
        {
        }

        public AddRoles(IUserValidator userValidator, IUsersController userController, IRolesController rolesController)
        {
            this._userValidator = userValidator;
            this._usersController = userController;
            this._rolesController = rolesController;
        }

        private void checkRoles()
        {
            IList<string> rolesFilter = new List<string>();
            if (!string.IsNullOrWhiteSpace(Roles))
            {
                Roles.Split(',').ToList().ForEach((role) => rolesFilter.Add(role.Trim()));
            }
            if (rolesFilter.Count() > 0)
            {
                IList<RoleInfo> foundRoles = _rolesController.GetRolesByNames(PortalSettings, -1, rolesFilter);            
                HashSet<string> foundRolesNames = new HashSet<string>(foundRoles.Select(role => role.RoleName));
                HashSet<string> roleFiltersSet = new HashSet<string>(rolesFilter);
                roleFiltersSet.ExceptWith(foundRolesNames);

                int notFoundCount = roleFiltersSet.Count();

                if (notFoundCount > 0)
                {
                    throw new Exception(string.Format(LocalizeString("Prompt_AddRoles_NotFound"), notFoundCount > 1 ? "s" : "", string.Join(",", roleFiltersSet)));
                }
            }
        }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {            
            UserId = GetFlagValue(FlagId, "User Id", -1, true, true, true);
            Roles = GetFlagValue(FlagRoles, "Roles", string.Empty, true);
            StartDate = GetFlagValue<DateTime?>(FlagStart, "Start Date", null);
            EndDate = GetFlagValue<DateTime?>(FlagEnd, "End Date", null);
            // validate end date is beyond the start date
            if (StartDate.HasValue && EndDate.HasValue)
            {
                if (EndDate < StartDate)
                {
                    AddMessage(LocalizeString("Prompt_StartDateGreaterThanEnd") + " ");
                }
            }
        }

        public override ConsoleResultModel Run()
        {
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;

            checkRoles();

            if ((errorResultModel = _userValidator.ValidateUser(UserId, PortalSettings, User, out userInfo)) != null)
            {
                return errorResultModel;
            }

            try
            {
                _usersController.AddUserToRoles(User, userInfo.UserID, userInfo.PortalID, Roles, ",", StartDate, EndDate);
                int totalRoles;
                var userRoles = _usersController.GetUserRoles(userInfo, "", out totalRoles).Select(UserRoleModel.FromDnnUserRoleInfo).ToList();
                return new ConsoleResultModel(string.Empty) { Data = userRoles, Output = "Total Roles: " + totalRoles, Records = userRoles.Count };
            }
            catch (Exception ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }
    }
}
using System;
using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

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


        private int UserId { get; set; }
        private string Roles { get; set; }
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }

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
            if ((errorResultModel = Utilities.ValidateUser(UserId, PortalSettings, User, out userInfo)) != null) return errorResultModel;
            try
            {
                UsersController.Instance.AddUserToRoles(User, userInfo.UserID, PortalId, Roles, ",", StartDate, EndDate);
                int totalRoles;
                var userRoles = UsersController.Instance.GetUserRoles(userInfo, "", out totalRoles).Select(UserRoleModel.FromDnnUserRoleInfo).ToList();
                return new ConsoleResultModel(string.Empty) { Data = userRoles, Output = "Total Roles: " + totalRoles, Records = userRoles.Count };
            }
            catch (Exception ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }
    }
}
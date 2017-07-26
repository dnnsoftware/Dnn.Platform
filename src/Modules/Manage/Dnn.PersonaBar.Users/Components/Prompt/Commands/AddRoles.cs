using System;
using System.Linq;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("add-roles", "Adds one or more DNN security roles to a user.", new[]{
        "id",
        "roles",
        "start",
        "end"
    })]
    public class AddRoles : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private const string FlagId = "id";
        private const string FlagRoles = "roles";
        private const string FlagStart = "start";
        private const string FlagEnd = "end";


        public int UserId { get; private set; }
        public string Roles { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            UserId = GetFlagValue(FlagId, "User Id", -1, true, true, true);
            Roles = GetFlagValue(FlagRoles, "Roles", string.Empty, true);
            StartDate = GetFlagValue<DateTime?>(FlagStart, "Start Date", null);
            EndDate = GetFlagValue<DateTime?>(FlagRoles, "End Date", null);
            var sbErrors = new StringBuilder();
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
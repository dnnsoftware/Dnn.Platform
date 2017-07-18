using System;
using System.Linq;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

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
        private const string FlagId = "id";
        private const string FlagRoles = "roles";
        private const string FlagStart = "start";
        private const string FlagEnd = "end";


        public int? UserId { get; private set; }
        public string Roles { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagId), out tmpId))
                    UserId = tmpId;
            }
            else
            {
                var tmpId = 0;
                if (int.TryParse(args[1], out tmpId))
                    UserId = tmpId;
            }

            if (!UserId.HasValue)
            {
                sbErrors.Append(Localization.GetString("Prompt_UserIdIsRequired", Constants.LocalResourcesFile) + " ");
            }

            if (HasFlag(FlagRoles))
            {
                if (string.IsNullOrEmpty(Flag(FlagRoles)))
                {
                    sbErrors.Append(Localization.GetString("Prompt_RolesEmpty", Constants.LocalResourcesFile) + " ");
                }
                else
                {
                    // non-empty roles flag.
                    Roles = Flag(FlagRoles);
                }
            }
            else if (HasFlag("role"))
            {
                sbErrors.Append(string.Format(Localization.GetString("Prompt_InvalidFlag", Constants.LocalResourcesFile), "role", "roles") + " ");
            }

            if (HasFlag(FlagStart))
            {
                DateTime tmpDate;
                if (DateTime.TryParse(Flag(FlagStart), out tmpDate))
                {
                    StartDate = tmpDate;
                }
                else
                {
                    sbErrors.Append(string.Format(Localization.GetString("Prompt_DateParseError", Constants.LocalResourcesFile), "Start", Flag(FlagStart)) + " ");
                }
            }

            if (HasFlag(FlagEnd))
            {
                DateTime tmpDate;
                if (DateTime.TryParse(Flag(FlagEnd), out tmpDate))
                {
                    EndDate = tmpDate;
                }
                else
                {
                    sbErrors.Append(string.Format(Localization.GetString("Prompt_DateParseError", Constants.LocalResourcesFile), "End", Flag(FlagEnd)) + " ");
                }
            }

            // validate end date is beyond the start date
            if (StartDate.HasValue && EndDate.HasValue)
            {
                if (EndDate < StartDate)
                {
                    sbErrors.Append(Localization.GetString("Prompt_StartDateGreaterThanEnd", Constants.LocalResourcesFile) + " ");
                }
            }

            ValidationMessage = sbErrors.ToString();
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
                return new ConsoleResultModel(string.Empty) { Data = userRoles, Output = "Total Roles: " + totalRoles };
            }
            catch (Exception ex)
            {
                return new ConsoleErrorResultModel(ex.Message);
            }
        }
    }
}
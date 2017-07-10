using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.User
{
    [ConsoleCommand("add-roles", "Adds one or more DNN security roles to a user.", new string[]{
        "id",
        "roles",
        "start",
        "end"
    })]
    public class AddRoles : ConsoleCommandBase, IConsoleCommand
    {
        private const string FLAG_ID = "id";
        private const string FLAG_ROLES = "roles";
        private const string FLAG_START = "start";
        private const string FLAG_END = "end";

        public string ValidationMessage { get; private set; }
        public int? UserId { get; private set; }
        public string Roles { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            if (HasFlag(FLAG_ID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_ID), out tmpId))
                    UserId = tmpId;
            }
            else
            {
                int tmpId = 0;
                if (int.TryParse(args[1], out tmpId))
                    UserId = tmpId;
            }

            if (!UserId.HasValue)
            {
                sbErrors.Append("You must specify a valid User ID as either the first argument or using the --id flag; ");
            }

            if (HasFlag(FLAG_ROLES))
            {
                if (string.IsNullOrEmpty(Flag(FLAG_ROLES)))
                {
                    sbErrors.Append("--roles cannot be empty; ");
                }
                else
                {
                    // non-empty roles flag.
                    Roles = Flag(FLAG_ROLES);
                }
            }
            else if (HasFlag("role"))
            {
                sbErrors.Append("Invalid flag '--role'. Did you mean --roles ?");
            }

            if (HasFlag(FLAG_START))
            {
                System.DateTime tmpDate = default(System.DateTime);
                if (System.DateTime.TryParse(Flag(FLAG_START), out tmpDate))
                {
                    StartDate = tmpDate;
                }
                else
                {
                    sbErrors.AppendFormat("Unable to parse the Start Date '{0}'. Try using YYYY-MM-DD format; ", Flag(FLAG_START));
                }
            }

            if (HasFlag(FLAG_END))
            {
                System.DateTime tmpDate = default(System.DateTime);
                if (System.DateTime.TryParse(Flag(FLAG_END), out tmpDate))
                {
                    EndDate = tmpDate;
                }
                else
                {
                    sbErrors.AppendFormat("Unable to parse the End Date '{0}'. Try using YYYY-MM-DD format; ", Flag(FLAG_END));
                }
            }

            // validate end date is beyond the start date
            if (StartDate.HasValue && EndDate.HasValue)
            {
                if (EndDate < StartDate)
                {
                    sbErrors.Append("Start Date cannot be less than End Date; ");
                }
            }

            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {

            StringBuilder sbErrors = new StringBuilder();
            if (UserId.HasValue)
            {
                // do lookup by user id
                var ui = UserController.GetUserById(PortalId, (int)UserId);
                if (ui != null)
                {
                    try
                    {
                        Prompt.Utilities.AddToRoles((int)UserId, PortalId, Roles, ",", StartDate, EndDate);
                        var lst = Prompt.Utilities.GetUserRoles(ui);
                        return new ConsoleResultModel(string.Empty) { data = lst };
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        return new ConsoleErrorResultModel("An unexpected error occurred while processing your request. Please see the Event Viewer for details.");
                    }
                }
                else
                {
                    return new ConsoleErrorResultModel(string.Format("No user found with the ID of '{0}'", UserId));
                }
            }
            else
            {
                return new ConsoleErrorResultModel("No User ID passed. Nothing to do.");
            }

        }
    }
}
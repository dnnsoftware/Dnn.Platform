using System;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.User
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
                sbErrors.Append("You must specify a valid User ID as either the first argument or using the --id flag; ");
            }

            if (HasFlag(FlagRoles))
            {
                if (string.IsNullOrEmpty(Flag(FlagRoles)))
                {
                    sbErrors.Append("--roles cannot be empty; ");
                }
                else
                {
                    // non-empty roles flag.
                    Roles = Flag(FlagRoles);
                }
            }
            else if (HasFlag("role"))
            {
                sbErrors.Append("Invalid flag '--role'. Did you mean --roles ?");
            }

            if (HasFlag(FlagStart))
            {
                var tmpDate = default(DateTime);
                if (DateTime.TryParse(Flag(FlagStart), out tmpDate))
                {
                    StartDate = tmpDate;
                }
                else
                {
                    sbErrors.AppendFormat("Unable to parse the Start Date '{0}'. Try using YYYY-MM-DD format; ", Flag(FlagStart));
                }
            }

            if (HasFlag(FlagEnd))
            {
                var tmpDate = default(DateTime);
                if (DateTime.TryParse(Flag(FlagEnd), out tmpDate))
                {
                    EndDate = tmpDate;
                }
                else
                {
                    sbErrors.AppendFormat("Unable to parse the End Date '{0}'. Try using YYYY-MM-DD format; ", Flag(FlagEnd));
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

        public override ConsoleResultModel Run()
        {

            var sbErrors = new StringBuilder();
            if (UserId.HasValue)
            {
                // do lookup by user id
                var ui = UserController.GetUserById(PortalId, (int)UserId);
                if (ui != null)
                {
                    try
                    {
                        Components.Utilities.AddToRoles((int)UserId, PortalId, Roles, ",", StartDate, EndDate);
                        var lst = Components.Utilities.GetUserRoles(ui);
                        return new ConsoleResultModel(string.Empty) { Data = lst };
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        return new ConsoleErrorResultModel("An unexpected error occurred while processing your request. Please see the Event Viewer for details.");
                    }
                }
                else
                {
                    return new ConsoleErrorResultModel($"No user found with the ID of '{UserId}'");
                }
            }
            else
            {
                return new ConsoleErrorResultModel("No User ID passed. Nothing to do.");
            }

        }
    }
}
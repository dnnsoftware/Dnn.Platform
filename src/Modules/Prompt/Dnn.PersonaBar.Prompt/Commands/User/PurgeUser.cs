using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.User
{
    [ConsoleCommand("purge-user", "Completely removes a previously deleted user from the portal.", new string[] { "id" })]
    public class PurgeUser : BaseConsoleCommand, IConsoleCommand
    {

        private const string FLAG_ID = "id";

        public string ValidationMessage { get; private set; }
        public int? UserId { get; private set; }
        public bool? Notify { get; private set; }

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
                if (args.Length == 2 && int.TryParse(args[1], out tmpId))
                {
                    UserId = tmpId;
                }
            }

            if (!UserId.HasValue)
            {
                sbErrors.AppendFormat("You must specify a valid numeric User ID using the --{0} flag or by passing it as the first argument; ", FLAG_ID);
            }

            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            List<UserModel> lst = new List<UserModel>();

            StringBuilder sbErrors = new StringBuilder();
            if (UserId.HasValue)
            {
                // do lookup by user id
                var ui = UserController.GetUserById(PortalId, (int)UserId);
                if (ui != null)
                {
                    if (ui.IsDeleted)
                    {
                        if (UserController.RemoveUser(ui))
                        {
                            lst.Add(new UserModel(ui));
                            return new ConsoleResultModel("The User has been permanently removed from the site.") { data = lst };
                        }
                    }
                    else
                    {
                        return new ConsoleErrorResultModel("Cannot purge user that has not been deleted first. Try delete-user.");
                    }
                }
                else
                {
                    return new ConsoleErrorResultModel(string.Format("No user found with the ID of '{0}'", UserId));
                }
            }

            // shouldn't get here.
            return new ConsoleResultModel("No user found to purge");
        }
    }
}
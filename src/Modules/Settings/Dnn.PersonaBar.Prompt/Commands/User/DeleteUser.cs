using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Commands.User
{
    [ConsoleCommand("delete-user", "Deletes the specifed user from the portal.", new string[]{
        "id",
        "notify"
    })]
    public class DeleteUser : ConsoleCommandBase
    {
        private const string FLAG_ID = "id";
        private const string FLAG_NOTIFY = "notify";


        public int? UserId { get; private set; }
        public bool? Notify { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
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
                sbErrors.Append("You must specify a valid User ID");
            }

            if (HasFlag(FLAG_NOTIFY))
            {
                bool tmpNotify = false;
                if (bool.TryParse(Flag(FLAG_NOTIFY), out tmpNotify))
                {
                    Notify = tmpNotify;
                }
                else
                {
                    sbErrors.Append("If you specify the --notify flag, it must be set to True or False; ");
                }
            }
            else
            {
                Notify = false;
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            List<UserModel> lst = new List<UserModel>();

            StringBuilder sbErrors = new StringBuilder();
            if (UserId.HasValue)
            {
                // do lookup by user id
                var ui = UserController.GetUserById(PortalId, (int)UserId);
                if (ui != null)
                {
                    if (UserController.DeleteUser(ref ui, (bool)Notify, false))
                    {
                        // We must clear User cache or else, when the user is 'removed' (so it can't be restored), you 
                        // will not be able to create a new user with the same username -- even though no user with that username
                        // exists.
                        DotNetNuke.Common.Utilities.DataCache.ClearUserCache(PortalId, ui.Username);
                        // attempt to retrieve the user from the dB
                        ui = UserController.GetUserById(PortalId, (int)UserId);
                        lst.Add(new UserModel(ui));
                        return new ConsoleResultModel("User was successfully deleted") { Data = lst };
                    }
                    else
                    {
                        return new ConsoleErrorResultModel("The user was found but the system is unable to delete it") { Data = lst };
                    }
                }
                else
                {
                    return new ConsoleErrorResultModel(string.Format("No user found with the ID of '{0}'", UserId));
                }
            }

            // shouldn't get here.
            return new ConsoleResultModel("No user found to delete");
        }
    }
}
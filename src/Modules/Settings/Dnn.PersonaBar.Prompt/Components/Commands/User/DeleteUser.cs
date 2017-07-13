using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.User
{
    [ConsoleCommand("delete-user", "Deletes the specifed user from the portal.", new[]{
        "id",
        "notify"
    })]
    public class DeleteUser : ConsoleCommandBase
    {
        private const string FlagId = "id";
        private const string FlagNotify = "notify";


        public int? UserId { get; private set; }
        public bool? Notify { get; private set; }

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
                sbErrors.Append("You must specify a valid User ID");
            }

            if (HasFlag(FlagNotify))
            {
                var tmpNotify = false;
                if (bool.TryParse(Flag(FlagNotify), out tmpNotify))
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
            var lst = new List<UserModel>();

            var sbErrors = new StringBuilder();
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
                    return new ConsoleErrorResultModel($"No user found with the ID of '{UserId}'");
                }
            }

            // shouldn't get here.
            return new ConsoleResultModel("No user found to delete");
        }
    }
}
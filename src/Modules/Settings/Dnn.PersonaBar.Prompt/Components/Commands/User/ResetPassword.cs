using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.User
{
    [ConsoleCommand("reset-password", "Resets the user's password", new[]{
        "id",
        "notify"
    })]
    public class ResetPassword : ConsoleCommandBase
    {
        private const string FlagId = "id";
        private const string FlagNotify = "notify";


        public bool? Notify { get; private set; }
        public int? UserId { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            var tmpId = 0;
            if (HasFlag(FlagId))
            {
                if (int.TryParse(Flag(FlagId), out tmpId))
                    UserId = tmpId;
            }
            else
            {
                if (int.TryParse(args[1], out tmpId))
                    UserId = tmpId;
            }

            var tmpNotify = false;
            if (HasFlag(FlagNotify))
            {
                if (!bool.TryParse(Flag(FlagNotify), out tmpNotify))
                {
                    sbErrors.AppendFormat("The --{0} flag takes True or False as its value (case-insensitive)", FlagNotify);
                }
                else
                {
                    Notify = tmpNotify;
                }
            }
            else
            {
                Notify = true; // reset password triggers sending the email to reset password. It's kind of useless if _Notify is False;
            }


            if (!UserId.HasValue)
            {
                sbErrors.Append("You must specify a number for the User's ID");
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var sendEmail = false;
            if (Notify.HasValue)
                sendEmail = (bool)Notify;

            var user = UserController.GetUserById(PortalId, (int)UserId);
            if (user == null)
            {
                return new ConsoleErrorResultModel($"No user found with the ID of '{UserId}'");
            }
            else
            {
                UserController.ResetPasswordToken(user, sendEmail);
            }

            return new ConsoleResultModel("User password has been reset." + ((bool)Notify ? " An email has been sent to the user" : ""));
        }

    }
}
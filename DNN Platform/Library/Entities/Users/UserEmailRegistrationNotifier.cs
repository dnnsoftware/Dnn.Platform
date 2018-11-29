using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Mail;
using static DotNetNuke.Common.Globals;

namespace DotNetNuke.Entities.Users
{
    public class UserEmailRegistrationNotifier
    {
        private static PortalSettings _portalSettings = PortalSettings.Current;

        public UserEmailRegistrationNotifier()
        {
        }

        public static void EmailToAdministrator(UserInfo user)
        {
            // avoid self-notification (i.e. on site installation/super user creation)
            var currentUser = UserController.Instance.GetCurrentUserInfo();
            if (currentUser != null && (currentUser.UserID == Null.NullInteger || currentUser.UserID == user.UserID))
            {
                return;
            }

            //send notification to portal administrator of new user registration
            //check the receive notification setting first, but if register type is Private, we will always send the notification email.
            //because the user need administrators to do the approve action so that he can continue use the website.
            if (_portalSettings.EnableRegisterNotification || _portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration)
            {
                EmailToUser(user, MessageType.UserRegistrationAdmin);
            }
        }

        public static void EmailToUser(UserInfo user)
        {
            switch (_portalSettings.UserRegistration)
            {
                case (int)PortalRegistrationType.PrivateRegistration:
                    EmailToUser(user, MessageType.UserRegistrationPrivate);
                    break;
                case (int)PortalRegistrationType.PublicRegistration:
                    EmailToUser(user, MessageType.UserRegistrationPublic);
                    break;
                case (int)PortalRegistrationType.VerifiedRegistration:
                    EmailToUser(user, MessageType.UserRegistrationVerified);
                    break;
                case (int)PortalRegistrationType.NoRegistration:
                    EmailToUser(user, MessageType.UserRegistrationPublic);
                    break;
            }
        }

        public static void EmailToUser(UserInfo user, MessageType type)
        {
            Mail.SendMail(user, type, _portalSettings);
        }
    }
}

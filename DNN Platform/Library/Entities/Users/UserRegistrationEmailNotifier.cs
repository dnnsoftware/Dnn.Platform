using System;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Mail;
using static DotNetNuke.Common.Globals;

namespace DotNetNuke.Entities.Users
{
    public class UserRegistrationEmailNotifier
    {
        private static UserInfo CurrentUser => UserController.Instance.GetCurrentUserInfo();

        public UserRegistrationEmailNotifier()
        {
        }

        public static void NotifyAdministrator(UserInfo user)
        {
            // avoid self-notification (i.e. on site installation/super user creation)
            if (CurrentUser != null && 
                (CurrentUser.UserID == Null.NullInteger || CurrentUser.UserID == user.UserID))
            {
                return;
            }

            //send notification to portal administrator of new user registration
            //check the receive notification setting first, but if register type is Private, we will always send the notification email.
            //because the user need administrators to do the approve action so that he can continue use the website.
            if (PortalSettings.Current.EnableRegisterNotification || PortalSettings.Current.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration)
            {
                NotifyUser(user, MessageType.UserRegistrationAdmin);
            }
        }

        public static void NotifyUser(UserInfo user)
        {
            switch (PortalSettings.Current.UserRegistration)
            {
                case (int)PortalRegistrationType.PrivateRegistration:
                    NotifyUser(user, CurrentUser != null && CurrentUser.UserID != Null.NullInteger ?
                        MessageType.UserRegistrationPrivateNoApprovalRequired :
                        MessageType.UserRegistrationPrivate);
                    break;
                case (int)PortalRegistrationType.PublicRegistration:
                    NotifyUser(user, MessageType.UserRegistrationPublic);
                    break;
                case (int)PortalRegistrationType.VerifiedRegistration:
                    NotifyUser(user, MessageType.UserRegistrationVerified);
                    break;
                case (int)PortalRegistrationType.NoRegistration:
                    NotifyUser(user, MessageType.UserRegistrationPublic);
                    break;
            }
        }

        public static void NotifyUser(UserInfo user, MessageType type)
        {
            Mail.SendMail(user, type, PortalSettings.Current);
        }
    }
}

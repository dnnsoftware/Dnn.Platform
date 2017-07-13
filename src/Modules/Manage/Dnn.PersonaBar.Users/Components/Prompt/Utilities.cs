using System;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;

namespace Dnn.PersonaBar.Users.Components.Prompt
{
    public class Utilities
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Utilities));

        public static string SendSystemEmail(UserInfo user, DotNetNuke.Services.Mail.MessageType msgType, DotNetNuke.Entities.Portals.PortalSettings ps)
        {
            var msg = string.Empty;
            try
            {
                msg = DotNetNuke.Services.Mail.Mail.SendMail(user, msgType, ps);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                // On some systems, if DNN has sent an email and the SMTP connection is still open, 
                // no other email can be sent until that connection times out. When that happens a transport error is thrown
                // but it seems to close the connection at that point. So, retrying after the exception always (in my tests) 
                // results in the email being sent on the 2nd go-round.
                // try again.
                try
                {
                    msg = DotNetNuke.Services.Mail.Mail.SendMail(user, msgType, ps);
                }
                catch (Exception ex2)
                {
                    Exceptions.LogException(ex2);
                }
            }
            return msg;
        }
    }
}



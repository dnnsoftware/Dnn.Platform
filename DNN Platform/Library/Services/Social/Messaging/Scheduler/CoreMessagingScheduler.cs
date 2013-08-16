#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Social.Messaging.Internal;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Scheduler
{
    public class CoreMessagingScheduler : SchedulerClient
    {
        private readonly PortalController _pController = new PortalController();
        private readonly UserController _uController = new UserController();

        public CoreMessagingScheduler(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }
           
        private bool SendEmail(int portalId)
        {
            return PortalController.GetPortalSetting("MessagingSendEmail", portalId, "YES") == "YES";
        }

        public override void DoWork()
        {
            try
            {
                var schedulerInstance = Guid.NewGuid();
                ScheduleHistoryItem.AddLogNote("MessagingScheduler DoWork Starting " + schedulerInstance);

                if (string.IsNullOrEmpty(Host.SMTPServer))
                {
                    ScheduleHistoryItem.AddLogNote("No SMTP Servers have been configured for this host. Terminating task.");
                    ScheduleHistoryItem.Succeeded = true;
                }
                else
                {
                    var messageLeft = true;
                    var messagesSent = 0;

                    while (messageLeft)
                    {
                        var batchMessages = InternalMessagingController.Instance.GetNextMessagesForDispatch(schedulerInstance, Host.MessageSchedulerBatchSize);

                        if (batchMessages != null && batchMessages.Count > 0)
                        {
                            try
                            {
                                foreach (var messageRecipient in batchMessages)
                                {
                                    SendMessage(messageRecipient);
                                    messagesSent = messagesSent + 1;
                                }

                            }
                            catch (Exception e)
                            {
                                Errored(ref e);
                            }
                        }
                        else
                        {
                            messageLeft = false;
                        }
                    }

                    ScheduleHistoryItem.AddLogNote(string.Format("Message Scheduler '{0}' sent a total of {1} message(s)", schedulerInstance, messagesSent));
                    ScheduleHistoryItem.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("MessagingScheduler Failed: " + ex);
                Errored(ref ex);
            }
        }

        private void SendMessage(MessageRecipient messageRecipient)
        {
            //todo: check if host user can send to multiple portals...
            var messageDetails = InternalMessagingController.Instance.GetMessage(messageRecipient.MessageID);

            if (!SendEmail(messageDetails.PortalID))
            {
                InternalMessagingController.Instance.MarkMessageAsSent(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }

            var fromAddress = _pController.GetPortal(messageDetails.PortalID).Email;
            var toAddress = _uController.GetUser(messageDetails.PortalID, messageRecipient.UserID).Email;

            var sender = _uController.GetUser(messageDetails.PortalID, messageDetails.SenderUserID).DisplayName;
            if (string.IsNullOrEmpty(sender))
                sender = _pController.GetPortal(messageDetails.PortalID).PortalName;
            var senderAddress = sender + "< " + fromAddress + ">";
            var subject = string.Format(Localization.Localization.GetString("EMAIL_SUBJECT_FORMAT",
                                                                  Localization.Localization.GlobalResourceFile), sender);

            var body = string.Format(Localization.Localization.GetString("EMAIL_BODY_FORMAT",
                                                                  Localization.Localization.GlobalResourceFile), messageDetails.Subject, messageDetails.Body);

            Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, body);

            InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
        }
    }
}
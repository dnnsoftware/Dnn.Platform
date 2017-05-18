#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections;

using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Services.Messaging.Scheduler
{[Obsolete("Deprecated in 6.2.0 - scheduled item type will automatically update.")]
    public class MessagingScheduler : SchedulerClient
    {
        
        private readonly MessagingController _mController = new MessagingController();

        public MessagingScheduler(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                Guid _schedulerInstance = Guid.NewGuid();
                ScheduleHistoryItem.AddLogNote("MessagingScheduler DoWork Starting " + _schedulerInstance);

                if ((string.IsNullOrEmpty(Host.SMTPServer)))
                {
                    ScheduleHistoryItem.AddLogNote("No SMTP Servers have been configured for this host. Terminating task.");
                    ScheduleHistoryItem.Succeeded = true;
                    //'Return
                }
                else
                {
                    Hashtable settings = ScheduleHistoryItem.GetSettings();

                    bool _messageLeft = true;
                    int _messagesSent = 0;


                    while (_messageLeft)
                    {
                       Data.Message currentMessage = _mController.GetNextMessageForDispatch(_schedulerInstance);
                        
                        if ((currentMessage != null))
                        {
                            try
                            {
                                SendMessage(currentMessage);
                                _messagesSent = _messagesSent + 1;
                            }
                            catch (Exception e)
                            {
                                Errored(ref e);
                            }
                        }
                        else
                        {
                            _messageLeft = false;
                        }
                    }

                    ScheduleHistoryItem.AddLogNote(string.Format("Message Scheduler '{0}' sent a total of {1} message(s)", _schedulerInstance, _messagesSent));
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

        private void SendMessage(Data.Message objMessage)
        {
            string senderAddress = UserController.GetUserById(objMessage.PortalID, objMessage.FromUserID).Email;
            string fromAddress = PortalController.Instance.GetPortal(objMessage.PortalID).Email;
            string toAddress = UserController.Instance.GetUser(objMessage.PortalID, objMessage.ToUserID).Email;


            Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, objMessage.Subject, objMessage.Body);

            _mController.MarkMessageAsDispatched(objMessage.MessageID);
        }
    }
}
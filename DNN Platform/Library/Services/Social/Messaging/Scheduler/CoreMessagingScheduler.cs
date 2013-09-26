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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Caching;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
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

        private const string SettingLastHourlyRun = "CoreMessagingLastHourlyDigestRun";
        private const string SettingLastDailyRun = "CoreMessagingLastDailyDigestRun";
        private const string SettingLastWeeklyRun = "CoreMessagingLastWeeklyDigestRun";
        private const string SettingLastMonthlyRun = "CoreMessagingLastMonthlyDigestRun";

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
                ScheduleHistoryItem.AddLogNote("Messaging Scheduler DoWork Starting " + schedulerInstance);

                if (string.IsNullOrEmpty(Host.SMTPServer))
                {
                    ScheduleHistoryItem.AddLogNote("<br>No SMTP Servers have been configured for this host. Terminating task.");
                    ScheduleHistoryItem.Succeeded = true;
                }
                else
                {
                    Progressing();
                   
                    var instantMessages = HandleInstantMessages(schedulerInstance);
                    var remainingMessages = Host.MessageSchedulerBatchSize - instantMessages;
                    if (remainingMessages > 0)
                    {
                        HandleFrequentDigests(schedulerInstance, remainingMessages);
                    }
                    ScheduleHistoryItem.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("<br>Messaging Scheduler Failed: " + ex);
                Errored(ref ex);
            }
        }

        private void HandleFrequentDigests(Guid schedulerInstance, int remainingMessages)
        {            
         
            var handledMessages = HandleFrenquencyDigest(DateTime.Now.AddHours(-1), SettingLastHourlyRun, Frequency.Hourly, schedulerInstance, remainingMessages);
            remainingMessages = remainingMessages - handledMessages;
         
            handledMessages = HandleFrenquencyDigest(DateTime.Now.AddDays(-1), SettingLastDailyRun, Frequency.Daily, schedulerInstance, remainingMessages);        
            remainingMessages = remainingMessages - handledMessages;
            
            handledMessages = HandleFrenquencyDigest(DateTime.Now.AddDays(-7), SettingLastWeeklyRun, Frequency.Weekly, schedulerInstance, remainingMessages);         
            remainingMessages = remainingMessages - handledMessages;           

            HandleFrenquencyDigest(DateTime.Now.AddDays(-30), SettingLastMonthlyRun, Frequency.Monthly, schedulerInstance, remainingMessages);                    
        }

        private int HandleFrenquencyDigest(DateTime dateToCompare, string settingKeyLastRunDate, Frequency frequency, Guid schedulerInstance, int remainingMessages)
        {
            int handledMessages = 0;
            if (remainingMessages <= 0)
            {
                return handledMessages;
            }

            var lastRunDate = GetScheduleItemDateSetting(settingKeyLastRunDate);            
            if (dateToCompare >= lastRunDate)
            {
                handledMessages = HandleDigest(schedulerInstance, frequency, remainingMessages);
                if (handledMessages < remainingMessages)
                {
                    SchedulingProvider.Instance().AddScheduleItemSetting(
                        ScheduleHistoryItem.ScheduleID, settingKeyLastRunDate, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                }
            }
            return handledMessages;
        }

        private int HandleDigest(Guid schedulerInstance, Frequency frequency, int remainingMessages)
        {
            var messagesSent = 0;
            // get subscribers based on frequency, utilize remaining batch size as part of count of users to return (note, if multiple subscriptions have the same frequency they will be combined into 1 email)
            ScheduleHistoryItem.AddLogNote("<br>Messaging Scheduler Starting Digest '" + schedulerInstance + "'.  ");
            
            var messageLeft = true;

            while (messageLeft)
            {
                var batchMessages = InternalMessagingController.Instance.GetNextSubscribersForDispatch(frequency, schedulerInstance, remainingMessages);

                if (batchMessages != null && batchMessages.Count > 0)
                {
                    try
                    {
                        foreach (var userId in (from t in batchMessages select t.UserID).Distinct())
                        {
                            var currentUserId = userId;
                            var colUserMessages = (from t in batchMessages where t.UserID == currentUserId select t);
                            var messageRecipients = colUserMessages as MessageRecipient[] ?? colUserMessages.ToArray();
                            var singleMessage = (from t in messageRecipients select t).First();
                            if (singleMessage != null)
                            {
                                var messageDetails = InternalMessagingController.Instance.GetMessage(singleMessage.MessageID);
                                var ps = new PortalSettings(messageDetails.PortalID);

                                var senderUser = _uController.GetUser(messageDetails.PortalID, messageDetails.SenderUserID);
                                var recipientUser = _uController.GetUser(messageDetails.PortalID, singleMessage.UserID);

                                SendDigest(messageRecipients, ps, senderUser, recipientUser);
                            }
                            messagesSent = messagesSent + 1;
                        }
                        // at this point we have sent all digest notifications for this batch
                        ScheduleHistoryItem.AddLogNote("Sent " + messagesSent + " digest subscription emails for this batch.  ");
                        return messagesSent;
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

            ScheduleHistoryItem.AddLogNote("Sent " + messagesSent + " " + frequency + " digest subscription emails.  ");
            
            return messagesSent;
        }

        private void SendDigest(IEnumerable<MessageRecipient> messages, PortalSettings ps, UserInfo senderUser, UserInfo recipientUser)
        {
            var msgContent = "";
            var messageRecipients = messages as MessageRecipient[] ?? messages.ToArray();
            var detailTemplate = Localization.Localization.GetString("Template_Item", Localization.Localization.SharedResourceFile);

            foreach (var message in messageRecipients)
            {
                var messageDetails = InternalMessagingController.Instance.GetMessage(message.MessageID);
                if (!SendEmail(messageDetails.PortalID))
                {
                    InternalMessagingController.Instance.MarkMessageAsSent(messageDetails.MessageID, message.RecipientID);
                    return;
                }

                var itemTemplate = detailTemplate;
                var authorId = message.CreatedByUserID > 0 ? message.CreatedByUserID : senderUser.UserID;

                itemTemplate = itemTemplate.Replace("[TITLE]", messageDetails.Subject);
                itemTemplate = itemTemplate.Replace("[CONTENT]", messageDetails.Body);
                itemTemplate = itemTemplate.Replace("[PROFILEPICURL]", GetProfilePicUrl(ps, authorId));

                msgContent += itemTemplate;
            }

            var fromAddress = ps.Email;
            var toAddress = recipientUser.Email;
            var sender = senderUser.DisplayName;

            if (string.IsNullOrEmpty(sender))
                sender = ps.PortalName;

            var senderAddress = sender + "< " + fromAddress + ">";
            var subject = string.Format(Localization.Localization.GetString("EMAIL_SUBJECT_FORMAT", Localization.Localization.GlobalResourceFile), ps.PortalName);
            var template = Localization.Localization.GetString("Template_Email", Localization.Localization.SharedResourceFile);

            template = template.Replace("[SITEURL]", GetPortalHomeUrl(ps));
            template = template.Replace("[NOTIFICATIONURL]", GetNotificationUrl(ps, recipientUser.UserID));
            template = template.Replace("[PORTALNAME]", ps.PortalName);
            template = template.Replace("[LOGOURL]", GetPortalLogoUrl(ps));
            template = template.Replace("[UNSUBSCRIBEURL]", GetSubscriptionsUrl(ps, recipientUser.UserID));
            template = template.Replace("[MESSAGEBODY]", msgContent);
            template = template.Replace("href=\"/", "href=\"http://" + ps.DefaultPortalAlias + "/");
            template = template.Replace("src=\"/", "src=\"http://" + ps.DefaultPortalAlias + "/");

            Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, template);

            foreach (var message in messageRecipients)
            {
                InternalMessagingController.Instance.MarkMessageAsDispatched(message.MessageID, message.RecipientID);
            }
        }

        private DateTime GetScheduleItemDateSetting(string settingKey)
        {
            var colScheduleItemSettings = SchedulingProvider.Instance().GetScheduleItemSettings(ScheduleHistoryItem.ScheduleID);
            var dateValue = DateTime.Now;

            if (colScheduleItemSettings.Count > 0)
            {
                if (colScheduleItemSettings.ContainsKey(settingKey))
                {
                    dateValue = Convert.ToDateTime(colScheduleItemSettings[settingKey], CultureInfo.InvariantCulture);
                }
            }
            else
            {
                SchedulingProvider.Instance().AddScheduleItemSetting(
                    ScheduleHistoryItem.ScheduleID, SettingLastHourlyRun, dateValue.ToString(CultureInfo.InvariantCulture));
                SchedulingProvider.Instance().AddScheduleItemSetting(
                   ScheduleHistoryItem.ScheduleID, SettingLastDailyRun, dateValue.ToString(CultureInfo.InvariantCulture));
                SchedulingProvider.Instance().AddScheduleItemSetting(
                   ScheduleHistoryItem.ScheduleID, SettingLastWeeklyRun, dateValue.ToString(CultureInfo.InvariantCulture));
                SchedulingProvider.Instance().AddScheduleItemSetting(
                   ScheduleHistoryItem.ScheduleID, SettingLastMonthlyRun, dateValue.ToString(CultureInfo.InvariantCulture));
            }

            return dateValue;
        }

        private int HandleInstantMessages(Guid schedulerInstance)
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

            ScheduleHistoryItem.AddLogNote(string.Format("<br>Messaging Scheduler '{0}' sent a total of {1} message(s)", schedulerInstance, messagesSent));
            return messagesSent;
        }

        private void SendMessage(MessageRecipient messageRecipient)
        {
            //todo: check if host user can send to multiple portals...
            var messageDetails = InternalMessagingController.Instance.GetMessage(messageRecipient.MessageID);
            var author = _uController.GetUser(messageDetails.PortalID, messageDetails.SenderUserID);

            if (!SendEmail(messageDetails.PortalID))
            {
                InternalMessagingController.Instance.MarkMessageAsSent(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }

            var ps = new PortalSettings(messageDetails.PortalID);
            var fromAddress = ps.Email;
            var toUser = _uController.GetUser(messageDetails.PortalID, messageRecipient.UserID);

            if (toUser == null || toUser.IsDeleted)
            {
                InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }
            var toAddress = toUser.Email;

            var sender = author.DisplayName;

            if (string.IsNullOrEmpty(sender))
                sender = ps.PortalName;

            var senderAddress = sender + "< " + fromAddress + ">";
            var subject = string.Format(Localization.Localization.GetString("EMAIL_SUBJECT_FORMAT", Localization.Localization.GlobalResourceFile), ps.PortalName);
            var template = Localization.Localization.GetString("Template_Email", Localization.Localization.SharedResourceFile);
            var authorId = messageDetails.CreatedByUserID > 0 ? messageDetails.CreatedByUserID : author.UserID;

            var detailTemplate = Localization.Localization.GetString("Template_Item", Localization.Localization.SharedResourceFile);
            detailTemplate = detailTemplate.Replace("[TITLE]", messageDetails.Subject);
            detailTemplate = detailTemplate.Replace("[CONTENT]", messageDetails.Body);
            detailTemplate = detailTemplate.Replace("[PROFILEPICURL]", GetProfilePicUrl(ps, authorId));

            template = template.Replace("[SITEURL]", GetPortalHomeUrl(ps));
            template = template.Replace("[NOTIFICATIONURL]", GetNotificationUrl(ps, messageRecipient.UserID));
            template = template.Replace("[PORTALNAME]", ps.PortalName);
            template = template.Replace("[LOGOURL]", GetPortalLogoUrl(ps));
            template = template.Replace("[UNSUBSCRIBEURL]", GetSubscriptionsUrl(ps, author.UserID));
            template = template.Replace("[MESSAGEBODY]", detailTemplate);
            template = template.Replace("href=\"/", "href=\"http://" + ps.DefaultPortalAlias + "/");
            template = template.Replace("src=\"/", "src=\"http://" + ps.DefaultPortalAlias + "/");

            Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, template);            
            InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
        }

        private static string GetProfilePicUrl(PortalSettings sendingPortal, int userId)
        {
            var url = "http://" + sendingPortal.DefaultPortalAlias + "/" + "profilepic.ashx?userId={0}&h={1}&w={2}";
            return string.Format(url, userId, 64, 64);
        }

        private static string GetNotificationUrl(PortalSettings sendingPortal, int userId)
        {
            var cacheKey = string.Format("MessageCenterTab:{0}", sendingPortal.PortalId);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
            if (messageTabId <= 0)
            {
                var tabController = new TabController();
                var moduleController = new ModuleController();

                messageTabId = sendingPortal.UserTabId;
                var profileTab = tabController.GetTab(sendingPortal.UserTabId, sendingPortal.PortalId, false);
                if (profileTab != null)
                {
                    var childTabs = tabController.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                    foreach (var tab in childTabs)
                    {
                        foreach (var kvp in moduleController.GetTabModules(tab.TabID))
                        {
                            var module = kvp.Value;
                            if (module.DesktopModule.FriendlyName == "Message Center")
                            {
                                messageTabId = tab.TabID;
                                break;
                            }
                        }
                    }
                }

                DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));
            }

            return "http://" + sendingPortal.DefaultPortalAlias + "/tabid/" + messageTabId + "/userId/" + userId + "/" + Globals.glbDefaultPage + "#dnnCoreNotification";
        }

        private static string GetPortalLogoUrl(PortalSettings sendingPortal)
        {
            return "http://" + sendingPortal.DefaultPortalAlias + "/" + sendingPortal.HomeDirectory + "/" + sendingPortal.LogoFile;
        }

        private static string GetPortalHomeUrl(PortalSettings sendingPortal)
        {

            return "http://" + sendingPortal.DefaultPortalAlias;
        }

        private string GetSubscriptionsUrl(PortalSettings sendingPortal, int userId)
        {
            return "http://" + sendingPortal.DefaultPortalAlias + "/tabid/" + GetMessageTab(sendingPortal) + "/userId/" + userId + "/" + Globals.glbDefaultPage;
        }

        private static int GetMessageTab(PortalSettings sendingPortal)
        {
            var cacheKey = string.Format("MessageTab:{0}", sendingPortal.PortalId);
            //TODO Create Dedault Const cache timeout
            var cacheItemArgs = new CacheItemArgs(cacheKey, 30, CacheItemPriority.Default, sendingPortal);

            return CBO.GetCachedObject<int>(cacheItemArgs, GetMessageTabCallback);

        }

        private static object GetMessageTabCallback(CacheItemArgs cacheItemArgs)
        {
            var sendingPortal = cacheItemArgs.Params[0] as PortalSettings;
            var tabController = new TabController();
            var moduleController = new ModuleController();

            var profileTab = tabController.GetTab(sendingPortal.UserTabId, sendingPortal.PortalId, false);
            if (profileTab != null)
            {
                var childTabs = tabController.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                foreach (var tab in childTabs)
                {
                    foreach (var kvp in moduleController.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center")
                        {
                            return tab.TabID;
                        }
                    }
                }
            }

            //default to User Profile Page
            return sendingPortal.UserTabId;
        }
    }
}
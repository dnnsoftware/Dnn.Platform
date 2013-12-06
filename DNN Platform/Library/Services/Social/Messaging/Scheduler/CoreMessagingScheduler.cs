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
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Social.Messaging.Internal;
#endregion

namespace DotNetNuke.Services.Social.Messaging.Scheduler
{
    public class CoreMessagingScheduler : SchedulerClient
    {
        private readonly UserController userController = new UserController();

        private const string SettingLastHourlyRun = "CoreMessagingLastHourlyDigestRun";
        private const string SettingLastDailyRun = "CoreMessagingLastDailyDigestRun";
        private const string SettingLastWeeklyRun = "CoreMessagingLastWeeklyDigestRun";
        private const string SettingLastMonthlyRun = "CoreMessagingLastMonthlyDigestRun";

        public CoreMessagingScheduler(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }
           
        private static bool IsSendEmailEnable(int portalId)
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
                var batchMessages = InternalMessagingController.Instance.GetNextMessagesForDigestDispatch(frequency, schedulerInstance, remainingMessages);

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
                                var portalSettings = new PortalSettings(messageDetails.PortalID);

                                var senderUser = userController.GetUser(messageDetails.PortalID, messageDetails.SenderUserID);
                                var recipientUser = userController.GetUser(messageDetails.PortalID, singleMessage.UserID);

                                SendDigest(messageRecipients, portalSettings, senderUser, recipientUser);
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

        private void SendDigest(IEnumerable<MessageRecipient> messages, PortalSettings portalSettings, UserInfo senderUser, UserInfo recipientUser)
        {
            var messageRecipients = messages as MessageRecipient[] ?? messages.ToArray();

            if (!IsUserAbleToReceiveAnEmail(recipientUser))
            {
                MarkMessagesAsDispatched(messageRecipients);
                return;
            }

            if (!IsSendEmailEnable(portalSettings.PortalId))
            {
                MarkMessagesAsDispatched(messageRecipients);
                return;
            }

            var defaultLanguage = recipientUser.Profile.PreferredLocale;

            var emailSubjectTemplate = GetEmailSubjectTemplate(defaultLanguage);
            var emailBodyTemplate = GetEmailBodyTemplate(defaultLanguage);
            var emailBodyItemTemplate = GetEmailBodyItemTemplate(defaultLanguage);

            var emailBodyItemContent = messageRecipients.Aggregate(string.Empty, 
                (current, message) => current + GetEmailItemContent(portalSettings, message, emailBodyItemTemplate));

            var fromAddress = portalSettings.Email;
            var toAddress = recipientUser.Email;

            var senderName = GetSenderName(senderUser.DisplayName, portalSettings.PortalName);
            var senderAddress = GetSenderAddress(senderName, fromAddress);

            var subject = string.Format(emailSubjectTemplate, portalSettings.PortalName);
            var body = GetEmailBody(emailBodyTemplate, emailBodyItemContent, portalSettings, recipientUser);

            Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, body);

            MarkMessagesAsDispatched(messageRecipients);
        }

        private static bool IsUserAbleToReceiveAnEmail(UserInfo recipientUser)
        {
            return recipientUser != null && !recipientUser.IsDeleted;
        }

        private static string GetSenderAddress(string sender, string fromAddress)
        {
            return string.Format("{0} < {1} >", sender, fromAddress);
        }

        private static string GetEmailBody(string template, string messageBody, PortalSettings portalSettings, UserInfo recipientUser)
        {
            template = template.Replace("[SITEURL]", GetPortalHomeUrl(portalSettings));
            template = template.Replace("[NOTIFICATIONURL]", GetNotificationUrl(portalSettings, recipientUser.UserID));
            template = template.Replace("[PORTALNAME]", portalSettings.PortalName);
            template = template.Replace("[LOGOURL]", GetPortalLogoUrl(portalSettings));
            template = template.Replace("[UNSUBSCRIBEURL]", GetSubscriptionsUrl(portalSettings, recipientUser.UserID));
            template = template.Replace("[MESSAGEBODY]", messageBody);
            template = template.Replace("href=\"/", "href=\"http://" + portalSettings.DefaultPortalAlias + "/");
            template = template.Replace("src=\"/", "src=\"http://" + portalSettings.DefaultPortalAlias + "/");

            return template;
        }

        private static string GetEmailItemContent(PortalSettings portalSettings, MessageRecipient message, string itemTemplate)
        {
            var messageDetails = InternalMessagingController.Instance.GetMessage(message.MessageID);

            var authorId = message.CreatedByUserID > 0 ? message.CreatedByUserID : messageDetails.SenderUserID;

            var emailItemContent = itemTemplate;
            emailItemContent = emailItemContent.Replace("[TITLE]", messageDetails.Subject);
            emailItemContent = emailItemContent.Replace("[CONTENT]", messageDetails.Body);
            emailItemContent = emailItemContent.Replace("[PROFILEPICURL]", GetProfilePicUrl(portalSettings, authorId));

            return emailItemContent;
        }

        private static void MarkMessagesAsDispatched(IEnumerable<MessageRecipient> messages)
        {
            foreach (var message in messages)
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
                var batchMessages = InternalMessagingController.Instance.GetNextMessagesForInstantDispatch(schedulerInstance, Host.MessageSchedulerBatchSize);

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

            var toUser = userController.GetUser(messageDetails.PortalID, messageRecipient.UserID);
            if (!IsUserAbleToReceiveAnEmail(toUser))
            {
                InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }

            if (!IsSendEmailEnable(messageDetails.PortalID))
            {
                InternalMessagingController.Instance.MarkMessageAsSent(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }

            var defaultLanguage = toUser.Profile.PreferredLocale;

            var emailSubjectTemplate = GetEmailSubjectTemplate(defaultLanguage);
            var emailBodyTemplate = GetEmailBodyTemplate(defaultLanguage);
            var emailBodyItemTemplate =  GetEmailBodyItemTemplate(defaultLanguage);
            
            var author = userController.GetUser(messageDetails.PortalID, messageDetails.SenderUserID);
            var portalSettings = new PortalSettings(messageDetails.PortalID);
            var fromAddress = portalSettings.Email;

            var toAddress = toUser.Email;

            var senderName = GetSenderName(author.DisplayName, portalSettings.PortalName);
            var senderAddress = GetSenderAddress(senderName, fromAddress);

            var emailBodyItemContent = GetEmailItemContent(portalSettings, messageRecipient, emailBodyItemTemplate);

            var subject = string.Format(emailSubjectTemplate, portalSettings.PortalName);
            var body = GetEmailBody(emailBodyTemplate, emailBodyItemContent, portalSettings, toUser);

            Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, body);          
  
            InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
        }

        private static string GetEmailBodyItemTemplate(string language)
        {
            return Localization.Localization.GetString("EMAIL_MESSAGING_DISPATCH_ITEM", Localization.Localization.GlobalResourceFile, language);
        }

        private static string GetEmailBodyTemplate(string language)
        {
            return Localization.Localization.GetString("EMAIL_MESSAGING_DISPATCH_BODY", Localization.Localization.GlobalResourceFile, language);
        }

        private static string GetEmailSubjectTemplate(string language)
        {
            return Localization.Localization.GetString("EMAIL_SUBJECT_FORMAT", Localization.Localization.GlobalResourceFile, language);
        }

        private static string GetSenderName(string displayName, string portalName)
        {
            return string.IsNullOrEmpty(displayName) ? portalName : displayName;
        }

        private static string GetProfilePicUrl(PortalSettings portalSettings, int userId)
        {
            return string.Format("http://{0}/profilepic.ashx?userId={1}&h={2}&w={3}", 
                portalSettings.DefaultPortalAlias, 
                userId, 
                64, 64);
        }

        private static string GetNotificationUrl(PortalSettings portalSettings, int userId)
        {
            var cacheKey = string.Format("MessageCenterTab:{0}", portalSettings.PortalId);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
            if (messageTabId <= 0)
            {
                var tabController = new TabController();
                var moduleController = new ModuleController();

                messageTabId = portalSettings.UserTabId;
                var profileTab = tabController.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
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

            return string.Format("http://{0}/tabid/{1}/userId/{2}/{3}#dnnCoreNotification",
                portalSettings.DefaultPortalAlias,
                messageTabId,
                userId,
                Globals.glbDefaultPage);
        }

        private static string GetPortalLogoUrl(PortalSettings portalSettings)
        {
            return string.Format("http://{0}/{1}/{2}",
                GetDomainName(portalSettings),
                portalSettings.HomeDirectory,
                portalSettings.LogoFile);
        }

        private static string GetDomainName(PortalSettings portalSettings)
        {
            var portalAlias = portalSettings.DefaultPortalAlias;
            return portalAlias.IndexOf("/", StringComparison.InvariantCulture) != -1 ?
                        portalAlias.Substring(0, portalAlias.IndexOf("/", StringComparison.InvariantCulture)) :
                        portalAlias;
        }

        private static string GetPortalHomeUrl(PortalSettings portalSettings)
        {
            return string.Format("http://{0}", portalSettings.DefaultPortalAlias);
        }

        private static string GetSubscriptionsUrl(PortalSettings portalSettings, int userId)
        {
            return string.Format("http://{0}/tabid/{1}/ctl/Profile/userId/{2}/pageno/3",
                portalSettings.DefaultPortalAlias,
                GetMessageTab(portalSettings),
                userId);
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
            var portalSettings = cacheItemArgs.Params[0] as PortalSettings;
            var tabController = new TabController();
            var moduleController = new ModuleController();

            var profileTab = tabController.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
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
            return portalSettings.UserTabId;
        }
    }
}
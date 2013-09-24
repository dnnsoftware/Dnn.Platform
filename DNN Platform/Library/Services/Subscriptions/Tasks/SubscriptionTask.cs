﻿// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Services.Social.Messaging.Internal;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Services.Subscriptions.Controllers;
using DotNetNuke.Services.Subscriptions.Entities;
using DotNetNuke.Services.Subscriptions.Data;

namespace DotNetNuke.Services.Subscriptions.Tasks
{
    public class SubscriptionTask : SchedulerClient
    {
        #region Protected members

        protected SchedulingProvider SchedulingProvider { get; private set; }

        #endregion

        #region Private members

        private readonly IDataService _dataService;
        private readonly UserController _userController = new UserController();

        private const string SettingLastHourlyRun = "SubscriptionsLastHourlyDigestRun";
        private const string SettingLastDailyRun = "SubscriptionsLastDailyDigestRun";
        private const string SettingLastWeeklyRun = "SubscriptionsLastWeeklyDigestRun";
        private const string SettingLastMonthlyRun = "SubscriptionsLastMonthlyDigestRun";
	    private const int DefaultCacheTimeout = 20;
        
        private static int BatchSize
        {
            get { return Host.MessageSchedulerBatchSize; }
        }

        #endregion

        #region Constructors

        public SubscriptionTask(ScheduleHistoryItem scheduleHistoryItem)
            : this(new DataService(), scheduleHistoryItem)
        {}

        public SubscriptionTask(IDataService dataService, ScheduleHistoryItem scheduleHistoryItem)
        {
            _dataService = dataService;

            SchedulingProvider = SchedulingProvider.Instance();

            ScheduleHistoryItem = scheduleHistoryItem;
        }

        #endregion

        #region Public methods

        public override void DoWork()
        {
            try
            {
                if (string.IsNullOrEmpty(Host.SMTPServer))
                {
                    ScheduleHistoryItem.AddLogNote("No SMTP Servers have been configured for this host. Terminating task.");
                    ScheduleHistoryItem.Succeeded = true;
                }
                else
                {
                    Progressing();
                    var schedulerInstance = Guid.NewGuid();

                    var instantMsgs = HandleInstant(schedulerInstance);
                    
                    if (instantMsgs < BatchSize)
                    {
                        var remainingBatchSize = BatchSize - instantMsgs;
                        var colScheduleItemSettings = SchedulingProvider.Instance().GetScheduleItemSettings(ScheduleHistoryItem.ScheduleID);
                        var lastHourlyRun = DateTime.Now;

                        if (colScheduleItemSettings.Count > 0)
                        {
                            if (colScheduleItemSettings.ContainsKey(SettingLastHourlyRun))
                            {
                                lastHourlyRun = Convert.ToDateTime(colScheduleItemSettings[SettingLastHourlyRun]);
                            }
                        }
                        else
                        {
                            SchedulingProvider.Instance().AddScheduleItemSetting(
                                ScheduleHistoryItem.ScheduleID, SettingLastHourlyRun, lastHourlyRun.ToString());
                            SchedulingProvider.Instance().AddScheduleItemSetting(
                               ScheduleHistoryItem.ScheduleID, SettingLastDailyRun, lastHourlyRun.ToString());
                            SchedulingProvider.Instance().AddScheduleItemSetting(
                               ScheduleHistoryItem.ScheduleID, SettingLastWeeklyRun, lastHourlyRun.ToString());
                            SchedulingProvider.Instance().AddScheduleItemSetting(
                               ScheduleHistoryItem.ScheduleID, SettingLastMonthlyRun, lastHourlyRun.ToString());
                        }

                        if (DateTime.Now.AddHours(-1) >= lastHourlyRun)
                        {
                            var currentRunDate = DateTime.Now;
                            remainingBatchSize = HandleDigest(schedulerInstance, Frequency.Hourly, remainingBatchSize);

                            if (remainingBatchSize > 0)
                            {
                                UpdateScheduleItemSetting(ScheduleHistoryItem.ScheduleID, SettingLastHourlyRun, currentRunDate.ToString());
                            }
                            
                            CompleteTask();
                        }
                        else
                        {
                            var lastDailyRun = Convert.ToDateTime(colScheduleItemSettings[SettingLastDailyRun]);
                            if (DateTime.Now.AddDays(-1) >= lastDailyRun)
                            {
                                var currentRunDate = DateTime.Now;
                                remainingBatchSize = HandleDigest(schedulerInstance, Frequency.Daily, remainingBatchSize);

                                if (remainingBatchSize > 0)
                                {
                                    UpdateScheduleItemSetting(ScheduleHistoryItem.ScheduleID, SettingLastDailyRun, currentRunDate.ToString());
                                }

                                CompleteTask();
                            }
                            // we do not support monthly at this time
                            //else
                            //{
                            //   var lastWeeklyRun = Convert.ToDateTime(colScheduleItemSettings[SettingLastWeeklyRun]);
                            //   if (DateTime.Now.AddDays(30) >= lastWeeklyRun)
                            //    {
                            //        var currentRunDate = DateTime.Now;
                            //        remainingBatchSize = HandleDigest(schedulerInstance, Frequency.Weekly, remainingBatchSize);

                            //        if (remainingBatchSize > 0)
                            //        {
                            //            SubscriptionController.Instance.UpdateScheduleItemSetting(ScheduleHistoryItem.ScheduleID, SettingLastWeeklyRun, currentRunDate.ToString());
                            //        }

                            //        CompleteTask();
                            //    }
                            //    // else handle monthly (if past 30 days, not v1). 
                            //}

                            CompleteTask();
                        }                   
                    }
                }
            }
            catch (Exception ex)
            {
                Log("SubscriptionTask failure: {0}", ex);

                ScheduleHistoryItem.Succeeded = false;

                Errored(ref ex);
            }
        }

        #endregion

        #region Private methods

        private void CompleteTask()
        {
            ScheduleHistoryItem.Succeeded = true;

        }

        private static bool SendEmail(int portalId)
        {
            return PortalController.GetPortalSetting("MessagingSendEmail", portalId, "YES") == "YES";
        }

        private void Log(string format, params object[] arguments)
        {
            ScheduleHistoryItem.AddLogNote(string.Format(format, arguments) + Environment.NewLine);
        }

        private int HandleInstant(Guid schedulerInstance)
        {
            ScheduleHistoryItem.AddLogNote("Suscriptions Starting Instant " + schedulerInstance);

            var messageLeft = true;
            var messagesSent = 0;

            while (messageLeft)
            {
                var batchMessages = GetNextMessagesForDispatch(schedulerInstance, BatchSize);

                if (batchMessages != null && batchMessages.Count > 0)
                {
                    try
                    {
                        foreach (var messageRecipient in batchMessages)
                        {
                            // add logic to see if a specific user has more than one here. 

                            SendMessage(messageRecipient);
                            messagesSent = messagesSent + 1;
                        }
                        // at this point we have sent all instant notifications 
                        ScheduleHistoryItem.AddLogNote(".  Sent " + messagesSent + " instant subscription emails for this batch.  ");
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
            return 0;
        }

        public int HandleDigest(Guid schedulerInstance, Frequency frequency, int remainingBatchSize)
        {
            var messagesSent = 0;
            // get subscribers based on frequency, utilize remaining batch size as part of count of users to return (note, if multiple subscriptions have the same frequency they will be combined into 1 email)
            ScheduleHistoryItem.AddLogNote("Suscriptions Starting Digest " + schedulerInstance + ".  ");

            var timeSpan = GetFrequencyTimeSpan(frequency);
            var messageLeft = true;

            while (messageLeft)
            {
                var batchMessages = GetNextSubscribersForDispatch(schedulerInstance, Convert.ToInt32(frequency), BatchSize);

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

                                var senderUser = _userController.GetUser(messageDetails.PortalID, singleMessage.UserID);
                                var recipientUser = _userController.GetUser(messageDetails.PortalID, singleMessage.UserID);

                                SendDigest(messageRecipients, ps, senderUser, recipientUser);
                            }
                            messagesSent = messagesSent + 1;
                        }
                        // at this point we have sent all digest notifications for this batch
                        ScheduleHistoryItem.AddLogNote(".  Sent " + messagesSent + " digest subscription emails for this batch.  ");
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

            ScheduleHistoryItem.AddLogNote(".  Sent " + messagesSent + " " + frequency + " digest subscription emails.  ");
            remainingBatchSize = remainingBatchSize - messagesSent;

            return remainingBatchSize;
        }

        private static TimeSpan GetFrequencyTimeSpan(Frequency frequency)
        {
            switch (frequency)
            {
                case Frequency.Instant:
                    return TimeSpan.FromMinutes(1d);
                case Frequency.Hourly:
                    return TimeSpan.FromHours(1d);
                case Frequency.Daily:
                    return TimeSpan.FromDays(1d);
                case Frequency.Weekly:
                    return TimeSpan.FromDays(7d);
                default:
                    throw new ArgumentOutOfRangeException("frequency");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageRecipient"></param>
        /// <remarks>Copy of core method CoreMessagingScheduler</remarks>
        private void SendMessage(MessageRecipient messageRecipient)
        {
            //TODO: check if host user can send to multiple portals...
            var messageDetails = InternalMessagingController.Instance.GetMessage(messageRecipient.MessageID);
            var author = _userController.GetUser(messageDetails.PortalID, messageDetails.SenderUserID);

            if (!SendEmail(messageDetails.PortalID))
            {
                InternalMessagingController.Instance.MarkMessageAsSent(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }

            var ps = new PortalSettings(messageDetails.PortalID);
            var fromAddress = ps.Email;
            var toUser=_userController.GetUser(messageDetails.PortalID, messageRecipient.UserID);
          
            if (toUser==null || toUser.IsDeleted)
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

            Services.Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, template);

            InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
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

        private static string GetPortalLogoUrl(PortalSettings sendingPortal)
        {
            return "http://" + sendingPortal.DefaultPortalAlias + "/" + sendingPortal.HomeDirectory + "/" + sendingPortal.LogoFile;
        }

        private static string GetPortalHomeUrl(PortalSettings sendingPortal)
        {

            return "http://" + sendingPortal.DefaultPortalAlias;
        }

        protected string GetSubscriptionsUrl(PortalSettings sendingPortal, int userId)
        {
			return "http://" + sendingPortal.DefaultPortalAlias + "/tabid/" + GetMessageTab(sendingPortal) + "/userId/" + userId + "/" + Globals.glbDefaultPage;
        }

        private static int GetMessageTab(PortalSettings sendingPortal)
        {
            var cacheKey = string.Format("MessageTab:{0}", sendingPortal.PortalId);
	        var cacheItemArgs = new CacheItemArgs(cacheKey, DefaultCacheTimeout, CacheItemPriority.Default, sendingPortal);

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

        private IList<MessageRecipient> GetNextMessagesForDispatch(Guid schedulerInstance, int batchSize)
        {
            return CBO.FillCollection<MessageRecipient>(_dataService.GetNextMessagesForDispatch(schedulerInstance, batchSize));
        }

        private IList<MessageRecipient> GetNextSubscribersForDispatch(Guid schedulerInstance, int frequency, int batchSize)
        {
            return CBO.FillCollection<MessageRecipient>(_dataService.GetNextSubscribersForDispatch(schedulerInstance, frequency, batchSize));
        }

        private void UpdateScheduleItemSetting(int scheduleId, string key, string value)
        {
            _dataService.UpdateScheduleItemSetting(scheduleId, key, value);
        }

        #endregion
    }
}
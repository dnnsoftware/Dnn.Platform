// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mail;
    using System.Text.RegularExpressions;
    using System.Web.Caching;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Social.Messaging.Internal;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// A SchedulerClient instance that handles all the offline messaging actions.
    /// </summary>
    public class CoreMessagingScheduler : SchedulerClient
    {
        /// <summary>The setting name for number hours since last hourly digest run.</summary>
        private const string SettingLastHourlyRun = "CoreMessagingLastHourlyDigestRun";

        /// <summary>The setting name for number hours since last daily digest run.</summary>
        private const string SettingLastDailyRun = "CoreMessagingLastDailyDigestRun";

        /// <summary>The setting name for number hours since last weekly digest run.</summary>
        private const string SettingLastWeeklyRun = "CoreMessagingLastWeeklyDigestRun";

        /// <summary>The setting name for number hours since last monthly digest run.</summary>
        private const string SettingLastMonthlyRun = "CoreMessagingLastMonthlyDigestRun";

        /// <summary>Initializes a new instance of the <see cref="CoreMessagingScheduler"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The object schedule history item.</param>
        public CoreMessagingScheduler(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        /// <summary>This is the method that kicks off the actual work within the SchedulerClient's subclass.</summary>
        public override void DoWork()
        {
            try
            {
                var schedulerInstance = Guid.NewGuid();
                this.ScheduleHistoryItem.AddLogNote("Messaging Scheduler DoWork Starting " + schedulerInstance);

                if (string.IsNullOrEmpty(Host.SMTPServer))
                {
                    this.ScheduleHistoryItem.AddLogNote("<br>No SMTP Servers have been configured for this host. Terminating task.");
                    this.ScheduleHistoryItem.Succeeded = true;
                }
                else
                {
                    this.Progressing();

                    var instantMessages = this.HandleInstantMessages(schedulerInstance);
                    var remainingMessages = Host.MessageSchedulerBatchSize - instantMessages;
                    if (remainingMessages > 0)
                    {
                        this.HandleFrequentDigests(schedulerInstance, remainingMessages);
                    }

                    this.ScheduleHistoryItem.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("<br>Messaging Scheduler Failed: " + ex);
                this.Errored(ref ex);
            }
        }

        /// <summary>Determines whether [is send email enable] [the specified portal identifier].</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>True if mail is enabled in PortalSettings.</returns>
        private static bool IsSendEmailEnable(int portalId)
        {
            return PortalController.GetPortalSetting("MessagingSendEmail", portalId, "YES") == "YES";
        }

        /// <summary>Determines whether [is user able to receive an email] [the specified recipient user].</summary>
        /// <param name="recipientUser">The recipient user.</param>
        /// <returns>True if the user can receive email, otherwise false.</returns>
        private static bool IsUserAbleToReceiveAnEmail(UserInfo recipientUser)
        {
            return recipientUser != null && !recipientUser.IsDeleted && recipientUser.Membership.Approved;
        }

        /// <summary>Gets the sender address.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="fromAddress">From address.</param>
        /// <returns>The formatted sender address.</returns>
        private static string GetSenderAddress(string sender, string fromAddress)
        {
            return string.Format("{0} < {1} >", sender, fromAddress);
        }

        /// <summary>Gets the email body.</summary>
        /// <param name="template">The template.</param>
        /// <param name="messageBody">The message body.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="recipientUser">The recipient user.</param>
        /// <returns>A string containing the email body with any tokens replaced.</returns>
        private static string GetEmailBody(string template, string messageBody, PortalSettings portalSettings, UserInfo recipientUser)
        {
            template = template.Replace("[MESSAGEBODY]", messageBody); // moved to top since that we we can replace tokens in there too...
            template = template.Replace("[RECIPIENTUSERID]", recipientUser.UserID.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("[RECIPIENTDISPLAYNAME]", recipientUser.DisplayName);
            template = template.Replace("[RECIPIENTEMAIL]", recipientUser.Email);
            template = template.Replace("[SITEURL]", GetPortalHomeUrl(portalSettings));
            template = template.Replace("[NOTIFICATIONURL]", GetNotificationUrl(portalSettings, recipientUser.UserID));
            template = template.Replace("[PORTALNAME]", portalSettings.PortalName);
            template = template.Replace("[LOGOURL]", GetPortalLogoUrl(portalSettings));
            template = template.Replace("[UNSUBSCRIBEURL]", GetSubscriptionsUrl(portalSettings, recipientUser.UserID));
            template = ResolveUrl(portalSettings, template);

            return template;
        }

        /// <summary>Gets the content of the email item.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="message">The message.</param>
        /// <param name="itemTemplate">The item template.</param>
        /// <returns>A string with all email tokens replaced with content.</returns>
        private static string GetEmailItemContent(PortalSettings portalSettings, MessageRecipient message, string itemTemplate)
        {
            var messageDetails = InternalMessagingController.Instance.GetMessage(message.MessageID);

            var authorId = message.CreatedByUserID > 0 ? message.CreatedByUserID : messageDetails.SenderUserID;

            var emailItemContent = itemTemplate;
            emailItemContent = emailItemContent.Replace("[TITLE]", messageDetails.Subject);
            emailItemContent = emailItemContent.Replace("[CONTENT]", messageDetails.Body);
            emailItemContent = emailItemContent.Replace("[PROFILEPICURL]", GetProfilePicUrl(portalSettings, authorId));
            emailItemContent = emailItemContent.Replace("[PROFILEURL]", GetProfileUrl(portalSettings, authorId));
            emailItemContent = emailItemContent.Replace("[DISPLAYNAME]", GetDisplayName(portalSettings, authorId));

            if (messageDetails.NotificationTypeID == 1)
            {
                var toUser = UserController.Instance.GetUser(messageDetails.PortalID, message.UserID);
                var defaultLanguage = toUser.Profile.PreferredLocale;

                var acceptUrl = GetRelationshipAcceptRequestUrl(portalSettings, authorId, "AcceptFriend");
                var profileUrl = GetProfileUrl(portalSettings, authorId);
                var linkContent = GetFriendRequestActionsTemplate(portalSettings, defaultLanguage);
                emailItemContent = emailItemContent.Replace("[FRIENDREQUESTACTIONS]", string.Format(linkContent, acceptUrl, profileUrl));
            }

            if (messageDetails.NotificationTypeID == 3)
            {
                var toUser = UserController.Instance.GetUser(messageDetails.PortalID, message.UserID);
                var defaultLanguage = toUser.Profile.PreferredLocale;

                var acceptUrl = GetRelationshipAcceptRequestUrl(portalSettings, authorId, "FollowBack");
                var profileUrl = GetProfileUrl(portalSettings, authorId);
                var linkContent = GetFollowRequestActionsTemplate(portalSettings, defaultLanguage);
                emailItemContent = emailItemContent.Replace("[FOLLOWREQUESTACTIONS]", string.Format(linkContent, acceptUrl, profileUrl));
            }

            // No social actions for the rest of notifications types
            emailItemContent = emailItemContent.Replace("[FOLLOWREQUESTACTIONS]", string.Empty);
            emailItemContent = emailItemContent.Replace("[FRIENDREQUESTACTIONS]", string.Empty);

            return emailItemContent;
        }

        /// <summary>Marks the messages as dispatched.</summary>
        /// <param name="messages">The messages.</param>
        private static void MarkMessagesAsDispatched(IEnumerable<MessageRecipient> messages)
        {
            foreach (var message in messages)
            {
                InternalMessagingController.Instance.MarkMessageAsDispatched(message.MessageID, message.RecipientID);
            }
        }

        /// <summary>Gets the email body item template.</summary>
        /// <param name="language">The language.</param>
        /// <returns>The email body template item from the Global Resource File: EMAIL_MESSAGING_DISPATCH_ITEM.</returns>
        private static string GetEmailBodyItemTemplate(PortalSettings portalSettings, string language)
        {
            return Localization.GetString("EMAIL_MESSAGING_DISPATCH_ITEM", Localization.GlobalResourceFile, portalSettings, language);
        }

        /// <summary>Gets the email body template.</summary>
        /// <param name="language">The language.</param>
        /// <returns>The email body template from the Global Resource File: EMAIL_MESSAGING_DISPATCH_BODY.</returns>
        private static string GetEmailBodyTemplate(PortalSettings portalSettings, string language)
        {
            return Localization.GetString("EMAIL_MESSAGING_DISPATCH_BODY", Localization.GlobalResourceFile, portalSettings, language);
        }

        /// <summary>Gets the email subject template.</summary>
        /// <param name="language">The language.</param>
        /// <returns>The email subject template from the Global Resource File: EMAIL_SUBJECT_FORMAT.</returns>
        private static string GetEmailSubjectTemplate(PortalSettings portalSettings, string language)
        {
            return Localization.GetString("EMAIL_SUBJECT_FORMAT", Localization.GlobalResourceFile, language);
        }

        /// <summary>Gets the friend request actions template.</summary>
        /// <param name="language">The language.</param>
        /// <returns>The friend request actions defined in the Global Resource File: EMAIL_SOCIAL_FRIENDREQUESTACTIONS.</returns>
        private static string GetFriendRequestActionsTemplate(PortalSettings portalSettings, string language)
        {
            return Localization.GetString("EMAIL_SOCIAL_FRIENDREQUESTACTIONS", Localization.GlobalResourceFile, portalSettings, language);
        }

        /// <summary>Gets the follow request actions template.</summary>
        /// <param name="language">The language.</param>
        /// <returns>The follow request actions defined in the Global Resource File: EMAIL_SOCIAL_FOLLOWREQUESTACTIONS.</returns>
        private static string GetFollowRequestActionsTemplate(PortalSettings portalSettings, string language)
        {
            return Localization.GetString("EMAIL_SOCIAL_FOLLOWREQUESTACTIONS", Localization.GlobalResourceFile, portalSettings, language);
        }

        /// <summary>Gets the name of the sender.</summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <returns>Either the display name for the sender or the portal name.</returns>
        private static string GetSenderName(string displayName, string portalName)
        {
            return string.IsNullOrEmpty(displayName) ? portalName : displayName;
        }

        /// <summary>Gets the profile pic URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The handler url to fetch the picture for the specified userId.</returns>
        private static string GetProfilePicUrl(PortalSettings portalSettings, int userId)
        {
            return string.Format(
                "http://{0}/DnnImageHandler.ashx?mode=profilepic&userId={1}&h={2}&w={3}",
                portalSettings.DefaultPortalAlias,
                userId,
                64,
                64);
        }

        /// <summary>Gets the relationship accept request URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="action">The action.</param>
        /// <returns>The handler url to fetch the relationship picture for the specified userId.</returns>
        private static string GetRelationshipAcceptRequestUrl(PortalSettings portalSettings, int userId, string action)
        {
            return string.Format(
                "http://{0}/tabid/{1}/userId/{2}/action/{3}/{4}",
                portalSettings.DefaultPortalAlias,
                portalSettings.UserTabId,
                userId.ToString(CultureInfo.InvariantCulture),
                action,
                Globals.glbDefaultPage);
        }

        /// <summary>Gets the profile URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The handler url to fetch the profile picture for the specified userId.</returns>
        private static string GetProfileUrl(PortalSettings portalSettings, int userId)
        {
            return string.Format(
                "http://{0}/tabid/{1}/userId/{2}/{3}",
                portalSettings.DefaultPortalAlias,
                portalSettings.UserTabId,
                userId.ToString(CultureInfo.InvariantCulture),
                Globals.glbDefaultPage);
        }

        /// <summary>Gets the display name.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The display name for the given user.</returns>
        private static string GetDisplayName(PortalSettings portalSettings, int userId)
        {
            return (UserController.GetUserById(portalSettings.PortalId, userId) != null)
                       ? UserController.GetUserById(portalSettings.PortalId, userId).DisplayName
                       : portalSettings.PortalName;
        }

        /// <summary>Gets the notification URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The handler url to fetch a notification for the specified userId.</returns>
        private static string GetNotificationUrl(PortalSettings portalSettings, int userId)
        {
            var cacheKey = string.Format("MessageCenterTab:{0}:{1}", portalSettings.PortalId, portalSettings.CultureCode);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
            if (messageTabId <= 0)
            {
                messageTabId = portalSettings.UserTabId;
                var profileTab = TabController.Instance.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
                if (profileTab != null)
                {
                    var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                    foreach (var tab in childTabs)
                    {
                        foreach (var kvp in ModuleController.Instance.GetTabModules(tab.TabID))
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

            return string.Format(
                "http://{0}/tabid/{1}/userId/{2}/{3}#dnnCoreNotification",
                portalSettings.DefaultPortalAlias,
                messageTabId,
                userId,
                Globals.glbDefaultPage);
        }

        /// <summary>Gets the portal logo URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>A Url to the portal logo.</returns>
        private static string GetPortalLogoUrl(PortalSettings portalSettings)
        {
            return string.Format(
                "http://{0}/{1}/{2}",
                GetDomainName(portalSettings),
                portalSettings.HomeDirectory,
                portalSettings.LogoFile);
        }

        /// <summary>Gets the name of the domain.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>Resolves the domain name (portal alias) for the specified portal.</returns>
        private static string GetDomainName(PortalSettings portalSettings)
        {
            var portalAlias = portalSettings.DefaultPortalAlias;
            return portalAlias.IndexOf("/", StringComparison.InvariantCulture) != -1 ?
                       portalAlias.Substring(0, portalAlias.IndexOf("/", StringComparison.InvariantCulture)) :
                       portalAlias;
        }

        /// <summary>Gets the portal home URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>The default portal alias url.</returns>
        private static string GetPortalHomeUrl(PortalSettings portalSettings)
        {
            return string.Format("http://{0}", portalSettings.DefaultPortalAlias);
        }

        /// <summary>Gets the subscriptions URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The url for viewing subscriptions.</returns>
        private static string GetSubscriptionsUrl(PortalSettings portalSettings, int userId)
        {
            return string.Format(
                "http://{0}/tabid/{1}/ctl/Profile/userId/{2}/pageno/3",
                portalSettings.DefaultPortalAlias,
                GetMessageTab(portalSettings),
                userId);
        }

        /// <summary>Gets the message tab.</summary>
        /// <param name="sendingPortal">The sending portal.</param>
        /// <returns>The tabId for where the Message Center is installed.</returns>
        private static int GetMessageTab(PortalSettings sendingPortal)
        {
            var cacheKey = string.Format("MessageTab:{0}", sendingPortal.PortalId);

            var cacheItemArgs = new CacheItemArgs(cacheKey, 30, CacheItemPriority.Default, sendingPortal);

            return CBO.GetCachedObject<int>(cacheItemArgs, GetMessageTabCallback);
        }

        /// <summary>Gets the message tab callback.</summary>
        /// <param name="cacheItemArgs">The cache item arguments.</param>
        /// <returns>The tab Id for the Message Center OR the user profile page tab Id.</returns>
        private static object GetMessageTabCallback(CacheItemArgs cacheItemArgs)
        {
            var portalSettings = cacheItemArgs.Params[0] as PortalSettings;

            var profileTab = TabController.Instance.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
            if (profileTab != null)
            {
                var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                foreach (var tab in childTabs)
                {
                    foreach (var kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center")
                        {
                            return tab.TabID;
                        }
                    }
                }
            }

            // default to User Profile Page
            return portalSettings.UserTabId;
        }

        private static string RemoveHttpUrlsIfSiteisSSLEnabled(string stringContainingHttp, PortalSettings portalSettings)
        {
            if (stringContainingHttp.IndexOf("http") > -1 && portalSettings != null && (portalSettings.SSLEnabled || portalSettings.SSLEnforced))
            {
                var urlToReplace = GetPortalHomeUrl(portalSettings);
                var urlReplaceWith = $"https://{portalSettings.DefaultPortalAlias}";
                stringContainingHttp = stringContainingHttp.Replace(urlToReplace, urlReplaceWith);
            }

            return stringContainingHttp;
        }

        private static string ResolveUrl(PortalSettings portalSettings, string template)
        {
            const string linkRegex = "(href|src)=\"(/[^\"]*?)\"";
            var matches = Regex.Matches(template, linkRegex, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var link = match.Groups[2].Value;
                var defaultAlias = portalSettings.DefaultPortalAlias;
                var domain = Globals.AddHTTP(defaultAlias);
                if (defaultAlias.Contains("/"))
                {
                    var subDomain =
                        defaultAlias.Substring(defaultAlias.IndexOf("/", StringComparison.InvariantCultureIgnoreCase));
                    if (link.StartsWith(subDomain, StringComparison.InvariantCultureIgnoreCase))
                    {
                        link = link.Substring(subDomain.Length);
                    }
                }

                template = template.Replace(match.Value, $"{match.Groups[1].Value}=\"{domain}{link}\"");
            }

            return template;
        }

        /// <summary>Handles the frequent digests.</summary>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="remainingMessages">The remaining messages.</param>
        private void HandleFrequentDigests(Guid schedulerInstance, int remainingMessages)
        {
            var handledMessages = this.HandleFrequencyDigest(DateTime.Now.AddHours(-1), SettingLastHourlyRun, Frequency.Hourly, schedulerInstance, remainingMessages);
            remainingMessages = remainingMessages - handledMessages;

            handledMessages = this.HandleFrequencyDigest(DateTime.Now.AddDays(-1), SettingLastDailyRun, Frequency.Daily, schedulerInstance, remainingMessages);
            remainingMessages = remainingMessages - handledMessages;

            handledMessages = this.HandleFrequencyDigest(DateTime.Now.AddDays(-7), SettingLastWeeklyRun, Frequency.Weekly, schedulerInstance, remainingMessages);
            remainingMessages = remainingMessages - handledMessages;

            this.HandleFrequencyDigest(DateTime.Now.AddDays(-30), SettingLastMonthlyRun, Frequency.Monthly, schedulerInstance, remainingMessages);
        }

        /// <summary>Handles the frequency digest.</summary>
        /// <param name="dateToCompare">The date to compare.</param>
        /// <param name="settingKeyLastRunDate">The setting key last run date.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="remainingMessages">The remaining messages.</param>
        /// <returns>The count of messages processed.</returns>
        private int HandleFrequencyDigest(DateTime dateToCompare, string settingKeyLastRunDate, Frequency frequency, Guid schedulerInstance, int remainingMessages)
        {
            int handledMessages = 0;
            if (remainingMessages <= 0)
            {
                return handledMessages;
            }

            var lastRunDate = this.GetScheduleItemDateSetting(settingKeyLastRunDate);
            if (dateToCompare >= lastRunDate)
            {
                handledMessages = this.HandleDigest(schedulerInstance, frequency, remainingMessages);
                if (handledMessages < remainingMessages)
                {
                    SchedulingProvider.Instance().AddScheduleItemSetting(
                        this.ScheduleHistoryItem.ScheduleID, settingKeyLastRunDate, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                }
            }

            return handledMessages;
        }

        /// <summary>Handles the digest.</summary>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="remainingMessages">The remaining messages.</param>
        /// <returns>The count of messages processed.</returns>
        private int HandleDigest(Guid schedulerInstance, Frequency frequency, int remainingMessages)
        {
            var messagesSent = 0;

            // get subscribers based on frequency, utilize remaining batch size as part of count of users to return (note, if multiple subscriptions have the same frequency they will be combined into 1 email)
            this.ScheduleHistoryItem.AddLogNote("<br>Messaging Scheduler Starting Digest '" + schedulerInstance + "'.  ");

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
                            var colUserMessages = from t in batchMessages where t.UserID == currentUserId select t;
                            var messageRecipients = colUserMessages as MessageRecipient[] ?? colUserMessages.ToArray();
                            var singleMessage = (from t in messageRecipients select t).First();
                            if (singleMessage != null)
                            {
                                var messageDetails = InternalMessagingController.Instance.GetMessage(singleMessage.MessageID);
                                var portalSettings = new PortalSettings(messageDetails.PortalID);

                                var senderUser = UserController.Instance.GetUser(messageDetails.PortalID, messageDetails.SenderUserID);
                                var recipientUser = UserController.Instance.GetUser(messageDetails.PortalID, singleMessage.UserID);

                                this.SendDigest(messageRecipients, portalSettings, senderUser, recipientUser);
                            }

                            messagesSent = messagesSent + 1;
                        }

                        // at this point we have sent all digest notifications for this batch
                        this.ScheduleHistoryItem.AddLogNote("Sent " + messagesSent + " digest subscription emails for this batch.  ");
                        return messagesSent;
                    }
                    catch (Exception e)
                    {
                        this.Errored(ref e);
                    }
                }
                else
                {
                    messageLeft = false;
                }
            }

            this.ScheduleHistoryItem.AddLogNote("Sent " + messagesSent + " " + frequency + " digest subscription emails.  ");

            return messagesSent;
        }

        /// <summary>Sends the digest.</summary>
        /// <param name="messages">The messages.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="senderUser">The sender user.</param>
        /// <param name="recipientUser">The recipient user.</param>
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

            var emailSubjectTemplate = GetEmailSubjectTemplate(portalSettings, defaultLanguage);
            var emailBodyTemplate = GetEmailBodyTemplate(portalSettings, defaultLanguage);
            var emailBodyItemTemplate = GetEmailBodyItemTemplate(portalSettings, defaultLanguage);

            var emailBodyItemContent = messageRecipients.Aggregate(
                string.Empty,
                (current, message) => current + GetEmailItemContent(portalSettings, message, emailBodyItemTemplate));

            var fromAddress = portalSettings.Email;
            var toAddress = recipientUser.Email;

            var senderName = GetSenderName(senderUser.DisplayName, portalSettings.PortalName);
            var senderAddress = GetSenderAddress(senderName, fromAddress);

            var subject = string.Format(emailSubjectTemplate, portalSettings.PortalName);
            var body = GetEmailBody(emailBodyTemplate, emailBodyItemContent, portalSettings, recipientUser);
            body = RemoveHttpUrlsIfSiteisSSLEnabled(body, portalSettings);

            Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, body);

            MarkMessagesAsDispatched(messageRecipients);
        }

        /// <summary>Gets the schedule item date setting.</summary>
        /// <param name="settingKey">The setting key.</param>
        /// <returns>The date the schedule was ran.</returns>
        private DateTime GetScheduleItemDateSetting(string settingKey)
        {
            var colScheduleItemSettings = SchedulingProvider.Instance().GetScheduleItemSettings(this.ScheduleHistoryItem.ScheduleID);
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
                    this.ScheduleHistoryItem.ScheduleID, SettingLastHourlyRun, dateValue.ToString(CultureInfo.InvariantCulture));
                SchedulingProvider.Instance().AddScheduleItemSetting(
                   this.ScheduleHistoryItem.ScheduleID, SettingLastDailyRun, dateValue.ToString(CultureInfo.InvariantCulture));
                SchedulingProvider.Instance().AddScheduleItemSetting(
                   this.ScheduleHistoryItem.ScheduleID, SettingLastWeeklyRun, dateValue.ToString(CultureInfo.InvariantCulture));
                SchedulingProvider.Instance().AddScheduleItemSetting(
                   this.ScheduleHistoryItem.ScheduleID, SettingLastMonthlyRun, dateValue.ToString(CultureInfo.InvariantCulture));
            }

            return dateValue;
        }

        /// <summary>Handles the sending of messages.</summary>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <returns>The count of messages sent.</returns>
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
                            this.SendMessage(messageRecipient);
                            messagesSent = messagesSent + 1;
                        }
                    }
                    catch (Exception e)
                    {
                        this.Errored(ref e);
                    }
                }
                else
                {
                    messageLeft = false;
                }
            }

            this.ScheduleHistoryItem.AddLogNote(string.Format("<br>Messaging Scheduler '{0}' sent a total of {1} message(s)", schedulerInstance, messagesSent));
            return messagesSent;
        }

        /// <summary>Sends the message and attachments if configured to include them.</summary>
        /// <param name="messageRecipient">The message recipient.</param>
        private void SendMessage(MessageRecipient messageRecipient)
        {
            var message = InternalMessagingController.Instance.GetMessage(messageRecipient.MessageID);

            var toUser = UserController.Instance.GetUser(message.PortalID, messageRecipient.UserID);
            if (!IsUserAbleToReceiveAnEmail(toUser))
            {
                InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }

            if (!IsSendEmailEnable(message.PortalID))
            {
                InternalMessagingController.Instance.MarkMessageAsSent(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }

            var defaultLanguage = toUser.Profile.PreferredLocale;
            var portalSettings = new PortalSettings(message.PortalID);

            var emailSubjectTemplate = GetEmailSubjectTemplate(portalSettings, defaultLanguage);
            var emailBodyTemplate = GetEmailBodyTemplate(portalSettings, defaultLanguage);
            var emailBodyItemTemplate = GetEmailBodyItemTemplate(portalSettings, defaultLanguage);

            var author = UserController.Instance.GetUser(message.PortalID, message.SenderUserID);
            var fromAddress = (UserController.GetUserByEmail(portalSettings.PortalId, portalSettings.Email) != null) ?
                string.Format("{0} < {1} >", UserController.GetUserByEmail(portalSettings.PortalId, portalSettings.Email).DisplayName, portalSettings.Email) : portalSettings.Email;
            var toAddress = toUser.Email;

            if (Mail.Mail.IsValidEmailAddress(toUser.Email, toUser.PortalID))
            {
                var senderName = GetSenderName(author.DisplayName, portalSettings.PortalName);
                var senderAddress = GetSenderAddress(senderName, portalSettings.Email);
                var emailBodyItemContent = GetEmailItemContent(portalSettings, messageRecipient, emailBodyItemTemplate);
                var subject = string.Format(emailSubjectTemplate, portalSettings.PortalName);
                var body = GetEmailBody(emailBodyTemplate, emailBodyItemContent, portalSettings, toUser);
                body = RemoveHttpUrlsIfSiteisSSLEnabled(body, portalSettings);

                // Include the attachment in the email message if configured to do so
                if (InternalMessagingController.Instance.AttachmentsAllowed(message.PortalID))
                {
                    Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, body, this.CreateAttachments(message.MessageID).ToList());
                }
                else
                {
                    Mail.Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, body);
                }
            }

            InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
        }

        /// <summary>Creates list of attachments for the specified message.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A list of attachments.</returns>
        private IEnumerable<Attachment> CreateAttachments(int messageId)
        {
            foreach (var fileView in InternalMessagingController.Instance.GetAttachments(messageId))
            {
                var file = FileManager.Instance.GetFile(fileView.FileId);
                var fileContent = FileManager.Instance.GetFileContent(file);
                if (file != null)
                {
                    yield return new Attachment(fileContent, file.ContentType);
                }
            }
        }
    }
}

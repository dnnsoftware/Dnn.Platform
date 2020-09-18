// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications.Data;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// Provides the methods to work with Notifications, NotificationTypes, NotificationTypeActions and NotificationActions.
    /// </summary>
    public class NotificationsController
                        : ServiceLocator<INotificationsController, NotificationsController>,
                        INotificationsController
    {
        internal const int ConstMaxSubject = 400;
        internal const int ConstMaxTo = 2000;
        private const string ToastsCacheKey = "GetToasts_{0}";
        private readonly IDataService _dataService;
        private readonly Messaging.Data.IDataService _messagingDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsController"/> class.
        /// Default constructor.
        /// </summary>
        public NotificationsController()
            : this(DataService.Instance, Messaging.Data.DataService.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsController"/> class.
        /// Constructor from specifict data service.
        /// </summary>
        /// <param name="dataService">Class with methods to do CRUD in database for the entities of types <see cref="NotificationType"></see>, <see cref="NotificationTypeAction"></see> and <see cref="Notification"></see>.</param>
        /// <param name="messagingDataService">Class with methods to do CRUD in database for the entities of types <see cref="Message"></see>, <see cref="MessageRecipient"></see> and <see cref="MessageAttachment"></see> and to interact with the stored procedures regarding messaging.</param>
        public NotificationsController(IDataService dataService, Messaging.Data.IDataService messagingDataService)
        {
            Requires.NotNull("dataService", dataService);
            Requires.NotNull("messagingDataService", messagingDataService);

            this._dataService = dataService;
            this._messagingDataService = messagingDataService;
        }

        public void SetNotificationTypeActions(IList<NotificationTypeAction> actions, int notificationTypeId)
        {
            Requires.NotNull("actions", actions);

            if (!actions.Any())
            {
                throw new ArgumentException("Actions must contain at least one item.");
            }

            if (actions.Any(x => string.IsNullOrEmpty(x.APICall)))
            {
                throw new ArgumentException("All actions must specify an APICall");
            }

            if (actions.Any(x => string.IsNullOrEmpty(x.NameResourceKey)))
            {
                throw new ArgumentException("All actions must specify a NameResourceKey");
            }

            foreach (var action in actions)
            {
                action.NotificationTypeActionId = this._dataService.AddNotificationTypeAction(
                    notificationTypeId,
                    action.NameResourceKey,
                    action.DescriptionResourceKey,
                    action.ConfirmResourceKey,
                    action.APICall,
                    this.GetCurrentUserId());
                action.NotificationTypeId = notificationTypeId;
            }
        }

        public virtual int CountNotifications(int userId, int portalId)
        {
            if (userId <= 0)
            {
                return 0;
            }

            var cacheKey = string.Format(DataCache.UserNotificationsCountCacheKey, portalId, userId);
            var cache = CachingProvider.Instance();
            var cacheObject = cache.GetItem(cacheKey);
            if (cacheObject is int)
            {
                return (int)cacheObject;
            }

            var count = this._dataService.CountNotifications(userId, portalId);
            cache.Insert(cacheKey, count, (DNNCacheDependency)null,
                DateTime.Now.AddSeconds(DataCache.NotificationsCacheTimeInSec), System.Web.Caching.Cache.NoSlidingExpiration);
            return count;
        }

        public virtual void SendNotification(Notification notification, int portalId, IList<RoleInfo> roles, IList<UserInfo> users)
        {
            Requires.NotNull("notification", notification);

            var pid = portalId;
            if (PortalController.IsMemberOfPortalGroup(portalId))
            {
                pid = PortalController.GetEffectivePortalId(portalId);
            }

            if (notification.SenderUserID < 1)
            {
                notification.SenderUserID = this.GetAdminUser().UserID;
            }

            if (string.IsNullOrEmpty(notification.Subject) && string.IsNullOrEmpty(notification.Body))
            {
                throw new ArgumentException(Localization.GetString("MsgSubjectOrBodyRequiredError", Localization.ExceptionsResourceFile));
            }

            if (roles == null && users == null)
            {
                throw new ArgumentException(Localization.GetString("MsgRolesOrUsersRequiredError", Localization.ExceptionsResourceFile));
            }

            if (!string.IsNullOrEmpty(notification.Subject) && notification.Subject.Length > ConstMaxSubject)
            {
                throw new ArgumentException(string.Format(Localization.GetString("MsgSubjectTooBigError", Localization.ExceptionsResourceFile), ConstMaxSubject, notification.Subject.Length));
            }

            var sbTo = new StringBuilder();
            if (roles != null)
            {
                foreach (var role in roles.Where(role => !string.IsNullOrEmpty(role.RoleName)))
                {
                    sbTo.Append(role.RoleName + ",");
                }
            }

            if (users != null)
            {
                foreach (var user in users.Where(user => !string.IsNullOrEmpty(user.DisplayName)))
                {
                    sbTo.Append(user.DisplayName + ",");
                }
            }

            if (sbTo.Length == 0)
            {
                throw new ArgumentException(Localization.GetString("MsgEmptyToListFoundError", Localization.ExceptionsResourceFile));
            }

            if (sbTo.Length > ConstMaxTo)
            {
                throw new ArgumentException(string.Format(Localization.GetString("MsgToListTooBigError", Localization.ExceptionsResourceFile), ConstMaxTo, sbTo.Length));
            }

            notification.To = sbTo.ToString().Trim(',');
            if (notification.ExpirationDate != default(DateTime))
            {
                notification.ExpirationDate = this.GetExpirationDate(notification.NotificationTypeID);
            }

            notification.NotificationID = this._dataService.SendNotification(notification, pid);

            // send message to Roles
            if (roles != null)
            {
                var roleIds = string.Empty;
                roleIds = roles
                    .Select(r => r.RoleID)
                    .Aggregate(roleIds, (current, roleId) => current + (roleId + ","))
                    .Trim(',');

                this._messagingDataService.CreateMessageRecipientsForRole(
                    notification.NotificationID,
                    roleIds,
                    UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // send message to each User - this should be called after CreateMessageRecipientsForRole.
            if (users == null)
            {
                users = new List<UserInfo>();
            }

            var recipients = from user in users
                             where InternalMessagingController.Instance.GetMessageRecipient(notification.NotificationID, user.UserID) == null
                             select new MessageRecipient
                             {
                                 MessageID = notification.NotificationID,
                                 UserID = user.UserID,
                                 Read = false,
                                 RecipientID = Null.NullInteger,
                             };

            foreach (var recipient in recipients)
            {
                this._messagingDataService.SaveMessageRecipient(
                    recipient,
                    UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // if sendToast is true, then mark all recipients' as ready for toast.
            if (notification.SendToast)
            {
                foreach (var messageRecipient in InternalMessagingController.Instance.GetMessageRecipients(notification.NotificationID))
                {
                    this.MarkReadyForToast(notification, messageRecipient.UserID);
                }
            }
        }

        public void CreateNotificationType(NotificationType notificationType)
        {
            Requires.NotNull("notificationType", notificationType);
            Requires.NotNullOrEmpty("notificationType.Name", notificationType.Name);

            if (notificationType.DesktopModuleId <= 0)
            {
                notificationType.DesktopModuleId = Null.NullInteger;
            }

            notificationType.NotificationTypeId = this._dataService.CreateNotificationType(
                notificationType.Name,
                notificationType.Description,
                (int)notificationType.TimeToLive.TotalMinutes == 0 ? Null.NullInteger : (int)notificationType.TimeToLive.TotalMinutes,
                notificationType.DesktopModuleId,
                this.GetCurrentUserId(),
                notificationType.IsTask);
        }

        public virtual void DeleteNotification(int notificationId)
        {
            var recipients = InternalMessagingController.Instance.GetMessageRecipients(notificationId);
            foreach (var recipient in recipients)
            {
                DataCache.RemoveCache(string.Format(ToastsCacheKey, recipient.UserID));
            }

            this._dataService.DeleteNotification(notificationId);
        }

        public int DeleteUserNotifications(UserInfo user)
        {
            DataCache.RemoveCache(string.Format(ToastsCacheKey, user.UserID));
            return this._dataService.DeleteUserNotifications(user.UserID, user.PortalID);
        }

        public virtual void DeleteNotificationRecipient(int notificationId, int userId)
        {
            DataCache.RemoveCache(string.Format(ToastsCacheKey, userId));
            InternalMessagingController.Instance.DeleteMessageRecipient(notificationId, userId);
            var recipients = InternalMessagingController.Instance.GetMessageRecipients(notificationId);
            if (recipients.Count == 0)
            {
                this.DeleteNotification(notificationId);
            }
        }

        public virtual void DeleteAllNotificationRecipients(int notificationId)
        {
            foreach (var recipient in InternalMessagingController.Instance.GetMessageRecipients(notificationId))
            {
                this.DeleteNotificationRecipient(notificationId, recipient.UserID);
            }
        }

        public virtual void DeleteNotificationRecipient(int notificationTypeId, string context, int userId)
        {
            foreach (var notification in this.GetNotificationByContext(notificationTypeId, context))
            {
                this.DeleteNotificationRecipient(notification.NotificationID, userId);
            }
        }

        public Notification GetNotification(int notificationId)
        {
            return CBO.FillObject<Notification>(this._dataService.GetNotification(notificationId));
        }

        public virtual IList<Notification> GetNotificationByContext(int notificationTypeId, string context)
        {
            return CBO.FillCollection<Notification>(this._dataService.GetNotificationByContext(notificationTypeId, context));
        }

        public virtual void DeleteNotificationType(int notificationTypeId)
        {
            this._dataService.DeleteNotificationType(notificationTypeId);

            this.RemoveNotificationTypeCache();
        }

        public virtual void DeleteNotificationTypeAction(int notificationTypeActionId)
        {
            this._dataService.DeleteNotificationTypeAction(notificationTypeActionId);

            this.RemoveNotificationTypeActionCache();
        }

        public virtual IList<Notification> GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords)
        {
            var pid = portalId;
            if (PortalController.IsMemberOfPortalGroup(portalId))
            {
                pid = PortalController.GetEffectivePortalId(portalId);
            }

            return userId <= 0
                ? new List<Notification>(0)
                : CBO.FillCollection<Notification>(this._dataService.GetNotifications(userId, pid, afterNotificationId, numberOfRecords));
        }

        public virtual NotificationType GetNotificationType(int notificationTypeId)
        {
            var notificationTypeCacheKey = string.Format(DataCache.NotificationTypesCacheKey, notificationTypeId);
            var cacheItemArgs = new CacheItemArgs(notificationTypeCacheKey, DataCache.NotificationTypesTimeOut, DataCache.NotificationTypesCachePriority, notificationTypeId);
            return CBO.GetCachedObject<NotificationType>(cacheItemArgs, this.GetNotificationTypeCallBack);
        }

        public virtual NotificationType GetNotificationType(string name)
        {
            Requires.NotNullOrEmpty("name", name);

            var notificationTypeCacheKey = string.Format(DataCache.NotificationTypesCacheKey, name);
            var cacheItemArgs = new CacheItemArgs(notificationTypeCacheKey, DataCache.NotificationTypesTimeOut, DataCache.NotificationTypesCachePriority, name);
            return CBO.GetCachedObject<NotificationType>(cacheItemArgs, this.GetNotificationTypeByNameCallBack);
        }

        public virtual NotificationTypeAction GetNotificationTypeAction(int notificationTypeActionId)
        {
            var notificationTypeActionCacheKey = string.Format(DataCache.NotificationTypeActionsCacheKey, notificationTypeActionId);
            var cacheItemArgs = new CacheItemArgs(notificationTypeActionCacheKey, DataCache.NotificationTypeActionsTimeOut, DataCache.NotificationTypeActionsPriority, notificationTypeActionId);
            return CBO.GetCachedObject<NotificationTypeAction>(cacheItemArgs, this.GetNotificationTypeActionCallBack);
        }

        public virtual NotificationTypeAction GetNotificationTypeAction(int notificationTypeId, string name)
        {
            Requires.NotNullOrEmpty("name", name);

            var notificationTypeActionCacheKey = string.Format(DataCache.NotificationTypeActionsByNameCacheKey, notificationTypeId, name);
            var cacheItemArgs = new CacheItemArgs(notificationTypeActionCacheKey, DataCache.NotificationTypeActionsTimeOut, DataCache.NotificationTypeActionsPriority, notificationTypeId, name);
            return CBO.GetCachedObject<NotificationTypeAction>(cacheItemArgs, this.GetNotificationTypeActionByNameCallBack);
        }

        public virtual IList<NotificationTypeAction> GetNotificationTypeActions(int notificationTypeId)
        {
            return CBO.FillCollection<NotificationTypeAction>(this._dataService.GetNotificationTypeActions(notificationTypeId));
        }

        public bool IsToastPending(int notificationId)
        {
            return this._dataService.IsToastPending(notificationId);
        }

        public void MarkReadyForToast(Notification notification, UserInfo userInfo)
        {
            this.MarkReadyForToast(notification, userInfo.UserID);
        }

        public void MarkReadyForToast(Notification notification, int userId)
        {
            DataCache.RemoveCache(string.Format(ToastsCacheKey, userId));
            this._dataService.MarkReadyForToast(notification.NotificationID, userId);
        }

        public void MarkToastSent(int notificationId, int userId)
        {
            this._dataService.MarkToastSent(notificationId, userId);
        }

        public IList<Notification> GetToasts(UserInfo userInfo)
        {
            var cacheKey = string.Format(ToastsCacheKey, userInfo.UserID);
            var toasts = DataCache.GetCache<IList<Notification>>(cacheKey);

            if (toasts == null)
            {
                toasts = CBO.FillCollection<Notification>(this._dataService.GetToasts(userInfo.UserID, userInfo.PortalID));
                foreach (var message in toasts)
                {
                    this._dataService.MarkToastSent(message.NotificationID, userInfo.UserID);
                }

                // Set the cache to empty toasts object because we don't want to make calls to database everytime for empty objects.
                // This empty object cache would be cleared by MarkReadyForToast emthod when a new notification arrives for the user.
                DataCache.SetCache(cacheKey, new List<Notification>());
            }

            return toasts;
        }

        internal virtual UserInfo GetAdminUser()
        {
            var current = PortalSettings.Current;
            return current == null
                ? new UserInfo()
                : UserController.GetUserById(current.PortalId, current.AdministratorId);
        }

        internal virtual int GetCurrentUserId()
        {
            return UserController.Instance.GetCurrentUserInfo().UserID;
        }

        internal virtual DateTime GetExpirationDate(int notificationTypeId)
        {
            var notificationType = this.GetNotificationType(notificationTypeId);

            return notificationType.TimeToLive.TotalMinutes > 0
                ? DateTime.UtcNow.AddMinutes(notificationType.TimeToLive.TotalMinutes)
                : DateTime.MinValue;
        }

        internal virtual object GetNotificationTypeActionCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationTypeActionId = (int)cacheItemArgs.ParamList[0];
            return CBO.FillObject<NotificationTypeAction>(this._dataService.GetNotificationTypeAction(notificationTypeActionId));
        }

        internal virtual object GetNotificationTypeActionByNameCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationTypeId = (int)cacheItemArgs.ParamList[0];
            var name = cacheItemArgs.ParamList[1].ToString();
            return CBO.FillObject<NotificationTypeAction>(this._dataService.GetNotificationTypeActionByName(notificationTypeId, name));
        }

        internal virtual object GetNotificationTypeByNameCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationName = cacheItemArgs.ParamList[0].ToString();
            return CBO.FillObject<NotificationType>(this._dataService.GetNotificationTypeByName(notificationName));
        }

        internal virtual object GetNotificationTypeCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationTypeId = (int)cacheItemArgs.ParamList[0];
            return CBO.FillObject<NotificationType>(this._dataService.GetNotificationType(notificationTypeId));
        }

        internal virtual string GetPortalSetting(string settingName, int portalId, string defaultValue)
        {
            return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
        }

        internal virtual string InputFilter(string input)
        {
            var ps = PortalSecurity.Instance;
            return ps.InputFilter(input, PortalSecurity.FilterFlag.NoProfanity);
        }

        internal virtual void RemoveNotificationTypeActionCache()
        {
            DataCache.ClearCache("NotificationTypeActions:");
        }

        internal virtual void RemoveNotificationTypeCache()
        {
            DataCache.ClearCache("NotificationTypes:");
        }

        protected override Func<INotificationsController> GetFactory()
        {
            return () => new NotificationsController();
        }
    }
}

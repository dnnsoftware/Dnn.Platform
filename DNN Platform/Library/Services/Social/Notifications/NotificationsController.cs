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
using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Services.Social.Messaging.Exceptions;
using DotNetNuke.Services.Social.Messaging.Internal;
using DotNetNuke.Services.Social.Notifications.Data;

namespace DotNetNuke.Services.Social.Notifications
{
    /// <summary>
    /// Provides the methods to work with Notifications, NotificationTypes, NotificationTypeActions and NotificationActions.
    /// </summary>
    public class NotificationsController 
                        : ServiceLocator<INotificationsController, NotificationsController>
                        , INotificationsController
    {
        protected override Func<INotificationsController> GetFactory()
        {
            return () => new NotificationsController();
        }

        #region Constants

        internal const int ConstMaxSubject = 400;
        internal const int ConstMaxTo = 2000;

        #endregion

        #region Private Variables

        private readonly IDataService _dataService;
        private readonly Messaging.Data.IDataService _messagingDataService;

        #endregion

        #region Constructors

        public NotificationsController()
            : this(DataService.Instance, Messaging.Data.DataService.Instance)
        {
        }

        public NotificationsController(IDataService dataService, Messaging.Data.IDataService messagingDataService)
        {
            Requires.NotNull("dataService", dataService);
            Requires.NotNull("messagingDataService", messagingDataService);

            _dataService = dataService;
            _messagingDataService = messagingDataService;
        }

        #endregion

        #region Public API

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
                action.NotificationTypeActionId = _dataService.AddNotificationTypeAction(notificationTypeId,
                                                                                         action.NameResourceKey,
                                                                                         action.DescriptionResourceKey,
                                                                                         action.ConfirmResourceKey,
                                                                                         action.APICall,
                                                                                         GetCurrentUserId());
                action.NotificationTypeId = notificationTypeId;
            }
        }

        public virtual int CountNotifications(int userId, int portalId)
        {
            return _dataService.CountNotifications(userId, portalId);
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
                notification.SenderUserID = GetAdminUser().UserID;
            }

            if (string.IsNullOrEmpty(notification.Subject) && string.IsNullOrEmpty(notification.Body))
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgSubjectOrBodyRequiredError", Localization.Localization.ExceptionsResourceFile));
            }

            if (roles == null && users == null)
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgRolesOrUsersRequiredError", Localization.Localization.ExceptionsResourceFile));
            }

            if (!string.IsNullOrEmpty(notification.Subject) && notification.Subject.Length > ConstMaxSubject)
            {
                throw new ArgumentException(string.Format(Localization.Localization.GetString("MsgSubjectTooBigError", Localization.Localization.ExceptionsResourceFile), ConstMaxSubject, notification.Subject.Length));
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
                foreach (var user in users.Where(user => !string.IsNullOrEmpty(user.DisplayName))) sbTo.Append(user.DisplayName + ",");
            }

            if (sbTo.Length == 0)
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgEmptyToListFoundError", Localization.Localization.ExceptionsResourceFile));
            }

            if (sbTo.Length > ConstMaxTo)
            {
                throw new ArgumentException(string.Format(Localization.Localization.GetString("MsgToListTooBigError", Localization.Localization.ExceptionsResourceFile), ConstMaxTo, sbTo.Length));
            }

            //Cannot exceed RecipientLimit
            var recipientCount = 0;
            if (users != null) recipientCount += users.Count;
            if (roles != null) recipientCount += roles.Count;
            if (recipientCount > InternalMessagingController.Instance.RecipientLimit(pid))
            {
                throw new RecipientLimitExceededException(Localization.Localization.GetString("MsgRecipientLimitExceeded", Localization.Localization.ExceptionsResourceFile));
            }

            //Profanity Filter
            var profanityFilterSetting = GetPortalSetting("MessagingProfanityFilters", pid, "NO");
            if (profanityFilterSetting.Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                notification.Subject = InputFilter(notification.Subject);
                notification.Body = InputFilter(notification.Body);
            }

            notification.To = sbTo.ToString().Trim(',');
            if (notification.ExpirationDate != new DateTime())
            {
                notification.ExpirationDate = GetExpirationDate(notification.NotificationTypeID);
            }

            notification.NotificationID = _dataService.SendNotification(notification, pid);

            //send message to Roles
            if (roles != null)
            {
                var roleIds = string.Empty;
                roleIds = roles
                    .Select(r => r.RoleID)
                    .Aggregate(roleIds, (current, roleId) => current + (roleId + ","))
                    .Trim(',');

                _messagingDataService.CreateMessageRecipientsForRole(
                    notification.NotificationID,
                    roleIds,
                    UserController.GetCurrentUserInfo().UserID);
            }

            //send message to each User - this should be called after CreateMessageRecipientsForRole.
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
                                            RecipientID = Null.NullInteger
                                        };

            foreach (var recipient in recipients)
            {
                _messagingDataService.SaveMessageRecipient(
                    recipient,
                    UserController.GetCurrentUserInfo().UserID);
            }

            //if sendToast is true, then mark all recipients' as ready for toast.
            if (notification.SendToast)
            {
                foreach (var messageRecipient in InternalMessagingController.Instance.GetMessageRecipients(notification.NotificationID))
                {
                    MarkReadyForToast(notification, messageRecipient.UserID);
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

            notificationType.NotificationTypeId = _dataService.CreateNotificationType(notificationType.Name,
                                                                                      notificationType.Description,
                                                                                      (int)notificationType.TimeToLive.TotalMinutes == 0 ? Null.NullInteger : (int)notificationType.TimeToLive.TotalMinutes,
                                                                                      notificationType.DesktopModuleId,
                                                                                      GetCurrentUserId());
        }

        public virtual void DeleteNotification(int notificationId)
        {
            _dataService.DeleteNotification(notificationId);
        }

        public virtual void DeleteNotificationRecipient(int notificationId, int userId)
        {
            InternalMessagingController.Instance.DeleteMessageRecipient(notificationId, userId);
            var recipients = InternalMessagingController.Instance.GetMessageRecipients(notificationId);
            if (recipients.Count == 0)
            {
                DeleteNotification(notificationId);
            }
        }

        public virtual void DeleteAllNotificationRecipients(int notificationId)
        {
            foreach (var recipient in InternalMessagingController.Instance.GetMessageRecipients(notificationId))
            {
                DeleteNotificationRecipient(notificationId, recipient.UserID);
            }
        }

        public virtual void DeleteNotificationRecipient(int notificationTypeId, string context, int userId)
        {
            foreach (var notification in GetNotificationByContext(notificationTypeId, context))
            {
                DeleteNotificationRecipient(notification.NotificationID, userId);
            }
        }

        public Notification GetNotification(int notificationId)
        {
            return CBO.FillObject<Notification>(_dataService.GetNotification(notificationId));
        }

        public virtual IList<Notification> GetNotificationByContext(int notificationTypeId, string context)
        {
            return CBO.FillCollection<Notification>(_dataService.GetNotificationByContext(notificationTypeId, context));
        }

        public virtual void DeleteNotificationType(int notificationTypeId)
        {
            _dataService.DeleteNotificationType(notificationTypeId);

            RemoveNotificationTypeCache();
        }

        public virtual void DeleteNotificationTypeAction(int notificationTypeActionId)
        {
            _dataService.DeleteNotificationTypeAction(notificationTypeActionId);

            RemoveNotificationTypeActionCache();
        }

        public virtual IList<Notification> GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords)
        {
            var pid = portalId;
            if (PortalController.IsMemberOfPortalGroup(portalId))
            {
                pid = PortalController.GetEffectivePortalId(portalId);
            }
            return CBO.FillCollection<Notification>(_dataService.GetNotifications(userId, pid, afterNotificationId, numberOfRecords));
        }

        public virtual NotificationType GetNotificationType(int notificationTypeId)
        {
            var notificationTypeCacheKey = string.Format(DataCache.NotificationTypesCacheKey, notificationTypeId);
            var cacheItemArgs = new CacheItemArgs(notificationTypeCacheKey, DataCache.NotificationTypesTimeOut, DataCache.NotificationTypesCachePriority, notificationTypeId);
            return CBO.GetCachedObject<NotificationType>(cacheItemArgs, GetNotificationTypeCallBack);
        }

        public virtual NotificationType GetNotificationType(string name)
        {
            Requires.NotNullOrEmpty("name", name);

            var notificationTypeCacheKey = string.Format(DataCache.NotificationTypesCacheKey, name);
            var cacheItemArgs = new CacheItemArgs(notificationTypeCacheKey, DataCache.NotificationTypesTimeOut, DataCache.NotificationTypesCachePriority, name);
            return CBO.GetCachedObject<NotificationType>(cacheItemArgs, GetNotificationTypeByNameCallBack);
        }

        public virtual NotificationTypeAction GetNotificationTypeAction(int notificationTypeActionId)
        {
            var notificationTypeActionCacheKey = string.Format(DataCache.NotificationTypeActionsCacheKey, notificationTypeActionId);
            var cacheItemArgs = new CacheItemArgs(notificationTypeActionCacheKey, DataCache.NotificationTypeActionsTimeOut, DataCache.NotificationTypeActionsPriority, notificationTypeActionId);
            return CBO.GetCachedObject<NotificationTypeAction>(cacheItemArgs, GetNotificationTypeActionCallBack);
        }

        public virtual NotificationTypeAction GetNotificationTypeAction(int notificationTypeId, string name)
        {
            Requires.NotNullOrEmpty("name", name);

            var notificationTypeActionCacheKey = string.Format(DataCache.NotificationTypeActionsByNameCacheKey, notificationTypeId, name);
            var cacheItemArgs = new CacheItemArgs(notificationTypeActionCacheKey, DataCache.NotificationTypeActionsTimeOut, DataCache.NotificationTypeActionsPriority, notificationTypeId, name);
            return CBO.GetCachedObject<NotificationTypeAction>(cacheItemArgs, GetNotificationTypeActionByNameCallBack);
        }

        public virtual IList<NotificationTypeAction> GetNotificationTypeActions(int notificationTypeId)
        {
            return CBO.FillCollection<NotificationTypeAction>(_dataService.GetNotificationTypeActions(notificationTypeId));
        }

        #endregion

		#region Toast APIS

		public bool IsToastPending(int notificationId)
		{
			return _dataService.IsToastPending(notificationId);
		}

		public void MarkReadyForToast(Notification notification, UserInfo userInfo)
		{
			MarkReadyForToast(notification, userInfo.UserID);
		}

        public void MarkReadyForToast(Notification notification, int userId)
        {
            _dataService.MarkReadyForToast(notification.NotificationID, userId);
        }

		public void MarkToastSent(int notificationId, int userId)
		{
			_dataService.MarkToastSent(notificationId, userId);
		}

		public IList<Notification> GetToasts(UserInfo userInfo)
		{
			var toasts = CBO.FillCollection<Notification>(_dataService.GetToasts(userInfo.UserID, userInfo.PortalID));

			foreach (var message in toasts)
			{
				_dataService.MarkToastSent(message.NotificationID, userInfo.UserID);
			}

			return toasts;
		}

		#endregion

        #region Internal Methods

        internal virtual UserInfo GetAdminUser()
        {
            return UserController.GetUserById(PortalSettings.Current.PortalId, PortalSettings.Current.AdministratorId);
        }

        internal virtual int GetCurrentUserId()
        {
            return UserController.GetCurrentUserInfo().UserID;
        }

        internal virtual DateTime GetExpirationDate(int notificationTypeId)
        {
            var notificationType = GetNotificationType(notificationTypeId);

            return notificationType.TimeToLive.TotalMinutes > 0
                ? DateTime.UtcNow.AddMinutes(notificationType.TimeToLive.TotalMinutes)
                : DateTime.MinValue;
        }

        internal virtual object GetNotificationTypeActionCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationTypeActionId = (int)cacheItemArgs.ParamList[0];
            return CBO.FillObject<NotificationTypeAction>(_dataService.GetNotificationTypeAction(notificationTypeActionId));
        }

        internal virtual object GetNotificationTypeActionByNameCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationTypeId = (int)cacheItemArgs.ParamList[0];
            var name = cacheItemArgs.ParamList[1].ToString();
            return CBO.FillObject<NotificationTypeAction>(_dataService.GetNotificationTypeActionByName(notificationTypeId, name));
        }

        internal virtual object GetNotificationTypeByNameCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationName = cacheItemArgs.ParamList[0].ToString();
            return CBO.FillObject<NotificationType>(_dataService.GetNotificationTypeByName(notificationName));
        }

        internal virtual object GetNotificationTypeCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationTypeId = (int)cacheItemArgs.ParamList[0];
            return CBO.FillObject<NotificationType>(_dataService.GetNotificationType(notificationTypeId));
        }

        internal virtual string GetPortalSetting(string settingName, int portalId, string defaultValue)
        {
            return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
        }

        internal virtual string InputFilter(string input)
        {
            var ps = new PortalSecurity();
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

        #endregion
    }
}

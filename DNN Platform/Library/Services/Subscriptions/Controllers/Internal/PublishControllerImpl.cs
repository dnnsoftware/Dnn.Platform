#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Subscriptions.Components.Common;
using DotNetNuke.Subscriptions.Components.Entities;
using DotNetNuke.Subscriptions.Providers.Data;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    public class PublishControllerImpl : IPublishController
    {
        #region Private Variables

        private readonly IDataService _dataService;

        #endregion

        #region Constructors

        public PublishControllerImpl()
            : this(DataService.Instance)
        {
        }

        public PublishControllerImpl(IDataService dataService)
        {
            Requires.NotNull("dataService", dataService);

            _dataService = dataService;
        }

        #endregion

        #region Implementation of IPublishController

        public void Publish(InstantNotification instantNotification)
        {
            var message = TemplateController.Instance.Format(instantNotification);

            Send(message);
        }

        public void Publish(DigestNotification digestNotification)
        {
            var message = TemplateController.Instance.Format(digestNotification);

            Send(message);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Turn a collection of Subscriber objects into a collection of UserInfo objects suitable for Core Messaging.
        /// </summary>
        private static IList<UserInfo> GetTargetList(IEnumerable<Subscriber> subscriber)
        {
            return subscriber.Select(GetUser).ToList();
        }

        private void Send(FormattedNotification message)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType("DigestSubscription");
            if (notificationType == null)
            {
                throw new SubscriptionsException("Cannot find Notification Type 'DigestSubscription'");
            }
            
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = message.Subject,
                Body = message.Body,
                IncludeDismissAction = true,
                SenderUserID = message.Author != null ? message.Author.UserID : GetAdminUser(),
                Context = null
            };

            // Group the user list into the owner portals so that we are calling one SendNotification per portal.
            var portalGroups = message.Subscribers.GroupBy(x => x.PortalId).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var group in portalGroups)
            {
                NotificationsController.Instance.SendNotification(
                    notification,
                    group.Key,
                    new List<RoleInfo>(),
                    GetTargetList(group.Value));
            }
        }

        private static UserInfo GetUser(Subscriber subscriber)
        {
            var user = UserController.GetUserById(subscriber.PortalId, subscriber.UserId) ??
                       UserController.GetUserById(Null.NullInteger, subscriber.UserId);

            if (user == null)
            {
                throw new SubscriptionsException(
                    string.Format("Cannot find user {0} on portal ID {1}", subscriber.UserId, subscriber.PortalId));
            }

            return user;
        }

        private static int GetAdminUser()
        {
            var userInfo = UserController.GetUsers(false, true, Null.NullInteger).Cast<UserInfo>().FirstOrDefault();
            if (userInfo != null)
            {
                return userInfo.UserID;
            }

            return Null.NullInteger;
        }

        #endregion
    }
}
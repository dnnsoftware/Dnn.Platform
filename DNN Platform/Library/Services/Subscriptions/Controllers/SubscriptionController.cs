#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Services.Subscriptions.Common;
using DotNetNuke.Services.Subscriptions.Data;
using DotNetNuke.Services.Subscriptions.Entities;

namespace DotNetNuke.Services.Subscriptions.Controllers
{
    public class SubscriptionController 
                            : ServiceLocator<ISubscriptionController, SubscriptionController>
                            , ISubscriptionController
    {
        protected override Func<ISubscriptionController> GetFactory()
        {
            return () => new SubscriptionController();
        }

        #region Private members

        private readonly IDataService _dataService;

        #endregion

        #region Constructors

        public SubscriptionController() : this(new DataService())
        {}

        public SubscriptionController(IDataService dataService)
        {
            _dataService = dataService;

            if (_dataService == null)
            {
                throw new ArgumentNullException("dataService");
            }
        }

        #endregion

        #region Implementation of ISubscriptionController

        public int Subscribe(Subscriber subscription)
        {
            try
            {
                var subscriptionId = _dataService.AddSubscription(
                    subscription.SubscriberId,
                    subscription.UserId,
                    subscription.PortalId,
                    subscription.SubscriptionTypeId,
                    (int)subscription.Frequency,
                    subscription.ContentItemId,
                    subscription.ObjectKey,
                    subscription.ModuleId,
                    subscription.GroupId);

                if (subscriptionId < 0)
                {
                    throw new SubscriptionsException("Unknown error");
                }

                return subscriptionId;
            }
            catch (Exception ex)
            {
                throw new SubscriptionsException(
                    string.Format("Unable to add or update Subscription: {0}", ex.Message), ex);
            }
        }

        public void Unsubscribe(SubscriptionDescription subscription)
        {
            try
            {
                var identity = CBO.FillObject<Subscriber>(_dataService.IsSubscribed(
                    subscription.PortalId,
                    subscription.UserId,
                    subscription.SubscriptionTypeId,
                    subscription.ContentItemId,
                    subscription.ObjectKey,
                    subscription.ModuleId,
                    subscription.GroupId));

                if (identity.SubscriberId >= 0)
                {
                    _dataService.Unsubscribe(identity.SubscriberId);
                }
            }
            catch (Exception ex)
            {
                throw new SubscriptionsException(string.Format("Unable to unsubscribe: {0}", ex.Message), ex);
            }
        }

        public void DeleteSubscription(int subscriptionId)
        {
            try
            {
                _dataService.Unsubscribe(subscriptionId);
            }
            catch (Exception ex)
            {
                throw new SubscriptionsException(string.Format("Unable to unsubscribe: {0}", ex.Message), ex);
            }
        }

        public Subscriber IsSubscribedToContentActivity(UserInfo userInfo, ContentItem contentItem, int subTypeId, string objectKey, int groupId)
        {
            try
            {
                if (contentItem == null || userInfo == null || subTypeId < 1)
                {
                    return null;
                }

                return CBO.FillObject<Subscriber>(_dataService.IsSubscribed(
                    userInfo.PortalID,
                    userInfo.UserID,
                    subTypeId,
                    contentItem.ContentItemId,
                    objectKey,
                    contentItem.ModuleID,
                    groupId));
            }
            catch (Exception ex)
            {
                throw new SubscriptionsException(
                    string.Format("Unable to determine whether user is subscribed to content activity: {0}", userInfo.UserID), ex);
            }
        }

        public Subscriber IsSubscribedToNewContent(UserInfo userInfo, int subTypeId, string objectKey, int moduleId, int groupId)
        {
            try
            {
                if (userInfo == null || subTypeId < 1)
                {
                    return null;
                }

                return CBO.FillObject<Subscriber>(_dataService.IsSubscribed(
                    userInfo.PortalID,
                    userInfo.UserID,
                    subTypeId,
                    Null.NullInteger,
                    objectKey,
                    moduleId,
                    groupId));
            }
            catch (Exception ex)
            {
                throw new SubscriptionsException(
                    string.Format("Unable to determine whether user is subscribed: {0}", userInfo.UserID), ex);
            }
        }

        public List<Subscriber> GetUserSubscriptions(int userId, int portalId)
        {
            return CBO.FillCollection<Subscriber>(_dataService.GetUserSubscriptions(userId, portalId));
        }

        public void UpdateScheduleItemSetting(int scheduleId, string key, string value)
        {
            _dataService.UpdateScheduleItemSetting(scheduleId, key, value);
        }

        public IList<MessageRecipient> GetNextMessagesForDispatch(Guid schedulerInstance, int batchSize)
        {
            return CBO.FillCollection<MessageRecipient>(_dataService.GetNextMessagesForDispatch(schedulerInstance, batchSize));
        }

        public IList<MessageRecipient> GetNextSubscribersForDispatch(Guid schedulerInstance, int frequency, int batchSize)
        {
            return CBO.FillCollection<MessageRecipient>(_dataService.GetNextSubscribersForDispatch(schedulerInstance, frequency, batchSize));
        }

        public List<Subscriber> GetContentItemSubscribers(int contentItemId, int portalId)
        {
            return CBO.FillCollection<Subscriber>(_dataService.GetContentItemSubscribers(contentItemId, portalId));
        }

        public List<Subscriber> GetNewContentSubscribers(int groupId, int moduleId, int portalId)
        {
            return CBO.FillCollection<Subscriber>(_dataService.GetNewContentSubscribers(groupId, moduleId, portalId));
        }

        #endregion
    }
}
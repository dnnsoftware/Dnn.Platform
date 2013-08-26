#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Subscriptions.Components.Entities;

namespace DotNetNuke.Subscriptions.Components.Controllers
{
    public interface ISubscriptionController
    {
        /// <summary>
        /// Create or update a subscription for a user to a particular set of data.
        /// </summary>
        int Subscribe(Subscriber subscription);

        /// <summary>
        /// Remove an existing Subscription.
        /// </summary>
        /// <param name="subscription">An entity describing the subscription--type, content, etc.</param>
        void Unsubscribe(SubscriptionDescription subscription);

        void DeleteSubscription(int subscriptionId);

        /// <summary>
        ///  Determines if the passed in user is subscribed to a specific content item.
        /// </summary>
        /// <param name="userSubscription"></param>
        /// <param name="contentItem"></param>
        /// <returns>SubscriberId PK</returns>
        Subscriber IsSubscribedToContentActivity(UserInfo userInfo, ContentItem contentItem, int subTypeId, string objectKey, int groupId);

        /// <summary>
        /// Determines whether a user is subscribed to the specified type (not content item specific).
        /// </summary>
        /// <param name="subscriptionType"></param>
        /// <returns>SubscriberId PK</returns>
        Subscriber IsSubscribedToNewContent(UserInfo userInfo, int subTypeId, string objectKey, int moduleId, int groupId);

        List<Subscriber> GetUserSubscriptions(int userId, int portalId);

        void UpdateScheduleItemSetting(int scheduleId, string key, string value);

        IList<MessageRecipient> GetNextMessagesForDispatch(Guid schedulerInstance, int batchSize);

        IList<MessageRecipient> GetNextSubscribersForDispatch(Guid schedulerInstance, int frequency, int batchSize);

        List<Subscriber> GetContentItemSubscribers(int contentItemId, int portalId);

        List<Subscriber> GetNewContentSubscribers(int groupId, int moduleId, int portalId);

        /// <summary>
        /// Determine whether a user is subscribed to the specified type and content item.
        /// </summary>
        [Obsolete("As of Social 1.1, please use IsSubscribedToContentActivity instead (which returns a subscriberId instead of a true/false value).")]
        bool IsSubscribed(UserInfo userInfo, SubscriptionType subscriptionType, ContentItem contentItem, string objectKey);
    }
}
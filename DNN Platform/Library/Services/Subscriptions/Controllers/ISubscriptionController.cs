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
using DotNetNuke.Services.Subscriptions.Entities;

namespace DotNetNuke.Services.Subscriptions.Controllers
{
    public interface ISubscriptionController
    {
        void DeleteSubscription(int subscriptionId);

        List<Subscriber> GetContentItemSubscribers(int contentItemId, int portalId);

        List<Subscriber> GetNewContentSubscribers(int groupId, int moduleId, int portalId);

        List<Subscriber> GetUserSubscriptions(int userId, int portalId);

        /// <summary>
        ///  Determines if the passed in user is subscribed to a specific content item.
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="contentItem"></param>
        /// <returns>SubscriberId PK</returns>
        Subscriber IsSubscribedToContentActivity(UserInfo userInfo, ContentItem contentItem, int subTypeId, string objectKey, int groupId);

        /// <summary>
        /// Determines whether a user is subscribed to the specified type (not content item specific).
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns>SubscriberId PK</returns>
        Subscriber IsSubscribedToNewContent(UserInfo userInfo, int subTypeId, string objectKey, int moduleId, int groupId);

        /// <summary>
        /// Create or update a subscription for a user to a particular set of data.
        /// </summary>
        int Subscribe(Subscriber subscription);

        /// <summary>
        /// Remove an existing Subscription.
        /// </summary>
        /// <param name="subscription">An entity describing the subscription--type, content, etc.</param>
        void Unsubscribe(Subscriber subscription);
    }
}
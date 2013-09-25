#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Data;
using DotNetNuke.Services.Subscriptions.Entities;

namespace DotNetNuke.Services.Subscriptions.Data
{
    public interface IDataService
    {
        #region Subscription Types

        /// <summary>
        /// Adds a subscription type to the data store and returns its identity
        /// </summary>
        /// <returns>The subscriptionTypeId of the newly created row</returns>
        int AddSubscriptionType(string subscriptionName, string friendlyName, int desktopModuleId);

        /// <summary>
        /// Retrieves all subscription types from the data store
        /// </summary>
        /// <returns>A collection of subscription types</returns>
        IDataReader GetAllSubscriptionTypes();

        /// <summary>
        /// Remove a subscription type by ID if there are no dependencies on it (unlikely)
        /// </summary>
        /// <remarks>Intended for debugging and testing</remarks>
        bool RemoveSubscriptionType(int subscriptionTypeId);

        #endregion

        #region Subscriptions

        /// <summary>
        /// Add or update an existing Subscription.
        /// </summary>
        /// <param name="subscriptionId">SubscriptionId if existing or <c>Null.NullInteger</c></param>
        /// <param name="userId">User ID of the person adding the Subscription</param>
        /// <param name="portalId">The portal ID the subscription is owned by</param>
        /// <param name="subscriptionTypeId">Subscription Type being subscribed to</param>
        /// <param name="frequency">Frequency of notifications</param>
        /// <param name="contentItemId">Unique content item ID or <c>Null.NullInteger</c></param>
        /// <param name="objectKey">Object Key of specific data being subscribed to, or <c>null</c></param>
        /// <param name="moduleId">Module ID associated with the subscription or <c>Null.NullInteger</c></param>
        /// <param name="groupId">Group ID (Role ID, really) or <c>Null.NullInteger</c></param>
        /// <returns>Integer represending the SubscriptionId (PK).</returns>
        int AddSubscription(int subscriptionId, int userId, int portalId, int subscriptionTypeId, int frequency,
                            int contentItemId, string objectKey, int moduleId, int groupId);

        /// <summary>
        /// Get the properties for the specified subscription, <paramref name="subscriptionId"/>.
        /// </summary>
        IDataReader GetSubscription(int subscriptionId);

        /// <summary>
        /// Determine whether a user has a specific Subscription.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="userId"></param>
        /// <param name="subscriptionTypeId"></param>
        /// <param name="contentItemId"></param>
        /// <param name="objectKey"></param>
        /// <param name="moduleId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        IDataReader IsSubscribed(int portalId, int userId, int subscriptionTypeId, int contentItemId, string objectKey, int moduleId, int groupId);

        /// <summary>
        /// Remove an existing subscription by ID
        /// </summary>
        /// <param name="subscriptionId">Subscription ID</param>
        bool Unsubscribe(int subscriptionId);

        /// <summary>
        /// Get a list of all Subscriptions for the specified portal ID (internal)
        /// </summary>
        IDataReader GetAllSubscribers(int portalId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="portalId"></param>
        /// <returns></returns>
        IDataReader GetUserSubscriptions(int userId, int portalId);

        IDataReader GetContentItemSubscribers(int contentItemId, int portalId);

        IDataReader GetNewContentSubscribers(int groupId, int moduleId, int portalId);

        #endregion

        #region Task

        #endregion
    }
}
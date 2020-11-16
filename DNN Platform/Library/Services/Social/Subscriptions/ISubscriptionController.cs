// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Social.Subscriptions.Entities;

    /// <summary>
    /// This controller is responsible to manage the user subscriptions.
    /// </summary>
    public interface ISubscriptionController
    {
        /// <summary>
        /// Returns the User Subscriptions.
        /// </summary>
        /// <param name="user">User Info.</param>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="subscriptionTypeId">Subscription Type Id.</param>
        /// <returns>Collection of subscriptions.</returns>
        IEnumerable<Subscription> GetUserSubscriptions(UserInfo user, int portalId, int subscriptionTypeId = -1);

        /// <summary>
        /// Returns the Content Subscriptions.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="subscriptionTypeId">Subscription Type Id.</param>
        /// <param name="objectKey">Object Key.</param>
        /// <returns>Collection of subscriptions.</returns>
        IEnumerable<Subscription> GetContentSubscriptions(int portalId, int subscriptionTypeId, string objectKey);

        /// <summary>
        /// Returns true if a user is subscribed to a Content.
        /// </summary>
        /// <param name="subscription">Subscription.</param>
        /// <returns>True if the user is subscribed to the content, false otherwise.</returns>
        bool IsSubscribed(Subscription subscription);

        /// <summary>
        /// Adds a new Subscription.
        /// If the operation succeed the SubscriptionId property of the Subscription entity will be filled up.
        /// </summary>
        /// <param name="subscription">Subscription.</param>
        void AddSubscription(Subscription subscription);

        /// <summary>
        ///  Deletes a Subscription.
        /// </summary>
        /// <param name="subscription">Subscription.</param>
        void DeleteSubscription(Subscription subscription);

        /// <summary>
        ///  Updates a Subscription Description.
        /// </summary>
        /// <param name="objectKey">Subscription Object Key.</param>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="newDescription">New Subscription Description.</param>
        /// <returns></returns>
        int UpdateSubscriptionDescription(string objectKey, int portalId, string newDescription);

        /// <summary>
        /// Deletes all subscriptions matching the specified object key.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="objectKey">Object Key.</param>
        void DeleteSubscriptionsByObjectKey(int portalId, string objectKey);
    }
}

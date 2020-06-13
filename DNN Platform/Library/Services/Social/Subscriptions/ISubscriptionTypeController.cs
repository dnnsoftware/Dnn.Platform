// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Social.Subscriptions.Entities;

    /// <summary>
    /// This controller is responsible to manage the subscription types.
    /// </summary>
    public interface ISubscriptionTypeController
    {
        /// <summary>
        /// Creates a new Subscription Type.
        /// If the operation succeed the SubscriptionTypeId property of the Subscription entity will be filled up.
        /// </summary>
        /// <param name="subscriptionType">Subscription Type.</param>
        void AddSubscriptionType(SubscriptionType subscriptionType);

        /// <summary>
        /// Retrieves a Single Subscription Type that match the where predicate.
        /// </summary>
        /// <param name="predicate">Where Predicate.</param>
        /// <returns>Subscription Type.</returns>
        SubscriptionType GetSubscriptionType(Func<SubscriptionType, bool> predicate);

        /// <summary>
        /// Retrieves all the Subscription Type.
        /// </summary>
        /// <returns>Subscription Types collection.</returns>
        IEnumerable<SubscriptionType> GetSubscriptionTypes();

        /// <summary>
        /// Retrieves a set of Subscription Types specifying a where predicate.
        /// </summary>
        /// <param name="predicate">Where predicate.</param>
        /// <returns>Subscription Type collection.</returns>
        IEnumerable<SubscriptionType> GetSubscriptionTypes(Func<SubscriptionType, bool> predicate);

        /// <summary>
        /// Deletes a Subscription Type from the system.
        /// </summary>
        /// <param name="subscriptionType">Subscription Type.</param>
        void DeleteSubscriptionType(SubscriptionType subscriptionType);
    }
}

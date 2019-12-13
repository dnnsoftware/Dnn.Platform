// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using DotNetNuke.Services.Social.Subscriptions.Entities;

namespace DotNetNuke.Services.Social.Subscriptions
{
    /// <summary>
    /// This controller is responsible to manage the subscription types.
    /// </summary>
    public interface ISubscriptionTypeController
    {
        /// <summary>
        /// Creates a new Subscription Type.
        /// If the operation succeed the SubscriptionTypeId property of the Subscription entity will be filled up.
        /// </summary>
        /// <param name="subscriptionType">Subscription Type</param>
        void AddSubscriptionType(SubscriptionType subscriptionType);

        /// <summary>
        /// Retrieves a Single Subscription Type that match the where predicate.
        /// </summary>
        /// <param name="predicate">Where Predicate</param>
        /// <returns>Subscription Type</returns>
        SubscriptionType GetSubscriptionType(Func<SubscriptionType, bool> predicate);

        /// <summary>
        /// Retrieves all the Subscription Type.
        /// </summary>
        /// <returns>Subscription Types collection</returns>
        IEnumerable<SubscriptionType> GetSubscriptionTypes();

        /// <summary>
        /// Retrieves a set of Subscription Types specifying a where predicate.
        /// </summary>
        /// <param name="predicate">Where predicate</param>
        /// <returns>Subscription Type collection</returns>
        IEnumerable<SubscriptionType> GetSubscriptionTypes(Func<SubscriptionType, bool> predicate);
        
        /// <summary>
        /// Deletes a Subscription Type from the system.
        /// </summary>
        /// <param name="subscriptionType">Subscription Type</param>
        void DeleteSubscriptionType(SubscriptionType subscriptionType);
    }
}

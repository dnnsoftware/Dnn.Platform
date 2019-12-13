﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Social.Subscriptions.Data;
using DotNetNuke.Services.Social.Subscriptions.Entities;

namespace DotNetNuke.Services.Social.Subscriptions
{
    /// <summary>
    /// This controller is responsible to manage the user subscriptions.
    /// </summary>
    public class SubscriptionController : ServiceLocator<ISubscriptionController, SubscriptionController>, ISubscriptionController
    {
        private readonly IDataService dataService;
        private readonly ISubscriptionSecurityController subscriptionSecurityController;

        public SubscriptionController()
        {
            dataService = DataService.Instance;
            subscriptionSecurityController = SubscriptionSecurityController.Instance;
        }

        protected override Func<ISubscriptionController> GetFactory()
        {
            return () => new SubscriptionController();
        }

        #region Implemented Methods
        public IEnumerable<Subscription> GetUserSubscriptions(UserInfo user, int portalId, int subscriptionTypeId = -1)
        {
            var subscriptions = CBO.FillCollection<Subscription>(dataService.GetSubscriptionsByUser(
                portalId,
                user.UserID,
                subscriptionTypeId));

            return subscriptions.Where(s => subscriptionSecurityController.HasPermission(s));
        }

        public IEnumerable<Subscription> GetContentSubscriptions(int portalId, int subscriptionTypeId, string objectKey)
        {
            var subscriptions = CBO.FillCollection<Subscription>(dataService.GetSubscriptionsByContent(
                portalId,
                subscriptionTypeId,
                objectKey));

            return subscriptions.Where(s => subscriptionSecurityController.HasPermission(s));
        }

        public bool IsSubscribed(Subscription subscription)
        {
            var fetchedSubscription = CBO.FillObject<Subscription>(dataService.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId));

            return fetchedSubscription != null && subscriptionSecurityController.HasPermission(fetchedSubscription);
        }

        public void AddSubscription(Subscription subscription)
        {
            Requires.NotNull("subscription", subscription);
            Requires.NotNegative("subscription.UserId", subscription.UserId);
            Requires.NotNegative("subscription.SubscriptionTypeId", subscription.SubscriptionTypeId);
            Requires.PropertyNotNull("subscription.ObjectKey", subscription.ObjectKey);

            subscription.SubscriptionId = dataService.AddSubscription(subscription.UserId,
                                               subscription.PortalId,
                                               subscription.SubscriptionTypeId,
                                               subscription.ObjectKey,
                                               subscription.Description,
                                               subscription.ModuleId,
                                               subscription.TabId,
                                               subscription.ObjectData);
        }

        public void DeleteSubscription(Subscription subscription)
        {
            Requires.NotNull("subscription", subscription);

            var subscriptionToDelete = CBO.FillObject<Subscription>(dataService.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId));

            if (subscriptionToDelete == null) return;

            dataService.DeleteSubscription(subscriptionToDelete.SubscriptionId);
        }

        public int UpdateSubscriptionDescription(string objectKey, int portalId, string newDescription)
        {
            Requires.PropertyNotNull("objectKey", objectKey);
            Requires.NotNull("portalId", portalId);
            Requires.PropertyNotNull("newDescription", newDescription);
            return dataService.UpdateSubscriptionDescription(objectKey, portalId, newDescription);
        }

        public void DeleteSubscriptionsByObjectKey(int portalId, string objectKey)
        {
            dataService.DeleteSubscriptionsByObjectKey(portalId, objectKey);
        }

        #endregion
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Subscriptions;

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Social.Subscriptions.Data;
using DotNetNuke.Services.Social.Subscriptions.Entities;

/// <summary>This controller is responsible to manage the user subscriptions.</summary>
public class SubscriptionController : ServiceLocator<ISubscriptionController, SubscriptionController>, ISubscriptionController
{
    private readonly IDataService dataService;
    private readonly ISubscriptionSecurityController subscriptionSecurityController;

    /// <summary>Initializes a new instance of the <see cref="SubscriptionController"/> class.</summary>
    public SubscriptionController()
    {
        this.dataService = DataService.Instance;
        this.subscriptionSecurityController = SubscriptionSecurityController.Instance;
    }

    /// <inheritdoc/>
    public IEnumerable<Subscription> GetUserSubscriptions(UserInfo user, int portalId, int subscriptionTypeId = -1)
    {
        var subscriptions = CBO.FillCollection<Subscription>(this.dataService.GetSubscriptionsByUser(
            portalId,
            user.UserID,
            subscriptionTypeId));

        return subscriptions.Where(s => this.subscriptionSecurityController.HasPermission(s));
    }

    /// <inheritdoc/>
    public IEnumerable<Subscription> GetContentSubscriptions(int portalId, int subscriptionTypeId, string objectKey)
    {
        var subscriptions = CBO.FillCollection<Subscription>(this.dataService.GetSubscriptionsByContent(
            portalId,
            subscriptionTypeId,
            objectKey));

        return subscriptions.Where(s => this.subscriptionSecurityController.HasPermission(s));
    }

    /// <inheritdoc/>
    public bool IsSubscribed(Subscription subscription)
    {
        var fetchedSubscription = CBO.FillObject<Subscription>(this.dataService.IsSubscribed(
            subscription.PortalId,
            subscription.UserId,
            subscription.SubscriptionTypeId,
            subscription.ObjectKey,
            subscription.ModuleId,
            subscription.TabId));

        return fetchedSubscription != null && this.subscriptionSecurityController.HasPermission(fetchedSubscription);
    }

    /// <inheritdoc/>
    public void AddSubscription(Subscription subscription)
    {
        Requires.NotNull("subscription", subscription);
        Requires.NotNegative("subscription.UserId", subscription.UserId);
        Requires.NotNegative("subscription.SubscriptionTypeId", subscription.SubscriptionTypeId);
        Requires.PropertyNotNull("subscription.ObjectKey", subscription.ObjectKey);

        subscription.SubscriptionId = this.dataService.AddSubscription(
            subscription.UserId,
            subscription.PortalId,
            subscription.SubscriptionTypeId,
            subscription.ObjectKey,
            subscription.Description,
            subscription.ModuleId,
            subscription.TabId,
            subscription.ObjectData);
    }

    /// <inheritdoc/>
    public void DeleteSubscription(Subscription subscription)
    {
        Requires.NotNull("subscription", subscription);

        var subscriptionToDelete = CBO.FillObject<Subscription>(this.dataService.IsSubscribed(
            subscription.PortalId,
            subscription.UserId,
            subscription.SubscriptionTypeId,
            subscription.ObjectKey,
            subscription.ModuleId,
            subscription.TabId));

        if (subscriptionToDelete == null)
        {
            return;
        }

        this.dataService.DeleteSubscription(subscriptionToDelete.SubscriptionId);
    }

    /// <inheritdoc/>
    public int UpdateSubscriptionDescription(string objectKey, int portalId, string newDescription)
    {
        Requires.PropertyNotNull("objectKey", objectKey);
        Requires.NotNull("portalId", portalId);
        Requires.PropertyNotNull("newDescription", newDescription);
        return this.dataService.UpdateSubscriptionDescription(objectKey, portalId, newDescription);
    }

    /// <inheritdoc/>
    public void DeleteSubscriptionsByObjectKey(int portalId, string objectKey)
    {
        this.dataService.DeleteSubscriptionsByObjectKey(portalId, objectKey);
    }

    /// <inheritdoc/>
    protected override Func<ISubscriptionController> GetFactory()
    {
        return () => new SubscriptionController();
    }
}

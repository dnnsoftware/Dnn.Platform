// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Social.Subscriptions.Entities;
    using DotNetNuke.Tests.Utilities;

    public class SubscriptionBuilder
    {
        private int subscriptionId;
        private int userId;
        private int portalId;
        private int subscriptionTypeId;
        private string objectKey;
        private string description;
        private int moduleId;
        private int tabId;
        private string objectData;

        internal SubscriptionBuilder()
        {
            this.subscriptionId = 1;
            this.userId = Constants.USER_InValidId;
            this.subscriptionTypeId = 1;
            this.portalId = Constants.PORTAL_ValidPortalId;
            this.moduleId = Null.NullInteger;
            this.tabId = Null.NullInteger;
            this.objectKey = "content";
            this.description = "my content description";
            this.objectData = string.Empty;
        }

        internal SubscriptionBuilder WithSubscriptionId(int subscriptionId)
        {
            this.subscriptionId = subscriptionId;
            return this;
        }

        internal SubscriptionBuilder WithUserId(int userId)
        {
            this.userId = userId;
            return this;
        }

        internal SubscriptionBuilder WithPortalId(int portalId)
        {
            this.portalId = portalId;
            return this;
        }

        internal SubscriptionBuilder WithSubscriptionTypeId(int subscriptionTypeId)
        {
            this.subscriptionTypeId = subscriptionTypeId;
            return this;
        }

        internal SubscriptionBuilder WithObjectKey(string objectKey)
        {
            this.objectKey = objectKey;
            return this;
        }

        internal SubscriptionBuilder WithDescription(string description)
        {
            this.description = description;
            return this;
        }

        internal SubscriptionBuilder WithModuleId(int moduleId)
        {
            this.moduleId = moduleId;
            return this;
        }

        internal SubscriptionBuilder WithTabId(int tabId)
        {
            this.tabId = tabId;
            return this;
        }

        internal Subscription Build()
        {
            return new Subscription
            {
                SubscriptionTypeId = this.subscriptionTypeId,
                SubscriptionId = this.subscriptionId,
                CreatedOnDate = DateTime.UtcNow,
                ModuleId = this.moduleId,
                ObjectKey = this.objectKey,
                Description = this.description,
                PortalId = this.portalId,
                TabId = this.tabId,
                UserId = this.userId,
                ObjectData = this.objectData,
            };
        }
    }
}

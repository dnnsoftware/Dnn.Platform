// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Social.Subscriptions.Entities;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
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
            subscriptionId = 1;
            userId = Constants.USER_InValidId;
            subscriptionTypeId = 1;
            portalId = Constants.PORTAL_ValidPortalId;
            moduleId = Null.NullInteger;
            tabId = Null.NullInteger;
            objectKey = "content";
            description = "my content description";
            objectData = "";
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
                           SubscriptionTypeId = subscriptionTypeId,
                           SubscriptionId = subscriptionId,
                           CreatedOnDate = DateTime.UtcNow,
                           ModuleId = moduleId,
                           ObjectKey = objectKey,
                           Description = description,
                           PortalId = portalId,
                           TabId = tabId,
                           UserId = userId,
                           ObjectData = objectData
                       };

        }
    }
}

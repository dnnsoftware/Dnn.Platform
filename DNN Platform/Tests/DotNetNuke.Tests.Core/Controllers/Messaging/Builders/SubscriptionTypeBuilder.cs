// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Social.Subscriptions.Entities;

    internal class SubscriptionTypeBuilder
    {
        private int subscriptionTypeId;
        private string subscriptionName;
        private string friendlyName;
        private int desktopModuleId;

        internal SubscriptionTypeBuilder()
        {
            this.subscriptionTypeId = 1;
            this.subscriptionName = "MySubscriptionType";
            this.friendlyName = "My Subscription Type";
            this.desktopModuleId = Null.NullInteger;
        }

        internal SubscriptionTypeBuilder WithSubscriptionTypeId(int subscriptionTypeId)
        {
            this.subscriptionTypeId = subscriptionTypeId;
            return this;
        }

        internal SubscriptionTypeBuilder WithSubscriptionName(string subscriptionName)
        {
            this.subscriptionName = subscriptionName;
            return this;
        }

        internal SubscriptionType Build()
        {
            return new SubscriptionType
            {
                SubscriptionTypeId = this.subscriptionTypeId,
                SubscriptionName = this.subscriptionName,
                DesktopModuleId = this.desktopModuleId,
                FriendlyName = this.friendlyName,
            };
        }
    }
}

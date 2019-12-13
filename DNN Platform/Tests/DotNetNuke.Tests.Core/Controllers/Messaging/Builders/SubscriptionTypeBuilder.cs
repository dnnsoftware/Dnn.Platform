using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Social.Subscriptions.Entities;

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    class SubscriptionTypeBuilder
    {
        private int subscriptionTypeId;
        private string subscriptionName;
        private string friendlyName;
        private int desktopModuleId;

        internal SubscriptionTypeBuilder()
        {
            subscriptionTypeId = 1;
            subscriptionName = "MySubscriptionType";
            friendlyName = "My Subscription Type";
            desktopModuleId = Null.NullInteger;
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
                           SubscriptionTypeId = subscriptionTypeId,
                           SubscriptionName = subscriptionName,
                           DesktopModuleId = desktopModuleId,
                           FriendlyName = friendlyName
                       };
        }
    }
}

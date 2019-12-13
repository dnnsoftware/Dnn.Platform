using System;

namespace DotNetNuke.Services.Social.Subscriptions.Entities
{
    /// <summary>
    /// This class represents a Subscription Type.
    /// </summary>
    [Serializable]
    public class SubscriptionType
    {
        /// <summary>
        /// The Subscription Type identifier.
        /// </summary>
        public int SubscriptionTypeId { get; set; }

        /// <summary>
        /// The Subscription Name.
        /// </summary>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// The Subscription Friendly Name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// The Desktop Module Id associated with the subscription type. 
        /// This is an optional field but it should be set if the Subscription Type belongs to a specific module.
        /// </summary>
        public int DesktopModuleId { get; set; }
    }
}

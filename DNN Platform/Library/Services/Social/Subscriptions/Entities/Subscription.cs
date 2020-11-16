// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions.Entities
{
    using System;

    /// <summary>
    /// This class represents a Subscription instance.
    /// </summary>
    [Serializable]
    public class Subscription
    {
        /// <summary>
        /// Gets or sets the subscription identifier.
        /// </summary>
        public int SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the user the subscription is associated with.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the site the subscription is associated with.
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets the type of subscription.
        /// </summary>
        public int SubscriptionTypeId { get; set; }

        /// <summary>
        /// Gets or sets object key that represent the content which user is subscribed to.
        /// The format of the ObjectKey is up to the consumer. (i.e.: blog:12, where 12 represents the post identifier).
        /// </summary>
        public string ObjectKey { get; set; }

        /// <summary>
        /// Gets or sets object Data that represents metadata to manage the subscription.
        /// The format of the ObjectData is up to the consumer. (i.e.: destinationModule:486, where 486 represents a extra property called Destination Module).
        /// </summary>
        public string ObjectData { get; set; }

        /// <summary>
        /// Gets or sets description of the content which user is subscribed to.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date the user subscribed.
        /// </summary>
        public DateTime CreatedOnDate { get; set; }

        /// <summary>
        /// Gets or sets associates the subscription with an instance of a module.
        /// If set it uses to apply to Security Trimming.
        /// If the user does not have view permission on that module the Subscription won't be retrieved by the SubscriptionController.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets associates the subscription with a tab.
        /// If set it uses to apply to Security Trimming.
        /// If the user does not have view permission on that tab the Subscription won't be retrieved by the SubscriptionController.
        /// </summary>
        public int TabId { get; set; }
    }
}

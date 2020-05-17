// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Services.Social.Subscriptions.Entities
{
    /// <summary>
    /// This class represents a Subscription instance.
    /// </summary>
    [Serializable]
    public class Subscription
    {
        /// <summary>
        /// The subscription identifier.
        /// </summary>
        public int SubscriptionId { get; set; }

        /// <summary>
        /// The user the subscription is associated with.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The site the subscription is associated with.
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// The type of subscription.
        /// </summary>
        public int SubscriptionTypeId { get; set; }
        
        /// <summary>
        /// Object key that represent the content which user is subscribed to.
        /// The format of the ObjectKey is up to the consumer. (i.e.: blog:12, where 12 represents the post identifier).
        /// </summary>
        public string ObjectKey { get; set; }

        /// <summary>
        /// Object Data that represents metadata to manage the subscription.
        /// The format of the ObjectData is up to the consumer. (i.e.: destinationModule:486, where 486 represents a extra property called Destination Module).
        /// </summary>
        public string ObjectData { get; set; }

        /// <summary>
        /// Description of the content which user is subscribed to.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date the user subscribed.
        /// </summary>
        public DateTime CreatedOnDate { get; set; }
        
        /// <summary>
        /// Associates the subscription with an instance of a module.
        /// If set it uses to apply to Security Trimming. 
        /// If the user does not have view permission on that module the Subscription won't be retrieved by the SubscriptionController.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Associates the subscription with a tab. 
        /// If set it uses to apply to Security Trimming. 
        /// If the user does not have view permission on that tab the Subscription won't be retrieved by the SubscriptionController.
        /// </summary>
        public int TabId { get; set; }
    }
}

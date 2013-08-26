#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Subscriptions.Components.Entities
{
    /// <summary>The Subscriber entity represents a single subscription.</summary>
    [DataContract]
    public class Subscriber
    {
        #region Public members

        /// <summary>
        /// The unique identifier
        /// </summary>
        [DataMember(Name = "subscriberId")]
        public int SubscriberId { get; set; }

        /// <summary>
        /// The user the subscription is associated with
        /// </summary>
        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        /// <summary>
        /// The site the subscription is associated with
        /// </summary>
        [DataMember(Name = "portalId")]
        public int PortalId { get; set; }

        /// <summary>
        /// The type of subscription.
        /// </summary>
        [DataMember(Name = "subscriptionTypeId")]
        public int SubscriptionTypeId { get; set; }

        /// <summary>
        /// How often the user receives emails for the subscription
        /// </summary>
        [DataMember(Name = "frequency")]
        public Frequency Frequency { get; set; }

        /// <summary>
        /// Content Item ID the subscription is referencing (if any)
        /// </summary>
        [DataMember(Name = "contentItemId")]
        public int ContentItemId { get; set; }

        /// <summary>
        /// Object key providing additional information on the subscription (if any). It is used as a 'filtering' mechanics for a specific subscription type.
        /// </summary>
        [DataMember(Name = "objectKey")]
        public string ObjectKey { get; set; }

        /// <summary>
        /// The date the user subscribed
        /// </summary>
        [DataMember(Name = "createdOnDate")]
        public DateTime CreatedOnDate { get; set; }

        /// <summary>
        /// The last time the user received an email related to this content
        /// </summary>
        [DataMember(Name = "lastSentOnDate")]
        public DateTime LastSentOnDate { get; set; }

        /// <summary>
        /// Associates the subsciptoin with a module (important for items not associated with a specific content item; ie. new content item subs)
        /// </summary>
        [DataMember(Name = "moduleId")]
        public int ModuleId { get; set; }

        /// <summary>
        /// Associates the subscription with a specific group (important for modules that operate in group mode). 
        /// </summary>
        [DataMember(Name = "groupId")]
        public int GroupId { get; set; }

        #endregion
    }
}
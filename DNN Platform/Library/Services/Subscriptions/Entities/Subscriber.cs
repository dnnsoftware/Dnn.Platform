#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Subscriptions.Entities
{
    /// <summary>The Subscriber entity represents a single subscription.</summary>
    public class Subscriber
    {
        #region Public members

        /// <summary>
        /// The unique identifier
        /// </summary>
        public int SubscriberId { get; set; }

        /// <summary>
        /// The user the subscription is associated with
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The site the subscription is associated with
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// The type of subscription.
        /// </summary>
        public int SubscriptionTypeId { get; set; }

        /// <summary>
        /// How often the user receives emails for the subscription
        /// </summary>
        public Frequency Frequency { get; set; }

        /// <summary>
        /// Content Item ID the subscription is referencing (if any)
        /// </summary>
        public int ContentItemId { get; set; }

        /// <summary>
        /// Object key providing additional information on the subscription (if any). It is used as a 'filtering' mechanics for a specific subscription type.
        /// </summary>
        public string ObjectKey { get; set; }

        /// <summary>
        /// The date the user subscribed
        /// </summary>
        public DateTime CreatedOnDate { get; set; }

        /// <summary>
        /// The last time the user received an email related to this content
        /// </summary>
        public DateTime LastSentOnDate { get; set; }

        /// <summary>
        /// Associates the subsciptoin with a module (important for items not associated with a specific content item; ie. new content item subs)
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Associates the subscription with a specific group (important for modules that operate in group mode). 
        /// </summary>
        public int GroupId { get; set; }

        #endregion
    }
}
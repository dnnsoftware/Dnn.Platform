// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

using System.Runtime.Serialization;

namespace DotNetNuke.Modules.CoreMessaging.Components.Subscriptions.Entities
{
    [DataContract]
    public class Subscriber
    {
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
        public int Frequency { get; set; }

        /// <summary>
        /// Content Item ID the subscription is referencing (if any)
        /// </summary>
        [DataMember(Name = "contentItemId")]
        public int ContentItemId { get; set; }

        /// <summary>
        /// Object key providing additional information on the subscription (if any)
        /// </summary>
        [DataMember(Name = "objectKey")]
        public string ObjectKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "friendlyName")]
        public string FriendlyName { get; set; }

        [DataMember(Name = "frequencyText")]
        public string FrequencyText
        {
            get
            {
                switch (Frequency)
                {
					case (int)DotNetNuke.Services.Subscriptions.Entities.Frequency.Hourly:
                        return "Hourly";
					case (int)DotNetNuke.Services.Subscriptions.Entities.Frequency.Daily:
                        return "Daily";
					case (int)DotNetNuke.Services.Subscriptions.Entities.Frequency.Weekly:
                        return "Weekly";
                    default:
                        return "Instant";
                }
            }
        }

        [DataMember(Name = "contentTitle")]
        public string ContentTitle { get; set; }

        [DataMember(Name = "totalRecords")]
        public int TotalRecords { get; set; }

        [DataMember(Name = "activity")]
        public string Activity
        {
            get
            {
                if (ContentItemId > 0)
                {
                    return ContentTitle;
                }
                return FriendlyName;
            }
        }
    }
}
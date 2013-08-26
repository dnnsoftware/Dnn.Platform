#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Runtime.Serialization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;

namespace DotNetNuke.Subscriptions.Components.Entities
{
    /// <summary>
    /// The Queue entity represents an item that is queued up for subscription sends. 
    /// </summary>
    [DataContract]
    public class QueueItem
    {
        #region Public members

        /// <summary>
        /// The unique identifier for the queue entity
        /// </summary>
        [DataMember(Name = "queueId")]
        public int QueueId { get; set; }

        /// <summary>
        /// The site the subscription item is attached to
        /// </summary>
        [DataMember(Name = "portalId")]
        public int PortalId { get; set; }

        /// <summary>
        /// Subscription Type associated with this Queue item
        /// </summary>
       [DataMember(Name = "subscriptionTypeId")]
        public int SubscriptionTypeId { get; set; }

        /// <summary>
        /// The Content Item ID associated with this queue item (if any)
        /// </summary>
        [DataMember(Name = "contentItemId")]
        public int ContentItemId { get; set; }

        /// <summary>
        /// Optional object key describing a unique ID of the item
        /// </summary>
        [DataMember(Name = "objectKey")]
        public string ObjectKey { get; set; }

        /// <summary>
        /// The subject used for instant subscriptions
        /// </summary>
        /// <remarks>400 chars</remarks>
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// The body of the outgoing email for instant subscriptions
        /// </summary>
        [DataMember(Name = "body")]
        public string Body { get; set; }

        /// <summary>
        /// The item summary used for an outgoing email for non-instant subscriptions
        /// </summary>
        /// <remarks>2000 chars</remarks>
        [DataMember(Name = "summary")]
        public string Summary { get; set; }

        /// <summary>
        /// The date an item was added to the queue (UTC)
        /// </summary>
        [DataMember(Name = "createdOnDate")]
        public DateTime CreatedOnDate { get; set; }

        /// <summary>
        /// The title of the item being subscribed to from ContentItem object
        /// </summary>
        [DataMember(Name = "contentTitle")]
        public string ContentTitle
        {
            get
            {
                var contentController = new ContentController() as IContentController;

                var contentItem = contentController.GetContentItem(ContentItemId);

                return contentItem != null ? contentItem.ContentTitle : Null.NullString;
            }
            set { }
        }


        #endregion
    }
}
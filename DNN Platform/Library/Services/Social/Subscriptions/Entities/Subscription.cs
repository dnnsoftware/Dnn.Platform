#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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

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

using System.Collections.Generic;
using DotNetNuke.Services.Social.Subscriptions.Entities;

namespace DotNetNuke.Services.Social.Subscriptions
{
    public interface ISubscriptionController
    {
        /// <summary>
        /// Returns the User Subscriptions
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="portalId">Portal Id</param>
        /// <param name="subscriptionTypeId">Subscription Type Id</param>
        /// <returns>Collection of subscriptions</returns>
        IEnumerable<Subscription> GetUserSubscriptions(int userId, int portalId, int subscriptionTypeId = -1);
        
        /// <summary>
        /// Returns the Content Subscriptions
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="subscriptionTypeId">Subscription Type Id</param>
        /// <param name="objectKey">Object Key</param>
        /// <returns>Collection of subscriptions</returns>
        IEnumerable<Subscription> GetContentSubscriptions(int portalId, int subscriptionTypeId, string objectKey);

        /// <summary>
        /// Returns true if a user is subscribed to a Content
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="portalId">Portal Id</param>
        /// <param name="subscriptionTypeId">Subscription Type Id</param>
        /// <param name="objectKey">Object Key</param>
        /// <param name="moduleId">Module Id</param>
        /// <param name="tabId">Tab Id</param>
        /// <returns>True if the user is subscribed to the content, false otherwise</returns>
        bool IsSubscribed(int userId, int portalId, int subscriptionTypeId, string objectKey, int moduleId = -1, int tabId = -1);
        
        /// <summary>
        /// Adds a new Subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <returns>Subsription Id</returns>
        int AddSubscription(Subscription subscription);

        /// <summary>
        ///  Deletes a Subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        void DeleteSubscription(Subscription subscription);

    }
}

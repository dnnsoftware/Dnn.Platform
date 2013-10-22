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

using System.Data;

namespace DotNetNuke.Services.Social.Subscriptions.Data
{
    public interface IDataService
    {
        /// <summary>
        /// Adds a Subscription Type
        /// </summary>
        /// <param name="subscriptionName">Subscription Type Name</param>
        /// <param name="friendlyName">Subscription Type FriendlyName</param>
        /// <param name="desktopModuleId">DesktopModule Id</param>
        /// <returns>Subscription Type Id</returns>
        int AddSubscriptionType(string subscriptionName, string friendlyName, int desktopModuleId);

        /// <summary>
        /// Returns all the Subscription Types
        /// </summary>
        /// <returns>Subscription types</returns>
        IDataReader GetSubscriptionTypes();
        
        /// <summary>
        /// Deletes a Subscription Type
        /// </summary>
        /// <param name="subscriptionTypeId">Subscription Type Id</param>
        /// <returns>True if the subscription type has been deleted, false otherwise</returns>
        bool DeleteSubscriptionType(int subscriptionTypeId);
        
        /// <summary>
        /// Adds a Subscription
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="portalId">Portal id</param>
        /// <param name="subscriptionTypeId">Subscription Type Id</param>
        /// <param name="objectKey">Object Key</param>
        /// <param name="description">Description</param>
        /// <param name="moduleId">Module Id</param>
        /// <param name="tabId">Tab Id</param>
        /// <param name="objectData">Object Data</param>
        /// <returns>Suscription Id</returns>
        int AddSubscription(int userId, int portalId, int subscriptionTypeId, string objectKey, string description, int moduleId, int tabId, string objectData);

        /// <summary>
        /// Returns the User Subscriptions
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="subscriptionTypeId">Subscription Type Id</param>
        /// <returns>Collection of Subscriptions</returns>
        IDataReader GetSubscriptionsByUser(int portalId, int userId, int subscriptionTypeId);

        /// <summary>
        /// Returns the Content Subscriptions
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="subscriptionTypeId">Subscription Type Id</param>
        /// <param name="objectKey">Object Key</param>
        /// <returns>Collection of Subscriptions</returns>
        IDataReader GetSubscriptionsByContent(int portalId, int subscriptionTypeId, string objectKey);
        
        /// <summary>
        /// Checks if the user is subscribed to an ObjectKey
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="subscriptionTypeId">Subscription Type</param>
        /// <param name="objectKey">Object Key</param>
        /// <param name="moduleId">Module Id</param>
        /// <param name="tabId">Tab Id</param>
        /// <returns>Subscription</returns>
        IDataReader IsSubscribed(int portalId, int userId, int subscriptionTypeId, string objectKey, int moduleId, int tabId);
        
        /// <summary>
        /// Deletes a Subscription
        /// </summary>
        /// <param name="subscriptionId">Subscription Id</param>
        /// <returns>True if the subscription has been deleted, false otherwise</returns>
        bool DeleteSubscription(int subscriptionId);
    }
}
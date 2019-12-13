// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

        /// <summary>
        /// Updates a Subscription Description
        /// </summary>
        /// <param name="objectKey">Subscription Object Key</param>
        /// <param name="portalId">Subscription Portal Id</param>
        /// <param name="newDescription">New Subscription Description</param>
        /// <returns>The number of subscription descriptions that have been updated</returns>
        int UpdateSubscriptionDescription(string objectKey, int portalId, string newDescription);

        /// <summary>
        /// Deletes all subscriptions matching the specified object key
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="objectKey">Object Key</param>
        void DeleteSubscriptionsByObjectKey(int portalId, string objectKey);
    }
}

﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions.Data
{
    using System;
    using System.Data;

    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    public class DataService : ServiceLocator<IDataService, DataService>, IDataService
    {
        private readonly DataProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        public DataService()
        {
            this.provider = DataProvider.Instance();
        }

        /// <inheritdoc/>
        public int AddSubscriptionType(string subscriptionName, string friendlyName, int desktopModuleId)
        {
            return this.provider.ExecuteScalar<int>("CoreMessaging_AddSubscriptionType", subscriptionName, friendlyName, desktopModuleId);
        }

        /// <inheritdoc/>
        public IDataReader GetSubscriptionTypes()
        {
            return this.provider.ExecuteReader("CoreMessaging_GetSubscriptionTypes");
        }

        /// <inheritdoc/>
        public bool DeleteSubscriptionType(int subscriptionTypeId)
        {
            return this.provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscriptionType", subscriptionTypeId) == 0;
        }

        /// <inheritdoc/>
        public int AddSubscription(int userId, int portalId, int subscriptionTypeId, string objectKey, string description, int moduleId, int tabId, string objectData)
        {
            return this.provider.ExecuteScalar<int>(
                "CoreMessaging_AddSubscription",
                userId,
                this.provider.GetNull(portalId),
                subscriptionTypeId,
                objectKey,
                description,
                this.provider.GetNull(moduleId),
                this.provider.GetNull(tabId),
                objectData);
        }

        /// <inheritdoc/>
        public IDataReader GetSubscriptionsByUser(int portalId, int userId, int subscriptionTypeId)
        {
            return this.provider.ExecuteReader("CoreMessaging_GetSubscriptionsByUser", this.provider.GetNull(portalId), userId, this.provider.GetNull(subscriptionTypeId));
        }

        /// <inheritdoc/>
        public IDataReader GetSubscriptionsByContent(int portalId, int subscriptionTypeId, string objectKey)
        {
            return this.provider.ExecuteReader("CoreMessaging_GetSubscriptionsByContent", this.provider.GetNull(portalId), subscriptionTypeId, objectKey);
        }

        /// <inheritdoc/>
        public IDataReader IsSubscribed(int portalId, int userId, int subscriptionTypeId, string objectKey, int moduleId, int tabId)
        {
            return this.provider.ExecuteReader(
                "CoreMessaging_IsSubscribed",
                this.provider.GetNull(portalId),
                userId,
                subscriptionTypeId,
                objectKey,
                this.provider.GetNull(moduleId),
                this.provider.GetNull(tabId));
        }

        /// <inheritdoc/>
        public bool DeleteSubscription(int subscriptionId)
        {
            return this.provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscription", subscriptionId) == 0;
        }

        /// <inheritdoc/>
        public int UpdateSubscriptionDescription(string objectKey, int portalId, string newDescription)
        {
            return this.provider.ExecuteScalar<int>(
                "CoreMessaging_UpdateSubscriptionDescription",
                objectKey,
                portalId,
                newDescription);
        }

        /// <inheritdoc/>
        public void DeleteSubscriptionsByObjectKey(int portalId, string objectKey)
        {
            this.provider.ExecuteNonQuery("CoreMessaging_DeleteSubscriptionsByObjectKey", portalId, objectKey);
        }

        /// <inheritdoc/>
        protected override Func<IDataService> GetFactory()
        {
            return () => new DataService();
        }
    }
}

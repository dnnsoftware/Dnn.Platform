// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Social.Subscriptions.Data;
    using DotNetNuke.Services.Social.Subscriptions.Entities;

    /// <summary>
    /// This controller is responsible to manage the subscription types.
    /// </summary>
    public class SubscriptionTypeController : ServiceLocator<ISubscriptionTypeController, SubscriptionTypeController>, ISubscriptionTypeController
    {
        private readonly IDataService dataService;

        public SubscriptionTypeController()
        {
            this.dataService = DataService.Instance;
        }

        public void AddSubscriptionType(SubscriptionType subscriptionType)
        {
            Requires.NotNull("subscriptionType", subscriptionType);

            subscriptionType.SubscriptionTypeId = this.dataService.AddSubscriptionType(
                subscriptionType.SubscriptionName,
                subscriptionType.FriendlyName,
                subscriptionType.DesktopModuleId);

            CleanCache();
        }

        public SubscriptionType GetSubscriptionType(Func<SubscriptionType, bool> predicate)
        {
            Requires.NotNull("predicate", predicate);

            return this.GetSubscriptionTypes().SingleOrDefault(predicate);
        }

        public IEnumerable<SubscriptionType> GetSubscriptionTypes()
        {
            var cacheArgs = new CacheItemArgs(
                DataCache.SubscriptionTypesCacheKey,
                DataCache.SubscriptionTypesTimeOut,
                DataCache.SubscriptionTypesCachePriority);

            return CBO.GetCachedObject<IEnumerable<SubscriptionType>>(
                cacheArgs,
                c => CBO.FillCollection<SubscriptionType>(this.dataService.GetSubscriptionTypes()));
        }

        public IEnumerable<SubscriptionType> GetSubscriptionTypes(Func<SubscriptionType, bool> predicate)
        {
            Requires.NotNull("predicate", predicate);

            return this.GetSubscriptionTypes().Where(predicate);
        }

        public void DeleteSubscriptionType(SubscriptionType subscriptionType)
        {
            Requires.NotNull("subscriptionType", subscriptionType);
            Requires.NotNegative("subscriptionType.SubscriptionTypeId", subscriptionType.SubscriptionTypeId);

            this.dataService.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId);
            CleanCache();
        }

        protected override Func<ISubscriptionTypeController> GetFactory()
        {
            return () => new SubscriptionTypeController();
        }

        private static void CleanCache()
        {
            DataCache.RemoveCache(DataCache.SubscriptionTypesCacheKey);
        }
    }
}

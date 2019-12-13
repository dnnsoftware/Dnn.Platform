﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Services.Social.Subscriptions.Data;
using DotNetNuke.Services.Social.Subscriptions.Entities;

namespace DotNetNuke.Services.Social.Subscriptions
{
    /// <summary>
    /// This controller is responsible to manage the subscription types.
    /// </summary>
    public class SubscriptionTypeController : ServiceLocator<ISubscriptionTypeController, SubscriptionTypeController>, ISubscriptionTypeController
    {
        private readonly IDataService dataService;

        public SubscriptionTypeController()
        {
            dataService = DataService.Instance;
        }

        protected override Func<ISubscriptionTypeController> GetFactory()
        {
            return () => new SubscriptionTypeController();
        }

        #region Implemented Methods
        public void AddSubscriptionType(SubscriptionType subscriptionType)
        {
            Requires.NotNull("subscriptionType", subscriptionType);

            subscriptionType.SubscriptionTypeId = dataService.AddSubscriptionType(
                subscriptionType.SubscriptionName,
                subscriptionType.FriendlyName,
                subscriptionType.DesktopModuleId);

            CleanCache();
        }

        public SubscriptionType GetSubscriptionType(Func<SubscriptionType, bool> predicate)
        {
            Requires.NotNull("predicate", predicate);

            return GetSubscriptionTypes().SingleOrDefault(predicate);
        }

        public IEnumerable<SubscriptionType> GetSubscriptionTypes()
        {
            var cacheArgs = new CacheItemArgs(DataCache.SubscriptionTypesCacheKey,
                                             DataCache.SubscriptionTypesTimeOut,
                                             DataCache.SubscriptionTypesCachePriority);

            return CBO.GetCachedObject<IEnumerable<SubscriptionType>>(cacheArgs,
                                                c => CBO.FillCollection<SubscriptionType>(dataService.GetSubscriptionTypes()));
        }

        public IEnumerable<SubscriptionType> GetSubscriptionTypes(Func<SubscriptionType, bool> predicate)
        {
            Requires.NotNull("predicate", predicate);

            return GetSubscriptionTypes().Where(predicate);
        }

        public void DeleteSubscriptionType(SubscriptionType subscriptionType)
        {
            Requires.NotNull("subscriptionType", subscriptionType);
            Requires.NotNegative("subscriptionType.SubscriptionTypeId", subscriptionType.SubscriptionTypeId);

            dataService.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId);
            CleanCache();
        }
        #endregion

        private static void CleanCache()
        {
            DataCache.RemoveCache(DataCache.SubscriptionTypesCacheKey);
        }
    }
}

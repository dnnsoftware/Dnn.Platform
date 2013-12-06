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

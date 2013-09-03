#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Framework;
using DotNetNuke.Services.Subscriptions.Common;
using DotNetNuke.Services.Subscriptions.Data;
using DotNetNuke.Services.Subscriptions.Entities;

namespace DotNetNuke.Services.Subscriptions.Controllers
{
    public class SubscriptionTypeController 
                            : ServiceLocator<ISubscriptionTypeController, SubscriptionTypeController>
                            , ISubscriptionTypeController
    {
        protected override Func<ISubscriptionTypeController> GetFactory()
        {
            return () => new SubscriptionTypeController();
        }
 
        #region Private members

        private readonly IDataService _dataService;

        #endregion

        #region Constructors

        public SubscriptionTypeController() : this(new DataService())
        {}

        public SubscriptionTypeController(IDataService dataService)
        {
            _dataService = dataService;
        }

        #endregion

        #region Implementation of ISubscriptionTypeController

        public int AddSubscriptionType(SubscriptionType subType)
        {
            Requires.NotNull("subType", subType);

            try
            {
                DataCache.ClearCache(DataCache.SubscriptionTypesCacheKey);

                subType.SubscriptionTypeId = _dataService.AddSubscriptionType(subType.SubscriptionName,
                                                     subType.FriendlyName,
                                                     subType.DesktopModuleId);

                return subType.SubscriptionTypeId;
            }
            catch (Exception ex)
            {
                throw new SubscriptionsException("Unable to add Subscription Type", ex);
            }
        }

        public IList<SubscriptionType> GetTypes()
        {
            return GetCachedTypes().ToList();
        }

        public IList<SubscriptionType> GetTypes(int desktopModuleId)
        {
            return (from t in GetCachedTypes() where t.DesktopModuleId == desktopModuleId select t).ToList();
        }

        public SubscriptionType GetTypeById(int subscriptionTypeId)
        {
            return
                (from t in GetCachedTypes() where t.SubscriptionTypeId == subscriptionTypeId select t)
                    .FirstOrDefault();
        }

        public SubscriptionType GetTypeByName(int desktopModuleId, string subscriptionName)
        {
            var match = from t in GetCachedTypes()
                        where t.SubscriptionName == subscriptionName && t.DesktopModuleId == desktopModuleId
                        select t;

            return match.FirstOrDefault();
        }

        public bool RemoveSubscriptionType(int subscriptionTypeId)
        {
            DataCache.ClearCache(DataCache.SubscriptionTypesCacheKey);
            return _dataService.RemoveSubscriptionType(subscriptionTypeId);
        }

        #endregion

        #region Private methods

        private IEnumerable<SubscriptionType> GetCachedTypes()
        {
            var cacheArgs = new CacheItemArgs(DataCache.SubscriptionTypesCacheKey,
                                              DataCache.SubscriptionTypesTimeOut,
                                              DataCache.SubscriptionTypesCachePriority);

            return CBO.GetCachedObject<IEnumerable<SubscriptionType>>(cacheArgs,
                                                c => CBO.FillCollection<SubscriptionType>(_dataService.GetAllSubscriptionTypes()));
        }

        #endregion
    }
}
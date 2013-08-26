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
using DotNetNuke.Services.Subscriptions.Common;
using DotNetNuke.Services.Subscriptions.Controllers.Internal;
using DotNetNuke.Services.Subscriptions.Entities;
using DotNetNuke.Services.Subscriptions.Data;

namespace DotNetNuke.Services.Subscriptions.Controllers
{
    public class SubscriptionTypeControllerImpl : ISubscriptionTypeController
    {
        #region Private members

        private readonly IDataService _dataService;

        #endregion

        #region Constructors

        public SubscriptionTypeControllerImpl()
            : this(new DataService())
        {}

        public SubscriptionTypeControllerImpl(IDataService dataService)
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
                DataCache.ClearCache(SubscriptionsCache.SubTypesCacheKey);

                subType.SubscriptionTypeId =
                    _dataService.AddSubscriptionType(subType.SubscriptionName,
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
            return _dataService.RemoveSubscriptionType(subscriptionTypeId);
        }

        #endregion

        #region Private methods

        private IEnumerable<SubscriptionType> GetCachedTypes()
        {
            try
            {
                var types = DataCache.GetCache(SubscriptionsCache.SubTypesCacheKey) as List<SubscriptionType>;
                if (types == null)
                {
                    types = CBO.FillCollection<SubscriptionType>(_dataService.GetAllSubscriptionTypes());

                    if (types.Count > 0)
                    {
                        if (GetTimeout() > 0)
                        {
                            DataCache.SetCache(SubscriptionsCache.SubTypesCacheKey,
                                               types,
                                               TimeSpan.FromMinutes(GetTimeout()));
                        }
                    }
                }

                return types;
            }
            catch (Exception ex)
            {
                throw new SubscriptionsException("Unable to retrieve Subscription Types", ex);
            }
        }

        private static int GetTimeout()
        {
            return Convert.ToInt32(Host.PerformanceSetting) * 20;
        }

        #endregion
    }
}
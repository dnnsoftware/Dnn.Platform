#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Data;
using System.Data.SqlTypes;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;

namespace DotNetNuke.Services.Subscriptions.Data
{
    public class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
        #region Private members

        private readonly DataProvider _provider = DataProvider.Instance();

        #endregion

        #region IDataService

        #region Subscription Types

        public int AddSubscriptionType(string subscriptionName, string friendlyName, int desktopModuleId)
        {
            return _provider.ExecuteScalar<int>("Subscriptions_Type_Add", subscriptionName, friendlyName, desktopModuleId);
        }

        public IDataReader GetAllSubscriptionTypes()
        {
            return _provider.ExecuteReader("Subscriptions_Type_GetAll");
        }

        public bool RemoveSubscriptionType(int subscriptionTypeId)
        {
            return _provider.ExecuteScalar<int>("Subscriptions_Type_Delete", subscriptionTypeId) == 0;
        }

        #endregion

        #region Subscriptions

        public int AddSubscription(int subscriptionId, int userId, int portalId, int subscriptionTypeId, int frequency, int contentItemId, string objectKey, int moduleId, int groupId)
        {
            return _provider.ExecuteScalar<int>("Subscriptions_AddSubscription", subscriptionId, userId, portalId,
                                                subscriptionTypeId, frequency, contentItemId, objectKey, moduleId, groupId);
        }

        public IDataReader GetSubscription(int subscriberId)
        {
            return _provider.ExecuteReader("Subscriptions_GetSubscription", subscriberId);
        }

        public IDataReader IsSubscribed(int portalId, int userId, int subscriptionTypeId, int contentItemId, string objectKey, int moduleId, int groupId)
        {
            return _provider.ExecuteReader("Subscriptions_IsSubscribed", portalId, userId, subscriptionTypeId, contentItemId, objectKey, moduleId, groupId);
        }

        public bool Unsubscribe(int subscriptionId)
        {
            return _provider.ExecuteScalar<int>("Subscriptions_RemoveSubscription", subscriptionId) == 0;
        }

        public IDataReader GetAllSubscribers(int portalId)
        {
            return _provider.ExecuteReader("Subscriptions_GetAllSubscribers", portalId);
        }

        public IDataReader GetUserSubscriptions(int userId, int portalId)
        {
            return _provider.ExecuteReader("Subscriptions_GetUserSubscriptions", userId, portalId);
        }

        public IDataReader GetContentItemSubscribers(int contentItemId, int portalId)
        {
            return _provider.ExecuteReader("Subscriptions_GetContentItemSubscribers", contentItemId, portalId);
        }

        public IDataReader GetNewContentSubscribers(int groupId, int moduleId, int portalId)
        {
            return _provider.ExecuteReader("Subscriptions_GetNewContentSubscribers", groupId, moduleId, portalId);
        }

        #endregion

        #region Task

        public SqlDateTime ToSqlDateTime(DateTime date)
        {
            if (date < (DateTime) SqlDateTime.MinValue)
            {
                return SqlDateTime.MinValue;
            }

            if (date > (DateTime) SqlDateTime.MaxValue)
            {
                return SqlDateTime.MaxValue;
            }

            return date;
        }

        #endregion

        #endregion
    }
}
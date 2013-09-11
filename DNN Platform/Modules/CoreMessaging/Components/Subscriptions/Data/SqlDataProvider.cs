// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

using System.Data;
using DotNetNuke.Data;

namespace DotNetNuke.Modules.CoreMessaging.Components.Subscriptions.Data
{
    public class SqlDataProvider : IDataProvider
    {
        #region Constructors

        public SqlDataProvider()
            : this(DataProvider.Instance())
        {
        }

        public SqlDataProvider(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        #region Private members

        private readonly DataProvider _dataProvider;

        #endregion

        public IDataReader GetUserContentSubscriptions(int userId, int portalId, int pageIndex, int pageSize)
        {
            return _dataProvider.ExecuteReader("SubscriptionMgmt_GetUserSubs", userId, portalId, pageIndex, pageSize);
        }

        public IDataReader GetUserInboxSubscriptions(int userId, int portalId)
        {
            return _dataProvider.ExecuteReader("SubscriptionMgmt_GetUserInboxPref", userId, portalId);
        }
    }
}

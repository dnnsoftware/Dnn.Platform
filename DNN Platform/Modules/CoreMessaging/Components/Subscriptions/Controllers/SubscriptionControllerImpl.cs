// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.SubscriptionsMgmt.Components.Entities;
using DotNetNuke.Modules.SubscriptionsMgmt.Providers.Data;
using DotNetNuke.Modules.SubscriptionsMgmt.Providers.Data.SqlDataProvider;

namespace DotNetNuke.Modules.SubscriptionsMgmt.Components.Controllers
{
    public class SubscriptionControllerImpl : ISubscriptionController
    {
        #region Constructors

        public SubscriptionControllerImpl()
            : this(new SqlDataProvider())
        {
        }

        public SubscriptionControllerImpl(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        #region Private members

        private readonly IDataProvider _dataProvider;

        #endregion

        #region Implementation of ISubscriptionsController

        public List<Subscriber> GetUserContentSubscriptions(int userId, int portalId, int pageIndex, int pageSize)
        {
            return CBO.FillCollection<Subscriber>(_dataProvider.GetUserContentSubscriptions(userId, portalId, pageIndex, pageSize));
        }

        public List<Subscriber> GetUserInboxSubscriptions(int userId, int portalId)
        {
            return CBO.FillCollection<Subscriber>(_dataProvider.GetUserInboxSubscriptions(userId, portalId));
        }

        #endregion
    }
}

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.CoreMessaging.Components.Subscriptions.Data;
using DotNetNuke.Modules.CoreMessaging.Components.Subscriptions.Entities;

namespace DotNetNuke.Modules.CoreMessaging.Components.Subscriptions.Controllers
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

        #endregion
    }
}

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

using System.Collections.Generic;
using DotNetNuke.Modules.SubscriptionsMgmt.Components.Entities;

namespace DotNetNuke.Modules.SubscriptionsMgmt.Components.Controllers
{
    public interface ISubscriptionController
    {
        List<Subscriber> GetUserContentSubscriptions(int userId, int portalId, int pageIndex, int pageSize);
        List<Subscriber> GetUserInboxSubscriptions(int userId, int portalId);
    }
}

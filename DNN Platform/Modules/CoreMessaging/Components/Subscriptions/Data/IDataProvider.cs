// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

using System.Data;

namespace DotNetNuke.Modules.CoreMessaging.Components.Subscriptions.Data
{
    public interface IDataProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="portalId"></param>
        /// <returns></returns>
        IDataReader GetUserContentSubscriptions(int userId, int portalId, int pageIndex, int pageSize);

        IDataReader GetUserInboxSubscriptions(int userId, int portalId);
    }
}

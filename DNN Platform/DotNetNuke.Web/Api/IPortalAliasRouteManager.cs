// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Web.Http.Routing;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.Api
{
    internal interface IPortalAliasRouteManager
    {
        IEnumerable<int> GetRoutePrefixCounts();
        string GetRouteName(string moduleFolderName, string routeName, int count);
        string GetRouteName(string moduleFolderName, string routeName, PortalAliasInfo portalAlias);
        HttpRouteValueDictionary GetAllRouteValues(PortalAliasInfo portalAliasInfo, object routeValues);
        string GetRouteUrl(string moduleFolderName, string url, int count);
        void ClearCachedData();
    }
}

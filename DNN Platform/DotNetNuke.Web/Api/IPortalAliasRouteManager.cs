// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Routing;

    using DotNetNuke.Entities.Portals;

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

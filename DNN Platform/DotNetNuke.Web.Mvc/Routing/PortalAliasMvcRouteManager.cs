// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Routing;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Routing;

using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Internal.SourceGenerators;

internal partial class PortalAliasMvcRouteManager : IPortalAliasMvcRouteManager
{
    private List<int> prefixCounts;

    /// <inheritdoc/>
    public string GetRouteName(string moduleFolderName, string routeName, int count)
    {
        Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);
        Requires.NotNegative("count", count);

        return moduleFolderName + "-" + routeName + "-" + count.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc cref="IPortalAliasMvcRouteManager.GetRouteName(string,string,int)"/>
    [DnnDeprecated(9, 10, 0, "Use overload taking DotNetNuke.Abstractions.Portals.IPortalAliasInfo instead")]
    public partial string GetRouteName(string moduleFolderName, string routeName, PortalAliasInfo portalAlias)
    {
        return this.GetRouteName(moduleFolderName, routeName, (IPortalAliasInfo)portalAlias);
    }

    public string GetRouteName(string moduleFolderName, string routeName, IPortalAliasInfo portalAlias)
    {
        var alias = portalAlias.HttpAlias;
        string appPath = TestableGlobals.Instance.ApplicationPath;
        if (!string.IsNullOrEmpty(appPath))
        {
            int i = alias.IndexOf(appPath, StringComparison.OrdinalIgnoreCase);
            if (i > 0)
            {
                alias = alias.Remove(i, appPath.Length);
            }
        }

        return this.GetRouteName(moduleFolderName, routeName, CalcAliasPrefixCount(alias));
    }

    /// <inheritdoc cref="IPortalAliasMvcRouteManager.GetAllRouteValues"/>
    [DnnDeprecated(9, 10, 0, "Use overload taking DotNetNuke.Abstractions.Portals.IPortalAliasInfo instead")]
    public partial RouteValueDictionary GetAllRouteValues(PortalAliasInfo portalAliasInfo, object routeValues)
    {
        return this.GetAllRouteValues((IPortalAliasInfo)portalAliasInfo, routeValues);
    }

    public RouteValueDictionary GetAllRouteValues(IPortalAliasInfo portalAliasInfo, object routeValues)
    {
        var allRouteValues = new RouteValueDictionary(routeValues);

        var segments = portalAliasInfo.HttpAlias.Split('/');

        if (segments.Length <= 1)
        {
            return allRouteValues;
        }

        for (var i = 1; i < segments.Length; i++)
        {
            var key = "prefix" + (i - 1).ToString(CultureInfo.InvariantCulture);
            var value = segments[i];
            allRouteValues.Add(key, value);
        }

        return allRouteValues;
    }

    /// <inheritdoc/>
    public string GetRouteUrl(string moduleFolderName, string url, int count)
    {
        Requires.NotNegative("count", count);
        Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

        return $"{GeneratePrefixString(count)}DesktopModules/MVC/{moduleFolderName}/{url}";
    }

    /// <inheritdoc/>
    public void ClearCachedData()
    {
        this.prefixCounts = null;
    }

    /// <inheritdoc/>
    public IEnumerable<int> GetRoutePrefixCounts()
    {
        if (this.prefixCounts != null)
        {
            return this.prefixCounts;
        }

        // prefixCounts are required for each route that is mapped but they only change
        // when a new portal is added so cache them until that time
        var portals = PortalController.Instance.GetPortals();
        var segmentCounts1 = new List<int>();

        foreach (
            var count in
            portals.Cast<IPortalInfo>()
                .Select(
                    portal =>
                        PortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalId)
                            .Cast<IPortalAliasInfo>()
                            .Select(x => x.HttpAlias))
                .Select(this.StripApplicationPath)
                .SelectMany(
                    aliases =>
                        aliases.Select(CalcAliasPrefixCount).Where(count => !segmentCounts1.Contains(count))))
        {
            segmentCounts1.Add(count);
        }

        IEnumerable<int> segmentCounts = segmentCounts1;
        this.prefixCounts = segmentCounts.OrderByDescending(x => x).ToList();

        return this.prefixCounts;
    }

    private static string GeneratePrefixString(int count)
    {
        if (count == 0)
        {
            return string.Empty;
        }

        var prefix = string.Empty;

        for (var i = count - 1; i >= 0; i--)
        {
            prefix = "{prefix" + i + "}/" + prefix;
        }

        return prefix;
    }

    private static int CalcAliasPrefixCount(string alias)
    {
        return alias.Count(c => c == '/');
    }

    private static IEnumerable<string> StripApplicationPathIterable(IEnumerable<string> aliases, string appPath)
    {
        foreach (var alias in aliases)
        {
            var i = alias.IndexOf(appPath, StringComparison.OrdinalIgnoreCase);

            if (i > 0)
            {
                yield return alias.Remove(i, appPath.Length);
            }
            else
            {
                yield return alias;
            }
        }
    }

    private IEnumerable<string> StripApplicationPath(IEnumerable<string> aliases)
    {
        var appPath = TestableGlobals.Instance.ApplicationPath;

        return string.IsNullOrEmpty(appPath) ? aliases : StripApplicationPathIterable(aliases, appPath);
    }
}

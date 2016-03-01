﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.Mvc.Routing
{
    internal class PortalAliasMvcRouteManager : IPortalAliasMvcRouteManager
    {
        private List<int> _prefixCounts;

        public string GetRouteName(string moduleFolderName, string routeName, int count)
        {
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);
            Requires.NotNegative("count", count);

            return moduleFolderName + "-" + routeName + "-" + count.ToString(CultureInfo.InvariantCulture);
        }

        public string GetRouteName(string moduleFolderName, string routeName, PortalAliasInfo portalAlias)
        {
            return GetRouteName(moduleFolderName, routeName, CalcAliasPrefixCount(portalAlias.HTTPAlias));
        }

        private static string GeneratePrefixString(int count)
        {
            if (count == 0)
            {
                return "";
            }

            var prefix = "";

            for (var i = count - 1; i >= 0; i--)
            {
                prefix = "{prefix" + i + "}/" + prefix;
            }

            return prefix;
        }

        public RouteValueDictionary GetAllRouteValues(PortalAliasInfo portalAliasInfo, object routeValues)
        {
            var allRouteValues = new RouteValueDictionary(routeValues);

            var segments = portalAliasInfo.HTTPAlias.Split('/');

            if (segments.Length <= 1) return allRouteValues;
            for (var i = 1; i < segments.Length; i++)
            {
                var key = "prefix" + (i - 1).ToString(CultureInfo.InvariantCulture);
                var value = segments[i];
                allRouteValues.Add(key, value);
            }

            return allRouteValues;
        }

        public string GetRouteUrl(string moduleFolderName, string url, int count)
        {
            Requires.NotNegative("count", count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

            return $"{GeneratePrefixString(count)}DesktopModules/MVC/{moduleFolderName}/{url}";
        }

        public void ClearCachedData()
        {
            _prefixCounts = null;
        }

        public IEnumerable<int> GetRoutePrefixCounts()
        {
            if (_prefixCounts != null) return _prefixCounts;
            //prefixCounts are required for each route that is mapped but they only change
            //when a new portal is added so cache them until that time
            var portals = PortalController.Instance.GetPortals();
            var segmentCounts1 = new List<int>();

            foreach (
                var count in
                    portals.Cast<PortalInfo>()
                        .Select(
                            portal =>
                                PortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID)
                                    .Select(x => x.HTTPAlias))
                        .Select(StripApplicationPath)
                        .SelectMany(
                            aliases =>
                                aliases.Select(CalcAliasPrefixCount).Where(count => !segmentCounts1.Contains(count))))
            {
                segmentCounts1.Add(count);
            }
            IEnumerable<int> segmentCounts = segmentCounts1;
            _prefixCounts = segmentCounts.OrderByDescending(x => x).ToList();

            return _prefixCounts;
        }

        private static int CalcAliasPrefixCount(string alias)
        {
            return alias.Count(c => c == '/');
        }

        private IEnumerable<string> StripApplicationPath(IEnumerable<string> aliases)
        {
            var appPath = TestableGlobals.Instance.ApplicationPath;

            return string.IsNullOrEmpty(appPath) ? aliases : StripApplicationPathIterable(aliases, appPath);
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
    }
}

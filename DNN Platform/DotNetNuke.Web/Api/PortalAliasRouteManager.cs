// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Http.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Entities.Portals;

    internal class PortalAliasRouteManager : IPortalAliasRouteManager
    {
        private List<int> _prefixCounts;

        // TODO: this method need remove after drop use old api format.
        [Obsolete("Replaced with GetRouteUrl.  Scheduled for removal in v11.0.0")]
        public static string GetOldRouteUrl(string moduleFolderName, string url, int count)
        {
            Requires.NotNegative("count", count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

            return string.Format("{0}DesktopModules/{1}/API/{2}", new PortalAliasRouteManager().GeneratePrefixString(count), moduleFolderName, url);
        }

        public string GetRouteName(string moduleFolderName, string routeName, int count)
        {
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);
            Requires.NotNegative("count", count);

            return moduleFolderName + "-" + routeName + "-" + count.ToString(CultureInfo.InvariantCulture);
        }

        public string GetRouteName(string moduleFolderName, string routeName, PortalAliasInfo portalAlias)
        {
            var alias = portalAlias.HTTPAlias;
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

        public HttpRouteValueDictionary GetAllRouteValues(PortalAliasInfo portalAliasInfo, object routeValues)
        {
            var allRouteValues = new HttpRouteValueDictionary(routeValues);

            var segments = portalAliasInfo.HTTPAlias.Split('/');

            if (segments.Length > 1)
            {
                for (int i = 1; i < segments.Length; i++)
                {
                    var key = "prefix" + (i - 1).ToString(CultureInfo.InvariantCulture);
                    var value = segments[i];
                    allRouteValues.Add(key, value);
                }
            }

            return allRouteValues;
        }

        public string GetRouteUrl(string moduleFolderName, string url, int count)
        {
            Requires.NotNegative("count", count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

            return string.Format("{0}API/{1}/{2}", this.GeneratePrefixString(count), moduleFolderName, url);
        }

        public void ClearCachedData()
        {
            this._prefixCounts = null;
        }

        public IEnumerable<int> GetRoutePrefixCounts()
        {
            if (this._prefixCounts == null)
            {
                // prefixCounts are required for each route that is mapped but they only change
                // when a new portal is added so cache them until that time
                var portals = PortalController.Instance.GetPortals();

                var segmentCounts1 = new List<int>();

                foreach (PortalInfo portal in portals)
                {
                    IEnumerable<string> aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).Select(x => x.HTTPAlias);

                    aliases = this.StripApplicationPath(aliases);

                    foreach (string alias in aliases)
                    {
                        var count = CalcAliasPrefixCount(alias);

                        if (!segmentCounts1.Contains(count))
                        {
                            segmentCounts1.Add(count);
                        }
                    }
                }

                IEnumerable<int> segmentCounts = segmentCounts1;
                this._prefixCounts = segmentCounts.OrderByDescending(x => x).ToList();
            }

            return this._prefixCounts;
        }

        private static int CalcAliasPrefixCount(string alias)
        {
            return alias.Count(c => c == '/');
        }

        private static IEnumerable<string> StripApplicationPathIterable(IEnumerable<string> aliases, string appPath)
        {
            foreach (string alias in aliases)
            {
                int i = alias.IndexOf(appPath, StringComparison.OrdinalIgnoreCase);

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
            string appPath = TestableGlobals.Instance.ApplicationPath;

            if (string.IsNullOrEmpty(appPath))
            {
                return aliases;
            }

            return StripApplicationPathIterable(aliases, appPath);
        }

        private string GeneratePrefixString(int count)
        {
            if (count == 0)
            {
                return string.Empty;
            }

            string prefix = string.Empty;

            for (int i = count - 1; i >= 0; i--)
            {
                prefix = "{prefix" + i + "}/" + prefix;
            }

            return prefix;
        }
    }
}

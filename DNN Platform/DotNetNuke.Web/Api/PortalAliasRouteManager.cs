// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Http.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>The default <see cref="IPortalAliasRouteManager"/> implementation.</summary>
    internal partial class PortalAliasRouteManager : IPortalAliasRouteManager
    {
        private List<int> prefixCounts;

        /// <summary>Gets the route URL for the old-style web API route.</summary>
        /// <param name="moduleFolderName">The module folder name.</param>
        /// <param name="url">The main URL component.</param>
        /// <param name="count">The count for the route name.</param>
        /// <returns>A route path.</returns>
        [DnnDeprecated(9, 0, 0, "Replaced with GetRouteUrl")]
        public static partial string GetOldRouteUrl(string moduleFolderName, string url, int count)
        {
            Requires.NotNegative("count", count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

            return $"{GeneratePrefixString(count)}DesktopModules/{moduleFolderName}/API/{url}";
        }

        /// <inheritdoc/>
        public string GetRouteName(string moduleFolderName, string routeName, int count)
        {
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);
            Requires.NotNegative("count", count);

            return moduleFolderName + "-" + routeName + "-" + count.ToString(CultureInfo.InvariantCulture);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public string GetRouteUrl(string moduleFolderName, string url, int count)
        {
            Requires.NotNegative("count", count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

            return $"{GeneratePrefixString(count)}API/{moduleFolderName}/{url}";
        }

        /// <inheritdoc/>
        public void ClearCachedData()
        {
            this.prefixCounts = null;
        }

        /// <inheritdoc/>
        public IEnumerable<int> GetRoutePrefixCounts()
        {
            if (this.prefixCounts == null)
            {
                // prefixCounts are required for each route that is mapped but they only change
                // when a new portal is added so cache them until that time
                var portals = PortalController.Instance.GetPortals();

                var segmentCounts1 = new List<int>();

                foreach (PortalInfo portal in portals)
                {
                    IEnumerable<string> aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).Select(x => x.HTTPAlias);

                    aliases = StripApplicationPath(aliases);

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
                this.prefixCounts = segmentCounts.OrderByDescending(x => x).ToList();
            }

            return this.prefixCounts;
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

        private static IEnumerable<string> StripApplicationPath(IEnumerable<string> aliases)
        {
            string appPath = TestableGlobals.Instance.ApplicationPath;

            if (string.IsNullOrEmpty(appPath))
            {
                return aliases;
            }

            return StripApplicationPathIterable(aliases, appPath);
        }

        private static string GeneratePrefixString(int count)
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

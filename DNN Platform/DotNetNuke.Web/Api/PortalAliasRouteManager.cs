#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http.Routing;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.Api
{
    internal class PortalAliasRouteManager : IPortalAliasRouteManager
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
            return GetRouteName(moduleFolderName, routeName, CalcAliasPrefixCount(alias));
        }

        private string GeneratePrefixString(int count)
        {
            if (count == 0)
            {
                return "";
            }
            
            string prefix = "";

            for (int i = count - 1; i >= 0; i--)
            {
                prefix = "{prefix" + i + "}/" + prefix;
            }

            return prefix;
        }

        public HttpRouteValueDictionary GetAllRouteValues(PortalAliasInfo portalAliasInfo, object routeValues)
        {
            var allRouteValues = new HttpRouteValueDictionary(routeValues);

            var segments = portalAliasInfo.HTTPAlias.Split('/');
            
            if(segments.Length > 1)
            {
                  for(int i = 1; i < segments.Length; i++)
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

            return string.Format("{0}API/{1}/{2}", GeneratePrefixString(count), moduleFolderName, url);
        }

        //TODO: this method need remove after drop use old api format.
        public static string GetOldRouteUrl(string moduleFolderName, string url, int count)
        {
            Requires.NotNegative("count", count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

            return string.Format("{0}DesktopModules/{1}/API/{2}", new PortalAliasRouteManager().GeneratePrefixString(count), moduleFolderName, url);
        }

        public void ClearCachedData()
        {
            _prefixCounts = null;
        }

        public IEnumerable<int> GetRoutePrefixCounts()
        {
            if (_prefixCounts == null)
            {
                //prefixCounts are required for each route that is mapped but they only change
                //when a new portal is added so cache them until that time

                
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
                _prefixCounts = segmentCounts.OrderByDescending(x => x).ToList();
            }

            return _prefixCounts;
        }

        private static int CalcAliasPrefixCount(string alias)
        {
            return alias.Count(c => c == '/');
        }

        private IEnumerable<string> StripApplicationPath(IEnumerable<string> aliases)
        {
            string appPath = TestableGlobals.Instance.ApplicationPath;

            if (String.IsNullOrEmpty(appPath))
            {
                return aliases;
            }

            return StripApplicationPathIterable(aliases, appPath);
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
    }
}
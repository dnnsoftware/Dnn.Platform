#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.UI.MenuControllers
{
    public class LinkMenuController : IMenuItemController
    {
        public void UpdateParameters(MenuItem menuItem)
        {
            if (Visible(menuItem))
            {
                var query = GetPathQuery(menuItem);

                int tabId, portalId;
                if (query.ContainsKey("path"))
                {
                    portalId = query.ContainsKey("portalId") ? Convert.ToInt32(query["portalId"]) : PortalSettings.Current.PortalId;
                    tabId = TabController.GetTabByTabPath(portalId, query["path"], string.Empty);
                }
                else
                {
                    portalId = Convert.ToInt32(query["portalId"]);
                    tabId = Convert.ToInt32(query["tabId"]);
                }

                var tabUrl = Globals.NavigateURL(tabId, portalId == Null.NullInteger);
                var alias = Globals.AddHTTP(PortalSettings.Current.PortalAlias.HTTPAlias);
                tabUrl = tabUrl.Replace(alias, string.Empty).TrimStart('/');

                menuItem.Link = tabUrl;
            }
        }

        public bool Visible(MenuItem menuItem)
        {
            var query = GetPathQuery(menuItem);
            if (PortalSettings.Current == null || query == null)
            {
                return false;
            }

            if (query.ContainsKey("sku") && !string.IsNullOrEmpty(query["sku"]))
            {
                if (DotNetNukeContext.Current.Application.SKU != query["sku"])
                {
                    return false;
                }
            }

            int tabId, portalId;
            if (query.ContainsKey("path") && !string.IsNullOrEmpty(query["path"]))
            {
                portalId = query.ContainsKey("portalId") ? Convert.ToInt32(query["portalId"]) : PortalSettings.Current.PortalId;
                tabId = TabController.GetTabByTabPath(portalId, query["path"], string.Empty);

                if (tabId == Null.NullInteger)
                {
                    return false;
                }
            }
            else
            {
                if (!query.ContainsKey("portalId") || !query.ContainsKey("tabId"))
                {
                    return false;
                }

                portalId = Convert.ToInt32(query["portalId"]);
                tabId = Convert.ToInt32(query["tabId"]);
            }

            var tab = TabController.Instance.GetTab(tabId, portalId);
            return (portalId == Null.NullInteger || portalId == PortalSettings.Current.PortalId)
                    && tab != null && !tab.IsDeleted && !tab.DisableLink && tab.IsVisible;
        }

        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
        }

        private IDictionary<string, string> GetPathQuery(MenuItem menuItem)
        {
            var path = menuItem.Path;
            if (!path.Contains("?"))
            {
                return null;
            }

            return path.Substring(path.IndexOf("?", StringComparison.InvariantCultureIgnoreCase) + 1)
                .Split('&')
                .Select(p => p.Split('='))
                .Where(q => q.Length == 2 && !string.IsNullOrEmpty(q[0]) && !string.IsNullOrEmpty(q[1]))
                .ToDictionary(q => q[0], q => q[1]);
        } 
    }
}

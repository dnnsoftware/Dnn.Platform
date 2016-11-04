#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
using System.IO;
using System.Linq;
using Dnn.PersonaBar.Library;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Pages.Components
{
    public class PageManagementController : ServiceLocator<IPageManagementController, PageManagementController>, IPageManagementController
    {
        public static string PageDateTimeFormat = "yyyy-MM-dd hh:mm tt";

        #region Fields
        private readonly ITabController _tabController;
        #endregion

        public PageManagementController()
        {
            _tabController = TabController.Instance;
        }

        #region Properties
        private static string LocalResourcesFile => Path.Combine(Constants.PersonaBarRelativePath, "Pages.resx");

        private static PortalSettings PortalSettings => PortalSettings.Current;

        #endregion

        #region Public Methods
        public string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourcesFile);
        }

        public string GetCreatedInfo(TabInfo tab)
        {
            var createdBy = tab.CreatedByUser(PortalSettings.PortalId);
            var displayName = LocalizeString("System");
            if (createdBy != null)
            {
                displayName = createdBy.DisplayName;
            }

            return displayName;
        }

        public string GetTabHierarchy(TabInfo tab)
        {
            _tabController.PopulateBreadCrumbs(ref tab);
            return tab.BreadCrumbs.Count == 1 ? string.Empty : string.Join(" > ", from t in tab.BreadCrumbs.Cast<TabInfo>().Take(tab.BreadCrumbs.Count - 1) select t.LocalizedTabName);
        }
        #endregion

        protected override Func<IPageManagementController> GetFactory()
        {
            return () => new PageManagementController();
        }
    }
}

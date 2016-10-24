#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Library.Common;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.Pages.Services
{
    [ServiceScope(Scope = ServiceScope.Admin)]
    public class PagesController : PersonaBarApiController
    {
        /// GET: api/Pages/GetPageDetails
        /// <summary>
        /// Get detail of a page
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetPageDetails(int pageId)
        {
            var tab = TabController.Instance.GetTab(pageId, PortalSettings.PortalId);
            if (tab == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Page doesn't exists." });
            }

            var description = !string.IsNullOrEmpty(tab.Description) ? tab.Description : PortalSettings.Description;
            var keywords = !string.IsNullOrEmpty(tab.KeyWords) ? tab.KeyWords : PortalSettings.KeyWords;

            var page = new PageSettings
            {
                TabId = tab.TabID,
                Name = tab.TabName,
                LocalizedName = tab.LocalizedTabName,
                Title = tab.Title,
                Description = description,
                Keywords = keywords,
                Tags = string.Join(",", from t in tab.Terms select t.Name),
                Alias = PortalSettings.PortalAlias.HTTPAlias,
                Url = tab.Url,
                CreatedOnDate = tab.CreatedOnDate,
                IncludeInMenu = tab.IsVisible,
                CustomUrlEnabled = !tab.IsSuperTab && (Config.GetFriendlyUrlProvider() == "advanced"),
                StartDate = tab.StartDate != Null.NullDate ? tab.StartDate : (DateTime?)null,
                EndDate = tab.EndDate != Null.NullDate ? tab.EndDate : (DateTime?)null
            };

            return Request.CreateResponse(HttpStatusCode.OK, page);
        }

        /// GET: api/Pages/GetPageList
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetPageList(int parentId = -1, string searchKey = "")
        {
            var adminTabId = PortalSettings.AdminTabId;

            var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, adminTabId, false, true, false, true);
            var pages = from t in tabs
                        where (t.ParentId != adminTabId) &&
                                !t.IsSystem &&
                                    ((string.IsNullOrEmpty(searchKey) && (t.ParentId == parentId))
                                        || (!string.IsNullOrEmpty(searchKey) &&
                                                (t.TabName.IndexOf(searchKey, StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger
                                                    || t.LocalizedTabName.IndexOf(searchKey, StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger)))
                        select ConvertToPageItem(t, tabs);

            return Request.CreateResponse(HttpStatusCode.OK, pages);
        }

        private PageItem ConvertToPageItem(TabInfo tab, IEnumerable<TabInfo> portalTabs)
        {
            return new PageItem
            {
                Id = tab.TabID,
                Name = tab.LocalizedTabName,
                Url = tab.FullUrl,
                ChildrenCount = portalTabs != null ? portalTabs.Count(ct => ct.ParentId == tab.TabID) : 0,
                PublishDate = tab.CreatedOnDate.ToString("MM/dd/yyyy"),
                ParentId = tab.ParentId,
                Level = tab.Level,
                IsSpecial = TabController.IsSpecialTab(tab.TabID, PortalSettings),
                TabPath = tab.TabPath.Replace("//", "/"),
                LastModifiedOnDate = tab.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.CreateSpecificCulture(tab.CultureCode ?? "en-US")),
                FriendlyLastModifiedOnDate = Utilities.RelativeDateFromUtcDate(tab.LastModifiedOnDate.ToUniversalTime()),
                UseDefaultSkin = UseDefaultSkin(tab)
            };
        }

        private bool UseDefaultSkin(TabInfo tab)
        {
            return !string.IsNullOrEmpty(tab.SkinSrc) && tab.SkinSrc.Equals(PortalSettings.DefaultPortalSkin, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
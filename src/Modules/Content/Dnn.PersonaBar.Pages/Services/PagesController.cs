#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Pages.Services
{
    [ServiceScope(Scope = ServiceScope.Admin)]
    public class PagesController : PersonaBarApiController
    {
        private static readonly IPagesController _pagesController = Components.PagesController.Instance;

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

        [HttpGet]
        public HttpResponseMessage GetPageHierarchy(int pageId)
        {
            var tab = TabController.Instance.GetTab(pageId, PortalSettings.PortalId);
            if (tab == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var paths = new List<int> { tab.TabID };
            while (tab.ParentId != Null.NullInteger)
            {
                tab = TabController.Instance.GetTab(tab.ParentId, PortalSettings.PortalId);
                if (tab != null)
                {
                    paths.Insert(0, tab.TabID);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, paths);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MovePage(PageMoveRequest request)
        {
            var tab = TabController.Instance.GetTab(request.PageId, PortalSettings.PortalId);
            if (tab == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (tab.ParentId != request.ParentId)
            {
                string errorMessage;

                if (!_pagesController.IsValidTabPath(tab, Globals.GenerateTabPath(request.ParentId, tab.TabName), out errorMessage))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, Message = errorMessage });
                }
            }

            switch (request.Action)
            {
                case "before":
                    TabController.Instance.MoveTabBefore(tab, request.RelatedPageId);
                    break;
                case "after":
                    TabController.Instance.MoveTabAfter(tab, request.RelatedPageId);
                    break;
                case "parent":
                    //avoid move tab into its child page
                    if (IsChild(PortalSettings.PortalId, tab.TabID, request.ParentId))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, Message = "DragInvalid" });
                    }

                    TabController.Instance.MoveTabToParent(tab, request.ParentId);
                    break;
            }

            //as tab's parent may changed, url need refresh.
            tab = TabController.Instance.GetTab(request.PageId, PortalSettings.PortalId);
            var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, false, true, false, true);
            var pageItem = ConvertToPageItem(tab, tabs);

            return Request.CreateResponse(HttpStatusCode.OK, new { Status = 0, Page = pageItem });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeletePage(PageItem page)
        {
            var tab = TabController.Instance.GetTab(page.Id, PortalSettings.PortalId);
            if (tab == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (TabPermissionController.CanDeletePage(tab))
            {
                TabController.Instance.SoftDeleteTab(tab.TabID, PortalSettings);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        private bool IsChild(int portalId, int tabId, int parentId)
        {
            if (parentId == Null.NullInteger)
            {
                return false;
            }

            if (tabId == parentId)
            {
                return true;
            }

            var tab = TabController.Instance.GetTab(parentId, portalId);
            while (tab != null && tab.ParentId != Null.NullInteger)
            {
                if (tab.ParentId == tabId)
                {
                    return true;
                }

                tab = TabController.Instance.GetTab(tab.ParentId, portalId);
            }

            return false;
        }

        // TODO: This should be a POST
        [HttpGet]
        public HttpResponseMessage EditModeForPage(int id)
        {
            var newCookie = new HttpCookie("LastPageId", $"{PortalSettings.PortalId}:{id}") { Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/") };
            HttpContext.Current.Response.Cookies.Add(newCookie);

            if (PortalSettings.UserMode != DotNetNuke.Entities.Portals.PortalSettings.Mode.Edit)
            {
                var personalizationController = new PersonalizationController();
                var personalization = personalizationController.LoadProfile(UserInfo.UserID, PortalSettings.PortalId);
                personalization.Profile["Usability:UserMode" + PortalSettings.PortalId] = "EDIT";
                personalization.IsModified = true;
                personalizationController.SaveProfile(personalization);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private PageItem ConvertToPageItem(TabInfo tab, IEnumerable<TabInfo> portalTabs)
        {
            return new PageItem
            {
                Id = tab.TabID,
                Name = tab.LocalizedTabName,
                Url = tab.FullUrl,
                ChildrenCount = portalTabs?.Count(ct => ct.ParentId == tab.TabID) ?? 0,
                Status = GetTabStatus(tab),
                ParentId = tab.ParentId,
                Level = tab.Level,
                IsSpecial = TabController.IsSpecialTab(tab.TabID, PortalSettings),
                TabPath = tab.TabPath.Replace("//", "/")
            };
        }

        // TODO: Refactor to use enum
        private string GetTabStatus(TabInfo tab)
        {
            if (tab.DisableLink)
            {
                return "Disabled";
            }

            return tab.IsVisible ? "Visible" : "Hidden";
        }
    }
}
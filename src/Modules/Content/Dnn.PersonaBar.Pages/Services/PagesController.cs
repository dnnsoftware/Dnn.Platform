#region Copyright

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Services.Dto;
using Dnn.PersonaBar.Themes.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.OutputCache;
using DotNetNuke.Web.Api;
using Localization = Dnn.PersonaBar.Pages.Components.Localization;

namespace Dnn.PersonaBar.Pages.Services
{
    [ServiceScope(Identifier = "Pages")]
    [DnnExceptionFilter]
    public class PagesController : PersonaBarApiController
    {
        private readonly Lazy<Dictionary<string, Locale>> _locales;
        private readonly IPagesController _pagesController;
        private readonly IBulkPagesController _bulkPagesController;
        private readonly IThemesController _themesController;

        public PagesController()
        {
            _locales = new Lazy<Dictionary<string, Locale>>(() => LocaleController.Instance.GetLocales(PortalId));
            _pagesController = Components.PagesController.Instance;
            _themesController = ThemesController.Instance;
            _bulkPagesController = BulkPagesController.Instance;
        }

        /// GET: api/Pages/GetPageDetails
        /// <summary>
        /// Get detail of a page
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetPageDetails(int pageId)
        {
            try
            {
                var tab = _pagesController.GetPageDetails(pageId);
                var page = Converters.ConvertToPageSettings<PageSettings>(tab);
                page.Modules = _pagesController.GetModules(page.TabId).Select(Converters.ConvertToModuleItem);
                page.PageUrls = _pagesController.GetPageUrls(page.TabId);
                page.Permissions = _pagesController.GetPermissionsData(pageId);
                page.SiteAliases = SiteAliases;
                page.PrimaryAliasId = PrimaryAliasId;
                page.Locales = Locales;
                page.HasParent = tab.ParentId > -1;

                return Request.CreateResponse(HttpStatusCode.OK, page);
            }
            catch (PageNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new {Message = "Page doesn't exists."});
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateCustomUrl(SaveUrlDto dto)
        {
            var urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
            bool modified;
            //Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(PortalSettings.PortalId)));

            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                                new
                                                {
                                                    Success = false,
                                                    ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error"),
                                                    SuggestedUrlPath = "/" + urlPath
                                                });
            }

            //Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, -1, PortalSettings, out modified);
            if (modified)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                                new
                                                {
                                                    Success = false,
                                                    ErrorMessage = Localization.GetString("UrlPathNotUnique.Error"),
                                                    SuggestedUrlPath = "/" + urlPath
                                                });
            }

            var tab = PortalSettings.ActiveTab;

            if (tab.TabUrls.Any(u => u.Url.ToLowerInvariant() == dto.Path.ValueOrEmpty().ToLowerInvariant()
                                     && (u.PortalAliasId == dto.SiteAliasKey || u.PortalAliasId == -1)))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("DuplicateUrl.Error")
                });
            }

            var seqNum = (tab.TabUrls.Count > 0) ? tab.TabUrls.Max(t => t.SeqNum) + 1 : 1;
            var portalLocales = LocaleController.Instance.GetLocales(PortalId);
            var cultureCode = portalLocales.Where(l => l.Value.KeyID == dto.LocaleKey)
                                .Select(l => l.Value.Code)
                                .SingleOrDefault();

            var portalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
            if (portalAliasUsage == PortalAliasUsageType.Default)
            {
                var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalId)
                                                        .SingleOrDefault(a => a.PortalAliasID == dto.SiteAliasKey);

                if (string.IsNullOrEmpty(cultureCode) || alias == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("InvalidRequest.Error")
                    });
                }
            }
            else
            {
                var cultureAlias = PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalId)
                                                            .FirstOrDefault(a => a.CultureCode == cultureCode);

                if (portalLocales.Count > 1 && !PortalSettings.ContentLocalizationEnabled && (string.IsNullOrEmpty(cultureCode) || cultureAlias == null))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("InvalidRequest.Error")
                    });
                }
            }

            var tabUrl = new TabUrlInfo
            {
                TabId = tab.TabID,
                SeqNum = seqNum,
                PortalAliasId = dto.SiteAliasKey,
                PortalAliasUsage = portalAliasUsage,
                QueryString = dto.QueryString.ValueOrEmpty(),
                Url = dto.Path.ValueOrEmpty(),
                CultureCode = cultureCode,
                HttpStatus = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture),
                IsSystem = false
            };

            TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);

            var response = new
            {
                Success = true,
                Id = seqNum // returns Id of the created Url
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateCustomUrl(SaveUrlDto dto)
        {
            var urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
            bool modified;
            //Clean Url
            var options =
                UrlRewriterUtils.ExtendOptionsForCustomURLs(
                    UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(PortalSettings.PortalId)));

            //now clean the path
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error"),
                        SuggestedUrlPath = "/" + urlPath
                    });
            }

            //Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, -1, PortalSettings, out modified);
            if (modified)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("UrlPathNotUnique.Error"),
                        SuggestedUrlPath = "/" + urlPath
                    });
            }

            var tab = PortalSettings.ActiveTab;
            var cultureCode = LocaleController.Instance.GetLocales(PortalId)
                .Where(l => l.Value.KeyID == dto.LocaleKey)
                .Select(l => l.Value.Code)
                .SingleOrDefault();

            if (dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture) == "200")
            {
                //We need to check if we are updating a current url or creating a new 200
                var tabUrl = tab.TabUrls.SingleOrDefault(t => t.SeqNum == dto.Id
                                                              && t.HttpStatus == "200");
                if (tabUrl == null)
                {
                    //Just create Url
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = dto.Id,
                        PortalAliasId = dto.SiteAliasKey,
                        PortalAliasUsage = (PortalAliasUsageType) dto.SiteAliasUsage,
                        QueryString = dto.QueryString.ValueOrEmpty(),
                        Url = dto.Path.ValueOrEmpty(),
                        CultureCode = cultureCode,
                        HttpStatus = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture),
                        IsSystem = dto.IsSystem // false
                    };
                    TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);
                }
                else
                {
                    //Change the original 200 url to a redirect
                    tabUrl.HttpStatus = "301";
                    tabUrl.SeqNum = dto.Id;
                    TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);

                    //Add new custom url
                    tabUrl.Url = dto.Path.ValueOrEmpty();
                    tabUrl.HttpStatus = "200";
                    tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                    TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);
                }
            }
            else
            {
                //Just update the url
                var tabUrl = new TabUrlInfo
                {
                    TabId = tab.TabID,
                    SeqNum = dto.Id,
                    PortalAliasId = dto.SiteAliasKey,
                    PortalAliasUsage = (PortalAliasUsageType) dto.SiteAliasUsage,
                    QueryString = dto.QueryString.ValueOrEmpty(),
                    Url = dto.Path.ValueOrEmpty(),
                    CultureCode = cultureCode,
                    HttpStatus = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture),
                    IsSystem = dto.IsSystem // false
                };
                TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);
            }


            var response = new
            {
                Success = true
            };

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteCustomUrl(UrlIdDto dto)
        {
            var tab = PortalSettings.ActiveTab;
            var tabUrl = tab.TabUrls.SingleOrDefault(u => u.SeqNum == dto.Id);

            TabController.Instance.DeleteTabUrl(tabUrl, PortalId, true);

            // Delete Custom Url
            var response = new
            {
                Success = true
            };
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
        public class UrlIdDto
        {
            public int Id { get; set; }
        }

        protected IOrderedEnumerable<KeyValuePair<int, string>> Locales
        {
            get
            {
                return _locales.Value.Values.Select(local => new KeyValuePair<int, string>(local.KeyID, local.EnglishName)).OrderBy(x => x.Value);
            }
        }
        protected IEnumerable<KeyValuePair<int, string>> SiteAliases
        {
            get
            {
                var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalId);
                return aliases.Select(alias => new KeyValuePair<int, string>(alias.KeyID, alias.HTTPAlias)).OrderBy(x => x.Value);
            }
        }
        protected int? PrimaryAliasId
        {
            get
            {
                var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalId);
                var primary = aliases.Where(a => a.IsPrimary
                                    && (a.CultureCode == PortalSettings.CultureCode || String.IsNullOrEmpty(a.CultureCode)))
                                .OrderByDescending(a => a.CultureCode)
                                .FirstOrDefault();
                return primary == null ? (int?)null : primary.KeyID;
            }
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
            var pages = from p in _pagesController.GetPageList(parentId, searchKey)
                select Converters.ConvertToPageItem<PageItem>(p, tabs);
            return Request.CreateResponse(HttpStatusCode.OK, pages);
        }

        [HttpGet]
        public HttpResponseMessage GetPageHierarchy(int pageId)
        {
            try
            {
                var paths = _pagesController.GetPageHierarchy(pageId);
                return Request.CreateResponse(HttpStatusCode.OK, paths);
            }
            catch (PageNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MovePage(PageMoveRequest request)
        {
            try
            {
                var tab = _pagesController.MovePage(request);
                var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, false, true, false,
                    true);
                var pageItem = Converters.ConvertToPageItem<PageItem>(tab, tabs);
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 0, Page = pageItem});
            }
            catch (PageNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (PageException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 1, ex.Message});
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeletePage(PageItem page)
        {
            try
            {
                _pagesController.DeletePage(page);
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 0});
            }
            catch (PageNotFoundException)
            {

                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeletePageModule(PageModuleItem module)
        {
            try
            {
                _pagesController.DeleteTabModule(module.PageId, module.ModuleId);
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 0});
            }
            catch (PageModuleNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CopyThemeToDescendantPages(CopyThemeRequest copyTheme)
        {
            _pagesController.CopyThemeToDescendantPages(copyTheme.PageId, copyTheme.Theme);
            return Request.CreateResponse(HttpStatusCode.OK, new {Status = 0});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CopyPermissionsToDescendantPages(CopyPermissionsRequest copyPermissions)
        {
            try
            {
                _pagesController.CopyPermissionsToDescendantPages(copyPermissions.PageId);
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 0});
            }
            catch (PageNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (PermissionsNotMetException)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }

        // TODO: This should be a POST
        [HttpGet]
        public HttpResponseMessage EditModeForPage(int id)
        {
            _pagesController.EditModeForPage(id, UserInfo.UserID);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SavePageDetails(PageSettings pageSettings)
        {
            try
            {
                var tab = _pagesController.SavePageDetails(pageSettings);
                var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, false, true, false,
                    true);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Page = Converters.ConvertToPageItem<PageItem>(tab, tabs)
                });
            }
            catch (PageNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new {Message = "Page doesn't exists."});
            }
            catch (PageValidationException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 1, ex.Field, ex.Message});
            }
        }

        [HttpGet]
        public HttpResponseMessage GetDefaultPermissions()
        {
            var permissions = _pagesController.GetPermissionsData(0);
            return Request.CreateResponse(HttpStatusCode.OK, permissions);
        }

        [HttpGet]
        public HttpResponseMessage GetCacheProviderList()
        {
            var providers = from p in OutputCachingProvider.GetProviderList() select p.Key;
            return Request.CreateResponse(HttpStatusCode.OK, providers);
        }

        [HttpGet]
        public HttpResponseMessage GetPageUrlPreview(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {Url = string.Empty});
            }

            var cleanedUrl = _pagesController.CleanTabUrl(url);
            return Request.CreateResponse(HttpStatusCode.OK, new {Url = cleanedUrl});
        }

        [HttpGet]
        public HttpResponseMessage GetThemes()
        {
            var themes = _themesController.GetLayouts(PortalSettings, ThemeLevel.Global | ThemeLevel.Site);
            var defaultPortalThemeName = GetDefaultPortalTheme();
            var defaultPortalLayout = GetDefaultPortalLayout();
            var defaultPortalContainer = GetDefaultPortalContainer();

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                themes,
                defaultPortalThemeName,
                defaultPortalLayout,
                defaultPortalContainer
            });
        }

        private string GetDefaultPortalTheme()
        {
            var layoutSrc = GetDefaultPortalLayout();
            if (string.IsNullOrWhiteSpace(layoutSrc))
            {
                return null;
            }
            var layout = _themesController.GetThemeFile(PortalSettings.Current, layoutSrc, ThemeType.Skin);
            return layout?.ThemeName;
        }

        private string GetDefaultPortalContainer()
        {
            return PortalController.GetPortalSetting("DefaultPortalContainer", PortalId, Host.DefaultPortalSkin, PortalSettings.CultureCode);
        }

        private string GetDefaultPortalLayout()
        {
            return PortalController.GetPortalSetting("DefaultPortalSkin", PortalId, Host.DefaultPortalSkin, PortalSettings.CultureCode);
        }

        [HttpGet]
        public HttpResponseMessage GetThemeFiles(string themeName)
        {
            const ThemeLevel level = ThemeLevel.Global | ThemeLevel.Site;
            var themeLayout = _themesController.GetLayouts(PortalSettings, level).FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));
            var themeContainer = _themesController.GetContainers(PortalSettings, level).FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));

            if (themeLayout == null || themeContainer == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "ThemeNotFound");
            }

            return Request.CreateResponse(HttpStatusCode.OK, new {
                layouts = _themesController.GetThemeFiles(PortalSettings, themeLayout),
                containers = _themesController.GetThemeFiles(PortalSettings, themeContainer)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveBulkPages(BulkPage bulkPage)
        {
            try
            {
                var bulkPageResponse = _bulkPagesController.AddBulkPages(bulkPage);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Response = bulkPageResponse
                });
            }
            catch (PageValidationException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Field, ex.Message });
            }
        }
    }
}

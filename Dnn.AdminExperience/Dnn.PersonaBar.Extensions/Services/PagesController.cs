// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Library.Dto.Tabs;
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Pages.Components.Dto;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Components.Security;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using Dnn.PersonaBar.Themes.Components;
    using Dnn.PersonaBar.Themes.Components.DTO;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.OutputCache;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.UI.WebControls;

    using Localization = Dnn.PersonaBar.Pages.Components.Localization;

    /// <summary>API controller for the Pages persona bar module.</summary>
    [MenuPermission(MenuName = "Dnn.Pages")]
    [DnnExceptionFilter]
    public class PagesController : PersonaBarApiController
    {
        private const string LocalResourceFile = Library.Constants.PersonaBarRelativePath + "Modules/Dnn.Pages/App_LocalResources/Pages.resx";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PagesController));
        private readonly IPagesController pagesController;

        private readonly IBulkPagesController bulkPagesController;
        private readonly IThemesController themesController;
        private readonly ITemplateController templateController;
        private readonly IDefaultPortalThemeController defaultPortalThemeController;

        private readonly ITabController tabController;
        private readonly ILocaleController localeController;
        private readonly ISecurityService securityService;
        private readonly IWorkflowManager workflowManager;

        /// <summary>Initializes a new instance of the <see cref="PagesController"/> class.</summary>
        /// <param name="navigationManager">the navigation manager to provide navigation features.</param>
        /// <param name="pagesController">The pages controller.</param>
        /// <param name="templateController">The template controller.</param>
        public PagesController(INavigationManager navigationManager, IPagesController pagesController, ITemplateController templateController)
        {
            this.NavigationManager = navigationManager;

            this.pagesController = pagesController;
            this.themesController = ThemesController.Instance;
            this.bulkPagesController = BulkPagesController.Instance;
            this.templateController = templateController;
            this.defaultPortalThemeController = DefaultPortalThemeController.Instance;

            this.tabController = TabController.Instance;
            this.localeController = LocaleController.Instance;
            this.securityService = SecurityService.Instance;
            this.workflowManager = WorkflowManager.Instance;
        }

        /// <summary>Gets the Navigation Manager that provides navigation features.</summary>
        protected INavigationManager NavigationManager { get; }

        /// GET: api/Pages/GetPageDetails
        /// <summary>Get detail of a page.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>The page details.</returns>
        [HttpGet]
        public HttpResponseMessage GetPageDetails(int pageId)
        {
            if (!this.securityService.CanManagePage(pageId))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                var page = this.pagesController.GetPageSettings(pageId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { page, new DnnFileUploadOptions().ValidationCode });
            }
            catch (PageNotFoundException)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Page doesn't exists." });
            }
        }

        /// GET: api/Pages/GetCustomUrls
        /// <summary>Get custom Urls of a page.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>A list of custom urls.</returns>
        [HttpGet]
        public HttpResponseMessage GetCustomUrls(int pageId)
        {
            if (!this.securityService.CanManagePage(pageId))
            {
                return this.GetForbiddenResponse();
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, this.pagesController.GetPageUrls(pageId));
        }

        /// <summary>Creates a custom url for SEO purposes.</summary>
        /// <param name="dto"><see cref="SeoUrl"/> DTO.</param>
        /// <returns>Information about success or failure as well as a possible error message and a new url suggestion.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage CreateCustomUrl(SeoUrl dto)
        {
            if (!this.securityService.CanManagePage(dto.TabId))
            {
                return this.GetForbiddenResponse();
            }

            var result = this.pagesController.CreateCustomUrl(dto);

            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    result.Id,
                    result.Success,
                    result.ErrorMessage,
                    result.SuggestedUrlPath,
                });
        }

        /// <summary>Updates a custom url.</summary>
        /// <param name="dto"><see cref="SeoUrl"/> DTO.</param>
        /// <returns>the id, if the call succeeded or faile, a possible error message and a possible new url suggestion.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage UpdateCustomUrl(SeoUrl dto)
        {
            if (!this.securityService.CanManagePage(dto.TabId))
            {
                return this.GetForbiddenResponse();
            }

            var result = this.pagesController.UpdateCustomUrl(dto);

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                result.Id,
                result.Success,
                result.ErrorMessage,
                result.SuggestedUrlPath,
            });
        }

        /// <summary>Deletes a custom URL.</summary>
        /// <param name="dto"><see cref="UrlIdDto"/> DTO.</param>
        /// <returns>A value indicating if the call succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage DeleteCustomUrl(UrlIdDto dto)
        {
            if (!this.securityService.CanManagePage(dto.TabId))
            {
                return this.GetForbiddenResponse();
            }

            this.pagesController.DeleteCustomUrl(dto);

            var response = new
            {
                Success = true,
            };

            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// GET: api/Pages/GetPageList
        /// <summary>Gets the list of pages for a given parent page.</summary>
        /// <param name="parentId">The page (tab) id for the parent.</param>
        /// <param name="searchKey">An optional search string.</param>
        /// <returns>A list of pages.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "VIEW_PAGE_LIST,VIEW")]
        public HttpResponseMessage GetPageList(int parentId = -1, string searchKey = "")
        {
            var adminTabId = this.PortalSettings.AdminTabId;
            var tabs = TabController.GetPortalTabs(this.PortalSettings.PortalId, adminTabId, false, true, false, true);
            var pages = from p in this.pagesController.GetPageList(this.PortalSettings, parentId, searchKey)
                        select Converters.ConvertToPageItem<PageItem>(p, tabs);
            return this.Request.CreateResponse(HttpStatusCode.OK, pages);
        }

        /// GET: api/Pages/SearchPages
        /// <summary>Searches for pages.</summary>
        /// <param name="searchKey">The search string.</param>
        /// <param name="pageType">The type of page.</param>
        /// <param name="tags">The page tags.</param>
        /// <param name="publishStatus">The publish status of the page.</param>
        /// <param name="publishDateStart">The publish start date.</param>
        /// <param name="publishDateEnd">The publish end date.</param>
        /// <param name="workflowId">The workflow id for the page.</param>
        /// <param name="pageIndex">What index of the pages paging to return.</param>
        /// <param name="pageSize">How many pages to return per paging page.</param>
        /// <returns>Paged list of pages.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "VIEW_PAGE_LIST,VIEW")]
        public HttpResponseMessage SearchPages(
            string searchKey = "",
            string pageType = "",
            string tags = "",
            string publishStatus = "All",
            string publishDateStart = "",
            string publishDateEnd = "",
            int workflowId = -1,
            int pageIndex = -1,
            int pageSize = -1)
        {
            int totalRecords;
            var adminTabId = this.PortalSettings.AdminTabId;
            var tabs = TabController.GetPortalTabs(this.PortalSettings.PortalId, adminTabId, false, true, false, true);
            var pages = from p in this.pagesController.SearchPages(out totalRecords, searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize)
                        select Converters.ConvertToPageItem<PageItem>(p, tabs);
            var response = new
            {
                Success = true,
                Results = pages,
                TotalResults = totalRecords,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Gets the pages hierarchy.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>The page hierarchy.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage GetPageHierarchy(int pageId)
        {
            try
            {
                var paths = this.pagesController.GetPageHierarchy(pageId);
                return this.Request.CreateResponse(HttpStatusCode.OK, paths);
            }
            catch (PageNotFoundException)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        /// <summary>Moves a page to another place in the hierarchy.</summary>
        /// <param name="request"><see cref="PageMoveRequest"/> DTO.</param>
        /// <returns>A status and information about the page at it's new location.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage MovePage(PageMoveRequest request)
        {
            if (!this.securityService.CanManagePage(request.PageId)
                || !this.securityService.CanManagePage(request.ParentId)
                || !this.securityService.CanManagePage(request.RelatedPageId)
                || !this.securityService.CanManagePage(TabController.Instance.GetTab(request.RelatedPageId, this.PortalId)?.ParentId ?? -1))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                var tab = this.pagesController.MovePage(request);
                var tabs = TabController.GetPortalTabs(
                    this.PortalSettings.PortalId,
                    Null.NullInteger,
                    false,
                    true,
                    false,
                    true);
                var pageItem = Converters.ConvertToPageItem<PageItem>(tab, tabs);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0, Page = pageItem });
            }
            catch (PageNotFoundException)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (PageException ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Message });
            }
        }

        /// <summary>Deletes a page.</summary>
        /// <param name="page">The page to delete, <see cref="PageItem"/> DTO.</param>
        /// <param name="hardDelete">Should the page be hard-deleted or not.</param>
        /// <returns>The status of the page deletion.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage DeletePage(PageItem page, [FromUri] bool hardDelete = false)
        {
            if (!this.securityService.CanDeletePage(page.Id))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                this.pagesController.DeletePage(page, hardDelete, this.PortalSettings);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
            }
            catch (PageNotFoundException)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        /// <summary>Deletes a module from a page.</summary>
        /// <param name="module">The module to delete, <see cref="PageModuleItem"/> DTO.</param>
        /// <returns>A status code or an error.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage DeletePageModule(PageModuleItem module)
        {
            if (!this.securityService.CanManagePage(module.PageId))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                this.pagesController.DeleteTabModule(module.PageId, module.ModuleId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
            }
            catch (PageModuleNotFoundException)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        /// <summary>Copies the theme from a page to descendend pages.</summary>
        /// <param name="copyTheme"><see cref="CopyThemeRequest"/>.</param>
        /// <returns>The status of the request.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage CopyThemeToDescendantPages(CopyThemeRequest copyTheme)
        {
            if (!this.securityService.CanManagePage(copyTheme.PageId))
            {
                return this.GetForbiddenResponse();
            }

            this.pagesController.CopyThemeToDescendantPages(copyTheme.PageId, copyTheme.Theme);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>Copies permissions from a page to the descendent pages.</summary>
        /// <param name="copyPermissions"><see cref="CopyPermissionsRequest"/> DTO.</param>
        /// <returns>The status of the operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage CopyPermissionsToDescendantPages(CopyPermissionsRequest copyPermissions)
        {
            if (!this.securityService.CanAdminPage(copyPermissions.PageId))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                this.pagesController.CopyPermissionsToDescendantPages(copyPermissions.PageId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
            }
            catch (PageNotFoundException)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (PermissionsNotMetException)
            {
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }

        /// <summary>Sets a page in edit mode.</summary>
        /// <param name="id">the page (tab) id.</param>
        /// <returns>Sets a cookie for edit mode on the specified page then returns a success message.</returns>
        [HttpPost]
        public HttpResponseMessage EditModeForPage([FromUri] int id)
        {
            if (!TabPermissionController.CanAddContentToPage(TabController.Instance.GetTab(id, this.PortalId)))
            {
                return this.GetForbiddenResponse();
            }

            this.pagesController.EditModeForPage(id, this.UserInfo.UserID);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
        }

        /// <summary>Saves the page details.</summary>
        /// <param name="pageSettings">The new page settings, <see cref="PageSettings"/> DTO.</param>
        /// <returns>The new page details.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SavePageDetails(PageSettings pageSettings)
        {
            if (!this.securityService.CanSavePageDetails(pageSettings))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                pageSettings.Clean();
                var tab = this.pagesController.SavePageDetails(this.PortalSettings, pageSettings);
                var tabs = TabController.GetPortalTabs(
                    this.PortalSettings.PortalId,
                    Null.NullInteger,
                    false,
                    true,
                    false,
                    true);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Page = Converters.ConvertToPageItem<PageItem>(tab, tabs),
                });
            }
            catch (PageNotFoundException)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Page doesn't exists." });
            }
            catch (PageValidationException ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Field, ex.Message });
            }
        }

        /// <summary>Gets the default page settings.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>The page default settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetDefaultSettings(int pageId = 0)
        {
            var settings = this.pagesController.GetDefaultSettings(pageId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { page = settings, new DnnFileUploadOptions().ValidationCode });
        }

        /// <summary>Gets the list of cache providers.</summary>
        /// <returns>List of cache providers.</returns>
        [HttpGet]
        public HttpResponseMessage GetCacheProviderList()
        {
            var providers = from p in OutputCachingProvider.GetProviderList() select p.Key;
            return this.Request.CreateResponse(HttpStatusCode.OK, providers);
        }

        /// <summary>Gets the available themes.</summary>
        /// <returns>Available themes.</returns>
        [HttpGet]
        public HttpResponseMessage GetThemes()
        {
            var themes = this.themesController.GetLayouts(this.PortalSettings, ThemeLevel.All);

            var defaultTheme = this.GetDefaultPortalTheme();
            var defaultPortalThemeName = defaultTheme?.ThemeName;
            var defaultPortalThemeLevel = defaultTheme?.Level;
            var defaultPortalLayout = this.defaultPortalThemeController.GetDefaultPortalLayout();
            var defaultPortalContainer = this.defaultPortalThemeController.GetDefaultPortalContainer();

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                themes,
                defaultPortalThemeName,
                defaultPortalThemeLevel,
                defaultPortalLayout,
                defaultPortalContainer,
            });
        }

        /// <summary>Gets the theme files for a given theme.</summary>
        /// <param name="themeName">The name of the theme.</param>
        /// <param name="level">The level of the theme, <see cref="ThemeLevel"/>.</param>
        /// <returns>Returns a list of available layouts and containers for each theme.</returns>
        [HttpGet]
        public HttpResponseMessage GetThemeFiles(string themeName, ThemeLevel level)
        {
            var themeLayout = this.themesController.GetLayouts(this.PortalSettings, level)
                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == level);
            var themeContainer = this.themesController.GetContainers(this.PortalSettings, level)
                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == level);

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                layouts = themeLayout == null ? new List<ThemeFileInfo>() : this.themesController.GetThemeFiles(this.PortalSettings, themeLayout),
                containers = themeContainer == null ? new List<ThemeFileInfo>() : this.themesController.GetThemeFiles(this.PortalSettings, themeContainer),
            });
        }

        /// <summary>Save bulk pages (Add multiple pages).</summary>
        /// <param name="bulkPage"><see cref="BulkPage"/> DTO.</param>
        /// <returns>A status code and the result of adding the multiple pages.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage SaveBulkPages(BulkPage bulkPage)
        {
            if (!this.securityService.IsPageAdminUser())
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                bulkPage.Clean();
                var bulkPageResponse = this.bulkPagesController.AddBulkPages(bulkPage, validateOnly: false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Response = bulkPageResponse,
                });
            }
            catch (PageValidationException ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Field, ex.Message });
            }
        }

        /// <summary>Validates if bulk pages (Add multiple pages) information is valid.</summary>
        /// <param name="bulkPage"><see cref="BulkPage"/> DTO.</param>
        /// <returns>A status code and the result of the bulk pages check.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage PreSaveBulkPagesValidate(BulkPage bulkPage)
        {
            if (!this.securityService.IsPageAdminUser())
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                bulkPage.Clean();
                var bulkPageResponse = this.bulkPagesController.AddBulkPages(bulkPage, validateOnly: true);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Response = bulkPageResponse,
                });
            }
            catch (PageValidationException ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Field, ex.Message });
            }
        }

        /// <summary>Saves a page as a template.</summary>
        /// <param name="pageTemplate"><see cref="PageTemplate"/> DTO.</param>
        /// <returns>The status of the operation with a possible error message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage SavePageAsTemplate(PageTemplate pageTemplate)
        {
            if (!this.securityService.CanExportPage(pageTemplate.TabId))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                pageTemplate.Clean();
                var templateFilename = this.templateController.SaveAsTemplate(pageTemplate);
                var response = string.Format(Localization.GetString("ExportedMessage"), templateFilename);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Response = response,
                });
            }
            catch (TemplateException ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Message });
            }
        }

        /// <summary>Makes a localized pages neutral.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>A success status or an exception.</returns>
        /// <example>POST /api/personabar/pages/MakePageNeutral?tabId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage MakePageNeutral([FromUri] int pageId)
        {
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                if (this.tabController.GetTabsByPortal(this.PortalId).WithParentId(pageId).Count > 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, LocalizeString("MakeNeutral.ErrorMessage"));
                }

                var defaultLocale = this.localeController.GetDefaultLocale(this.PortalId);
                this.tabController.ConvertTabToNeutralLanguage(this.PortalId, pageId, defaultLocale.Code, true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to make this page neutral, please consult the logs for more details.";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Makes a neutral page localizable.</summary>
        /// <param name="pageId">the page (tab) id.</param>
        /// <returns>A status code or an error message.</returns>
        /// <example>POST /api/personabar/pages/MakePageTranslatable?tabId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage MakePageTranslatable([FromUri] int pageId)
        {
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                var currentTab = this.tabController.GetTab(pageId, this.PortalId, false);
                if (currentTab == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidTab");
                }

                var defaultLocale = this.localeController.GetDefaultLocale(this.PortalId);
                this.tabController.LocalizeTab(currentTab, defaultLocale, true);
                this.tabController.AddMissingLanguagesWithWarnings(this.PortalId, pageId);
                this.tabController.ClearCache(this.PortalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to make this page translatable.";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Adds all missing languages to a page.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>A status code or an exception message.</returns>
        /// <example>POST /api/personabar/pages/AddMissingLanguages?tabId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage AddMissingLanguages([FromUri] int pageId)
        {
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                bool allLanguagesAdded = this.tabController.AddMissingLanguagesWithWarnings(this.PortalId, pageId);
                this.tabController.ClearCache(this.PortalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, AllLanguagesAdded = allLanguagesAdded });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to add missing languages to this page, consult the logs for more details.";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Notifies the translators with a comment.</summary>
        /// <param name="comment"><see cref="TranslatorsComment"/> DTO.</param>
        /// <returns>A status code and a message.</returns>
        /// <example>POST /api/personabar/pages/NotifyTranslators .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage NotifyTranslators(TranslatorsComment comment)
        {
            try
            {
                if (!this.securityService.CanManagePage(comment.TabId))
                {
                    return this.GetForbiddenResponse();
                }

                // loop through all localized version of this page
                var currentTab = this.tabController.GetTab(comment.TabId, this.PortalId, false);
                foreach (var localizedTab in currentTab.LocalizedTabs.Values)
                {
                    var users = new Dictionary<int, UserInfo>();

                    // Give default translators for this language and administrators permissions
                    this.tabController.GiveTranslatorRoleEditRights(localizedTab, users);

                    // Send Messages to all the translators of new content
                    foreach (var translator in users.Values.Where(user => user.UserID != this.PortalSettings.AdministratorId))
                    {
                        this.AddTranslationSubmittedNotification(localizedTab, translator, comment.Text);
                    }
                }

                var msgToUser = LocalizeString("TranslationMessageConfirmMessage.Text");
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Message = msgToUser });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to notify the translators, please consult the logs for more details.";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>
        /// Gets the view data that used to be in the old ControlBar's localization tab
        /// under Page Settings ( /{page}/ctl/Tab/action/edit/activeTab/settingTab ).
        /// </summary>
        /// <param name="pageId">The ID of the tab to get localization for.</param>
        /// <returns>Information about localiz.</returns>
        /// <example>/api/personabar/pages/GetTabLocalization?pageId=123.</example>
        [HttpGet]
        public HttpResponseMessage GetTabLocalization(int pageId)
        {
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                var currentTab = this.tabController.GetTab(pageId, this.PortalId, false);
                var locales = new List<LocaleInfoDto>();
                var pages = new DnnPagesDto(locales);
                if (!currentTab.IsNeutralCulture)
                {
                    pages = this.GetNonLocalizedPages(pageId);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, pages);
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurrfed trying to get this page localization, consult the logs for more details.";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Updates the page (tab) localization.</summary>
        /// <param name="request"><see cref="DnnPagesRequest"/> DTO.</param>
        /// <returns>A status code or an exception message.</returns>
        /// <example>POST /api/personabar/pages/UpdateTabLocalization .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage UpdateTabLocalization(DnnPagesRequest request)
        {
            try
            {
                if (request.Pages.Any(x => x.TabId > 0 && !this.securityService.CanManagePage(x.TabId)))
                {
                    return this.GetForbiddenResponse();
                }

                this.SaveNonLocalizedPages(request);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred trying to update the page localization, please consult the logs for more details.";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Restores a deleted module on a page (tab).</summary>
        /// <param name="tabModuleId">The TabModule id.</param>
        /// <returns>A success message or an exception message.</returns>
        /// <example>POST /api/personabar/pages/RestoreModule?tabModuleId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage RestoreModule(int tabModuleId)
        {
            try
            {
                var module = ModuleController.Instance.GetTabModule(tabModuleId);
                if (!this.securityService.CanManagePage(module.TabID))
                {
                    return this.GetForbiddenResponse();
                }

                var moduleController = ModuleController.Instance;
                var moduleInfo = moduleController.GetTabModule(tabModuleId);
                if (moduleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidTabModule");
                }

                moduleController.RestoreModule(moduleInfo);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to restore the module onto that page.";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Deletes a module from a page (tab).</summary>
        /// <param name="tabModuleId">The TabModuleId.</param>
        /// <returns>A status message or an error message.</returns>
        /// <example>POST /api/personabar/pages/DeleteModule?tabModuleId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage DeleteModule(int tabModuleId)
        {
            try
            {
                var module = ModuleController.Instance.GetTabModule(tabModuleId);
                if (!this.securityService.CanManagePage(module.TabID))
                {
                    return this.GetForbiddenResponse();
                }

                var moduleController = ModuleController.Instance;
                var moduleInfo = moduleController.GetTabModule(tabModuleId);
                if (moduleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidTabModule");
                }

                moduleController.DeleteTabModule(moduleInfo.TabID, moduleInfo.ModuleID, false);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to delete the module, consult the logs for more details.";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Gets ContentLocalizationEnabled.</summary>
        /// <returns>A value indicating if content localization is enabled.</returns>
        /// <example>GET /api/personabar/pages/GetContentLocalizationEnabled.</example>
        [HttpGet]
        public HttpResponseMessage GetContentLocalizationEnabled()
        {
            try
            {
                if (!TabPermissionController.CanManagePage())
                {
                    return this.GetForbiddenResponse();
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, this.PortalSettings.ContentLocalizationEnabled });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to find if content localization is enabled";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Gets GetCachedItemCount.</summary>
        /// <param name="cacheProvider">The cache profider.</param>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>Caching information.</returns>
        /// <example>GET /api/personabar/pages/GetCachedItemCount.</example>
        [HttpGet]
        public HttpResponseMessage GetCachedItemCount(string cacheProvider, int pageId)
        {
            try
            {
                if (!TabPermissionController.CanManagePage())
                {
                    return this.GetForbiddenResponse();
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Count = OutputCachingProvider.Instance(cacheProvider).GetItemCount(pageId) });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred trying to get the cached items count, please consult the logs for more details.";
                Logger.Error(errorMessage, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Clears the page cache for a given page.</summary>
        /// <param name="cacheProvider">The cache provider to clear the cache for.</param>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>A status code or an error message.</returns>
        /// <example>POST /api/personabar/pages/ClearCache .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage ClearCache([FromUri] string cacheProvider, [FromUri] int pageId)
        {
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                OutputCachingProvider.Instance(cacheProvider).Remove(pageId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while trying to clear the cache for this page, see logs for more details.";
                Logger.Error(message, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets the workflows.</summary>
        /// <returns>List of portal workflows.</returns>
        /// <example>GET /api/personabar/pages/GetWorkflows.</example>
        [HttpGet]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "VIEW_PAGE_LIST,VIEW")]
        public HttpResponseMessage GetWorkflows()
        {
            var workflows = this.workflowManager.GetWorkflows(this.PortalSettings.PortalId).Select(w => new { workflowId = w.WorkflowID, workflowName = w.WorkflowName });
            return this.Request.CreateResponse(HttpStatusCode.OK, workflows);
        }

        private static string LocalizeString(string key)
        {
            return DotNetNuke.Services.Localization.Localization.GetString(key, LocalResourceFile);
        }

        private static void EnableTabVersioningAndWorkflow(TabInfo tab)
        {
            var tabVersionSettings = TabVersionSettings.Instance;
            var tabWorkflowSettings = TabWorkflowSettings.Instance;

            if (tabVersionSettings.IsVersioningEnabled(tab.PortalID))
            {
                tabVersionSettings.SetEnabledVersioningForTab(tab.TabID, true);
            }

            if (tabWorkflowSettings.IsWorkflowEnabled(tab.PortalID))
            {
                tabWorkflowSettings.SetWorkflowEnabled(tab.PortalID, tab.TabID, true);
            }
        }

        private static void DisableTabVersioningAndWorkflow(TabInfo tab)
        {
            var tabVersionSettings = TabVersionSettings.Instance;
            var tabWorkflowSettings = TabWorkflowSettings.Instance;

            if (tabVersionSettings.IsVersioningEnabled(tab.PortalID))
            {
                tabVersionSettings.SetEnabledVersioningForTab(tab.TabID, false);
            }

            if (tabWorkflowSettings.IsWorkflowEnabled(tab.PortalID))
            {
                tabWorkflowSettings.SetWorkflowEnabled(tab.PortalID, tab.TabID, false);
            }
        }

        private DnnPagesDto GetNonLocalizedPages(int tabId)
        {
            var currentTab = this.tabController.GetTab(tabId, this.PortalId, false);

            // Unique id of default language page
            var uniqueId = currentTab.DefaultLanguageGuid != Null.NullGuid
                ? currentTab.DefaultLanguageGuid
                : currentTab.UniqueId;

            // get all non admin pages and not deleted
            var allPages = this.tabController.GetTabsByPortal(this.PortalId).Values.Where(
                t => t.TabID != this.PortalSettings.AdminTabId && (Null.IsNull(t.ParentId) || t.ParentId != this.PortalSettings.AdminTabId));
            allPages = allPages.Where(t => t.IsDeleted == false);

            // get all localized pages of current page
            var tabInfos = allPages as IList<TabInfo> ?? allPages.ToList();
            var localizedPages = tabInfos.Where(
                t => t.DefaultLanguageGuid == uniqueId || t.UniqueId == uniqueId).OrderBy(t => t.DefaultLanguageGuid).ToList();
            Dictionary<string, TabInfo> localizedTabs = null;

            // we are going to build up a list of locales
            // this is a bit more involved, since we want the default language to be first.
            // also, we do not want to add any locales the user has no access to
            var locales = new List<LocaleInfoDto>();
            var localeController = new LocaleController();
            var localeDict = localeController.GetLocales(this.PortalId);
            if (localeDict.Count == 0)
            {
                locales.Add(new LocaleInfoDto(string.Empty));
            }
            else
            {
                if (localizedPages.Count == 1 && localizedPages.First().CultureCode == string.Empty)
                {
                    // locale neutral page
                    locales.Add(new LocaleInfoDto(string.Empty));
                }
                else if (localizedPages.Count == 1 && localizedPages.First().CultureCode != this.PortalSettings.DefaultLanguage)
                {
                    var first = localizedPages.First();
                    locales.Add(new LocaleInfoDto(first.CultureCode));
                }
                else
                {
                    // force sort order, so first add default language
                    locales.Add(new LocaleInfoDto(this.PortalSettings.DefaultLanguage));

                    // build up a list of localized tabs.
                    // depending on whether or not the selected page is in the default langauge
                    // we will add the localized tabs from the current page
                    // or from the defaultlanguage page
                    if (currentTab.CultureCode == this.PortalSettings.DefaultLanguage)
                    {
                        localizedTabs = currentTab.LocalizedTabs;
                    }
                    else
                    {
                        // selected page is not in default language
                        // add localizedtabs from defaultlanguage page
                        if (currentTab.DefaultLanguageTab != null)
                        {
                            localizedTabs = currentTab.DefaultLanguageTab.LocalizedTabs;
                        }
                    }

                    if (localizedTabs != null)
                    {
                        // only add locales from tabs the user has at least view permissions to.
                        // we will handle the edit permissions at a later stage
                        locales.AddRange(
                            from localizedTab in localizedTabs
                            where TabPermissionController.CanViewPage(localizedTab.Value)
                            select new LocaleInfoDto(localizedTab.Value.CultureCode));
                    }
                }
            }

            var dnnPages = new DnnPagesDto(locales)
            {
                HasMissingLanguages = this.tabController.HasMissingLanguages(this.PortalId, tabId),
            };

            // filter the list of localized pages to only those that have a culture we want to see
            var viewableLocalizedPages = localizedPages.Where(
                localizedPage => locales.Find(locale => locale.CultureCode == localizedPage.CultureCode) != null).ToList();

            foreach (var tabInfo in viewableLocalizedPages)
            {
                var localTabInfo = tabInfo;
                var dnnPage = dnnPages.Page(localTabInfo.CultureCode);
                if (!TabPermissionController.CanViewPage(tabInfo))
                {
                    dnnPages.RemoveLocale(localTabInfo.CultureCode);
                    dnnPages.Pages.Remove(dnnPage);
                    break;
                }

                dnnPage.TabId = localTabInfo.TabID;
                dnnPage.TabName = localTabInfo.TabName;
                dnnPage.Title = localTabInfo.Title;
                dnnPage.Description = localTabInfo.Description;
                dnnPage.Path = localTabInfo.TabPath.Substring(0, localTabInfo.TabPath.LastIndexOf("//", StringComparison.Ordinal)).Replace("//", string.Empty);
                dnnPage.HasChildren = this.tabController.GetTabsByPortal(this.PortalId).WithParentId(tabInfo.TabID).Count != 0;
                dnnPage.CanAdminPage = TabPermissionController.CanAdminPage(tabInfo);
                dnnPage.CanViewPage = TabPermissionController.CanViewPage(tabInfo);
                dnnPage.LocalResourceFile = LocalResourceFile;
                dnnPage.PageUrl = this.NavigationManager.NavigateURL(localTabInfo.TabID, false, this.PortalSettings, string.Empty, localTabInfo.CultureCode);
                dnnPage.IsSpecial = TabController.IsSpecialTab(localTabInfo.TabID, localTabInfo.PortalID);

                // calculate position in the form of 1.3.2...
                var siblingTabs = tabInfos.Where(t => (t.ParentId == localTabInfo.ParentId && t.CultureCode == localTabInfo.CultureCode) || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
                dnnPage.Position = (siblingTabs.IndexOf(localTabInfo) + 1).ToString(CultureInfo.InvariantCulture);
                var parentTabId = localTabInfo.ParentId;
                while (parentTabId > 0)
                {
                    var parentTab = tabInfos.Single(t => t.TabID == parentTabId);
                    var id = parentTabId;
                    siblingTabs = tabInfos.Where(t => (t.ParentId == id && t.CultureCode == localTabInfo.CultureCode) || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
                    dnnPage.Position = (siblingTabs.IndexOf(localTabInfo) + 1).ToString(CultureInfo.InvariantCulture) + "." + dnnPage.Position;
                    parentTabId = parentTab.ParentId;
                }

                dnnPage.DefaultLanguageGuid = localTabInfo.DefaultLanguageGuid;
                dnnPage.IsTranslated = localTabInfo.IsTranslated;
                dnnPage.IsPublished = this.tabController.IsTabPublished(localTabInfo);

                // generate modules information
                var moduleController = ModuleController.Instance;
                foreach (var moduleInfo in moduleController.GetTabModules(localTabInfo.TabID).Values)
                {
                    var guid = moduleInfo.DefaultLanguageGuid == Null.NullGuid ? moduleInfo.UniqueId : moduleInfo.DefaultLanguageGuid;

                    var dnnModules = dnnPages.Module(guid); // modules of each language
                    var dnnModule = dnnModules.Module(localTabInfo.CultureCode);

                    // detect error : 2 modules with same uniqueId on the same page
                    dnnModule.LocalResourceFile = LocalResourceFile;
                    if (dnnModule.TabModuleId > 0)
                    {
                        dnnModule.ErrorDuplicateModule = true;
                        dnnPages.ErrorExists = true;
                        continue;
                    }

                    dnnModule.ModuleTitle = moduleInfo.ModuleTitle;
                    dnnModule.DefaultLanguageGuid = moduleInfo.DefaultLanguageGuid;
                    dnnModule.TabId = localTabInfo.TabID;
                    dnnModule.TabModuleId = moduleInfo.TabModuleID;
                    dnnModule.ModuleId = moduleInfo.ModuleID;
                    dnnModule.CanAdminModule = ModulePermissionController.CanAdminModule(moduleInfo);
                    dnnModule.CanViewModule = ModulePermissionController.CanViewModule(moduleInfo);
                    dnnModule.IsDeleted = moduleInfo.IsDeleted;
                    if (moduleInfo.DefaultLanguageGuid != Null.NullGuid)
                    {
                        var defaultLanguageModule = moduleController.GetModuleByUniqueID(moduleInfo.DefaultLanguageGuid);
                        if (defaultLanguageModule != null)
                        {
                            dnnModule.DefaultModuleId = defaultLanguageModule.ModuleID;
                            if (defaultLanguageModule.ParentTab.UniqueId != moduleInfo.ParentTab.DefaultLanguageGuid)
                            {
                                dnnModule.DefaultTabName = defaultLanguageModule.ParentTab.TabName;
                            }
                        }
                    }

                    dnnModule.SetModuleInfoHelp();
                    dnnModule.IsTranslated = moduleInfo.IsTranslated;
                    dnnModule.IsLocalized = moduleInfo.IsLocalized;

                    dnnModule.IsShared = this.tabController.GetTabsByModuleID(moduleInfo.ModuleID).Values.Count(t => t.CultureCode == moduleInfo.CultureCode) > 1;

                    // detect error : the default language module is on an other page
                    dnnModule.ErrorDefaultOnOtherTab = moduleInfo.DefaultLanguageGuid != Null.NullGuid && moduleInfo.DefaultLanguageModule == null;

                    // detect error : different culture on tab and module
                    dnnModule.ErrorCultureOfModuleNotCultureOfTab = moduleInfo.CultureCode != localTabInfo.CultureCode;

                    dnnPages.ErrorExists |= dnnModule.ErrorDefaultOnOtherTab || dnnModule.ErrorCultureOfModuleNotCultureOfTab;
                }
            }

            return dnnPages;
        }

        private void SaveNonLocalizedPages(DnnPagesRequest pages)
        {
            // check all pages
            foreach (var page in pages.Pages)
            {
                var tabInfo = this.tabController.GetTab(page.TabId, this.PortalId, true);
                if (tabInfo != null &&
                    (tabInfo.TabName != page.TabName ||
                     tabInfo.Title != page.Title ||
                     tabInfo.Description != page.Description))
                {
                    tabInfo.TabName = page.TabName;
                    tabInfo.Title = page.Title;
                    tabInfo.Description = page.Description;
                    this.tabController.UpdateTab(tabInfo);
                }
            }

            var tabsToPublish = new List<TabInfo>();
            var moduleTranslateOverrides = new Dictionary<int, bool>();
            var moduleController = ModuleController.Instance;

            // manage all actions we need to take for all modules on all pages
            foreach (var modulesCollection in pages.Modules)
            {
                foreach (var moduleDto in modulesCollection.Modules)
                {
                    var tabModule = moduleController.GetTabModule(moduleDto.TabModuleId);
                    if (tabModule != null)
                    {
                        if (tabModule.ModuleTitle != moduleDto.ModuleTitle)
                        {
                            tabModule.ModuleTitle = moduleDto.ModuleTitle;
                            moduleController.UpdateModule(tabModule);
                        }

                        if (tabModule.DefaultLanguageGuid != Null.NullGuid &&
                            tabModule.IsLocalized != moduleDto.IsLocalized)
                        {
                            var locale = this.localeController.GetLocale(tabModule.CultureCode);
                            if (moduleDto.IsLocalized)
                            {
                                moduleController.LocalizeModule(tabModule, locale);
                            }
                            else
                            {
                                moduleController.DeLocalizeModule(tabModule);
                            }
                        }

                        bool moduleTranslateOverride;
                        moduleTranslateOverrides.TryGetValue(tabModule.TabID, out moduleTranslateOverride);

                        if (!moduleTranslateOverride && tabModule.IsTranslated != moduleDto.IsTranslated)
                        {
                            moduleController.UpdateTranslationStatus(tabModule, moduleDto.IsTranslated);
                        }
                    }
                    else if (moduleDto.CopyModule)
                    {
                        // find the first existing module on the line
                        foreach (var moduleToCopyFrom in modulesCollection.Modules)
                        {
                            if (moduleToCopyFrom.ModuleId > 0)
                            {
                                var toTabInfo = this.GetLocalizedTab(moduleToCopyFrom.TabId, moduleDto.CultureCode);
                                var miCopy = moduleController.GetTabModule(moduleToCopyFrom.TabModuleId);
                                if (miCopy.DefaultLanguageGuid == Null.NullGuid)
                                {
                                    // default
                                    DisableTabVersioningAndWorkflow(toTabInfo);
                                    moduleController.CopyModule(miCopy, toTabInfo, Null.NullString, true);
                                    EnableTabVersioningAndWorkflow(toTabInfo);
                                    var localizedModule = moduleController.GetModule(miCopy.ModuleID, toTabInfo.TabID, false);
                                    moduleController.LocalizeModule(localizedModule, LocaleController.Instance.GetLocale(localizedModule.CultureCode));
                                }
                                else
                                {
                                    var miCopyDefault = moduleController.GetModuleByUniqueID(miCopy.DefaultLanguageGuid);
                                    moduleController.CopyModule(miCopyDefault, toTabInfo, Null.NullString, true);
                                }

                                if (moduleDto == modulesCollection.Modules.First())
                                {
                                    // default language
                                    var miDefault = moduleController.GetModule(miCopy.ModuleID, pages.Pages.First().TabId, false);
                                    foreach (var page in pages.Pages.Skip(1))
                                    {
                                        var moduleInfo = moduleController.GetModule(miCopy.ModuleID, page.TabId, false);
                                        if (moduleInfo != null)
                                        {
                                            if (miDefault != null)
                                            {
                                                moduleInfo.DefaultLanguageGuid = miDefault.UniqueId;
                                            }

                                            moduleController.UpdateModule(moduleInfo);
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }

            foreach (var page in pages.Pages)
            {
                var tabInfo = this.tabController.GetTab(page.TabId, this.PortalId, true);
                if (tabInfo != null)
                {
                    var moduleTranslateOverride = false;
                    if (!tabInfo.IsDefaultLanguage)
                    {
                        if (tabInfo.IsTranslated != page.IsTranslated)
                        {
                            this.tabController.UpdateTranslationStatus(tabInfo, page.IsTranslated);
                            if (page.IsTranslated)
                            {
                                moduleTranslateOverride = true;
                                var tabModules = moduleController.GetTabModules(tabInfo.TabID)
                                    .Where(moduleKvp =>
                                        moduleKvp.Value.DefaultLanguageModule != null &&
                                        moduleKvp.Value.LocalizedVersionGuid != moduleKvp.Value.DefaultLanguageModule.LocalizedVersionGuid);

                                foreach (var moduleKvp in tabModules)
                                {
                                    moduleController.UpdateTranslationStatus(moduleKvp.Value, true);
                                }
                            }
                        }

                        if (page.IsPublished)
                        {
                            tabsToPublish.Add(tabInfo);
                        }
                    }

                    moduleTranslateOverrides.Add(page.TabId, moduleTranslateOverride);
                }
            }

            // if we have tabs to publish, do it.
            // marks all modules as translated, marks page as translated
            foreach (var tabInfo in tabsToPublish)
            {
                // First mark all modules as translated
                foreach (var module in moduleController.GetTabModules(tabInfo.TabID).Values)
                {
                    moduleController.UpdateTranslationStatus(module, true);
                }

                // Second mark tab as translated
                this.tabController.UpdateTranslationStatus(tabInfo, true);

                // Third publish Tab (update Permissions)
                this.tabController.PublishTab(tabInfo);
            }

            // manage translated status of tab. In order to do that, we need to check if all modules on the page are translated
            var tabTranslatedStatus = true;
            foreach (var page in pages.Pages)
            {
                var tabInfo = this.tabController.GetTab(page.TabId, this.PortalId, true);
                if (tabInfo != null)
                {
                    if (tabInfo.ChildModules.Any(moduleKvp => !moduleKvp.Value.IsTranslated))
                    {
                        tabTranslatedStatus = false;
                    }

                    if (tabTranslatedStatus && !tabInfo.IsTranslated)
                    {
                        this.tabController.UpdateTranslationStatus(tabInfo, true);
                    }
                }
            }
        }

        private void AddTranslationSubmittedNotification(TabInfo tabInfo, UserInfo translator, string comment)
        {
            var notificationsController = NotificationsController.Instance;
            var notificationType = notificationsController.GetNotificationType("TranslationSubmitted");
            var subject = LocalizeString("NewContentMessage.Subject");
            var body = string.Format(
                LocalizeString("NewContentMessage.Body"),
                tabInfo.TabName,
                this.NavigationManager.NavigateURL(tabInfo.TabID, false, this.PortalSettings, Null.NullString, tabInfo.CultureCode),
                comment);

            var sender = UserController.GetUserById(this.PortalSettings.PortalId, this.PortalSettings.AdministratorId);
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = true,
                SenderUserID = sender.UserID,
            };

            notificationsController.SendNotification(notification, this.PortalSettings.PortalId, null, new List<UserInfo> { translator });
        }

        private HttpResponseMessage GetForbiddenResponse()
        {
            return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "The user is not allowed to access this method." });
        }

        private ThemeFileInfo GetDefaultPortalTheme()
        {
            var layoutSrc = this.defaultPortalThemeController.GetDefaultPortalLayout();
            if (string.IsNullOrWhiteSpace(layoutSrc))
            {
                return null;
            }

            var layout = this.themesController.GetThemeFile(PortalSettings.Current, layoutSrc, ThemeType.Skin);
            return layout;
        }

        private TabInfo GetLocalizedTab(int tabId, string cultureCode)
        {
            var currentTab = this.tabController.GetTab(tabId, this.PortalId, false);

            // Unique id of default language page
            var uniqueId = currentTab.DefaultLanguageGuid != Null.NullGuid
                ? currentTab.DefaultLanguageGuid
                : currentTab.UniqueId;

            // get all non admin pages and not deleted
            var allPages = this.tabController.GetTabsByPortal(this.PortalId).Values.Where(
                t => t.TabID != this.PortalSettings.AdminTabId && (Null.IsNull(t.ParentId) || t.ParentId != this.PortalSettings.AdminTabId));
            allPages = allPages.Where(t => t.IsDeleted == false);

            // get all localized pages of current page
            var tabInfos = allPages as IList<TabInfo> ?? allPages.ToList();
            return tabInfos.SingleOrDefault(t => (t.DefaultLanguageGuid == uniqueId || t.UniqueId == uniqueId) && t.CultureCode == cultureCode);
        }
    }
}

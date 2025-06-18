// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Library.Dto.Tabs;

    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;

    /// <summary>A Persona Bar API controller for pages.</summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class TabsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabsController));
        private readonly Library.Controllers.TabsController controller = new Library.Controllers.TabsController();

        /// <summary>Gets the local resource file path.</summary>
        public string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx");

        /// GET: api/Tabs/GetPortalTabs
        /// <summary>Gets list of portal tabs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="isMultiLanguage">Whether it's multi-language.</param>
        /// <param name="excludeAdminTabs">Whether to exclude admin tabs.</param>
        /// <param name="roles">A semicolon-delimited list of role names by which to filter the results.</param>
        /// <param name="disabledNotSelectable">If disabled pages should not be selectable.</param>
        /// <param name="sortOrder">1 to sort A-Z, 2 to sort Z-A, any other value to not sort.</param>
        /// <param name="selectedTabId">Currently Selected tab ID.</param>
        /// <param name="validateTab">The friendly name of a module to ensure is on the page.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeDeleted">Whether to include deleted pages.</param>
        /// <param name="includeDeletedChildren">The value of this parameter affects <see cref="TabInfo.HasChildren"></see> property.</param>
        /// <returns>List of portal tabs.</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalTabs(int portalId, string cultureCode, bool isMultiLanguage = false, bool excludeAdminTabs = true, string roles = "", bool disabledNotSelectable = false, int sortOrder = 0, int selectedTabId = -1, string validateTab = "", bool includeHostPages = false, bool includeDisabled = false, bool includeDeleted = false, bool includeDeletedChildren = true)
        {
            try
            {
                if (!this.UserInfo.IsSuperUser && portalId != this.PortalId && portalId > -1)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, Localization.GetString("UnauthorizedRequest", this.LocalResourcesFile));
                }

                var response = new
                {
                    Success = true,
                    Results =
                        this.controller.GetPortalTabs(
                            this.UserInfo,
                            portalId < 0 ? this.PortalId : portalId,
                            cultureCode,
                            isMultiLanguage,
                            excludeAdminTabs,
                            roles,
                            disabledNotSelectable,
                            sortOrder,
                            selectedTabId,
                            validateTab,
                            includeHostPages,
                            includeDisabled,
                            includeDeleted,
                            includeDeletedChildren),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>A web API action to get pages search results.</summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="roles">A semicolon-delimited list of role names by which to filter the results.</param>
        /// <param name="disabledNotSelectable">Whether disabled tabs should be selectable.</param>
        /// <param name="sortOrder">1 for A-Z, 2 for Z-A, any other value to not sort the results.</param>
        /// <param name="validateTab">The friendly name of a module which must be on the page.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeDeleted">WHether to include deleted pages.</param>
        /// <returns>A response with a <see cref="TabDto"/> <c>Results</c> field.</returns>
        [HttpGet]
        public HttpResponseMessage SearchPortalTabs(string searchText, int portalId, string roles = "", bool disabledNotSelectable = false, int sortOrder = 0, string validateTab = "", bool includeHostPages = false, bool includeDisabled = false, bool includeDeleted = false)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results =
                        this.controller.SearchPortalTabs(this.UserInfo, searchText, portalId < 0 ? this.PortalId : portalId, roles, disabledNotSelectable, sortOrder, validateTab, includeHostPages, includeDisabled, includeDeleted),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Tabs/GetPortalTab
        /// <summary>Gets list of portal tabs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>List of portal tabs.</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalTab(int portalId, int tabId, string cultureCode)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results = this.controller.GetTabByCulture(tabId, portalId < 0 ? this.PortalId : portalId, cultureCode),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>A web API action to get the descendants of a tab.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="parentId">The parent ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="isMultiLanguage">Whether it's multi-language.</param>
        /// <param name="roles">A semicolon-delimited list of role names.</param>
        /// <param name="disabledNotSelectable">Whether disabled pages should be selectable.</param>
        /// <param name="sortOrder">1 for A-Z, 2 for Z-A, or any other value to not sort the results.</param>
        /// <param name="validateTab">The friendly name of module that must be on the page.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeDeletedChildren">The value of this parameter affects <see cref="TabInfo.HasChildren"></see> property.</param>
        /// <returns>A response with <see cref="TabDto"/> <c>Results</c> collection.</returns>
        [HttpGet]
        public HttpResponseMessage GetTabsDescendants(int portalId, int parentId, string cultureCode, bool isMultiLanguage = false, string roles = "", bool disabledNotSelectable = false, int sortOrder = 0, string validateTab = "", bool includeHostPages = false, bool includeDisabled = false, bool includeDeletedChildren = true)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results =
                        this.controller.GetTabsDescendants(
                            portalId < 0 ? this.PortalId : portalId,
                            parentId,
                            cultureCode,
                            isMultiLanguage,
                            roles,
                            disabledNotSelectable,
                            sortOrder,
                            validateTab,
                            includeHostPages,
                            includeDisabled,
                            includeDeletedChildren),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}

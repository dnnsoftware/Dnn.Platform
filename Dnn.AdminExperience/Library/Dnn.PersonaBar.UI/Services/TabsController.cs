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
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;

    [MenuPermission(Scope = ServiceScope.Regular)]
    public class TabsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabsController));
        private readonly Library.Controllers.TabsController _controller = new Library.Controllers.TabsController();

        public string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx");

        /// GET: api/Tabs/GetPortalTabs
        /// <summary>
        /// Gets list of portal tabs.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="isMultiLanguage"></param>
        /// <param name="excludeAdminTabs"></param>
        /// <param name="roles"></param>
        /// <param name="disabledNotSelectable"></param>
        /// <param name="sortOrder"></param>
        /// <param name="selectedTabId">Currently Selected tab id.</param>
        /// <param name="validateTab"></param>
        /// <param name="includeHostPages"></param>
        /// <param name="includeDisabled"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="includeDeletedChildren"></param>
        /// <returns>List of portal tabs.</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalTabs(int portalId, string cultureCode, bool isMultiLanguage = false,
            bool excludeAdminTabs = true, string roles = "", bool disabledNotSelectable = false, int sortOrder = 0, int selectedTabId = -1, string validateTab = "", bool includeHostPages = false, bool includeDisabled = false, bool includeDeleted = false, bool includeDeletedChildren = true)
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
                        this._controller.GetPortalTabs(this.UserInfo, portalId < 0 ? this.PortalId : portalId, cultureCode, isMultiLanguage,
                            excludeAdminTabs, roles,
                            disabledNotSelectable, sortOrder, selectedTabId, validateTab, includeHostPages, includeDisabled, includeDeleted, includeDeletedChildren),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="portalId"></param>
        /// <param name="roles"></param>
        /// <param name="disabledNotSelectable"></param>
        /// <param name="sortOrder"></param>
        /// <param name="validateTab"></param>
        /// <param name="includeHostPages"></param>
        /// <param name="includeDisabled"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage SearchPortalTabs(string searchText, int portalId, string roles = "", bool disabledNotSelectable = false, int sortOrder = 0, string validateTab = "", bool includeHostPages = false, bool includeDisabled = false, bool includeDeleted = false)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results =
                        this._controller.SearchPortalTabs(this.UserInfo, searchText, portalId < 0 ? this.PortalId : portalId, roles, disabledNotSelectable, sortOrder, validateTab, includeHostPages, includeDisabled, includeDeleted),
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
        /// <summary>
        /// Gets list of portal tabs.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <param name="cultureCode"></param>
        /// <returns>List of portal tabs.</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalTab(int portalId, int tabId, string cultureCode)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results = this._controller.GetTabByCulture(tabId, portalId < 0 ? this.PortalId : portalId, cultureCode),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="parentId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="isMultiLanguage"></param>
        /// <param name="roles"></param>
        /// <param name="disabledNotSelectable"></param>
        /// <param name="sortOrder"></param>
        /// <param name="validateTab"></param>
        /// <param name="includeHostPages"></param>
        /// <param name="includeDisabled"></param>
        /// <param name="includeDeletedChildren"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTabsDescendants(int portalId, int parentId, string cultureCode,
            bool isMultiLanguage = false, string roles = "", bool disabledNotSelectable = false, int sortOrder = 0, string validateTab = "", bool includeHostPages = false, bool includeDisabled = false, bool includeDeletedChildren = true)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results =
                        this._controller.GetTabsDescendants(portalId < 0 ? this.PortalId : portalId, parentId, cultureCode, isMultiLanguage, roles,
                            disabledNotSelectable, sortOrder, validateTab, includeHostPages, includeDisabled, includeDeletedChildren),
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

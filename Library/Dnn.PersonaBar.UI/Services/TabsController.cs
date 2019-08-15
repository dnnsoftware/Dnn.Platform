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
using System.IO;
using System.Net;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.UI.Services
{
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class TabsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (TabsController));
        private readonly Library.Controllers.TabsController _controller = new Library.Controllers.TabsController();
        public string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx");

        /// GET: api/Tabs/GetPortalTabs
        /// <summary>
        /// Gets list of portal tabs
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="isMultiLanguage"></param>
        /// <param name="excludeAdminTabs"></param>
        /// <param name="roles"></param>
        /// <param name="disabledNotSelectable"></param>
        /// <param name="sortOrder"></param>
        /// <param name="selectedTabId">Currently Selected tab id</param>
        /// <param name="validateTab"></param>
        /// <param name="includeHostPages"></param>
        /// <param name="includeDisabled"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="includeDeletedChildren"></param>
        /// <returns>List of portal tabs</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalTabs(int portalId, string cultureCode, bool isMultiLanguage = false,
            bool excludeAdminTabs = true, string roles = "", bool disabledNotSelectable = false, int sortOrder = 0, int selectedTabId = -1, string validateTab="", bool includeHostPages = false, bool includeDisabled = false, bool includeDeleted = false, bool includeDeletedChildren = true)
        {
            try
            {
                if (!UserInfo.IsSuperUser && portalId != PortalId && portalId > -1)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, Localization.GetString("UnauthorizedRequest", LocalResourcesFile));
                }

                var response = new
                {
                    Success = true,
                    Results =
                        _controller.GetPortalTabs(UserInfo, portalId < 0 ? PortalId : portalId, cultureCode, isMultiLanguage,
                            excludeAdminTabs, roles,
                            disabledNotSelectable, sortOrder, selectedTabId, validateTab, includeHostPages, includeDisabled, includeDeleted, includeDeletedChildren)
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
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
                        _controller.SearchPortalTabs(UserInfo, searchText, portalId < 0 ? PortalId : portalId, roles, disabledNotSelectable, sortOrder, validateTab, includeHostPages, includeDisabled, includeDeleted)
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Tabs/GetPortalTab
        /// <summary>
        /// Gets list of portal tabs
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <param name="cultureCode"></param>
        /// <returns>List of portal tabs</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalTab(int portalId, int tabId, string cultureCode)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results = _controller.GetTabByCulture(tabId, portalId < 0 ? PortalId : portalId, cultureCode)
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
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
                        _controller.GetTabsDescendants(portalId < 0 ? PortalId : portalId, parentId, cultureCode, isMultiLanguage, roles,
                            disabledNotSelectable, sortOrder, validateTab, includeHostPages, includeDisabled, includeDeletedChildren)
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
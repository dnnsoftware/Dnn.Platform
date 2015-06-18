// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentManager.Services.ViewModels;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentManager.Services
{
    /// <summary>
    /// TemplateController provides the Web Services to manage Templates
    /// </summary>
    [SupportedModules("Dnn.DynamicContentManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class TemplateController : DnnApiController
    {
        /// <summary>
        /// GetTemplates retrieves a page of Templates that satisfy the searchTerm
        /// </summary>
        /// <param name="searchTerm">The text to use to search for Templates</param>
        /// <param name="pageIndex">The page to return, where 0 is the first page</param>
        /// <param name="pageSize">The size of the page</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTemplates(string searchTerm, int pageIndex, int pageSize)
        {
            var templateList = ContentTemplateManager.Instance.GetContentTemplates(searchTerm, PortalSettings.PortalId, pageIndex, pageSize, true);

            var templates = templateList
                                .Select(template => new TemplateViewModel(template, PortalSettings.UserInfo.IsSuperUser))
                                .ToList();

            var response = new
            {
                success = true,
                data = new
                        {
                            results = templates,
                            totalResults = templateList.TotalCount
                        }
            };

            return Request.CreateResponse(response);
        }

    }
}
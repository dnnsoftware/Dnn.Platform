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
    /// DataTypeController provides the Web Services to manage Data Types
    /// </summary>
    [SupportedModules("Dnn.DynamicContentManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class DataTypeController : DnnApiController
    {
        /// <summary>
        /// GetDataTypes retrieves a page of DataTypes that satisfy the searchTerm
        /// </summary>
        /// <param name="searchTerm">The text to use to search for DataTypes</param>
        /// <param name="pageIndex">The page to return, where 0 is the first page</param>
        /// <param name="pageSize">The size of the page</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDataTypes(string searchTerm, int pageIndex, int pageSize)
        {
            var dataTypeList = DataTypeManager.Instance.GetDataTypes(searchTerm, PortalSettings.PortalId, pageIndex, pageSize, true);
            var dataTypes = dataTypeList
                            .Select(dataType => new DataTypeViewModel(dataType))
                            .ToList();

            var response = new
                            {
                                success = true,
                                data = new
                                        {
                                            results = dataTypes,
                                            totalResults = dataTypeList.TotalCount
                                        }
                            };

            return Request.CreateResponse(response);
        }
    }
}

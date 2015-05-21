// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentManager.Services.ViewModels;
using DotNetNuke.Common.Utilities;
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
        /// DeleteDataType deletes a single DataType
        /// </summary>
        /// <param name="viewModel">The Data Type to delete</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage DeleteDataType(DataTypeViewModel viewModel)
        {
            var dataType = DataTypeManager.Instance.GetDataType(viewModel.DataTypeId, PortalSettings.PortalId, true);

            if (dataType != null)
            {
                DataTypeManager.Instance.DeleteDataType(dataType);
            }

            var response = new
                            {
                                success = true
                            };

            return Request.CreateResponse(response);

        }

        /// <summary>
        /// GetDataType retrieves a single DataType
        /// </summary>
        /// <param name="dataTypeId">The id of the DataType</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDataType(int dataTypeId)
        {
            var dataType = DataTypeManager.Instance.GetDataType(dataTypeId, PortalSettings.PortalId, true);

            var response = new
                            {
                                success = true,
                                data = new
                                            {
                                                dataType = new DataTypeViewModel(dataType, PortalSettings.UserInfo.IsSuperUser)
                                }
                            };

            return Request.CreateResponse(response);
        }

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
                            .Select(dataType => new DataTypeViewModel(dataType, PortalSettings.UserInfo.IsSuperUser))
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

        /// <summary>
        /// SaveDataType saves the data type
        /// </summary>
        /// <param name="viewModel">The ViewModel for the DataType to save</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SaveDataType(DataTypeViewModel viewModel)
        {
            if (viewModel.DataTypeId == -1)
            {
                var dataType = new DataType
                {
                    Name = viewModel.Name,
                    UnderlyingDataType = viewModel.BaseType,
                    PortalId = viewModel.IsSystem ? -1 : PortalSettings.PortalId
                };
                DataTypeManager.Instance.AddDataType(dataType);
            }
            else
            {
                //Update
                var dataType = DataTypeManager.Instance.GetDataType(viewModel.DataTypeId, PortalSettings.PortalId, true);

                if (dataType != null)
                {
                    dataType.Name = viewModel.Name;
                    dataType.UnderlyingDataType = viewModel.BaseType;
                    DataTypeManager.Instance.UpdateDataType(dataType);
                }
            }
            var response = new
                            {
                                success = true
                            };

            return Request.CreateResponse(response);
        }
    }
}

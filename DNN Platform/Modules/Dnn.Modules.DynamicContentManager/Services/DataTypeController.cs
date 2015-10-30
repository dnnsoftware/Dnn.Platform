// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Localization;
using Dnn.Modules.DynamicContentManager.Components.Entities;
using Dnn.Modules.DynamicContentManager.Services.ViewModels;
using DotNetNuke.Security;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentManager.Services
{
    /// <summary>
    /// DataTypeController provides the Web Services to manage Data Types
    /// </summary>
    [SupportedModules("Dnn.DynamicContentManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class DataTypeController : BaseController
    {
        /// <summary>
        /// DeleteDataType deletes a single DataType
        /// </summary>
        /// <param name="viewModel">The Data Type to delete</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteDataType(DataTypeViewModel viewModel)
        {
             return DeleteEntity(() => DataTypeManager.Instance.GetDataType(viewModel.DataTypeId, PortalSettings.PortalId, true),
                                 dataType => DataTypeManager.Instance.DeleteDataType(dataType));
        }

        /// <summary>
        /// GetDataType retrieves a single DataType
        /// </summary>
        /// <param name="dataTypeId">The id of the DataType</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDataType(int dataTypeId)
        {
            return GetEntity(() => DataTypeManager.Instance.GetDataType(dataTypeId, PortalSettings.PortalId, true),
                           dataType => new DataTypeViewModel(dataType, PortalSettings));
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
            var settings = (DCCSettings)Personalization.GetProfile("DCC", "UserSettings" + PortalSettings.PortalId + ActiveModule.ModuleID) ?? GetDefaultSettings();
            settings.DataTypePageSize = pageSize;            
            UpdateUserDccSettings(settings, ActiveModule.ModuleID);
            return GetPage(() => DataTypeManager.Instance.GetDataTypes(searchTerm, PortalSettings.PortalId, pageIndex, pageSize, true),
                           dataType => new DataTypeViewModel(dataType, PortalSettings));
        }

        /// <summary>
        /// SaveDataType saves the data type
        /// </summary>
        /// <param name="viewModel">The ViewModel for the DataType to save</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveDataType(DataTypeViewModel viewModel)
        {
            var dataTypeId = viewModel.DataTypeId;
            var portalId = viewModel.IsSystem ? -1 : PortalSettings.PortalId;
            var localizedNames = new List<ContentTypeLocalization>();
            string defaultName = ParseLocalizations(viewModel.LocalizedNames, localizedNames, portalId);

            return SaveEntity(dataTypeId,
                /*CheckEntity*/ () => DataTypeManager.Instance.GetDataTypes(portalId, true).SingleOrDefault((t => t.Name == defaultName)),

                /*ErrorMsg*/    LocalizeString("DataTypeExists"),

                /*CreateEntity*/() => new DataType
                                            {
                                                Name = defaultName,
                                                UnderlyingDataType = viewModel.BaseType,
                                                PortalId = portalId
                                            },

                /*AddEntity*/   dataType => DataTypeManager.Instance.AddDataType(dataType),

                /*GetEntity*/   () => DataTypeManager.Instance.GetDataType(dataTypeId, PortalSettings.PortalId, true),

                /*UpdateEntity*/dataType =>
                                            {
                                                dataType.Name = defaultName;
                                                dataType.UnderlyingDataType = viewModel.BaseType;
                                                DataTypeManager.Instance.UpdateDataType(dataType);
                                            },

                /*SaveLocal*/   id => SaveContentLocalizations(localizedNames, DataTypeManager.NameKey, id, portalId));
        }
    }
}

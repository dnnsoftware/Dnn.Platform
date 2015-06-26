// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Dnn.DynamicContent;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// DataTypeViewModel represents a Data Type object within the DataType Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DataTypeViewModel : BaseViewModel
    {
        /// <summary>
        /// Constructs a DataTypeViewModel
        /// </summary>
        public DataTypeViewModel()
        {
            
        }

        /// <summary>
        /// Constructs a DataTypeViewModel from a DataType object
        /// </summary>
        /// <param name="dataType">The DataType object</param>
        /// <param name="portalSettings">The portal Settings for the current portal</param>
        public DataTypeViewModel(DataType dataType, PortalSettings portalSettings)
        {
            BaseType = dataType.UnderlyingDataType;
            Created = dataType.CreatedOnDate.ToShortDateString();
            DataTypeId = dataType.DataTypeId;
            IsSystem = dataType.IsSystem;
            CanEdit = !(IsSystem) || portalSettings.UserInfo.IsSuperUser;

            LocalizedNames = GetLocalizedValues(dataType.Name, DataTypeManager.DataTypeNameKey, DataTypeId, dataType.PortalId, portalSettings);
        }

        /// <summary>
        /// The base Data Type of the Data Type
        /// </summary>
        [JsonProperty("baseType")]
        public UnderlyingDataType BaseType { get; set; }

        /// <summary>
        /// A flag that determines if the current user can edit the object
        /// </summary>
        [JsonProperty("canEdit")]
        public bool CanEdit { get; set; }

        /// <summary>
        /// The Date when the Data Type was created
        /// </summary>
        [JsonProperty("created")]
        public string Created { get; set; }

        /// <summary>
        /// The Id of the Data Type
        /// </summary>
        [JsonProperty("dataTypeId")]
        public int DataTypeId { get; set; }

        /// <summary>
        /// The name of the Data Type
        /// </summary>
        [JsonProperty("isSystem")]
        public bool IsSystem { get; set; }

        /// <summary>
        /// A List of localized values for the Name property
        /// </summary>
        [JsonProperty("localizedNames")]
        public List<dynamic> LocalizedNames { get; set; }
    }
}

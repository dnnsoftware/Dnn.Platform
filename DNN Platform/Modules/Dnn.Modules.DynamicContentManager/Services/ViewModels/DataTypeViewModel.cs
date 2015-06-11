// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// DataTypeViewModel represents a Data Type object within the DataType Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DataTypeViewModel
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
        /// <param name="isSuperUser">A flag that indicates the user is a Superuser</param>
        public DataTypeViewModel(DataType dataType, bool isSuperUser)
        {
            BaseType = dataType.UnderlyingDataType;
            Created = dataType.CreatedOnDate.ToShortDateString();
            DataTypeId = dataType.DataTypeId;
            IsSystem = dataType.IsSystem;
            Name = dataType.Name;
            CanEdit = !(IsSystem) || isSuperUser;
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
        /// The name of the Data Type
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

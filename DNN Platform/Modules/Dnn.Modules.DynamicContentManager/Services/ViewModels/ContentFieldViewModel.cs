// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// ContentFieldViewModel represents a Content Field object within the ContentType Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContentFieldViewModel
    {
        /// <summary>
        /// Constructs a ContentFieldViewModel
        /// </summary>
        public ContentFieldViewModel()
        {

        }


        /// <summary>
        /// Constructs a ContentFieldViewModel from a FieldDefinition object
        /// </summary>
        /// <param name="definition">The field Definition to use</param>
        public ContentFieldViewModel(FieldDefinition definition)
        {
            ContentFieldId = definition.FieldDefinitionId;
            ContentTypeId = definition.ContentTypeId;
            DataTypeId = definition.DataTypeId;
            DataType = definition.DataType.Name;
            Description = definition.Description;
            Label = definition.Label;
            Name = definition.Name;
        }

        /// <summary>
        /// The id of the Content Field
        /// </summary>
        [JsonProperty("contentFieldId")]
        public int ContentFieldId { get; set; }

        /// <summary>
        /// The id of the parent Content Type
        /// </summary>
        [JsonProperty("contentTypeId")]
        public int ContentTypeId { get; set; }

        /// <summary>
        /// The data type of the Content Field
        /// </summary>
        [JsonProperty("dataType")]
        public string DataType { get; set; }

        /// <summary>
        /// The id of the data type of the Content Field
        /// </summary>
        [JsonProperty("dataTypeId")]
        public int DataTypeId { get; set; }

        /// <summary>
        /// The description of the Content Field
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The label of the Content Field
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// The name of the Content Field
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
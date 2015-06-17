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
        /// Constructs a ContentFieldViewModel from a FieldDefinition object
        /// </summary>
        /// <param name="definition">The field Definition to use</param>
        public ContentFieldViewModel(FieldDefinition definition)
        {
            DataType = definition.DataType.Name;
            Label = definition.Label;
            Name = definition.Name;
        }

        /// <summary>
        /// The data type of the Content Field
        /// </summary>
        [JsonProperty("dataType")]
        public string DataType { get; set; }

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
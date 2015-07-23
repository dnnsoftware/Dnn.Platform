﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Dnn.DynamicContent;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// ContentFieldViewModel represents a Content Field object within the ContentType Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContentFieldViewModel : BaseViewModel
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
        public ContentFieldViewModel(FieldDefinition definition, PortalSettings portalSettings)
        {
            ContentFieldId = definition.FieldDefinitionId;
            ContentTypeId = definition.ContentTypeId;
            DataTypeId = definition.DataTypeId;
            DataType = definition.DataType.Name;

            LocalizedDescriptions = GetLocalizedValues(definition.Description, FieldDefinitionManager.DescriptionKey, ContentFieldId, definition.PortalId, portalSettings);
            LocalizedLabels = GetLocalizedValues(definition.Label, FieldDefinitionManager.LabelKey, ContentFieldId, definition.PortalId, portalSettings);
            LocalizedNames = GetLocalizedValues(definition.Name, FieldDefinitionManager.NameKey, ContentFieldId, definition.PortalId, portalSettings);
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
        /// A List of localized values for the Description property
        /// </summary>
        [JsonProperty("localizedDescriptions")]
        public List<dynamic> LocalizedDescriptions { get; set; }

        /// <summary>
        /// A List of localized values for the Label property
        /// </summary>
        [JsonProperty("localizedLabels")]
        public List<dynamic> LocalizedLabels { get; set; }

        /// <summary>
        /// A List of localized values for the Name property
        /// </summary>
        [JsonProperty("localizedNames")]
        public List<dynamic> LocalizedNames { get; set; }
    }
}
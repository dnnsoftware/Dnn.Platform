// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// ContentTypeViewModel represents a Content Type object within the ContentType Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContentTypeViewModel : BaseViewModel
    {
        /// <summary>
        /// Constructs a ContentTypeViewModel
        /// </summary>
        public ContentTypeViewModel()
        {

        }

        /// <summary>
        /// Constructs a ContentTypeViewModel from a DynamicContentType object
        /// </summary>
        /// <param name="contentType">The DynamicContentType object</param>
        /// <param name="portalSettings">The portal Settings for the current portal</param>
        /// <param name="includeChildren">A flag that indicates whether the children collections should be parsed</param>
        public ContentTypeViewModel(DynamicContentType contentType, PortalSettings portalSettings, bool includeChildren = false)
        {
            ContentTypeId = contentType.ContentTypeId;
            Created = contentType.CreatedOnDate.ToShortDateString();
            IsSystem = contentType.IsSystem;
            CanEdit = !(IsSystem) || portalSettings.UserInfo.IsSuperUser;

            if (includeChildren)
            {
                ContentFields = new ContentFieldsViewModel(contentType.FieldDefinitions);
            }

            LocalizedDescriptions = GetLocalizedValues(contentType.Description, DataTypeManager.DataTypeNameKey, ContentTypeId, contentType.PortalId, portalSettings);
            LocalizedNames = GetLocalizedValues(contentType.Name, DataTypeManager.DataTypeNameKey, ContentTypeId, contentType.PortalId, portalSettings);
        }

        /// <summary>
        /// A flag that determines if the current user can edit the object
        /// </summary>
        [JsonProperty("canEdit")]
        public bool CanEdit { get; set; }

        /// <summary>
        /// A collection of content fields
        /// </summary>
        [JsonProperty("contentFields")]
        public ContentFieldsViewModel ContentFields { get; set; }

        /// <summary>
        /// The Id of the Content Type
        /// </summary>
        [JsonProperty("contentTypeId")]
        public int ContentTypeId { get; set; }

        /// <summary>
        /// The Date when the Data Type was created
        /// </summary>
        [JsonProperty("created")]
        public string Created { get; set; }

        /// <summary>
        /// The name of the Data Type
        /// </summary>
        [JsonProperty("isSystem")]
        public bool IsSystem { get; set; }

        /// <summary>
        /// A List of localized values for the Description property
        /// </summary>
        [JsonProperty("localizedDescriptions")]
        public List<dynamic> LocalizedDescriptions { get; set; }

        /// <summary>
        /// A List of localized values for the Name property
        /// </summary>
        [JsonProperty("localizedNames")]
        public List<dynamic> LocalizedNames { get; set; }
    }
}

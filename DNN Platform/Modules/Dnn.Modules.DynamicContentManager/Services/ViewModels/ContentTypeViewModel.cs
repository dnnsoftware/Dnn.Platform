// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Dnn.DynamicContent;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// ContentTypeViewModel represents a Content Type object within the ContentType Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContentTypeViewModel
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
        public ContentTypeViewModel(DynamicContentType contentType, bool isSuperUser)
        {
            ContentTypeId = contentType.ContentTypeId;
            Created = contentType.CreatedOnDate.ToShortDateString();
            IsSystem = contentType.IsSystem;
            Name = contentType.Name;
            Description = contentType.Description;
            CanEdit = !(IsSystem) || isSuperUser;
        }

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
        /// The Id of the Content Type
        /// </summary>
        [JsonProperty("contentTypeId")]
        public int ContentTypeId { get; set; }

        /// <summary>
        /// The description of the Content Type
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

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

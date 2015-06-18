// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// TemplateViewModel represents a Content Template object within the ContentType Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TemplateViewModel
    {
        /// <summary>
        /// Constructs a TemplateViewModel
        /// </summary>
        public TemplateViewModel()
        {

        }

        /// <summary>
        /// Constructs a TemplateViewModel from a ContentTemplate object
        /// </summary>
        /// <param name="template">The ContentTemplate object</param>
        /// <param name="isSuperUser">A flag that indicates whether the current user is a super user</param>
        public TemplateViewModel(ContentTemplate template, bool isSuperUser)
        {
            TemplateId = template.TemplateId;
            ContentTypeId = template.ContentTypeId;
            ContentType = template.ContentType.Name;
            IsSystem = template.IsSystem;
            Name = template.Name;
            CanEdit = !(IsSystem) || isSuperUser;
        }

        /// <summary>
        /// A flag that determines if the current user can edit the object
        /// </summary>
        [JsonProperty("canEdit")]
        public bool CanEdit { get; set; }

        /// <summary>
        /// The Content Type
        /// </summary>
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        /// <summary>
        /// The Id of the Content Type
        /// </summary>
        [JsonProperty("contentTypeId")]
        public int ContentTypeId { get; set; }

        /// <summary>
        /// A flag indicating if the template is a system template
        /// </summary>
        [JsonProperty("isSystem")]
        public bool IsSystem { get; set; }

        /// <summary>
        /// The name of the Template
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The Id of the Template
        /// </summary>
        [JsonProperty("templateId")]
        public int TemplateId { get; set; }
    }
}
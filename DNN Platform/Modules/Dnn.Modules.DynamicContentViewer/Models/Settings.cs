// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Dnn.DynamicContent;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Dnn.Modules.DynamicContentViewer.Models
{
    /// <summary>
    /// Setting class manages the settings for the module instance
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Settings
    {
#pragma warning disable 1591
        public const string DCC_ContentTypeId = "DCC_ContentTypeId";
        public const string DCC_EditTemplateId = "DCC_EditTemplateId";
        public const string DCC_ViewTemplateId = "DCC_ViewTemplateId";
#pragma warning restore 1591

        /// <summary>
        /// The Id of the Content Type
        /// </summary>
        [JsonProperty("contentTypeId")]
        public int ContentTypeId { get; set; }

        /// <summary>
        /// A list of available content types for the site
        /// </summary>
        public IList<DynamicContentType> ContentTypes { get; set; }

        /// <summary>
        /// The Id of the Edit Template
        /// </summary>
        [JsonProperty("editTemplateId")]
        public int EditTemplateId { get; set; }

        /// <summary>
        /// A list of templates for the currently selected contentType
        /// </summary>
        public IList<ContentTemplate> Templates { get; set; }

        /// <summary>
        /// The Id of the View Template
        /// </summary>
        [JsonProperty("viewTemplateId")]
        public int ViewTemplateId { get; set; }
    }
}

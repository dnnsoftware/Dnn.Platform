// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Dnn.DynamicContent;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// TemplateViewModel represents a Content Template object within the ContentType Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TemplateViewModel : BaseViewModel
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
        /// <param name="portalSettings">The portal Settings for the current portal</param>
        public TemplateViewModel(ContentTemplate template, PortalSettings portalSettings)
        {
            TemplateId = template.TemplateId;
            ContentTypeId = template.ContentTypeId;
            ContentType = template.ContentType.Name;
            IsSystem = template.IsSystem;
            CanEdit = !(IsSystem) || portalSettings.UserInfo.IsSuperUser;
            FilePath = template.TemplateFile.RelativePath;

            using (var sw = new StreamReader(FileManager.Instance.GetFileContent(template.TemplateFile)))
            {
                Content = sw.ReadToEnd();
            }

            LocalizedNames = GetLocalizedValues(template.Name, ContentTemplateManager.TemplateNameKey, TemplateId, template.PortalId, portalSettings);
        }

        /// <summary>
        /// A flag that determines if the current user can edit the object
        /// </summary>
        [JsonProperty("canEdit")]
        public bool CanEdit { get; set; }

        /// <summary>
        /// The Content of the template
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

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
        /// The path of the Template File
        /// </summary>
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        /// <summary>
        /// A flag indicating if the template is a system template
        /// </summary>
        [JsonProperty("isSystem")]
        public bool IsSystem { get; set; }

        /// <summary>
        /// A List of localized values for the Name property
        /// </summary>
        [JsonProperty("localizedNames")]
        public List<dynamic> LocalizedNames { get; set; }

        /// <summary>
        /// The Id of the Template
        /// </summary>
        [JsonProperty("templateId")]
        public int TemplateId { get; set; }
    }
}
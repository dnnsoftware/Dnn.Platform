// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Dnn.DynamicContent
{

    /// <summary>
    /// Represents a Content Template for a Dynamic Content Type
    /// </summary>
    [Serializable]
    [TableName("ContentTypes_Templates")]
    [PrimaryKey("TemplateID", "TemplateId")]
    [Cacheable(ContentTemplateController.ContentTemplateCacheKey, CacheItemPriority.Normal, 20)]
    [Scope(ContentTemplateController.ContentTemplateScope)]
    public class ContentTemplate
    {
        public ContentTemplate()
        {
            ContentTypeId = -1;
            TemplateId = -1;
            TemplateFileId = -1;
            Name = String.Empty;
        }

        /// <summary>
        /// The Id of the <see cref="T:DotNetNuke.Entities.Content.DynamicContent.DynamicContentType"/> to which this <see cref="T:Dnn.DynamicContent.ContentTemplate"/> belongs
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// The id of the File which contains the HTML for this <see cref="T:Dnn.DynamicContent.ContentTemplate"/>
        /// </summary>
        public int TemplateFileId { get; set; }

        /// <summary>
        /// The name of this <see cref="T:Dnn.DynamicContent.ContentTemplate"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Id of this <see cref="T:Dnn.DynamicContent.ContentTemplate"/>
        /// </summary>

        public int TemplateId { get; set; }
    }
}

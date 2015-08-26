// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Services.FileSystem;
// ReSharper disable ConvertPropertyToExpressionBody

namespace Dnn.DynamicContent
{

    /// <summary>
    /// Represents a Content Template for a Dynamic Content Type
    /// </summary>
    [Serializable]
    [TableName("ContentTypes_Templates")]
    [PrimaryKey("TemplateID", "TemplateId")]
    [Cacheable(ContentTemplateManager.ContentTemplateCacheKey, CacheItemPriority.Normal, 20)]
    [Scope(ContentTemplateManager.PortalScope)]
    public class ContentTemplate : BaseEntity
    {
        private DynamicContentType _contentType;
        private IFileInfo _templateFile;

        public ContentTemplate() : this(-1) { }

        public ContentTemplate(int portalId)
        {
            ContentTypeId = -1;
            TemplateId = -1;
            TemplateFileId = -1;
            Name = String.Empty;
            PortalId = portalId;
        }

        //TODO - add Unit Tests for this
        [IgnoreColumn]
        public DynamicContentType ContentType
        {
            get
            {
                return _contentType ?? (_contentType = DynamicContentTypeManager.Instance.GetContentTypes(PortalId, true)
                                                            .SingleOrDefault(ct => ct.ContentTypeId == ContentTypeId));
            }
        }

        [IgnoreColumn]
        public bool IsSystem { get { return (PortalId == -1); } }

        /// <summary>
        /// The Id of the <see cref="T:DotNetNuke.Entities.Content.DynamicContent.DynamicContentType"/> to which this <see cref="T:Dnn.DynamicContent.ContentTemplate"/> belongs
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// The name of this <see cref="T:Dnn.DynamicContent.ContentTemplate"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Id of the portal
        /// </summary>
        public int PortalId { get; set; }

        //TODO - add Unit Tests for this
        [IgnoreColumn]
        public IFileInfo TemplateFile
        {
            get { return _templateFile ?? (_templateFile = FileManager.Instance.GetFile(TemplateFileId)); }
        }

        /// <summary>
        /// The id of the File which contains the HTML for this <see cref="T:Dnn.DynamicContent.ContentTemplate"/>
        /// </summary>
        public int TemplateFileId { get; set; }

        /// <summary>
        /// The Id of this <see cref="T:Dnn.DynamicContent.ContentTemplate"/>
        /// </summary>
        public int TemplateId { get; set; }
    }
}

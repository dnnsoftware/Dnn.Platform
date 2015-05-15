// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Content;

// ReSharper disable ConvertPropertyToExpressionBody

namespace Dnn.DynamicContent
{
    [Serializable]
    [TableName("ContentTypes")]
    [PrimaryKey("ContentTypeID", "ContentTypeId")]
    [Cacheable(DataCache.ContentTypesCacheKey, DataCache.ContentTypesCachePriority, DataCache.ContentTypesCacheTimeOut)]
    [Scope("PortalId")]
    public class DynamicContentType : BaseEntity
    {
        private IList<FieldDefinition> _fieldDefitions;
        private IList<ContentTemplate> _templates;

        public DynamicContentType() : this(-1) { }

        public DynamicContentType(int portalId) : base()
        {
            ContentTypeId = Null.NullInteger;
            Name = String.Empty;
            PortalId = portalId;
            IsDynamic = true;
        }

        /// <summary>
        /// Gets or sets the name of the ContentType.
        /// </summary>
        [ColumnName("ContentType")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the content type id.
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// Gets a list of Field Definitions associated with this Content Type
        /// </summary>
        [IgnoreColumn]
        public IList<FieldDefinition> FieldDefinitions
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_fieldDefitions == null)
                {
                    _fieldDefitions = (ContentTypeId == -1)
                                        ? new List<FieldDefinition>()
                                        : FieldDefinitionManager.Instance.GetFieldDefinitions(ContentTypeId).ToList();
                }
                return _fieldDefitions;
            }
        }

        /// <summary>
        /// A flag that indicates whether the Content Type is Dynamic
        /// </summary>
        public bool IsDynamic { get; set; }

        /// <summary>
        /// A flag that indicates whether the Content Type is a system type
        /// </summary>
        [IgnoreColumn]
        public bool IsSystem { get { return (PortalId == -1); } }

        /// <summary>
        /// The Id of the portal
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// Gets a list of Content Templates associated with this Content Type
        /// </summary>
        [IgnoreColumn]
        public IList<ContentTemplate> Templates
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_templates == null)
                {
                    _templates = (ContentTypeId == -1)
                                        ? new List<ContentTemplate>()
                                        : ContentTemplateManager.Instance.GetContentTemplates(ContentTypeId, PortalId, true).ToList();
                }
                return _templates;
            }
        }
    }
}

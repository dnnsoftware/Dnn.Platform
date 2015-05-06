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
    public class DynamicContentType : ContentType
    {
        private IList<FieldDefinition> _fieldDefitions;
        private IList<ContentTemplate> _templates;

        public DynamicContentType()
        {
            PortalId = -1;
            IsDynamic = true;
        }

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
                                        : FieldDefinitionController.Instance.GetFieldDefinitions(ContentTypeId).ToList();
                }
                return _fieldDefitions;
            }
        }

        public bool IsDynamic { get; set; }

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
                                        : ContentTemplateController.Instance.GetContentTemplates(ContentTypeId).ToList();
                }
                return _templates;
            }
        }

    }
}

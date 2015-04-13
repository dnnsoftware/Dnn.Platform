#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
// ReSharper disable ConvertPropertyToExpressionBody

namespace DotNetNuke.Entities.Content.DynamicContent
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

        /// <summary>
        /// Gets a list of Field Definitions associated with this Content Type
        /// </summary>
        [IgnoreColumn]
        public IList<FieldDefinition> FieldDefinitions
        {
            get
            {
                return _fieldDefitions ?? (_fieldDefitions = FieldDefinitionController.Instance.GetFieldDefinitions(ContentTypeId).ToList());
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
                return _templates ?? (_templates = ContentTemplateController.Instance.GetContentTemplates(ContentTypeId).ToList());
            }
        }

    }
}

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
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.Entities.Content.DynamicContent
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
        /// The Id of the <see cref="T:DotNetNuke.Entities.Content.DynamicContent.DynamicContentType"/> to which this <see cref="T:DotNetNuke.Entities.Content.DynamicContent.ContentTemplate"/> belongs
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// The id of the File which contains the HTML for this <see cref="T:DotNetNuke.Entities.Content.DynamicContent.ContentTemplate"/>
        /// </summary>
        public int TemplateFileId { get; set; }

        /// <summary>
        /// The name of this <see cref="T:DotNetNuke.Entities.Content.DynamicContent.ContentTemplate"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Id of this <see cref="T:DotNetNuke.Entities.Content.DynamicContent.ContentTemplate"/>
        /// </summary>

        public int TemplateId { get; set; }
    }
}

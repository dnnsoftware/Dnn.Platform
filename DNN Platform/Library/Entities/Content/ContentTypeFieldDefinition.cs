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

namespace DotNetNuke.Entities.Content
{
    [Serializable]
    [TableName("ContentTypes_FieldDefinitions")]
    [PrimaryKey("FieldDefinitionID", "FieldDefinitionId")]
    [Cacheable("ContentTypes_FieldDefinitions", CacheItemPriority.Normal, 20)]
    [Scope("ContentTypeId")]
    public class ContentTypeFieldDefinition
    {
        public ContentTypeFieldDefinition()
        {
            ContentTypeId = -1;
            DataTypeId = -1;
        }

        public int ContentTypeId { get; set; }

        public int DataTypeId { get; set; }

        public string Description { get; set; }

        public int FieldDefinitionId { get; set; }

        public string Label { get; set; }

        public string Name { get; set; }
    }
}

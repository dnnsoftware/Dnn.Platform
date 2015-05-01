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
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content;
using DotNetNuke.Framework;

namespace Dnn.DynamicContent
{
    public class DynamicContentItemController : ServiceLocator<IDynamicContentItemController, DynamicContentItemController>, IDynamicContentItemController
    {
        protected override Func<IDynamicContentItemController> GetFactory()
        {
            return () => new DynamicContentItemController();
        }

        public int AddContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNull(dynamicContent, "ContentType");
            Requires.PropertyNotNegative(dynamicContent.ContentType, "ContentTypeId");
            Requires.PropertyNotNegative(dynamicContent, "ModuleId");

            var contentItem = new ContentItem
                                    {
                                        ContentTypeId = dynamicContent.ContentType.ContentTypeId,
                                        Content = dynamicContent.ToJson(),
                                        ModuleID = dynamicContent.ModuleId,
                                        TabID = -1,
                                        ContentKey = String.Empty
                                    };

            return ContentController.Instance.AddContentItem(contentItem);
        }

        public DynamicContentItem CreateContentItem(int moduleId, DynamicContentType contentType)
        {
            Requires.PropertyNotNegative(contentType, "PortalId");

            if (contentType.FieldDefinitions.Count == 0)
            {
                throw new InvalidOperationException("The content type has no fields defined.");
            }
            return new DynamicContentItem(contentType) { ModuleId = moduleId };
        }

        public DynamicContentItem CreateContentItem(int portalId, ContentItem contentItem)
        {
            Requires.NotNull("contentItem", contentItem);

            var dynamicContentItem = new DynamicContentItem(portalId) {ModuleId = contentItem.ModuleID};

            dynamicContentItem.FromJson(contentItem.Content);

            return dynamicContentItem;
        }

        public void DeleteContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNegative(dynamicContent, "ContentItemId");

            ContentController.Instance.DeleteContentItem(dynamicContent.ContentItemId);
        }

        public IQueryable<DynamicContentItem> GetContentItems(int contentTypeId, int moduleId)
        {
            return new List<DynamicContentItem>().AsQueryable();
        }

        public IPagedList<DynamicContentItem> GetContentItems(int contentTypeId, int moduleId, int pageIndex, int pageSize)
        {
            return new PagedList<DynamicContentItem>(new List<DynamicContentItem>(), pageIndex, pageSize);
        }

        public void UpdateContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNegative(dynamicContent, "ContentItemId");
            Requires.PropertyNotNull(dynamicContent, "ContentType");
            Requires.PropertyNotNegative(dynamicContent.ContentType, "ContentTypeId");
            Requires.PropertyNotNegative(dynamicContent, "ModuleId");

            var contentItem = new ContentItem
                                {
                                    ContentItemId = dynamicContent.ContentItemId,
                                    ContentTypeId = dynamicContent.ContentType.ContentTypeId,
                                    Content = dynamicContent.ToJson(),
                                    ModuleID = dynamicContent.ModuleId,
                                    TabID = -1,
                                    ContentKey = String.Empty
                                };

            ContentController.Instance.UpdateContentItem(contentItem);
        }
    }
}

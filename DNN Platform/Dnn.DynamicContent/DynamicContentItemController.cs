// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

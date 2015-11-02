// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace Dnn.DynamicContent
{
    public class DynamicContentItemManager : ServiceLocator<IDynamicContentItemManager, DynamicContentItemManager>, IDynamicContentItemManager
    {
        protected override Func<IDynamicContentItemManager> GetFactory()
        {
            return () => new DynamicContentItemManager();
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
                                        TabID = dynamicContent.TabId,
                                        ContentKey = String.Empty
                                    };

            return ContentController.Instance.AddContentItem(contentItem);
        }

        public DynamicContentItem CreateContentItem(int portalId, int tabId, int moduleId, DynamicContentType contentType)
        {
            Requires.NotNegative("portalId", portalId);
            Requires.NotNegative("moduleId", moduleId);
            Requires.NotNegative("tabId", tabId);
            Requires.NotNull("contentType", contentType);

            if (contentType.FieldDefinitions.Count == 0)
            {
                throw new InvalidOperationException("The content type has no fields defined.");
            }
            return new DynamicContentItem(portalId, contentType) { ModuleId = moduleId, TabId = tabId };
        }

        public DynamicContentItem CreateContentItem(ContentItem contentItem)
        {
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative(contentItem, "TabID");
            Requires.PropertyNotNegative(contentItem, "ModuleID");

            var module = ModuleController.Instance.GetModule(contentItem.ModuleID, contentItem.TabID, false);

            var contentType = DynamicContentTypeManager.Instance.GetContentType(contentItem.ContentTypeId, module.PortalID, true);

            var dynamicContentItem = new DynamicContentItem(module.PortalID, contentType)
                                            {
                                                ContentItemId = contentItem.ContentItemId,
                                                ModuleId = contentItem.ModuleID,
                                                TabId = contentItem.TabID
                                            };

            dynamicContentItem.FromJson(contentItem.Content);

            return dynamicContentItem;
        }

        public void DeleteContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNegative(dynamicContent, "ContentItemId");

            ContentController.Instance.DeleteContentItem(dynamicContent.ContentItemId);
        }

        public DynamicContentItem GetContentItem(int contentItemId)
        {
            Requires.NotNegative("contentItemId", contentItemId);

            var contentItem = ContentController.Instance.GetContentItem(contentItemId);
            DynamicContentItem dynamicContentItem = null;
            if (contentItem != null)
            {
                dynamicContentItem = CreateContentItem(contentItem);
            }

            return dynamicContentItem;
        }

        public IQueryable<DynamicContentItem> GetContentItems(int moduleId, int contentTypeId)
        {
            Requires.NotNegative("moduleId", moduleId);

            var contentItems = ContentController.Instance.GetContentItemsByModuleId(moduleId) .Where(c => c.ContentTypeId == contentTypeId);
            var dynamicContentItems = new List<DynamicContentItem>();
            foreach (var contentItem in contentItems)
            {
                dynamicContentItems.Add(CreateContentItem(contentItem));
            }

            return dynamicContentItems.AsQueryable();
        }

        public void UpdateContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNegative(dynamicContent, "ContentItemId");
            Requires.PropertyNotNull(dynamicContent, "ContentType");
            Requires.PropertyNotNegative(dynamicContent.ContentType, "ContentTypeId");
            Requires.PropertyNotNegative(dynamicContent, "ModuleId");
            Requires.PropertyNotNegative(dynamicContent, "TabId");

            var contentItem = new ContentItem
                                {
                                    ContentItemId = dynamicContent.ContentItemId,
                                    ContentTypeId = dynamicContent.ContentType.ContentTypeId,
                                    Content = dynamicContent.ToJson(),
                                    ModuleID = dynamicContent.ModuleId,
                                    TabID = dynamicContent.TabId,
                                    ContentKey = String.Empty
                                };

            ContentController.Instance.UpdateContentItem(contentItem);
        }
    }
}

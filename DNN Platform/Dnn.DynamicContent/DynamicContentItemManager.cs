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

        private readonly IContentController _contentController;
        private readonly IDynamicContentTypeManager _dynamicContentTypeManager;

        public DynamicContentItemManager()
        {
            _contentController = ContentController.Instance;
            _dynamicContentTypeManager = DynamicContentTypeManager.Instance;
        }

        public int AddContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNull(dynamicContent, "ContentType");
            Requires.PropertyNotNegative(dynamicContent.ContentType, "ContentTypeId");

            var contentItem = new ContentItem
                                    {
                                        ContentTypeId = dynamicContent.ContentType.ContentTypeId,
                                        Content = dynamicContent.ToJson(),
                                        ContentKey = String.Empty
                                    };

            return _contentController.AddContentItem(contentItem);
        }

        public DynamicContentItem CreateContentItem(DynamicContentType contentType, int portalId)
        {
            Requires.NotNull("contentType", contentType);
            Requires.NotNegative("portalId", portalId);

            if (contentType.FieldDefinitions.Count == 0)
            {
                throw new InvalidOperationException("The content type has no fields defined.");
            }
            return new DynamicContentItem(portalId, contentType);
        }

        public DynamicContentItem CreateContentItem(ContentItem contentItem, int portalId)
        {
            Requires.NotNull("contentItem", contentItem);

            var contentType = _dynamicContentTypeManager.GetContentType(portalId, contentItem.ContentTypeId, true);

            var dynamicContentItem = new DynamicContentItem(contentType.PortalId, contentType)
                                            {
                                                ContentItemId = contentItem.ContentItemId
                                            };

            dynamicContentItem.FromJson(contentItem.Content);

            return dynamicContentItem;
        }

        public void DeleteContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNegative(dynamicContent, "ContentItemId");

            _contentController.DeleteContentItem(dynamicContent.ContentItemId);
        }

        public DynamicContentItem GetContentItem(int contentItemId, int portalId)
        {
            Requires.NotNegative("contentItemId", contentItemId);

            var contentItem = _contentController.GetContentItem(contentItemId);
            DynamicContentItem dynamicContentItem = null;
            if (contentItem != null)
            {
                dynamicContentItem = CreateContentItem(contentItem, portalId);
            }

            return dynamicContentItem;
        }

        public void UpdateContentItem(DynamicContentItem dynamicContent)
        {
            Requires.NotNull(dynamicContent);
            Requires.PropertyNotNegative(dynamicContent, "ContentItemId");
            Requires.PropertyNotNull(dynamicContent, "ContentType");
            Requires.PropertyNotNegative(dynamicContent.ContentType, "ContentTypeId");

            var contentItem = new ContentItem
                                {
                                    ContentItemId = dynamicContent.ContentItemId,
                                    ContentTypeId = dynamicContent.ContentType.ContentTypeId,
                                    Content = dynamicContent.ToJson(),
                                    ContentKey = String.Empty
                                };

            _contentController.UpdateContentItem(contentItem);
        }
    }
}

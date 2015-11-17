// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.Entities.Content;

namespace Dnn.DynamicContent
{
    public interface IDynamicContentItemManager
    {
        int AddContentItem(DynamicContentItem dynamicContent);

        DynamicContentItem CreateContentItem(DynamicContentType contentType, int portalId);

        DynamicContentItem CreateContentItem(ContentItem contentItem, int portalId);

        void DeleteContentItem(DynamicContentItem dynamicContent);

        DynamicContentItem GetContentItem(int contentItemId, int portalId);

        void UpdateContentItem(DynamicContentItem dynamicContent);
    }
}

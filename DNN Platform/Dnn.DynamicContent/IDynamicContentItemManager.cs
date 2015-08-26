// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Content;

namespace Dnn.DynamicContent
{
    public interface IDynamicContentItemManager
    {
        int AddContentItem(DynamicContentItem dynamicContent);

        DynamicContentItem CreateContentItem(int portalId, int tabId, int moduleId, DynamicContentType contentType);

        DynamicContentItem CreateContentItem(ContentItem contentItem);

        void DeleteContentItem(DynamicContentItem dynamicContent);

        IQueryable<DynamicContentItem> GetContentItems(int moduleId, int contentTypeId);

        void UpdateContentItem(DynamicContentItem dynamicContent);
    }
}

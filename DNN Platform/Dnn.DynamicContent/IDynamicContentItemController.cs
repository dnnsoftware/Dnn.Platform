// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Content;

namespace Dnn.DynamicContent
{
    public interface IDynamicContentItemController
    {
        int AddContentItem(DynamicContentItem dynamicContent);

        DynamicContentItem CreateContentItem(int moduleId, DynamicContentType contentType);

        DynamicContentItem CreateContentItem(int portalId, ContentItem contentItem);

        void DeleteContentItem(DynamicContentItem dynamicContent);

        IQueryable<DynamicContentItem> GetContentItems(int contentTypeId, int moduleId);

        IPagedList<DynamicContentItem> GetContentItems(int contentTypeId, int moduleId, int pageIndex, int pageSize);

        void UpdateContentItem(DynamicContentItem dynamicContent);
    }
}

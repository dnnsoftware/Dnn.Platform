// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;
using DotNetNuke.Entities.Modules;

namespace Dnn.Modules.DynamicContentViewer.Components
{
    public interface IDynamicContentViewerManager
    {
        DynamicContentItem GetOrCreateContentItem(ModuleInfo moduleInfo, int contentTypeId);
        DynamicContentItem CreateDefaultContentItem(ModuleInfo moduleInfo);
        int GetContentTypeId(ModuleInfo moduleInfo);
        int GetViewTemplateId(ModuleInfo moduleInfo);
        int GetEditTemplateId(ModuleInfo moduleInfo);
        DynamicContentItem GetContentItem(ModuleInfo moduleInfo);
        void UpdateContentItem(DynamicContentItem contentItem);

    }
}
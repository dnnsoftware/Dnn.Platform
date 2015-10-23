// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.Services.Search.Entities;

namespace Dnn.DynamicContent
{
    public interface IDynamicContentSearchManager
    {
        SearchDocument GetSearchDocument(DynamicContentItem dynamicContent);
    }
}

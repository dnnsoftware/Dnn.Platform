// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Search.Entities;

namespace Dnn.DynamicContent
{
    /// <summary>
    /// This interface is responsible for the search capabilities for Dynamic Content Items
    /// </summary>
    public interface IDynamicContentSearchManager
    {
        /// <summary>
        /// Given a Dynamic Content Item this methods generate a Search Document to be indexed by the Search Engine
        /// </summary>
        /// <param name="moduleInfo">Module Info</param>
        /// <param name="dynamicContent">Dynamic Content Item that need to be converted to a Search Document</param>
        /// <returns>Search Document</returns>
        SearchDocument GetSearchDocument(ModuleInfo moduleInfo, DynamicContentItem dynamicContent);
    }
}

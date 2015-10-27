// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;
using DotNetNuke.Entities.Modules;

namespace Dnn.Modules.DynamicContentViewer.Components
{
    public interface IDynamicContentViewerManager
    {
        /// <summary>
        /// Gets the module Dynamic Content Item or creates based on the passed Content Type
        /// </summary>
        /// <param name="moduleInfo">Module related with the Content Item</param>
        /// <param name="contentTypeId">Id of the Content Type on which the Content Item is based</param>
        /// <returns>The Content Item related with the module</returns>
        DynamicContentItem GetOrCreateContentItem(ModuleInfo moduleInfo, int contentTypeId);
        
        /// <summary>
        /// Creates a default Content Item for a module using HTML as default Content Type
        /// </summary>
        /// <param name="moduleInfo">Module related with the Content Item</param>
        /// <returns>A Content Item related with module filled with default values</returns>
        DynamicContentItem CreateDefaultContentItem(ModuleInfo moduleInfo);
        
        /// <summary>
        /// Get the setting Content Type Id configured by the module
        /// </summary>
        /// <param name="moduleInfo">Module related with the setting</param>
        /// <returns>Id of the Content Type</returns>
        int GetContentTypeId(ModuleInfo moduleInfo);

        /// <summary>
        /// Get the setting View Template Id configured by the module
        /// </summary>
        /// <param name="moduleInfo">Module related with the setting</param>
        /// <returns>Id of the View Template</returns>
        int GetViewTemplateId(ModuleInfo moduleInfo);

        /// <summary>
        /// Get the setting Edit Template Id configured by the module
        /// </summary>
        /// <param name="moduleInfo">Module related with the setting</param>
        /// <returns>Id of the Edit Template</returns>
        int GetEditTemplateId(ModuleInfo moduleInfo);

        /// <summary>
        /// Gets the module Dynamic Content Item
        /// </summary>
        /// <param name="moduleInfo">Module related with the Content Item</param>
        /// <returns>The Content Item related with the module</returns>
        DynamicContentItem GetContentItem(ModuleInfo moduleInfo);

        /// <summary>
        /// Updates a given Content Item
        /// </summary>
        /// <param name="contentItem">Content Item to be updated</param>
        void UpdateContentItem(DynamicContentItem contentItem);

    }
}
// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;

using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Models;

using DotNetNuke.Collections;
using DotNetNuke.Entities.Modules;

namespace Dnn.Modules.DynamicContentViewer.Components
{
    /// <summary>The DCC Viewer module's business controller</summary>
    public class BusinessController : IPortable
    {
        /// <summary>Exports the content of this module</summary>
        /// <param name="ModuleID">The ID of the module to export</param>
        /// <returns>The module's content serialized as a <see cref="string"/></returns>
        public string ExportModule(int ModuleID)
        {
            var moduleSettings = new ModuleController().GetModule(ModuleID).ModuleSettings;

            var contentTypeId = moduleSettings.GetValueOrDefault(Settings.DCC_ContentTypeId, -1);
            var value = DynamicContentItemManager.Instance.GetContentItems(ModuleID, contentTypeId).SingleOrDefault();
            return value == null ? string.Empty : value.ToJson();
        }

        /// <summary>Imports the content of a module</summary>
        /// <param name="ModuleID">The ID of the module into which the content is being imported</param>
        /// <param name="Content">The content to import</param>
        /// <param name="Version">The version of the module from which the content is coming</param>
        /// <param name="UserID">The ID of the user performing the import</param>
        public void ImportModule(int ModuleID, string Content, string Version, int UserID)
        {
            if (string.IsNullOrEmpty(Content))
            {
                return;
            }

            var module = new ModuleController().GetModule(ModuleID);
            var moduleSettings = module.ModuleSettings;

            var contentTypeId = moduleSettings.GetValueOrDefault(Settings.DCC_ContentTypeId, -1);
            var contentItem = DynamicContentItemManager.Instance.GetContentItems(ModuleID, contentTypeId).SingleOrDefault();
            if (contentItem == null)
            {
                var contentType = DynamicContentTypeManager.Instance.GetContentType(contentTypeId, module.PortalID, true);
                contentItem = DynamicContentItemManager.Instance.CreateContentItem(module.PortalID, module.TabID, ModuleID, contentType);
                contentItem.FromJson(Content);
                DynamicContentItemManager.Instance.AddContentItem(contentItem);
            }
            else
            {
                contentItem.FromJson(Content);
                DynamicContentItemManager.Instance.UpdateContentItem(contentItem);
            }
        }
    }
}
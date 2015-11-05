// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Common.Utilities;

namespace Dnn.Modules.DynamicContentViewer.Components
{
    public class DynamicContentViewerManager : ServiceLocator<IDynamicContentViewerManager, DynamicContentViewerManager>, IDynamicContentViewerManager
    {
        #region Members

        private readonly IDynamicContentTypeManager _dynamicContentTypeManager;
        private readonly IDynamicContentItemManager _dynamicContentItemManager;
        private readonly IModuleController _moduleController;
        #endregion

        public DynamicContentViewerManager()
        {
            _dynamicContentTypeManager = DynamicContentTypeManager.Instance;
            _dynamicContentItemManager = DynamicContentItemManager.Instance;
            _moduleController = ModuleController.Instance;
        }

        public  DynamicContentItem GetOrCreateContentItem(ModuleInfo moduleInfo, int contentTypeId)
        {
            var contentItem = GetContentItem(moduleInfo);
            if (contentItem != null)
            {
                return contentItem;
            }

            var contentType = _dynamicContentTypeManager.GetContentType(contentTypeId, moduleInfo.PortalID, true);
            contentItem = _dynamicContentItemManager.CreateContentItem(contentType, moduleInfo.PortalID);
            _dynamicContentItemManager.AddContentItem(contentItem);
            _moduleController.UpdateModuleSetting(moduleInfo.ModuleID, Settings.DCC_ContentItemId, contentItem.ContentItemId.ToString());
            return contentItem;
        }

        public DynamicContentItem CreateDefaultContentItem(ModuleInfo moduleInfo)
        {
            var contentType = _dynamicContentTypeManager.GetContentTypes(moduleInfo.PortalID, true).Single(ct => ct.Name == "HTML");
            return _dynamicContentItemManager.CreateContentItem(contentType, moduleInfo.PortalID);
        }

        public DynamicContentItem GetContentItem(ModuleInfo moduleInfo)
        {
            var contentItemId = GetContentItemId(moduleInfo);
            return contentItemId == Null.NullInteger ? 
                null : 
                _dynamicContentItemManager.GetContentItem(contentItemId, moduleInfo.PortalID);
        }

        public void UpdateContentItem(DynamicContentItem contentItem)
        {
            _dynamicContentItemManager.UpdateContentItem(contentItem);
        }

        public int GetContentItemId(ModuleInfo moduleInfo)
        {
            return moduleInfo.ModuleSettings.GetValueOrDefault(Settings.DCC_ContentItemId, -1);
        }

        public int GetContentTypeId(ModuleInfo moduleInfo)
        {
            var contentItem = GetContentItem(moduleInfo);
            if (contentItem != null)
            {
                return contentItem.ContentType.ContentTypeId;
            }
            
            return moduleInfo.ModuleSettings.GetValueOrDefault(Settings.DCC_ContentTypeId, -1);
        }

        public int GetViewTemplateId(ModuleInfo moduleInfo)
        {
            return moduleInfo.ModuleSettings.GetValueOrDefault(Settings.DCC_ViewTemplateId, -1);
        }

        public int GetEditTemplateId(ModuleInfo moduleInfo)
        {
            return moduleInfo.ModuleSettings.GetValueOrDefault(Settings.DCC_EditTemplateId, -1);
        }

        protected override System.Func<IDynamicContentViewerManager> GetFactory()
        {
            return () => new DynamicContentViewerManager();
        }
    }
}
// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Dnn.DynamicContent;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Search.Entities;

namespace Dnn.Modules.DynamicContentViewer.Components
{
    /// <summary>
    /// Module Business controller. It overwrites ModuleSearchBase methods
    /// </summary>
    public class BusinessController: ModuleSearchBase
    {
        #region Members

        private readonly IDynamicContentViewerManager _dynamicContentViewerManager;
        private readonly IDynamicContentSearchManager _dynamicContentSearchManager;
        #endregion

        /// <summary>
        /// BusinessController Constructor
        /// </summary>
        public BusinessController()
        {
            _dynamicContentViewerManager = DynamicContentViewerManager.Instance;
        }
        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
            var dynamicContentItem = _dynamicContentViewerManager.GetContentItem(moduleInfo);
            if (dynamicContentItem == null)
            {
                return new List<SearchDocument>();
            }

            var contentItem = ContentController.Instance.GetContentItem(dynamicContentItem.ContentItemId);
            if (contentItem.LastModifiedOnDate > beginDateUtc)
            {
                return new[]
                {
                    _dynamicContentSearchManager.GetSearchDocument(moduleInfo, dynamicContentItem)
                };
            }
            return new List<SearchDocument>();
        }
    }
}
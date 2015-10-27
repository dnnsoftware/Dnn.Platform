// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Dnn.DynamicContent;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
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
        private readonly IContentController _contentController;
        #endregion

        /// <summary>
        /// BusinessController Constructor
        /// </summary>
        public BusinessController()
        {
            _dynamicContentViewerManager = DynamicContentViewerManager.Instance;
            _dynamicContentSearchManager = DynamicContentSearchManager.Instance;
            _contentController = ContentController.Instance;
        }

        /// <summary>
        /// Implements the ModuleSearchable method
        /// </summary>
        /// <returns>A list of SearchDocument. One SearchDocument per DynamicContentItem</returns>
        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
            try
            {
                var conteTypeId = _dynamicContentViewerManager.GetContentTypeId(moduleInfo);
                if (conteTypeId <= Null.NullInteger)
                {
                    return new List<SearchDocument>();                    
                }

                var dynamicContentItem = _dynamicContentViewerManager.GetContentItem(moduleInfo);
                
                var contentItem = _contentController.GetContentItem(dynamicContentItem.ContentItemId);
                if (contentItem.LastModifiedOnDate.ToUniversalTime() > beginDateUtc &&
                    contentItem.LastModifiedOnDate.ToUniversalTime() < DateTime.UtcNow)
                {
                    return new[]
                    {
                        _dynamicContentSearchManager.GetSearchDocument(moduleInfo, dynamicContentItem)
                    };
                }
                return new List<SearchDocument>();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return new List<SearchDocument>();
            }
            
        }
    }
}
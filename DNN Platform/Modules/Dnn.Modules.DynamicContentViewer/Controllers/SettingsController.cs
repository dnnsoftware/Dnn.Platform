// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Web.Mvc;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Components;
using Dnn.Modules.DynamicContentViewer.Models;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace Dnn.Modules.DynamicContentViewer.Controllers
{
    /// <summary>
    /// The Settings Controller manages the modules settings
    /// </summary>
    public class SettingsController : DnnController
    {
        #region Members

        private readonly IDynamicContentViewerManager _dynamicContentViewerManager;
        #endregion

        /// <summary>
        /// SettingsController Constructor
        /// </summary>
        public SettingsController()
        {
            _dynamicContentViewerManager = DynamicContentViewerManager.Instance;
        }

        /// <summary>
        /// The Index action renders the default Settings View
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var contentTypeId = _dynamicContentViewerManager.GetContentTypeId(ActiveModule);
            var settings = new Settings
                                {
                                    ModuleId = ActiveModule.ModuleID,
                                    ContentTypeId = contentTypeId,
                                    ContentTypes = DynamicContentTypeManager.Instance.GetContentTypes(PortalSettings.PortalId, true).ToList(),
                                    ViewTemplateId = _dynamicContentViewerManager.GetViewTemplateId(ActiveModule),
                                    Templates = ContentTemplateManager.Instance.GetContentTemplates(PortalSettings.PortalId, true).Where(t => t.ContentTypeId == contentTypeId).ToList(),
                                    EditTemplateId = _dynamicContentViewerManager.GetEditTemplateId(ActiveModule)
                                };
            return View(settings);
        }
    }
}

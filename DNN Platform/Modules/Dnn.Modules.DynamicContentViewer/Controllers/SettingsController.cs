// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Web.Mvc;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentViewer.Models;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace Dnn.Modules.DynamicContentViewer.Controllers
{
    public class SettingsController : DnnController
    {

        public ActionResult Index()
        {
            var settings = new Settings
                                {
                                    ContentTypeId = -1,
                                    ContentTypes = DynamicContentTypeManager.Instance.GetContentTypes(PortalSettings.PortalId, true).ToList(),
                                    ViewTemplateId = -1,
                                    EditTemplateId = -1,
                                };
            return View(settings);
        }
    }
}

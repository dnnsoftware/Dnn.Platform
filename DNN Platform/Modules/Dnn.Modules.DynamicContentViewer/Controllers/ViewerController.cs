// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Mvc;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace Dnn.Modules.DynamicContentViewer.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewerController : DnnController
    {
        [ModuleActionItems]
        public ActionResult Index()
        {
            return View();
        }

        private ModuleActionCollection GetIndexActions()
        {
            var actions = new ModuleActionCollection();

            var managerModule = ModuleController.Instance.GetModuleByDefinition(PortalSettings.PortalId, "Dnn.DynamicContentManager");

            if (managerModule != null && ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "EDIT", managerModule))
            {
                actions.Add(-1,
                        LocalizeString("EditTemplates"),
                        ModuleActionType.AddContent,
                        "",
                        "",
                        Globals.NavigateURL(managerModule.TabID, String.Empty, "tab=Templates"),
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false);
            }
            return actions;
        }
    }
}

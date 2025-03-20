// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Website.Controllers
{
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.MvcPipeline.Website.Models;

    public class ModuleActionsController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ModuleActionsDeleteModel model)
        {
            var module = ModuleController.Instance.GetModule(model.ModuleId, model.TabId, false);
            if (module == null)
            {
                return this.HttpNotFound();
            }

            var portalSettings = PortalSettings.Current;
            var user = UserController.Instance.GetCurrentUserInfo();
            if (!module.IsShared)
            {
                foreach (ModuleInfo instance in ModuleController.Instance.GetTabModulesByModule(module.ModuleID))
                {
                    if (instance.IsShared)
                    {
                        // HARD Delete Shared Instance
                        ModuleController.Instance.DeleteTabModule(instance.TabID, instance.ModuleID, false);
                        EventLogController.Instance.AddLog(instance, portalSettings, user.UserID, string.Empty, EventLogController.EventLogType.MODULE_DELETED);
                    }
                }
            }

            ModuleController.Instance.DeleteTabModule(model.TabId, model.ModuleId, true);
            EventLogController.Instance.AddLog(module, portalSettings, user.UserID, string.Empty, EventLogController.EventLogType.MODULE_SENT_TO_RECYCLE_BIN);
            return new EmptyResult();
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.GlobalResourceFile);
        }
    }
}

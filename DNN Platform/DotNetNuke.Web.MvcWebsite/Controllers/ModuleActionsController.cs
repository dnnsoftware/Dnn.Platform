// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcWebsite.Controllers
{
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcWebsite.Models;

    /// <summary>
    /// Handles module-related actions for the MVC website.
    /// </summary>
    public class ModuleActionsController : Controller
    {
        private readonly IPortalSettings portalSettings;
        private readonly IPortalController portalController;
        private readonly IModuleController moduleController;
        private readonly IUserController userController;
        private readonly IEventLogger eventLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleActionsController"/> class.
        /// </summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="moduleController">The module controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        public ModuleActionsController(
            IPortalController portalController,
            IModuleController moduleController,
            IUserController userController,
            IEventLogger eventLogger)
        {
            this.portalController = portalController;
            this.portalSettings = this.portalController.GetCurrentSettings();
            this.moduleController = moduleController;
            this.userController = userController;
            this.eventLogger = eventLogger;
        }

        /// <summary>
        /// Deletes a module instance from the specified tab and logs the operation.
        /// </summary>
        /// <param name="model">The delete request model containing module and tab identifiers.</param>
        /// <returns>An empty result when the operation completes, or <see cref="HttpNotFoundResult"/> if the module does not exist.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ModuleActionsDeleteModel model)
        {
            var module = this.moduleController.GetModule(model.ModuleId, model.TabId, false);
            if (module == null)
            {
                return this.HttpNotFound();
            }

            var user = this.userController.GetCurrentUserInfo();
            if (!module.IsShared)
            {
                foreach (var instance in this.moduleController.GetTabModulesByModule(module.ModuleID))
                {
                    if (instance.IsShared)
                    {
                        // HARD Delete Shared Instance
                        this.moduleController.DeleteTabModule(instance.TabID, instance.ModuleID, false);
                        this.eventLogger.AddLog(instance, this.portalSettings, user.UserID, string.Empty, EventLogType.MODULE_DELETED);
                    }
                }
            }

            this.moduleController.DeleteTabModule(model.TabId, model.ModuleId, true);
            this.eventLogger.AddLog(module, this.portalSettings, user.UserID, string.Empty, EventLogType.MODULE_SENT_TO_RECYCLE_BIN);
            return new EmptyResult();
        }

        /// <summary>
        /// Retrieves a localized string from the global resource file.
        /// </summary>
        /// <param name="key">The resource key to localize.</param>
        /// <returns>The localized string for the specified key.</returns>
        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.GlobalResourceFile);
        }
    }
}

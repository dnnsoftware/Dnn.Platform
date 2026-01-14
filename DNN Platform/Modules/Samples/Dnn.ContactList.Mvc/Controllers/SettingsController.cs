// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Web.Mvc;
using Dnn.ContactList.Mvc.Models;
using DotNetNuke.Collections;
using DotNetNuke.Security;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace Dnn.ContactList.Mvc.Controllers
{
    /// <summary>
    /// The Settings Controller manages the modules settings
    /// </summary>
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
    [DnnHandleError]
    public class SettingsController : DnnController
    {
        /// <summary>
        /// The Index action renders the default Settings View
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            var settings = new Settings();
            settings.PageSize = ModuleContext.Configuration.ModuleSettings.GetValueOrDefault("PageSize", 9);
            settings.AllowContactCreation = ModuleContext.Configuration.ModuleSettings.GetValueOrDefault("AllowContactCreation", false);
            return View(settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [DotNetNuke.Web.Mvc.Framework.ActionFilters.ValidateAntiForgeryToken]
        public ActionResult Index(Settings settings)
        {
            ModuleContext.Configuration.ModuleSettings["PageSize"] = settings.PageSize.ToString();
            ModuleContext.Configuration.ModuleSettings["AllowContactCreation"] = settings.AllowContactCreation.ToString();
            return RedirectToDefaultRoute();
        }
    }
}

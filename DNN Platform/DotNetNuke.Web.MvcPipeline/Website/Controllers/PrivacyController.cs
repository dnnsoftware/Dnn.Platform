// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Website.Controllers
{
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    public class PrivacyController : Controller
    {
        public ActionResult Index()
        {
            var privacy = Localization.GetSystemMessage(PortalSettings.Current, "MESSAGE_PORTAL_PRIVACY");
            return this.View("Index", string.Empty, privacy);
        }
    }
}

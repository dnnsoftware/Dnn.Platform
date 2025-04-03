// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcWebsite.Controllers
{
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    public class TermsViewController : Controller
    {
        public ActionResult Invoke()
        {
            var terms = Localization.GetSystemMessage(PortalSettings.Current, "MESSAGE_PORTAL_TERMS");
            return this.View("Index", string.Empty, terms);
        }
    }
}

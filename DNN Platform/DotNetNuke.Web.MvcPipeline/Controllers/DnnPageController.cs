// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Framework;

    public abstract class DnnPageController : Controller, IMvcController
    {
        /// <summary>Initializes a new instance of the <see cref="DnnPageController"/> class.</summary>
        protected DnnPageController()
        {
        }

        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }
    }
}

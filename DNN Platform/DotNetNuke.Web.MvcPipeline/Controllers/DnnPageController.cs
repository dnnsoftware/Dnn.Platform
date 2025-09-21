// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;

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

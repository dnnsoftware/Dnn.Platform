// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;

    /// <summary>
    /// Base controller for DNN MVC page controllers, exposing common services and portal context.
    /// </summary>
    public abstract class DnnPageController : Controller, IMvcController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnnPageController"/> class.
        /// </summary>
        /// <param name="dependencyProvider">The dependency injection service provider.</param>
        protected DnnPageController(IServiceProvider dependencyProvider)
        {
            this.DependencyProvider = dependencyProvider;
        }

        /// <summary>
        /// Gets the dependency injection service provider for the current request.
        /// </summary>
        public IServiceProvider DependencyProvider  { get; private set; }

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }
    }
}

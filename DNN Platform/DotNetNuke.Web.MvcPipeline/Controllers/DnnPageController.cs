// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Base controller for DNN MVC page controllers, exposing common services and portal context.
    /// </summary>
    public abstract class DnnPageController : Controller, IMvcController
    {
        private readonly IPortalSettings portalSettings;
        private readonly IUserController userController;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnnPageController"/> class.
        /// </summary>
        /// <param name="dependencyProvider">The dependency injection service provider.</param>
        protected DnnPageController(IServiceProvider dependencyProvider)
        {
            this.DependencyProvider = dependencyProvider;
            this.portalSettings = dependencyProvider.GetService<IPortalSettings>();
            this.userController = dependencyProvider.GetService<IUserController>();
        }

        /// <summary>
        /// Gets the dependency injection service provider for the current request.
        /// </summary>
        public IServiceProvider DependencyProvider { get; private set; }

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        public IPortalSettings PortalSettings
        {
            get
            {
                return this.portalSettings;
            }
        }

        /// <summary>
        /// Gets the user information for the current user.
        /// </summary>
        public UserInfo UserInfo
        {
            get { return this.userController.GetCurrentUserInfo(); }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.MvcPipeline.Routing;
    using DotNetNuke.Web.MvcPipeline.Utils;

    /// <summary>
    /// Base controller for MVC module controllers, exposing common DNN context and services.
    /// </summary>
    public class ModuleControllerBase : DnnPageController, IMvcController
    {
        private readonly Lazy<ModuleInfo> activeModule;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleControllerBase"/> class.
        /// </summary>
        /// <param name="dependencyProvider">The dependency injection service provider.</param>
        public ModuleControllerBase(IServiceProvider dependencyProvider) 
            : base(dependencyProvider)
        {
            this.activeModule = new Lazy<ModuleInfo>(this.InitModuleInfo);
        }

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

        /// <summary>
        /// Gets the user information for the current user.
        /// </summary>
        public UserInfo UserInfo
        {
            get { return this.PortalSettings.UserInfo; }
        }

        /// <summary>
        /// Gets the active module associated with the current request.
        /// </summary>
        public ModuleInfo ActiveModule
        {
            get { return this.activeModule.Value; }
        }

        private ModuleInfo InitModuleInfo()
        {
            return this.HttpContext.Request.FindModuleInfo();
        }
    }
}

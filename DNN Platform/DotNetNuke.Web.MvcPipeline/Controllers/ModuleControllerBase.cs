// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Mvc.Routing;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Base controller for MVC module controllers, exposing common DNN context and services.
    /// </summary>
    public class ModuleControllerBase : DnnPageController, IMvcController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleControllerBase"/> class.
        /// </summary>
        /// <param name="dependencyProvider">The dependency injection service provider.</param>
        public ModuleControllerBase(IServiceProvider dependencyProvider)
            : base(dependencyProvider)
        {
        }

        /// <summary>
        /// Gets the module info.
        /// </summary>
        /// <param name="input">The module model containing the module and tab IDs.</param>
        /// <returns>ModuleInfo.</returns>
        public static ModuleInfo GetModuleInfo(ModuleModelBase input)
        {
            return ModuleController.Instance.GetModule(input.ModuleId, input.TabId, false);
        }
    }
}

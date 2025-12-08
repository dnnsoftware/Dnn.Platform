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

    public class ModuleControllerBase : DnnPageController, IMvcController
    {
        private readonly Lazy<ModuleInfo> activeModule;

        public ModuleControllerBase(IServiceProvider dependencyProvider) :
            base(dependencyProvider)
        {
            this.activeModule = new Lazy<ModuleInfo>(this.InitModuleInfo);
        }

        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// <summary>Gets userInfo for the current user.</summary>
        public UserInfo UserInfo
        {
            get { return this.PortalSettings.UserInfo; }
        }

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

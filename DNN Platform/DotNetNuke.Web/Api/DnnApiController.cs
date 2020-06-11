// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Web.Http;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Web.Api
{
    [DnnExceptionFilter]
    public abstract class DnnApiController : ApiController
    {
        private readonly Lazy<ModuleInfo> _activeModule;

        protected DnnApiController()
        {
            this._activeModule = new Lazy<ModuleInfo>(this.InitModuleInfo);
        }

        private ModuleInfo InitModuleInfo()
        {
            return this.Request.FindModuleInfo();
        }

        /// <summary>
        /// PortalSettings for the current portal
        /// </summary>
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// <summary>
        /// UserInfo for the current user
        /// </summary>
        public UserInfo UserInfo { get { return this.PortalSettings.UserInfo; } }

        /// <summary>
        /// ModuleInfo for the current module
        /// <remarks>Will be null unless a valid pair of module and tab ids were provided in the request</remarks>
        /// </summary>
        public ModuleInfo ActiveModule { 
            get { return this._activeModule.Value; } 
        }
    }
}

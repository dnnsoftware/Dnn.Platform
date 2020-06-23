// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Controllers;

    using DotNetNuke.Entities.Modules;

    public class SupportedModulesAttribute : AuthorizeAttributeBase
    {
        private readonly string[] _supportedModules;

        public SupportedModulesAttribute(string supportedModules)
        {
            this._supportedModules = supportedModules.Split(new[] { ',' });
        }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            var module = this.FindModuleInfo(context.ActionContext.Request);

            if (module != null)
            {
                return this.ModuleIsSupported(module);
            }

            return false;
        }

        protected virtual ModuleInfo FindModuleInfo(HttpRequestMessage request)
        {
            return request.FindModuleInfo();
        }

        protected override bool SkipAuthorization(HttpActionContext actionContext)
        {
            return false;
        }

        private bool ModuleIsSupported(ModuleInfo module)
        {
            return this._supportedModules.Contains(module.DesktopModule.ModuleName);
        }
    }
}

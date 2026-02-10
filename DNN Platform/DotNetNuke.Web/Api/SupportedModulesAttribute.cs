// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Controllers;

    using DotNetNuke.Entities.Modules;

    /// <summary>A web API authorization filter which requires the current module to be from particular desktop module.</summary>
    public class SupportedModulesAttribute(params string[] supportedModules) : AuthorizeAttributeBase
    {
        /// <summary>Initializes a new instance of the <see cref="SupportedModulesAttribute"/> class.</summary>
        /// <param name="supportedModules">A comma-delimited list of desktop module names.</param>
        public SupportedModulesAttribute(string supportedModules)
            : this(supportedModules.Split(','))
        {
        }

        /// <inheritdoc />
        public override bool IsAuthorized(AuthFilterContext context)
        {
            var module = this.FindModuleInfo(context.ActionContext.Request);
            if (module != null)
            {
                return this.ModuleIsSupported(module);
            }

            return false;
        }

        /// <summary>Gets the module associated with the <paramref name="request"/>.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The <see cref="ModuleInfo"/> instance or <see langword="null"/>.</returns>
        protected virtual ModuleInfo FindModuleInfo(HttpRequestMessage request)
        {
            return request.FindModuleInfo();
        }

        /// <inheritdoc />
        protected override bool SkipAuthorization(HttpActionContext actionContext)
        {
            return false;
        }

        private bool ModuleIsSupported(ModuleInfo module)
        {
            return supportedModules.Contains(module.DesktopModule.ModuleName);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Tokens
{
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Modules.Html5;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A <see cref="TokenReplace"/> for MVC HTML5 modules.</summary>
    public class MvcHtml5ModuleTokenReplace : MvcHtmlTokenReplace
    {
        /// <summary>Initializes a new instance of the <see cref="MvcHtml5ModuleTokenReplace"/> class.</summary>
        /// <param name="controllerContext">The controller context in which the module is rendering.</param>
        /// <param name="html5File">The path to the module's HTML file.</param>
        /// <param name="moduleContext">The module context.</param>
        /// <param name="moduleActions">The module actions collection.</param>
        public MvcHtml5ModuleTokenReplace(
            ControllerContext controllerContext,
            string html5File,
            ModuleInstanceContext moduleContext,
            ModuleActionCollection moduleActions)
            : this(controllerContext, null, html5File, moduleContext, moduleActions)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MvcHtml5ModuleTokenReplace"/> class.</summary>
        /// <param name="controllerContext">The controller context in which the module is rendering.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="html5File">The path to the module's HTML file.</param>
        /// <param name="moduleContext">The module context.</param>
        /// <param name="moduleActions">The module actions collection.</param>
        public MvcHtml5ModuleTokenReplace(
            ControllerContext controllerContext,
            IBusinessControllerProvider businessControllerProvider,
            string html5File,
            ModuleInstanceContext moduleContext,
            ModuleActionCollection moduleActions)
            : base(controllerContext)
        {
            this.AccessingUser = moduleContext.PortalSettings.UserInfo;
            this.DebugMessages = Personalization.GetUserMode() != Entities.Portals.PortalSettings.Mode.View;
            this.ModuleId = moduleContext.ModuleId;
            this.PortalSettings = moduleContext.PortalSettings;

            this.AddPropertySource("moduleaction", new ModuleActionsPropertyAccess(moduleContext, moduleActions));
            this.AddPropertySource("resx", new ModuleLocalizationPropertyAccess(moduleContext, html5File));
            this.AddPropertySource("modulecontext", new ModuleContextPropertyAccess(moduleContext));
            this.AddPropertySource("request", new MvcRequestPropertyAccess(controllerContext));

            // DNN-7750
            businessControllerProvider ??= Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            var customTokenProvider = businessControllerProvider.GetInstance<ICustomTokenProvider>(moduleContext);
            if (customTokenProvider != null)
            {
                var tokens = customTokenProvider.GetTokens(controllerContext?.HttpContext?.Handler as System.Web.UI.Page, moduleContext);
                foreach (var token in tokens)
                {
                    this.AddPropertySource(token.Key, token.Value);
                }
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules.Html5
{
    using System.Web.UI;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Tokens;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A <see cref="TokenReplace"/> for HTML modules.</summary>
    public class Html5ModuleTokenReplace : HtmlTokenReplace
    {
        /// <summary>Initializes a new instance of the <see cref="Html5ModuleTokenReplace"/> class.</summary>
        /// <param name="page">The page on which the module is rendering.</param>
        /// <param name="html5File">The path to the module's HTML file.</param>
        /// <param name="moduleContext">The module context.</param>
        /// <param name="moduleActions">The module actions collection.</param>
        public Html5ModuleTokenReplace(
            Page page,
            string html5File,
            ModuleInstanceContext moduleContext,
            ModuleActionCollection moduleActions)
            : this(page, null, html5File, moduleContext, moduleActions)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Html5ModuleTokenReplace"/> class.</summary>
        /// <param name="page">The page on which the module is rendering.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="html5File">The path to the module's HTML file.</param>
        /// <param name="moduleContext">The module context.</param>
        /// <param name="moduleActions">The module actions collection.</param>
        public Html5ModuleTokenReplace(
            Page page,
            IBusinessControllerProvider businessControllerProvider,
            string html5File,
            ModuleInstanceContext moduleContext,
            ModuleActionCollection moduleActions)
            : base(page)
        {
            this.AccessingUser = moduleContext.PortalSettings.UserInfo;
            this.DebugMessages = Personalization.GetUserMode() != Entities.Portals.PortalSettings.Mode.View;
            this.ModuleId = moduleContext.ModuleId;
            this.PortalSettings = moduleContext.PortalSettings;

            this.PropertySource["moduleaction"] = new ModuleActionsPropertyAccess(moduleContext, moduleActions);
            this.PropertySource["resx"] = new ModuleLocalizationPropertyAccess(moduleContext, html5File);
            this.PropertySource["modulecontext"] = new ModuleContextPropertyAccess(moduleContext);
            this.PropertySource["request"] = new RequestPropertyAccess(page.Request);

            // DNN-7750
            businessControllerProvider ??= Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            var customTokenProvider = businessControllerProvider.GetInstance<ICustomTokenProvider>(moduleContext);
            if (customTokenProvider != null)
            {
                var tokens = customTokenProvider.GetTokens(page, moduleContext);
                foreach (var token in tokens)
                {
                    this.PropertySource.Add(token.Key, token.Value);
                }
            }
        }
    }
}

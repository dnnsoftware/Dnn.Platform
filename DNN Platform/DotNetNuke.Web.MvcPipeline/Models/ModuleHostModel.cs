// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Modules;

    /// <summary>
    /// Hosts a module control (or its cached content) for use within the MVC pipeline.
    /// </summary>
    public sealed class ModuleHostModel
    {
        private readonly ModuleInfo moduleConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleHostModel"/> class.
        /// </summary>
        /// <param name="moduleConfiguration">The module configuration to host.</param>
        /// <param name="hostSettings">The host settings.</param>
        public ModuleHostModel(ModuleInfo moduleConfiguration, IHostSettings hostSettings)
        {
            this.moduleConfiguration = moduleConfiguration;
            if (hostSettings.EnableCustomModuleCssClass)
            {
                string moduleName = this.moduleConfiguration.DesktopModule.ModuleName;
                if (moduleName != null)
                {
                    moduleName = Globals.CleanName(moduleName);
                }

                this.CssClass = $"DNNModuleContent Mod{moduleName}C";
            }
        }

        /// <summary>
        /// Gets the CSS class applied to the module content.
        /// </summary>
        public string CssClass { get; private set; }
    }
}

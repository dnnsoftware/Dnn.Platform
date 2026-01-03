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
        private const string DefaultCssProvider = "DnnPageHeaderProvider";
        private const string DefaultJsProvider = "DnnBodyProvider";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleHostModel));

        private static readonly Regex CdfMatchRegex = new Regex(
            @"<\!--CDF\((?<type>JAVASCRIPT|CSS|JS-LIBRARY)\|(?<path>.+?)(\|(?<provider>.+?)\|(?<priority>\d+?))?\)-->",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ModuleInfo moduleConfiguration;

        private IModuleControl control = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleHostModel"/> class.
        /// </summary>
        /// <param name="moduleConfiguration">The module configuration to host.</param>
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

                this.CssClass = string.Format("DNNModuleContent Mod{0}C", moduleName);
            }
        }

        /// <summary>
        /// Gets the attached <see cref="IModuleControl"/> instance.
        /// </summary>
        public IModuleControl ModuleControl
        {
            get
            {
                // Make sure the Control tree has been created
                // this.EnsureChildControls();
                return this.control as IModuleControl;
            }
        }

        /// <summary>
        /// Gets the CSS class applied to the module content.
        /// </summary>
        public string CssClass { get; private set; }

        /// <summary>
        /// Determines whether the specified module is currently in view mode.
        /// </summary>
        /// <param name="moduleInfo">The module configuration.</param>
        /// <param name="settings">The current portal settings.</param>
        /// <returns><c>true</c> if the module is in view mode; otherwise, <c>false</c>.</returns>
        internal static bool IsViewMode(ModuleInfo moduleInfo, PortalSettings settings)
        {
            bool viewMode;

            if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.ViewPermissions, Null.NullString, moduleInfo))
            {
                viewMode = false;
            }
            else
            {
                viewMode = !ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, moduleInfo);
            }

            return viewMode || Personalization.GetUserMode() == PortalSettings.Mode.View;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Text.RegularExpressions;

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

    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class    : ModuleHost
    /// <summary>ModuleHost hosts a Module Control (or its cached Content).</summary>
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

        public ModuleHostModel(ModuleInfo moduleConfiguration)
        {
            this.moduleConfiguration = moduleConfiguration;
            if (Host.EnableCustomModuleCssClass)
            {
                string moduleName = this.moduleConfiguration.DesktopModule.ModuleName;
                if (moduleName != null)
                {
                    moduleName = Globals.CleanName(moduleName);
                }

                this.CssClass = string.Format("DNNModuleContent Mod{0}C", moduleName);
            }
        }

        /// <summary>Gets the attached ModuleControl.</summary>
        /// <returns>An IModuleControl.</returns>
        public IModuleControl ModuleControl
        {
            get
            {
                // Make sure the Control tree has been created
                // this.EnsureChildControls();
                return this.control as IModuleControl;
            }
        }

        public string CssClass { get; private set; }

        /// <summary>Gets a flag that indicates whether the Module is in View Mode.</summary>
        /// <returns>A Boolean.</returns>
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

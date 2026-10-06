// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc
{
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;

    public class MvcModuleControlFactory : BaseModuleControlFactory
    {
        /// <inheritdoc />
        public override int Priority => 100;

        /// <inheritdoc />
        public override bool SupportsControl(ModuleInfo moduleConfiguration, string controlSrc)
        {
            return controlSrc.EndsWith(".mvc", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
            if (IsAsyncControl(controlSrc))
            {
                return new AsyncMvcHostControl(controlKey);
            }

            return new MvcHostControl(controlKey);
        }

        /// <inheritdoc />
        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            if (IsAsyncControl(moduleConfiguration.ModuleControl.ControlSrc))
            {
                return new AsyncMvcHostControl();
            }

            return new MvcHostControl();
        }

        /// <inheritdoc />
        public override ModuleControlBase CreateModuleControl(ModuleInfo moduleConfiguration)
        {
            ModuleControlBase moduleControl = base.CreateModuleControl(moduleConfiguration);

            var segments = moduleConfiguration.ModuleControl.ControlSrc.Split('/');

            moduleControl.LocalResourceFile = $"~/DesktopModules/MVC/{moduleConfiguration.DesktopModule.FolderName}/{Localization.LocalResourceDirectory}/{(segments.Length == 2 ? segments[0] : segments[1])}.resx";

            return moduleControl;
        }

        /// <inheritdoc />
        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            if (IsAsyncControl(controlSrc))
            {
                return new AsyncMvcSettingsControl();
            }

            return new MvcSettingsControl();
        }

        private static bool IsAsyncControl(string controlSrc)
        {
            var segments = controlSrc.Split('/');
            return segments.Length == 4 && segments[2].Equals("async", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}

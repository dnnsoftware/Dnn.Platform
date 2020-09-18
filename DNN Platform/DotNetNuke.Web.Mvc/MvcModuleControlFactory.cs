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
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
            return new MvcHostControl(controlKey);
        }

        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            return new MvcHostControl();
        }

        public override ModuleControlBase CreateModuleControl(ModuleInfo moduleConfiguration)
        {
            ModuleControlBase moduleControl = base.CreateModuleControl(moduleConfiguration);

            var segments = moduleConfiguration.ModuleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');

            moduleControl.LocalResourceFile = string.Format(
                "~/DesktopModules/MVC/{0}/{1}/{2}.resx",
                moduleConfiguration.DesktopModule.FolderName,
                Localization.LocalResourceDirectory,
                segments[0]);

            return moduleControl;
        }

        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            return new MvcSettingsControl();
        }
    }
}

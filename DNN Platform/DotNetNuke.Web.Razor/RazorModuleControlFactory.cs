// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Razor
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;

    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public class RazorModuleControlFactory : BaseModuleControlFactory
    {
        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
            return new RazorHostControl("~/" + controlSrc);
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            return this.CreateControl(containerControl, string.Empty, moduleConfiguration.ModuleControl.ControlSrc);
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            return this.CreateControl(containerControl, string.Empty, controlSrc);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Web.UI;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.UI.Modules.Html5
{
    public class Html5ModuleControlFactory : BaseModuleControlFactory
    {
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
            return new Html5HostControl("~/" + controlSrc);
        }

        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            return CreateControl(containerControl, String.Empty, moduleConfiguration.ModuleControl.ControlSrc);
        }

        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            return CreateControl(containerControl, String.Empty, controlSrc);
        }
    }
}

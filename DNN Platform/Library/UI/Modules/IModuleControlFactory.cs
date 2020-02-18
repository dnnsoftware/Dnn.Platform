// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web.UI;

using DotNetNuke.Entities.Modules;

namespace DotNetNuke.UI.Modules
{
    public interface IModuleControlFactory
    {
        Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc);
        Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration);
        ModuleControlBase CreateModuleControl(ModuleInfo moduleConfiguration);
        Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc);
    }
}

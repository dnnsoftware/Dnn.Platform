// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;

    public interface IModuleControlPipeline
    {
        Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlKey, string controlSrc);

        Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration);

        Control LoadSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc);

        Control CreateCachedControl(string cachedContent, ModuleInfo moduleConfiguration);

        Control CreateModuleControl(ModuleInfo moduleConfiguration);
    }
}

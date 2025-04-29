// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules;

using System.Web.UI;

using DotNetNuke.Entities.Modules;

public interface IModuleControlFactory
{
    /// <summary>Creates a new instance of a control.</summary>
    /// <param name="containerControl">The container control.</param>
    /// <param name="controlKey">The control key.</param>
    /// <param name="controlSrc">The control source.</param>
    /// <returns>Instance of the control.</returns>
    Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc);

    Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration);

    ModuleControlBase CreateModuleControl(ModuleInfo moduleConfiguration);

    Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc);
}

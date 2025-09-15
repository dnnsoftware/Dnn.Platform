// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;

    /// <summary>Factory that creates a WebForms control for a module.</summary>
    public interface IModuleControlFactory
    {
        /// <summary>
        /// Gets the priority of the factory.
        /// The factory with the highest priority will be used.
        /// </summary>
        /// <remarks>
        /// DNN Control factories (WebForms, Razor, HTML, MVC) have a priority of 100
        /// and the default factory (Reflected) has a priority of -1.
        /// </remarks>
        int Priority { get; }

        /// <summary>Validates if the factory supports the given configuration and <see cref="controlSrc"/>.</summary>
        /// <param name="moduleConfiguration">Module configuration.</param>
        /// <param name="controlSrc">Control source.</param>
        /// <returns><see langword="true"/> if the factory supports the control; otherwise, <see langword="false"/>.</returns>
        bool SupportsControl(ModuleInfo moduleConfiguration, string controlSrc);

        /// <summary>Creates a new instance of a control.</summary>
        /// <param name="containerControl">The container control.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="controlSrc">The control source.</param>
        /// <returns>Instance of the control.</returns>
        Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc);

        /// <summary>Creates a new instance of a control.</summary>
        /// <param name="containerControl">The container control.</param>
        /// <param name="moduleConfiguration">The module configuration.</param>
        /// <returns>Instance of the control.</returns>
        Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration);

        /// <summary>Creates a new instance of a control.</summary>
        /// <param name="moduleConfiguration">The module configuration.</param>
        /// <returns>Instance of the control.</returns>
        ModuleControlBase CreateModuleControl(ModuleInfo moduleConfiguration);

        /// <summary>Creates a new instance of a settings control.</summary>
        /// <param name="containerControl">The container control.</param>
        /// <param name="moduleConfiguration">The module configuration.</param>
        /// <param name="controlSrc">The control source.</param>
        /// <returns>Instance of the control.</returns>
        Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc);
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// Defines the contract for a pipeline that handles the loading and creation of module controls.
    /// </summary>
    public interface IModuleControlPipeline
    {
        /// <summary>
        /// Loads a module control based on the specified container control, module configuration, control key, and control source.
        /// </summary>
        /// <param name="containerControl">The container control in which the module control will be loaded.</param>
        /// <param name="moduleConfiguration">The configuration of the module to be loaded.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="controlSrc">The control source from the module control.</param>
        /// <returns>The loaded module control.</returns>
        Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlKey, string controlSrc);

        /// <summary>
        /// Loads a module control based on the specified container control and module configuration.
        /// </summary>
        /// <param name="containerControl">The container control in which the module control will be loaded.</param>
        /// <param name="moduleConfiguration">The configuration of the module to be loaded.</param>
        /// <returns>The loaded module control.</returns>
        Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration);

        /// <summary>
        /// Loads a settings control based on the specified container control, module configuration, and control source.
        /// </summary>
        /// <param name="containerControl">The container control in which the settings control will be loaded.</param>
        /// <param name="moduleConfiguration">The configuration of the module for which the settings control will be loaded.</param>
        /// <param name="controlSrc">The control source from the module control.</param>
        /// <returns>The loaded settings control.</returns>
        Control LoadSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc);

        /// <summary>
        /// Creates a cached control based on the specified cached content and module configuration.
        /// </summary>
        /// <param name="cachedContent">The cached content to use for creating the control.</param>
        /// <param name="moduleConfiguration">The configuration of the module for which the cached control will be created.</param>
        /// <returns>The created cached control.</returns>
        Control CreateCachedControl(string cachedContent, ModuleInfo moduleConfiguration);

        /// <summary>
        /// Creates a module control based on the specified module configuration.
        /// </summary>
        /// <param name="moduleConfiguration">The configuration of the module for which the control will be created.</param>
        /// <returns>The created module control.</returns>
        Control CreateModuleControl(ModuleInfo moduleConfiguration);
    }
}

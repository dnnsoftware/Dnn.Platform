// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Internal
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use version in DotNetNuke.Entities.Modules instead. Scheduled removal in v10.0.0.")]
    public interface IModuleController
    {
        /// <summary>
        /// Gets the module.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <returns>module info.</returns>
        ModuleInfo GetModule(int moduleId, int tabId);

        /// <summary>
        /// Adds or updates a module's setting value.
        /// </summary>
        /// <param name="moduleId">ID of the tabmodule, the setting belongs to.</param>
        /// <param name="settingName">name of the setting property.</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>Empty SettingValue will remove the setting.</remarks>
        void UpdateModuleSetting(int moduleId, string settingName, string settingValue);

        /// <summary>
        /// Adds or updates a tab module's setting value.
        /// </summary>
        /// <param name="tabModuleId">ID of the tabmodule, the setting belongs to.</param>
        /// <param name="settingName">name of the setting property.</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>Empty SettingValue will remove the setting.</remarks>
        void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue);
    }
}

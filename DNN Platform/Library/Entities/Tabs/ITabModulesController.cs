// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs;

using System.Collections;
using System.Collections.Generic;

using DotNetNuke.Entities.Modules;

public interface ITabModulesController
{
    /// <summary>Returns an array of Modules well configured to be used into a Skin.</summary>
    /// <param name="tab">TabInfo object.</param>
    /// <returns>An <see cref="ArrayList"/> or <see cref="ModuleInfo"/>.</returns>
    ArrayList GetTabModules(TabInfo tab);

    /// <summary>Gets a collection of all setting values of <see cref="ModuleInfo"/> that contains the setting name in its collection of settings.</summary>
    /// <param name="settingName">Name of the setting to look for.</param>
    /// <returns>A <see cref="Dictionary{TKey,TValue}"/> with tab module ID as the key and setting value as the value.</returns>
    Dictionary<int, string> GetTabModuleSettingsByName(string settingName);

    /// <summary>Gets a collection of all ID's of <see cref="ModuleInfo"/> that contains the setting name and specific value in its collection of settings.</summary>
    /// <param name="settingName">Name of the setting to look for.</param>
    /// <param name="expectedValue">Value of the setting to look for.</param>
    /// <returns>An <see cref="IList{T}"/> of tab module IDs.</returns>
    IList<int> GetTabModuleIdsBySetting(string settingName, string expectedValue);
}

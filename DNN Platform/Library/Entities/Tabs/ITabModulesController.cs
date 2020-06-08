// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Entities.Tabs
{
    public interface ITabModulesController
    {
        /// <summary>
        /// Returns an array of Modules well configured to be used into a Skin
        /// </summary>
        /// <param name="tab">TabInfo object</param>       
        ArrayList GetTabModules(TabInfo tab);

        /// <summary>
        /// Gets a collection of all setting values of <see cref="ModuleInfo"/> that contains the
        /// setting name in its collection of settings.
        /// </summary>
        /// <param name="settingName">Name of the setting to look for</param>
        Dictionary<int, string> GetTabModuleSettingsByName(string settingName);

        /// <summary>
        /// Gets a collection of all ID's of <see cref="ModuleInfo"/> that contains the setting name and
        /// specific value in its collection of settings.
        /// </summary>
        /// <param name="settingName">Name of the setting to look for</param>
        /// <param name="expectedValue">Value of the setting to look for</param>
        IList<int> GetTabModuleIdsBySetting(string settingName, string expectedValue);
    }
}

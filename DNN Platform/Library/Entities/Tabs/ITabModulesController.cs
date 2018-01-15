#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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

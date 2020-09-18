// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Internal
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    using DotNetNuke.Framework;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use ModuleController instead. Scheduled removal in v10.0.0.")]
    public class TestableModuleController : ServiceLocator<IModuleController, TestableModuleController>, IModuleController
    {
        public ModuleInfo GetModule(int moduleId, int tabId)
        {
            return ModuleController.Instance.GetModule(moduleId, tabId, false);
        }

        public void UpdateModuleSetting(int moduleId, string settingName, string settingValue)
        {
            ModuleController.Instance.UpdateModuleSetting(moduleId, settingName, settingValue);
        }

        public void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue)
        {
            ModuleController.Instance.UpdateTabModuleSetting(tabModuleId, settingName, settingValue);
        }

        protected override Func<IModuleController> GetFactory()
        {
            return () => new TestableModuleController();
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace Dnn.ContactList.Spa.Components
{
    public class SettingsService : ServiceLocator<ISettingsService, SettingsService>, ISettingsService
    {
        private readonly IModuleController _moduleController;
        private const string IsFormEnabledKey = "IsFormEnabledKey";

        public SettingsService()
        {
            _moduleController = ModuleController.Instance;
        }
        protected override Func<ISettingsService> GetFactory()
        {
            return () => new SettingsService();
        }

        public bool IsFormEnabled(int moduleId, int tabId)
        {
            var module = _moduleController.GetModule(moduleId, tabId, true);
            var moduleSettings = module.ModuleSettings;

            return moduleSettings[IsFormEnabledKey] != null && Boolean.Parse((string) moduleSettings[IsFormEnabledKey]);
        }

        public void SaveFormEnabled(bool isEnabled, int moduleId)
        {
            _moduleController.UpdateModuleSetting(moduleId, IsFormEnabledKey, isEnabled.ToString());
        }
    }
}

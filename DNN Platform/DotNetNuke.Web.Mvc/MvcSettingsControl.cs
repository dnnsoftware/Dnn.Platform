// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvc
{
    public class MvcSettingsControl : MvcHostControl, ISettingsControl
    {
        public MvcSettingsControl() : base("Settings")
        {
            ExecuteModuleImmediately = false;
        }

        public void LoadSettings()
        {
            ExecuteModule();
        }

        public void UpdateSettings()
        {
            ExecuteModule();

            ModuleController.Instance.UpdateModule(ModuleContext.Configuration);
        }
    }
}

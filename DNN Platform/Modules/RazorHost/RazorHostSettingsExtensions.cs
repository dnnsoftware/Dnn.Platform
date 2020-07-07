// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.RazorHost
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;

    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public static class RazorHostSettingsExtensions
    {
        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public static Settings LoadRazorSettingsControl(this UserControl parent, ModuleInfo configuration, string localResourceFile)
        {
            var control = (Settings)parent.LoadControl("~/DesktopModules/RazorModules/RazorHost/Settings.ascx");
            control.ModuleConfiguration = configuration;
            control.LocalResourceFile = localResourceFile;
            EnsureEditScriptControlIsRegistered(configuration.ModuleDefID);
            return control;
        }

        private static void EnsureEditScriptControlIsRegistered(int moduleDefId)
        {
            if (ModuleControlController.GetModuleControlByControlKey("EditRazorScript", moduleDefId) != null)
            {
                return;
            }

            var m = new ModuleControlInfo
            {
                ControlKey = "EditRazorScript",
                ControlSrc = "DesktopModules/RazorModules/RazorHost/EditScript.ascx",
                ControlTitle = "Edit Script",
                ControlType = SecurityAccessLevel.Host,
                ModuleDefID = moduleDefId,
            };
            ModuleControlController.UpdateModuleControl(m);
        }
    }
}

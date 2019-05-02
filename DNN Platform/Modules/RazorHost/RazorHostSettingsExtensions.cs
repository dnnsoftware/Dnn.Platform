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
using System;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;

namespace DotNetNuke.Modules.RazorHost
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public static class RazorHostSettingsExtensions
    {
        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public static Settings LoadRazorSettingsControl(this UserControl parent, ModuleInfo configuration, string localResourceFile)
        {
            var control = (Settings) parent.LoadControl("~/DesktopModules/RazorModules/RazorHost/Settings.ascx");
            control.ModuleConfiguration = configuration;
            control.LocalResourceFile = localResourceFile;
            EnsureEditScriptControlIsRegistered(configuration.ModuleDefID);
            return control;
        }


        private static void EnsureEditScriptControlIsRegistered(int moduleDefId)
        {
            if (ModuleControlController.GetModuleControlByControlKey("EditRazorScript", moduleDefId) != null) return;
            var m = new ModuleControlInfo
                        {
                            ControlKey = "EditRazorScript",
                            ControlSrc = "DesktopModules/RazorModules/RazorHost/EditScript.ascx",
                            ControlTitle = "Edit Script",
                            ControlType = SecurityAccessLevel.Host,
                            ModuleDefID = moduleDefId
                        };
            ModuleControlController.UpdateModuleControl(m);
        }
    }
}
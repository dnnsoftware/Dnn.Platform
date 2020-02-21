// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Razor;
using System;

#endregion

namespace DotNetNuke.Modules.RazorHost
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public partial class RazorHost : RazorModuleBase, IActionable
    {
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override string RazorScriptFile
        {
            get
            {
                string m_RazorScriptFile = base.RazorScriptFile;
                var scriptFileSetting = ModuleContext.Settings["ScriptFile"] as string;
                if (! (string.IsNullOrEmpty(scriptFileSetting)))
                {
                    m_RazorScriptFile = string.Format(razorScriptFileFormatString, scriptFileSetting);
                }
                return m_RazorScriptFile;
            }
        }

        #region IActionable Members

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public ModuleActionCollection ModuleActions
        {
            get
            {
                var Actions = new ModuleActionCollection();
                Actions.Add(ModuleContext.GetNextActionID(),
                            Localization.GetString(ModuleActionType.EditContent, LocalResourceFile),
                            ModuleActionType.AddContent,
                            "",
                            "edit.gif",
                            ModuleContext.EditUrl(),
                            false,
                            SecurityAccessLevel.Host,
                            true,
                            false);
                Actions.Add(ModuleContext.GetNextActionID(),
                            Localization.GetString("CreateModule.Action", LocalResourceFile),
                            ModuleActionType.AddContent,
                            "",
                            "edit.gif",
                            ModuleContext.EditUrl("CreateModule"),
                            false,
                            SecurityAccessLevel.Host,
                            true,
                            false);
                return Actions;
            }
        }

        #endregion
    }
}

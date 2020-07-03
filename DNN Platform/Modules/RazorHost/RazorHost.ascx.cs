// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.RazorHost
{
    using System;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Razor;

    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public partial class RazorHost : RazorModuleBase, IActionable
    {
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public ModuleActionCollection ModuleActions
        {
            get
            {
                var Actions = new ModuleActionCollection();
                Actions.Add(
                    this.ModuleContext.GetNextActionID(),
                    Localization.GetString(ModuleActionType.EditContent, this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    "edit.gif",
                    this.ModuleContext.EditUrl(),
                    false,
                    SecurityAccessLevel.Host,
                    true,
                    false);
                Actions.Add(
                    this.ModuleContext.GetNextActionID(),
                    Localization.GetString("CreateModule.Action", this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    "edit.gif",
                    this.ModuleContext.EditUrl("CreateModule"),
                    false,
                    SecurityAccessLevel.Host,
                    true,
                    false);
                return Actions;
            }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override string RazorScriptFile
        {
            get
            {
                string m_RazorScriptFile = base.RazorScriptFile;
                var scriptFileSetting = this.ModuleContext.Settings["ScriptFile"] as string;
                if (!string.IsNullOrEmpty(scriptFileSetting))
                {
                    m_RazorScriptFile = string.Format(this.razorScriptFileFormatString, scriptFileSetting);
                }

                return m_RazorScriptFile;
            }
        }
    }
}

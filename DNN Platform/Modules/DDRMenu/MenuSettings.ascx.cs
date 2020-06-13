// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;

    using DotNetNuke.Entities.Modules;

    public partial class MenuSettings : ModuleSettingsBase
    {
        public override void LoadSettings()
        {
            if (this.IsPostBack)
            {
                return;
            }

            this.MenuStyle.Value = this.Settings["MenuStyle"] ?? string.Empty;
            this.NodeXmlPath.Value = this.Settings["NodeXmlPath"] ?? string.Empty;
            this.NodeSelector.Value = this.Settings["NodeSelector"] ?? string.Empty;
            this.IncludeNodes.Value = this.Settings["IncludeNodes"] ?? string.Empty;
            this.ExcludeNodes.Value = this.Settings["ExcludeNodes"] ?? string.Empty;
            this.NodeManipulator.Value = this.Settings["NodeManipulator"] ?? string.Empty;
            this.IncludeContext.Value = Convert.ToBoolean(this.Settings["IncludeContext"] ?? "false");
            this.IncludeHidden.Value = Convert.ToBoolean(this.Settings["IncludeHidden"] ?? "false");
            this.TemplateArguments.Value = this.Settings["TemplateArguments"] ?? string.Empty;
            this.ClientOptions.Value = this.Settings["ClientOptions"] ?? string.Empty;
        }

        public override void UpdateSettings()
        {
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "MenuStyle", (this.MenuStyle.Value ?? string.Empty).ToString());
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "NodeXmlPath", (this.NodeXmlPath.Value ?? string.Empty).ToString());
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "NodeSelector", (this.NodeSelector.Value ?? string.Empty).ToString());
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeNodes", (this.IncludeNodes.Value ?? string.Empty).ToString());
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ExcludeNodes", (this.ExcludeNodes.Value ?? string.Empty).ToString());
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "NodeManipulator", (this.NodeManipulator.Value ?? string.Empty).ToString());
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeContext", (this.IncludeContext.Value ?? "false").ToString());
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeHidden", (this.IncludeHidden.Value ?? "false").ToString());
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "TemplateArguments", (this.TemplateArguments.Value ?? string.Empty).ToString());
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ClientOptions", (this.ClientOptions.Value ?? string.Empty).ToString());
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.IncludeHiddenSection.Visible = DNNAbstract.IncludeHiddenSupported();
        }
    }
}

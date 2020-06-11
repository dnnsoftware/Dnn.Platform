// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.DDRMenu
{
	partial class MenuSettings : ModuleSettingsBase
	{
		public override void LoadSettings()
		{
			if (this.IsPostBack)
			{
				return;
			}
			this.MenuStyle.Value = this.Settings["MenuStyle"] ?? "";
			this.NodeXmlPath.Value = this.Settings["NodeXmlPath"] ?? "";
			this.NodeSelector.Value = this.Settings["NodeSelector"] ?? "";
			this.IncludeNodes.Value = this.Settings["IncludeNodes"] ?? "";
			this.ExcludeNodes.Value = this.Settings["ExcludeNodes"] ?? "";
			this.NodeManipulator.Value = this.Settings["NodeManipulator"] ?? "";
			this.IncludeContext.Value = Convert.ToBoolean(this.Settings["IncludeContext"] ?? "false");
			this.IncludeHidden.Value = Convert.ToBoolean(this.Settings["IncludeHidden"] ?? "false");
			this.TemplateArguments.Value = this.Settings["TemplateArguments"] ?? "";
			this.ClientOptions.Value = this.Settings["ClientOptions"] ?? "";
		}

		public override void UpdateSettings()
		{

			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "MenuStyle", (this.MenuStyle.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "NodeXmlPath", (this.NodeXmlPath.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "NodeSelector", (this.NodeSelector.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeNodes", (this.IncludeNodes.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ExcludeNodes", (this.ExcludeNodes.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "NodeManipulator", (this.NodeManipulator.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeContext", (this.IncludeContext.Value ?? "false").ToString());
			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeHidden", (this.IncludeHidden.Value ?? "false").ToString());
			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "TemplateArguments", (this.TemplateArguments.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ClientOptions", (this.ClientOptions.Value ?? "").ToString());
		}

		protected override void OnPreRender(EventArgs e)
		{
			this.IncludeHiddenSection.Visible = DNNAbstract.IncludeHiddenSupported();
		}
	}
}

using System;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.DDRMenu
{
	partial class MenuSettings : ModuleSettingsBase
	{
		public override void LoadSettings()
		{
			if (IsPostBack)
			{
				return;
			}
			MenuStyle.Value = Settings["MenuStyle"] ?? "";
			NodeXmlPath.Value = Settings["NodeXmlPath"] ?? "";
			NodeSelector.Value = Settings["NodeSelector"] ?? "";
			IncludeNodes.Value = Settings["IncludeNodes"] ?? "";
			ExcludeNodes.Value = Settings["ExcludeNodes"] ?? "";
			NodeManipulator.Value = Settings["NodeManipulator"] ?? "";
			IncludeContext.Value = Convert.ToBoolean(Settings["IncludeContext"] ?? "false");
			IncludeHidden.Value = Convert.ToBoolean(Settings["IncludeHidden"] ?? "false");
			TemplateArguments.Value = Settings["TemplateArguments"] ?? "";
			ClientOptions.Value = Settings["ClientOptions"] ?? "";
		}

		public override void UpdateSettings()
		{
			var controller = new ModuleController();

			controller.UpdateModuleSetting(ModuleId, "MenuStyle", (MenuStyle.Value ?? "").ToString());
			controller.UpdateModuleSetting(ModuleId, "NodeXmlPath", (NodeXmlPath.Value ?? "").ToString());
			controller.UpdateModuleSetting(ModuleId, "NodeSelector", (NodeSelector.Value ?? "").ToString());
			controller.UpdateModuleSetting(ModuleId, "IncludeNodes", (IncludeNodes.Value ?? "").ToString());
			controller.UpdateModuleSetting(ModuleId, "ExcludeNodes", (ExcludeNodes.Value ?? "").ToString());
			controller.UpdateModuleSetting(ModuleId, "NodeManipulator", (NodeManipulator.Value ?? "").ToString());
			controller.UpdateModuleSetting(ModuleId, "IncludeContext", (IncludeContext.Value ?? "false").ToString());
			controller.UpdateModuleSetting(ModuleId, "IncludeHidden", (IncludeHidden.Value ?? "false").ToString());
			controller.UpdateModuleSetting(ModuleId, "TemplateArguments", (TemplateArguments.Value ?? "").ToString());
			controller.UpdateModuleSetting(ModuleId, "ClientOptions", (ClientOptions.Value ?? "").ToString());
		}

		protected override void OnPreRender(EventArgs e)
		{
			IncludeHiddenSection.Visible = DNNAbstract.IncludeHiddenSupported();
		}
	}
}
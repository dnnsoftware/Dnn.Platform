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

			ModuleController.Instance.UpdateModuleSetting(ModuleId, "MenuStyle", (MenuStyle.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(ModuleId, "NodeXmlPath", (NodeXmlPath.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(ModuleId, "NodeSelector", (NodeSelector.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(ModuleId, "IncludeNodes", (IncludeNodes.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(ModuleId, "ExcludeNodes", (ExcludeNodes.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(ModuleId, "NodeManipulator", (NodeManipulator.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(ModuleId, "IncludeContext", (IncludeContext.Value ?? "false").ToString());
			ModuleController.Instance.UpdateModuleSetting(ModuleId, "IncludeHidden", (IncludeHidden.Value ?? "false").ToString());
			ModuleController.Instance.UpdateModuleSetting(ModuleId, "TemplateArguments", (TemplateArguments.Value ?? "").ToString());
			ModuleController.Instance.UpdateModuleSetting(ModuleId, "ClientOptions", (ClientOptions.Value ?? "").ToString());
		}

		protected override void OnPreRender(EventArgs e)
		{
			IncludeHiddenSection.Visible = DNNAbstract.IncludeHiddenSupported();
		}
	}
}
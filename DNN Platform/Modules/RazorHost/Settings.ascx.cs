#region Usings

using System;
using System.IO;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Modules.RazorHost
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public partial class Settings : ModuleSettingsBase
    {
        private string razorScriptFolder = "~/DesktopModules/RazorModules/RazorHost/Scripts/";

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public override void LoadSettings()
        {
            string basePath = Server.MapPath(razorScriptFolder);
            var scriptFileSetting = Settings["ScriptFile"] as string;

            foreach (string script in Directory.GetFiles(Server.MapPath(razorScriptFolder), "*.??html"))
            {
                string scriptPath = script.Replace(basePath, "");
                var item = new ListItem(scriptPath, scriptPath);
                if (! (string.IsNullOrEmpty(scriptFileSetting)) && scriptPath.ToLowerInvariant() == scriptFileSetting.ToLowerInvariant())
                {
                    item.Selected = true;
                }
                scriptList.Items.Add(item);
            }

            base.LoadSettings();
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public override void UpdateSettings()
        {
            ModuleController.Instance.UpdateModuleSetting(ModuleId, "ScriptFile", scriptList.SelectedValue);
        }
    }
}

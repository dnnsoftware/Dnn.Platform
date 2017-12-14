#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
#region Usings

using System.IO;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Modules.RazorHost
{
    public partial class Settings : ModuleSettingsBase
    {
        private string razorScriptFolder = "~/DesktopModules/RazorModules/RazorHost/Scripts/";

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

        public override void UpdateSettings()
        {
            ModuleController.Instance.UpdateModuleSetting(ModuleId, "ScriptFile", scriptList.SelectedValue);
        }
    }
}
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

using System;
using System.IO;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Modules.RazorHost
{
    public partial class EditScript : ModuleUserControlBase
    {
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";
        private string razorScriptFolder = "~/DesktopModules/RazorModules/RazorHost/Scripts/";


        protected string RazorScriptFile
        {
            get
            {
                string m_RazorScriptFile = Null.NullString;
                var scriptFileSetting = ModuleContext.Settings["ScriptFile"] as string;
                if (!(string.IsNullOrEmpty(scriptFileSetting)))
                {
                    m_RazorScriptFile = string.Format(razorScriptFileFormatString, scriptFileSetting);
                }
                return m_RazorScriptFile;
            }
        }

        private void LoadScripts()
        {
            string basePath = Server.MapPath(razorScriptFolder);
            var scriptFileSetting = ModuleContext.Settings["ScriptFile"] as string;

            foreach (string script in Directory.GetFiles(Server.MapPath(razorScriptFolder), "*.??html"))
            {
                string scriptPath = script.Replace(basePath, "");
                var item = new ListItem(scriptPath, scriptPath);
                if (!(string.IsNullOrEmpty(scriptFileSetting)) && scriptPath.ToLowerInvariant() == scriptFileSetting.ToLowerInvariant())
                {
                    item.Selected = true;
                }
                scriptList.Items.Add(item);
            }
        }

        private void DisplayFile()
        {
            var scriptFileSetting = ModuleContext.Settings["ScriptFile"] as string;
            string scriptFile = string.Format(razorScriptFileFormatString, scriptList.SelectedValue);
            string srcFile = Server.MapPath(scriptFile);

            lblSourceFile.Text = string.Format(Localization.GetString("SourceFile", LocalResourceFile), scriptFile);

            StreamReader objStreamReader = null;
            objStreamReader = File.OpenText(srcFile);
            txtSource.Text = objStreamReader.ReadToEnd();
            objStreamReader.Close();

            if (!(string.IsNullOrEmpty(scriptFileSetting)))
            {
                isCurrentScript.Checked = (scriptList.SelectedValue.ToLowerInvariant() == scriptFileSetting.ToLowerInvariant());
            }
        }

        private void SaveScript()
        {
            string srcFile = Server.MapPath(string.Format(razorScriptFileFormatString, scriptList.SelectedValue));

            // write file
            StreamWriter objStream = null;
            objStream = File.CreateText(srcFile);
            objStream.WriteLine(txtSource.Text);
            objStream.Close();

            if (isCurrentScript.Checked)
            {
                //Update setting
                ModuleController.Instance.UpdateModuleSetting(ModuleContext.ModuleId, "ScriptFile", scriptList.SelectedValue);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdSave.Click += cmdSave_Click;
            cmdSaveClose.Click += cmdSaveClose_Click;
            cmdAdd.Click += cmdAdd_Click;
            scriptList.SelectedIndexChanged += scriptList_SelectedIndexChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                LoadScripts();
                DisplayFile();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveScript();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdSaveClose_Click(object sender, EventArgs e)
        {
            try
            {
                SaveScript();
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
       
        private void cmdAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(ModuleContext.EditUrl("Add"), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void scriptList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayFile();
        }


    }
}
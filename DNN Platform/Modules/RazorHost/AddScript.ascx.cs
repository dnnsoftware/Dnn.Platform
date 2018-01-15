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
#region Usings

using System;
using System.IO;

using DotNetNuke.Common;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Modules.RazorHost
{
    public partial class AddScript : ModuleUserControlBase
    {
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";
        
        private void DisplayExtension()
        {
            fileExtension.Text = "." + scriptFileType.SelectedValue.ToLowerInvariant();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
 
            cmdCancel.Click += cmdCancel_Click;
            cmdAdd.Click += cmdAdd_Click;
            scriptFileType.SelectedIndexChanged += scriptFileType_SelectedIndexChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
 
            DisplayExtension();
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(ModuleContext.EditUrl("Edit"), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cmdAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ModuleContext.PortalSettings.UserInfo.IsSuperUser)
                {
                    Response.Redirect(Globals.NavigateURL("Access Denied"), true);
                }

                if (Page.IsValid)
                {
                    string scriptFileName = "_" + Path.GetFileNameWithoutExtension(fileName.Text) + "." + scriptFileType.SelectedValue.ToLowerInvariant();

                    string srcFile = Server.MapPath(string.Format(razorScriptFileFormatString, scriptFileName));

                    // write file
                    StreamWriter objStream = null;
                    objStream = File.CreateText(srcFile);
                    objStream.WriteLine(Localization.GetString("NewScript", LocalResourceFile));
                    objStream.Close();

                    Response.Redirect(ModuleContext.EditUrl("Edit"), true);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void scriptFileType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayExtension();
        }
    }
}
#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Globalization;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    public partial class ViewProfileSettings : ModuleSettingsBase
    {
        #region "Base Method Implementations"

        public override void LoadSettings()
        {
            try
            {
                ClientAPI.AddButtonConfirm(cmdLoadDefault, Localization.GetString("LoadDefault.Confirm", LocalResourceFile));
                cmdLoadDefault.ToolTip = Localization.GetString("LoadDefault.Help", LocalResourceFile);

                if (!Page.IsPostBack)
                {
                    if (!string.IsNullOrEmpty((string) TabModuleSettings["ProfileTemplate"]))
                    {
                        txtTemplate.Text = (string) TabModuleSettings["ProfileTemplate"];
                    }
                    if (Settings.ContainsKey("IncludeButton"))
                    {
                        IncludeButton.Checked = Convert.ToBoolean(Settings["IncludeButton"]);
                    }
                }
            }
            catch (Exception exc)
            {
                //Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cmdLoadDefault.Click += cmdLoadDefault_Click;
        }

        public override void UpdateSettings()
        {
            try
            {
                var objModules = new ModuleController();

                objModules.UpdateTabModuleSetting(TabModuleId, "ProfileTemplate", txtTemplate.Text);
                objModules.UpdateTabModuleSetting(TabModuleId, "IncludeButton", IncludeButton.Checked.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exc)
            {
                //Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        protected void cmdLoadDefault_Click(object sender, EventArgs e)
        {
            txtTemplate.Text = Localization.GetString("DefaultTemplate", LocalResourceFile);
        }
    }
}
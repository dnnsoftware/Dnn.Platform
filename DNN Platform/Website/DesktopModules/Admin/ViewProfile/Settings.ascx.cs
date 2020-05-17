// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
                ModuleController.Instance.UpdateTabModuleSetting(TabModuleId, "ProfileTemplate", txtTemplate.Text);
                ModuleController.Instance.UpdateTabModuleSetting(TabModuleId, "IncludeButton", IncludeButton.Checked.ToString(CultureInfo.InvariantCulture));
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

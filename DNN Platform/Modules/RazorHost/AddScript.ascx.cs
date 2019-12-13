// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;

using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace DotNetNuke.Modules.RazorHost
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public partial class AddScript : ModuleUserControlBase
    {
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";
        private readonly INavigationManager _navigationManager;

        public AddScript()
        {
            _navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private void DisplayExtension()
        {
            fileExtension.Text = "." + scriptFileType.SelectedValue.ToLowerInvariant();
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdAdd.Click += cmdAdd_Click;
            scriptFileType.SelectedIndexChanged += scriptFileType_SelectedIndexChanged;
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DisplayExtension();
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
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

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected void cmdAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ModuleContext.PortalSettings.UserInfo.IsSuperUser)
                {
                    Response.Redirect(_navigationManager.NavigateURL("Access Denied"), true);
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

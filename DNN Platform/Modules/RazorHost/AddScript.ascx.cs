// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.RazorHost
{
    using System;
    using System.IO;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public partial class AddScript : ModuleUserControlBase
    {
        private readonly INavigationManager _navigationManager;
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";

        public AddScript()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cmdCancel.Click += this.cmdCancel_Click;
            this.cmdAdd.Click += this.cmdAdd_Click;
            this.scriptFileType.SelectedIndexChanged += this.scriptFileType_SelectedIndexChanged;
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.DisplayExtension();
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Response.Redirect(this.ModuleContext.EditUrl("Edit"), true);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected void cmdAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!this.ModuleContext.PortalSettings.UserInfo.IsSuperUser)
                {
                    this.Response.Redirect(this._navigationManager.NavigateURL("Access Denied"), true);
                }

                if (this.Page.IsValid)
                {
                    string scriptFileName = "_" + Path.GetFileNameWithoutExtension(this.fileName.Text) + "." + this.scriptFileType.SelectedValue.ToLowerInvariant();

                    string srcFile = this.Server.MapPath(string.Format(this.razorScriptFileFormatString, scriptFileName));

                    // write file
                    StreamWriter objStream = null;
                    objStream = File.CreateText(srcFile);
                    objStream.WriteLine(Localization.GetString("NewScript", this.LocalResourceFile));
                    objStream.Close();

                    this.Response.Redirect(this.ModuleContext.EditUrl("Edit"), true);
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void DisplayExtension()
        {
            this.fileExtension.Text = "." + this.scriptFileType.SelectedValue.ToLowerInvariant();
        }

        private void scriptFileType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.DisplayExtension();
        }
    }
}

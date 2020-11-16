﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.RazorHost
{
    using System;
    using System.IO;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Implements the EditScript view logic.
    /// </summary>
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public partial class EditScript : ModuleUserControlBase
    {
        private readonly INavigationManager navigationManager;
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";
        private string razorScriptFolder = "~/DesktopModules/RazorModules/RazorHost/Scripts/";

        /// <summary>
        /// Initializes a new instance of the <see cref="EditScript"/> class.
        /// </summary>
        public EditScript()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>
        /// Gets the razor script file.
        /// </summary>
        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        protected string RazorScriptFile
        {
            get
            {
                string m_RazorScriptFile = Null.NullString;
                var scriptFileSetting = this.ModuleContext.Settings["ScriptFile"] as string;
                if (!string.IsNullOrEmpty(scriptFileSetting))
                {
                    m_RazorScriptFile = string.Format(this.razorScriptFileFormatString, scriptFileSetting);
                }

                return m_RazorScriptFile;
            }
        }

        /// <inheritdoc/>
        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        protected override void OnInit(EventArgs e)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            base.OnInit(e);

            this.cmdCancel.Click += this.CmdCancel_Click;
            this.cmdSave.Click += this.CmdSave_Click;
            this.cmdSaveClose.Click += this.CmdSaveClose_Click;
            this.cmdAdd.Click += this.CmdAdd_Click;
            this.scriptList.SelectedIndexChanged += this.ScriptList_SelectedIndexChanged;
        }

        /// <inheritdoc/>
        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        protected override void OnLoad(EventArgs e)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            base.OnLoad(e);

            if (!this.Page.IsPostBack)
            {
                this.LoadScripts();
                this.DisplayFile();
            }
        }

        private void LoadScripts()
        {
            string basePath = this.Server.MapPath(this.razorScriptFolder);
            var scriptFileSetting = this.ModuleContext.Settings["ScriptFile"] as string;

            foreach (string script in Directory.GetFiles(this.Server.MapPath(this.razorScriptFolder), "*.??html"))
            {
                string scriptPath = script.Replace(basePath, string.Empty);
                var item = new ListItem(scriptPath, scriptPath);
                if (!string.IsNullOrEmpty(scriptFileSetting) && scriptPath.ToLowerInvariant() == scriptFileSetting.ToLowerInvariant())
                {
                    item.Selected = true;
                }

                this.scriptList.Items.Add(item);
            }
        }

        private void DisplayFile()
        {
            var scriptFileSetting = this.ModuleContext.Settings["ScriptFile"] as string;
            string scriptFile = string.Format(this.razorScriptFileFormatString, this.scriptList.SelectedValue);
            string srcFile = this.Server.MapPath(scriptFile);

            this.lblSourceFile.Text = string.Format(Localization.GetString("SourceFile", this.LocalResourceFile), scriptFile);

            StreamReader objStreamReader = null;
            objStreamReader = File.OpenText(srcFile);
            this.txtSource.Text = objStreamReader.ReadToEnd();
            objStreamReader.Close();

            if (!string.IsNullOrEmpty(scriptFileSetting))
            {
                this.isCurrentScript.Checked = this.scriptList.SelectedValue.ToLowerInvariant() == scriptFileSetting.ToLowerInvariant();
            }
        }

        private void SaveScript()
        {
            string srcFile = this.Server.MapPath(string.Format(this.razorScriptFileFormatString, this.scriptList.SelectedValue));

            // write file
            StreamWriter objStream = null;
            objStream = File.CreateText(srcFile);
            objStream.WriteLine(this.txtSource.Text);
            objStream.Close();

            if (this.isCurrentScript.Checked)
            {
                // Update setting
                ModuleController.Instance.UpdateModuleSetting(this.ModuleContext.ModuleId, "ScriptFile", this.scriptList.SelectedValue);
            }
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Response.Redirect(this.navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void CmdSave_Click(object sender, EventArgs e)
        {
            try
            {
                this.SaveScript();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void CmdSaveClose_Click(object sender, EventArgs e)
        {
            try
            {
                this.SaveScript();
                this.Response.Redirect(this.navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void CmdAdd_Click(object sender, EventArgs e)
        {
            try
            {
                this.Response.Redirect(this.ModuleContext.EditUrl("Add"), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void ScriptList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.DisplayFile();
        }
    }
}

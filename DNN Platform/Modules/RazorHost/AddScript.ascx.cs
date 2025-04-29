// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.RazorHost;

using System;
using System.IO;

using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Implements the AddScript view logic.</summary>
[DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
public partial class AddScript : ModuleUserControlBase
{
    private readonly INavigationManager navigationManager;
    private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";

    /// <summary>Initializes a new instance of the <see cref="AddScript"/> class.</summary>
    public AddScript()
    {
        this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
    }

    /// <inheritdoc/>
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        this.cmdCancel.Click += this.CmdCancel_Click;
        this.cmdAdd.Click += this.CmdAdd_Click;
        this.scriptFileType.SelectedIndexChanged += this.ScriptFileType_SelectedIndexChanged;
    }

    /// <inheritdoc/>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        this.DisplayExtension();
    }

    /// <summary>Cancel button click event handler.</summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    protected void CmdCancel_Click(object sender, EventArgs e)
    {
        try
        {
            this.Response.Redirect(this.ModuleContext.EditUrl("Edit"), true);
        }
        catch (Exception exc)
        {
            Exceptions.ProcessModuleLoadException(this, exc);
        }
    }

    /// <summary>Add button event handler.</summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    protected void CmdAdd_Click(object sender, EventArgs e)
    {
        try
        {
            if (!this.ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                this.Response.Redirect(this.navigationManager.NavigateURL("Access Denied"), true);
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
        catch (Exception exc)
        {
            Exceptions.ProcessModuleLoadException(this, exc);
        }
    }

    private void DisplayExtension()
    {
        this.fileExtension.Text = "." + this.scriptFileType.SelectedValue.ToLowerInvariant();
    }

    private void ScriptFileType_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.DisplayExtension();
    }
}

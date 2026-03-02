// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Modules
{
    using System;
    using System.Globalization;
    using System.IO;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which displays the source code for a module.</summary>
    public partial class ViewSource : PortalModuleBase
    {
        private readonly INavigationManager navigationManager;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="ViewSource"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ViewSource()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ViewSource"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="hostSettings">The host settings.</param>
        public ViewSource(INavigationManager navigationManager, IHostSettings hostSettings)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
        }

        protected bool CanEditSource => this.Request.IsLocal;

        protected int ModuleControlId
        {
            get
            {
                var moduleControlId = Null.NullInteger;
                if (this.Request.QueryString["ctlid"] != null)
                {
                    moduleControlId = int.Parse(this.Request.QueryString["ctlid"]);
                }

                return moduleControlId;
            }
        }

        private string ReturnUrl => UrlUtils.ValidReturnUrl(this.Request.Params["ReturnURL"]) ?? this.navigationManager.NavigateURL();

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cboFile.SelectedIndexChanged += this.OnFileIndexChanged;
            this.cmdUpdate.Click += this.OnUpdateClick;

            if (!this.Page.IsPostBack)
            {
                this.cmdCancel.NavigateUrl = this.ReturnUrl;

                var objModuleControl = ModuleControlController.GetModuleControl(this.hostSettings, this.ModuleControlId);
                if (objModuleControl != null)
                {
                    this.BindFiles(objModuleControl.ControlSrc);
                }

                if (this.Request.UrlReferrer != null)
                {
                    this.ViewState["UrlReferrer"] = Convert.ToString(this.Request.UrlReferrer);
                }
                else
                {
                    this.ViewState["UrlReferrer"] = string.Empty;
                }
            }

            this.cmdUpdate.Visible = this.CanEditSource;
            this.txtSource.Enabled = this.CanEditSource;
        }

        protected void OnFileIndexChanged(object sender, EventArgs e)
        {
            this.DisplayFile();
        }

        private void BindFiles(string controlSrc)
        {
            this.cboFile.Items.Clear();

            // cboFile.Items.Add(new ListItem(Localization.GetString("None_Specified"), "None"));
            // cboFile.Items.Add(new ListItem("User Control", "UserControl"));
            this.cboFile.AddItem(Localization.GetString("None_Specified"), "None");
            this.cboFile.AddItem("User Control", "UserControl");

            var srcPhysicalPath = this.Server.MapPath(controlSrc);
            if (File.Exists(srcPhysicalPath + ".vb") || File.Exists(srcPhysicalPath + ".cs"))
            {
                // cboFile.Items.Add(new ListItem("Code File", "CodeFile"));
                this.cboFile.AddItem("Code File", "CodeFile");
            }

            var fileName = Path.GetFileName(srcPhysicalPath);
            var folder = Path.GetDirectoryName(srcPhysicalPath);
            if (File.Exists(folder + "\\App_LocalResources\\" + fileName + ".resx"))
            {
                // cboFile.Items.Add(new ListItem("Resource File", "ResourceFile"));
                this.cboFile.AddItem("Resource File", "ResourceFile");
            }
        }

        private string GetSourceFileName(string controlSrc)
        {
            var srcPhysicalPath = this.Server.MapPath(controlSrc);
            var srcFile = Null.NullString;
            switch (this.cboFile.SelectedValue)
            {
                case "UserControl":
                    srcFile = srcPhysicalPath;
                    break;
                case "CodeFile":
                    if (File.Exists(srcPhysicalPath + ".vb"))
                    {
                        srcFile = srcPhysicalPath + ".vb";
                    }
                    else if (File.Exists(srcPhysicalPath + ".cs"))
                    {
                        srcFile = srcPhysicalPath + ".cs";
                    }

                    break;
                case "ResourceFile":
                    var fileName = Path.GetFileName(srcPhysicalPath);
                    var folder = Path.GetDirectoryName(srcPhysicalPath);
                    srcFile = folder + "\\App_LocalResources\\" + fileName + ".resx";
                    break;
            }

            return srcFile;
        }

        private void DisplayFile()
        {
            var objModuleControl = ModuleControlController.GetModuleControl(this.hostSettings, this.ModuleControlId);
            if (objModuleControl != null)
            {
                var srcVirtualPath = objModuleControl.ControlSrc;
                var srcFile = Null.NullString;
                var displaySource = this.cboFile.SelectedValue != "None";

                if (displaySource)
                {
                    srcFile = this.GetSourceFileName(srcVirtualPath);
                    this.lblSourceFile.Text = string.Format(CultureInfo.CurrentCulture, Localization.GetString("SourceFile", this.LocalResourceFile), srcFile);

                    var objStreamReader = File.OpenText(srcFile);
                    this.txtSource.Text = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                }

                this.lblSourceFile.Visible = displaySource;
                this.trSource.Visible = displaySource;
            }
        }

        private void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                if (this.cboFile.SelectedValue == "None")
                {
                    // No file type selected
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoFileTypeSelected", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }
                else
                {
                    var objModuleControl = ModuleControlController.GetModuleControl(this.hostSettings, this.ModuleControlId);
                    if (objModuleControl != null)
                    {
                        var srcVirtualPath = objModuleControl.ControlSrc;
                        var srcPhysicalPath = this.GetSourceFileName(srcVirtualPath);
                        if (File.Exists(srcPhysicalPath))
                        {
                            File.SetAttributes(srcPhysicalPath, FileAttributes.Normal);
                            var objStream = File.CreateText(srcPhysicalPath);
                            objStream.WriteLine(this.txtSource.Text);
                            objStream.Close();
                        }
                    }

                    this.Response.Redirect(this.ReturnUrl, true);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}

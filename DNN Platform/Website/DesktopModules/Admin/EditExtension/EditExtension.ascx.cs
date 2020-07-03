// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.EditExtension
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Installer.Writers;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditExtension control is used to edit a Extension.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class EditExtension : ModuleUserControlBase
    {
        private readonly INavigationManager _navigationManager;

        private Control _control;
        private PackageInfo _package;

        public EditExtension()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string Mode
        {
            get
            {
                return Convert.ToString(this.ModuleContext.Settings["Extensions_Mode"]);
            }
        }

        public int PackageID
        {
            get
            {
                var packageID = Null.NullInteger;
                if (this.Request.QueryString["PackageID"] != null)
                {
                    packageID = int.Parse(this.Request.QueryString["PackageID"]);
                }

                return packageID;
            }
        }

        protected bool IsSuperTab
        {
            get
            {
                return this.ModuleContext.PortalSettings.ActiveTab.IsSuperTab;
            }
        }

        protected string DisplayMode => (this.Request.QueryString["Display"] ?? string.Empty).ToLowerInvariant();

        protected PackageInfo Package
        {
            get
            {
                return this._package ?? (this._package = this.PackageID == Null.NullInteger ? new PackageInfo() : PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == this.PackageID, true));
            }
        }

        protected IPackageEditor PackageEditor
        {
            get
            {
                if (this._control == null)
                {
                    if (this.Package != null)
                    {
                        var pkgType = PackageController.Instance.GetExtensionPackageType(t => t.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase));
                        if ((pkgType != null) && (!string.IsNullOrEmpty(pkgType.EditorControlSrc)))
                        {
                            this._control = ControlUtilities.LoadControl<Control>(this, pkgType.EditorControlSrc);
                        }
                    }
                }

                return this._control as IPackageEditor;
            }
        }

        protected PropertyEditorMode ViewMode
        {
            get
            {
                var viewMode = PropertyEditorMode.View;
                if (this.Request.IsLocal && this.IsSuperTab)
                {
                    viewMode = PropertyEditorMode.Edit;
                }

                return viewMode;
            }
        }

        protected string ReturnUrl
        {
            get { return (string)this.ViewState["ReturnUrl"]; }
            set { this.ViewState["ReturnUrl"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdCancel.Click += this.OnCancelClick;
            this.cmdDelete.Click += this.OnDeleteClick;
            this.cmdPackage.Click += this.OnPackageClick;
            this.cmdUpdate.Click += this.OnUpdateClick;
            this.Page.PreRenderComplete += (sender, args) =>
                                          {
                                              if (UrlUtils.InPopUp())
                                              {
                                                  var title = string.Format("{0} > {1}", this.Page.Title, this.Package.FriendlyName);
                                                  this.Page.Title = title;
                                              }
                                          };

            this.BindData();

            if (!this.IsPostBack)
            {
                this.ReturnUrl = this.Request.UrlReferrer != null ? this.Request.UrlReferrer.ToString() : this._navigationManager.NavigateURL();
                switch (this.DisplayMode)
                {
                    case "editor":
                        this.packageSettingsSection.Visible = false;
                        break;
                    case "settings":
                        this.extensionSection.Visible = false;
                        break;
                }
            }

            switch (this.DisplayMode)
            {
                case "editor":
                    this.cmdCancel.Visible = this.cmdCancel.Enabled = false;
                    this.cmdUpdate.Visible = this.cmdUpdate.Enabled = false;
                    this.cmdPackage.Visible = this.cmdPackage.Enabled = false;
                    this.cmdDelete.Visible = this.cmdDelete.Enabled = false;
                    break;
                case "settings":
                    this.cmdCancel.Visible = this.cmdCancel.Enabled = false;
                    break;
            }
        }

        protected void OnCancelClick(object sender, EventArgs e)
        {
            this.Response.Redirect(this.ReturnUrl);
        }

        protected void OnDeleteClick(object sender, EventArgs e)
        {
            this.Response.Redirect(Util.UnInstallURL(this.ModuleContext.TabId, this.PackageID));
        }

        protected void OnPackageClick(object sender, EventArgs e)
        {
            try
            {
                this.UpdatePackage(false);
                this.Response.Redirect(Util.PackageWriterURL(this.ModuleContext, this.PackageID));
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                this.UpdatePackage(true);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        private void BindData()
        {
            this.email.ValidationExpression = Globals.glbEmailRegEx;
            this.trLanguagePackType.Visible = false;
            switch (this.Mode)
            {
                case "All":
                    this.lblHelp.Text = Localization.GetString("EditHelp", this.LocalResourceFile);
                    this.cmdUpdate.Text = Localization.GetString("cmdUpdate", this.LocalResourceFile);
                    break;
                case "LanguagePack":
                    this.lblHelp.Text = Localization.GetString("EditLanguageHelp", this.LocalResourceFile);
                    this.cmdUpdate.Text = Localization.GetString("cmdUpdateLanguage", this.LocalResourceFile);
                    break;
                case "Module":
                    this.lblHelp.Text = Localization.GetString("EditModuleHelp", this.LocalResourceFile);
                    this.cmdUpdate.Text = Localization.GetString("cmdUpdateModule", this.LocalResourceFile);
                    break;
                case "Skin":
                    this.lblHelp.Text = Localization.GetString("EditSkinHelp", this.LocalResourceFile);
                    this.cmdUpdate.Text = Localization.GetString("cmdUpdateSkin", this.LocalResourceFile);
                    break;
            }

            this.cmdPackage.Visible = this.IsSuperTab;
            this.cmdUpdate.Visible = this.IsSuperTab;
            if (this.Package != null)
            {
                if (this.PackageEditor == null || this.PackageID == Null.NullInteger)
                {
                    this.extensionSection.Visible = false;
                }
                else
                {
                    this.phEditor.Controls.Clear();
                    this.phEditor.Controls.Add(this.PackageEditor as Control);
                    var moduleControl = this.PackageEditor as IModuleControl;
                    if (moduleControl != null)
                    {
                        moduleControl.ModuleContext.Configuration = this.ModuleContext.Configuration;
                    }

                    this.PackageEditor.PackageID = this.PackageID;
                    this.PackageEditor.Initialize();

                    this.Package.IconFile = Util.ParsePackageIconFileName(this.Package);
                }

                switch (this.Package.PackageType)
                {
                    case "Auth_System":
                    case "Container":
                    case "Module":
                    case "Skin":
                        this.iconFile.Enabled = true;
                        this.Package.IconFile = Util.ParsePackageIconFileName(this.Package);
                        break;
                    default:
                        this.iconFile.Enabled = false;
                        this.Package.IconFile = "Not Available";
                        break;
                }

                if (this.Mode != "All")
                {
                    this.packageType.Visible = false;
                }

                // Determine if Package is ready for packaging
                PackageWriterBase writer = PackageWriterFactory.GetWriter(this.Package);
                this.cmdPackage.Visible = this.IsSuperTab && writer != null && Directory.Exists(Path.Combine(Globals.ApplicationMapPath, writer.BasePath));

                this.cmdDelete.Visible = this.IsSuperTab && (!this.Package.IsSystemPackage) && PackageController.CanDeletePackage(this.Package, this.ModuleContext.PortalSettings);
                this.ctlAudit.Entity = this.Package;

                this.packageForm.DataSource = this.Package;
                this.packageFormReadOnly.DataSource = this.Package;
                if (!this.Page.IsPostBack)
                {
                    this.packageForm.DataBind();
                    this.packageFormReadOnly.DataBind();
                }

                this.packageForm.Visible = this.IsSuperTab;
                this.packageFormReadOnly.Visible = !this.IsSuperTab;
            }
        }

        private void UpdatePackage(bool displayMessage)
        {
            if (this.packageForm.IsValid)
            {
                var package = this.packageForm.DataSource as PackageInfo;
                if (package != null)
                {
                    var pkgIconFile = Util.ParsePackageIconFileName(package);
                    package.IconFile = (pkgIconFile.Trim().Length > 0) ? Util.ParsePackageIconFile(package) : null;
                    PackageController.Instance.SaveExtensionPackage(package);
                }

                if (displayMessage)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PackageUpdated", this.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
            }

            if (this.PackageEditor != null)
            {
                this.PackageEditor.UpdatePackage();
            }
        }
    }
}

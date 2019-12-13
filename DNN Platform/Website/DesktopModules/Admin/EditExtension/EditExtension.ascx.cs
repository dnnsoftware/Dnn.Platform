﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using Microsoft.Extensions.DependencyInjection;

using DotNetNuke.Common;
using DotNetNuke.Abstractions;
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

#endregion

namespace DotNetNuke.Modules.Admin.EditExtension
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditExtension control is used to edit a Extension
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
            _navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected bool IsSuperTab
        {
            get
            {
                return (ModuleContext.PortalSettings.ActiveTab.IsSuperTab);
            }
        }

        protected string ReturnUrl
        {
            get { return (string)ViewState["ReturnUrl"]; }
            set { ViewState["ReturnUrl"] = value; }
        }

        public string Mode
        {
            get
            {
                return Convert.ToString(ModuleContext.Settings["Extensions_Mode"]);
            }
        }

        protected string DisplayMode => (Request.QueryString["Display"] ?? "").ToLowerInvariant();

        protected PackageInfo Package
        {
            get
            {
                return _package ?? (_package = PackageID == Null.NullInteger ? new PackageInfo() : PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == PackageID, true));
            }
        }

        protected IPackageEditor PackageEditor
        {
            get
            {
                if (_control == null)
                {
                    if (Package != null)
                    {
                        var pkgType = PackageController.Instance.GetExtensionPackageType(t => t.PackageType.Equals(Package.PackageType, StringComparison.OrdinalIgnoreCase));
                        if ((pkgType != null) && (!string.IsNullOrEmpty(pkgType.EditorControlSrc)))
                        {
                            _control = ControlUtilities.LoadControl<Control>(this, pkgType.EditorControlSrc);
                        }
                    }
                }
                return _control as IPackageEditor;
            }
        }

        public int PackageID
        {
            get
            {
                var packageID = Null.NullInteger;
                if ((Request.QueryString["PackageID"] != null))
                {
                    packageID = Int32.Parse(Request.QueryString["PackageID"]);
                }
                return packageID;
            }
        }

        protected PropertyEditorMode ViewMode
        {
            get
            {
                var viewMode = PropertyEditorMode.View;
                if (Request.IsLocal && IsSuperTab)
                {
                    viewMode = PropertyEditorMode.Edit;
                }
                return viewMode;
            }
        }

        private void BindData()
        {
            email.ValidationExpression = Globals.glbEmailRegEx;
            trLanguagePackType.Visible = false;
            switch (Mode)
            {
                case "All":
                    lblHelp.Text = Localization.GetString("EditHelp", LocalResourceFile);
                    cmdUpdate.Text = Localization.GetString("cmdUpdate", LocalResourceFile);
                    break;
                case "LanguagePack":
                    lblHelp.Text = Localization.GetString("EditLanguageHelp", LocalResourceFile);
                    cmdUpdate.Text = Localization.GetString("cmdUpdateLanguage", LocalResourceFile);
                    break;
                case "Module":
                    lblHelp.Text = Localization.GetString("EditModuleHelp", LocalResourceFile);
                    cmdUpdate.Text = Localization.GetString("cmdUpdateModule", LocalResourceFile);
                    break;
                case "Skin":
                    lblHelp.Text = Localization.GetString("EditSkinHelp", LocalResourceFile);
                    cmdUpdate.Text = Localization.GetString("cmdUpdateSkin", LocalResourceFile);
                    break;
            }

            cmdPackage.Visible = IsSuperTab;
            cmdUpdate.Visible = IsSuperTab;
            if (Package != null)
            {
                
                if (PackageEditor == null || PackageID == Null.NullInteger)
                {
                    extensionSection.Visible = false;
                }
                else
                {
                    phEditor.Controls.Clear();
                    phEditor.Controls.Add(PackageEditor as Control);
                    var moduleControl = PackageEditor as IModuleControl;
                    if (moduleControl != null)
                    {
                        moduleControl.ModuleContext.Configuration = ModuleContext.Configuration;
                    }
                    PackageEditor.PackageID = PackageID;
                    PackageEditor.Initialize();

                    Package.IconFile = Util.ParsePackageIconFileName(Package);
                }
                
                switch (Package.PackageType)
                {                                        
                    case "Auth_System":
                    case "Container":
                    case "Module":
                    case "Skin":
                        iconFile.Enabled = true;
                        Package.IconFile = Util.ParsePackageIconFileName(Package);
                        break;
                    default:
                        iconFile.Enabled = false;
                        Package.IconFile = "Not Available";
                        break;
                }
                
                if (Mode != "All")
                {
                    packageType.Visible = false;
                }
                //Determine if Package is ready for packaging
                PackageWriterBase writer = PackageWriterFactory.GetWriter(Package);
                cmdPackage.Visible = IsSuperTab && writer != null && Directory.Exists(Path.Combine(Globals.ApplicationMapPath, writer.BasePath));

                cmdDelete.Visible = IsSuperTab && (!Package.IsSystemPackage) && (PackageController.CanDeletePackage(Package, ModuleContext.PortalSettings));
                ctlAudit.Entity = Package;

                packageForm.DataSource = Package;
                packageFormReadOnly.DataSource = Package;
                if(!Page.IsPostBack)
                {
                    packageForm.DataBind();
                    packageFormReadOnly.DataBind();
                }
                packageForm.Visible = IsSuperTab;
                packageFormReadOnly.Visible = !IsSuperTab;

            }
        }

        private void UpdatePackage(bool displayMessage)
        {
            if (packageForm.IsValid)
            {
                var package = packageForm.DataSource as PackageInfo;
                if (package != null)
                {
                    var pkgIconFile = Util.ParsePackageIconFileName(package);
                    package.IconFile = (pkgIconFile.Trim().Length > 0)? Util.ParsePackageIconFile(package) : null;
                    PackageController.Instance.SaveExtensionPackage(package);
                }
                if (displayMessage)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PackageUpdated", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
            }
            if (PackageEditor != null)
            {
                PackageEditor.UpdatePackage();
            }
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

            cmdCancel.Click += OnCancelClick;
            cmdDelete.Click += OnDeleteClick;
            cmdPackage.Click += OnPackageClick;
            cmdUpdate.Click += OnUpdateClick;
            Page.PreRenderComplete += (sender, args) =>
                                          {
                                              if (UrlUtils.InPopUp())
                                              {
                                                  var title = string.Format("{0} > {1}", Page.Title, Package.FriendlyName);
                                                  Page.Title = title;
                                              }
                                          };

            BindData();

            if (!IsPostBack)
            {
                ReturnUrl = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : _navigationManager.NavigateURL();
                switch (DisplayMode)
                {
                    case "editor":
                        packageSettingsSection.Visible = false;
                        break;
                    case "settings":
                        extensionSection.Visible = false;
                        break;
                }
            }

            switch (DisplayMode)
            {
                case "editor":
                    cmdCancel.Visible = cmdCancel.Enabled = false;
                    cmdUpdate.Visible = cmdUpdate.Enabled = false;
                    cmdPackage.Visible = cmdPackage.Enabled = false;
                    cmdDelete.Visible = cmdDelete.Enabled = false;
                    break;
                case "settings":
                    cmdCancel.Visible = cmdCancel.Enabled = false;
                    break;
            }
        }

        protected void OnCancelClick(object sender, EventArgs e)
        {
            Response.Redirect(ReturnUrl);
        }

        protected void OnDeleteClick(object sender, EventArgs e)
        {
            Response.Redirect(Util.UnInstallURL(ModuleContext.TabId, PackageID));
        }

        protected void OnPackageClick(object sender, EventArgs e)
        {
            try
            {
                UpdatePackage(false);
                Response.Redirect(Util.PackageWriterURL(ModuleContext, PackageID));
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
                UpdatePackage(true);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

    }
}

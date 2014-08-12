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
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ExtensionWizard control is used to create a Extension
    /// </summary>
    /// <history>
    /// 	[cnurse]	08/25/2008	Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class ExtensionWizard : ModuleUserControlBase
    {
        private Control _Control;
        private PackageInfo _Package;

        protected string ExtensionType
        {
            get
            {
                if (Mode == "All")
                {
                    return cboExtensionType.Text;
                }
                else
                {
                    if (!string.IsNullOrEmpty(Request.QueryString["SkinType"]))
                    {
                        return Request.QueryString["SkinType"];
                    }
                    else
                    {
                        return Mode;
                    }
                }
            }
        }

        protected string Mode
        {
            get
            {
                string _Mode = "All";
                if (!string.IsNullOrEmpty(Request.QueryString["Type"]))
                {
                    _Mode = Request.QueryString["Type"];
                }
                return _Mode;
            }
        }

        protected PackageInfo Package
        {
            get
            {
                if (ViewState["Package"] == null)
                {
                    if (PackageID == Null.NullInteger)
                    {
                        _Package = new PackageInfo();
                        _Package.PackageType = ExtensionType;
                    }
                    else
                    {
                        _Package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == PackageID);
                    }

                    ViewState["Package"] = _Package;
                }
                return ViewState["Package"] as PackageInfo;
            }
        }

        protected IPackageEditor PackageEditor
        {
            get
            {
                if (_Control == null)
                {
                    if (Package != null)
                    {
                        PackageType _PackageType = PackageController.Instance.GetExtensionPackageType(t => t.PackageType == Package.PackageType);
                        if ((_PackageType != null) && (!string.IsNullOrEmpty(_PackageType.EditorControlSrc)))
                        {
                            _Control = ControlUtilities.LoadControl<Control>(this, _PackageType.EditorControlSrc);
                        }
                    }
                }
                return _Control as IPackageEditor;
            }
        }

        protected int PackageID
        {
            get
            {
                int _PackageID = Null.NullInteger;
                if (ViewState["PackageID"] != null)
                {
                    _PackageID = Int32.Parse(ViewState["PackageID"].ToString());
                }
                return _PackageID;
            }
            set
            {
                ViewState["PackageID"] = value;
            }
        }

        private void BindExtensionTypes()
        {
            cboExtensionType.DataSource = PackageController.Instance.GetExtensionPackageTypes();
            cboExtensionType.DataBind();
        }

        private void BindPackageEditor()
        {
            phEditor.Controls.Clear();
            if (PackageEditor != null)
            {
                PackageEditor.PackageID = PackageID;
                PackageEditor.Initialize();
                PackageEditor.IsWizard = true;

                phEditor.Controls.Add(PackageEditor as Control);

                var moduleControl = PackageEditor as IModuleControl;
                if (moduleControl != null)
                {
                    moduleControl.ModuleContext.Configuration = ModuleContext.Configuration;
                }
            }
        }

        private void DisplayLanguageHelp()
        {
            switch (cboExtensionType.SelectedValue)
            {
                case "CoreLanguagePack":
                    lblLanguageHelp.Visible = true;
                    lblExtensionLanguageHelp.Visible = false;
                    break;
                case "ExtensionLanguagePack":
                    lblLanguageHelp.Visible = true;
                    lblExtensionLanguageHelp.Visible = true;
                    break;
                default:
                    lblLanguageHelp.Visible = false;
                    lblExtensionLanguageHelp.Visible = false;
                    break;
            }
        }

        protected string GetText(string type)
        {
            string text = Null.NullString;
            string pageName = wizNewExtension.ActiveStep.Title;
            if (wizNewExtension.ActiveStepIndex == 1)
            {
                if (string.IsNullOrEmpty(Package.PackageType))
                {
                    pageName += "_" + Mode;
                }
                else
                {
                    pageName += "_" + Package.PackageType;
                }
            }
            if (type == "Title")
            {
                text = Localization.GetString(pageName + ".Title", LocalResourceFile);
            }
            else if (type == "Help")
            {
                text = Localization.GetString(pageName + ".Help", LocalResourceFile);
            }
            return text;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            switch (Mode)
            {
                case "All":
                    extensionTypeRow.Visible = true;
                    break;
                case "Module":
                    extensionTypeRow.Visible = false;
                    break;
                case "CoreLanguagePack, ExtensionLanguagePack":
                    extensionTypeRow.Visible = false;
                    break;
                case "Skin":
                case "Container":
                    extensionTypeRow.Visible = false;
                    break;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            wizNewExtension.ActiveStepChanged += wizNewExtension_ActiveStepChanged;
            wizNewExtension.CancelButtonClick += wizNewExtension_CancelButtonClick;
            wizNewExtension.NextButtonClick += wizNewExtension_NextButtonClick;

            extensionForm.DataSource = Package;

            //Bind the Owner control
            ownerForm.DataSource = Package;
            if (Package != null)
            {
                if (PackageEditor != null && PackageID > Null.NullInteger)
                {
                    BindPackageEditor();
                }
            }
            if (!Page.IsPostBack)
            {
                BindExtensionTypes();
                extensionForm.DataBind();
                ownerForm.DataBind();
            }
            DisplayLanguageHelp();
        }

        protected void wizNewExtension_ActiveStepChanged(object sender, EventArgs e)
        {
            switch (wizNewExtension.ActiveStepIndex)
            {
                case 1:
                    if (Package.PackageType != "Module" && 
                        Package.PackageType != "CoreLanguagePack" && 
                        Package.PackageType != "ExtensionLanguagePack")
                    {
                        wizNewExtension.ActiveStepIndex = 2;
                    }
                    break;
                case 2:
                    wizNewExtension.DisplayCancelButton = false;
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// wizNewExtension_CancelButtonClick runs when the Cancel Button on the Wizard is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	08/25/2008	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void wizNewExtension_CancelButtonClick(object sender, EventArgs e)
        {
			//Redirect to Definitions page
            Response.Redirect(Globals.NavigateURL(), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// wizNewExtension_NextButtonClick when the next Button is clicked.  It provides
        ///	a mechanism for cancelling the page change if certain conditions aren't met.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	08/25/2008	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void wizNewExtension_NextButtonClick(object sender, WizardNavigationEventArgs e)
        {
            switch (e.CurrentStepIndex)
            {
                case 0:
                    if (extensionForm.IsValid)
                    {
                        var newPackage = extensionForm.DataSource as PackageInfo;
                        PackageInfo tmpPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == newPackage.Name);
                        if (tmpPackage == null)
                        {
                            switch (Mode)
                            {
                                case "All":
                                    newPackage.PackageType = cboExtensionType.SelectedValue;
                                    break;
                                default:
                                    newPackage.PackageType = Mode;
                                    break;
                            }
                            PackageController.Instance.SaveExtensionPackage(newPackage);
                            PackageID = newPackage.PackageID;
                            Locale locale;
                            LanguagePackInfo languagePack;
                            switch (newPackage.PackageType)
                            {
                                case "Auth_System":
                                    //Create a new Auth System
                                    var authSystem = new AuthenticationInfo
                                    {
                                        AuthenticationType = newPackage.Name,
                                        IsEnabled = Null.NullBoolean,
                                        PackageID = newPackage.PackageID
                                    };
                                    AuthenticationController.AddAuthentication(authSystem);
                                    break;
                                case "Container":
                                case "Skin":
                                    var skinPackage = new SkinPackageInfo
                                    {
                                        SkinName = newPackage.Name,
                                        PackageID = newPackage.PackageID,
                                        SkinType = newPackage.PackageType
                                    };
                                    SkinController.AddSkinPackage(skinPackage);
                                    break;
                                case "CoreLanguagePack":
                                    locale = LocaleController.Instance.GetLocale(PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage);
                                    languagePack = new LanguagePackInfo
                                    {
                                        PackageID = newPackage.PackageID,
                                        LanguageID = locale.LanguageId,
                                        DependentPackageID = -2
                                    };
                                    LanguagePackController.SaveLanguagePack(languagePack);
                                    break;
                                case "ExtensionLanguagePack":
                                    locale = LocaleController.Instance.GetLocale(PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage);
                                    languagePack = new LanguagePackInfo
                                    {
                                        PackageID = newPackage.PackageID,
                                        LanguageID = locale.LanguageId,
                                        DependentPackageID = Null.NullInteger
                                    };
                                    LanguagePackController.SaveLanguagePack(languagePack);
                                    break;
                                case "Module":
                                    //Create a new DesktopModule
                                    var desktopModule = new DesktopModuleInfo
                                    {
                                        PackageID = newPackage.PackageID,
                                        ModuleName = newPackage.Name,
                                        FriendlyName = newPackage.FriendlyName,
                                        FolderName = newPackage.Name,
                                        Description = newPackage.Description,
                                        Version = newPackage.Version.ToString(3),
                                        SupportedFeatures = 0
                                    };
                                    int desktopModuleId = DesktopModuleController.SaveDesktopModule(desktopModule, false, true);
                                    if (desktopModuleId > Null.NullInteger)
                                    {
                                        DesktopModuleController.AddDesktopModuleToPortals(desktopModuleId);
                                    }
                                    break;
                                case "SkinObject":
                                    var skinControl = new SkinControlInfo { PackageID = newPackage.PackageID, ControlKey = newPackage.Name };
                                    SkinControlController.SaveSkinControl(skinControl);
                                    break;
                            }

                        }
                        else
                        {
                            e.Cancel = true;
                            lblError.Text = string.Format(Localization.GetString("DuplicateName", LocalResourceFile), newPackage.Name);
                        	lblError.Visible = true;
                        }
                    }
                    if (PackageEditor != null && PackageID > Null.NullInteger)
                    {
                        BindPackageEditor();
                    }
                    break;
                case 1:
                    if (PackageEditor != null)
                    {
                        PackageEditor.UpdatePackage();
                    }
                    break;
                case 2:
                    if (ownerForm.IsValid)
                    {
                        PackageController.Instance.SaveExtensionPackage(ownerForm.DataSource as PackageInfo);
                    }
                    Response.Redirect(Globals.NavigateURL(), true);
                    break;
            }
        }
    }
}
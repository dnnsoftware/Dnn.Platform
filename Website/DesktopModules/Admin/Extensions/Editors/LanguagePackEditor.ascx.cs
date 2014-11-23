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
using System.Collections.Generic;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LanguagePackEditor ModuleUserControlBase is used to edit a Language
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	02/14/2008  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class LanguagePackEditor : PackageEditorBase
    {
		#region "Private Methods"

        private LanguagePackInfo _languagePack;

		#endregion

		#region "Protected Properties"

        protected override string EditorID
        {
            get
            {
                return "LanguagePackEditor";
            }
        }

        protected Locale Language
        {
            get
            {
                return LocaleController.Instance.GetLocale(LanguagePack.LanguageID);
            }
        }

        protected LanguagePackInfo LanguagePack
        {
            get
            {
                if (_languagePack == null)
                {
                    _languagePack = LanguagePackController.GetLanguagePackByPackage(PackageID);
                }
                return _languagePack;
            }
        }

		#endregion

		#region "Private Methods"

        private void BindLanguagePack()
        {
            cboLanguage.DataSource = LocaleController.Instance.GetLocales(Null.NullInteger).Values;
            cboLanguage.DataBind();
            if (cboLanguage.FindItemByValue(LanguagePack.LanguageID.ToString()) != null)
            {
                cboLanguage.FindItemByValue(LanguagePack.LanguageID.ToString()).Selected = true;
            }
            if (LanguagePack != null)
            {
                if (LanguagePack.PackageType == LanguagePackType.Extension)
                {
					//Get all the packages but only bind to combo if not a language package
                    var packages = new List<PackageInfo>();
                    foreach (PackageInfo package in PackageController.Instance.GetExtensionPackages(Null.NullInteger))
                    {
                        if (package.PackageType != "CoreLanguagePack" && package.PackageType != "ExtensionLanguagePack")
                        {
                            packages.Add(package);
                        }
                    }
                    cboPackage.DataSource = packages;
                    cboPackage.DataBind();
                    if (cboPackage.FindItemByValue(LanguagePack.DependentPackageID.ToString()) != null)
                    {
                        cboPackage.FindItemByValue(LanguagePack.DependentPackageID.ToString()).Selected = true;
                    }
                    packageRow.Visible = true;
                }
                else
                {
                    packageRow.Visible = false;
                }
            }
        }
		
		#endregion
		
		#region "Event Handlers"

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            cmdEdit.Visible = !IsWizard;
        }
		
		#endregion
		
		#region "Public Methods"

        public override void Initialize()
        {
            BindLanguagePack();
        }

        public override void UpdatePackage()
        {
            LanguagePack.LanguageID = int.Parse(cboLanguage.SelectedValue);
            if (LanguagePack.PackageType == LanguagePackType.Extension)
            {
                LanguagePack.DependentPackageID = int.Parse(cboPackage.SelectedValue);
            }
            LanguagePackController.SaveLanguagePack(LanguagePack);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cmdEdit.Click += cmdEdit_Click;
        }
		
		#endregion
		
		#region "Event Handlers"

        protected void cmdEdit_Click(object sender, EventArgs e)
        {
            int languagesTab = TabController.GetTabByTabPath(ModuleContext.PortalId, "//Admin//Languages", Null.NullString);
            Response.Redirect(Globals.NavigateURL(languagesTab, "", "Locale=" + Language.Code), true);
        }
		
		#endregion
    }
}
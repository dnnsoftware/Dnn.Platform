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

using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    public partial class SkinEditor : PackageEditorBase
    {
        protected override string EditorID
        {
            get
            {
                return "SkinEditor";
            }
        }

        private void BindSkin()
        {
            SkinPackageInfo skin = SkinController.GetSkinByPackageID(PackageID);
            if (!ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
            {
                skinFormReadOnly.DataSource = skin;
                skinFormReadOnly.DataBind();
                skinFormReadOnly.Visible = true;

                lblHelp.Visible = false;
            }
            if (skin != null && ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
            {

                skinForm.DataSource = skin;
                skinForm.DataBind();
                skinForm.Visible = true;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            BindSkin();
        }

        public override void Initialize()
        {
            if (Package.PackageType == "Skin")
            {
                lblTitle.Text = Localization.GetString("SkinTitle", LocalResourceFile);
                lblHelp.Text = Localization.GetString("SkinHelp", LocalResourceFile);
            }
            else
            {
                lblTitle.Text = Localization.GetString("ContainerTitle", LocalResourceFile);
                lblHelp.Text = Localization.GetString("ContainerHelp", LocalResourceFile);
            }
        }

        public override void UpdatePackage()
        {
            if (skinForm.IsValid)
            {
                var skin = skinForm.DataSource as SkinPackageInfo;
                if (skin != null)
                {
                    SkinController.UpdateSkinPackage(skin);
                }
            }
        }

        protected void ctlSkin_ItemAdded(object sender, PropertyEditorEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.StringValue))
            {
                SkinPackageInfo skin = SkinController.GetSkinByPackageID(PackageID);
                SkinController.AddSkin(skin.SkinPackageID, e.StringValue);
            }
            BindSkin();
        }

        protected void ctlSkin_ItemDeleted(object sender, PropertyEditorEventArgs e)
        {
            if (e.Key != null)
            {
                SkinController.DeleteSkin(Convert.ToInt32(e.Key));
            }
            BindSkin();
        }
    }
}
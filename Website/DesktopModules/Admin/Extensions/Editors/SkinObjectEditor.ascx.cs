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

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.SkinObjects
{
    public partial class SkinObjectEditor : PackageEditorBase
    {
        protected override string EditorID
        {
            get
            {
                return "SkinObjectEditor";
            }
        }

        private void BindSkinObject()
        {
            SkinControlInfo skinControl = SkinControlController.GetSkinControlByPackageID(PackageID);
            if (!ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
            {
                skinObjectFormReadOnly.DataSource = skinControl;
                skinObjectFormReadOnly.DataBind();
                helpPanel.Visible = false;
            }
            if (skinControl != null && ModuleContext.PortalSettings.ActiveTab.IsSuperTab)
            {
                skinObjectForm.DataSource = skinControl;
                skinObjectForm.DataBind();
                helpPanel.Visible = true;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            BindSkinObject();
        }

        public override void UpdatePackage()
        {
            if (skinObjectForm.IsValid)
            {
                var skinControl = skinObjectForm.DataSource as SkinControlInfo;
                if (skinControl != null)
                {
                    SkinControlController.SaveSkinControl(skinControl);
                }
            }
        }
    }
}
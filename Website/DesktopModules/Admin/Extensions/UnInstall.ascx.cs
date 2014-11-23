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

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : UnInstall
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Supplies the functionality for uninstalling Extensions(packages) from the Portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///     [cnurse]   07/26/2007    Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class UnInstall : ModuleUserControlBase
    {
		#region "Members"

        private PackageInfo _Package;

		#endregion

		#region "Public Properties"

        public int PackageID
        {
            get
            {
                int _PageNo = 0;
                if (ViewState["PackageID"] != null)
                {
                    _PageNo = Convert.ToInt32(ViewState["PackageID"]);
                }
                return _PageNo;
            }
            set
            {
                ViewState["PackageID"] = value;
            }
        }

        public PackageInfo Package
        {
            get
            {
                if (_Package == null && PackageID > Null.NullInteger)
                {
                    _Package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == PackageID);
                }
                return _Package;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Return Url
        /// </summary>
        /// <history>
        ///     [cnurse]   07/31/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string ReturnURL
        {
            get
            {
                string _ReturnUrl = Server.UrlDecode(Request.Params["returnUrl"]);
                if (string.IsNullOrEmpty(_ReturnUrl))
                {
                    int TabID = ModuleContext.PortalSettings.HomeTabId;
                    if (Request.Params["rtab"] != null)
                    {
                        TabID = int.Parse(Request.Params["rtab"]);
                    }
                    _ReturnUrl = Globals.NavigateURL(TabID);
                }
                return _ReturnUrl;
            }
        }

		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine checks the Access Security
        /// </summary>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void CheckSecurity()
        {
            if (!ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine uninstalls the package
        /// </summary>
        /// <history>
        ///     [cnurse]   07/31/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void UnInstallPackage()
        {
            phPaLogs.Visible = true;
            var installer = new Installer(Package, Request.MapPath("."));
            installer.UnInstall(chkDelete.Checked);
            phPaLogs.Controls.Add(installer.InstallerInfo.Log.GetLogsTable());
        }

		#endregion

		#region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	01/21/2008	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if ((Request.QueryString["packageid"] != null))
            {
                PackageID = Int32.Parse(Request.QueryString["packageid"]);
            }
            else
            {
                PackageID = Null.NullInteger;
            }
            cmdReturn1.NavigateUrl = ReturnURL;
            cmdReturn2.NavigateUrl = ReturnURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Page_Load runs when the page loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdUninstall.Click += cmdUninstall_Click;
            CheckSecurity();
            try
            {
                if (Package != null && string.IsNullOrEmpty(Package.Manifest))
                {
                    deleteRow.Visible = false;
                }
				
                //Make Uninstall and Delete Files option unavailable if package cannot be deleted. Also display a message to the user
                if (Package != null && !PackageController.CanDeletePackage(Package, ModuleContext.PortalSettings))
                {
                    cmdUninstall.Visible = false;
                    deleteRow.Visible = false;
                    lblMessage.CssClass = "NormalRed";
                    switch (Package.PackageType)
                    {
                        case "Skin":
                            lblMessage.Text = Localization.GetString("CannotDeleteSkin.ErrorMessage", LocalResourceFile);
                            break;
                        case "Container":
                            lblMessage.Text = Localization.GetString("CannotDeleteContainer.ErrorMessage", LocalResourceFile);
                            break;
                        case "Provider":
                            lblMessage.Text = Localization.GetString("CannotDeleteProvider.ErrorMessage", LocalResourceFile);
                            break;
                    }
                }
                else
                {
                    lblMessage.CssClass = "Normal";
                    lblMessage.Text = "";
                }
                packageForm.DataSource = Package;
                packageForm.DataBind();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The cmdUninstall_Click runs when the Uninstall Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse] 07/31/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdUninstall_Click(object sender, EventArgs e)
        {
            CheckSecurity();
            try
            {
                UnInstallPackage();

                if (phPaLogs.Controls.Count > 0)
                {
					dnnUnInstallExtension.Visible = false; 
                    tblLogs.Visible = true;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
		
		#endregion
    }
}
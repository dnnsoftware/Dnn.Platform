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
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Application;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.UI.ControlPanels
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The IconBar ControlPanel provides an icon bar based Page/Module manager
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	10/06/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class IconBar : ControlPanelBase
    {
		#region "Private Methods"

        private void BindData()
        {
            switch (optModuleType.SelectedItem.Value)
            {
                case "0": //new module
                    cboTabs.Visible = false;
                    cboModules.Visible = false;
                    cboDesktopModules.Visible = true;
                    txtTitle.Visible = true;
                    lblModule.Text = Localization.GetString("Module", LocalResourceFile);
                    lblTitle.Text = Localization.GetString("Title", LocalResourceFile);
                    cboPermission.Enabled = true;

                    //get list of modules
                    cboDesktopModules.DataSource = DesktopModuleController.GetPortalDesktopModules(PortalSettings.PortalId).Values;
                    cboDesktopModules.DataBind();
                    cboDesktopModules.Items.Insert(0, new ListItem("<" + Localization.GetString("SelectModule", LocalResourceFile) + ">", "-1"));

                    //select default module
                    int intDesktopModuleID = -1;
                    if (!String.IsNullOrEmpty(Localization.GetString("DefaultModule", LocalResourceFile)))
                    {
                        DesktopModuleInfo objDesktopModule;
                        objDesktopModule = DesktopModuleController.GetDesktopModuleByModuleName(Localization.GetString("DefaultModule", LocalResourceFile, PortalSettings, null, true),
                                                                                                PortalSettings.PortalId);
                        if (objDesktopModule != null)
                        {
                            intDesktopModuleID = objDesktopModule.DesktopModuleID;
                        }
                    }
                    if (intDesktopModuleID != -1 && (cboDesktopModules.Items.FindByValue(intDesktopModuleID.ToString()) != null))
                    {
                        cboDesktopModules.Items.FindByValue(intDesktopModuleID.ToString()).Selected = true;
                    }
                    else
                    {
                        cboDesktopModules.SelectedIndex = 0;
                    }
                    break;
                case "1": //existing module
                    cboTabs.Visible = true;
                    cboModules.Visible = true;
                    cboDesktopModules.Visible = false;
                    txtTitle.Visible = false;
                    lblModule.Text = Localization.GetString("Tab", LocalResourceFile);
                    lblTitle.Text = Localization.GetString("Module", LocalResourceFile);
                    cboPermission.Enabled = false;
                    cboTabs.DataSource = TabController.GetPortalTabs(PortalSettings.PortalId,
                                                                     PortalSettings.ActiveTab.TabID,
                                                                     true,
                                                                     "<" + Localization.GetString("SelectPage", LocalResourceFile) + ">",
                                                                     true,
                                                                     false,
                                                                     false,
                                                                     false,
                                                                     true);
                    cboTabs.DataBind();
                    break;
            }
        }

        private void DisableAction(Image image, string imageUrl, LinkButton imageButton, LinkButton button)
        {
            image.ImageUrl = "~/Admin/ControlPanel/images/" + imageUrl;
            imageButton.Enabled = false;
            button.Enabled = false;
        }

        private void Localize()
        {
            lblMode.Text = Localization.GetString("Mode", LocalResourceFile);
            imgAdmin.AlternateText = Localization.GetString("imgAdmin.AlternateText", LocalResourceFile);
            cmdAdmin.Text = Localization.GetString("cmdAdmin", LocalResourceFile);
            imgHost.AlternateText = Localization.GetString("imgHost.AlternateText", LocalResourceFile);
            cmdHost.Text = Localization.GetString("cmdHost", LocalResourceFile);
            lblPageFunctions.Text = Localization.GetString("PageFunctions", LocalResourceFile);
            lblCommonTasks.Text = Localization.GetString("CommonTasks", LocalResourceFile);
            lblModule.Text = Localization.GetString("Module", LocalResourceFile);
            lblPane.Text = Localization.GetString("Pane", LocalResourceFile);
            lblTitle.Text = Localization.GetString("Title", LocalResourceFile);
            lblInstance.Text = Localization.GetString("Instance", LocalResourceFile);

            imgAddTabIcon.AlternateText = Localization.GetString("AddTab.AlternateText", LocalResourceFile);
            cmdAddTab.Text = Localization.GetString("AddTab", LocalResourceFile);

            imgEditTabIcon.AlternateText = Localization.GetString("EditTab.AlternateText", LocalResourceFile);
            cmdEditTab.Text = Localization.GetString("EditTab", LocalResourceFile);

            imgDeleteTabIcon.AlternateText = Localization.GetString("DeleteTab.AlternateText", LocalResourceFile);
            cmdDeleteTab.Text = Localization.GetString("DeleteTab", LocalResourceFile);

            imgCopyTabIcon.AlternateText = Localization.GetString("CopyTab.AlternateText", LocalResourceFile);
            cmdCopyTab.Text = Localization.GetString("CopyTab", LocalResourceFile);

            imgExportTabIcon.AlternateText = Localization.GetString("ExportTab.AlternateText", LocalResourceFile);
            cmdExportTab.Text = Localization.GetString("ExportTab", LocalResourceFile);

            imgImportTabIcon.AlternateText = Localization.GetString("ImportTab.AlternateText", LocalResourceFile);
            cmdImportTab.Text = Localization.GetString("ImportTab", LocalResourceFile);

            imgAddModule.AlternateText = Localization.GetString("AddModule.AlternateText", LocalResourceFile);
            cmdAddModule.Text = Localization.GetString("AddModule", LocalResourceFile);

            imgSiteIcon.AlternateText = Localization.GetString("Site.AlternateText", LocalResourceFile);
            cmdSite.Text = Localization.GetString("Site", LocalResourceFile);

            imgUsersIcon.AlternateText = Localization.GetString("Users.AlternateText", LocalResourceFile);
            cmdUsers.Text = Localization.GetString("Users", LocalResourceFile);

            imgRolesIcon.AlternateText = Localization.GetString("Roles.AlternateText", LocalResourceFile);
            cmdRoles.Text = Localization.GetString("Roles", LocalResourceFile);

            imgFilesIcon.AlternateText = Localization.GetString("Files.AlternateText", LocalResourceFile);
            cmdFiles.Text = Localization.GetString("Files", LocalResourceFile);

            imgHelpIcon.AlternateText = Localization.GetString("Help.AlternateText", LocalResourceFile);
            cmdHelp.Text = Localization.GetString("Help", LocalResourceFile);

            imgExtensionsIcon.AlternateText = Localization.GetString("Extensions.AlternateText", LocalResourceFile);
            cmdExtensions.Text = Localization.GetString("Extensions", LocalResourceFile);
        }

        private void SetMode(bool Update)
        {
            if (Update)
            {
                SetUserMode(optMode.SelectedValue);
            }
            if (!TabPermissionController.CanAddContentToPage())
            {
                optMode.Items.Remove(optMode.Items.FindByValue("LAYOUT"));
            }
            switch (UserMode)
            {
                case PortalSettings.Mode.View:
                    optMode.Items.FindByValue("VIEW").Selected = true;
                    break;
                case PortalSettings.Mode.Edit:
                    optMode.Items.FindByValue("EDIT").Selected = true;
                    break;
                case PortalSettings.Mode.Layout:
                    optMode.Items.FindByValue("LAYOUT").Selected = true;
                    break;
            }
        }

        private void SetVisibility(bool Toggle)
        {
            if (Toggle)
            {
                SetVisibleMode(!IsVisible);
            }
        }

        private void LoadPositions()
        {
            LoadInstances();
            cboPosition.Items.Clear();
            if (cboInstances.Items.Count > 1)
            {
                cboPosition.Items.Add(new ListItem(Localization.GetString("Top", LocalResourceFile), "TOP"));
                cboPosition.Items.Add(new ListItem(Localization.GetString("Above", LocalResourceFile), "ABOVE"));
                cboPosition.Items.Add(new ListItem(Localization.GetString("Below", LocalResourceFile), "BELOW"));
            }
            cboPosition.Items.Add(new ListItem(Localization.GetString("Bottom", LocalResourceFile), "BOTTOM"));
            cboPosition.SelectedIndex = cboPosition.Items.Count - 1;
            DisplayInstances();
        }

        private void DisplayInstances()
        {
            if (cboPosition.SelectedItem != null)
            {
                switch (cboPosition.SelectedItem.Value)
                {
                    case "TOP":
                    case "BOTTOM":
                        cboInstances.Visible = false;
                        break;
                    case "ABOVE":
                    case "BELOW":
                        cboInstances.Visible = true;
                        break;
                }
            }
            lblInstance.Visible = cboInstances.Visible;
        }

        private void LoadInstances()
        {
            cboInstances.Items.Clear();
            foreach (ModuleInfo objModule in PortalSettings.ActiveTab.Modules)
            {
				//if user is allowed to view module and module is not deleted
                if (ModulePermissionController.CanViewModule(objModule) && objModule.IsDeleted == false)
                {
					//modules which are displayed on all tabs should not be displayed on the Admin or Super tabs
                    if (objModule.AllTabs == false || PortalSettings.ActiveTab.IsSuperTab == false)
                    {
                        if (objModule.PaneName == cboPanes.SelectedItem.Value)
                        {
                            cboInstances.Items.Add(new ListItem(objModule.ModuleTitle, objModule.ModuleOrder.ToString()));
                        }
                    }
                }
            }
            cboInstances.Items.Insert(0, new ListItem("", ""));
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ID = "IconBar.ascx";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            imgAddModule.Click += imgAddModule_Click;
            optMode.SelectedIndexChanged += optMode_SelectedIndexChanged;
            optModuleType.SelectedIndexChanged += optModuleType_SelectedIndexChanged;
            cboTabs.SelectedIndexChanged += cboTabs_SelectedIndexChanged;
            cmdVisibility.Click += cmdVisibility_Click;
            cboPanes.SelectedIndexChanged += cboPanes_SelectedIndexChanged;
            cboPosition.SelectedIndexChanged += cboPosition_SelectedIndexChanged;
            imgAdmin.Click += imgAdmin_Click;
            cmdAdmin.Click += cmdAdmin_Click;
            imgHost.Click += imgHost_Click;
            cmdHost.Click += cmdHost_Click;
            cmdAddModule.Click += AddModule_Click;

            cmdAddTab.Click += PageFunctions_Click;
            cmdAddTabIcon.Click += PageFunctions_Click;
            cmdEditTab.Click += PageFunctions_Click;
            cmdEditTabIcon.Click += PageFunctions_Click;
            cmdDeleteTab.Click += PageFunctions_Click;
            cmdDeleteTabIcon.Click += PageFunctions_Click;
            cmdCopyTab.Click += PageFunctions_Click;
            cmdCopyTabIcon.Click += PageFunctions_Click;
            cmdExportTab.Click += PageFunctions_Click;
            cmdExportTabIcon.Click += PageFunctions_Click;
            cmdImportTab.Click += PageFunctions_Click;
            cmdImportTabIcon.Click += PageFunctions_Click;

            cmdExtensions.Click += CommonTasks_Click;
            cmdExtensionsIcon.Click += CommonTasks_Click;
            cmdFiles.Click += CommonTasks_Click;
            cmdFilesIcon.Click += CommonTasks_Click;
            cmdRoles.Click += CommonTasks_Click;
            cmdRolesIcon.Click += CommonTasks_Click;
            cmdSite.Click += CommonTasks_Click;
            cmdSiteIcon.Click += CommonTasks_Click;
            cmdUsers.Click += CommonTasks_Click;
            cmdUsersIcon.Click += CommonTasks_Click;

            try
            {
                if (IsPageAdmin())
                {
                    tblControlPanel.Visible = true;
                    cmdVisibility.Visible = true;
                    rowControlPanel.Visible = true;

                    Localize();

                    if (Globals.IsAdminControl())
                    {
                        cmdAddModule.Enabled = false;
                    }
                    if (!Page.IsPostBack)
                    {
                        optModuleType.Items.FindByValue("0").Selected = true;

                        if (!TabPermissionController.CanAddPage())
                        {
                            DisableAction(imgAddTabIcon, "iconbar_addtab_bw.gif", cmdAddTabIcon, cmdAddTab);
                        }
                        if (!TabPermissionController.CanManagePage())
                        {
                            DisableAction(imgEditTabIcon, "iconbar_edittab_bw.gif", cmdEditTabIcon, cmdEditTab);
                        }
                        if (!TabPermissionController.CanDeletePage() || TabController.IsSpecialTab(TabController.CurrentPage.TabID, PortalSettings))
                        {
                            DisableAction(imgDeleteTabIcon, "iconbar_deletetab_bw.gif", cmdDeleteTabIcon, cmdDeleteTab);
                        }
                        else
                        {
                            ClientAPI.AddButtonConfirm(cmdDeleteTab, Localization.GetString("DeleteTabConfirm", LocalResourceFile));
                            ClientAPI.AddButtonConfirm(cmdDeleteTabIcon, Localization.GetString("DeleteTabConfirm", LocalResourceFile));
                        }
                        if (!TabPermissionController.CanCopyPage())
                        {
                            DisableAction(imgCopyTabIcon, "iconbar_copytab_bw.gif", cmdCopyTabIcon, cmdCopyTab);
                        }
                        if (!TabPermissionController.CanExportPage())
                        {
                            DisableAction(imgExportTabIcon, "iconbar_exporttab_bw.gif", cmdExportTabIcon, cmdExportTab);
                        }
                        if (!TabPermissionController.CanImportPage())
                        {
                            DisableAction(imgImportTabIcon, "iconbar_importtab_bw.gif", cmdImportTabIcon, cmdImportTab);
                        }
                        if (!TabPermissionController.CanAddContentToPage())
                        {
                            pnlModules.Visible = false;
                        }
                        if (!GetModulePermission(PortalSettings.PortalId, "Site Settings"))
                        {
                            DisableAction(imgSiteIcon, "iconbar_site_bw.gif", cmdSiteIcon, cmdSite);
                        }
                        if (GetModulePermission(PortalSettings.PortalId, "User Accounts") == false)
                        {
                            DisableAction(imgUsersIcon, "iconbar_users_bw.gif", cmdUsersIcon, cmdUsers);
                        }
                        if (GetModulePermission(PortalSettings.PortalId, "Security Roles") == false)
                        {
                            DisableAction(imgRolesIcon, "iconbar_roles_bw.gif", cmdRolesIcon, cmdRoles);
                        }
						if (GetModulePermission(PortalSettings.PortalId, "Digital Asset Management") == false)
                        {
                            DisableAction(imgFilesIcon, "iconbar_files_bw.gif", cmdFilesIcon, cmdFiles);
                        }
                        if (GetModulePermission(PortalSettings.PortalId, "Extensions") == false)
                        {
                            DisableAction(imgExtensionsIcon, "iconbar_extensions_bw.gif", cmdExtensionsIcon, cmdExtensions);
                        }
                        UserInfo objUser = UserController.GetCurrentUserInfo();
                        if (objUser != null)
                        {
                            if (objUser.IsSuperUser)
                            {
                                hypMessage.ImageUrl = Upgrade.UpgradeIndicator(DotNetNukeContext.Current.Application.Version, Request.IsLocal, Request.IsSecureConnection);
                                if (!String.IsNullOrEmpty(hypMessage.ImageUrl))
                                {
                                    hypMessage.ToolTip = Localization.GetString("hypUpgrade.Text", LocalResourceFile);
                                    hypMessage.NavigateUrl = Upgrade.UpgradeRedirect();
                                }
                                cmdHost.Visible = true;
                            }
                            else //branding
                            {
                                if (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName) && Host.DisplayCopyright)
                                {
                                    hypMessage.ImageUrl = "~/images/branding/iconbar_logo.png";
                                    hypMessage.ToolTip = DotNetNukeContext.Current.Application.Description;
                                    hypMessage.NavigateUrl = Localization.GetString("hypMessageUrl.Text", LocalResourceFile);
                                }
                                else
                                {
                                    hypMessage.Visible = false;
                                }
                                cmdHost.Visible = false;
                                cmdAdmin.Visible = GetModulePermission(PortalSettings.PortalId, "Console");
                            }
                            imgHost.Visible = cmdHost.Visible;
                            imgAdmin.Visible = cmdAdmin.Visible;
                        }
                        BindData();
                        int intItem;
                        for (intItem = 0; intItem <= PortalSettings.ActiveTab.Panes.Count - 1; intItem++)
                        {
                            cboPanes.Items.Add(Convert.ToString(PortalSettings.ActiveTab.Panes[intItem]));
                        }
                        if (cboPanes.Items.FindByValue(Globals.glbDefaultPane) != null)
                        {
                            cboPanes.Items.FindByValue(Globals.glbDefaultPane).Selected = true;
                        }
                        if (cboPermission.Items.Count > 0)
                        {
                            cboPermission.SelectedIndex = 0; //view
                        }
                        LoadPositions();

                        if (!string.IsNullOrEmpty(Host.HelpURL))
                        {
                            var version = Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, false);
                            cmdHelp.NavigateUrl = Globals.FormatHelpUrl(Host.HelpURL, PortalSettings, version);
                            cmdHelpIcon.NavigateUrl = cmdHelp.NavigateUrl;
                            cmdHelp.Enabled = true;
                            cmdHelpIcon.Enabled = true;
                        }
                        else
                        {
                            cmdHelp.Enabled = false;
                            cmdHelpIcon.Enabled = false;
                        }
                        SetMode(false);
                        SetVisibility(false);
                    }
					
                    //Register jQuery
                    jQuery.RequestRegistration();
                }
                else if (IsModuleAdmin())
                {
                    tblControlPanel.Visible = true;
                    cmdVisibility.Visible = false;
                    rowControlPanel.Visible = false;
                    if (!Page.IsPostBack)
                    {
                        SetMode(false);
                        SetVisibility(false);
                    }
                }
                else
                {
                    tblControlPanel.Visible = false;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
			//Set initial value
            base.OnPreRender(e);
            DNNClientAPI.EnableMinMax(imgVisibility,
                                      rowControlPanel,
                                      PortalSettings.DefaultControlPanelVisibility,
                                      Globals.ApplicationPath + "/images/collapse.gif",
                                      Globals.ApplicationPath + "/images/expand.gif",
                                      DNNClientAPI.MinMaxPersistanceType.Personalization,
                                      "Usability",
                                      "ControlPanelVisible" + PortalSettings.PortalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// PageFunctions_Click runs when any button in the Page toolbar is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	10/06/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        private void PageFunctions_Click(object sender, EventArgs e)
        {
            try
            {
                string URL = Request.RawUrl;
                switch (((LinkButton) sender).ID)
                {
                    case "cmdAddTab":
                    case "cmdAddTabIcon":
                        URL = Globals.NavigateURL("Tab");
                        break;
                    case "cmdEditTab":
                    case "cmdEditTabIcon":
                        URL = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=edit");
                        break;
                    case "cmdDeleteTab":
                    case "cmdDeleteTabIcon":
                        URL = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=delete");
                        break;
                    case "cmdCopyTab":
                    case "cmdCopyTabIcon":
                        URL = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=copy");
                        break;
                    case "cmdExportTab":
                    case "cmdExportTabIcon":
                        URL = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "ExportTab");
                        break;
                    case "cmdImportTab":
                    case "cmdImportTabIcon":
                        URL = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "ImportTab");
                        break;
                }
                Response.Redirect(URL, true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CommonTasks_Click runs when any button in the Common Tasks toolbar is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	10/06/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        private void CommonTasks_Click(object sender, EventArgs e)
        {
            try
            {
                string URL = Request.RawUrl;
                switch (((LinkButton) sender).ID)
                {
                    case "cmdSite":
                    case "cmdSiteIcon":
                        URL = BuildURL(PortalSettings.PortalId, "Site Settings");
                        break;
                    case "cmdUsers":
                    case "cmdUsersIcon":
                        URL = BuildURL(PortalSettings.PortalId, "User Accounts");
                        break;
                    case "cmdRoles":
                    case "cmdRolesIcon":
                        URL = BuildURL(PortalSettings.PortalId, "Security Roles");
                        break;
                    case "cmdFiles":
                    case "cmdFilesIcon":
						URL = BuildURL(PortalSettings.PortalId, "Digital Asset Management");
                        break;
                    case "cmdExtensions":
                    case "cmdExtensionsIcon":
                        URL = BuildURL(PortalSettings.PortalId, "Extensions");
                        break;
                }
                Response.Redirect(URL, true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// AddModule_Click runs when the Add Module Icon or text button is clicked
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[cnurse]	10/06/2004	Updated to reflect design changes for Help, 508 support
		///                       and localisation
		///     [vmasanas]  01/07/2005  Modified to add view perm. to all roles with edit perm.
		/// </history>
		/// -----------------------------------------------------------------------------
        protected void imgAddModule_Click(object sender, ImageClickEventArgs e)
        {
            AddModule_Click(sender, e);
        }

        protected void AddModule_Click(object sender, EventArgs e)
        {
            try
            {
                if (TabPermissionController.CanAddContentToPage())
                {
                    string title = txtTitle.Text;

                    ViewPermissionType permissionType = ViewPermissionType.View;
                    if (cboPermission.SelectedItem != null)
                    {
                        permissionType = (ViewPermissionType) Enum.Parse(typeof (ViewPermissionType), cboPermission.SelectedItem.Value);
                    }
                    int position = -1;
                    if (cboPosition.SelectedItem != null)
                    {
                        switch (cboPosition.SelectedItem.Value)
                        {
                            case "TOP":
                                position = 0;
                                break;
                            case "ABOVE":
                                if (!string.IsNullOrEmpty(cboInstances.SelectedValue))
                                {
                                    position = int.Parse(cboInstances.SelectedItem.Value) - 1;
                                }
                                else
                                {
                                    position = 0;
                                }
                                break;
                            case "BELOW":
                                if (!string.IsNullOrEmpty(cboInstances.SelectedValue))
                                {
                                    position = int.Parse(cboInstances.SelectedItem.Value) + 1;
                                }
                                else
                                {
                                    position = -1;
                                }
                                break;
                            case "BOTTOM":
                                position = -1;
                                break;
                        }
                    }
                    switch (optModuleType.SelectedItem.Value)
                    {
                        case "0": //new module
                            if (cboDesktopModules.SelectedIndex > 0)
                            {
                                AddNewModule(title, int.Parse(cboDesktopModules.SelectedItem.Value), cboPanes.SelectedItem.Text, position, permissionType, "");

                                //Redirect to the same page to pick up changes
                                Response.Redirect(Request.RawUrl, true);
                            }
                            break;
                        case "1": //existing module
                            if (cboModules.SelectedItem != null)
                            {
                                AddExistingModule(int.Parse(cboModules.SelectedItem.Value), int.Parse(cboTabs.SelectedItem.Value), cboPanes.SelectedItem.Text, position, "");

                                //Redirect to the same page to pick up changes
                                Response.Redirect(Request.RawUrl, true);
                            }
                            break;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void optModuleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindData();
        }

        private void cboTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var objModules = new ModuleController();
            var arrModules = new ArrayList();

            ModuleInfo objModule;
            Dictionary<int, ModuleInfo> arrPortalModules = objModules.GetTabModules(int.Parse(cboTabs.SelectedItem.Value));
            foreach (KeyValuePair<int, ModuleInfo> kvp in arrPortalModules)
            {
                objModule = kvp.Value;
                if (ModulePermissionController.CanAdminModule(objModule) && objModule.IsDeleted == false)
                {
                    arrModules.Add(objModule);
                }
            }
            lblModule.Text = Localization.GetString("Tab", LocalResourceFile);
            lblTitle.Text = Localization.GetString("Module", LocalResourceFile);
            cboModules.DataSource = arrModules;
            cboModules.DataBind();
        }

        protected void cmdVisibility_Click(object sender, EventArgs e)
        {
            SetVisibility(true);
            Response.Redirect(Request.RawUrl, true);
        }

        protected void optMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Page.IsCallback)
            {
                SetMode(true);
                Response.Redirect(Request.RawUrl, true);
            }
        }

        private void cboPanes_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPositions();
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/ControlPanel/ControlPanel.debug.js");
        }

        protected void cboPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayInstances();
        }

        protected void imgAdmin_Click(object sender, ImageClickEventArgs e)
        {
            cmdAdmin_Click(sender, e);
        }

        private void cmdAdmin_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL(PortalSettings.AdminTabId), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void imgHost_Click(object sender, ImageClickEventArgs e)
        {
            cmdHost_Click(sender, e);
        }

        private void cmdHost_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL(PortalSettings.SuperTabId, true), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
		
		#endregion
    }
}
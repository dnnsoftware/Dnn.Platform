// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.ControlPanel
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.UI;
    using DotNetNuke.Web.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;
    using Telerik.Web.UI;

    public partial class UpdatePage : UserControl, IDnnRibbonBarTool
    {
        private readonly INavigationManager _navigationManager;

        private TabInfo _currentTab;

        public UpdatePage()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public override bool Visible
        {
            get
            {
                return base.Visible && TabPermissionController.CanManagePage();
            }

            set
            {
                base.Visible = value;
            }
        }

        public string ToolName
        {
            get
            {
                return "QuickUpdatePage";
            }

            set
            {
                throw new NotSupportedException("Set ToolName not supported");
            }
        }

        private static PortalSettings PortalSettings
        {
            get
            {
                return PortalSettings.Current;
            }
        }

        private TabInfo CurrentTab
        {
            get
            {
                // Weird - but the activetab has different skin src value than getting from the db
                if (this._currentTab == null)
                {
                    this._currentTab = TabController.Instance.GetTab(PortalSettings.ActiveTab.TabID, PortalSettings.ActiveTab.PortalID, false);
                }

                return this._currentTab;
            }
        }

        private string LocalResourceFile
        {
            get
            {
                return string.Format("{0}/{1}/{2}.ascx.resx", this.TemplateSourceDirectory, Localization.LocalResourceDirectory, this.GetType().BaseType.Name);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdUpdate.Click += this.CmdUpdateClick;

            try
            {
                if (this.Visible && !this.IsPostBack)
                {
                    this.Name.Text = this.CurrentTab.TabName;
                    this.IncludeInMenu.Checked = this.CurrentTab.IsVisible;
                    this.IsDisabled.Checked = this.CurrentTab.DisableLink;
                    this.IsSecurePanel.Visible = PortalSettings.SSLEnabled;
                    this.IsSecure.Enabled = PortalSettings.SSLEnabled;
                    this.IsSecure.Checked = this.CurrentTab.IsSecure;
                    this.LoadAllLists();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void CmdUpdateClick(object sender, EventArgs e)
        {
            if (TabPermissionController.CanManagePage())
            {
                TabInfo selectedTab = null;
                if (!string.IsNullOrEmpty(this.PageLst.SelectedValue))
                {
                    int selectedTabID = int.Parse(this.PageLst.SelectedValue);
                    selectedTab = TabController.Instance.GetTab(selectedTabID, PortalSettings.ActiveTab.PortalID, false);
                }

                TabRelativeLocation tabLocation = TabRelativeLocation.NOTSET;
                if (!string.IsNullOrEmpty(this.LocationLst.SelectedValue))
                {
                    tabLocation = (TabRelativeLocation)Enum.Parse(typeof(TabRelativeLocation), this.LocationLst.SelectedValue);
                }

                TabInfo tab = this.CurrentTab;

                tab.TabName = this.Name.Text;
                tab.IsVisible = this.IncludeInMenu.Checked;
                tab.DisableLink = this.IsDisabled.Checked;
                tab.IsSecure = this.IsSecure.Checked;
                tab.SkinSrc = this.SkinLst.SelectedValue;

                string errMsg = string.Empty;
                try
                {
                    RibbonBarManager.SaveTabInfoObject(tab, selectedTab, tabLocation, null);
                }
                catch (DotNetNukeException ex)
                {
                    Exceptions.LogException(ex);
                    errMsg = (ex.ErrorCode != DotNetNukeErrorCode.NotSet) ? this.GetString("Err." + ex.ErrorCode) : ex.Message;
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                    errMsg = ex.Message;
                }

                // Clear the Tab's Cached modules
                DataCache.ClearModuleCache(PortalSettings.ActiveTab.TabID);

                // Update Cached Tabs as TabPath may be needed before cache is cleared
                TabInfo tempTab;
                if (TabController.Instance.GetTabsByPortal(PortalSettings.ActiveTab.PortalID).TryGetValue(tab.TabID, out tempTab))
                {
                    tempTab.TabPath = tab.TabPath;
                }

                if (string.IsNullOrEmpty(errMsg))
                {
                    this.Response.Redirect(this._navigationManager.NavigateURL(tab.TabID));
                }
                else
                {
                    errMsg = string.Format("<p>{0}</p><p>{1}</p>", this.GetString("Err.Header"), errMsg);
                    Web.UI.Utilities.RegisterAlertOnPageLoad(this, new MessageWindowParameters(errMsg) { Title = this.GetString("Err.Title") });
                }
            }
        }

        private static string FormatSkinName(string strSkinFolder, string strSkinFile)
        {
            if (strSkinFolder.ToLowerInvariant() == "_default")
            {
                return strSkinFile;
            }

            switch (strSkinFile.ToLowerInvariant())
            {
                case "skin":
                case "container":
                case "default":
                    return strSkinFolder;
                default:
                    return strSkinFolder + " - " + strSkinFile;
            }
        }

        private void LoadAllLists()
        {
            this.LocationLst.Enabled = RibbonBarManager.CanMovePage();
            this.PageLst.Enabled = RibbonBarManager.CanMovePage();
            if (this.LocationLst.Enabled)
            {
                this.LoadLocationList();
                this.LoadPageList();
            }

            this.LoadSkinList();
        }

        private void LoadSkinList()
        {
            this.SkinLst.ClearSelection();
            this.SkinLst.Items.Clear();
            this.SkinLst.Items.Add(new RadComboBoxItem(this.GetString("DefaultSkin"), string.Empty));

            // load portal skins
            var portalSkinsHeader = new RadComboBoxItem(this.GetString("PortalSkins"), string.Empty) { Enabled = false, CssClass = "SkinListHeader" };
            this.SkinLst.Items.Add(portalSkinsHeader);

            string[] arrFolders;
            string[] arrFiles;
            string strLastFolder = string.Empty;
            string strRoot = PortalSettings.HomeDirectoryMapPath + SkinController.RootSkin;
            if (Directory.Exists(strRoot))
            {
                arrFolders = Directory.GetDirectories(strRoot);
                foreach (string strFolder in arrFolders)
                {
                    arrFiles = Directory.GetFiles(strFolder, "*.ascx");
                    foreach (string strFile in arrFiles)
                    {
                        string folder = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);
                        if (strLastFolder != folder)
                        {
                            if (!string.IsNullOrEmpty(strLastFolder))
                            {
                                this.SkinLst.Items.Add(this.GetSeparatorItem());
                            }

                            strLastFolder = folder;
                        }

                        this.SkinLst.Items.Add(new RadComboBoxItem(
                            FormatSkinName(folder, Path.GetFileNameWithoutExtension(strFile)),
                            "[L]" + SkinController.RootSkin + "/" + folder + "/" + Path.GetFileName(strFile)));
                    }
                }
            }

            // No portal skins added, remove the header
            if (this.SkinLst.Items.Count == 2)
            {
                this.SkinLst.Items.Remove(1);
            }

            // load host skins
            var hostSkinsHeader = new RadComboBoxItem(this.GetString("HostSkins"), string.Empty) { Enabled = false, CssClass = "SkinListHeader" };
            this.SkinLst.Items.Add(hostSkinsHeader);

            strRoot = Globals.HostMapPath + SkinController.RootSkin;
            if (Directory.Exists(strRoot))
            {
                arrFolders = Directory.GetDirectories(strRoot);
                foreach (string strFolder in arrFolders)
                {
                    if (!strFolder.EndsWith(Globals.glbHostSkinFolder))
                    {
                        arrFiles = Directory.GetFiles(strFolder, "*.ascx");
                        foreach (string strFile in arrFiles)
                        {
                            string folder = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);
                            if (strLastFolder != folder)
                            {
                                if (!string.IsNullOrEmpty(strLastFolder))
                                {
                                    this.SkinLst.Items.Add(this.GetSeparatorItem());
                                }

                                strLastFolder = folder;
                            }

                            this.SkinLst.Items.Add(new RadComboBoxItem(
                                FormatSkinName(folder, Path.GetFileNameWithoutExtension(strFile)),
                                "[G]" + SkinController.RootSkin + "/" + folder + "/" + Path.GetFileName(strFile)));
                        }
                    }
                }
            }

            // Set the selected item
            this.SkinLst.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(this.CurrentTab.SkinSrc))
            {
                RadComboBoxItem selectItem = this.SkinLst.FindItemByValue(this.CurrentTab.SkinSrc);
                if (selectItem != null)
                {
                    selectItem.Selected = true;
                }
            }
        }

        private RadComboBoxItem GetSeparatorItem()
        {
            return new RadComboBoxItem(this.GetString("SkinLstSeparator"), string.Empty) { CssClass = "SkinLstSeparator", Enabled = false };
        }

        private void LoadLocationList()
        {
            this.LocationLst.ClearSelection();
            this.LocationLst.Items.Clear();

            // LocationLst.Items.Add(new ListItem(GetString("NoLocationSelection"), ""));
            // LocationLst.Items.Add(new ListItem(GetString("Before"), "BEFORE"));
            // LocationLst.Items.Add(new ListItem(GetString("After"), "AFTER"));
            // LocationLst.Items.Add(new ListItem(GetString("Child"), "CHILD"));
            this.LocationLst.AddItem(this.GetString("NoLocationSelection"), string.Empty);
            this.LocationLst.AddItem(this.GetString("Before"), "BEFORE");
            this.LocationLst.AddItem(this.GetString("After"), "AFTER");
            this.LocationLst.AddItem(this.GetString("Child"), "CHILD");

            this.LocationLst.SelectedIndex = 0;
        }

        private void LoadPageList()
        {
            this.PageLst.ClearSelection();
            this.PageLst.Items.Clear();

            this.PageLst.DataTextField = "IndentedTabName";
            this.PageLst.DataValueField = "TabID";
            this.PageLst.DataSource = RibbonBarManager.GetPagesList().Where(t => !this.IsParentTab(t, this.CurrentTab.TabID));
            this.PageLst.DataBind();

            // PageLst.Items.Insert(0, new ListItem(GetString("NoPageSelection"), string.Empty));
            this.PageLst.InsertItem(0, this.GetString("NoPageSelection"), string.Empty);
            this.PageLst.SelectedIndex = 0;
        }

        private string GetString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        private bool IsParentTab(TabInfo tab, int parentTabId)
        {
            while (tab != null)
            {
                if (tab.TabID == parentTabId)
                {
                    return true;
                }

                tab = tab.ParentId != Null.NullInteger ? TabController.Instance.GetTab(tab.ParentId, tab.PortalID, false) : null;
            }

            return false;
        }
    }
}

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
using System.IO;
using System.Linq;
using System.Web.UI;

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

using Telerik.Web.UI;


#endregion

namespace DotNetNuke.UI.ControlPanel
{
    using System.Web.UI.WebControls;

    public partial class UpdatePage : UserControl, IDnnRibbonBarTool
    {
        #region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdUpdate.Click += CmdUpdateClick;

            try
            {
                if (Visible && !IsPostBack)
                {
                    Name.Text = CurrentTab.TabName;
                    IncludeInMenu.Checked = CurrentTab.IsVisible;
                    IsDisabled.Checked = CurrentTab.DisableLink;
                    IsSecurePanel.Visible = PortalSettings.SSLEnabled;
                    IsSecure.Enabled = PortalSettings.SSLEnabled;
                    IsSecure.Checked = CurrentTab.IsSecure;
                    LoadAllLists();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void CmdUpdateClick(object sender, EventArgs e)
        {
            if ((TabPermissionController.CanManagePage()))
            {
                var tabCtrl = new TabController();

                TabInfo selectedTab = null;
                if ((!string.IsNullOrEmpty(PageLst.SelectedValue)))
                {
                    int selectedTabID = Int32.Parse(PageLst.SelectedValue);
                    selectedTab = tabCtrl.GetTab(selectedTabID, PortalSettings.ActiveTab.PortalID, false);
                }

                TabRelativeLocation tabLocation = TabRelativeLocation.NOTSET;
                if ((!string.IsNullOrEmpty(LocationLst.SelectedValue)))
                {
                    tabLocation = (TabRelativeLocation) Enum.Parse(typeof (TabRelativeLocation), LocationLst.SelectedValue);
                }

                TabInfo tab = CurrentTab;

                tab.TabName = Name.Text;
                tab.IsVisible = IncludeInMenu.Checked;
                tab.DisableLink = IsDisabled.Checked;
                tab.IsSecure = IsSecure.Checked;
                tab.SkinSrc = SkinLst.SelectedValue;

                string errMsg = "";
                try
                {
                    RibbonBarManager.SaveTabInfoObject(tab, selectedTab, tabLocation, null);
                }
                catch (DotNetNukeException ex)
                {
                    Exceptions.LogException(ex);
                    errMsg = (ex.ErrorCode != DotNetNukeErrorCode.NotSet) ? GetString("Err." + ex.ErrorCode) : ex.Message;
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                    errMsg = ex.Message;
                }

                //Clear the Tab's Cached modules
                DataCache.ClearModuleCache(PortalSettings.ActiveTab.TabID);

                //Update Cached Tabs as TabPath may be needed before cache is cleared
                TabInfo tempTab;
                if (new TabController().GetTabsByPortal(PortalSettings.ActiveTab.PortalID).TryGetValue(tab.TabID, out tempTab))
                {
                    tempTab.TabPath = tab.TabPath;
                }

                if ((string.IsNullOrEmpty(errMsg)))
                {
                    Response.Redirect(Globals.NavigateURL(tab.TabID));
                }
                else
                {
                    errMsg = string.Format("<p>{0}</p><p>{1}</p>", GetString("Err.Header"), errMsg);
                    Web.UI.Utilities.RegisterAlertOnPageLoad(this, new MessageWindowParameters(errMsg) { Title = GetString("Err.Title") });
                }
            }
        }

        #endregion

        #region "Properties"

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

        #endregion

        #region "Methods"

        private TabInfo _currentTab;

        private TabInfo CurrentTab
        {
            get
            {
                //Weird - but the activetab has different skin src value than getting from the db
                if (((_currentTab == null)))
                {
                    _currentTab = new TabController().GetTab(PortalSettings.ActiveTab.TabID, PortalSettings.ActiveTab.PortalID, false);
                }
                return _currentTab;
            }
        }

        private string LocalResourceFile
        {
            get
            {
                return string.Format("{0}/{1}/{2}.ascx.resx", TemplateSourceDirectory, Localization.LocalResourceDirectory, GetType().BaseType.Name);
            }
        }

        private static PortalSettings PortalSettings
        {
            get
            {
                return PortalSettings.Current;
            }
        }

        private void LoadAllLists()
        {
            LocationLst.Enabled = RibbonBarManager.CanMovePage();
            PageLst.Enabled = RibbonBarManager.CanMovePage();
            if ((LocationLst.Enabled))
            {
                LoadLocationList();
                LoadPageList();
            }

            LoadSkinList();
        }

        private void LoadSkinList()
        {
            SkinLst.ClearSelection();
            SkinLst.Items.Clear();
            SkinLst.Items.Add(new RadComboBoxItem(GetString("DefaultSkin"), string.Empty));

            // load portal skins
            var portalSkinsHeader = new RadComboBoxItem(GetString("PortalSkins"), string.Empty) {Enabled = false, CssClass = "SkinListHeader"};
            SkinLst.Items.Add(portalSkinsHeader);

            string[] arrFolders;
            string[] arrFiles;
            string strLastFolder = "";
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
                                SkinLst.Items.Add(GetSeparatorItem());
                            }
                            strLastFolder = folder;
                        }
                        SkinLst.Items.Add(new RadComboBoxItem(FormatSkinName(folder, Path.GetFileNameWithoutExtension(strFile)),
                                                              "[L]" + SkinController.RootSkin + "/" + folder + "/" + Path.GetFileName(strFile)));
                    }
                }
            }

            //No portal skins added, remove the header
            if ((SkinLst.Items.Count == 2))
            {
                SkinLst.Items.Remove(1);
            }

            //load host skins
            var hostSkinsHeader = new RadComboBoxItem(GetString("HostSkins"), string.Empty) {Enabled = false, CssClass = "SkinListHeader"};
            SkinLst.Items.Add(hostSkinsHeader);

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
                                    SkinLst.Items.Add(GetSeparatorItem());
                                }
                                strLastFolder = folder;
                            }
                            SkinLst.Items.Add(new RadComboBoxItem(FormatSkinName(folder, Path.GetFileNameWithoutExtension(strFile)),
                                                                  "[G]" + SkinController.RootSkin + "/" + folder + "/" + Path.GetFileName(strFile)));
                        }
                    }
                }
            }

            //Set the selected item
            SkinLst.SelectedIndex = 0;
            if ((!string.IsNullOrEmpty(CurrentTab.SkinSrc)))
            {
                RadComboBoxItem selectItem = SkinLst.FindItemByValue(CurrentTab.SkinSrc);
                if (((selectItem != null)))
                {
                    selectItem.Selected = true;
                }
            }
        }

        private RadComboBoxItem GetSeparatorItem()
        {
            return new RadComboBoxItem(GetString("SkinLstSeparator"), string.Empty) {CssClass = "SkinLstSeparator", Enabled = false};
        }

        private static string FormatSkinName(string strSkinFolder, string strSkinFile)
        {
            if (strSkinFolder.ToLower() == "_default")
            {
                return strSkinFile;
            }
            switch (strSkinFile.ToLower())
            {
                case "skin":
                case "container":
                case "default":
                    return strSkinFolder;
                default:
                    return strSkinFolder + " - " + strSkinFile;
            }
        }

        private void LoadLocationList()
        {
            LocationLst.ClearSelection();
            LocationLst.Items.Clear();

            //LocationLst.Items.Add(new ListItem(GetString("NoLocationSelection"), ""));
            //LocationLst.Items.Add(new ListItem(GetString("Before"), "BEFORE"));
            //LocationLst.Items.Add(new ListItem(GetString("After"), "AFTER"));
            //LocationLst.Items.Add(new ListItem(GetString("Child"), "CHILD"));

            LocationLst.AddItem(GetString("NoLocationSelection"), "");
            LocationLst.AddItem(GetString("Before"), "BEFORE");
            LocationLst.AddItem(GetString("After"), "AFTER");
            LocationLst.AddItem(GetString("Child"), "CHILD");

            LocationLst.SelectedIndex = 0;
        }

        private void LoadPageList()
        {
            PageLst.ClearSelection();
            PageLst.Items.Clear();

            PageLst.DataTextField = "IndentedTabName";
            PageLst.DataValueField = "TabID";
            PageLst.DataSource = RibbonBarManager.GetPagesList().Where(t => !IsParentTab(t, CurrentTab.TabID));
            PageLst.DataBind();

            //PageLst.Items.Insert(0, new ListItem(GetString("NoPageSelection"), string.Empty));
            PageLst.InsertItem(0, GetString("NoPageSelection"), string.Empty);
            PageLst.SelectedIndex = 0;
        }

        private string GetString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

		private bool IsParentTab(TabInfo tab, int parentTabId)
		{
			var tabController = new TabController();
			while (tab != null)
			{
				if (tab.TabID == parentTabId)
				{
					return true;
				}
				tab = tab.ParentId != Null.NullInteger ? tabController.GetTab(tab.ParentId, tab.PortalID, false) : null;
			}

			return false;
		}

        #endregion
    }
}
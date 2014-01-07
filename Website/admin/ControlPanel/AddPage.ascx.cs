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
using System.IO;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.UI;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.UI.ControlPanel
{
    using System.Web.UI.WebControls;

    public partial class AddPage : UserControl, IDnnRibbonBarTool
    {
        #region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdAddPage.Click += CmdAddPageClick;

            try
            {
                if (PortalSettings.Pages < PortalSettings.PageQuota || UserController.GetCurrentUserInfo().IsSuperUser || PortalSettings.PageQuota == 0)
                {
                    cmdAddPage.Enabled = true;
                }
                else
                {
                    cmdAddPage.Enabled = false;
                    cmdAddPage.ToolTip = Localization.GetString("ExceededQuota", LocalResourceFile);
                }
                if (!IsPostBack)
                {
                    if ((Visible))
                    {
                        LoadAllLists();
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void CmdAddPageClick(object sender, EventArgs e)
        {
            var tabCtrl = new TabController();

            int selectedTabID = Int32.Parse(PageLst.SelectedValue);
            TabInfo selectedTab = tabCtrl.GetTab(selectedTabID, PortalSettings.ActiveTab.PortalID, false);
            var tabLocation = (TabRelativeLocation) Enum.Parse(typeof (TabRelativeLocation), LocationLst.SelectedValue);
            TabInfo newTab = RibbonBarManager.InitTabInfoObject(selectedTab, tabLocation);

            string templateFile = string.Empty;
            if ((!string.IsNullOrEmpty(TemplateLst.SelectedValue)))
            {
                templateFile = Path.Combine(PortalSettings.HomeDirectoryMapPath, "Templates\\" + TemplateLst.SelectedValue);
            }

            newTab.TabName = Name.Text;
            newTab.IsVisible = IncludeInMenu.Checked;

            string errMsg = string.Empty;
            try
            {
                RibbonBarManager.SaveTabInfoObject(newTab, selectedTab, tabLocation, templateFile);
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
            if (new TabController().GetTabsByPortal(PortalSettings.ActiveTab.PortalID).TryGetValue(newTab.TabID, out tempTab))
            {
                tempTab.TabPath = newTab.TabPath;
            }

            if ((string.IsNullOrEmpty(errMsg)))
            {
                Response.Redirect(Globals.NavigateURL(newTab.TabID));
            }
            else
            {
                errMsg = string.Format("<p>{0}</p><p>{1}</p>", GetString("Err.Header"), errMsg);
                Web.UI.Utilities.RegisterAlertOnPageLoad(this, new MessageWindowParameters(errMsg) { Title = GetString("Err.Title")});
            }
        }

        #endregion

        #region "Properties"

        public override bool Visible
        {
            get
            {
                return base.Visible && TabPermissionController.CanAddPage();
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
                return "QuickAddPage";
            }
            set
            {
                throw new NotSupportedException("Set ToolName not supported");
            }
        }

        #endregion

        #region "Methods"

        private TabInfo _newTabObject;

        protected TabInfo NewTabObject
        {
            get
            {
                if (((_newTabObject == null)))
                {
                    _newTabObject = RibbonBarManager.InitTabInfoObject(PortalSettings.ActiveTab);
                }
                return _newTabObject;
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
            LoadLocationList();
            LoadTemplateList();
            LoadPageList();
        }

        private void LoadTemplateList()
        {
            TemplateLst.ClearSelection();
            TemplateLst.Items.Clear();

            //Get Templates Folder
            ArrayList templateFiles = Globals.GetFileList(PortalSettings.PortalId, "page.template", false, "Templates/");
            foreach (FileItem dnnFile in templateFiles)
            {
                var item = new DnnComboBoxItem(dnnFile.Text.Replace(".page.template", ""), dnnFile.Text);
                //TemplateLst.Items.Add(item);
                TemplateLst.Items.Add(item);
                if (item.Text == "Default")
                {
                    item.Selected = true;
                }
            }

            //TemplateLst.Items.Insert(0, new ListItem(GetString("NoTemplate"), ""));
            TemplateLst.InsertItem(0, GetString("NoTemplate"), "");
        }

        private void LoadLocationList()
        {
            LocationLst.ClearSelection();
            LocationLst.Items.Clear();

            //LocationLst.Items.Add(new ListItem(GetString("Before"), "BEFORE"));
            //LocationLst.Items.Add(new ListItem(GetString("After"), "AFTER"));
            //LocationLst.Items.Add(new ListItem(GetString("Child"), "CHILD"));

            LocationLst.AddItem(GetString("Before"), "BEFORE");
            LocationLst.AddItem(GetString("After"), "AFTER");
            LocationLst.AddItem(GetString("Child"), "CHILD");

            LocationLst.SelectedIndex = (!PortalSecurity.IsInRole("Administrators")) ? 2 : 1;
        }

        private void LoadPageList()
        {
            PageLst.ClearSelection();
            PageLst.Items.Clear();

            PageLst.DataTextField = "IndentedTabName";
            PageLst.DataValueField = "TabID";
            PageLst.DataSource = RibbonBarManager.GetPagesList();
            PageLst.DataBind();

            var item = PageLst.FindItemByValue(PortalSettings.ActiveTab.TabID.ToString());
            if (((item != null)))
            {
                item.Selected = true;
            }
        }

        private string GetString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

        #endregion
    }
}
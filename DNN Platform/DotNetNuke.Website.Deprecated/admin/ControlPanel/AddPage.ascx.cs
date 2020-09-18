// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.ControlPanel
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
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
    using Microsoft.Extensions.DependencyInjection;

    public partial class AddPage : UserControl, IDnnRibbonBarTool
    {
        private readonly INavigationManager _navigationManager;

        private TabInfo _newTabObject;

        public AddPage()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

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

        protected TabInfo NewTabObject
        {
            get
            {
                if (this._newTabObject == null)
                {
                    this._newTabObject = RibbonBarManager.InitTabInfoObject(PortalSettings.ActiveTab);
                }

                return this._newTabObject;
            }
        }

        private static PortalSettings PortalSettings
        {
            get
            {
                return PortalSettings.Current;
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

            this.cmdAddPage.Click += this.CmdAddPageClick;

            try
            {
                if (PortalSettings.Pages < PortalSettings.PageQuota || UserController.Instance.GetCurrentUserInfo().IsSuperUser || PortalSettings.PageQuota == 0)
                {
                    this.cmdAddPage.Enabled = true;
                }
                else
                {
                    this.cmdAddPage.Enabled = false;
                    this.cmdAddPage.ToolTip = Localization.GetString("ExceededQuota", this.LocalResourceFile);
                }

                if (!this.IsPostBack)
                {
                    if (this.Visible)
                    {
                        this.LoadAllLists();
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
            int selectedTabID = int.Parse(this.PageLst.SelectedValue);
            TabInfo selectedTab = TabController.Instance.GetTab(selectedTabID, PortalSettings.ActiveTab.PortalID, false);
            var tabLocation = (TabRelativeLocation)Enum.Parse(typeof(TabRelativeLocation), this.LocationLst.SelectedValue);
            TabInfo newTab = RibbonBarManager.InitTabInfoObject(selectedTab, tabLocation);

            newTab.TabName = this.Name.Text;
            newTab.IsVisible = this.IncludeInMenu.Checked;

            string errMsg = string.Empty;
            try
            {
                RibbonBarManager.SaveTabInfoObject(newTab, selectedTab, tabLocation, this.TemplateLst.SelectedValue);
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
            if (TabController.Instance.GetTabsByPortal(PortalSettings.ActiveTab.PortalID).TryGetValue(newTab.TabID, out tempTab))
            {
                tempTab.TabPath = newTab.TabPath;
            }

            if (string.IsNullOrEmpty(errMsg))
            {
                this.Response.Redirect(this._navigationManager.NavigateURL(newTab.TabID));
            }
            else
            {
                errMsg = string.Format("<p>{0}</p><p>{1}</p>", this.GetString("Err.Header"), errMsg);
                Web.UI.Utilities.RegisterAlertOnPageLoad(this, new MessageWindowParameters(errMsg) { Title = this.GetString("Err.Title") });
            }
        }

        private void LoadAllLists()
        {
            this.LoadLocationList();
            this.LoadTemplateList();
            this.LoadPageList();
        }

        private void LoadTemplateList()
        {
            this.TemplateLst.ClearSelection();
            this.TemplateLst.Items.Clear();

            // Get Templates Folder
            ArrayList templateFiles = Globals.GetFileList(PortalSettings.PortalId, "page.template", false, "Templates/");
            foreach (FileItem dnnFile in templateFiles)
            {
                var item = new DnnComboBoxItem(dnnFile.Text.Replace(".page.template", string.Empty), dnnFile.Value);
                this.TemplateLst.Items.Add(item);
                if (item.Text == "Default")
                {
                    item.Selected = true;
                }
            }

            this.TemplateLst.InsertItem(0, this.GetString("NoTemplate"), string.Empty);
        }

        private void LoadLocationList()
        {
            this.LocationLst.ClearSelection();
            this.LocationLst.Items.Clear();

            // LocationLst.Items.Add(new ListItem(GetString("Before"), "BEFORE"));
            // LocationLst.Items.Add(new ListItem(GetString("After"), "AFTER"));
            // LocationLst.Items.Add(new ListItem(GetString("Child"), "CHILD"));
            this.LocationLst.AddItem(this.GetString("Before"), "BEFORE");
            this.LocationLst.AddItem(this.GetString("After"), "AFTER");
            this.LocationLst.AddItem(this.GetString("Child"), "CHILD");

            this.LocationLst.SelectedIndex = (!PortalSecurity.IsInRole("Administrators")) ? 2 : 1;
        }

        private void LoadPageList()
        {
            this.PageLst.ClearSelection();
            this.PageLst.Items.Clear();

            this.PageLst.DataTextField = "IndentedTabName";
            this.PageLst.DataValueField = "TabID";
            this.PageLst.DataSource = RibbonBarManager.GetPagesList();
            this.PageLst.DataBind();

            var item = this.PageLst.FindItemByValue(PortalSettings.ActiveTab.TabID.ToString());
            if (item != null)
            {
                item.Selected = true;
            }
        }

        private string GetString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }
    }
}

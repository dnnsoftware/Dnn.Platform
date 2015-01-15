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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI;

using Telerik.Web.UI;

using DataCache = DotNetNuke.Common.Utilities.DataCache;
using Globals = DotNetNuke.Common.Globals;

namespace DesktopModules.Admin.Tabs
{
    
    public enum Position
    {
        Child,
        Below,
        Above
    }

    /// <summary>
    /// The Tabs PortalModuleBase is used to manage the Tabs/Pages for a 
    /// portal.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class View : PortalModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (View));

        #region Private Members

        private const string DefaultPageTemplate = "Default.page.template";

        #endregion

        #region Properties

        private string AllUsersIcon
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Everyone.png";
            }
        }

        private string AdminOnlyIcon
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_UserAdmin.png";
            }
        }

        private string IconAdd
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Add.png";
            }
        }

        private string IconDelete
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Delete.png";
            }
        }

        private string IconDown
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Down.png";
            }
        }

        private string IconEdit
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Edit.png";
            }
        }

        private string IconHome
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Home.png";
            }
        }

        private string IconPageDisabled
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Disabled.png";
            }
        }

        private string IconPageHidden
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Hidden.png";
            }
        }

        private string IconPortal
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Portal.png";
            }
        }

        private string IconUp
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_Up.png";
            }
        }

        private string IconView
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_View.png";
            }
        }

        private string RegisteredUsersIcon
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_User.png";
            }
        }

        private string SecuredIcon
        {
            get
            {
                return TemplateSourceDirectory + "/images/Icon_UserSecure.png";
            }
        }

        private string IconRedirect
        {
            get
            {
                return ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Redirect.png");
            }
        }

        private string SelectedNode
        {
            get
            {
                return (string)ViewState["SelectedNode"];
            }
            set
            {
                ViewState["SelectedNode"] = value;
            }
        }

        protected List<TabInfo> Tabs
        {
            get
            {
                return TabController.GetPortalTabs(rblMode.SelectedValue == "H" ? Null.NullInteger : PortalId, Null.NullInteger, false, true, false, true);
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdCopySkin.Click += CmdCopySkinClick;
            rblMode.SelectedIndexChanged += RblModeSelectedIndexChanged;
            ctlPages.NodeClick += CtlPagesNodeClick;
            ctlPages.ContextMenuItemClick += CtlPagesContextMenuItemClick;
            ctlPages.NodeEdit += CtlPagesNodeEdit;
            if (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName))
            {                
                ctlPages.EnableDragAndDrop = true;
                ctlPages.EnableDragAndDropBetweenNodes = true;
                ctlPages.NodeDrop += CtlPagesNodeDrop;                
            }
            else
            {             
                ctlPages.EnableDragAndDrop = false;
                ctlPages.EnableDragAndDropBetweenNodes = false;
            }
            cmdExpandTree.Click += OnExpandTreeClick;
            grdModules.NeedDataSource += GrdModulesNeedDataSource;
            ctlPages.NodeExpand += CtlPagesNodeExpand;
            btnBulkCreate.Click += OnCreatePagesClick;
            cmdUpdate.Click += CmdUpdateClick;

            jQuery.RequestDnnPluginsRegistration();

            JavaScript.RegisterClientReference(Page, ClientAPI.ClientNamespaceReferences.dnn_dom);
            ClientResourceManager.RegisterScript(Page, ClientAPI.ScriptPath + "dnn.controls.js", 14);
            dgPermissions.RegisterScriptsForAjaxPanel();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (PortalSettings.Pages < PortalSettings.PageQuota || UserController.Instance.GetCurrentUserInfo().IsSuperUser || PortalSettings.PageQuota == 0)
                {
                    btnBulkCreate.Enabled = true;
                }
                else
                {
                    btnBulkCreate.Enabled = false;
                    btnBulkCreate.ToolTip = Localization.GetString("ExceededQuota", LocalResourceFile);
                }
                CheckSecurity();
                pnlHost.Visible = UserInfo.IsSuperUser;

                // If this is the first visit to the page, bind the tab data to the page listbox
                if (Page.IsPostBack == false)
                {
                    LocalizeControl();
                    BindSkinsAndContainers();

                    if (!(string.IsNullOrEmpty(Request.QueryString["isHost"])))
                    {
                        if (bool.Parse(Request.QueryString["isHost"]))
                        {
                            rblMode.SelectedValue = "H";
                        }
                    }
                    BindTree();

                    if(!string.IsNullOrEmpty(Request.QueryString["edittabid"]))
                    {
                        var tabId = Request.QueryString["edittabid"];
                        var node = ctlPages.FindNodeByValue(tabId);
                        if(node != null)
                        {
                            node.Selected = true;
                            node.ExpandParentNodes();
                            CtlPagesNodeClick(ctlPages, new RadTreeNodeEventArgs(node));
                        }
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void CmdCopySkinClick(object sender, EventArgs e)
        {
            try
            {
                TabController.CopyDesignToChildren(TabController.Instance.GetTab(Convert.ToInt32(ctlPages.SelectedNode.Value), PortalId, false), drpSkin.SelectedValue, drpContainer.SelectedValue);
                ShowSuccessMessage(Localization.GetString("DesignCopied", LocalResourceFile));
            }
            catch (Exception ex)
            {
                ShowErrorMessage(Localization.GetString("DesignCopyError", LocalResourceFile));
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected void RblModeSelectedIndexChanged(object sender, EventArgs e)
        {
            BindTree();
        }

        protected void CtlPagesNodeClick(object sender, RadTreeNodeEventArgs e)
        {
            if (e.Node.Attributes["isPortalRoot"] != null && Boolean.Parse(e.Node.Attributes["isPortalRoot"]))
            {
                pnlDetails.Visible = false;
                pnlBulk.Visible = false;
            }
            else
            {
                var tabid = Convert.ToInt32(e.Node.Value);
                BindTab(tabid);
            }
        }

        protected void CtlPagesContextMenuItemClick(object sender, RadTreeViewContextMenuEventArgs e)
        {
            SelectedNode = e.Node.Value;

            var portalId = rblMode.SelectedValue == "H" ? Null.NullInteger : PortalId;
            var objTab = TabController.Instance.GetTab(int.Parse(e.Node.Value), portalId, false);

            switch (e.MenuItem.Value.ToLower())
            {
                case "makehome":
                    if (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName))
                    {
                        var portalInfo = PortalController.Instance.GetPortal(PortalId);
                        portalInfo.HomeTabId = objTab.TabID;
                        PortalSettings.HomeTabId = objTab.TabID;
                        PortalController.Instance.UpdatePortalInfo(portalInfo);                        
                        DataCache.ClearPortalCache(PortalId, false);
                        BindTreeAndShowTab(objTab.TabID);
                        ShowSuccessMessage(string.Format(Localization.GetString("TabMadeHome", LocalResourceFile), objTab.TabName));
                    }
                    break;
                case "view":
                    Response.Redirect(objTab.FullUrl);
                    break;
                case "edit":
                    if (TabPermissionController.CanManagePage(objTab))
                    {
                        var editUrl = Globals.NavigateURL(objTab.TabID, "Tab", "action=edit", "returntabid=" + TabId);
                        // Prevent PageSettings of the current page in a popup if SSL is enabled and enforced, which causes redirection/javascript broswer security issues.                        
                        if (PortalSettings.EnablePopUps && !(objTab.TabID == TabId && (PortalSettings.SSLEnabled && PortalSettings.SSLEnforced)))
                        {
                            editUrl = UrlUtils.PopUpUrl(editUrl, this, PortalSettings, true, false);
                            var script = string.Format("<script type=\"text/javascript\">{0}</script>", editUrl);
                            ClientAPI.RegisterStartUpScript(Page, "EditInPopup", script);
                        }
                        else
                        {
                            Response.Redirect(editUrl, true);
                        }
                    }
                    break;
                case "delete":
                    if (TabPermissionController.CanDeletePage(objTab))
                    {
                        TabController.Instance.SoftDeleteTab(objTab.TabID, PortalSettings);
                        BindTree();
                        //keep the parent tab selected
                        if (objTab.ParentId != Null.NullInteger)
                        {
                            SelectedNode = objTab.ParentId.ToString(CultureInfo.InvariantCulture);
                            ctlPages.FindNodeByValue(SelectedNode).Selected = true;
                            ctlPages.FindNodeByValue(SelectedNode).ExpandParentNodes();
                            BindTab(objTab.ParentId);
                        }
                        else
                        {
                            pnlDetails.Visible = false;
                        }
                        ShowSuccessMessage(string.Format(Localization.GetString("TabDeleted", LocalResourceFile), objTab.TabName));
                    }
                    break;
                case "add":
                    if ((objTab!= null && TabPermissionController.CanAddPage(objTab)) || (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName)))
                    {
                        pnlBulk.Visible = true;
                        btnBulkCreate.CommandArgument = e.Node.Value;
                        ctlPages.FindNodeByValue(e.Node.Value).Selected = true;
                        txtBulk.Focus();
                        pnlDetails.Visible = false;
                        //Response.Redirect(NavigateURL(objTab.TabID, "Tab", "action=add", "returntabid=" & TabId.ToString), True)
                    }
                    break;
                case "hide":
                    if (TabPermissionController.CanManagePage(objTab))
                    {
                        objTab.IsVisible = false;
                        TabController.Instance.UpdateTab(objTab);
                        BindTreeAndShowTab(objTab.TabID);
                        ShowSuccessMessage(string.Format(Localization.GetString("TabHidden", LocalResourceFile), objTab.TabName));
                    }
                    break;
                case "show":
                    if (TabPermissionController.CanManagePage(objTab))
                    {
                        objTab.IsVisible = true;
                        TabController.Instance.UpdateTab(objTab);
                        BindTreeAndShowTab(objTab.TabID);
                        ShowSuccessMessage(string.Format(Localization.GetString("TabShown", LocalResourceFile), objTab.TabName));
                    }
                    break;
                case "disable":
                    if (TabPermissionController.CanManagePage(objTab))
                    {
                        objTab.DisableLink = true;
                        TabController.Instance.UpdateTab(objTab);
                        BindTreeAndShowTab(objTab.TabID);
                        ShowSuccessMessage(string.Format(Localization.GetString("TabDisabled", LocalResourceFile), objTab.TabName));
                    }
                    break;
                case "enable":
                    if (TabPermissionController.CanManagePage(objTab))
                    {
                        objTab.DisableLink = false;
                        TabController.Instance.UpdateTab(objTab);
                        BindTreeAndShowTab(objTab.TabID);
                        ShowSuccessMessage(string.Format(Localization.GetString("TabEnabled", LocalResourceFile), objTab.TabName));
                    }
                    break;
            }
        }

        protected void CtlPagesNodeDrop(object sender, RadTreeNodeDragDropEventArgs e)
        {
            if (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName))
            {
                var sourceNode = e.SourceDragNode;
                var destNode = e.DestDragNode;
                var dropPosition = e.DropPosition;
                if (destNode != null)
                {
                    if (sourceNode.TreeView.SelectedNodes.Count <= 1)
                    {
                        PerformDragAndDrop(dropPosition, sourceNode, destNode);
                    }
                    else if (sourceNode.TreeView.SelectedNodes.Count > 1)
                    {
                        foreach (var node in sourceNode.TreeView.SelectedNodes)
                        {
                            PerformDragAndDrop(dropPosition, node, destNode);
                        }
                    }

                    destNode.Expanded = true;

                    foreach (var node in ctlPages.GetAllNodes())
                    {
                        node.Selected = node.Value == e.SourceDragNode.Value;
                    }
                }
            }
        }

        protected void CtlPagesNodeEdit(object sender, RadTreeNodeEditEventArgs e)
        {            
            var objTab = TabController.Instance.GetTab(int.Parse(e.Node.Value), PortalId, false);
            if (objTab != null && TabPermissionController.CanManagePage(objTab))
            {
                //Check for invalid
                string invalidType;
                if (!TabController.IsValidTabName(e.Text, out invalidType))
                {
                    ShowErrorMessage(string.Format(Localization.GetString(invalidType, LocalResourceFile), e.Text));
                    e.Node.Text = objTab.TabName;
                    e.Text = objTab.TabName;
                }
                else if (!IsValidTabPath(objTab, Globals.GenerateTabPath(objTab.ParentId, e.Text)))
                {
                    e.Node.Text = objTab.TabName;
                    e.Text = objTab.TabName;
                }
                else
                {
                    objTab.TabName = e.Text;
                    TabController.Instance.UpdateTab(objTab);
                }

                BindTreeAndShowTab(objTab.TabID);
            }
        }

        protected void OnExpandTreeClick(object sender, EventArgs e)
        {
            var btn = (LinkButton)sender;
            if (btn.CommandName.ToLower() == "expand")
            {
                ctlPages.ExpandAllNodes();
                btn.CommandName = "Collapse";
                cmdExpandTree.Text = LocalizeString("CollapseAll");
            }
            else
            {
                ctlPages.CollapseAllNodes();
                ctlPages.Nodes[0].Expanded = true;
                btn.CommandName = "Expand";
                cmdExpandTree.Text = LocalizeString("ExpandAll");
            }
        }

        protected void GrdModulesNeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            var lst = new List<ModuleInfo>();

            if (ctlPages.SelectedNode != null)
            {
                var tabid = Convert.ToInt32(ctlPages.SelectedNode.Value);
                var dic = ModuleController.Instance.GetTabModules(tabid);

                lst.AddRange(dic.Values.Where(objModule => objModule.IsDeleted == false));
            }

            grdModules.DataSource = lst;
        }

        protected void CtlPagesNodeExpand(object sender, RadTreeNodeEventArgs e)
        {
            AddChildNodes(e.Node);
        }

        protected void OnCreatePagesClick(object sender, EventArgs e)
        {
            if (!PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName)) 
                return;

            var strValue = txtBulk.Text;
            strValue = strValue.Replace("\r", "\n");
            strValue = strValue.Replace(Environment.NewLine, "\n");
            strValue = strValue.Replace("\n" + "\n", "\n").Trim();

            string invalidType;
            if (!TabController.IsValidTabName(strValue, out invalidType))
            {
                ShowErrorMessage(string.Format(Localization.GetString(invalidType, LocalResourceFile), strValue));
                return;
            }

            var pages = strValue.Split(char.Parse("\n"));
            var parentId = Convert.ToInt32(((LinkButton)sender).CommandArgument);
            var rootTab = TabController.Instance.GetTab(parentId, PortalId, true);
            var tabs = new List<TabInfo>();

            foreach (var strLine in pages)
            {
                tabs.Add(new TabInfo
                            {
                                TabName = Regex.Replace(strLine, ">*(.*)", "${1}"),
                                Level = strLine.LastIndexOf(">", StringComparison.Ordinal) + 1
                            });
            }

            var currentIndex = -1;
            foreach (var oTab in tabs)
            {
                currentIndex += 1;

                try
                {
                    if (oTab.Level == 0)
                    {
                        oTab.TabID = CreateTabFromParent(rootTab, oTab.TabName, parentId);
                    }
                    else
                    {
                        var parentTabId = GetParentTabId(tabs, currentIndex, oTab.Level - 1);
                        if (parentTabId != Null.NullInteger)
                        {
                            oTab.TabID = CreateTabFromParent(rootTab, oTab.TabName, parentTabId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.ToString());
                    //Instrumentation.Logger.Error(ex); --this code shows unexpected results.
                }
            }

            var tabId = Convert.ToInt32(tabs[0].TabID);
            if (tabId == Null.NullInteger)
            {
                tabId = parentId;
            }

            txtBulk.Text = string.Empty;
            BindTreeAndShowTab(tabId);
        }

        protected void CmdDeleteModuleClick(object sender, EventArgs e)
        {
            var moduleId = Convert.ToInt32(((ImageButton)sender).CommandArgument);
            var tabId = Convert.ToInt32(ctlPages.SelectedNode.Value);

            ModuleController.Instance.DeleteTabModule(tabId, moduleId, true);
            ModuleController.Instance.ClearCache(tabId);

            grdModules.Rebind();
        }

        protected void CmdUpdateClick(object sender, EventArgs e)
        {
            //Often times grid stays but node is not selected (e.g. when node is deleted or update page is clicked)
            if (ctlPages.SelectedNode == null)
                return;

            var intTab = Convert.ToInt32(ctlPages.SelectedNode.Value);
            var tab = TabController.Instance.GetTab(intTab, PortalId, true);
            Page.Validate();
            if (!Page.IsValid) 
                return;
            if (tab != null && TabPermissionController.CanManagePage(tab))
            {
                tab.TabName = txtName.Text;
                tab.Title = txtTitle.Text;
                tab.Description = txtDescription.Text;
                tab.KeyWords = txtKeywords.Text;
                tab.IsVisible = chkVisible.Checked;
                tab.DisableLink = chkDisabled.Checked;

                tab.IsDeleted = false;
                tab.Url = ctlURL.Url;
                TabController.Instance.UpdateTabSetting(tab.TabID, "LinkNewWindow", ctlURL.NewWindow.ToString());
                TabController.Instance.UpdateTabSetting(tab.TabID, "AllowIndex", chkAllowIndex.Checked.ToString());

                tab.SkinSrc = drpSkin.SelectedValue;
                tab.ContainerSrc = drpContainer.SelectedValue;
                tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);

                tab.TabPermissions.Clear();
                if (tab.PortalID != Null.NullInteger)
                {
                    tab.TabPermissions.AddRange(dgPermissions.Permissions);
                }

                //All validations have been done in the Page.Validate()

                //Check for invalid
                string invalidType;
                if (!TabController.IsValidTabName(tab.TabName, out invalidType))
                {
                    ShowErrorMessage(string.Format(Localization.GetString(invalidType, LocalResourceFile), tab.TabName));
                    return;
                }

                //Validate Tab Path
                if (!IsValidTabPath(tab, tab.TabPath))
                {
                    return;
                }

                tab.RefreshInterval = txtRefresh.Text == "" ? Null.NullInteger : Convert.ToInt32(txtRefresh.Text);

                tab.SiteMapPriority = float.Parse(txtSitemapPriority.Text);
                tab.PageHeadText = txtMeta.Text;
                tab.IsSecure = chkSecure.Checked;
                tab.PermanentRedirect = chkPermanentRedirect.Checked;

                var iconFile = ctlIcon.Url;
                var iconFileLarge = ctlIconLarge.Url;

                tab.IconFile = iconFile;
                tab.IconFileLarge = iconFileLarge;

                tab.Terms.Clear();
                tab.Terms.AddRange(termsSelector.Terms);

                TabController.Instance.UpdateTab(tab);
                ShowSuccessMessage(string.Format(Localization.GetString("TabUpdated", LocalResourceFile), tab.TabName));

                BindTree();

                //keep the tab selected
                SelectedNode = intTab.ToString(CultureInfo.InvariantCulture);
                ctlPages.FindNodeByValue(SelectedNode).Selected = true;
                ctlPages.FindNodeByValue(SelectedNode).ExpandParentNodes();
            }
        }

        #endregion

        #region Private Methods

        private void AddAttributes(ref RadTreeNode node, TabInfo tab)
        {
            var canView = true;
            bool canEdit;
            bool canAdd;
            bool canDelete;
            bool canHide;
            bool canMakeVisible;
            bool canEnable;
            bool canDisable;
            bool canMakeHome;

            if (node.Attributes["isPortalRoot"] != null && Boolean.Parse(node.Attributes["isPortalRoot"]))
            {
                canAdd = PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName);
                canView = false;
                canEdit = false;
                canDelete = false;
                canHide = false;
                canMakeVisible = false;
                canEnable = false;
                canDisable = false;
                canMakeHome = false;
            }
            else if (tab == null)
            {
                canView = false;
                canEdit = false;
                canAdd = false;
                canDelete = false;
                canHide = false;
                canMakeVisible = false;
                canEnable = false;
                canDisable = false;
                canMakeHome = false;
            }
            else
            {
                canAdd = TabPermissionController.CanAddPage(tab);
                canDelete = TabPermissionController.CanDeletePage(tab);
                canMakeVisible = canHide = canDisable = canEnable = canEdit = TabPermissionController.CanManagePage(tab);
                canMakeHome = PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName) && !tab.DisableLink;

                if (TabController.IsSpecialTab(tab.TabID, PortalSettings.PortalId))
                {
                    canDelete = false;
                    canMakeHome = false;
                }

                if (rblMode.SelectedValue == "H")
                {
                    canMakeHome = false;
                }

                if (tab.IsVisible)
                {
                    canMakeVisible = false;
                }
                else
                {
                    canHide = false;
                }

				if (tab.DisableLink 
					|| (tab.TabID == PortalSettings.AdminTabId || tab.TabID == PortalSettings.SplashTabId ||
							tab.TabID == PortalSettings.HomeTabId || tab.TabID == PortalSettings.LoginTabId ||
							tab.TabID == PortalSettings.UserTabId || tab.TabID == PortalSettings.SuperTabId))
                {
                    canDisable = false;
                }
				
				if (!tab.DisableLink)
                {
                    canEnable = false;
                }
            }

            node.Attributes.Add("CanView", canView.ToString(CultureInfo.InvariantCulture));
            node.Attributes.Add("CanEdit", canEdit.ToString(CultureInfo.InvariantCulture));
            node.Attributes.Add("CanAdd", canAdd.ToString(CultureInfo.InvariantCulture));
            node.Attributes.Add("CanDelete", canDelete.ToString(CultureInfo.InvariantCulture));
            node.Attributes.Add("CanHide", canHide.ToString(CultureInfo.InvariantCulture));
            node.Attributes.Add("CanMakeVisible", canMakeVisible.ToString(CultureInfo.InvariantCulture));
            node.Attributes.Add("CanEnable", canEnable.ToString(CultureInfo.InvariantCulture));
            node.Attributes.Add("CanDisable", canDisable.ToString(CultureInfo.InvariantCulture));
            node.Attributes.Add("CanMakeHome", canMakeHome.ToString(CultureInfo.InvariantCulture));

            node.AllowEdit = canEdit;
        }

        private void AddChildNodes(RadTreeNode parentNode)
        {
            parentNode.Nodes.Clear();

            var parentId = int.Parse(parentNode.Value);

            foreach (var objTab in Tabs)
            {
                if (objTab.ParentId == parentId)
                {
                    var node = new RadTreeNode
                    {
                        Text = string.Format("{0} {1}", objTab.TabName, GetNodeStatusIcon(objTab)),
                        Value = objTab.TabID.ToString(CultureInfo.InvariantCulture),
                        AllowEdit = true,
                        ImageUrl = GetNodeIcon(objTab)
                    };
                    AddAttributes(ref node, objTab);
                    //If objTab.HasChildren Then
                    //    node.ExpandMode = TreeNodeExpandMode.ServerSide
                    //End If

                    AddChildNodes(node);
                    parentNode.Nodes.Add(node);
                }
            }
        }

        private void BindSkinsAndContainers()
        {
            drpSkin.PortalId = PortalId;
            drpSkin.RootPath = SkinController.RootSkin;
            drpSkin.Scope = SkinScope.All;
            drpSkin.IncludeNoneSpecificItem = true;
            drpSkin.NoneSpecificText = Localization.GetString("DefaultSkin", LocalResourceFile);

            drpContainer.PortalId = PortalId;
            drpContainer.RootPath = SkinController.RootContainer;
            drpContainer.Scope = SkinScope.All;
            drpContainer.IncludeNoneSpecificItem = true;
            drpContainer.NoneSpecificText = Localization.GetString("DefaultContainer", LocalResourceFile);
        }

        private void BindTab(int tabId)
        {
            pnlBulk.Visible = false;

            var tab = TabController.Instance.GetTab(tabId, PortalId, true);
            
            if (tab != null)
            {
                //check for manage permissions
                if (!TabPermissionController.CanManagePage(tab))
                {
                    pnlDetails.Visible = false;
                    return;
                }

                pnlDetails.Visible = true;

                SelectedNode = tabId.ToString(CultureInfo.InvariantCulture);

                //Bind TabPermissionsGrid to TabId 
                dgPermissions.TabID = tab.TabID;
                dgPermissions.DataBind();

                var returnUrl = Globals.NavigateURL(TabId, string.Empty, "edittabid=" + tabId, "isHost=" + (rblMode.SelectedValue == "H"));
                cmdMore.NavigateUrl = ModuleContext.NavigateUrl(tabId, "", false, "ctl=Tab", "action=edit", "returnurl=" + returnUrl);

                txtTitle.Text = tab.Title;
                txtName.Text = tab.TabName;
                chkVisible.Checked = tab.IsVisible;

                txtSitemapPriority.Text = tab.SiteMapPriority.ToString();
                txtDescription.Text = tab.Description;
                txtKeywords.Text = tab.KeyWords;
                txtMeta.Text = tab.PageHeadText;                
                if (tab.RefreshInterval != Null.NullInteger)
                {
                    txtRefresh.Text = tab.RefreshInterval.ToString(); 
                }

                drpSkin.SelectedValue = tab.SkinSrc;
                drpContainer.SelectedValue = tab.ContainerSrc;

                ctlURL.Url = tab.Url;
                if (string.IsNullOrEmpty(tab.Url))
                {
                    ctlURL.UrlType = "N";
                }
                bool newWindow = false;
                if (tab.TabSettings["LinkNewWindow"] != null && Boolean.TryParse((string)tab.TabSettings["LinkNewWindow"], out newWindow) && newWindow)
                {
                    ctlURL.NewWindow = newWindow;
                }

                chkPermanentRedirect.Checked = tab.PermanentRedirect;
                txtKeywords.Text = tab.KeyWords;
                txtDescription.Text = tab.Description;

                chkDisabled.Checked = tab.DisableLink;
                if (tab.TabID == PortalSettings.AdminTabId || tab.TabID == PortalSettings.SplashTabId ||
                    tab.TabID == PortalSettings.HomeTabId || tab.TabID == PortalSettings.LoginTabId ||
                    tab.TabID == PortalSettings.UserTabId || tab.TabID == PortalSettings.SuperTabId)
                {
                    chkDisabled.Enabled = false;
                }
                else
                {
					chkDisabled.Enabled = true;
                }

                if (PortalSettings.SSLEnabled)
                {
                    chkSecure.Enabled = true;
                    chkSecure.Checked = tab.IsSecure;
                }
                else
                {
                    chkSecure.Enabled = false;
                    chkSecure.Checked = tab.IsSecure;
                }
                var allowIndex = false;
                chkAllowIndex.Checked = !tab.TabSettings.ContainsKey("AllowIndex") || !bool.TryParse(tab.TabSettings["AllowIndex"].ToString(), out allowIndex) || allowIndex;

                ctlIcon.Url = tab.IconFileRaw;
                ctlIconLarge.Url = tab.IconFileLargeRaw;

                ShowPermissions(!tab.IsSuperTab && TabPermissionController.CanAdminPage());

                termsSelector.PortalId = tab.PortalID;
                termsSelector.Terms = tab.Terms;
                termsSelector.DataBind();

                grdModules.Rebind();
            }
        }

        private void BindTree()
        {
            ctlPages.Nodes.Clear();

            var rootNode = new RadTreeNode();
            var strParent = "-1";

            if (Settings["ParentPageFilter"] != null)
            {
                strParent = Convert.ToString(Settings["ParentPageFilter"]);
            }

            if (strParent == "-1")
            {
                rootNode.Text = PortalSettings.PortalName;
                rootNode.ImageUrl = IconPortal;
                rootNode.Value = Null.NullInteger.ToString(CultureInfo.InvariantCulture);
                rootNode.Expanded = true;
                rootNode.AllowEdit = false;
                rootNode.EnableContextMenu = true;
                rootNode.Attributes.Add("isPortalRoot", "True");
                AddAttributes(ref rootNode, null);
            }
            else
            {
                var parent = TabController.Instance.GetTab(Convert.ToInt32(strParent), -1, false);
                if (parent != null)
                {
                    rootNode.Text = parent.TabName;
                    rootNode.ImageUrl = IconPortal;
                    rootNode.Value = parent.TabID.ToString(CultureInfo.InvariantCulture);
                    rootNode.Expanded = true;
                    rootNode.EnableContextMenu = true;
                    rootNode.PostBack = false;
                }
            }


            foreach (var tab in Tabs)
            {
                if (TabPermissionController.CanViewPage(tab))
                {
                    if (strParent != "-1")
                    {
                        if (tab.ParentId == Convert.ToInt32(strParent))
                        {
                            var node = new RadTreeNode
                            {
                                Text = string.Format("{0} {1}", tab.TabName, GetNodeStatusIcon(tab)),
                                Value = tab.TabID.ToString(CultureInfo.InvariantCulture),
                                AllowEdit = true,
                                ImageUrl = GetNodeIcon(tab)
                            };
                            AddAttributes(ref node, tab);

                            AddChildNodes(node);
                            rootNode.Nodes.Add(node);
                        }
                    }
                    else
                    {
                        if (tab.Level == 0)
                        {
                            var node = new RadTreeNode
                            {
                                Text = string.Format("{0} {1}", tab.TabName, GetNodeStatusIcon(tab)),
                                Value = tab.TabID.ToString(CultureInfo.InvariantCulture),
                                AllowEdit = true,
                                ImageUrl = GetNodeIcon(tab)
                            };
                            AddAttributes(ref node, tab);

                            AddChildNodes(node);
                            rootNode.Nodes.Add(node);
                        }
                    }
                }
            }

            ctlPages.Nodes.Add(rootNode);
            //AttachContextMenu(ctlPages)

            if (SelectedNode != null)
            {
                if (!Page.IsPostBack)
                {
                    try
                    {
                        ctlPages.FindNodeByValue(SelectedNode).Selected = true;
                        ctlPages.FindNodeByValue(SelectedNode).ExpandParentNodes();
                        var tabid = Convert.ToInt32(SelectedNode);
                        BindTab(tabid);
                        pnlBulk.Visible = false;
                    }
                    catch (Exception exc)
                    {
                        Exceptions.ProcessModuleLoadException(this, exc);
                    }
                }
            }
        }

        private void BindTreeAndShowTab(int tabId)
        {
            BindTree();
            var node = ctlPages.FindNodeByValue(tabId.ToString(CultureInfo.InvariantCulture));
            //rare cases it is null (e.g. when a page is created when page local is not default locale)
            if (node != null)
            {
                node.Selected = true;
                node.ExpandParentNodes();
            }

            BindTab(tabId);
        }

        private void CheckSecurity()
        {
            if ((!(TabPermissionController.HasTabPermission("CONTENT"))) && !(ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "CONTENT, EDIT")))
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }
        }

        private string GetNodeIcon(TabInfo tab)
        {
            if (PortalSettings.HomeTabId == tab.TabID)
            {
                return IconHome;
            }

            if (IsSecuredTab(tab))
            {
                if (IsAdminTab(tab))
                {
                    return AdminOnlyIcon;
                }

                if (IsRegisteredUserTab(tab))
                {
                    return RegisteredUsersIcon;
                }

                return SecuredIcon;
            }

            return AllUsersIcon;
        }

        private string GetNodeStatusIcon(TabInfo tab)
        {
            string s = "";
            if (tab.DisableLink)
            {
                s = s + string.Format("<img src=\"{0}\" alt=\"\" title=\"{1}\" class=\"statusicon\" />", IconPageDisabled, LocalizeString("lblDisabled"));
            }
            if (tab.IsVisible == false)
            {
                s = s + string.Format("<img src=\"{0}\" alt=\"\" title=\"{1}\" class=\"statusicon\" />", IconPageHidden, LocalizeString("lblHidden"));
            }
            if (tab.Url != "")
            {
                s = s + string.Format("<img src=\"{0}\" alt=\"\" title=\"{1}\" class=\"statusicon\" />", IconRedirect, LocalizeString("lblRedirect"));
            }
            return s;
        }

        private bool IsAdminTab(TabInfo tab)
        {
            var perms = tab.TabPermissions;
            return perms.Cast<TabPermissionInfo>().All(perm => perm.RoleName == PortalSettings.AdministratorRoleName || !perm.AllowAccess);
        }

        private static bool IsNumeric(object expression)
        {
            if (expression == null)
                return false;

            double testDouble;
            if (double.TryParse(expression.ToString(), out testDouble))
                return true;

            //VB's 'IsNumeric' returns true for any boolean value:
            bool testBool;
            return bool.TryParse(expression.ToString(), out testBool);
        }

        private bool IsRegisteredUserTab(TabInfo tab)
        {
            var perms = tab.TabPermissions;
            return perms.Cast<TabPermissionInfo>().Any(perm => perm.RoleName == PortalSettings.RegisteredRoleName && perm.AllowAccess);
        }

        private static bool IsSecuredTab(TabInfo tab)
        {
            var perms = tab.TabPermissions;
            return perms.Cast<TabPermissionInfo>().All(perm => perm.RoleName != Globals.glbRoleAllUsersName || !perm.AllowAccess);
        }

        private void LocalizeControl()
        {
            ctlIcon.ShowFiles = true;
            ctlIcon.ShowImages = true;
            ctlIcon.ShowTabs = false;
            ctlIcon.ShowUrls = false;
            ctlIcon.Required = false;

            ctlIcon.ShowLog = false;
            ctlIcon.ShowNewWindow = false;
            ctlIcon.ShowTrack = false;
            ctlIcon.FileFilter = Globals.glbImageFileTypes;
            ctlIcon.Width = "275px";

            ctlIconLarge.ShowFiles = ctlIcon.ShowFiles;
            ctlIconLarge.ShowImages = ctlIcon.ShowImages;
            ctlIconLarge.ShowTabs = ctlIcon.ShowTabs;
            ctlIconLarge.ShowUrls = ctlIcon.ShowUrls;
            ctlIconLarge.Required = ctlIcon.Required;

            ctlIconLarge.ShowLog = ctlIcon.ShowLog;
            ctlIconLarge.ShowNewWindow = ctlIcon.ShowNewWindow;
            ctlIconLarge.ShowTrack = ctlIcon.ShowTrack;
            ctlIconLarge.FileFilter = ctlIcon.FileFilter;
            ctlIconLarge.Width = ctlIcon.Width;

            ctlPages.ContextMenus[0].Items[0].Text = LocalizeString("ViewPage");
            ctlPages.ContextMenus[0].Items[1].Text = LocalizeString("EditPage");
            ctlPages.ContextMenus[0].Items[2].Text = LocalizeString("DeletePage");
            ctlPages.ContextMenus[0].Items[3].Text = LocalizeString("AddPage");
            ctlPages.ContextMenus[0].Items[4].Text = LocalizeString("HidePage");
            ctlPages.ContextMenus[0].Items[5].Text = LocalizeString("ShowPage");
            ctlPages.ContextMenus[0].Items[6].Text = LocalizeString("EnablePage");
            ctlPages.ContextMenus[0].Items[7].Text = LocalizeString("DisablePage");
            ctlPages.ContextMenus[0].Items[8].Text = LocalizeString("MakeHome");

            lblBulkIntro.Text = LocalizeString("BulkCreateIntro");
            btnBulkCreate.Text = LocalizeString("btnBulkCreate");

            ctlPages.ContextMenus[0].Items[0].ImageUrl = IconView;
            ctlPages.ContextMenus[0].Items[1].ImageUrl = IconEdit;
            ctlPages.ContextMenus[0].Items[2].ImageUrl = IconDelete;
            ctlPages.ContextMenus[0].Items[3].ImageUrl = IconAdd;
            ctlPages.ContextMenus[0].Items[4].ImageUrl = IconPageHidden;
            ctlPages.ContextMenus[0].Items[5].ImageUrl = IconPageHidden;
            ctlPages.ContextMenus[0].Items[6].ImageUrl = IconPageDisabled;
            ctlPages.ContextMenus[0].Items[7].ImageUrl = IconPageDisabled;
            ctlPages.ContextMenus[0].Items[8].ImageUrl = IconHome;

            rblMode.Items[0].Text = LocalizeString("ShowPortalTabs");
            rblMode.Items[1].Text = LocalizeString("ShowHostTabs");

            cmdExpandTree.Text = LocalizeString("ExpandAll");
            lblDisabled.Text = LocalizeString("lblDisabled");
            lblHidden.Text = LocalizeString("lblHidden");
            lblRedirect.Text = LocalizeString("lblRedirect");
            lblHome.Text = LocalizeString("lblHome");
            lblSecure.Text = LocalizeString("lblSecure");
            lblEveryone.Text = LocalizeString("lblEveryone");
            lblRegistered.Text = LocalizeString("lblRegistered");
            lblAdminOnly.Text = LocalizeString("lblAdminOnly");
        }

        private bool MoveTab(TabInfo tab, TabInfo targetTab, Position position)
        {
            //Validate Tab Path
            if (targetTab == null || !IsValidTabPath(tab, Globals.GenerateTabPath((targetTab == null) ? Null.NullInteger : targetTab.TabID, tab.TabName)))
            {
                return false;
            }

            switch (position)
            {
                case Position.Above:
                    TabController.Instance.MoveTabBefore(tab, targetTab.TabID);
                    break;
                case Position.Below:
                    TabController.Instance.MoveTabAfter(tab, targetTab.TabID);
                    break;
            }

            ShowSuccessMessage(string.Format(Localization.GetString("TabMoved", LocalResourceFile), tab.TabName));
            return true;
        }

        private bool MoveTabToParent(TabInfo tab, TabInfo targetTab)
        {
            //Validate Tab Path
            if (!IsValidTabPath(tab, Globals.GenerateTabPath((targetTab == null) ? Null.NullInteger : targetTab.TabID, tab.TabName)))
            {
                return false;
            }

            TabController.Instance.MoveTabToParent(tab, (targetTab == null) ? Null.NullInteger : targetTab.TabID);

            ShowSuccessMessage(string.Format(Localization.GetString("TabMoved", LocalResourceFile), tab.TabName));
            return true;
        }

        private void PerformDragAndDrop(RadTreeViewDropPosition dropPosition, RadTreeNode sourceNode, RadTreeNode destNode)
        {
            var sourceTab = TabController.Instance.GetTab(int.Parse(sourceNode.Value), PortalId, false);
            var targetTab = TabController.Instance.GetTab(int.Parse(destNode.Value), PortalId, false);

            switch (dropPosition)
            {
                case RadTreeViewDropPosition.Over:
                    if (!(sourceNode.IsAncestorOf(destNode)))
                    {
                        if (MoveTabToParent(sourceTab, targetTab))
                        {
                            sourceNode.Owner.Nodes.Remove(sourceNode);
                            destNode.Nodes.Add(sourceNode);
                        }
                    }
                    break;
                case RadTreeViewDropPosition.Above:
                    if (MoveTab(sourceTab, targetTab, Position.Above))
                    {
                        sourceNode.Owner.Nodes.Remove(sourceNode);
                        destNode.InsertBefore(sourceNode);
                    }
                    break;
                case RadTreeViewDropPosition.Below:
                    if (MoveTab(sourceTab, targetTab, Position.Below))
                    {
                        sourceNode.Owner.Nodes.Remove(sourceNode);
                        destNode.InsertAfter(sourceNode);
                    }
                    break;
            }
        }

        private void ShowPermissions(bool show)
        {
            PermissionsSection.Visible = show;
        }

        #endregion

        #region Public Methods

        public string GetConfirmString()
        {
            return ClientAPI.GetSafeJSString(Localization.GetString("ConfirmDelete", LocalResourceFile));
        }

        public string ModuleEditUrl(int moduleId)
        {
            if (IsNumeric(moduleId))
            {
                var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
                if (module != null)
                {
                    return ModuleContext.NavigateUrl(module.TabID, "", false, "ctl=Module", "ModuleId=" + moduleId);
                }
            }

            return "#";
        }

        #endregion

        #region Tab Moving Helpers

        private void ApplyDefaultTabTemplate(TabInfo tab)
        {
            var templateFile = Path.Combine(PortalSettings.HomeDirectoryMapPath, "Templates\\" + DefaultPageTemplate);
            if (File.Exists(templateFile))
            {
                var xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(templateFile);
                    TabController.DeserializePanes(xmlDoc.SelectSingleNode("//portal/tabs/tab/panes"), tab.PortalID, tab.TabID, PortalTemplateModuleAction.Ignore, new Hashtable());
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                    throw new DotNetNukeException("Unable to process page template.", ex, DotNetNukeErrorCode.DeserializePanesFailed);
                }
            }
        }

        private int CreateTabFromParent(TabInfo objRoot, string tabName, int parentId)
        {
            var tab = new TabInfo
            {
                PortalID = PortalId,
                TabName = tabName,
                ParentId = parentId,
                Title = "",
                Description = "",
                KeyWords = "",
                IsVisible = true,
                DisableLink = false,
                IconFile = "",
                IconFileLarge = "",
                IsDeleted = false,
                Url = "",
                SkinSrc = "",
                ContainerSrc = "",
                CultureCode = Null.NullString
            };

            if (objRoot != null)
            {
                tab.IsVisible = objRoot.IsVisible;
                tab.DisableLink = objRoot.DisableLink;
                tab.SkinSrc = objRoot.SkinSrc;
                tab.ContainerSrc = objRoot.ContainerSrc;
            }

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings.ContentLocalizationEnabled)
            {
                tab.CultureCode = LocaleController.Instance.GetDefaultLocale(tab.PortalID).Code;
            }

            var parentTab = TabController.Instance.GetTab(parentId, -1, false);

            if (parentTab != null)
            {
                tab.PortalID = parentTab.PortalID;
                tab.ParentId = parentTab.TabID;
                if (parentTab.IsSuperTab)
                    ShowPermissions(false);
            }
            else
            {
                //return Null.NullInteger;
                tab.PortalID = PortalId;
                tab.ParentId = Null.NullInteger;
            }

            tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);

            //Check for invalid
            string invalidType;
            if (!TabController.IsValidTabName(tab.TabName, out invalidType))
            {
                ShowErrorMessage(string.Format(Localization.GetString(invalidType, LocalResourceFile), tab.TabName));
                return Null.NullInteger;
            }

            //Validate Tab Path
            if (!IsValidTabPath(tab, tab.TabPath))
            {
                return Null.NullInteger;
            }

            //Inherit permissions from parent
            tab.TabPermissions.Clear();
            if (tab.PortalID != Null.NullInteger && objRoot != null)
            {
                tab.TabPermissions.AddRange(objRoot.TabPermissions);
            }
            else if (tab.PortalID != Null.NullInteger)
            {
                //Give admin full permission
                ArrayList permissions = PermissionController.GetPermissionsByTab();

                foreach (PermissionInfo permission in permissions)
                {
                    var newTabPermission = new TabPermissionInfo
                    {
                        PermissionID = permission.PermissionID,
                        PermissionKey = permission.PermissionKey,
                        PermissionName = permission.PermissionName,
                        AllowAccess = true,
                        RoleID = PortalSettings.Current.AdministratorRoleId
                    };
                    tab.TabPermissions.Add(newTabPermission);
                }
            }

            //Inherit other information from Parent
            if (objRoot != null)
            {
                tab.Terms.Clear();
                tab.StartDate = objRoot.StartDate;
                tab.EndDate = objRoot.EndDate;
                tab.RefreshInterval = objRoot.RefreshInterval;
                tab.SiteMapPriority = objRoot.SiteMapPriority;
                tab.PageHeadText = objRoot.PageHeadText;
                tab.IsSecure = objRoot.IsSecure;
                tab.PermanentRedirect = objRoot.PermanentRedirect;
            }

            tab.TabID = TabController.Instance.AddTab(tab);
            ApplyDefaultTabTemplate(tab);

            //create localized tabs if content localization is enabled
            if (portalSettings.ContentLocalizationEnabled)
            {
                TabController.Instance.CreateLocalizedCopies(tab);
            }

            ShowSuccessMessage(string.Format(Localization.GetString("TabCreated", LocalResourceFile), tab.TabName));
            return tab.TabID;
        }

        private static int GetParentTabId(List<TabInfo> lstTabs, int currentIndex, int parentLevel)
        {
            var oParent = lstTabs[0];

            for (var i = 0; i < lstTabs.Count; i++)
            {
                if (i == currentIndex)
                {
                    return oParent.TabID;
                }
                if (lstTabs[i].Level == parentLevel)
                {
                    oParent = lstTabs[i];
                }
            }

            return Null.NullInteger;
        }

        private bool IsValidTabPath(TabInfo tab, string newTabPath)
        {
            var valid = true;

            //get default culture if the tab's culture is null
            var cultureCode = tab.CultureCode;
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = PortalSettings.DefaultLanguage;
            }

            //Validate Tab Path
            var tabID = TabController.GetTabByTabPath(tab.PortalID, newTabPath, cultureCode);
            if (tabID != Null.NullInteger && tabID != tab.TabID)
            {
                var existingTab = TabController.Instance.GetTab(tabID, tab.PortalID, false);
                if (existingTab != null && existingTab.IsDeleted)
                    ShowErrorMessage(Localization.GetString("TabRecycled", LocalResourceFile));
                else
                    ShowErrorMessage(Localization.GetString("TabExists", LocalResourceFile));

                valid = false;
            }

            //check whether have conflict between tab path and portal alias.
            if (TabController.IsDuplicateWithPortalAlias(tab.PortalID, newTabPath))
            {
                ShowErrorMessage(Localization.GetString("PathDuplicateWithAlias", LocalResourceFile));
                valid = false;
            }

            return valid;
        }

        private void ShowErrorMessage(string message)
        {
            Skin.AddModuleMessage(this, message, ModuleMessage.ModuleMessageType.RedError);
        }

        private void ShowSuccessMessage(string message)
        {
            Skin.AddModuleMessage(this, message, ModuleMessage.ModuleMessageType.GreenSuccess);
        }

        private void ShowWarningMessage(string message)
        {
            Skin.AddModuleMessage(this, message, ModuleMessage.ModuleMessageType.YellowWarning);
        }

        #endregion
    }
}
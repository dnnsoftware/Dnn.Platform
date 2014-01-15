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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Tabs
{

    public partial class Import : PortalModuleBase
    {

        private TabInfo _tab;

        public TabInfo Tab
        {
            get
            {
                if (_tab == null)
                {
                    var objTabs = new TabController();
                    _tab = objTabs.GetTab(TabId, PortalId, false);
                }
                return _tab;
            }
        }

        private void BindBeforeAfterTabControls()
        {
            var noneSpecified = "<" + Localization.GetString("None_Specified") + ">";
            cboParentTab.UndefinedItem = new ListItem(noneSpecified, string.Empty);
            var parentTab = cboParentTab.SelectedPage;

            List<TabInfo> listTabs = parentTab != null ? new TabController().GetTabsByPortal(parentTab.PortalID).WithParentId(parentTab.TabID) : new TabController().GetTabsByPortal(PortalId).WithParentId(Null.NullInteger);
            listTabs = TabController.GetPortalTabs(listTabs, Null.NullInteger, true, noneSpecified, false, false, false, false, true);
            cboPositionTab.DataSource = listTabs;
            cboPositionTab.DataBind();
            rbInsertPosition.Items.Clear();
            rbInsertPosition.Items.Add(new ListItem(Localization.GetString("InsertBefore", LocalResourceFile), "Before"));
            rbInsertPosition.Items.Add(new ListItem(Localization.GetString("InsertAfter", LocalResourceFile), "After"));
            rbInsertPosition.Items.Add(new ListItem(Localization.GetString("InsertAtEnd", LocalResourceFile), "AtEnd"));
            rbInsertPosition.SelectedValue = "After";
        }

        private void BindFiles()
        {
            cboTemplate.Items.Clear();
            if (cboFolders.SelectedItem != null)
            {
                var folder = FolderManager.Instance.GetFolder(cboFolders.SelectedItemValueAsInt);
                if (folder != null)
                {
                    var files = Directory.GetFiles(PortalSettings.HomeDirectoryMapPath + folder.FolderPath, "*.page.template");
                    foreach (var file in files)
                    {
                        var f = file.Replace(PortalSettings.HomeDirectoryMapPath + folder.FolderPath, "");
                        cboTemplate.AddItem(f.Replace(".page.template", ""), f);
                    }
                    cboTemplate.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "None_Specified");
                    cboTemplate.SelectedIndex = 0;
                }

            }
        }

        private void BindTabControls()
        {
            BindBeforeAfterTabControls();
            divInsertPositionRow.Visible = cboPositionTab.Items.Count > 0;
            cboParentTab.AutoPostBack = true;
            if (cboPositionTab.FindItemByValue(TabId.ToString(CultureInfo.InvariantCulture)) != null)
            {
                cboPositionTab.ClearSelection();
                cboPositionTab.FindItemByValue(TabId.ToString(CultureInfo.InvariantCulture)).Selected = true;
            }
        }

        private void DisplayNewRows()
        {
            divTabName.Visible = (optMode.SelectedIndex == 0);
            divParentTab.Visible = (optMode.SelectedIndex == 0);
            divInsertPositionRow.Visible = (optMode.SelectedIndex == 0);
            divInsertPositionRow.Visible = (optMode.SelectedIndex == 0);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!TabPermissionController.CanImportPage())
            {
                Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cboFolders.SelectionChanged += OnFolderIndexChanged;
            cmdImport.Click += OnImportClick;
            cboParentTab.SelectionChanged += OnParentTabIndexChanged;
            cboTemplate.SelectedIndexChanged += OnTemplateIndexChanged;
            optMode.SelectedIndexChanged += OptModeSelectedIndexChanged;

            try
            {
                if (!Page.IsPostBack)
                {
                    cmdCancel.NavigateUrl = Globals.NavigateURL();
                    cboFolders.UndefinedItem = new ListItem("<" + Localization.GetString("None_Specified") + ">", string.Empty);
                    var folders = FolderManager.Instance.GetFolders(UserInfo, "BROWSE, ADD");
                    var templateFolder = folders.SingleOrDefault(f => f.FolderPath == "Templates/");
                    if (templateFolder != null) cboFolders.SelectedFolder = templateFolder;

                    BindFiles();
                    BindTabControls();
                    DisplayNewRows();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnFolderIndexChanged(object sender, EventArgs e)
        {
            BindFiles();
        }

        protected void OnImportClick(object sender, EventArgs e)
        {
            try
            {
                if (cboTemplate.SelectedItem == null || cboTemplate.SelectedValue == "None_Specified")
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SpecifyFile", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }
                if (optMode.SelectedIndex == -1)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SpecifyMode", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }
                if (cboFolders.SelectedItem == null) return;
                var selectedFolder = FolderManager.Instance.GetFolder(cboFolders.SelectedItemValueAsInt);
                if (selectedFolder == null) return;

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(PortalSettings.HomeDirectoryMapPath + selectedFolder.FolderPath + cboTemplate.SelectedValue);

                var tabNodes = new List<XmlNode>();
                var selectSingleNode = xmlDoc.SelectSingleNode("//portal/tabs");
                if (selectSingleNode != null)
                {
                    tabNodes.AddRange(selectSingleNode.ChildNodes.Cast<XmlNode>());
                }
                if (tabNodes.Count == 0)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoTabsInTemplate", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                TabInfo objTab;
                if (optMode.SelectedValue == "ADD")
                {
                    if (string.IsNullOrEmpty(txtTabName.Text))
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SpecifyName", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                        return;
                    }

                    //New Tab
                    objTab = new TabInfo { PortalID = PortalId, TabName = txtTabName.Text, IsVisible = true };
                    var parentId = cboParentTab.SelectedItemValueAsInt;
                    if (parentId != Null.NullInteger)
                    {
                        objTab.ParentId = parentId;
                    }
                    objTab.TabPath = Globals.GenerateTabPath(objTab.ParentId, objTab.TabName);
                    var tabId = TabController.GetTabByTabPath(objTab.PortalID, objTab.TabPath, Null.NullString);
                    var objTabs = new TabController();

                    //Check if tab exists
                    if (tabId != Null.NullInteger)
                    {
                        TabInfo existingTab = objTabs.GetTab(tabId, PortalId, false);
                        if (existingTab != null && existingTab.IsDeleted)
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabRecycled", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                        }
                        else
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabExists", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                        }
                        return;
                    }

                    var positionTabId = Int32.Parse(cboPositionTab.SelectedItem.Value);

                    //var pc = new PermissionController();

                    //var permission = pc.GetPermissionByCodeAndKey("SYSTEM_TAB", "VIEW");
                    //if (permission.Count > 0)
                    //{
                    //    var pid = ((PermissionInfo)permission[0]).PermissionID;
                    //    objTab.TabPermissions.Add(new TabPermissionInfo { PermissionID = pid, AllowAccess = true, RoleID = 0 });
                    //}

                    //permission = pc.GetPermissionByCodeAndKey("SYSTEM_TAB", "EDIT");
                    //if (permission.Count > 0)
                    //{
                    //    var pid = ((PermissionInfo)permission[0]).PermissionID;
                    //    objTab.TabPermissions.Add(new TabPermissionInfo { PermissionID = pid, AllowAccess = true, RoleID = 0 });
                    //}

                    var objEventLog = new EventLogController();
                    if (rbInsertPosition.SelectedValue == "After" && positionTabId > Null.NullInteger)
                    {
                        objTab.TabID = objTabs.AddTabAfter(objTab, positionTabId);
                    }
                    else if (rbInsertPosition.SelectedValue == "Before" && positionTabId > Null.NullInteger)
                    {
                        objTab.TabID = objTabs.AddTabBefore(objTab, positionTabId);
                    }
                    else
                    {
                        objTab.TabID = objTabs.AddTab(objTab);
                    }
                    objEventLog.AddLog(objTab, PortalSettings, UserId, "", EventLogController.EventLogType.TAB_CREATED);

                    objTab = TabController.DeserializeTab(tabNodes[0], objTab, PortalId, PortalTemplateModuleAction.Replace);

                    var exceptions = string.Empty;
                    //Create second tabs onwards. For firs tab, we like to use tab details from text box, for rest it'll come from template
                    for (var tab = 1; tab < tabNodes.Count; tab++)
                    {
                        try
                        {
                            TabController.DeserializeTab(tabNodes[tab], null, PortalId, PortalTemplateModuleAction.Replace);
                        }
                        catch (Exception ex)
                        {
                            Exceptions.LogException(ex);
                            exceptions += string.Format("Template Tab # {0}. Error {1}<br/>", tab + 1, ex.Message);
                        }
                    }
                    if (!string.IsNullOrEmpty(exceptions))
                    {
                        UI.Skins.Skin.AddModuleMessage(this, exceptions, ModuleMessage.ModuleMessageType.RedError);
                        return;
                    }
                }
                else
                {
                    //Replace Existing Tab
                    objTab = TabController.DeserializeTab(tabNodes[0], Tab, PortalId, PortalTemplateModuleAction.Replace);
                }
                switch (optRedirect.SelectedValue)
                {
                    case "VIEW":
                        Response.Redirect(Globals.NavigateURL(objTab.TabID), true);
                        break;
                    default:
                        Response.Redirect(Globals.NavigateURL(objTab.TabID, "Tab", "action=edit"), true);
                        break;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnParentTabIndexChanged(object sender, EventArgs e)
        {
            BindBeforeAfterTabControls();
        }

        protected void OnTemplateIndexChanged(Object sender, EventArgs e)
        {
            try
            {
                if (cboTemplate.SelectedIndex > 0 && cboFolders.SelectedItem != null)
                {

                    var selectedFolder = FolderManager.Instance.GetFolder(cboFolders.SelectedItemValueAsInt);
                    var filename = PortalSettings.HomeDirectoryMapPath + selectedFolder.FolderPath + cboTemplate.SelectedValue;
                    var xmldoc = new XmlDocument();
                    xmldoc.Load(filename);
                    var node = xmldoc.SelectSingleNode("//portal/description");
                    if (node != null && !String.IsNullOrEmpty(node.InnerXml))
                    {
                        lblTemplateDescription.Visible = true;
                        lblTemplateDescription.Text = Server.HtmlDecode(node.InnerXml);
                        txtTabName.Text = cboTemplate.SelectedItem.Text;
                    }
                    else
                    {
                        lblTemplateDescription.Visible = false;
                    }
                }
                else
                {
                    lblTemplateDescription.Visible = false;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OptModeSelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayNewRows();
        }

    }
}
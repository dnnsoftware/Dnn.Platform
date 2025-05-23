﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Tabs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
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
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The view for the global import page action.</summary>
    public partial class Import : PortalModuleBase
    {
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly INavigationManager navigationManager;
        private readonly IEventLogger eventLogger;

        private TabInfo tab;

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        public Import()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public Import(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager)
            : this(businessControllerProvider, navigationManager, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="eventLogger">The event logger.</param>
        public Import(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager, IEventLogger eventLogger)
        {
            this.businessControllerProvider = businessControllerProvider ?? this.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
        }

        public TabInfo Tab
        {
            get
            {
                if (this.tab == null)
                {
                    this.tab = TabController.Instance.GetTab(this.TabId, this.PortalId, false);
                }

                return this.tab;
            }
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!TabPermissionController.CanImportPage())
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cboFolders.SelectionChanged += this.OnFolderIndexChanged;
            this.cmdImport.Click += this.OnImportClick;
            this.cboParentTab.SelectionChanged += this.OnParentTabIndexChanged;
            this.cboTemplate.SelectedIndexChanged += this.OnTemplateIndexChanged;
            this.optMode.SelectedIndexChanged += this.OptModeSelectedIndexChanged;

            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.cmdCancel.NavigateUrl = this.navigationManager.NavigateURL();
                    this.cboFolders.UndefinedItem = new ListItem("<" + Localization.GetString("None_Specified") + ">", string.Empty);
                    var folders = FolderManager.Instance.GetFolders(this.UserInfo, "BROWSE, ADD");
                    var templateFolder = folders.SingleOrDefault(f => f.FolderPath == "Templates/");
                    if (templateFolder != null)
                    {
                        this.cboFolders.SelectedFolder = templateFolder;
                    }

                    this.BindFiles();
                    this.BindTabControls();
                    this.DisplayNewRows();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnFolderIndexChanged(object sender, EventArgs e)
        {
            this.BindFiles();
        }

        protected void OnImportClick(object sender, EventArgs e)
        {
            try
            {
                if (this.cboTemplate.SelectedItem == null || this.cboTemplate.SelectedValue == "None_Specified")
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SpecifyFile", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                if (this.optMode.SelectedIndex == -1)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SpecifyMode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                if (this.cboFolders.SelectedItem == null)
                {
                    return;
                }

                var selectedFolder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
                if (selectedFolder == null)
                {
                    return;
                }

                var selectedFile = Services.FileSystem.FileManager.Instance.GetFile(Convert.ToInt32(this.cboTemplate.SelectedValue));
                var xmlDoc = new XmlDocument { XmlResolver = null };
                using (var content = Services.FileSystem.FileManager.Instance.GetFileContent(selectedFile))
                {
                    xmlDoc.Load(content);
                }

                var tabNodes = new List<XmlNode>();
                var selectSingleNode = xmlDoc.SelectSingleNode("//portal/tabs");
                if (selectSingleNode != null)
                {
                    tabNodes.AddRange(selectSingleNode.ChildNodes.Cast<XmlNode>());
                }

                if (tabNodes.Count == 0)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoTabsInTemplate", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                TabInfo objTab;
                if (this.optMode.SelectedValue == "ADD")
                {
                    // Check for invalid
                    string invalidType;
                    if (!TabController.IsValidTabName(this.txtTabName.Text, out invalidType))
                    {
                        var warningMessage = string.Format(Localization.GetString(invalidType, this.LocalResourceFile), this.txtTabName.Text);
                        UI.Skins.Skin.AddModuleMessage(this, warningMessage, ModuleMessage.ModuleMessageType.RedError);
                        return;
                    }

                    // New Tab
                    objTab = new TabInfo { PortalID = this.PortalId, TabName = this.txtTabName.Text, IsVisible = true };
                    var parentId = this.cboParentTab.SelectedItemValueAsInt;
                    if (parentId != Null.NullInteger)
                    {
                        objTab.ParentId = parentId;
                    }

                    objTab.TabPath = Globals.GenerateTabPath(objTab.ParentId, objTab.TabName);
                    var tabId = TabController.GetTabByTabPath(objTab.PortalID, objTab.TabPath, Null.NullString);

                    // Check if tab exists
                    if (tabId != Null.NullInteger)
                    {
                        TabInfo existingTab = TabController.Instance.GetTab(tabId, this.PortalId, false);
                        if (existingTab != null && existingTab.IsDeleted)
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabRecycled", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                        }
                        else
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabExists", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                        }

                        return;
                    }

                    var positionTabId = int.Parse(this.cboPositionTab.SelectedItem.Value);

                    if (this.rbInsertPosition.SelectedValue == "After" && positionTabId > Null.NullInteger)
                    {
                        objTab.TabID = TabController.Instance.AddTabAfter(objTab, positionTabId);
                    }
                    else if (this.rbInsertPosition.SelectedValue == "Before" && positionTabId > Null.NullInteger)
                    {
                        objTab.TabID = TabController.Instance.AddTabBefore(objTab, positionTabId);
                    }
                    else
                    {
                        objTab.TabID = TabController.Instance.AddTab(objTab);
                    }

                    this.eventLogger.AddLog(objTab, this.PortalSettings, this.UserId, string.Empty, EventLogType.TAB_CREATED);

                    objTab = TabController.DeserializeTab(this.businessControllerProvider, tabNodes[0], objTab, this.PortalId, PortalTemplateModuleAction.Replace);

                    var exceptions = string.Empty;

                    // Create second tabs onwards. For firs tab, we like to use tab details from text box, for rest it'll come from template
                    for (var tab = 1; tab < tabNodes.Count; tab++)
                    {
                        try
                        {
                            TabController.DeserializeTab(this.businessControllerProvider, tabNodes[tab], null, this.PortalId, PortalTemplateModuleAction.Replace);
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
                    // Replace Existing Tab
                    objTab = TabController.DeserializeTab(this.businessControllerProvider, tabNodes[0], this.Tab, this.PortalId, PortalTemplateModuleAction.Replace);
                }

                switch (this.optRedirect.SelectedValue)
                {
                    case "VIEW":
                        this.Response.Redirect(this.navigationManager.NavigateURL(objTab.TabID), true);
                        break;
                    default:
                        this.Response.Redirect(this.navigationManager.NavigateURL(objTab.TabID, "Tab", "action=edit"), true);
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
            this.BindBeforeAfterTabControls();
        }

        protected void OnTemplateIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.cboTemplate.SelectedIndex > 0 && this.cboFolders.SelectedItem != null)
                {
                    var selectedFile = Services.FileSystem.FileManager.Instance.GetFile(Convert.ToInt32(this.cboTemplate.SelectedValue));
                    var xmldoc = new XmlDocument { XmlResolver = null };
                    using (var fileContent = Services.FileSystem.FileManager.Instance.GetFileContent(selectedFile))
                    {
                        xmldoc.Load(fileContent);
                        var node = xmldoc.SelectSingleNode("//portal/description");
                        if (node != null && !string.IsNullOrEmpty(node.InnerXml))
                        {
                            this.lblTemplateDescription.Visible = true;
                            this.lblTemplateDescription.Text = this.Server.HtmlDecode(node.InnerXml);
                            this.txtTabName.Text = this.cboTemplate.SelectedItem.Text;
                        }
                        else
                        {
                            this.lblTemplateDescription.Visible = false;
                        }
                    }
                }
                else
                {
                    this.lblTemplateDescription.Visible = false;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OptModeSelectedIndexChanged(object sender, EventArgs e)
        {
            this.DisplayNewRows();
        }

        private void BindBeforeAfterTabControls()
        {
            var noneSpecified = "<" + Localization.GetString("None_Specified") + ">";
            this.cboParentTab.UndefinedItem = new ListItem(noneSpecified, string.Empty);
            var parentTab = this.cboParentTab.SelectedPage;

            List<TabInfo> listTabs = parentTab != null ? TabController.Instance.GetTabsByPortal(parentTab.PortalID).WithParentId(parentTab.TabID) : TabController.Instance.GetTabsByPortal(this.PortalId).WithParentId(Null.NullInteger);
            listTabs = TabController.GetPortalTabs(listTabs, Null.NullInteger, true, noneSpecified, false, false, false, false, true);
            this.cboPositionTab.DataSource = listTabs;
            this.cboPositionTab.DataBind();
            this.rbInsertPosition.Items.Clear();
            this.rbInsertPosition.Items.Add(new ListItem(Localization.GetString("InsertBefore", this.LocalResourceFile), "Before"));
            this.rbInsertPosition.Items.Add(new ListItem(Localization.GetString("InsertAfter", this.LocalResourceFile), "After"));
            this.rbInsertPosition.Items.Add(new ListItem(Localization.GetString("InsertAtEnd", this.LocalResourceFile), "AtEnd"));
            this.rbInsertPosition.SelectedValue = "After";
        }

        private void BindFiles()
        {
            this.cboTemplate.Items.Clear();
            if (this.cboFolders.SelectedItem != null)
            {
                var folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
                if (folder != null)
                {
                    // var files = Directory.GetFiles(PortalSettings.HomeDirectoryMapPath + folder.FolderPath, "*.page.template");
                    var files = Globals.GetFileList(this.PortalId, "page.template", false, folder.FolderPath);
                    foreach (FileItem file in files)
                    {
                        this.cboTemplate.AddItem(file.Text.Replace(".page.template", string.Empty), file.Value);
                    }

                    this.cboTemplate.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "None_Specified");
                    this.cboTemplate.SelectedIndex = 0;
                }
            }
        }

        private void BindTabControls()
        {
            this.BindBeforeAfterTabControls();
            this.divInsertPositionRow.Visible = this.cboPositionTab.Items.Count > 0;
            this.cboParentTab.AutoPostBack = true;
            if (this.cboPositionTab.FindItemByValue(this.TabId.ToString(CultureInfo.InvariantCulture)) != null)
            {
                this.cboPositionTab.ClearSelection();
                this.cboPositionTab.FindItemByValue(this.TabId.ToString(CultureInfo.InvariantCulture)).Selected = true;
            }
        }

        private void DisplayNewRows()
        {
            this.divTabName.Visible = this.optMode.SelectedIndex == 0;
            this.divParentTab.Visible = this.optMode.SelectedIndex == 0;
            this.divInsertPositionRow.Visible = this.optMode.SelectedIndex == 0;
            this.divInsertPositionRow.Visible = this.optMode.SelectedIndex == 0;
        }
    }
}
